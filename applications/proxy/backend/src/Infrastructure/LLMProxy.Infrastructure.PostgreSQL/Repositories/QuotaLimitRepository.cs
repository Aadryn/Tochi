using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'agrégat QuotaLimit avec support Result Pattern.
/// </summary>
internal class QuotaLimitRepository : RepositoryBase<QuotaLimit>, IQuotaLimitRepository
{
    public QuotaLimitRepository(LLMProxyDbContext context, ILogger<QuotaLimitRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<Result<IReadOnlyList<QuotaLimit>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var quotas = await DbSet.Where(q => q.UserId == userId).ToListAsync(cancellationToken);
            Logger.LogDebug("Récupéré {Count} QuotaLimits pour user {UserId}", quotas.Count, userId);
            return Result<IReadOnlyList<QuotaLimit>>.Success(quotas.AsReadOnly());
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByUserIdAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération des QuotaLimits de l'utilisateur {UserId}", userId); return Error.Database.AccessError("GetByUserIdAsync", ex.Message); }
    }

    public async Task<Result<QuotaLimit>> GetByUserAndTypeAsync(Guid userId, QuotaType quotaType, QuotaPeriod period, CancellationToken cancellationToken = default)
    {
        try
        {
            var quota = await DbSet.FirstOrDefaultAsync(q => q.UserId == userId && q.QuotaType == quotaType && q.Period == period, cancellationToken);
            if (quota is null) 
            {
                Logger.LogWarning("QuotaLimit non trouvé pour user {UserId}, type {Type}, période {Period}", userId, quotaType, period);
                return Error.Database.EntityNotFound("QuotaLimit", Guid.Empty);
            }
            return Result<QuotaLimit>.Success(quota);
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByUserAndTypeAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération du QuotaLimit pour user {UserId}", userId); return Error.Database.AccessError("GetByUserAndTypeAsync", ex.Message); }
    }
}
