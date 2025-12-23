using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository pour l'agrégat QuotaLimit (Port dans l'architecture hexagonale).
/// </summary>
public interface IQuotaLimitRepository : IRepository<QuotaLimit>
{
    /// <summary>
    /// Récupère tous les quotas d'un utilisateur.
    /// </summary>
    Task<Result<IReadOnlyList<QuotaLimit>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère un quota par utilisateur, type et période.
    /// </summary>
    Task<Result<QuotaLimit>> GetByUserAndTypeAsync(Guid userId, QuotaType quotaType, QuotaPeriod period, CancellationToken cancellationToken = default);
}
