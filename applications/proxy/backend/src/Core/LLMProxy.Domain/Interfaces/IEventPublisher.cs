namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Interface pour la publication d'événements vers un message broker.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-040 Outbox Pattern.
/// </para>
/// <para>
/// Cette interface définit le contrat pour publier des événements vers un système
/// de messagerie externe (RabbitMQ, Azure Service Bus, Kafka, etc.).
/// </para>
/// <para>
/// Utilisée par l'OutboxProcessor pour publier les événements de manière asynchrone
/// après qu'ils aient été sauvegardés dans la table Outbox.
/// </para>
/// </remarks>
public interface IEventPublisher
{
    /// <summary>
    /// Publie un événement sur le message broker.
    /// </summary>
    /// <typeparam name="TEvent">Type de l'événement à publier.</typeparam>
    /// <param name="domainEvent">Événement à publier.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Tâche représentant l'opération asynchrone.</returns>
    /// <exception cref="ArgumentNullException">Si domainEvent est null.</exception>
    /// <exception cref="InvalidOperationException">Si la publication échoue.</exception>
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : class;
}
