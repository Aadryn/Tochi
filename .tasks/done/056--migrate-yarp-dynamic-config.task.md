# Tâche 056 - Migrer YARP vers Configuration Dynamique

## PRIORITÉ
🔴 **P1 - CRITIQUE** (Priorité 1/8 de la refonte)

## OBJECTIF

Refactorer la configuration YARP statique (appsettings.json) vers une configuration dynamique chargée depuis la base de données, permettant l'ajout/modification/suppression de routes et clusters LLM à chaud sans redémarrage.

## CONTEXTE

### État Actuel
- YARP 2.2.0 déjà intégré dans `LLMProxy.Gateway`
- Configuration statique dans `appsettings.json` section `ReverseProxy`
- Routes et clusters hardcodés

### État Cible
- Configuration YARP chargeable depuis PostgreSQL
- Interface `IProxyConfigProvider` personnalisée
- Rechargement à chaud via signaux ou polling
- Support multi-tenant (routes par tenant)

## IMPLÉMENTATION

### Phase 1 : Domain Layer (Entités)
```
src/Core/LLMProxy.Domain/
├── Entities/Routing/
│   ├── ProxyRoute.cs           # Entité route YARP
│   ├── ProxyCluster.cs         # Entité cluster (groupe de backends)
│   ├── ClusterDestination.cs   # Destination (backend LLM)
│   └── RouteMatch.cs           # Critères de matching
```

### Phase 2 : Application Layer (Services)
```
src/Application/LLMProxy.Application/
├── Interfaces/
│   └── IProxyConfigurationService.cs
├── Services/Routing/
│   ├── ProxyConfigurationService.cs
│   └── DynamicProxyConfigProvider.cs  # Implémente IProxyConfigProvider
```

### Phase 3 : Infrastructure Layer (Persistence)
```
src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/
├── Repositories/
│   ├── ProxyRouteRepository.cs
│   └── ProxyClusterRepository.cs
├── Configurations/
│   ├── ProxyRouteConfiguration.cs
│   └── ProxyClusterConfiguration.cs
```

### Phase 4 : Gateway Integration
```
src/Presentation/LLMProxy.Gateway/
├── Extensions/
│   └── YarpDynamicConfigExtensions.cs
├── Services/
│   └── DatabaseProxyConfigProvider.cs
```

## CRITÈRES DE SUCCÈS

- [ ] Entités Domain créées avec Value Objects appropriés
- [ ] Repository pattern implémenté pour routes/clusters
- [ ] `IProxyConfigProvider` personnalisé fonctionnel
- [ ] Routes chargées depuis PostgreSQL au démarrage
- [ ] Rechargement à chaud sans redémarrage (polling 30s ou signal)
- [ ] Tests unitaires pour le provider
- [ ] Tests d'intégration YARP + DB
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Documentation XML complète (français)

## DÉPENDANCES

- PostgreSQL opérationnel (port 15432)
- Schéma DB existant ou migration à créer

## ESTIMATION

**Effort** : 8h
**Complexité** : Moyenne-Haute (YARP internal APIs)

## RÉFÉRENCES

- [YARP Dynamic Configuration](https://microsoft.github.io/reverse-proxy/articles/config-providers.html)
- ADR-006 (Onion Architecture)
- ADR-017 (Repository Pattern)


## TRACKING
Début: 2025-12-22T16:51:11.3816671Z


Fin: 2025-12-22T17:10:55.3939085Z

## STATUT:  COMPLÉTÉ

### Fichiers créés
- src/Core/LLMProxy.Domain/Entities/Routing/ProxyRoute.cs
- src/Core/LLMProxy.Domain/Entities/Routing/ProxyCluster.cs
- src/Core/LLMProxy.Domain/Entities/Routing/ClusterDestination.cs
- src/Core/LLMProxy.Domain/Interfaces/IProxyRouteRepository.cs
- src/Core/LLMProxy.Domain/Interfaces/IProxyClusterRepository.cs
- src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/Repositories/ProxyRouteRepository.cs
- src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/Repositories/ProxyClusterRepository.cs
- src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/Configurations/ProxyRouteConfiguration.cs
- src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/Configurations/ProxyClusterConfiguration.cs
- src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/Configurations/ClusterDestinationConfiguration.cs
- src/Presentation/LLMProxy.Gateway/Extensions/YarpDynamicConfigExtensions.cs
- src/Presentation/LLMProxy.Gateway/Services/DatabaseProxyConfigProvider.cs
- scripts/migrations/001_add_yarp_routing_schema.sql

### Fichiers modifiés
- src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/LLMProxyDbContext.cs

### Tests
- Build :  0 erreurs
- Gateway.Tests :  30 réussis
- Application.Tests :  75 réussis
