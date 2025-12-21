# 11. Composition over Inheritance

Date: 2025-12-21

## Statut

Accepté

## Contexte

L'héritage de classes est souvent surutilisé, créant des problèmes :
- **Hiérarchies rigides** : Difficile de changer la structure une fois établie
- **Couplage fort** : La classe fille dépend intimement de la classe parente
- **Problème du diamant** : Ambiguïté avec l'héritage multiple (interdit en C#)
- **Fragile Base Class** : Modifier la classe parente casse les classes filles
- **Explosion combinatoire** : Besoin de nombreuses sous-classes pour chaque combinaison

Exemple typique de problème :

```csharp
// ❌ HÉRITAGE PROBLÉMATIQUE : Hiérarchie rigide
public class Bird
{
    public virtual void Fly() { /* ... */ }
    public virtual void Eat() { /* ... */ }
}

public class Penguin : Bird
{
    public override void Fly() 
    {
        throw new NotSupportedException("Penguins can't fly!"); // Violation LSP
    }
}

public class Duck : Bird { /* OK */ }

// Et si on veut un oiseau robot qui vole mais ne mange pas ?
// Impossible sans casser la hiérarchie
```

## Décision

**Privilégier la composition à l'héritage : construire des objets complexes en assemblant des composants plutôt qu'en héritant.**

### 1. Composition avec interfaces

```csharp
// ✅ COMPOSITION : Comportements modulaires via interfaces

public interface IFlyable
{
    void Fly();
}

public interface ISwimmable
{
    void Swim();
}

public interface IFeedable
{
    void Eat();
}

// Chaque classe implémente uniquement ce qu'elle sait faire
public class Duck : IFlyable, ISwimmable, IFeedable
{
    public void Fly() { /* ... */ }
    public void Swim() { /* ... */ }
    public void Eat() { /* ... */ }
}

public class Penguin : ISwimmable, IFeedable
{
    public void Swim() { /* ... */ }
    public void Eat() { /* ... */ }
}

public class RobotBird : IFlyable
{
    public void Fly() { /* ... */ }
    // Pas besoin de Eat() - c'est un robot !
}
```

### 2. Composition avec injection de stratégies

```csharp
// ❌ HÉRITAGE : Explosion de sous-classes
public abstract class NotificationService
{
    public abstract void Send(string message);
}

public class EmailNotificationService : NotificationService { }
public class SmsNotificationService : NotificationService { }
public class PushNotificationService : NotificationService { }
public class EmailAndSmsNotificationService : NotificationService { } // Combo !
public class AllChannelsNotificationService : NotificationService { } // Tous !

// ✅ COMPOSITION : Stratégies injectables
public interface INotificationChannel
{
    Task SendAsync(string recipient, string message, CancellationToken ct);
}

public class EmailChannel : INotificationChannel { /* ... */ }
public class SmsChannel : INotificationChannel { /* ... */ }
public class PushChannel : INotificationChannel { /* ... */ }

public class NotificationService
{
    private readonly IEnumerable<INotificationChannel> _channels;
    
    public NotificationService(IEnumerable<INotificationChannel> channels)
    {
        _channels = channels; // Composition : on assemble les canaux voulus
    }
    
    public async Task NotifyAsync(string recipient, string message, CancellationToken ct)
    {
        foreach (var channel in _channels)
        {
            await channel.SendAsync(recipient, message, ct);
        }
    }
}

// Configuration flexible
services.AddScoped<INotificationChannel, EmailChannel>();
services.AddScoped<INotificationChannel, SmsChannel>();
// Ajouter/retirer des canaux = changer la config, pas le code
```

### 3. Decorator Pattern (Composition dynamique)

```csharp
// ✅ COMPOSITION : Décorateurs pour ajouter des comportements

public interface ILLMProvider
{
    Task<string> CompleteAsync(string prompt, CancellationToken ct);
}

// Implémentation de base
public class OpenAIProvider : ILLMProvider
{
    public async Task<string> CompleteAsync(string prompt, CancellationToken ct)
    {
        // Appel API OpenAI
        return await CallOpenAIAsync(prompt, ct);
    }
}

// Décorateur : ajoute du cache
public class CachedLLMProvider : ILLMProvider
{
    private readonly ILLMProvider _inner;
    private readonly ICache _cache;
    
    public CachedLLMProvider(ILLMProvider inner, ICache cache)
    {
        _inner = inner;
        _cache = cache;
    }
    
    public async Task<string> CompleteAsync(string prompt, CancellationToken ct)
    {
        var cacheKey = ComputeHash(prompt);
        
        if (_cache.TryGet(cacheKey, out string cached))
            return cached;
        
        var result = await _inner.CompleteAsync(prompt, ct);
        _cache.Set(cacheKey, result);
        return result;
    }
}

// Décorateur : ajoute du retry
public class RetryingLLMProvider : ILLMProvider
{
    private readonly ILLMProvider _inner;
    
    public async Task<string> CompleteAsync(string prompt, CancellationToken ct)
    {
        return await Policy
            .Handle<HttpRequestException>()
            .RetryAsync(3)
            .ExecuteAsync(() => _inner.CompleteAsync(prompt, ct));
    }
}

// Décorateur : ajoute du logging
public class LoggingLLMProvider : ILLMProvider
{
    private readonly ILLMProvider _inner;
    private readonly ILogger _logger;
    
    public async Task<string> CompleteAsync(string prompt, CancellationToken ct)
    {
        _logger.LogInformation("Calling LLM with prompt length: {Length}", prompt.Length);
        var result = await _inner.CompleteAsync(prompt, ct);
        _logger.LogInformation("LLM response length: {Length}", result.Length);
        return result;
    }
}

// Composition à l'enregistrement DI
services.AddScoped<ILLMProvider>(sp =>
{
    var openAI = new OpenAIProvider(/* ... */);
    var cached = new CachedLLMProvider(openAI, sp.GetRequiredService<ICache>());
    var retrying = new RetryingLLMProvider(cached);
    var logging = new LoggingLLMProvider(retrying, sp.GetRequiredService<ILogger>());
    return logging;
});
// Chaîne : Logging → Retry → Cache → OpenAI
```

### 4. Composition avec délégation

```csharp
// ❌ HÉRITAGE : Pour réutiliser du code
public class AuditableRepository<T> : Repository<T>
{
    public override async Task SaveAsync(T entity, CancellationToken ct)
    {
        SetAuditFields(entity);
        await base.SaveAsync(entity, ct);
    }
}

// ✅ COMPOSITION : Délégation explicite
public class TenantRepository : ITenantRepository
{
    private readonly DbContext _context;
    private readonly IAuditService _auditService;
    
    public async Task SaveAsync(Tenant tenant, CancellationToken ct)
    {
        _auditService.SetAuditFields(tenant); // Délégation
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(ct);
    }
}
```

### 5. Quand utiliser l'héritage ?

L'héritage reste approprié pour :

```csharp
// ✅ HÉRITAGE APPROPRIÉ : Vraie relation "est-un" stable

// 1. Classes de base abstraites avec comportement commun minimal
public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
}

// 2. Hiérarchies stables et peu profondes (max 2-3 niveaux)
public abstract class DomainException : Exception
{
    public string Code { get; }
    protected DomainException(string code, string message) : base(message)
    {
        Code = code;
    }
}

public class TenantNotFoundException : DomainException
{
    public TenantNotFoundException(Guid id) 
        : base("TENANT_NOT_FOUND", $"Tenant {id} not found") { }
}

// 3. Framework impose l'héritage (ex: ControllerBase, DbContext)
public class TenantsController : ControllerBase { }
```

## Conséquences

### Positives

- **Flexibilité** : Comportements combinables à volonté
- **Testabilité** : Composants mockables individuellement
- **Évolutivité** : Ajouter des comportements sans modifier l'existant
- **Découplage** : Moins de dépendances entre classes
- **Respect de LSP** : Pas de sous-classes qui violent le contrat parent

### Négatives

- **Plus de classes** : Chaque comportement = une classe
  - *Mitigation* : Organisation claire, nommage explicite
- **Indirection** : Le flux traverse plusieurs objets
  - *Mitigation* : Débogueur et bonne documentation
- **Configuration DI** : Plus de câblage à faire
  - *Mitigation* : Extensions methods pour simplifier l'enregistrement

### Neutres

- La composition demande plus de réflexion initiale mais paie sur le long terme

## Alternatives considérées

### Option A : Héritage systématique

- **Description** : Utiliser l'héritage pour toute réutilisation de code
- **Avantages** : Familier, moins de code initial
- **Inconvénients** : Rigidité, couplage fort, violations LSP
- **Raison du rejet** : Crée des hiérarchies fragiles et difficiles à maintenir

### Option B : Mixins / Traits

- **Description** : Utiliser des mécanismes de mixin (non natifs en C#)
- **Avantages** : Réutilisation horizontale
- **Inconvénients** : Pas supporté nativement en C#, complexité
- **Raison du rejet** : Préférer les mécanismes idiomatiques du langage

## Références

- [Design Patterns - Gang of Four](https://www.amazon.com/Design-Patterns-Elements-Reusable-Object-Oriented/dp/0201633612)
- [Effective Java - Joshua Bloch (Item 18)](https://www.amazon.com/Effective-Java-Joshua-Bloch/dp/0134685997)
- [Composition over Inheritance - Wikipedia](https://en.wikipedia.org/wiki/Composition_over_inheritance)
