# Tâche 061 - Implémenter OpenFGA Authorization ReBAC

## PRIORITÉ
🟠 **P2 - HAUTE** (Priorité 6/8 de la refonte)

## OBJECTIF

Intégrer OpenFGA pour une autorisation fine ReBAC (Relationship-Based Access Control) hiérarchique sur toutes les ressources : tenants, providers, API keys, routes, configurations.

## CONTEXTE

### Choix Technique
- **Engine** : OpenFGA (open source, CNCF)
- **Modèle** : ReBAC hiérarchique fin
- **SDK** : OpenFga.Sdk .NET 0.7.0

### Ressources à Protéger
- `tenant` : isolation multi-tenant
- `provider` : accès aux backends LLM
- `api_key` : gestion des clés
- `route` : configuration routage
- `config` : rate limits, quotas
- `stats` : métriques et analytics
- `audit_log` : logs d'audit

## CRITÈRES DE SUCCÈS

- [x] Modèle FGA créé avec 9 types de ressources
- [x] Relations hiérarchiques organization → tenant → resources
- [x] OpenFgaAuthorizationService implémenté
- [x] IAuthorizationService abstraction dans projet dédié
- [x] AuthorizationBehavior MediatR
- [x] Middleware protection routes API
- [x] Health check OpenFGA
- [x] Docker Compose avec OpenFGA + PostgreSQL
- [x] Tuples seed pour développement
- [x] Tests unitaires service (33 tests)
- [x] Build : 0 erreurs, 0 warnings
- [x] Documentation XML complète (français)
- [x] ADR créé (055-openfga-authorization-rebac.adr.md)

## LIVRABLES

### Phase 1 : Modèle OpenFGA + Docker
| Fichier | Description |
|---------|-------------|
| `infrastructure/openfga/model.fga` | Modèle d'autorisation ReBAC |
| `infrastructure/openfga/tuples-seed.json` | Données de développement |
| `infrastructure/openfga/docker-compose.openfga.yml` | Docker Compose PostgreSQL + OpenFGA |
| `infrastructure/openfga/init-openfga.sh` | Script d'initialisation |

### Phase 2 : Infrastructure Layer
| Projet | Fichiers |
|--------|----------|
| `LLMProxy.Infrastructure.Authorization.Abstractions` | `AuthorizationRequest.cs`, `AuthorizationResult.cs`, `IAuthorizationService.cs` |
| `LLMProxy.Infrastructure.Authorization` | `OpenFgaConfiguration.cs`, `OpenFgaAuthorizationService.cs`, `OpenFgaHealthCheck.cs`, `ServiceCollectionExtensions.cs` |
| `LLMProxy.Infrastructure.Authorization.Tests` | 3 classes de tests, 33 tests passants |

### Phase 3 : Application Layer
| Fichier | Description |
|---------|-------------|
| `Authorization/RequirePermissionAttribute.cs` | Attribut pour marquer les requêtes MediatR |
| `Authorization/IAuthorizedRequest.cs` | Interface pour requêtes avec UserId/ObjectId |
| `Authorization/UnauthorizedException.cs` | Exception personnalisée |
| `Authorization/AuthorizationBehavior.cs` | Pipeline MediatR pour vérification automatique |

### Phase 4 : Middleware ASP.NET Core
| Fichier | Description |
|---------|-------------|
| `Middleware/OpenFgaAuthorizationMiddleware.cs` | Middleware HTTP pour routes Gateway |
| `Middleware/FgaAuthorizeAttribute.cs` | Attribut pour endpoints |

### Documentation
| Fichier | Description |
|---------|-------------|
| `docs/adr/055-openfga-authorization-rebac.adr.md` | ADR documentant la décision architecturale |

## TRACKING

Début: 2025-01-07T14:30:00Z
Fin: 2025-01-07T18:45:00Z
Durée: 4h15

## VALIDATION FINALE

### Build
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Tests
- Authorization.Tests : 33/33 passed ✅
- Application.Tests : 75/75 passed ✅
- Gateway.Tests : 30/31 passed (1 skipped) ✅
- Security.Tests : 35/35 passed ✅

### Notes
Les échecs dans Domain.Tests (6), Admin.API.Tests (7) et Redis.Tests (4) sont des tests préexistants non liés à cette tâche.

## STATUT : ✅ TERMINÉ
