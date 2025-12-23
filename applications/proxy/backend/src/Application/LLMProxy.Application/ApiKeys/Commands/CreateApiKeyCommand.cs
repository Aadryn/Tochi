using LLMProxy.Application.Common;

namespace LLMProxy.Application.ApiKeys.Commands;

/// <summary>
/// Commande pour créer une nouvelle clé API.
/// </summary>
public record CreateApiKeyCommand : ICommand<ApiKeyDto>
{
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime? ExpiresAt { get; init; }
}
