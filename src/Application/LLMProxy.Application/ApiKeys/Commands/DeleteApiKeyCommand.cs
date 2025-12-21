using LLMProxy.Application.Common;

namespace LLMProxy.Application.ApiKeys.Commands;

/// <summary>
/// Commande pour supprimer une clé API.
/// </summary>
public record DeleteApiKeyCommand : ICommand
{
    public Guid ApiKeyId { get; init; }
}
