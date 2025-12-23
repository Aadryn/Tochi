---
description: C# Fundamentals - ADR compliance, solution structure, project organization, Clean Architecture
name: CSharp_Fundamentals
applyTo: "**/backend/**/*.cs,**/*.csproj"
---

# C# - Règles Fondamentales

Guide des principes fondamentaux pour le développement C# et .NET.

## � Types de Fichiers à Créer

| Type de fichier | Usage | Nomenclature |
|----------------|-------|-------------|
| `*Service.cs` | Services métier avec logique asynchrone | `[Domain]Service.cs` (ex: `UserService.cs`, `TenantService.cs`) |
| `*Handler.cs` | Handlers CQRS pour commandes/queries | `[Action][Entity]Handler.cs` (ex: `CreateUserHandler.cs`) |
| `*Repository.cs` | Accès aux données, implémentation Repository Pattern | `[Entity]Repository.cs` (ex: `UserRepository.cs`) |
| `*Controller.cs` | Contrôleurs WebApi REST | `[Resource]Controller.cs` (ex: `UsersController.cs`) |
| `*Endpoint.cs` | Endpoints Minimal API | `[Action][Resource]Endpoint.cs` (ex: `CreateUserEndpoint.cs`) |
| `*Provider.cs` | Fournisseurs de services/configuration | `[Feature]Provider.cs` (ex: `CacheProvider.cs`) |
| `*Tests.cs` | Tests unitaires xUnit | `[ClassTested]Tests.cs` (ex: `UserServiceTests.cs`) |
| `*Validator.cs` | Validateurs FluentValidation | `[Command]Validator.cs` (ex: `CreateUserCommandValidator.cs`) |

## ⛔ À NE PAS FAIRE

- **Ne génère jamais** de code sans avoir lu les ADR dans `docs/adr/`
- **Ne viole jamais** un ADR sans justification documentée
- **Ne crée jamais** plusieurs types dans un même fichier (ADR-001)
- **N'ajoute jamais** de fonctionnalité non demandée (YAGNI - ADR-004)
- **Ne duplique jamais** de code (DRY - ADR-003)
- **N'introduis jamais** de dépendance circulaire entre couches
- **Ne place jamais** de logique métier dans la couche Presentation

## ✅ À FAIRE

- **Consulte toujours** les ADR avant de coder (surtout ADR-001 à ADR-025)
- **Respecte toujours** l'architecture en couches (Domain → Application → Infrastructure → Presentation)
- **Crée toujours** un fichier par type (classe, interface, enum, record)
- **Applique toujours** les principes SOLID (ADR-005)
- **Utilise toujours** l'injection de dépendances (ADR-014)
- **Utilise toujours** le Result Pattern pour les erreurs métier (ADR-023)
- **Documente toujours** les décisions architecturales avec un nouvel ADR

## 🎯 Actions Obligatoires (Mandatory)

### ⚠️ LECTURE ADR OBLIGATOIRE

**AVANT de générer du code C#, TOUJOURS lire les ADR applicables dans `docs/adr/` :**

1. ✅ **Consulter les ADR architecturaux** :
   - [001-un-seul-type-par-fichier-csharp.adr.md](../../docs/adr/001-un-seul-type-par-fichier-csharp.adr.md) - Un seul type par fichier
   - [002-principe-kiss.adr.md](../../docs/adr/002-principe-kiss.adr.md) - Keep It Simple, Stupid
   - [003-principe-dry.adr.md](../../docs/adr/003-principe-dry.adr.md) - Don't Repeat Yourself
   - [004-principe-yagni.adr.md](../../docs/adr/004-principe-yagni.adr.md) - You Ain't Gonna Need It
   - [005-principes-solid.adr.md](../../docs/adr/005-principes-solid.adr.md) - Principes SOLID
   - [006-onion-architecture.adr.md](../../docs/adr/006-onion-architecture.adr.md) - Onion Architecture
   - [014-dependency-injection.adr.md](../../docs/adr/014-dependency-injection.adr.md) - Dependency Injection
   - [017-repository-pattern.adr.md](../../docs/adr/017-repository-pattern.adr.md) - Repository Pattern
   - [023-result-pattern.adr.md](../../docs/adr/023-result-pattern.adr.md) - Result Pattern

2. ✅ **Vérifier les ADR spécifiques au projet** avant toute implémentation

3. ✅ **Respecter les décisions documentées** - Ne jamais contourner un ADR sans justification

## 📁 Structure de Solution OBLIGATOIRE

### Clean Architecture / Onion Architecture

```
[Solution].sln
│
├── src/
│   ├── Core/                              # Couche centrale (pas de dépendances)
│   │   ├── [Solution].Domain/             # Entités, Value Objects, Domain Events
│   │   │   ├── Entities/
│   │   │   │   └── [Entity].cs
│   │   │   ├── ValueObjects/
│   │   │   │   └── [ValueObject].cs
│   │   │   ├── Events/
│   │   │   │   └── [DomainEvent].cs
│   │   │   ├── Enums/
│   │   │   │   └── [Enum].cs
│   │   │   ├── Exceptions/
│   │   │   │   └── [DomainException].cs
│   │   │   └── Interfaces/
│   │   │       └── I[Repository].cs
│   │   │
│   │   └── [Solution].Application/        # Cas d'utilisation, CQRS
│   │       ├── Common/
│   │       │   ├── Behaviors/             # Pipeline behaviors (validation, logging)
│   │       │   ├── Interfaces/
│   │       │   └── Models/
│   │       ├── Features/
│   │       │   └── [Feature]/
│   │       │       ├── Commands/
│   │       │       │   ├── Create[Entity]/
│   │       │       │   │   ├── Create[Entity]Command.cs
│   │       │       │   │   ├── Create[Entity]CommandHandler.cs
│   │       │       │   │   └── Create[Entity]CommandValidator.cs
│   │       │       │   └── Update[Entity]/
│   │       │       └── Queries/
│   │       │           └── Get[Entity]/
│   │       │               ├── Get[Entity]Query.cs
│   │       │               ├── Get[Entity]QueryHandler.cs
│   │       │               └── [Entity]Dto.cs
│   │       ├── Mappings/
│   │       │   └── [Entity]MappingProfile.cs
│   │       └── DependencyInjection.cs
│   │
│   ├── Infrastructure/                    # Implémentations externes
│   │   ├── [Solution].Infrastructure/     # Infrastructure commune
│   │   │   ├── Persistence/
│   │   │   │   ├── Configurations/        # EF Core configurations
│   │   │   │   ├── Repositories/
│   │   │   │   ├── ApplicationDbContext.cs
│   │   │   │   └── UnitOfWork.cs
│   │   │   ├── Services/
│   │   │   │   └── [ExternalService].cs
│   │   │   └── DependencyInjection.cs
│   │   │
│   │   ├── [Solution].Infrastructure.Redis/
│   │   ├── [Solution].Infrastructure.MessageBus/
│   │   └── [Solution].Infrastructure.Storage/
│   │
│   └── Presentation/                      # Points d'entrée
│       ├── [Solution].WebApi/             # API REST
│       │   ├── Controllers/
│       │   │   └── [Feature]Controller.cs
│       │   ├── Filters/
│       │   ├── Middlewares/
│       │   ├── appsettings.json
│       │   └── Program.cs
│       │
│       ├── [Solution].WebApp/             # Application Blazor/MVC
│       │   ├── Components/
│       │   ├── Pages/
│       │   └── Program.cs
│       │
│       └── [Solution].Worker/             # Background services
│           └── Program.cs
│
├── tests/
│   ├── [Solution].Domain.Tests/           # Tests unitaires Domain
│   ├── [Solution].Application.Tests/      # Tests unitaires Application
│   ├── [Solution].Infrastructure.Tests/   # Tests d'intégration
│   └── [Solution].WebApi.Tests/           # Tests API
│
├── docs/
│   └── adr/                               # Architecture Decision Records
│
└── Directory.Build.props                  # Propriétés MSBuild partagées
```

### Structure d'un Projet

```
[Solution].[Layer]/
├── [Solution].[Layer].csproj              # Fichier projet
├── GlobalUsings.cs                        # Global usings
├── DependencyInjection.cs                 # Extension pour DI
│
├── [Folder]/                              # Organisation par fonctionnalité
│   └── [Class].cs                         # UN SEUL type par fichier (ADR-001)
│
└── README.md                              # Documentation du projet (optionnel)
```

## 📝 Structure des Fichiers

### Un Type Par Fichier (ADR-001)

```csharp
// ✅ BON : Un seul type par fichier
// Fichier: User.cs
namespace MyApp.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    // ...
}

// Fichier: UserCreatedEvent.cs
namespace MyApp.Domain.Events;

public record UserCreatedEvent(Guid UserId, string Email) : IDomainEvent;

// ❌ MAUVAIS : Plusieurs types dans le même fichier
// public class User { }
// public class UserRole { }
// public record UserCreatedEvent { }
```

### Organisation des Namespaces

```csharp
// ✅ BON : Namespace = chemin du dossier
// Fichier: src/Core/MyApp.Domain/Entities/User.cs
namespace MyApp.Domain.Entities;

// Fichier: src/Core/MyApp.Application/Features/Users/Commands/CreateUser/CreateUserCommand.cs
namespace MyApp.Application.Features.Users.Commands.CreateUser;
```

## 🏗️ Patterns Architecturaux

### Entity (Domain)

```csharp
// Domain/Entities/User.cs
namespace MyApp.Domain.Entities;

/// <summary>
/// Entité représentant un utilisateur du système.
/// </summary>
public class User : BaseEntity, IAggregateRoot
{
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; }

    private readonly List<UserRole> _roles = [];
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    // Constructeur privé pour EF Core
    private User() { }

    // Factory method
    public static User Create(string email, string firstName, string lastName)
    {
        Guard.Against.NullOrEmpty(email, nameof(email));
        Guard.Against.InvalidEmail(email, nameof(email));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Status = UserStatus.Pending
        };

        user.AddDomainEvent(new UserCreatedEvent(user.Id, email));

        return user;
    }

    public void Activate()
    {
        if (Status != UserStatus.Pending)
            throw new InvalidOperationException("User must be pending to activate");

        Status = UserStatus.Active;
        AddDomainEvent(new UserActivatedEvent(Id));
    }

    public void AddRole(UserRole role)
    {
        Guard.Against.Null(role, nameof(role));
        
        if (!_roles.Contains(role))
        {
            _roles.Add(role);
        }
    }
}
```

### Command (CQRS)

```csharp
// Application/Features/Users/Commands/CreateUser/CreateUserCommand.cs
namespace MyApp.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Commande pour créer un nouvel utilisateur.
/// </summary>
public record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName
) : IRequest<Result<Guid>>;
```

### Command Handler

```csharp
// Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs
namespace MyApp.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Handler pour la création d'un utilisateur.
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        // Vérifier l'unicité de l'email
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            return Result.Failure<Guid>(UserErrors.EmailAlreadyExists);
        }

        // Créer l'utilisateur
        var user = User.Create(
            request.Email,
            request.FirstName,
            request.LastName);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
```

### Repository Interface (Domain)

```csharp
// Domain/Interfaces/IUserRepository.cs
namespace MyApp.Domain.Interfaces;

/// <summary>
/// Interface du repository pour les utilisateurs.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    void Delete(User user);
}
```

### Repository Implementation (Infrastructure)

```csharp
// Infrastructure/Persistence/Repositories/UserRepository.cs
namespace MyApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implémentation du repository utilisateur avec EF Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }
}
```

### Controller (Presentation)

```csharp
// WebApi/Controllers/UsersController.cs
namespace MyApp.WebApi.Controllers;

/// <summary>
/// Contrôleur API pour la gestion des utilisateurs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Crée un nouvel utilisateur.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(
            request.Email,
            request.FirstName,
            request.LastName);

        var result = await _sender.Send(command, cancellationToken);

        return result.Match(
            success => CreatedAtAction(nameof(GetById), new { id = success }, success),
            failure => Problem(detail: failure.Message, statusCode: 400));
    }

    /// <summary>
    /// Récupère un utilisateur par son identifiant.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success),
            failure => NotFound());
    }
}
```

## 🔌 Dependency Injection

```csharp
// Application/DependencyInjection.cs
namespace MyApp.Application;

/// <summary>
/// Configuration de l'injection de dépendances pour la couche Application.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(assembly);

        return services;
    }
}

// Infrastructure/DependencyInjection.cs
namespace MyApp.Infrastructure;

/// <summary>
/// Configuration de l'injection de dépendances pour la couche Infrastructure.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
```

## ✅ Checklist C#

**Avant de compléter du code C#, VÉRIFIER :**

- [ ] ADR pertinents consultés et respectés
- [ ] Un seul type par fichier (ADR-001)
- [ ] Structure Clean Architecture respectée
- [ ] Namespaces = chemin des dossiers
- [ ] SOLID respecté (ADR-005)
- [ ] DI configurée correctement (ADR-014)
- [ ] Result Pattern pour les erreurs (ADR-023)
- [ ] Entities avec comportements (pas anémiques)
- [ ] Repositories avec interfaces dans Domain
- [ ] Documentation XML sur les membres publics
- [ ] Tests unitaires pour la logique métier
