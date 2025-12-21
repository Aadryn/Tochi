# 48. Graceful Shutdown pour l'arrêt propre des services

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM gère des requêtes longues (streaming, gros contextes). Un arrêt brutal cause :
- Requêtes interrompues en plein milieu
- Données corrompues en base
- Messages perdus dans les queues
- Mauvaise expérience utilisateur

```csharp
// ❌ SANS GRACEFUL SHUTDOWN : Arrêt brutal
public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        host.Run();
        // Ctrl+C → Process.Kill() → Toutes les requêtes en cours sont perdues !
    }
}
```

### Le problème de l'arrêt brutal

```
┌─────────────────────────────────────────────────────────────────┐
│                    ARRÊT BRUTAL                                 │
│                                                                 │
│  Client A ────► [Request en cours... 50%] ──────► ❌ PERDU !    │
│  Client B ────► [Streaming tokens...] ──────────► ❌ PERDU !    │
│  Client C ────► [Écriture DB...] ────────────────► ❌ CORROMPU! │
│                                                                 │
│  Background Jobs ────► [Processing...] ──────────► ❌ PERDU !   │
│  Queue Consumer ────► [Message en cours] ────────► ❌ ORPHELIN! │
│                                                                 │
│                         SIGTERM / SIGKILL                       │
│                              ⚡                                  │
│                         Process mort                            │
└─────────────────────────────────────────────────────────────────┘
```

## Décision

**Implémenter un Graceful Shutdown complet avec phases de drainage et timeout.**

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    GRACEFUL SHUTDOWN                            │
│                                                                 │
│  Phase 1: STOP ACCEPTING (immédiat)                             │
│  ├── Health check → Unhealthy                                   │
│  ├── Load balancer retire l'instance                            │
│  └── Nouvelles requêtes → 503 Service Unavailable               │
│                                                                 │
│  Phase 2: DRAIN (30s timeout)                                   │
│  ├── Requêtes en cours → Terminent normalement                  │
│  ├── Background jobs → Terminent ou checkpoint                  │
│  └── Queue consumers → Commit ou reject messages                │
│                                                                 │
│  Phase 3: CLEANUP (5s timeout)                                  │
│  ├── Connexions DB fermées proprement                           │
│  ├── Cache flushed                                              │
│  └── Métriques envoyées                                         │
│                                                                 │
│  Phase 4: TERMINATE                                             │
│  └── Process exit avec code 0                                   │
└─────────────────────────────────────────────────────────────────┘
```

### 1. Configuration du Host

```csharp
/// <summary>
/// Configuration du graceful shutdown dans Program.cs.
/// </summary>
public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configuration du timeout de shutdown
        builder.Host.ConfigureHostOptions(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(45);
        });
        
        // Services avec graceful shutdown
        builder.Services.AddHostedService<GracefulShutdownService>();
        builder.Services.AddSingleton<IShutdownCoordinator, ShutdownCoordinator>();
        
        var app = builder.Build();
        
        // Enregistrer le handler de shutdown
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        var coordinator = app.Services.GetRequiredService<IShutdownCoordinator>();
        
        lifetime.ApplicationStopping.Register(() =>
        {
            coordinator.OnStopping();
        });
        
        lifetime.ApplicationStopped.Register(() =>
        {
            coordinator.OnStopped();
        });
        
        await app.RunAsync();
    }
}
```

### 2. Coordinateur de Shutdown

```csharp
/// <summary>
/// Coordonne l'arrêt propre de tous les composants.
/// </summary>
public interface IShutdownCoordinator
{
    CancellationToken ShutdownToken { get; }
    bool IsShuttingDown { get; }
    
    void OnStopping();
    void OnStopped();
    Task RegisterComponentAsync(string name, Func<CancellationToken, Task> shutdownTask);
}

public sealed class ShutdownCoordinator : IShutdownCoordinator
{
    private readonly CancellationTokenSource _shutdownCts = new();
    private readonly ConcurrentDictionary<string, Func<CancellationToken, Task>> _components = new();
    private readonly ILogger<ShutdownCoordinator> _logger;
    private int _isShuttingDown;
    
    public ShutdownCoordinator(ILogger<ShutdownCoordinator> logger)
    {
        _logger = logger;
    }
    
    public CancellationToken ShutdownToken => _shutdownCts.Token;
    public bool IsShuttingDown => _isShuttingDown == 1;
    
    public async Task RegisterComponentAsync(
        string name,
        Func<CancellationToken, Task> shutdownTask)
    {
        _components.TryAdd(name, shutdownTask);
        _logger.LogInformation("Component {Component} registered for graceful shutdown", name);
    }
    
    public void OnStopping()
    {
        if (Interlocked.Exchange(ref _isShuttingDown, 1) == 1)
            return;
        
        _logger.LogWarning(
            "🛑 Graceful shutdown initiated. {ComponentCount} components to stop.",
            _components.Count);
        
        _shutdownCts.Cancel();
    }
    
    public void OnStopped()
    {
        _logger.LogInformation("✅ Graceful shutdown completed.");
    }
}
```

### 3. Service de Shutdown avec Phases

```csharp
/// <summary>
/// Service gérant les phases de shutdown.
/// </summary>
public sealed class GracefulShutdownService : BackgroundService
{
    private readonly IShutdownCoordinator _coordinator;
    private readonly IServiceProvider _services;
    private readonly ILogger<GracefulShutdownService> _logger;
    private readonly TimeSpan _drainTimeout = TimeSpan.FromSeconds(30);
    private readonly TimeSpan _cleanupTimeout = TimeSpan.FromSeconds(5);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Attendre le signal de shutdown
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            await PerformGracefulShutdownAsync();
        }
    }
    
    private async Task PerformGracefulShutdownAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogWarning("🛑 Phase 1: Stopping acceptance of new requests...");
        await Phase1_StopAcceptingAsync();
        
        _logger.LogWarning("🔄 Phase 2: Draining in-flight requests (timeout: {Timeout}s)...", 
            _drainTimeout.TotalSeconds);
        await Phase2_DrainAsync();
        
        _logger.LogWarning("🧹 Phase 3: Cleanup (timeout: {Timeout}s)...", 
            _cleanupTimeout.TotalSeconds);
        await Phase3_CleanupAsync();
        
        _logger.LogInformation(
            "✅ Graceful shutdown completed in {ElapsedMs}ms",
            stopwatch.ElapsedMilliseconds);
    }
    
    private async Task Phase1_StopAcceptingAsync()
    {
        // Marquer comme non-healthy pour le load balancer
        var healthCheck = _services.GetService<ShutdownHealthCheck>();
        healthCheck?.SetUnhealthy();
        
        // Petit délai pour que le LB retire l'instance
        await Task.Delay(TimeSpan.FromSeconds(2));
    }
    
    private async Task Phase2_DrainAsync()
    {
        using var cts = new CancellationTokenSource(_drainTimeout);
        
        try
        {
            // Attendre que les requêtes en cours se terminent
            var requestTracker = _services.GetService<IRequestTracker>();
            if (requestTracker is not null)
            {
                await requestTracker.WaitForDrainAsync(cts.Token);
            }
            
            // Arrêter les background jobs
            var backgroundJobs = _services.GetServices<IBackgroundJob>();
            var stopTasks = backgroundJobs.Select(j => j.StopAsync(cts.Token));
            await Task.WhenAll(stopTasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("⚠️ Drain timeout exceeded, forcing continuation...");
        }
    }
    
    private async Task Phase3_CleanupAsync()
    {
        using var cts = new CancellationTokenSource(_cleanupTimeout);
        
        try
        {
            // Fermer les connexions DB proprement
            var dbContext = _services.GetService<LlmProxyDbContext>();
            if (dbContext is not null)
            {
                await dbContext.DisposeAsync();
            }
            
            // Flush les métriques
            var metricsExporter = _services.GetService<IMetricsExporter>();
            if (metricsExporter is not null)
            {
                await metricsExporter.FlushAsync(cts.Token);
            }
            
            // Flush les logs
            var loggerProvider = _services.GetService<ILoggerProvider>();
            if (loggerProvider is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("⚠️ Cleanup timeout exceeded.");
        }
    }
}
```

### 4. Tracker de Requêtes en Cours

```csharp
/// <summary>
/// Suit les requêtes en cours pour le drainage.
/// </summary>
public interface IRequestTracker
{
    IDisposable TrackRequest();
    Task WaitForDrainAsync(CancellationToken ct);
    int ActiveRequests { get; }
}

public sealed class RequestTracker : IRequestTracker
{
    private int _activeRequests;
    private readonly TaskCompletionSource _drainedTcs = new();
    private readonly ILogger<RequestTracker> _logger;
    private bool _draining;
    
    public int ActiveRequests => _activeRequests;
    
    public IDisposable TrackRequest()
    {
        Interlocked.Increment(ref _activeRequests);
        return new RequestScope(this);
    }
    
    public async Task WaitForDrainAsync(CancellationToken ct)
    {
        _draining = true;
        
        if (_activeRequests == 0)
        {
            _drainedTcs.TrySetResult();
            return;
        }
        
        _logger.LogInformation(
            "Waiting for {Count} active requests to complete...",
            _activeRequests);
        
        using var registration = ct.Register(() => _drainedTcs.TrySetCanceled());
        await _drainedTcs.Task;
    }
    
    private void OnRequestCompleted()
    {
        var remaining = Interlocked.Decrement(ref _activeRequests);
        
        if (_draining && remaining == 0)
        {
            _drainedTcs.TrySetResult();
        }
    }
    
    private sealed class RequestScope : IDisposable
    {
        private readonly RequestTracker _tracker;
        private bool _disposed;
        
        public RequestScope(RequestTracker tracker) => _tracker = tracker;
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _tracker.OnRequestCompleted();
        }
    }
}
```

### 5. Middleware de Rejet Pendant Shutdown

```csharp
/// <summary>
/// Rejette les nouvelles requêtes pendant le shutdown.
/// </summary>
public sealed class ShutdownMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IShutdownCoordinator _coordinator;
    private readonly IRequestTracker _tracker;
    private readonly ILogger<ShutdownMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Rejeter si en cours de shutdown
        if (_coordinator.IsShuttingDown)
        {
            _logger.LogDebug(
                "Rejecting request during shutdown: {Path}",
                context.Request.Path);
            
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.Headers["Retry-After"] = "30";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "SERVICE_SHUTTING_DOWN",
                message = "Service is shutting down, please retry on another instance",
                retryAfter = 30
            });
            return;
        }
        
        // Tracker la requête
        using (_tracker.TrackRequest())
        {
            await _next(context);
        }
    }
}
```

### 6. Health Check pour Load Balancer

```csharp
/// <summary>
/// Health check qui devient unhealthy pendant le shutdown.
/// </summary>
public sealed class ShutdownHealthCheck : IHealthCheck
{
    private volatile bool _isHealthy = true;
    
    public void SetUnhealthy() => _isHealthy = false;
    
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        return Task.FromResult(_isHealthy
            ? HealthCheckResult.Healthy("Service is accepting requests")
            : HealthCheckResult.Unhealthy("Service is shutting down"));
    }
}

// Enregistrement
services.AddHealthChecks()
    .AddCheck<ShutdownHealthCheck>(
        "shutdown",
        tags: new[] { "ready" });
```

### 7. Background Job avec Checkpoint

```csharp
/// <summary>
/// Job de background avec support du graceful shutdown.
/// </summary>
public abstract class GracefulBackgroundJob : BackgroundService
{
    protected readonly IShutdownCoordinator ShutdownCoordinator;
    protected readonly ILogger Logger;
    
    protected GracefulBackgroundJob(
        IShutdownCoordinator coordinator,
        ILogger logger)
    {
        ShutdownCoordinator = coordinator;
        Logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Background job {Job} started", GetType().Name);
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecuteIterationAsync(stoppingToken);
                
                // Vérifier si shutdown en cours
                if (ShutdownCoordinator.IsShuttingDown)
                {
                    Logger.LogInformation(
                        "Background job {Job} stopping due to shutdown",
                        GetType().Name);
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown
        }
        
        // Checkpoint final
        await SaveCheckpointAsync();
        Logger.LogInformation("Background job {Job} stopped", GetType().Name);
    }
    
    protected abstract Task ExecuteIterationAsync(CancellationToken ct);
    protected virtual Task SaveCheckpointAsync() => Task.CompletedTask;
}

/// <summary>
/// Exemple : Job de traitement de métriques.
/// </summary>
public sealed class MetricsAggregationJob : GracefulBackgroundJob
{
    private readonly IMetricsBuffer _buffer;
    private readonly IMetricsRepository _repository;
    
    protected override async Task ExecuteIterationAsync(CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), ct);
        
        var metrics = _buffer.Flush();
        if (metrics.Any())
        {
            await _repository.SaveBatchAsync(metrics, ct);
        }
    }
    
    protected override async Task SaveCheckpointAsync()
    {
        // Sauvegarder les métriques restantes avant arrêt
        var remaining = _buffer.Flush();
        if (remaining.Any())
        {
            await _repository.SaveBatchAsync(remaining, CancellationToken.None);
            Logger.LogInformation("Saved {Count} remaining metrics", remaining.Count);
        }
    }
}
```

### 8. Queue Consumer avec Graceful Shutdown

```csharp
/// <summary>
/// Consumer de queue avec support du graceful shutdown.
/// </summary>
public sealed class QueueConsumer : IHostedService
{
    private readonly IMessageBroker _broker;
    private readonly IShutdownCoordinator _coordinator;
    private readonly ILogger<QueueConsumer> _logger;
    private CancellationTokenSource? _cts;
    private Task? _consumeTask;
    
    public Task StartAsync(CancellationToken ct)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _consumeTask = ConsumeAsync(_cts.Token);
        return Task.CompletedTask;
    }
    
    public async Task StopAsync(CancellationToken ct)
    {
        _logger.LogInformation("Stopping queue consumer...");
        
        // Arrêter de consommer de nouveaux messages
        _cts?.Cancel();
        
        // Attendre que les messages en cours soient traités
        if (_consumeTask is not null)
        {
            await Task.WhenAny(_consumeTask, Task.Delay(Timeout.Infinite, ct));
        }
        
        _logger.LogInformation("Queue consumer stopped");
    }
    
    private async Task ConsumeAsync(CancellationToken ct)
    {
        await foreach (var message in _broker.ConsumeAsync(ct))
        {
            try
            {
                // Vérifier si shutdown en cours avant de traiter
                if (_coordinator.IsShuttingDown)
                {
                    // Rejeter le message pour qu'il soit retraité par une autre instance
                    await message.RejectAsync();
                    _logger.LogDebug("Message rejected due to shutdown");
                    continue;
                }
                
                await ProcessMessageAsync(message, ct);
                await message.AcknowledgeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                await message.NackAsync();
            }
        }
    }
    
    private async Task ProcessMessageAsync(IMessage message, CancellationToken ct)
    {
        // Traitement du message...
    }
}
```

### 9. SignalR avec Graceful Disconnect

```csharp
/// <summary>
/// Hub SignalR avec notification de shutdown aux clients.
/// </summary>
public sealed class LlmStreamingHub : Hub
{
    private readonly IShutdownCoordinator _coordinator;
    
    public override async Task OnConnectedAsync()
    {
        // Vérifier si shutdown en cours
        if (_coordinator.IsShuttingDown)
        {
            Context.Abort();
            return;
        }
        
        await base.OnConnectedAsync();
    }
}

/// <summary>
/// Service pour notifier les clients du shutdown.
/// </summary>
public sealed class SignalRShutdownNotifier : IDisposable
{
    private readonly IHubContext<LlmStreamingHub> _hubContext;
    private readonly IShutdownCoordinator _coordinator;
    private readonly IDisposable _registration;
    
    public SignalRShutdownNotifier(
        IHubContext<LlmStreamingHub> hubContext,
        IShutdownCoordinator coordinator)
    {
        _hubContext = hubContext;
        _coordinator = coordinator;
        
        // S'abonner au shutdown
        _registration = coordinator.ShutdownToken.Register(NotifyClients);
    }
    
    private async void NotifyClients()
    {
        try
        {
            await _hubContext.Clients.All.SendAsync(
                "ServiceShuttingDown",
                new
                {
                    message = "Service is restarting, please reconnect in 30 seconds",
                    reconnectDelay = 30
                });
        }
        catch
        {
            // Ignore errors during shutdown
        }
    }
    
    public void Dispose() => _registration.Dispose();
}
```

## Conséquences

### Positives

- **Zéro perte** : Requêtes en cours terminées proprement
- **Intégrité** : Données sauvegardées avant arrêt
- **Rolling update** : Déploiements sans downtime
- **Observabilité** : Logs clairs du processus

### Négatives

- **Délai d'arrêt** : 30-45s au lieu de instantané
  - *Mitigation* : Acceptable pour les bénéfices
- **Complexité** : Plus de code
  - *Mitigation* : Centraliser dans le coordinateur
- **Timeout** : Requêtes très longues peuvent être coupées
  - *Mitigation* : Checkpoint et reprise côté client

### Neutres

- Standard Kubernetes (SIGTERM + preStop)
- Compatible avec tous les orchestrateurs

## Alternatives considérées

### Option A : Arrêt brutal

- **Description** : Process.Kill() immédiat
- **Avantages** : Simple, rapide
- **Inconvénients** : Perte de données
- **Raison du rejet** : Inacceptable pour la prod

### Option B : Timeout court (5s)

- **Description** : Graceful mais rapide
- **Avantages** : Déploiements rapides
- **Inconvénients** : Requêtes LLM longues coupées
- **Raison du rejet** : 30s minimum pour LLM

## Références

- [ASP.NET Core Hosted Services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [Kubernetes Graceful Shutdown](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/)
- [Release It! - Michael Nygard](https://pragprog.com/titles/mnee2/release-it-second-edition/)
