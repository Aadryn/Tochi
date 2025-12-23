namespace Authorization.Domain.ValueObjects;

/// <summary>
/// Représente l'identifiant unique d'une assignation de rôle.
/// </summary>
public readonly record struct RoleAssignmentId : IEquatable<RoleAssignmentId>
{
    /// <summary>
    /// Valeur du GUID de l'assignation.
    /// </summary>
    public Guid Value { get; }

    private RoleAssignmentId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("RoleAssignmentId ne peut pas être un GUID vide.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Crée un nouveau RoleAssignmentId.
    /// </summary>
    /// <returns>Nouvelle instance avec un GUID généré.</returns>
    public static RoleAssignmentId New() => new(Guid.NewGuid());

    /// <summary>
    /// Crée un RoleAssignmentId à partir d'un GUID existant.
    /// </summary>
    /// <param name="id">GUID de l'assignation.</param>
    /// <returns>Instance de RoleAssignmentId.</returns>
    public static RoleAssignmentId Create(Guid id) => new(id);

    /// <summary>
    /// Parse une chaîne en RoleAssignmentId.
    /// </summary>
    /// <param name="value">Chaîne représentant un GUID.</param>
    /// <returns>Instance de RoleAssignmentId.</returns>
    public static RoleAssignmentId Parse(string value)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            throw new ArgumentException($"Format RoleAssignmentId invalide : {value}", nameof(value));
        }

        return new RoleAssignmentId(guid);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Conversion implicite vers Guid.
    /// </summary>
    public static implicit operator Guid(RoleAssignmentId id) => id.Value;
}
