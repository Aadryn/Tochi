using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository pour l'agrégat LLMProvider (Port dans l'architecture hexagonale).
/// </summary>
public interface ILLMProviderRepository : IRepository<LLMProvider>
{
    /// <summary>
    /// Récupère les providers d'un tenant avec filtrage optionnel par statut actif.
    /// </summary>
    Task<Result<IReadOnlyList<LLMProvider>>> GetByTenantIdAsync(Guid tenantId, bool onlyActive = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les providers par stratégie de routage.
    /// </summary>
    Task<Result<IReadOnlyList<LLMProvider>>> GetByRoutingStrategyAsync(Guid tenantId, RoutingMethod method, string value, CancellationToken cancellationToken = default);
}
