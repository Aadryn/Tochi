using LLMProxy.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Common;

/// <summary>
/// Dispatcher d'événements du domaine utilisant MediatR.
/// </summary>
/// <remarks>
/// Publie les événements via MediatR qui les route vers les handlers appropriés.
/// Conforme à l'ADR-025 Domain Events.
/// Implémentation thread-safe et asynchrone.
/// </remarks>
public sealed class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<MediatRDomainEventDispatcher> _logger;

    /// <summary>
    /// Initialise une nouvelle instance du dispatcher.
    /// </summary>
    /// <param name="mediator">Instance MediatR pour la publication.</param>
    /// <param name="logger">Logger pour tracer les événements.</param>
    public MediatRDomainEventDispatcher(
        IMediator mediator,
        ILogger<MediatRDomainEventDispatcher> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Dispatche les événements du domaine via MediatR.
    /// </summary>
    /// <param name="events">Collection d'événements à dispatcher.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    /// <remarks>
    /// Les événements sont publiés séquentiellement.
    /// Les exceptions des handlers sont loguées mais ne bloquent pas les autres handlers.
    /// </remarks>
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> events, 
        CancellationToken cancellationToken = default)
    {
        if (events == null)
        {
            _logger.LogWarning("DispatchAsync appelé avec events null");
            return;
        }

        var eventsList = events.ToList();
        
        if (eventsList.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "Dispatching {EventCount} domain event(s)", 
            eventsList.Count);

        foreach (var domainEvent in eventsList)
        {
            try
            {
                _logger.LogDebug(
                    "Dispatching {EventType} - EventId: {EventId}, OccurredAt: {OccurredAt}",
                    domainEvent.GetType().Name,
                    domainEvent.EventId,
                    domainEvent.OccurredAt);

                await _mediator.Publish(domainEvent, cancellationToken);

                _logger.LogDebug(
                    "Successfully dispatched {EventType} - EventId: {EventId}",
                    domainEvent.GetType().Name,
                    domainEvent.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erreur lors du dispatch de {EventType} - EventId: {EventId}. Le dispatch continue.",
                    domainEvent.GetType().Name,
                    domainEvent.EventId);
                
                // Continue le dispatch des autres événements
                // Conforme à la remarque de l'ADR : les handlers ne doivent pas bloquer
            }
        }

        _logger.LogInformation(
            "Dispatched {EventCount} domain event(s)", 
            eventsList.Count);
    }
}
