---
id: 004
title: Ajouter CancellationToken à tous les middlewares
concerns: middlewares, async, performance
type: refactoring
priority: major
effort: small
risk: low
value: high
dependencies: []
status: to-do
created: 2025-12-21
---

# Ajouter CancellationToken à tous les middlewares

## 🎯 Objectif

Ajouter le paramètre `CancellationToken` à tous les `InvokeAsync` des middlewares pour respecter les bonnes pratiques async/await et permettre annulation gracieuse des requêtes.

**Amélioration visée :**
- **Performance** : Libérer ressources immédiatement si client annule requête
- **Robustesse** : Respecter mécanisme standard .NET de cancellation
- **Conformité** : Suivre guidelines Microsoft pour middleware ASP.NET Core

**Bénéfice mesurable :** 
- Réduction temps de traitement pour requêtes annulées
- Libération immédiate des connexions DB/Redis

## 📊 Contexte

### Problème Identifié

- **Type** : Performance / Robustesse / Conformité Standards
- **Localisation** : Tous les middlewares:
  - `ApiKeyAuthenticationMiddleware.cs:29`
  - `QuotaEnforcementMiddleware.cs:35`
  - `StreamInterceptionMiddleware.cs:45`
  - `RequestLoggingMiddleware.cs:30`
- **Description Factuelle** : Aucun middleware ne passe le `CancellationToken` aux opérations async, empêchant annulation gracieuse quand le client ferme la connexion.
- **Impact Actuel** : 
  - Requêtes annulées continuent de consommer ressources (DB, Redis, compute)
  - Impossible de détecter déconnexion client en cours de traitement
  - Non-respect des guidelines Microsoft ASP.NET Core
- **Preuve** :

```csharp
// ❌ Tous les middlewares ont cette signature
public async Task InvokeAsync(HttpContext context)  // ⚠️ Manque CancellationToken
{
    // Opérations async sans cancellation
    var apiKey = await _repository.GetByKeyHashAsync(keyHash);  // ⚠️ Pas de CT
    await _quotaService.CheckQuotaAsync(userId);  // ⚠️ Pas de CT
    await context.Response.WriteAsJsonAsync(data);  // ⚠️ Pas de CT
}
```

**Impact mesurable :**
- Si client annule requête après 100ms → Traitement continue jusqu'à la fin (gaspillage)
- Requête DB/Redis non annulées → Charge inutile sur infrastructure

### Conformité Standards

**Instructions Applicables :**
- `.github/instructions/csharp.async.instructions.md` - **CancellationToken obligatoire**
- Microsoft ASP.NET Core Middleware Guidelines

**Citation csharp.async.instructions.md :**
> **RÈGLE 3 : Toutes les méthodes async publiques DOIVENT accepter un CancellationToken**
> ```csharp
> // ✅ CORRECT
> public async Task ProcessAsync(CancellationToken cancellationToken)
> ```

**Vérification de Conformité :**
- [x] Respecte instructions projet (csharp.async.instructions.md)
- [x] Suit guidelines Microsoft
- [x] Aucun standard violé

## 🔧 Implémentation

### Approche de Refactoring

**Stratégie :** Ajouter paramètre `CancellationToken` et le passer à toutes opérations async dans le pipeline

**Principe appliqué :**
- **Propagation de cancellation** : Token propagé à toutes opérations async
- **Graceful Shutdown** : Respecter demande d'annulation client
- **Performance** : Éviter traitement inutile

### Fichiers à Modifier

- `src/Presentation/LLMProxy.Gateway/Middleware/ApiKeyAuthenticationMiddleware.cs`
- `src/Presentation/LLMProxy.Gateway/Middleware/QuotaEnforcementMiddleware.cs`
- `src/Presentation/LLMProxy.Gateway/Middleware/StreamInterceptionMiddleware.cs`
- `src/Presentation/LLMProxy.Gateway/Middleware/RequestLoggingMiddleware.cs`

### Modifications Détaillées

#### Étape 1 : ApiKeyAuthenticationMiddleware - Ajouter CancellationToken

**État actuel (AVANT) :**
```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Skip authentication for health checks
    if (context.Request.Path.StartsWithSegments("/health"))
    {
        await _next(context);
        return;
    }

    var apiKey = ExtractApiKey(context);

    if (string.IsNullOrEmpty(apiKey))
    {
        // ...
        return;
    }

    var keyHash = ComputeHash(apiKey);
    var apiKeyEntity = await _repository.GetByKeyHashAsync(keyHash);
    // ...
}
```

**État cible (APRÈS) :**
```csharp
public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)  // ✅ AJOUTÉ
{
    // Skip authentication for health checks
    if (context.Request.Path.StartsWithSegments("/health"))
    {
        await _next(context);
        return;
    }

    var apiKey = ExtractApiKey(context);

    if (string.IsNullOrEmpty(apiKey))
    {
        await context.Response.WriteAsJsonAsync(
            new { error = "API key is required" },
            cancellationToken);  // ✅ AJOUTÉ
        return;
    }

    var keyHash = ComputeHash(apiKey);
    var apiKeyEntity = await _repository.GetByKeyHashAsync(keyHash, cancellationToken);  // ✅ AJOUTÉ

    if (apiKeyEntity == null || !apiKeyEntity.IsActive)
    {
        await context.Response.WriteAsJsonAsync(
            new { error = "Invalid or inactive API key" },
            cancellationToken);  // ✅ AJOUTÉ
        return;
    }

    // Set user context
    context.Items["UserId"] = apiKeyEntity.UserId;
    context.Items["TenantId"] = apiKeyEntity.TenantId;
    context.Items["ApiKeyId"] = apiKeyEntity.Id;

    await _next(context);  // Note: _next ne prend pas de CT (géré par framework)
}
```

**Justification :**
- Token propagé à toutes opérations async
- Default parameter pour compatibilité
- Framework ASP.NET Core gère cancellation de `_next()`

**Validation de l'étape :**
- [ ] Build réussi
- [ ] Comportement identique si pas de cancellation
- [ ] Requête annulée si client déconnecte

#### Étape 2 : QuotaEnforcementMiddleware - Ajouter CancellationToken

**État cible :**
```csharp
public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
{
    // Extract user context
    if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
    {
        await context.Response.WriteAsJsonAsync(
            new { error = "Authentication required" },
            cancellationToken);
        return;
    }

    // Check quota
    var quotaCheck = await _quotaService.CheckQuotaAsync(userId, cancellationToken);

    if (!quotaCheck.IsAllowed)
    {
        context.Response.Headers["X-RateLimit-Limit"] = quotaCheck.Usage?.Limit.ToString() ?? "0";
        // ...
        await context.Response.WriteAsJsonAsync(
            new { /* ... */ },
            cancellationToken);
        return;
    }

    await _next(context);

    // Record usage
    if (context.Items.TryGetValue("TokensUsed", out var tokensObj) && tokensObj is int tokens)
    {
        await _quotaService.RecordUsageAsync(userId, tokens, cancellationToken);
    }
}
```

**Validation :**
- [ ] Quota check annulable
- [ ] Usage recording annulable
- [ ] Pas de leak si annulation

#### Étape 3 : StreamInterceptionMiddleware - Ajouter CancellationToken

**État cible :**
```csharp
public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
{
    var isStreaming = await IsStreamingRequest(context, cancellationToken);

    if (!isStreaming)
    {
        await _next(context);
        return;
    }

    var originalBodyStream = context.Response.Body;

    try
    {
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        await ProcessStreamingResponse(context, responseBody, originalBodyStream, cancellationToken);
    }
    finally
    {
        context.Response.Body = originalBodyStream;
    }
}

private async Task<bool> IsStreamingRequest(HttpContext context, CancellationToken cancellationToken)
{
    if (!context.Request.HasJsonContentType())
        return false;

    context.Request.EnableBuffering();
    
    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
    var body = await reader.ReadToEndAsync(cancellationToken);  // ✅ AJOUTÉ CT
    
    // ... reste
}

private async Task ProcessStreamingResponse(
    HttpContext context, 
    Stream responseBody, 
    Stream originalStream,
    CancellationToken cancellationToken)
{
    // ...
    await foreach (var line in reader.ReadAllLinesAsync(cancellationToken))  // ✅ AJOUTÉ CT
    {
        // ...
        await originalStream.WriteAsync(lineBytes, cancellationToken);  // ✅ AJOUTÉ CT
        await originalStream.FlushAsync(cancellationToken);  // ✅ AJOUTÉ CT
    }
}
```

**Validation :**
- [ ] Streaming annulable proprement
- [ ] Pas de corruption de données si annulation
- [ ] Logs corrects si annulation

#### Étape 4 : RequestLoggingMiddleware - Ajouter CancellationToken

**État cible :**
```csharp
public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
{
    var requestId = Guid.NewGuid();
    context.Items["RequestId"] = requestId;

    var stopwatch = Stopwatch.StartNew();

    try
    {
        await _next(context);
    }
    finally
    {
        stopwatch.Stop();

        _logger.LogInformation(
            "Request completed: {RequestId} | {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
```

**Note :** Ce middleware n'a pas d'opérations async qui bénéficient du CT, mais le paramètre doit être présent pour conformité.

**Validation :**
- [ ] Logging fonctionne même si requête annulée
- [ ] Durée correcte même avec annulation

### Considérations Techniques

**Points d'Attention :**
- `_next(context)` ne prend PAS de CancellationToken (géré par framework)
- `context.RequestAborted` déjà disponible mais doit être passé explicitement
- Default parameter `= default` pour rétrocompatibilité

**Bonnes Pratiques :**
- Toujours passer CT aux méthodes qui l'acceptent
- Ne pas swallow `OperationCanceledException` (laisser propager)
- Logger si opération annulée (niveau Information)

**Pièges à Éviter :**
- Ne pas créer nouveau CancellationToken (utiliser celui reçu)
- Ne pas ignorer CT dans opérations longues (DB, HTTP)
- Ne pas catch `OperationCanceledException` sans relancer

## ✅ Critères de Validation

### Tests de Non-Régression

**Tests Obligatoires :**
- [ ] Tous tests existants passent
- [ ] Comportement identique si requête complète normalement
- [ ] Nouveau test : Annulation pendant auth → OperationCanceledException
- [ ] Nouveau test : Annulation pendant quota check → OperationCanceledException
- [ ] Nouveau test : Annulation pendant streaming → Stream fermé proprement

**Tests de Performance :**
- [ ] Mesurer temps de libération ressources après annulation
- [ ] Vérifier pas de connexions DB/Redis orphelines

**Validation Fonctionnelle :**
- [ ] Client ferme connexion → Traitement s'arrête immédiatement
- [ ] Logs indiquent annulation propre
- [ ] Pas de corruption données

### Amélioration des Piliers

**Piliers Améliorés :**
- [x] **Performance** : Libération immédiate ressources si annulation
- [x] **Robustesse** : Gestion propre des déconnexions client
- [x] **Conformité** : Respect guidelines Microsoft + instructions projet
- [x] **Maintenabilité** : Code suit patterns standards .NET

**Piliers Non Dégradés :**
- [x] Simplicité maintenue (ajout minimal)
- [x] Fonctionnalité préservée
- [x] Sécurité identique

### Conformité et Documentation

- [x] Respecte `.github/instructions/csharp.async.instructions.md`
- [x] Suit Microsoft Middleware Guidelines
- [ ] Documentation XML mise à jour pour InvokeAsync
- [ ] Git commit : `refactor(middlewares): add CancellationToken parameter to all InvokeAsync`

### Plan de Rollback

**En cas de problème :**
1. `git revert <commit-hash>` (rollback simple)
2. Vérifier tests passent
3. Aucun risque de breaking change (default parameter)

## 📈 Métriques d'Amélioration

**Avant Refactoring :**
- Middlewares avec CancellationToken : 0/4 (0%)
- Requêtes annulées continuant traitement : 100%
- Conformité csharp.async.instructions.md : Non

**Après Refactoring (attendu) :**
- Middlewares avec CancellationToken : 4/4 (100%)
- Requêtes annulées arrêtées immédiatement : 100%
- Conformité csharp.async.instructions.md : Oui

**Bénéfice Mesurable :**
- Réduction temps traitement requêtes annulées : -90% (estimation)
- Réduction charge DB/Redis inutile : Significative
- Conformité standards : +100%

## 🔗 Références

**Microsoft Documentation :**
- [ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write)
- [CancellationToken Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)

**Instructions Projet :**
- `.github/instructions/csharp.async.instructions.md` - Règle 3 : CancellationToken obligatoire

**Patterns :**
- Cooperative Cancellation Pattern
- Async/Await Best Practices


##  TRACKING

Début: 2025-12-21T06:23:46.7585634Z


Fin: 2025-12-21T06:24:33.3868514Z
Durée: 00:00:46

##  VALIDATION

- [x] CancellationToken ajouté aux 4 middlewares
- [x] Documentation XML en français ajoutée
- [x] Paramètre optionnel (default) pour compatibilité
- [x] Build sans warning
- [x] Conformité csharp.async.instructions.md

