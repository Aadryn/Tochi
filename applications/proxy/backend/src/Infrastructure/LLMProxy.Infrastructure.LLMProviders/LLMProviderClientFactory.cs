using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using LLMProxy.Infrastructure.LLMProviders.Configuration;
using LLMProxy.Infrastructure.LLMProviders.Providers;
using LLMProxy.Infrastructure.LLMProviders.Providers.Local;
using LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LLMProxy.Infrastructure.LLMProviders;

/// <summary>
/// Fabrique de clients LLM.
/// Crée et gère les instances de clients selon la configuration.
/// </summary>
public sealed class LLMProviderClientFactory : ILLMProviderClientFactory, IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly LLMProvidersOptions _options;
    private readonly Dictionary<string, ILLMProviderClient> _clients = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Constructeur.
    /// </summary>
    public LLMProviderClientFactory(
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        IOptions<LLMProvidersOptions> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public ILLMProviderClient CreateClient(ProviderType providerType)
    {
        var config = _options.Providers?.FirstOrDefault(p => p.Type == providerType)
            ?? throw new NotSupportedException($"Provider {providerType} non supporté ou non configuré");

        return CreateFromConfiguration(config);
    }

    /// <inheritdoc />
    public ILLMProviderClient CreateClient(string providerName)
    {
        if (string.IsNullOrEmpty(providerName))
            throw new ArgumentNullException(nameof(providerName));

        lock (_lock)
        {
            if (_clients.TryGetValue(providerName, out var existingClient))
                return existingClient;

            var config = _options.Providers?.FirstOrDefault(p =>
                p.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase))
                ?? throw new KeyNotFoundException($"Provider '{providerName}' non trouvé dans la configuration");

            var client = CreateFromConfiguration(config);
            _clients[providerName] = client;

            return client;
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<ILLMProviderClient> GetAllClients()
    {
        if (_options.Providers == null || _options.Providers.Count == 0)
            return Array.Empty<ILLMProviderClient>();

        lock (_lock)
        {
            foreach (var config in _options.Providers.Where(c => c.IsEnabled))
            {
                if (!_clients.ContainsKey(config.Name))
                {
                    var client = CreateFromConfiguration(config);
                    _clients[config.Name] = client;
                }
            }

            return _clients.Values.ToList();
        }
    }

    /// <inheritdoc />
    public bool IsSupported(ProviderType providerType)
    {
        return _options.Providers?.Any(p => p.Type == providerType && p.IsEnabled) ?? false;
    }

    /// <inheritdoc />
    public IReadOnlyList<ProviderType> GetSupportedTypes()
    {
        if (_options.Providers == null || _options.Providers.Count == 0)
        {
            return Array.Empty<ProviderType>();
        }

        return _options.Providers
            .Where(p => p.IsEnabled)
            .Select(p => p.Type)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Crée un client à partir d'une configuration.
    /// </summary>
    private ILLMProviderClient CreateFromConfiguration(LLMProviderConfiguration config)
    {
        var httpClient = _httpClientFactory.CreateClient(config.Name);

        return config.Type switch
        {
            ProviderType.OpenAI => new OpenAIProviderClient(
                httpClient,
                config,
                _loggerFactory.CreateLogger<OpenAIProviderClient>()),

            ProviderType.AzureOpenAI => new AzureOpenAIProviderClient(
                httpClient,
                config,
                _loggerFactory.CreateLogger<AzureOpenAIProviderClient>()),

            ProviderType.Anthropic => new AnthropicProviderClient(
                httpClient,
                config,
                _loggerFactory.CreateLogger<AnthropicProviderClient>()),

            ProviderType.GoogleGemini => new GoogleGeminiProviderClient(
                httpClient,
                config,
                _loggerFactory.CreateLogger<GoogleGeminiProviderClient>()),

            ProviderType.Cohere => new CohereProviderClient(
                httpClient,
                config,
                _loggerFactory.CreateLogger<CohereProviderClient>()),

            ProviderType.HuggingFace => new HuggingFaceProviderClient(
                httpClient,
                config,
                _loggerFactory.CreateLogger<HuggingFaceProviderClient>()),

            ProviderType.Ollama => new OllamaProviderClient(
                httpClient,
                config,
                _loggerFactory.CreateLogger<OllamaProviderClient>()),

            ProviderType.AWSBedrock => new AWSBedrockProviderClient(
                httpClient,
                config,
                _loggerFactory.CreateLogger<AWSBedrockProviderClient>()),

            ProviderType.VLLM or
            ProviderType.OVHCloud or
            ProviderType.Scaleway => new OpenAICompatibleProviderClient(
                httpClient,
                config,
                _loggerFactory.CreateLogger<OpenAICompatibleProviderClient>()),

            _ => throw new NotSupportedException($"Type de provider non supporté: {config.Type}")
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            foreach (var client in _clients.Values)
            {
                client.Dispose();
            }

            _clients.Clear();
            _disposed = true;
        }
    }
}
