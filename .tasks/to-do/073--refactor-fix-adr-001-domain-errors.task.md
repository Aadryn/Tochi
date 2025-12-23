# Tâche 073 - Corriger ADR-001 : Domain Error Types

## PRIORITÉ
🟡 **P3 - MINEURE** (Violation ADR-001)

## OBJECTIF

Refactoriser `Error.cs` du Domain pour respecter ADR-001. Ce fichier contient **7 types**.

## CONTEXTE

**Fichier :** `applications/proxy/backend/src/Core/LLMProxy.Domain/Common/Error.cs`

**Types à extraire :**
1. `Error` (record principal)
2. `ErrorType` (enum)
3. `ValidationError` (record)
4. `NotFoundError` (record)
5. `ConflictError` (record)
6. `UnauthorizedError` (record)
7. `InternalError` (record)

## IMPLÉMENTATION

### Structure Cible

```
Domain/Common/Errors/
├── Error.cs
├── ErrorType.cs
├── ValidationError.cs
├── NotFoundError.cs
├── ConflictError.cs
├── UnauthorizedError.cs
└── InternalError.cs
```

## CRITÈRES DE SUCCÈS

- [ ] 7 fichiers séparés
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 2h
**Risque** : Faible
**Valeur** : Moyenne

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#

