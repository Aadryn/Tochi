using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Représente une configuration de fournisseur LLM pour un tenant
/// </summary>
public class LLMProvider : Entity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public ProviderType Type { get; private set; }
    public string BaseUrl { get; private set; }
    public string Model { get; private set; }
    public bool IsActive { get; private set; }
    public int Priority { get; private set; } // For failover ordering
    
    // Provider-specific configuration (stored as JSON or separate entity)
    public ProviderConfiguration Configuration { get; private set; }
    
    // Routing configuration
    public RoutingStrategy RoutingStrategy { get; private set; }
    
    // Navigation
    public Tenant Tenant { get; private set; } = null!;

    private LLMProvider() 
    {
        Name = string.Empty;
        BaseUrl = string.Empty;
        Model = string.Empty;
        Configuration = null!;
        RoutingStrategy = null!;
    } // EF Core

    private LLMProvider(
        Guid tenantId,
        string name,
        ProviderType type,
        string baseUrl,
        string model,
        ProviderConfiguration configuration,
        RoutingStrategy routingStrategy,
        int priority)
    {
        TenantId = tenantId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type;
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        RoutingStrategy = routingStrategy ?? throw new ArgumentNullException(nameof(routingStrategy));
        Priority = priority;
        IsActive = true;
    }

    public static Result<LLMProvider> Create(
        Guid tenantId,
        string name,
        ProviderType type,
        string baseUrl,
        string model,
        ProviderConfiguration configuration,
        RoutingStrategy routingStrategy,
        int priority = 0)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<LLMProvider>("Invalid tenant ID.");

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<LLMProvider>("Provider name cannot be empty.");

        if (!IsValidUrl(baseUrl))
            return Result.Failure<LLMProvider>("Invalid base URL.");

        if (string.IsNullOrWhiteSpace(model))
            return Result.Failure<LLMProvider>("Model name cannot be empty.");

        if (priority < 0)
            return Result.Failure<LLMProvider>("Priority cannot be negative.");

        var provider = new LLMProvider(tenantId, name, type, baseUrl, model, configuration, routingStrategy, priority);
        
        return Result.Success(provider);
    }

    // Overload for command-based creation with simpler parameters
    public static LLMProvider Create(
        Guid tenantId,
        string name,
        ProviderType type,
        string baseUrl,
        string apiKeySecretName,
        RoutingStrategy routingStrategy,
        int priority,
        Dictionary<string, string>? customHeaders = null,
        int timeoutSeconds = 30,
        int maxRetries = 3)
    {
        var configuration = new ProviderConfiguration(
            apiKeySecretName,
            customHeaders,
            timeoutSeconds,
            maxRetries,
            supportsStreaming: true
        );

        // Determine default model based on provider type
        var model = type switch
        {
            ProviderType.OpenAI => "gpt-4",
            ProviderType.AzureOpenAI => "gpt-4",
            ProviderType.Ollama => "llama2",
            ProviderType.Anthropic => "claude-3-opus-20240229",
            ProviderType.Mistral => "mistral-medium",
            ProviderType.GoogleGemini => "gemini-pro",
            _ => "default"
        };

        return new LLMProvider(tenantId, name, type, baseUrl, model, configuration, routingStrategy, priority);
    }

    public Result UpdateConfiguration(ProviderConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure("Provider is already inactive.");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure("Provider is already active.");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success();
    }

    public void UpdateBaseUrl(string baseUrl)
    {
        if (!IsValidUrl(baseUrl))
            throw new ArgumentException("Invalid base URL.", nameof(baseUrl));

        BaseUrl = baseUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePriority(int priority)
    {
        if (priority < 0)
            throw new ArgumentException("Priority cannot be negative.", nameof(priority));

        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCustomHeaders(Dictionary<string, string> headers)
    {
        Configuration = new ProviderConfiguration(
            Configuration.ApiKeySecretName,
            headers,
            Configuration.TimeoutSeconds,
            Configuration.MaxRetries,
            Configuration.SupportsStreaming,
            Configuration.RequiresCertificate,
            Configuration.CertificateSecretName
        );
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateConfiguration(int timeoutSeconds, int maxRetries)
    {
        Configuration = new ProviderConfiguration(
            Configuration.ApiKeySecretName,
            Configuration.Headers,
            timeoutSeconds,
            maxRetries,
            Configuration.SupportsStreaming,
            Configuration.RequiresCertificate,
            Configuration.CertificateSecretName
        );
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Valide qu'une URL est bien formée et absolue.
    /// </summary>
    /// <param name="url">URL à valider.</param>
    /// <returns>True si l'URL est valide, sinon false.</returns>
    private static bool IsValidUrl(string url)
    {
        return !string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
