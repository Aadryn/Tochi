# 17. Repository Pattern

Date: 2025-12-21

## Statut

Accepté

## Contexte

L'accès direct aux données depuis la logique métier crée des problèmes :
- **Couplage à l'infrastructure** : Le domaine connaît EF Core, SQL, etc.
- **Tests difficiles** : Impossible de tester sans base de données
- **Duplication de requêtes** : Mêmes requêtes écrites à plusieurs endroits
- **Violation de SoC** : La logique métier mélangée avec l'accès aux données
- **Difficulté de changement** : Migrer de SQL Server à PostgreSQL impacte tout le code

## Décision

**Utiliser le Repository Pattern pour abstraire l'accès aux données du domaine.**

### 1. Interface du Repository dans le Domain

```csharp
// Domain/Interfaces/ITenantRepository.cs
namespace LLMProxy.Domain.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> GetByStatusAsync(TenantStatus status, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task DeleteAsync(Tenant tenant, CancellationToken ct = default);
}
```

### 2. Implémentation dans l'Infrastructure

```csharp
// Infrastructure/Repositories/TenantRepository.cs
namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly LLMProxyDbContext _context;
    
    public TenantRepository(LLMProxyDbContext context)
    {
        _context = context;
    }
    
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }
    
    public async Task<Tenant?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Name == name, ct);
    }
    
    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Tenants
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }
    
    public async Task<IReadOnlyList<Tenant>> GetByStatusAsync(
        TenantStatus status, 
        CancellationToken ct = default)
    {
        return await _context.Tenants
            .Where(t => t.Status == status)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }
    
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Tenants
            .AnyAsync(t => t.Name == name, ct);
    }
    
    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        await _context.Tenants.AddAsync(tenant, ct);
    }
    
    public async Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        _context.Tenants.Update(tenant);
    }
    
    public async Task DeleteAsync(Tenant tenant, CancellationToken ct = default)
    {
        _context.Tenants.Remove(tenant);
    }
}
```

### 3. Unit of Work pour les transactions

```csharp
// Domain/Interfaces/IUnitOfWork.cs
namespace LLMProxy.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}

// Infrastructure/UnitOfWork.cs
namespace LLMProxy.Infrastructure.PostgreSQL;

public class UnitOfWork : IUnitOfWork
{
    private readonly LLMProxyDbContext _context;
    private IDbContextTransaction? _transaction;
    
    public UnitOfWork(LLMProxyDbContext context)
    {
        _context = context;
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
    
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }
    
    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No transaction started");
        
        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
    
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No transaction started");
        
        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}
```

### 4. Utilisation dans les Handlers

```csharp
public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateTenantCommandHandler(
        ITenantRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        // Validation métier utilisant le repository
        if (await _repository.ExistsByNameAsync(request.Name, ct))
            throw new TenantNameAlreadyExistsException(request.Name);
        
        // Création de l'entité domain
        var tenant = Tenant.Create(request.Name, request.ContactEmail);
        
        // Persistance via repository
        await _repository.AddAsync(tenant, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return tenant.Id;
    }
}
```

### 5. Repository générique (optionnel)

```csharp
// Interface générique de base
public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}

// Implémentation générique
public class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly LLMProxyDbContext Context;
    protected readonly DbSet<T> DbSet;
    
    public Repository(LLMProxyDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }
    
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync(new object[] { id }, ct);
    }
    
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet.ToListAsync(ct);
    }
    
    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
    }
    
    public virtual Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }
    
    public virtual Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }
}

// Repository spécifique qui étend le générique
public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(LLMProxyDbContext context) : base(context) { }
    
    // Méthodes spécifiques à Tenant
    public async Task<Tenant?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(t => t.Name == name, ct);
    }
    
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(t => t.Name == name, ct);
    }
}
```

### 6. Specification Pattern (pour requêtes complexes)

```csharp
// Specification de base
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int? Take { get; }
    int? Skip { get; }
}

// Specification concrète
public class ActiveTenantsWithUsersSpecification : ISpecification<Tenant>
{
    public Expression<Func<Tenant, bool>> Criteria => 
        t => t.Status == TenantStatus.Active;
    
    public List<Expression<Func<Tenant, object>>> Includes => 
        new() { t => t.Users };
    
    public Expression<Func<Tenant, object>>? OrderBy => 
        t => t.Name;
    
    public Expression<Func<Tenant, object>>? OrderByDescending => null;
    public int? Take => null;
    public int? Skip => null;
}

// Extension du repository
public interface ITenantRepository
{
    Task<IReadOnlyList<Tenant>> GetAsync(
        ISpecification<Tenant> spec, 
        CancellationToken ct = default);
}

// Utilisation
var spec = new ActiveTenantsWithUsersSpecification();
var tenants = await _repository.GetAsync(spec, ct);
```

### 7. Read-only Repository pour Queries (CQRS)

```csharp
// Pour les queries, un repository simplifié en lecture seule
public interface IReadOnlyTenantRepository
{
    Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<TenantSummaryDto>> SearchAsync(
        TenantSearchCriteria criteria, 
        CancellationToken ct = default);
}

// Implémentation optimisée avec Dapper
public class ReadOnlyTenantRepository : IReadOnlyTenantRepository
{
    private readonly IDbConnection _connection;
    
    public async Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT id, name, contact_email, status, created_at
            FROM tenants
            WHERE id = @Id";
        
        return await _connection.QuerySingleOrDefaultAsync<TenantDto>(
            sql, 
            new { Id = id });
    }
    
    public async Task<PagedResult<TenantSummaryDto>> SearchAsync(
        TenantSearchCriteria criteria, 
        CancellationToken ct = default)
    {
        // SQL optimisé pour la recherche avec pagination
        var sql = BuildSearchQuery(criteria);
        var items = await _connection.QueryAsync<TenantSummaryDto>(sql, criteria);
        var total = await CountAsync(criteria);
        
        return new PagedResult<TenantSummaryDto>(
            items.ToList(), 
            total, 
            criteria.Page, 
            criteria.PageSize);
    }
}
```

## Conséquences

### Positives

- **Découplage** : Le domaine ne connaît pas les détails de persistance
- **Testabilité** : Repositories mockables facilement
- **Centralisation** : Requêtes de données centralisées, réutilisables
- **Flexibilité** : Changer d'ORM ou de BDD = changer l'implémentation
- **Single Responsibility** : Séparation claire données/métier

### Négatives

- **Indirection** : Une couche supplémentaire
  - *Mitigation* : Le découplage justifie l'indirection
- **Risque d'abstraction qui fuit** : IQueryable exposé
  - *Mitigation* : Ne jamais exposer IQueryable, retourner des collections
- **Over-engineering** : Repository générique parfois superflu
  - *Mitigation* : Préférer les repositories spécifiques avec méthodes métier

### Neutres

- Compatible avec EF Core, Dapper, ou tout autre ORM/Micro-ORM

## Alternatives considérées

### Option A : Accès direct au DbContext

- **Description** : Utiliser DbContext directement dans les handlers
- **Avantages** : Moins de code, accès à toute la puissance d'EF Core
- **Inconvénients** : Couplage fort, tests difficiles
- **Raison du rejet** : Viole le principe de séparation des préoccupations

### Option B : CQRS sans Repository (pour queries)

- **Description** : Utiliser Dapper directement dans les Query Handlers
- **Avantages** : Performance maximale, pas d'abstraction
- **Inconvénients** : Duplication de requêtes SQL
- **Raison du rejet** : Acceptable pour les queries, mais repository pour les commands

## Références

- [Repository Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/repository.html)
- [Implementing the Repository Pattern - Microsoft](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Domain-Driven Design - Eric Evans](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)
