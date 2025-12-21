# 42. Stratégie de Cache distribuée

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM effectue de nombreuses opérations répétitives :
- Validation d'API keys (chaque requête)
- Chargement de configuration tenant
- Vérification de quotas
- Résolution de modèles et routes

Sans cache :
- Chaque requête = multiple accès base de données
- Latence élevée (50-100ms par requête)
- Charge importante sur la base de données
- Scalabilité limitée

```csharp
// ❌ SANS CACHE : Accès DB à chaque requête
public async Task<ApiKey?> ValidateApiKeyAsync(string keyValue)
{
    // 1. Hash de la clé
    var hash = HashKey(keyValue);
    
    // 2. Requête DB (10-50ms)
    var apiKey = await _context.ApiKeys
        .Include(k => k.Tenant)
        .FirstOrDefaultAsync(k => k.KeyHash == hash);
    
    // 3. Vérification tenant (autre requête si lazy loading)
    if (apiKey?.Tenant.Status != TenantStatus.Active)
        return null;
    
    return apiKey;
    // Total : 20-100ms par validation
}
```

## Décision

**Implémenter une stratégie de cache multi-niveau avec Redis comme cache distribué et invalidation intelligente.**

### Architecture

```
┌────────────────────────────────────────────────────────────────────┐
│                     STRATÉGIE CACHE MULTI-NIVEAU                   │
│                                                                    │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │  NIVEAU 1 : In-Memory Cache (L1)                            │  │
│  │  • IMemoryCache                                              │  │
│  │  • TTL court (1-5 min)                                       │  │
│  │  • Par instance                                              │  │
│  │  • Hot data uniquement                                       │  │
│  └─────────────────────────────────────────────────────────────┘  │
│                              │                                     │
│                              │ Cache miss                          │
│                              ▼                                     │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │  NIVEAU 2 : Distributed Cache (L2)                          │  │
│  │  • Redis                                                     │  │
│  │  • TTL moyen (5-60 min)                                      │  │
│  │  • Partagé entre instances                                   │  │
│  │  • Warm data                                                 │  │
│  └─────────────────────────────────────────────────────────────┘  │
│                              │                                     │
│                              │ Cache miss                          │
│                              ▼                                     │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │  NIVEAU 3 : Database                                        │  │
│  │  • PostgreSQL                                                │  │
│  │  • Source of truth                                           │  │
│  │  • Cold data                                                 │  │
│  └─────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────┘
```

### 1. Abstraction du Cache

```csharp
/// <summary>
/// Interface d'abstraction du cache.
/// Permet de basculer entre différentes implémentations.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Récupère une valeur du cache.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) 
        where T : class;
    
    /// <summary>
    /// Stocke une valeur dans le cache.
    /// </summary>
    Task SetAsync<T>(
        string key, 
        T value, 
        CacheOptions? options = null,
        CancellationToken ct = default) 
        where T : class;
    
    /// <summary>
    /// Supprime une valeur du cache.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken ct = default);
    
    /// <summary>
    /// Supprime toutes les clés correspondant au pattern.
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken ct = default);
    
    /// <summary>
    /// Récupère ou crée une valeur (cache-aside pattern).
    /// </summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheOptions? options = null,
        CancellationToken ct = default)
        where T : class;
}

/// <summary>
/// Options de cache.
/// </summary>
public sealed record CacheOptions
{
    /// <summary>
    /// Durée de vie absolue.
    /// </summary>
    public TimeSpan? AbsoluteExpiration { get; init; }
    
    /// <summary>
    /// Durée de vie glissante (reset à chaque accès).
    /// </summary>
    public TimeSpan? SlidingExpiration { get; init; }
    
    /// <summary>
    /// Tags pour invalidation groupée.
    /// </summary>
    public IReadOnlyList<string>? Tags { get; init; }
    
    // Presets communs
    public static CacheOptions Short => new() 
    { 
        AbsoluteExpiration = TimeSpan.FromMinutes(1) 
    };
    
    public static CacheOptions Medium => new() 
    { 
        AbsoluteExpiration = TimeSpan.FromMinutes(15),
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };
    
    public static CacheOptions Long => new() 
    { 
        AbsoluteExpiration = TimeSpan.FromHours(1) 
    };
}
```

### 2. Cache Multi-niveau

```csharp
/// <summary>
/// Implémentation du cache multi-niveau (L1 Memory + L2 Redis).
/// </summary>
public sealed class MultiLevelCacheService : ICacheService
{
    private readonly IMemoryCache _l1Cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<MultiLevelCacheService> _logger;
    private readonly CacheConfiguration _config;
    
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct) 
        where T : class
    {
        // 1. Vérifier L1 (Memory)
        if (_l1Cache.TryGetValue(key, out T? l1Value))
        {
            _logger.LogDebug("Cache L1 HIT: {Key}", key);
            return l1Value;
        }
        
        // 2. Vérifier L2 (Redis)
        var db = _redis.GetDatabase();
        var redisValue = await db.StringGetAsync(key);
        
        if (redisValue.HasValue)
        {
            _logger.LogDebug("Cache L2 HIT: {Key}", key);
            var value = JsonSerializer.Deserialize<T>(redisValue!);
            
            // Promouvoir en L1
            _l1Cache.Set(key, value, _config.L1DefaultExpiration);
            
            return value;
        }
        
        _logger.LogDebug("Cache MISS: {Key}", key);
        return null;
    }
    
    public async Task SetAsync<T>(
        string key, 
        T value, 
        CacheOptions? options,
        CancellationToken ct) 
        where T : class
    {
        options ??= CacheOptions.Medium;
        var json = JsonSerializer.Serialize(value);
        
        // 1. Stocker en L2 (Redis) - source de vérité du cache
        var db = _redis.GetDatabase();
        var expiry = options.AbsoluteExpiration ?? _config.L2DefaultExpiration;
        await db.StringSetAsync(key, json, expiry);
        
        // 2. Stocker les tags pour invalidation
        if (options.Tags?.Any() == true)
        {
            foreach (var tag in options.Tags)
            {
                await db.SetAddAsync($"cache:tags:{tag}", key);
            }
        }
        
        // 3. Stocker en L1 (Memory) - TTL plus court
        var l1Expiry = TimeSpan.FromTicks(
            Math.Min(expiry.Ticks, _config.L1DefaultExpiration.Ticks));
        _l1Cache.Set(key, value, l1Expiry);
        
        _logger.LogDebug("Cache SET: {Key} (L1: {L1}, L2: {L2})", 
            key, l1Expiry, expiry);
    }
    
    public async Task RemoveAsync(string key, CancellationToken ct)
    {
        // Supprimer des deux niveaux
        _l1Cache.Remove(key);
        
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
        
        // Publier l'invalidation pour les autres instances
        var subscriber = _redis.GetSubscriber();
        await subscriber.PublishAsync(
            RedisChannel.Literal("cache:invalidate"), 
            key);
        
        _logger.LogDebug("Cache REMOVE: {Key}", key);
    }
    
    public async Task RemoveByPatternAsync(string pattern, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        
        // Trouver toutes les clés correspondant au pattern
        await foreach (var key in server.KeysAsync(pattern: pattern))
        {
            await RemoveAsync(key!, ct);
        }
    }
    
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheOptions? options,
        CancellationToken ct)
        where T : class
    {
        // 1. Essayer de récupérer du cache
        var cached = await GetAsync<T>(key, ct);
        if (cached is not null)
            return cached;
        
        // 2. Créer la valeur (avec lock distribué pour éviter stampede)
        var lockKey = $"lock:{key}";
        var db = _redis.GetDatabase();
        
        // Acquérir le lock
        var lockAcquired = await db.LockTakeAsync(
            lockKey, 
            Environment.MachineName, 
            TimeSpan.FromSeconds(30));
        
        try
        {
            if (lockAcquired)
            {
                // Double-check après acquisition du lock
                cached = await GetAsync<T>(key, ct);
                if (cached is not null)
                    return cached;
                
                // Créer la valeur
                var value = await factory(ct);
                await SetAsync(key, value, options, ct);
                return value;
            }
            else
            {
                // Attendre que l'autre instance popule le cache
                await Task.Delay(100, ct);
                return await GetOrCreateAsync(key, factory, options, ct);
            }
        }
        finally
        {
            if (lockAcquired)
            {
                await db.LockReleaseAsync(lockKey, Environment.MachineName);
            }
        }
    }
}
```

### 3. Invalidation par événements

```csharp
/// <summary>
/// Service d'écoute des invalidations de cache.
/// </summary>
public sealed class CacheInvalidationSubscriber : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IMemoryCache _l1Cache;
    private readonly ILogger<CacheInvalidationSubscriber> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();
        
        await subscriber.SubscribeAsync(
            RedisChannel.Literal("cache:invalidate"),
            (channel, message) =>
            {
                var key = message.ToString();
                _l1Cache.Remove(key);
                _logger.LogDebug("L1 cache invalidated: {Key}", key);
            });
        
        _logger.LogInformation("Cache invalidation subscriber started");
        
        // Garder le service actif
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}

/// <summary>
/// Handler de Domain Events pour invalidation automatique.
/// </summary>
public sealed class CacheInvalidationEventHandler :
    INotificationHandler<TenantUpdatedEvent>,
    INotificationHandler<ApiKeyRevokedEvent>,
    INotificationHandler<ApiKeyCreatedEvent>
{
    private readonly ICacheService _cache;
    
    public async Task Handle(TenantUpdatedEvent notification, CancellationToken ct)
    {
        // Invalider toutes les clés liées au tenant
        await _cache.RemoveByPatternAsync(
            $"tenant:{notification.TenantId}:*", ct);
    }
    
    public async Task Handle(ApiKeyRevokedEvent notification, CancellationToken ct)
    {
        // Invalider la clé API spécifique
        await _cache.RemoveAsync(
            $"apikey:{notification.ApiKeyId}", ct);
        
        // Invalider le cache du tenant
        await _cache.RemoveAsync(
            $"tenant:{notification.TenantId}:apikeys", ct);
    }
    
    public async Task Handle(ApiKeyCreatedEvent notification, CancellationToken ct)
    {
        // Invalider la liste des clés du tenant
        await _cache.RemoveAsync(
            $"tenant:{notification.TenantId}:apikeys", ct);
    }
}
```

### 4. Cache pour les API Keys

```csharp
/// <summary>
/// Repository d'API Keys avec cache intégré.
/// </summary>
public sealed class CachedApiKeyRepository : IApiKeyRepository
{
    private readonly IApiKeyRepository _inner;
    private readonly ICacheService _cache;
    private readonly ILogger<CachedApiKeyRepository> _logger;
    
    /// <summary>
    /// Valide une clé API avec cache.
    /// </summary>
    public async Task<ApiKey?> GetByKeyHashAsync(
        string keyHash, 
        CancellationToken ct = default)
    {
        var cacheKey = $"apikey:hash:{keyHash}";
        
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async ct => await _inner.GetByKeyHashAsync(keyHash, ct),
            new CacheOptions
            {
                AbsoluteExpiration = TimeSpan.FromMinutes(5),
                Tags = new[] { "apikeys" }
            },
            ct);
    }
    
    /// <summary>
    /// Récupère une clé par ID avec cache.
    /// </summary>
    public async Task<ApiKey?> GetByIdAsync(
        Guid id, 
        CancellationToken ct = default)
    {
        var cacheKey = $"apikey:{id}";
        
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async ct => await _inner.GetByIdAsync(id, ct),
            CacheOptions.Medium,
            ct);
    }
}
```

### 5. Cache pour la configuration

```csharp
/// <summary>
/// Service de configuration tenant avec cache.
/// </summary>
public sealed class CachedTenantConfigurationService : ITenantConfigurationService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICacheService _cache;
    
    public async Task<TenantConfiguration?> GetConfigurationAsync(
        Guid tenantId,
        CancellationToken ct = default)
    {
        var cacheKey = $"tenant:{tenantId}:config";
        
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct);
                if (tenant is null)
                    return null;
                
                return new TenantConfiguration
                {
                    TenantId = tenant.Id,
                    Name = tenant.Name,
                    Status = tenant.Status,
                    Settings = tenant.Settings,
                    RateLimits = tenant.GetRateLimits(),
                    AllowedModels = tenant.GetAllowedModels()
                };
            },
            new CacheOptions
            {
                AbsoluteExpiration = TimeSpan.FromMinutes(15),
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Tags = new[] { $"tenant:{tenantId}" }
            },
            ct);
    }
}
```

### 6. Patterns de Cache

```csharp
/// <summary>
/// Patterns de cache communs.
/// </summary>
public static class CachePatterns
{
    // ═══════════════════════════════════════════════════════════════
    // CACHE-ASIDE (Lazy Loading)
    // ═══════════════════════════════════════════════════════════════
    // Application gère le cache explicitement
    // Le plus commun, flexible
    
    public static async Task<T?> CacheAsideAsync<T>(
        ICacheService cache,
        string key,
        Func<Task<T?>> factory) where T : class
    {
        // 1. Vérifier le cache
        var cached = await cache.GetAsync<T>(key);
        if (cached is not null)
            return cached;
        
        // 2. Charger depuis la source
        var value = await factory();
        
        // 3. Stocker en cache
        if (value is not null)
            await cache.SetAsync(key, value);
        
        return value;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // WRITE-THROUGH
    // ═══════════════════════════════════════════════════════════════
    // Écriture synchrone dans cache ET source
    // Garantit la cohérence
    
    public static async Task WriteThroughAsync<T>(
        ICacheService cache,
        IRepository<T> repository,
        string key,
        T entity) where T : class
    {
        // 1. Écrire dans la source
        await repository.UpdateAsync(entity);
        
        // 2. Mettre à jour le cache
        await cache.SetAsync(key, entity);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // WRITE-BEHIND (Write-Back)
    // ═══════════════════════════════════════════════════════════════
    // Écriture asynchrone dans la source
    // Meilleure performance, risque de perte
    
    public static async Task WriteBehindAsync<T>(
        ICacheService cache,
        IBackgroundJobService jobs,
        string key,
        T entity) where T : class
    {
        // 1. Écrire dans le cache immédiatement
        await cache.SetAsync(key, entity);
        
        // 2. Planifier l'écriture en source (asynchrone)
        await jobs.EnqueueAsync<PersistEntityJob<T>>(j => j.Execute(key, entity));
    }
    
    // ═══════════════════════════════════════════════════════════════
    // REFRESH-AHEAD
    // ═══════════════════════════════════════════════════════════════
    // Rafraîchit le cache AVANT expiration
    // Évite les cache misses
    
    public static async Task<T?> RefreshAheadAsync<T>(
        ICacheService cache,
        string key,
        Func<Task<T?>> factory,
        TimeSpan ttl,
        double refreshThreshold = 0.8) where T : class
    {
        var (value, timeRemaining) = await cache.GetWithTtlAsync<T>(key);
        
        if (value is not null)
        {
            // Si proche de l'expiration, rafraîchir en background
            if (timeRemaining < ttl * (1 - refreshThreshold))
            {
                _ = Task.Run(async () =>
                {
                    var fresh = await factory();
                    if (fresh is not null)
                        await cache.SetAsync(key, fresh, 
                            new CacheOptions { AbsoluteExpiration = ttl });
                });
            }
            return value;
        }
        
        // Cache miss - charger et stocker
        var loaded = await factory();
        if (loaded is not null)
            await cache.SetAsync(key, loaded, 
                new CacheOptions { AbsoluteExpiration = ttl });
        
        return loaded;
    }
}
```

### 7. Configuration

```csharp
/// <summary>
/// Configuration du cache.
/// </summary>
public sealed class CacheConfiguration
{
    public TimeSpan L1DefaultExpiration { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan L2DefaultExpiration { get; set; } = TimeSpan.FromMinutes(15);
    public long L1SizeLimit { get; set; } = 1024; // Nombre d'entrées
    public string RedisConnectionString { get; set; } = "localhost:6379";
    public string KeyPrefix { get; set; } = "llmproxy";
}

/// <summary>
/// Enregistrement des services.
/// </summary>
public static class CacheServiceCollectionExtensions
{
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = configuration
            .GetSection("Cache")
            .Get<CacheConfiguration>() ?? new CacheConfiguration();
        
        services.AddSingleton(config);
        
        // L1 - Memory Cache
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = config.L1SizeLimit;
        });
        
        // L2 - Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(config.RedisConnectionString));
        
        // Service principal
        services.AddSingleton<ICacheService, MultiLevelCacheService>();
        
        // Invalidation subscriber
        services.AddHostedService<CacheInvalidationSubscriber>();
        
        return services;
    }
}
```

## Conséquences

### Positives

- **Performance** : Latence réduite de 50ms à <1ms pour les hits L1
- **Scalabilité** : Réduction de la charge DB
- **Résilience** : Le cache peut servir en cas de DB lente
- **Cohérence** : Invalidation distribuée automatique

### Négatives

- **Complexité** : Gestion de deux niveaux de cache
  - *Mitigation* : Abstraction ICacheService
- **Cohérence** : Données potentiellement stales
  - *Mitigation* : TTL courts, invalidation par événements
- **Mémoire** : Utilisation mémoire accrue
  - *Mitigation* : Limites de taille, éviction LRU

### Neutres

- Redis requis en infrastructure
- Métriques de cache importantes à monitorer

## Alternatives considérées

### Option A : Cache mémoire seul

- **Description** : IMemoryCache uniquement
- **Avantages** : Simple, rapide
- **Inconvénients** : Pas partagé entre instances
- **Raison du rejet** : Incohérent en cluster

### Option B : Redis seul

- **Description** : Pas de cache L1
- **Avantages** : Cohérent, simple
- **Inconvénients** : Latence réseau à chaque accès
- **Raison du rejet** : Latence trop élevée pour hot data

### Option C : CDN Cache

- **Description** : Cache HTTP au niveau CDN
- **Avantages** : Scalable, découplé
- **Inconvénients** : Pas adapté pour données dynamiques/auth
- **Raison du rejet** : Données trop dynamiques

## Références

- [Caching in .NET - Microsoft](https://learn.microsoft.com/en-us/dotnet/core/extensions/caching)
- [Redis Caching Patterns](https://redis.io/docs/manual/patterns/)
- [Cache-Aside Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cache-aside)
