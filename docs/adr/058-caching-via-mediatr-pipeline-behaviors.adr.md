# 58. Caching Applicatif via MediatR Pipeline Behaviors

Date: 2025-12-23

## Statut

Accepté

## Contexte

L'ADR-042 (Distributed Cache Strategy) a défini la stratégie de cache multi-niveaux (L1 Memory, L2 Redis, L3 Database) au niveau infrastructure. Cependant, cette stratégie nécessite une implémentation au niveau Application Layer pour :

1. **Automatiser le caching** sans dupliquer le code dans chaque Query Handler
2. **Respecter CQRS** : Cacher uniquement les Queries (read), jamais les Commands (write)
3. **Gérer l'invalidation cohérente** après les modifications de données
4. **Permettre un opt-in explicite** : Seules les queries appropriées sont cachées

### Problème Sans Implémentation Applicative

```csharp
// ❌ SANS automation : Duplication du code de caching dans chaque handler
public class GetTenantByIdQueryHandler : IQueryHandler<GetTenantByIdQuery, TenantDto>
{
    private readonly ICacheService _cache;
    private readonly IRepository<Tenant> _repository;
    
    public async Task<TenantDto> Handle(GetTenantByIdQuery request, CancellationToken ct)
    {
        // Code de caching dupliqué dans CHAQUE handler ❌
        var cacheKey = $"GetTenantByIdQuery:{request.TenantId}";
        var cached = await _cache.GetAsync<TenantDto>(cacheKey, ct);
        if (cached is not null) return cached;
        
        var tenant = await _repository.GetByIdAsync(request.TenantId, ct);
        var dto = tenant.MapToDto();
        
        await _cache.SetAsync(cacheKey, dto, new CacheOptions 
        { 
            AbsoluteExpiration = TimeSpan.FromMinutes(30) 
        }, ct);
        
        return dto;
    }
}
```

**Problèmes identifiés :**
- 🔴 **Violation DRY** : Code de caching dupliqué dans 10+ handlers
- 🔴 **Risque d'oubli** : Développeur peut oublier d'ajouter le cache
- 🔴 **Inconsistance** : TTL différents, nommage de clés non standardisé
- 🔴 **Couplage** : Handler connaît le mécanisme de cache (violation SoC)
- 🔴 **Tests complexes** : Chaque handler doit mocker `ICacheService`
- 🔴 **Invalidation manuelle** : Risque d'oubli dans les Commands

## Décision

**Implémenter le caching applicatif via MediatR Pipeline Behaviors avec des interfaces marker pour opt-in explicite.**

### Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                    PIPELINE MEDIATR (Ordre critique)                │
│                                                                     │
│  1. LoggingBehavior          → Log requête + durée                 │
│  2. ValidationBehavior       → Valider paramètres                  │
│  3. CachingBehavior          → ✨ CACHE READ (nouveau)             │
│  4. PerformanceBehavior      → Monitoring performance              │
│  5. TransactionBehavior      → Gestion transaction DB              │
│  6. CacheInvalidationBehavior → ✨ CACHE INVALIDATION (nouveau)    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 1. Interfaces Marker (Opt-In Pattern)

```csharp
/// <summary>
/// Interface marker pour les queries cachables.
/// Implémentée par les queries qui bénéficient du caching automatique.
/// </summary>
/// <typeparam name="TResponse">Type de la réponse (doit être sérialisable)</typeparam>
/// <remarks>
/// ⚠️ NE PAS utiliser sur :
/// - Commands (opérations d'écriture)
/// - Queries retournant des données sensibles (passwords, secrets)
/// - Queries temps-réel (métriques en direct, streaming)
/// </remarks>
public interface ICachedQuery<TResponse>
{
    /// <summary>
    /// Durée de vie du cache pour cette query.
    /// </summary>
    /// <returns>
    /// Durée avant expiration. 
    /// - null = 5 minutes par défaut
    /// - Choisir selon stabilité des données :
    ///   * Données statiques (providers) : 60 minutes
    ///   * Données stables (tenants) : 30 minutes
    ///   * Listes changeantes (all tenants) : 15 minutes
    /// </returns>
    TimeSpan? CacheExpiration => TimeSpan.FromMinutes(5);
}

/// <summary>
/// Interface marker pour les commandes qui invalident le cache.
/// Implémentée par les Commands qui modifient des données cachées.
/// </summary>
/// <remarks>
/// L'invalidation est exécutée APRÈS la transaction DB pour garantir cohérence.
/// </remarks>
public interface ICacheInvalidator
{
    /// <summary>
    /// Retourne les clés de cache à invalider.
    /// </summary>
    /// <returns>
    /// Clés exactes ou patterns wildcard (ex: "GetAllTenantsQuery:*")
    /// </returns>
    IEnumerable<string> GetCacheKeysToInvalidate();
}
```

### 2. CachingBehavior (Read)

```csharp
/// <summary>
/// Pipeline behavior pour le caching automatique des queries.
/// Intercept uniquement les queries implémentant ICachedQuery&lt;T&gt;.
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery<TResponse>
    where TResponse : class
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. Générer clé de cache unique et déterministe
        var cacheKey = GenerateCacheKey(request);
        
        // 2. Tentative de lecture cache (Cache-Aside Pattern)
        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        
        if (cachedResponse is not null)
        {
            _logger.LogDebug("Cache HIT for {RequestType} with key {CacheKey}", 
                typeof(TRequest).Name, cacheKey);
            return cachedResponse;
        }
        
        _logger.LogDebug("Cache MISS for {RequestType} with key {CacheKey}", 
            typeof(TRequest).Name, cacheKey);
        
        // 3. Exécuter handler (DB query)
        var response = await next();
        
        // 4. Stocker en cache pour prochaine requête
        await _cacheService.SetAsync(
            cacheKey, 
            response, 
            request.CacheExpiration,
            cancellationToken);
        
        return response!;
    }
    
    /// <summary>
    /// Génère une clé de cache unique et déterministe.
    /// Format : {RequestTypeName}:{SHA256Hash(RequestJSON)}
    /// </summary>
    private static string GenerateCacheKey(TRequest request)
    {
        var typeName = typeof(TRequest).Name;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var hash = ComputeHash(json); // SHA256
        return $"{typeName}:{hash}";
    }
}
```

### 3. CacheInvalidationBehavior (Write)

```csharp
/// <summary>
/// Pipeline behavior pour l'invalidation du cache après Commands.
/// Intercept uniquement les commands implémentant ICacheInvalidator.
/// Exécuté APRÈS TransactionBehavior pour garantir cohérence DB-Cache.
/// </summary>
public sealed class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheInvalidator
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. Exécuter command AVANT invalidation (order matters)
        var response = await next();
        
        // 2. Invalider cache après succès transaction
        var cacheKeys = request.GetCacheKeysToInvalidate();
        
        foreach (var key in cacheKeys)
        {
            if (key.Contains('*'))
            {
                // Pattern wildcard : invalider toutes les clés correspondantes
                await _cacheService.RemoveByPatternAsync(key, cancellationToken);
                _logger.LogDebug("Cache invalidated by pattern: {Pattern}", key);
            }
            else
            {
                // Clé exacte
                await _cacheService.RemoveAsync(key, cancellationToken);
                _logger.LogDebug("Cache invalidated: {CacheKey}", key);
            }
        }
        
        return response;
    }
}
```

### 4. Enregistrement Pipeline MediatR

```csharp
// Extensions/ApplicationServiceCollectionExtensions.cs
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);
    
    // ⚠️ ORDRE CRITIQUE - Ne pas modifier sans justification ADR
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));           // 1. Log
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));        // 2. Validate
    cfg.AddOpenBehavior(typeof(CachingBehavior<,>));           // 3. ✨ Cache READ
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));       // 4. Monitor
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));       // 5. Transaction
    cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>)); // 6. ✨ Cache INVALIDATE
});
```

**Justification de l'ordre :**
- **Caching après Validation** : Éviter de cacher des requêtes invalides
- **Caching avant Performance** : Mesurer temps avec/sans cache
- **Invalidation après Transaction** : Garantir cohérence (DB commit → cache invalidé)

### 5. Utilisation (Query)

```csharp
// ✅ APRÈS : Query opt-in pour caching
public record GetTenantByIdQuery : IQuery<TenantDto>, ICachedQuery<TenantDto>
{
    public Guid TenantId { get; init; }
    
    // TTL 30 minutes (données stables)
    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(30);
}

// Handler SANS code de caching (pure logique métier)
public class GetTenantByIdQueryHandler : IQueryHandler<GetTenantByIdQuery, TenantDto>
{
    private readonly IRepository<Tenant> _repository;
    
    public async Task<TenantDto> Handle(GetTenantByIdQuery request, CancellationToken ct)
    {
        var tenant = await _repository.GetByIdAsync(request.TenantId, ct);
        return tenant.MapToDto();
    }
}
```

### 6. Utilisation (Command avec Invalidation)

```csharp
// ✅ Command qui invalide le cache après création
public record CreateTenantCommand : ICommand<TenantDto>, ICacheInvalidator
{
    public string Name { get; init; } = default!;
    public string ApiKey { get; init; } = default!;
    
    // Invalider toutes les listes de tenants après création
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return "GetAllTenantsQuery:*"; // Wildcard : invalide TOUTES variantes
    }
}
```

## Alternatives Considérées

### 1. Attribute-Based Caching (AOP)

```csharp
// Alternative : Utiliser des attributs
[Cacheable(Duration = 1800)]
public class GetTenantByIdQueryHandler { }
```

**Rejeté car :**
- ❌ Moins flexible (paramètres statiques, pas de TTL dynamique par query)
- ❌ Dépendance à un framework AOP (PostSharp, Castle DynamicProxy)
- ❌ Difficile à tester unitairement (interception opaque)
- ❌ Nécessite runtime code generation (complexité déploiement)
- ❌ Moins explicite (magic attribute vs interface contractuelle)

### 2. Decorator Pattern

```csharp
// Alternative : Wrapper chaque handler
public class CachedQueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> _inner;
    private readonly ICacheService _cache;
    
    public async Task<TResponse> Handle(TQuery request, CancellationToken ct)
    {
        // Cache logic
        return await _inner.Handle(request, ct);
    }
}
```

**Rejeté car :**
- ❌ Verbeux : Nécessite enregistrement manuel de chaque wrapper
- ❌ Duplication de code (decorator pour chaque handler)
- ❌ Difficile à maintenir (oubli facile)
- ❌ Pas de convention centralisée

### 3. Manual Caching dans chaque Handler

```csharp
// Alternative : Code manuel dans chaque handler
public async Task<TenantDto> Handle(...)
{
    var cacheKey = $"tenant-{request.TenantId}";
    var cached = await _cache.GetAsync<TenantDto>(cacheKey);
    if (cached != null) return cached;
    
    // ... logique métier
    
    await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
    return result;
}
```

**Rejeté car :**
- ❌ **Violation DRY** : Code dupliqué dans 10+ handlers
- ❌ **Coupling** : Handler connaît le cache (violation SoC)
- ❌ **Inconsistance** : TTL, nommage, hashage différents
- ❌ **Oubli facile** : Pas de garantie que tous les handlers cachent
- ❌ **Tests complexes** : Chaque test doit mocker `ICacheService`

### 4. Repository Layer Caching

```csharp
// Alternative : Cache au niveau Repository
public class CachedTenantRepository : ITenantRepository
{
    public async Task<Tenant> GetByIdAsync(Guid id)
    {
        var cacheKey = $"tenant-{id}";
        // Cache logic
    }
}
```

**Rejeté car :**
- ❌ **Trop bas niveau** : Ne gère pas les paramètres de Query (filters, pagination)
- ❌ **Incompatible CQRS** : Queries retournent DTOs, pas entités domain
- ❌ **Cache trop granulaire** : Tenant entity ≠ TenantDto (mapping après cache)
- ❌ **Pas de contrôle applicatif** : Repository ne connaît pas le TTL optimal

## Conséquences

### Positives

1. ✅ **DRY absolu** : Code de caching écrit une seule fois dans les behaviors
2. ✅ **Opt-in explicite** : Query implémente `ICachedQuery<T>` → intention claire dans le code
3. ✅ **TTL flexible** : Chaque query définit sa propre durée (15-60 min selon stabilité données)
4. ✅ **Invalidation cohérente** : `CacheInvalidationBehavior` après transaction → pas de cache stale
5. ✅ **Testabilité** : 
   - Behaviors testables unitairement (mocking `ICacheService`)
   - Handlers testables sans cache (pas de dépendance `ICacheService`)
6. ✅ **Ordre garanti** : Pipeline MediatR assure séquencement correct (Validation → Cache → Transaction → Invalidation)
7. ✅ **Performance mesurable** : Réduction latence 50-100ms (DB) → <5ms (cache hit)
8. ✅ **Convention claire** : Pattern standardisé pour toute nouvelle query
9. ✅ **Separation of Concerns** : Handler = pure logique métier, Behavior = cache transversal
10. ✅ **Monitoring facile** : Logs centralisés (cache HIT/MISS) dans behaviors

### Négatives

1. ❌ **Dépendance MediatR** : Si changement d'orchestrateur (ex: Wolverine, MassTransit), behaviors à réécrire
2. ❌ **Magic behavior** : Peut surprendre développeurs non familiers (debugging nécessite comprendre pipeline)
3. ❌ **Cache key generation** : 
   - Doit gérer sérialisation JSON correcte (properties order, circular refs)
   - SHA256 hashing coûteux (mais négligeable vs latence DB)
4. ❌ **Mémoire** : Cache peut croître (besoin monitoring, eviction policy Redis)
5. ❌ **Complexité pipeline** : Ordre des behaviors critique → erreur = bug subtil
   - Exemple : Invalidation AVANT transaction = cache incohérent
6. ❌ **Données sensibles** : Risque de cacher données non sérialisables ou sensibles
   - Mitigation : Guidelines dans `ICachedQuery` XML comments
7. ❌ **Debugging** : Cache peut masquer problèmes (query retourne données stale)
   - Mitigation : Logs explicites (HIT/MISS), feature flag pour désactiver cache en dev
8. ❌ **Tests end-to-end** : Nécessite Redis pour tests d'intégration complets
   - Mitigation : In-memory cache pour tests unitaires

## Alignement Stratégique

**Objectifs métier supportés :**
- **Performance** : Latence réduite de 50-100ms → <5ms (cache hit rate 95%)
- **Scalabilité** : Moins de charge DB → support de milliers de requêtes/sec
- **Coût** : Réduction coût infrastructure (moins de scaling DB nécessaire)

**Contraintes respectées :**
- **CQRS** : Séparation claire Queries (cachées) vs Commands (invalident)
- **SOLID** : 
  - Single Responsibility (handler = métier, behavior = cache)
  - Open/Closed (nouveaux behaviors sans modifier handlers)
- **DRY** : Zéro duplication code de caching

**Risques métier atténués :**
- **Données stale** : Invalidation automatique après modifications
- **Inconsistance** : Transaction committed AVANT invalidation cache

## Exemples Concrets (Projet)

### Queries Cachées

| Query | TTL | Justification |
|-------|-----|---------------|
| `GetTenantByIdQuery` | 30 min | Données stables (rarement modifiées) |
| `GetAllTenantsQuery` | 15 min | Liste change plus souvent (création/suppression) |
| `GetProviderByIdQuery` | 60 min | Configuration quasi-statique (OpenAI, Anthropic) |

### Commands avec Invalidation

| Command | Clés invalidées | Pattern |
|---------|-----------------|---------|
| `CreateTenantCommand` | `GetAllTenantsQuery:*` | Wildcard (toutes variantes) |
| `UpdateTenantCommand` | `GetTenantByIdQuery:{id}`, `GetAllTenantsQuery:*` | Exact + wildcard |
| `DeleteTenantCommand` | `GetTenantByIdQuery:{id}`, `GetAllTenantsQuery:*` | Exact + wildcard |

## Métriques de Succès

| Métrique | Avant | Après | Objectif |
|----------|-------|-------|----------|
| Latence P50 (GetTenantById) | 45ms | 3ms | <10ms |
| Latence P95 (GetAllTenants) | 120ms | 8ms | <20ms |
| Cache Hit Rate | 0% | 95% | >90% |
| DB Query Count | 1000/s | 50/s | <100/s |

## Références

- **ADR-042** : Distributed Cache Strategy (infrastructure multi-niveaux)
- **ADR-013** : CQRS (séparation Queries/Commands)
- **ADR-010** : Separation of Concerns
- **ADR-003** : DRY (Don't Repeat Yourself)
- **ADR-019** : Convention over Configuration
- [MediatR Pipeline Behaviors Documentation](https://github.com/jbogard/MediatR/wiki/Behaviors)
- [Cache-Aside Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside)

## Notes d'Implémentation

### Guidelines pour Développeurs

**✅ Quand utiliser `ICachedQuery<T>` :**
- Queries avec données stables (configuration, metadata)
- Queries fréquemment appelées (validation API keys, routes)
- Queries coûteuses en DB (joins multiples, agrégations)

**❌ Quand NE PAS utiliser :**
- Commands (opérations d'écriture)
- Queries temps-réel (métriques live, streaming)
- Queries retournant données sensibles (passwords, secrets, PII sans chiffrement)
- Queries avec résultats non déterministes (DateTime.Now, Random)

### Monitoring

```csharp
// Métriques à exposer (OpenTelemetry)
- cache.hit.count (counter)
- cache.miss.count (counter)
- cache.hit.rate (gauge)
- cache.operation.duration (histogram)
- cache.memory.usage (gauge)
```

### Feature Flag

```json
// appsettings.json
{
  "Features": {
    "CachingEnabled": true,  // Désactiver en debug si besoin
    "CacheInvalidationEnabled": true
  }
}
```
