using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'agrégat TokenUsageMetric avec support Result Pattern.
/// </summary>
internal class TokenUsageMetricRepository : RepositoryBase<TokenUsageMetric>, ITokenUsageMetricRepository
{
    public TokenUsageMetricRepository(LLMProxyDbContext context, ILogger<TokenUsageMetricRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<Result<TokenUsageMetric>> GetByPeriodAsync(Guid tenantId, Guid? userId, Guid? providerId, DateTime periodStart, MetricPeriod period, CancellationToken cancellationToken = default)
    {
        try
        {
            var metric = await DbSet.FirstOrDefaultAsync(m => m.TenantId == tenantId && m.UserId == userId && m.ProviderId == providerId && m.PeriodStart == periodStart && m.Period == period, cancellationToken);
            if (metric is null)
            {
                Logger.LogDebug("Aucune métrique trouvée pour tenant {TenantId}, période {PeriodStart}", tenantId, periodStart);
                return Error.Database.EntityNotFound("TokenUsageMetric", Guid.Empty);
            }
            return Result<TokenUsageMetric>.Success(metric);
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByPeriodAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération de la métrique pour tenant {TenantId}", tenantId); return Error.Database.AccessError("GetByPeriodAsync", ex.Message); }
    }

    public async Task<Result<IReadOnlyList<TokenUsageMetric>>> GetByTenantIdAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await DbSet.Where(m => m.TenantId == tenantId && m.PeriodStart >= from && m.PeriodStart <= to).OrderBy(m => m.PeriodStart).ToListAsync(cancellationToken);
            Logger.LogDebug("Récupéré {Count} métriques pour tenant {TenantId} de {From} à {To}", metrics.Count, tenantId, from, to);
            return Result<IReadOnlyList<TokenUsageMetric>>.Success(metrics.AsReadOnly());
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByTenantIdAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération des métriques du tenant {TenantId}", tenantId); return Error.Database.AccessError("GetByTenantIdAsync", ex.Message); }
    }

    public async Task<Result<IReadOnlyList<TokenUsageMetric>>> GetByUserIdAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await DbSet.Where(m => m.UserId == userId && m.PeriodStart >= from && m.PeriodStart <= to).OrderBy(m => m.PeriodStart).ToListAsync(cancellationToken);
            Logger.LogDebug("Récupéré {Count} métriques pour user {UserId} de {From} à {To}", metrics.Count, userId, from, to);
            return Result<IReadOnlyList<TokenUsageMetric>>.Success(metrics.AsReadOnly());
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByUserIdAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération des métriques de l'utilisateur {UserId}", userId); return Error.Database.AccessError("GetByUserIdAsync", ex.Message); }
    }
}
