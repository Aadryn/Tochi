# Tâche 070 - Corriger ADR-001 : Extraire DTOs OpenAI Provider

## PRIORITÉ
🔴 **P1 - CRITIQUE** (Violation majeure ADR-001)

## OBJECTIF

Refactoriser `OpenAIProviderClient.cs` pour respecter ADR-001 (Un seul type par fichier). Actuellement, ce fichier contient **15 types**.

## CONTEXTE

**Fichier :** `applications/proxy/backend/src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Providers/PublicCloud/OpenAIProviderClient.cs`

**Types à extraire (14 DTOs) :**
1. `OpenAIChatRequest`
2. `OpenAIChatResponse`
3. `OpenAIMessage`
4. `OpenAIChoice`
5. `OpenAIUsage`
6. `OpenAIModel`
7. `OpenAIModelsResponse`
8. `OpenAIEmbedRequest`
9. `OpenAIEmbedResponse`
10. `OpenAIEmbedData`
11. `OpenAICompletionRequest`
12. `OpenAICompletionResponse`
13. `OpenAIToolCall`
14. `OpenAIError`

## IMPLÉMENTATION

### Structure Cible

```
Providers/PublicCloud/OpenAI/
├── OpenAIProviderClient.cs
└── Contracts/
    ├── OpenAIChatRequest.cs
    ├── OpenAIChatResponse.cs
    ├── OpenAIMessage.cs
    ├── OpenAIChoice.cs
    ├── OpenAIUsage.cs
    ├── OpenAIModel.cs
    ├── OpenAIModelsResponse.cs
    ├── OpenAIEmbedRequest.cs
    ├── OpenAIEmbedResponse.cs
    ├── OpenAIEmbedData.cs
    ├── OpenAICompletionRequest.cs
    ├── OpenAICompletionResponse.cs
    ├── OpenAIToolCall.cs
    └── OpenAIError.cs
```

## CRITÈRES DE SUCCÈS

- [ ] `OpenAIProviderClient.cs` contient UNIQUEMENT la classe principale
- [ ] 14 nouveaux fichiers créés dans `OpenAI/Contracts/`
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 5h
**Risque** : Moyen
**Valeur** : Haute

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#
- Tâche 069 (pattern similaire pour Cohere)

