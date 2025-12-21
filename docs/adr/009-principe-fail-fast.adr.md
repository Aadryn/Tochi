# 9. Principe Fail Fast

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les erreurs dans un système peuvent se manifester de différentes manières :
- **Erreurs silencieuses** : Le système continue avec des données corrompues
- **Erreurs tardives** : L'erreur est détectée loin de sa source
- **Erreurs masquées** : Des try-catch génériques avalent les exceptions
- **État incohérent** : Le système fonctionne partiellement avec des données invalides

Ces comportements mènent à :
- Des bugs difficiles à diagnostiquer (cause éloignée de l'effet)
- Des données corrompues en production
- Des heures de débogage perdues
- Une perte de confiance dans le système

## Décision

**Appliquer le principe Fail Fast : détecter et signaler les erreurs le plus tôt possible.**

### 1. Validation à l'entrée

```csharp
// ❌ FAIL SLOW : Validation tardive, erreur loin de la source
public class OrderService
{
    public void ProcessOrder(Order order)
    {
        // ... 50 lignes de code ...
        var total = order.Items.Sum(i => i.Price * i.Quantity);
        // NullReferenceException ici si Items est null
        // Mais d'où vient le null ? Mystère...
    }
}

// ✅ FAIL FAST : Validation immédiate à l'entrée
public class OrderService
{
    public void ProcessOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(order.Items);
        
        if (order.Items.Count == 0)
            throw new ArgumentException("Order must have at least one item", nameof(order));
        
        // Le code suivant peut faire confiance aux données
        var total = order.Items.Sum(i => i.Price * i.Quantity);
    }
}
```

### 2. Guard Clauses (Clauses de garde)

```csharp
// ✅ FAIL FAST : Guards en début de méthode
public async Task<Tenant> GetTenantAsync(Guid tenantId, CancellationToken ct)
{
    // Guards - échec immédiat si préconditions non respectées
    if (tenantId == Guid.Empty)
        throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
    
    var tenant = await _repository.FindByIdAsync(tenantId, ct);
    
    if (tenant is null)
        throw new TenantNotFoundException(tenantId);
    
    if (tenant.Status == TenantStatus.Deleted)
        throw new TenantDeletedException(tenantId);
    
    return tenant;
}
```

### 3. Constructeurs qui valident

```csharp
// ✅ FAIL FAST : Entité toujours valide après construction
public class Email
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    
    public string Value { get; }
    
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));
        
        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException($"Invalid email format: {value}", nameof(value));
        
        Value = value.ToLowerInvariant();
    }
}

// Utilisation : impossible de créer un Email invalide
var email = new Email("invalid"); // Exception immédiate
```

### 4. Pas de catch générique

```csharp
// ❌ FAIL SLOW : Exception avalée, erreur masquée
public User? GetUser(Guid id)
{
    try
    {
        return _repository.FindById(id);
    }
    catch (Exception)
    {
        return null; // L'erreur est perdue !
    }
}

// ✅ FAIL FAST : Laisser remonter ou gérer spécifiquement
public User GetUser(Guid id)
{
    try
    {
        return _repository.FindById(id) 
            ?? throw new UserNotFoundException(id);
    }
    catch (SqlException ex) when (ex.Number == 1205) // Deadlock
    {
        _logger.LogWarning(ex, "Deadlock detected, will retry");
        throw new RetryableException("Database deadlock", ex);
    }
    // Autres exceptions : remontent naturellement
}
```

### 5. Assertions en développement

```csharp
public class PriceCalculator
{
    public decimal CalculateDiscount(decimal price, decimal discountPercent)
    {
        // Assertions pour attraper les bugs de logique en dev
        Debug.Assert(price >= 0, "Price should never be negative at this point");
        Debug.Assert(discountPercent >= 0 && discountPercent <= 100, 
            "Discount should be between 0 and 100");
        
        var discount = price * (discountPercent / 100);
        
        Debug.Assert(discount <= price, "Discount cannot exceed price");
        
        return discount;
    }
}
```

### 6. Configuration au démarrage

```csharp
// ✅ FAIL FAST : Valider la configuration au démarrage
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("Default");
        
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException(
                "Database connection string 'Default' is not configured");
        
        var openAiKey = Configuration["OpenAI:ApiKey"];
        
        if (string.IsNullOrEmpty(openAiKey))
            throw new InvalidOperationException(
                "OpenAI API key is not configured");
        
        // L'application ne démarre pas si mal configurée
    }
}
```

## Conséquences

### Positives

- **Débogage facilité** : L'erreur est signalée à sa source
- **Données fiables** : Impossible de propager des données invalides
- **Feedback rapide** : Les problèmes sont détectés immédiatement
- **Code robuste** : Les invariants sont garantis
- **Confiance accrue** : Si le code s'exécute, les données sont valides

### Négatives

- **Plus d'exceptions** : Le système échoue plus souvent (mais de manière contrôlée)
  - *Mitigation* : Mieux vaut échouer explicitement que corrompre silencieusement
- **Code de validation** : Plus de lignes de code pour les guards
  - *Mitigation* : Utiliser des helpers et des Value Objects

### Neutres

- Nécessite une gestion d'erreurs appropriée dans les couches supérieures

## Alternatives considérées

### Option A : Valeurs par défaut

- **Description** : Remplacer les valeurs invalides par des défauts
- **Avantages** : Le système ne plante jamais
- **Inconvénients** : Masque les bugs, données potentiellement incorrectes
- **Raison du rejet** : Crée une fausse impression de fonctionnement

### Option B : Logging sans exception

- **Description** : Logger l'erreur et continuer
- **Avantages** : Pas d'interruption du flux
- **Inconvénients** : Les logs sont ignorés, l'erreur persiste
- **Raison du rejet** : Ne résout pas le problème, le diffère

## Références

- [Fail Fast - Jim Shore](https://www.martinfowler.com/ieeeSoftware/failFast.pdf)
- [Guard Clauses - Refactoring Guru](https://refactoring.guru/replace-nested-conditional-with-guard-clauses)
- [Defensive Programming - Wikipedia](https://en.wikipedia.org/wiki/Defensive_programming)
