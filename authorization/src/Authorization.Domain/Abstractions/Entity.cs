namespace Authorization.Domain.Abstractions;

/// <summary>
/// Classe de base pour toutes les entités du domaine.
/// Fournit une identité et la gestion des événements de domaine.
/// </summary>
/// <typeparam name="TId">Type de l'identifiant de l'entité.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Identifiant unique de l'entité.
    /// </summary>
    public TId Id { get; protected init; } = default!;

    /// <summary>
    /// Événements de domaine en attente de publication.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Ajoute un événement de domaine à publier.
    /// </summary>
    /// <param name="domainEvent">Événement à ajouter.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Supprime tous les événements de domaine après publication.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Détermine l'égalité basée sur l'identifiant.
    /// </summary>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Equals(entity);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }

    /// <summary>
    /// Opérateur d'égalité.
    /// </summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Opérateur d'inégalité.
    /// </summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}
