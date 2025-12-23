namespace LLMProxy.Domain.Common;

/// <summary>
/// Classe de base pour tous les événements du domaine.
/// </summary>
/// <remarks>
/// Implémente IDomainEvent avec des valeurs par défaut pour EventId et OccurredAt.
/// Utilise un record pour bénéficier de l'immutabilité et de l'égalité structurelle.
/// Conforme à l'ADR-025 Domain Events.
/// </remarks>
public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    /// Identifiant unique de l'événement, généré automatiquement.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// Date et heure UTC de l'occurrence de l'événement, initialisée automatiquement.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
