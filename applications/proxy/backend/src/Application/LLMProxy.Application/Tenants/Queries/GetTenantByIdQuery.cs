using LLMProxy.Application.Common;
using LLMProxy.Application.Common.Interfaces;
using LLMProxy.Domain.Common;

namespace LLMProxy.Application.Tenants.Queries;

/// <summary>
/// Requête de récupération d'un tenant par son identifiant unique.
/// </summary>
/// <remarks>
/// Retourne les détails complets d'un tenant spécifique incluant ses paramètres de configuration.
/// Cette Query est mise en cache avec une durée de vie de 30 minutes (ADR-042).
/// Raison : Les tenants changent rarement, mise en cache agressive justifiée.
/// </remarks>
public record GetTenantByIdQuery : IQuery<TenantDto>, ICachedQuery<TenantDto>
{
    /// <summary>
    /// Obtient ou initialise l'identifiant unique du tenant à récupérer.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// Durée de vie du cache : 30 minutes (données stables).
    /// </summary>
    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(30);
}
