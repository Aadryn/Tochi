# Tâche 026 - Implémenter ADR-026 : Null Object Pattern

**Statut** : À faire  
**Priorité** : 🟡 MOYENNE (P3)  
**Conformité cible** : ADR-026 de 0% → 90%  
**Dépendances** : Aucune

## CONTEXTE

**Analyse ADR-026** : `docs/ANALYSE_CONFORMITE_ADR-013-030.md` (lignes 601-657)  
**ADR** : `docs/adr/026-null-object-pattern.adr.md`

**Conformité actuelle** : **0%** (pattern non implémenté)

**Problème identifié** :
- 🟡 **Null checks dupliqués** : Validations `if (x == null)` partout dans le code
- 🟡 **NullReferenceException risque** : Erreurs runtime potentielles
- 🟡 **Code verbeux** : Logique par défaut dupliquée
- 🟡 **Manque de polymorphisme** : Traitement spécial pour null

**Cas d'usage identifiés** (docs/ANALYSE_CONFORMITE_ADR-013-030.md lignes 608-612) :
1. **DefaultTenant** : Tenant par défaut pour utilisateurs sans tenant assigné
2. **NullQuotaLimit** : Quota illimité (au lieu de `null`)
3. **EmptyCache** : Cache désactivé (retourne toujours miss)

**Risques sans Null Object Pattern** :
- 🟡 **NullReferenceException** : Erreurs runtime si null non vérifié
- 🟡 **Code dupliqué** : Null checks répétés dans tous les handlers
- 🟡 **Logique métier dispersée** : Comportement par défaut non centralisé
- 🟡 **Tests complexes** : Mock de null difficile

**Violations détectées** :
```csharp
// ❌ V-NULL-001 : Null checks partout (ANALYSE_CONFORMITE ligne 613)
public async Task ProcessRequest(Guid tenantId)
{
    var tenant = await _repository.GetByIdAsync(tenantId);
    
    if (tenant == null)  // ❌ Null check manuel répété
    {
        return DefaultBehavior();  // Logique par défaut dupliquée
    }
    
    return tenant.ProcessRequest();
}

// ❌ V-NULL-002 : Quota null → logique spéciale
var quota = await _quotaService.GetQuotaAsync(userId);
if (quota == null)  // ❌ Traitement spécial pour null
{
    // Logique quota illimité dupliquée
    return Result.Success();
}

// ❌ V-NULL-003 : Cache null → désactivé
var cache = GetCache();
if (cache != null)  // ❌ Null check avant chaque utilisation
{
    cache.Set(key, value);
}
```

## OBJECTIF

Implémenter le Null Object Pattern pour éliminer les null checks et centraliser le comportement par défaut.

**Spécifications ADR-026** :
- Créer Null Objects avec comportement neutre par défaut
- Utiliser Singleton Pattern (Instance statique)
- Polymorphisme : Null Object hérite de l'interface/classe de base
- Éviter `null` dans le code métier

## CRITÈRES DE SUCCÈS

### Fonctionnels
- [ ] `NullTenant` créé (comportement par défaut tenant)
- [ ] `UnlimitedQuotaLimit` créé (quota illimité)
- [ ] `NullCache` créé (cache désactivé)
- [ ] Null checks éliminés dans repositories et services
- [ ] Comportement par défaut centralisé

### Techniques
- [ ] Null Objects héritent des classes de base
- [ ] Singleton Pattern (Instance statique)
- [ ] Constructeur privé (empêche instanciation externe)
- [ ] Tests unitaires : au moins 6 scénarios

### Qualité
- [ ] **Build** : 0 erreurs, 0 warnings
- [ ] **Tests** : 6+ nouveaux tests Null Objects
  - NullTenant behavior
  - UnlimitedQuotaLimit behavior
  - NullCache behavior
  - Repository GetOrDefault
  - Service default behavior
  - Polymorphisme validation
- [ ] Tests existants : 100% passing (non-régression)
- [ ] Documentation README.md mise à jour

## ÉTAPES D'IMPLÉMENTATION

### 1. Créer NullTenant (2h)

**Fichier** : `src/Core/LLMProxy.Domain/Entities/NullTenant.cs`

**Création** : Null Object pour Tenant par défaut.

```csharp
namespace LLMProxy.Domain.Entities;

/// <summary>
/// Null Object représentant un tenant par défaut pour les utilisateurs non assignés.
/// Conforme à ADR-026 (Null Object Pattern).
/// </summary>
/// <remarks>
/// Ce tenant fictif retourne toujours des résultats d'échec pour éviter les NullReferenceException.
/// Utilisé lorsque aucun tenant réel n'est trouvé au lieu de retourner <c>null</c>.
/// </remarks>
public sealed class NullTenant : Tenant
{
    /// <summary>
    /// Instance singleton du tenant par défaut.
    /// </summary>
    public static NullTenant Instance { get; } = new NullTenant();

    /// <summary>
    /// Constructeur privé pour empêcher l'instanciation externe.
    /// </summary>
    private NullTenant()
        : base(
            id: Guid.Empty,
            name: "Default Tenant",
            slug: "default",
            settings: TenantSettings.CreateDefault())
    {
    }

    /// <summary>
    /// Le tenant par défaut est toujours inactif.
    /// </summary>
    public override bool IsActive => false;

    /// <summary>
    /// Toute tentative de traitement retourne une erreur d'autorisation.
    /// </summary>
    public override Result ProcessRequest()
    {
        return Result.Failure(Error.Unauthorized("No tenant assigned"));
    }

    /// <summary>
    /// Le tenant par défaut n'a jamais de quotas.
    /// </summary>
    public override bool HasQuota(long tokens) => false;
}
```

**Action** : Créer classe `NullTenant` héritant de `Tenant`.

---

### 2. Créer UnlimitedQuotaLimit (1h30)

**Fichier** : `src/Core/LLMProxy.Domain/Entities/UnlimitedQuotaLimit.cs`

**Création** : Null Object pour quota illimité.

```csharp
namespace LLMProxy.Domain.Entities;

/// <summary>
/// Null Object représentant un quota illimité (aucune limite).
/// Conforme à ADR-026 (Null Object Pattern).
/// </summary>
/// <remarks>
/// Ce quota fictif accepte toujours toutes les requêtes.
/// Utilisé pour les utilisateurs premium ou les environnements de test.
/// </remarks>
public sealed class UnlimitedQuotaLimit : QuotaLimit
{
    /// <summary>
    /// Instance singleton du quota illimité.
    /// </summary>
    public static UnlimitedQuotaLimit Instance { get; } = new UnlimitedQuotaLimit();

    /// <summary>
    /// Constructeur privé pour empêcher l'instanciation externe.
    /// </summary>
    private UnlimitedQuotaLimit()
        : base(
            userId: Guid.Empty,
            quotaType: QuotaType.RequestsPerMinute,
            limit: long.MaxValue,
            period: QuotaPeriod.Unlimited)
    {
    }

    /// <summary>
    /// Le quota illimité accepte toujours n'importe quelle quantité de tokens.
    /// </summary>
    public override Result<long> RecordUsage(Guid transactionId, long tokens)
    {
        // Toujours accepté, retourne 0 (pas de quota consommé)
        return Result.Success(0L);
    }

    /// <summary>
    /// Le quota illimité n'est jamais dépassé.
    /// </summary>
    public override bool IsExceeded => false;

    /// <summary>
    /// Le quota illimité a toujours de la capacité restante.
    /// </summary>
    public override long RemainingCapacity => long.MaxValue;
}
```

**Action** : Créer classe `UnlimitedQuotaLimit` héritant de `QuotaLimit`.

---

### 3. Créer NullCache (1h30)

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Redis/NullCache.cs`

**Création** : Null Object pour cache désactivé.

```csharp
using Microsoft.Extensions.Caching.Distributed;

namespace LLMProxy.Infrastructure.Redis;

/// <summary>
/// Null Object représentant un cache désactivé (no-op).
/// Conforme à ADR-026 (Null Object Pattern).
/// </summary>
/// <remarks>
/// Ce cache fictif ne stocke rien et retourne toujours des cache misses.
/// Utilisé lorsque Redis est indisponible ou en mode dégradé.
/// </remarks>
public sealed class NullCache : IDistributedCache
{
    /// <summary>
    /// Instance singleton du cache désactivé.
    /// </summary>
    public static NullCache Instance { get; } = new NullCache();

    /// <summary>
    /// Constructeur privé pour empêcher l'instanciation externe.
    /// </summary>
    private NullCache() { }

    /// <summary>
    /// Retourne toujours null (cache miss).
    /// </summary>
    public byte[]? Get(string key) => null;

    /// <summary>
    /// Retourne toujours null (cache miss) de manière asynchrone.
    /// </summary>
    public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        return Task.FromResult<byte[]?>(null);
    }

    /// <summary>
    /// Ne stocke rien (no-op).
    /// </summary>
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        // No-op : ne fait rien
    }

    /// <summary>
    /// Ne stocke rien (no-op) de manière asynchrone.
    /// </summary>
    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ne rafraîchit rien (no-op).
    /// </summary>
    public void Refresh(string key)
    {
        // No-op : ne fait rien
    }

    /// <summary>
    /// Ne rafraîchit rien (no-op) de manière asynchrone.
    /// </summary>
    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ne supprime rien (no-op).
    /// </summary>
    public void Remove(string key)
    {
        // No-op : ne fait rien
    }

    /// <summary>
    /// Ne supprime rien (no-op) de manière asynchrone.
    /// </summary>
    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }
}
```

**Action** : Créer classe `NullCache` implémentant `IDistributedCache`.

---

### 4. Modifier Tenant Repository (1h30)

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/Repositories/TenantRepository.cs`

**Modification** : Ajouter méthode `GetByIdOrDefaultAsync`.

```csharp
/// <summary>
/// Récupère un tenant par son identifiant ou retourne le tenant par défaut.
/// </summary>
/// <param name="id">Identifiant unique du tenant.</param>
/// <param name="cancellationToken">Token d'annulation.</param>
/// <returns>Le tenant trouvé ou <see cref="NullTenant.Instance"/>.</returns>
public async Task<Tenant> GetByIdOrDefaultAsync(Guid id, CancellationToken cancellationToken = default)
{
    var tenant = await _context.Tenants
        .Include(t => t.Settings)
        .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    return tenant ?? NullTenant.Instance;
}

/// <summary>
/// Récupère un tenant par son slug ou retourne le tenant par défaut.
/// </summary>
/// <param name="slug">Slug unique du tenant.</param>
/// <param name="cancellationToken">Token d'annulation.</param>
/// <returns>Le tenant trouvé ou <see cref="NullTenant.Instance"/>.</returns>
public async Task<Tenant> GetBySlugOrDefaultAsync(string slug, CancellationToken cancellationToken = default)
{
    var tenant = await _context.Tenants
        .Include(t => t.Settings)
        .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);

    return tenant ?? NullTenant.Instance;
}
```

**Action** : Ajouter méthodes `GetByIdOrDefaultAsync` et `GetBySlugOrDefaultAsync`.

---

### 5. Modifier Quota Service (1h30)

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Redis/QuotaService.cs`

**Modification** : Retourner `UnlimitedQuotaLimit` au lieu de null.

```csharp
/// <summary>
/// Récupère le quota actuel d'un utilisateur ou retourne un quota illimité.
/// </summary>
/// <param name="userId">Identifiant de l'utilisateur.</param>
/// <param name="quotaType">Type de quota.</param>
/// <param name="cancellationToken">Token d'annulation.</param>
/// <returns>Le quota trouvé ou <see cref="UnlimitedQuotaLimit.Instance"/>.</returns>
public async Task<QuotaLimit> GetQuotaOrUnlimitedAsync(
    Guid userId,
    QuotaType quotaType,
    CancellationToken cancellationToken = default)
{
    var quota = await GetQuotaAsync(userId, quotaType, cancellationToken);
    return quota ?? UnlimitedQuotaLimit.Instance;
}
```

**Action** : Ajouter méthode `GetQuotaOrUnlimitedAsync`.

---

### 6. Modifier Cache Configuration (1h)

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Redis/RedisInfrastructureExtensions.cs`

**Modification** : Retourner `NullCache` si Redis indisponible.

```csharp
/// <summary>
/// Enregistre l'infrastructure Redis avec gestion graceful de l'indisponibilité.
/// </summary>
public static IServiceCollection AddRedisInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("Redis");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        // Redis désactivé → Utiliser NullCache (Null Object Pattern)
        services.AddSingleton<IDistributedCache>(NullCache.Instance);
        return services;
    }

    try
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });
    }
    catch (Exception ex)
    {
        // Redis inaccessible → Utiliser NullCache (mode dégradé)
        services.AddSingleton<IDistributedCache>(NullCache.Instance);
    }

    // Autres services Redis...
    return services;
}
```

**Action** : Utiliser `NullCache.Instance` quand Redis indisponible.

---

### 7. Refactoring Usages (2h)

**Fichiers à modifier** :
- `src/Application/LLMProxy.Application/Tenants/Queries/GetTenantQueryHandler.cs`
- `src/Application/LLMProxy.Application/Quotas/Commands/RecordUsageCommandHandler.cs`
- Middlewares utilisant cache

**Changements** :

```csharp
// ❌ AVANT : Null check manuel
var tenant = await _tenantRepository.GetByIdAsync(tenantId);
if (tenant == null)
{
    return Result.Failure(Error.NotFound("Tenant not found"));
}
return Result.Success(tenant);

// ✅ APRÈS : Null Object Pattern
var tenant = await _tenantRepository.GetByIdOrDefaultAsync(tenantId);
if (!tenant.IsActive)  // NullTenant.IsActive = false
{
    return Result.Failure(Error.NotFound("Tenant not found"));
}
return Result.Success(tenant);
```

**Action** : Remplacer tous les null checks par utilisation Null Objects.

---

### 8. Créer tests unitaires (2h)

**Fichier** : `tests/LLMProxy.Domain.Tests/Entities/NullObjectTests.cs`

**Création** : Tests pour Null Objects.

```csharp
using LLMProxy.Domain.Entities;
using Xunit;

namespace LLMProxy.Domain.Tests.Entities;

/// <summary>
/// Tests unitaires pour les Null Objects.
/// Conforme à ADR-026 (Null Object Pattern).
/// </summary>
public sealed class NullObjectTests
{
    [Fact]
    public void NullTenant_ShouldHaveSingletonInstance()
    {
        // ARRANGE & ACT
        var instance1 = NullTenant.Instance;
        var instance2 = NullTenant.Instance;

        // ASSERT
        Assert.Same(instance1, instance2);  // Même instance (Singleton)
    }

    [Fact]
    public void NullTenant_ShouldAlwaysBeInactive()
    {
        // ARRANGE
        var tenant = NullTenant.Instance;

        // ACT & ASSERT
        Assert.False(tenant.IsActive);
    }

    [Fact]
    public void NullTenant_ProcessRequest_ShouldReturnUnauthorizedError()
    {
        // ARRANGE
        var tenant = NullTenant.Instance;

        // ACT
        var result = tenant.ProcessRequest();

        // ASSERT
        Assert.True(result.IsFailure);
        Assert.Equal("Unauthorized", result.Error.Code);
    }

    [Fact]
    public void UnlimitedQuotaLimit_ShouldHaveSingletonInstance()
    {
        // ARRANGE & ACT
        var instance1 = UnlimitedQuotaLimit.Instance;
        var instance2 = UnlimitedQuotaLimit.Instance;

        // ASSERT
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void UnlimitedQuotaLimit_RecordUsage_ShouldAlwaysSucceed()
    {
        // ARRANGE
        var quota = UnlimitedQuotaLimit.Instance;
        var transactionId = Guid.NewGuid();

        // ACT
        var result = quota.RecordUsage(transactionId, 1_000_000);

        // ASSERT
        Assert.True(result.IsSuccess);
        Assert.Equal(0L, result.Value);  // Aucun quota consommé
    }

    [Fact]
    public void UnlimitedQuotaLimit_ShouldNeverBeExceeded()
    {
        // ARRANGE
        var quota = UnlimitedQuotaLimit.Instance;

        // ACT & ASSERT
        Assert.False(quota.IsExceeded);
        Assert.Equal(long.MaxValue, quota.RemainingCapacity);
    }

    [Fact]
    public async Task NullCache_GetAsync_ShouldAlwaysReturnNull()
    {
        // ARRANGE
        var cache = NullCache.Instance;

        // ACT
        var result = await cache.GetAsync("any-key");

        // ASSERT
        Assert.Null(result);  // Cache miss toujours
    }

    [Fact]
    public async Task NullCache_SetAsync_ShouldDoNothing()
    {
        // ARRANGE
        var cache = NullCache.Instance;
        var value = new byte[] { 1, 2, 3 };

        // ACT & ASSERT (pas d'exception)
        await cache.SetAsync("key", value, new DistributedCacheEntryOptions());

        // Vérifier qu'aucune valeur n'est stockée
        var retrieved = await cache.GetAsync("key");
        Assert.Null(retrieved);
    }
}
```

**Action** : Créer 8 tests couvrant tous les Null Objects.

---

### 9. Mettre à jour README.md (30 min)

**Fichier** : `README.md`

**Ajout** : Section "Null Object Pattern (ADR-026)".

```markdown
### Null Object Pattern (ADR-026)

**Élimination des null checks** avec objets par défaut à comportement neutre.

**Null Objects disponibles :**

| Null Object | Usage | Comportement |
|-------------|-------|--------------|
| `NullTenant.Instance` | Tenant par défaut | Toujours inactif, retourne erreurs |
| `UnlimitedQuotaLimit.Instance` | Quota illimité | Accepte toujours toutes les requêtes |
| `NullCache.Instance` | Cache désactivé | Retourne toujours cache miss |

**Avantages :**
- ✅ Élimine NullReferenceException
- ✅ Code plus propre (pas de `if (x == null)`)
- ✅ Polymorphisme (Null Object hérite de la classe de base)
- ✅ Comportement par défaut centralisé

**Exemple d'utilisation :**

```csharp
// ❌ AVANT : Null check manuel
var tenant = await _repository.GetByIdAsync(tenantId);
if (tenant == null)
{
    return Error.NotFound();
}
return tenant.ProcessRequest();

// ✅ APRÈS : Null Object Pattern
var tenant = await _repository.GetByIdOrDefaultAsync(tenantId);
return tenant.ProcessRequest();  // NullTenant retourne erreur automatiquement
```

**Configuration Redis dégradée :**

Si Redis est indisponible, le système utilise automatiquement `NullCache.Instance` :
- Aucune erreur levée
- Application continue de fonctionner
- Cache désactivé temporairement (mode dégradé)

**Singleton Pattern :**

Tous les Null Objects utilisent le pattern Singleton :
```csharp
var tenant1 = NullTenant.Instance;
var tenant2 = NullTenant.Instance;
// tenant1 == tenant2 (même instance)
```
```

**Action** : Documenter Null Object Pattern et usage.

---

### 10. Build, test et validation (1h)

**Commandes** :

```powershell
# Build
dotnet build --no-restore

# Tests
dotnet test --no-build --no-restore

# Validation : Vérifier sortie
# - 0 errors, 0 warnings
# - Tous tests passing (89 anciens + 8 nouveaux = 97 total)
```

**Action** :
1. Compiler sans erreurs ni warnings
2. Exécuter tests (100% passing)
3. Valider polymorphisme avec tests manuels

---

### 11. Commit et merge (30 min)

**Commits atomiques** :

```powershell
# Commit 1: NullTenant
git add src/Core/LLMProxy.Domain/Entities/NullTenant.cs
git commit -m "feat(null-object): Add NullTenant Null Object

- Singleton instance for default tenant
- Always inactive (IsActive = false)
- ProcessRequest returns Unauthorized error
- Prevents NullReferenceException in tenant lookups

ADR-026 conformity: NullTenant created"

# Commit 2: UnlimitedQuotaLimit
git add src/Core/LLMProxy.Domain/Entities/UnlimitedQuotaLimit.cs
git commit -m "feat(null-object): Add UnlimitedQuotaLimit Null Object

- Singleton instance for unlimited quota
- Always accepts any token count
- Never exceeded (RemainingCapacity = MaxValue)
- Used for premium users or test environments

ADR-026 conformity: UnlimitedQuotaLimit created"

# Commit 3: NullCache
git add src/Infrastructure/LLMProxy.Infrastructure.Redis/NullCache.cs
git commit -m "feat(null-object): Add NullCache Null Object

- Singleton instance for disabled cache
- Implements IDistributedCache with no-op operations
- Always returns cache miss
- Used when Redis unavailable (degraded mode)

ADR-026 conformity: NullCache created"

# Commit 4: Repository modifications
git add src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/Repositories/TenantRepository.cs
git commit -m "feat(null-object): Add GetByIdOrDefaultAsync methods to TenantRepository

- GetByIdOrDefaultAsync returns NullTenant if not found
- GetBySlugOrDefaultAsync returns NullTenant if not found
- Eliminates null checks in callers

ADR-026 conformity: Repository updated"

# Commit 5: Quota Service
git add src/Infrastructure/LLMProxy.Infrastructure.Redis/QuotaService.cs
git commit -m "feat(null-object): Add GetQuotaOrUnlimitedAsync to QuotaService

- Returns UnlimitedQuotaLimit if quota not found
- Eliminates null checks for quota operations
- Graceful degradation for missing quotas

ADR-026 conformity: QuotaService updated"

# Commit 6: Redis Configuration
git add src/Infrastructure/LLMProxy.Infrastructure.Redis/RedisInfrastructureExtensions.cs
git commit -m "feat(null-object): Use NullCache when Redis unavailable

- AddRedisInfrastructure returns NullCache if connection fails
- Graceful degradation (app continues without cache)
- No exceptions thrown

ADR-026 conformity: Redis configuration updated"

# Commit 7: Refactoring usages
git add src/Application/LLMProxy.Application/**/*.cs
git commit -m "refactor(null-object): Replace null checks with Null Object Pattern

- Use GetByIdOrDefaultAsync instead of GetByIdAsync + null check
- Use GetQuotaOrUnlimitedAsync instead of GetQuotaAsync + null check
- Simplified logic in handlers and middlewares

ADR-026 conformity: Null checks eliminated"

# Commit 8: Tests
git add tests/LLMProxy.Domain.Tests/Entities/NullObjectTests.cs
git commit -m "test(null-object): Add comprehensive Null Object tests

- 8 tests covering all Null Objects
- Singleton pattern validation
- Behavior validation (inactive, unlimited, cache miss)
- Polymorphism validation

ADR-026 conformity: Test coverage complete"

# Commit 9: Documentation
git add README.md
git commit -m "docs(null-object): Document Null Object Pattern usage

- Table of available Null Objects
- Before/After code examples
- Redis degraded mode explanation
- Singleton pattern documentation

ADR-026 conformity: Documentation complete"
```

**Merge** :

```powershell
git checkout main
git merge --no-ff feature/026--implement-adr-026-null-object-pattern -m "Merge feature/026 - Implement ADR-026 Null Object Pattern"
git branch -d feature/026--implement-adr-026-null-object-pattern
```

**Action** : 9 commits atomiques, merge, supprimer feature branch.

---

## RÉFÉRENCE ADR

**ADR-026** : `docs/adr/026-null-object-pattern.adr.md`

**Principes clés** :
1. **Null Object** : Objet qui représente "rien" avec comportement neutre
2. **Polymorphisme** : Hérite de la classe de base (pas de traitement spécial)
3. **Singleton** : Instance statique unique (évite allocations multiples)
4. **Constructeur privé** : Empêche instanciation externe
5. **Comportement neutre** : Opérations no-op ou résultats par défaut sûrs

**Null Objects recommandés** :
- **NullTenant** : Tenant par défaut (toujours inactif)
- **UnlimitedQuotaLimit** : Quota illimité (toujours accepté)
- **NullCache** : Cache désactivé (toujours cache miss)

**Bénéfices** :
- Élimine NullReferenceException
- Simplifie le code (moins de `if (x == null)`)
- Centralise le comportement par défaut
- Facilite les tests (pas de mock de null)

---

## DURÉE ESTIMÉE

**Total** : 15h  
- NullTenant : 2h
- UnlimitedQuotaLimit : 1h30
- NullCache : 1h30
- Modifier TenantRepository : 1h30
- Modifier QuotaService : 1h30
- Modifier RedisInfrastructure : 1h
- Refactoring usages : 2h
- Tests unitaires : 2h
- README.md : 30 min
- Build/test/validation : 1h
- Commits/merge : 30 min

---

## NOTES

**Impacts sur architecture** :
- Élimination complète des null checks dans domain/application
- Simplification des handlers CQRS
- Mode dégradé automatique (Redis down → NullCache)

**Trade-offs** :
- ✅ Code plus propre et robuste
- ✅ Moins de bugs (NullReferenceException éliminés)
- ✅ Polymorphisme amélioré
- ⚠️ Légère complexité initiale (création Null Objects)
- ⚠️ Singleton Pattern (attention multi-threading si état mutable)

**Éviter abus** :
- Ne pas créer Null Object pour chaque classe
- Seulement si utilisation fréquente de null
- Comportement par défaut doit être clair et sûr

**Complémentarité** :
- Fonctionne bien avec Value Objects (ADR-024)
- Prépare Specification Pattern (ADR-028)
- Améliore Result Pattern (ADR-023)


## TRACKING
Début: 2025-12-22T08:25:49.5774015Z


Fin: 2025-12-22T08:32:13.3384100Z
Durée: 00:06:23

