using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;

namespace LLMProxy.Application.Tenants.Queries;

/// <summary>
/// Requête de récupération d'un tenant par son identifiant unique.
/// </summary>
/// <remarks>
/// Retourne les détails complets d'un tenant spécifique incluant ses paramètres de configuration.
/// </remarks>
public record GetTenantByIdQuery : IQuery<TenantDto>
{
    /// <summary>
    /// Obtient ou initialise l'identifiant unique du tenant à récupérer.
    /// </summary>
    public Guid TenantId { get; init; }
}
