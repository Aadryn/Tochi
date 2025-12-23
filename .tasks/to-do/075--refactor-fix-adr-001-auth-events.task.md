# Tâche 075 - Corriger ADR-001 : Authorization Domain Events

## PRIORITÉ
🟡 **P3 - MINEURE** (Violation ADR-001)

## OBJECTIF

Refactoriser les fichiers d'événements du domaine Authorization pour respecter ADR-001.

## CONTEXTE

### Fichiers Concernés

| Fichier | Types | Effort |
|---------|-------|--------|
| `RoleAssignmentEvents.cs` | 4 | 1h |
| `RoleDefinitionEvents.cs` | 3 | 45min |
| `PrincipalSyncedEvent.cs` | 2 | 30min |

## IMPLÉMENTATION

### Structure Cible

```
Authorization.Domain/Events/
├── RoleAssignment/
│   ├── RoleAssignmentCreatedEvent.cs
│   ├── RoleAssignmentRevokedEvent.cs
│   ├── RoleAssignmentExpiredEvent.cs
│   └── RoleAssignmentUpdatedEvent.cs
├── RoleDefinition/
│   ├── RoleDefinitionCreatedEvent.cs
│   ├── RoleDefinitionUpdatedEvent.cs
│   └── RoleDefinitionDeletedEvent.cs
└── Principal/
    ├── PrincipalSyncedEvent.cs
    └── PrincipalDeactivatedEvent.cs
```

## CRITÈRES DE SUCCÈS

- [ ] 1 événement par fichier
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 2.5h
**Risque** : Faible

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#
- ADR-025 : Domain Events

