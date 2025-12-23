using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.Idp.Sync;
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authorization.Infrastructure.Idp.Jobs;

/// <summary>
/// Service d'arrière-plan pour la synchronisation batch des groupes IDP.
/// Exécute périodiquement la synchronisation selon l'expression cron configurée.
/// </summary>
public class IdpSyncJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IdpSyncJob> _logger;
    private readonly IdpConfiguration _configuration;
    private readonly CronExpression _cronExpression;

    /// <summary>
    /// Initialise une nouvelle instance du job de synchronisation.
    /// </summary>
    public IdpSyncJob(
        IServiceProvider serviceProvider,
        ILogger<IdpSyncJob> logger,
        IOptions<IdpConfiguration> configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration.Value;

        _cronExpression = CronExpression.Parse(_configuration.BatchSyncCronExpression);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.EnableBatchSync)
        {
            _logger.LogInformation("Batch sync is disabled, IdpSyncJob will not run");
            return;
        }

        _logger.LogInformation(
            "IdpSyncJob started with cron expression: {CronExpression}",
            _configuration.BatchSyncCronExpression);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nextOccurrence = _cronExpression.GetNextOccurrence(
                    DateTime.UtcNow,
                    TimeZoneInfo.Utc);

                if (nextOccurrence is null)
                {
                    _logger.LogWarning("Could not calculate next occurrence, waiting 1 minute");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                var delay = nextOccurrence.Value - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    _logger.LogDebug(
                        "Next batch sync scheduled for {NextRun} (in {Delay})",
                        nextOccurrence.Value,
                        delay);

                    await Task.Delay(delay, stoppingToken);
                }

                await ExecuteBatchSyncAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("IdpSyncJob is shutting down");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IdpSyncJob, will retry at next scheduled time");
                // Attendre un peu avant de réessayer pour éviter une boucle rapide en cas d'erreur
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Exécute la synchronisation batch pour tous les tenants.
    /// </summary>
    private async Task ExecuteBatchSyncAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting batch sync for all tenants");
        var startTime = DateTime.UtcNow;

        using var scope = _serviceProvider.CreateScope();
        var syncService = scope.ServiceProvider.GetRequiredService<IIdpSyncService>();
        var tenantProvider = scope.ServiceProvider.GetService<ITenantProvider>();

        // Récupérer la liste des tenants actifs
        var tenants = await GetActiveTenantsAsync(tenantProvider, cancellationToken);

        var totalResults = new List<BatchSyncResult>();
        var errors = new List<string>();

        foreach (var tenantId in tenants)
        {
            try
            {
                _logger.LogDebug("Starting batch sync for tenant {TenantId}", tenantId);

                var result = await syncService.SyncAllAsync(tenantId, cancellationToken);
                totalResults.Add(result);

                if (result.Errors.Count > 0)
                {
                    errors.AddRange(result.Errors.Select(e => $"[{tenantId}] {e}"));
                }

                _logger.LogInformation(
                    "Batch sync completed for tenant {TenantId}: {Groups} groups, {Users} users, +{Added} -{Removed} relations",
                    tenantId,
                    result.TotalGroups,
                    result.TotalUsers,
                    result.TotalRelationsAdded,
                    result.TotalRelationsRemoved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch sync for tenant {TenantId}", tenantId);
                errors.Add($"[{tenantId}] {ex.Message}");
            }
        }

        var duration = DateTime.UtcNow - startTime;
        var totalGroups = totalResults.Sum(r => r.TotalGroups);
        var totalUsers = totalResults.Sum(r => r.TotalUsers);
        var totalAdded = totalResults.Sum(r => r.TotalRelationsAdded);
        var totalRemoved = totalResults.Sum(r => r.TotalRelationsRemoved);

        _logger.LogInformation(
            "Batch sync completed in {Duration}: {Tenants} tenants, {Groups} groups, {Users} users, +{Added} -{Removed} relations, {Errors} errors",
            duration,
            tenants.Count,
            totalGroups,
            totalUsers,
            totalAdded,
            totalRemoved,
            errors.Count);
    }

    /// <summary>
    /// Récupère la liste des tenants actifs.
    /// </summary>
    private async Task<IReadOnlyList<TenantId>> GetActiveTenantsAsync(
        ITenantProvider? tenantProvider,
        CancellationToken cancellationToken)
    {
        if (tenantProvider is not null)
        {
            return await tenantProvider.GetActiveTenantsAsync(cancellationToken);
        }

        // Fallback: retourner un tenant par défaut si pas de provider
        _logger.LogWarning("No ITenantProvider registered, using default tenant");
        return new[] { TenantId.Create("default") };
    }
}

/// <summary>
/// Interface pour récupérer les tenants actifs.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Récupère la liste des tenants actifs pour la synchronisation.
    /// </summary>
    Task<IReadOnlyList<TenantId>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);
}
