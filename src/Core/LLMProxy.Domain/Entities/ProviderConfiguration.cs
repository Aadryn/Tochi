using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Configuration spécifique à un fournisseur
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
