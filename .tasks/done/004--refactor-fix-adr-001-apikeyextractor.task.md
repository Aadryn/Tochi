# Tâche 004 - Séparer types ApiKeyExtractor (ADR-001)

## MÉTADONNÉES

- **Priorité** : P1
- **Effort** : 1h
- **Risque** : Faible
- **Valeur** : Moyenne
- **Dépendances** : Aucune

## CONTEXTE

Fichier `src/Infrastructure/LLMProxy.Infrastructure.Security/ApiKeyExtractor.cs` contient 3 types distincts :
1. `HttpHeaderConstants` (classe statique interne avec constantes)
2. `IApiKeyExtractor` (interface)
3. `HeaderApiKeyExtractor` (implémentation)

**Violation** : ADR-001 (Un seul type par fichier C#)

**Analyse origine** : refactor.analysis.md - Section ApiKeyExtractor.cs

**Justification** : La violation de ADR-001 nuit à :
- La navigabilité du code (3 types dans un seul fichier)
- La cohérence architecturale du projet
- Le respect des standards de développement documentés

## OBJECTIF

Séparer les 3 types en 3 fichiers distincts pour respecter strictement ADR-001.

## CRITÈRES DE SUCCÈS

- [ ] 3 fichiers créés : `HttpHeaderConstants.cs`, `IApiKeyExtractor.cs`, `HeaderApiKeyExtractor.cs`
- [ ] Chaque fichier contient EXACTEMENT 1 type
- [ ] Build réussi sans aucun warning (0 Error(s), 0 Warning(s))
- [ ] Tests unitaires 100% réussis (49/49 passing, 0 failed, 0 skipped)
- [ ] Documentation XML complète et en français
- [ ] Conformité ADR-001 validée
- [ ] Aucune régression fonctionnelle

## ÉTAPES (BABY STEPS)

### 1. Créer HttpHeaderConstants.cs

**Action** : Extraire la classe `HttpHeaderConstants` dans son propre fichier.

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Security/HttpHeaderConstants.cs`

**Contenu** :
```csharp
namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Constantes pour les en-têtes HTTP utilisés dans l'authentification par clé API.
/// </summary>
/// <remarks>
/// Cette classe contient les noms des en-têtes HTTP utilisés pour extraire les clés API.
/// Classe interne pour éviter la pollution du namespace public.
/// </remarks>
internal static class HttpHeaderConstants
{
    /// <summary>
    /// Nom de l'en-tête HTTP Authorization standard.
    /// </summary>
    /// <remarks>
    /// Utilisé avec le schéma "Bearer" : Authorization: Bearer sk-abc123...
    /// </remarks>
    public const string Authorization = "Authorization";
    
    /// <summary>
    /// Nom de l'en-tête HTTP personnalisé pour les clés API.
    /// </summary>
    /// <remarks>
    /// En-tête alternatif si Authorization n'est pas utilisé : X-API-Key: sk-abc123...
    /// </remarks>
    public const string ApiKey = "X-API-Key";
    
    /// <summary>
    /// Préfixe du schéma Bearer dans l'en-tête Authorization.
    /// </summary>
    /// <remarks>
    /// Format attendu : "Bearer sk-abc123..."
    /// </remarks>
    public const string BearerPrefix = "Bearer ";
}
```

**Validation** :
```powershell
dotnet build src/Infrastructure/LLMProxy.Infrastructure.Security --no-restore
```

### 2. Créer IApiKeyExtractor.cs

**Action** : Extraire l'interface `IApiKeyExtractor` dans son propre fichier.

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Security/IApiKeyExtractor.cs`

**Contenu** :
```csharp
using Microsoft.AspNetCore.Http;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Service d'extraction des clés API depuis les en-têtes HTTP.
/// </summary>
/// <remarks>
/// Cette interface définit le contrat pour extraire les clés API
/// des requêtes HTTP entrantes.
/// Conforme à ADR-005 (SRP) et ADR-048 (Security - pas de query parameters).
/// </remarks>
public interface IApiKeyExtractor
{
    /// <summary>
    /// Extrait la clé API des en-têtes HTTP de la requête.
    /// </summary>
    /// <param name="context">Contexte HTTP de la requête.</param>
    /// <returns>
    /// Clé API extraite si présente, sinon null.
    /// </returns>
    /// <remarks>
    /// Tente d'extraire la clé API dans l'ordre suivant :
    /// 1. En-tête Authorization avec schéma Bearer
    /// 2. En-tête personnalisé X-API-Key
    /// 
    /// IMPORTANT : Les query parameters ne sont PAS supportés pour des raisons de sécurité
    /// (éviter le logging des clés API dans les logs serveur).
    /// </remarks>
    /// <example>
    /// <code>
    /// var apiKey = extractor.ExtractApiKey(httpContext);
    /// if (apiKey == null)
    /// {
    ///     return Results.Unauthorized();
    /// }
    /// </code>
    /// </example>
    string? ExtractApiKey(HttpContext context);
}
```

**Validation** :
```powershell
dotnet build src/Infrastructure/LLMProxy.Infrastructure.Security --no-restore
```

### 3. Modifier HeaderApiKeyExtractor.cs

**Action** : Supprimer les 2 types extraits et conserver uniquement l'implémentation.

**Avant** : 3 types dans le fichier (ApiKeyExtractor.cs)
**Après** : 1 seul type (classe HeaderApiKeyExtractor) dans HeaderApiKeyExtractor.cs

**Modifications** :
- Renommer le fichier en `HeaderApiKeyExtractor.cs` si nécessaire
- Supprimer la classe `HttpHeaderConstants`
- Supprimer l'interface `IApiKeyExtractor`
- Conserver uniquement la classe `HeaderApiKeyExtractor`
- Vérifier que tous les `using` nécessaires sont présents

**Validation** :
```powershell
dotnet build src/Infrastructure/LLMProxy.Infrastructure.Security --no-restore
```

### 4. Build complet du projet

**Action** : Valider que toutes les modifications compilent sans erreur ni warning.

**Commande** :
```powershell
dotnet build --no-restore
```

**Critères** :
- `Build succeeded.`
- `0 Error(s)`
- `0 Warning(s)`

### 5. Exécuter les tests unitaires

**Action** : Valider qu'aucune régression n'a été introduite.

**Commande** :
```powershell
dotnet test --no-build
```

**Critères** :
- `Test Run Successful`
- `Passed: 49`
- `Failed: 0`
- `Skipped: 0`

### 6. Validation fonctionnelle

**Action** : Tester l'extraction de clés API depuis différents en-têtes.

**Commande** :
```powershell
run_task → "dotnet: watch WebApi Default"
```

**Tests Chrome DevTools** :
- Requête avec `Authorization: Bearer sk-abc123...` → Clé extraite correctement
- Requête avec `X-API-Key: sk-abc123...` → Clé extraite correctement
- Requête sans en-tête API key → 401 Unauthorized + message approprié
- Requête avec en-tête malformé → 401 Unauthorized
- Vérifier console DevTools : Aucune erreur
- Vérifier logs : Extraction correctement loguée

**Tests manuels (Postman/curl)** :
```bash
# Test Authorization header
curl -H "Authorization: Bearer sk-test123" http://localhost:5000/api/test

# Test X-API-Key header
curl -H "X-API-Key: sk-test123" http://localhost:5000/api/test

# Test sans header (devrait échouer)
curl http://localhost:5000/api/test
```

### 7. Commit atomique

**Action** : Commiter les changements avec message conventionnel.

**Commandes** :
```powershell
git add src/Infrastructure/LLMProxy.Infrastructure.Security/
git commit -m "refactor(security): separate ApiKeyExtractor types (ADR-001)

- Extract HttpHeaderConstants to dedicated file
- Extract IApiKeyExtractor interface to dedicated file
- Rename implementation file to HeaderApiKeyExtractor.cs
- Keep only HeaderApiKeyExtractor implementation in file
- Add complete XML documentation in French
- Ensure ADR-001 compliance (one type per file)

Refs: ADR-001, ADR-005, ADR-048, refactor.analysis.md"
```

## RÉFÉRENCES

- **ADR violé** : [docs/adr/001-un-seul-type-par-fichier-csharp.adr.md](../../../docs/adr/001-un-seul-type-par-fichier-csharp.adr.md)
- **Instructions applicables** :
  - `.github/instructions/csharp.standards.instructions.md`
  - `.github/instructions/csharp.documentation.instructions.md`
- **Analyse origine** : [refactor.analysis.md](../../refactor.analysis.md) (Section ApiKeyExtractor.cs)
- **ADR connexes** :
  - ADR-005 (SOLID Principles - SRP)
  - ADR-048 (Connection String Security - applicable aux clés API)


## TRACKING
Début: 2025-12-21T16:31:08.0085037Z


Fin: 2025-12-21T16:32:23.3149304Z
Durée: 00:01:15

## RÉSULTATS

 3 fichiers créés:
- HttpHeaderConstants.cs (43 lignes)
- IApiKeyExtractor.cs (49 lignes)
- HeaderApiKeyExtractor.cs (42 lignes, renommé depuis ApiKeyExtractor.cs)

 Build: 0 errors, 2 warnings pré-existants (xUnit)
 Tests: 66/66 passing (100%)
 Conformité ADR-001: STRICTE
 SPRINT 1 COMPLÉTÉ (4/4 tâches P1)

Commit: e2ac5da 'refactor(security): separate ApiKeyExtractor types (ADR-001)'

