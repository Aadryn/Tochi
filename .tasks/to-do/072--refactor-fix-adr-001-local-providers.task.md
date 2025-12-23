# Tâche 072 - Corriger ADR-001 : Extraire DTOs Local Providers (Ollama/vLLM)

## PRIORITÉ
🟠 **P2 - MAJEURE** (Violation ADR-001)

## OBJECTIF

Refactoriser les providers locaux pour respecter ADR-001.

## CONTEXTE

### Fichiers Concernés

| Fichier | Types | Effort |
|---------|-------|--------|
| `OllamaProviderClient.cs` | 11 | 3h |
| `VllmProviderClient.cs` | ~8 | 2h |

**Total estimé** : 5h

## IMPLÉMENTATION

### Structure Cible

```
Providers/Local/
├── Ollama/
│   ├── OllamaProviderClient.cs
│   └── Contracts/
│       ├── OllamaChatRequest.cs
│       └── ... (10 DTOs)
└── Vllm/
    ├── VllmProviderClient.cs
    └── Contracts/
        └── ... (~7 DTOs)
```

## CRITÈRES DE SUCCÈS

- [ ] OllamaProviderClient.cs : 1 type uniquement
- [ ] VllmProviderClient.cs : 1 type uniquement
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 5h
**Risque** : Faible
**Valeur** : Moyenne

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#

