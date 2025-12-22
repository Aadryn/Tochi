using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.PostgreSQL.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL;

/// <summary>
/// Implémentation du pattern Unit of Work.
/// Centralise la gestion des transactions et des repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly LLMProxyDbContext _context;
    private readonly ILoggerFactory _loggerFactory;
    private IDbContextTransaction? _transaction;

    // Lazy initialization of repositories
    private ITenantRepository? _tenants;
    private IUserRepository? _users;
    private IApiKeyRepository? _apiKeys;
    private ILLMProviderRepository? _providers;
    private IQuotaLimitRepository? _quotaLimits;
    private IAuditLogRepository? _auditLogs;
    private ITokenUsageMetricRepository? _tokenMetrics;

    public UnitOfWork(LLMProxyDbContext context, ILoggerFactory loggerFactory)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public ITenantRepository Tenants => _tenants ??= new TenantRepository(_context, _loggerFactory.CreateLogger<TenantRepository>());

    public IUserRepository Users => _users ??= new UserRepository(_context, _loggerFactory.CreateLogger<UserRepository>());

    public IApiKeyRepository ApiKeys => _apiKeys ??= new ApiKeyRepository(_context, _loggerFactory.CreateLogger<ApiKeyRepository>());

    public ILLMProviderRepository Providers => _providers ??= new LLMProviderRepository(_context, _loggerFactory.CreateLogger<LLMProviderRepository>());

    public IQuotaLimitRepository QuotaLimits => _quotaLimits ??= new QuotaLimitRepository(_context, _loggerFactory.CreateLogger<QuotaLimitRepository>());

    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context, _loggerFactory.CreateLogger<AuditLogRepository>());

    public ITokenUsageMetricRepository TokenMetrics => _tokenMetrics ??= new TokenUsageMetricRepository(_context, _loggerFactory.CreateLogger<TokenUsageMetricRepository>());

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

