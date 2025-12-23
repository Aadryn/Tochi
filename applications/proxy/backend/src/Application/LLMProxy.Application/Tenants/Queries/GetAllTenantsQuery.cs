using LLMProxy.Application.Common;
using LLMProxy.Application.Common.Interfaces;

namespace LLMProxy.Application.Tenants.Queries;

/// <summary>
/// Requête pour récupérer tous les tenants.
/// </summary>
/// <remarks>
/// Cette Query est mise en cache avec une durée de vie de 15 minutes (ADR-042).
/// Raison : Liste de tenants rarement modifiée, mais peut évoluer plus souvent que les détails d'un tenant unique.
/// </remarks>
public record GetAllTenantsQuery : IQuery<IEnumerable<TenantDto>>, ICachedQuery<IEnumerable<TenantDto>>
{
    /// <summary>
    /// Durée de vie du cache : 15 minutes (liste de données stables).
    /// </summary>
    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(15);
}
