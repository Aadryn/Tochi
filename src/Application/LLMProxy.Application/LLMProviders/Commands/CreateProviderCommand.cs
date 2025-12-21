using LLMProxy.Application.Common;

namespace LLMProxy.Application.LLMProviders.Commands;

/// <summary>
/// Commande pour créer un nouveau fournisseur LLM.
/// </summary>
public record CreateProviderCommand : ICommand<LLMProviderDto>
{
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ProviderType { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string ApiKeySecretName { get; init; } = string.Empty;
    public string RoutingStrategy { get; init; } = string.Empty;
    public int Priority { get; init; }
    public Dictionary<string, string> CustomHeaders { get; init; } = new();
    public int TimeoutSeconds { get; init; } = 30;
    public int MaxRetries { get; init; } = 3;
}
