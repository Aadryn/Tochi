# Application Layer - CQRS Pattern

## Vue d'ensemble

Cette couche implémente le pattern **CQRS (Command Query Responsibility Segregation)** tel que défini dans [ADR-013](../../docs/adr/013-cqrs.adr.md). Le pattern sépare les opérations d'écriture (Commands) des opérations de lecture (Queries) pour optimiser la performance, la scalabilité et la maintenabilité.

## Architecture CQRS

```
┌────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│             (Controllers → IMediator.Send)                  │
└──────────────────┬─────────────────────────────────────────┘
                   │
                   ▼
┌────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│                                                              │
│   ┌──────────────────┐         ┌─────────────────────┐    │
│   │   WRITE SIDE     │         │     READ SIDE       │    │
│   │   (Commands)     │         │     (Queries)       │    │
│   ├──────────────────┤         ├─────────────────────┤    │
│   │ ICommand<T>      │         │ IQuery<T>           │    │
│   │ ICommandHandler  │         │ IQueryHandler       │    │
│   │                  │         │                     │    │
│   │ Validation       │         │ Projections         │    │
│   │ Business Rules   │         │ Read-only           │    │
│   │ Side Effects     │         │ Optimized           │    │
│   └──────┬───────────┘         └─────────┬───────────┘    │
│          │                               │                 │
└──────────┼───────────────────────────────┼─────────────────┘
           │                               │
           ▼                               ▼
┌────────────────────────────────────────────────────────────┐
│                     Domain Layer                            │
│         (Entities, Value Objects, Result<T>)               │
└──────────────────┬─────────────────────────────────────────┘
                   │
                   ▼
┌────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                        │
│         (Repositories, UnitOfWork, EF Core)                │
└────────────────────────────────────────────────────────────┘
```

## Principes fondamentaux

### 1. Séparation stricte Write/Read

**Commands (Write Side)** :
- Modifient l'état du système
- Retournent `Result` ou `Result<T>`
- Appliquent les règles métier
- Déclenchent des effets de bord
- Utilisent les repositories pour persister

**Queries (Read Side)** :
- Lisent l'état sans le modifier
- Retournent `Result<T>` avec DTOs
- Optimisées avec projections SQL
- Ne déclenchent jamais d'effets de bord
- Utilisent `AsNoTracking()` pour performance

### 2. Railway-Oriented Programming

Toutes les opérations utilisent `Result<T>` pour une gestion explicite des erreurs :

```csharp
// Command Handler
var result = await _mediator.Send(new CreateUserCommand {...});
if (result.IsFailure)
    return BadRequest(result.Error);

var user = result.Value;
return Ok(user);
```

### 3. MediatR comme pipeline

MediatR dispatch les Commands/Queries vers leurs handlers :

```csharp
// Dans un controller
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        // ...
    }
}
```

## Structure des fichiers

```
Application/
├── README.md                    # Cette documentation
├── Common/
│   ├── ICommand.cs             # Interface Command sans retour
│   ├── ICommand<T>.cs          # Interface Command avec retour
│   ├── ICommandHandler.cs      # Handler pour ICommand
│   ├── ICommandHandler<T>.cs   # Handler pour ICommand<T>
│   ├── IQuery<T>.cs            # Interface Query
│   ├── IQueryHandler<T>.cs     # Handler pour IQuery<T>
│   ├── BaseDto.cs              # Classe de base pour DTOs
│   └── *Dto.cs                 # DTOs de transfert
│
├── Users/
│   ├── Commands/
│   │   ├── CreateUserCommand.cs
│   │   ├── CreateUserCommandHandler.cs
│   │   ├── UpdateUserCommand.cs
│   │   ├── UpdateUserCommandHandler.cs
│   │   ├── DeleteUserCommand.cs
│   │   └── DeleteUserCommandHandler.cs
│   └── Queries/
│       ├── GetUserByIdQuery.cs
│       ├── GetUserByIdQueryHandler.cs
│       ├── GetUsersByTenantIdQuery.cs
│       └── GetUsersByTenantIdQueryHandler.cs
│
├── Tenants/
│   ├── Commands/
│   └── Queries/
│
├── ApiKeys/
│   ├── Commands/
│   └── Queries/
│
└── LLMProviders/
    ├── Commands/
    └── Queries/
```

## Guide d'utilisation

### Créer une nouvelle Command

**1. Définir la Command :**

```csharp
using LLMProxy.Application.Common;

namespace LLMProxy.Application.Users.Commands;

/// <summary>
/// Commande pour activer un utilisateur.
/// </summary>
public record ActivateUserCommand : ICommand<UserDto>
{
    /// <summary>
    /// Identifiant de l'utilisateur à activer.
    /// </summary>
    public Guid UserId { get; init; }
}
```

**2. Créer le Handler :**

```csharp
using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;

namespace LLMProxy.Application.Users.Commands;

/// <summary>
/// Gestionnaire pour la commande ActivateUserCommand.
/// </summary>
public class ActivateUserCommandHandler : ICommandHandler<ActivateUserCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(
        ActivateUserCommand request, 
        CancellationToken ct)
    {
        // 1. Récupérer l'entité
        var userResult = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (userResult.IsFailure)
            return Result<UserDto>.Failure(userResult.Error);

        var user = userResult.Value;

        // 2. Appliquer la logique métier (Domain)
        var activateResult = user.Activate();
        if (activateResult.IsFailure)
            return Result<UserDto>.Failure(activateResult.Error);

        // 3. Persister via Repository
        var updateResult = await _unitOfWork.Users.UpdateAsync(user, ct);
        if (updateResult.IsFailure)
            return Result<UserDto>.Failure(updateResult.Error);

        await _unitOfWork.SaveChangesAsync(ct);

        // 4. Mapper vers DTO et retourner
        var dto = MapToDto(updateResult.Value);
        return dto; // Implicit conversion to Result<UserDto>
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        TenantId = user.TenantId,
        Email = user.Email,
        Name = user.Name,
        Role = user.Role.ToString(),
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt ?? DateTime.MinValue
    };
}
```

**3. Utiliser dans un Controller :**

```csharp
[HttpPost("{id}/activate")]
public async Task<IActionResult> Activate(Guid id)
{
    var command = new ActivateUserCommand { UserId = id };
    var result = await _mediator.Send(command);
    
    if (result.IsFailure)
        return BadRequest(new { error = result.Error.Message });
    
    return Ok(result.Value);
}
```

### Créer une nouvelle Query

**1. Définir la Query :**

```csharp
using LLMProxy.Application.Common;

namespace LLMProxy.Application.Users.Queries;

/// <summary>
/// Requête pour obtenir les utilisateurs actifs d'un tenant.
/// </summary>
public record GetActiveUsersByTenantIdQuery : IQuery<IEnumerable<UserDto>>
{
    /// <summary>
    /// Identifiant du tenant.
    /// </summary>
    public Guid TenantId { get; init; }
}
```

**2. Créer le Handler avec projection optimisée :**

```csharp
using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Application.Users.Queries;

/// <summary>
/// Gestionnaire pour GetActiveUsersByTenantIdQuery.
/// </summary>
public class GetActiveUsersByTenantIdQueryHandler 
    : IQueryHandler<GetActiveUsersByTenantIdQuery, IEnumerable<UserDto>>
{
    private readonly LLMProxyDbContext _context;

    public GetActiveUsersByTenantIdQueryHandler(LLMProxyDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(
        GetActiveUsersByTenantIdQuery request, 
        CancellationToken ct)
    {
        try
        {
            // ✅ PROJECTION SQL DIRECTE - Optimal
            var dtos = await _context.Users
                .AsNoTracking() // Read-only, pas de tracking
                .Where(u => u.TenantId == request.TenantId && u.IsActive)
                .OrderBy(u => u.Name)
                .Select(u => new UserDto // Projection au niveau SQL
                {
                    Id = u.Id,
                    TenantId = u.TenantId,
                    Email = u.Email,
                    Name = u.Name,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt ?? DateTime.MinValue
                })
                .ToListAsync(ct);

            return Result<IEnumerable<UserDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<UserDto>>.Failure(
                new Error("Query.Failed", $"Erreur lors de la requête: {ex.Message}"));
        }
    }
}
```

**3. Utiliser dans un Controller :**

```csharp
[HttpGet("tenants/{tenantId}/users/active")]
public async Task<IActionResult> GetActiveUsers(Guid tenantId)
{
    var query = new GetActiveUsersByTenantIdQuery { TenantId = tenantId };
    var result = await _mediator.Send(query);
    
    if (result.IsFailure)
        return BadRequest(new { error = result.Error.Message });
    
    return Ok(result.Value);
}
```

## Optimisation des Queries

### Projection SQL directe (Recommandé)

✅ **Bon - Projection au niveau SQL :**

```csharp
var dtos = await _context.Users
    .AsNoTracking()
    .Where(u => u.TenantId == tenantId)
    .Select(u => new UserDto // SQL: SELECT Id, Email, Name...
    {
        Id = u.Id,
        Email = u.Email,
        Name = u.Name
    })
    .ToListAsync(ct);
```

**Avantages :**
- SQL génère uniquement les colonnes nécessaires
- Pas de tracking EF Core (performance)
- Pas de matérialisation d'entités complètes
- Moins de mémoire utilisée

❌ **Mauvais - Charger entité puis mapper :**

```csharp
var users = await _unitOfWork.Users.GetByTenantIdAsync(tenantId, ct);
var dtos = users.Value.Select(u => new UserDto 
{
    Id = u.Id,
    Email = u.Email,
    Name = u.Name
});
```

**Problèmes :**
- Charge TOUTES les colonnes de l'entité
- EF Core track les entités (overhead)
- Mapping en mémoire après lecture
- Plus de données transférées depuis la DB

### AsNoTracking pour lecture seule

```csharp
// ✅ Toujours utiliser AsNoTracking() dans les Queries
var dtos = await _context.Users
    .AsNoTracking() // Désactive le change tracking
    .Where(...)
    .Select(...)
    .ToListAsync(ct);
```

### Include pour éviter N+1

Si vous devez charger des relations (moins recommandé, préférer les projections) :

```csharp
var apiKeys = await _context.ApiKeys
    .AsNoTracking()
    .Include(k => k.User)       // 1 requête avec JOIN
    .Include(k => k.Tenant)     // au lieu de N+1
    .Where(k => k.UserId == userId)
    .ToListAsync(ct);
```

### Pagination

```csharp
public record GetUsersPaginatedQuery : IQuery<PagedResult<UserDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

// Handler
var totalCount = await _context.Users.CountAsync(ct);

var dtos = await _context.Users
    .AsNoTracking()
    .OrderBy(u => u.Name)
    .Skip((request.Page - 1) * request.PageSize)
    .Take(request.PageSize)
    .Select(u => new UserDto {...})
    .ToListAsync(ct);

return new PagedResult<UserDto>
{
    Items = dtos,
    TotalCount = totalCount,
    Page = request.Page,
    PageSize = request.PageSize
};
```

## Validation des Commands

Les Commands doivent valider leurs données avant d'appeler le Domain :

```csharp
public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken ct)
{
    // 1. Validation des données d'entrée
    if (string.IsNullOrWhiteSpace(request.Email))
        return Error.Validation.Required(nameof(request.Email));
    
    if (string.IsNullOrWhiteSpace(request.Name))
        return Error.Validation.Required(nameof(request.Name));
    
    // 2. Vérification des contraintes métier
    var emailExistsResult = await _unitOfWork.Users.EmailExistsAsync(
        request.Email, 
        request.TenantId, 
        ct);
    
    if (emailExistsResult.IsFailure)
        return Result<UserDto>.Failure(emailExistsResult.Error);
    
    if (emailExistsResult.Value)
        return Error.User.EmailAlreadyExists(request.Email);
    
    // 3. Délégation au Domain pour création
    var createResult = User.Create(
        request.TenantId,
        request.Email,
        request.Name,
        request.Role);
    
    if (createResult.IsFailure)
        return Result<UserDto>.Failure(createResult.Error);
    
    // 4. Persistance
    var user = createResult.Value;
    var addResult = await _unitOfWork.Users.AddAsync(user, ct);
    if (addResult.IsFailure)
        return Result<UserDto>.Failure(addResult.Error);
    
    await _unitOfWork.SaveChangesAsync(ct);
    
    // 5. Retour DTO
    return MapToDto(addResult.Value);
}
```

## Tests unitaires

### Tester un Command Handler

```csharp
using NSubstitute;
using NFluent;
using Xunit;

public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var user = User.Create(
            Guid.NewGuid(), 
            "test@example.com", 
            "Test User", 
            UserRole.User).Value;
        
        unitOfWork.Users
            .EmailExistsAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(false));
        
        unitOfWork.Users
            .AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(user);
        
        var handler = new CreateUserCommandHandler(unitOfWork);
        var command = new CreateUserCommand
        {
            TenantId = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            Role = UserRole.User
        };
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Check.That(result.IsSuccess).IsTrue();
        Check.That(result.Value.Email).IsEqualTo("test@example.com");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_EmailAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        
        unitOfWork.Users
            .EmailExistsAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(true)); // Email existe déjà
        
        var handler = new CreateUserCommandHandler(unitOfWork);
        var command = new CreateUserCommand
        {
            Email = "existing@example.com",
            // ...
        };
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Check.That(result.IsFailure).IsTrue();
        Check.That(result.Error.Code).IsEqualTo("User.EmailAlreadyExists");
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
```

### Tester un Query Handler

```csharp
public class GetUserByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExistingUser_ReturnsUserDto()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var userId = Guid.NewGuid();
        var user = User.Create(
            Guid.NewGuid(), 
            "test@example.com", 
            "Test", 
            UserRole.User).Value;
        
        unitOfWork.Users
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        
        var handler = new GetUserByIdQueryHandler(unitOfWork);
        var query = new GetUserByIdQuery { UserId = userId };
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        Check.That(result.IsSuccess).IsTrue();
        Check.That(result.Value.Id).IsEqualTo(user.Id);
        Check.That(result.Value.Email).IsEqualTo("test@example.com");
    }
    
    [Fact]
    public async Task Handle_NonExistentUser_ReturnsFailure()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var userId = Guid.NewGuid();
        
        unitOfWork.Users
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Error.Database.EntityNotFound(nameof(User), userId));
        
        var handler = new GetUserByIdQueryHandler(unitOfWork);
        var query = new GetUserByIdQuery { UserId = userId };
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        Check.That(result.IsFailure).IsTrue();
        Check.That(result.Error.Code).IsEqualTo("Database.EntityNotFound");
    }
}
```

## Bonnes pratiques

### ✅ À FAIRE

1. **Séparer strictement Commands et Queries**
   ```csharp
   // ✅ Query ne modifie JAMAIS l'état
   public class GetUserByIdQueryHandler : IQueryHandler<...>
   {
       public async Task<Result<UserDto>> Handle(...)
       {
           // READ ONLY - AsNoTracking()
           return await _context.Users.AsNoTracking()...
       }
   }
   ```

2. **Utiliser projections SQL dans les Queries**
   ```csharp
   // ✅ Projection au niveau SQL
   .Select(u => new UserDto { Id = u.Id, ... })
   ```

3. **Valider dans les Command Handlers**
   ```csharp
   // ✅ Validation avant Domain
   if (string.IsNullOrWhiteSpace(request.Email))
       return Error.Validation.Required(nameof(request.Email));
   ```

4. **Logique métier dans le Domain, pas dans les Handlers**
   ```csharp
   // ✅ Déléguer au Domain
   var result = user.Activate();
   if (result.IsFailure) return result.Error;
   ```

5. **Retourner des DTOs, jamais des entités Domain**
   ```csharp
   // ✅ Mapping vers DTO
   return new UserDto { ... };
   
   // ❌ Exposer l'entité
   return user; // INTERDIT
   ```

### ❌ À ÉVITER

1. **Ne pas modifier l'état dans une Query**
   ```csharp
   // ❌ INTERDIT dans une Query
   user.UpdateLastLogin(DateTime.UtcNow);
   await _unitOfWork.SaveChangesAsync();
   ```

2. **Ne pas charger des entités complètes pour des Queries**
   ```csharp
   // ❌ Inefficace
   var users = await _context.Users.ToListAsync();
   return users.Select(u => new UserDto {...});
   ```

3. **Ne pas valider dans le Domain ET dans le Handler**
   ```csharp
   // ❌ Duplication de validation
   // Validation Handler = données d'entrée + contraintes DB
   // Validation Domain = règles métier invariants
   ```

4. **Ne pas mélanger Repository et DbContext dans un Handler**
   ```csharp
   // ❌ Incohérent
   var user = await _unitOfWork.Users.GetByIdAsync(...);
   var tenants = await _context.Tenants.ToListAsync(); // Mélange
   ```

## Configuration MediatR

Dans `Program.cs` :

```csharp
// Enregistrer tous les handlers de l'assembly Application
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(ICommand).Assembly));
```

## Références

- **ADR-013** : [CQRS Pattern](../../docs/adr/013-cqrs.adr.md)
- **ADR-023** : [Result Pattern](../../docs/adr/023-result-pattern.adr.md)
- **ADR-017** : [Repository Pattern](../../docs/adr/017-repository-pattern.adr.md)
- **MediatR** : [Documentation officielle](https://github.com/jbogard/MediatR)

---

**Date de création** : 2025-12-22  
**Dernière mise à jour** : 2025-12-22  
**Auteur** : LLMProxy Team
