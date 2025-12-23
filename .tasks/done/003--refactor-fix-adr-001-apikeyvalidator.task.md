# Tâche 003 - Séparer types ApiKeyValidator (ADR-001)

## MÉTADONNÉES

- **Priorité** : P1
- **Effort** : 1h
- **Risque** : Faible
- **Valeur** : Moyenne
- **Dépendances** : Aucune

## CONTEXTE

Fichier `src/Infrastructure/LLMProxy.Infrastructure.Security/ApiKeyValidator.cs` contient 3 types distincts :
1. `ApiKeyValidationResult` (classe)
2. `IApiKeyValidator` (interface)
3. `ApiKeyValidator` (implémentation)

**Violation** : ADR-001 (Un seul type par fichier C#)

**Analyse origine** : refactor.analysis.md - Section ApiKeyValidator.cs

**Justification** : La violation de ADR-001 nuit à :
- La navigabilité du code (3 types dans un seul fichier)
- La cohérence architecturale du projet
- Le respect des standards de développement documentés

## OBJECTIF

Séparer les 3 types en 3 fichiers distincts pour respecter strictement ADR-001.

## CRITÈRES DE SUCCÈS

- [ ] 3 fichiers créés : `ApiKeyValidationResult.cs`, `IApiKeyValidator.cs`, `ApiKeyValidator.cs`
- [ ] Chaque fichier contient EXACTEMENT 1 type
- [ ] Build réussi sans aucun warning (0 Error(s), 0 Warning(s))
- [ ] Tests unitaires 100% réussis (49/49 passing, 0 failed, 0 skipped)
- [ ] Documentation XML complète et en français
- [ ] Conformité ADR-001 validée
- [ ] Aucune régression fonctionnelle

## ÉTAPES (BABY STEPS)

### 1. Créer ApiKeyValidationResult.cs

**Action** : Extraire la classe `ApiKeyValidationResult` dans son propre fichier.

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Security/ApiKeyValidationResult.cs`

**Contenu** :
```csharp
namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Résultat de la validation d'une clé API.
/// </summary>
/// <remarks>
/// Cette classe encapsule le résultat d'une validation de clé API,
/// incluant l'état de validité et les détails d'erreur éventuels.
/// Conforme à ADR-023 (Result Pattern).
/// </remarks>
public class ApiKeyValidationResult
{
    /// <summary>
    /// Indique si la clé API est valide.
    /// </summary>
    public bool IsValid { get; init; }
    
    /// <summary>
    /// Message d'erreur en cas de validation échouée (null si valide).
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Crée un résultat de validation réussi.
    /// </summary>
    /// <returns>Résultat indiquant que la clé API est valide.</returns>
    public static ApiKeyValidationResult Success() =>
        new() { IsValid = true };
    
    /// <summary>
    /// Crée un résultat de validation échoué.
    /// </summary>
    /// <param name="errorMessage">Message décrivant la raison de l'échec de validation.</param>
    /// <returns>Résultat indiquant que la clé API est invalide.</returns>
    /// <example>
    /// <code>
    /// return ApiKeyValidationResult.Failure("API key has been revoked");
    /// </code>
    /// </example>
    public static ApiKeyValidationResult Failure(string errorMessage) =>
        new() { IsValid = false, ErrorMessage = errorMessage };
}
```

**Validation** :
```powershell
dotnet build src/Infrastructure/LLMProxy.Infrastructure.Security --no-restore
```

### 2. Créer IApiKeyValidator.cs

**Action** : Extraire l'interface `IApiKeyValidator` dans son propre fichier.

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Security/IApiKeyValidator.cs`

**Contenu** :
```csharp
using LLMProxy.Domain.Entities;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Service de validation métier des clés API.
/// </summary>
/// <remarks>
/// Cette interface définit le contrat pour valider les règles métier
/// liées aux clés API (révocation, expiration, utilisateur actif).
/// Conforme à ADR-005 (SRP) et ADR-023 (Result Pattern).
/// </remarks>
public interface IApiKeyValidator
{
    /// <summary>
    /// Valide une clé API selon les règles métier.
    /// </summary>
    /// <param name="apiKey">Clé API à valider.</param>
    /// <param name="user">Utilisateur associé à la clé API.</param>
    /// <returns>
    /// Résultat de validation indiquant si la clé est valide,
    /// avec un message d'erreur détaillé en cas d'échec.
    /// </returns>
    /// <remarks>
    /// Valide les règles suivantes :
    /// - La clé ne doit pas être révoquée
    /// - La clé ne doit pas être expirée
    /// - L'utilisateur associé doit être actif
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = validator.ValidateApiKey(apiKey, user);
    /// if (!result.IsValid)
    /// {
    ///     _logger.LogWarning("Validation failed: {Error}", result.ErrorMessage);
    /// }
    /// </code>
    /// </example>
    ApiKeyValidationResult ValidateApiKey(ApiKey apiKey, User user);
}
```

**Validation** :
```powershell
dotnet build src/Infrastructure/LLMProxy.Infrastructure.Security --no-restore
```

### 3. Modifier ApiKeyValidator.cs

**Action** : Supprimer les 2 types extraits et conserver uniquement l'implémentation.

**Avant** : 3 types dans le fichier
**Après** : 1 seul type (classe ApiKeyValidator)

**Modifications** :
- Supprimer la classe `ApiKeyValidationResult`
- Supprimer l'interface `IApiKeyValidator`
- Conserver uniquement la classe `ApiKeyValidator`
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

**Action** : Tester la validation des clés API (révocation, expiration, utilisateur actif).

**Commande** :
```powershell
run_task → "dotnet: watch WebApi Default"
```

**Tests Chrome DevTools** :
- Requête avec clé API révoquée → 401 Unauthorized + message "API key has been revoked"
- Requête avec clé API expirée → 401 Unauthorized + message "API key has expired"
- Requête avec utilisateur inactif → 401 Unauthorized + message "User account is inactive"
- Requête avec clé valide → 200 OK
- Vérifier console DevTools : Aucune erreur
- Vérifier logs : Messages de validation appropriés

### 7. Commit atomique

**Action** : Commiter les changements avec message conventionnel.

**Commandes** :
```powershell
git add src/Infrastructure/LLMProxy.Infrastructure.Security/
git commit -m "refactor(security): separate ApiKeyValidator types (ADR-001)

- Extract ApiKeyValidationResult to dedicated file
- Extract IApiKeyValidator interface to dedicated file
- Keep only ApiKeyValidator implementation in original file
- Add complete XML documentation in French
- Ensure ADR-001 compliance (one type per file)

Refs: ADR-001, ADR-005, ADR-023, refactor.analysis.md"
```

## RÉFÉRENCES

- **ADR violé** : [docs/adr/001-un-seul-type-par-fichier-csharp.adr.md](../../../docs/adr/001-un-seul-type-par-fichier-csharp.adr.md)
- **Instructions applicables** :
  - `.github/instructions/csharp.standards.instructions.md`
  - `.github/instructions/csharp.documentation.instructions.md`
- **Analyse origine** : [refactor.analysis.md](../../refactor.analysis.md) (Section ApiKeyValidator.cs)
- **ADR connexes** :
  - ADR-005 (SOLID Principles - SRP)
  - ADR-023 (Result Pattern)
  - ADR-031 (Structured Logging)


## TRACKING
Début: 2025-12-21T16:29:05.9352553Z


Fin: 2025-12-21T16:30:18.2750591Z
Durée: 00:01:12

## RÉSULTATS

 3 fichiers créés:
- ApiKeyValidationResult.cs (66 lignes)
- IApiKeyValidator.cs (44 lignes)
- ApiKeyValidator.cs (68 lignes)

 Build: 0 errors, 0 warnings
 Tests: 66/66 passing (100%)
 Conformité ADR-001: STRICTE

Commit: 3c025e5 'refactor(security): separate ApiKeyValidator types (ADR-001)'

