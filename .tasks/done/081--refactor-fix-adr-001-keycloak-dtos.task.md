# Tâche 081 - Corriger ADR-001 : Extraire KeycloakClient DTOs

## PRIORITÉ
🟠 **P2 - MAJEURE** (Violation ADR-001)

## OBJECTIF

Extraire les 5 types du fichier `KeycloakClient.cs` vers des fichiers séparés.

## CONTEXTE

### État Actuel

**Fichier :** `applications/authorization/backend/src/Infrastructure/LLMProxy.Authorization.Infrastructure/Clients/KeycloakClient.cs`

**Types à extraire :**
1. `KeycloakClient` (classe principale)
2. `KeycloakTokenResponse`
3. `KeycloakUserInfo`
4. `KeycloakClientConfig`
5. `KeycloakOptions`

## IMPLÉMENTATION

### Structure Cible

```
Clients/
├── Keycloak/
│   ├── IKeycloakClient.cs
│   ├── KeycloakClient.cs
│   ├── KeycloakOptions.cs
│   └── Contracts/
│       ├── KeycloakTokenResponse.cs
│       ├── KeycloakUserInfo.cs
│       └── KeycloakClientConfig.cs
```

### Étapes

1. Créer dossier `Keycloak/Contracts/`
2. Extraire chaque DTO dans fichier dédié
3. Mettre à jour using statements
4. Valider build et tests

## CRITÈRES DE SUCCÈS

- [ ] 1 type par fichier
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 1.5h

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#

