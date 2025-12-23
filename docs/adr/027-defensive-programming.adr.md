# 27. Defensive Programming

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un code naïf suppose que tout fonctionne comme prévu :
- **Données invalides** : Entrées non validées propagées
- **États impossibles** : Hypothèses non vérifiées
- **Effets de bord** : Modifications inattendues
- **Erreurs silencieuses** : Problèmes non détectés

```csharp
// ❌ CODE NAÏF : Suppose que tout va bien
public class OrderService
{
    public decimal CalculateDiscount(Order order, decimal percentage)
    {
        // Pas de validation !
        return order.Total * percentage / 100;
        // Que se passe-t-il si order est null ?
        // Que se passe-t-il si percentage est négatif ?
        // Que se passe-t-il si percentage > 100 ?
    }
}
```

## Décision

**Appliquer la programmation défensive : protéger le code contre les mauvaises utilisations tout en maintenant un équilibre entre sécurité et lisibilité.**

### 1. Validation des entrées publiques

```csharp
/// <summary>
/// Crée un nouveau tenant avec validation complète.
/// </summary>
public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public int MonthlyQuota { get; private set; }
    
    private Tenant() { }
    
    /// <summary>
    /// Crée un tenant avec validation défensive.
    /// </summary>
    /// <exception cref="ArgumentNullException">Si le nom est null.</exception>
    /// <exception cref="ArgumentException">Si le nom est invalide.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Si le quota est invalide.</exception>
    public static Tenant Create(string name, int monthlyQuota)
    {
        // Validation défensive de toutes les entrées
        ArgumentNullException.ThrowIfNull(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        if (name.Length > 100)
            throw new ArgumentException(
                "Tenant name cannot exceed 100 characters", 
                nameof(name));
        
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(monthlyQuota);
        
        if (monthlyQuota > 10_000_000)
            throw new ArgumentOutOfRangeException(
                nameof(monthlyQuota), 
                "Monthly quota cannot exceed 10,000,000");
        
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            MonthlyQuota = monthlyQuota
        };
    }
}
```

### 2. Assertions pour invariants internes

```csharp
using System.Diagnostics;

public class ShoppingCart
{
    private readonly List<CartItem> _items = new();
    
    /// <summary>
    /// Ajoute un article au panier.
    /// </summary>
    public void AddItem(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        
        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new CartItem(product.Id, product.Price, quantity));
        }
        
        // Assertion pour invariant : le total d'articles est cohérent
        Debug.Assert(
            _items.Sum(i => i.Quantity) > 0, 
            "Cart should have at least one item after AddItem");
        
        // Assertion pour invariant : pas de doublons de produit
        Debug.Assert(
            _items.Select(i => i.ProductId).Distinct().Count() == _items.Count,
            "Cart should not have duplicate products");
    }
    
    /// <summary>
    /// Calcule le total du panier.
    /// </summary>
    public Money CalculateTotal()
    {
        // Assertion de précondition interne
        Debug.Assert(_items != null, "_items should never be null");
        
        var total = _items.Sum(item => 
        {
            // Assertion pour chaque item
            Debug.Assert(item.Quantity > 0, "Item quantity should be positive");
            Debug.Assert(item.UnitPrice.IsPositive, "Item price should be positive");
            
            return item.UnitPrice.Amount * item.Quantity;
        });
        
        var result = Money.Euros(total);
        
        // Assertion de postcondition
        Debug.Assert(
            result.Amount >= 0, 
            "Total should never be negative");
        
        return result;
    }
}
```

### 3. Copies défensives

```csharp
public class Order
{
    private readonly List<OrderItem> _items;
    private readonly DateTime _createdAt;
    
    /// <summary>
    /// Constructeur avec copies défensives.
    /// </summary>
    public Order(IEnumerable<OrderItem> items, DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNull(items);
        
        // COPIE DÉFENSIVE : créer une nouvelle liste
        // pour éviter les modifications externes
        _items = items.ToList();
        
        if (_items.Count == 0)
            throw new ArgumentException("Order must have at least one item", nameof(items));
        
        // Copier le DateTime (struct, donc copie par valeur)
        _createdAt = createdAt;
    }
    
    /// <summary>
    /// Retourne une copie en lecture seule des items.
    /// </summary>
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    
    // Ou avec une vraie copie si les items sont mutables
    public IReadOnlyList<OrderItem> GetItemsCopy() => 
        _items.Select(i => i.Clone()).ToList().AsReadOnly();
}

// Exemple avec objet mutable
public class Configuration
{
    private readonly Dictionary<string, string> _settings;
    
    public Configuration(IDictionary<string, string> settings)
    {
        // COPIE DÉFENSIVE du dictionnaire
        _settings = new Dictionary<string, string>(settings);
    }
    
    /// <summary>
    /// Retourne une copie des settings (pas la référence interne).
    /// </summary>
    public IReadOnlyDictionary<string, string> GetSettings()
    {
        // COPIE DÉFENSIVE en sortie aussi
        return new Dictionary<string, string>(_settings);
    }
}
```

### 4. Immutabilité pour la défense

```csharp
/// <summary>
/// Configuration immutable - impossible à modifier après création.
/// </summary>
public sealed class TenantConfiguration
{
    public Guid TenantId { get; }
    public int MaxRequestsPerMinute { get; }
    public IReadOnlyList<string> AllowedModels { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
    
    public TenantConfiguration(
        Guid tenantId,
        int maxRequestsPerMinute,
        IEnumerable<string> allowedModels,
        IDictionary<string, string>? metadata = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRequestsPerMinute);
        ArgumentNullException.ThrowIfNull(allowedModels);
        
        TenantId = tenantId;
        MaxRequestsPerMinute = maxRequestsPerMinute;
        
        // Collections immuables
        AllowedModels = allowedModels.ToList().AsReadOnly();
        Metadata = metadata is null 
            ? new Dictionary<string, string>().AsReadOnly()
            : new Dictionary<string, string>(metadata).AsReadOnly();
    }
    
    /// <summary>
    /// Crée une nouvelle configuration avec un quota différent.
    /// </summary>
    public TenantConfiguration WithMaxRequestsPerMinute(int newLimit)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newLimit);
        
        return new TenantConfiguration(
            TenantId,
            newLimit,
            AllowedModels,
            Metadata.ToDictionary(kv => kv.Key, kv => kv.Value));
    }
}
```

### 5. Sealed classes pour prévenir l'héritage non prévu

```csharp
/// <summary>
/// Classe sealed pour empêcher l'extension non contrôlée.
/// </summary>
public sealed class ApiKeyValidator
{
    private readonly IApiKeyRepository _repository;
    
    public ApiKeyValidator(IApiKeyRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
    
    /// <summary>
    /// Valide une clé API.
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(
        string apiKey, 
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        
        var key = await _repository.FindByHashAsync(
            ComputeHash(apiKey), 
            ct);
        
        if (key is null)
            return ValidationResult.Invalid("API key not found");
        
        if (key.IsExpired)
            return ValidationResult.Invalid("API key has expired");
        
        if (!key.IsActive)
            return ValidationResult.Invalid("API key is not active");
        
        return ValidationResult.Valid(key);
    }
    
    private static string ComputeHash(string apiKey)
    {
        // Implémentation du hash
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey)));
    }
}
```

### 6. Fail-Fast avec états impossibles

```csharp
public class OrderStateMachine
{
    public OrderStatus Status { get; private set; }
    
    /// <summary>
    /// Confirme la commande avec vérification d'état.
    /// </summary>
    public void Confirm()
    {
        // FAIL-FAST : rejeter les transitions invalides immédiatement
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot confirm order in status {Status}. " +
                $"Order must be in {OrderStatus.Pending} status.");
        }
        
        Status = OrderStatus.Confirmed;
    }
    
    /// <summary>
    /// Expédie la commande.
    /// </summary>
    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException(
                $"Cannot ship order in status {Status}. " +
                $"Order must be in {OrderStatus.Confirmed} status.");
        }
        
        Status = OrderStatus.Shipped;
    }
    
    /// <summary>
    /// Annule la commande.
    /// </summary>
    public void Cancel(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        
        // Seuls certains états permettent l'annulation
        var cancellableStatuses = new[] 
        { 
            OrderStatus.Pending, 
            OrderStatus.Confirmed 
        };
        
        if (!cancellableStatuses.Contains(Status))
        {
            throw new InvalidOperationException(
                $"Cannot cancel order in status {Status}. " +
                $"Only orders in [{string.Join(", ", cancellableStatuses)}] can be cancelled.");
        }
        
        Status = OrderStatus.Cancelled;
    }
}
```

### 7. Vérifications de cohérence

```csharp
public class Invoice
{
    private readonly List<InvoiceLine> _lines = new();
    
    public Money TotalAmount { get; private set; }
    
    /// <summary>
    /// Ajoute une ligne à la facture.
    /// </summary>
    public void AddLine(InvoiceLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        
        _lines.Add(line);
        RecalculateTotal();
        
        // VÉRIFICATION DE COHÉRENCE après modification
        VerifyInvariant();
    }
    
    private void RecalculateTotal()
    {
        TotalAmount = _lines.Aggregate(
            Money.Zero(Currency.EUR),
            (total, line) => total.Add(line.Amount));
    }
    
    /// <summary>
    /// Vérifie que l'invariant de la facture est respecté.
    /// </summary>
    private void VerifyInvariant()
    {
        var calculatedTotal = _lines.Aggregate(
            Money.Zero(Currency.EUR),
            (total, line) => total.Add(line.Amount));
        
        if (TotalAmount != calculatedTotal)
        {
            throw new InvalidOperationException(
                $"Invoice total mismatch: stored={TotalAmount}, calculated={calculatedTotal}");
        }
        
        if (_lines.Any(l => l.Amount.IsNegative))
        {
            throw new InvalidOperationException("Invoice cannot have negative line amounts");
        }
    }
}
```

### 8. Équilibre entre défense et lisibilité

```csharp
public class TenantService
{
    // ✅ DÉFENSIF aux frontières (API publique)
    public async Task<Tenant> CreateTenantAsync(
        CreateTenantCommand command, 
        CancellationToken ct)
    {
        // Validation complète des entrées
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.Name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(command.MonthlyQuota);
        
        // Une fois validé, on peut faire confiance aux données internes
        var tenant = await CreateTenantInternalAsync(command.Name, command.MonthlyQuota, ct);
        
        return tenant;
    }
    
    // ✅ CONFIANT en interne (méthode privée, données déjà validées)
    private async Task<Tenant> CreateTenantInternalAsync(
        string name, 
        int monthlyQuota, 
        CancellationToken ct)
    {
        // Debug.Assert pour vérifier les hypothèses en développement
        Debug.Assert(!string.IsNullOrWhiteSpace(name), "Name should be validated");
        Debug.Assert(monthlyQuota > 0, "Quota should be validated");
        
        var tenant = Tenant.Create(name, monthlyQuota);
        await _repository.AddAsync(tenant, ct);
        
        return tenant;
    }
}
```

### 9. Logging défensif

```csharp
public class ApiKeyService
{
    private readonly ILogger<ApiKeyService> _logger;
    
    /// <summary>
    /// Valide une clé API avec logging défensif.
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(
        string apiKey, 
        CancellationToken ct)
    {
        // NE JAMAIS logger les secrets !
        _logger.LogDebug(
            "Validating API key (hash: {KeyHash})", 
            ComputeHash(apiKey)[..8]); // Seulement les premiers caractères du hash
        
        try
        {
            var result = await ValidateInternalAsync(apiKey, ct);
            
            _logger.LogInformation(
                "API key validation result: {IsValid}", 
                result.IsValid);
            
            return result;
        }
        catch (Exception ex)
        {
            // Logger l'erreur SANS le secret
            _logger.LogError(
                ex, 
                "Error validating API key");
            
            throw;
        }
    }
}
```

## Conséquences

### Positives

- **Bugs détectés tôt** : Fail-fast aux frontières
- **Debug facilité** : Messages d'erreur explicites
- **Code robuste** : Résiste aux mauvaises utilisations
- **Documentation vivante** : Les validations documentent les attentes

### Négatives

- **Verbosité** : Plus de code de validation
  - *Mitigation* : Centraliser dans des helpers/attributes
- **Performance** : Vérifications additionnelles
  - *Mitigation* : Debug.Assert pour les vérifications coûteuses
- **Over-engineering** : Peut aller trop loin
  - *Mitigation* : Défensif aux frontières, confiant en interne

### Neutres

- La programmation défensive est un investissement qui paie sur le long terme

## Alternatives considérées

### Option A : Faire confiance aux appelants

- **Description** : Pas de validation, supposer que les données sont correctes
- **Avantages** : Code plus court, performance
- **Inconvénients** : Bugs difficiles à tracer
- **Raison du rejet** : Trop risqué, surtout aux frontières

### Option B : Tout valider partout

- **Description** : Validation à chaque niveau
- **Avantages** : Sécurité maximale
- **Inconvénients** : Duplication, performance, illisibilité
- **Raison du rejet** : Équilibre préféré (frontières/interne)

## Références

- [Defensive Programming - Wikipedia](https://en.wikipedia.org/wiki/Defensive_programming)
- [Code Complete - Steve McConnell](https://www.amazon.com/Code-Complete-Practical-Handbook-Construction/dp/0735619670)
- [The Pragmatic Programmer - Hunt & Thomas](https://www.amazon.com/Pragmatic-Programmer-Journeyman-Master/dp/020161622X)
