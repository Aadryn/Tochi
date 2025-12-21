# 46. Saga Pattern pour les transactions distribuées

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM implique des opérations qui traversent plusieurs services/aggregates :
- Créer un tenant → initialiser quotas → créer clé API → configurer billing
- Envoyer requête LLM → décrémenter quota → facturer → logger métriques

Une transaction traditionnelle (ACID) ne fonctionne pas :
- Services distribués (pas de transaction DB unique)
- Appels externes (providers LLM, Stripe)
- Opérations longues (timeout)

```csharp
// ❌ IMPOSSIBLE : Transaction distribuée ACID
using var transaction = await _db.BeginTransactionAsync();
try
{
    await _tenantRepo.CreateAsync(tenant);      // DB locale
    await _billingService.SetupAsync(tenant);   // API Stripe externe
    await _quotaService.InitializeAsync(tenant); // Redis
    await _emailService.SendWelcomeAsync(tenant); // Service email
    
    await transaction.CommitAsync(); // ❌ Ne couvre pas Stripe, Redis, Email !
}
catch
{
    await transaction.RollbackAsync(); // ❌ Ne rollback pas les services externes
    throw;
}
```

## Décision

**Utiliser le pattern Saga pour orchestrer les transactions distribuées avec compensation en cas d'échec.**

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        SAGA PATTERN                             │
│                                                                 │
│  CHOREOGRAPHY (événements)    vs    ORCHESTRATION (central)     │
│                                                                 │
│  ┌─────┐    ┌─────┐    ┌─────┐     ┌──────────────────────┐    │
│  │ Svc │───►│ Svc │───►│ Svc │     │    Saga Orchestrator │    │
│  │  A  │◄───│  B  │◄───│  C  │     │                      │    │
│  └─────┘    └─────┘    └─────┘     │  ┌───┐ ┌───┐ ┌───┐  │    │
│     │          │          │        │  │ A │►│ B │►│ C │  │    │
│     └──────────┴──────────┘        │  └───┘ └───┘ └───┘  │    │
│        (via Event Bus)             └──────────────────────┘    │
│                                                                 │
│  ✅ Découplé                       ✅ Flux clair               │
│  ❌ Flux difficile à suivre        ✅ Compensation centralisée  │
│  ❌ Compensation complexe          ❌ Single point of failure   │
└─────────────────────────────────────────────────────────────────┘
```

### 1. Définition d'une Saga

```csharp
/// <summary>
/// Interface de base pour une Saga.
/// </summary>
public interface ISaga<TData> where TData : class
{
    /// <summary>
    /// Identifiant unique de l'instance de Saga.
    /// </summary>
    Guid SagaId { get; }
    
    /// <summary>
    /// Données de la Saga.
    /// </summary>
    TData Data { get; }
    
    /// <summary>
    /// État actuel de la Saga.
    /// </summary>
    SagaState State { get; }
    
    /// <summary>
    /// Étape actuelle.
    /// </summary>
    int CurrentStep { get; }
    
    /// <summary>
    /// Exécute la Saga.
    /// </summary>
    Task ExecuteAsync(CancellationToken ct = default);
}

public enum SagaState
{
    NotStarted,
    InProgress,
    Completed,
    Compensating,
    Compensated,
    Failed
}

/// <summary>
/// Données de la Saga de création de Tenant.
/// </summary>
public sealed class CreateTenantSagaData
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string BillingEmail { get; set; } = string.Empty;
    
    // Résultats des étapes (pour compensation)
    public Guid? CreatedTenantId { get; set; }
    public string? StripeCustomerId { get; set; }
    public Guid? CreatedApiKeyId { get; set; }
    public bool QuotasInitialized { get; set; }
    public bool WelcomeEmailSent { get; set; }
    
    // Tracking
    public List<string> CompletedSteps { get; set; } = new();
    public string? FailureReason { get; set; }
}
```

### 2. Implémentation Saga Orchestrator

```csharp
/// <summary>
/// Saga pour la création d'un Tenant complet.
/// </summary>
public sealed class CreateTenantSaga : ISaga<CreateTenantSagaData>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBillingService _billingService;
    private readonly IQuotaService _quotaService;
    private readonly IApiKeyService _apiKeyService;
    private readonly IEmailService _emailService;
    private readonly ISagaRepository _sagaRepository;
    private readonly ILogger<CreateTenantSaga> _logger;
    
    public Guid SagaId { get; private set; }
    public CreateTenantSagaData Data { get; private set; }
    public SagaState State { get; private set; }
    public int CurrentStep { get; private set; }
    
    private readonly List<SagaStep> _steps;
    
    public CreateTenantSaga(
        CreateTenantSagaData data,
        /* dependencies */)
    {
        SagaId = Guid.NewGuid();
        Data = data;
        State = SagaState.NotStarted;
        
        // Définir les étapes avec leurs compensations
        _steps = new List<SagaStep>
        {
            new SagaStep(
                name: "CreateTenant",
                execute: CreateTenantAsync,
                compensate: DeleteTenantAsync),
            
            new SagaStep(
                name: "SetupBilling",
                execute: SetupBillingAsync,
                compensate: CancelBillingAsync),
            
            new SagaStep(
                name: "InitializeQuotas",
                execute: InitializeQuotasAsync,
                compensate: RemoveQuotasAsync),
            
            new SagaStep(
                name: "CreateDefaultApiKey",
                execute: CreateDefaultApiKeyAsync,
                compensate: RevokeApiKeyAsync),
            
            new SagaStep(
                name: "SendWelcomeEmail",
                execute: SendWelcomeEmailAsync,
                compensate: null) // Pas de compensation pour l'email
        };
    }
    
    /// <summary>
    /// Exécute la Saga étape par étape.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        State = SagaState.InProgress;
        await SaveStateAsync(ct);
        
        _logger.LogInformation(
            "Starting saga {SagaId} for tenant {TenantName}",
            SagaId, Data.Name);
        
        try
        {
            for (CurrentStep = 0; CurrentStep < _steps.Count; CurrentStep++)
            {
                var step = _steps[CurrentStep];
                
                _logger.LogDebug(
                    "Executing step {StepNumber}/{TotalSteps}: {StepName}",
                    CurrentStep + 1, _steps.Count, step.Name);
                
                await step.Execute(ct);
                
                Data.CompletedSteps.Add(step.Name);
                await SaveStateAsync(ct);
            }
            
            State = SagaState.Completed;
            await SaveStateAsync(ct);
            
            _logger.LogInformation(
                "Saga {SagaId} completed successfully",
                SagaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Saga {SagaId} failed at step {StepName}. Starting compensation.",
                SagaId, _steps[CurrentStep].Name);
            
            Data.FailureReason = ex.Message;
            await CompensateAsync(ct);
            
            throw new SagaFailedException(SagaId, _steps[CurrentStep].Name, ex);
        }
    }
    
    /// <summary>
    /// Compense les étapes déjà exécutées en ordre inverse.
    /// </summary>
    private async Task CompensateAsync(CancellationToken ct)
    {
        State = SagaState.Compensating;
        await SaveStateAsync(ct);
        
        // Compenser en ordre inverse
        for (var i = CurrentStep - 1; i >= 0; i--)
        {
            var step = _steps[i];
            
            if (step.Compensate is null)
            {
                _logger.LogDebug(
                    "Step {StepName} has no compensation action",
                    step.Name);
                continue;
            }
            
            try
            {
                _logger.LogDebug(
                    "Compensating step {StepName}",
                    step.Name);
                
                await step.Compensate(ct);
            }
            catch (Exception ex)
            {
                // Log mais continue la compensation
                _logger.LogError(
                    ex,
                    "Compensation failed for step {StepName}. Manual intervention required.",
                    step.Name);
            }
        }
        
        State = SagaState.Compensated;
        await SaveStateAsync(ct);
    }
    
    // ═══════════════════════════════════════════════════════════════
    // ÉTAPES D'EXÉCUTION
    // ═══════════════════════════════════════════════════════════════
    
    private async Task CreateTenantAsync(CancellationToken ct)
    {
        var tenant = Tenant.Create(Data.Name, Data.Slug);
        await _tenantRepository.AddAsync(tenant, ct);
        
        Data.CreatedTenantId = tenant.Id;
        Data.TenantId = tenant.Id;
    }
    
    private async Task SetupBillingAsync(CancellationToken ct)
    {
        var customerId = await _billingService.CreateCustomerAsync(
            Data.TenantId,
            Data.Name,
            Data.BillingEmail,
            ct);
        
        Data.StripeCustomerId = customerId;
    }
    
    private async Task InitializeQuotasAsync(CancellationToken ct)
    {
        await _quotaService.InitializeDefaultQuotasAsync(Data.TenantId, ct);
        Data.QuotasInitialized = true;
    }
    
    private async Task CreateDefaultApiKeyAsync(CancellationToken ct)
    {
        var apiKey = await _apiKeyService.CreateAsync(
            Data.TenantId,
            "Default API Key",
            new[] { "chat:read", "chat:write" },
            ct);
        
        Data.CreatedApiKeyId = apiKey.Id;
    }
    
    private async Task SendWelcomeEmailAsync(CancellationToken ct)
    {
        await _emailService.SendWelcomeAsync(Data.BillingEmail, Data.Name, ct);
        Data.WelcomeEmailSent = true;
    }
    
    // ═══════════════════════════════════════════════════════════════
    // ÉTAPES DE COMPENSATION
    // ═══════════════════════════════════════════════════════════════
    
    private async Task DeleteTenantAsync(CancellationToken ct)
    {
        if (Data.CreatedTenantId.HasValue)
        {
            await _tenantRepository.DeleteAsync(Data.CreatedTenantId.Value, ct);
        }
    }
    
    private async Task CancelBillingAsync(CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(Data.StripeCustomerId))
        {
            await _billingService.DeleteCustomerAsync(Data.StripeCustomerId, ct);
        }
    }
    
    private async Task RemoveQuotasAsync(CancellationToken ct)
    {
        if (Data.QuotasInitialized)
        {
            await _quotaService.RemoveQuotasAsync(Data.TenantId, ct);
        }
    }
    
    private async Task RevokeApiKeyAsync(CancellationToken ct)
    {
        if (Data.CreatedApiKeyId.HasValue)
        {
            await _apiKeyService.RevokeAsync(
                Data.CreatedApiKeyId.Value, 
                "Saga compensation",
                ct);
        }
    }
    
    private async Task SaveStateAsync(CancellationToken ct)
    {
        await _sagaRepository.SaveAsync(this, ct);
    }
}

/// <summary>
/// Définition d'une étape de Saga.
/// </summary>
public sealed class SagaStep
{
    public string Name { get; }
    public Func<CancellationToken, Task> Execute { get; }
    public Func<CancellationToken, Task>? Compensate { get; }
    
    public SagaStep(
        string name,
        Func<CancellationToken, Task> execute,
        Func<CancellationToken, Task>? compensate)
    {
        Name = name;
        Execute = execute;
        Compensate = compensate;
    }
}
```

### 3. Saga Repository pour la persistance

```csharp
/// <summary>
/// Repository pour persister l'état des Sagas.
/// </summary>
public interface ISagaRepository
{
    Task SaveAsync<TData>(ISaga<TData> saga, CancellationToken ct) 
        where TData : class;
    
    Task<SagaSnapshot<TData>?> GetAsync<TData>(Guid sagaId, CancellationToken ct) 
        where TData : class;
    
    Task<IReadOnlyList<SagaSnapshot<TData>>> GetPendingAsync<TData>(CancellationToken ct) 
        where TData : class;
}

/// <summary>
/// Snapshot d'une Saga pour persistance.
/// </summary>
public sealed class SagaSnapshot<TData> where TData : class
{
    public Guid Id { get; set; }
    public string SagaType { get; set; } = string.Empty;
    public TData Data { get; set; } = null!;
    public SagaState State { get; set; }
    public int CurrentStep { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// Implémentation avec PostgreSQL.
/// </summary>
public sealed class PostgresSagaRepository : ISagaRepository
{
    private readonly LlmProxyDbContext _context;
    
    public async Task SaveAsync<TData>(ISaga<TData> saga, CancellationToken ct)
        where TData : class
    {
        var entity = await _context.SagaSnapshots
            .FirstOrDefaultAsync(s => s.Id == saga.SagaId, ct);
        
        var dataJson = JsonSerializer.Serialize(saga.Data);
        
        if (entity is null)
        {
            entity = new SagaSnapshotEntity
            {
                Id = saga.SagaId,
                SagaType = typeof(TData).Name,
                DataJson = dataJson,
                State = saga.State.ToString(),
                CurrentStep = saga.CurrentStep,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _context.SagaSnapshots.Add(entity);
        }
        else
        {
            entity.DataJson = dataJson;
            entity.State = saga.State.ToString();
            entity.CurrentStep = saga.CurrentStep;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        await _context.SaveChangesAsync(ct);
    }
}
```

### 4. Service de récupération des Sagas

```csharp
/// <summary>
/// Service de récupération des Sagas interrompues.
/// </summary>
public sealed class SagaRecoveryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SagaRecoveryService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RecoverPendingSagasAsync(stoppingToken);
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
    
    private async Task RecoverPendingSagasAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var sagaRepository = scope.ServiceProvider
            .GetRequiredService<ISagaRepository>();
        
        // Récupérer les Sagas en cours depuis plus de 5 minutes
        var pendingSagas = await sagaRepository
            .GetPendingAsync<CreateTenantSagaData>(ct);
        
        foreach (var snapshot in pendingSagas)
        {
            if (snapshot.State == SagaState.InProgress)
            {
                _logger.LogWarning(
                    "Found stuck saga {SagaId} at step {Step}. Attempting recovery.",
                    snapshot.Id, snapshot.CurrentStep);
                
                try
                {
                    // Recréer et reprendre la Saga
                    var saga = await RecreateSagaAsync(snapshot, scope.ServiceProvider);
                    await saga.ExecuteAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to recover saga {SagaId}",
                        snapshot.Id);
                }
            }
        }
    }
}
```

### 5. Saga basée sur les événements (Choreography)

```csharp
/// <summary>
/// Alternative : Saga par chorégraphie avec événements.
/// Chaque service écoute les événements et réagit.
/// </summary>

// Événements de la Saga
public sealed record TenantCreatedEvent(Guid TenantId, string Name) : IDomainEvent;
public sealed record BillingSetupCompletedEvent(Guid TenantId, string CustomerId) : IDomainEvent;
public sealed record BillingSetupFailedEvent(Guid TenantId, string Error) : IDomainEvent;
public sealed record QuotasInitializedEvent(Guid TenantId) : IDomainEvent;

/// <summary>
/// Handler qui écoute TenantCreatedEvent et setup le billing.
/// </summary>
public sealed class SetupBillingOnTenantCreated : INotificationHandler<TenantCreatedEvent>
{
    private readonly IBillingService _billingService;
    private readonly IEventPublisher _eventPublisher;
    
    public async Task Handle(TenantCreatedEvent notification, CancellationToken ct)
    {
        try
        {
            var customerId = await _billingService.CreateCustomerAsync(
                notification.TenantId,
                notification.Name,
                ct);
            
            await _eventPublisher.PublishAsync(
                new BillingSetupCompletedEvent(notification.TenantId, customerId), ct);
        }
        catch (Exception ex)
        {
            await _eventPublisher.PublishAsync(
                new BillingSetupFailedEvent(notification.TenantId, ex.Message), ct);
        }
    }
}

/// <summary>
/// Handler qui écoute BillingSetupCompletedEvent et initialise les quotas.
/// </summary>
public sealed class InitializeQuotasOnBillingSetup 
    : INotificationHandler<BillingSetupCompletedEvent>
{
    private readonly IQuotaService _quotaService;
    private readonly IEventPublisher _eventPublisher;
    
    public async Task Handle(BillingSetupCompletedEvent notification, CancellationToken ct)
    {
        await _quotaService.InitializeDefaultQuotasAsync(notification.TenantId, ct);
        
        await _eventPublisher.PublishAsync(
            new QuotasInitializedEvent(notification.TenantId), ct);
    }
}

/// <summary>
/// Handler de compensation sur échec.
/// </summary>
public sealed class CompensateOnBillingFailure 
    : INotificationHandler<BillingSetupFailedEvent>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<CompensateOnBillingFailure> _logger;
    
    public async Task Handle(BillingSetupFailedEvent notification, CancellationToken ct)
    {
        _logger.LogWarning(
            "Billing setup failed for tenant {TenantId}. Compensating by deleting tenant.",
            notification.TenantId);
        
        await _tenantRepository.DeleteAsync(notification.TenantId, ct);
    }
}
```

### 6. Utilisation dans un Handler

```csharp
/// <summary>
/// Handler qui lance la Saga de création de tenant.
/// </summary>
public sealed class CreateTenantHandler 
    : IRequestHandler<CreateTenantCommand, Result<TenantDto>>
{
    private readonly ISagaFactory _sagaFactory;
    
    public async Task<Result<TenantDto>> Handle(
        CreateTenantCommand request,
        CancellationToken ct)
    {
        var sagaData = new CreateTenantSagaData
        {
            Name = request.Name,
            Slug = request.Slug,
            BillingEmail = request.BillingEmail
        };
        
        var saga = _sagaFactory.Create<CreateTenantSaga, CreateTenantSagaData>(sagaData);
        
        try
        {
            await saga.ExecuteAsync(ct);
            
            return Result<TenantDto>.Success(new TenantDto
            {
                Id = sagaData.TenantId,
                Name = sagaData.Name,
                Slug = sagaData.Slug
            });
        }
        catch (SagaFailedException ex)
        {
            return Result<TenantDto>.Failure(
                $"Tenant creation failed: {ex.FailedStep}");
        }
    }
}
```

## Conséquences

### Positives

- **Cohérence** : Garantie de cohérence éventuelle
- **Compensation** : Rollback automatique sur erreur
- **Visibilité** : État de la transaction traçable
- **Récupération** : Reprise après crash

### Négatives

- **Complexité** : Plus de code que transaction simple
  - *Mitigation* : Abstraction réutilisable
- **Cohérence éventuelle** : Pas ACID immédiat
  - *Mitigation* : Acceptable pour les cas d'usage
- **Compensation imparfaite** : Effets de bord possibles
  - *Mitigation* : Compensation idempotente

### Neutres

- Pattern standard pour les microservices
- Nécessite persistance de l'état

## Alternatives considérées

### Option A : Two-Phase Commit (2PC)

- **Description** : Transaction distribuée ACID
- **Avantages** : Cohérence forte
- **Inconvénients** : Lent, pas supporté partout
- **Raison du rejet** : Incompatible avec services externes

### Option B : Pas de compensation

- **Description** : Ignorer les échecs partiels
- **Avantages** : Simple
- **Inconvénients** : Données incohérentes
- **Raison du rejet** : Inacceptable en production

## Références

- [Saga Pattern - microservices.io](https://microservices.io/patterns/data/saga.html)
- [Compensating Transactions](https://docs.microsoft.com/en-us/azure/architecture/patterns/compensating-transaction)
- [NServiceBus Sagas](https://docs.particular.net/nservicebus/sagas/)
