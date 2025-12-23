using LLMProxy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.BackgroundServices;

/// <summary>
/// Background service qui déplace les messages Outbox en échec vers la Dead Letter Queue.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-040 Outbox Pattern.
/// </para>
/// <para>
/// Ce service identifie les messages Outbox qui ont dépassé le nombre maximum de tentatives
/// de traitement (RetryCount >= MaxRetries) et les déplace vers la table OutboxDeadLetter
/// pour investigation manuelle.
/// </para>
/// <para>
/// Cela évite que l'OutboxProcessor reste bloqué sur des messages corrompus ou
/// impossibles à traiter.
/// </para>
/// </remarks>
public sealed class OutboxDeadLetterService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxDeadLetterService> _logger;
    private readonly int _maxRetries;
    private readonly TimeSpan _checkInterval;

    public OutboxDeadLetterService(
        IServiceProvider serviceProvider,
        ILogger<OutboxDeadLetterService> logger,
        int? maxRetries = null,
        TimeSpan? checkInterval = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _maxRetries = maxRetries ?? 3;
        _checkInterval = checkInterval ?? TimeSpan.FromMinutes(5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox dead letter service started (max retries: {MaxRetries}, check interval: {CheckInterval})",
            _maxRetries,
            _checkInterval);

        // Attendre avant la première vérification
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MoveToDeadLetterAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled error moving messages to dead letter");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox dead letter service stopped");
    }

    /// <summary>
    /// Déplace les messages en échec vers la Dead Letter Queue.
    /// </summary>
    private async Task MoveToDeadLetterAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DbContext>();

        // Messages qui ont dépassé le nombre max de retries
        // ProcessedAt IS NULL AND RetryCount >= MaxRetries
        var failedMessages = await context.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null && m.RetryCount >= _maxRetries)
            .ToListAsync(cancellationToken);

        if (!failedMessages.Any())
        {
            return; // Aucun message en échec à déplacer
        }

        _logger.LogWarning(
            "Found {FailedCount} messages to move to dead letter",
            failedMessages.Count);

        foreach (var message in failedMessages)
        {
            // Créer une entrée dead letter à partir du message échoué
            var deadLetter = new OutboxDeadLetter(message);

            context.Set<OutboxDeadLetter>().Add(deadLetter);
            context.Set<OutboxMessage>().Remove(message);

            _logger.LogWarning(
                "Moved message {MessageId} (type: {EventType}) to dead letter after {RetryCount} retries. Error: {Error}",
                message.Id,
                message.Type,
                message.RetryCount,
                message.Error);
        }

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Moved {Count} messages to dead letter queue",
            failedMessages.Count);
    }
}
