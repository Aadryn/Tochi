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
            var metric = await DbSet.FirstOrDefaultAsync(m => MatchesPeriodCriteria(m, tenantId, userId, providerId, periodStart, period), cancellationToken);
            
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
            var metrics = await DbSet
                .Where(m => MatchesTenantAndDateRange(m, tenantId, from, to))
                .OrderBy(m => m.PeriodStart)
                .ToListAsync(cancellationToken);
            
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
            var metrics = await DbSet
                .Where(m => MatchesUserAndDateRange(m, userId, from, to))
                .OrderBy(m => m.PeriodStart)
                .ToListAsync(cancellationToken);
            
            Logger.LogDebug("Récupéré {Count} métriques pour user {UserId} de {From} à {To}", metrics.Count, userId, from, to);
            return Result<IReadOnlyList<TokenUsageMetric>>.Success(metrics.AsReadOnly());
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByUserIdAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération des métriques de l'utilisateur {UserId}", userId); return Error.Database.AccessError("GetByUserIdAsync", ex.Message); }
    }

    /// <summary>
    /// Vérifie si une métrique correspond aux critères de période spécifiés.
    /// </summary>
    /// <param name="metric">Métrique à évaluer.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="userId">Identifiant de l'utilisateur (optionnel).</param>
    /// <param name="providerId">Identifiant du fournisseur LLM (optionnel).</param>
    /// <param name="periodStart">Début de la période.</param>
    /// <param name="period">Type de période (horaire, quotidien, mensuel).</param>
    /// <returns>True si la métrique correspond à tous les critères.</returns>
    private static bool MatchesPeriodCriteria(
        TokenUsageMetric metric,
        Guid tenantId,
        Guid? userId,
        Guid? providerId,
        DateTime periodStart,
        MetricPeriod period)
    {
        return metric.TenantId == tenantId
            && metric.UserId == userId
            && metric.ProviderId == providerId
            && metric.PeriodStart == periodStart
            && metric.Period == period;
    }

    /// <summary>
    /// Vérifie si une métrique correspond au tenant et à la plage de dates.
    /// </summary>
    /// <param name="metric">Métrique à évaluer.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="from">Date de début (inclusive).</param>
    /// <param name="to">Date de fin (inclusive).</param>
    /// <returns>True si la métrique correspond aux critères.</returns>
    private static bool MatchesTenantAndDateRange(TokenUsageMetric metric, Guid tenantId, DateTime from, DateTime to)
    {
        return metric.TenantId == tenantId
            && metric.PeriodStart >= from
            && metric.PeriodStart <= to;
    }

    /// <summary>
    /// Vérifie si une métrique correspond à l'utilisateur et à la plage de dates.
    /// </summary>
    /// <param name="metric">Métrique à évaluer.</param>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="from">Date de début (inclusive).</param>
    /// <param name="to">Date de fin (inclusive).</param>
    /// <returns>True si la métrique correspond aux critères.</returns>
    private static bool MatchesUserAndDateRange(TokenUsageMetric metric, Guid userId, DateTime from, DateTime to)
    {
        return metric.UserId == userId
            && metric.PeriodStart >= from
            && metric.PeriodStart <= to;
    }
}
