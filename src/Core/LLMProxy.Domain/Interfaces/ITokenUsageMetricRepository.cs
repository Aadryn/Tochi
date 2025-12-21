using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository interface for TokenUsageMetric aggregate (Port)
/// </summary>
public interface ITokenUsageMetricRepository
{
    Task<TokenUsageMetric?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TokenUsageMetric?> GetByPeriodAsync(Guid tenantId, Guid? userId, Guid? providerId, DateTime periodStart, MetricPeriod period, CancellationToken cancellationToken = default);
    Task<IEnumerable<TokenUsageMetric>> GetByTenantIdAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IEnumerable<TokenUsageMetric>> GetByUserIdAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task AddAsync(TokenUsageMetric metric, CancellationToken cancellationToken = default);
    Task UpdateAsync(TokenUsageMetric metric, CancellationToken cancellationToken = default);
}
