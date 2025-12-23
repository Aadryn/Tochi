# 28. Specification Pattern

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les requêtes complexes avec multiples critères posent problème :
- **Logique dispersée** : Conditions dupliquées partout
- **Difficile à tester** : Requêtes entières à tester
- **Rigidité** : Modifier un critère impacte plusieurs endroits
- **Composition difficile** : Combiner des filtres dynamiquement

```csharp
// ❌ LOGIQUE DISPERSÉE : Conditions répétées partout
public class TenantRepository
{
    public async Task<List<Tenant>> GetActiveTenantsAsync()
    {
        return await _context.Tenants
            .Where(t => t.Status == TenantStatus.Active)
            .Where(t => !t.IsDeleted)
            .ToListAsync();
    }
    
    public async Task<List<Tenant>> GetActiveTenantsWithHighUsageAsync()
    {
        return await _context.Tenants
            .Where(t => t.Status == TenantStatus.Active) // Dupliqué !
            .Where(t => !t.IsDeleted) // Dupliqué !
            .Where(t => t.CurrentUsage > t.MonthlyQuota * 0.8m)
            .ToListAsync();
    }
}
```

## Décision

**Utiliser le Specification Pattern pour encapsuler la logique de filtrage dans des objets réutilisables et composables.**

### 1. Interface de base

```csharp
/// <summary>
/// Interface pour les spécifications.
/// </summary>
public interface ISpecification<T>
{
    /// <summary>
    /// Vérifie si l'entité satisfait la spécification (in-memory).
    /// </summary>
    bool IsSatisfiedBy(T entity);
    
    /// <summary>
    /// Convertit en expression pour les requêtes EF Core.
    /// </summary>
    Expression<Func<T, bool>> ToExpression();
}

/// <summary>
/// Classe de base pour les spécifications.
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();
    
    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }
    
    public static implicit operator Expression<Func<T, bool>>(Specification<T> spec)
    {
        return spec.ToExpression();
    }
}
```

### 2. Spécifications concrètes

```csharp
/// <summary>
/// Tenant actif.
/// </summary>
public sealed class ActiveTenantSpecification : Specification<Tenant>
{
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return tenant => tenant.Status == TenantStatus.Active;
    }
}

/// <summary>
/// Tenant non supprimé.
/// </summary>
public sealed class NotDeletedTenantSpecification : Specification<Tenant>
{
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return tenant => !tenant.IsDeleted;
    }
}

/// <summary>
/// Tenant avec usage élevé (>80% du quota).
/// </summary>
public sealed class HighUsageTenantSpecification : Specification<Tenant>
{
    private readonly decimal _threshold;
    
    public HighUsageTenantSpecification(decimal threshold = 0.8m)
    {
        _threshold = threshold;
    }
    
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return tenant => tenant.CurrentUsage > tenant.MonthlyQuota * _threshold;
    }
}

/// <summary>
/// Tenant créé dans une période.
/// </summary>
public sealed class TenantCreatedBetweenSpecification : Specification<Tenant>
{
    private readonly DateTime _start;
    private readonly DateTime _end;
    
    public TenantCreatedBetweenSpecification(DateTime start, DateTime end)
    {
        _start = start;
        _end = end;
    }
    
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return tenant => tenant.CreatedAt >= _start && tenant.CreatedAt <= _end;
    }
}

/// <summary>
/// Tenant par nom (recherche partielle).
/// </summary>
public sealed class TenantNameContainsSpecification : Specification<Tenant>
{
    private readonly string _searchTerm;
    
    public TenantNameContainsSpecification(string searchTerm)
    {
        _searchTerm = searchTerm.ToLowerInvariant();
    }
    
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return tenant => tenant.Name.ToLower().Contains(_searchTerm);
    }
}
```

### 3. Composition de spécifications

```csharp
/// <summary>
/// Extensions pour combiner les spécifications.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Combine deux spécifications avec AND.
    /// </summary>
    public static ISpecification<T> And<T>(
        this ISpecification<T> left, 
        ISpecification<T> right)
    {
        return new AndSpecification<T>(left, right);
    }
    
    /// <summary>
    /// Combine deux spécifications avec OR.
    /// </summary>
    public static ISpecification<T> Or<T>(
        this ISpecification<T> left, 
        ISpecification<T> right)
    {
        return new OrSpecification<T>(left, right);
    }
    
    /// <summary>
    /// Inverse une spécification avec NOT.
    /// </summary>
    public static ISpecification<T> Not<T>(this ISpecification<T> spec)
    {
        return new NotSpecification<T>(spec);
    }
}

/// <summary>
/// Spécification AND.
/// </summary>
public sealed class AndSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;
    
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }
    
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();
        
        var parameter = Expression.Parameter(typeof(T));
        
        var combined = Expression.AndAlso(
            Expression.Invoke(leftExpr, parameter),
            Expression.Invoke(rightExpr, parameter));
        
        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}

/// <summary>
/// Spécification OR.
/// </summary>
public sealed class OrSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;
    
    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }
    
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();
        
        var parameter = Expression.Parameter(typeof(T));
        
        var combined = Expression.OrElse(
            Expression.Invoke(leftExpr, parameter),
            Expression.Invoke(rightExpr, parameter));
        
        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}

/// <summary>
/// Spécification NOT.
/// </summary>
public sealed class NotSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _spec;
    
    public NotSpecification(ISpecification<T> spec)
    {
        _spec = spec;
    }
    
    public override Expression<Func<T, bool>> ToExpression()
    {
        var expr = _spec.ToExpression();
        var parameter = Expression.Parameter(typeof(T));
        
        var negated = Expression.Not(Expression.Invoke(expr, parameter));
        
        return Expression.Lambda<Func<T, bool>>(negated, parameter);
    }
}
```

### 4. Utilisation dans le Repository

```csharp
/// <summary>
/// Interface repository avec support des spécifications.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> FindAsync(ISpecification<T> spec, CancellationToken ct);
    Task<IReadOnlyList<T>> FindAllAsync(ISpecification<T> spec, CancellationToken ct);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct);
    Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct);
}

/// <summary>
/// Implémentation EF Core du repository.
/// </summary>
public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    
    public async Task<T?> FindAsync(ISpecification<T> spec, CancellationToken ct)
    {
        return await _context.Set<T>()
            .Where(spec.ToExpression())
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<IReadOnlyList<T>> FindAllAsync(
        ISpecification<T> spec, 
        CancellationToken ct)
    {
        return await _context.Set<T>()
            .Where(spec.ToExpression())
            .ToListAsync(ct);
    }
    
    public async Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct)
    {
        return await _context.Set<T>()
            .CountAsync(spec.ToExpression(), ct);
    }
    
    public async Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct)
    {
        return await _context.Set<T>()
            .AnyAsync(spec.ToExpression(), ct);
    }
}
```

### 5. Spécifications métier composées

```csharp
/// <summary>
/// Spécifications prédéfinies pour le domaine Tenant.
/// </summary>
public static class TenantSpecifications
{
    /// <summary>
    /// Tenant valide (actif et non supprimé).
    /// </summary>
    public static ISpecification<Tenant> Valid()
    {
        return new ActiveTenantSpecification()
            .And(new NotDeletedTenantSpecification());
    }
    
    /// <summary>
    /// Tenant valide avec usage élevé.
    /// </summary>
    public static ISpecification<Tenant> ValidWithHighUsage(decimal threshold = 0.8m)
    {
        return Valid()
            .And(new HighUsageTenantSpecification(threshold));
    }
    
    /// <summary>
    /// Tenant nécessitant attention (quota proche ou dépassé).
    /// </summary>
    public static ISpecification<Tenant> NeedsAttention()
    {
        return Valid()
            .And(new HighUsageTenantSpecification(0.9m)
                .Or(new QuotaExceededTenantSpecification()));
    }
    
    /// <summary>
    /// Tenant créé ce mois.
    /// </summary>
    public static ISpecification<Tenant> CreatedThisMonth()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
        
        return new TenantCreatedBetweenSpecification(startOfMonth, endOfMonth);
    }
}
```

### 6. Utilisation dans les services

```csharp
public class TenantService
{
    private readonly IRepository<Tenant> _repository;
    
    /// <summary>
    /// Récupère les tenants valides.
    /// </summary>
    public async Task<IReadOnlyList<Tenant>> GetValidTenantsAsync(
        CancellationToken ct)
    {
        return await _repository.FindAllAsync(
            TenantSpecifications.Valid(), 
            ct);
    }
    
    /// <summary>
    /// Récupère les tenants nécessitant attention.
    /// </summary>
    public async Task<IReadOnlyList<Tenant>> GetTenantsNeedingAttentionAsync(
        CancellationToken ct)
    {
        return await _repository.FindAllAsync(
            TenantSpecifications.NeedsAttention(), 
            ct);
    }
    
    /// <summary>
    /// Recherche de tenants avec critères dynamiques.
    /// </summary>
    public async Task<IReadOnlyList<Tenant>> SearchAsync(
        TenantSearchCriteria criteria,
        CancellationToken ct)
    {
        // Commence avec "tous les tenants valides"
        ISpecification<Tenant> spec = TenantSpecifications.Valid();
        
        // Ajoute les filtres dynamiquement
        if (!string.IsNullOrEmpty(criteria.NameContains))
        {
            spec = spec.And(new TenantNameContainsSpecification(criteria.NameContains));
        }
        
        if (criteria.CreatedAfter.HasValue)
        {
            spec = spec.And(new TenantCreatedBetweenSpecification(
                criteria.CreatedAfter.Value, 
                DateTime.MaxValue));
        }
        
        if (criteria.OnlyHighUsage)
        {
            spec = spec.And(new HighUsageTenantSpecification());
        }
        
        return await _repository.FindAllAsync(spec, ct);
    }
}
```

### 7. Tests unitaires des spécifications

```csharp
public class TenantSpecificationsTests
{
    [Fact]
    public void ActiveTenantSpecification_Should_Match_Active_Tenant()
    {
        // Arrange
        var spec = new ActiveTenantSpecification();
        var activeTenant = new Tenant { Status = TenantStatus.Active };
        var inactiveTenant = new Tenant { Status = TenantStatus.Inactive };
        
        // Act & Assert
        spec.IsSatisfiedBy(activeTenant).Should().BeTrue();
        spec.IsSatisfiedBy(inactiveTenant).Should().BeFalse();
    }
    
    [Fact]
    public void Combined_Specification_Should_Work()
    {
        // Arrange
        var spec = TenantSpecifications.Valid();
        
        var validTenant = new Tenant 
        { 
            Status = TenantStatus.Active, 
            IsDeleted = false 
        };
        
        var deletedTenant = new Tenant 
        { 
            Status = TenantStatus.Active, 
            IsDeleted = true 
        };
        
        // Act & Assert
        spec.IsSatisfiedBy(validTenant).Should().BeTrue();
        spec.IsSatisfiedBy(deletedTenant).Should().BeFalse();
    }
    
    [Theory]
    [InlineData(80, 100, true)]   // 80% usage
    [InlineData(79, 100, false)]  // < 80% usage
    [InlineData(100, 100, true)]  // 100% usage
    public void HighUsageSpecification_Should_Match_Threshold(
        int currentUsage, 
        int monthlyQuota, 
        bool expectedMatch)
    {
        // Arrange
        var spec = new HighUsageTenantSpecification(0.8m);
        var tenant = new Tenant 
        { 
            CurrentUsage = currentUsage, 
            MonthlyQuota = monthlyQuota 
        };
        
        // Act & Assert
        spec.IsSatisfiedBy(tenant).Should().Be(expectedMatch);
    }
}
```

### 8. Spécification avec Include (EF Core)

```csharp
/// <summary>
/// Spécification étendue avec support des Includes.
/// </summary>
public abstract class SpecificationWithIncludes<T> : Specification<T> where T : class
{
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }
    
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }
}

/// <summary>
/// Tenant avec ses clés API.
/// </summary>
public sealed class TenantWithApiKeysSpecification : SpecificationWithIncludes<Tenant>
{
    public TenantWithApiKeysSpecification(Guid tenantId)
    {
        _tenantId = tenantId;
        AddInclude(t => t.ApiKeys);
    }
    
    private readonly Guid _tenantId;
    
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        return tenant => tenant.Id == _tenantId;
    }
}

/// <summary>
/// Repository avec support des Includes.
/// </summary>
public class EfRepositoryWithIncludes<T> : IRepository<T> where T : class
{
    public async Task<T?> FindAsync(ISpecification<T> spec, CancellationToken ct)
    {
        var query = _context.Set<T>().AsQueryable();
        
        // Appliquer les includes si présents
        if (spec is SpecificationWithIncludes<T> specWithIncludes)
        {
            query = specWithIncludes.Includes
                .Aggregate(query, (current, include) => current.Include(include));
            
            query = specWithIncludes.IncludeStrings
                .Aggregate(query, (current, include) => current.Include(include));
        }
        
        return await query
            .Where(spec.ToExpression())
            .FirstOrDefaultAsync(ct);
    }
}
```

## Conséquences

### Positives

- **Réutilisation** : Spécifications utilisables partout
- **Testabilité** : Chaque spécification testable en isolation
- **Composition** : Combinaisons dynamiques faciles
- **Lisibilité** : Noms métier expressifs
- **Single Responsibility** : Chaque spec = un critère

### Négatives

- **Overhead** : Plus de classes
  - *Mitigation* : Utiliser seulement pour les critères réutilisés
- **Complexité expressions** : Combinaison d'expressions complexe
  - *Mitigation* : Bibliothèques comme LINQKit ou Ardalis.Specification
- **Performance** : Expression.Invoke peut être moins optimal
  - *Mitigation* : PredicateBuilder pour expressions optimisées

### Neutres

- Le Specification Pattern est complémentaire au Repository Pattern

## Alternatives considérées

### Option A : Méthodes de repository dédiées

- **Description** : Une méthode par combinaison de critères
- **Avantages** : Simple
- **Inconvénients** : Explosion combinatoire, duplication
- **Raison du rejet** : Ne scale pas avec les critères

### Option B : LINQ dynamique (System.Linq.Dynamic)

- **Description** : Construire les requêtes avec des strings
- **Avantages** : Flexible
- **Inconvénients** : Pas de typage, erreurs runtime
- **Raison du rejet** : Perd les avantages du typage fort

### Option C : Ardalis.Specification (bibliothèque)

- **Description** : Utiliser une bibliothèque existante
- **Avantages** : Fonctionnalités riches, communauté
- **Inconvénients** : Dépendance externe
- **Raison du rejet** : Acceptable, mais implémentation simple suffit souvent

## Références

- [Specification Pattern - Martin Fowler](https://martinfowler.com/apsupp/spec.pdf)
- [Ardalis.Specification](https://github.com/ardalis/Specification)
- [Domain-Driven Design - Eric Evans](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)
