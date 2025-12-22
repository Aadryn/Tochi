using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Extensions;
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
            .FirstOrDefaultAsync(t => t.Slug == slug.NormalizeSlug(), cancellationToken);
    }

    /// <summary>
    /// Récupère un tenant par son identifiant ou retourne le tenant par défaut.
    /// Conforme à ADR-026 (Null Object Pattern).
    /// </summary>
    /// <param name="id">Identifiant unique du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le tenant trouvé ou <see cref="NullTenant.Instance"/>.</returns>
    /// <remarks>
    /// Élimine les null checks dans le code appelant.
    /// Au lieu de vérifier <c>if (tenant == null)</c>, vérifier <c>if (!tenant.IsActive)</c>.
    /// </remarks>
    public async Task<Tenant> GetByIdOrDefaultAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return tenant ?? NullTenant.Instance;
    }

    /// <summary>
    /// Récupère un tenant par son slug ou retourne le tenant par défaut.
    /// Conforme à ADR-026 (Null Object Pattern).
    /// </summary>
    /// <param name="slug">Slug unique du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le tenant trouvé ou <see cref="NullTenant.Instance"/>.</returns>
    /// <remarks>
    /// Élimine les null checks dans le code appelant.
    /// Au lieu de vérifier <c>if (tenant == null)</c>, vérifier <c>if (!tenant.IsActive)</c>.
    /// </remarks>
    public async Task<Tenant> GetBySlugOrDefaultAsync(string slug, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug.NormalizeSlug(), cancellationToken);

        return tenant ?? NullTenant.Instance;
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
            .AnyAsync(t => t.Slug == slug.NormalizeSlug(), cancellationToken);
    }
}
