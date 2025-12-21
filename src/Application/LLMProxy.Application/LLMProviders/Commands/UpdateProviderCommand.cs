using LLMProxy.Application.Common;

namespace LLMProxy.Application.LLMProviders.Commands;

/// <summary>
/// Commande pour mettre à jour un fournisseur LLM existant.
/// </summary>
public record UpdateProviderCommand : ICommand<LLMProviderDto>
{
    public Guid ProviderId { get; set; }
    public string? BaseUrl { get; init; }
    public int? Priority { get; init; }
    public Dictionary<string, string>? CustomHeaders { get; init; }
    public int? TimeoutSeconds { get; init; }
    public int? MaxRetries { get; init; }
}
