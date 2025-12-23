# TÂCHE 128 : Refactor ADR-001 - Auth Domain Events

**Créée le** : 2025-12-24T00:00:00Z UTC  
**Priorité** : 🟢 Mineure  
**ADR Violée** : ADR-001

---

## CONTEXTE

**Fichier** : `RoleAssignmentEvents.cs`  
**Types** : 4 events dans un fichier

---

## OBJECTIF

Extraire chaque event dans son propre fichier :
- `RoleAssignedEvent.cs`
- `RoleRevokedEvent.cs`
- `RoleUpdatedEvent.cs`
- `RoleDeletedEvent.cs`

---

## MÉTADONNÉES

- **Effort** : 2h
- **Risque** : 1/10
