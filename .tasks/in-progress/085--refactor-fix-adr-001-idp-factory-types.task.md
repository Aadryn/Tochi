# Tâche 085 - Corriger ADR-001 : Extraire IdpClientFactory Types

## PRIORITÉ
🟢 **P3 - MINEURE** (Violation ADR-001)

## OBJECTIF

Extraire les 2 types du fichier `IdpClientFactory.cs` vers des fichiers séparés.

## CONTEXTE

### État Actuel

**Fichier :** `applications/authorization/backend/src/Infrastructure/LLMProxy.Authorization.Infrastructure/Clients/IdpClientFactory.cs`

**Types à extraire :**
1. `IdpClientFactory` (factory)
2. `IdpClientOptions` (configuration)

## IMPLÉMENTATION

### Structure Cible

```
Clients/
├── IdpClientFactory.cs
└── IdpClientOptions.cs
```

### Étapes

1. Extraire `IdpClientOptions` dans fichier dédié
2. Mettre à jour using statements

## CRITÈRES DE SUCCÈS

- [ ] 1 type par fichier
- [ ] Build : 0 erreurs, 0 warnings

## ESTIMATION

**Effort** : 30min

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#

