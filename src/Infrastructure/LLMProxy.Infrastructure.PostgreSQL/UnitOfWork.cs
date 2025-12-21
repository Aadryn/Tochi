using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.PostgreSQL.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace LLMProxy.Infrastructure.PostgreSQL;

/// <summary>
/// Implémentation du pattern Unit of Work.
/// Centralise la gestion des transactions et des repositories.
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

