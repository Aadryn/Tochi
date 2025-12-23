using Authorization.Infrastructure.PostgreSQL.Data;
using Authorization.Infrastructure.PostgreSQL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository des logs d'audit avec EF Core et PostgreSQL.
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly AuthorizationAuditDbContext _context;
    private readonly ILogger<AuditLogRepository> _logger;

    /// <summary>
    /// Initialise une nouvelle instance du repository.
    /// </summary>
    /// <param name="context">Contexte de base de données.</param>
    /// <param name="logger">Logger.</param>
    public AuditLogRepository(
        AuthorizationAuditDbContext context,
        ILogger<AuditLogRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Audit log created: {OperationType} for {ResourceType}/{ResourceId} by {ActorId}",
            auditLog.OperationType,
            auditLog.ResourceType,
            auditLog.ResourceId,
            auditLog.ActorId);
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default)
    {
        var logsList = auditLogs.ToList();
        if (logsList.Count == 0)
        {
            return;
        }

        await _context.AuditLogs.AddRangeAsync(logsList, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Added {Count} audit log entries in batch", logsList.Count);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetByTenantAsync(
        string tenantId,
        DateTime? from = null,
        DateTime? to = null,
        AuditOperationType? operationType = null,
        string? actorId = null,
        AuditResult? result = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.TenantId == tenantId);

        if (from.HasValue)
        {
            query = query.Where(l => l.Timestamp >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(l => l.Timestamp <= to.Value);
        }

        if (operationType.HasValue)
        {
            query = query.Where(l => l.OperationType == operationType.Value);
        }

        if (!string.IsNullOrEmpty(actorId))
        {
            query = query.Where(l => l.ActorId == actorId);
        }

        if (result.HasValue)
        {
            query = query.Where(l => l.Result == result.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AuditLog>> GetByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.CorrelationId == correlationId)
            .OrderBy(l => l.Timestamp)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuditStatistics> GetStatisticsAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var logs = await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.TenantId == tenantId && l.Timestamp >= from && l.Timestamp <= to)
            .ToListAsync(cancellationToken);

        if (logs.Count == 0)
        {
            return new AuditStatistics();
        }

        var permissionChecks = logs.Where(l => l.OperationType == AuditOperationType.PermissionCheck).ToList();
        var granted = permissionChecks.Count(l => l.Result == AuditResult.Success);
        var denied = permissionChecks.Count(l => l.Result == AuditResult.Denied);
        var cacheHits = permissionChecks.Count(l => l.CacheHit);

        return new AuditStatistics
        {
            TotalOperations = logs.Count,
            PermissionChecks = permissionChecks.Count,
            PermissionsGranted = granted,
            PermissionsDenied = denied,
            RoleAssignments = logs.Count(l => l.OperationType == AuditOperationType.RoleAssigned),
            RoleRevocations = logs.Count(l => l.OperationType == AuditOperationType.RoleRevoked),
            Errors = logs.Count(l => l.Result == AuditResult.Error),
            AverageDurationMs = logs.Average(l => l.DurationMs),
            CacheHitRate = permissionChecks.Count > 0 ? (double)cacheHits / permissionChecks.Count * 100 : 0
        };
    }

    /// <inheritdoc />
    public async Task<int> PurgeOldLogsAsync(
        string tenantId,
        DateTime before,
        CancellationToken cancellationToken = default)
    {
        var count = await _context.AuditLogs
            .Where(l => l.TenantId == tenantId && l.Timestamp < before)
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogInformation(
            "Purged {Count} audit logs for tenant {TenantId} before {Before:O}",
            count,
            tenantId,
            before);

        return count;
    }
}
