using LLMProxy.Application.Common;

namespace LLMProxy.Application.LLMProviders.Queries;

/// <summary>
/// Requête pour récupérer un fournisseur LLM par son identifiant
/// </summary>
public record GetProviderByIdQuery : IQuery<LLMProviderDto>
{
    public Guid ProviderId { get; init; }
}
