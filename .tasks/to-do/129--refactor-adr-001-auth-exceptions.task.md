# TÂCHE 129 : Refactor ADR-001 - Auth Exceptions

**Créée le** : 2025-12-24T00:00:00Z UTC  
**Priorité** : 🟢 Mineure  
**ADR Violée** : ADR-001

---

## CONTEXTE

**Fichier** : `NotFoundException.cs`  
**Types** : 4 exceptions

---

## OBJECTIF

Extraire :
- `TenantNotFoundException.cs`
- `UserNotFoundException.cs`
- `RoleNotFoundException.cs`
- `PermissionNotFoundException.cs`

---

## MÉTADONNÉES

- **Effort** : 2h
- **Risque** : 1/10
