using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'entité ApiKey.
/// </summary>
internal class ApiKeyRepository : RepositoryBase<Domain.Entities.ApiKey>, IApiKeyRepository
{
    public ApiKeyRepository(LLMProxyDbContext context) : base(context) { }

    public override async Task<Domain.Entities.ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.Id == id, cancellationToken);
    }

    public async Task<Domain.Entities.ApiKey?> GetByKeyHashAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.KeyHash == keyHash, cancellationToken);
    }

    public async Task<Domain.Entities.ApiKey?> GetByKeyPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.KeyPrefix == keyPrefix, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.ApiKey>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.ApiKey>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .Where(k => k.TenantId == tenantId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
