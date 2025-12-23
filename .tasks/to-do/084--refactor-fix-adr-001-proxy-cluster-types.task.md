# Tâche 084 - Corriger ADR-001 : Extraire ProxyCluster Types

## PRIORITÉ
🟢 **P3 - MINEURE** (Violation ADR-001)

## OBJECTIF

Extraire les 4 types du fichier `ProxyCluster.cs` vers des fichiers séparés.

## CONTEXTE

### État Actuel

**Fichier :** `applications/proxy/backend/src/Domain/LLMProxy.Domain/ProxyClusters/ProxyCluster.cs`

**Types à extraire :**
1. `ProxyCluster` (aggregate root)
2. `ClusterStatus`
3. `ClusterConfiguration`
4. `ClusterHealthStatus`

## IMPLÉMENTATION

### Structure Cible

```
ProxyClusters/
├── ProxyCluster.cs              # Aggregate root
├── ClusterStatus.cs             # Value object
├── ClusterConfiguration.cs      # Value object
└── ClusterHealthStatus.cs       # Value object
```

### Étapes

1. Extraire chaque Value Object dans fichier dédié
2. Garder ProxyCluster en aggregate root
3. Mettre à jour references

## CRITÈRES DE SUCCÈS

- [ ] 1 type par fichier
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests passent

## ESTIMATION

**Effort** : 1h

## RÉFÉRENCES

- ADR-001 : Un seul type par fichier C#
- ADR-014 : DDD Value Objects

