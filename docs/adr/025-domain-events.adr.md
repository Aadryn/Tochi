# 25. Domain Events

Date: 2025-12-21

## Statut

Accepté

## Contexte

Le couplage direct entre les composants pose problème :
- **Dépendances rigides** : Le service A appelle directement B, C, D
- **Transactions longues** : Tout dans une seule transaction
- **Testabilité réduite** : Difficile de tester en isolation
- **Violation SRP** : Un service fait trop de choses

```csharp
// ❌ COUPLAGE FORT : Le service fait tout directement
public class OrderService
{
    public async Task CreateOrderAsync(CreateOrderCommand command)
    {
        // 1. Créer la commande
        var order = new Order(command);
        await _orderRepository.AddAsync(order);
        
        // 2. Mettre à jour le stock (couplage direct)
        await _inventoryService.ReserveStockAsync(order.Items);
        
        // 3. Envoyer email (couplage direct)
        await _emailService.SendOrderConfirmationAsync(order);
        
        // 4. Notifier analytics (couplage direct)
        await _analyticsService.TrackOrderCreatedAsync(order);
        
        // 5. Créer facture (couplage direct)
        await _billingService.CreateInvoiceAsync(order);
        
        // Le service connaît TOUS les effets secondaires !
    }
}
```

## Décision

**Utiliser les Domain Events pour découpler les composants et communiquer les changements d'état du domaine de manière réactive.**

### 1. Interface et classe de base

```csharp
/// <summary>
/// Marqueur pour les événements du domaine.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

/// <summary>
/// Classe de base pour les événements du domaine.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>
/// Interface pour les entités qui émettent des événements.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```

### 2. Entité avec événements

```csharp
/// <summary>
/// Classe de base pour les entités avec événements.
/// </summary>
public abstract class Entity : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => 
        _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// Entité Tenant avec événements.
/// </summary>
public class Tenant : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public TenantStatus Status { get; private set; }
    
    private Tenant() { }
    
    /// <summary>
    /// Crée un nouveau tenant et émet un événement.
    /// </summary>
    public static Tenant Create(string name, string contactEmail)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Status = TenantStatus.Active
        };
        
        // Émet l'événement de création
        tenant.AddDomainEvent(new TenantCreatedEvent(
            tenant.Id, 
            tenant.Name, 
            contactEmail));
        
        return tenant;
    }
    
    /// <summary>
    /// Active le tenant et émet un événement.
    /// </summary>
    public void Activate()
    {
        if (Status == TenantStatus.Active)
            return;
        
        var previousStatus = Status;
        Status = TenantStatus.Active;
        
        AddDomainEvent(new TenantActivatedEvent(Id, previousStatus));
    }
    
    /// <summary>
    /// Désactive le tenant et émet un événement.
    /// </summary>
    public void Deactivate(string reason)
    {
        if (Status == TenantStatus.Inactive)
            return;
        
        Status = TenantStatus.Inactive;
        
        AddDomainEvent(new TenantDeactivatedEvent(Id, reason));
    }
    
    /// <summary>
    /// Met à jour le quota et émet un événement si dépassé.
    /// </summary>
    public void RecordUsage(int tokensUsed)
    {
        CurrentUsage += tokensUsed;
        
        if (CurrentUsage >= MonthlyQuota)
        {
            AddDomainEvent(new TenantQuotaExceededEvent(
                Id, 
                CurrentUsage, 
                MonthlyQuota));
        }
    }
}
```

### 3. Définition des événements

```csharp
/// <summary>
/// Événement émis lors de la création d'un tenant.
/// </summary>
public sealed record TenantCreatedEvent(
    Guid TenantId,
    string TenantName,
    string ContactEmail
) : DomainEvent;

/// <summary>
/// Événement émis lors de l'activation d'un tenant.
/// </summary>
public sealed record TenantActivatedEvent(
    Guid TenantId,
    TenantStatus PreviousStatus
) : DomainEvent;

/// <summary>
/// Événement émis lors de la désactivation d'un tenant.
/// </summary>
public sealed record TenantDeactivatedEvent(
    Guid TenantId,
    string Reason
) : DomainEvent;

/// <summary>
/// Événement émis lors du dépassement de quota.
/// </summary>
public sealed record TenantQuotaExceededEvent(
    Guid TenantId,
    int CurrentUsage,
    int MonthlyQuota
) : DomainEvent;

/// <summary>
/// Événement émis lors de la création d'une requête LLM.
/// </summary>
public sealed record LlmRequestCompletedEvent(
    Guid RequestId,
    Guid TenantId,
    Guid ApiKeyId,
    string Model,
    int InputTokens,
    int OutputTokens,
    TimeSpan Latency,
    bool Success
) : DomainEvent;
```

### 4. Handler d'événements

```csharp
/// <summary>
/// Interface pour les handlers d'événements.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct);
}

/// <summary>
/// Handler qui envoie un email de bienvenue.
/// </summary>
public class SendWelcomeEmailOnTenantCreated : IDomainEventHandler<TenantCreatedEvent>
{
    private readonly IEmailService _emailService;
    
    public SendWelcomeEmailOnTenantCreated(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    public async Task HandleAsync(TenantCreatedEvent evt, CancellationToken ct)
    {
        await _emailService.SendWelcomeEmailAsync(
            evt.ContactEmail, 
            evt.TenantName, 
            ct);
    }
}

/// <summary>
/// Handler qui crée des statistiques initiales.
/// </summary>
public class CreateInitialStatsOnTenantCreated : IDomainEventHandler<TenantCreatedEvent>
{
    private readonly IStatisticsService _statisticsService;
    
    public async Task HandleAsync(TenantCreatedEvent evt, CancellationToken ct)
    {
        await _statisticsService.InitializeStatsForTenantAsync(
            evt.TenantId, 
            ct);
    }
}

/// <summary>
/// Handler qui notifie les administrateurs lors du dépassement de quota.
/// </summary>
public class NotifyAdminsOnQuotaExceeded : IDomainEventHandler<TenantQuotaExceededEvent>
{
    private readonly INotificationService _notificationService;
    
    public async Task HandleAsync(TenantQuotaExceededEvent evt, CancellationToken ct)
    {
        await _notificationService.NotifyQuotaExceededAsync(
            evt.TenantId,
            evt.CurrentUsage,
            evt.MonthlyQuota,
            ct);
    }
}

/// <summary>
/// Handler qui enregistre les métriques de requête LLM.
/// </summary>
public class RecordMetricsOnLlmRequestCompleted : IDomainEventHandler<LlmRequestCompletedEvent>
{
    private readonly IMetricsService _metricsService;
    
    public async Task HandleAsync(LlmRequestCompletedEvent evt, CancellationToken ct)
    {
        await _metricsService.RecordRequestAsync(
            evt.TenantId,
            evt.Model,
            evt.InputTokens,
            evt.OutputTokens,
            evt.Latency,
            evt.Success,
            ct);
    }
}
```

### 5. Dispatcher d'événements

```csharp
/// <summary>
/// Interface pour le dispatcher d'événements.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct);
}

/// <summary>
/// Dispatcher qui utilise MediatR.
/// </summary>
public class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<MediatRDomainEventDispatcher> _logger;
    
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> events, 
        CancellationToken ct)
    {
        foreach (var domainEvent in events)
        {
            _logger.LogDebug(
                "Dispatching domain event {EventType} - {EventId}",
                domainEvent.GetType().Name,
                domainEvent.EventId);
            
            await _mediator.Publish(domainEvent, ct);
        }
    }
}

/// <summary>
/// Dispatcher manuel avec résolution DI.
/// </summary>
public class SimpleDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> events, 
        CancellationToken ct)
    {
        foreach (var domainEvent in events)
        {
            var eventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            
            var handlers = _serviceProvider.GetServices(handlerType);
            
            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("HandleAsync");
                await (Task)method!.Invoke(handler, new object[] { domainEvent, ct })!;
            }
        }
    }
}
```

### 6. Intégration avec Unit of Work

```csharp
/// <summary>
/// Unit of Work qui dispatch les événements après SaveChanges.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly LLMProxyDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    
    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        // 1. Collecter les événements avant save
        var domainEvents = _context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();
        
        // 2. Clear les événements des entités
        foreach (var entry in _context.ChangeTracker.Entries<IHasDomainEvents>())
        {
            entry.Entity.ClearDomainEvents();
        }
        
        // 3. Sauvegarder dans la base
        var result = await _context.SaveChangesAsync(ct);
        
        // 4. Dispatcher les événements APRÈS commit
        await _eventDispatcher.DispatchAsync(domainEvents, ct);
        
        return result;
    }
}
```

### 7. Événements asynchrones (Outbox Pattern)

```csharp
/// <summary>
/// Événement persisté pour traitement asynchrone.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
}

/// <summary>
/// Unit of Work avec Outbox Pattern.
/// </summary>
public class OutboxUnitOfWork : IUnitOfWork
{
    private readonly LLMProxyDbContext _context;
    
    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        // 1. Collecter les événements
        var domainEvents = _context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();
        
        // 2. Convertir en OutboxMessages
        foreach (var evt in domainEvents)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = evt.EventId,
                EventType = evt.GetType().AssemblyQualifiedName!,
                Payload = JsonSerializer.Serialize(evt, evt.GetType()),
                OccurredAt = evt.OccurredAt
            };
            
            _context.OutboxMessages.Add(outboxMessage);
        }
        
        // 3. Clear les événements
        foreach (var entry in _context.ChangeTracker.Entries<IHasDomainEvents>())
        {
            entry.Entity.ClearDomainEvents();
        }
        
        // 4. Tout est sauvegardé dans la même transaction
        return await _context.SaveChangesAsync(ct);
    }
}

/// <summary>
/// Background service qui traite les événements de l'outbox.
/// </summary>
public class OutboxProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<LLMProxyDbContext>();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
            
            // Récupérer les messages non traités
            var messages = await context.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.OccurredAt)
                .Take(100)
                .ToListAsync(stoppingToken);
            
            foreach (var message in messages)
            {
                try
                {
                    var eventType = Type.GetType(message.EventType);
                    var domainEvent = (IDomainEvent)JsonSerializer.Deserialize(
                        message.Payload, 
                        eventType!)!;
                    
                    await dispatcher.DispatchAsync(new[] { domainEvent }, stoppingToken);
                    
                    message.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    message.RetryCount++;
                    _logger.LogError(ex, "Failed to process outbox message {Id}", message.Id);
                }
            }
            
            await context.SaveChangesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

### 8. Service découplé avec événements

```csharp
// ✅ SERVICE DÉCOUPLÉ : Ne connaît que son domaine
public class TenantService
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Tenant> CreateTenantAsync(
        CreateTenantCommand command, 
        CancellationToken ct)
    {
        // Le service ne fait QUE son travail
        var tenant = Tenant.Create(command.Name, command.ContactEmail);
        
        await _repository.AddAsync(tenant, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Les événements (TenantCreatedEvent) sont dispatchés automatiquement
        // par le UnitOfWork après SaveChanges
        
        return tenant;
    }
}

// Les handlers font les effets secondaires de manière découplée
// - SendWelcomeEmailOnTenantCreated
// - CreateInitialStatsOnTenantCreated
// - NotifySlackOnTenantCreated
// etc.
```

## Conséquences

### Positives

- **Découplage** : Les composants ne se connaissent pas directement
- **Extensibilité** : Ajouter un effet secondaire = ajouter un handler
- **Testabilité** : Chaque handler testable en isolation
- **Single Responsibility** : Chaque handler a une seule responsabilité
- **Audit** : Les événements forment un journal naturel

### Négatives

- **Complexité** : Plus d'indirection
  - *Mitigation* : Documentation claire des événements et handlers
- **Debugging** : Flux moins évident
  - *Mitigation* : Logging des événements avec correlation ID
- **Eventual Consistency** : Avec Outbox, les handlers sont asynchrones
  - *Mitigation* : Design pour la tolérance à l'eventual consistency

### Neutres

- Les Domain Events sont un concept fondamental du DDD

## Alternatives considérées

### Option A : Appels directs entre services

- **Description** : Service A appelle B, C, D directement
- **Avantages** : Simple, flux évident
- **Inconvénients** : Couplage fort, transactions longues
- **Raison du rejet** : Ne scale pas, maintenance difficile

### Option B : Message broker externe (RabbitMQ, Kafka)

- **Description** : Publier sur un broker pour découplage total
- **Avantages** : Découplage complet, scalabilité
- **Inconvénients** : Complexité infrastructure, eventual consistency forcée
- **Raison du rejet** : Over-engineering pour le cas d'usage actuel

## Références

- [Domain Events - Martin Fowler](https://martinfowler.com/eaaDev/DomainEvent.html)
- [Domain-Driven Design - Eric Evans](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)
- [Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [MediatR](https://github.com/jbogard/MediatR)
