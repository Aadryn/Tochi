---
id: 005
title: Corriger ordre pipeline middlewares (Auth avant Quota)
concerns: middlewares, sécurité, architecture
type: refactoring
priority: major
effort: small
risk: medium
value: high
dependencies: []
status: to-do
created: 2025-12-21
---

# Corriger ordre pipeline middlewares (Auth avant Quota)

## 🎯 Objectif

Réorganiser l'ordre des middlewares dans le pipeline pour placer Authentication AVANT QuotaEnforcement et éviter exploitation du système de quota par utilisateurs non authentifiés.

**Amélioration visée :**
- **Sécurité** : Bloquer requêtes non authentifiées avant consommation de ressources
- **Architecture** : Pipeline middleware logique (Auth → Authz → Business Logic)
- **Performance** : Éviter traitement inutile pour requêtes invalides

**Bénéfice mesurable :** 
- Zéro requête non auth ne consomme quota
- Ordre conforme aux best practices ASP.NET Core

## 📊 Contexte

### Problème Identifié

- **Type** : Sécurité / Architecture
- **Localisation** : `src/Presentation/LLMProxy.Gateway/Program.cs:25-30`
- **Description Factuelle** : Le pipeline actuel exécute RequestLogging → QuotaEnforcement → ApiKeyAuthentication, permettant à des requêtes sans API key de consommer du temps CPU pour le quota check avant d'être rejetées.
- **Impact Actuel** : 
  - Requêtes non auth consomment ressources (DB query quota)
  - Potentielle attaque DoS en flood de requêtes sans API key
  - Non-respect pattern de sécurité standard (Auth first)
- **Preuve** :

```csharp
// ❌ ORDRE INCORRECT - Quota avant Authentication
app.UseMiddleware<RequestLoggingMiddleware>();      // 1. OK (logging first)
app.UseMiddleware<QuotaEnforcementMiddleware>();     // 2. ⚠️ AVANT AUTH!
app.UseMiddleware<ApiKeyAuthenticationMiddleware>(); // 3. ⚠️ APRÈS QUOTA!
app.UseMiddleware<StreamInterceptionMiddleware>();   // 4. OK
```

**Scénario d'exploitation :**
```bash
# Attaquant envoie 1000 requêtes/s SANS API key
curl http://gateway/v1/chat/completions  # Pas d'API key

# Pipeline actuel:
# 1. RequestLogging ✅ (log request)
# 2. QuotaEnforcement ⚠️ (query DB pour userId null → erreur)
# 3. ApiKeyAuth ❌ (rejette requête)
# → 1000 requêtes/s font 1000 DB queries inutiles avant rejet
```

### Conformité Standards

**Instructions Applicables :**
- `.github/instructions/csharp.standards.instructions.md` - Architecture et sécurité
- ASP.NET Core Middleware Ordering Best Practices

**Standards ASP.NET Core :**
> **Ordre recommandé :**
> 1. Exception Handling
> 2. HTTPS Redirection
> 3. Static Files
> 4. Routing
> 5. **Authentication** ⚠️
> 6. **Authorization** ⚠️
> 7. Custom Middleware (business logic)

**Vérification de Conformité :**
- [x] Respecte patterns de sécurité standard
- [x] Suit ordre Microsoft recommandé
- [x] Améliore posture sécurité

## 🔧 Implémentation

### Approche de Refactoring

**Stratégie :** Réorganiser les `app.UseMiddleware<>()` dans Program.cs pour ordre logique

**Ordre cible :**
1. **RequestLogging** (diagnostic - avant tout)
2. **ApiKeyAuthentication** (sécurité - bloquer non-auth)
3. **QuotaEnforcement** (business logic - après auth)
4. **StreamInterception** (fonctionnalité - fin du pipeline)

**Principe appliqué :**
- **Security First** : Authentication avant toute business logic
- **Fail Fast** : Rejeter requêtes invalides le plus tôt possible
- **Defense in Depth** : Chaque couche assume la précédente a validé

### Fichiers à Modifier

- `src/Presentation/LLMProxy.Gateway/Program.cs` - Réorganiser ordre middlewares

### Modifications Détaillées

#### Étape 1 : Réorganiser ordre dans Program.cs

**État actuel (AVANT) :**
```csharp
// Configure middleware pipeline
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<QuotaEnforcementMiddleware>();     // ⚠️ ORDRE INCORRECT
app.UseMiddleware<ApiKeyAuthenticationMiddleware>(); // ⚠️
app.UseMiddleware<StreamInterceptionMiddleware>();

// Map YARP routes
app.MapReverseProxy();
```

**État cible (APRÈS) :**
```csharp
// ✅ Configure middleware pipeline (ordre logique: Logging → Auth → Quota → Stream)
app.UseMiddleware<RequestLoggingMiddleware>();       // 1. Diagnostic (avant tout)
app.UseMiddleware<ApiKeyAuthenticationMiddleware>(); // 2. Sécurité (bloquer non-auth)
app.UseMiddleware<QuotaEnforcementMiddleware>();     // 3. Business logic (après auth)
app.UseMiddleware<StreamInterceptionMiddleware>();   // 4. Fonctionnalité (fin pipeline)

// Map YARP routes
app.MapReverseProxy();
```

**Justification :**
- **RequestLogging** en premier : Trace TOUTES les requêtes (même invalides)
- **ApiKeyAuthentication** en deuxième : Rejette non-auth AVANT consommation ressources
- **QuotaEnforcement** en troisième : S'exécute uniquement pour requêtes auth (userId garanti présent)
- **StreamInterception** en dernier : Traite uniquement réponses valides

**Validation de l'étape :**
- [ ] Build réussi
- [ ] Tests passent
- [ ] Requête sans API key → Rejetée AVANT quota check

#### Étape 2 : Vérifier impact sur QuotaEnforcementMiddleware

**Analyse :**
```csharp
// QuotaEnforcementMiddleware.InvokeAsync
if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
{
    // ✅ Avec nouvel ordre: Ce cas ne devrait JAMAIS arriver
    //    (ApiKeyAuth garantit UserId présent)
    
    _logger.LogWarning("Quota check without authenticated user");
    context.Response.StatusCode = 401;
    await context.Response.WriteAsJsonAsync(new { error = "Authentication required" });
    return;
}
```

**Action recommandée :**
- Garder le check défensif (defense in depth)
- Mais log en `LogError` car indique bug dans le pipeline

**État cible :**
```csharp
if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
{
    // ⚠️ NE DEVRAIT JAMAIS ARRIVER si pipeline correctement configuré
    _logger.LogError(
        "Quota middleware reached without authenticated user - check middleware order. RequestId: {RequestId}",
        context.Items["RequestId"]);
    
    context.Response.StatusCode = 500; // Internal Server Error (config error)
    await context.Response.WriteAsJsonAsync(new 
    { 
        error = "Internal server error",
        request_id = context.Items["RequestId"]
    });
    return;
}
```

**Validation :**
- [ ] Message d'erreur clair si problème config
- [ ] LogError au lieu de LogWarning (indique bug)
- [ ] 500 au lieu de 401 (erreur serveur, pas client)

#### Étape 3 : Ajouter test d'intégration pour ordre pipeline

**État cible :**
```csharp
// tests/LLMProxy.Gateway.IntegrationTests/MiddlewarePipelineTests.cs
[Fact]
public async Task UnauthenticatedRequest_ShouldBeRejectedBeforeQuotaCheck()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Mock: Quota service should NOT be called for unauth requests
    var quotaServiceMock = _factory.Services.GetRequiredService<IQuotaService>();
    
    // Act
    var response = await client.GetAsync("/v1/chat/completions"); // No API key
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    
    // ✅ CRITIQUE: Quota service should NOT have been called
    quotaServiceMock.Verify(
        x => x.CheckQuotaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
        Times.Never,
        "Quota check should not execute for unauthenticated requests");
}

[Fact]
public async Task AuthenticatedRequest_ShouldCheckQuotaAfterAuth()
{
    // Arrange
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Add("X-API-Key", "valid-test-key");
    
    var quotaServiceMock = _factory.Services.GetRequiredService<IQuotaService>();
    
    // Act
    var response = await client.GetAsync("/v1/chat/completions");
    
    // Assert
    // ✅ Quota service SHOULD have been called for authenticated requests
    quotaServiceMock.Verify(
        x => x.CheckQuotaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
        Times.Once);
}
```

**Validation :**
- [ ] Test verify que quota pas appelé si non auth
- [ ] Test verify que quota appelé si auth
- [ ] Tests d'intégration passent

### Considérations Techniques

**Points d'Attention :**
- **BREAKING CHANGE potentiel** : Si code dépend de l'ordre actuel (peu probable)
- **Performance** : Amélioration (moins de DB queries pour requêtes invalides)
- **Sécurité** : Amélioration (surface d'attaque réduite)

**Bonnes Pratiques :**
- Toujours Authentication avant Authorization/Business Logic
- Logging en premier pour tracer toutes requêtes
- Defense in depth : Garder checks défensifs même avec bon ordre

**Pièges à Éviter :**
- Ne pas inverser Logging et Auth (voudrait tracer auth failures)
- Ne pas placer StreamInterception avant Quota (voudrait quota sur réponses)

## ✅ Critères de Validation

### Tests de Non-Régression

**Tests Obligatoires :**
- [ ] Tous tests existants passent
- [ ] Requête avec API key valide → Fonctionne normalement
- [ ] Requête sans API key → 401 SANS query DB quota
- [ ] Requête avec quota dépassé → 429 (après auth)

**Tests de Sécurité :**
- [ ] Flood de requêtes sans API key → Aucune query DB quota
- [ ] Performance améliorée (moins de CPU/DB pour requêtes invalides)

**Tests d'Intégration :**
- [ ] Nouveau test: Quota non appelé si non auth
- [ ] Nouveau test: Quota appelé si auth

### Amélioration des Piliers

**Piliers Améliorés :**
- [x] **Sécurité** : Requêtes non auth bloquées avant consommation ressources (CRITIQUE)
- [x] **Architecture** : Pipeline conforme aux best practices
- [x] **Performance** : Réduction charge pour requêtes invalides
- [x] **Robustesse** : Ordre logique plus facile à comprendre/maintenir

**Piliers Non Dégradés :**
- [x] Fonctionnalité préservée
- [x] Simplicité maintenue (juste réorganisation)

### Conformité et Documentation

- [x] Respecte ASP.NET Core Middleware Ordering Best Practices
- [x] Suit principe Security First
- [ ] Commentaire dans Program.cs expliquant ordre
- [ ] Documentation ARCHITECTURE.md mise à jour
- [ ] Git commit : `fix(pipeline): reorder middlewares - auth before quota`

### Plan de Rollback

**En cas de problème :**
1. `git revert <commit-hash>` (rollback simple)
2. Vérifier tests passent
3. **Attention** : Risque faible mais si dépendances implicites sur ordre

## 📈 Métriques d'Amélioration

**Avant Refactoring :**
- Requêtes non-auth consommant ressources : 100%
- DB queries inutiles par requête invalide : 1+
- Conformité best practices ASP.NET : Non
- Surface d'attaque DoS : Élevée

**Après Refactoring (attendu) :**
- Requêtes non-auth consommant ressources : 0% (rejetées immédiatement)
- DB queries inutiles par requête invalide : 0
- Conformité best practices ASP.NET : Oui
- Surface d'attaque DoS : Réduite

**Bénéfice Mesurable :**
- Réduction charge DB pour requêtes invalides : -100%
- Réduction temps traitement requête invalide : -50% (estimation)
- Amélioration posture sécurité : Significative

## 🔗 Références

**Microsoft Documentation :**
- [ASP.NET Core Middleware Ordering](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/#middleware-order)
- [ASP.NET Core Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)

**OWASP :**
- [API Security - Authentication](https://owasp.org/www-project-api-security/)

**Instructions Projet :**
- `.github/instructions/csharp.standards.instructions.md` - Architecture patterns

**Patterns :**
- Security First Pattern
- Fail Fast Pattern
- Defense in Depth


##  TRACKING

Début: 2025-12-21T06:24:58.9878403Z


Fin: 2025-12-21T06:25:30.3361321Z
Durée: 00:00:31

##  VALIDATION

- [x] Ordre corrigé: RequestLogging  Auth  Quota
- [x] Commentaire explicatif ajouté
- [x] Build sans warning
- [x] Sécurité renforcée (auth avant quota)
- [x] Conforme best practices ASP.NET Core

