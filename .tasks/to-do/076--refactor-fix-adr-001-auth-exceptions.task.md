# Tâche 076 - Corriger ADR-001 : Authorization Exceptions

## PRIORITÉ
🟡 **P3 - MINEURE** (Violation ADR-001)

## OBJECTIF

Refactoriser les fichiers d'exceptions du domaine Authorization pour respecter ADR-001.

## CONTEXTE

### Fichiers Concernés

| Fichier | Types | Effort |
|---------|-------|--------|
| `AuthorizationException.cs` | 3 | 30min |
| `NotFoundException.cs` | 4 | 45min |
| `DuplicateException.cs` | 2 | 20min |

## IMPLÉMENTATION

### Structure Cible

```
Authorization.Domain/Exceptions/
├── AuthorizationException.cs
├── AuthorizationDomainException.cs
├── ValidationException.cs
├── NotFoundException.cs
├── PrincipalNotFoundException.cs
├── RoleNotFoundException.cs
├── AssignmentNotFoundException.cs
├── DuplicateException.cs
└── DuplicateAssignmentException.cs
```

## CRITÈRES DE SUCCÈS

- [ ] 1 exception par fichier
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 1.5h
**Risque** : Faible

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#
- ADR-043 : Exception Handling Strategy

