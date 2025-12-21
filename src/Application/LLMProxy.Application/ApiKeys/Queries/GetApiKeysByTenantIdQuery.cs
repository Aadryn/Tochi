using LLMProxy.Application.Common;

namespace LLMProxy.Application.ApiKeys.Queries;

/// <summary>
/// Requête pour récupérer toutes les clés API d'un tenant
/// </summary>
public record GetApiKeysByTenantIdQuery : IQuery<IEnumerable<ApiKeyDto>>
{
    public Guid TenantId { get; init; }
}
