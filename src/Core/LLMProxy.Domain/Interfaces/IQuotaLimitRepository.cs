using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository interface for QuotaLimit aggregate (Port)
/// </summary>
public interface IQuotaLimitRepository
{
    Task<QuotaLimit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<QuotaLimit>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<QuotaLimit?> GetByUserAndTypeAsync(Guid userId, QuotaType quotaType, QuotaPeriod period, CancellationToken cancellationToken = default);
    Task AddAsync(QuotaLimit quotaLimit, CancellationToken cancellationToken = default);
    Task UpdateAsync(QuotaLimit quotaLimit, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
