using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository pour l'agrégat TokenUsageMetric (Port dans l'architecture hexagonale).
/// </summary>
public interface ITokenUsageMetricRepository : IRepository<TokenUsageMetric>
{
    /// <summary>
    /// Récupère une métrique par période.
    /// </summary>
    Task<Result<TokenUsageMetric>> GetByPeriodAsync(Guid tenantId, Guid? userId, Guid? providerId, DateTime periodStart, MetricPeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les métriques d'un tenant dans une plage temporelle.
    /// </summary>
    Task<Result<IReadOnlyList<TokenUsageMetric>>> GetByTenantIdAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les métriques d'un utilisateur dans une plage temporelle.
    /// </summary>
    Task<Result<IReadOnlyList<TokenUsageMetric>>> GetByUserIdAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
