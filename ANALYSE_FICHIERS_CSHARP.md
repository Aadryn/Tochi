# ANALYSE COMPLÈTE DES FICHIERS C# - LLMProxy

Date: 2025-12-21 11:43:57

---

## TABLEAU DÉTAILLÉ

| Fichier | Lignes | Nb Types | Types Définis | Namespace |
|---------|--------|----------|---------------|-----------|
| src\Application\LLMProxy.Application\ApiKeys\Commands\ApiKeyCommands.cs | 116 | 7 | record CreateApiKeyCommand, class CreateApiKeyCommandValidator, class CreateApiKeyCommandHandler, record RevokeApiKeyCommand, class RevokeApiKeyCommandHandler, record DeleteApiKeyCommand, class DeleteApiKeyCommandHandler | LLMProxy.Application.ApiKeys.Commands |
| src\Application\LLMProxy.Application\ApiKeys\Queries\GetApiKeyQueries.cs | 66 | 4 | record GetApiKeysByUserIdQuery, class GetApiKeysByUserIdQueryHandler, record GetApiKeysByTenantIdQuery, class GetApiKeysByTenantIdQueryHandler | LLMProxy.Application.ApiKeys.Queries |
| src\Application\LLMProxy.Application\Common\BaseDto.cs | 33 | 1 | class PagedResult | LLMProxy.Application.Common |
| src\Application\LLMProxy.Application\Common\CQRS.cs | 42 | 6 | interface ICommand, interface ICommand, interface IQuery, interface ICommandHandler, interface ICommandHandler, interface IQueryHandler | LLMProxy.Application.Common |
| src\Application\LLMProxy.Application\Common\Dtos.cs | 56 | 5 | record TenantDto, record TenantSettingsDto, record UserDto, record ApiKeyDto, record LLMProviderDto | LLMProxy.Application.Common |
| src\Application\LLMProxy.Application\LLMProviders\Commands\ProviderCommands.cs | 164 | 7 | record CreateProviderCommand, class CreateProviderCommandValidator, class CreateProviderCommandHandler, record UpdateProviderCommand, class UpdateProviderCommandHandler, record DeleteProviderCommand, class DeleteProviderCommandHandler | LLMProxy.Application.LLMProviders.Commands |
| src\Application\LLMProxy.Application\LLMProviders\Queries\GetProviderQueries.cs | 70 | 4 | record GetProviderByIdQuery, class GetProviderByIdQueryHandler, record GetProvidersByTenantIdQuery, class GetProvidersByTenantIdQueryHandler | LLMProxy.Application.LLMProviders.Queries |
| src\Application\LLMProxy.Application\Tenants\Commands\ActivateTenantCommand.cs | 29 | 2 | record ActivateTenantCommand, class ActivateTenantCommandHandler | LLMProxy.Application.Tenants.Commands |
| src\Application\LLMProxy.Application\Tenants\Commands\CreateTenantCommand.cs | 99 | 3 | record CreateTenantCommand, class CreateTenantCommandValidator, class CreateTenantCommandHandler | LLMProxy.Application.Tenants.Commands |
| src\Application\LLMProxy.Application\Tenants\Commands\DeactivateTenantCommand.cs | 29 | 2 | record DeactivateTenantCommand, class DeactivateTenantCommandHandler | LLMProxy.Application.Tenants.Commands |
| src\Application\LLMProxy.Application\Tenants\Commands\UpdateTenantSettingsCommand.cs | 73 | 3 | record UpdateTenantSettingsCommand, class UpdateTenantSettingsCommandValidator, class UpdateTenantSettingsCommandHandler | LLMProxy.Application.Tenants.Commands |
| src\Application\LLMProxy.Application\Tenants\Queries\GetAllTenantsQuery.cs | 37 | 2 | record GetAllTenantsQuery, class GetAllTenantsQueryHandler | LLMProxy.Application.Tenants.Queries |
| src\Application\LLMProxy.Application\Tenants\Queries\GetTenantByIdQuery.cs | 67 | 3 | record GetTenantByIdQuery, class GetTenantByIdQueryValidator, class GetTenantByIdQueryHandler | LLMProxy.Application.Tenants.Queries |
| src\Application\LLMProxy.Application\Users\Commands\CreateUserCommand.cs | 95 | 3 | record CreateUserCommand, class CreateUserCommandValidator, class CreateUserCommandHandler | LLMProxy.Application.Users.Commands |
| src\Application\LLMProxy.Application\Users\Commands\UserCommands.cs | 83 | 5 | record UpdateUserCommand, class UpdateUserCommandValidator, class UpdateUserCommandHandler, record DeleteUserCommand, class DeleteUserCommandHandler | LLMProxy.Application.Users.Commands |
| src\Application\LLMProxy.Application\Users\Queries\GetUserQueries.cs | 66 | 4 | record GetUserByIdQuery, class GetUserByIdQueryHandler, record GetUsersByTenantIdQuery, class GetUsersByTenantIdQueryHandler | LLMProxy.Application.Users.Queries |
| src\Core\LLMProxy.Domain\Common\Entity.cs | 52 | 0 |  | LLMProxy.Domain.Common |
| src\Core\LLMProxy.Domain\Common\IDomainEvent.cs | 8 | 1 | interface IDomainEvent | LLMProxy.Domain.Common |
| src\Core\LLMProxy.Domain\Common\Result.cs | 35 | 2 | class Result, class Result | LLMProxy.Domain.Common |
| src\Core\LLMProxy.Domain\Common\ValueObject.cs | 33 | 0 |  | LLMProxy.Domain.Common |
| src\Core\LLMProxy.Domain\Entities\ApiKey.cs | 119 | 2 | class ApiKey, record ApiKeyCreatedEvent | LLMProxy.Domain.Entities |
| src\Core\LLMProxy.Domain\Entities\AuditLog.cs | 216 | 3 | class AuditLog, class TokenUsageMetric, enum MetricPeriod | LLMProxy.Domain.Entities |
| src\Core\LLMProxy.Domain\Entities\LLMProvider.cs | 274 | 5 | class LLMProvider, enum ProviderType, class ProviderConfiguration, class RoutingStrategy, enum RoutingMethod | LLMProxy.Domain.Entities |
| src\Core\LLMProxy.Domain\Entities\QuotaLimit.cs | 98 | 4 | class QuotaLimit, enum QuotaType, enum QuotaPeriod, class QuotaUsage | LLMProxy.Domain.Entities |
| src\Core\LLMProxy.Domain\Entities\Tenant.cs | 133 | 4 | class Tenant, class TenantSettings, record TenantCreatedEvent, record TenantDeactivatedEvent | LLMProxy.Domain.Entities |
| src\Core\LLMProxy.Domain\Entities\User.cs | 134 | 2 | class User, enum UserRole | LLMProxy.Domain.Entities |
| src\Core\LLMProxy.Domain\Interfaces\IRepositories.cs | 110 | 8 | interface ITenantRepository, interface IUserRepository, interface IApiKeyRepository, interface ILLMProviderRepository, interface IQuotaLimitRepository, interface IAuditLogRepository, interface ITokenUsageMetricRepository, interface IUnitOfWork | LLMProxy.Domain.Interfaces |
| src\Core\LLMProxy.Domain\Interfaces\IServices.cs | 97 | 5 | interface IQuotaService, class QuotaCheckResult, interface ICacheService, interface ISecretService, interface ITokenCounterService | LLMProxy.Domain.Interfaces |
| src\Infrastructure\LLMProxy.Infrastructure.LLMProviders\ServiceCollectionExtensions.cs | 11 | 0 |  | LLMProxy.Infrastructure.LLMProviders |
| src\Infrastructure\LLMProxy.Infrastructure.LLMProviders\TokenCounterService.cs | 188 | 1 | class TokenCounterService | LLMProxy.Infrastructure.LLMProviders |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Configurations\ApiKeyConfiguration.cs | 62 | 1 | class ApiKeyConfiguration | LLMProxy.Infrastructure.PostgreSQL.Configurations |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Configurations\AuditLogConfiguration.cs | 100 | 1 | class AuditLogConfiguration | LLMProxy.Infrastructure.PostgreSQL.Configurations |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Configurations\LLMProviderConfiguration.cs | 104 | 1 | class LLMProviderConfiguration | LLMProxy.Infrastructure.PostgreSQL.Configurations |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Configurations\QuotaLimitConfiguration.cs | 51 | 1 | class QuotaLimitConfiguration | LLMProxy.Infrastructure.PostgreSQL.Configurations |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Configurations\TenantConfiguration.cs | 68 | 1 | class TenantConfiguration | LLMProxy.Infrastructure.PostgreSQL.Configurations |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Configurations\TokenUsageMetricConfiguration.cs | 78 | 1 | class TokenUsageMetricConfiguration | LLMProxy.Infrastructure.PostgreSQL.Configurations |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Configurations\UserConfiguration.cs | 59 | 1 | class UserConfiguration | LLMProxy.Infrastructure.PostgreSQL.Configurations |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\LLMProxyDbContext.cs | 73 | 1 | class LLMProxyDbContext | LLMProxy.Infrastructure.PostgreSQL |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Migrations\20251221031424_InitialCreate.cs | 366 | 1 | class InitialCreate | LLMProxy.Infrastructure.PostgreSQL.Migrations |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Migrations\20251221031424_InitialCreate.Designer.cs | 568 | 1 | class InitialCreate | LLMProxy.Infrastructure.PostgreSQL.Migrations |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Migrations\LLMProxyDbContextModelSnapshot.cs | 565 | 1 | class LLMProxyDbContextModelSnapshot | LLMProxy.Infrastructure.PostgreSQL.Migrations |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Repositories\TenantRepository.cs | 61 | 1 | class TenantRepository | LLMProxy.Infrastructure.PostgreSQL.Repositories |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\UnitOfWork.cs | 328 | 7 | class UnitOfWork, class UserRepository, class ApiKeyRepository, class LLMProviderRepository, class QuotaLimitRepository, class AuditLogRepository, class TokenUsageMetricRepository | LLMProxy.Infrastructure.PostgreSQL |
| src\Infrastructure\LLMProxy.Infrastructure.Redis\CacheService.cs | 136 | 1 | class CacheService | LLMProxy.Infrastructure.Redis |
| src\Infrastructure\LLMProxy.Infrastructure.Redis\QuotaService.cs | 236 | 2 | class QuotaService, class QuotaLimitCache | LLMProxy.Infrastructure.Redis |
| src\Infrastructure\LLMProxy.Infrastructure.Redis\ServiceCollectionExtensions.cs | 28 | 0 |  | LLMProxy.Infrastructure.Redis |
| src\Infrastructure\LLMProxy.Infrastructure.Security\ApiKeyAuthenticator.cs | 136 | 3 | class ApiKeyAuthenticationResult, interface IApiKeyAuthenticator, class ApiKeyAuthenticator | LLMProxy.Infrastructure.Security |
| src\Infrastructure\LLMProxy.Infrastructure.Security\ApiKeyExtractor.cs | 58 | 2 | interface IApiKeyExtractor, class HeaderApiKeyExtractor | LLMProxy.Infrastructure.Security |
| src\Infrastructure\LLMProxy.Infrastructure.Security\ApiKeyValidator.cs | 88 | 3 | class ApiKeyValidationResult, interface IApiKeyValidator, class ApiKeyValidator | LLMProxy.Infrastructure.Security |
| src\Infrastructure\LLMProxy.Infrastructure.Security\Guard.cs | 225 | 0 |  | LLMProxy.Infrastructure.Security |
| src\Infrastructure\LLMProxy.Infrastructure.Security\HashService.cs | 37 | 2 | interface IHashService, class Sha256HashService | LLMProxy.Infrastructure.Security |
| src\Infrastructure\LLMProxy.Infrastructure.Security\SecretService.cs | 241 | 2 | class SecretService, enum SecretProviderType | LLMProxy.Infrastructure.Security |
| src\Infrastructure\LLMProxy.Infrastructure.Security\ServiceCollectionExtensions.cs | 12 | 0 |  | LLMProxy.Infrastructure.Security |
| src\Presentation\LLMProxy.Admin.API\Controllers\ApiKeysController.cs | 88 | 1 | class ApiKeysController | LLMProxy.Admin.API.Controllers |
| src\Presentation\LLMProxy.Admin.API\Controllers\ProvidersController.cs | 94 | 1 | class ProvidersController | LLMProxy.Admin.API.Controllers |
| src\Presentation\LLMProxy.Admin.API\Controllers\TenantsController.cs | 110 | 1 | class TenantsController | LLMProxy.Admin.API.Controllers |
| src\Presentation\LLMProxy.Admin.API\Controllers\UsersController.cs | 94 | 1 | class UsersController | LLMProxy.Admin.API.Controllers |
| src\Presentation\LLMProxy.Admin.API\Program.cs | 116 | 0 |  | N/A |
| src\Presentation\LLMProxy.Gateway\Constants\HttpConstants.cs | 39 | 0 |  | LLMProxy.Gateway.Constants |
| src\Presentation\LLMProxy.Gateway\Middleware\ApiKeyAuthenticationMiddleware.cs | 76 | 1 | class ApiKeyAuthenticationMiddleware | LLMProxy.Gateway.Middleware |
| src\Presentation\LLMProxy.Gateway\Middleware\GlobalExceptionHandlerMiddleware.cs | 136 | 3 | class GlobalExceptionHandlerMiddleware, class ErrorResponse, class ErrorDetail | LLMProxy.Gateway.Middleware |
| src\Presentation\LLMProxy.Gateway\Middleware\QuotaEnforcementMiddleware.cs | 87 | 1 | class QuotaEnforcementMiddleware | LLMProxy.Gateway.Middleware |
| src\Presentation\LLMProxy.Gateway\Middleware\RequestLoggingMiddleware.cs | 65 | 1 | class RequestLoggingMiddleware | LLMProxy.Gateway.Middleware |
| src\Presentation\LLMProxy.Gateway\Middleware\StreamInterceptionMiddleware.cs | 247 | 1 | class StreamInterceptionMiddleware | LLMProxy.Gateway.Middleware |
| src\Presentation\LLMProxy.Gateway\Program.cs | 100 | 0 |  | N/A |

---

## STATISTIQUES GLOBALES

### Vue d'Ensemble
- **Total fichiers** : 65 fichiers C#
- **Total lignes de code** : 7565 lignes
- **Moyenne lignes/fichier** : 116 lignes
- **Fichiers avec types multiples** : 32 fichiers (49.2%)

### Distribution par Couche

| Couche | Fichiers | Lignes | % Total Lignes | Moyenne Lignes/Fichier |
|--------|----------|--------|----------------|------------------------|
| **Core (Domain)** | 12 | 1309 | 17.3% | 109 |
| **Infrastructure** | 25 | 3879 | 51.3% | 155 |
| **Presentation** | 12 | 1252 | 16.5% | 104 |
| **Application** | 16 | 1125 | 14.9% | 70 |

### Analyse ADR-001 (Un seul type par fichier)

**Fichiers avec MULTIPLE types (violation potentielle) :**

| Fichier | Nb Types | Types Définis |
|---------|----------|---------------|
| src\Core\LLMProxy.Domain\Interfaces\IRepositories.cs | 8 | interface ITenantRepository, interface IUserRepository, interface IApiKeyRepository, interface ILLMProviderRepository, interface IQuotaLimitRepository, interface IAuditLogRepository, interface ITokenUsageMetricRepository, interface IUnitOfWork |
| src\Application\LLMProxy.Application\LLMProviders\Commands\ProviderCommands.cs | 7 | record CreateProviderCommand, class CreateProviderCommandValidator, class CreateProviderCommandHandler, record UpdateProviderCommand, class UpdateProviderCommandHandler, record DeleteProviderCommand, class DeleteProviderCommandHandler |
| src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\UnitOfWork.cs | 7 | class UnitOfWork, class UserRepository, class ApiKeyRepository, class LLMProviderRepository, class QuotaLimitRepository, class AuditLogRepository, class TokenUsageMetricRepository |
| src\Application\LLMProxy.Application\ApiKeys\Commands\ApiKeyCommands.cs | 7 | record CreateApiKeyCommand, class CreateApiKeyCommandValidator, class CreateApiKeyCommandHandler, record RevokeApiKeyCommand, class RevokeApiKeyCommandHandler, record DeleteApiKeyCommand, class DeleteApiKeyCommandHandler |
| src\Application\LLMProxy.Application\Common\CQRS.cs | 6 | interface ICommand, interface ICommand, interface IQuery, interface ICommandHandler, interface ICommandHandler, interface IQueryHandler |
| src\Core\LLMProxy.Domain\Interfaces\IServices.cs | 5 | interface IQuotaService, class QuotaCheckResult, interface ICacheService, interface ISecretService, interface ITokenCounterService |
| src\Core\LLMProxy.Domain\Entities\LLMProvider.cs | 5 | class LLMProvider, enum ProviderType, class ProviderConfiguration, class RoutingStrategy, enum RoutingMethod |
| src\Application\LLMProxy.Application\Users\Commands\UserCommands.cs | 5 | record UpdateUserCommand, class UpdateUserCommandValidator, class UpdateUserCommandHandler, record DeleteUserCommand, class DeleteUserCommandHandler |
| src\Application\LLMProxy.Application\Common\Dtos.cs | 5 | record TenantDto, record TenantSettingsDto, record UserDto, record ApiKeyDto, record LLMProviderDto |
| src\Core\LLMProxy.Domain\Entities\QuotaLimit.cs | 4 | class QuotaLimit, enum QuotaType, enum QuotaPeriod, class QuotaUsage |
| src\Application\LLMProxy.Application\LLMProviders\Queries\GetProviderQueries.cs | 4 | record GetProviderByIdQuery, class GetProviderByIdQueryHandler, record GetProvidersByTenantIdQuery, class GetProvidersByTenantIdQueryHandler |
| src\Application\LLMProxy.Application\Users\Queries\GetUserQueries.cs | 4 | record GetUserByIdQuery, class GetUserByIdQueryHandler, record GetUsersByTenantIdQuery, class GetUsersByTenantIdQueryHandler |
| src\Application\LLMProxy.Application\ApiKeys\Queries\GetApiKeyQueries.cs | 4 | record GetApiKeysByUserIdQuery, class GetApiKeysByUserIdQueryHandler, record GetApiKeysByTenantIdQuery, class GetApiKeysByTenantIdQueryHandler |
| src\Core\LLMProxy.Domain\Entities\Tenant.cs | 4 | class Tenant, class TenantSettings, record TenantCreatedEvent, record TenantDeactivatedEvent |
| src\Core\LLMProxy.Domain\Entities\AuditLog.cs | 3 | class AuditLog, class TokenUsageMetric, enum MetricPeriod |
| src\Infrastructure\LLMProxy.Infrastructure.Security\ApiKeyValidator.cs | 3 | class ApiKeyValidationResult, interface IApiKeyValidator, class ApiKeyValidator |
| src\Infrastructure\LLMProxy.Infrastructure.Security\ApiKeyAuthenticator.cs | 3 | class ApiKeyAuthenticationResult, interface IApiKeyAuthenticator, class ApiKeyAuthenticator |
| src\Application\LLMProxy.Application\Tenants\Queries\GetTenantByIdQuery.cs | 3 | record GetTenantByIdQuery, class GetTenantByIdQueryValidator, class GetTenantByIdQueryHandler |
| src\Application\LLMProxy.Application\Tenants\Commands\UpdateTenantSettingsCommand.cs | 3 | record UpdateTenantSettingsCommand, class UpdateTenantSettingsCommandValidator, class UpdateTenantSettingsCommandHandler |
| src\Application\LLMProxy.Application\Users\Commands\CreateUserCommand.cs | 3 | record CreateUserCommand, class CreateUserCommandValidator, class CreateUserCommandHandler |
| src\Application\LLMProxy.Application\Tenants\Commands\CreateTenantCommand.cs | 3 | record CreateTenantCommand, class CreateTenantCommandValidator, class CreateTenantCommandHandler |
| src\Presentation\LLMProxy.Gateway\Middleware\GlobalExceptionHandlerMiddleware.cs | 3 | class GlobalExceptionHandlerMiddleware, class ErrorResponse, class ErrorDetail |
| src\Core\LLMProxy.Domain\Common\Result.cs | 2 | class Result, class Result |
| src\Infrastructure\LLMProxy.Infrastructure.Security\ApiKeyExtractor.cs | 2 | interface IApiKeyExtractor, class HeaderApiKeyExtractor |
| src\Application\LLMProxy.Application\Tenants\Commands\ActivateTenantCommand.cs | 2 | record ActivateTenantCommand, class ActivateTenantCommandHandler |
| src\Infrastructure\LLMProxy.Infrastructure.Security\SecretService.cs | 2 | class SecretService, enum SecretProviderType |
| src\Infrastructure\LLMProxy.Infrastructure.Security\HashService.cs | 2 | interface IHashService, class Sha256HashService |
| src\Core\LLMProxy.Domain\Entities\User.cs | 2 | class User, enum UserRole |
| src\Core\LLMProxy.Domain\Entities\ApiKey.cs | 2 | class ApiKey, record ApiKeyCreatedEvent |
| src\Application\LLMProxy.Application\Tenants\Queries\GetAllTenantsQuery.cs | 2 | record GetAllTenantsQuery, class GetAllTenantsQueryHandler |
| src\Infrastructure\LLMProxy.Infrastructure.Redis\QuotaService.cs | 2 | class QuotaService, class QuotaLimitCache |
| src\Application\LLMProxy.Application\Tenants\Commands\DeactivateTenantCommand.cs | 2 | record DeactivateTenantCommand, class DeactivateTenantCommandHandler |

**Total violations potentielles** : 32 fichiers

**Note** : Certains fichiers peuvent contenir types intimement liés (ex: Interface + Implémentation + Result),
ce qui est acceptable selon ADR-001 si la cohésion est forte et la séparation nuirait à la lisibilité.
---

## TOP 10 FICHIERS LES PLUS LONGS

| Rang | Fichier | Lignes | Types |
|------|---------|--------|-------|
| 1 | src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Migrations\20251221031424_InitialCreate.Designer.cs | 568 | class InitialCreate |
| 2 | src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Migrations\LLMProxyDbContextModelSnapshot.cs | 565 | class LLMProxyDbContextModelSnapshot |
| 3 | src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\Migrations\20251221031424_InitialCreate.cs | 366 | class InitialCreate |
| 4 | src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL\UnitOfWork.cs | 328 | class UnitOfWork, class UserRepository, class ApiKeyRepository, class LLMProviderRepository, class QuotaLimitRepository, class AuditLogRepository, class TokenUsageMetricRepository |
| 5 | src\Core\LLMProxy.Domain\Entities\LLMProvider.cs | 274 | class LLMProvider, enum ProviderType, class ProviderConfiguration, class RoutingStrategy, enum RoutingMethod |
| 6 | src\Presentation\LLMProxy.Gateway\Middleware\StreamInterceptionMiddleware.cs | 247 | class StreamInterceptionMiddleware |
| 7 | src\Infrastructure\LLMProxy.Infrastructure.Security\SecretService.cs | 241 | class SecretService, enum SecretProviderType |
| 8 | src\Infrastructure\LLMProxy.Infrastructure.Redis\QuotaService.cs | 236 | class QuotaService, class QuotaLimitCache |
| 9 | src\Infrastructure\LLMProxy.Infrastructure.Security\Guard.cs | 225 |  |
| 10 | src\Core\LLMProxy.Domain\Entities\AuditLog.cs | 216 | class AuditLog, class TokenUsageMetric, enum MetricPeriod |

---

## DISTRIBUTION PAR NAMESPACE

| Namespace | Fichiers | Total Lignes |
|-----------|----------|--------------|
| LLMProxy.Infrastructure.PostgreSQL.Configurations | 7 | 522 |
| LLMProxy.Infrastructure.Security | 7 | 797 |
| LLMProxy.Domain.Entities | 6 | 974 |
| LLMProxy.Gateway.Middleware | 5 | 611 |
| LLMProxy.Domain.Common | 4 | 128 |
| LLMProxy.Application.Tenants.Commands | 4 | 230 |
| LLMProxy.Admin.API.Controllers | 4 | 386 |
| LLMProxy.Application.Common | 3 | 131 |
| LLMProxy.Infrastructure.Redis | 3 | 400 |
| LLMProxy.Infrastructure.PostgreSQL.Migrations | 3 | 1499 |
| LLMProxy.Infrastructure.PostgreSQL | 2 | 401 |
| N/A | 2 | 216 |
| LLMProxy.Infrastructure.LLMProviders | 2 | 199 |
| LLMProxy.Application.Users.Commands | 2 | 178 |
| LLMProxy.Application.Tenants.Queries | 2 | 104 |
| LLMProxy.Domain.Interfaces | 2 | 207 |
| LLMProxy.Application.ApiKeys.Queries | 1 | 66 |
| LLMProxy.Application.ApiKeys.Commands | 1 | 116 |
| LLMProxy.Gateway.Constants | 1 | 39 |
| LLMProxy.Application.LLMProviders.Commands | 1 | 164 |
| LLMProxy.Application.Users.Queries | 1 | 66 |
| LLMProxy.Infrastructure.PostgreSQL.Repositories | 1 | 61 |
| LLMProxy.Application.LLMProviders.Queries | 1 | 70 |

---

## CONCLUSION

Cette analyse exhaustive couvre **65 fichiers C#** représentant **7565 lignes de code**.

**Points clés** :
- Architecture en couches respectée (Core/Infrastructure/Presentation/Application)
- Fichiers avec multiples types : 32 (49.2%)
- Moyenne de 116 lignes par fichier (maintenabilité)

