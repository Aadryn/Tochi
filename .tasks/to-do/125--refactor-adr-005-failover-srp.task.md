# TÂCHE 125 : Refactor ADR-005 - FailoverManager SRP

**Créée le** : 2025-12-24T00:00:00Z UTC  
**Priorité** : 🟡 Majeure  
**ADR Violée** : ADR-005 - SOLID (SRP)

---

## CONTEXTE

**Fichier** : `FailoverManager.cs`  
**Responsabilités** : Retry + Circuit Breaker + Metrics

---

## OBJECTIF

Séparer :
1. `RetryPolicy` : Gestion retry
2. `CircuitBreakerPolicy` : Circuit breaker
3. `FailoverMetrics` : Métriques failover

---

## MÉTADONNÉES

- **Effort** : 4h
- **Risque** : 6/10
