---
id: 002
title: Retirer support API key dans query parameters (OWASP Security)
concerns: middlewares, sécurité
type: refactoring
priority: critical
effort: small
risk: low
value: high
dependencies: []
status: to-do
created: 2025-12-21
---

# Retirer support API key dans query parameters (OWASP Security)

## 🎯 Objectif

Supprimer la possibilité d'envoyer l'API key via query parameter (`?api_key=...`) pour éliminer le risque d'exposition des secrets dans les logs, l'historique navigateur et les proxies.

**Amélioration visée :**
- **Sécurité** : Éliminer vecteur d'exposition de secrets sensibles
- **Conformité OWASP** : Respecter les bonnes pratiques de sécurité des APIs

**Bénéfice mesurable :** Zéro API key exposée dans query strings, logs d'accès, ou historique navigateur

## 📊 Contexte

### Problème Identifié

- **Type** : Sécurité (OWASP A01:2021 - Broken Access Control)
- **Localisation** : `src/Presentation/LLMProxy.Gateway/Middleware/ApiKeyAuthenticationMiddleware.cs:143-146`
- **Description Factuelle** : Le middleware accepte les API keys via query parameter `?api_key=xxx`, ce qui expose le secret dans multiples vecteurs de logging et cache.
- **Impact Actuel** : 
  - API keys loggées dans access logs serveur (IIS, Nginx, Apache)
  - API keys stockées dans browser history
  - API keys cachées dans proxies HTTP
  - API keys visibles dans URL sharing (Slack, emails, etc.)
- **Preuve** :

```csharp
// ❌ VULNÉRABILITÉ SÉCURITÉ - API key dans query string
private string? ExtractApiKey(HttpContext context)
{
    // ... headers check ...
    
    // Try query parameter as fallback ⚠️ PROBLÈME ICI
    if (context.Request.Query.TryGetValue("api_key", out var apiKeyQuery))
    {
        return apiKeyQuery.ToString();
    }

    return null;
}
```

**Exemples d'exposition :**
```
# ❌ API key visible dans tous ces logs:
GET /v1/chat/completions?api_key=sk_live_abc123xyz HTTP/1.1
→ Loggé dans: IIS access log, Nginx access.log, browser history, proxy cache
```

### Conformité Standards

**Instructions Applicables :**
- `.github/instructions/csharp.standards.instructions.md` - Sécurité
- OWASP API Security Top 10

**Références Sécurité :**
- [OWASP API Security Top 10 - A01:2021 Broken Access Control](https://owasp.org/API-Security/editions/2021/en/0xa1-broken-object-level-authorization/)
- [OWASP Cheat Sheet - REST Security](https://cheatsheetseries.owasp.org/cheatsheets/REST_Security_Cheat_Sheet.html)

**Vérification de Conformité :**
- [x] Élimine vulnérabilité OWASP
- [x] Suit les best practices REST API security
- [x] Aucun standard projet n'est violé

## 🔧 Implémentation

### Approche de Refactoring

**Stratégie :** Retirer le code qui extrait API key depuis query parameters et mettre à jour la documentation

**Principe appliqué :**
- **Security by Design** : Ne pas offrir de méthode non sécurisée même si "pratique"
- **Least Privilege** : Forcer l'utilisation de méthodes sécurisées uniquement

### Fichiers à Modifier

- `src/Presentation/LLMProxy.Gateway/Middleware/ApiKeyAuthenticationMiddleware.cs` - Retirer extraction query param
- `docs/API.md` (si existe) - Documenter méthodes acceptées uniquement

### Modifications Détaillées

#### Étape 1 : Retirer extraction depuis query parameters

**État actuel (AVANT) :**
```csharp
private string? ExtractApiKey(HttpContext context)
{
    // Try Authorization header first (Bearer token format)
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        var headerValue = authHeader.ToString();
        if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return headerValue.Substring("Bearer ".Length).Trim();
        }
    }

    // Try X-API-Key header
    if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
    {
        return apiKeyHeader.ToString();
    }

    // ❌ RETIRER CE CODE - Vulnérabilité sécurité
    // Try query parameter as fallback
    if (context.Request.Query.TryGetValue("api_key", out var apiKeyQuery))
    {
        return apiKeyQuery.ToString();
    }

    return null;
}
```

**État cible (APRÈS) :**
```csharp
/// <summary>
/// Extrait l'API key depuis les headers HTTP sécurisés uniquement.
/// </summary>
/// <param name="context">Contexte HTTP de la requête</param>
/// <returns>L'API key si trouvée dans les headers, null sinon</returns>
/// <remarks>
/// Méthodes acceptées (par ordre de priorité):
/// 1. Authorization: Bearer {api_key}
/// 2. X-API-Key: {api_key}
/// 
/// ⚠️ SÉCURITÉ: Query parameters ne sont PAS acceptés pour éviter exposition dans logs.
/// </remarks>
private static string? ExtractApiKey(HttpContext context)
{
    // Try Authorization header first (Bearer token format)
    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        var headerValue = authHeader.ToString();
        if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return headerValue.Substring("Bearer ".Length).Trim();
        }
    }

    // Try X-API-Key header
    if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
    {
        return apiKeyHeader.ToString();
    }

    // ✅ Query parameters VOLONTAIREMENT non supportés pour raisons de sécurité
    return null;
}
```

**Justification :**
- Élimine vecteur d'exposition de secrets
- Commentaire explicite pour éviter réintroduction future
- Documentation XML claire sur méthodes acceptées
- Méthode devient `static` (pas de dépendance instance)

**Validation de l'étape :**
- [ ] Build réussi
- [ ] API key via headers fonctionne toujours
- [ ] API key via query param retourne 401 (comportement attendu)
- [ ] Documentation XML complète

#### Étape 2 : Ajouter log explicite si query param détecté (optionnel mais recommandé)

**État cible (APRÈS) :**
```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Skip authentication for health checks
    if (context.Request.Path.StartsWithSegments("/health"))
    {
        await _next(context);
        return;
    }

    // ✅ NOUVEAU - Log warning si tentative d'utiliser query param (éducatif)
    if (context.Request.Query.ContainsKey("api_key"))
    {
        _logger.LogWarning(
            "API key in query parameter detected (INSECURE). Use Authorization or X-API-Key headers instead. Path: {Path}, IP: {IP}",
            context.Request.Path,
            context.Connection.RemoteIpAddress);
    }

    var apiKey = ExtractApiKey(context);

    if (string.IsNullOrEmpty(apiKey))
    {
        _logger.LogWarning("Request without API key: {Path}", context.Request.Path);
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new 
        { 
            error = "API key is required",
            hint = "Use 'Authorization: Bearer {api_key}' or 'X-API-Key: {api_key}' header"
        });
        return;
    }

    // ... reste du code ...
}
```

**Justification :**
- Éduque les développeurs qui tentent d'utiliser query param
- Trace les tentatives pour détection d'attaques
- Message d'erreur clair avec hint sur méthode correcte

**Validation de l'étape :**
- [ ] Log warning généré si `?api_key=` détecté
- [ ] Message d'erreur 401 inclut hint sur bonne méthode
- [ ] Aucune régression sur comportement normal

### Considérations Techniques

**Points d'Attention :**
- **BREAKING CHANGE** : Les clients utilisant query param vont échouer
- Solution : Communication avant déploiement + période de transition si nécessaire
- Documenter dans CHANGELOG.md

**Bonnes Pratiques :**
- Suivre les recommandations OWASP pour authentification API
- Toujours préférer headers HTTP pour credentials
- Logger les tentatives d'utilisation de méthodes non sécurisées

**Pièges à Éviter :**
- Ne pas réintroduire query param support "pour compatibilité"
- Ne pas logger la valeur de l'API key (même si invalide)

## ✅ Critères de Validation

### Tests de Non-Régression

**Tests Obligatoires :**
- [ ] API key via `Authorization: Bearer` fonctionne
- [ ] API key via `X-API-Key` fonctionne
- [ ] API key via query param retourne 401 avec hint
- [ ] Log warning généré si query param détecté

**Tests de Sécurité :**
- [ ] Vérifier qu'aucune API key n'apparaît dans logs (grep logs)
- [ ] Vérifier qu'aucune API key n'apparaît dans métriques/telemetry
- [ ] Tester avec scanner sécurité (ex: OWASP ZAP)

**Validation Fonctionnelle :**
- [ ] Tous les clients utilisant headers continuent de fonctionner
- [ ] Nouveaux clients reçoivent message clair avec hint

### Amélioration des Piliers

**Piliers Améliorés :**
- [x] **Sécurité** : Élimination vecteur d'exposition de secrets (CRITIQUE)
- [x] **Conformité** : Respect OWASP API Security Top 10
- [x] **Maintenabilité** : Code simplifié (moins de branches)

**Piliers Non Dégradés :**
- [x] Performance identique (moins de code = plus rapide)
- [x] Robustesse maintenue
- [x] Simplicité améliorée

### Conformité et Documentation

- [x] Suit OWASP API Security guidelines
- [x] Respecte REST API best practices
- [ ] Documentation API mise à jour (méthodes acceptées)
- [ ] CHANGELOG.md mis à jour (BREAKING CHANGE)
- [ ] Migration guide créé si clients existants impactés
- [ ] Git commit : `security(auth): remove insecure API key query parameter support`

### Plan de Rollback

**En cas de problème :**
1. Si clients critiques impactés:
   - Option A: `git revert <commit-hash>` (rollback complet)
   - Option B: Ajouter feature flag temporaire pour période de transition
2. Communiquer aux clients impactés
3. Planifier migration avec deadline claire

## 📈 Métriques d'Amélioration

**Avant Refactoring :**
- Vecteurs d'exposition API key : 3 (Authorization header, X-API-Key header, **query param**)
- API keys potentiellement loggées : Oui (query param dans access logs)
- Conformité OWASP : Non (vulnérabilité A01:2021)

**Après Refactoring (attendu) :**
- Vecteurs d'exposition API key : 2 (headers sécurisés uniquement)
- API keys potentiellement loggées : Non (headers non loggés par défaut)
- Conformité OWASP : Oui (vulnérabilité éliminée)

**Bénéfice Mesurable :**
- Réduction risque exposition secrets : 100% (query param éliminé)
- Conformité sécurité : Amélioration de Non-Conforme → Conforme
- Simplicité code : -10 lignes (code retiré)

## 🔗 Références

**Standards de Sécurité :**
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [OWASP REST Security Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/REST_Security_Cheat_Sheet.html)
- [RFC 6750 - OAuth 2.0 Bearer Token Usage](https://tools.ietf.org/html/rfc6750) (bonnes pratiques headers)

**Instructions Appliquées :**
- `.github/instructions/csharp.standards.instructions.md` - Sécurité

**Articles Recommandés :**
- [Why You Shouldn't Put Credentials in URLs](https://www.ncsc.gov.uk/blog-post/dont-put-credentials-in-urls)


##  TRACKING

Début: 2025-12-21T06:19:56.8051280Z


Fin: 2025-12-21T06:20:27.5661679Z
Durée: 00:00:30

##  VALIDATION

- [x] Query param supprimé de ExtractApiKey()
- [x] Commentaire sécurité ajouté
- [x] Build sans warning
- [x] Sécurité renforcée (pas de logs API keys)

