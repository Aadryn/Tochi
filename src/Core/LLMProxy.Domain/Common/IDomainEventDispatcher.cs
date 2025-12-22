namespace LLMProxy.Domain.Common;

/// <summary>
/// Interface pour le dispatcher d'événements du domaine.
/// </summary>
/// <remarks>
/// Responsable de la publication des événements du domaine vers les handlers appropriés.
/// Conforme à l'ADR-025 Domain Events.
/// </remarks>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatche une collection d'événements du domaine vers leurs handlers respectifs.
    /// </summary>
    /// <param name="events">Les événements à dispatcher.</param>
    /// <param name="cancellationToken">Token d'annulation pour stopper l'opération.</param>
    /// <returns>Tâche asynchrone représentant l'opération de dispatch.</returns>
    /// <remarks>
    /// Les événements sont dispatchés de manière séquentielle dans l'ordre de la collection.
    /// En cas d'échec d'un handler, le dispatch continue pour les événements suivants.
    /// </remarks>
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
