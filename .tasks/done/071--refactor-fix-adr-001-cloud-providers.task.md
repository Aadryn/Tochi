# Tâche 071 - Corriger ADR-001 : Extraire DTOs Azure/Google/AWS/Anthropic Providers

## PRIORITÉ
🟠 **P2 - MAJEURE** (Violation ADR-001)

## OBJECTIF

Refactoriser les providers cloud restants pour respecter ADR-001 (Un seul type par fichier).

## CONTEXTE

### Fichiers Concernés

| Fichier | Types | Effort |
|---------|-------|--------|
| `AzureOpenAIProviderClient.cs` | 13 | 4h |
| `GoogleGeminiProviderClient.cs` | 13 | 4h |
| `HuggingFaceProviderClient.cs` | 12 | 4h |
| `AWSBedrockProviderClient.cs` | 12 | 4h |
| `AnthropicProviderClient.cs` | 10 | 3h |

**Total estimé** : 19h

## IMPLÉMENTATION

### Pattern de Refactoring (Identique pour chaque provider)

```
Providers/PublicCloud/{ProviderName}/
├── {ProviderName}ProviderClient.cs
└── Contracts/
    ├── {ProviderName}ChatRequest.cs
    ├── {ProviderName}ChatResponse.cs
    └── ... (autres DTOs)
```

### Ordre d'Exécution

1. **AzureOpenAI** (réutilise structure OpenAI)
2. **GoogleGemini** (structure unique)
3. **Anthropic** (structure unique)
4. **AWSBedrock** (complexité AWS)
5. **HuggingFace** (nombreux modèles)

## CRITÈRES DE SUCCÈS

- [ ] Chaque provider : 1 type par fichier
- [ ] Structure `{Provider}/Contracts/` pour chaque
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 19h (3-4 jours)
**Risque** : Moyen
**Valeur** : Haute

## DÉPENDANCES

- Tâche 069 (Cohere - pattern établi)
- Tâche 070 (OpenAI - peut réutiliser certains DTOs)

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#

