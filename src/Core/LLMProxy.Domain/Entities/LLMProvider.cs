using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Represents an LLM provider configuration for a tenant
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

        if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
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
        if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
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
}

public enum ProviderType
{
    OpenAI = 0,
    AzureOpenAI = 1,
    Ollama = 2,
    Anthropic = 3,
    Mistral = 4,
    GoogleGemini = 5,
    Cohere = 6,
    HuggingFace = 7,
    Custom = 99
}

/// <summary>
/// Provider-specific configuration
/// </summary>
public class ProviderConfiguration : ValueObject
{
    public string ApiKeySecretName { get; private set; } // Reference to secret vault
    public Dictionary<string, string> Headers { get; private set; }
    public int TimeoutSeconds { get; private set; }
    public int MaxRetries { get; private set; }
    public bool SupportsStreaming { get; private set; }
    public bool RequiresCertificate { get; private set; }
    public string? CertificateSecretName { get; private set; }

    private ProviderConfiguration() 
    { 
        ApiKeySecretName = string.Empty;
        Headers = new(); 
    }

    public ProviderConfiguration(
        string apiKeySecretName,
        Dictionary<string, string>? headers = null,
        int timeoutSeconds = 30,
        int maxRetries = 3,
        bool supportsStreaming = true,
        bool requiresCertificate = false,
        string? certificateSecretName = null)
    {
        ApiKeySecretName = apiKeySecretName ?? throw new ArgumentNullException(nameof(apiKeySecretName));
        Headers = headers ?? new Dictionary<string, string>();
        TimeoutSeconds = timeoutSeconds;
        MaxRetries = maxRetries;
        SupportsStreaming = supportsStreaming;
        RequiresCertificate = requiresCertificate;
        CertificateSecretName = certificateSecretName;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ApiKeySecretName;
        yield return TimeoutSeconds;
        yield return MaxRetries;
        yield return SupportsStreaming;
        yield return RequiresCertificate;
        yield return CertificateSecretName;
    }
}

/// <summary>
/// Routing strategy for provider selection
/// </summary>
public class RoutingStrategy : ValueObject
{
    public RoutingMethod Method { get; private set; }
    public string? PathPattern { get; private set; }
    public string? HeaderName { get; private set; }
    public string? Subdomain { get; private set; }

    private RoutingStrategy() { }

    public RoutingStrategy(
        RoutingMethod method,
        string? pathPattern = null,
        string? headerName = null,
        string? subdomain = null)
    {
        Method = method;
        PathPattern = pathPattern;
        HeaderName = headerName;
        Subdomain = subdomain;
    }

    public static RoutingStrategy ByPath(string pathPattern) =>
        new(RoutingMethod.Path, pathPattern: pathPattern);

    public static RoutingStrategy ByHeader(string headerName) =>
        new(RoutingMethod.Header, headerName: headerName);

    public static RoutingStrategy BySubdomain(string subdomain) =>
        new(RoutingMethod.Subdomain, subdomain: subdomain);

    public static RoutingStrategy ByUser() =>
        new(RoutingMethod.UserConfig);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Method;
        yield return PathPattern;
        yield return HeaderName;
        yield return Subdomain;
    }
}

public enum RoutingMethod
{
    Path = 0,
    Header = 1,
    Subdomain = 2,
    UserConfig = 3
}
