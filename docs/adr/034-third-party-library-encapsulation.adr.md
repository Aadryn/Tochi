# 34. Encapsulation des Bibliothèques Tierces

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les dépendances directes aux bibliothèques tierces créent des problèmes majeurs :
- **Couplage fort** : Le code métier dépend directement de l'implémentation
- **Migration difficile** : Changer de bibliothèque impacte tout le codebase
- **Tests compliqués** : Difficile de mocker les appels directs
- **Breaking changes** : Une mise à jour peut casser l'application

```csharp
// ❌ COUPLAGE DIRECT : La bibliothèque tierce est partout
public class TenantService
{
    public async Task<Tenant> GetTenantAsync(Guid id)
    {
        // Dépendance directe à Dapper
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Tenant>(
            "SELECT * FROM tenants WHERE id = @Id",
            new { Id = id });
    }
}

public class ReportService
{
    public byte[] GeneratePdf(Report report)
    {
        // Dépendance directe à iTextSharp
        var document = new iTextSharp.text.Document();
        var writer = PdfWriter.GetInstance(document, stream);
        // ... 200 lignes de code iTextSharp
    }
}

// Si on veut changer Dapper pour EF Core ou iTextSharp pour QuestPDF
// → Modifier TOUS les fichiers qui utilisent ces libs
```

## Décision

**Encapsuler TOUTES les bibliothèques tierces derrière des abstractions internes pour faciliter la maintenance, les tests et les migrations futures.**

### 1. Principe d'encapsulation (Anti-Corruption Layer)

```csharp
/// <summary>
/// Architecture d'encapsulation :
/// 
/// ┌─────────────────────────────────────────────────────────────┐
/// │                    CODE MÉTIER                               │
/// │  (Services, Handlers, Domain)                                │
/// │                                                              │
/// │  Ne connaît QUE les abstractions internes                    │
/// └─────────────────────────┬────────────────────────────────────┘
///                           │
///                           ▼
/// ┌─────────────────────────────────────────────────────────────┐
/// │              ABSTRACTIONS INTERNES                           │
/// │  (Interfaces dans Domain ou Application)                     │
/// │                                                              │
/// │  ICacheService, IPdfGenerator, IEmailSender, etc.            │
/// └─────────────────────────┬────────────────────────────────────┘
///                           │
///                           ▼
/// ┌─────────────────────────────────────────────────────────────┐
/// │              ADAPTERS (Infrastructure)                       │
/// │                                                              │
/// │  RedisCacheService, QuestPdfGenerator, SendGridEmailSender   │
/// │                                                              │
/// │  SEUL ENDROIT où la bibliothèque tierce est utilisée         │
/// └─────────────────────────────────────────────────────────────┘
/// </summary>
```

### 2. Exemple : Encapsulation du cache (Redis)

```csharp
// ═══════════════════════════════════════════════════════════════
// COUCHE DOMAIN/APPLICATION : Abstraction interne
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Abstraction du service de cache.
/// Aucune référence à Redis, Memcached, ou autre implémentation.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Récupère une valeur du cache.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) 
        where T : class;
    
    /// <summary>
    /// Stocke une valeur dans le cache.
    /// </summary>
    Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan? expiration = null,
        CancellationToken ct = default) where T : class;
    
    /// <summary>
    /// Supprime une valeur du cache.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken ct = default);
    
    /// <summary>
    /// Vérifie si une clé existe.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}

/// <summary>
/// Options de cache (indépendantes de l'implémentation).
/// </summary>
public sealed record CacheOptions
{
    public TimeSpan DefaultExpiration { get; init; } = TimeSpan.FromMinutes(5);
    public string KeyPrefix { get; init; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════
// COUCHE INFRASTRUCTURE : Adapter Redis
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Implémentation Redis du cache.
/// SEUL fichier qui référence StackExchange.Redis.
/// </summary>
internal sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly CacheOptions _options;
    private readonly ILogger<RedisCacheService> _logger;
    
    public RedisCacheService(
        IConnectionMultiplexer redis,
        IOptions<CacheOptions> options,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _options = options.Value;
        _logger = logger;
    }
    
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct) 
        where T : class
    {
        var db = _redis.GetDatabase();
        var fullKey = BuildKey(key);
        
        var value = await db.StringGetAsync(fullKey);
        
        if (value.IsNullOrEmpty)
        {
            _logger.LogDebug("Cache miss for key {Key}", fullKey);
            return null;
        }
        
        _logger.LogDebug("Cache hit for key {Key}", fullKey);
        return JsonSerializer.Deserialize<T>(value!);
    }
    
    public async Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan? expiration,
        CancellationToken ct) where T : class
    {
        var db = _redis.GetDatabase();
        var fullKey = BuildKey(key);
        var json = JsonSerializer.Serialize(value);
        var ttl = expiration ?? _options.DefaultExpiration;
        
        await db.StringSetAsync(fullKey, json, ttl);
        
        _logger.LogDebug(
            "Cached {Key} for {Ttl}s", 
            fullKey, 
            ttl.TotalSeconds);
    }
    
    public async Task RemoveAsync(string key, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(BuildKey(key));
    }
    
    public async Task<bool> ExistsAsync(string key, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(BuildKey(key));
    }
    
    private string BuildKey(string key) => 
        string.IsNullOrEmpty(_options.KeyPrefix) 
            ? key 
            : $"{_options.KeyPrefix}:{key}";
}

// ═══════════════════════════════════════════════════════════════
// MIGRATION FACILE : Changer pour Memcached
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Nouvelle implémentation Memcached.
/// Le code métier ne change pas du tout !
/// </summary>
internal sealed class MemcachedCacheService : ICacheService
{
    private readonly IMemcachedClient _memcached;
    
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct) 
        where T : class
    {
        var result = await _memcached.GetAsync<T>(key);
        return result.HasValue ? result.Value : null;
    }
    
    // ... autres méthodes
}

// Changement dans DI uniquement :
// services.AddSingleton<ICacheService, RedisCacheService>();
// devient :
// services.AddSingleton<ICacheService, MemcachedCacheService>();
```

### 3. Exemple : Encapsulation génération PDF

```csharp
// ═══════════════════════════════════════════════════════════════
// ABSTRACTION
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Abstraction pour la génération de documents PDF.
/// </summary>
public interface IPdfGenerator
{
    /// <summary>
    /// Génère un PDF à partir d'un modèle de rapport.
    /// </summary>
    Task<byte[]> GenerateReportAsync(
        ReportModel report, 
        CancellationToken ct = default);
    
    /// <summary>
    /// Génère un PDF à partir d'HTML.
    /// </summary>
    Task<byte[]> GenerateFromHtmlAsync(
        string html, 
        PdfOptions? options = null,
        CancellationToken ct = default);
}

/// <summary>
/// Options de génération (indépendantes de l'implémentation).
/// </summary>
public sealed record PdfOptions
{
    public PageSize PageSize { get; init; } = PageSize.A4;
    public PageOrientation Orientation { get; init; } = PageOrientation.Portrait;
    public Margins Margins { get; init; } = Margins.Normal;
}

public enum PageSize { A4, Letter, Legal }
public enum PageOrientation { Portrait, Landscape }
public sealed record Margins(int Top, int Right, int Bottom, int Left)
{
    public static Margins Normal => new(20, 20, 20, 20);
    public static Margins Narrow => new(10, 10, 10, 10);
}

/// <summary>
/// Modèle de rapport (Domain).
/// </summary>
public sealed record ReportModel
{
    public required string Title { get; init; }
    public required DateOnly Period { get; init; }
    public required IReadOnlyList<ReportSection> Sections { get; init; }
}

// ═══════════════════════════════════════════════════════════════
// ADAPTER QUESTPDF
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Implémentation avec QuestPDF.
/// SEUL fichier qui référence QuestPDF.
/// </summary>
internal sealed class QuestPdfGenerator : IPdfGenerator
{
    public Task<byte[]> GenerateReportAsync(
        ReportModel report, 
        CancellationToken ct)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                
                page.Header()
                    .Text(report.Title)
                    .FontSize(20)
                    .Bold();
                
                page.Content()
                    .Column(column =>
                    {
                        foreach (var section in report.Sections)
                        {
                            column.Item().Text(section.Title).FontSize(14).Bold();
                            column.Item().Text(section.Content);
                            column.Item().PaddingVertical(10);
                        }
                    });
                
                page.Footer()
                    .AlignCenter()
                    .Text(x => x.CurrentPageNumber());
            });
        });
        
        var bytes = document.GeneratePdf();
        return Task.FromResult(bytes);
    }
    
    public Task<byte[]> GenerateFromHtmlAsync(
        string html, 
        PdfOptions? options,
        CancellationToken ct)
    {
        // QuestPDF ne supporte pas HTML nativement
        // On pourrait utiliser une autre lib ou lever une exception
        throw new NotSupportedException(
            "HTML to PDF not supported by QuestPDF adapter");
    }
}

// ═══════════════════════════════════════════════════════════════
// MIGRATION VERS UNE AUTRE LIB (ex: Playwright PDF)
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Implémentation avec Playwright (supporte HTML).
/// </summary>
internal sealed class PlaywrightPdfGenerator : IPdfGenerator
{
    private readonly IBrowser _browser;
    
    public async Task<byte[]> GenerateFromHtmlAsync(
        string html, 
        PdfOptions? options,
        CancellationToken ct)
    {
        var page = await _browser.NewPageAsync();
        await page.SetContentAsync(html);
        
        var pdfOptions = new PagePdfOptions
        {
            Format = options?.PageSize.ToString() ?? "A4",
            Landscape = options?.Orientation == PageOrientation.Landscape
        };
        
        return await page.PdfAsync(pdfOptions);
    }
    
    // ... GenerateReportAsync avec template HTML
}
```

### 4. Exemple : Encapsulation HTTP Client

```csharp
// ═══════════════════════════════════════════════════════════════
// ABSTRACTION
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Abstraction pour les appels HTTP aux providers LLM.
/// </summary>
public interface ILlmHttpClient
{
    /// <summary>
    /// Envoie une requête de complétion.
    /// </summary>
    Task<LlmResponse> SendCompletionAsync(
        LlmCompletionRequest request,
        CancellationToken ct = default);
    
    /// <summary>
    /// Envoie une requête de streaming.
    /// </summary>
    IAsyncEnumerable<LlmStreamChunk> StreamCompletionAsync(
        LlmCompletionRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// Requête de complétion (modèle interne).
/// </summary>
public sealed record LlmCompletionRequest
{
    public required string Model { get; init; }
    public required IReadOnlyList<LlmMessage> Messages { get; init; }
    public decimal? Temperature { get; init; }
    public int? MaxTokens { get; init; }
}

// ═══════════════════════════════════════════════════════════════
// ADAPTER OPENAI
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Implémentation pour OpenAI.
/// SEUL fichier qui connaît le format API OpenAI.
/// </summary>
internal sealed class OpenAiHttpClient : ILlmHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiHttpClient> _logger;
    
    public async Task<LlmResponse> SendCompletionAsync(
        LlmCompletionRequest request,
        CancellationToken ct)
    {
        // Transformation vers le format OpenAI
        var openAiRequest = new
        {
            model = request.Model,
            messages = request.Messages.Select(m => new
            {
                role = m.Role.ToString().ToLower(),
                content = m.Content
            }),
            temperature = request.Temperature,
            max_tokens = request.MaxTokens
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            "/v1/chat/completions",
            openAiRequest,
            ct);
        
        response.EnsureSuccessStatusCode();
        
        var openAiResponse = await response.Content
            .ReadFromJsonAsync<OpenAiCompletionResponse>(ct);
        
        // Transformation vers le format interne
        return new LlmResponse
        {
            Content = openAiResponse!.Choices[0].Message.Content,
            Model = openAiResponse.Model,
            InputTokens = openAiResponse.Usage.PromptTokens,
            OutputTokens = openAiResponse.Usage.CompletionTokens
        };
    }
    
    // Classes internes pour le format OpenAI (pas exposées)
    private sealed record OpenAiCompletionResponse(
        string Model,
        OpenAiChoice[] Choices,
        OpenAiUsage Usage);
    
    private sealed record OpenAiChoice(OpenAiMessage Message);
    private sealed record OpenAiMessage(string Content);
    private sealed record OpenAiUsage(int PromptTokens, int CompletionTokens);
}

// ═══════════════════════════════════════════════════════════════
// ADAPTER ANTHROPIC (même interface, format différent)
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Implémentation pour Anthropic.
/// </summary>
internal sealed class AnthropicHttpClient : ILlmHttpClient
{
    public async Task<LlmResponse> SendCompletionAsync(
        LlmCompletionRequest request,
        CancellationToken ct)
    {
        // Format Anthropic différent
        var anthropicRequest = new
        {
            model = request.Model,
            max_tokens = request.MaxTokens ?? 4096,
            messages = request.Messages.Select(m => new
            {
                role = m.Role == MessageRole.User ? "user" : "assistant",
                content = m.Content
            })
        };
        
        // ... appel et transformation
    }
}
```

### 5. Registrations avec Factory Pattern

```csharp
/// <summary>
/// Factory pour créer le bon adapter selon la configuration.
/// </summary>
public sealed class LlmHttpClientFactory
{
    private readonly IServiceProvider _services;
    
    public ILlmHttpClient CreateClient(string providerName)
    {
        return providerName.ToLower() switch
        {
            "openai" => _services.GetRequiredService<OpenAiHttpClient>(),
            "anthropic" => _services.GetRequiredService<AnthropicHttpClient>(),
            "azure" => _services.GetRequiredService<AzureOpenAiHttpClient>(),
            _ => throw new NotSupportedException($"Provider {providerName} not supported")
        };
    }
}

/// <summary>
/// Extension pour l'enregistrement DI.
/// </summary>
public static class ThirdPartyEncapsulationExtensions
{
    public static IServiceCollection AddEncapsulatedServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Cache - Un seul changement ici pour migrer
        var cacheProvider = configuration["Cache:Provider"];
        services.AddSingleton<ICacheService>(cacheProvider switch
        {
            "Redis" => sp => ActivatorUtilities.CreateInstance<RedisCacheService>(sp),
            "Memcached" => sp => ActivatorUtilities.CreateInstance<MemcachedCacheService>(sp),
            "InMemory" => sp => ActivatorUtilities.CreateInstance<InMemoryCacheService>(sp),
            _ => throw new InvalidOperationException($"Unknown cache provider: {cacheProvider}")
        });
        
        // PDF - Pareil
        var pdfProvider = configuration["Pdf:Provider"];
        services.AddSingleton<IPdfGenerator>(pdfProvider switch
        {
            "QuestPdf" => sp => ActivatorUtilities.CreateInstance<QuestPdfGenerator>(sp),
            "Playwright" => sp => ActivatorUtilities.CreateInstance<PlaywrightPdfGenerator>(sp),
            _ => throw new InvalidOperationException($"Unknown PDF provider: {pdfProvider}")
        });
        
        // LLM Clients
        services.AddHttpClient<OpenAiHttpClient>();
        services.AddHttpClient<AnthropicHttpClient>();
        services.AddSingleton<LlmHttpClientFactory>();
        
        return services;
    }
}
```

### 6. Tests simplifiés avec abstractions

```csharp
/// <summary>
/// Les tests utilisent des mocks des abstractions, pas des libs tierces.
/// </summary>
public sealed class TenantServiceTests
{
    [Fact]
    public async Task GetTenant_ShouldUseCache_WhenAvailable()
    {
        // Arrange - Mock de l'abstraction, pas de Redis
        var cacheService = Substitute.For<ICacheService>();
        var expectedTenant = new Tenant { Id = Guid.NewGuid(), Name = "Test" };
        
        cacheService
            .GetAsync<Tenant>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedTenant);
        
        var service = new TenantService(cacheService, _repository);
        
        // Act
        var result = await service.GetTenantAsync(expectedTenant.Id);
        
        // Assert
        result.Should().Be(expectedTenant);
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
    }
}
```

### 7. Checklist d'encapsulation

```csharp
/// <summary>
/// Règles pour encapsuler une nouvelle bibliothèque tierce :
/// 
/// ✅ CHECKLIST :
/// 
/// 1. CRÉER une interface dans Domain ou Application
///    - Nommer selon le BESOIN métier (IPdfGenerator, pas IQuestPdf)
///    - Définir les méthodes avec des types INTERNES
///    - Aucune référence à la bibliothèque tierce
/// 
/// 2. DÉFINIR des DTOs internes
///    - PdfOptions, CacheOptions, etc.
///    - Pas de dépendance aux types de la lib
/// 
/// 3. CRÉER l'adapter dans Infrastructure
///    - Classe internal (pas exposed)
///    - Référencer la lib UNIQUEMENT dans ce fichier
///    - Transformer entre formats interne ↔ externe
/// 
/// 4. ENREGISTRER via DI
///    - Permettre le changement d'implémentation par config
///    - Factory si plusieurs implémentations runtime
/// 
/// 5. DOCUMENTER
///    - Raison du choix de la bibliothèque
///    - Alternatives considérées
///    - Guide de migration
/// 
/// ❌ ANTI-PATTERNS :
/// 
/// - Exposer des types de la lib dans les interfaces publiques
/// - Utiliser la lib directement dans le code métier
/// - Référencer la lib dans plusieurs projets
/// - Créer des "passthrough" sans transformation
/// </summary>
```

## Conséquences

### Positives

- **Migration facile** : Changer de lib = 1 fichier adapter + DI
- **Tests simples** : Mock des abstractions internes
- **Couplage faible** : Code métier indépendant des libs
- **Évolution** : Changement de version lib isolé
- **Multi-implémentation** : Plusieurs providers en parallèle

### Négatives

- **Indirection** : Une couche supplémentaire
  - *Mitigation* : Overhead négligeable, maintenabilité prioritaire
- **Effort initial** : Créer les abstractions prend du temps
  - *Mitigation* : Investissement rentabilisé à la première migration
- **Features manquantes** : L'abstraction peut ne pas exposer toutes les features
  - *Mitigation* : Enrichir l'abstraction au besoin

### Neutres

- Pattern Anti-Corruption Layer du DDD
- Standard dans les architectures propres

## Alternatives considérées

### Option A : Dépendances directes

- **Description** : Utiliser les libs partout directement
- **Avantages** : Accès à toutes les features, pas d'abstraction
- **Inconvénients** : Migration = réécriture massive
- **Raison du rejet** : Coût de migration prohibitif

### Option B : Abstractions génériques existantes

- **Description** : Utiliser IDistributedCache, etc.
- **Avantages** : Standard Microsoft
- **Inconvénients** : Pas toujours adapté au besoin métier
- **Raison du rejet** : Abstractions trop génériques

## Références

- [Anti-Corruption Layer - Martin Fowler](https://martinfowler.com/bliki/AntiCorruptionLayer.html)
- [Ports and Adapters](https://alistair.cockburn.us/hexagonal-architecture/)
- [Clean Architecture - Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
