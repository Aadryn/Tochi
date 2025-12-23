# Tâche 087 - Résoudre ambiguïté TokenUsage dans AWSBedrockProviderClient

## PRIORITÉ
🔴 **P1 - CRITIQUE** (4 erreurs de build)

## OBJECTIF

Résoudre le conflit de nommage `TokenUsage` dans `AWSBedrockProviderClient` en qualifiant explicitement les types.

## CONTEXTE

### État Actuel

**Erreurs de build :**
- `AWSBedrockProviderClient.cs` : 4 erreurs (lignes 294, 409, 426, 443)

**Message d'erreur :**
```
error CS0104: 'TokenUsage' is an ambiguous reference between 
'LLMProxy.Domain.LLM.TokenUsage' and 'Amazon.BedrockRuntime.Model.TokenUsage'
```

**Problème :**
Le fichier importe à la fois :
- `LLMProxy.Domain.LLM.TokenUsage` (notre domaine)
- `Amazon.BedrockRuntime.Model.TokenUsage` (SDK AWS)

Les deux types ont le même nom, causant une ambiguïté.

### Fichier à Modifier

**Fichier :** `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/PublicCloud/AWSBedrockProviderClient.cs`

## ANALYSE

### Solution Recommandée : Alias de Namespace

Créer un alias pour le type AWS afin de distinguer les deux :

```csharp
using AwsTokenUsage = Amazon.BedrockRuntime.Model.TokenUsage;
using LLMProxy.Domain.LLM; // TokenUsage du domaine
```

### Lignes à Modifier

**4 occurrences à qualifier :**
1. Ligne 294
2. Ligne 409
3. Ligne 426
4. Ligne 443

## IMPLÉMENTATION

### Étape 1 : Analyser les Usings

Lire le début du fichier AWSBedrockProviderClient.cs :

```bash
head -30 applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/PublicCloud/AWSBedrockProviderClient.cs
```

Identifier les `using` statements existants.

### Étape 2 : Ajouter l'Alias

Ajouter après les imports existants :

```csharp
using AwsTokenUsage = Amazon.BedrockRuntime.Model.TokenUsage;
```

### Étape 3 : Analyser les Occurrences

Lire les lignes concernées pour comprendre le contexte :

```bash
sed -n '290,300p' AWSBedrockProviderClient.cs
sed -n '405,415p' AWSBedrockProviderClient.cs
sed -n '422,432p' AWSBedrockProviderClient.cs
sed -n '439,449p' AWSBedrockProviderClient.cs
```

### Étape 4 : Qualifier les Types

Pour chaque occurrence, déterminer s'il s'agit de :
- **TokenUsage AWS** (provenant de la réponse Bedrock) → utiliser `AwsTokenUsage`
- **TokenUsage Domaine** (notre modèle) → garder `TokenUsage`

**Pattern typique :**
```csharp
// AVANT (ambigu)
var usage = response.Usage; // AWS TokenUsage
return new TokenUsage(...); // Domain TokenUsage

// APRÈS (clair)
AwsTokenUsage usage = response.Usage; // AWS explicite
return new TokenUsage(...); // Domain implicite
```

### Étape 5 : Vérifier Cohérence

S'assurer que :
- Les conversions AWS → Domaine sont correctes
- Aucune autre ambiguïté introduite
- Le code reste lisible

## CRITÈRES DE SUCCÈS

- [ ] Alias `AwsTokenUsage` créé dans les using statements
- [ ] 4 occurrences qualifiées explicitement
- [ ] Build Proxy Backend : 0 erreurs (4 erreurs résolues)
- [ ] Build Proxy Backend : 0 warnings
- [ ] Tests AWSBedrockProviderClient passent (si existants)
- [ ] Code lisible et maintenable

## VALIDATION

### Build
```bash
cd /workspaces/proxy/applications/proxy/backend
dotnet build --no-restore
# Attendu: 0 erreurs, 0 warnings (si tâche 086 complétée)
```

### Tests
```bash
cd /workspaces/proxy/applications/proxy/backend
dotnet test --no-build --filter "FullyQualifiedName~AWSBedrock"
# Attendu: Tous les tests AWSBedrock passent
```

## ESTIMATION

**Effort** : 30 minutes

## DÉPENDANCES

**Optionnel** : Cette tâche est indépendante de la tâche 086 (MaxContextLength). Les deux peuvent être traitées en parallèle ou dans n'importe quel ordre.

## RÉFÉRENCES

- ADR-002 : KISS - Simplicité
- Microsoft C# Docs : [Using alias directive](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-directive#using-alias)
- Fichier source : `AWSBedrockProviderClient.cs`

## NOTES

**Approche alternative (non recommandée) :**
- Renommer notre `TokenUsage` en `LLMTokenUsage` → Casse beaucoup de code existant
- Fully qualify tous les usages → Code verbeux et moins lisible

**Approche recommandée (alias) :**
- Minimal impact sur le code
- Lisibilité préservée
- Pattern standard C#
