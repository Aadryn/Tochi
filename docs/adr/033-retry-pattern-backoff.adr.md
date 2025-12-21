# 33. Retry Pattern avec Backoff Exponentiel

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les erreurs transitoires sont courantes dans les systèmes distribués :
- **Réseau** : Perte de connexion temporaire
- **Rate limiting** : Trop de requêtes simultanées
- **Surcharge** : Service temporairement surchargé
- **Timeout** : Requête qui n'aboutit pas à temps

Sans retry, ces erreurs temporaires deviennent des échecs permanents :

```csharp
// ❌ SANS RETRY : Échec définitif au premier problème
public async Task<LlmResponse> SendRequestUnsafeAsync(LlmRequest request)
{
    // Une erreur réseau = échec total
    // Le client doit retry manuellement
    return await _httpClient.PostAsJsonAsync<LlmResponse>(url, request);
}
```

## Décision

**Implémenter le pattern Retry avec backoff exponentiel et jitter pour gérer les erreurs transitoires de manière résiliente.**

### 1. Principes du Retry

```csharp
/// <summary>
/// Principes fondamentaux du retry pattern :
/// 
/// 1. IDEMPOTENCE : Ne retry que les opérations idempotentes
///    - GET, PUT, DELETE : OK
///    - POST : Attention ! Peut créer des doublons
/// 
/// 2. SÉLECTIVITÉ : Ne retry que les erreurs transitoires
///    - 429 TooManyRequests : ✅ Retry
///    - 503 ServiceUnavailable : ✅ Retry
///    - 500 InternalServerError : ⚠️ Parfois
///    - 400 BadRequest : ❌ Jamais (erreur client)
///    - 401 Unauthorized : ❌ Jamais (credentials)
/// 
/// 3. BACKOFF : Augmenter le délai entre les retries
///    - Évite de surcharger un service déjà en difficulté
///    - Linéaire : 1s, 2s, 3s, 4s
///    - Exponentiel : 1s, 2s, 4s, 8s
/// 
/// 4. JITTER : Ajouter de l'aléatoire au délai
///    - Évite le "thundering herd" (toutes les retries en même temps)
///    - 1s ± 0.5s, 2s ± 1s, 4s ± 2s
/// 
/// 5. LIMITE : Nombre max de retries
///    - Évite les boucles infinies
///    - Généralement 3-5 retries
/// </summary>
```

### 2. Implémentation avec Polly

```csharp
/// <summary>
/// Configuration du retry avec backoff exponentiel.
/// </summary>
public static class RetryPolicyConfiguration
{
    /// <summary>
    /// Crée une politique de retry pour les appels HTTP.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(
        ILogger logger)
    {
        return Policy<HttpResponseMessage>
            // Quelles erreurs retry ?
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .OrResult(r => r.StatusCode == HttpStatusCode.ServiceUnavailable)
            .OrResult(r => r.StatusCode == HttpStatusCode.GatewayTimeout)
            .OrResult(r => r.StatusCode == HttpStatusCode.RequestTimeout)
            
            // Configuration du retry
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: (attempt, result, context) =>
                {
                    // Backoff exponentiel : 2^attempt secondes
                    var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    
                    // Respecter Retry-After si présent
                    if (result.Result?.Headers.RetryAfter?.Delta is { } retryAfter)
                    {
                        return retryAfter;
                    }
                    
                    // Jitter : ±50% du délai
                    var jitter = baseDelay * Random.Shared.NextDouble() * 0.5;
                    var finalDelay = baseDelay + jitter;
                    
                    return finalDelay;
                },
                onRetryAsync: async (outcome, delay, attempt, context) =>
                {
                    logger.LogWarning(
                        "Retry attempt {Attempt} after {DelayMs}ms due to {Reason}",
                        attempt,
                        delay.TotalMilliseconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    
                    await Task.CompletedTask;
                });
    }
}
```

### 3. Retry sélectif selon le type d'erreur

```csharp
/// <summary>
/// Stratégie de retry différenciée selon l'erreur.
/// </summary>
public sealed class SelectiveRetryPolicy
{
    private readonly ILogger<SelectiveRetryPolicy> _logger;
    
    /// <summary>
    /// Détermine si une erreur mérite un retry.
    /// </summary>
    public bool ShouldRetry(HttpResponseMessage response, int attemptNumber)
    {
        // Limite absolue
        if (attemptNumber >= 5)
        {
            _logger.LogWarning("Max retry attempts reached");
            return false;
        }
        
        return response.StatusCode switch
        {
            // ✅ Toujours retry
            HttpStatusCode.TooManyRequests => true,
            HttpStatusCode.ServiceUnavailable => true,
            HttpStatusCode.GatewayTimeout => true,
            HttpStatusCode.RequestTimeout => true,
            
            // ⚠️ Retry conditionnel (erreur serveur transitoire possible)
            HttpStatusCode.InternalServerError => attemptNumber < 2,
            HttpStatusCode.BadGateway => attemptNumber < 3,
            
            // ❌ Jamais retry (erreur client ou permanente)
            HttpStatusCode.BadRequest => false,
            HttpStatusCode.Unauthorized => false,
            HttpStatusCode.Forbidden => false,
            HttpStatusCode.NotFound => false,
            HttpStatusCode.Conflict => false,
            HttpStatusCode.UnprocessableEntity => false,
            
            // Toute autre erreur 4xx : pas de retry
            _ when (int)response.StatusCode >= 400 
                   && (int)response.StatusCode < 500 => false,
            
            // Erreur 5xx non listée : retry avec prudence
            _ when (int)response.StatusCode >= 500 => attemptNumber < 2,
            
            // Succès ou autres : pas de retry
            _ => false
        };
    }
    
    /// <summary>
    /// Calcule le délai de retry selon le contexte.
    /// </summary>
    public TimeSpan CalculateDelay(
        HttpResponseMessage response, 
        int attemptNumber)
    {
        // 1. Respecter Retry-After du serveur
        if (response.Headers.RetryAfter?.Delta is { } retryAfter)
        {
            _logger.LogDebug(
                "Using server Retry-After: {Seconds}s",
                retryAfter.TotalSeconds);
            
            // Limiter à 60 secondes max
            return retryAfter > TimeSpan.FromSeconds(60)
                ? TimeSpan.FromSeconds(60)
                : retryAfter;
        }
        
        // 2. Délai spécial pour rate limiting (plus long)
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(3, attemptNumber)); // 3, 9, 27s
            return AddJitter(delay);
        }
        
        // 3. Backoff exponentiel standard
        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, attemptNumber)); // 2, 4, 8s
        return AddJitter(baseDelay);
    }
    
    private static TimeSpan AddJitter(TimeSpan delay)
    {
        // Jitter : ±25% du délai
        var jitterRange = delay * 0.25;
        var jitter = (Random.Shared.NextDouble() * 2 - 1) * jitterRange.TotalMilliseconds;
        return delay + TimeSpan.FromMilliseconds(jitter);
    }
}
```

### 4. Retry pour opérations non-HTTP

```csharp
/// <summary>
/// Service de retry générique pour toute opération.
/// </summary>
public sealed class RetryService
{
    private readonly ILogger<RetryService> _logger;
    
    /// <summary>
    /// Exécute une opération avec retry.
    /// </summary>
    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        RetryOptions options,
        CancellationToken ct)
    {
        var attempt = 0;
        var exceptions = new List<Exception>();
        
        while (true)
        {
            attempt++;
            
            try
            {
                return await operation(ct);
            }
            catch (Exception ex) when (ShouldRetry(ex, attempt, options))
            {
                exceptions.Add(ex);
                
                var delay = CalculateDelay(attempt, options);
                
                _logger.LogWarning(
                    ex,
                    "Operation failed, attempt {Attempt}/{MaxAttempts}. " +
                    "Retrying in {DelayMs}ms",
                    attempt,
                    options.MaxAttempts,
                    delay.TotalMilliseconds);
                
                await Task.Delay(delay, ct);
            }
            catch (Exception ex)
            {
                // Erreur non-retriable
                _logger.LogError(
                    ex,
                    "Operation failed with non-retryable error after {Attempt} attempt(s)",
                    attempt);
                
                throw;
            }
        }
    }
    
    private bool ShouldRetry(Exception ex, int attempt, RetryOptions options)
    {
        if (attempt >= options.MaxAttempts)
            return false;
        
        // Vérifier si l'exception est dans la liste des retriables
        return options.RetryableExceptions.Any(type => type.IsInstanceOfType(ex))
               && !options.NonRetryableExceptions.Any(type => type.IsInstanceOfType(ex));
    }
    
    private TimeSpan CalculateDelay(int attempt, RetryOptions options)
    {
        var delay = options.BackoffType switch
        {
            BackoffType.Constant => options.InitialDelay,
            BackoffType.Linear => options.InitialDelay * attempt,
            BackoffType.Exponential => TimeSpan.FromMilliseconds(
                options.InitialDelay.TotalMilliseconds * Math.Pow(2, attempt - 1)),
            _ => options.InitialDelay
        };
        
        // Limiter le délai max
        if (delay > options.MaxDelay)
            delay = options.MaxDelay;
        
        // Ajouter jitter si activé
        if (options.UseJitter)
        {
            var jitter = delay * Random.Shared.NextDouble() * 0.5;
            delay += jitter;
        }
        
        return delay;
    }
}

/// <summary>
/// Options de configuration du retry.
/// </summary>
public sealed class RetryOptions
{
    public int MaxAttempts { get; init; } = 3;
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(30);
    public BackoffType BackoffType { get; init; } = BackoffType.Exponential;
    public bool UseJitter { get; init; } = true;
    
    public IReadOnlyList<Type> RetryableExceptions { get; init; } = new[]
    {
        typeof(HttpRequestException),
        typeof(TimeoutException),
        typeof(TaskCanceledException)
    };
    
    public IReadOnlyList<Type> NonRetryableExceptions { get; init; } = new[]
    {
        typeof(ArgumentException),
        typeof(InvalidOperationException),
        typeof(UnauthorizedAccessException)
    };
}

public enum BackoffType
{
    Constant,
    Linear,
    Exponential
}
```

### 5. Retry avec contexte et métriques

```csharp
/// <summary>
/// Retry avec collecte de métriques.
/// </summary>
public sealed class MeteredRetryPolicy
{
    private readonly ILogger<MeteredRetryPolicy> _logger;
    private readonly Counter<long> _retryCounter;
    private readonly Histogram<double> _retryDelayHistogram;
    
    public MeteredRetryPolicy(IMeterFactory meterFactory, ILogger<MeteredRetryPolicy> logger)
    {
        _logger = logger;
        var meter = meterFactory.Create("LLMProxy.Retry");
        
        _retryCounter = meter.CreateCounter<long>(
            "retry.attempts_total",
            description: "Total retry attempts");
        
        _retryDelayHistogram = meter.CreateHistogram<double>(
            "retry.delay_seconds",
            description: "Retry delay duration");
    }
    
    public IAsyncPolicy<HttpResponseMessage> CreatePolicy(string operationName)
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => (int)r.StatusCode >= 500)
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: (attempt, outcome, context) =>
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    
                    // Enregistrer la métrique
                    _retryDelayHistogram.Record(
                        delay.TotalSeconds,
                        new KeyValuePair<string, object?>("operation", operationName),
                        new KeyValuePair<string, object?>("attempt", attempt));
                    
                    return delay;
                },
                onRetryAsync: (outcome, delay, attempt, context) =>
                {
                    // Incrémenter le compteur
                    _retryCounter.Add(1,
                        new KeyValuePair<string, object?>("operation", operationName),
                        new KeyValuePair<string, object?>("reason", GetRetryReason(outcome)));
                    
                    _logger.LogWarning(
                        "Retry {Attempt} for {Operation}: {Reason}. Waiting {DelayMs}ms",
                        attempt,
                        operationName,
                        GetRetryReason(outcome),
                        delay.TotalMilliseconds);
                    
                    return Task.CompletedTask;
                });
    }
    
    private static string GetRetryReason(DelegateResult<HttpResponseMessage> outcome)
    {
        if (outcome.Exception is not null)
            return outcome.Exception.GetType().Name;
        
        return outcome.Result?.StatusCode.ToString() ?? "Unknown";
    }
}
```

### 6. Polly v8 Retry Strategy

```csharp
/// <summary>
/// Configuration moderne avec Polly v8.
/// </summary>
public static class PollyV8RetryConfiguration
{
    public static ResiliencePipeline<HttpResponseMessage> CreateRetryPipeline()
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                // Nombre de retries
                MaxRetryAttempts = 3,
                
                // Type de backoff
                BackoffType = DelayBackoffType.Exponential,
                
                // Délai initial
                Delay = TimeSpan.FromSeconds(1),
                
                // Ajouter du jitter
                UseJitter = true,
                
                // Quoi retry
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .HandleResult(static response => response.StatusCode switch
                    {
                        HttpStatusCode.TooManyRequests => true,
                        HttpStatusCode.ServiceUnavailable => true,
                        HttpStatusCode.GatewayTimeout => true,
                        _ when (int)response.StatusCode >= 500 => true,
                        _ => false
                    }),
                
                // Callback sur retry
                OnRetry = static args =>
                {
                    Console.WriteLine(
                        $"Retry {args.AttemptNumber} after {args.RetryDelay.TotalMilliseconds}ms");
                    return ValueTask.CompletedTask;
                },
                
                // Calculer le délai dynamiquement
                DelayGenerator = static args =>
                {
                    // Respecter Retry-After
                    if (args.Outcome.Result?.Headers.RetryAfter?.Delta is { } retryAfter)
                    {
                        return new ValueTask<TimeSpan?>(retryAfter);
                    }
                    
                    // Laisser le backoff par défaut
                    return new ValueTask<TimeSpan?>((TimeSpan?)null);
                }
            })
            .Build();
    }
}
```

### 7. Retry avec idempotency key

```csharp
/// <summary>
/// Garantir l'idempotence des opérations avec retry.
/// </summary>
public sealed class IdempotentRetryService
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    
    /// <summary>
    /// Envoie une requête avec idempotency key pour éviter les doublons.
    /// </summary>
    public async Task<HttpResponseMessage> SendWithRetryAsync(
        HttpRequestMessage request,
        CancellationToken ct)
    {
        // Générer une clé unique pour cette requête logique
        var idempotencyKey = Guid.NewGuid().ToString();
        
        return await _retryPolicy.ExecuteAsync(async token =>
        {
            // Clone la requête (HttpRequestMessage ne peut pas être réutilisé)
            var clonedRequest = await CloneRequestAsync(request);
            
            // Ajouter la clé d'idempotence
            // Le serveur doit retourner le même résultat si la clé existe déjà
            clonedRequest.Headers.Add("Idempotency-Key", idempotencyKey);
            
            return await _httpClient.SendAsync(clonedRequest, token);
        }, ct);
    }
    
    private static async Task<HttpRequestMessage> CloneRequestAsync(
        HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        
        // Copier le contenu si présent
        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);
            
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(
                    header.Key, 
                    header.Value);
            }
        }
        
        // Copier les headers
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        
        return clone;
    }
}
```

### 8. Tester le retry

```csharp
/// <summary>
/// Tests unitaires pour les politiques de retry.
/// </summary>
public sealed class RetryPolicyTests
{
    [Fact]
    public async Task Should_Retry_On_Transient_Error()
    {
        // Arrange
        var attempts = 0;
        var policy = Policy<string>
            .Handle<HttpRequestException>()
            .RetryAsync(3);
        
        // Act
        var result = await policy.ExecuteAsync(() =>
        {
            attempts++;
            if (attempts < 3)
                throw new HttpRequestException("Transient error");
            return Task.FromResult("Success");
        });
        
        // Assert
        result.Should().Be("Success");
        attempts.Should().Be(3);
    }
    
    [Fact]
    public async Task Should_Not_Retry_On_NonTransient_Error()
    {
        // Arrange
        var attempts = 0;
        var policy = Policy<string>
            .Handle<HttpRequestException>()
            .RetryAsync(3);
        
        // Act
        var act = () => policy.ExecuteAsync(() =>
        {
            attempts++;
            throw new InvalidOperationException("Non-transient");
        });
        
        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        attempts.Should().Be(1);
    }
    
    [Fact]
    public async Task Should_Respect_RetryAfter_Header()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        handler.SetupSequence(
            new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Headers = { RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(1)) }
            },
            new HttpResponseMessage(HttpStatusCode.OK));
        
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        await ExecuteWithRetryAsync(handler);
        
        // Assert
        stopwatch.Elapsed.Should().BeGreaterThan(TimeSpan.FromSeconds(1));
    }
}
```

## Conséquences

### Positives

- **Résilience** : Gère les erreurs transitoires automatiquement
- **UX** : L'utilisateur ne voit pas les erreurs temporaires
- **Protection** : Backoff protège le service distant
- **Observabilité** : Métriques sur les retries

### Négatives

- **Latence** : Les retries ajoutent du délai
  - *Mitigation* : Timeout global + limiter les retries
- **Idempotence** : Risque de doublons si mal géré
  - *Mitigation* : Idempotency keys
- **Complexité** : Configuration à affiner
  - *Mitigation* : Polly encapsule la complexité

### Neutres

- Le retry est complémentaire au circuit breaker
- Doit être combiné avec timeout pour éviter les blocages

## Alternatives considérées

### Option A : Pas de retry (fail fast)

- **Description** : Échouer immédiatement à la première erreur
- **Avantages** : Simple, rapide
- **Inconvénients** : Pas de résilience aux erreurs transitoires
- **Raison du rejet** : UX dégradée

### Option B : Retry infini

- **Description** : Retry jusqu'au succès
- **Avantages** : Garantie de succès éventuel
- **Inconvénients** : Peut bloquer indéfiniment
- **Raison du rejet** : Risque de boucle infinie

## Références

- [Retry Pattern - Microsoft](https://docs.microsoft.com/en-us/azure/architecture/patterns/retry)
- [Polly Retry](https://github.com/App-vNext/Polly/wiki/Retry)
- [Exponential Backoff - AWS](https://aws.amazon.com/blogs/architecture/exponential-backoff-and-jitter/)
- [Jitter - Marc Brooker](https://aws.amazon.com/builders-library/timeouts-retries-and-backoff-with-jitter/)
