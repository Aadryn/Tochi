# Tâche 024 - Implémenter ADR-022 : Idempotence

**Statut** : À faire  
**Priorité** : 🔴 CRITIQUE (P0)  
**Conformité cible** : ADR-022 de 0% → 95%  
**Dépendances** : Aucune

## CONTEXTE

**Analyse ADR-022** : `docs/ANALYSE_CONFORMITE_ADR-013-030.md` (lignes 1297-1391)  
**ADR** : `docs/adr/022-idempotence.adr.md`

**Conformité actuelle** : **0%** (aucune garantie d'idempotence)

**Problème identifié** :
- 🔴 **15 endpoints POST** sans Idempotency-Key header (création doublons si retry)
- 🔴 **Opérations quotas non-idempotentes** : `CurrentUsage += tokens` (facturation incorrecte)
- 🔴 **Aucun middleware d'idempotence** générique
- 🔴 **Pas de tracking requests traitées** (ProcessedRequestRepository)
- 🔴 **Retry sans idempotency key** (risque doublons après timeout)

**Risques sans idempotence** :
- 🔴 **CRITIQUE** : Création doublons (tenants, users, API keys) lors retry réseau
- 🔴 **CRITIQUE** : Facturation incorrecte (quotas comptés 2x)
- 🔴 Corruption données (état incohérent après retry partiel)
- 🔴 Debugging impossible (comportement imprévisible selon nb exécutions)
- 🟡 Coûts API LLM doublés (même requête envoyée 2x)

**Violations critiques détectées** :
```csharp
// ❌ V-IDP-001 : POST Create sans Idempotency-Key (15 endpoints)
[HttpPost]
public async Task<IActionResult> Create(CreateTenantCommand command, CancellationToken ct)
{
    var result = await _mediator.Send(command, ct);
    return Created($"/api/tenants/{result.Value}", result.Value);  // Doublon si retry !
}

// ❌ V-IDP-016 : Quotas non-idempotents
public void IncrementUsage(int tokens)
{
    CurrentUsage += tokens;  // Appelé 2x = tokens comptés 2x !
}
```

## OBJECTIF

Implémenter middleware d'idempotence générique + refactorer opérations critiques.

**Spécifications ADR-022** :
- Middleware vérifie header `Idempotency-Key` sur POST/PATCH
- Store Redis pour cacher réponses (TTL 24h)
- Endpoints POST/PATCH retournent réponse cachée si key déjà traitée
- Opérations quotas deviennent idempotentes (tracking transactions)
- IdempotencyKey ajouté aux commandes de création (CQRS)

## CRITÈRES DE SUCCÈS

### Fonctionnels
- [ ] Middleware d'idempotence actif sur Gateway
- [ ] Header `Idempotency-Key` obligatoire pour POST (400 Bad Request si absent)
- [ ] Réponses cachées dans Redis (24h TTL)
- [ ] Replay réponse identique si key déjà traitée
- [ ] Opérations quotas idempotentes (transaction ID unique)
- [ ] 15 endpoints POST refactorisés avec IdempotencyKey

### Techniques
- [ ] `IdempotencyMiddleware.cs` créé
- [ ] `IIdempotencyStore` interface + implémentation Redis
- [ ] `IdempotencyKey` ajouté aux Commands CQRS
- [ ] Quota tracking avec `_appliedTransactions HashSet<Guid>`
- [ ] Tests unitaires : middleware, store, quota idempotence

### Qualité
- [ ] **Build** : 0 erreurs, 0 warnings
- [ ] **Tests** : 10+ nouveaux tests idempotence
  - Middleware retourne 400 si key absente
  - Middleware retourne réponse cachée (2ème appel identique)
  - POST doublon avec même key → même résultat
  - Quota increment idempotent (même transaction ID ignorée)
  - TTL expire après 24h
- [ ] Tests existants : 100% passing (non-régression)
- [ ] Documentation README.md mise à jour

## ÉTAPES D'IMPLÉMENTATION

### 1. Créer interface IIdempotencyStore (30 min)

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Redis/Idempotency/IIdempotencyStore.cs`

```csharp
namespace LLMProxy.Infrastructure.Redis.Idempotency;

/// <summary>
/// Store pour cacher les réponses idempotentes (conforme ADR-022).
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Récupère une réponse cachée par clé d'idempotence.
    /// </summary>
    /// <returns>Réponse cachée ou null si inexistante.</returns>
    Task<CachedResponse?> GetAsync(string idempotencyKey, CancellationToken ct = default);

    /// <summary>
    /// Stocke une réponse avec TTL.
    /// </summary>
    Task SetAsync(string idempotencyKey, CachedResponse response, TimeSpan ttl, CancellationToken ct = default);
}

/// <summary>
/// Réponse HTTP cachée.
/// </summary>
public sealed record CachedResponse(
    int StatusCode,
    string ContentType,
    string Body,
    DateTime CreatedAt
);
```

**Action** : Créer interface et DTO.

---

### 2. Implémenter RedisIdempotencyStore (1h)

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Redis/Idempotency/RedisIdempotencyStore.cs`

```csharp
using StackExchange.Redis;
using System.Text.Json;

namespace LLMProxy.Infrastructure.Redis.Idempotency;

/// <summary>
/// Implémentation Redis du store d'idempotence.
/// Conforme à ADR-022 (Idempotence).
/// </summary>
public sealed class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly IDatabase _redis;
    private const string KeyPrefix = "idempotency:";

    public RedisIdempotencyStore(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task<CachedResponse?> GetAsync(string idempotencyKey, CancellationToken ct = default)
    {
        var key = GetRedisKey(idempotencyKey);
        var value = await _redis.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<CachedResponse>(value!);
    }

    public async Task SetAsync(
        string idempotencyKey,
        CachedResponse response,
        TimeSpan ttl,
        CancellationToken ct = default)
    {
        var key = GetRedisKey(idempotencyKey);
        var value = JsonSerializer.Serialize(response);

        await _redis.StringSetAsync(key, value, ttl);
    }

    private static string GetRedisKey(string idempotencyKey)
        => $"{KeyPrefix}{idempotencyKey}";
}
```

**Action** : Implémenter store avec Redis.

---

### 3. Créer IdempotencyMiddleware (2h)

**Fichier** : `src/Presentation/LLMProxy.Gateway/Middleware/IdempotencyMiddleware.cs`

```csharp
using LLMProxy.Infrastructure.Redis.Idempotency;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Middleware garantissant l'idempotence des requêtes POST/PATCH.
/// Conforme à ADR-022 (Idempotence).
/// </summary>
public sealed partial class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IIdempotencyStore _store;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    private const string IdempotencyKeyHeader = "Idempotency-Key";
    private static readonly string[] IdempotentMethods = { "POST", "PATCH" };

    public IdempotencyMiddleware(
        RequestDelegate next,
        IIdempotencyStore store,
        ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _store = store;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Ne traiter que POST/PATCH
        if (!IdempotentMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Vérifier présence header Idempotency-Key
        if (!context.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var keyHeader))
        {
            LogIdempotencyKeyMissing(_logger, context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "idempotency_key_required",
                message = $"Header '{IdempotencyKeyHeader}' is required for {context.Request.Method} requests"
            });
            return;
        }

        var idempotencyKey = keyHeader.ToString();

        // Vérifier si déjà traité
        var cachedResponse = await _store.GetAsync(idempotencyKey, context.RequestAborted);
        if (cachedResponse is not null)
        {
            // Replay de la réponse cachée
            LogIdempotencyReplay(_logger, idempotencyKey, cachedResponse.StatusCode);
            context.Response.StatusCode = cachedResponse.StatusCode;
            context.Response.ContentType = cachedResponse.ContentType;
            await context.Response.WriteAsync(cachedResponse.Body, context.RequestAborted);
            return;
        }

        // Capturer la réponse originale
        var originalBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        // Cacher la réponse pour futurs replays
        memoryStream.Position = 0;
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

        var response = new CachedResponse(
            context.Response.StatusCode,
            context.Response.ContentType ?? "application/json",
            responseBody,
            DateTime.UtcNow
        );

        await _store.SetAsync(idempotencyKey, response, TimeSpan.FromHours(24), context.RequestAborted);

        LogIdempotencyCached(_logger, idempotencyKey, context.Response.StatusCode);

        // Écrire la réponse originale au client
        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(originalBody, context.RequestAborted);
        context.Response.Body = originalBody;
    }

    [LoggerMessage(
        EventId = 6001,
        Level = LogLevel.Warning,
        Message = "Idempotency-Key header missing for {Method} {Path}")]
    private static partial void LogIdempotencyKeyMissing(ILogger logger, string method, string path);

    [LoggerMessage(
        EventId = 6002,
        Level = LogLevel.Information,
        Message = "Idempotency replay for key {IdempotencyKey} - Status {StatusCode}")]
    private static partial void LogIdempotencyReplay(ILogger logger, string idempotencyKey, int statusCode);

    [LoggerMessage(
        EventId = 6003,
        Level = LogLevel.Debug,
        Message = "Idempotency response cached for key {IdempotencyKey} - Status {StatusCode}")]
    private static partial void LogIdempotencyCached(ILogger logger, string idempotencyKey, int statusCode);
}
```

**Action** : Créer middleware avec capture réponse et cache Redis.

---

### 4. Enregistrer dans Program.cs (30 min)

**Fichier** : `src/Presentation/LLMProxy.Gateway/Program.cs`

**Modification** : Ajouter IdempotencyStore + Middleware.

```csharp
// Après builder.Services.AddRedis...
builder.Services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();

// ...

// APRÈS UseRouting, AVANT UseAuthorization
app.UseMiddleware<IdempotencyMiddleware>(); // ADR-022: Idempotence
app.UseAuthentication();
app.UseAuthorization();
```

**Action** : Enregistrer store et middleware dans pipeline.

---

### 5. Refactorer Quota Operations (idempotence) (1h30)

**Fichier** : `src/Domain/LLMProxy.Domain/Entities/QuotaLimit.cs`

**Modification** : Tracking transactions appliquées.

```csharp
public class QuotaLimit : BaseEntity
{
    // ... propriétés existantes ...

    private readonly HashSet<Guid> _appliedTransactions = new();

    /// <summary>
    /// Incrémente l'usage de manière idempotente.
    /// </summary>
    /// <param name="transactionId">ID unique de la transaction.</param>
    /// <param name="tokens">Nombre de tokens à incrémenter.</param>
    public void RecordUsage(Guid transactionId, long tokens)
    {
        Guard.AgainstEmptyGuid(transactionId, nameof(transactionId));
        Guard.AgainstNegativeOrZero(tokens, nameof(tokens));

        // Vérifier si transaction déjà appliquée (idempotence)
        if (_appliedTransactions.Contains(transactionId))
        {
            // Déjà appliqué - skip (idempotent)
            return;
        }

        // Première application
        CurrentUsage += tokens;
        _appliedTransactions.Add(transactionId);
        MarkAsModified();
    }
}
```

**Action** : Ajouter HashSet pour tracking transactions + méthode idempotente.

---

### 6. Ajouter IdempotencyKey aux Commands CQRS (2h)

**Fichiers** : Commands de création (15 total)

**Exemple** : `src/Application/Commands/CreateTenantCommand.cs`

```csharp
public sealed record CreateTenantCommand(
    Guid IdempotencyKey,  // NOUVEAU
    string Name,
    string Slug
) : IRequest<Result<Guid>>;

public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        // Vérifier si déjà créé (via IdempotencyKey unique constraint DB)
        var existing = await _unitOfWork.Tenants.GetByIdempotencyKeyAsync(request.IdempotencyKey, ct);
        if (existing is not null)
        {
            return Result<Guid>.Success(existing.Id); // Retourne résultat précédent
        }

        var tenant = Tenant.Create(request.IdempotencyKey, request.Name, request.Slug);
        await _unitOfWork.Tenants.AddAsync(tenant, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(tenant.Id);
    }
}
```

**Fichiers à modifier** (15 total) :
1. CreateTenantCommand
2. CreateUserCommand
3. CreateApiKeyCommand
4. CreateQuotaLimitCommand
5. CreateStatisticCommand
6. ... (11 autres)

**Action** : Ajouter `Guid IdempotencyKey` à chaque commande + handler vérifie si déjà traité.

---

### 7. Créer tests unitaires (2h)

**Fichier** : `tests/LLMProxy.Gateway.Tests/Middleware/IdempotencyMiddlewareTests.cs`

```csharp
using LLMProxy.Gateway.Middleware;
using LLMProxy.Infrastructure.Redis.Idempotency;
using NFluent;
using NSubstitute;
using Xunit;

namespace LLMProxy.Gateway.Tests.Middleware;

/// <summary>
/// Tests pour le middleware d'idempotence.
/// Conforme à ADR-022 (Idempotence).
/// </summary>
public sealed class IdempotencyMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Get_SkipsMiddleware()
    {
        // GET ne nécessite pas Idempotency-Key
    }

    [Fact]
    public async Task InvokeAsync_PostWithoutKey_Returns400()
    {
        // POST sans header → 400 Bad Request
    }

    [Fact]
    public async Task InvokeAsync_PostWithKey_FirstCall_CachesResponse()
    {
        // Premier appel → exécute + cache
    }

    [Fact]
    public async Task InvokeAsync_PostWithKey_SecondCall_ReplaysResponse()
    {
        // Deuxième appel même key → replay réponse cachée (identique)
    }

    [Fact]
    public async Task InvokeAsync_PatchWithoutKey_Returns400()
    {
        // PATCH sans header → 400 Bad Request
    }

    [Fact]
    public async Task InvokeAsync_CachedResponse_ExpiresTTL()
    {
        // Après 24h, réponse cachée expire (nouveau traitement)
    }

    [Fact]
    public void RecordUsage_SameTransactionId_IsIdempotent()
    {
        // QuotaLimit.RecordUsage() avec même transactionId → usage non doublé
        var quota = new QuotaLimit();
        var transactionId = Guid.NewGuid();

        quota.RecordUsage(transactionId, 100);
        Check.That(quota.CurrentUsage).IsEqualTo(100);

        quota.RecordUsage(transactionId, 100); // Replay
        Check.That(quota.CurrentUsage).IsEqualTo(100); // Inchangé (idempotent)
    }

    [Fact]
    public void RecordUsage_DifferentTransactionIds_Accumulates()
    {
        // Transactions différentes → usage cumulé
        var quota = new QuotaLimit();

        quota.RecordUsage(Guid.NewGuid(), 100);
        quota.RecordUsage(Guid.NewGuid(), 50);

        Check.That(quota.CurrentUsage).IsEqualTo(150);
    }
}
```

**Action** : Créer 8+ tests couvrant tous les scénarios.

---

### 8. Mettre à jour README.md (30 min)

**Fichier** : `README.md`

**Ajout** : Section "Idempotence (ADR-022)".

```markdown
### Idempotence (ADR-022)

Le proxy garantit l'idempotence des opérations critiques (créations, quotas).

**Middleware automatique** :
- Header `Idempotency-Key` **obligatoire** pour POST/PATCH
- Réponses cachées dans Redis (TTL 24h)
- Replay automatique si key déjà traitée

**Utilisation API** :
```bash
# POST avec Idempotency-Key (UUID v4)
curl -X POST https://api.example.com/tenants \
  -H "Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000" \
  -H "Content-Type: application/json" \
  -d '{"name": "Acme Corp", "slug": "acme"}'

# Même requête → même résultat (replay)
curl -X POST https://api.example.com/tenants \
  -H "Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000" \
  -H "Content-Type: application/json" \
  -d '{"name": "Acme Corp", "slug": "acme"}'
```

**Logs** :
- `[6001]` Idempotency-Key missing (Warning) → 400 Bad Request
- `[6002]` Idempotency replay (Information) → Réponse cachée retournée
- `[6003]` Idempotency cached (Debug) → Nouvelle réponse stockée
```

**Action** : Documenter comportement et utilisation.

---

### 9. Build, test et validation (1h)

**Commandes** :

```powershell
# Build
dotnet build --no-restore

# Tests
dotnet test --no-build --no-restore

# Validation : Vérifier sortie
# - 0 errors, 0 warnings
# - Tous tests passing (72 anciens + 8 nouveaux = 80 total)
```

**Action** :
1. Compiler sans erreurs ni warnings
2. Exécuter tests (100% passing)
3. Vérifier logs structurés (EventIds 6001-6003)

---

### 10. Commit et merge (30 min)

**Commits atomiques** :

```powershell
# Commit 1: Infrastructure idempotence
git add src/Infrastructure/LLMProxy.Infrastructure.Redis/Idempotency/*
git commit -m "feat(idempotence): Add IIdempotencyStore and Redis implementation

- Created IIdempotencyStore interface
- Implemented RedisIdempotencyStore with TTL support
- Added CachedResponse DTO

ADR-022 conformity: Infrastructure layer complete"

# Commit 2: Middleware
git add src/Presentation/LLMProxy.Gateway/Middleware/IdempotencyMiddleware.cs
git commit -m "feat(idempotence): Add IdempotencyMiddleware

- Middleware validates Idempotency-Key header (POST/PATCH)
- Returns 400 if key missing
- Caches responses in Redis (24h TTL)
- Replays cached responses automatically
- Structured logging (EventIds 6001-6003)

ADR-022 conformity: Middleware layer complete"

# Commit 3: Program.cs registration
git add src/Presentation/LLMProxy.Gateway/Program.cs
git commit -m "feat(idempotence): Register IdempotencyMiddleware in pipeline

- Registered IIdempotencyStore singleton
- Added IdempotencyMiddleware to pipeline (after UseRouting)

ADR-022 conformity: Integration complete"

# Commit 4: Quota idempotence
git add src/Domain/LLMProxy.Domain/Entities/QuotaLimit.cs
git commit -m "feat(idempotence): Make quota operations idempotent

- Added _appliedTransactions HashSet tracking
- RecordUsage() ignores duplicate transaction IDs
- Prevents double-counting tokens on retry

ADR-022 conformity: Domain layer idempotent"

# Commit 5: Commands refactoring (15 fichiers)
git add src/Application/Commands/*
git commit -m "feat(idempotence): Add IdempotencyKey to CQRS commands

- Added IdempotencyKey to 15 creation commands
- Handlers check existing resources by key
- Returns existing resource if already created

ADR-022 conformity: Application layer idempotent"

# Commit 6: Tests
git add tests/LLMProxy.Gateway.Tests/Middleware/IdempotencyMiddlewareTests.cs
git commit -m "test(idempotence): Add IdempotencyMiddleware tests

- 8 tests covering all scenarios
- Middleware behavior validation
- Quota idempotence verification

ADR-022 conformity: Test coverage complete"

# Commit 7: Documentation
git add README.md
git commit -m "docs(idempotence): Document idempotence behavior

- Added Idempotence section to README
- API usage examples with Idempotency-Key header
- Logging EventIds documentation

ADR-022 conformity: Documentation complete"
```

**Merge** :

```powershell
git checkout main
git merge --no-ff feature/024--implement-adr-022-idempotence -m "Merge feature/024 - Implement ADR-022 Idempotence"
git branch -d feature/024--implement-adr-022-idempotence
```

**Action** : 7 commits atomiques, merge, supprimer feature branch.

---

## RÉFÉRENCE ADR

**ADR-022** : `docs/adr/022-idempotence.adr.md`

**Principes clés** :
1. **Idempotency Key** : Clé unique fournie par client (UUID v4)
2. **Store Redis** : Cache réponses 24h, replay identique si retry
3. **Middleware générique** : Appliqué automatiquement POST/PATCH
4. **Tracking transactions** : HashSet pour opérations répétées
5. **CQRS compatible** : IdempotencyKey dans Commands

**Erreurs transitoires gérées par retry (ADR-033)** :
- Timeout réseau
- 503 Service Unavailable
- 429 Too Many Requests

**Idempotence garantit** :
- Retry safe (même résultat si appelé N fois)
- Pas de doublons (création unique par key)
- Quotas corrects (tokens comptés 1 seule fois)

**Méthodes HTTP naturellement idempotentes** :
- GET : Lecture (safe + idempotent)
- PUT : Remplacement complet (idempotent)
- DELETE : Suppression (idempotent - delete 2x = même état)
- PATCH : Modification partielle (nécessite key)
- POST : Création (NON idempotent → nécessite key)

---

## DURÉE ESTIMÉE

**Total** : 12h  
- IIdempotencyStore interface : 30 min
- RedisIdempotencyStore : 1h
- IdempotencyMiddleware : 2h
- Program.cs registration : 30 min
- Quota operations refactoring : 1h30
- Commands CQRS refactoring : 2h (15 fichiers)
- Tests unitaires : 2h
- README.md : 30 min
- Build/test/validation : 1h
- Commits/merge : 30 min

---

## NOTES

**Impacts sur architecture** :
- Dépendance Redis obligatoire (déjà présent)
- Pipeline HTTP enrichi (nouveau middleware après Routing)
- Domain entities avec tracking état (HashSet<Guid>)
- Commands CQRS enrichies (nouveau paramètre IdempotencyKey)

**Trade-offs** :
- ✅ Garantie robustesse production (no doublons)
- ✅ Simplicité implémentation (middleware transparent)
- ⚠️ Mémoire Redis consommée (cache 24h par request)
- ⚠️ Latence ajoutée (~5ms lookup Redis par request)

**Migration existant** :
- Clients existants devront ajouter header `Idempotency-Key`
- Breaking change API (400 si header absent)
- Migration progressive possible (middleware optionnel par route)

**Monitoring recommandé** :
- Taux replay idempotence (nombre cache hits)
- Taille store Redis (monitoring clés `idempotency:*`)
- Latence middleware (time to lookup + cache)
- Erreurs 400 (clients sans header)


## TRACKING
Début: 2025-12-22T00:03:24.0261346Z

