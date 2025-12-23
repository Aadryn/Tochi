using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'agrégat AuditLog avec support Result Pattern.
/// </summary>
internal class AuditLogRepository : RepositoryBase<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(LLMProxyDbContext context, ILogger<AuditLogRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<Result<IReadOnlyList<AuditLog>>> GetByTenantIdAsync(Guid tenantId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = DbSet.Where(a => a.TenantId == tenantId);
            if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);
            var logs = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
            Logger.LogDebug("Récupéré {Count} AuditLogs pour tenant {TenantId}", logs.Count, tenantId);
            return Result<IReadOnlyList<AuditLog>>.Success(logs.AsReadOnly());
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByTenantIdAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération des AuditLogs du tenant {TenantId}", tenantId); return Error.Database.AccessError("GetByTenantIdAsync", ex.Message); }
    }

    public async Task<Result<IReadOnlyList<AuditLog>>> GetByUserIdAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = DbSet.Where(a => a.UserId == userId);
            if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);
            var logs = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
            Logger.LogDebug("Récupéré {Count} AuditLogs pour user {UserId}", logs.Count, userId);
            return Result<IReadOnlyList<AuditLog>>.Success(logs.AsReadOnly());
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération GetByUserIdAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la récupération des AuditLogs de l'utilisateur {UserId}", userId); return Error.Database.AccessError("GetByUserIdAsync", ex.Message); }
    }

    public async Task<Result<int>> DeleteOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default)
    {
        try
        {
            var logsToDelete = await DbSet.Where(a => a.CreatedAt < threshold).ToListAsync(cancellationToken);
            DbSet.RemoveRange(logsToDelete);
            var count = logsToDelete.Count;
            Logger.LogInformation("Supprimé {Count} AuditLogs antérieurs à {Threshold}", count, threshold);
            return Result<int>.Success(count);
        }
        catch (OperationCanceledException) { Logger.LogInformation("Opération DeleteOlderThanAsync annulée"); throw; }
        catch (Exception ex) { Logger.LogError(ex, "Erreur lors de la suppression des AuditLogs antérieurs à {Threshold}", threshold); return Error.Database.AccessError("DeleteOlderThanAsync", ex.Message); }
    }
}
