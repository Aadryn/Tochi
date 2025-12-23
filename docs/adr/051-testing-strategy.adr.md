# 51. Testing Strategy - Pyramide de Tests

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM critique nécessite une couverture de tests complète. Sans stratégie :
- Tests aléatoires, pas de structure
- Lenteur des pipelines CI/CD
- Bugs en production malgré les tests
- Coût de maintenance élevé

```csharp
// ❌ SANS STRATÉGIE : Tests mal structurés
public class AllTests
{
    [Fact]
    public async Task TestEverything()
    {
        // Un test géant qui fait tout
        // - Démarre la DB
        // - Démarre l'API
        // - Fait des appels HTTP
        // - Vérifie la DB
        // = Lent, fragile, impossible à maintenir
    }
}
```

## Décision

**Adopter la pyramide de tests avec une répartition claire : Unit > Integration > E2E.**

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    PYRAMIDE DE TESTS                            │
│                                                                 │
│                         ▲                                       │
│                        /E\          E2E Tests (5%)              │
│                       /2E \         - Scénarios critiques       │
│                      /─────\        - Smoke tests               │
│                     /       \       - 5-10 tests                │
│                    /─────────\                                  │
│                   / Intégration\    Integration Tests (20%)     │
│                  /    Tests     \   - APIs, DB, Cache           │
│                 /────────────────\  - 50-100 tests              │
│                /                  \                             │
│               /    Unit Tests      \  Unit Tests (75%)          │
│              /        (Fast)        \ - Logique métier          │
│             /────────────────────────\- 500+ tests              │
│            /                          \                         │
│           ▼──────────────────────────▼                          │
│                                                                 │
│  Temps d'exécution :  ▼ Rapide (ms) ─────────► Lent (min)       │
│  Coût maintenance  :  ▼ Faible ──────────────► Élevé            │
│  Confiance         :  ▼ Isolée ──────────────► Bout en bout     │
└─────────────────────────────────────────────────────────────────┘
```

### 1. Tests Unitaires (75%)

```csharp
/// <summary>
/// Tests unitaires du domaine - Rapides et isolés.
/// </summary>
public sealed class TenantTests
{
    private readonly Faker _faker = new();
    
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var name = _faker.Company.CompanyName();
        var plan = TenantPlan.Professional;
        
        // Act
        var result = Tenant.Create(name, plan);
        
        // Assert
        result.Should().BeSuccess();
        result.Value.Name.Should().Be(name);
        result.Value.Plan.Should().Be(plan);
        result.Value.Status.Should().Be(TenantStatus.Active);
        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TenantCreatedEvent>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldFail(string? invalidName)
    {
        // Act
        var result = Tenant.Create(invalidName!, TenantPlan.Free);
        
        // Assert
        result.Should().BeFailure()
            .Which.Error.Code.Should().Be("TENANT_NAME_REQUIRED");
    }
    
    [Fact]
    public void Suspend_WhenActive_ShouldTransitionToSuspended()
    {
        // Arrange
        var tenant = CreateActiveTenant();
        
        // Act
        var result = tenant.Suspend("Non-payment");
        
        // Assert
        result.Should().BeSuccess();
        tenant.Status.Should().Be(TenantStatus.Suspended);
        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TenantSuspendedEvent>();
    }
    
    [Fact]
    public void Suspend_WhenAlreadySuspended_ShouldFail()
    {
        // Arrange
        var tenant = CreateActiveTenant();
        tenant.Suspend("First suspension");
        tenant.ClearDomainEvents();
        
        // Act
        var result = tenant.Suspend("Second suspension");
        
        // Assert
        result.Should().BeFailure()
            .Which.Error.Code.Should().Be("TENANT_ALREADY_SUSPENDED");
    }
    
    private Tenant CreateActiveTenant()
    {
        var result = Tenant.Create(_faker.Company.CompanyName(), TenantPlan.Professional);
        result.Value.ClearDomainEvents();
        return result.Value;
    }
}

/// <summary>
/// Tests unitaires des services applicatifs avec mocks.
/// </summary>
public sealed class CreateApiKeyCommandHandlerTests
{
    private readonly IApiKeyRepository _repository = Substitute.For<IApiKeyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ISecretService _secretService = Substitute.For<ISecretService>();
    private readonly CreateApiKeyCommandHandler _handler;
    
    public CreateApiKeyCommandHandlerTests()
    {
        _handler = new CreateApiKeyCommandHandler(
            _repository,
            _unitOfWork,
            _secretService);
    }
    
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateApiKey()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateApiKeyCommand(tenantId, "My API Key", ApiKeyPermissions.Read);
        
        _secretService.GenerateApiKeyAsync()
            .Returns(new ApiKeySecret("sk-test-123", "hash-123"));
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().BeSuccess();
        result.Value.Name.Should().Be("My API Key");
        result.Value.KeyPrefix.Should().Be("sk-te..."); // Masqué
        
        await _repository.Received(1).AddAsync(Arg.Any<ApiKey>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_WhenRepositoryFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateApiKeyCommand(Guid.NewGuid(), "Key", ApiKeyPermissions.Read);
        _repository.When(r => r.AddAsync(Arg.Any<ApiKey>()))
            .Throw(new InvalidOperationException("DB error"));
        
        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
```

### 2. Tests d'Intégration (20%)

```csharp
/// <summary>
/// Tests d'intégration avec vraie base de données.
/// </summary>
[Collection("Database")]
public sealed class TenantRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private readonly LlmProxyDbContext _dbContext;
    private readonly TenantRepository _repository;
    
    public TenantRepositoryIntegrationTests()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        
        var options = new DbContextOptionsBuilder<LlmProxyDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        
        _dbContext = new LlmProxyDbContext(options);
        await _dbContext.Database.MigrateAsync();
        
        _repository = new TenantRepository(_dbContext);
    }
    
    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }
    
    [Fact]
    public async Task AddAsync_ShouldPersistTenant()
    {
        // Arrange
        var tenant = Tenant.Create("Test Company", TenantPlan.Professional).Value;
        
        // Act
        await _repository.AddAsync(tenant);
        await _dbContext.SaveChangesAsync();
        
        // Assert
        var retrieved = await _repository.GetByIdAsync(tenant.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Company");
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var tenant = Tenant.Create("Original Name", TenantPlan.Free).Value;
        await _repository.AddAsync(tenant);
        await _dbContext.SaveChangesAsync();
        
        // Act
        tenant.UpdatePlan(TenantPlan.Enterprise);
        _repository.Update(tenant);
        await _dbContext.SaveChangesAsync();
        
        // Assert - Nouvelle instance pour vérifier persistence
        var newContext = CreateNewContext();
        var newRepo = new TenantRepository(newContext);
        var retrieved = await newRepo.GetByIdAsync(tenant.Id);
        retrieved!.Plan.Should().Be(TenantPlan.Enterprise);
    }
}

/// <summary>
/// Tests d'intégration API avec WebApplicationFactory.
/// </summary>
public sealed class ChatApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    
    public ChatApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remplacer le vrai LLM par un mock
                services.RemoveAll<ILlmProvider>();
                services.AddSingleton<ILlmProvider, MockLlmProvider>();
                
                // Utiliser une DB de test
                services.RemoveAll<DbContextOptions<LlmProxyDbContext>>();
                services.AddDbContext<LlmProxyDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        });
        
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", "test-api-key");
    }
    
    [Fact]
    public async Task CreateChatCompletion_WithValidRequest_ShouldReturn200()
    {
        // Arrange
        var request = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "user", content = "Hello!" }
            }
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/v1/chat/completions", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>();
        content.Should().NotBeNull();
        content!.Choices.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task CreateChatCompletion_WithoutApiKey_ShouldReturn401()
    {
        // Arrange
        var clientWithoutKey = _factory.CreateClient();
        var request = new { model = "gpt-4", messages = new[] { new { role = "user", content = "Hi" } } };
        
        // Act
        var response = await clientWithoutKey.PostAsJsonAsync("/v1/chat/completions", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task CreateChatCompletion_WithInvalidModel_ShouldReturn400()
    {
        // Arrange
        var request = new
        {
            model = "invalid-model",
            messages = new[] { new { role = "user", content = "Hello!" } }
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/v1/chat/completions", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Error.Code.Should().Be("INVALID_MODEL");
    }
}
```

### 3. Tests End-to-End (5%)

```csharp
/// <summary>
/// Tests E2E avec infrastructure réelle.
/// </summary>
[Collection("E2E")]
public sealed class CriticalPathE2ETests : IAsyncLifetime
{
    private readonly IContainer _apiContainer;
    private readonly IContainer _postgresContainer;
    private readonly IContainer _redisContainer;
    private HttpClient _client = null!;
    
    public async Task InitializeAsync()
    {
        // Démarrer l'infrastructure complète avec Docker Compose
        var network = new NetworkBuilder().Build();
        await network.CreateAsync();
        
        _postgresContainer = new PostgreSqlBuilder()
            .WithNetwork(network)
            .WithNetworkAliases("postgres")
            .Build();
        
        _redisContainer = new RedisBuilder()
            .WithNetwork(network)
            .WithNetworkAliases("redis")
            .Build();
        
        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _redisContainer.StartAsync());
        
        _apiContainer = new ContainerBuilder()
            .WithImage("llm-proxy:test")
            .WithNetwork(network)
            .WithEnvironment("ConnectionStrings__Database", 
                "Host=postgres;Database=test;Username=postgres;Password=test")
            .WithEnvironment("ConnectionStrings__Redis", "redis:6379")
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/health")))
            .Build();
        
        await _apiContainer.StartAsync();
        
        _client = new HttpClient
        {
            BaseAddress = new Uri($"http://localhost:{_apiContainer.GetMappedPublicPort(8080)}")
        };
    }
    
    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _apiContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
    
    [Fact]
    [Trait("Category", "E2E")]
    public async Task FullUserJourney_CreateTenant_CreateApiKey_MakeRequest()
    {
        // 1. Créer un tenant (admin)
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer admin-token");
        
        var createTenantResponse = await _client.PostAsJsonAsync("/admin/tenants", new
        {
            name = "E2E Test Company",
            plan = "professional"
        });
        createTenantResponse.EnsureSuccessStatusCode();
        
        var tenant = await createTenantResponse.Content.ReadFromJsonAsync<TenantDto>();
        
        // 2. Créer une clé API
        var createKeyResponse = await _client.PostAsJsonAsync(
            $"/admin/tenants/{tenant!.Id}/api-keys",
            new { name = "E2E Key", permissions = "read,write" });
        createKeyResponse.EnsureSuccessStatusCode();
        
        var apiKey = await createKeyResponse.Content.ReadFromJsonAsync<ApiKeyDto>();
        
        // 3. Utiliser la clé API pour faire une requête chat
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add("X-API-Key", apiKey!.Key);
        
        var chatResponse = await _client.PostAsJsonAsync("/v1/chat/completions", new
        {
            model = "gpt-4",
            messages = new[] { new { role = "user", content = "Hello!" } }
        });
        chatResponse.EnsureSuccessStatusCode();
        
        // 4. Vérifier les statistiques d'usage
        _client.DefaultRequestHeaders.Remove("X-API-Key");
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer admin-token");
        
        var usageResponse = await _client.GetAsync($"/admin/tenants/{tenant.Id}/usage");
        usageResponse.EnsureSuccessStatusCode();
        
        var usage = await usageResponse.Content.ReadFromJsonAsync<UsageDto>();
        usage!.TotalRequests.Should().BeGreaterThan(0);
    }
    
    [Fact]
    [Trait("Category", "Smoke")]
    public async Task SmokeTest_HealthCheck_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/health");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### 4. Tests de Performance

```csharp
/// <summary>
/// Tests de performance avec NBomber.
/// </summary>
public sealed class PerformanceTests
{
    [Fact]
    [Trait("Category", "Performance")]
    public async Task ChatCompletion_ShouldHandle100RPS()
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        httpClient.DefaultRequestHeaders.Add("X-API-Key", "perf-test-key");
        
        var scenario = Scenario.Create("chat_completion", async context =>
        {
            var request = new
            {
                model = "gpt-4",
                messages = new[] { new { role = "user", content = "Hello!" } }
            };
            
            var response = await httpClient.PostAsJsonAsync("/v1/chat/completions", request);
            
            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(
                rate: 100,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromMinutes(1)));
        
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
        
        // Assertions
        var scenarioStats = stats.ScenarioStats[0];
        scenarioStats.Ok.Request.RPS.Should().BeGreaterThan(90);
        scenarioStats.Ok.Latency.Percent99.Should().BeLessThan(500);
        scenarioStats.Fail.Request.Count.Should().Be(0);
    }
}
```

### 5. Organisation et Configuration

```csharp
/// <summary>
/// Configuration de test partagée.
/// </summary>
public sealed class TestConfiguration
{
    public static IConfiguration Create()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .AddEnvironmentVariables()
            .Build();
    }
}

/// <summary>
/// Fixtures partagées pour tests d'intégration.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    public PostgreSqlContainer Postgres { get; private set; } = null!;
    public string ConnectionString => Postgres.GetConnectionString();
    
    public async Task InitializeAsync()
    {
        Postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_db")
            .Build();
        
        await Postgres.StartAsync();
    }
    
    public async Task DisposeAsync()
    {
        await Postgres.DisposeAsync();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}

/// <summary>
/// Extensions pour tests avec Bogus.
/// </summary>
public static class TestDataExtensions
{
    private static readonly Faker Faker = new();
    
    public static Tenant CreateRandomTenant(TenantPlan? plan = null)
    {
        return Tenant.Create(
            Faker.Company.CompanyName(),
            plan ?? Faker.PickRandom<TenantPlan>()
        ).Value;
    }
    
    public static ChatCompletionRequest CreateRandomChatRequest()
    {
        return new ChatCompletionRequest
        {
            Model = Faker.PickRandom("gpt-4", "gpt-3.5-turbo", "claude-3"),
            Messages = new[]
            {
                new ChatMessage
                {
                    Role = ChatRole.User,
                    Content = Faker.Lorem.Sentence()
                }
            },
            Temperature = Faker.Random.Double(0, 2),
            MaxTokens = Faker.Random.Int(100, 4000)
        };
    }
}
```

## Conséquences

### Positives

- **Feedback rapide** : Tests unitaires en < 1 min
- **Confiance** : Couverture à tous les niveaux
- **Maintenance** : Tests isolés faciles à corriger
- **Documentation** : Tests = spécifications vivantes

### Négatives

- **Discipline** : Respect de la pyramide requis
  - *Mitigation* : Reviews et métriques
- **Setup** : Infrastructure pour tests intégration
  - *Mitigation* : Testcontainers

### Neutres

- Pratique standard de l'industrie
- Compatible tous frameworks de test

## Alternatives considérées

### Option A : Tests manuels

- **Description** : QA manuelle uniquement
- **Avantages** : Pas de code de test
- **Inconvénients** : Lent, non reproductible
- **Raison du rejet** : Non viable à l'échelle

### Option B : Tout en E2E

- **Description** : Tests E2E uniquement
- **Avantages** : Confiance maximale
- **Inconvénients** : Lent, fragile, coûteux
- **Raison du rejet** : Pipeline CI/CD de 2h

## Références

- [Test Pyramid - Martin Fowler](https://martinfowler.com/bliki/TestPyramid.html)
- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore/cmdline)
- [Testcontainers for .NET](https://testcontainers.com/guides/getting-started-with-testcontainers-for-dotnet/)
