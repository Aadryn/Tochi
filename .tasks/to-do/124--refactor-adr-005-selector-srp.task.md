# TÂCHE 124 : Refactor ADR-005 - ProviderSelector SRP

**Créée le** : 2025-12-24T00:00:00Z UTC  
**Priorité** : 🟡 Majeure  
**ADR Violée** : ADR-005 - SOLID (SRP)

---

## CONTEXTE

**Fichier** : `ProviderSelector.cs`  
**Violations** : Multiples responsabilités

---

## OBJECTIF

Séparer les stratégies de sélection en classes distinctes.

---

## APPROCHE

Strategy Pattern :
- `IProviderSelectionStrategy`
- `RoundRobinStrategy`
- `LeastLatencyStrategy`
- `WeightedStrategy`

---

## MÉTADONNÉES

- **Effort** : 4h
- **Risque** : 5/10
