# 14. Dependency Injection

Date: 2025-12-21

## Statut

Accepté

## Contexte

Sans injection de dépendances, le code crée ses propres dépendances :
- **Couplage fort** : Les classes sont liées à des implémentations concrètes
- **Tests impossibles** : Impossible de remplacer les dépendances par des mocks
- **Configuration rigide** : Changer d'implémentation nécessite de modifier le code
- **Violation de DIP** : Les modules de haut niveau dépendent des modules de bas niveau

Exemple problématique :

```csharp
// ❌ PROBLÈME : Couplage fort, création interne des dépendances
public class OrderService
{
    private readonly SqlServerOrderRepository _repository = new();
    private readonly SmtpEmailService _emailService = new();
    private readonly StripePaymentGateway _paymentGateway = new();
    
    public async Task ProcessOrder(Order order)
    {
        await _repository.SaveAsync(order);
        await _paymentGateway.ChargeAsync(order.Total);
        await _emailService.SendAsync(order.CustomerEmail, "Order confirmed");
    }
}

// Problèmes :
// - Comment tester sans vraie BDD, Stripe, serveur SMTP ?
// - Comment passer à PostgreSQL ou SendGrid ?
// - Comment ajouter du logging sans modifier cette classe ?
```

## Décision

**Utiliser l'Injection de Dépendances (DI) pour toutes les dépendances externes et les services.**

### 1. Principe fondamental

Les dépendances sont **injectées** (passées de l'extérieur), pas **créées** en interne.

```csharp
// ✅ DI : Dépendances injectées via le constructeur
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IEmailService _emailService;
    
    public OrderService(
        IOrderRepository repository,
        IPaymentGateway paymentGateway,
        IEmailService emailService)
    {
        _repository = repository;
        _paymentGateway = paymentGateway;
        _emailService = emailService;
    }
    
    public async Task ProcessOrderAsync(Order order, CancellationToken ct)
    {
        await _repository.SaveAsync(order, ct);
        await _paymentGateway.ChargeAsync(order.Total, ct);
        await _emailService.SendAsync(order.CustomerEmail, "Order confirmed", ct);
    }
}
```

### 2. Types d'injection

#### Constructor Injection (préféré)

```csharp
// ✅ PRÉFÉRÉ : Constructeur injection
public class TenantService
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<TenantService> _logger;
    
    public TenantService(
        ITenantRepository repository,
        ILogger<TenantService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

#### Method Injection (pour dépendances optionnelles/contextuelles)

```csharp
// OK pour dépendances contextuelles
public async Task ProcessAsync(
    Order order,
    IDiscountStrategy discountStrategy, // Injecté par méthode
    CancellationToken ct)
{
    var discount = discountStrategy.Calculate(order);
    // ...
}
```

#### Property Injection (à éviter sauf frameworks)

```csharp
// ⚠️ ÉVITER : Property injection (difficile à garantir l'initialisation)
public class SomeService
{
    public ILogger Logger { get; set; } // Peut être null !
}
```

### 3. Durées de vie (Lifetimes)

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // TRANSIENT : Nouvelle instance à chaque injection
    // Utiliser pour : Services légers, sans état
    services.AddTransient<IEmailService, SmtpEmailService>();
    
    // SCOPED : Une instance par requête HTTP
    // Utiliser pour : DbContext, services avec état par requête
    services.AddScoped<ITenantRepository, TenantRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped<LLMProxyDbContext>();
    
    // SINGLETON : Une seule instance pour toute l'application
    // Utiliser pour : Cache, configuration, HttpClient factories
    services.AddSingleton<ICacheService, RedisCacheService>();
    services.AddSingleton<IConfiguration>(Configuration);
}
```

### 4. Éviter le Service Locator (anti-pattern)

```csharp
// ❌ ANTI-PATTERN : Service Locator
public class OrderService
{
    private readonly IServiceProvider _serviceProvider;
    
    public OrderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void Process()
    {
        // Dépendances cachées, découvertes à l'exécution !
        var repo = _serviceProvider.GetService<IOrderRepository>();
        var email = _serviceProvider.GetService<IEmailService>();
    }
}

// ✅ CORRECT : Injection explicite
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IEmailService _emailService;
    
    public OrderService(
        IOrderRepository repository,
        IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }
}
```

### 5. Enregistrement des services

```csharp
// Program.cs ou Startup.cs - Composition Root
public void ConfigureServices(IServiceCollection services)
{
    // Infrastructure
    services.AddDbContext<LLMProxyDbContext>(options =>
        options.UseNpgsql(Configuration.GetConnectionString("Default")));
    
    // Repositories
    services.AddScoped<ITenantRepository, TenantRepository>();
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
    
    // Services
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped<ITokenCounterService, TokenCounterService>();
    
    // External services
    services.AddSingleton<ICacheService, RedisCacheService>();
    services.AddHttpClient<IOpenAIClient, OpenAIClient>();
    
    // MediatR
    services.AddMediatR(cfg => 
        cfg.RegisterServicesFromAssemblyContaining<CreateTenantCommand>());
}
```

### 6. Extensions pour organisation

```csharp
// Extensions/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<LLMProxyDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));
        
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }
    
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(
                configuration.GetConnectionString("Redis")));
        
        services.AddSingleton<ICacheService, RedisCacheService>();
        
        return services;
    }
}

// Program.cs - Utilisation propre
services.AddInfrastructure(Configuration);
services.AddRedisCache(Configuration);
```

### 7. Validation des dépendances au démarrage

```csharp
// Valider que toutes les dépendances sont résolues
public static class ServiceProviderValidation
{
    public static void ValidateServices(this IServiceProvider serviceProvider)
    {
        // Liste des services critiques à valider
        var criticalServices = new[]
        {
            typeof(ITenantRepository),
            typeof(IUserRepository),
            typeof(ICacheService),
            typeof(LLMProxyDbContext)
        };
        
        foreach (var serviceType in criticalServices)
        {
            var service = serviceProvider.GetService(serviceType);
            if (service is null)
            {
                throw new InvalidOperationException(
                    $"Critical service {serviceType.Name} is not registered");
            }
        }
    }
}

// Program.cs
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.Services.ValidateServices(); // Fail fast en dev
}
```

### 8. Tests avec DI

```csharp
public class OrderServiceTests
{
    [Fact]
    public async Task ProcessOrder_ShouldSaveAndNotify()
    {
        // Arrange - Injection de mocks
        var mockRepository = Substitute.For<IOrderRepository>();
        var mockPayment = Substitute.For<IPaymentGateway>();
        var mockEmail = Substitute.For<IEmailService>();
        
        var service = new OrderService(
            mockRepository,
            mockPayment,
            mockEmail);
        
        var order = new Order { Total = 100, CustomerEmail = "test@example.com" };
        
        // Act
        await service.ProcessOrderAsync(order, CancellationToken.None);
        
        // Assert
        await mockRepository.Received(1).SaveAsync(order, Arg.Any<CancellationToken>());
        await mockPayment.Received(1).ChargeAsync(100, Arg.Any<CancellationToken>());
        await mockEmail.Received(1).SendAsync(
            "test@example.com", 
            "Order confirmed", 
            Arg.Any<CancellationToken>());
    }
}
```

## Conséquences

### Positives

- **Testabilité** : Dépendances mockables, tests unitaires purs
- **Découplage** : Classes dépendent d'abstractions, pas d'implémentations
- **Flexibilité** : Changer d'implémentation = changer l'enregistrement DI
- **Transparence** : Dépendances visibles dans le constructeur
- **Respect DIP** : Inversion de dépendance naturelle

### Négatives

- **Configuration initiale** : Enregistrement de tous les services
  - *Mitigation* : Extensions methods, conventions, auto-registration
- **Constructors verbeux** : Beaucoup de paramètres si beaucoup de dépendances
  - *Mitigation* : Signe que la classe a trop de responsabilités (refactoring)
- **Indirection** : Navigation moins directe vers l'implémentation
  - *Mitigation* : IDE "Go to Implementation"

### Neutres

- DI est un standard de facto en .NET moderne

## Alternatives considérées

### Option A : Instanciation directe

- **Description** : Créer les dépendances avec `new` dans les classes
- **Avantages** : Simple, direct
- **Inconvénients** : Couplage fort, tests impossibles
- **Raison du rejet** : Rend le code non testable et rigide

### Option B : Service Locator

- **Description** : Utiliser un conteneur global pour récupérer les services
- **Avantages** : Flexibilité dynamique
- **Inconvénients** : Dépendances cachées, erreurs à l'exécution
- **Raison du rejet** : Anti-pattern reconnu

## Références

- [Dependency Injection in .NET - Mark Seemann](https://www.amazon.com/Dependency-Injection-NET-Mark-Seemann/dp/1935182501)
- [Microsoft DI Guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)
- [Inversion of Control Containers - Martin Fowler](https://martinfowler.com/articles/injection.html)
