# Tâche 086 - Ajouter propriété MaxContextLength à LLMModel

## PRIORITÉ
🔴 **P1 - CRITIQUE** (9 erreurs de build)

## OBJECTIF

Ajouter la propriété `MaxContextLength` à l'entité `LLMModel` pour résoudre 9 erreurs de compilation dans les providers.

## CONTEXTE

### État Actuel

**Erreurs de build :**
- `CohereProviderClient.cs` : 2 erreurs (lignes 85, 96)
- `GoogleGeminiProviderClient.cs` : 3 erreurs (lignes 90, 102, 114)
- `HuggingFaceProviderClient.cs` : 4 erreurs (lignes 83, 92, 101, 110)

**Message d'erreur :**
```
error CS0117: 'LLMModel' does not contain a definition for 'MaxContextLength'
```

**Fichiers affectés :**
- `CohereProviderClient.cs` utilise `model.MaxContextLength` (2 occurrences)
- `GoogleGeminiProviderClient.cs` utilise `model.MaxContextLength` (3 occurrences)
- `HuggingFaceProviderClient.cs` utilise `model.MaxContextLength` (4 occurrences)

### Fichier à Modifier

**Fichier :** `applications/proxy/backend/src/Core/LLMProxy.Domain/LLM/LLMModel.cs`

**Localisation :** Chercher le fichier LLMModel dans le domaine

## IMPLÉMENTATION

### Étape 1 : Localiser LLMModel.cs

```bash
find /workspaces/proxy/applications/proxy/backend/src/Core -name "LLMModel.cs"
```

### Étape 2 : Analyser la Structure

Lire le fichier pour comprendre :
- Structure actuelle de la classe
- Propriétés existantes
- Pattern de nommage (PascalCase, documentation XML)

### Étape 3 : Ajouter MaxContextLength

Ajouter la propriété avec documentation XML :

```csharp
/// <summary>
/// Longueur maximale du contexte en tokens.
/// </summary>
/// <remarks>
/// <para>
/// Détermine le nombre maximum de tokens (input + output) que le modèle peut traiter.
/// </para>
/// <para>
/// <b>Exemples de valeurs typiques :</b>
/// <list type="bullet">
/// <item><c>4096</c> - GPT-3.5-turbo</item>
/// <item><c>8192</c> - GPT-4</item>
/// <item><c>32768</c> - GPT-4-32k</item>
/// <item><c>128000</c> - GPT-4-turbo, Claude-3</item>
/// <item><c>200000</c> - Claude-3.5-sonnet</item>
/// <item><c>1000000</c> - Gemini-1.5-pro</item>
/// </list>
/// </para>
/// </remarks>
/// <example>4096, 8192, 32768, 128000</example>
public int? MaxContextLength { get; private set; }
```

### Étape 4 : Mettre à Jour le Constructeur/Factory

Si `LLMModel` a un constructeur ou une méthode factory `Create()`, ajouter le paramètre :

```csharp
public static LLMModel Create(
    string name,
    string provider,
    // ... autres paramètres existants
    int? maxContextLength = null)
{
    return new LLMModel
    {
        // ... propriétés existantes
        MaxContextLength = maxContextLength
    };
}
```

### Étape 5 : Vérifier les Tests

Chercher les tests unitaires de LLMModel :

```bash
find /workspaces/proxy/applications/proxy/backend/tests -name "*LLMModel*Tests.cs"
```

Mettre à jour les tests si nécessaire.

## CRITÈRES DE SUCCÈS

- [ ] Propriété `MaxContextLength` ajoutée à `LLMModel`
- [ ] Documentation XML complète et en français
- [ ] Build Proxy Backend : 0 erreurs (9 erreurs résolues)
- [ ] Build Proxy Backend : 0 warnings
- [ ] Tests LLMModel passent (si existants)
- [ ] Conformité ADR-001 (déjà 1 type par fichier)

## VALIDATION

### Build
```bash
cd /workspaces/proxy/applications/proxy/backend
dotnet build --no-restore
# Attendu: 4 erreurs restantes (TokenUsage), 0 warning
```

### Tests
```bash
cd /workspaces/proxy/applications/proxy/backend
dotnet test --no-build --filter "FullyQualifiedName~LLMModel"
# Attendu: Tous les tests LLMModel passent
```

## ESTIMATION

**Effort** : 30 minutes

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#
- ADR-014 : DDD Value Objects et Entities
- Fichiers sources des erreurs :
  - `CohereProviderClient.cs`
  - `GoogleGeminiProviderClient.cs`
  - `HuggingFaceProviderClient.cs`

## NOTES

Cette propriété est déjà utilisée dans 3 providers, indiquant qu'elle a probablement été supprimée accidentellement lors d'un refactoring précédent.
