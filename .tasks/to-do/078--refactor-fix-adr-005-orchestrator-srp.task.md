# Tâche 078 - Corriger ADR-005 : Refactorer ProviderOrchestrator (SRP)

## PRIORITÉ
🟠 **P2 - MAJEURE** (Violation ADR-005 SOLID)

## OBJECTIF

Refactoriser `ProviderOrchestrator.cs` qui viole le principe SRP (Single Responsibility Principle) avec 7 types et ~600 lignes.

## CONTEXTE

### ADR-005 Règle Violée
> Une classe ne doit avoir qu'une seule raison de changer.

### État Actuel

**Fichier :** `applications/proxy/backend/src/Application/LLMProxy.Application/LLMProviders/Services/ProviderOrchestrator.cs`

**Problèmes :**
- 7 types dans un seul fichier
- Classe principale ~500 lignes
- Responsabilités multiples : routing, failover, metrics, logging

## IMPLÉMENTATION

### Structure Cible

```
LLMProviders/Services/
├── Orchestration/
│   ├── IProviderOrchestrator.cs
│   ├── ProviderOrchestrator.cs         # Orchestration uniquement
│   ├── OrchestrationContext.cs
│   └── OrchestrationResult.cs
├── Routing/
│   ├── IProviderRouter.cs
│   ├── ProviderRouter.cs
│   └── RoutingDecision.cs
├── Failover/
│   ├── IFailoverStrategy.cs
│   ├── FailoverStrategy.cs
│   └── FailoverContext.cs
└── Metrics/
    ├── IOrchestrationMetrics.cs
    └── OrchestrationMetrics.cs
```

### Étapes de Refactoring

1. **Extraire les DTOs** dans fichiers séparés (OrchestrationContext, OrchestrationResult)
2. **Extraire ProviderRouter** (logique de sélection)
3. **Extraire FailoverStrategy** (logique de retry)
4. **Extraire OrchestrationMetrics** (collecte métriques)
5. **Simplifier ProviderOrchestrator** (coordination uniquement)

## CRITÈRES DE SUCCÈS

- [ ] ProviderOrchestrator < 200 lignes
- [ ] 1 responsabilité par classe
- [ ] 1 type par fichier
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent (ajouter tests si manquants)

## ESTIMATION

**Effort** : 6h
**Risque** : Élevé (refactoring majeur)

## DÉPENDANCES

- Tâches 069-072 (DTOs providers) pour cohérence

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#
- ADR-005 : Principes SOLID (SRP)

