# Repository Pattern - Documentation

## Vue d'ensemble

Ce dossier contient l'implémentation du **Repository Pattern** tel que défini dans [ADR-017](../../../docs/adr/017-repository-pattern.adr.md). Le pattern centralise la logique d'accès aux données et fournit une abstraction entre la couche Domain et la couche Infrastructure.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│  (Handlers utilisent IUnitOfWork + IRepository interfaces)  │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                             │
│        IRepository<T>, ITenantRepository, etc.              │
│              + Result<T>, Error classes                      │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                         │
│    RepositoryBase<T>, TenantRepository, etc.                │
│         + Entity Framework Core (PostgreSQL)                 │
└─────────────────────────────────────────────────────────────┘
```

## Principes clés

### 1. Railway-Oriented Programming avec Result<T>

Toutes les opérations retournent `Result<T>` pour une gestion explicite des erreurs :

```csharp
// ✅ Bon - Gestion explicite des erreurs
var userResult = await _repository.GetByIdAsync(userId, ct);
if (userResult.IsFailure)
    return userResult.Error; // Propagation de l'erreur typée

var user = userResult.Value;
// Utilisation sécurisée de user
```

```csharp
// ❌ Mauvais - Exceptions pour contrôle de flux
try 
{
    var user = await _repository.GetByIdAsync(userId, ct);
    // user peut être null → NullReferenceException
}
catch (Exception ex) 
{
    // Gestion d'erreur implicite
}
```

### 2. Erreurs typées avec Error classes

Utilisez les classes `Error.*` au lieu de messages string :

```csharp
// ✅ Bon - Erreur typée, testable, internationalisable
if (tenant is null)
    return Error.Database.EntityNotFound(nameof(Tenant), id);

// ❌ Mauvais - Message string, non typé
if (tenant is null)
    return Result.Failure("Tenant not found");
```

### 3. Logging systématique

Chaque opération critique doit être journalisée :

```csharp
public override async Task<Result<Tenant>> GetByIdAsync(Guid id, CancellationToken ct)
{
    try
    {
        var tenant = await DbSet.FirstOrDefaultAsync(t => t.Id == id, ct);
        
        if (tenant is null)
        {
            Logger.LogWarning("Tenant avec ID {Id} non trouvé", id);
            return Error.Database.EntityNotFound(nameof(Tenant), id);
        }
        
        return tenant; // Conversion implicite vers Result<Tenant>
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur lors de la récupération du Tenant {Id}", id);
        return Error.Database.AccessError("GetByIdAsync(Tenant)", ex.Message);
    }
}
```

## Structure des fichiers

```
Repositories/
├── README.md                          # Cette documentation
├── RepositoryBase.cs                  # Classe de base générique
├── TenantRepository.cs                # Repository Tenant
├── UserRepository.cs                  # Repository User
├── ApiKeyRepository.cs                # Repository ApiKey
├── AuditLogRepository.cs              # Repository AuditLog
├── LLMProviderRepository.cs           # Repository LLMProvider
├── QuotaLimitRepository.cs            # Repository QuotaLimit
└── TokenUsageMetricRepository.cs      # Repository TokenUsageMetric
```

## Guide d'utilisation

### Créer un nouveau repository

**1. Définir l'interface dans Domain/Interfaces :**

```csharp
using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Contrat du repository pour l'entité MyEntity.
/// </summary>
public interface IMyEntityRepository : IRepository<MyEntity>
{
    /// <summary>
    /// Recherche une entité par son code unique.
    /// </summary>
    /// <param name="code">Le code de l'entité.</param>
    /// <param name="ct">Jeton d'annulation.</param>
    /// <returns>Résultat contenant l'entité ou une erreur si introuvable.</returns>
    Task<Result<MyEntity>> GetByCodeAsync(string code, CancellationToken ct);
}
```

**2. Implémenter dans Infrastructure/PostgreSQL/Repositories :**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation EF Core du repository MyEntity.
/// </summary>
public sealed class MyEntityRepository : RepositoryBase<MyEntity>, IMyEntityRepository
{
    /// <summary>
    /// Initialise une nouvelle instance de <see cref="MyEntityRepository"/>.
    /// </summary>
    /// <param name="context">Le contexte de base de données.</param>
    /// <param name="logger">Le service de journalisation.</param>
    public MyEntityRepository(
        LLMProxyDbContext context, 
        ILogger<MyEntityRepository> logger) 
        : base(context, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<Result<MyEntity>> GetByCodeAsync(string code, CancellationToken ct)
    {
        try
        {
            var entity = await DbSet
                .FirstOrDefaultAsync(e => e.Code == code, ct);
            
            if (entity is null)
            {
                Logger.LogWarning("MyEntity avec code {Code} non trouvée", code);
                return new Error("MyEntity.NotFound", $"MyEntity avec code '{code}' introuvable");
            }
            
            return entity;
        }
        catch (OperationCanceledException)
        {
            // Propagation silencieuse de l'annulation
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erreur lors de la recherche de MyEntity par code {Code}", code);
            return Error.Database.AccessError("GetByCodeAsync(MyEntity)", ex.Message);
        }
    }
}
```

**3. Enregistrer dans UnitOfWork :**

```csharp
public class UnitOfWork : IUnitOfWork
{
    private IMyEntityRepository? _myEntities;
    
    public IMyEntityRepository MyEntities => 
        _myEntities ??= new MyEntityRepository(_context, _loggerFactory.CreateLogger<MyEntityRepository>());
}
```

**4. Ajouter à l'interface IUnitOfWork :**

```csharp
public interface IUnitOfWork : IDisposable
{
    IMyEntityRepository MyEntities { get; }
    // ... autres repositories
}
```

### Utiliser un repository dans un handler

```csharp
public class GetMyEntityByCodeQueryHandler : IRequestHandler<GetMyEntityByCodeQuery, Result<MyEntityDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyEntityByCodeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MyEntityDto>> Handle(GetMyEntityByCodeQuery request, CancellationToken ct)
    {
        // 1. Appeler le repository
        var entityResult = await _unitOfWork.MyEntities.GetByCodeAsync(request.Code, ct);
        
        // 2. Vérifier le résultat
        if (entityResult.IsFailure)
            return Result<MyEntityDto>.Failure(entityResult.Error);
        
        // 3. Mapper vers DTO
        var entity = entityResult.Value;
        var dto = new MyEntityDto 
        { 
            Id = entity.Id, 
            Code = entity.Code 
        };
        
        // 4. Retourner le résultat (conversion implicite)
        return dto;
    }
}
```

## Méthodes de RepositoryBase<T>

La classe `RepositoryBase<T>` fournit les opérations CRUD de base :

| Méthode | Description | Retour |
|---------|-------------|--------|
| `GetByIdAsync(Guid id, CancellationToken ct)` | Récupère une entité par son ID | `Result<T>` |
| `GetAllAsync(CancellationToken ct)` | Récupère toutes les entités | `Result<IReadOnlyList<T>>` |
| `AddAsync(T entity, CancellationToken ct)` | Ajoute une nouvelle entité | `Result<T>` |
| `UpdateAsync(T entity, CancellationToken ct)` | Met à jour une entité existante | `Result<T>` |
| `DeleteAsync(T entity, CancellationToken ct)` | Supprime une entité | `Result` |

### Exemple d'utilisation complète

```csharp
// Création
var createResult = User.Create(tenantId, email, name, role);
if (createResult.IsFailure)
    return createResult.Error;

var user = createResult.Value;
var addResult = await _unitOfWork.Users.AddAsync(user, ct);
if (addResult.IsFailure)
    return addResult.Error;

await _unitOfWork.SaveChangesAsync(ct);

// Lecture
var userResult = await _unitOfWork.Users.GetByIdAsync(userId, ct);
if (userResult.IsFailure)
    return userResult.Error;

var existingUser = userResult.Value;

// Modification
existingUser.UpdateEmail(newEmail);
var updateResult = await _unitOfWork.Users.UpdateAsync(existingUser, ct);
if (updateResult.IsFailure)
    return updateResult.Error;

await _unitOfWork.SaveChangesAsync(ct);

// Suppression
var deleteResult = await _unitOfWork.Users.DeleteAsync(existingUser, ct);
if (deleteResult.IsFailure)
    return deleteResult.Error;

await _unitOfWork.SaveChangesAsync(ct);
```

## Gestion des erreurs

### Types d'erreurs Database

La classe `Error.Database` fournit des erreurs typées pour les opérations de persistance :

```csharp
// Entité non trouvée
Error.Database.EntityNotFound(string entityType, Guid id)
// Code: "Database.EntityNotFound"
// Message: "L'entité {entityType} avec l'ID {id} est introuvable."

// Entité déjà existante
Error.Database.EntityAlreadyExists(string entityType, Guid id)
// Code: "Database.EntityAlreadyExists"
// Message: "L'entité {entityType} avec l'ID {id} existe déjà."

// Erreur d'accès générale
Error.Database.AccessError(string operation, string details)
// Code: "Database.AccessError"
// Message: "Erreur lors de l'opération '{operation}': {details}"

// Violation de contrainte unique
Error.Database.UniqueConstraintViolation(string constraintName)
// Code: "Database.UniqueConstraintViolation"
// Message: "Violation de contrainte unique '{constraintName}'."

// Violation de clé étrangère
Error.Database.ForeignKeyViolation(string constraintName)
// Code: "Database.ForeignKeyViolation"
// Message: "Violation de contrainte de clé étrangère '{constraintName}'."

// Conflit de concurrence
Error.Database.ConcurrencyConflict(string entityType)
// Code: "Database.ConcurrencyConflict"
// Message: "Conflit de concurrence détecté pour l'entité {entityType}."

// Timeout
Error.Database.Timeout(string operation)
// Code: "Database.Timeout"
// Message: "Timeout lors de l'opération '{operation}'."

// Connexion échouée
Error.Database.ConnectionFailed
// Code: "Database.ConnectionFailed"
// Message: "Impossible de se connecter à la base de données."
```

### Gestion des contraintes de base de données

```csharp
public override async Task<Result<User>> AddAsync(User entity, CancellationToken ct)
{
    try
    {
        await DbSet.AddAsync(entity, ct);
        return entity;
    }
    catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
    {
        // Violation de contrainte unique
        if (pgEx.SqlState == "23505")
        {
            Logger.LogWarning("Violation de contrainte unique lors de l'ajout de {Entity}", typeof(T).Name);
            return Error.Database.UniqueConstraintViolation(pgEx.ConstraintName ?? "unknown");
        }
        
        // Violation de clé étrangère
        if (pgEx.SqlState == "23503")
        {
            Logger.LogWarning("Violation de contrainte FK lors de l'ajout de {Entity}", typeof(T).Name);
            return Error.Database.ForeignKeyViolation(pgEx.ConstraintName ?? "unknown");
        }
        
        Logger.LogError(ex, "Erreur PostgreSQL lors de l'ajout de {Entity}", typeof(T).Name);
        return Error.Database.AccessError($"AddAsync({typeof(T).Name})", ex.Message);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur lors de l'ajout de {Entity}", typeof(T).Name);
        return Error.Database.AccessError($"AddAsync({typeof(T).Name})", ex.Message);
    }
}
```

## Performance et optimisation

### Eager Loading avec Include

Pour éviter le problème N+1, utilisez `Include()` pour charger les relations :

```csharp
public override async Task<Result<ApiKey>> GetByIdAsync(Guid id, CancellationToken ct)
{
    try
    {
        var apiKey = await DbSet
            .Include(k => k.User)        // Charge User en même temps
            .Include(k => k.Tenant)      // Charge Tenant en même temps
            .FirstOrDefaultAsync(k => k.Id == id, ct);
        
        if (apiKey is null)
            return Error.Database.EntityNotFound(nameof(ApiKey), id);
        
        return apiKey;
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur lors de la récupération de ApiKey {Id}", id);
        return Error.Database.AccessError("GetByIdAsync(ApiKey)", ex.Message);
    }
}
```

### AsNoTracking pour les lectures seules

Pour les requêtes en lecture seule (queries), désactivez le tracking :

```csharp
public async Task<Result<IReadOnlyList<Tenant>>> GetAllActiveAsync(CancellationToken ct)
{
    try
    {
        var tenants = await DbSet
            .AsNoTracking()              // Pas de tracking pour performance
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
        
        return Result<IReadOnlyList<Tenant>>.Success(tenants);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur lors de la récupération des tenants actifs");
        return Error.Database.AccessError("GetAllActiveAsync", ex.Message);
    }
}
```

### Pagination pour grandes listes

```csharp
public async Task<Result<IReadOnlyList<AuditLog>>> GetPagedAsync(
    int pageIndex, 
    int pageSize, 
    CancellationToken ct)
{
    try
    {
        var logs = await DbSet
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        
        return Result<IReadOnlyList<AuditLog>>.Success(logs);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur lors de la pagination des logs");
        return Error.Database.AccessError("GetPagedAsync", ex.Message);
    }
}
```

## Tests unitaires

### Mocker un repository

```csharp
using NSubstitute;

[Fact]
public async Task Handle_UserNotFound_ReturnsFailure()
{
    // Arrange
    var userId = Guid.NewGuid();
    var unitOfWork = Substitute.For<IUnitOfWork>();
    
    unitOfWork.Users
        .GetByIdAsync(userId, Arg.Any<CancellationToken>())
        .Returns(Error.Database.EntityNotFound(nameof(User), userId));
    
    var handler = new GetUserByIdQueryHandler(unitOfWork);
    var query = new GetUserByIdQuery(userId);
    
    // Act
    var result = await handler.Handle(query, CancellationToken.None);
    
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be("Database.EntityNotFound");
}
```

### Tester un repository réel avec base de données en mémoire

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

[Fact]
public async Task GetByIdAsync_ExistingTenant_ReturnsSuccess()
{
    // Arrange
    var options = new DbContextOptionsBuilder<LLMProxyDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    
    using var context = new LLMProxyDbContext(options);
    var logger = NullLogger<TenantRepository>.Instance;
    var repository = new TenantRepository(context, logger);
    
    var tenant = Tenant.Create("Test Tenant", "test-tenant", null).Value;
    await context.Tenants.AddAsync(tenant);
    await context.SaveChangesAsync();
    
    // Act
    var result = await repository.GetByIdAsync(tenant.Id, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be("Test Tenant");
}
```

## Bonnes pratiques

### ✅ À FAIRE

1. **Toujours vérifier IsFailure avant d'accéder à .Value**
   ```csharp
   var result = await _repository.GetByIdAsync(id, ct);
   if (result.IsFailure)
       return result.Error;
   
   var entity = result.Value; // Sécurisé
   ```

2. **Logger les opérations critiques**
   ```csharp
   Logger.LogWarning("Entity {Type} avec ID {Id} non trouvée", typeof(T).Name, id);
   ```

3. **Utiliser les erreurs typées**
   ```csharp
   return Error.Database.EntityNotFound(nameof(Tenant), id);
   ```

4. **Gérer les OperationCanceledException**
   ```csharp
   catch (OperationCanceledException)
   {
       throw; // Propagation silencieuse
   }
   ```

5. **Retourner des collections en lecture seule**
   ```csharp
   Task<Result<IReadOnlyList<T>>> GetAllAsync(CancellationToken ct);
   ```

### ❌ À ÉVITER

1. **Ne pas vérifier IsFailure**
   ```csharp
   // ❌ DANGEREUX
   var entity = (await _repository.GetByIdAsync(id, ct)).Value;
   ```

2. **Utiliser des messages d'erreur string**
   ```csharp
   // ❌ Non typé, non testable
   return Result.Failure("Not found");
   ```

3. **Swallow des exceptions sans logging**
   ```csharp
   // ❌ Perte d'information
   catch (Exception) 
   { 
       return null; 
   }
   ```

4. **Retourner null au lieu de Result**
   ```csharp
   // ❌ Ancien pattern, à éviter
   public async Task<User?> GetByIdAsync(Guid id) { ... }
   ```

5. **Mélanger logique métier dans le repository**
   ```csharp
   // ❌ Responsabilité du Domain
   public async Task<bool> CanUserCreateApiKey(Guid userId) { ... }
   ```

## Références

- **ADR-017** : [Repository Pattern](../../../docs/adr/017-repository-pattern.adr.md)
- **ADR-023** : [Result Pattern](../../../docs/adr/023-result-pattern.adr.md)
- **ADR-027** : [Defensive Programming](../../../docs/adr/027-defensive-programming.adr.md)
- **ADR-029** : [Unit of Work Pattern](../../../docs/adr/029-unit-of-work-pattern.adr.md)

## Migration depuis l'ancien code

Si vous avez du code existant qui n'utilise pas les repositories :

**Avant (accès direct EF Core) :**
```csharp
var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
if (user == null)
    return NotFound();
```

**Après (avec repository + Result<T>) :**
```csharp
var userResult = await _unitOfWork.Users.GetByIdAsync(userId, ct);
if (userResult.IsFailure)
    return Result<UserDto>.Failure(userResult.Error);

var user = userResult.Value;
```

---

**Date de création** : 2025-12-22  
**Dernière mise à jour** : 2025-12-22  
**Auteur** : LLMProxy Team
