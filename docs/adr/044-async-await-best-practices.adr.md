# 44. Async/Await Best Practices

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM effectue de nombreuses opérations I/O :
- Appels HTTP vers les providers (OpenAI, Anthropic)
- Requêtes base de données
- Accès cache Redis
- Streaming de réponses

Une mauvaise utilisation de async/await peut causer :
- Deadlocks
- Thread pool starvation
- Memory leaks
- Performances dégradées

```csharp
// ❌ MAUVAISES PRATIQUES : Problèmes classiques

// 1. Bloquer sur async (DEADLOCK potentiel)
public string GetResponse()
{
    return GetResponseAsync().Result; // ❌ DEADLOCK en ASP.NET
}

// 2. async void (exceptions non catchées)
public async void ProcessRequest()  // ❌ Fire-and-forget dangereux
{
    await DoWorkAsync();
}

// 3. Oublier ConfigureAwait dans les libraries
public async Task<string> LibraryMethodAsync()
{
    return await httpClient.GetStringAsync(url); // ❌ Capture le contexte
}
```

## Décision

**Adopter des règles strictes pour l'utilisation de async/await garantissant performance, fiabilité et absence de deadlocks.**

### Règles Fondamentales

```csharp
/// <summary>
/// RÈGLES ASYNC/AWAIT
/// 
/// 1. NEVER block on async (.Result, .Wait(), .GetAwaiter().GetResult())
/// 2. ALWAYS use async Task, NEVER async void (sauf event handlers)
/// 3. ALWAYS pass CancellationToken
/// 4. ALWAYS use ConfigureAwait(false) dans les libraries
/// 5. PREFER ValueTask pour les hot paths
/// 6. NEVER await dans un lock
/// 7. ALWAYS dispose les resources async (IAsyncDisposable)
/// </summary>
```

### 1. Propagation Async Complète

```csharp
// ═══════════════════════════════════════════════════════════════
// ASYNC ALL THE WAY
// ═══════════════════════════════════════════════════════════════

// ❌ MAUVAIS : Mélange sync/async
public class BadService
{
    public string GetTenantName(Guid tenantId)
    {
        // Bloque le thread, risque de deadlock
        return GetTenantNameAsync(tenantId).Result;
    }
    
    private async Task<string> GetTenantNameAsync(Guid tenantId)
    {
        var tenant = await _repository.GetByIdAsync(tenantId);
        return tenant?.Name ?? "Unknown";
    }
}

// ✅ BON : Async jusqu'au bout
public class GoodService
{
    public async Task<string> GetTenantNameAsync(
        Guid tenantId,
        CancellationToken ct = default)
    {
        var tenant = await _repository.GetByIdAsync(tenantId, ct);
        return tenant?.Name ?? "Unknown";
    }
}

// ✅ Controller async
[ApiController]
public class TenantsController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTenant(
        Guid id,
        CancellationToken ct) // Automatiquement fourni par ASP.NET
    {
        var name = await _service.GetTenantNameAsync(id, ct);
        return Ok(new { name });
    }
}
```

### 2. CancellationToken partout

```csharp
/// <summary>
/// CancellationToken DOIT être propagé dans toute la chaîne.
/// </summary>

// ❌ MAUVAIS : Pas de CancellationToken
public async Task<LlmResponse> SendRequestAsync(LlmRequest request)
{
    var response = await _httpClient.PostAsync(url, content);
    // Si le client abandonne, la requête continue inutilement
    return await ParseResponseAsync(response);
}

// ✅ BON : CancellationToken propagé
public async Task<LlmResponse> SendRequestAsync(
    LlmRequest request,
    CancellationToken ct = default)
{
    ct.ThrowIfCancellationRequested();
    
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    linkedCts.CancelAfter(TimeSpan.FromSeconds(30)); // Timeout
    
    var response = await _httpClient.PostAsync(url, content, linkedCts.Token);
    return await ParseResponseAsync(response, linkedCts.Token);
}

/// <summary>
/// Pattern pour timeout avec CancellationToken.
/// </summary>
public async Task<T> WithTimeoutAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    TimeSpan timeout,
    CancellationToken ct = default)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(timeout);
    
    try
    {
        return await operation(cts.Token);
    }
    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
    {
        throw new TimeoutException($"Operation timed out after {timeout}");
    }
}
```

### 3. ConfigureAwait dans les Libraries

```csharp
/// <summary>
/// ConfigureAwait(false) dans les libraries pour éviter les deadlocks
/// et améliorer les performances.
/// </summary>

// Dans le code APPLICATION (Controllers, Services appelés par l'app)
// => PAS besoin de ConfigureAwait(false), le contexte est géré par ASP.NET

public class ApplicationService
{
    public async Task<TenantDto> GetTenantAsync(Guid id, CancellationToken ct)
    {
        // Pas de ConfigureAwait - on veut garder le contexte HTTP
        var tenant = await _repository.GetByIdAsync(id, ct);
        return tenant.ToDto();
    }
}

// Dans le code INFRASTRUCTURE (Libraries, packages NuGet)
// => TOUJOURS ConfigureAwait(false)

public class RedisCache : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
        where T : class
    {
        // ConfigureAwait(false) - on ne dépend pas du contexte
        var value = await _redis.StringGetAsync(key)
            .ConfigureAwait(false);
        
        if (value.IsNullOrEmpty)
            return null;
        
        return JsonSerializer.Deserialize<T>(value!)
            ?? throw new InvalidOperationException("Deserialization failed");
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry, CancellationToken ct)
        where T : class
    {
        var json = JsonSerializer.Serialize(value);
        
        await _redis.StringSetAsync(key, json, expiry)
            .ConfigureAwait(false);
    }
}
```

### 4. ValueTask pour les Hot Paths

```csharp
/// <summary>
/// ValueTask évite l'allocation de Task quand le résultat est synchrone.
/// Idéal pour les caches où le hit rate est élevé.
/// </summary>

// ❌ Task : Toujours une allocation
public async Task<T?> GetFromCacheAsync<T>(string key)
{
    if (_memoryCache.TryGetValue(key, out T? value))
        return value; // Allocation inutile de Task
    
    return await _redisCache.GetAsync<T>(key);
}

// ✅ ValueTask : Pas d'allocation si synchrone
public ValueTask<T?> GetFromCacheAsync<T>(string key)
{
    // Hit L1 = synchrone, pas d'allocation
    if (_memoryCache.TryGetValue(key, out T? value))
        return ValueTask.FromResult<T?>(value);
    
    // Miss = async vers Redis
    return new ValueTask<T?>(GetFromRedisAsync<T>(key));
}

/// <summary>
/// ATTENTION : ValueTask ne peut être awaité qu'une fois !
/// </summary>
public async Task DemoValueTaskRules()
{
    var valueTask = GetFromCacheAsync<string>("key");
    
    // ❌ MAUVAIS : Await multiple
    // var result1 = await valueTask;
    // var result2 = await valueTask; // ERREUR !
    
    // ✅ BON : Un seul await
    var result = await valueTask;
    
    // ✅ Ou conversion en Task si besoin de réutiliser
    var task = valueTask.AsTask();
    var r1 = await task;
    var r2 = await task; // OK car c'est un Task
}
```

### 5. Éviter les Captures de Contexte Inutiles

```csharp
/// <summary>
/// Éviter les closures qui capturent trop de contexte.
/// </summary>

// ❌ MAUVAIS : Capture this et toutes ses dépendances
public class HeavyService
{
    private readonly ILogger _logger;
    private readonly IRepository _repo;
    private readonly ICache _cache;
    // ... 10 autres dépendances
    
    public async Task ProcessAsync(Guid id, CancellationToken ct)
    {
        // Lambda capture 'this' = toutes les dépendances en mémoire
        await Task.Run(async () =>
        {
            _logger.LogInformation("Processing {Id}", id);
            var data = await _repo.GetAsync(id, ct);
        }, ct);
    }
}

// ✅ BON : Passer uniquement ce qui est nécessaire
public class LightService
{
    public async Task ProcessAsync(Guid id, CancellationToken ct)
    {
        var logger = _logger;
        var repo = _repo;
        
        // Capture seulement logger et repo
        await Task.Run(async () =>
        {
            logger.LogInformation("Processing {Id}", id);
            var data = await repo.GetAsync(id, ct);
        }, ct);
    }
}

// ✅ ENCORE MIEUX : Méthode locale statique
public async Task ProcessAsync(Guid id, CancellationToken ct)
{
    await ProcessInternalAsync(id, _logger, _repo, ct);
    
    static async Task ProcessInternalAsync(
        Guid id,
        ILogger logger,
        IRepository repo,
        CancellationToken ct)
    {
        logger.LogInformation("Processing {Id}", id);
        var data = await repo.GetAsync(id, ct);
    }
}
```

### 6. Parallélisme Contrôlé

```csharp
/// <summary>
/// Exécution parallèle avec contrôle du degré de parallélisme.
/// </summary>

// ❌ MAUVAIS : Parallélisme non contrôlé
public async Task ProcessAllTenantsAsync(IEnumerable<Guid> tenantIds)
{
    // Lance TOUTES les requêtes en même temps = surcharge
    var tasks = tenantIds.Select(id => ProcessTenantAsync(id));
    await Task.WhenAll(tasks);
}

// ✅ BON : Parallélisme limité avec SemaphoreSlim
public async Task ProcessAllTenantsAsync(
    IEnumerable<Guid> tenantIds,
    CancellationToken ct)
{
    const int maxDegreeOfParallelism = 10;
    using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
    
    var tasks = tenantIds.Select(async id =>
    {
        await semaphore.WaitAsync(ct);
        try
        {
            await ProcessTenantAsync(id, ct);
        }
        finally
        {
            semaphore.Release();
        }
    });
    
    await Task.WhenAll(tasks);
}

// ✅ BON : Parallel.ForEachAsync (.NET 6+)
public async Task ProcessAllTenantsAsync(
    IEnumerable<Guid> tenantIds,
    CancellationToken ct)
{
    await Parallel.ForEachAsync(
        tenantIds,
        new ParallelOptions
        {
            MaxDegreeOfParallelism = 10,
            CancellationToken = ct
        },
        async (id, ct) =>
        {
            await ProcessTenantAsync(id, ct);
        });
}
```

### 7. Streaming Async (IAsyncEnumerable)

```csharp
/// <summary>
/// Streaming de données volumineuses avec IAsyncEnumerable.
/// Idéal pour les réponses LLM en streaming.
/// </summary>

// ❌ MAUVAIS : Charger tout en mémoire
public async Task<List<string>> GetAllTokensAsync(string prompt)
{
    var tokens = new List<string>();
    await foreach (var token in GenerateTokensAsync(prompt))
    {
        tokens.Add(token); // Accumule tout en mémoire
    }
    return tokens;
}

// ✅ BON : Streaming vers le client
public async IAsyncEnumerable<string> StreamResponseAsync(
    string prompt,
    [EnumeratorCancellation] CancellationToken ct = default)
{
    await foreach (var token in _llmProvider.GenerateAsync(prompt, ct))
    {
        yield return token;
    }
}

// ✅ Controller avec streaming SSE
[HttpPost("chat/completions")]
public async Task StreamChatCompletion(
    [FromBody] ChatRequest request,
    CancellationToken ct)
{
    Response.ContentType = "text/event-stream";
    
    await foreach (var chunk in _service.StreamResponseAsync(request, ct))
    {
        await Response.WriteAsync($"data: {chunk}\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }
    
    await Response.WriteAsync("data: [DONE]\n\n", ct);
}

/// <summary>
/// Transformation de stream.
/// </summary>
public async IAsyncEnumerable<ProcessedChunk> TransformStreamAsync(
    IAsyncEnumerable<RawChunk> source,
    [EnumeratorCancellation] CancellationToken ct = default)
{
    await foreach (var chunk in source.WithCancellation(ct))
    {
        var processed = ProcessChunk(chunk);
        if (processed is not null)
        {
            yield return processed;
        }
    }
}
```

### 8. Dispose Async

```csharp
/// <summary>
/// IAsyncDisposable pour les ressources async.
/// </summary>
public class LlmProviderClient : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;
    
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        
        _disposed = true;
        
        // Dispose async des ressources
        _semaphore.Dispose();
        _httpClient.Dispose();
        
        // Supprimer le GC Finalizer si présent
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Utilisation avec await using.
/// </summary>
public async Task UseClientAsync()
{
    await using var client = new LlmProviderClient();
    await client.SendAsync(request);
} // DisposeAsync appelé automatiquement
```

### 9. Éviter les Pièges Courants

```csharp
/// <summary>
/// Pièges async/await et solutions.
/// </summary>

// ═══════════════════════════════════════════════════════════════
// PIÈGE 1 : async void
// ═══════════════════════════════════════════════════════════════

// ❌ MAUVAIS : Exceptions non catchées
public async void BadMethod()
{
    throw new Exception("Nobody will catch this!");
}

// ✅ BON : async Task
public async Task GoodMethod()
{
    throw new Exception("This can be caught");
}

// ✅ EXCEPTION : Event handlers (seul cas valide)
private async void Button_Click(object sender, EventArgs e)
{
    try
    {
        await ProcessClickAsync();
    }
    catch (Exception ex)
    {
        // Gérer l'exception explicitement
        _logger.LogError(ex, "Click processing failed");
    }
}

// ═══════════════════════════════════════════════════════════════
// PIÈGE 2 : Await dans un lock
// ═══════════════════════════════════════════════════════════════

// ❌ MAUVAIS : Compile pas (et c'est tant mieux)
// lock (_syncObject)
// {
//     await DoWorkAsync(); // Erreur de compilation
// }

// ✅ BON : SemaphoreSlim
private readonly SemaphoreSlim _asyncLock = new(1, 1);

public async Task SafeMethodAsync()
{
    await _asyncLock.WaitAsync();
    try
    {
        await DoWorkAsync();
    }
    finally
    {
        _asyncLock.Release();
    }
}

// ═══════════════════════════════════════════════════════════════
// PIÈGE 3 : Task.Run inutile
// ═══════════════════════════════════════════════════════════════

// ❌ MAUVAIS : Task.Run sur du code déjà async
public async Task BadAsync()
{
    await Task.Run(async () =>
    {
        await httpClient.GetAsync(url); // Déjà async !
    });
}

// ✅ BON : Appel direct
public async Task GoodAsync()
{
    await httpClient.GetAsync(url);
}

// ✅ Task.Run pour du code CPU-bound seulement
public async Task ComputeAsync()
{
    var result = await Task.Run(() =>
    {
        // Calcul intensif CPU
        return ComputeHeavyCalculation();
    });
}
```

### 10. Testing Async

```csharp
/// <summary>
/// Patterns de test pour le code async.
/// </summary>
public class AsyncServiceTests
{
    [Fact]
    public async Task GetTenant_WhenExists_ReturnsTenant()
    {
        // Arrange
        var service = CreateService();
        
        // Act - await direct, pas de .Result
        var result = await service.GetTenantAsync(tenantId, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetTenant_WhenCancelled_ThrowsOperationCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.GetTenantAsync(tenantId, cts.Token));
    }
    
    [Fact]
    public async Task StreamResponse_YieldsTokens()
    {
        // Arrange
        var service = CreateService();
        var tokens = new List<string>();
        
        // Act
        await foreach (var token in service.StreamResponseAsync("prompt"))
        {
            tokens.Add(token);
        }
        
        // Assert
        tokens.Should().NotBeEmpty();
    }
}
```

## Conséquences

### Positives

- **Performance** : Pas de threads bloqués
- **Scalabilité** : Meilleure utilisation du thread pool
- **Réactivité** : Annulation rapide des requêtes
- **Fiabilité** : Pas de deadlocks

### Négatives

- **Complexité** : Plus verbeux que le code synchrone
  - *Mitigation* : Guidelines claires, analyseurs statiques
- **Debugging** : Stack traces async plus complexes
  - *Mitigation* : Structured logging, correlation IDs

### Neutres

- Pattern standard en .NET moderne
- Support outillage mature (Roslyn analyzers)

## Alternatives considérées

### Option A : Code synchrone

- **Description** : Éviter async complètement
- **Avantages** : Plus simple
- **Inconvénients** : Pas scalable, bloque les threads
- **Raison du rejet** : Incompatible avec les besoins I/O du proxy

### Option B : Reactive Extensions (Rx)

- **Description** : Observable/Observer pattern
- **Avantages** : Puissant pour le streaming
- **Inconvénients** : Courbe d'apprentissage, overkill
- **Raison du rejet** : IAsyncEnumerable suffit

## Références

- [Async Best Practices - Microsoft](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)
- [ConfigureAwait FAQ](https://devblogs.microsoft.com/dotnet/configureawait-faq/)
- [ValueTask Guidance](https://devblogs.microsoft.com/dotnet/understanding-the-whys-whats-and-whens-of-valuetask/)
