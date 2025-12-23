---
mode: 'agent'
model: Claude Sonnet 4.5
description: 'Perform a comprehensive, methodical and pragmatic refactoring of the solution'
---

# Objectif Principal
Réaliser un refactoring complet, méthodique et pragmatique de la codebase pour améliorer la qualité, la maintenabilité, la performance et la modularité de la solution, tout en garantissant la non-régression par des tests robustes.

# Méthodologie de Refactoring
## Phase 0 : Préparation
Lit l'ensemble des ADRs présents dans le dossier documentations\technicals\adr\**\*.adr.md pour comprendre les décisions architecturales passées et le contexte.

## Phase 1 : Analyse Approfondie (Code Smells & Anti-patterns)
Effectuer une analyse exhaustive de la codebase pour identifier :

### 1.1 Code Smells
- **Duplication** : Code dupliqué entre projets (WebApi/WebApp)
- **Méthodes trop longues** : Méthodes > 20 lignes, complexité > 10
- **Classes trop larges** : Classes > 300 lignes, responsabilités multiples
- **Couplage fort** : Dépendances concrètes au lieu d'abstractions
- **Cohésion faible** : Classes avec méthodes non liées

### 1.2 Violations SOLID
- **Single Responsibility** : Controllers/Services faisant plusieurs choses
- **Open/Closed** : Code modifié au lieu d'étendu
- **Liskov Substitution** : Hiérarchies d'héritage cassées
- **Interface Segregation** : Interfaces trop larges
- **Dependency Inversion** : Dépendance sur implémentations concrètes

### 1.3 Anti-patterns Spécifiques
- **God Objects** : Classes omniscientes (ex: UserService faisant tout)
- **Spaghetti Code** : Logique entremêlée
- **Magic Numbers/Strings** : Constantes hardcodées
- **Inappropriate Intimacy** : Accès excessif aux détails internes
- **Anemic Domain Model** : Entités sans comportement
- **Service Locator** : Anti-pattern DI
- **Repository Anti-patterns** : Generic Repository trop abstrait, IQueryable leakage

### 1.4 Dépendances Cycliques
- Entre projets (ADP.Application ↔ ADP.Infrastructure)
- Entre namespaces (Services ↔ Repositories)
- Entre classes (Services mutuellement dépendants)
- Analyser avec NetArchTest.Rules

### 1.5 Code Mort
- Méthodes/classes non utilisées (analyser avec Roslyn Analyzers)
- Variables non lues
- Paramètres inutilisés
- Using statements superflus
- Projets/fichiers obsolètes

### 1.6 Complexité Cyclomatique
- Identifier méthodes avec complexité > 10
- Analyser avec Code Metrics (Visual Studio)
- Refactorer en méthodes plus petites
- Appliquer Strategy Pattern si nécessaire

### 1.7 Dette Technique
- **TODOs actifs** : Lister et prioriser
- **FIXMEs** : Traiter immédiatement
- **Commentaires obsolètes** : Supprimer ou mettre à jour
- **Code commenté** : Supprimer (utiliser Git history)
- **Dépendances obsolètes** : Mettre à jour NuGet packages

### 1.8 Performance Issues
- **Requêtes N+1** : Utiliser Include/projections
- **Allocations excessives** : Profiler avec dotMemory
- **Manque de cache** : Implémenter IMemoryCache/IDistributedCache
- **Async/Await mal utilisé** : Éviter .Result, Task.Wait()
- **String concatenation** : Utiliser StringBuilder
- **LINQ inefficace** : Optimiser queries

### 1.9 Sécurité
- **Validation insuffisante** : Implémenter FluentValidation partout
- **SQL Injection** : Vérifier paramétrage (SqlCommand/Dapper)
- **XSS** : Encoder outputs Blazor
- **CSRF** : Vérifier tokens antiforgery
- **Secrets hardcodés** : Migrer vers Azure Key Vault/User Secrets
- **JWT/Claims** : Validation stricte
- **Authorization** : Vérifier [Authorize] sur toutes les actions sensibles
- **CORS** : Configuration restrictive
- **Rate Limiting** : Implémenter middleware

### 1.10 Testabilité
- **Code difficile à tester** : Trop de dépendances concrètes
- **Manque de couverture** : < 80% coverage
- **Tests fragiles** : Dépendants de l'ordre d'exécution
- **Manque de tests** : Paths non couverts, edge cases
- **Tests lents** : Tests d'intégration sans isolation
- **Mutation score faible** : < 85% (Stryker.NET)

## Phase 2 : Planification Atomique
Pour chaque problème identifié, créer une tâche atomique avec :
- **Scope** : Périmètre précis du refactoring
- **Objectif** : Résultat attendu mesurable
- **Impact** : Risque et bénéfice estimés
- **Priorité** : Critique, Haute, Moyenne, Basse
- **Dépendances** : Autres tâches requises avant celle-ci
- **Tests** : Stratégie de test pour garantir la non-régression

Ordre de priorité recommandé :
1. Corrections critiques (sécurité, bugs majeurs)
2. Améliorations structurelles (architecture, SOLID)
3. Extraction de composants réutilisables (librairies NuGet)
4. Optimisations de performance
5. Améliorations de lisibilité et documentation

## Phase 3 : Exécution TDD (Red-Green-Refactor)
Pour chaque tâche de refactoring :

### Étape 1 : RED - Écrire les tests en premier
- Écrire des tests caractérisant le comportement actuel
- Identifier les scénarios de régression possibles
- Créer des tests pour le comportement attendu après refactoring
- Vérifier que les nouveaux tests échouent (RED)

### Étape 2 : GREEN - Implémenter le refactoring
- Effectuer le refactoring minimal pour passer les tests
- Maintenir le comportement fonctionnel existant
- Vérifier que tous les tests passent (GREEN)
- Valider avec les tests d'intégration et acceptances

### Étape 3 : REFACTOR - Optimiser et nettoyer
- Améliorer la qualité du code refactoré
- Éliminer toute duplication introduite
- Optimiser pour la performance et la lisibilité
- S'assurer que tous les tests restent verts

### Étape 4 : VALIDATION - Mutation Testing
- Exécuter Stryker.NET sur le code refactoré
- Viser un mutation score ≥ 85%
- Améliorer les tests si des mutants survivent
- Documenter les résultats

## Phase 4 : Extraction de Composants Réutilisables
Identifier et extraire les éléments mutualisables en packages NuGet :

### Candidats à l'extraction :

#### 4.1 Middlewares (ADP.Common.Middleware)
- **CorrelationIdMiddleware** : Gestion X-Correlation-ID
- **ExceptionHandlingMiddleware** : Gestion centralisée erreurs
- **RequestLoggingMiddleware** : Logging requêtes/réponses
- **PerformanceMonitoringMiddleware** : Métriques de performance
- **RateLimitingMiddleware** : Limitation de débit
- **AuthenticationMiddleware** : Authentification personnalisée

#### 4.2 Extensions (ADP.Common.Extensions)
- **ServiceCollectionExtensions** : Extensions DI
- **HttpContextExtensions** : Extensions HttpContext
- **StringExtensions** : Manipulations string courantes
- **DateTimeExtensions** : Conversions dates
- **ClaimsPrincipalExtensions** : Extraction claims
- **ConfigurationExtensions** : Configuration helpers

#### 4.3 Data Access (ADP.Common.Data)
- **SqlCommandExtensions** : Extensions CRUD SqlCommand (déjà en cours)
- **DapperExtensions** : Extensions CRUD Dapper (déjà en cours)
- **UnitOfWorkPattern** : Implémentation générique
- **RepositoryBase** : Repository base générique
- **QueryBuilder** : Construction requêtes dynamiques
- **ConnectionFactory** : Factory de connexions DB

#### 4.4 Validation (ADP.Common.Validation)
- **FluentValidation rules** : Règles personnalisées
- **ValidationBehavior** : Validation pipeline (MediatR)
- **Custom validators** : Email, Phone, IBAN, etc.
- **Business rules validators** : Règles métier réutilisables

#### 4.5 Authentication & Authorization (ADP.Common.Auth)
- **EntraIdConfiguration** : Configuration Entra ID
- **JwtTokenHandler** : Gestion tokens JWT
- **ClaimsTransformer** : Transformation claims
- **AuthorizationHandlers** : Handlers personnalisés
- **PolicyProvider** : Fournisseur de policies
- **RoleChecker** : Vérification rôles/groupes

#### 4.6 Logging (ADP.Common.Logging)
- **SerilogConfiguration** : Configuration Serilog
- **CustomEnrichers** : CorrelationId, UserInfo, etc.
- **StructuredLogging** : Helpers logging structuré
- **AuditLogger** : Logging d'audit
- **PerformanceLogger** : Logging performance

#### 4.7 Caching (ADP.Common.Caching)
- **CacheService** : Service cache générique
- **DistributedCacheExtensions** : Extensions cache distribué
- **CacheKeyBuilder** : Construction clés cache
- **CacheInvalidation** : Stratégies invalidation

#### 4.8 Testing Utilities (ADP.Common.Testing)
- **TestBuilders** : Builders pour objets complexes
- **TestFixtures** : Fixtures réutilisables
- **MockFactories** : Factories de mocks
- **CustomAssertions** : Assertions personnalisées
- **IntegrationTestBase** : Base pour tests intégration
- **TestContainersHelpers** : Helpers TestContainers

#### 4.9 API Common (ADP.Common.Api)
- **ApiResponse<T>** : Modèle réponse standardisé
- **PaginationHelper** : Helpers pagination
- **ApiVersioning** : Configuration versioning
- **SwaggerConfiguration** : Configuration Swagger/OpenAPI
- **ProblemDetailsFactory** : Factory ProblemDetails (RFC 7807)
- **HealthChecks** : Health checks personnalisés

#### 4.10 Blazor Common (ADP.Common.Blazor)
- **BaseComponents** : Composants base réutilisables
- **StateManagement** : Gestion état (Fluxor)
- **NavigationHelpers** : Helpers navigation
- **LocalizationHelpers** : Helpers localisation
- **MudBlazor presets** : Configurations MudBlazor standard

### Structure des packages :
```
ADP.Common.{ComponentName}/
├── README.md                          # Documentation principale
├── CHANGELOG.md                       # Historique versions
├── LICENSE                            # Licence
├── .editorconfig                      # Style code
├── Directory.Build.props              # Propriétés MSBuild
├── src/
│   └── ADP.Common.{ComponentName}/
│       ├── ADP.Common.{ComponentName}.csproj
│       ├── Extensions/                # Extensions methods
│       ├── Configuration/             # Configuration & Options
│       ├── Services/                  # Services
│       ├── Interfaces/                # Abstractions
│       ├── Models/                    # DTOs/Models
│       ├── Constants/                 # Constantes
│       └── Resources/                 # Resources (si nécessaire)
├── tests/
│   ├── ADP.Common.{ComponentName}.Tests/
│   │   ├── ADP.Common.{ComponentName}.Tests.csproj
│   │   ├── Unit/                      # Tests unitaires
│   │   ├── Integration/               # Tests intégration
│   │   ├── Fixtures/                  # Fixtures
│   │   └── stryker-config.json        # Config Stryker
│   └── ADP.Common.{ComponentName}.Benchmarks/
│       ├── ADP.Common.{ComponentName}.Benchmarks.csproj
│       └── Benchmarks/                # BenchmarkDotNet
├── samples/
│   └── ADP.Common.{ComponentName}.Sample/
│       ├── ADP.Common.{ComponentName}.Sample.csproj
│       └── Program.cs                 # Exemple utilisation
└── documentations/
    ├── getting-started.md             # Guide démarrage
    ├── configuration.md               # Guide configuration
    ├── api-reference.md               # Référence API
    └── migration-guide.md             # Guide migration versions
```

### Exigences pour chaque package :

#### Qualité Code
- ✅ **Autonomie** : Aucune dépendance interne à la solution
- ✅ **Naming conventions** : ADP.Common.{Category}.{SubCategory}
- ✅ **Code analyzers** : StyleCop, Roslynator, SonarAnalyzer
- ✅ **EditorConfig** : Style uniforme
- ✅ **Nullable reference types** : Activé
- ✅ **XML Documentation** : > 90% API publiques documentées

#### Tests & Qualité
- ✅ **Unit Tests** : xUnit, NSubstitute, NFluent
- ✅ **Coverage** : ≥ 80%, idéalement 90%+
- ✅ **Mutation Score** : ≥ 85% (Stryker.NET)
- ✅ **Integration Tests** : TestContainers si applicable
- ✅ **Benchmarks** : BenchmarkDotNet pour opérations critiques
- ✅ **Architecture Tests** : NetArchTest.Rules

#### Documentation
- ✅ **README complet** : Installation, usage, exemples
- ✅ **CHANGELOG** : Semantic versioning (SemVer 2.0)
- ✅ **API Documentation** : XML comments + DocFx
- ✅ **Samples** : Projet exemple fonctionnel
- ✅ **Migration guides** : Entre versions majeures

#### Configuration
- ✅ **Options Pattern** : IOptions<T>, IOptionsSnapshot<T>
- ✅ **Validation** : ValidateDataAnnotations, ValidateOnStart
- ✅ **Configuration builders** : Fluent API
- ✅ **Environment-specific** : appsettings.{Environment}.json

#### Performance
- ✅ **Benchmarks** : Comparaison avant/après refactoring
- ✅ **Memory efficient** : Span<T>, ArrayPool<T>, ValueTask
- ✅ **Async optimizations** : ConfigureAwait, ValueTask
- ✅ **Caching** : Où approprié

#### CI/CD
- ✅ **GitHub Actions / Azure Pipelines**
- ✅ **Build** : Compilation multi-target si nécessaire
- ✅ **Test** : Exécution tests + coverage + mutation
- ✅ **Pack** : Génération package NuGet
- ✅ **Publish** : Publication sur feed interne/NuGet.org
- ✅ **Release notes** : Automatiquement générées

## Phase 5 : Réorganisation par Concerns
Restructurer la codebase selon les principes :

### 5.1 Vertical Slices (Feature-based Organization)
Organiser par features/use cases plutôt que par layers techniques :

```
src/ADP.Application/
├── Features/                          # Organisation par features
│   ├── Users/                         # Feature: Gestion utilisateurs
│   │   ├── Commands/                  # Write operations (CQRS)
│   │   │   ├── CreateUser/
│   │   │   │   ├── CreateUserCommand.cs
│   │   │   │   ├── CreateUserCommandHandler.cs
│   │   │   │   ├── CreateUserCommandValidator.cs
│   │   │   │   └── CreateUserCommandTests.cs
│   │   │   ├── UpdateUser/
│   │   │   │   ├── UpdateUserCommand.cs
│   │   │   │   ├── UpdateUserCommandHandler.cs
│   │   │   │   ├── UpdateUserCommandValidator.cs
│   │   │   │   └── UpdateUserCommandTests.cs
│   │   │   └── DeleteUser/
│   │   │       └── ...
│   │   ├── Queries/                   # Read operations (CQRS)
│   │   │   ├── GetUser/
│   │   │   │   ├── GetUserQuery.cs
│   │   │   │   ├── GetUserQueryHandler.cs
│   │   │   │   ├── GetUserQueryValidator.cs
│   │   │   │   └── GetUserQueryTests.cs
│   │   │   ├── GetUsers/
│   │   │   │   ├── GetUsersQuery.cs
│   │   │   │   ├── GetUsersQueryHandler.cs
│   │   │   │   └── ...
│   │   │   └── SearchUsers/
│   │   │       └── ...
│   │   ├── Events/                    # Domain events
│   │   │   ├── UserCreatedEvent.cs
│   │   │   ├── UserUpdatedEvent.cs
│   │   │   └── UserDeletedEvent.cs
│   │   ├── DTOs/                      # Feature-specific DTOs
│   │   │   ├── UserDto.cs
│   │   │   └── UserListDto.cs
│   │   └── Mappings/                  # AutoMapper profiles
│   │       └── UserMappingProfile.cs
│   │
│   ├── Authentication/                # Feature: Authentification
│   │   ├── Commands/
│   │   │   ├── Login/
│   │   │   └── Logout/
│   │   ├── Queries/
│   │   │   ├── GetCurrentUser/
│   │   │   └── ValidateToken/
│   │   └── Events/
│   │
│   └── Claims/                        # Feature: Gestion claims
│       ├── Queries/
│       │   ├── GetUserClaims/
│       │   └── GetAvailableClaims/
│       └── Commands/
│           ├── AddClaim/
│           └── RemoveClaim/
│
└── Common/                            # Code commun à toutes features
    ├── Behaviors/                     # MediatR behaviors
    │   ├── ValidationBehavior.cs
    │   ├── LoggingBehavior.cs
    │   ├── PerformanceBehavior.cs
    │   └── TransactionBehavior.cs
    ├── Exceptions/                    # Exceptions applicatives
    ├── Interfaces/                    # Interfaces communes
    └── Models/                        # Models communs
```

### 5.2 Separation of Concerns (Layered Architecture)

#### UI Concerns (Presentation Layer)
```
src/ADP.BlazorApp/
├── Components/                        # Composants UI
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   ├── NavMenu.razor
│   │   └── Header.razor
│   ├── Shared/                        # Composants réutilisables
│   │   ├── DataGrid.razor
│   │   ├── LoadingSpinner.razor
│   │   ├── ConfirmDialog.razor
│   │   └── ErrorBoundary.razor
│   └── Forms/                         # Composants formulaires
│       ├── UserForm.razor
│       └── ValidationSummary.razor
│
├── Pages/                             # Pages Blazor
│   ├── Users/
│   │   ├── Index.razor                # Liste
│   │   ├── Details.razor              # Détails
│   │   ├── Create.razor               # Création
│   │   └── Edit.razor                 # Edition
│   ├── Claims/
│   └── Dashboard/
│
├── ViewModels/                        # ViewModels (si nécessaire)
│   └── UserViewModel.cs
│
└── Services/                          # Services UI
    ├── StateContainer.cs              # Gestion état
    ├── NavigationService.cs
    └── NotificationService.cs
```

#### Application Concerns (Use Cases)
```
src/ADP.Application/
├── Features/                          # Vertical slices (voir 5.1)
├── Services/                          # Application services
│   ├── IUserService.cs
│   ├── UserService.cs
│   └── ClaimsService.cs
├── Interfaces/                        # Abstractions
│   ├── IEmailService.cs
│   └── ICacheService.cs
└── Validators/                        # FluentValidation
    └── UserValidator.cs
```

#### Domain Concerns (Business Logic)
```
src/ADP.Domain/
├── Entities/                          # Entités avec comportement
│   ├── User.cs
│   ├── Role.cs
│   └── Claim.cs
├── ValueObjects/                      # Value objects immutables
│   ├── Email.cs
│   ├── PhoneNumber.cs
│   └── Address.cs
├── Aggregates/                        # Aggregate roots
│   └── UserAggregate.cs
├── Events/                            # Domain events
│   ├── UserCreatedEvent.cs
│   └── UserUpdatedEvent.cs
├── Specifications/                    # Business rules (Specification pattern)
│   ├── UserActiveSpecification.cs
│   └── UserHasRoleSpecification.cs
├── Interfaces/                        # Domain interfaces
│   ├── IUserRepository.cs
│   ├── IUnitOfWork.cs
│   └── IDomainEventDispatcher.cs
└── Exceptions/                        # Domain exceptions
    ├── UserNotFoundException.cs
    └── InvalidUserException.cs
```

#### Infrastructure Concerns (Technical Details)
```
src/ADP.Infrastructure/
├── Data/                              # Data access
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   └── ClaimRepository.cs
│   ├── UnitOfWork/
│   │   └── UnitOfWork.cs
│   ├── Configurations/                # EF Core configurations
│   │   └── UserConfiguration.cs
│   └── Migrations/
│
├── ExternalServices/                  # Services externes
│   ├── Email/
│   │   └── EmailService.cs
│   ├── Storage/
│   │   └── BlobStorageService.cs
│   └── Graph/
│       └── GraphApiService.cs
│
├── Authentication/                    # Auth infrastructure
│   ├── EntraIdAuthenticationHandler.cs
│   └── JwtTokenService.cs
│
├── Caching/                          # Cache implementation
│   ├── RedisCacheService.cs
│   └── MemoryCacheService.cs
│
└── Logging/                          # Logging infrastructure
    └── SerilogConfiguration.cs
```

#### Cross-Cutting Concerns
```
src/ADP.Core/                          # Shared kernel
├── Constants/                         # Constantes globales
│   ├── AuthConstants.cs
│   ├── CacheKeys.cs
│   └── ValidationMessages.cs
├── Extensions/                        # Extensions globales
│   ├── StringExtensions.cs
│   └── DateTimeExtensions.cs
├── Helpers/                          # Helpers globaux
│   └── CorrelationIdHelper.cs
└── Attributes/                       # Custom attributes
    └── AuditableAttribute.cs
```

### 5.3 Modular Monolith Pattern
Chaque module doit être :

#### Structure d'un module
```
src/ADP.Modules.{ModuleName}/
├── ADP.Modules.{ModuleName}.csproj
├── Module.cs                          # Point d'entrée module
├── ModuleExtensions.cs                # Extensions DI
├── Api/                               # Endpoints publics
│   └── {ModuleName}Controller.cs
├── Application/                       # Use cases
│   ├── Commands/
│   └── Queries/
├── Domain/                            # Domain logic
│   ├── Entities/
│   └── Events/
├── Infrastructure/                    # Implémentations
│   └── Repositories/
└── Tests/                             # Tests module
    ├── Unit/
    └── Integration/
```

#### Principes modulaires
- ✅ **Indépendant** : Peut fonctionner seul si nécessaire
- ✅ **Encapsulé** : N'expose que son contrat public (interfaces)
- ✅ **Cohésif** : Responsabilité unique et claire
- ✅ **Découplé** : Communication via interfaces et events
- ✅ **Testable** : Tests isolés par module
- ✅ **Déployable** : Peut devenir microservice si nécessaire

#### Communication inter-modules
```csharp
// Mauvais : Référence directe
public class OrderService
{
    private readonly UserRepository _userRepo; // ❌ Couplage fort
}

// Bon : Via interface
public class OrderService
{
    private readonly IUserService _userService; // ✅ Découplé
}

// Meilleur : Via events
public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        // Réaction asynchrone, découplage temporel
    }
}
```

### 5.4 Patterns Architecturaux Recommandés

#### CQRS (Command Query Responsibility Segregation)
- **Commands** : Modifient l'état (Write)
- **Queries** : Lisent l'état (Read)
- **MediatR** : Implémentation via pipeline
- **Séparation** : Modèles différents read/write si nécessaire

#### Repository Pattern
```csharp
// Interface dans Domain
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task UpdateAsync(User user, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}

// Implémentation dans Infrastructure
public class UserRepository : IUserRepository
{
    // Implémentation avec Dapper ou SqlCommand
}
```

#### Unit of Work Pattern
```csharp
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IClaimRepository Claims { get; }
    Task<int> SaveChangesAsync(CancellationToken ct);
    Task BeginTransactionAsync(CancellationToken ct);
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync(CancellationToken ct);
}
```

#### Specification Pattern
```csharp
public class UserActiveSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
        => user => user.IsActive && !user.IsDeleted;
}

// Usage
var activeUsers = await _repository
    .FindAsync(new UserActiveSpecification());
```

### 5.5 Organisation Blazor par Areas/Concerns

```
src/ADP.BlazorApp/
├── Areas/                             # Regroupements fonctionnels
│   ├── Identity/                      # Area: Identité
│   │   ├── Pages/                     # Concerns
│   │   │   ├── Profile/               # Concern: Profil
│   │   │   │   ├── Index.razor
│   │   │   │   ├── Edit/              # SubConcern: Edition
│   │   │   │   │   └── Edit.razor
│   │   │   │   └── Security/          # SubConcern: Sécurité
│   │   │   │       └── Security.razor
│   │   │   └── Account/               # Concern: Compte
│   │   │       ├── Login.razor
│   │   │       └── Logout.razor
│   │   └── Components/                # Composants area-specific
│   │       └── ProfileCard.razor
│   │
│   ├── Users/                         # Area: Utilisateurs
│   │   ├── Pages/
│   │   │   ├── Management/            # Concern: Gestion
│   │   │   │   ├── List.razor
│   │   │   │   ├── Create/            # SubConcern: Création
│   │   │   │   │   └── Create.razor
│   │   │   │   └── Edit/              # SubConcern: Edition
│   │   │   │       └── Edit.razor
│   │   │   └── Permissions/           # Concern: Permissions
│   │   │       └── Index.razor
│   │   └── Components/
│   │       ├── UserGrid.razor
│   │       └── UserCard.razor
│   │
│   └── Administration/                # Area: Administration
│       └── Pages/
│           ├── Settings/
│           └── Logs/
│
└── Shared/                            # Composants globaux
    ├── MainLayout.razor
    └── NavMenu.razor
```

## Phase 6 : Optimisations

### 6.1 Performance

#### Database & Queries
- **Requêtes N+1** : Détecter et éliminer
  ```csharp
  // ❌ N+1 Problem
  var users = await _repo.GetAllAsync();
  foreach (var user in users)
  {
      var claims = await _claimRepo.GetByUserIdAsync(user.Id); // N queries!
  }
  
  // ✅ Solution: Single query avec projection
  var usersWithClaims = await _connection.QueryAsync<User, Claim, User>(
      sql: @"SELECT u.*, c.* 
             FROM users u 
             LEFT JOIN claims c ON u.id = c.user_id",
      map: (user, claim) => { /* mapping */ },
      splitOn: "id"
  );
  ```

- **Indexation** : Analyser et créer index appropriés
  ```sql
  -- Analyser les requêtes lentes
  SELECT * FROM pg_stat_statements ORDER BY mean_exec_time DESC;
  
  -- Créer index sur colonnes fréquemment filtrées
  CREATE INDEX idx_users_email ON users(email);
  CREATE INDEX idx_users_active ON users(is_active) WHERE is_active = true;
  ```

- **Projections** : Sélectionner uniquement colonnes nécessaires
  ```csharp
  // ❌ Select *
  var users = await _connection.QueryAsync<User>("SELECT * FROM users");
  
  // ✅ Projection ciblée
  var users = await _connection.QueryAsync<UserListDto>(
      "SELECT id, firstname, lastname, email FROM users"
  );
  ```

- **Pagination** : Toujours paginer grandes listes
  ```csharp
  public async Task<PagedResult<UserDto>> GetUsersAsync(
      int page, int pageSize, CancellationToken ct)
  {
      var offset = (page - 1) * pageSize;
      var sql = @"
          SELECT id, firstname, lastname, email 
          FROM users 
          ORDER BY lastname, firstname
          LIMIT @PageSize OFFSET @Offset";
      
      var users = await _connection.QueryAsync<UserDto>(
          sql, new { PageSize = pageSize, Offset = offset });
      
      var total = await _connection.ExecuteScalarAsync<int>(
          "SELECT COUNT(*) FROM users");
      
      return new PagedResult<UserDto>(users, total, page, pageSize);
  }
  ```

#### Caching Stratégies
- **Memory Cache** : Données fréquemment accédées
  ```csharp
  public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct)
  {
      var cacheKey = $"user:{id}";
      
      return await _cache.GetOrCreateAsync(cacheKey, async entry =>
      {
          entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
          entry.SlidingExpiration = TimeSpan.FromMinutes(5);
          return await _repository.GetByIdAsync(id, ct);
      });
  }
  ```

- **Distributed Cache** : Environnements multi-instances
  ```csharp
  public async Task<T?> GetOrSetAsync<T>(
      string key, 
      Func<Task<T>> factory,
      TimeSpan? expiration = null)
  {
      var cached = await _distributedCache.GetStringAsync(key);
      if (cached != null)
          return JsonSerializer.Deserialize<T>(cached);
      
      var value = await factory();
      var serialized = JsonSerializer.Serialize(value);
      
      await _distributedCache.SetStringAsync(key, serialized, new()
      {
          AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
      });
      
      return value;
  }
  ```

- **Cache invalidation** : Stratégies explicites
  ```csharp
  public async Task UpdateUserAsync(User user, CancellationToken ct)
  {
      await _repository.UpdateAsync(user, ct);
      
      // Invalidation cache
      _cache.Remove($"user:{user.Id}");
      _cache.Remove("users:all");
      
      // Ou utiliser pattern Notification
      await _mediator.Publish(new UserUpdatedEvent(user.Id), ct);
  }
  ```

#### Allocations & Memory
- **Span<T>** pour manipulations strings/arrays
  ```csharp
  // ❌ Allocations
  public string FormatName(string firstName, string lastName)
  {
      return firstName.ToUpper() + " " + lastName.ToUpper();
  }
  
  // ✅ Span<T>
  public string FormatName(ReadOnlySpan<char> firstName, ReadOnlySpan<char> lastName)
  {
      Span<char> buffer = stackalloc char[firstName.Length + lastName.Length + 1];
      firstName.ToUpperInvariant(buffer);
      buffer[firstName.Length] = ' ';
      lastName.ToUpperInvariant(buffer[(firstName.Length + 1)..]);
      return new string(buffer);
  }
  ```

- **ArrayPool<T>** pour buffers temporaires
  ```csharp
  public async Task ProcessLargeDataAsync(Stream stream)
  {
      var buffer = ArrayPool<byte>.Shared.Rent(8192);
      try
      {
          int bytesRead;
          while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
          {
              // Process buffer[0..bytesRead]
          }
      }
      finally
      {
          ArrayPool<byte>.Shared.Return(buffer);
      }
  }
  ```

- **ValueTask** pour opérations souvent synchrones
  ```csharp
  // Cache hit souvent synchrone
  public ValueTask<User?> GetCachedUserAsync(Guid id)
  {
      if (_cache.TryGetValue($"user:{id}", out User? user))
          return new ValueTask<User?>(user); // Synchrone, pas d'allocation
      
      return new ValueTask<User?>(LoadUserAsync(id)); // Asynchrone
  }
  ```

- **StringBuilder** pour concaténations multiples
  ```csharp
  // ❌ Multiple allocations
  string BuildQuery(IEnumerable<string> columns)
  {
      string query = "SELECT ";
      foreach (var col in columns)
          query += col + ", "; // Allocation à chaque itération!
      return query.TrimEnd(',', ' ') + " FROM users";
  }
  
  // ✅ StringBuilder
  string BuildQuery(IEnumerable<string> columns)
  {
      var sb = new StringBuilder("SELECT ");
      foreach (var col in columns)
          sb.Append(col).Append(", ");
      sb.Length -= 2; // Enlever dernière virgule
      sb.Append(" FROM users");
      return sb.ToString();
  }
  ```

#### Async/Await Optimizations
- **ConfigureAwait(false)** dans libraries
  ```csharp
  public async Task<User?> GetUserAsync(Guid id)
  {
      // Dans une library, pas besoin de retourner au contexte original
      var user = await _repository
          .GetByIdAsync(id)
          .ConfigureAwait(false);
      
      return user;
  }
  ```

- **Éviter .Result et .Wait()**
  ```csharp
  // ❌ Blocking, risque deadlock
  var user = GetUserAsync(id).Result;
  
  // ✅ Async all the way
  var user = await GetUserAsync(id);
  ```

- **Parallélisation** quand approprié
  ```csharp
  // ❌ Séquentiel
  var user = await GetUserAsync(userId);
  var claims = await GetClaimsAsync(userId);
  var roles = await GetRolesAsync(userId);
  
  // ✅ Parallèle (opérations indépendantes)
  var userTask = GetUserAsync(userId);
  var claimsTask = GetClaimsAsync(userId);
  var rolesTask = GetRolesAsync(userId);
  
  await Task.WhenAll(userTask, claimsTask, rolesTask);
  
  var user = await userTask;
  var claims = await claimsTask;
  var roles = await rolesTask;
  ```

#### LINQ Optimizations
```csharp
// ❌ Inefficace
var result = users
    .Where(u => u.IsActive)
    .ToList()
    .Where(u => u.Email != null) // Après ToList()!
    .Select(u => new UserDto { /* ... */ });

// ✅ Optimisé
var result = users
    .Where(u => u.IsActive && u.Email != null)
    .Select(u => new UserDto { /* ... */ })
    .ToList(); // ToList() en dernier
```

#### BenchmarkDotNet
Créer benchmarks pour opérations critiques :
```csharp
[MemoryDiagnoser]
public class UserRepositoryBenchmarks
{
    private IUserRepository _repository;
    
    [Benchmark(Baseline = true)]
    public async Task GetUserById_Original()
    {
        await _repository.GetByIdAsync(Guid.NewGuid());
    }
    
    [Benchmark]
    public async Task GetUserById_Optimized()
    {
        await _repositoryOptimized.GetByIdAsync(Guid.NewGuid());
    }
}
```

### 6.2 Scalabilité

#### Stateless Services
```csharp
// ❌ Stateful (problème en multi-instances)
public class UserService
{
    private User _currentUser; // État!
}

// ✅ Stateless
public class UserService
{
    public async Task ProcessUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await _repository.GetByIdAsync(userId, ct);
        // Pas d'état conservé
    }
}
```

#### Idempotence
```csharp
public async Task<Result> CreateUserAsync(CreateUserCommand cmd, CancellationToken ct)
{
    // Vérifier si déjà créé (idempotence key)
    if (await _repository.ExistsByIdempotenceKeyAsync(cmd.IdempotenceKey, ct))
    {
        return Result.Success(); // Déjà traité
    }
    
    var user = new User { /* ... */ };
    await _repository.AddAsync(user, ct);
    await _repository.SaveIdempotenceKeyAsync(cmd.IdempotenceKey, user.Id, ct);
    
    return Result.Success();
}
```

#### Circuit Breaker Pattern
```csharp
// Utiliser Polly
services.AddHttpClient<IGraphApiService, GraphApiService>()
    .AddTransientHttpErrorPolicy(builder => 
        builder.CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30)
        ));
```

#### Rate Limiting
```csharp
// ASP.NET Core 7+
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", options =>
    {
        options.Window = TimeSpan.FromMinutes(1);
        options.PermitLimit = 100;
        options.QueueLimit = 0;
    });
});

// Sur endpoint
[EnableRateLimiting("api")]
[HttpGet]
public async Task<IActionResult> GetUsers() { /* ... */ }
```

#### Retry Policies
```csharp
services.AddHttpClient<IExternalService, ExternalService>()
    .AddTransientHttpErrorPolicy(builder =>
        builder.WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                _logger.LogWarning(
                    "Retry {RetryAttempt} after {Delay}s", 
                    retryAttempt, 
                    timespan.TotalSeconds);
            }
        ));
```

### 6.3 Profiling & Monitoring

#### Application Insights / OpenTelemetry
```csharp
// Métriques personnalisées
public class UserService
{
    private readonly Counter<int> _userCreatedCounter;
    private readonly Histogram<double> _getUserDuration;
    
    public UserService(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("ADP.Application");
        _userCreatedCounter = meter.CreateCounter<int>("users.created");
        _getUserDuration = meter.CreateHistogram<double>("users.get.duration");
    }
    
    public async Task CreateUserAsync(User user)
    {
        await _repository.AddAsync(user);
        _userCreatedCounter.Add(1, new("tenant", user.TenantId));
    }
    
    public async Task<User?> GetUserAsync(Guid id)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return await _repository.GetByIdAsync(id);
        }
        finally
        {
            _getUserDuration.Record(sw.Elapsed.TotalMilliseconds);
        }
    }
}
```

#### Health Checks
```csharp
services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql")
    .AddRedis(redisConnection, name: "redis")
    .AddUrlGroup(new Uri("https://graph.microsoft.com"), name: "graph-api")
    .AddCheck<CustomHealthCheck>("custom");
```

#### Performance Counters
```csharp
[EventSource(Name = "ADP.Application")]
public sealed class ApplicationEventSource : EventSource
{
    public static readonly ApplicationEventSource Log = new();
    
    [Event(1, Level = EventLevel.Informational)]
    public void UserCreated(string userId) => WriteEvent(1, userId);
    
    [Event(2, Level = EventLevel.Warning)]
    public void SlowQuery(string queryName, double durationMs)
        => WriteEvent(2, queryName, durationMs);
}
```

## Phase 7 : Amélioration de la Testabilité

### Architecture Testable
- Dependency Injection pour tous les services
- Interfaces pour tous les services externes
- Pure functions quand possible
- Éviter les static methods et singletons

### Types de Tests
- **Unit Tests** : xUnit, NSubstitute, NFluent, Bogus
- **Integration Tests** : TestContainers, WebApplicationFactory
- **Acceptance Tests** : SpecFlow, BDD scenarios
- **Performance Tests** : BenchmarkDotNet
- **Architecture Tests** : NetArchTest.Rules
- **Mutation Tests** : Stryker.NET

### Test Helpers Réutilisables
- Builders pour objets de test complexes
- Fixtures pour données de test
- Custom assertions
- Test utilities partagés

## Phase 8 : Documentation et Standards

### Documentation Code
- XML comments pour APIs publiques
- README pour chaque projet/package
- Architecture Decision Records (ADRs)
- Diagrammes d'architecture (C4 model)

### Standards
- EditorConfig pour style de code
- Analyzers et code quality rules
- Pre-commit hooks
- CI/CD pipelines

## Principes Directeurs

### Pragmatisme
- **Boy Scout Rule** : Laisser le code plus propre qu'on l'a trouvé
- **Refactoring incrémental** : Petits changements, commits fréquents
- **Mesure avant/après** : Métriques objectives (complexité, couverture, performance)
- **ROI** : Prioriser les refactorings à fort impact
- **Risk Management** : Commencer par les zones à faible risque

### Qualité
- **Code Coverage** : Minimum 80%, idéalement 90%+
- **Mutation Score** : Minimum 85%, idéalement 90%+
- **Complexity** : Complexité cyclomatique < 10 par méthode
- **Maintainability Index** : > 80
- **Technical Debt** : Réduire systématiquement

### Collaboration
- **Code Reviews** : Systematic peer review
- **Pair Programming** : Pour refactorings complexes
- **Knowledge Sharing** : Documentation et sessions de partage

## Checklist par Tâche de Refactoring

### Avant le Refactoring
- [ ] **Analyse** : Problème clairement identifié et documenté
- [ ] **Impact Assessment** : Risques et bénéfices évalués
- [ ] **Baseline Metrics** : Métriques actuelles capturées
  - [ ] Code coverage actuel (%)
  - [ ] Mutation score actuel (%)
  - [ ] Complexité cyclomatique
  - [ ] Performance baseline (benchmarks si applicable)
- [ ] **Dépendances** : Tâches préalables complétées
- [ ] **Branche** : Créer feature branch `refactor/{description}`

### Phase RED (Tests First)
- [ ] **Tests caractérisants** : Comportement actuel documenté par tests
  ```csharp
  [Fact]
  public async Task CurrentBehavior_Should_BePreserved()
  {
      // Documenter comportement existant
  }
  ```
- [ ] **Scénarios de régression** : Cas limites identifiés et testés
  - [ ] Happy path
  - [ ] Edge cases
  - [ ] Error cases
  - [ ] Null/empty inputs
  - [ ] Boundary conditions
- [ ] **Nouveaux tests** : Tests pour comportement post-refactoring écrits
- [ ] **Tests échouent** : Vérifier que nouveaux tests sont RED
- [ ] **Coverage gap analysis** : Identifier code non couvert

### Phase GREEN (Implementation)
- [ ] **Refactoring minimal** : Changement le plus simple pour passer tests
- [ ] **Comportement préservé** : Fonctionnalités existantes inchangées
- [ ] **Tests passent** : Tous les tests (anciens + nouveaux) GREEN
- [ ] **Compilation** : Aucune erreur, aucun warning
- [ ] **Tests d'intégration** : Validés si applicable
- [ ] **Tests d'acceptance** : Validés si applicable

### Phase REFACTOR (Optimize)
- [ ] **Code cleanup** : Duplication éliminée
- [ ] **Naming** : Noms explicites et cohérents
- [ ] **SOLID** : Principes respectés
- [ ] **Patterns** : Patterns appropriés appliqués
- [ ] **Comments** : Code self-documenting, comments uniquement si nécessaire
- [ ] **Magic values** : Remplacés par constantes nommées
- [ ] **Performance** : Optimisations appliquées si nécessaire
- [ ] **Tests restent verts** : Validation continue

### Phase VALIDATION (Mutation Testing)
- [ ] **Stryker.NET exécuté** : `dotnet stryker --config-file stryker-config.json`
- [ ] **Mutation score ≥ 85%** : Objectif atteint
- [ ] **Mutants survivants analysés** : Comprendre pourquoi
- [ ] **Tests améliorés** : Si mutants survivants pertinents
- [ ] **Re-run si nécessaire** : Jusqu'à score satisfaisant
- [ ] **Rapport sauvegardé** : Dans StrykerOutput/reports/

### Documentation
- [ ] **Code comments** : XML documentation pour APIs publiques
- [ ] **README** : Mis à jour si nécessaire
- [ ] **CHANGELOG** : Changements documentés
- [ ] **ADR** : Architecture Decision Record si décision architecturale
- [ ] **Migration guide** : Si breaking changes

### Métriques & Qualité
- [ ] **Code coverage amélioré** : Avant → Après documenté
- [ ] **Mutation score amélioré** : Avant → Après documenté
- [ ] **Complexité réduite** : Avant → Après documenté
- [ ] **Code analyzers** : Aucun nouveau warning
- [ ] **Performance** : Non dégradée (benchmarks si critique)
- [ ] **Memory** : Pas de fuites détectées

### Code Review
- [ ] **Self-review** : Relecture personnelle complète
- [ ] **Diff review** : Changements cohérents et minimaux
- [ ] **Tests review** : Tests pertinents et complets
- [ ] **PR créée** : Description claire avec contexte
- [ ] **Reviewers assignés** : Au moins 1 reviewer
- [ ] **CI/CD green** : Tous les checks passent
- [ ] **Feedback incorporé** : Commentaires traités

### Avant Merge
- [ ] **Rebase** : Branch à jour avec main/develop
- [ ] **Conflicts résolus** : Si applicable
- [ ] **Final test run** : Tous tests passent
- [ ] **Final mutation test** : Score toujours ≥ 85%
- [ ] **Squash commits** : Si nécessaire (commits atomiques propres)
- [ ] **Commit message** : Descriptif et suit conventions
  ```
  refactor(Users): Extract UserValidator to separate class
  
  - Moved validation logic from UserService to UserValidator
  - Implemented FluentValidation
  - Added comprehensive validation tests
  - Mutation score: 78% → 92%
  - Coverage: 75% → 88%
  
  Closes #123
  ```

### Après Merge
- [ ] **Deploy** : En staging/dev
- [ ] **Smoke tests** : Vérifications basiques
- [ ] **Monitoring** : Surveiller métriques
- [ ] **Performance** : Comparer avec baseline
- [ ] **Rollback plan** : Prêt si nécessaire
- [ ] **Documentation deployment** : Wiki/docs mis à jour
- [ ] **Team notification** : Équipe informée des changements

### Refactoring Patterns Checklist

#### Extract Method
- [ ] Méthode > 20 lignes → extraire logique
- [ ] Nom explicite du comportement
- [ ] Paramètres minimaux
- [ ] Single responsibility

#### Extract Class
- [ ] Classe > 300 lignes → extraire responsabilité
- [ ] Cohésion élevée dans nouvelle classe
- [ ] Minimal coupling avec classe originale

#### Extract Interface
- [ ] Pour testabilité
- [ ] Pour découplage
- [ ] Interface Segregation Principle

#### Replace Conditional with Polymorphism
- [ ] Switch/if-else complexes → Strategy Pattern
- [ ] Tests pour chaque stratégie

#### Introduce Parameter Object
- [ ] > 3 paramètres → grouper en objet
- [ ] Validation dans l'objet

#### Replace Magic Number with Constant
- [ ] Tous nombres magiques identifiés
- [ ] Constantes nommées explicitement
- [ ] Constantes groupées logiquement

#### Decompose Conditional
- [ ] Conditions complexes → méthodes nommées
- [ ] Intention claire

### Architecture Validation Checklist

#### Dépendances
- [ ] **Domain** → Aucune dépendance externe
- [ ] **Application** → Domain uniquement
- [ ] **Infrastructure** → Application + Domain
- [ ] **Presentation** → Application + Infrastructure (via DI)
- [ ] **Aucune dépendance cyclique** : Validé avec NetArchTest

#### SOLID Validation
- [ ] **SRP** : Chaque classe 1 raison de changer
- [ ] **OCP** : Extensible sans modification
- [ ] **LSP** : Substitution sous-types OK
- [ ] **ISP** : Interfaces spécifiques
- [ ] **DIP** : Dépend abstractions, pas concrétions

#### Patterns Validation
- [ ] **Repository** : Uniquement dans Infrastructure
- [ ] **Services** : Stateless, dans Application
- [ ] **Entities** : Avec comportement, dans Domain
- [ ] **DTOs** : Sans logique, pour transfert
- [ ] **Value Objects** : Immutables, dans Domain

### Performance Validation Checklist

- [ ] **N+1 queries** : Aucune détectée
- [ ] **Indexes** : Sur colonnes filtrées/jointes
- [ ] **Projections** : Uniquement colonnes nécessaires
- [ ] **Pagination** : Sur toutes listes
- [ ] **Caching** : Stratégie définie et implémentée
- [ ] **Async/await** : Utilisé correctement
- [ ] **Allocations** : Minimisées (profiler si critique)
- [ ] **Benchmarks** : Exécutés si changements performance-critical

### Security Validation Checklist

- [ ] **Input validation** : FluentValidation sur tous inputs
- [ ] **SQL injection** : Paramètres utilisés partout
- [ ] **XSS** : Encoding approprié
- [ ] **CSRF** : Tokens validés
- [ ] **Authentication** : Sur endpoints protégés
- [ ] **Authorization** : [Authorize] avec policies/roles
- [ ] **Secrets** : Aucun hardcodé
- [ ] **Sensitive data** : Logs nettoyés
- [ ] **HTTPS** : Forcé en production
- [ ] **CORS** : Configuration restrictive

## Mesures de Succès

### Métriques Quantitatives
- ✅ Code Coverage : 80% → 90%+
- ✅ Mutation Score : Baseline → 85%+
- ✅ Complexité Cyclomatique : < 10 moyenne
- ✅ Code Duplication : < 3%
- ✅ Build Time : Réduction de X%
- ✅ Test Execution Time : < Y minutes
- ✅ Technical Debt Ratio : Réduction de Z%

### Métriques Qualitatives
- ✅ Architecture alignée avec les principes SOLID
- ✅ Séparation claire des concerns
- ✅ Modules réutilisables extraits en packages
- ✅ Documentation complète et à jour
- ✅ Testabilité améliorée
- ✅ Performance optimisée

## Exécution

Procéder méthodiquement et de manière incrémentale :

### Workflow de Refactoring

1. **📊 ANALYSER** (Phase 1)
   - Exécuter les 10+ analyses ciblées
   - Documenter tous les problèmes identifiés
   - Créer rapport d'analyse avec métriques baseline
   - Identifier patterns récurrents

2. **📋 PLANIFIER** (Phase 2)
   - Créer tâches atomiques pour chaque problème
   - Prioriser selon impact et risque
   - Estimer effort et dépendances
   - Créer backlog de refactoring dans manage_todo_list

3. **🎯 EXÉCUTER UNE TÂCHE À LA FOIS** (Phases 3-8)
   
   **Pour chaque tâche :**
   
   a. **Setup**
   - [ ] Créer branche feature: `git checkout -b refactor/{task-name}`
   - [ ] Marquer tâche "in-progress" dans todo list
   - [ ] Capturer métriques baseline
   
   b. **RED - Tests First**
   - [ ] Écrire tests caractérisant comportement actuel
   - [ ] Écrire tests pour comportement attendu
   - [ ] Vérifier que nouveaux tests échouent (RED)
   - [ ] `dotnet test` → au moins 1 test rouge
   
   c. **GREEN - Implementation**
   - [ ] Implémenter refactoring minimal
   - [ ] `dotnet test` → tous tests verts
   - [ ] `dotnet build` → aucune erreur/warning
   
   d. **REFACTOR - Optimize**
   - [ ] Nettoyer code
   - [ ] Éliminer duplication
   - [ ] Appliquer patterns appropriés
   - [ ] `dotnet test` → toujours verts
   
   e. **VALIDATE - Mutation Testing**
   - [ ] `dotnet stryker --config-file stryker-config.json`
   - [ ] Vérifier mutation score ≥ 85%
   - [ ] Améliorer tests si mutants survivent
   - [ ] Re-run jusqu'à score satisfaisant
   
   f. **DOCUMENT**
   - [ ] Mettre à jour documentation
   - [ ] Capturer métriques finales
   - [ ] Comparer avant/après
   
   g. **REVIEW & COMMIT**
   - [ ] Self-review complet
   - [ ] Vérifier checklist complète
   - [ ] Commit atomique avec message descriptif
   - [ ] Push branche
   
   h. **MERGE & CLOSE**
   - [ ] Créer Pull Request
   - [ ] Code review par pairs
   - [ ] CI/CD verte
   - [ ] Merge vers main/develop
   - [ ] Marquer tâche "completed" dans todo list
   - [ ] Supprimer branche feature

4. **🔄 ITÉRER** 
   - Passer à la tâche suivante
   - Répéter workflow pour chaque tâche
   - Faire commits fréquents
   - Garder PRs petites et focalisées

5. **📈 MESURER & REPORTER**
   - Mettre à jour rapport de refactoring
   - Documenter améliorations métriques
   - Partager avec équipe
   - Identifier leçons apprises

### Principes d'Exécution

#### ⚡ Incrémental > Big Bang
- Petits changements fréquents
- PRs ≤ 500 lignes changées idéalement
- Merges fréquents pour éviter conflicts
- Continuous delivery

#### 🎯 Focus > Multitasking
- 1 tâche à la fois
- Finir complètement avant de passer à suivante
- Éviter WIP (Work In Progress) multiples
- Maintenir flow

#### ✅ Validation Continue
- Tests après chaque modification
- Mutation testing après chaque tâche
- CI/CD après chaque push
- Feedback rapide

#### 🚫 Ne Pas Procéder Si
- **Incertitude** : Analyse insuffisante → analyser davantage
- **Tests rouges** : Régression détectée → fixer immédiatement
- **CI/CD rouge** : Build cassé → fixer avant de continuer
- **Mutation score < 85%** : Tests faibles → améliorer tests
- **Code review bloquante** : Feedback non adressé → corriger

#### ✅ Procéder Seulement Si
- **Analyse complète** : Problème bien compris
- **Tests verts** : Aucune régression
- **Mutation score ≥ 85%** : Tests robustes
- **Code review approuvée** : Consensus équipe
- **CI/CD verte** : Automatisation validée
- **Documentation à jour** : Changements documentés

### Gestion des Risques

#### Risques Faibles → Refactoring Direct
- Code avec haute couverture tests
- Composants bien isolés
- Changements cosmétiques (naming, formatting)
- Extraction de constantes

#### Risques Moyens → Refactoring Prudent
- Code avec couverture moyenne
- Changements logique métier
- Refactoring patterns (Extract Class, etc.)
- Double validation (tests + mutation)

#### Risques Élevés → Stratégie de Mitigation
- **Strangler Fig Pattern** : Nouvelle implémentation en parallèle
- **Branch by Abstraction** : Interface stable, implémentations multiples
- **Feature Flags** : Activation progressive
- **Canary Releases** : Déploiement graduel
- **Rollback Plan** : Plan B toujours prêt

### Outils & Commandes Utiles

#### Tests
```bash
# Tous les tests
dotnet test

# Tests spécifiques
dotnet test --filter "FullyQualifiedName~UserService"

# Avec coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Verbose
dotnet test --logger "console;verbosity=detailed"
```

#### Mutation Testing
```bash
# Run Stryker
dotnet stryker --config-file stryker-config.json

# Avec reporters
dotnet stryker --reporter html --reporter json --reporter cleartext

# Verbose
dotnet stryker --verbosity info

# Baseline (première run)
dotnet stryker --dashboard-api-key <key> --version baseline
```

#### Code Analysis
```bash
# Build avec warnings as errors
dotnet build /warnaserror

# Code metrics
dotnet-coverage collect -f xml -o coverage.xml dotnet test

# Analyzers
dotnet build /p:EnforceCodeStyleInBuild=true
```

#### Benchmarks
```bash
cd tests/ADP.{Component}.Benchmarks
dotnet run -c Release
```

### Templates de Documentation

#### Rapport d'Analyse
```markdown
# Refactoring Analysis Report - {Date}

## Summary
- **Files analyzed**: X
- **Issues found**: Y
- **Priority distribution**: Critical: A, High: B, Medium: C, Low: D

## Metrics Baseline
- **Code Coverage**: X%
- **Mutation Score**: Y%
- **Average Complexity**: Z
- **Technical Debt Ratio**: W%

## Issues by Category
### 1. Code Smells (N issues)
- Issue 1: Description
- Issue 2: Description

### 2. SOLID Violations (N issues)
[...]

## Refactoring Plan
1. Task 1 (Priority: Critical)
2. Task 2 (Priority: High)
[...]
```

#### ADR (Architecture Decision Record)
```markdown
# ADR {Number}: {Title}

**Date**: {Date}
**Status**: Proposed | Accepted | Deprecated | Superseded

## Context
{Describe the forces at play, including technological, political, 
social, and project local. These forces are likely in tension.}

## Decision
{Describe our response to these forces, i.e., the decision.}

## Consequences
{Describe the resulting context, after applying the decision. 
All consequences should be listed here, not just the positive ones.}

### Positive
- Benefit 1
- Benefit 2

### Negative
- Cost 1
- Cost 2

### Neutral
- Trade-off 1
```

#### Commit Message Template
```
{type}({scope}): {short description}

{detailed description}

- Change 1
- Change 2
- Change 3

Metrics:
- Coverage: {before}% → {after}%
- Mutation: {before}% → {after}%
- Complexity: {before} → {after}

{Breaking changes if any}

Closes #{issue}
```

Types: `refactor`, `feat`, `fix`, `docs`, `test`, `perf`, `chore`

## Livrables Attendus

### 1. Code Refactoré de Haute Qualité
- ✅ **Modulaire** : Composants indépendants et réutilisables
- ✅ **Testable** : 80%+ coverage, 85%+ mutation score
- ✅ **Performant** : Optimisations mesurables appliquées
- ✅ **Maintenable** : SOLID, Clean Code, patterns appropriés
- ✅ **Sécurisé** : Validation complète, pas de vulnérabilités
- ✅ **Scalable** : Stateless, idempotent, resilient

### 2. Tests Robustes et Complets
- ✅ **Unit Tests** : xUnit, NSubstitute, NFluent, Bogus
  - Coverage ≥ 80% (idéalement 90%+)
  - Tous les paths critiques couverts
  - AAA pattern (Arrange-Act-Assert)
  - Tests nommés descriptifs
  
- ✅ **Integration Tests** : TestContainers, WebApplicationFactory
  - Scénarios end-to-end
  - Base de données réelle (PostgreSQL via TestContainers)
  - APIs testées avec client HTTP
  
- ✅ **Acceptance Tests** : SpecFlow (optionnel)
  - Scénarios métier en Gherkin
  - Tests BDD lisibles par non-techniques
  
- ✅ **Architecture Tests** : NetArchTest.Rules
  - Validation dépendances layers
  - Enforcement règles architecturales
  
- ✅ **Performance Tests** : BenchmarkDotNet
  - Benchmarks opérations critiques
  - Comparaison avant/après
  
- ✅ **Mutation Tests** : Stryker.NET
  - Mutation score ≥ 85%
  - Rapports HTML générés
  - Tests résistants aux mutations

### 3. Packages NuGet Réutilisables
Composants extraits et packagés :
- ✅ **ADP.Common.Middleware** : Middlewares réutilisables
- ✅ **ADP.Common.Extensions** : Extensions methods
- ✅ **ADP.Common.Data** : Data access abstractions
- ✅ **ADP.Common.Auth** : Authentication/Authorization
- ✅ **ADP.Common.Logging** : Logging configuration
- ✅ **ADP.Common.Caching** : Caching services
- ✅ **ADP.Common.Testing** : Testing utilities
- ✅ **ADP.Common.Api** : API common components
- ✅ **ADP.Common.Blazor** : Blazor common components

Chaque package avec :
- README complet
- CHANGELOG (SemVer)
- Tests (≥ 80% coverage, ≥ 85% mutation)
- Samples/exemples
- Documentation API (XML + DocFx)

### 4. Documentation Complète

#### README Principal
```markdown
# ADP Demo Solution

## Architecture Overview
[Diagram C4 - Context, Container, Component, Code]

## Project Structure
[Description organisation projets]

## Getting Started
[Setup instructions]

## Development
[Development workflow]

## Testing Strategy
[Testing approach et tools]

## Deployment
[Deployment instructions]
```

#### Architecture Decision Records (ADRs)
```
documentations/technical/adr/
├── 0001-record-architecture-decisions.md
├── 0002-use-vertical-slice-architecture.md
├── 0003-extract-common-packages.md
├── 0004-implement-cqrs-with-mediatr.md
└── ...
```

#### API Documentation
- Swagger/OpenAPI specs
- Endpoint documentation
- Authentication guide
- Error handling guide
- Rate limiting policy

#### Guides Techniques
```
documentations/technical/
├── architecture/
│   ├── overview.md
│   ├── layers.md
│   └── patterns.md
├── development/
│   ├── getting-started.md
│   ├── coding-standards.md
│   ├── testing-guide.md
│   └── contributing.md
├── deployment/
│   ├── local.md
│   ├── staging.md
│   └── production.md
└── operations/
    ├── monitoring.md
    ├── troubleshooting.md
    └── performance-tuning.md
```

### 5. Métriques et Rapports

#### Rapport de Refactoring Final
```markdown
# Refactoring Report - {Date Range}

## Executive Summary
- **Total tasks completed**: X
- **Files refactored**: Y
- **Lines of code**: Before: A → After: B (Δ: C)
- **Test files**: Before: D → After: E
- **Time invested**: F hours

## Quality Metrics Improvement

### Code Coverage
- **Before**: 65%
- **After**: 88%
- **Improvement**: +23%
- **Target**: 80% ✅ ACHIEVED

### Mutation Score
- **Before**: 72%
- **After**: 89%
- **Improvement**: +17%
- **Target**: 85% ✅ ACHIEVED

### Complexity
- **Before**: Avg 8.5, Max 45
- **After**: Avg 4.2, Max 9
- **Improvement**: -51% avg, -80% max
- **Target**: Avg < 10, Max < 15 ✅ ACHIEVED

### Technical Debt
- **Before**: 12 hours
- **After**: 3 hours
- **Improvement**: -75%
- **Target**: < 5 hours ✅ ACHIEVED

### Code Duplication
- **Before**: 8.5%
- **After**: 1.2%
- **Improvement**: -86%
- **Target**: < 3% ✅ ACHIEVED

## Performance Improvements
- **API Response Time**: -35% (p95: 450ms → 295ms)
- **Database Queries**: -60% (N+1 eliminated)
- **Memory Allocation**: -40% (via Span<T>, ArrayPool)
- **Build Time**: -25% (improved dependencies)

## Security Enhancements
- ✅ Input validation: 100% endpoints
- ✅ SQL injection: 0 vulnerabilities
- ✅ Authentication: EntraID on all protected endpoints
- ✅ Authorization: Fine-grained policies
- ✅ Secrets: 0 hardcoded (migrated to User Secrets)
- ✅ HTTPS: Enforced
- ✅ CORS: Restrictive policy

## Packages Extracted
1. ADP.Common.Middleware (v1.0.0)
2. ADP.Common.Extensions (v1.0.0)
3. ADP.Common.Data (v1.0.0)
[...]

## Architecture Improvements
- ✅ Vertical Slice Architecture implemented
- ✅ CQRS with MediatR
- ✅ Clean separation of concerns
- ✅ No circular dependencies
- ✅ Domain-driven design patterns

## Lessons Learned
### What Went Well
- TDD approach ensured no regressions
- Incremental refactoring kept PRs small
- Mutation testing caught weak tests early

### Challenges
- Some legacy code required extensive rewrite
- Balancing refactoring with feature delivery

### Recommendations
- Continue TDD practice
- Schedule regular refactoring sprints
- Automate mutation testing in CI/CD
```

### 6. CI/CD Pipelines Optimisés

#### Pipeline .NET (GitHub Actions / Azure Pipelines)
```yaml
name: .NET CI/CD

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage"
    
    - name: Upload Coverage
      uses: codecov/codecov-action@v3
      with:
        files: coverage.opencover.xml
    
    - name: Mutation Testing
      run: |
        dotnet tool install -g dotnet-stryker
        dotnet stryker --config-file stryker-config.json --reporter dashboard
      env:
        STRYKER_DASHBOARD_API_KEY: ${{ secrets.STRYKER_API_KEY }}
    
    - name: SonarCloud Scan
      run: |
        dotnet sonarscanner begin /k:"project-key" /o:"org" /d:sonar.login="${{ secrets.SONAR_TOKEN }}"
        dotnet build
        dotnet sonarscanner end /d:sonar.login="${{ secrets.SONAR_TOKEN }}"
```

### 7. Standards et Guidelines

#### .editorconfig
```ini
root = true

[*.cs]
# Indentation
indent_style = space
indent_size = 4

# Naming conventions
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.severity = warning
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.symbols = interface
dotnet_naming_rule.interfaces_should_be_prefixed_with_i.style = begins_with_i

# Code style rules
csharp_style_var_for_built_in_types = false:warning
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_prefer_braces = true:warning

# Nullable reference types
nullable = enable
```

#### Analyzers Configuration
```xml
<PropertyGroup>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <AnalysisLevel>latest-all</AnalysisLevel>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.435">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
  <PackageReference Include="Roslynator.Analyzers" Version="4.6.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
  <PackageReference Include="SonarAnalyzer.CSharp" Version="9.12.0.78982">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

---

## Ressources & Références

### Livres Recommandés
- **Refactoring** - Martin Fowler
- **Clean Code** - Robert C. Martin
- **Clean Architecture** - Robert C. Martin
- **Domain-Driven Design** - Eric Evans
- **Implementing Domain-Driven Design** - Vaughn Vernon
- **Patterns of Enterprise Application Architecture** - Martin Fowler

### Patterns & Practices
- [Refactoring Catalog](https://refactoring.com/catalog/)
- [Microsoft Architecture Guides](https://docs.microsoft.com/en-us/dotnet/architecture/)
- [C4 Model](https://c4model.com/)
- [ADR GitHub Organization](https://adr.github.io/)

### Tools Documentation
- [xUnit](https://xunit.net/)
- [NSubstitute](https://nsubstitute.github.io/)
- [NFluent](https://www.n-fluent.net/)
- [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [NetArchTest](https://github.com/BenMorris/NetArchTest)
- [TestContainers](https://dotnet.testcontainers.org/)

---

## ⚠️ IMPORTANT - Règles d'Or

### ✅ TOUJOURS
1. **Écrire les tests AVANT le refactoring** (TDD strict)
2. **Commits atomiques et fréquents** (sauvegarder progression)
3. **Valider avec mutation testing** (≥ 85% obligatoire)
4. **Documenter les décisions** (ADRs pour choix architecturaux)
5. **Mesurer avant/après** (métriques objectives)
6. **Code review systématique** (pair review)
7. **Respecter SOLID** (sans compromis)
8. **Rester pragmatique** (ROI > perfectionnisme)

### ❌ JAMAIS
1. **Refactorer sans tests** (risque de régression)
2. **Big Bang refactoring** (changements massifs)
3. **Ignorer les tests rouges** (fixer immédiatement)
4. **Commiter du code non testé** (qualité d'abord)
5. **Sauter le mutation testing** (tests faibles)
6. **Merge sans code review** (qualité collective)
7. **Sacrifier qualité pour vitesse** (dette technique)
8. **Refactorer ET ajouter features** (1 chose à la fois)

### 🎯 OBJECTIFS QUANTIFIABLES
- **Code Coverage**: ≥ 80% (idéal: 90%+)
- **Mutation Score**: ≥ 85% (idéal: 90%+)
- **Cyclomatic Complexity**: < 10 (max: 15)
- **Maintainability Index**: > 80
- **Code Duplication**: < 3%
- **Technical Debt Ratio**: < 5%
- **Build Time**: Amélioration mesurable
- **Test Execution Time**: < 5 minutes pour tous tests

---

## 🚀 Commencer le Refactoring

**Êtes-vous prêt ?**

✅ J'ai lu et compris la méthodologie complète  
✅ J'ai les outils nécessaires installés  
✅ J'ai accès au code source et droits commit  
✅ Je m'engage à suivre le processus TDD  
✅ Je vais procéder de manière méthodique et incrémentale  
✅ Je ne procéderai qu'avec certitude, sinon j'analyserai davantage  

**Alors, commençons ! 🎯**

**Prochaine étape : Phase 1 - Analyse Approfondie (10+ analyses ciblées)**
