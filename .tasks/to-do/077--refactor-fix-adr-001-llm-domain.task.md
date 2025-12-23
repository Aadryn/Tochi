# Tâche 077 - Corriger ADR-001 : LLM Domain Models

## PRIORITÉ
🟡 **P3 - MINEURE** (Violation ADR-001)

## OBJECTIF

Refactoriser les fichiers LLM du Domain pour respecter ADR-001.

## CONTEXTE

### Fichiers Concernés

| Fichier | Types | Effort |
|---------|-------|--------|
| `LLMRequest.cs` | 2 | 30min |
| `LLMResponse.cs` | 2 | 30min |
| `LLMMessage.cs` | 2 | 30min |
| `LLMModel.cs` | 2 | 30min |
| `EmbeddingRequest.cs` | 2 | 30min |
| `EmbeddingResponse.cs` | 2 | 30min |
| `LLMRequestEvents.cs` | 3 | 45min |

## IMPLÉMENTATION

### Structure Cible

```
LLMProxy.Domain/LLM/
├── Models/
│   ├── LLMRequest.cs
│   ├── LLMRequestOptions.cs
│   ├── LLMResponse.cs
│   ├── LLMResponseChoice.cs
│   ├── LLMMessage.cs
│   ├── LLMMessageRole.cs
│   ├── LLMModel.cs
│   └── LLMModelCapabilities.cs
├── Embeddings/
│   ├── EmbeddingRequest.cs
│   ├── EmbeddingRequestOptions.cs
│   ├── EmbeddingResponse.cs
│   └── EmbeddingData.cs
└── Events/
    ├── LLMRequestStartedEvent.cs
    ├── LLMRequestCompletedEvent.cs
    └── LLMRequestFailedEvent.cs
```

## CRITÈRES DE SUCCÈS

- [ ] 1 type par fichier
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 4h
**Risque** : Moyen (nombreuses références)

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#
- ADR-057 : Multi-Provider LLM Abstraction

