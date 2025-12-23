using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LLMProxy.Application.BackgroundServices;

/// <summary>
/// Background service qui traite les messages Outbox en les publiant sur le message broker.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-040 Outbox Pattern.
/// </para>
/// <para>
/// Ce service s'exécute en arrière-plan et interroge périodiquement la table Outbox
/// pour récupérer les messages non traités, les publier sur le message broker,
/// et marquer ceux qui ont réussi comme traités.
/// </para>
/// <para>
/// En cas d'échec, le message est marqué avec l'erreur et RetryCount est incrémenté.
/// </para>
/// </remarks>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _pollingInterval;
    private readonly int _batchSize;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger,
        TimeSpan? pollingInterval = null,
        int? batchSize = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(5);
        _batchSize = batchSize ?? 100;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox processor started (polling interval: {PollingInterval}, batch size: {BatchSize})",
            _pollingInterval,
            _batchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled error in outbox processor");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox processor stopped");
    }

    /// <summary>
    /// Traite un batch de messages Outbox non traités.
    /// </summary>
    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        // Récupérer les messages non traités (ProcessedAt IS NULL)
        var messages = await context.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(_batchSize)
            .ToListAsync(cancellationToken);

        if (!messages.Any())
        {
            return; // Aucun message à traiter
        }

        _logger.LogDebug(
            "Processing {MessageCount} outbox messages",
            messages.Count);

        var successCount = 0;
        var failureCount = 0;

        foreach (var message in messages)
        {
            try
            {
                // Désérialiser l'événement
                var eventType = Type.GetType(message.Type);
                if (eventType is null)
                {
                    _logger.LogWarning(
                        "Unknown event type: {EventType} for message {MessageId}",
                        message.Type,
                        message.Id);

                    message.MarkAsFailed($"Unknown event type: {message.Type}");
                    failureCount++;
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(
                    message.Content,
                    eventType,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                if (domainEvent is null)
                {
                    _logger.LogWarning(
                        "Failed to deserialize event for message {MessageId}",
                        message.Id);

                    message.MarkAsFailed("Failed to deserialize event");
                    failureCount++;
                    continue;
                }

                // Publier sur le message broker
                await publisher.PublishAsync(domainEvent, cancellationToken);

                // Marquer comme traité
                message.MarkAsProcessed();
                successCount++;

                _logger.LogDebug(
                    "Published outbox message {MessageId} of type {EventType}",
                    message.Id,
                    eventType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process outbox message {MessageId}",
                    message.Id);

                message.MarkAsFailed(ex.Message);
                failureCount++;
            }
        }

        // Sauvegarder les mises à jour (ProcessedAt, Error, RetryCount)
        await context.SaveChangesAsync(cancellationToken);

        if (successCount > 0 || failureCount > 0)
        {
            _logger.LogInformation(
                "Outbox batch processed: {SuccessCount} succeeded, {FailureCount} failed",
                successCount,
                failureCount);
        }
    }
}
