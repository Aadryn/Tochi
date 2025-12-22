using LLMProxy.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Common.EventPublishing;

/// <summary>
/// Implémentation de IEventPublisher utilisant MediatR.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-040 Outbox Pattern.
/// </para>
/// <para>
/// Cette implémentation utilise MediatR pour publier les événements.
/// En production, vous pouvez remplacer cette implémentation par une qui
/// publie vers RabbitMQ, Azure Service Bus, Kafka, etc.
/// </para>
/// <para>
/// MediatR est utilisé ici pour la cohérence avec le MediatRDomainEventDispatcher.
/// </para>
/// </remarks>
public sealed class MediatREventPublisher : IEventPublisher
{
    private readonly IMediator _mediator;
    private readonly ILogger<MediatREventPublisher> _logger;

    public MediatREventPublisher(
        IMediator mediator,
        ILogger<MediatREventPublisher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Publie un événement via MediatR.
    /// </summary>
    public async Task PublishAsync<TEvent>(
        TEvent domainEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var eventType = domainEvent.GetType();

        _logger.LogDebug(
            "Publishing event {EventType} via MediatR",
            eventType.Name);

        try
        {
            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation(
                "Successfully published event {EventType}",
                eventType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish event {EventType}",
                eventType.Name);

            throw;
        }
    }
}
