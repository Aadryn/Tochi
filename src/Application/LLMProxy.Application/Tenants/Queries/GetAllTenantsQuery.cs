using LLMProxy.Application.Common;

namespace LLMProxy.Application.Tenants.Queries;

/// <summary>
/// Requête pour récupérer tous les tenants
/// </summary>
public record GetAllTenantsQuery : IQuery<IEnumerable<TenantDto>>;
