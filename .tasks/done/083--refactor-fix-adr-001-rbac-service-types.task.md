# Tâche 083 - Corriger ADR-001 : Extraire IRbacAuthorizationService Types

## PRIORITÉ
🟢 **P3 - MINEURE** (Violation ADR-001)

## OBJECTIF

Extraire les 3 types du fichier `IRbacAuthorizationService.cs` vers des fichiers séparés.

## CONTEXTE

### État Actuel

**Fichier :** `applications/authorization/backend/src/Application/LLMProxy.Authorization.Application/Services/IRbacAuthorizationService.cs`

**Types à extraire :**
1. `IRbacAuthorizationService` (interface principale)
2. `AuthorizationCheckRequest`
3. `AuthorizationCheckResult`

## IMPLÉMENTATION

### Structure Cible

```
Services/
├── Authorization/
│   ├── IRbacAuthorizationService.cs
│   ├── AuthorizationCheckRequest.cs
│   └── AuthorizationCheckResult.cs
```

### Étapes

1. Créer dossier `Authorization/`
2. Extraire chaque type dans fichier dédié
3. Mettre à jour references

## CRITÈRES DE SUCCÈS

- [ ] 1 type par fichier
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 45min

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#

