using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implementation of ITenantRepository (Adapter)
/// </summary>
public class TenantRepository : RepositoryBase<Tenant>, ITenantRepository
{
    public TenantRepository(LLMProxyDbContext context) : base(context) { }

    public override async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug.ToLowerInvariant(), cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Tenants.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public override async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .AnyAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .AnyAsync(t => t.Slug == slug.ToLowerInvariant(), cancellationToken);
    }
}
