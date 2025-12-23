# Tâche 082 - Corriger ADR-001 : Extraire Result Types

## PRIORITÉ
🟢 **P3 - MINEURE** (Violation ADR-001)

## OBJECTIF

Extraire les 3 types du fichier `Result.cs` vers des fichiers séparés.

## CONTEXTE

### État Actuel

**Fichier :** `applications/authorization/backend/src/Domain/LLMProxy.Authorization.Domain/Common/Result.cs`

**Types à extraire :**
1. `Result` (classe base)
2. `Result<T>` (classe générique)
3. `Error` (record)

## IMPLÉMENTATION

### Structure Cible

```
Common/
├── Result.cs              # Result non-générique
├── ResultOfT.cs           # Result<T> générique
└── Error.cs               # Error record
```

### Étapes

1. Extraire `Error` dans `Error.cs`
2. Extraire `Result<T>` dans `ResultOfT.cs`
3. Garder `Result` dans `Result.cs`
4. Mettre à jour references

## CRITÈRES DE SUCCÈS

- [ ] 1 type par fichier
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 1h

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#

