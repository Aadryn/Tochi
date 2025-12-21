using LLMProxy.Application.Common;

namespace LLMProxy.Application.LLMProviders.Commands;

/// <summary>
/// Commande pour supprimer un fournisseur LLM.
/// </summary>
public record DeleteProviderCommand : ICommand
{
    public Guid ProviderId { get; init; }
}
