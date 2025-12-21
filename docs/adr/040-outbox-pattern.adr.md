# 40. Outbox Pattern pour la publication fiable d'événements

Date: 2025-12-21

## Statut

Accepté

## Contexte

Lors de la sauvegarde d'un Aggregate avec des Domain Events :
- L'Aggregate est sauvegardé en base de données
- Les événements sont publiés sur un message broker (RabbitMQ, Kafka)

**Problème** : Ces deux opérations ne sont pas atomiques.

```csharp
// ❌ PROBLÈME : Double commit - pas atomique
public async Task SaveTenantAsync(Tenant tenant)
{
    // Transaction 1 : DB
    await _context.SaveChangesAsync(); // ✅ Réussit
    
    // Transaction 2 : Message Broker
    foreach (var domainEvent in tenant.DomainEvents)
    {
        await _messageBroker.PublishAsync(domainEvent); // ❌ Peut échouer !
    }
    
    // Si le broker échoue :
    // - Les données sont sauvegardées
    // - Les événements sont perdus
    // - Système incohérent !
}
```

### Scénarios de défaillance

```
SCÉNARIO 1 : Broker down
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Application   │───►│    Database     │───►│  Message Broker │
│   SaveAsync()   │    │   COMMIT ✅     │    │    DOWN ❌      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                       Données OK              Événement PERDU

SCÉNARIO 2 : Crash après DB commit
┌─────────────────┐    ┌─────────────────┐
│   Application   │───►│    Database     │
│   SaveAsync()   │    │   COMMIT ✅     │
│      CRASH ⚡   │    └─────────────────┘
└─────────────────┘    Événement jamais publié
```

## Décision

**Utiliser le pattern Outbox pour garantir la publication fiable des événements dans la même transaction que les données.**

### Architecture

```
┌────────────────────────────────────────────────────────────────────┐
│                        MÊME TRANSACTION                            │
│                                                                    │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐         │
│  │   Tenant     │    │   ApiKey     │    │   Outbox     │         │
│  │   UPDATE     │    │   INSERT     │    │   INSERT     │         │
│  └──────────────┘    └──────────────┘    └──────────────┘         │
│         │                  │                   │                   │
│         └──────────────────┴───────────────────┘                   │
│                            │                                       │
│                            ▼                                       │
│                   ┌──────────────┐                                 │
│                   │   COMMIT     │                                 │
│                   │  ATOMIQUE    │                                 │
│                   └──────────────┘                                 │
└────────────────────────────────────────────────────────────────────┘
                             │
                             │ Background Worker
                             ▼
                    ┌──────────────┐
                    │   Outbox     │
                    │   Processor  │
                    └──────────────┘
                             │
                             ▼
                    ┌──────────────┐
                    │   Message    │
                    │   Broker     │
                    └──────────────┘
```

### 1. Table Outbox

```csharp
/// <summary>
/// Entité représentant un message dans la table Outbox.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    
    /// <summary>
    /// Type complet de l'événement (pour désérialisation).
    /// </summary>
    public string Type { get; private set; } = string.Empty;
    
    /// <summary>
    /// Contenu JSON de l'événement.
    /// </summary>
    public string Content { get; private set; } = string.Empty;
    
    /// <summary>
    /// Date de création.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }
    
    /// <summary>
    /// Date de traitement (null si non traité).
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; private set; }
    
    /// <summary>
    /// Erreur lors du dernier traitement.
    /// </summary>
    public string? Error { get; private set; }
    
    /// <summary>
    /// Nombre de tentatives de traitement.
    /// </summary>
    public int RetryCount { get; private set; }
    
    private OutboxMessage() { }
    
    public static OutboxMessage Create<TEvent>(TEvent domainEvent)
        where TEvent : IDomainEvent
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(TEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
    
    public void MarkAsProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
        Error = null;
    }
    
    public void MarkAsFailed(string error)
    {
        Error = error;
        RetryCount++;
    }
}
```

### 2. Configuration EF Core

```csharp
/// <summary>
/// Configuration de la table Outbox.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "messaging");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Type)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(x => x.Content)
            .HasColumnType("jsonb")
            .IsRequired();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.Property(x => x.ProcessedAt);
        
        builder.Property(x => x.Error)
            .HasMaxLength(2000);
        
        // Index pour le traitement
        builder.HasIndex(x => x.ProcessedAt)
            .HasFilter("processed_at IS NULL")
            .HasDatabaseName("IX_outbox_messages_unprocessed");
        
        // Index pour le cleanup
        builder.HasIndex(x => x.ProcessedAt)
            .HasFilter("processed_at IS NOT NULL")
            .HasDatabaseName("IX_outbox_messages_processed");
    }
}
```

### 3. Intercepteur EF Core pour Outbox automatique

```csharp
/// <summary>
/// Intercepteur qui convertit automatiquement les Domain Events en messages Outbox.
/// </summary>
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        var context = eventData.Context;
        
        if (context is null)
            return await base.SavingChangesAsync(eventData, result, ct);
        
        // Collecter tous les Domain Events des Aggregates modifiés
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();
        
        // Convertir en messages Outbox
        var outboxMessages = aggregates
            .SelectMany(a => a.DomainEvents)
            .Select(e => OutboxMessage.Create(e))
            .ToList();
        
        // Ajouter au contexte (même transaction)
        if (outboxMessages.Any())
        {
            context.Set<OutboxMessage>().AddRange(outboxMessages);
        }
        
        // Nettoyer les events des Aggregates
        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }
        
        return await base.SavingChangesAsync(eventData, result, ct);
    }
}

/// <summary>
/// Enregistrement de l'intercepteur.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder AddOutboxInterceptor(
        this DbContextOptionsBuilder builder)
    {
        return builder.AddInterceptors(new OutboxInterceptor());
    }
}
```

### 4. Worker de traitement Outbox

```csharp
/// <summary>
/// Background service qui traite les messages Outbox.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
    private readonly int _batchSize = 100;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }
            
            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }
    
    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LlmProxyDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        
        // Récupérer les messages non traités
        var messages = await context.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(_batchSize)
            .ToListAsync(ct);
        
        if (!messages.Any())
            return;
        
        _logger.LogDebug("Processing {Count} outbox messages", messages.Count);
        
        foreach (var message in messages)
        {
            try
            {
                // Désérialiser l'événement
                var eventType = Type.GetType(message.Type);
                if (eventType is null)
                {
                    message.MarkAsFailed($"Unknown event type: {message.Type}");
                    continue;
                }
                
                var domainEvent = JsonSerializer.Deserialize(
                    message.Content,
                    eventType,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                
                if (domainEvent is null)
                {
                    message.MarkAsFailed("Failed to deserialize event");
                    continue;
                }
                
                // Publier sur le message broker
                await publisher.PublishAsync(domainEvent, ct);
                
                // Marquer comme traité
                message.MarkAsProcessed();
                
                _logger.LogDebug(
                    "Published outbox message {MessageId} of type {Type}",
                    message.Id,
                    eventType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process outbox message {MessageId}",
                    message.Id);
                
                message.MarkAsFailed(ex.Message);
            }
        }
        
        await context.SaveChangesAsync(ct);
    }
}
```

### 5. Publisher d'événements

```csharp
/// <summary>
/// Interface pour la publication d'événements.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : class;
}

/// <summary>
/// Implémentation avec MassTransit/RabbitMQ.
/// </summary>
public sealed class MassTransitEventPublisher : IEventPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<MassTransitEventPublisher> _logger;
    
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct)
        where TEvent : class
    {
        _logger.LogDebug("Publishing event {EventType}", typeof(TEvent).Name);
        
        await _bus.Publish(domainEvent, ct);
    }
}

/// <summary>
/// Implémentation avec Azure Service Bus.
/// </summary>
public sealed class AzureServiceBusEventPublisher : IEventPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<AzureServiceBusEventPublisher> _logger;
    
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct)
        where TEvent : class
    {
        var json = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = typeof(TEvent).Name,
            MessageId = Guid.NewGuid().ToString()
        };
        
        await _sender.SendMessageAsync(message, ct);
    }
}
```

### 6. Cleanup des messages traités

```csharp
/// <summary>
/// Service de nettoyage des messages traités.
/// </summary>
public sealed class OutboxCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxCleanupService> _logger;
    private readonly TimeSpan _retentionPeriod = TimeSpan.FromDays(7);
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during outbox cleanup");
            }
            
            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }
    
    private async Task CleanupOldMessagesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LlmProxyDbContext>();
        
        var cutoffDate = DateTimeOffset.UtcNow - _retentionPeriod;
        
        // Supprimer les messages traités depuis plus de X jours
        var deletedCount = await context.OutboxMessages
            .Where(m => m.ProcessedAt != null && m.ProcessedAt < cutoffDate)
            .ExecuteDeleteAsync(ct);
        
        if (deletedCount > 0)
        {
            _logger.LogInformation(
                "Cleaned up {Count} processed outbox messages older than {Days} days",
                deletedCount,
                _retentionPeriod.TotalDays);
        }
    }
}
```

### 7. Gestion des messages en échec (Dead Letter)

```csharp
/// <summary>
/// Service de gestion des messages en échec.
/// </summary>
public sealed class OutboxDeadLetterService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxDeadLetterService> _logger;
    private readonly int _maxRetries = 3;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MoveToDeadLetterAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving messages to dead letter");
            }
            
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
    
    private async Task MoveToDeadLetterAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LlmProxyDbContext>();
        
        // Messages qui ont dépassé le nombre max de retries
        var failedMessages = await context.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount >= _maxRetries)
            .ToListAsync(ct);
        
        foreach (var message in failedMessages)
        {
            // Créer une entrée dead letter
            var deadLetter = new OutboxDeadLetter
            {
                Id = Guid.NewGuid(),
                OriginalMessageId = message.Id,
                Type = message.Type,
                Content = message.Content,
                Error = message.Error,
                RetryCount = message.RetryCount,
                CreatedAt = message.CreatedAt,
                DeadLetteredAt = DateTimeOffset.UtcNow
            };
            
            context.OutboxDeadLetters.Add(deadLetter);
            context.OutboxMessages.Remove(message);
            
            _logger.LogWarning(
                "Moved message {MessageId} to dead letter after {RetryCount} retries. Error: {Error}",
                message.Id,
                message.RetryCount,
                message.Error);
        }
        
        await context.SaveChangesAsync(ct);
    }
}
```

### 8. Configuration complète

```csharp
/// <summary>
/// Extension pour configurer l'Outbox pattern.
/// </summary>
public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxPattern(
        this IServiceCollection services,
        Action<OutboxOptions>? configure = null)
    {
        var options = new OutboxOptions();
        configure?.Invoke(options);
        
        services.AddSingleton(options);
        
        // Processor principal
        services.AddHostedService<OutboxProcessor>();
        
        // Cleanup
        if (options.EnableCleanup)
        {
            services.AddHostedService<OutboxCleanupService>();
        }
        
        // Dead letter
        if (options.EnableDeadLetter)
        {
            services.AddHostedService<OutboxDeadLetterService>();
        }
        
        // Publisher
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        
        return services;
    }
}

public sealed class OutboxOptions
{
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
    public int BatchSize { get; set; } = 100;
    public int MaxRetries { get; set; } = 3;
    public bool EnableCleanup { get; set; } = true;
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(7);
    public bool EnableDeadLetter { get; set; } = true;
}
```

## Conséquences

### Positives

- **Fiabilité** : Les événements ne sont jamais perdus
- **Atomicité** : DB + Events dans la même transaction
- **Résilience** : Retry automatique en cas d'échec
- **Traçabilité** : Historique complet dans la table Outbox

### Négatives

- **Latence** : Délai entre commit et publication (polling interval)
  - *Mitigation* : Réduire l'intervalle ou utiliser des notifications DB
- **Complexité** : Worker supplémentaire à gérer
  - *Mitigation* : Bien encapsulé, peu de maintenance
- **Stockage** : Messages stockés en base
  - *Mitigation* : Cleanup régulier

### Neutres

- Pattern standard pour l'event-driven architecture
- Compatible avec tous les message brokers

## Alternatives considérées

### Option A : Distributed Transaction (2PC)

- **Description** : Two-Phase Commit entre DB et broker
- **Avantages** : Atomicité vraie
- **Inconvénients** : Performance, complexité, pas supporté partout
- **Raison du rejet** : Trop complexe, pas universel

### Option B : Publish-then-commit

- **Description** : Publier avant de commit la DB
- **Avantages** : Simple
- **Inconvénients** : Si DB échoue après publish → incohérence
- **Raison du rejet** : Données incohérentes possibles

### Option C : Change Data Capture (CDC)

- **Description** : Utiliser Debezium/CDC pour capturer les changements
- **Avantages** : Découplé, temps réel
- **Inconvénients** : Infrastructure supplémentaire
- **Raison du rejet** : Trop de dépendances, plus complexe

## Références

- [Outbox Pattern - microservices.io](https://microservices.io/patterns/data/transactional-outbox.html)
- [Reliable Messaging - NServiceBus](https://docs.particular.net/nservicebus/outbox/)
- [EF Core Interceptors](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)
