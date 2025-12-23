# Tâche 059 - Refactorer vers Vertical Slice Architecture

## PRIORITÉ
🟠 **P2 - HAUTE** (Priorité 4/8 de la refonte)

## OBJECTIF

Réorganiser le code Application layer vers une architecture Vertical Slice, où chaque feature est autonome avec sa commande/query, handler, et validateur dans un même dossier.

## CONTEXTE

### État Actuel
- Clean Architecture classique (Domain/Application/Infrastructure/Presentation)
- Application layer organisée par type technique (Services/, Interfaces/, DTOs/)
- Couplage horizontal entre services

### État Cible
- Organisation par feature (slice vertical)
- Chaque slice contient : Command/Query, Handler, Validator, DTOs
- MediatR pour CQRS (si pas déjà utilisé)
- Découplage maximal entre features

## IMPLÉMENTATION

### Phase 1 : Structure Cible
```
src/Application/LLMProxy.Application/
├── Features/
│   ├── Routing/
│   │   ├── Commands/
│   │   │   ├── CreateRoute/
│   │   │   │   ├── CreateRouteCommand.cs
│   │   │   │   ├── CreateRouteHandler.cs
│   │   │   │   └── CreateRouteValidator.cs
│   │   │   └── UpdateRoute/
│   │   │       └── ...
│   │   └── Queries/
│   │       ├── GetRouteById/
│   │       │   ├── GetRouteByIdQuery.cs
│   │       │   ├── GetRouteByIdHandler.cs
│   │       │   └── RouteDto.cs
│   │       └── ListRoutes/
│   │           └── ...
│   ├── Providers/
│   │   ├── Commands/
│   │   │   ├── RegisterProvider/
│   │   │   ├── UpdateProviderConfig/
│   │   │   └── DeactivateProvider/
│   │   └── Queries/
│   │       ├── GetProviderHealth/
│   │       └── ListAvailableProviders/
│   ├── Tenants/
│   │   ├── Commands/
│   │   │   ├── CreateTenant/
│   │   │   ├── UpdateQuota/
│   │   │   └── SuspendTenant/
│   │   └── Queries/
│   │       ├── GetTenantStats/
│   │       └── GetTenantConfiguration/
│   ├── ApiKeys/
│   │   ├── Commands/
│   │   │   ├── GenerateApiKey/
│   │   │   ├── RevokeApiKey/
│   │   │   └── RotateApiKey/
│   │   └── Queries/
│   │       └── ListApiKeys/
│   ├── RateLimiting/
│   │   ├── Commands/
│   │   │   └── UpdateRateLimits/
│   │   └── Queries/
│   │       └── GetCurrentUsage/
│   └── Statistics/
│       └── Queries/
│           ├── GetUsageMetrics/
│           ├── GetCostBreakdown/
│           └── GetProviderPerformance/
├── Common/
│   ├── Behaviors/
│   │   ├── LoggingBehavior.cs
│   │   ├── ValidationBehavior.cs
│   │   ├── TransactionBehavior.cs
│   │   └── PerformanceBehavior.cs
│   ├── Exceptions/
│   │   └── ValidationException.cs
│   └── Interfaces/
│       └── ICurrentTenantService.cs
```

### Phase 2 : MediatR Configuration
```csharp
// Si pas déjà installé
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />

// Registration
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
    cfg.AddBehavior<LoggingBehavior<,>>();
    cfg.AddBehavior<ValidationBehavior<,>>();
    cfg.AddBehavior<TransactionBehavior<,>>();
});
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
```

### Phase 3 : Migration Progressive
1. Créer la nouvelle structure Features/
2. Migrer un slice pilote (ex: Routing)
3. Valider le pattern avec tests
4. Migrer les autres slices progressivement
5. Supprimer l'ancienne structure Services/

## CRITÈRES DE SUCCÈS

- [ ] Structure Features/ créée avec 6+ slices
- [ ] MediatR configuré avec behaviors
- [ ] FluentValidation intégré
- [ ] Au moins 3 slices migrés (Routing, Tenants, ApiKeys)
- [ ] Anciens services refactorés ou supprimés
- [ ] Tests unitaires par slice
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Documentation XML complète (français)

## DÉPENDANCES

- Tâche 056 (YARP Dynamic Config) pour slice Routing

## ESTIMATION

**Effort** : 12h (migration progressive)
**Complexité** : Haute (refactoring majeur)

## RÉFÉRENCES

- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
- ADR-007 (Vertical Slice Architecture)
- ADR-013 (CQRS)
