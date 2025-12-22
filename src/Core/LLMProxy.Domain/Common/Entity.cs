using System.Diagnostics;

namespace LLMProxy.Domain.Common;

/// <summary>
/// Classe de base pour toutes les entités suivant les principes DDD.
/// </summary>
/// <remarks>
/// Conforme à l'ADR-025 : supporte les Domain Events via IHasDomainEvents.
/// Les entités peuvent lever des événements qui seront publiés après la persistance.
/// </remarks>
public abstract class Entity : IHasDomainEvents
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        
        // Invariants : Chaque entité doit avoir un Id unique et une date de création
        Debug.Assert(Id != Guid.Empty, "Entity Id must not be empty after construction");
        Debug.Assert(CreatedAt != default, "Entity CreatedAt must be set after construction");
        Debug.Assert(CreatedAt <= DateTime.UtcNow, "Entity CreatedAt must not be in the future");
    }

    /// <summary>
    /// Ajoute un événement du domaine à la collection interne.
    /// </summary>
    /// <param name="domainEvent">L'événement du domaine à ajouter.</param>
    /// <remarks>
    /// Les événements seront publiés après la persistance de l'entité.
    /// Conforme à l'ADR-025 Domain Events.
    /// </remarks>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        Debug.Assert(domainEvent != null, "Domain event must not be null");
        Debug.Assert(_domainEvents != null, "Domain events collection must not be null");
        
        _domainEvents.Add(domainEvent);
        
        Debug.Assert(_domainEvents.Contains(domainEvent), "Domain event must be added to collection");
    }

    /// <summary>
    /// Marque l'entité comme modifiée en mettant à jour la date de modification.
    /// </summary>
    /// <remarks>
    /// Centralise la mise à jour de UpdatedAt pour éviter la duplication (ADR-003 DRY).
    /// </remarks>
    protected void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Efface tous les événements du domaine après leur publication.
    /// </summary>
    /// <remarks>
    /// Appelé par le UnitOfWork après la publication des événements.
    /// Conforme à l'ADR-025 Domain Events.
    /// </remarks>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        Debug.Assert(Id != Guid.Empty, "Entity Id must not be empty when computing hash code");
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}
