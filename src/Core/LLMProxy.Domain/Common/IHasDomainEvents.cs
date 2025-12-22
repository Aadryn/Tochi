namespace LLMProxy.Domain.Common;

/// <summary>
/// Interface pour les entités qui peuvent émettre des événements du domaine.
/// </summary>
/// <remarks>
/// Conforme à l'ADR-025 Domain Events.
/// Permet de collecter les événements avant la persistance et de les dispatcher après.
/// </remarks>
public interface IHasDomainEvents
{
    /// <summary>
    /// Collection en lecture seule des événements du domaine levés par l'entité.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    
    /// <summary>
    /// Efface tous les événements du domaine de l'entité.
    /// </summary>
    /// <remarks>
    /// Appelé après la publication des événements pour éviter leur re-publication.
    /// </remarks>
    void ClearDomainEvents();
}
