# Tâche 001 - Implémenter Request/Response Logging (ADR-054)

**Créée le** : 2025-12-21  
**Criticité** : 🔴🔴 CRITIQUE  
**Priorité** : P1 (IMMÉDIATE)  
**Effort estimé** : 2-3 heures  
**Risque** : FAIBLE

---

## OBJECTIF

Implémenter middleware de logging request/response pour conformité **ADR-054** et améliorer l'observabilité en production.

**Manquement actuel** :
- Pas de logging structuré des requêtes HTTP (méthode, path, headers)
- Pas de logging des réponses (status code, durée)
- Pas de correlation IDs (RequestId)
- Pas de masquage des données sensibles (API keys, tokens)

**Impact** : Observabilité limitée - difficile de tracer les problèmes en production

---

## CRITÈRES DE SUCCÈS

- [ ] **Middleware créé** : `RequestLoggingMiddleware.cs` dans `LLMProxy.Gateway/Middleware/`
- [ ] **Logging requêtes** : Méthode HTTP, Path, QueryString, Headers (sanitisés)
- [ ] **Logging réponses** : Status code, durée (ms), taille (bytes)
- [ ] **Correlation ID** : RequestId généré et propagé dans tous les logs
- [ ] **Masquage sensible** : API keys, Authorization headers masqués
- [ ] **Configuration** : Middleware enregistré dans `Program.cs` (après GlobalExceptionHandler, avant ApiKeyAuthentication)
- [ ] **Tests** : Tests unitaires pour le middleware
- [ ] **Build : 0 errors, 0 warnings**
- [ ] **Tests : 100% passed**

---

## SPÉCIFICATIONS TECHNIQUES

### Fonctionnalités Obligatoires

1. **Génération RequestId** :
   ```csharp
   var requestId = Guid.NewGuid().ToString("N");
   context.Items["RequestId"] = requestId;
   ```

2. **Log Requête (début)** :
   ```csharp
   _logger.LogInformation(
       "HTTP {Method} {Path}{QueryString} started - RequestId: {RequestId}",
       context.Request.Method,
       context.Request.Path,
       context.Request.QueryString,
       requestId);
   ```

3. **Log Réponse (fin)** :
   ```csharp
   _logger.LogInformation(
       "HTTP {Method} {Path} completed in {DurationMs}ms with status {StatusCode} - RequestId: {RequestId}",
       context.Request.Method,
       context.Request.Path,
       duration,
       context.Response.StatusCode,
       requestId);
   ```

4. **Masquage données sensibles** :
   - Header `X-API-Key` → masqué (garder 4 premiers + 4 derniers caractères)
   - Header `Authorization` → masqué complètement
   - QueryString avec `apikey=` → masqué

5. **Performance** :
   - Mesure durée avec `Stopwatch` (plus précis que DateTime)
   - Logging async si possible

### Ordre Middleware (Program.cs)

```csharp
// 1. GlobalExceptionHandlerMiddleware (catch all exceptions)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 2. RequestLoggingMiddleware (log all requests) ← NOUVEAU
app.UseMiddleware<RequestLoggingMiddleware>();

// 3. ApiKeyAuthenticationMiddleware (authenticate)
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

// 4. QuotaEnforcementMiddleware (check quotas)
app.UseMiddleware<QuotaEnforcementMiddleware>();
```

---

## PLAN D'EXÉCUTION

### ÉTAPE 1 : Créer feature branch (2 min)

```powershell
git checkout -b feature/001--implement-request-logging
```

### ÉTAPE 2 : Créer RequestLoggingMiddleware (45 min)

Créer `src/Presentation/LLMProxy.Gateway/Middleware/RequestLoggingMiddleware.cs`

**Structure** :
- Constructor injection : `ILogger<RequestLoggingMiddleware>`, `RequestDelegate next`
- Méthode `InvokeAsync(HttpContext context, CancellationToken cancellationToken)`
- Méthode privée `SanitizeHeaders(IHeaderDictionary headers)`
- Méthode privée `SanitizeQueryString(QueryString queryString)`

### ÉTAPE 3 : Configurer dans Program.cs (5 min)

Ajouter `app.UseMiddleware<RequestLoggingMiddleware>()` après GlobalExceptionHandler

### ÉTAPE 4 : Créer tests unitaires (60 min)

Créer `tests/LLMProxy.Gateway.Tests/Middleware/RequestLoggingMiddlewareTests.cs`

**Tests requis** :
- `InvokeAsync_LogsRequestStart_WithRequestId`
- `InvokeAsync_LogsResponseEnd_WithDuration`
- `InvokeAsync_MasksApiKeyHeader_InLogs`
- `InvokeAsync_MasksAuthorizationHeader_InLogs`
- `InvokeAsync_MasksApiKeyQueryParam_InLogs`
- `InvokeAsync_PropagatesRequestId_ToContext`
- `InvokeAsync_HandlesException_LogsError`

### ÉTAPE 5 : Build et tests (10 min)

```powershell
dotnet build --no-restore
dotnet test --no-build
```

### ÉTAPE 6 : Commit et merge (5 min)

```powershell
git add .
git commit -m "feat(gateway): Implement RequestLoggingMiddleware (ADR-054)"
git checkout main
git merge --no-ff feature/001--implement-request-logging
git branch -D feature/001--implement-request-logging
```

---

## DÉPENDANCES

- **Bloqué par** : Aucune
- **Bloquant pour** : m1 (Contexte Logging enrichi)

---

## RÉFÉRENCES

- **ADR-054** : Request/Response Logging
- **ADR-031** : Structured Logging
- **Rapport** : `docs/ANALYSE_CONFORMITE_ADR.md` (Problème M1)

---

_Conforme à : ADR-054, ADR-031, ADR-043 (Exception Handling)_


## TRACKING
Début: 2025-12-21T16:44:31.3966987Z



## RÉSULTATS

**Fin:** 2025-12-21T16:46:16.1099096Z
**Durée:** 00:01:44

**Améliorations apportées:**
-  Masquage QueryString (apikey=  ***MASKED***)
-  Masquage X-API-Key (4 premiers + 4 derniers chars conservés)
-  Masquage Authorization (complètement masqué)
-  Niveau de log adapté selon status code (Info/Warning/Error)
-  Documentation XML complète en français
-  Support OpenTelemetry (ActivitySource)

**Build:**
- Résultat: SUCCÈS
- Erreurs: 0
- Warnings: 0 (2 pré-existants dans TenantTests.cs)

**Tests:**
- Total: 66/66 (100%)
- Échecs: 0
- Ignorés: 0

**Conformité ADR-054:**
 CONFORME - Request/Response Logging avec masquage données sensibles

