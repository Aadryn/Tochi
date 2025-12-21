# Tâche 002 - Séparer types HashService (ADR-001)

## MÉTADONNÉES

- **Priorité** : P1
- **Effort** : 1h
- **Risque** : Faible
- **Valeur** : Moyenne
- **Dépendances** : Aucune

## CONTEXTE

Fichier `src/Infrastructure/LLMProxy.Infrastructure.Security/HashService.cs` contient 2 types distincts :
1. `IHashService` (interface)
2. `Sha256HashService` (implémentation)

**Violation** : ADR-001 (Un seul type par fichier C#)

**Analyse origine** : refactor.analysis.md - Section HashService.cs

**Justification** : La violation de ADR-001 nuit à :
- La navigabilité du code (2 types dans un seul fichier)
- La cohérence architecturale du projet
- Le respect des standards de développement documentés

## OBJECTIF

Séparer les 2 types en 2 fichiers distincts pour respecter strictement ADR-001.

## CRITÈRES DE SUCCÈS

- [ ] 2 fichiers créés : `IHashService.cs`, `Sha256HashService.cs`
- [ ] Chaque fichier contient EXACTEMENT 1 type
- [ ] Build réussi sans aucun warning (0 Error(s), 0 Warning(s))
- [ ] Tests unitaires 100% réussis (49/49 passing, 0 failed, 0 skipped)
- [ ] Documentation XML complète et en français
- [ ] Conformité ADR-001 validée
- [ ] Aucune régression fonctionnelle

## ÉTAPES (BABY STEPS)

### 1. Créer IHashService.cs

**Action** : Extraire l'interface `IHashService` dans son propre fichier.

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Security/IHashService.cs`

**Contenu** :
```csharp
namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Service de hachage cryptographique.
/// </summary>
/// <remarks>
/// Cette interface définit le contrat pour les services de hachage.
/// Permet l'encapsulation des bibliothèques cryptographiques tierces (ADR-034).
/// </remarks>
public interface IHashService
{
    /// <summary>
    /// Calcule le hachage SHA-256 d'une chaîne de caractères.
    /// </summary>
    /// <param name="input">Chaîne à hacher (ne peut être null ou vide).</param>
    /// <returns>
    /// Hachage SHA-256 en format hexadécimal minuscule (64 caractères).
    /// </returns>
    /// <exception cref="ArgumentNullException">Si <paramref name="input"/> est null.</exception>
    /// <exception cref="ArgumentException">Si <paramref name="input"/> est vide ou ne contient que des espaces.</exception>
    /// <example>
    /// <code>
    /// var hash = hashService.ComputeSha256Hash("sk-abc123def456");
    /// // Résultat : "a1b2c3d4e5f6..." (64 caractères hexadécimaux)
    /// </code>
    /// </example>
    string ComputeSha256Hash(string input);
}
```

**Validation** :
```powershell
dotnet build src/Infrastructure/LLMProxy.Infrastructure.Security --no-restore
```

### 2. Créer Sha256HashService.cs

**Action** : Extraire la classe `Sha256HashService` dans son propre fichier.

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.Security/Sha256HashService.cs`

**Contenu** :
```csharp
using System.Security.Cryptography;
using System.Text;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Implémentation du service de hachage utilisant l'algorithme SHA-256.
/// </summary>
/// <remarks>
/// Cette classe fournit une implémentation thread-safe du hachage SHA-256.
/// Conforme à ADR-027 (Defensive Programming - Cryptographie) et ADR-034 (Encapsulation).
/// </remarks>
public class Sha256HashService : IHashService
{
    /// <summary>
    /// Calcule le hachage SHA-256 d'une chaîne de caractères.
    /// </summary>
    /// <param name="input">Chaîne à hacher (ne peut être null ou vide).</param>
    /// <returns>
    /// Hachage SHA-256 en format hexadécimal minuscule (64 caractères).
    /// </returns>
    /// <exception cref="ArgumentNullException">Si <paramref name="input"/> est null.</exception>
    /// <exception cref="ArgumentException">Si <paramref name="input"/> est vide ou ne contient que des espaces.</exception>
    /// <remarks>
    /// La méthode utilise l'encodage UTF-8 pour convertir la chaîne en bytes.
    /// Le résultat est retourné en hexadécimal minuscule pour cohérence avec la base de données.
    /// L'instance SHA256 est correctement disposée via 'using' (ADR-027).
    /// </remarks>
    /// <example>
    /// <code>
    /// var hashService = new Sha256HashService();
    /// var hash = hashService.ComputeSha256Hash("sk-abc123def456");
    /// // Résultat : "a1b2c3d4e5f6..." (64 caractères hexadécimaux en minuscules)
    /// </code>
    /// </example>
    public string ComputeSha256Hash(string input)
    {
        Guard.AgainstNullOrWhiteSpace(input, nameof(input));
        
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
```

**Validation** :
```powershell
dotnet build src/Infrastructure/LLMProxy.Infrastructure.Security --no-restore
```

### 3. Supprimer l'ancien HashService.cs

**Action** : Supprimer le fichier original qui contenait les 2 types.

**Commande** :
```powershell
Remove-Item src/Infrastructure/LLMProxy.Infrastructure.Security/HashService.cs
```

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

**Action** : Tester le hachage dans le contexte de l'authentification.

**Commande** :
```powershell
run_task → "dotnet: watch WebApi Default"
```

**Tests Chrome DevTools** :
- Effectuer requête avec API key valide → 200 OK (hachage fonctionne)
- Vérifier que l'authentification utilise correctement le service de hachage
- Vérifier console DevTools : Aucune erreur
- Vérifier logs : Hachage SHA-256 appliqué correctement

### 7. Commit atomique

**Action** : Commiter les changements avec message conventionnel.

**Commandes** :
```powershell
git add src/Infrastructure/LLMProxy.Infrastructure.Security/
git commit -m "refactor(security): separate HashService types (ADR-001)

- Extract IHashService interface to dedicated file
- Extract Sha256HashService implementation to dedicated file
- Remove original HashService.cs file
- Add complete XML documentation in French
- Ensure ADR-001 compliance (one type per file)

Refs: ADR-001, ADR-027, ADR-034, refactor.analysis.md"
```

## RÉFÉRENCES

- **ADR violé** : [docs/adr/001-un-seul-type-par-fichier-csharp.adr.md](../../../docs/adr/001-un-seul-type-par-fichier-csharp.adr.md)
- **Instructions applicables** :
  - `.github/instructions/csharp.standards.instructions.md`
  - `.github/instructions/csharp.documentation.instructions.md`
- **Analyse origine** : [refactor.analysis.md](../../refactor.analysis.md) (Section HashService.cs)
- **ADR connexes** :
  - ADR-027 (Defensive Programming - Cryptographie)
  - ADR-034 (Encapsulation des Bibliothèques Tierces)


## TRACKING
Début: 2025-12-21T16:25:39.9714783Z

Fin: 2025-12-21T16:26:41.7274888Z
Durée: 00:01:01

## RÉSULTAT

 **SUCCÈS** - Tous les critères validés

- Build : 0 errors, 0 warnings
- Tests : 66/66 passing
- Conformité ADR-001 : 2 fichiers distincts + suppression original
- Documentation XML complète en français

