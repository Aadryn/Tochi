using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'entité QuotaLimit.
/// </summary>
internal class QuotaLimitRepository : IQuotaLimitRepository
{
    private readonly LLMProxyDbContext _context;

    public QuotaLimitRepository(LLMProxyDbContext context) => _context = context;

    public async Task<Domain.Entities.QuotaLimit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.QuotaLimits.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.QuotaLimit>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.QuotaLimits.Where(q => q.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<Domain.Entities.QuotaLimit?> GetByUserAndTypeAsync(Guid userId, Domain.Entities.QuotaType quotaType, Domain.Entities.QuotaPeriod period, CancellationToken cancellationToken = default)
    {
        return await _context.QuotaLimits.FirstOrDefaultAsync(q => q.UserId == userId && q.QuotaType == quotaType && q.Period == period, cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.QuotaLimit quotaLimit, CancellationToken cancellationToken = default)
    {
        await _context.QuotaLimits.AddAsync(quotaLimit, cancellationToken);
    }

    public Task UpdateAsync(Domain.Entities.QuotaLimit quotaLimit, CancellationToken cancellationToken = default)
    {
        _context.QuotaLimits.Update(quotaLimit);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quotaLimit = await GetByIdAsync(id, cancellationToken);
        if (quotaLimit != null)
        {
            _context.QuotaLimits.Remove(quotaLimit);
        }
    }
}
