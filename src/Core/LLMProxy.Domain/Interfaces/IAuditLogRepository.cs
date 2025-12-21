using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository interface for AuditLog aggregate (Port)
/// </summary>
public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByTenantIdAsync(Guid tenantId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<int> DeleteOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default);
}
