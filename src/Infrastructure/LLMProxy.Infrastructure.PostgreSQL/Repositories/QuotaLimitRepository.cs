using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'entité QuotaLimit.
/// </summary>
internal class QuotaLimitRepository : RepositoryBase<Domain.Entities.QuotaLimit>, IQuotaLimitRepository
{
    public QuotaLimitRepository(LLMProxyDbContext context) : base(context) { }

    public override async Task<Domain.Entities.QuotaLimit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
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
}
