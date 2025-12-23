namespace Authorization.Domain.Abstractions;

/// <summary>
/// Interface marqueur pour les événements de domaine.
/// Les événements de domaine capturent les faits importants survenus dans le domaine.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Timestamp de l'événement (UTC).
    /// </summary>
    DateTimeOffset OccurredAt => DateTimeOffset.UtcNow;
}
