namespace LLMProxy.Domain.Common;

/// <summary>
/// Interface pour les handlers d'événements du domaine.
/// </summary>
/// <typeparam name="TEvent">Type d'événement du domaine traité par ce handler.</typeparam>
/// <remarks>
/// Les handlers d'événements implémentent la logique métier réactive aux changements du domaine.
/// Conforme à l'ADR-025 Domain Events.
/// Un handler doit avoir une seule responsabilité (SRP - ADR-005).
/// </remarks>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Traite l'événement du domaine de manière asynchrone.
    /// </summary>
    /// <param name="domainEvent">L'événement du domaine à traiter.</param>
    /// <param name="cancellationToken">Token d'annulation pour stopper l'opération.</param>
    /// <returns>Tâche asynchrone représentant le traitement de l'événement.</returns>
    /// <remarks>
    /// Le handler ne doit PAS modifier l'état de l'événement (immutable).
    /// Les exceptions levées sont loguées mais ne bloquent pas les autres handlers.
    /// </remarks>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
