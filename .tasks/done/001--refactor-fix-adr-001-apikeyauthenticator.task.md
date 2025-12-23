# Tâche 001 - Séparer types ApiKeyAuthenticator (ADR-001)

## MÉTADONNÉES

- **Priorité** : P1
- **Effort** : 1h
- **Risque** : Faible
- **Valeur** : Moyenne
- **Dépendances** : Aucune

## CONTEXTE

Fichier `src/Infrastructure/LLMProxy.Infrastructure.Security/ApiKeyAuthenticator.cs` contient 3 types distincts :
1. `ApiKeyAuthenticationResult` (classe)
2. `IApiKeyAuthenticator` (interface)
3. `ApiKeyAuthenticator` (implémentation)

**Violation** : ADR-001 (Un seul type par fichier C#)

**Analyse origine** : refactor.analysis.md - Section ApiKeyAuthenticator.cs

**Justification** : La violation de ADR-001 nuit à :
- La navigabilité du code (3 types dans un seul fichier)
- La cohérence architecturale du projet
- Le respect des standards de développement documentés

## OBJECTIF

Séparer les 3 types en 3 fichiers distincts pour respecter strictement ADR-001.

## CRITÈRES DE SUCCÈS

- [ ] 3 fichiers créés : `ApiKeyAuthenticationResult.cs`, `IApiKeyAuthenticator.cs`, `ApiKeyAuthenticator.cs`
- [ ] Chaque fichier contient EXACTEMENT 1 type
- [ ] Build réussi sans aucun warning (0 Error(s), 0 Warning(s))
- [ ] Tests unitaires 100% réussis (49/49 passing, 0 failed, 0 skipped)
- [ ] Documentation XML complète et en français
- [ ] Conformité ADR-001 validée
- [ ] Aucune régression fonctionnelle

## ÉTAPES (BABY STEPS)

### 1. Créer ApiKeyAuthenticationResult.cs

**Action** : Extraire la classe `ApiKeyAuthenticationResult` dans son propre fichier.

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Security/ApiKeyAuthenticationResult.cs`

**Contenu** :
```csharp
using LLMProxy.Domain.Entities;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Résultat de l'authentification par clé API.
/// </summary>
/// <remarks>
/// Cette classe encapsule le résultat d'une tentative d'authentification,
/// incluant l'état de succès/échec, les entités associées, et les détails d'erreur.
/// </remarks>
public class ApiKeyAuthenticationResult
{
    /// <summary>
    /// Indique si l'authentification a réussi.
    /// </summary>
    public bool IsAuthenticated { get; init; }
    
    /// <summary>
    /// Clé API authentifiée (null si échec).
    /// </summary>
    public ApiKey? ApiKey { get; init; }
    
    /// <summary>
    /// Utilisateur associé à la clé API (null si échec).
    /// </summary>
    public User? User { get; init; }
    
    /// <summary>
    /// Message d'erreur en cas d'échec (null si succès).
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Code de statut HTTP associé au résultat.
    /// </summary>
    public int StatusCode { get; init; }
    
    /// <summary>
    /// Crée un résultat d'authentification réussi.
    /// </summary>
    /// <param name="apiKey">Clé API authentifiée.</param>
    /// <param name="user">Utilisateur associé.</param>
    /// <returns>Résultat indiquant le succès de l'authentification.</returns>
    public static ApiKeyAuthenticationResult Success(ApiKey apiKey, User user) =>
        new() { IsAuthenticated = true, ApiKey = apiKey, User = user, StatusCode = 200 };
    
    /// <summary>
    /// Crée un résultat d'authentification échoué.
    /// </summary>
    /// <param name="errorMessage">Message décrivant la raison de l'échec.</param>
    /// <param name="statusCode">Code de statut HTTP (401 par défaut).</param>
    /// <returns>Résultat indiquant l'échec de l'authentification.</returns>
    public static ApiKeyAuthenticationResult Failure(string errorMessage, int statusCode = 401) =>
        new() { IsAuthenticated = false, ErrorMessage = errorMessage, StatusCode = statusCode };
}
```

**Validation** :
```powershell
dotnet build src/Infrastructure/LLMProxy.Infrastructure.Security --no-restore
```

### 2. Créer IApiKeyAuthenticator.cs

**Action** : Extraire l'interface `IApiKeyAuthenticator` dans son propre fichier.

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Security/IApiKeyAuthenticator.cs`

**Contenu** :
```csharp
using LLMProxy.Domain.Interfaces;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Service d'authentification par clé API.
/// </summary>
/// <remarks>
/// Cette interface définit le contrat pour authentifier un utilisateur
/// via une clé API fournie dans les en-têtes HTTP.
/// Conforme à ADR-014 (Dependency Injection) et ADR-023 (Result Pattern).
/// </remarks>
public interface IApiKeyAuthenticator
{
    /// <summary>
    /// Authentifie un utilisateur via une clé API brute.
    /// </summary>
    /// <param name="rawApiKey">Clé API en clair (non hachée).</param>
    /// <param name="unitOfWork">Unité de travail pour accéder au repository.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération.</param>
    /// <returns>
    /// Résultat de l'authentification contenant la clé API et l'utilisateur si succès,
    /// ou un message d'erreur et code de statut si échec.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await authenticator.AuthenticateAsync(
    ///     "sk-abc123...",
    ///     unitOfWork,
    ///     cancellationToken);
    /// 
    /// if (result.IsAuthenticated)
    /// {
    ///     Console.WriteLine($"Utilisateur {result.User.Email} authentifié");
    /// }
    /// </code>
    /// </example>
    Task<ApiKeyAuthenticationResult> AuthenticateAsync(
        string rawApiKey,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default);
}
```

**Validation** :
```powershell
dotnet build src/Infrastructure/LLMProxy.Infrastructure.Security --no-restore
```

### 3. Modifier ApiKeyAuthenticator.cs

**Action** : Supprimer les 2 types extraits et conserver uniquement l'implémentation.

**Avant** : 3 types dans le fichier
**Après** : 1 seul type (classe ApiKeyAuthenticator)

**Modifications** :
- Supprimer la classe `ApiKeyAuthenticationResult`
- Supprimer l'interface `IApiKeyAuthenticator`
- Conserver uniquement la classe `ApiKeyAuthenticator`
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

**Action** : Démarrer l'application et tester l'authentification par API key.

**Commande** :
```powershell
run_task → "dotnet: watch WebApi Default"
```

**Tests Chrome DevTools** :
- Effectuer requête avec API key valide → 200 OK
- Effectuer requête avec API key invalide → 401 Unauthorized
- Effectuer requête avec API key révoquée → 401 Unauthorized
- Vérifier console DevTools : Aucune erreur JavaScript
- Vérifier Network tab : Appels HTTP corrects

### 7. Commit atomique

**Action** : Commiter les changements avec message conventionnel.

**Commandes** :
```powershell
git add src/Infrastructure/LLMProxy.Infrastructure.Security/
git commit -m "refactor(security): separate ApiKeyAuthenticator types (ADR-001)

- Extract ApiKeyAuthenticationResult to dedicated file
- Extract IApiKeyAuthenticator interface to dedicated file
- Keep only ApiKeyAuthenticator implementation in original file
- Add complete XML documentation in French
- Ensure ADR-001 compliance (one type per file)

Refs: ADR-001, refactor.analysis.md"
```

## RÉFÉRENCES

- **ADR violé** : [docs/adr/001-un-seul-type-par-fichier-csharp.adr.md](../../../docs/adr/001-un-seul-type-par-fichier-csharp.adr.md)
- **Instructions applicables** :
  - `.github/instructions/csharp.standards.instructions.md`
  - `.github/instructions/csharp.documentation.instructions.md`
- **Analyse origine** : [refactor.analysis.md](../../refactor.analysis.md) (Section ApiKeyAuthenticator.cs)
- **ADR connexes** :
  - ADR-014 (Dependency Injection)
  - ADR-023 (Result Pattern)


## TRACKING
Début: 2025-12-21T16:22:38.3905109Z

Fin: 2025-12-21T16:24:47.2035946Z
Durée: 00:02:08

## RÉSULTAT

 **SUCCÈS** - Tous les critères validés

- Build : 0 errors, 0 warnings
- Tests : 66/66 passing (0 failed, 0 skipped)
- Conformité ADR-001 : 3 fichiers distincts créés
- Documentation XML complète en français

