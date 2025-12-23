# 32. Circuit Breaker Pattern

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les appels aux services externes (LLM providers) peuvent échouer ou être lents :
- **Timeout** : Provider qui ne répond pas
- **Rate limiting** : Trop de requêtes
- **Pannes** : Service temporairement indisponible
- **Cascade** : Un provider défaillant surcharge les autres

Sans protection, le système :
- Continue d'envoyer des requêtes vouées à l'échec
- Gaspille des ressources (threads, connexions)
- Dégrade l'expérience utilisateur (timeouts longs)
- Empêche la récupération du service défaillant

```csharp
// ❌ SANS CIRCUIT BREAKER : Appels répétés vers un service down
public async Task<LlmResponse> SendRequestUnsafeAsync(LlmRequest request)
{
    // Si OpenAI est down, on va timeout sur CHAQUE requête
    // pendant que les utilisateurs attendent 30 secondes à chaque fois
    return await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat", request);
}
```

## Décision

**Implémenter le pattern Circuit Breaker pour protéger le système contre les défaillances en cascade et permettre une récupération gracieuse.**

### 1. États du Circuit Breaker

```csharp
/// <summary>
/// Les trois états possibles d'un circuit breaker.
/// </summary>
public enum CircuitState
{
    /// <summary>
    /// Circuit fermé : les appels passent normalement.
    /// </summary>
    Closed,
    
    /// <summary>
    /// Circuit ouvert : les appels sont bloqués immédiatement.
    /// </summary>
    Open,
    
    /// <summary>
    /// Circuit semi-ouvert : un appel test est autorisé.
    /// </summary>
    HalfOpen
}

/// <summary>
/// Diagramme d'états :
/// 
///                    Succès (reset compteur)
///                    ┌───────────────────────────┐
///                    │                           │
///                    ▼                           │
///              ┌──────────┐                      │
///    ─────────►│  CLOSED  │◄─────────────────────┤
///              └────┬─────┘                      │
///                   │                            │
///                   │ Échecs >= seuil            │
///                   ▼                            │
///              ┌──────────┐                      │
///              │   OPEN   │───────────────┐      │
///              └────┬─────┘               │      │
///                   │                     │      │
///                   │ Timeout expiré      │      │
///                   ▼                     │      │
///              ┌──────────┐    Échec      │      │
///              │ HALF-OPEN│───────────────┘      │
///              └────┬─────┘                      │
///                   │                            │
///                   │ Succès                     │
///                   └────────────────────────────┘
/// </summary>
```

### 2. Implémentation avec Polly

```csharp
/// <summary>
/// Configuration du Circuit Breaker pour les LLM providers.
/// </summary>
public static class CircuitBreakerConfiguration
{
    /// <summary>
    /// Crée une politique de circuit breaker pour un provider.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(
        string providerName,
        ILogger logger)
    {
        return Policy<HttpResponseMessage>
            // Définir quelles réponses sont des échecs
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>() // Timeout
            .OrResult(response => (int)response.StatusCode >= 500) // Erreur serveur
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests) // Rate limit
            
            // Configuration du circuit breaker
            .AdvancedCircuitBreakerAsync(
                // Seuil d'échec : 50% des requêtes échouent
                failureThreshold: 0.5,
                // Sur une fenêtre de 10 secondes
                samplingDuration: TimeSpan.FromSeconds(10),
                // Minimum 5 requêtes pour être significatif
                minimumThroughput: 5,
                // Rester ouvert 30 secondes avant de tester
                durationOfBreak: TimeSpan.FromSeconds(30),
                
                // Callbacks pour le logging
                onBreak: (result, state, duration, context) =>
                {
                    logger.LogWarning(
                        "Circuit OPEN for {Provider}: {Reason}. Breaking for {Duration}s",
                        providerName,
                        result.Exception?.Message ?? result.Result?.StatusCode.ToString(),
                        duration.TotalSeconds);
                },
                onReset: context =>
                {
                    logger.LogInformation(
                        "Circuit CLOSED for {Provider}: Service recovered",
                        providerName);
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation(
                        "Circuit HALF-OPEN for {Provider}: Testing service",
                        providerName);
                });
    }
}
```

### 3. Service avec Circuit Breaker

```csharp
/// <summary>
/// Service d'appel aux providers LLM avec protection Circuit Breaker.
/// </summary>
public sealed class ResilientLlmProviderService
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreaker;
    private readonly ILogger<ResilientLlmProviderService> _logger;
    
    public ResilientLlmProviderService(
        HttpClient httpClient,
        IAsyncPolicy<HttpResponseMessage> circuitBreaker,
        ILogger<ResilientLlmProviderService> logger)
    {
        _httpClient = httpClient;
        _circuitBreaker = circuitBreaker;
        _logger = logger;
    }
    
    /// <summary>
    /// Envoie une requête au provider avec protection.
    /// </summary>
    public async Task<Result<LlmResponse>> SendRequestAsync(
        LlmRequest request,
        CancellationToken ct)
    {
        try
        {
            var response = await _circuitBreaker.ExecuteAsync(async token =>
            {
                var httpRequest = CreateHttpRequest(request);
                return await _httpClient.SendAsync(httpRequest, token);
            }, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                return Result<LlmResponse>.Failure(
                    $"Provider error: {response.StatusCode} - {error}");
            }
            
            var llmResponse = await response.Content
                .ReadFromJsonAsync<LlmResponse>(ct);
            
            return Result<LlmResponse>.Success(llmResponse!);
        }
        catch (BrokenCircuitException ex)
        {
            // Le circuit est ouvert - réponse immédiate sans appel
            _logger.LogWarning(
                "Circuit breaker prevented call: {Message}",
                ex.Message);
            
            return Result<LlmResponse>.Failure(
                "Service temporarily unavailable. Please retry later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling LLM provider");
            return Result<LlmResponse>.Failure(ex.Message);
        }
    }
    
    /// <summary>
    /// Vérifie l'état actuel du circuit.
    /// </summary>
    public CircuitState GetCircuitState()
    {
        if (_circuitBreaker is CircuitBreakerPolicy<HttpResponseMessage> policy)
        {
            return policy.CircuitState switch
            {
                Polly.CircuitBreaker.CircuitState.Closed => CircuitState.Closed,
                Polly.CircuitBreaker.CircuitState.Open => CircuitState.Open,
                Polly.CircuitBreaker.CircuitState.HalfOpen => CircuitState.HalfOpen,
                Polly.CircuitBreaker.CircuitState.Isolated => CircuitState.Open,
                _ => CircuitState.Closed
            };
        }
        
        return CircuitState.Closed;
    }
}
```

### 4. Enregistrement avec HttpClientFactory

```csharp
/// <summary>
/// Configuration DI pour HttpClient avec Circuit Breaker.
/// </summary>
public static class HttpClientCircuitBreakerExtensions
{
    public static IServiceCollection AddLlmProviderClient(
        this IServiceCollection services,
        string providerName,
        Action<HttpClient> configureClient)
    {
        // Politique de retry (avant le circuit breaker)
        var retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2, 4, 8 seconds
                onRetry: (outcome, delay, attempt, _) =>
                {
                    Console.WriteLine($"Retry {attempt} after {delay.TotalSeconds}s");
                });
        
        // Circuit breaker
        var circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => (int)r.StatusCode >= 500)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
        
        // Timeout global
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(60));
        
        // Combiner : Timeout > Retry > Circuit Breaker
        var combinedPolicy = Policy.WrapAsync(
            timeoutPolicy,
            retryPolicy,
            circuitBreakerPolicy);
        
        services.AddHttpClient(providerName, configureClient)
            .AddPolicyHandler(combinedPolicy);
        
        return services;
    }
}

// Program.cs
builder.Services.AddLlmProviderClient("OpenAI", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", openAiApiKey);
    client.Timeout = TimeSpan.FromSeconds(120);
});

builder.Services.AddLlmProviderClient("Anthropic", client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com/v1/");
    client.DefaultRequestHeaders.Add("x-api-key", anthropicApiKey);
});
```

### 5. Circuit Breaker avec fallback

```csharp
/// <summary>
/// Service avec fallback vers un provider secondaire.
/// </summary>
public sealed class FallbackLlmService
{
    private readonly IReadOnlyDictionary<string, ILlmProvider> _providers;
    private readonly IReadOnlyDictionary<string, IAsyncPolicy<LlmResponse>> _circuitBreakers;
    private readonly ILogger<FallbackLlmService> _logger;
    
    private static readonly string[] ProviderPriority = 
    {
        "OpenAI",
        "Anthropic",
        "Azure"
    };
    
    /// <summary>
    /// Tente les providers dans l'ordre avec fallback automatique.
    /// </summary>
    public async Task<Result<LlmResponse>> SendWithFallbackAsync(
        LlmRequest request,
        CancellationToken ct)
    {
        var errors = new List<string>();
        
        foreach (var providerName in ProviderPriority)
        {
            if (!_providers.TryGetValue(providerName, out var provider))
                continue;
            
            var circuitBreaker = _circuitBreakers[providerName];
            
            // Vérifier l'état du circuit avant d'essayer
            if (IsCircuitOpen(circuitBreaker))
            {
                _logger.LogDebug(
                    "Skipping {Provider}: circuit is open",
                    providerName);
                continue;
            }
            
            try
            {
                var response = await circuitBreaker.ExecuteAsync(
                    () => provider.SendAsync(request, ct));
                
                if (providerName != ProviderPriority[0])
                {
                    _logger.LogInformation(
                        "Request served by fallback provider {Provider}",
                        providerName);
                }
                
                return Result<LlmResponse>.Success(response);
            }
            catch (BrokenCircuitException)
            {
                errors.Add($"{providerName}: Circuit open");
                continue;
            }
            catch (Exception ex)
            {
                errors.Add($"{providerName}: {ex.Message}");
                _logger.LogWarning(
                    ex,
                    "Provider {Provider} failed, trying fallback",
                    providerName);
                continue;
            }
        }
        
        // Tous les providers ont échoué
        return Result<LlmResponse>.Failure(
            $"All providers failed: {string.Join("; ", errors)}");
    }
    
    private static bool IsCircuitOpen(IAsyncPolicy<LlmResponse> policy)
    {
        // Polly v8 avec ResilienceContext
        return false; // Simplified check
    }
}
```

### 6. Health Check basé sur le Circuit State

```csharp
/// <summary>
/// Health check qui utilise l'état des circuits.
/// </summary>
public sealed class LlmProvidersHealthCheck : IHealthCheck
{
    private readonly IEnumerable<ILlmProviderCircuitState> _providerStates;
    
    public LlmProvidersHealthCheck(
        IEnumerable<ILlmProviderCircuitState> providerStates)
    {
        _providerStates = providerStates;
    }
    
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var statuses = _providerStates
            .Select(p => new { p.ProviderName, p.CircuitState })
            .ToList();
        
        var allOpen = statuses.All(s => s.CircuitState == CircuitState.Open);
        var anyOpen = statuses.Any(s => s.CircuitState == CircuitState.Open);
        
        var data = statuses.ToDictionary(
            s => s.ProviderName,
            s => (object)s.CircuitState.ToString());
        
        if (allOpen)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "All LLM providers are unavailable",
                data: data));
        }
        
        if (anyOpen)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                "Some LLM providers are unavailable",
                data: data));
        }
        
        return Task.FromResult(HealthCheckResult.Healthy(
            "All LLM providers are available",
            data: data));
    }
}

// Registration
builder.Services.AddHealthChecks()
    .AddCheck<LlmProvidersHealthCheck>(
        "llm-providers",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "ready", "llm" });
```

### 7. Métriques du Circuit Breaker

```csharp
/// <summary>
/// Métriques pour surveiller les circuits.
/// </summary>
public sealed class CircuitBreakerMetrics
{
    private readonly ILogger<CircuitBreakerMetrics> _logger;
    private readonly Counter<long> _circuitOpenCounter;
    private readonly Counter<long> _circuitClosedCounter;
    private readonly Counter<long> _rejectedCallsCounter;
    
    public CircuitBreakerMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("LLMProxy.CircuitBreaker");
        
        _circuitOpenCounter = meter.CreateCounter<long>(
            "circuit_breaker.open_total",
            description: "Number of times circuit opened");
        
        _circuitClosedCounter = meter.CreateCounter<long>(
            "circuit_breaker.closed_total",
            description: "Number of times circuit closed");
        
        _rejectedCallsCounter = meter.CreateCounter<long>(
            "circuit_breaker.rejected_calls_total",
            description: "Calls rejected due to open circuit");
    }
    
    public void RecordCircuitOpened(string providerName, TimeSpan breakDuration)
    {
        _circuitOpenCounter.Add(1, 
            new KeyValuePair<string, object?>("provider", providerName));
        
        _logger.LogWarning(
            "Circuit opened for {Provider}, breaking for {DurationSeconds}s",
            providerName,
            breakDuration.TotalSeconds);
    }
    
    public void RecordCircuitClosed(string providerName)
    {
        _circuitClosedCounter.Add(1,
            new KeyValuePair<string, object?>("provider", providerName));
        
        _logger.LogInformation(
            "Circuit closed for {Provider}, service recovered",
            providerName);
    }
    
    public void RecordRejectedCall(string providerName)
    {
        _rejectedCallsCounter.Add(1,
            new KeyValuePair<string, object?>("provider", providerName));
    }
}
```

### 8. Polly v8 avec Resilience Pipeline

```csharp
/// <summary>
/// Configuration moderne avec Polly v8 ResiliencePipeline.
/// </summary>
public static class PollyV8Configuration
{
    public static IServiceCollection AddResilientLlmClient(
        this IServiceCollection services,
        string name)
    {
        services.AddResiliencePipeline<string, HttpResponseMessage>(name, builder =>
        {
            builder
                // 1. Rate Limiter (protège le provider)
                .AddRateLimiter(new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6
                })
                
                // 2. Timeout
                .AddTimeout(TimeSpan.FromSeconds(60))
                
                // 3. Retry avec jitter
                .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    UseJitter = true,
                    BackoffType = DelayBackoffType.Exponential,
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                        .HandleResult(r => (int)r.StatusCode >= 500)
                })
                
                // 4. Circuit Breaker
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(10),
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(30),
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(r => (int)r.StatusCode >= 500)
                });
        });
        
        return services;
    }
}

// Utilisation
public class LlmService
{
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;
    private readonly HttpClient _httpClient;
    
    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct)
    {
        return await _pipeline.ExecuteAsync(
            async token => await _httpClient.SendAsync(request, token),
            ct);
    }
}
```

## Conséquences

### Positives

- **Fail fast** : Réponse immédiate si service down
- **Protection** : Évite de surcharger un service défaillant
- **Récupération** : Permet au service de récupérer
- **Isolation** : Un provider défaillant n'impacte pas les autres
- **Observabilité** : Métriques sur l'état des circuits

### Négatives

- **Configuration** : Tuner les seuils correctement
  - *Mitigation* : Commencer conservateur, ajuster avec les métriques
- **Tests** : Difficile à tester unitairement
  - *Mitigation* : Tests d'intégration avec chaos engineering
- **Complexité** : Ajoute une couche de logique
  - *Mitigation* : Polly encapsule la complexité

### Neutres

- Le circuit breaker est un pattern standard pour la résilience
- Polly est la bibliothèque de référence en .NET

## Alternatives considérées

### Option A : Timeout simple

- **Description** : Juste un timeout sur les appels HTTP
- **Avantages** : Simple
- **Inconvénients** : Continue de surcharger le service défaillant
- **Raison du rejet** : Pas de protection système

### Option B : Bulkhead (isolation)

- **Description** : Limiter le nombre d'appels concurrents
- **Avantages** : Bonne isolation des ressources
- **Inconvénients** : Ne gère pas la récupération
- **Raison du rejet** : Complémentaire, pas suffisant seul

## Références

- [Circuit Breaker - Martin Fowler](https://martinfowler.com/bliki/CircuitBreaker.html)
- [Polly Documentation](https://github.com/App-vNext/Polly)
- [Resilience Patterns - Microsoft](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
- [Release It! - Michael Nygard](https://pragprog.com/titles/mnee2/release-it-second-edition/)
