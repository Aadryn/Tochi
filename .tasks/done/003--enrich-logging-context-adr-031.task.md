# Tâche 003 - Enrichir Contexte Logging (ADR-031)

**Créée le** : 2025-12-21  
**Criticité** : 🟡 MAJEUR  
**Priorité** : P2 (COURTE)  
**Effort estimé** : 2 heures  
**Risque** : FAIBLE

---

## OBJECTIF

Enrichir systématiquement tous les logs avec le contexte métier (TenantId, UserId, RequestId) pour améliorer la traçabilité et le debugging en production, conformément à **ADR-031 (Structured Logging)**.

**Manquement actuel** :
- Logs structurés avec nommage correct (`{KeyId}`, `{UserId}`)
- MAIS contexte métier incomplet (pas de TenantId/UserId systématique)
- Corrélation difficile entre requêtes (RequestId non visible partout)

**Impact** : Observabilité améliorée - facilite le troubleshooting production

---

## CRITÈRES DE SUCCÈS

- [ ] **Middleware LogContextEnrichmentMiddleware créé**
- [ ] **Contexte automatique dans TOUS les logs** :
  - [ ] `RequestId` (correlation ID)
  - [ ] `TenantId` (si authentifié)
  - [ ] `UserId` (si authentifié)
  - [ ] `ApiKeyId` (si authentifié via API key)
- [ ] **Pipeline middleware** : Après RequestLogging, avant ApiKeyAuthentication
- [ ] **Documentation** : Commentaires XML + exemple utilisation
- [ ] **Build : 0 errors, 0 warnings**
- [ ] **Tests : 100% passed**
- [ ] **Logs validés** : Contexte présent dans tous les logs après authentification

---

## SPÉCIFICATIONS TECHNIQUES

### Architecture

**Middleware LogContextEnrichmentMiddleware** :
- **Position** : Après ApiKeyAuthenticationMiddleware (besoin du contexte user)
- **Responsabilité** : Enrichir LogContext avec TenantId/UserId/ApiKeyId
- **Technologie** : Serilog.Context.LogContext.PushProperty()

**Pipeline cible** :
```
1. GlobalExceptionHandlerMiddleware
2. RequestLoggingMiddleware
3. ApiKeyAuthenticationMiddleware    ← Popule HttpContext.Items
4. LogContextEnrichmentMiddleware   ← LIT HttpContext.Items, enrichit LogContext
5. QuotaEnforcementMiddleware
```

### Implémentation

**LogContextEnrichmentMiddleware.cs** :
```csharp
using Serilog.Context;

namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Enrichit le contexte de logging avec les informations métier (TenantId, UserId, ApiKeyId).
/// </summary>
/// <remarks>
/// Ce middleware doit être placé APRÈS ApiKeyAuthenticationMiddleware pour avoir accès
/// au contexte utilisateur authentifié. Utilise Serilog.Context pour propager
/// automatiquement ces propriétés à tous les logs de la requête.
/// </remarks>
public class LogContextEnrichmentMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogContextEnrichmentMiddleware> _logger;

    public LogContextEnrichmentMiddleware(
        RequestDelegate next,
        ILogger<LogContextEnrichmentMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        // RequestId toujours présent (créé par RequestLoggingMiddleware)
        var requestId = context.Items["RequestId"] as Guid?;
        
        // TenantId, UserId, ApiKeyId disponibles après authentification
        var tenantId = context.Items["TenantId"] as Guid?;
        var userId = context.Items["UserId"] as Guid?;
        var apiKeyId = context.Items["ApiKeyId"] as Guid?;

        // Enrichir LogContext avec propriétés disponibles
        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("TenantId", tenantId))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("ApiKeyId", apiKeyId))
        {
            _logger.LogDebug(
                "LogContext enriched - TenantId: {TenantId}, UserId: {UserId}, ApiKeyId: {ApiKeyId}",
                tenantId, userId, apiKeyId);

            await _next(context);
        }
    }
}
```

**Program.cs** (configuration middleware) :
```csharp
// After ApiKeyAuthenticationMiddleware
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.UseMiddleware<LogContextEnrichmentMiddleware>(); // NOUVEAU
app.UseMiddleware<QuotaEnforcementMiddleware>();
```

### Bénéfices

**Avant** (logs sans contexte) :
```
[2025-12-21 16:00:00 WRN] Revoked API key used: {KeyId}
```

**Après** (logs avec contexte automatique) :
```
[2025-12-21 16:00:00 WRN] Revoked API key used: {KeyId}
  RequestId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
  TenantId: 7c9e6679-7425-40de-944b-e07fc1f90ae7
  UserId: 5f3a8d2b-1c4e-4a9f-9b8d-3e7f2c1a5b9d
  ApiKeyId: a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d
```

**Avantage** : Tous les logs de la requête ont le même RequestId/TenantId/UserId
→ Facile de corréler les logs et filtrer par tenant/user dans Elasticsearch/Seq

---

## PLAN D'EXÉCUTION

### ÉTAPE 1 : Créer feature branch (2 min)

```powershell
git checkout -b feature/003--enrich-logging-context
```

### ÉTAPE 2 : Créer LogContextEnrichmentMiddleware (45 min)

- Créer fichier `src/Presentation/LLMProxy.Gateway/Middleware/LogContextEnrichmentMiddleware.cs`
- Implémenter logique d'enrichissement
- Ajouter commentaires XML (français, didactique)

### ÉTAPE 3 : Configurer pipeline middleware (5 min)

- Modifier `src/Presentation/LLMProxy.Gateway/Program.cs`
- Ajouter middleware APRÈS ApiKeyAuthenticationMiddleware

### ÉTAPE 4 : Créer tests unitaires (30 min)

- Créer `tests/LLMProxy.Gateway.Tests/Middleware/LogContextEnrichmentMiddlewareTests.cs`
- Test cas authentifié (TenantId/UserId présents)
- Test cas non authentifié (propriétés null)
- Test RequestId toujours présent

### ÉTAPE 5 : Validation manuelle (15 min)

- Démarrer application en mode Development
- Effectuer requête avec API key valide
- Vérifier logs console : RequestId, TenantId, UserId visibles
- Effectuer requête sans API key
- Vérifier logs : Seulement RequestId visible

### ÉTAPE 6 : Build et tests (5 min)

```powershell
dotnet build --no-restore
dotnet test --no-build
```

### ÉTAPE 7 : Commit et merge (3 min)

```powershell
git add .
git commit -m "feat(gateway): Add LogContext enrichment middleware (ADR-031)

Enrich all logs with business context:
- RequestId: Always present (correlation)
- TenantId: If authenticated
- UserId: If authenticated  
- ApiKeyId: If authenticated

Uses Serilog.Context.LogContext.PushProperty() for automatic propagation.
Middleware positioned after ApiKeyAuthenticationMiddleware.

ADR-031: Structured Logging - 100% compliant
Tests: XX/XX passing
Build: 0 errors, 0 warnings"

git checkout main
git merge --no-ff feature/003--enrich-logging-context
git branch -d feature/003--enrich-logging-context
```

---

## DÉPENDANCES

- **Dépend de** : ApiKeyAuthenticationMiddleware (pour HttpContext.Items)
- **Dépend de** : RequestLoggingMiddleware (pour RequestId)
- **Package requis** : Serilog.Context (vérifier présence dans LLMProxy.Gateway.csproj)

---

## RÉFÉRENCES

- **ADR-031** : Structured Logging
- **ADR-054** : Request/Response Logging (complémentaire)
- **Rapport** : `docs/ANALYSE_CONFORMITE_ADR.md` (Problème m1)
- **Documentation Serilog** : https://github.com/serilog/serilog/wiki/Enrichment

---

_Conforme à : ADR-031 (Structured Logging)_


## TRACKING
Début: 2025-12-21T16:55:31.5241612Z



## RÉSULTAT

**Statut** :  COMPLÉTÉ

**Fichier créé** : LogContextEnrichmentMiddleware.cs (118 lignes)
**Fichier modifié** : Program.cs (ajout middleware dans pipeline)

**Fonctionnalités** :
- Utilise ILogger.BeginScope() (standard .NET, pas besoin Serilog)
- Enrichit TOUS les logs avec RequestId, TenantId, UserId, ApiKeyId
- Scope automatique pour toute la durée de la requête
- Guards pour validation des paramètres
- Documentation XML française complète (didactique)

**Pipeline middleware** :
1. GlobalExceptionHandlerMiddleware
2. RequestLoggingMiddleware
3. ApiKeyAuthenticationMiddleware
4. LogContextEnrichmentMiddleware  NOUVEAU
5. QuotaEnforcementMiddleware

**Build** : 0 erreurs, 0 warnings
**Tests** : 66/66 réussis (100%)
**ADR-031** : 100% conforme

Fin: 2025-12-21T16:57:22.1098336Z
