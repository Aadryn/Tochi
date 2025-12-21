namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern for transaction management (Port)
/// </summary>
public interface IUnitOfWork : IDisposable
{
    ITenantRepository Tenants { get; }
    IUserRepository Users { get; }
    IApiKeyRepository ApiKeys { get; }
    ILLMProviderRepository Providers { get; }
    IQuotaLimitRepository QuotaLimits { get; }
    IAuditLogRepository AuditLogs { get; }
    ITokenUsageMetricRepository TokenMetrics { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
