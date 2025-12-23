# 12. Law of Demeter (Principe de connaissance minimale)

Date: 2025-12-21

## Statut

Accepté

## Contexte

Le couplage excessif entre objets crée des dépendances fragiles :
- **Train Wreck** : `order.Customer.Address.City.Name` - chaînes d'appels
- **Connaissance intime** : Une classe connaît la structure interne d'autres classes
- **Fragilité** : Changer une classe impacte toutes celles qui la traversent
- **Tests difficiles** : Mocker toute une chaîne d'objets

Ce type de code est symptomatique :

```csharp
// ❌ VIOLATION : Train wreck - connaissance de la structure interne
public string GetCustomerCity(Order order)
{
    return order.Customer.Address.City.Name;
    // Cette méthode connaît : Order → Customer → Address → City → Name
    // Si Address change, ce code casse !
}
```

## Décision

**Appliquer la Loi de Demeter : un objet ne doit interagir qu'avec ses voisins immédiats.**

### Principe

Une méthode M d'un objet O ne peut appeler que les méthodes de :
1. O lui-même (`this`)
2. Les paramètres de M
3. Les objets créés par M
4. Les attributs/propriétés directs de O
5. Les variables globales accessibles par O (à éviter)

### 1. Éliminer les chaînes d'appels

```csharp
// ❌ VIOLATION : Train wreck
public decimal CalculateShipping(Order order)
{
    var city = order.Customer.Address.City.Name;
    var country = order.Customer.Address.Country.Code;
    return _shippingCalculator.Calculate(city, country);
}

// ✅ DEMETER : Demander directement ce dont on a besoin
public decimal CalculateShipping(Order order)
{
    var shippingInfo = order.GetShippingInfo(); // Order expose ce qui est nécessaire
    return _shippingCalculator.Calculate(shippingInfo);
}

// Dans Order.cs
public class Order
{
    public ShippingInfo GetShippingInfo()
    {
        return new ShippingInfo(
            Customer.Address.City.Name,
            Customer.Address.Country.Code,
            Customer.Address.PostalCode);
    }
}
```

### 2. Tell, Don't Ask

```csharp
// ❌ VIOLATION : Demander des données pour décider
public void ProcessOrder(Order order)
{
    if (order.Customer.LoyaltyPoints > 1000 
        && order.Customer.MemberSince.Year < 2020
        && order.Items.Sum(i => i.Total) > 500)
    {
        order.ApplyDiscount(0.15m);
    }
}

// ✅ DEMETER : Dire à l'objet quoi faire
public void ProcessOrder(Order order)
{
    order.ApplyLoyaltyDiscountIfEligible();
}

// Dans Order.cs - l'objet décide lui-même
public class Order
{
    public void ApplyLoyaltyDiscountIfEligible()
    {
        if (Customer.IsEligibleForLoyaltyDiscount() && Total > 500)
        {
            ApplyDiscount(0.15m);
        }
    }
}

// Dans Customer.cs
public class Customer
{
    public bool IsEligibleForLoyaltyDiscount()
    {
        return LoyaltyPoints > 1000 && MemberSince.Year < DateTime.UtcNow.Year - 5;
    }
}
```

### 3. Encapsuler les collections

```csharp
// ❌ VIOLATION : Exposer la collection interne
public class Order
{
    public List<OrderItem> Items { get; set; } // Expose l'implémentation !
}

// Client peut faire n'importe quoi :
order.Items.Clear();
order.Items.Add(new OrderItem { Price = -100 }); // Prix négatif !

// ✅ DEMETER : Encapsuler et exposer des opérations
public class Order
{
    private readonly List<OrderItem> _items = new();
    
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    public void AddItem(Product product, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive");
        
        _items.Add(new OrderItem(product, quantity));
        RecalculateTotal();
    }
    
    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotal();
        }
    }
    
    public decimal Total => _items.Sum(i => i.Total);
}
```

### 4. Façades pour systèmes complexes

```csharp
// ❌ VIOLATION : Client connaît toute la structure
public class OrderController
{
    public async Task<IActionResult> Submit(OrderRequest request)
    {
        var customer = await _customerRepository.GetAsync(request.CustomerId);
        var inventory = await _inventoryService.CheckAvailability(request.Items);
        var pricing = _pricingEngine.Calculate(request.Items, customer.Tier);
        var payment = await _paymentGateway.Charge(customer.PaymentMethod, pricing.Total);
        var order = await _orderRepository.CreateAsync(/* ... */);
        await _notificationService.SendConfirmation(customer.Email, order);
        
        return Ok(order.Id);
    }
}

// ✅ DEMETER : Façade qui encapsule la complexité
public class OrderController
{
    private readonly IOrderFacade _orderFacade;
    
    public async Task<IActionResult> Submit(OrderRequest request, CancellationToken ct)
    {
        var orderId = await _orderFacade.SubmitOrderAsync(request, ct);
        return Ok(orderId);
    }
}

// La façade gère la complexité interne
public class OrderFacade : IOrderFacade
{
    public async Task<Guid> SubmitOrderAsync(OrderRequest request, CancellationToken ct)
    {
        // Toute la logique complexe est ici, encapsulée
        var customer = await _customerRepository.GetAsync(request.CustomerId, ct);
        // ...
        return order.Id;
    }
}
```

### 5. DTOs pour traverser les couches

```csharp
// ✅ DEMETER : DTO aplati pour éviter les chaînes
public record OrderSummaryDto(
    Guid OrderId,
    string CustomerName,      // Pas Customer.Name
    string ShippingCity,      // Pas Customer.Address.City.Name
    string ShippingCountry,   // Pas Customer.Address.Country.Name
    decimal Total,
    int ItemCount
);

// Mapping dans le repository ou le handler
public OrderSummaryDto MapToSummary(Order order)
{
    return new OrderSummaryDto(
        order.Id,
        order.Customer.FullName,
        order.Customer.Address.City.Name,
        order.Customer.Address.Country.Name,
        order.Total,
        order.Items.Count);
}
```

### 6. Exceptions à la règle

La Loi de Demeter est plus souple pour :

```csharp
// ✅ OK : Fluent interfaces (conçues pour le chaînage)
var query = _context.Orders
    .Where(o => o.Status == OrderStatus.Pending)
    .Include(o => o.Customer)
    .OrderBy(o => o.CreatedAt)
    .Take(10);

// ✅ OK : Builders
var order = Order.Create()
    .WithCustomer(customerId)
    .WithItem(productId, quantity)
    .WithShipping(addressId)
    .Build();

// ✅ OK : DTOs/Records (structures de données pures)
var address = orderDto.ShippingAddress.City; // DTO n'a pas de comportement
```

## Conséquences

### Positives

- **Découplage** : Les classes ne dépendent que de leurs voisins immédiats
- **Encapsulation** : La structure interne reste privée
- **Maintenabilité** : Modifier une classe n'impacte pas les classes distantes
- **Testabilité** : Moins de mocks à créer
- **Lisibilité** : Code plus expressif (`order.Ship()` vs `order.Status = Shipped`)

### Négatives

- **Plus de méthodes** : Les objets exposent plus de méthodes déléguées
  - *Mitigation* : Ces méthodes encapsulent de la logique métier utile
- **Façades** : Parfois nécessité de créer des façades
  - *Mitigation* : Les façades simplifient l'API pour les clients
- **Risque de Feature Envy** : Trop de logique peut migrer dans une seule classe
  - *Mitigation* : Répartir la logique dans les classes appropriées

### Neutres

- Demande une réflexion sur la distribution des responsabilités

## Alternatives considérées

### Option A : Accès direct autorisé

- **Description** : Permettre les chaînes d'appels pour simplicité
- **Avantages** : Moins de méthodes à écrire
- **Inconvénients** : Couplage fort, fragilité, tests difficiles
- **Raison du rejet** : Le couplage crée une dette technique majeure

### Option B : Demeter strict absolu

- **Description** : Appliquer Demeter même aux DTOs et fluent APIs
- **Avantages** : Découplage maximal
- **Inconvénients** : Code verbeux, contre-productif pour les patterns établis
- **Raison du rejet** : Le pragmatisme prime sur le dogmatisme

## Références

- [Law of Demeter - Wikipedia](https://en.wikipedia.org/wiki/Law_of_Demeter)
- [Tell, Don't Ask - Martin Fowler](https://martinfowler.com/bliki/TellDontAsk.html)
- [The Paperboy, The Wallet, and The Law Of Demeter](https://www.ccs.neu.edu/home/lieber/LoD/LoD-2011-Zurich.pdf)
