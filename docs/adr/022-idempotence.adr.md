# 22. Idempotence

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les opérations non-idempotentes causent des problèmes majeurs :
- **Duplications** : Retry après timeout crée des doublons
- **États incohérents** : Exécution partielle puis retry
- **Problèmes de concurrence** : Plusieurs instances traitent le même message
- **Debugging difficile** : Effets imprévisibles selon le nombre d'exécutions

```csharp
// ❌ NON-IDEMPOTENT : Chaque appel ajoute un montant
public void AddBonus(Guid userId, decimal amount)
{
    var user = _repository.GetById(userId);
    user.Balance += amount; // Problème si appelé 2 fois !
    _repository.Save(user);
}

// ❌ NON-IDEMPOTENT : Chaque appel crée un enregistrement
public void CreateOrder(CreateOrderCommand command)
{
    var order = new Order(command.CustomerId, command.Items);
    _repository.Add(order); // Doublon si retry !
}
```

## Décision

**Concevoir toutes les opérations pour être idempotentes : exécuter une opération plusieurs fois doit produire le même résultat qu'une seule exécution.**

### 1. Idempotency Key pour les créations

```csharp
/// <summary>
/// Commande de création avec clé d'idempotence.
/// </summary>
public record CreateOrderCommand(
    Guid IdempotencyKey,  // Fourni par le client
    Guid CustomerId,
    List<OrderItemDto> Items
);

public class CreateOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly IIdempotencyService _idempotency;
    
    /// <summary>
    /// Crée une commande de manière idempotente.
    /// </summary>
    public async Task<Order> HandleAsync(
        CreateOrderCommand command, 
        CancellationToken ct)
    {
        // Vérifie si déjà traité
        var existingOrder = await _repository.GetByIdempotencyKeyAsync(
            command.IdempotencyKey, ct);
        
        if (existingOrder is not null)
        {
            // Retourne le résultat précédent
            return existingOrder;
        }
        
        // Première exécution
        var order = Order.Create(
            command.IdempotencyKey,
            command.CustomerId,
            command.Items);
        
        await _repository.AddAsync(order, ct);
        
        return order;
    }
}

public class Order
{
    public Guid Id { get; private set; }
    public Guid IdempotencyKey { get; private set; } // Unique
    
    public static Order Create(
        Guid idempotencyKey, 
        Guid customerId, 
        List<OrderItemDto> items)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            CustomerId = customerId,
            // ...
        };
    }
}
```

### 2. Vérification d'état avant action

```csharp
// ❌ NON-IDEMPOTENT : Incrémente à chaque appel
public void ActivateTenant(Guid tenantId)
{
    var tenant = _repository.GetById(tenantId);
    tenant.Status = TenantStatus.Active;
    tenant.ActivationCount++; // Problème !
}

// ✅ IDEMPOTENT : Vérifie l'état avant d'agir
public class Tenant
{
    /// <summary>
    /// Active le tenant (idempotent).
    /// </summary>
    public void Activate()
    {
        // Si déjà actif, ne rien faire (idempotent)
        if (Status == TenantStatus.Active)
            return;
        
        if (Status == TenantStatus.Deleted)
            throw new InvalidOperationException(
                "Cannot activate a deleted tenant");
        
        Status = TenantStatus.Active;
        ActivatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Désactive le tenant (idempotent).
    /// </summary>
    public void Deactivate(string reason)
    {
        // Si déjà inactif, ne rien faire (idempotent)
        if (Status == TenantStatus.Inactive)
            return;
        
        Status = TenantStatus.Inactive;
        DeactivatedAt = DateTime.UtcNow;
        DeactivationReason = reason;
    }
}
```

### 3. Upsert au lieu de Insert/Update séparés

```csharp
// ❌ NON-IDEMPOTENT : Insert peut échouer au retry
public async Task AddApiKeyAsync(ApiKey apiKey, CancellationToken ct)
{
    await _context.ApiKeys.AddAsync(apiKey, ct);
    await _context.SaveChangesAsync(ct);
}

// ✅ IDEMPOTENT : Upsert basé sur une clé naturelle
public async Task UpsertApiKeyAsync(ApiKey apiKey, CancellationToken ct)
{
    var existing = await _context.ApiKeys
        .FirstOrDefaultAsync(k => k.KeyHash == apiKey.KeyHash, ct);
    
    if (existing is null)
    {
        await _context.ApiKeys.AddAsync(apiKey, ct);
    }
    else
    {
        // Met à jour les propriétés modifiables
        existing.UpdateFrom(apiKey);
    }
    
    await _context.SaveChangesAsync(ct);
}

// ✅ IDEMPOTENT : Avec ExecuteUpdate (EF Core 7+)
public async Task UpsertConfigurationAsync(
    string key, 
    string value, 
    CancellationToken ct)
{
    var affected = await _context.Configurations
        .Where(c => c.Key == key)
        .ExecuteUpdateAsync(c => c.SetProperty(x => x.Value, value), ct);
    
    if (affected == 0)
    {
        await _context.Configurations.AddAsync(
            new Configuration(key, value), ct);
        await _context.SaveChangesAsync(ct);
    }
}
```

### 4. Opérations monétaires idempotentes

```csharp
// ❌ NON-IDEMPOTENT : Double crédit possible
public async Task CreditAccountAsync(Guid accountId, decimal amount)
{
    var account = await _repository.GetByIdAsync(accountId);
    account.Balance += amount; // Dangereux !
    await _repository.SaveAsync(account);
}

// ✅ IDEMPOTENT : Transaction avec ID unique
public record CreditCommand(
    Guid TransactionId,  // ID unique fourni par l'appelant
    Guid AccountId,
    decimal Amount,
    string Description
);

public class AccountService
{
    /// <summary>
    /// Crédite un compte de manière idempotente.
    /// </summary>
    public async Task<Transaction> CreditAsync(
        CreditCommand command, 
        CancellationToken ct)
    {
        // Vérifie si la transaction existe déjà
        var existing = await _transactionRepository
            .GetByIdAsync(command.TransactionId, ct);
        
        if (existing is not null)
        {
            // Transaction déjà traitée, retourne le résultat
            return existing;
        }
        
        // Exécute la transaction dans une transaction DB
        await using var transaction = await _context
            .Database.BeginTransactionAsync(ct);
        
        try
        {
            var account = await _accountRepository
                .GetByIdForUpdateAsync(command.AccountId, ct);
            
            var tx = account.Credit(
                command.TransactionId, 
                command.Amount, 
                command.Description);
            
            await _transactionRepository.AddAsync(tx, ct);
            await _accountRepository.SaveAsync(account, ct);
            
            await transaction.CommitAsync(ct);
            
            return tx;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}

public class Account
{
    public decimal Balance { get; private set; }
    
    /// <summary>
    /// Crédite le compte et crée une transaction.
    /// </summary>
    public Transaction Credit(
        Guid transactionId, 
        decimal amount, 
        string description)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        
        Balance += amount;
        
        return new Transaction(
            transactionId,
            Id,
            TransactionType.Credit,
            amount,
            Balance, // Solde après opération
            description);
    }
}
```

### 5. Messages et événements idempotents

```csharp
/// <summary>
/// Handler de message idempotent.
/// </summary>
public class OrderCreatedHandler : IMessageHandler<OrderCreatedEvent>
{
    private readonly IProcessedMessageRepository _processed;
    private readonly IEmailService _emailService;
    
    public async Task HandleAsync(
        OrderCreatedEvent message, 
        CancellationToken ct)
    {
        // Vérifie si le message a déjà été traité
        if (await _processed.ExistsAsync(message.MessageId, ct))
        {
            // Message déjà traité, skip
            return;
        }
        
        // Traite le message
        await _emailService.SendOrderConfirmationAsync(
            message.OrderId, 
            message.CustomerEmail, 
            ct);
        
        // Marque comme traité (avec TTL pour cleanup)
        await _processed.MarkAsProcessedAsync(
            message.MessageId, 
            TimeSpan.FromDays(7), 
            ct);
    }
}

/// <summary>
/// Repository pour suivre les messages traités.
/// </summary>
public interface IProcessedMessageRepository
{
    Task<bool> ExistsAsync(Guid messageId, CancellationToken ct);
    Task MarkAsProcessedAsync(Guid messageId, TimeSpan ttl, CancellationToken ct);
}

/// <summary>
/// Implémentation Redis avec TTL automatique.
/// </summary>
public class RedisProcessedMessageRepository : IProcessedMessageRepository
{
    private readonly IDatabase _redis;
    
    public async Task<bool> ExistsAsync(Guid messageId, CancellationToken ct)
    {
        return await _redis.KeyExistsAsync($"processed:{messageId}");
    }
    
    public async Task MarkAsProcessedAsync(
        Guid messageId, 
        TimeSpan ttl, 
        CancellationToken ct)
    {
        await _redis.StringSetAsync(
            $"processed:{messageId}",
            DateTime.UtcNow.ToString("O"),
            ttl);
    }
}
```

### 6. API HTTP idempotentes

```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    /// <summary>
    /// Crée une commande (idempotent via header).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        CancellationToken ct)
    {
        var command = new CreateOrderCommand(
            idempotencyKey,
            request.CustomerId,
            request.Items);
        
        var result = await _mediator.Send(command, ct);
        
        // 201 Created si nouvelle, 200 OK si existante
        if (result.IsNew)
        {
            return CreatedAtAction(
                nameof(GetOrder), 
                new { id = result.Order.Id }, 
                result.Order.ToDto());
        }
        
        return Ok(result.Order.ToDto());
    }
    
    /// <summary>
    /// PUT est naturellement idempotent (remplacement complet).
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrderDto>> UpdateOrder(
        Guid id,
        [FromBody] UpdateOrderRequest request,
        CancellationToken ct)
    {
        // PUT remplace entièrement - idempotent par nature
        var command = new UpdateOrderCommand(id, request);
        var order = await _mediator.Send(command, ct);
        return Ok(order.ToDto());
    }
    
    /// <summary>
    /// DELETE est naturellement idempotent.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteOrder(Guid id, CancellationToken ct)
    {
        // DELETE est idempotent - supprimer 2x = même résultat
        var command = new DeleteOrderCommand(id);
        var deleted = await _mediator.Send(command, ct);
        
        // 204 que ce soit la 1ère ou Nième suppression
        return NoContent();
    }
}
```

### 7. Middleware d'idempotence générique

```csharp
/// <summary>
/// Middleware qui gère l'idempotence automatiquement.
/// </summary>
public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IIdempotencyStore _store;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Ne s'applique qu'aux POST/PATCH
        if (context.Request.Method != "POST" && 
            context.Request.Method != "PATCH")
        {
            await _next(context);
            return;
        }
        
        // Cherche le header Idempotency-Key
        if (!context.Request.Headers.TryGetValue(
            "Idempotency-Key", out var keyHeader))
        {
            await _next(context);
            return;
        }
        
        var idempotencyKey = keyHeader.ToString();
        
        // Vérifie si déjà traité
        var cachedResponse = await _store.GetAsync(idempotencyKey);
        if (cachedResponse is not null)
        {
            // Rejoue la réponse cachée
            context.Response.StatusCode = cachedResponse.StatusCode;
            context.Response.ContentType = cachedResponse.ContentType;
            await context.Response.WriteAsync(cachedResponse.Body);
            return;
        }
        
        // Capture la réponse
        var originalBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;
        
        await _next(context);
        
        // Cache la réponse pour les futurs replays
        memoryStream.Position = 0;
        var responseBody = await new StreamReader(memoryStream)
            .ReadToEndAsync();
        
        await _store.SetAsync(idempotencyKey, new CachedResponse
        {
            StatusCode = context.Response.StatusCode,
            ContentType = context.Response.ContentType,
            Body = responseBody
        }, TimeSpan.FromHours(24));
        
        // Écrit la réponse originale
        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }
}
```

## Conséquences

### Positives

- **Résilience** : Retry sans effets indésirables
- **Simplicité** : Pas besoin de traquer "déjà exécuté ?"
- **Cohérence** : État prévisible même avec pannes
- **Scalabilité** : Messages peuvent être réessayés
- **Debugging** : Comportement reproductible

### Négatives

- **Stockage** : Besoin de tracker les clés d'idempotence
  - *Mitigation* : TTL pour auto-nettoyage
- **Complexité initiale** : Design plus réfléchi
  - *Mitigation* : Patterns réutilisables (middleware, base classes)
- **Latence** : Vérification avant chaque opération
  - *Mitigation* : Cache rapide (Redis)

### Neutres

- L'idempotence est une propriété fondamentale des systèmes distribués fiables

## Alternatives considérées

### Option A : Transactions distribuées (2PC)

- **Description** : Utiliser des transactions distribuées pour garantir l'atomicité
- **Avantages** : Garanties fortes
- **Inconvénients** : Lent, complexe, single point of failure
- **Raison du rejet** : Ne scale pas, l'idempotence est plus pratique

### Option B : Deduplication côté message broker

- **Description** : Laisser Kafka/RabbitMQ gérer la déduplication
- **Avantages** : Géré par l'infrastructure
- **Inconvénients** : Ne couvre pas tous les cas (API HTTP)
- **Raison du rejet** : Solution partielle, idempotence applicative nécessaire

## Références

- [Idempotency Patterns - Martin Fowler](https://martinfowler.com/articles/patterns-of-distributed-systems/idempotent-receiver.html)
- [Stripe Idempotency](https://stripe.com/docs/api/idempotent_requests)
- [HTTP Idempotency - RFC 7231](https://tools.ietf.org/html/rfc7231#section-4.2.2)
