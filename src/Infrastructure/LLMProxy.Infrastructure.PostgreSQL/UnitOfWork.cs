using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.PostgreSQL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LLMProxy.Infrastructure.PostgreSQL;

/// <summary>
/// Unit of Work implementation (Adapter)
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly LLMProxyDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy initialization of repositories
    private ITenantRepository? _tenants;
    private IUserRepository? _users;
    private IApiKeyRepository? _apiKeys;
    private ILLMProviderRepository? _providers;
    private IQuotaLimitRepository? _quotaLimits;
    private IAuditLogRepository? _auditLogs;
    private ITokenUsageMetricRepository? _tokenMetrics;

    public UnitOfWork(LLMProxyDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public ITenantRepository Tenants => _tenants ??= new TenantRepository(_context);

    public IUserRepository Users => _users ??= new UserRepository(_context);

    public IApiKeyRepository ApiKeys => _apiKeys ??= new ApiKeyRepository(_context);

    public ILLMProviderRepository Providers => _providers ??= new LLMProviderRepository(_context);

    public IQuotaLimitRepository QuotaLimits => _quotaLimits ??= new QuotaLimitRepository(_context);

    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

    public ITokenUsageMetricRepository TokenMetrics => _tokenMetrics ??= new TokenUsageMetricRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction to commit.");

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

// UserRepository implementation
internal class UserRepository : IUserRepository
{
    private readonly LLMProxyDbContext _context;
    public UserRepository(LLMProxyDbContext context) => _context = context;
    
    public async Task<Domain.Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.ApiKeys)
            .Include(u => u.QuotaLimits)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Domain.Entities.User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.User>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.TenantId == tenantId && u.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(Domain.Entities.User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }
}

internal class ApiKeyRepository : IApiKeyRepository
{
    private readonly LLMProxyDbContext _context;
    public ApiKeyRepository(LLMProxyDbContext context) => _context = context;
    
    public async Task<Domain.Entities.ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
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

    public async Task AddAsync(Domain.Entities.ApiKey apiKey, CancellationToken cancellationToken = default)
    {
        await _context.ApiKeys.AddAsync(apiKey, cancellationToken);
    }

    public Task UpdateAsync(Domain.Entities.ApiKey apiKey, CancellationToken cancellationToken = default)
    {
        _context.ApiKeys.Update(apiKey);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var apiKey = await GetByIdAsync(id, cancellationToken);
        if (apiKey != null)
        {
            _context.ApiKeys.Remove(apiKey);
        }
    }
}

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

internal class AuditLogRepository : IAuditLogRepository
{
    private readonly LLMProxyDbContext _context;
    public AuditLogRepository(LLMProxyDbContext context) => _context = context;
    
    public async Task<Domain.Entities.AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.AuditLog>> GetByTenantIdAsync(Guid tenantId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.Where(a => a.TenantId == tenantId);
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);
        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.AuditLog>> GetByUserIdAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.Where(a => a.UserId == userId);
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);
        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task<int> DeleteOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default)
    {
        var logsToDelete = await _context.AuditLogs.Where(a => a.CreatedAt < threshold).ToListAsync(cancellationToken);
        _context.AuditLogs.RemoveRange(logsToDelete);
        return logsToDelete.Count;
    }
}

internal class TokenUsageMetricRepository : ITokenUsageMetricRepository
{
    private readonly LLMProxyDbContext _context;
    public TokenUsageMetricRepository(LLMProxyDbContext context) => _context = context;
    
    public async Task<Domain.Entities.TokenUsageMetric?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TokenUsageMetrics.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<Domain.Entities.TokenUsageMetric?> GetByPeriodAsync(Guid tenantId, Guid? userId, Guid? providerId, DateTime periodStart, Domain.Entities.MetricPeriod period, CancellationToken cancellationToken = default)
    {
        return await _context.TokenUsageMetrics.FirstOrDefaultAsync(m => m.TenantId == tenantId && m.UserId == userId && m.ProviderId == providerId && m.PeriodStart == periodStart && m.Period == period, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.TokenUsageMetric>> GetByTenantIdAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.TokenUsageMetrics.Where(m => m.TenantId == tenantId && m.PeriodStart >= from && m.PeriodStart <= to).OrderBy(m => m.PeriodStart).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.TokenUsageMetric>> GetByUserIdAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.TokenUsageMetrics.Where(m => m.UserId == userId && m.PeriodStart >= from && m.PeriodStart <= to).OrderBy(m => m.PeriodStart).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.TokenUsageMetric metric, CancellationToken cancellationToken = default)
    {
        await _context.TokenUsageMetrics.AddAsync(metric, cancellationToken);
    }

    public Task UpdateAsync(Domain.Entities.TokenUsageMetric metric, CancellationToken cancellationToken = default)
    {
        _context.TokenUsageMetrics.Update(metric);
        return Task.CompletedTask;
    }
}
