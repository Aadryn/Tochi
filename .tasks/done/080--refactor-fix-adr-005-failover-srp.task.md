# Tâche 080 - Corriger ADR-005 : Refactorer FailoverManager (SRP)

## PRIORITÉ
🟠 **P2 - MAJEURE** (Violation ADR-005 SOLID)

## OBJECTIF

Refactoriser `FailoverManager.cs` qui viole le principe SRP avec 6 types et responsabilités multiples.

## CONTEXTE

### État Actuel

**Fichier :** `applications/proxy/backend/src/Application/LLMProxy.Application/LLMProviders/Services/FailoverManager.cs`

**Problèmes :**
- 6 types dans un seul fichier
- Responsabilités : retry, circuit breaker, health check, logging

## IMPLÉMENTATION

### Structure Cible

```
LLMProviders/Services/Failover/
├── IFailoverManager.cs
├── FailoverManager.cs               # Coordination
├── FailoverContext.cs
├── FailoverResult.cs
├── Retry/
│   ├── IRetryPolicy.cs
│   └── ExponentialRetryPolicy.cs
└── CircuitBreaker/
    ├── ICircuitBreaker.cs
    └── CircuitBreakerState.cs
```

### Étapes

1. Extraire FailoverContext et FailoverResult
2. Créer RetryPolicy (logique de retry)
3. Créer CircuitBreaker (logique circuit breaker)
4. Simplifier FailoverManager

## CRITÈRES DE SUCCÈS

- [ ] FailoverManager < 150 lignes
- [ ] 1 responsabilité par classe
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 4h

## RÉFÉRENCES

- ADR-001, ADR-005, ADR-023 (Resilience Polly)

