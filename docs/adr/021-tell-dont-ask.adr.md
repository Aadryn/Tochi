# 21. Tell, Don't Ask (TDA)

Date: 2025-12-21

## Statut

Accepté

## Contexte

Le pattern "Ask" viole l'encapsulation et disperse la logique :

```csharp
// ❌ ASK : Interroger puis agir (logique à l'extérieur)
public class OrderController
{
    public void ProcessOrder(Order order)
    {
        // Demande l'état...
        if (order.Status == OrderStatus.Pending)
        {
            // ... puis décide quoi faire
            if (order.Items.Count > 0)
            {
                if (order.TotalAmount > 0)
                {
                    if (order.Customer != null)
                    {
                        order.Status = OrderStatus.Confirmed;
                        order.ConfirmedAt = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}
```

Problèmes :
- **Logique dispersée** : La logique de confirmation est dans le contrôleur
- **Duplication** : Cette logique sera dupliquée ailleurs
- **Violation encapsulation** : L'extérieur connaît les détails internes
- **Difficile à tester** : Tester toutes les combinaisons

## Décision

**Appliquer le principe "Tell, Don't Ask" : dire à un objet quoi faire plutôt que lui demander son état pour décider à sa place.**

### 1. Encapsuler la logique dans l'objet

```csharp
// ❌ ASK : Demander puis agir
if (order.Status == OrderStatus.Pending &&
    order.Items.Count > 0 &&
    order.TotalAmount > 0)
{
    order.Status = OrderStatus.Confirmed;
}

// ✅ TELL : Dire quoi faire
public class Order
{
    public OrderStatus Status { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    
    /// <summary>
    /// Confirme la commande si elle est valide.
    /// </summary>
    /// <exception cref="InvalidOperationException">Si la commande ne peut pas être confirmée.</exception>
    public void Confirm()
    {
        // La logique est encapsulée dans l'objet
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot confirm order in status {Status}");
        
        if (!_items.Any())
            throw new InvalidOperationException(
                "Cannot confirm order with no items");
        
        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }
}

// Utilisation simple
order.Confirm();
```

### 2. Comportements riches dans les entités

```csharp
// ❌ ASK : Logique anémique
public class Tenant
{
    public TenantStatus Status { get; set; }
    public DateTime? DeactivatedAt { get; set; }
    public string? DeactivationReason { get; set; }
}

// Dans le service...
if (tenant.Status == TenantStatus.Active)
{
    tenant.Status = TenantStatus.Inactive;
    tenant.DeactivatedAt = DateTime.UtcNow;
    tenant.DeactivationReason = reason;
}

// ✅ TELL : Comportements encapsulés
public class Tenant
{
    public TenantStatus Status { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }
    public string? DeactivationReason { get; private set; }
    
    /// <summary>
    /// Désactive le tenant avec une raison obligatoire.
    /// </summary>
    public void Deactivate(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        
        if (Status == TenantStatus.Inactive)
            return; // Idempotent
        
        if (Status == TenantStatus.Deleted)
            throw new InvalidOperationException(
                "Cannot deactivate a deleted tenant");
        
        Status = TenantStatus.Inactive;
        DeactivatedAt = DateTime.UtcNow;
        DeactivationReason = reason;
    }
    
    /// <summary>
    /// Réactive le tenant précédemment désactivé.
    /// </summary>
    public void Reactivate()
    {
        if (Status == TenantStatus.Active)
            return; // Idempotent
        
        if (Status == TenantStatus.Deleted)
            throw new InvalidOperationException(
                "Cannot reactivate a deleted tenant");
        
        Status = TenantStatus.Active;
        DeactivatedAt = null;
        DeactivationReason = null;
    }
}
```

### 3. Éviter les getters pour décider

```csharp
// ❌ ASK : Getter puis condition
public class ShoppingCart
{
    public decimal TotalAmount { get; }
    public bool HasDiscount { get; }
    public decimal DiscountPercentage { get; }
}

// À l'extérieur...
var finalAmount = cart.TotalAmount;
if (cart.HasDiscount)
{
    finalAmount = cart.TotalAmount * (1 - cart.DiscountPercentage / 100);
}

// ✅ TELL : Méthode qui encapsule le calcul
public class ShoppingCart
{
    private decimal _totalAmount;
    private Discount? _discount;
    
    /// <summary>
    /// Calcule le montant final après application des remises.
    /// </summary>
    public decimal CalculateFinalAmount()
    {
        if (_discount is null)
            return _totalAmount;
        
        return _discount.Apply(_totalAmount);
    }
    
    /// <summary>
    /// Applique une remise au panier.
    /// </summary>
    public void ApplyDiscount(Discount discount)
    {
        ArgumentNullException.ThrowIfNull(discount);
        
        if (!discount.IsApplicableTo(this))
            throw new InvalidOperationException(
                "Discount is not applicable to this cart");
        
        _discount = discount;
    }
}
```

### 4. Conditions avec méthodes de requête

```csharp
// ❌ ASK : Conditions basées sur l'état
if (user.EmailVerifiedAt != null && 
    user.Status == UserStatus.Active &&
    user.FailedLoginAttempts < 5)
{
    // Allow login
}

// ✅ TELL : Méthode qui encapsule la condition
public class User
{
    public DateTime? EmailVerifiedAt { get; private set; }
    public UserStatus Status { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    
    /// <summary>
    /// Indique si l'utilisateur peut se connecter.
    /// </summary>
    public bool CanLogin()
    {
        return EmailVerifiedAt.HasValue &&
               Status == UserStatus.Active &&
               FailedLoginAttempts < MaxFailedLoginAttempts;
    }
    
    /// <summary>
    /// Vérifie et effectue la connexion.
    /// </summary>
    public void Login()
    {
        if (!CanLogin())
            throw new UnauthorizedException("User cannot login");
        
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
    }
    
    /// <summary>
    /// Enregistre une tentative de connexion échouée.
    /// </summary>
    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        
        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
            Lock("Too many failed login attempts");
    }
}
```

### 5. Collections avec comportements

```csharp
// ❌ ASK : Manipuler la collection depuis l'extérieur
public class Order
{
    public List<OrderItem> Items { get; set; } = new();
}

// À l'extérieur...
if (order.Items.Any(i => i.ProductId == productId))
{
    var item = order.Items.First(i => i.ProductId == productId);
    item.Quantity += quantity;
}
else
{
    order.Items.Add(new OrderItem(productId, quantity, price));
}

// ✅ TELL : Méthodes qui encapsulent les opérations
public class Order
{
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    
    /// <summary>
    /// Ajoute un produit à la commande (ou incrémente la quantité).
    /// </summary>
    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);
        
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new OrderItem(productId, quantity, unitPrice));
        }
    }
    
    /// <summary>
    /// Retire un produit de la commande.
    /// </summary>
    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        
        if (item is not null)
            _items.Remove(item);
    }
    
    /// <summary>
    /// Calcule le total de la commande.
    /// </summary>
    public decimal CalculateTotal()
    {
        return _items.Sum(i => i.CalculateSubtotal());
    }
}
```

### 6. Pattern Command avec TDA

```csharp
// ❌ ASK : Vérifier les conditions à l'extérieur
public class OrderService
{
    public void CancelOrder(Guid orderId)
    {
        var order = _repository.GetById(orderId);
        
        if (order.Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Cannot cancel shipped order");
        
        if (order.Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel delivered order");
        
        order.Status = OrderStatus.Cancelled;
        // etc.
    }
}

// ✅ TELL : L'objet gère ses propres règles
public class Order
{
    /// <summary>
    /// Annule la commande si elle peut l'être.
    /// </summary>
    public void Cancel(string reason)
    {
        EnsureCanBeCancelled();
        
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        
        // Émet un événement pour les effets secondaires
        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }
    
    private void EnsureCanBeCancelled()
    {
        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException(
                "Cannot cancel order that has been shipped");
        
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException(
                "Cannot cancel order that has been delivered");
        
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException(
                "Order is already cancelled");
    }
}
```

### 7. Éviter le Feature Envy

```csharp
// ❌ ASK : Feature Envy - utilise trop les données d'un autre objet
public class InvoiceCalculator
{
    public decimal CalculateTotal(Customer customer, List<InvoiceLine> lines)
    {
        var subtotal = lines.Sum(l => l.Quantity * l.UnitPrice);
        
        // Utilise les données du customer pour calculer
        if (customer.IsPreferredCustomer)
        {
            subtotal *= (1 - customer.PreferredDiscountRate);
        }
        
        if (customer.TaxExempt)
        {
            return subtotal;
        }
        
        return subtotal * (1 + customer.TaxRate);
    }
}

// ✅ TELL : Chaque objet calcule ce qui le concerne
public class Invoice
{
    private readonly List<InvoiceLine> _lines = new();
    private readonly Customer _customer;
    
    /// <summary>
    /// Calcule le total de la facture.
    /// </summary>
    public decimal CalculateTotal()
    {
        var subtotal = CalculateSubtotal();
        var afterDiscount = _customer.ApplyDiscount(subtotal);
        return _customer.ApplyTax(afterDiscount);
    }
    
    private decimal CalculateSubtotal()
    {
        return _lines.Sum(l => l.CalculateAmount());
    }
}

public class Customer
{
    /// <summary>
    /// Applique la remise client au montant.
    /// </summary>
    public decimal ApplyDiscount(decimal amount)
    {
        if (!IsPreferredCustomer)
            return amount;
        
        return amount * (1 - PreferredDiscountRate);
    }
    
    /// <summary>
    /// Applique la taxe au montant.
    /// </summary>
    public decimal ApplyTax(decimal amount)
    {
        if (TaxExempt)
            return amount;
        
        return amount * (1 + TaxRate);
    }
}
```

## Conséquences

### Positives

- **Encapsulation** : Logique métier au bon endroit
- **Réutilisation** : Comportements centralisés
- **Testabilité** : Tests unitaires sur les objets
- **Maintenance** : Un seul endroit à modifier
- **Cohésion** : Données et comportements ensemble

### Négatives

- **Objets plus gros** : Plus de méthodes dans les entités
  - *Mitigation* : Extraire en Value Objects ou Domain Services si nécessaire
- **Changement de mentalité** : Penser "comportement" plutôt que "données"
  - *Mitigation* : Formation et revues de code

### Neutres

- TDA favorise naturellement un modèle de domaine riche (DDD)

## Alternatives considérées

### Option A : Modèle anémique avec services

- **Description** : Entités avec uniquement des données, services avec la logique
- **Avantages** : Séparation claire données/logique
- **Inconvénients** : Logique dispersée, feature envy, duplication
- **Raison du rejet** : Viole les principes OO, difficile à maintenir

### Option B : Extension methods

- **Description** : Logique dans des extensions plutôt que l'objet
- **Avantages** : Objets légers
- **Inconvénients** : Pas d'accès aux membres privés, logique séparée
- **Raison du rejet** : Ne résout pas le problème d'encapsulation

## Références

- [Tell, Don't Ask - Martin Fowler](https://martinfowler.com/bliki/TellDontAsk.html)
- [Feature Envy - Refactoring Guru](https://refactoring.guru/smells/feature-envy)
- [Domain-Driven Design - Eric Evans](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)
