# Tâche 069 - Corriger ADR-001 : Extraire DTOs Cohere Provider

## PRIORITÉ
🔴 **P1 - CRITIQUE** (Violation majeure ADR-001)

## OBJECTIF

Refactoriser `CohereProviderClient.cs` pour respecter ADR-001 (Un seul type par fichier). Actuellement, ce fichier contient **19 types** alors qu'il devrait en contenir 1 seul.

## CONTEXTE

### ADR-001 Règle Violée
> Chaque fichier `.cs` doit contenir exactement un type (class, interface, enum, struct, record).

### État Actuel

**Fichier :** `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/PublicCloud/CohereProviderClient.cs`

**Types identifiés (19) :**
1. `CohereProviderClient` (classe principale)
2. `CohereChatRequest` (record DTO)
3. `CohereChatResponse` (record DTO)
4. `CohereMessage` (record DTO)
5. `CohereTextContent` (record DTO)
6. `CohereImageContent` (record DTO)
7. `CohereGenerateRequest` (record DTO)
8. `CohereGenerateResponse` (record DTO)
9. `CohereEmbedRequest` (record DTO)
10. `CohereEmbedResponse` (record DTO)
11. `CohereModel` (record DTO)
12. `CohereModelsResponse` (record DTO)
13. `CohereUsage` (record DTO)
14. `CohereBilledUnits` (record DTO)
15. `CohereApiVersion` (record DTO)
16. `CohereToolCall` (record DTO)
17. `CohereToolResult` (record DTO)
18. `CohereCitation` (record DTO)
19. `CohereError` (record DTO)

## IMPLÉMENTATION

### Structure Cible

```
Providers/PublicCloud/Cohere/
├── CohereProviderClient.cs           # Classe principale uniquement
└── Contracts/
    ├── CohereChatRequest.cs
    ├── CohereChatResponse.cs
    ├── CohereMessage.cs
    ├── CohereTextContent.cs
    ├── CohereImageContent.cs
    ├── CohereGenerateRequest.cs
    ├── CohereGenerateResponse.cs
    ├── CohereEmbedRequest.cs
    ├── CohereEmbedResponse.cs
    ├── CohereModel.cs
    ├── CohereModelsResponse.cs
    ├── CohereUsage.cs
    ├── CohereBilledUnits.cs
    ├── CohereApiVersion.cs
    ├── CohereToolCall.cs
    ├── CohereToolResult.cs
    ├── CohereCitation.cs
    └── CohereError.cs
```

### Étapes de Refactoring

1. **Créer le dossier `Cohere/Contracts/`**
2. **Extraire chaque record/class DTO** dans un fichier séparé avec :
   - Namespace : `LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts`
   - Mêmes attributs et documentation
3. **Mettre à jour les imports** dans `CohereProviderClient.cs`
4. **Vérifier la compilation**
5. **Exécuter les tests existants**

### Baby Steps

**Step 1** : Créer dossier + 5 premiers DTOs
```csharp
// CohereChatRequest.cs
namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

public sealed record CohereChatRequest(
    string Model,
    List<CohereMessage> Messages,
    float? Temperature,
    int? MaxTokens,
    bool Stream
);
```

**Step 2** : Extraire les 5 DTOs suivants
**Step 3** : Extraire les 9 DTOs restants
**Step 4** : Nettoyer CohereProviderClient.cs
**Step 5** : Valider build + tests

## CRITÈRES DE SUCCÈS

- [ ] `CohereProviderClient.cs` contient UNIQUEMENT la classe `CohereProviderClient`
- [ ] 18 nouveaux fichiers créés dans `Cohere/Contracts/`
- [ ] Tous les imports correctement mis à jour
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests unitaires existants passent
- [ ] Tests d'intégration (si existants) passent

## ESTIMATION

**Effort** : 6h
**Risque** : Moyen (nombreux fichiers, références à mettre à jour)
**Valeur** : Haute (navigation IDE, historique Git propre)

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#
- `refactor.analysis.md` : Analyse détaillée

## DÉPENDANCES

- Aucune (peut être exécutée indépendamment)

