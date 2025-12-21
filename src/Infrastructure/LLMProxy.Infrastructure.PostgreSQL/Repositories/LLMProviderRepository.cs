using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'entité LLMProvider.
/// </summary>
internal class LLMProviderRepository : RepositoryBase<Domain.Entities.LLMProvider>, ILLMProviderRepository
{
    public LLMProviderRepository(LLMProxyDbContext context) : base(context) { }

    public override async Task<Domain.Entities.LLMProvider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.LLMProviders
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.LLMProvider>> GetByTenantIdAsync(Guid tenantId, bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var query = _context.LLMProviders.Where(p => p.TenantId == tenantId);

        if (onlyActive)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.LLMProvider>> GetByRoutingStrategyAsync(Guid tenantId, Domain.Entities.RoutingMethod method, string value, CancellationToken cancellationToken = default)
    {
        return await _context.LLMProviders
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .ToListAsync(cancellationToken);
    }
}
