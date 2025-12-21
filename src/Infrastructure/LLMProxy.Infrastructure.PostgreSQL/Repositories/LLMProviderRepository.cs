using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'entité LLMProvider.
/// </summary>
internal class LLMProviderRepository : ILLMProviderRepository
{
    private readonly LLMProxyDbContext _context;

    public LLMProviderRepository(LLMProxyDbContext context) => _context = context;

    public async Task<Domain.Entities.LLMProvider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
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

    public async Task AddAsync(Domain.Entities.LLMProvider provider, CancellationToken cancellationToken = default)
    {
        await _context.LLMProviders.AddAsync(provider, cancellationToken);
    }

    public Task UpdateAsync(Domain.Entities.LLMProvider provider, CancellationToken cancellationToken = default)
    {
        _context.LLMProviders.Update(provider);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await GetByIdAsync(id, cancellationToken);
        if (provider != null)
        {
            _context.LLMProviders.Remove(provider);
        }
    }
}
