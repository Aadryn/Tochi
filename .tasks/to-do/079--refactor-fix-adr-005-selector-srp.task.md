# Tâche 079 - Corriger ADR-005 : Refactorer ProviderSelector (SRP)

## PRIORITÉ
🟠 **P2 - MAJEURE** (Violation ADR-005 SOLID)

## OBJECTIF

Refactoriser `ProviderSelector.cs` qui viole le principe SRP avec 6 types et responsabilités multiples.

## CONTEXTE

### ADR-005 Règle Violée
> Une classe ne doit avoir qu'une seule raison de changer.

### État Actuel

**Fichier :** `applications/proxy/backend/src/Application/LLMProxy.Application/LLMProviders/Services/ProviderSelector.cs`

**Problèmes :**
- 6 types dans un seul fichier
- Responsabilités : sélection, filtrage, scoring, validation

## IMPLÉMENTATION

### Structure Cible

```
LLMProviders/Services/Selection/
├── IProviderSelector.cs
├── ProviderSelector.cs              # Coordination
├── SelectionContext.cs
├── SelectionResult.cs
├── Scoring/
│   ├── IProviderScorer.cs
│   └── ProviderScorer.cs
└── Filtering/
    ├── IProviderFilter.cs
    └── ProviderFilter.cs
```

### Étapes

1. Extraire SelectionContext et SelectionResult
2. Créer ProviderScorer (logique de scoring)
3. Créer ProviderFilter (logique de filtrage)
4. Simplifier ProviderSelector

## CRITÈRES DE SUCCÈS

- [ ] ProviderSelector < 150 lignes
- [ ] 1 responsabilité par classe
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 4h

## RÉFÉRENCES

- ADR-001, ADR-005

