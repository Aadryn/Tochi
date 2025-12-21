using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'entité AuditLog.
/// </summary>
internal class AuditLogRepository : IAuditLogRepository
{
    private readonly LLMProxyDbContext _context;

    public AuditLogRepository(LLMProxyDbContext context) => _context = context;

    public async Task<Domain.Entities.AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.AuditLog>> GetByTenantIdAsync(Guid tenantId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.Where(a => a.TenantId == tenantId);
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);
        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.AuditLog>> GetByUserIdAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.Where(a => a.UserId == userId);
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);
        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task<int> DeleteOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default)
    {
        var logsToDelete = await _context.AuditLogs.Where(a => a.CreatedAt < threshold).ToListAsync(cancellationToken);
        _context.AuditLogs.RemoveRange(logsToDelete);
        return logsToDelete.Count;
    }
}
