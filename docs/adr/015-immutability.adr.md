# 15. Immutability (Immutabilité)

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les objets mutables créent des problèmes subtils et difficiles à déboguer :
- **Effets de bord** : Une méthode modifie un objet partagé, impactant d'autres parties du code
- **Concurrence dangereuse** : Accès simultanés à des objets mutables = race conditions
- **État incohérent** : Un objet peut être dans un état intermédiaire invalide
- **Debugging difficile** : "Qui a modifié cette valeur ?" devient un mystère
- **Tests non déterministes** : L'ordre d'exécution peut changer le résultat

Exemple de problème :

```csharp
// ❌ MUTABLE : Effets de bord inattendus
public class ShoppingCart
{
    public List<CartItem> Items { get; set; } = new();
    public decimal Total { get; set; }
}

public void ProcessCart(ShoppingCart cart)
{
    ApplyDiscounts(cart);
    CalculateShipping(cart);
    // Quelqu'un d'autre peut avoir modifié cart.Items pendant ce temps !
}

// Quelque part ailleurs...
cart.Items.Clear(); // Surprise ! Le panier est vidé
cart.Total = -100;  // Total négatif ? Possible !
```

## Décision

**Privilégier l'immutabilité : les objets ne doivent pas changer après leur création.**

### 1. Records pour les DTOs et Value Objects

```csharp
// ✅ IMMUTABLE : Record pour DTO (immutable par défaut)
public record TenantDto(
    Guid Id,
    string Name,
    string ContactEmail,
    TenantStatus Status,
    DateTime CreatedAt
);

// Modification = création d'une nouvelle instance (with expression)
var updated = original with { Status = TenantStatus.Active };

// ✅ IMMUTABLE : Value Object
public record Email
{
    public string Value { get; }
    
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty");
        
        if (!IsValidEmail(value))
            throw new ArgumentException($"Invalid email: {value}");
        
        Value = value.ToLowerInvariant();
    }
    
    // Après création, Value ne peut plus changer
}
```

### 2. Classes avec propriétés init-only

```csharp
// ✅ IMMUTABLE : Propriétés init-only
public class CreateTenantCommand
{
    public string Name { get; init; }
    public string ContactEmail { get; init; }
    public TenantPlan Plan { get; init; }
    
    // Peut être initialisé seulement à la création
    // var cmd = new CreateTenantCommand { Name = "Test", ... };
    // cmd.Name = "Other"; // ERREUR DE COMPILATION
}
```

### 3. Collections immutables

```csharp
// ❌ MUTABLE : Collection exposée
public class Order
{
    public List<OrderItem> Items { get; set; } = new();
}

// Externe peut modifier :
order.Items.Add(new OrderItem());
order.Items.Clear();

// ✅ IMMUTABLE : Collection encapsulée
public class Order
{
    private readonly List<OrderItem> _items = new();
    
    // Expose une vue en lecture seule
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    // Modification contrôlée via méthodes
    public Order AddItem(Product product, int quantity)
    {
        // Optionnel : retourner une nouvelle instance (full immutability)
        var newItems = _items.ToList();
        newItems.Add(new OrderItem(product, quantity));
        return new Order(newItems);
        
        // Ou : modifier en interne mais valider
        // _items.Add(new OrderItem(product, quantity));
        // return this;
    }
}

// ✅ IMMUTABLE : Utiliser les collections immutables
using System.Collections.Immutable;

public class Configuration
{
    public ImmutableList<string> AllowedOrigins { get; init; }
    public ImmutableDictionary<string, string> Settings { get; init; }
}
```

### 4. Entités avec état contrôlé

```csharp
// ✅ IMMUTABILITÉ CONTRÔLÉE pour entités Domain
public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public TenantStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    
    // Constructeur privé : création contrôlée
    private Tenant() { }
    
    // Factory method : garantit un état valide initial
    public static Tenant Create(string name, string contactEmail)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");
        
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Status = TenantStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    // Mutations via méthodes métier (état toujours valide)
    public void Activate()
    {
        if (Status == TenantStatus.Suspended)
            throw new InvalidOperationException("Cannot activate suspended tenant");
        
        Status = TenantStatus.Active;
        ModifiedAt = DateTime.UtcNow;
    }
    
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty");
        
        Name = newName;
        ModifiedAt = DateTime.UtcNow;
    }
}
```

### 5. Builders pour construction complexe

```csharp
// ✅ IMMUTABLE avec Builder pattern
public class LLMRequest
{
    public string Model { get; }
    public string Prompt { get; }
    public int MaxTokens { get; }
    public double Temperature { get; }
    public IReadOnlyList<string> StopSequences { get; }
    
    private LLMRequest(
        string model,
        string prompt,
        int maxTokens,
        double temperature,
        IEnumerable<string> stopSequences)
    {
        Model = model;
        Prompt = prompt;
        MaxTokens = maxTokens;
        Temperature = temperature;
        StopSequences = stopSequences.ToList().AsReadOnly();
    }
    
    public static Builder CreateBuilder() => new();
    
    public class Builder
    {
        private string _model = "gpt-4";
        private string _prompt = "";
        private int _maxTokens = 1000;
        private double _temperature = 0.7;
        private readonly List<string> _stopSequences = new();
        
        public Builder WithModel(string model)
        {
            _model = model;
            return this;
        }
        
        public Builder WithPrompt(string prompt)
        {
            _prompt = prompt;
            return this;
        }
        
        public Builder WithMaxTokens(int maxTokens)
        {
            _maxTokens = maxTokens;
            return this;
        }
        
        public Builder WithTemperature(double temperature)
        {
            _temperature = temperature;
            return this;
        }
        
        public Builder AddStopSequence(string sequence)
        {
            _stopSequences.Add(sequence);
            return this;
        }
        
        public LLMRequest Build()
        {
            if (string.IsNullOrEmpty(_prompt))
                throw new InvalidOperationException("Prompt is required");
            
            return new LLMRequest(
                _model,
                _prompt,
                _maxTokens,
                _temperature,
                _stopSequences);
        }
    }
}

// Utilisation
var request = LLMRequest.CreateBuilder()
    .WithModel("gpt-4-turbo")
    .WithPrompt("Explain quantum computing")
    .WithMaxTokens(2000)
    .WithTemperature(0.5)
    .AddStopSequence("END")
    .Build();
```

### 6. Règles d'immutabilité par couche

| Couche | Type | Immutabilité |
|--------|------|--------------|
| Application | Commands | ✅ Totale (records) |
| Application | Queries | ✅ Totale (records) |
| Application | DTOs | ✅ Totale (records) |
| Domain | Value Objects | ✅ Totale |
| Domain | Entities | ⚡ Contrôlée (mutations via méthodes) |
| Domain | Events | ✅ Totale (records) |
| Infrastructure | Configuration | ✅ Totale |

### 7. Concurrence sûre avec immutabilité

```csharp
// ✅ Thread-safe grâce à l'immutabilité
public class RateLimiter
{
    private ImmutableDictionary<string, TokenBucket> _buckets = 
        ImmutableDictionary<string, TokenBucket>.Empty;
    
    public bool TryConsume(string clientId)
    {
        var bucket = _buckets.GetValueOrDefault(clientId) 
            ?? new TokenBucket(100, DateTime.UtcNow);
        
        var (newBucket, allowed) = bucket.TryConsume();
        
        // Mise à jour atomique thread-safe
        ImmutableInterlocked.AddOrUpdate(
            ref _buckets,
            clientId,
            newBucket,
            (_, _) => newBucket);
        
        return allowed;
    }
}

public record TokenBucket(int Tokens, DateTime LastRefill)
{
    public (TokenBucket NewBucket, bool Allowed) TryConsume()
    {
        var now = DateTime.UtcNow;
        var refilled = RefillIfNeeded(now);
        
        if (refilled.Tokens > 0)
        {
            return (refilled with { Tokens = refilled.Tokens - 1 }, true);
        }
        
        return (refilled, false);
    }
    
    private TokenBucket RefillIfNeeded(DateTime now)
    {
        if ((now - LastRefill).TotalSeconds >= 1)
        {
            return new TokenBucket(100, now);
        }
        return this;
    }
}
```

## Conséquences

### Positives

- **Thread-safety** : Objets immutables sont naturellement thread-safe
- **Prévisibilité** : Pas d'effets de bord, comportement déterministe
- **Debugging facile** : L'état ne change pas mystérieusement
- **Historique gratuit** : Chaque modification crée une nouvelle version
- **Égalité simplifiée** : Records ont l'égalité par valeur automatique

### Négatives

- **Allocation mémoire** : Nouvelles instances à chaque modification
  - *Mitigation* : Le GC .NET est optimisé pour les petites allocations
- **Performance** : Copie de données pour les modifications
  - *Mitigation* : Utiliser les structures immuables optimisées (`Immutable*`)
- **Complexité apparente** : `with` et builders vs assignation directe
  - *Mitigation* : L'habitude vient vite, les bénéfices compensent

### Neutres

- Records C# 9+ rendent l'immutabilité naturelle et élégante

## Alternatives considérées

### Option A : Tout mutable

- **Description** : Permettre la modification libre de tous les objets
- **Avantages** : Code plus direct initialement
- **Inconvénients** : Bugs subtils, concurrence dangereuse
- **Raison du rejet** : Coût de maintenance et debugging trop élevé

### Option B : Immutabilité totale stricte

- **Description** : Même les entités sont totalement immutables
- **Avantages** : Cohérence maximale
- **Inconvénients** : Verbeux pour les entités avec beaucoup d'état
- **Raison du rejet** : Pragmatisme - entités avec mutations contrôlées

## Références

- [Immutability in C# - Microsoft](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
- [Immutable Collections - Microsoft](https://learn.microsoft.com/en-us/dotnet/api/system.collections.immutable)
- [Domain-Driven Design - Eric Evans (Value Objects)](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)
