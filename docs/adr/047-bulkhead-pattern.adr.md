# 47. Bulkhead Pattern pour l'isolation des ressources

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM interagit avec plusieurs providers (OpenAI, Anthropic, Azure) et services (Redis, PostgreSQL). Sans isolation :
- Un provider lent peut bloquer tous les threads
- Un tenant gourmand peut impacter les autres
- Une dépendance défaillante peut faire tomber tout le système

```csharp
// ❌ SANS BULKHEAD : Pas d'isolation
public class LlmService
{
    private readonly HttpClient _httpClient; // Partagé pour tous les providers !
    
    public async Task<Response> CallOpenAiAsync(Request request)
    {
        // Si OpenAI est lent, tous les threads sont bloqués
        // Anthropic, Azure... tout est impacté
        return await _httpClient.PostAsync("https://api.openai.com/...", content);
    }
}
```

### Le problème sans isolation

```
┌─────────────────────────────────────────────────────────────────┐
│                    SANS BULKHEAD                                │
│                                                                 │
│  Requêtes ─────────────────────────────────────────────────►    │
│                           │                                     │
│                           ▼                                     │
│                    ┌────────────┐                               │
│                    │  Thread    │                               │
│                    │   Pool     │  ← Pool partagé               │
│                    │  (limité)  │                               │
│                    └────────────┘                               │
│                           │                                     │
│         ┌─────────────────┼─────────────────┐                   │
│         ▼                 ▼                 ▼                   │
│    ┌─────────┐      ┌─────────┐      ┌─────────┐               │
│    │ OpenAI  │      │Anthropic│      │  Azure  │               │
│    │  LENT   │      │   OK    │      │   OK    │               │
│    │ ⏳⏳⏳   │      │         │      │         │               │
│    └─────────┘      └─────────┘      └─────────┘               │
│         │                                                       │
│         └──► Tous les threads bloqués sur OpenAI                │
│              Anthropic et Azure ne peuvent plus être appelés !  │
└─────────────────────────────────────────────────────────────────┘
```

## Décision

**Implémenter le pattern Bulkhead pour isoler les ressources par provider, par tenant, et par criticité.**

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     AVEC BULKHEAD                               │
│                                                                 │
│  Requêtes ─────────────────────────────────────────────────►    │
│                           │                                     │
│         ┌─────────────────┼─────────────────┐                   │
│         ▼                 ▼                 ▼                   │
│  ┌────────────┐   ┌────────────┐   ┌────────────┐              │
│  │  Bulkhead  │   │  Bulkhead  │   │  Bulkhead  │              │
│  │   OpenAI   │   │  Anthropic │   │   Azure    │              │
│  │  10 slots  │   │  10 slots  │   │  10 slots  │              │
│  └────────────┘   └────────────┘   └────────────┘              │
│         │                 │                 │                   │
│         ▼                 ▼                 ▼                   │
│    ┌─────────┐      ┌─────────┐      ┌─────────┐               │
│    │ OpenAI  │      │Anthropic│      │  Azure  │               │
│    │  LENT   │      │   OK    │      │   OK    │               │
│    │ ⏳⏳⏳   │      │   ✅    │      │   ✅    │               │
│    └─────────┘      └─────────┘      └─────────┘               │
│         │                 │                 │                   │
│         │                 └─────────────────┘                   │
│         │                 Continuent de fonctionner !           │
│         └──► Seul OpenAI est bloqué (10 slots max)              │
└─────────────────────────────────────────────────────────────────┘
```

### 1. Bulkhead avec SemaphoreSlim

```csharp
/// <summary>
/// Bulkhead basé sur SemaphoreSlim pour limiter la concurrence.
/// </summary>
public sealed class SemaphoreBulkhead : IBulkhead
{
    private readonly SemaphoreSlim _semaphore;
    private readonly string _name;
    private readonly int _maxConcurrency;
    private readonly ILogger<SemaphoreBulkhead> _logger;
    private int _currentCount;
    private int _rejectedCount;
    
    public SemaphoreBulkhead(
        string name,
        int maxConcurrency,
        ILogger<SemaphoreBulkhead> logger)
    {
        _name = name;
        _maxConcurrency = maxConcurrency;
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        _logger = logger;
    }
    
    public int CurrentCount => _currentCount;
    public int AvailableSlots => _semaphore.CurrentCount;
    public int RejectedCount => _rejectedCount;
    
    /// <summary>
    /// Exécute une opération dans le bulkhead.
    /// </summary>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct = default)
    {
        // Tenter d'acquérir un slot
        var acquired = await _semaphore.WaitAsync(TimeSpan.Zero, ct);
        
        if (!acquired)
        {
            Interlocked.Increment(ref _rejectedCount);
            
            _logger.LogWarning(
                "Bulkhead {BulkheadName} rejected request. " +
                "Current: {Current}/{Max}, Rejected: {Rejected}",
                _name, _currentCount, _maxConcurrency, _rejectedCount);
            
            throw new BulkheadRejectedException(_name, _maxConcurrency);
        }
        
        Interlocked.Increment(ref _currentCount);
        
        try
        {
            _logger.LogDebug(
                "Bulkhead {BulkheadName} executing. Slots: {Available}/{Max}",
                _name, AvailableSlots, _maxConcurrency);
            
            return await operation(ct);
        }
        finally
        {
            Interlocked.Decrement(ref _currentCount);
            _semaphore.Release();
        }
    }
    
    /// <summary>
    /// Version avec timeout d'attente.
    /// </summary>
    public async Task<T> ExecuteWithWaitAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan waitTimeout,
        CancellationToken ct = default)
    {
        var acquired = await _semaphore.WaitAsync(waitTimeout, ct);
        
        if (!acquired)
        {
            throw new BulkheadRejectedException(_name, _maxConcurrency);
        }
        
        Interlocked.Increment(ref _currentCount);
        
        try
        {
            return await operation(ct);
        }
        finally
        {
            Interlocked.Decrement(ref _currentCount);
            _semaphore.Release();
        }
    }
}

/// <summary>
/// Exception quand le bulkhead rejette la requête.
/// </summary>
public sealed class BulkheadRejectedException : DomainException
{
    public override int HttpStatusCode => 503;
    
    public BulkheadRejectedException(string bulkheadName, int maxConcurrency)
        : base(
            "BULKHEAD_REJECTED",
            $"Service '{bulkheadName}' is at capacity ({maxConcurrency} concurrent requests)",
            new Dictionary<string, object>
            {
                ["bulkhead"] = bulkheadName,
                ["maxConcurrency"] = maxConcurrency
            })
    {
    }
}
```

### 2. Bulkhead par Provider LLM

```csharp
/// <summary>
/// Configuration des bulkheads par provider.
/// </summary>
public sealed class BulkheadOptions
{
    public const string SectionName = "Bulkheads";
    
    public int OpenAiMaxConcurrency { get; set; } = 50;
    public int AnthropicMaxConcurrency { get; set; } = 30;
    public int AzureOpenAiMaxConcurrency { get; set; } = 40;
    public int DefaultMaxConcurrency { get; set; } = 20;
    
    public TimeSpan DefaultWaitTimeout { get; set; } = TimeSpan.FromSeconds(5);
}

/// <summary>
/// Factory pour créer les bulkheads par provider.
/// </summary>
public interface IBulkheadFactory
{
    IBulkhead GetBulkhead(string provider);
}

public sealed class BulkheadFactory : IBulkheadFactory
{
    private readonly ConcurrentDictionary<string, IBulkhead> _bulkheads = new();
    private readonly BulkheadOptions _options;
    private readonly ILoggerFactory _loggerFactory;
    
    public BulkheadFactory(
        IOptions<BulkheadOptions> options,
        ILoggerFactory loggerFactory)
    {
        _options = options.Value;
        _loggerFactory = loggerFactory;
    }
    
    public IBulkhead GetBulkhead(string provider)
    {
        return _bulkheads.GetOrAdd(provider, name =>
        {
            var maxConcurrency = name.ToLowerInvariant() switch
            {
                "openai" => _options.OpenAiMaxConcurrency,
                "anthropic" => _options.AnthropicMaxConcurrency,
                "azure" => _options.AzureOpenAiMaxConcurrency,
                _ => _options.DefaultMaxConcurrency
            };
            
            return new SemaphoreBulkhead(
                name,
                maxConcurrency,
                _loggerFactory.CreateLogger<SemaphoreBulkhead>());
        });
    }
}
```

### 3. Intégration avec le Service LLM

```csharp
/// <summary>
/// Service LLM avec isolation par bulkhead.
/// </summary>
public sealed class BulkheadLlmService : ILlmService
{
    private readonly IBulkheadFactory _bulkheadFactory;
    private readonly ILlmProviderFactory _providerFactory;
    private readonly ILogger<BulkheadLlmService> _logger;
    
    public async Task<LlmResponse> ChatCompletionAsync(
        string provider,
        ChatRequest request,
        CancellationToken ct = default)
    {
        var bulkhead = _bulkheadFactory.GetBulkhead(provider);
        var llmProvider = _providerFactory.GetProvider(provider);
        
        try
        {
            return await bulkhead.ExecuteAsync(
                async token => await llmProvider.ChatAsync(request, token),
                ct);
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogWarning(
                "Provider {Provider} bulkhead rejected request. " +
                "Consider using fallback provider.",
                provider);
            
            // Fallback vers un autre provider si disponible
            var fallbackProvider = GetFallbackProvider(provider);
            if (fallbackProvider is not null)
            {
                return await ChatCompletionAsync(fallbackProvider, request, ct);
            }
            
            throw;
        }
    }
    
    private string? GetFallbackProvider(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "openai" => "anthropic",
            "anthropic" => "azure",
            "azure" => "openai",
            _ => null
        };
    }
}
```

### 4. Bulkhead par Tenant (Fair Share)

```csharp
/// <summary>
/// Bulkhead par tenant pour garantir un partage équitable.
/// </summary>
public sealed class TenantBulkheadManager
{
    private readonly ConcurrentDictionary<Guid, IBulkhead> _tenantBulkheads = new();
    private readonly int _defaultMaxPerTenant;
    private readonly ILoggerFactory _loggerFactory;
    
    public TenantBulkheadManager(
        IOptions<BulkheadOptions> options,
        ILoggerFactory loggerFactory)
    {
        _defaultMaxPerTenant = options.Value.DefaultMaxConcurrency;
        _loggerFactory = loggerFactory;
    }
    
    public IBulkhead GetTenantBulkhead(Guid tenantId, int? customLimit = null)
    {
        return _tenantBulkheads.GetOrAdd(tenantId, id =>
        {
            var limit = customLimit ?? _defaultMaxPerTenant;
            return new SemaphoreBulkhead(
                $"tenant-{id}",
                limit,
                _loggerFactory.CreateLogger<SemaphoreBulkhead>());
        });
    }
    
    /// <summary>
    /// Met à jour la limite pour un tenant (plan upgrade).
    /// </summary>
    public void UpdateTenantLimit(Guid tenantId, int newLimit)
    {
        // Remplacer le bulkhead avec la nouvelle limite
        var newBulkhead = new SemaphoreBulkhead(
            $"tenant-{tenantId}",
            newLimit,
            _loggerFactory.CreateLogger<SemaphoreBulkhead>());
        
        _tenantBulkheads.AddOrUpdate(tenantId, newBulkhead, (_, _) => newBulkhead);
    }
}
```

### 5. Bulkhead avec Polly

```csharp
/// <summary>
/// Intégration avec Polly pour des stratégies avancées.
/// </summary>
public static class PollyBulkheadExtensions
{
    public static IServiceCollection AddPollyBulkheads(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(BulkheadOptions.SectionName)
            .Get<BulkheadOptions>() ?? new BulkheadOptions();
        
        // Bulkhead OpenAI
        services.AddResiliencePipeline("openai-bulkhead", builder =>
        {
            builder.AddConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = options.OpenAiMaxConcurrency,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 100
            });
        });
        
        // Bulkhead Anthropic
        services.AddResiliencePipeline("anthropic-bulkhead", builder =>
        {
            builder.AddConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = options.AnthropicMaxConcurrency,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 50
            });
        });
        
        return services;
    }
}

/// <summary>
/// Service utilisant Polly Bulkhead.
/// </summary>
public sealed class PollyLlmService
{
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly ILlmProviderFactory _providerFactory;
    
    public async Task<LlmResponse> ChatAsync(
        string provider,
        ChatRequest request,
        CancellationToken ct)
    {
        var pipeline = _pipelineProvider.GetPipeline($"{provider}-bulkhead");
        var llmProvider = _providerFactory.GetProvider(provider);
        
        return await pipeline.ExecuteAsync(
            async token => await llmProvider.ChatAsync(request, token),
            ct);
    }
}
```

### 6. Bulkhead par Thread Pool (pour CPU-bound)

```csharp
/// <summary>
/// Bulkhead avec thread pool isolé pour opérations CPU-bound.
/// </summary>
public sealed class ThreadPoolBulkhead : IDisposable
{
    private readonly BlockingCollection<Func<Task>> _workQueue;
    private readonly Thread[] _workers;
    private readonly string _name;
    private readonly ILogger<ThreadPoolBulkhead> _logger;
    private bool _disposed;
    
    public ThreadPoolBulkhead(
        string name,
        int workerCount,
        ILogger<ThreadPoolBulkhead> logger)
    {
        _name = name;
        _logger = logger;
        _workQueue = new BlockingCollection<Func<Task>>(boundedCapacity: 1000);
        _workers = new Thread[workerCount];
        
        for (var i = 0; i < workerCount; i++)
        {
            _workers[i] = new Thread(Worker)
            {
                Name = $"{name}-worker-{i}",
                IsBackground = true
            };
            _workers[i].Start();
        }
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        var tcs = new TaskCompletionSource<T>();
        
        var added = _workQueue.TryAdd(async () =>
        {
            try
            {
                var result = await operation();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        
        if (!added)
        {
            throw new BulkheadRejectedException(_name, _workQueue.BoundedCapacity);
        }
        
        return await tcs.Task;
    }
    
    private void Worker()
    {
        foreach (var work in _workQueue.GetConsumingEnumerable())
        {
            try
            {
                work().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker error in bulkhead {Name}", _name);
            }
        }
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        _workQueue.CompleteAdding();
        foreach (var worker in _workers)
        {
            worker.Join(TimeSpan.FromSeconds(5));
        }
        _workQueue.Dispose();
    }
}
```

### 7. Monitoring des Bulkheads

```csharp
/// <summary>
/// Métriques des bulkheads pour monitoring.
/// </summary>
public sealed class BulkheadMetrics
{
    private readonly ConcurrentDictionary<string, BulkheadStats> _stats = new();
    
    public void RecordExecution(string bulkheadName, TimeSpan duration, bool success)
    {
        var stats = _stats.GetOrAdd(bulkheadName, _ => new BulkheadStats());
        
        if (success)
            Interlocked.Increment(ref stats.SuccessCount);
        else
            Interlocked.Increment(ref stats.FailureCount);
        
        // Update average duration (simplified)
        stats.TotalDuration += duration.TotalMilliseconds;
    }
    
    public void RecordRejection(string bulkheadName)
    {
        var stats = _stats.GetOrAdd(bulkheadName, _ => new BulkheadStats());
        Interlocked.Increment(ref stats.RejectionCount);
    }
    
    public IReadOnlyDictionary<string, BulkheadStats> GetStats() => _stats;
}

public sealed class BulkheadStats
{
    public long SuccessCount;
    public long FailureCount;
    public long RejectionCount;
    public double TotalDuration;
    
    public double AverageDuration => 
        (SuccessCount + FailureCount) > 0 
            ? TotalDuration / (SuccessCount + FailureCount) 
            : 0;
    
    public double RejectionRate =>
        (SuccessCount + FailureCount + RejectionCount) > 0
            ? (double)RejectionCount / (SuccessCount + FailureCount + RejectionCount)
            : 0;
}

/// <summary>
/// Endpoint pour exposer les métriques.
/// </summary>
[ApiController]
[Route("_internal/bulkheads")]
public class BulkheadMetricsController : ControllerBase
{
    private readonly BulkheadMetrics _metrics;
    private readonly IBulkheadFactory _bulkheadFactory;
    
    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        return Ok(_metrics.GetStats());
    }
}
```

## Conséquences

### Positives

- **Isolation** : Un provider lent n'impacte pas les autres
- **Équité** : Chaque tenant a sa part garantie
- **Résilience** : Défaillance contenue dans son compartiment
- **Observabilité** : Métriques de saturation visibles

### Négatives

- **Sous-utilisation** : Ressources réservées même si non utilisées
  - *Mitigation* : Dimensionnement adaptatif
- **Configuration** : Trouver les bonnes limites
  - *Mitigation* : Monitoring et ajustement progressif
- **Complexité** : Plus de code
  - *Mitigation* : Abstraction via Polly

### Neutres

- Pattern standard de résilience
- Compatible avec Circuit Breaker

## Alternatives considérées

### Option A : Thread Pool partagé

- **Description** : Pool global pour tout
- **Avantages** : Simple
- **Inconvénients** : Pas d'isolation, cascade de défaillances
- **Raison du rejet** : Risque trop élevé

### Option B : Process isolation (microservices)

- **Description** : Un process par provider
- **Avantages** : Isolation parfaite
- **Inconvénients** : Overhead opérationnel
- **Raison du rejet** : Over-engineering pour ce cas

## Références

- [Bulkhead Pattern - Microsoft](https://docs.microsoft.com/en-us/azure/architecture/patterns/bulkhead)
- [Polly Bulkhead](https://github.com/App-vNext/Polly/wiki/Bulkhead)
- [Release It! - Michael Nygard](https://pragprog.com/titles/mnee2/release-it-second-edition/)
