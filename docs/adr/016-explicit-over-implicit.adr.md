# 16. Explicit over Implicit

Date: 2025-12-21

## Statut

Accepté

## Contexte

Le code implicite crée de la confusion et des bugs :
- **Magic strings/numbers** : Valeurs codées en dur sans signification évidente
- **Conversions implicites** : Changements de type automatiques non visibles
- **Comportements cachés** : Effets de bord dans des getters/setters
- **Conventions non documentées** : "Tout le monde sait que..."
- **Configuration par défaut** : Comportements activés sans déclaration explicite

Exemple de problèmes :

```csharp
// ❌ IMPLICITE : Qu'est-ce que ces valeurs signifient ?
if (user.Status == 2 && user.Type == "A") 
{
    ProcessPayment(order.Total * 0.85); // Pourquoi 0.85 ?
}

// ❌ IMPLICITE : Conversion silencieuse
double price = 19.99;
int roundedPrice = (int)price; // 19 - truncation silencieuse !

// ❌ IMPLICITE : Effet de bord caché dans un getter
public decimal Total
{
    get
    {
        _lastAccessed = DateTime.UtcNow; // Mutation cachée !
        return _items.Sum(i => i.Price);
    }
}
```

## Décision

**Privilégier l'explicite sur l'implicite : le code doit exprimer clairement son intention.**

### 1. Enums au lieu de magic numbers/strings

```csharp
// ❌ IMPLICITE : Que signifie 2 ?
if (user.Status == 2) { }
if (order.Type == "P") { }

// ✅ EXPLICITE : Enums nommés
public enum UserStatus
{
    Pending = 0,
    Active = 1,
    Suspended = 2,
    Deleted = 3
}

public enum OrderType
{
    Standard,
    Premium,
    Enterprise
}

if (user.Status == UserStatus.Suspended) { }
if (order.Type == OrderType.Premium) { }
```

### 2. Constantes nommées

```csharp
// ❌ IMPLICITE : Que représente 0.85 ?
var discountedPrice = price * 0.85;
if (retryCount < 3) { }
if (password.Length >= 8) { }

// ✅ EXPLICITE : Constantes nommées
public static class DiscountRates
{
    public const decimal VipDiscount = 0.15m;
    public const decimal SeasonalDiscount = 0.10m;
}

public static class RetryPolicy
{
    public const int MaxRetries = 3;
    public const int DelayMilliseconds = 1000;
}

public static class PasswordPolicy
{
    public const int MinimumLength = 8;
    public const int MaximumLength = 128;
}

var discountedPrice = price * (1 - DiscountRates.VipDiscount);
if (retryCount < RetryPolicy.MaxRetries) { }
if (password.Length >= PasswordPolicy.MinimumLength) { }
```

### 3. Types explicites pour la clarté

```csharp
// ❌ IMPLICITE : var cache les types, intentions floues
var x = GetResult();
var data = ProcessData(items);

// ✅ EXPLICITE quand le type n'est pas évident
TenantDto tenant = await GetTenantAsync(id);
IReadOnlyList<OrderSummary> orders = await GetOrdersAsync();
Result<Tenant, Error> result = await CreateTenantAsync(command);

// ✅ var OK quand le type est évident
var tenant = new Tenant();                    // Type évident
var orders = new List<Order>();               // Type évident
var count = items.Count();                    // int évident
var name = "John";                            // string évident
```

### 4. Paramètres nommés pour la lisibilité

```csharp
// ❌ IMPLICITE : Que représentent true, false, 5 ?
var user = CreateUser("john@example.com", true, false, 5);

// ✅ EXPLICITE : Paramètres nommés
var user = CreateUser(
    email: "john@example.com",
    isAdmin: true,
    requiresApproval: false,
    maxLoginAttempts: 5);

// ✅ EXPLICITE : Ou utiliser un objet de configuration
var user = CreateUser(new CreateUserOptions
{
    Email = "john@example.com",
    IsAdmin = true,
    RequiresApproval = false,
    MaxLoginAttempts = 5
});
```

### 5. Retours explicites (pas de null implicite)

```csharp
// ❌ IMPLICITE : null peut signifier "pas trouvé" ou "erreur"
public Tenant? GetTenant(Guid id)
{
    return _context.Tenants.Find(id); // null si pas trouvé
}

// Appelant ne sait pas interpréter null
var tenant = GetTenant(id);
if (tenant == null) // Pas trouvé ? Erreur ? Supprimé ?

// ✅ EXPLICITE : Result pattern
public async Task<Result<Tenant, TenantError>> GetTenantAsync(Guid id)
{
    var tenant = await _context.Tenants.FindAsync(id);
    
    if (tenant is null)
        return TenantError.NotFound(id);
    
    if (tenant.Status == TenantStatus.Deleted)
        return TenantError.Deleted(id);
    
    return tenant;
}

// Appelant doit gérer explicitement chaque cas
var result = await GetTenantAsync(id);
return result.Match(
    success: tenant => Ok(tenant),
    failure: error => error switch
    {
        TenantError.NotFound => NotFound(),
        TenantError.Deleted => Gone(),
        _ => BadRequest()
    });
```

### 6. Exceptions explicites

```csharp
// ❌ IMPLICITE : Exception générique
throw new Exception("Something went wrong");
throw new InvalidOperationException("Error");

// ✅ EXPLICITE : Exceptions typées et descriptives
public class TenantNotFoundException : Exception
{
    public Guid TenantId { get; }
    
    public TenantNotFoundException(Guid tenantId)
        : base($"Tenant with ID {tenantId} was not found")
    {
        TenantId = tenantId;
    }
}

public class TenantQuotaExceededException : Exception
{
    public Guid TenantId { get; }
    public string QuotaType { get; }
    public int Limit { get; }
    public int Current { get; }
    
    public TenantQuotaExceededException(
        Guid tenantId, 
        string quotaType, 
        int limit, 
        int current)
        : base($"Tenant {tenantId} exceeded {quotaType} quota: {current}/{limit}")
    {
        TenantId = tenantId;
        QuotaType = quotaType;
        Limit = limit;
        Current = current;
    }
}
```

### 7. Configuration explicite

```csharp
// ❌ IMPLICITE : Comportements par défaut cachés
services.AddDbContext<AppDbContext>();
services.AddAuthentication();

// ✅ EXPLICITE : Configuration visible
services.AddDbContext<LLMProxyDbContext>(options =>
{
    options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        });
    
    options.EnableSensitiveDataLogging(isDevelopment);
    options.EnableDetailedErrors(isDevelopment);
});

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
    };
});
```

### 8. Conversions explicites

```csharp
// ❌ IMPLICITE : Conversions silencieuses et potentiellement dangereuses
double price = 19.99;
int roundedPrice = (int)price; // 19 - truncation !

decimal amount = 100.50m;
int intAmount = (int)amount; // Perte de précision

// ✅ EXPLICITE : Intention claire
int roundedPrice = (int)Math.Round(price);
int truncatedPrice = (int)Math.Truncate(price);
int ceilingPrice = (int)Math.Ceiling(price);

// ✅ EXPLICITE : Méthodes de conversion nommées
public static class MoneyConverter
{
    public static int ToCents(decimal amount) => (int)(amount * 100);
    public static decimal FromCents(int cents) => cents / 100m;
}

var cents = MoneyConverter.ToCents(19.99m); // 1999
```

### 9. Nommage explicite

```csharp
// ❌ IMPLICITE : Noms vagues
var data = GetData();
var result = Process(items);
var flag = Check(user);
void Handle(object obj);

// ✅ EXPLICITE : Noms descriptifs
var activeTenants = GetActiveTenants();
var validatedOrders = ValidateAndEnrichOrders(pendingOrders);
var hasPermission = CheckUserHasAdminPermission(user);
void HandleTenantCreatedEvent(TenantCreatedEvent domainEvent);
```

### 10. Comportements explicites (pas d'effets de bord cachés)

```csharp
// ❌ IMPLICITE : Getter avec effet de bord
public decimal Total
{
    get
    {
        _accessCount++; // Effet de bord !
        _lastAccessed = DateTime.UtcNow; // Effet de bord !
        return CalculateTotal();
    }
}

// ✅ EXPLICITE : Méthode pour l'action, propriété pour la lecture
public decimal Total => CalculateTotal();

public decimal GetTotalAndRecordAccess()
{
    _accessCount++;
    _lastAccessed = DateTime.UtcNow;
    return Total;
}

// Ou séparer complètement
public void RecordAccess()
{
    _accessCount++;
    _lastAccessed = DateTime.UtcNow;
}
```

## Conséquences

### Positives

- **Lisibilité** : Le code est auto-documenté
- **Maintenabilité** : Moins de "connaissances tribales" nécessaires
- **Débogage** : Les intentions sont visibles
- **Onboarding** : Nouveaux développeurs comprennent le code rapidement
- **Refactoring sûr** : Pas de surprises cachées

### Négatives

- **Verbosité** : Code plus long
  - *Mitigation* : La clarté compense la longueur
- **Ceremony** : Plus de "boilerplate"
  - *Mitigation* : IDE génère beaucoup de code automatiquement

### Neutres

- L'explicite demande plus d'effort initial mais économise du temps à long terme

## Alternatives considérées

### Option A : Conventions implicites

- **Description** : S'appuyer sur des conventions non écrites
- **Avantages** : Moins de code
- **Inconvénients** : Connaissances tribales, bugs subtils
- **Raison du rejet** : Les conventions s'oublient, le code explicite reste

### Option B : Documentation externe

- **Description** : Documenter les comportements implicites dans des docs séparées
- **Avantages** : Code court
- **Inconvénients** : Documentation désynchronisée, rarement lue
- **Raison du rejet** : Le code est la meilleure documentation

## Références

- [The Zen of Python - PEP 20](https://peps.python.org/pep-0020/) - "Explicit is better than implicit"
- [Clean Code - Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
- [Code Complete - Steve McConnell](https://www.amazon.com/Code-Complete-Practical-Handbook-Construction/dp/0735619670)
