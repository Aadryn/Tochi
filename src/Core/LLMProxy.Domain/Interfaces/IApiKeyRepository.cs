using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository pour l'agrégat ApiKey (Port dans l'architecture hexagonale).
/// </summary>
public interface IApiKeyRepository : IRepository<ApiKey>
{
    /// <summary>
    /// Récupère une clé API par son hash.
    /// </summary>
    Task<Result<ApiKey>> GetByKeyHashAsync(string keyHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère une clé API par son préfixe.
    /// </summary>
    Task<Result<ApiKey>> GetByKeyPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère toutes les clés API d'un utilisateur.
    /// </summary>
    Task<Result<IReadOnlyList<ApiKey>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère toutes les clés API d'un tenant.
    /// </summary>
    Task<Result<IReadOnlyList<ApiKey>>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
