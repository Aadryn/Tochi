# Tâche 061 - Implémenter OpenFGA Authorization ReBAC

## PRIORITÉ
🟠 **P2 - HAUTE** (Priorité 6/8 de la refonte)

## OBJECTIF

Intégrer OpenFGA pour une autorisation fine ReBAC (Relationship-Based Access Control) hiérarchique sur toutes les ressources : tenants, providers, API keys, routes, configurations.

## CONTEXTE

### Choix Technique
- **Engine** : OpenFGA (open source, CNCF)
- **Modèle** : ReBAC hiérarchique fin
- **SDK** : OpenFga.Sdk .NET

### Ressources à Protéger
- `tenant` : isolation multi-tenant
- `provider` : accès aux backends LLM
- `api_key` : gestion des clés
- `route` : configuration routage
- `config` : rate limits, quotas
- `stats` : métriques et analytics
- `audit_log` : logs d'audit

## IMPLÉMENTATION

### Phase 1 : Modèle OpenFGA
```
infrastructure/openfga/
├── model.fga                 # Modèle d'autorisation
├── tuples-seed.json          # Tuples initiaux
└── docker-compose.openfga.yml
```

```fga
# model.fga - Modèle ReBAC Hiérarchique
model
  schema 1.1

# Types de base
type user

type organization
  relations
    define admin: [user]
    define member: [user] or admin

type tenant
  relations
    define organization: [organization]
    define owner: [user]
    define admin: [user] or owner or admin from organization
    define operator: [user] or admin
    define viewer: [user] or operator
    define can_manage: admin
    define can_operate: operator
    define can_view: viewer

type provider
  relations
    define tenant: [tenant]
    define can_use: can_view from tenant
    define can_configure: can_operate from tenant
    define can_delete: can_manage from tenant

type api_key
  relations
    define tenant: [tenant]
    define owner: [user]
    define can_use: owner or can_view from tenant
    define can_revoke: owner or can_manage from tenant

type route
  relations
    define tenant: [tenant]
    define can_view: can_view from tenant
    define can_modify: can_operate from tenant
    define can_delete: can_manage from tenant

type config
  relations
    define tenant: [tenant]
    define can_view: can_view from tenant
    define can_modify: can_manage from tenant

type stats
  relations
    define tenant: [tenant]
    define can_view: can_view from tenant
    define can_export: can_operate from tenant

type audit_log
  relations
    define tenant: [tenant]
    define can_view: can_manage from tenant
```

### Phase 2 : Infrastructure Layer
```
src/Infrastructure/LLMProxy.Infrastructure.Authorization/
├── LLMProxy.Infrastructure.Authorization.csproj
├── OpenFga/
│   ├── OpenFgaAuthorizationService.cs
│   ├── OpenFgaConfiguration.cs
│   └── OpenFgaHealthCheck.cs
├── Abstractions/
│   ├── IAuthorizationService.cs
│   ├── AuthorizationRequest.cs
│   └── AuthorizationResult.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

### Phase 3 : Application Layer Integration
```
src/Application/LLMProxy.Application/
├── Common/
│   ├── Behaviors/
│   │   └── AuthorizationBehavior.cs   # MediatR pipeline
│   └── Attributes/
│       └── RequirePermissionAttribute.cs
```

```csharp
// Exemple d'utilisation
[RequirePermission("tenant", "can_manage")]
public class UpdateTenantQuotaCommand : IRequest<Result>
{
    public Guid TenantId { get; init; }
    public QuotaConfiguration NewQuota { get; init; }
}
```

### Phase 4 : Middleware ASP.NET Core
```
src/Presentation/LLMProxy.Gateway/
├── Middleware/
│   └── OpenFgaAuthorizationMiddleware.cs
├── Filters/
│   └── FgaAuthorizeAttribute.cs
```

### Phase 5 : Docker Compose
```yaml
# docker-compose.openfga.yml
services:
  openfga:
    image: openfga/openfga:latest
    ports:
      - "8080:8080"   # HTTP
      - "8081:8081"   # gRPC
      - "3000:3000"   # Playground
    command: run
    environment:
      - OPENFGA_DATASTORE_ENGINE=postgres
      - OPENFGA_DATASTORE_URI=postgres://postgres:password@postgres:5432/openfga
    depends_on:
      - postgres
```

## CRITÈRES DE SUCCÈS

- [ ] Modèle FGA créé avec 7 types de ressources
- [ ] Relations hiérarchiques organization → tenant → resources
- [ ] OpenFgaAuthorizationService implémenté
- [ ] IAuthorizationService abstraction Domain
- [ ] AuthorizationBehavior MediatR
- [ ] Middleware protection routes API
- [ ] Health check OpenFGA
- [ ] Docker Compose avec OpenFGA + PostgreSQL
- [ ] Tuples seed pour développement
- [ ] Tests unitaires service
- [ ] Tests d'intégration avec OpenFGA
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Documentation XML complète (français)

## DÉPENDANCES

- Docker Desktop
- Tâche 059 (Vertical Slices) pour MediatR behaviors

## ESTIMATION

**Effort** : 12h
**Complexité** : Haute

## RÉFÉRENCES

- [OpenFGA Documentation](https://openfga.dev/docs)
- [OpenFGA .NET SDK](https://github.com/openfga/dotnet-sdk)
- [Zanzibar Paper (Google)](https://research.google/pubs/pub48190/)
