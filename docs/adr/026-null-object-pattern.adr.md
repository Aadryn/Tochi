# 26. Null Object Pattern

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les vérifications de null dispersées polluent le code :
- **Null checks partout** : `if (x != null)` répétés
- **NullReferenceException** : Oubli de vérification
- **Code défensif excessif** : Même le code sûr vérifie null
- **Violation Open/Closed** : Chaque cas spécial demande une condition

```csharp
// ❌ NULL CHECKS PARTOUT
public class OrderService
{
    public decimal CalculateTotal(Order order)
    {
        if (order == null)
            return 0;
        
        decimal total = 0;
        
        if (order.Items != null)
        {
            foreach (var item in order.Items)
            {
                if (item != null)
                {
                    total += item.Price * item.Quantity;
                    
                    if (order.Discount != null)
                    {
                        total -= order.Discount.Calculate(total);
                    }
                }
            }
        }
        
        if (order.Customer != null && order.Customer.Loyalty != null)
        {
            total = order.Customer.Loyalty.ApplyBenefits(total);
        }
        
        return total;
    }
}
```

## Décision

**Utiliser le Null Object Pattern : remplacer null par un objet qui représente "rien" avec un comportement neutre.**

### 1. Pattern de base

```csharp
/// <summary>
/// Interface pour le service de remise.
/// </summary>
public interface IDiscountService
{
    Money CalculateDiscount(Money originalPrice);
}

/// <summary>
/// Implémentation réelle du service de remise.
/// </summary>
public class PercentageDiscountService : IDiscountService
{
    private readonly Percentage _discountRate;
    
    public PercentageDiscountService(Percentage discountRate)
    {
        _discountRate = discountRate;
    }
    
    public Money CalculateDiscount(Money originalPrice)
    {
        return originalPrice.Multiply(_discountRate.ToFraction());
    }
}

/// <summary>
/// Null Object : pas de remise.
/// </summary>
public class NoDiscountService : IDiscountService
{
    public static readonly NoDiscountService Instance = new();
    
    private NoDiscountService() { }
    
    public Money CalculateDiscount(Money originalPrice)
    {
        return Money.Zero(originalPrice.Currency);
    }
}

// Utilisation : plus de null check !
public class OrderService
{
    public Money CalculateTotal(Order order, IDiscountService discount)
    {
        var subtotal = order.CalculateSubtotal();
        
        // discount n'est jamais null, toujours IDiscountService
        var discountAmount = discount.CalculateDiscount(subtotal);
        
        return subtotal.Subtract(discountAmount);
    }
}
```

### 2. Null Object pour Logger

```csharp
/// <summary>
/// Interface de logging.
/// </summary>
public interface IApplicationLogger
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? ex = null);
}

/// <summary>
/// Logger réel.
/// </summary>
public class ApplicationLogger : IApplicationLogger
{
    private readonly ILogger _logger;
    
    public void LogInfo(string message) => _logger.LogInformation(message);
    public void LogWarning(string message) => _logger.LogWarning(message);
    public void LogError(string message, Exception? ex) => _logger.LogError(ex, message);
}

/// <summary>
/// Null Object : ne log rien (utile pour les tests).
/// </summary>
public sealed class NullLogger : IApplicationLogger
{
    public static readonly NullLogger Instance = new();
    
    private NullLogger() { }
    
    public void LogInfo(string message) { }
    public void LogWarning(string message) { }
    public void LogError(string message, Exception? ex = null) { }
}

// Utilisation dans les tests
public class MyServiceTests
{
    [Fact]
    public void Should_Process_Without_Logging()
    {
        var service = new MyService(NullLogger.Instance);
        // Pas de logs pollués pendant les tests
    }
}
```

### 3. Null Object pour Cache

```csharp
/// <summary>
/// Interface de cache.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct);
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct);
    Task RemoveAsync(string key, CancellationToken ct);
}

/// <summary>
/// Cache Redis réel.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _redis;
    
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        var value = await _redis.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(value);
        await _redis.StringSetAsync(key, json, expiration);
    }
    
    public async Task RemoveAsync(string key, CancellationToken ct)
    {
        await _redis.KeyDeleteAsync(key);
    }
}

/// <summary>
/// Null Object : pas de cache (pass-through).
/// </summary>
public sealed class NoCacheService : ICacheService
{
    public static readonly NoCacheService Instance = new();
    
    private NoCacheService() { }
    
    public Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        return Task.FromResult<T?>(default);
    }
    
    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
    
    public Task RemoveAsync(string key, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}

// Service qui utilise le cache - pas de null check
public class TenantService
{
    private readonly ICacheService _cache;
    
    public TenantService(ICacheService cache)
    {
        // Si pas de Redis configuré, on injecte NoCacheService
        _cache = cache;
    }
    
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        // Pas de "if (_cache != null)"
        var cached = await _cache.GetAsync<Tenant>($"tenant:{id}", ct);
        if (cached is not null)
            return cached;
        
        var tenant = await _repository.FindAsync(id, ct);
        
        if (tenant is not null)
            await _cache.SetAsync($"tenant:{id}", tenant, TimeSpan.FromMinutes(5), ct);
        
        return tenant;
    }
}
```

### 4. Null Object pour Customer/Guest

```csharp
/// <summary>
/// Interface customer.
/// </summary>
public interface ICustomer
{
    string Name { get; }
    Email? Email { get; }
    Percentage GetDiscountRate();
    bool CanAccessPremiumFeatures();
}

/// <summary>
/// Client enregistré.
/// </summary>
public class RegisteredCustomer : ICustomer
{
    public string Name { get; }
    public Email? Email { get; }
    public CustomerTier Tier { get; }
    
    public Percentage GetDiscountRate()
    {
        return Tier switch
        {
            CustomerTier.Bronze => Percentage.Create(5),
            CustomerTier.Silver => Percentage.Create(10),
            CustomerTier.Gold => Percentage.Create(15),
            _ => Percentage.Zero
        };
    }
    
    public bool CanAccessPremiumFeatures() => Tier >= CustomerTier.Silver;
}

/// <summary>
/// Null Object : visiteur anonyme.
/// </summary>
public sealed class GuestCustomer : ICustomer
{
    public static readonly GuestCustomer Instance = new();
    
    private GuestCustomer() { }
    
    public string Name => "Guest";
    public Email? Email => null;
    
    public Percentage GetDiscountRate() => Percentage.Zero;
    public bool CanAccessPremiumFeatures() => false;
}

// Utilisation : pas de null check
public class PricingService
{
    public Money CalculatePrice(Product product, ICustomer customer)
    {
        var basePrice = product.Price;
        
        // customer n'est jamais null
        var discount = customer.GetDiscountRate();
        
        return basePrice.ApplyDiscount(discount);
    }
}

// Dans le contrôleur
public class CartController
{
    public IActionResult GetCart()
    {
        // Retourne GuestCustomer si pas connecté
        ICustomer customer = User.Identity?.IsAuthenticated == true
            ? _customerService.GetCurrent()
            : GuestCustomer.Instance;
        
        var price = _pricingService.CalculatePrice(product, customer);
        return Ok(price);
    }
}
```

### 5. Null Object pour Collections

```csharp
/// <summary>
/// Repository qui retourne toujours une collection (jamais null).
/// </summary>
public class OrderRepository : IOrderRepository
{
    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        Guid customerId, 
        CancellationToken ct)
    {
        var orders = await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(ct);
        
        // Retourne une liste vide plutôt que null
        return orders;
    }
}

// Plus besoin de vérifier null
public class OrderService
{
    public async Task<OrderSummary> GetCustomerSummaryAsync(
        Guid customerId, 
        CancellationToken ct)
    {
        var orders = await _repository.GetByCustomerIdAsync(customerId, ct);
        
        // orders n'est jamais null, peut être vide
        return new OrderSummary
        {
            TotalOrders = orders.Count,
            TotalAmount = orders.Sum(o => o.Total),
            LastOrderDate = orders.MaxBy(o => o.CreatedAt)?.CreatedAt
        };
    }
}
```

### 6. Null Object pour Specification

```csharp
/// <summary>
/// Interface de spécification.
/// </summary>
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
    Expression<Func<T, bool>> ToExpression();
}

/// <summary>
/// Null Object : toujours satisfait.
/// </summary>
public sealed class AlwaysTrueSpecification<T> : ISpecification<T>
{
    public static readonly AlwaysTrueSpecification<T> Instance = new();
    
    private AlwaysTrueSpecification() { }
    
    public bool IsSatisfiedBy(T entity) => true;
    public Expression<Func<T, bool>> ToExpression() => _ => true;
}

/// <summary>
/// Null Object : jamais satisfait.
/// </summary>
public sealed class AlwaysFalseSpecification<T> : ISpecification<T>
{
    public static readonly AlwaysFalseSpecification<T> Instance = new();
    
    private AlwaysFalseSpecification() { }
    
    public bool IsSatisfiedBy(T entity) => false;
    public Expression<Func<T, bool>> ToExpression() => _ => false;
}

// Utilisation dans une query
public class TenantQueryService
{
    public async Task<IReadOnlyList<Tenant>> SearchAsync(
        TenantSearchCriteria criteria,
        CancellationToken ct)
    {
        // Construit la spécification, utilise AlwaysTrue si pas de filtre
        ISpecification<Tenant> spec = AlwaysTrueSpecification<Tenant>.Instance;
        
        if (!string.IsNullOrEmpty(criteria.Name))
            spec = new TenantNameContainsSpecification(criteria.Name);
        
        if (criteria.Status.HasValue)
            spec = spec.And(new TenantStatusSpecification(criteria.Status.Value));
        
        return await _repository.FindAsync(spec, ct);
    }
}
```

### 7. Null Object avec Factory

```csharp
/// <summary>
/// Factory qui retourne le bon type ou le Null Object.
/// </summary>
public class NotificationServiceFactory
{
    private readonly IServiceProvider _services;
    
    public INotificationService Create(NotificationType type)
    {
        return type switch
        {
            NotificationType.Email => _services.GetRequiredService<EmailNotificationService>(),
            NotificationType.Sms => _services.GetRequiredService<SmsNotificationService>(),
            NotificationType.Push => _services.GetRequiredService<PushNotificationService>(),
            NotificationType.None => NullNotificationService.Instance,
            _ => NullNotificationService.Instance
        };
    }
}

/// <summary>
/// Null Object : n'envoie rien.
/// </summary>
public sealed class NullNotificationService : INotificationService
{
    public static readonly NullNotificationService Instance = new();
    
    private NullNotificationService() { }
    
    public Task SendAsync(string recipient, string message, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
```

### 8. Éviter les Null Object abusifs

```csharp
// ⚠️ NE PAS utiliser Null Object quand null a une signification métier

// ❌ MAUVAIS : Null Object pour une entité manquante
public class NullTenant : ITenant
{
    // Un tenant inexistant N'EST PAS un tenant !
}

// ✅ BON : Utiliser Optional/Result pour les entités
public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct)
{
    return await _context.Tenants.FindAsync(id, ct);
    // null = pas trouvé (signification métier claire)
}

// ✅ BON : Null Object pour les SERVICES/COMPORTEMENTS, pas les DONNÉES
public interface IDiscountPolicy { }
public class NoDiscount : IDiscountPolicy { } // OK - comportement neutre

public interface ITenant { }
public class NullTenant : ITenant { } // MAUVAIS - entité absente
```

## Conséquences

### Positives

- **Moins de null checks** : Code plus fluide
- **Polymorphisme** : Traitement uniforme
- **Testabilité** : Null Objects pour les tests
- **Open/Closed** : Ajouter un comportement sans modifier l'existant
- **NullReferenceException** : Évitées par conception

### Négatives

- **Classes supplémentaires** : Un Null Object par interface
  - *Mitigation* : Utiliser seulement où pertinent
- **Confusion possible** : Est-ce un vrai objet ou un Null Object ?
  - *Mitigation* : Nommer clairement (NoX, NullX, EmptyX)
- **Masquer les erreurs** : Peut cacher un vrai problème
  - *Mitigation* : Logging dans les Null Objects si nécessaire

### Neutres

- Le Null Object Pattern complète bien le Result Pattern

## Alternatives considérées

### Option A : Nullable reference types (C# 8+)

- **Description** : Utiliser `?` pour indiquer les nullables
- **Avantages** : Support compilateur, warnings
- **Inconvénients** : Toujours besoin de gérer null
- **Raison du rejet** : Complémentaire mais ne remplace pas Null Object

### Option B : Optional<T> monad

- **Description** : Type Option pour représenter l'absence
- **Avantages** : Force le handling explicite
- **Inconvénients** : Overhead, moins idiomatique en C#
- **Raison du rejet** : Null Object plus simple pour les comportements

## Références

- [Null Object Pattern - Martin Fowler](https://refactoring.com/catalog/introduceNullObject.html)
- [Design Patterns - Gang of Four](https://www.amazon.com/Design-Patterns-Elements-Reusable-Object-Oriented/dp/0201633612)
- [Working Effectively with Legacy Code - Michael Feathers](https://www.amazon.com/Working-Effectively-Legacy-Michael-Feathers/dp/0131177052)
