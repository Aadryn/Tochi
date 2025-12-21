using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Extensions;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Implémentation du repository pour l'entité User.
/// </summary>
internal class UserRepository : RepositoryBase<Domain.Entities.User>, IUserRepository
{
    public UserRepository(LLMProxyDbContext context) : base(context) { }

    public override async Task<Domain.Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.ApiKeys)
            .Include(u => u.QuotaLimits)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Domain.Entities.User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email.NormalizeEmail(), cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.User>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    public override async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.TenantId == tenantId && u.Email == email.NormalizeEmail(), cancellationToken);
    }
}
