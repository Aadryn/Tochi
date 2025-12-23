# 10. Separation of Concerns (SoC)

Date: 2025-12-21

## Statut

Accepté

## Contexte

Le mélange de responsabilités dans le code crée des problèmes majeurs :
- **Classes monolithiques** : Une classe gère UI, logique métier et accès données
- **Méthodes "fourre-tout"** : Une méthode fait validation, calcul, persistance et notification
- **Couplage fort** : Modifier un aspect impacte tous les autres
- **Tests impossibles** : Impossible de tester un aspect sans les autres

Exemple typique de violation :

```csharp
// ❌ VIOLATION : Tout mélangé dans une seule méthode
public async Task<IActionResult> CreateOrder(OrderRequest request)
{
    // Validation
    if (string.IsNullOrEmpty(request.CustomerEmail)) 
        return BadRequest("Email required");
    
    // Logique métier
    var total = request.Items.Sum(i => i.Price * i.Quantity);
    var tax = total * 0.20m;
    
    // Accès données
    var order = new Order { Total = total + tax };
    _context.Orders.Add(order);
    await _context.SaveChangesAsync();
    
    // Notification
    await _emailService.SendAsync(request.CustomerEmail, "Order created");
    
    // Logging
    _logger.LogInformation("Order {Id} created", order.Id);
    
    return Ok(order);
}
```

## Décision

**Appliquer le principe de Séparation des Préoccupations : chaque module/classe/méthode ne doit gérer qu'un seul aspect du système.**

### 1. Séparation par couche

```
┌─────────────────────────────────────────┐
│           Presentation Layer             │  → Affichage, API endpoints
├─────────────────────────────────────────┤
│           Application Layer              │  → Orchestration, use cases
├─────────────────────────────────────────┤
│             Domain Layer                 │  → Logique métier pure
├─────────────────────────────────────────┤
│          Infrastructure Layer            │  → BDD, APIs externes, IO
└─────────────────────────────────────────┘
```

### 2. Séparation par responsabilité

```csharp
// ✅ SOC : Chaque classe a une responsabilité unique

// Validation séparée
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new OrderItemValidator());
    }
}

// Logique métier séparée
public class OrderPricingService
{
    private readonly ITaxCalculator _taxCalculator;
    
    public OrderPricing CalculatePricing(IEnumerable<OrderItem> items)
    {
        var subtotal = items.Sum(i => i.Price * i.Quantity);
        var tax = _taxCalculator.Calculate(subtotal);
        return new OrderPricing(subtotal, tax, subtotal + tax);
    }
}

// Persistance séparée
public class OrderRepository : IOrderRepository
{
    public async Task<Order> SaveAsync(Order order, CancellationToken ct)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);
        return order;
    }
}

// Notification séparée
public class OrderNotificationService : IOrderNotificationService
{
    public async Task NotifyOrderCreatedAsync(Order order, CancellationToken ct)
    {
        await _emailService.SendAsync(
            order.CustomerEmail, 
            "Order Confirmation",
            BuildOrderConfirmationEmail(order));
    }
}

// Orchestration dans le handler
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _repository;
    private readonly OrderPricingService _pricingService;
    private readonly IOrderNotificationService _notificationService;
    
    public async Task<OrderResponse> Handle(
        CreateOrderCommand request, 
        CancellationToken ct)
    {
        // Chaque étape déléguée à son responsable
        var pricing = _pricingService.CalculatePricing(request.Items);
        
        var order = new Order(request.CustomerEmail, request.Items, pricing);
        
        await _repository.SaveAsync(order, ct);
        
        await _notificationService.NotifyOrderCreatedAsync(order, ct);
        
        return new OrderResponse(order.Id, pricing.Total);
    }
}
```

### 3. Cross-Cutting Concerns avec Middleware/Behaviors

```csharp
// ✅ SOC : Les aspects transverses sont centralisés

// Logging via behavior MediatR
public class LoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        
        var response = await next();
        
        _logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
        
        return response;
    }
}

// Validation via behavior MediatR
public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();
        
        if (failures.Any())
            throw new ValidationException(failures);
        
        return await next();
    }
}

// Transaction via behavior
public class TransactionBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(ct);
        
        var response = await next();
        
        await transaction.CommitAsync(ct);
        
        return response;
    }
}
```

### 4. Séparation UI / Logique

```csharp
// ❌ VIOLATION : Logique métier dans le controller
[HttpPost]
public IActionResult ApplyDiscount(Guid orderId, string couponCode)
{
    var order = _context.Orders.Find(orderId);
    
    // Logique métier dans le controller !
    if (couponCode == "SUMMER20")
        order.Total *= 0.80m;
    else if (couponCode == "VIP50")
        order.Total *= 0.50m;
    
    _context.SaveChanges();
    return Ok(order);
}

// ✅ SOC : Controller ne fait que router
[HttpPost]
public async Task<IActionResult> ApplyDiscount(
    Guid orderId, 
    string couponCode,
    CancellationToken ct)
{
    var command = new ApplyDiscountCommand(orderId, couponCode);
    var result = await _mediator.Send(command, ct);
    return Ok(result);
}
```

### 5. Règles de séparation

| Préoccupation | Responsable | Ne doit PAS faire |
|--------------|-------------|-------------------|
| Routing HTTP | Controllers | Logique métier |
| Validation | Validators | Accès BDD |
| Orchestration | Handlers | Calculs métier complexes |
| Logique métier | Domain Services | Accès infrastructure |
| Persistance | Repositories | Validation métier |
| Notification | Notification Services | Décisions métier |
| Logging | Behaviors/Middleware | Logique applicative |

## Conséquences

### Positives

- **Testabilité** : Chaque aspect testable isolément
- **Maintenabilité** : Modification d'un aspect n'impacte pas les autres
- **Lisibilité** : Chaque fichier a un objectif clair
- **Réutilisabilité** : Les composants sont utilisables dans différents contextes
- **Évolutivité** : Ajouter/modifier un aspect sans toucher aux autres

### Négatives

- **Plus de fichiers** : Le code est distribué sur plus de fichiers
  - *Mitigation* : Organisation claire par feature/domaine
- **Indirection** : Le flux d'exécution traverse plusieurs classes
  - *Mitigation* : Nommage explicite, documentation du flux

### Neutres

- SoC est un principe fondamental qui sous-tend SOLID, Clean Architecture, etc.

## Alternatives considérées

### Option A : Scripts transactionnels

- **Description** : Tout le code d'un use case dans une méthode
- **Avantages** : Flux linéaire facile à suivre
- **Inconvénients** : Duplication, tests impossibles, maintenance difficile
- **Raison du rejet** : Ne scale pas au-delà d'un petit projet

### Option B : Séparation partielle

- **Description** : Séparer uniquement certains aspects (ex: validation)
- **Avantages** : Moins de fichiers
- **Inconvénients** : Incohérence, dette technique
- **Raison du rejet** : L'application incohérente crée de la confusion

## Références

- [Separation of Concerns - Wikipedia](https://en.wikipedia.org/wiki/Separation_of_concerns)
- [Clean Code - Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
- [Aspect-Oriented Programming - Wikipedia](https://en.wikipedia.org/wiki/Aspect-oriented_programming)
