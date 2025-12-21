# Tâche 006 - Séparation Complète ADR-001 : Un Seul Type Par Fichier

## OBJECTIF

Séparer les 29 fichiers C# violant ADR-001 pour atteindre 100% conformité.

## JUSTIFICATION

Audit ADR-001 a révélé 29 violations (conformité actuelle : 46.5%).
ADR-001 exige strictement UN SEUL type (class/interface/enum/struct/record) par fichier.

## PÉRIMÈTRE

### Violations Critiques (7 types)
- src/Application/LLMProxy.Application/LLMProviders/Commands/ProviderCommands.cs
- src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/UnitOfWork.cs  
- src/Application/LLMProxy.Application/ApiKeys/Commands/ApiKeyCommands.cs

### Violations Importantes (6 types)
- src/Application/LLMProxy.Application/Common/CQRS.cs

### Violations Majeures (5 types)
- src/Core/LLMProxy.Domain/Entities/LLMProvider.cs
- src/Application/LLMProxy.Application/Users/Commands/UserCommands.cs
- src/Application/LLMProxy.Application/Common/Dtos.cs

### Violations Standard (4 types)
- src/Core/LLMProxy.Domain/Entities/QuotaLimit.cs
- src/Application/LLMProxy.Application/Users/Queries/GetUserQueries.cs
- src/Application/LLMProxy.Application/LLMProviders/Queries/GetProviderQueries.cs
- src/Application/LLMProxy.Application/ApiKeys/Queries/GetApiKeyQueries.cs
- src/Core/LLMProxy.Domain/Entities/Tenant.cs

### Violations Mineures (3 types)
- src/Presentation/LLMProxy.Gateway/Constants/HttpConstants.cs
- src/Core/LLMProxy.Domain/Entities/AuditLog.cs
- src/Presentation/LLMProxy.Gateway/Middleware/GlobalExceptionHandlerMiddleware.cs
- src/Application/LLMProxy.Application/Tenants/Commands/CreateTenantCommand.cs
- src/Application/LLMProxy.Application/Tenants/Commands/UpdateTenantSettingsCommand.cs
- src/Application/LLMProxy.Application/Tenants/Queries/GetTenantByIdQuery.cs
- src/Application/LLMProxy.Application/Users/Commands/CreateUserCommand.cs

### Violations Légères (2 types)
- src/Infrastructure/LLMProxy.Infrastructure.Redis/QuotaService.cs
- src/Application/LLMProxy.Application/Tenants/Commands/ActivateTenantCommand.cs
- src/Infrastructure/LLMProxy.Infrastructure.Security/SecretService.cs
- src/Core/LLMProxy.Domain/Entities/ApiKey.cs
- src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/LLMProxyDbContext.cs
- src/Application/LLMProxy.Application/Tenants/Queries/GetAllTenantsQuery.cs
- src/Core/LLMProxy.Domain/Common/Result.cs
- src/Application/LLMProxy.Application/Common/BaseDto.cs
- src/Core/LLMProxy.Domain/Entities/User.cs
- src/Application/LLMProxy.Application/Tenants/Commands/DeactivateTenantCommand.cs

## STRATÉGIE

1. Créer feature branch `feature/006--complete-adr-001-separation`
2. Traiter par ordre décroissant (7 types → 2 types)
3. Commits atomiques après chaque fichier séparé
4. Build + Tests après chaque commit
5. Validation finale : script ADR-001 doit afficher 100% conformité

## CRITÈRES DE SUCCÈS

- [ ] 29 fichiers séparés en fichiers individuels (1 type/fichier)
- [ ] Build réussi : 0 errors, 0 warnings critiques
- [ ] Tests : 100% passing (ou 99%+ si skip justifié)
- [ ] Script ADR-001 : 100% conformité (114/114 fichiers conformes)
- [ ] Commits atomiques (1 commit par fichier séparé minimum)
- [ ] Documentation XML préservée
- [ ] Namespaces cohérents

## DÉPENDANCES

Aucune

## NOTES

- Certains fichiers ont déjà été partiellement séparés (ex: ApiKeyAuthenticator)
- Attention aux using statements à ajouter dans les nouveaux fichiers
- Vérifier les tests unitaires après séparation (imports potentiellement cassés)


## TRACKING
Début: 2025-12-21T21:22:02.9632262Z

