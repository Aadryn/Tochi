using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'entité TokenUsageMetric.
/// </summary>
internal class TokenUsageMetricRepository : ITokenUsageMetricRepository
{
    private readonly LLMProxyDbContext _context;

    public TokenUsageMetricRepository(LLMProxyDbContext context) => _context = context;

    public async Task<Domain.Entities.TokenUsageMetric?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TokenUsageMetrics.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<Domain.Entities.TokenUsageMetric?> GetByPeriodAsync(Guid tenantId, Guid? userId, Guid? providerId, DateTime periodStart, Domain.Entities.MetricPeriod period, CancellationToken cancellationToken = default)
    {
        return await _context.TokenUsageMetrics.FirstOrDefaultAsync(m => m.TenantId == tenantId && m.UserId == userId && m.ProviderId == providerId && m.PeriodStart == periodStart && m.Period == period, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.TokenUsageMetric>> GetByTenantIdAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.TokenUsageMetrics.Where(m => m.TenantId == tenantId && m.PeriodStart >= from && m.PeriodStart <= to).OrderBy(m => m.PeriodStart).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.TokenUsageMetric>> GetByUserIdAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.TokenUsageMetrics.Where(m => m.UserId == userId && m.PeriodStart >= from && m.PeriodStart <= to).OrderBy(m => m.PeriodStart).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.TokenUsageMetric metric, CancellationToken cancellationToken = default)
    {
        await _context.TokenUsageMetrics.AddAsync(metric, cancellationToken);
    }

    public Task UpdateAsync(Domain.Entities.TokenUsageMetric metric, CancellationToken cancellationToken = default)
    {
        _context.TokenUsageMetrics.Update(metric);
        return Task.CompletedTask;
    }
}
