namespace Authorization.Domain.ValueObjects;

/// <summary>
/// Représente l'identifiant unique d'un rôle.
/// </summary>
/// <remarks>
/// <para>
/// Les rôles de base utilisent des identifiants prédéfinis (owner, contributor, reader).
/// Les rôles personnalisés utilisent des GUIDs générés par le système.
/// </para>
/// </remarks>
public readonly record struct RoleId : IEquatable<RoleId>
{
    /// <summary>
    /// Valeur de l'identifiant du rôle.
    /// </summary>
    public string Value { get; }

    private RoleId(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Crée un RoleId à partir d'une chaîne.
    /// </summary>
    /// <param name="value">Valeur de l'identifiant.</param>
    /// <returns>Instance de RoleId.</returns>
    /// <exception cref="ArgumentException">Si la valeur est vide.</exception>
    public static RoleId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("L'identifiant de rôle ne peut pas être vide.", nameof(value));
        }

        return new RoleId(value.Trim().ToLowerInvariant());
    }

    /// <summary>
    /// Crée un nouveau RoleId avec un GUID pour les rôles personnalisés.
    /// </summary>
    /// <returns>Nouveau RoleId unique.</returns>
    public static RoleId NewCustom() => new($"custom-{Guid.NewGuid()}");

    /// <summary>
    /// Vérifie si ce rôle est un rôle de base.
    /// </summary>
    public bool IsBuiltIn => Value is "owner" or "contributor" or "reader";

    /// <summary>
    /// Vérifie si ce rôle est un rôle personnalisé.
    /// </summary>
    public bool IsCustom => Value.StartsWith("custom-", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Conversion implicite vers string.
    /// </summary>
    public static implicit operator string(RoleId id) => id.Value;

    #region Rôles de Base Prédéfinis

    /// <summary>
    /// Rôle Owner - Toutes les permissions + délégation.
    /// </summary>
    public static RoleId Owner => new("owner");

    /// <summary>
    /// Rôle Contributor - Lecture et écriture.
    /// </summary>
    public static RoleId Contributor => new("contributor");

    /// <summary>
    /// Rôle Reader - Lecture seule.
    /// </summary>
    public static RoleId Reader => new("reader");

    #endregion
}
