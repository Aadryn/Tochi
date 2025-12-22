using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.PostgreSQL.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL;

/// <summary>
/// Implémentation du pattern Unit of Work.
/// Centralise la gestion des transactions et des repositories.
/// </summary>
/// <remarks>
/// Conforme à l'ADR-029 (Unit of Work Pattern) et ADR-025 (Domain Events).
/// Dispatche automatiquement les Domain Events après SaveChanges.
/// </remarks>
public class UnitOfWork : IUnitOfWork
{
    private readonly LLMProxyDbContext _context;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private IDbContextTransaction? _transaction;

    // Lazy initialization of repositories
    private ITenantRepository? _tenants;
    private IUserRepository? _users;
    private IApiKeyRepository? _apiKeys;
    private ILLMProviderRepository? _providers;
    private IQuotaLimitRepository? _quotaLimits;
    private IAuditLogRepository? _auditLogs;
    private ITokenUsageMetricRepository? _tokenMetrics;

    /// <summary>
    /// Initialise une nouvelle instance du Unit of Work.
    /// </summary>
    /// <param name="context">Le contexte de base de données.</param>
    /// <param name="loggerFactory">Factory pour créer les loggers.</param>
    /// <param name="eventDispatcher">Dispatcher pour les événements du domaine.</param>
    public UnitOfWork(
        LLMProxyDbContext context, 
        ILoggerFactory loggerFactory,
        IDomainEventDispatcher eventDispatcher)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    }

    public ITenantRepository Tenants => _tenants ??= new TenantRepository(_context, _loggerFactory.CreateLogger<TenantRepository>());

    public IUserRepository Users => _users ??= new UserRepository(_context, _loggerFactory.CreateLogger<UserRepository>());

    public IApiKeyRepository ApiKeys => _apiKeys ??= new ApiKeyRepository(_context, _loggerFactory.CreateLogger<ApiKeyRepository>());

    public ILLMProviderRepository Providers => _providers ??= new LLMProviderRepository(_context, _loggerFactory.CreateLogger<LLMProviderRepository>());

    public IQuotaLimitRepository QuotaLimits => _quotaLimits ??= new QuotaLimitRepository(_context, _loggerFactory.CreateLogger<QuotaLimitRepository>());

    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context, _loggerFactory.CreateLogger<AuditLogRepository>());

    public ITokenUsageMetricRepository TokenMetrics => _tokenMetrics ??= new TokenUsageMetricRepository(_context, _loggerFactory.CreateLogger<TokenUsageMetricRepository>());

    /// <summary>
    /// Sauvegarde les changements et dispatche les événements du domaine.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le nombre d'entités sauvegardées.</returns>
    /// <remarks>
    /// Conforme à l'ADR-025 : collecte les événements AVANT SaveChanges,
    /// sauvegarde les entités, puis dispatche les événements APRÈS commit.
    /// Cela garantit que les événements ne sont publiés que si la persistance réussit.
    /// </remarks>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Collecter les événements du domaine AVANT SaveChanges
        var domainEvents = _context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // 2. Sauvegarder les changements dans la base de données
        var result = await _context.SaveChangesAsync(cancellationToken);

        // 3. Clear les événements des entités
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        // 4. Dispatcher les événements APRÈS commit réussi
        if (domainEvents.Any())
        {
            await _eventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
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

