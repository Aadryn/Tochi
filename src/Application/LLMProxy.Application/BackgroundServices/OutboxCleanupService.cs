using LLMProxy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.BackgroundServices;

/// <summary>
/// Background service qui nettoie les messages Outbox traités depuis trop longtemps.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-040 Outbox Pattern.
/// </para>
/// <para>
/// Ce service s'exécute périodiquement pour supprimer les messages Outbox qui ont été
/// traités avec succès (ProcessedAt non null) et qui dépassent la période de rétention.
/// </para>
/// <para>
/// Cela évite que la table Outbox croisse indéfiniment et maintient de bonnes performances.
/// </para>
/// </remarks>
public sealed class OutboxCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxCleanupService> _logger;
    private readonly TimeSpan _retentionPeriod;
    private readonly TimeSpan _cleanupInterval;

    public OutboxCleanupService(
        IServiceProvider serviceProvider,
        ILogger<OutboxCleanupService> logger,
        TimeSpan? retentionPeriod = null,
        TimeSpan? cleanupInterval = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _retentionPeriod = retentionPeriod ?? TimeSpan.FromDays(7);
        _cleanupInterval = cleanupInterval ?? TimeSpan.FromHours(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox cleanup service started (retention: {RetentionDays} days, interval: {CleanupInterval})",
            _retentionPeriod.TotalDays,
            _cleanupInterval);

        // Attendre avant le premier cleanup (laisser le temps au système de démarrer)
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled error during outbox cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox cleanup service stopped");
    }

    /// <summary>
    /// Supprime les messages Outbox traités depuis plus de la période de rétention.
    /// </summary>
    private async Task CleanupOldMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DbContext>();

        var cutoffDate = DateTimeOffset.UtcNow - _retentionPeriod;

        _logger.LogDebug(
            "Starting outbox cleanup for messages processed before {CutoffDate}",
            cutoffDate);

        // Supprimer les messages traités ET anciens
        // ProcessedAt IS NOT NULL AND ProcessedAt < cutoffDate
        var deletedCount = await context.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt != null && m.ProcessedAt < cutoffDate)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount > 0)
        {
            _logger.LogInformation(
                "Cleaned up {DeletedCount} processed outbox messages older than {RetentionDays} days",
                deletedCount,
                _retentionPeriod.TotalDays);
        }
        else
        {
            _logger.LogDebug("No old outbox messages to clean up");
        }
    }
}
