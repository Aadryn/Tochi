# 114. Implement Provider-Specific HTTP Clients

**Statut:** À faire  
**Priorité:** HIGH (Week 1)  
**Catégorie:** Gateway Enhancements  
**Dépendances:** 113 (Polly Resilience Policies)

## OBJECTIF

Créer des wrappers HttpClient spécifiques pour chaque type de fournisseur LLM afin de :
- Centraliser la configuration HTTP (headers, timeouts, retry policies)
- Isoler les dépendances par fournisseur
- Faciliter les tests unitaires avec mocking

## CONTEXTE

Actuellement, les clients de fournisseurs LLM (OpenAI, Anthropic, Ollama, etc.) créent et configurent leurs propres HttpClient, ce qui entraîne :
- Duplication de configuration (headers, authentication, timeouts)
- Difficulté à tester (HttpClient difficile à mocker)
- Gestion incohérente des erreurs HTTP

L'utilisation de `IHttpClientFactory` avec clients nommés permet de centraliser la configuration et d'appliquer les politiques Polly de manière cohérente.

## CRITÈRES DE SUCCÈS

- [ ] Interface `ILLMProviderHttpClient` définissant les opérations HTTP communes
- [ ] Implémentation `OpenAIHttpClient`, `AnthropicHttpClient`, `OllamaHttpClient`, etc.
- [ ] Configuration centralisée dans `DependencyInjection.cs` via `AddHttpClient`
- [ ] Politiques Polly (Retry, Circuit Breaker) attachées à chaque client
- [ ] Headers spécifiques par fournisseur (Authorization, User-Agent, etc.)
- [ ] Timeouts configurables par fournisseur
- [ ] Tests unitaires avec `MockHttpMessageHandler` ou `WireMock.Net`
- [ ] Documentation XML complète
- [ ] Build : 0 erreurs, 0 warnings

## FICHIERS CONCERNÉS

- `src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Clients/ILLMProviderHttpClient.cs` (nouveau)
- `src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Clients/OpenAIHttpClient.cs` (nouveau)
- `src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Clients/AnthropicHttpClient.cs` (nouveau)
- `src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Clients/OllamaHttpClient.cs` (nouveau)
- `src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/DependencyInjection.cs` (modifier)
- `tests/LLMProxy.Infrastructure.LLMProviders.Tests/Clients/OpenAIHttpClientTests.cs` (nouveau)

## APPROCHE TECHNIQUE

### 1. Définir interface commune

```csharp
namespace LLMProxy.Infrastructure.LLMProviders.Clients;

/// <summary>
/// Interface pour les clients HTTP des fournisseurs LLM.
/// </summary>
public interface ILLMProviderHttpClient
{
    /// <summary>
    /// Envoie une requête POST asynchrone vers le fournisseur LLM.
    /// </summary>
    /// <param name="endpoint">Endpoint relatif (ex: /v1/chat/completions).</param>
    /// <param name="content">Contenu JSON de la requête.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Réponse HTTP du fournisseur.</returns>
    Task<HttpResponseMessage> PostAsync(
        string endpoint,
        HttpContent content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envoie une requête GET asynchrone vers le fournisseur LLM.
    /// </summary>
    /// <param name="endpoint">Endpoint relatif (ex: /v1/models).</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Réponse HTTP du fournisseur.</returns>
    Task<HttpResponseMessage> GetAsync(
        string endpoint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envoie une requête POST asynchrone avec streaming SSE.
    /// </summary>
    /// <param name="endpoint">Endpoint relatif.</param>
    /// <param name="content">Contenu JSON de la requête.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Stream de réponse pour SSE.</returns>
    Task<Stream> PostStreamAsync(
        string endpoint,
        HttpContent content,
        CancellationToken cancellationToken = default);
}
```

### 2. Implémenter OpenAIHttpClient

```csharp
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LLMProxy.Infrastructure.LLMProviders.Clients;

/// <summary>
/// Client HTTP spécifique pour OpenAI API.
/// </summary>
public sealed class OpenAIHttpClient : ILLMProviderHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIHttpClient> _logger;
    private readonly OpenAIOptions _options;

    /// <summary>
    /// Initialise une nouvelle instance du client OpenAI.
    /// </summary>
    /// <param name="httpClient">HttpClient injecté par IHttpClientFactory.</param>
    /// <param name="options">Options de configuration OpenAI.</param>
    /// <param name="logger">Logger pour traçabilité.</param>
    public OpenAIHttpClient(
        HttpClient httpClient,
        IOptions<OpenAIOptions> options,
        ILogger<OpenAIHttpClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        // Configuration des headers
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "LLMProxy/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> PostAsync(
        string endpoint,
        HttpContent content,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("OpenAI POST {Endpoint}", endpoint);
        
        var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        
        _logger.LogDebug("OpenAI Response {StatusCode}", response.StatusCode);
        
        return response;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> GetAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("OpenAI GET {Endpoint}", endpoint);
        
        var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        
        _logger.LogDebug("OpenAI Response {StatusCode}", response.StatusCode);
        
        return response;
    }

    /// <inheritdoc/>
    public async Task<Stream> PostStreamAsync(
        string endpoint,
        HttpContent content,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("OpenAI POST Stream {Endpoint}", endpoint);
        
        var response = await _httpClient.PostAsync(endpoint, content, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        _logger.LogDebug("OpenAI Stream opened");
        
        return stream;
    }
}

/// <summary>
/// Options de configuration pour OpenAI.
/// </summary>
public class OpenAIOptions
{
    /// <summary>
    /// URL de base de l'API OpenAI.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>
    /// Clé API OpenAI.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Timeout des requêtes en secondes.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;
}
```

### 3. Implémenter AnthropicHttpClient

```csharp
namespace LLMProxy.Infrastructure.LLMProviders.Clients;

/// <summary>
/// Client HTTP spécifique pour Anthropic Claude API.
/// </summary>
public sealed class AnthropicHttpClient : ILLMProviderHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AnthropicHttpClient> _logger;
    private readonly AnthropicOptions _options;

    /// <summary>
    /// Initialise une nouvelle instance du client Anthropic.
    /// </summary>
    public AnthropicHttpClient(
        HttpClient httpClient,
        IOptions<AnthropicOptions> options,
        ILogger<AnthropicHttpClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        // Configuration spécifique Anthropic
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", _options.ApiVersion);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "LLMProxy/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    // Implémenter ILLMProviderHttpClient (similaire à OpenAIHttpClient)
    // ...
}

/// <summary>
/// Options de configuration pour Anthropic.
/// </summary>
public class AnthropicOptions
{
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    public string ApiKey { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2023-06-01";
    public int TimeoutSeconds { get; set; } = 60;
}
```

### 4. Configurer DependencyInjection.cs

```csharp
public static IServiceCollection AddLLMProviders(this IServiceCollection services, IConfiguration configuration)
{
    // Configurer Options depuis appsettings.json
    services.Configure<OpenAIOptions>(configuration.GetSection("LLMProviders:OpenAI"));
    services.Configure<AnthropicOptions>(configuration.GetSection("LLMProviders:Anthropic"));
    services.Configure<OllamaOptions>(configuration.GetSection("LLMProviders:Ollama"));

    // HttpClient OpenAI avec politiques Polly
    services.AddHttpClient<ILLMProviderHttpClient, OpenAIHttpClient>("OpenAI")
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        })
        .AddPolicyHandler(ResiliencePolicies.GetRetryPolicy(maxRetryAttempts: 3))
        .AddPolicyHandler(ResiliencePolicies.GetCircuitBreakerPolicy(failureThreshold: 5));

    // HttpClient Anthropic avec politiques Polly
    services.AddHttpClient<ILLMProviderHttpClient, AnthropicHttpClient>("Anthropic")
        .AddPolicyHandler(ResiliencePolicies.GetRetryPolicy(maxRetryAttempts: 3))
        .AddPolicyHandler(ResiliencePolicies.GetCircuitBreakerPolicy(failureThreshold: 5));

    // Répéter pour Ollama, Cohere, Mistral...

    return services;
}
```

### 5. Configuration appsettings.json

```json
{
  "LLMProviders": {
    "OpenAI": {
      "BaseUrl": "https://api.openai.com/v1",
      "ApiKey": "sk-...",
      "TimeoutSeconds": 60
    },
    "Anthropic": {
      "BaseUrl": "https://api.anthropic.com",
      "ApiKey": "sk-ant-...",
      "ApiVersion": "2023-06-01",
      "TimeoutSeconds": 60
    },
    "Ollama": {
      "BaseUrl": "http://localhost:11434",
      "TimeoutSeconds": 120
    }
  }
}
```

### 6. Tests unitaires avec WireMock.Net

```csharp
using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace LLMProxy.Infrastructure.LLMProviders.Tests.Clients;

/// <summary>
/// Tests unitaires pour OpenAIHttpClient avec WireMock.
/// </summary>
public class OpenAIHttpClientTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly OpenAIHttpClient _client;

    public OpenAIHttpClientTests()
    {
        // Démarrer WireMock server
        _server = WireMockServer.Start();

        // Configurer client avec URL WireMock
        var options = Options.Create(new OpenAIOptions
        {
            BaseUrl = _server.Url!,
            ApiKey = "test-key",
            TimeoutSeconds = 30
        });

        var httpClient = new HttpClient();
        var logger = new NullLogger<OpenAIHttpClient>();
        _client = new OpenAIHttpClient(httpClient, options, logger);
    }

    [Fact]
    public async Task PostAsync_ShouldSendRequestWithAuthHeader()
    {
        // Arrange
        _server
            .Given(Request.Create().WithPath("/v1/chat/completions").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"id\":\"test\"}"));

        var content = new StringContent("{\"model\":\"gpt-4\"}", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/v1/chat/completions", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _server.LogEntries.Should().HaveCount(1);
        _server.LogEntries.First().RequestMessage.Headers.Should().ContainKey("Authorization");
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }
}
```

## DÉFINITION DE TERMINÉ

- [ ] Interface `ILLMProviderHttpClient` créée et documentée
- [ ] Clients HTTP implémentés : OpenAI, Anthropic, Ollama
- [ ] Configuration centralisée dans `DependencyInjection.cs`
- [ ] Options configurables via `appsettings.json`
- [ ] Politiques Polly attachées à tous les clients
- [ ] Tests unitaires avec WireMock validant headers et responses
- [ ] Documentation XML complète
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests : 100% passing

## RÉFÉRENCES

- **Source:** `docs/NEXT_STEPS.md` (High Priority - Gateway Enhancements)
- **ADR-005:** SOLID Principles (Dependency Inversion)
- **IHttpClientFactory:** https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory
- **WireMock.Net:** https://github.com/WireMock-Net/WireMock.Net
