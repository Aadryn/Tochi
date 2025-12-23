namespace LLMProxy.Domain.Common;

/// <summary>
/// Interface marqueur pour les événements du domaine.
/// </summary>
/// <remarks>
/// Les Domain Events représentent des faits métier importants qui se sont produits.
/// Conformément à l'ADR-025, ils permettent le découplage entre composants.
/// </remarks>
public interface IDomainEvent
{
    /// <summary>
/// Identifiant unique de l'événement.
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// Date et heure UTC de l'occurrence de l'événement.
    /// </summary>
    DateTime OccurredAt { get; }
}
