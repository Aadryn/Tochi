using LLMProxy.Application.Common;

namespace LLMProxy.Application.ApiKeys.Commands;

/// <summary>
/// Commande pour révoquer une clé API.
/// </summary>
public record RevokeApiKeyCommand : ICommand
{
    public Guid ApiKeyId { get; init; }
}
