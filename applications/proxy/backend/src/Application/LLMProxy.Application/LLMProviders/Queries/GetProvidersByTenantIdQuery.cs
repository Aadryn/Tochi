using LLMProxy.Application.Common;

namespace LLMProxy.Application.LLMProviders.Queries;

/// <summary>
/// Requête pour récupérer tous les fournisseurs LLM d'un tenant
/// </summary>
public record GetProvidersByTenantIdQuery : IQuery<IEnumerable<LLMProviderDto>>
{
    public Guid TenantId { get; init; }
}
