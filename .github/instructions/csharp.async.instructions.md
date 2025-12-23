---
description: Asynchronous C# programming - async/await, Task, parallelism, cancellation, best practices
name: CSharp_Async_Programming
applyTo: "**/backend/*Service.cs,**/backend/*Handler.cs,**/backend/*Repository.cs,**/backend/*Controller.cs"
---

# Programmation Asynchrone C# - Guide Complet

Guide exhaustif pour maîtriser `async`/`await`, `Task`, parallélisme, cancellation, et éviter les pièges classiques.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** `.Result` ou `.Wait()` sur une Task (deadlock potentiel)
- **Ne bloque jamais** un contexte de synchronisation avec du code synchrone
- **N'oublie jamais** le suffixe `Async` sur les méthodes asynchrones
- **Ne crée jamais** de méthode `async void` (sauf pour event handlers)
- **N'ignore jamais** les `CancellationToken` passés en paramètre
- **N'utilise jamais** `Task.Run()` pour du code déjà asynchrone
- **N'oublie jamais** `.ConfigureAwait(false)` dans les bibliothèques

## ✅ À FAIRE

- **Propage toujours** l'async du bas vers le haut (async all the way)
- **Utilise toujours** `await` au lieu de `.Result` ou `.Wait()`
- **Nomme toujours** les méthodes async avec le suffixe `Async`
- **Retourne toujours** `Task` ou `Task<T>` (jamais `async void`)
- **Propage toujours** les `CancellationToken` jusqu'aux API de bas niveau
- **Utilise toujours** `ValueTask<T>` pour les chemins chauds avec retour fréquent synchrone
- **Gère toujours** `OperationCanceledException` pour les annulations

## 🎯 Principes Fondamentaux (OBLIGATOIRES)

**RESPECTER ces 7 règles absolues :**

1. ✅ **Async tout le long** : Si une méthode appelle du code async, elle DOIT être async
   ```csharp
   // ✅ BON - Async propagé
   public async Task<User> GetUserAsync(Guid id)
   {
       return await _repository.GetByIdAsync(id);
   }
   
   // ❌ MAUVAIS - Bloque le thread
   public User GetUser(Guid id)
   {
       return _repository.GetByIdAsync(id).Result;  // ❌ Deadlock potentiel
   }
   ```

2. ✅ **Suffixe Async OBLIGATOIRE** : Toute méthode async DOIT se terminer par `Async`
   ```csharp
   // ✅ BON
   public async Task<User> GetUserAsync(Guid id)
   public async Task SendEmailAsync(string to, string subject)
   public async Task<bool> ValidateAsync(User user)
   
   // ❌ MAUVAIS - Manque le suffixe
   public async Task<User> GetUser(Guid id)
   ```

3. ✅ **CancellationToken partout** : Toute méthode async publique DOIT accepter un `CancellationToken`
   ```csharp
   // ✅ BON
   public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
   {
       return await _repository.GetByIdAsync(id, cancellationToken);
   }
   
   // ❌ MAUVAIS - Pas de CancellationToken
   public async Task<User> GetUserAsync(Guid id)
   ```

4. ✅ **ConfigureAwait(false) UNIQUEMENT en bibliothèques** : Pas dans applications ASP.NET/Blazor
   ```csharp
   // ✅ BON - Application ASP.NET/Blazor
   public async Task<User> GetUserAsync(Guid id)
   {
       return await _repository.GetByIdAsync(id);  // Pas de ConfigureAwait
   }
   
   // ✅ BON - Bibliothèque réutilisable
   public async Task<string> ReadFileAsync(string path)
   {
       return await File.ReadAllTextAsync(path).ConfigureAwait(false);
   }
   ```

5. ✅ **JAMAIS .Result ou .Wait()** : Toujours utiliser `await`
   ```csharp
   // ❌ MAUVAIS - Risque de deadlock
   var user = GetUserAsync(id).Result;
   GetUserAsync(id).Wait();
   
   // ✅ BON
   var user = await GetUserAsync(id);
   ```

6. ✅ **ValueTask pour optimisation uniquement** : Utiliser `Task` par défaut
   ```csharp
   // ✅ BON - Cas général
   public async Task<User> GetUserAsync(Guid id)
   
   // ✅ BON - Optimisation si souvent synchrone
   public ValueTask<User> GetCachedUserAsync(Guid id)
   {
       if (_cache.TryGetValue(id, out var user))
           return new ValueTask<User>(user);  // Synchrone
       
       return new ValueTask<User>(LoadUserAsync(id));  // Asynchrone
   }
   ```

7. ✅ **Exceptions propagées automatiquement** : Ne pas wrapper dans try-catch sans raison
   ```csharp
   // ✅ BON - Exception propagée naturellement
   public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken)
   {
       return await _repository.GetByIdAsync(id, cancellationToken);
   }
   
   // ❌ MAUVAIS - Wrapper inutile
   public async Task<User> GetUserAsync(Guid id)
   {
       try
       {
           return await _repository.GetByIdAsync(id);
       }
       catch (Exception ex)
       {
           throw;  // Inutile, se propage automatiquement
       }
   }
   ```

## 📐 Task vs ValueTask

### Quand Utiliser Task (99% des cas)

```csharp
// ✅ BON - Cas standard avec Task<T>
public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
{
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    
    if (user is null)
        throw new UserNotFoundException(id);
    
    return user;
}

// ✅ BON - Task sans valeur de retour
public async Task SendNotificationAsync(User user, CancellationToken cancellationToken = default)
{
    await _emailService.SendAsync(user.Email, "Welcome!", cancellationToken);
    await _smsService.SendAsync(user.PhoneNumber, "Welcome!", cancellationToken);
}

// ✅ BON - Task.FromResult pour retour synchrone
public Task<int> GetCachedCountAsync()
{
    return Task.FromResult(_cachedCount);  // Pas besoin d'async/await
}
```

### Quand Utiliser ValueTask (cas avancés)

```csharp
// ✅ BON - ValueTask si souvent synchrone (cache)
public ValueTask<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
{
    // Cas 1 : Valeur en cache (synchrone)
    if (_cache.TryGetValue(id, out var cachedUser))
    {
        return new ValueTask<User>(cachedUser);
    }
    
    // Cas 2 : Chargement depuis DB (asynchrone)
    return new ValueTask<User>(LoadUserFromDatabaseAsync(id, cancellationToken));
}

private async Task<User> LoadUserFromDatabaseAsync(Guid id, CancellationToken cancellationToken)
{
    var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
    
    if (user is not null)
    {
        _cache[id] = user;
    }
    
    return user;
}

// ✅ BON - ValueTask pour interfaces haute performance
public interface IHighPerformanceRepository<T>
{
    ValueTask<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
```

**⚠️ RÈGLES CRITIQUES ValueTask :**
- Ne JAMAIS await un ValueTask plusieurs fois
- Ne JAMAIS stocker un ValueTask dans un champ
- Ne JAMAIS utiliser ValueTask après l'avoir await
- Si doute, utiliser Task

```csharp
// ❌ MAUVAIS - ValueTask utilisé plusieurs fois
var task = GetUserAsync(id);
var user1 = await task;  // ❌ Première utilisation OK
var user2 = await task;  // ❌ ERREUR - Réutilisation interdite

// ✅ BON - Convertir en Task si besoin de réutilisation
var task = GetUserAsync(id).AsTask();
var user1 = await task;
var user2 = await task;
```

## 🔄 Parallélisme et Concurrence

### Task.WhenAll - Exécution Parallèle

```csharp
// ✅ BON - Parallélisme avec Task.WhenAll
public async Task<UserDetails> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
{
    // Lancer toutes les requêtes en parallèle
    var userTask = _userRepository.GetByIdAsync(userId, cancellationToken);
    var ordersTask = _orderRepository.GetByUserIdAsync(userId, cancellationToken);
    var addressesTask = _addressRepository.GetByUserIdAsync(userId, cancellationToken);
    var preferencesTask = _preferencesRepository.GetByUserIdAsync(userId, cancellationToken);
    
    // Attendre que TOUTES soient terminées
    await Task.WhenAll(userTask, ordersTask, addressesTask, preferencesTask);
    
    // Récupérer les résultats
    return new UserDetails
    {
        User = await userTask,
        Orders = await ordersTask,
        Addresses = await addressesTask,
        Preferences = await preferencesTask
    };
}

// ❌ MAUVAIS - Séquentiel au lieu de parallèle
public async Task<UserDetails> GetUserDetailsAsync(Guid userId)
{
    var user = await _userRepository.GetByIdAsync(userId);          // 100ms
    var orders = await _orderRepository.GetByUserIdAsync(userId);   // 100ms
    var addresses = await _addressRepository.GetByUserIdAsync(userId); // 100ms
    var preferences = await _preferencesRepository.GetByUserIdAsync(userId); // 100ms
    // Total : 400ms au lieu de 100ms !
    
    return new UserDetails { User = user, Orders = orders, Addresses = addresses, Preferences = preferences };
}

// ✅ BON - Gestion des erreurs avec Task.WhenAll
public async Task<UserDetails> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
{
    var userTask = _userRepository.GetByIdAsync(userId, cancellationToken);
    var ordersTask = _orderRepository.GetByUserIdAsync(userId, cancellationToken);
    var addressesTask = _addressRepository.GetByUserIdAsync(userId, cancellationToken);
    
    try
    {
        await Task.WhenAll(userTask, ordersTask, addressesTask);
    }
    catch (Exception ex)
    {
        // Task.WhenAll lève la première exception
        // Pour récupérer TOUTES les exceptions :
        var exceptions = new[] { userTask, ordersTask, addressesTask }
            .Where(t => t.IsFaulted)
            .SelectMany(t => t.Exception?.InnerExceptions ?? Enumerable.Empty<Exception>())
            .ToList();
        
        _logger.LogError("Multiple errors loading user details: {Errors}", 
            string.Join(", ", exceptions.Select(e => e.Message)));
        
        throw;
    }
    
    return new UserDetails
    {
        User = userTask.Result,
        Orders = ordersTask.Result,
        Addresses = addressesTask.Result
    };
}
```

### Task.WhenAny - Course de Tâches

```csharp
// ✅ BON - Timeout avec Task.WhenAny
public async Task<User> GetUserWithTimeoutAsync(Guid id, TimeSpan timeout, CancellationToken cancellationToken = default)
{
    var userTask = _repository.GetByIdAsync(id, cancellationToken);
    var timeoutTask = Task.Delay(timeout, cancellationToken);
    
    var completedTask = await Task.WhenAny(userTask, timeoutTask);
    
    if (completedTask == timeoutTask)
    {
        throw new TimeoutException($"User retrieval timed out after {timeout.TotalSeconds}s");
    }
    
    return await userTask;
}

// ✅ BON - Fallback avec WhenAny
public async Task<Product> GetProductAsync(string productId, CancellationToken cancellationToken = default)
{
    var primaryTask = _primaryService.GetProductAsync(productId, cancellationToken);
    var fallbackTask = Task.Delay(TimeSpan.FromSeconds(2), cancellationToken)
        .ContinueWith(_ => _fallbackService.GetProductAsync(productId, cancellationToken), cancellationToken)
        .Unwrap();
    
    var completedTask = await Task.WhenAny(primaryTask, fallbackTask);
    
    try
    {
        return await completedTask;
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Primary service failed, trying fallback");
        
        // Essayer l'autre tâche
        var otherTask = completedTask == primaryTask ? fallbackTask : primaryTask;
        return await otherTask;
    }
}
```

### Parallel.ForEachAsync - Traitement Parallèle Contrôlé

```csharp
// ✅ BON - Parallel.ForEachAsync pour traiter collection en parallèle
public async Task ProcessUsersAsync(
    IEnumerable<User> users, 
    int maxDegreeOfParallelism = 10,
    CancellationToken cancellationToken = default)
{
    var options = new ParallelOptions
    {
        MaxDegreeOfParallelism = maxDegreeOfParallelism,
        CancellationToken = cancellationToken
    };
    
    await Parallel.ForEachAsync(users, options, async (user, ct) =>
    {
        await ProcessUserAsync(user, ct);
    });
}

// ❌ MAUVAIS - Tout séquentiel
public async Task ProcessUsersAsync(IEnumerable<User> users)
{
    foreach (var user in users)  // ❌ Un par un
    {
        await ProcessUserAsync(user);
    }
}

// ❌ MAUVAIS - Task.WhenAll avec trop de tâches simultanées
public async Task ProcessUsersAsync(IEnumerable<User> users)
{
    var tasks = users.Select(u => ProcessUserAsync(u));
    await Task.WhenAll(tasks);  // ❌ Peut créer 10 000 tâches simultanées !
}

// ✅ BON - SemaphoreSlim pour limiter concurrence
private readonly SemaphoreSlim _semaphore = new(10);  // Max 10 simultanées

public async Task ProcessUsersAsync(IEnumerable<User> users, CancellationToken cancellationToken = default)
{
    var tasks = users.Select(async user =>
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await ProcessUserAsync(user, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    });
    
    await Task.WhenAll(tasks);
}
```

### Channels - Producteur/Consommateur

```csharp
// ✅ BON - Pattern producteur/consommateur avec Channels
public async Task ProcessOrdersAsync(CancellationToken cancellationToken = default)
{
    var channel = Channel.CreateBounded<Order>(new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.Wait
    });
    
    // Producteur
    var producerTask = Task.Run(async () =>
    {
        await foreach (var order in _orderStream.ReadAllAsync(cancellationToken))
        {
            await channel.Writer.WriteAsync(order, cancellationToken);
        }
        
        channel.Writer.Complete();
    }, cancellationToken);
    
    // Consommateurs (plusieurs en parallèle)
    var consumerTasks = Enumerable.Range(0, 5).Select(_ => Task.Run(async () =>
    {
        await foreach (var order in channel.Reader.ReadAllAsync(cancellationToken))
        {
            await ProcessOrderAsync(order, cancellationToken);
        }
    }, cancellationToken));
    
    await Task.WhenAll(consumerTasks);
    await producerTask;
}
```

## 🛑 CancellationToken - Annulation Coopérative

### Utilisation Correcte de CancellationToken

```csharp
// ✅ BON - CancellationToken propagé partout
public async Task<List<User>> SearchUsersAsync(
    string query, 
    CancellationToken cancellationToken = default)
{
    // Vérification rapide au début
    cancellationToken.ThrowIfCancellationRequested();
    
    var users = await _context.Users
        .Where(u => u.Name.Contains(query))
        .ToListAsync(cancellationToken);  // Passe le token à EF Core
    
    var enrichedUsers = new List<User>();
    
    foreach (var user in users)
    {
        // Vérification dans les boucles longues
        cancellationToken.ThrowIfCancellationRequested();
        
        var details = await _detailsService.GetDetailsAsync(user.Id, cancellationToken);
        user.Details = details;
        enrichedUsers.Add(user);
    }
    
    return enrichedUsers;
}

// ❌ MAUVAIS - CancellationToken ignoré
public async Task<List<User>> SearchUsersAsync(string query)
{
    var users = await _context.Users
        .Where(u => u.Name.Contains(query))
        .ToListAsync();  // ❌ Pas de cancellation
    
    foreach (var user in users)
    {
        var details = await _detailsService.GetDetailsAsync(user.Id);  // ❌ Pas de cancellation
        user.Details = details;
    }
    
    return users;
}
```

### Créer et Gérer CancellationToken

```csharp
// ✅ BON - CancellationTokenSource avec timeout
public async Task<User> GetUserWithTimeoutAsync(Guid id)
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    
    try
    {
        return await _repository.GetByIdAsync(id, cts.Token);
    }
    catch (OperationCanceledException)
    {
        throw new TimeoutException("User retrieval timed out after 5 seconds");
    }
}

// ✅ BON - Combiner plusieurs CancellationToken
public async Task<User> GetUserAsync(
    Guid id, 
    CancellationToken requestToken,
    CancellationToken applicationToken)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(requestToken, applicationToken);
    
    return await _repository.GetByIdAsync(id, cts.Token);
}

// ✅ BON - Annulation manuelle
public class UserService
{
    private CancellationTokenSource? _backgroundTaskCts;
    
    public async Task StartBackgroundProcessingAsync()
    {
        _backgroundTaskCts = new CancellationTokenSource();
        
        await Task.Run(async () =>
        {
            while (!_backgroundTaskCts.Token.IsCancellationRequested)
            {
                await ProcessBatchAsync(_backgroundTaskCts.Token);
                await Task.Delay(TimeSpan.FromMinutes(5), _backgroundTaskCts.Token);
            }
        }, _backgroundTaskCts.Token);
    }
    
    public void StopBackgroundProcessing()
    {
        _backgroundTaskCts?.Cancel();
        _backgroundTaskCts?.Dispose();
        _backgroundTaskCts = null;
    }
}

// ✅ BON - Enregistrer callback d'annulation
public async Task DownloadFileAsync(string url, string path, CancellationToken cancellationToken = default)
{
    using var client = new HttpClient();
    
    // Enregistrer action de nettoyage si annulation
    using var registration = cancellationToken.Register(() =>
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        
        _logger.LogInformation("Download cancelled, temporary file deleted");
    });
    
    var response = await client.GetAsync(url, cancellationToken);
    response.EnsureSuccessStatusCode();
    
    await using var fileStream = File.Create(path);
    await response.Content.CopyToAsync(fileStream, cancellationToken);
}
```

## ⚡ Optimisations et Performance

### Éviter les Allocations Inutiles

```csharp
// ✅ BON - Pas d'async/await si simple return
public Task<User> GetByIdAsync(Guid id)
{
    return _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    // Pas besoin d'async/await, retourne directement la Task
}

// ❌ MAUVAIS - async/await inutile
public async Task<User> GetByIdAsync(Guid id)
{
    return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    // Crée une state machine inutilement
}

// ✅ EXCEPTION - async nécessaire pour try-catch
public async Task<User> GetByIdAsync(Guid id)
{
    try
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        // async/await nécessaire pour catcher l'exception ici
    }
    catch (DbException ex)
    {
        _logger.LogError(ex, "Database error");
        throw new DataAccessException("Failed to retrieve user", ex);
    }
}

// ✅ EXCEPTION - async nécessaire pour using
public async Task<User> GetByIdAsync(Guid id)
{
    using var connection = await _connectionFactory.CreateAsync();
    return await connection.QueryFirstAsync<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = id });
    // async/await nécessaire pour disposer correctement connection
}
```

### AsyncLocal pour Contexte Asynchrone

```csharp
// ✅ BON - AsyncLocal pour contexte qui suit les appels async
public class CorrelationContext
{
    private static readonly AsyncLocal<string?> _correlationId = new();
    
    public static string? CorrelationId
    {
        get => _correlationId.Value;
        set => _correlationId.Value = value;
    }
}

public class RequestLoggingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();
        
        CorrelationContext.CorrelationId = correlationId;
        
        try
        {
            await next(context);  // Le correlationId suit tout le flux async
        }
        finally
        {
            CorrelationContext.CorrelationId = null;
        }
    }
}

public class UserService
{
    public async Task CreateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        // Peut utiliser CorrelationId même dans méthodes appelées
        _logger.LogInformation("Creating user, CorrelationId: {CorrelationId}", 
            CorrelationContext.CorrelationId);
        
        await _repository.AddAsync(user, cancellationToken);
    }
}
```

### IAsyncEnumerable - Streaming Asynchrone

```csharp
// ✅ BON - IAsyncEnumerable pour grands datasets
public async IAsyncEnumerable<Product> GetProductsStreamAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    var page = 0;
    const int pageSize = 100;
    
    while (true)
    {
        var products = await _context.Products
            .OrderBy(p => p.Id)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        if (products.Count == 0)
            yield break;
        
        foreach (var product in products)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return product;
        }
        
        page++;
    }
}

// ✅ BON - Consommer IAsyncEnumerable
public async Task ProcessAllProductsAsync(CancellationToken cancellationToken = default)
{
    var processedCount = 0;
    
    await foreach (var product in GetProductsStreamAsync(cancellationToken))
    {
        await ProcessProductAsync(product, cancellationToken);
        processedCount++;
        
        if (processedCount % 100 == 0)
        {
            _logger.LogInformation("Processed {Count} products", processedCount);
        }
    }
}

// ❌ MAUVAIS - Charger tout en mémoire
public async Task ProcessAllProductsAsync()
{
    var allProducts = await _context.Products.ToListAsync();  // ❌ 100 000 produits en RAM !
    
    foreach (var product in allProducts)
    {
        await ProcessProductAsync(product);
    }
}
```

## 🚫 Anti-Patterns et Pièges

### Async Void - À ÉVITER

```csharp
// ❌ TRÈS MAUVAIS - async void (sauf event handlers)
public async void ProcessUserAsync(User user)  // ❌ Exceptions non catchables !
{
    await _repository.SaveAsync(user);
}

// ✅ BON - async Task
public async Task ProcessUserAsync(User user, CancellationToken cancellationToken = default)
{
    await _repository.SaveAsync(user, cancellationToken);
}

// ✅ EXCEPTION - Event handlers seulement
private async void OnButtonClicked(object sender, EventArgs e)
{
    try
    {
        await ProcessDataAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in button click handler");
        // OBLIGATOIRE de catcher ici, sinon crash application
    }
}
```

### Sync over Async - Deadlock Garanti

```csharp
// ❌ TRÈS MAUVAIS - .Result ou .Wait() = DEADLOCK
public User GetUser(Guid id)
{
    return GetUserAsync(id).Result;  // ❌ DEADLOCK dans ASP.NET/Blazor
}

public void ProcessUser(Guid id)
{
    GetUserAsync(id).Wait();  // ❌ DEADLOCK dans ASP.NET/Blazor
}

// ✅ BON - Async tout le long
public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
{
    return await _repository.GetByIdAsync(id, cancellationToken);
}

// ✅ BON - Si vraiment besoin de synchrone (rare), utiliser GetAwaiter().GetResult()
public User GetUserSync(Guid id)
{
    // Moins de risque de deadlock que .Result, mais toujours à éviter
    return GetUserAsync(id).GetAwaiter().GetResult();
}
```

### Fire and Forget - Gestion des Erreurs

```csharp
// ❌ MAUVAIS - Fire and forget sans gestion d'erreurs
public async Task CreateUserAsync(User user)
{
    await _repository.AddAsync(user);
    
    // ❌ Exception perdue si SendWelcomeEmailAsync échoue
    _ = SendWelcomeEmailAsync(user);
}

// ✅ BON - Background task avec gestion d'erreurs
public async Task CreateUserAsync(User user, CancellationToken cancellationToken = default)
{
    await _repository.AddAsync(user, cancellationToken);
    
    // Fire and forget avec try-catch
    _ = Task.Run(async () =>
    {
        try
        {
            await SendWelcomeEmailAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
            // Peut enregistrer pour retry
        }
    }, cancellationToken);
}

// ✅ MEILLEUR - Utiliser IHostedService ou BackgroundService
public class EmailBackgroundService : BackgroundService
{
    private readonly Channel<EmailRequest> _channel;
    
    public EmailBackgroundService()
    {
        _channel = Channel.CreateUnbounded<EmailRequest>();
    }
    
    public void QueueEmail(EmailRequest request)
    {
        _channel.Writer.TryWrite(request);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await SendEmailAsync(request, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email");
            }
        }
    }
}
```

### Capture de Contexte Inutile

```csharp
// ❌ MAUVAIS - Capture de variable modifiée dans boucle
public async Task ProcessItemsAsync(List<Item> items)
{
    foreach (var item in items)
    {
        // ❌ 'item' peut changer avant que la task s'exécute
        _ = Task.Run(async () => await ProcessAsync(item));
    }
}

// ✅ BON - Copie locale de la variable
public async Task ProcessItemsAsync(List<Item> items, CancellationToken cancellationToken = default)
{
    var tasks = new List<Task>();
    
    foreach (var item in items)
    {
        var localItem = item;  // Copie locale
        tasks.Add(Task.Run(async () => await ProcessAsync(localItem, cancellationToken), cancellationToken));
    }
    
    await Task.WhenAll(tasks);
}
```

## 🔒 Synchronisation et Thread-Safety

### Lock vs SemaphoreSlim

```csharp
// ❌ MAUVAIS - lock avec await (ne compile pas)
private readonly object _lock = new();

public async Task<User> GetOrCreateUserAsync(string email)
{
    lock (_lock)  // ❌ Ne peut pas avoir await dans lock
    {
        var user = await _repository.FindByEmailAsync(email);
        if (user is null)
        {
            user = new User { Email = email };
            await _repository.AddAsync(user);
        }
        return user;
    }
}

// ✅ BON - SemaphoreSlim pour async
private readonly SemaphoreSlim _semaphore = new(1, 1);

public async Task<User> GetOrCreateUserAsync(string email, CancellationToken cancellationToken = default)
{
    await _semaphore.WaitAsync(cancellationToken);
    try
    {
        var user = await _repository.FindByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            user = new User { Email = email };
            await _repository.AddAsync(user, cancellationToken);
        }
        return user;
    }
    finally
    {
        _semaphore.Release();
    }
}

// ✅ BON - AsyncLock pattern personnalisé
public class AsyncLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        return new Releaser(_semaphore);
    }
    
    private class Releaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        
        public Releaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }
        
        public void Dispose()
        {
            _semaphore.Release();
        }
    }
}

// Utilisation
private readonly AsyncLock _lock = new();

public async Task<User> GetOrCreateUserAsync(string email, CancellationToken cancellationToken = default)
{
    using (await _lock.LockAsync(cancellationToken))
    {
        var user = await _repository.FindByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            user = new User { Email = email };
            await _repository.AddAsync(user, cancellationToken);
        }
        return user;
    }
}
```

### Collections Thread-Safe

```csharp
// ✅ BON - ConcurrentDictionary pour cache thread-safe
private readonly ConcurrentDictionary<Guid, User> _userCache = new();

public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
{
    return await _userCache.GetOrAddAsync(id, async key =>
    {
        return await _repository.GetByIdAsync(key, cancellationToken);
    });
}

// Extension method pour GetOrAddAsync
public static class ConcurrentDictionaryExtensions
{
    public static async Task<TValue> GetOrAddAsync<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TKey, Task<TValue>> valueFactory)
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var existingValue))
        {
            return existingValue;
        }
        
        var newValue = await valueFactory(key);
        return dictionary.GetOrAdd(key, newValue);
    }
}

// ✅ BON - Channel pour queue thread-safe
private readonly Channel<Order> _orderQueue = Channel.CreateUnbounded<Order>();

public void EnqueueOrder(Order order)
{
    _orderQueue.Writer.TryWrite(order);  // Thread-safe
}

public async Task<Order> DequeueOrderAsync(CancellationToken cancellationToken = default)
{
    return await _orderQueue.Reader.ReadAsync(cancellationToken);
}
```

## 📊 Tests Unitaires Async

### Tester du Code Asynchrone

```csharp
// ✅ BON - Test async
[Fact]
public async Task GetUserAsync_ValidId_ReturnsUser()
{
    // Arrange
    var userId = Guid.NewGuid();
    var expectedUser = new User { Id = userId, Name = "John" };
    _mockRepository
        .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedUser);
    
    var service = new UserService(_mockRepository.Object);
    
    // Act
    var result = await service.GetUserAsync(userId);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(userId, result.Id);
    Assert.Equal("John", result.Name);
}

// ✅ BON - Tester avec CancellationToken
[Fact]
public async Task GetUserAsync_CancellationRequested_ThrowsOperationCanceledException()
{
    // Arrange
    var userId = Guid.NewGuid();
    var cts = new CancellationTokenSource();
    cts.Cancel();  // Annulation immédiate
    
    var service = new UserService(_mockRepository.Object);
    
    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(
        async () => await service.GetUserAsync(userId, cts.Token)
    );
}

// ✅ BON - Tester timeout
[Fact]
public async Task GetUserAsync_Timeout_ThrowsTimeoutException()
{
    // Arrange
    var userId = Guid.NewGuid();
    _mockRepository
        .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
        .Returns(async (Guid id, CancellationToken ct) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);  // Simule lenteur
            return new User { Id = id };
        });
    
    var service = new UserService(_mockRepository.Object);
    
    // Act & Assert
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
    await Assert.ThrowsAsync<OperationCanceledException>(
        async () => await service.GetUserAsync(userId, cts.Token)
    );
}

// ✅ BON - Tester Task.WhenAll
[Fact]
public async Task GetUserDetailsAsync_CallsAllRepositories()
{
    // Arrange
    var userId = Guid.NewGuid();
    var user = new User { Id = userId };
    var orders = new List<Order>();
    var addresses = new List<Address>();
    
    _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
    _mockOrderRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(orders);
    _mockAddressRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(addresses);
    
    var service = new UserService(_mockUserRepo.Object, _mockOrderRepo.Object, _mockAddressRepo.Object);
    
    // Act
    var result = await service.GetUserDetailsAsync(userId);
    
    // Assert
    Assert.NotNull(result);
    _mockUserRepo.Verify(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    _mockOrderRepo.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    _mockAddressRepo.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
}
```

## ✅ Checklist Code Asynchrone

Avant de commiter du code async, VÉRIFIER :

### Signatures et Conventions
- [ ] Toutes méthodes async ont suffixe `Async`
- [ ] Type de retour `Task` ou `Task<T>` (ou `ValueTask` si justifié)
- [ ] CancellationToken en dernier paramètre avec `= default`
- [ ] Pas de `async void` (sauf event handlers)

### Gestion des Erreurs
- [ ] CancellationToken propagé à toutes les opérations async
- [ ] `cancellationToken.ThrowIfCancellationRequested()` dans boucles longues
- [ ] Try-catch uniquement si traitement spécifique nécessaire
- [ ] Exceptions custom pour erreurs métier

### Performance
- [ ] Pas de `.Result` ou `.Wait()` (risque deadlock)
- [ ] Pas de `ConfigureAwait(false)` dans applications ASP.NET/Blazor
- [ ] `Task.WhenAll` pour paralléliser opérations indépendantes
- [ ] `IAsyncEnumerable` pour grands datasets streaming
- [ ] Pas d'async/await inutile (simple return Task)

### Synchronisation
- [ ] `SemaphoreSlim` au lieu de `lock` pour code async
- [ ] `ConcurrentDictionary` ou `Channel` pour collections thread-safe
- [ ] Pas de capture de variable modifiée dans closures async

### Tests
- [ ] Tests async avec `async Task`
- [ ] Tests de cancellation avec `CancellationTokenSource`
- [ ] Tests de timeout
- [ ] Vérification de tous les chemins d'exécution

## 📚 Ressources

### Documentation Officielle Microsoft
- [Async/Await Best Practices](https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [Task Asynchronous Programming Model (TAP)](https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
- [Async Return Types](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/async-return-types)
- [Cancellation in Managed Threads](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)

### Articles Recommandés
- **Async/Await Best Practices** - Stephen Cleary
- **There Is No Thread** - Stephen Cleary
- **ConfigureAwait FAQ** - Stephen Toub
- **Task.Run Etiquette and Proper Usage** - Stephen Toub
