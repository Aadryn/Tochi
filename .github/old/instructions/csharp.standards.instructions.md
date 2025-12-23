---
applyTo: "**/*.cs"
---

# Standards de Développement C# - Projet Janus

## 0. RÈGLES ABSOLUES (NON NÉGOCIABLES)

### 0.1. Principes SOLID - Application Systématique

**TOUJOURS appliquer ces principes dans chaque classe :**

1. **Single Responsibility Principle (SRP)**
   - ✅ Une classe = Une seule responsabilité
   - ❌ Classes > 300 lignes → Refactoring obligatoire
   - ✅ Extraire en services spécialisés

2. **Open/Closed Principle (OCP)**
   - ✅ Ouverte à l'extension, fermée à la modification
   - ✅ Utiliser interfaces, abstractions, strategy pattern
   - ❌ Modifier code existant pour ajouter fonctionnalités

3. **Liskov Substitution Principle (LSP)**
   - ✅ Classes dérivées substituables à leur classe de base
   - ✅ Respecter contrats des interfaces/classes abstraites

4. **Interface Segregation Principle (ISP)**
   - ✅ Interfaces petites et spécifiques
   - ❌ Interfaces monolithiques avec > 10 méthodes
   - ✅ Séparer en plusieurs interfaces ciblées

5. **Dependency Inversion Principle (DIP)**
   - ✅ Dépendre d'abstractions (interfaces), pas d'implémentations
   - ✅ Injection de dépendances systématique
   - ❌ `new` dans constructeurs (sauf DTOs, ValueObjects)

### 0.2. Isolation des Dépendances Tierces (MANDATORY - ADR-030)

**JAMAIS de librairie tierce directement dans le code métier**

❌ **INTERDIT** :
```csharp
// Dans Domains.Resources/ResourceService.cs
using ZiggyCreatures.Caching.Fusion;

public class ResourceService
{
    private readonly IFusionCache _cache; // ❌ Dépendance directe
}
```

✅ **OBLIGATOIRE** :
```csharp
// SharedKernels.Caching.Abstractions/ICacheService.cs
public interface ICacheService
{
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null);
}

// SharedKernels.Caching/FusionCacheService.cs
public class FusionCacheService : ICacheService
{
    private readonly IFusionCache _cache; // ✅ Isolé dans SharedKernel
    // Implémentation...
}

// Domains.Resources/ResourceService.cs
public class ResourceService
{
    private readonly ICacheService _cache; // ✅ Abstraction
}
```

**Règles** :
- ✅ Packages NuGet UNIQUEMENT dans `SharedKernels.*` ou `Tools.*`
- ❌ JAMAIS dans `Domains.*`, `Hostings.*`, `Infrastructures.*`
- ✅ Toujours créer interface + implémentation
- ✅ Enregistrer dans DI avec extension method

### 0.3. Nullable Reference Types (Activé par Défaut)

**TOUJOURS utiliser les annotations nullable C# 9+**

```xml
<!-- *.csproj -->
<PropertyGroup>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

✅ **BON** :
```csharp
public class User
{
    public string Name { get; init; } = string.Empty; // Non-nullable, valeur par défaut
    public string? MiddleName { get; init; } // Nullable explicite
    public DateTime CreatedAt { get; init; } // Non-nullable
}

public User? GetUserById(int id) // Retour nullable explicite
{
    return id > 0 ? new User() : null;
}
```

❌ **MAUVAIS** :
```csharp
public class User
{
    public string Name { get; set; } // Warning CS8618
    public string MiddleName { get; set; } // Devrait être nullable
}

public User GetUserById(int id) // Peut retourner null mais non déclaré
{
    return null; // Warning CS8603
}
```

### 0.4. Records pour Immutabilité (Privilégier)

**Utiliser `record` pour DTOs, ValueObjects, Results**

✅ **BON** :
```csharp
// DTOs
public sealed record ResourceDto(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAt
);

// ValueObjects
public sealed record Money(decimal Amount, string Currency)
{
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        
        return this with { Amount = Amount + other.Amount };
    }
}

// Results
public sealed record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
```

❌ **MAUVAIS** :
```csharp
public class ResourceDto // Devrait être un record
{
    public Guid Id { get; set; } // Mutable
    public string Name { get; set; }
}
```

### 0.5. Pattern Matching et Switch Expressions

**Toujours privilégier les switch expressions modernes**

✅ **BON** :
```csharp
public string GetStatusLabel(OrderStatus status) => status switch
{
    OrderStatus.Pending => "En attente",
    OrderStatus.Processing => "En cours",
    OrderStatus.Completed => "Terminée",
    OrderStatus.Cancelled => "Annulée",
    _ => throw new ArgumentOutOfRangeException(nameof(status))
};

public decimal CalculateDiscount(Customer customer) => customer switch
{
    { IsPremium: true, YearsActive: > 5 } => 0.20m,
    { IsPremium: true } => 0.15m,
    { YearsActive: > 3 } => 0.10m,
    _ => 0.05m
};
```

❌ **MAUVAIS** :
```csharp
public string GetStatusLabel(OrderStatus status)
{
    if (status == OrderStatus.Pending) return "En attente";
    else if (status == OrderStatus.Processing) return "En cours";
    else if (status == OrderStatus.Completed) return "Terminée";
    else if (status == OrderStatus.Cancelled) return "Annulée";
    else throw new ArgumentOutOfRangeException(nameof(status));
}
```

## 1. Organisation et Structure

### 1.1. Namespaces (File-Scoped)

✅ **OBLIGATOIRE - Namespaces file-scoped** :
```csharp
namespace Exakis.Janus.Domains.Resources;

public class Resource
{
    // ...
}
```

❌ **INTERDIT - Namespaces avec accolades** :
```csharp
namespace Exakis.Janus.Domains.Resources
{
    public class Resource
    {
        // ...
    }
}
```

### 1.2. Usings (Organisation)

**Ordre OBLIGATOIRE** :
```csharp
// 1. System usings (triés alphabétiquement)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 2. Usings externes (packages NuGet)
using Microsoft.Extensions.Logging;
using MediatR;

// 3. Usings internes (projet)
using Exakis.Janus.Domains.Resources.Entities;
using Exakis.Janus.SharedKernels.Domain.Abstractions;

namespace Exakis.Janus.Domains.Resources;
```

**Global Usings** (dans `GlobalUsings.cs`) :
```csharp
// GlobalUsings.cs (un par projet)
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;
```

### 1.3. Ordre des Membres dans une Classe

**Ordre STRICT** :
```csharp
public class ResourceService : IResourceService
{
    // 1. Champs privés constants
    private const int MaxRetries = 3;
    private const string DefaultCulture = "fr-FR";

    // 2. Champs privés readonly statiques
    private static readonly ActivitySource ActivitySource = new("Janus.Resources");

    // 3. Champs privés readonly
    private readonly IResourceRepository _repository;
    private readonly ILogger<ResourceService> _logger;
    private readonly ICacheService _cache;

    // 4. Constructeur(s)
    public ResourceService(
        IResourceRepository repository,
        ILogger<ResourceService> logger,
        ICacheService cache)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    // 5. Propriétés publiques
    public int TotalResources { get; private set; }

    // 6. Méthodes publiques (interface en premier)
    public async Task<Result<Resource>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // ...
    }

    public async Task<Result<Resource>> CreateAsync(CreateResourceCommand command, CancellationToken ct = default)
    {
        // ...
    }

    // 7. Méthodes privées
    private async Task<bool> ValidateResourceAsync(Resource resource)
    {
        // ...
    }

    private string GenerateCacheKey(Guid resourceId)
    {
        return $"resource:{resourceId}";
    }
}
```

## 2. Conventions de Nommage

### 2.1. Règles Générales (ADR-013)

| **Type** | **Convention** | **Exemple** | **Règle** |
|----------|----------------|-------------|-----------|
| Namespace | PascalCase | `Exakis.Janus.Domains.Resources` | Hiérarchie métier |
| Classe | PascalCase | `ResourceService`, `UserRepository` | Nom singulier descriptif |
| Interface | IPascalCase | `IResourceService`, `IRepository<T>` | Préfixe `I` obligatoire |
| Méthode | PascalCase | `GetByIdAsync`, `CreateResourceAsync` | Verbe d'action + suffixe `Async` |
| Propriété | PascalCase | `TotalCount`, `IsActive` | Nom ou état |
| Champ privé | _camelCase | `_repository`, `_logger`, `_cache` | Préfixe `_` obligatoire |
| Paramètre | camelCase | `resourceId`, `cancellationToken` | Descriptif sans préfixe |
| Variable locale | camelCase | `result`, `userId`, `cacheKey` | Court et clair |
| Constante | PascalCase | `MaxRetries`, `DefaultPageSize` | Tout en PascalCase |
| Enum | PascalCase | `OrderStatus`, `ResourceType` | Singulier |
| Enum valeur | PascalCase | `Pending`, `InProgress`, `Completed` | Pas de préfixe |

### 2.2. Nommage des Méthodes Async

✅ **OBLIGATOIRE - Suffixe Async** :
```csharp
public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct = default)
{
    return await _repository.FindAsync(id, ct);
}

public async Task<Result> CreateAsync(Resource resource, CancellationToken ct = default)
{
    await _repository.AddAsync(resource, ct);
    return Result.Success();
}
```

❌ **INTERDIT** :
```csharp
public async Task<Resource?> GetById(Guid id) // Manque "Async"
public Task<Resource?> GetByIdAsync(Guid id) // Pas de await mais suffixe Async confus
```

### 2.3. CancellationToken (MANDATORY)

**TOUJOURS ajouter CancellationToken en dernier paramètre**

✅ **BON** :
```csharp
public async Task<Resource?> GetByIdAsync(
    Guid id,
    bool includeDeleted = false,
    CancellationToken ct = default) // Dernier paramètre avec valeur par défaut
{
    return await _repository.GetByIdAsync(id, includeDeleted, ct);
}
```

❌ **MAUVAIS** :
```csharp
public async Task<Resource?> GetByIdAsync(Guid id) // Manque CancellationToken
public async Task<Resource?> GetByIdAsync(CancellationToken ct, Guid id) // Mauvais ordre
```

## 3. Gestion des Erreurs et Exceptions

### 3.1. Result Pattern (OBLIGATOIRE pour logique métier)

**Ne PAS utiliser d'exceptions pour le flux métier**

✅ **BON** :
```csharp
public sealed record Result
{
    public bool IsSuccess { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    
    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string code, string message) => 
        new() { IsSuccess = false, ErrorCode = code, ErrorMessage = message };
}

public sealed record Result<T> : Result
{
    public T? Value { get; init; }
    
    public static Result<T> Success(T value) => 
        new() { IsSuccess = true, Value = value };
    
    public new static Result<T> Failure(string code, string message) => 
        new() { IsSuccess = false, ErrorCode = code, ErrorMessage = message };
}

// Usage
public async Task<Result<Resource>> CreateResourceAsync(CreateResourceCommand command)
{
    if (string.IsNullOrWhiteSpace(command.Name))
        return Result<Resource>.Failure("RES-001", "Name is required");
    
    if (await _repository.ExistsByEmailAsync(command.Email))
        return Result<Resource>.Failure("RES-002", "Email already exists");
    
    var resource = new Resource(command.Name, command.Email);
    await _repository.AddAsync(resource);
    
    return Result<Resource>.Success(resource);
}
```

❌ **MAUVAIS** :
```csharp
public async Task<Resource> CreateResourceAsync(CreateResourceCommand command)
{
    if (string.IsNullOrWhiteSpace(command.Name))
        throw new ValidationException("Name is required"); // ❌ Exception pour validation
    
    if (await _repository.ExistsByEmailAsync(command.Email))
        throw new BusinessException("Email already exists"); // ❌ Exception pour règle métier
    
    return await _repository.AddAsync(new Resource(command.Name, command.Email));
}
```

### 3.2. Exceptions Personnalisées

**Créer des exceptions spécifiques pour erreurs exceptionnelles**

✅ **BON** :
```csharp
// Exceptions pour erreurs techniques SEULEMENT
public sealed class ResourceNotFoundException : Exception
{
    public Guid ResourceId { get; }
    
    public ResourceNotFoundException(Guid resourceId)
        : base($"Resource with ID {resourceId} was not found")
    {
        ResourceId = resourceId;
    }
}

public sealed class DatabaseConnectionException : Exception
{
    public string ConnectionString { get; }
    
    public DatabaseConnectionException(string connectionString, Exception innerException)
        : base($"Failed to connect to database: {connectionString}", innerException)
    {
        ConnectionString = connectionString;
    }
}
```

### 3.3. Guard Clauses

**Toujours valider les paramètres en début de méthode**

✅ **BON** :
```csharp
public async Task<Result<Resource>> UpdateAsync(
    Guid id,
    UpdateResourceCommand command,
    CancellationToken ct = default)
{
    ArgumentNullException.ThrowIfNull(command);
    
    if (id == Guid.Empty)
        return Result<Resource>.Failure("RES-003", "Invalid resource ID");
    
    var resource = await _repository.GetByIdAsync(id, ct);
    if (resource is null)
        return Result<Resource>.Failure("RES-004", "Resource not found");
    
    // Logique métier...
    return Result<Resource>.Success(resource);
}
```

## 4. Async/Await

### 4.1. Règles Async

✅ **BON** :
```csharp
// Toujours await dans les méthodes async
public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct = default)
{
    return await _repository.GetByIdAsync(id, ct);
}

// ConfigureAwait(false) dans librairies (pas dans applications)
public async Task<Resource?> GetByIdAsync(Guid id)
{
    return await _repository.GetByIdAsync(id).ConfigureAwait(false);
}

// ValueTask pour hot-paths avec cache
public async ValueTask<Resource?> GetCachedAsync(Guid id)
{
    if (_cache.TryGetValue(id, out Resource? cached))
        return cached;
    
    return await _repository.GetByIdAsync(id);
}
```

❌ **MAUVAIS** :
```csharp
// Ne pas retourner Task directement (perd stack trace)
public Task<Resource?> GetByIdAsync(Guid id)
{
    return _repository.GetByIdAsync(id); // ❌
}

// Ne pas bloquer avec .Result ou .Wait()
public Resource? GetById(Guid id)
{
    return GetByIdAsync(id).Result; // ❌ Deadlock potentiel
}
```

### 4.2. Async Void (INTERDIT sauf event handlers)

❌ **INTERDIT** :
```csharp
public async void ProcessDataAsync() // ❌ Ne peut pas être attendu
{
    await _service.ProcessAsync();
}
```

✅ **Exception - Event handlers** :
```csharp
private async void Button_Click(object sender, EventArgs e) // ✅ OK pour events
{
    await ProcessDataAsync();
}
```

## 5. LINQ et Collections

### 5.1. LINQ Moderne

✅ **BON** :
```csharp
// Query syntax pour queries complexes
var activeUsers = from user in users
                  where user.IsActive
                  join role in roles on user.RoleId equals role.Id
                  orderby user.CreatedAt descending
                  select new UserDto(user.Id, user.Name, role.Name);

// Method syntax pour opérations simples
var activeUserIds = users
    .Where(u => u.IsActive)
    .OrderByDescending(u => u.CreatedAt)
    .Select(u => u.Id)
    .ToList();

// Nouvelles méthodes C# 9+
var firstActive = users.FirstOrDefault(u => u.IsActive);
var anyActive = users.Any(u => u.IsActive);
var allActive = users.All(u => u.IsActive);
```

### 5.2. Collections Immutables (Privilégier)

✅ **BON** :
```csharp
using System.Collections.Immutable;

public sealed class Order
{
    public ImmutableList<OrderLine> Lines { get; init; } = ImmutableList<OrderLine>.Empty;
    
    public Order AddLine(OrderLine line)
    {
        return this with { Lines = Lines.Add(line) };
    }
}
```

### 5.3. Avoid ToList() When Unnecessary

✅ **BON** :
```csharp
// Pas besoin de ToList() si pas de modifications
public IEnumerable<UserDto> GetActiveUsers()
{
    return _users
        .Where(u => u.IsActive)
        .Select(u => new UserDto(u.Id, u.Name)); // Deferred execution
}

// ToList() uniquement si nécessaire
public List<UserDto> GetActiveUsersList()
{
    return _users
        .Where(u => u.IsActive)
        .Select(u => new UserDto(u.Id, u.Name))
        .ToList(); // Force immediate execution
}
```

## 6. Dependency Injection

### 6.1. Constructeur avec Validation

✅ **BON** :
```csharp
public sealed class ResourceService : IResourceService
{
    private readonly IResourceRepository _repository;
    private readonly ILogger<ResourceService> _logger;
    private readonly ICacheService _cache;

    public ResourceService(
        IResourceRepository repository,
        ILogger<ResourceService> logger,
        ICacheService cache)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }
}
```

### 6.2. Enregistrement DI

✅ **BON** :
```csharp
// Extension method dans chaque projet
public static class DependencyInjection
{
    public static IServiceCollection AddResourceServices(this IServiceCollection services)
    {
        // Transient : nouvelle instance à chaque injection
        services.AddTransient<IResourceValidator, ResourceValidator>();
        
        // Scoped : une instance par requête HTTP
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        
        // Singleton : une seule instance pour l'application
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        
        return services;
    }
}

// Usage dans Program.cs
builder.Services.AddResourceServices();
```

## 7. Documentation et Commentaires

### 7.1. XML Documentation (OBLIGATOIRE pour API publique)

✅ **BON** :
```csharp
/// <summary>
/// Service de gestion des ressources avec support du caching et de la validation
/// </summary>
public sealed class ResourceService : IResourceService
{
    /// <summary>
    /// Récupère une ressource par son identifiant
    /// </summary>
    /// <param name="id">Identifiant unique de la ressource</param>
    /// <param name="ct">Token d'annulation de l'opération</param>
    /// <returns>
    /// Un <see cref="Result{T}"/> contenant la ressource si trouvée,
    /// ou une erreur avec le code RES-404 si non trouvée
    /// </returns>
    /// <exception cref="ArgumentException">Si l'ID est invalide (Guid.Empty)</exception>
    public async Task<Result<Resource>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Resource ID cannot be empty", nameof(id));
        
        // Implementation...
    }
}
```

### 7.2. Commentaires Inline (Parcimonie)

✅ **BON** :
```csharp
public async Task<Result> ProcessOrderAsync(Order order)
{
    // Validation métier : vérifier que le stock est suffisant
    if (!await ValidateStockAsync(order))
        return Result.Failure("ORD-001", "Insufficient stock");
    
    // Calcul du prix avec remises applicables
    var totalPrice = CalculateTotalPrice(order);
    
    // Transaction atomique : réserver stock + créer commande
    await using var transaction = await _dbContext.Database.BeginTransactionAsync();
    try
    {
        await ReserveStockAsync(order);
        await SaveOrderAsync(order);
        await transaction.CommitAsync();
        
        return Result.Success();
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
        throw;
    }
}
```

❌ **MAUVAIS** :
```csharp
public void Process()
{
    // Incrémente le compteur
    counter++; // ❌ Commentaire inutile
    
    // Appelle la méthode Save
    Save(); // ❌ Redondant avec le code
}
```

## 8. Performance et Optimisation

### 8.1. Span<T> et Memory<T> pour Performance

✅ **BON pour hot-paths** :
```csharp
public ReadOnlySpan<char> ParseUserId(ReadOnlySpan<char> input)
{
    var colonIndex = input.IndexOf(':');
    return colonIndex >= 0 ? input[(colonIndex + 1)..] : input;
}

public void ProcessLargeArray(ReadOnlySpan<int> numbers)
{
    foreach (var number in numbers)
    {
        // Process without allocations
    }
}
```

### 8.2. StringBuilder pour Concaténations Multiples

✅ **BON** :
```csharp
public string BuildReport(IEnumerable<Order> orders)
{
    var sb = new StringBuilder();
    sb.AppendLine("Order Report");
    sb.AppendLine("============");
    
    foreach (var order in orders)
    {
        sb.AppendLine($"Order {order.Id}: {order.Total:C}");
    }
    
    return sb.ToString();
}
```

❌ **MAUVAIS** :
```csharp
public string BuildReport(IEnumerable<Order> orders)
{
    string report = "Order Report\n";
    report += "============\n"; // ❌ Nouvelle allocation à chaque +=
    
    foreach (var order in orders)
    {
        report += $"Order {order.Id}: {order.Total:C}\n"; // ❌ N allocations
    }
    
    return report;
}
```

## 9. Tests et Qualité

### 9.1. Couverture de Code (MANDATORY)

- ✅ **Minimum 80% de couverture** pour tout nouveau code
- ✅ **Mutation score 85%+** avec Stryker.NET
- ✅ Tests unitaires pour toute logique métier
- ✅ Tests d'intégration pour repositories

### 9.2. Nommage des Tests

✅ **BON** :
```csharp
[Fact]
public void GetById_WithValidId_ReturnsResource()
{
    // Arrange
    var id = Guid.NewGuid();
    var expected = new Resource(id, "Test", "test@example.com");
    
    // Act
    var result = _service.GetById(id);
    
    // Assert
    Check.That(result).IsEqualTo(expected);
}

[Fact]
public void GetById_WithInvalidId_ThrowsNotFoundException()
{
    // Arrange
    var id = Guid.Empty;
    
    // Act & Assert
    Check.ThatCode(() => _service.GetById(id))
        .Throws<ResourceNotFoundException>();
}
```

## 10. Sécurité

### 10.1. Validation des Entrées (TOUJOURS)

✅ **BON** :
```csharp
public async Task<Result> CreateUserAsync(CreateUserCommand command)
{
    // Validation paramètres
    if (command is null)
        throw new ArgumentNullException(nameof(command));
    
    // Validation métier
    if (string.IsNullOrWhiteSpace(command.Email))
        return Result.Failure("USR-001", "Email is required");
    
    if (!EmailValidator.IsValid(command.Email))
        return Result.Failure("USR-002", "Email format invalid");
    
    // Sanitize input
    var sanitizedName = HtmlEncoder.Default.Encode(command.Name);
    
    // Process...
}
```

### 10.2. Secrets et Configuration

❌ **INTERDIT** :
```csharp
var connectionString = "Server=prod-db;Database=janus;User=admin;Password=secret123"; // ❌
```

✅ **BON** :
```csharp
// appsettings.json (pas de secrets!)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=janus;Integrated Security=true"
  }
}

// secrets.json (dev uniquement, gitignored)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db;Database=janus;User=sa;Password=<secret>"
  }
}

// Usage
var connectionString = configuration.GetConnectionString("DefaultConnection");
```

## Résumé des Règles MANDATORY

1. ✅ **SOLID** appliqué systématiquement
2. ✅ **Isolation dépendances tierces** (ADR-030)
3. ✅ **Nullable enabled** avec annotations correctes
4. ✅ **Records** pour DTOs/ValueObjects/Results
5. ✅ **Result Pattern** pour logique métier (pas d'exceptions)
6. ✅ **CancellationToken** en dernier paramètre
7. ✅ **Async/Await** systématique, jamais .Result/.Wait()
8. ✅ **XML Documentation** pour API publique
9. ✅ **Tests** : 80%+ couverture, 85%+ mutation score
10. ✅ **Validation** des entrées et Guard Clauses

