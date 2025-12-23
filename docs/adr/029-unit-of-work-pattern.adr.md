# 29. Unit of Work Pattern

Date: 2025-12-21

## Statut

Accepté

## Contexte

La gestion des transactions pose plusieurs défis :
- **Transactions implicites** : Chaque SaveChanges = une transaction
- **Cohérence** : Plusieurs repositories, une seule transaction ?
- **Couplage** : Les services connaissent-ils le DbContext ?
- **Testabilité** : Comment mocker les transactions ?

```csharp
// ❌ PROBLÉMATIQUE : Chaque save = transaction séparée
public class OrderService
{
    public async Task CreateOrderAsync(CreateOrderCommand command)
    {
        // Transaction 1
        var order = new Order(command);
        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync(); // Commit 1
        
        // Transaction 2 - Si ça échoue, order est déjà créé !
        var payment = new Payment(order.Id, command.Amount);
        await _paymentRepository.AddAsync(payment);
        await _paymentRepository.SaveChangesAsync(); // Commit 2
        
        // État incohérent possible !
    }
}
```

## Décision

**Utiliser le Unit of Work Pattern pour coordonner les opérations sur plusieurs repositories dans une seule transaction.**

### 1. Interface Unit of Work

```csharp
/// <summary>
/// Interface pour le Unit of Work.
/// Coordonne les opérations sur plusieurs repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Sauvegarde toutes les modifications dans une seule transaction.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Démarre une transaction explicite.
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Commit la transaction courante.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Rollback la transaction courante.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
```

### 2. Implémentation avec EF Core

```csharp
/// <summary>
/// Implémentation du Unit of Work avec EF Core.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly LLMProxyDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private IDbContextTransaction? _currentTransaction;
    
    public UnitOfWork(
        LLMProxyDbContext context, 
        IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }
    
    /// <summary>
    /// Sauvegarde avec dispatch des événements de domaine.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // 1. Collecter les événements de domaine avant save
        var domainEvents = CollectDomainEvents();
        
        // 2. Sauvegarder dans la base
        var result = await _context.SaveChangesAsync(ct);
        
        // 3. Dispatcher les événements après save réussi
        if (domainEvents.Any())
        {
            await _eventDispatcher.DispatchAsync(domainEvents, ct);
        }
        
        return result;
    }
    
    public async Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken ct = default)
    {
        if (_currentTransaction is not null)
        {
            throw new InvalidOperationException(
                "A transaction is already in progress");
        }
        
        _currentTransaction = await _context.Database
            .BeginTransactionAsync(ct);
        
        return _currentTransaction;
    }
    
    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
        {
            throw new InvalidOperationException("No transaction in progress");
        }
        
        try
        {
            await _context.SaveChangesAsync(ct);
            await _currentTransaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }
    
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }
        
        try
        {
            await _currentTransaction.RollbackAsync(ct);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }
    
    private IReadOnlyList<IDomainEvent> CollectDomainEvents()
    {
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();
        
        var events = entitiesWithEvents
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();
        
        // Clear events après collection
        foreach (var entry in entitiesWithEvents)
        {
            entry.Entity.ClearDomainEvents();
        }
        
        return events;
    }
    
    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
    
    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}
```

### 3. Unit of Work avec accès aux repositories

```csharp
/// <summary>
/// Unit of Work avec accès centralisé aux repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    ITenantRepository Tenants { get; }
    IApiKeyRepository ApiKeys { get; }
    IUserRepository Users { get; }
    ILlmRequestRepository LlmRequests { get; }
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

/// <summary>
/// Implémentation avec lazy loading des repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly LLMProxyDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    
    private ITenantRepository? _tenants;
    private IApiKeyRepository? _apiKeys;
    private IUserRepository? _users;
    private ILlmRequestRepository? _llmRequests;
    
    public UnitOfWork(
        LLMProxyDbContext context, 
        IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }
    
    // Lazy loading des repositories
    public ITenantRepository Tenants => 
        _tenants ??= new TenantRepository(_context);
    
    public IApiKeyRepository ApiKeys => 
        _apiKeys ??= new ApiKeyRepository(_context);
    
    public IUserRepository Users => 
        _users ??= new UserRepository(_context);
    
    public ILlmRequestRepository LlmRequests => 
        _llmRequests ??= new LlmRequestRepository(_context);
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var domainEvents = CollectDomainEvents();
        var result = await _context.SaveChangesAsync(ct);
        
        if (domainEvents.Any())
        {
            await _eventDispatcher.DispatchAsync(domainEvents, ct);
        }
        
        return result;
    }
    
    // ... reste de l'implémentation
}
```

### 4. Utilisation dans les services

```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    /// <summary>
    /// Crée une commande avec paiement dans une seule transaction.
    /// </summary>
    public async Task<Order> CreateOrderWithPaymentAsync(
        CreateOrderCommand command,
        CancellationToken ct)
    {
        // Toutes les opérations partagent le même contexte
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(command.TenantId, ct);
        
        if (tenant is null)
            throw new TenantNotFoundException(command.TenantId);
        
        var order = Order.Create(tenant.Id, command.Items);
        var payment = Payment.Create(order.Id, command.Amount);
        
        await _unitOfWork.Tenants.UpdateUsageAsync(tenant.Id, order.TokenCount, ct);
        
        // Un seul SaveChanges = une seule transaction
        await _unitOfWork.SaveChangesAsync(ct);
        
        return order;
    }
}
```

### 5. Transaction explicite pour opérations complexes

```csharp
public class TenantService
{
    private readonly IUnitOfWork _unitOfWork;
    
    /// <summary>
    /// Supprime un tenant avec toutes ses données (opération complexe).
    /// </summary>
    public async Task DeleteTenantWithDataAsync(
        Guid tenantId, 
        CancellationToken ct)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        
        try
        {
            // 1. Récupérer le tenant
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, ct);
            if (tenant is null)
                throw new TenantNotFoundException(tenantId);
            
            // 2. Supprimer toutes les clés API
            await _unitOfWork.ApiKeys.DeleteByTenantIdAsync(tenantId, ct);
            
            // 3. Supprimer tous les utilisateurs
            await _unitOfWork.Users.DeleteByTenantIdAsync(tenantId, ct);
            
            // 4. Supprimer les requêtes LLM (archive)
            await _unitOfWork.LlmRequests.ArchiveByTenantIdAsync(tenantId, ct);
            
            // 5. Marquer le tenant comme supprimé
            tenant.Delete();
            
            // 6. Commit de la transaction
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
```

### 6. Scope du Unit of Work

```csharp
/// <summary>
/// Configuration DI pour le scope du Unit of Work.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUnitOfWork(
        this IServiceCollection services)
    {
        // Scoped : un UoW par requête HTTP
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Les repositories partagent le même DbContext (scoped aussi)
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        
        return services;
    }
}

/// <summary>
/// Middleware qui assure un SaveChanges automatique.
/// </summary>
public class UnitOfWorkMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        await _next(context);
        
        // Auto-save à la fin de la requête si succès
        if (context.Response.StatusCode < 400)
        {
            await unitOfWork.SaveChangesAsync();
        }
    }
}
```

### 7. Unit of Work avec Retry

```csharp
/// <summary>
/// Unit of Work avec support du retry pour concurrence optimiste.
/// </summary>
public class ResilientUnitOfWork : IUnitOfWork
{
    private readonly IUnitOfWork _inner;
    private readonly ILogger<ResilientUnitOfWork> _logger;
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        const int maxRetries = 3;
        var attempt = 0;
        
        while (true)
        {
            try
            {
                return await _inner.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex) when (attempt < maxRetries)
            {
                attempt++;
                _logger.LogWarning(
                    ex, 
                    "Concurrency conflict, retrying ({Attempt}/{MaxRetries})",
                    attempt, maxRetries);
                
                // Recharger les entités en conflit
                foreach (var entry in ex.Entries)
                {
                    await entry.ReloadAsync(ct);
                }
            }
        }
    }
}
```

### 8. Tests avec Unit of Work

```csharp
/// <summary>
/// Fake Unit of Work pour les tests.
/// </summary>
public class FakeUnitOfWork : IUnitOfWork
{
    private readonly List<object> _added = new();
    private readonly List<object> _updated = new();
    private readonly List<object> _deleted = new();
    
    public ITenantRepository Tenants { get; }
    public IApiKeyRepository ApiKeys { get; }
    
    public int SaveChangesCallCount { get; private set; }
    public bool ShouldThrowOnSave { get; set; }
    
    public FakeUnitOfWork()
    {
        Tenants = new FakeTenantRepository(_added, _updated, _deleted);
        ApiKeys = new FakeApiKeyRepository(_added, _updated, _deleted);
    }
    
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCallCount++;
        
        if (ShouldThrowOnSave)
            throw new DbUpdateException("Simulated failure");
        
        var count = _added.Count + _updated.Count + _deleted.Count;
        
        _added.Clear();
        _updated.Clear();
        _deleted.Clear();
        
        return Task.FromResult(count);
    }
    
    public void Dispose() { }
}

/// <summary>
/// Tests du service avec fake UoW.
/// </summary>
public class TenantServiceTests
{
    [Fact]
    public async Task CreateTenant_Should_CallSaveChanges()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var service = new TenantService(unitOfWork);
        
        // Act
        await service.CreateTenantAsync(
            new CreateTenantCommand("Test", 1000), 
            CancellationToken.None);
        
        // Assert
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }
    
    [Fact]
    public async Task CreateTenant_WhenSaveFails_Should_NotCommit()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork { ShouldThrowOnSave = true };
        var service = new TenantService(unitOfWork);
        
        // Act
        var act = () => service.CreateTenantAsync(
            new CreateTenantCommand("Test", 1000), 
            CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
```

## Conséquences

### Positives

- **Atomicité** : Une transaction pour toutes les opérations
- **Cohérence** : Pas d'état intermédiaire invalide
- **Abstraction** : Les services ignorent les détails de persistance
- **Testabilité** : Facilement mockable
- **Domain Events** : Point central pour dispatch

### Négatives

- **Complexité** : Une couche supplémentaire
  - *Mitigation* : EF Core DbContext est déjà un UoW basique
- **Scope management** : Attention au lifetime (Scoped)
  - *Mitigation* : Documentation et conventions claires
- **Memory** : Change tracker peut grandir
  - *Mitigation* : AsNoTracking pour les lectures

### Neutres

- EF Core DbContext implémente déjà UoW et Repository implicitement

## Alternatives considérées

### Option A : DbContext directement dans les services

- **Description** : Injecter DbContext dans les services
- **Avantages** : Simple, moins de couches
- **Inconvénients** : Couplage à EF, pas d'abstraction
- **Raison du rejet** : Moins testable, fuite d'abstraction

### Option B : Repository avec SaveChanges individuel

- **Description** : Chaque repository gère sa transaction
- **Avantages** : Autonomie des repositories
- **Inconvénients** : Transactions multiples, incohérence
- **Raison du rejet** : Perte d'atomicité

### Option C : Transaction explicite partout

- **Description** : Toujours utiliser BeginTransaction
- **Avantages** : Contrôle total
- **Inconvénients** : Verbeux, oublis possibles
- **Raison du rejet** : SaveChanges suffit dans la plupart des cas

## Références

- [Unit of Work - Martin Fowler](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [Repository and Unit of Work - Microsoft](https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application)
- [EF Core DbContext](https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/)
