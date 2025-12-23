namespace Authorization.Domain.ValueObjects;

/// <summary>
/// Représente l'identifiant unique d'un principal (utilisateur, groupe ou service account).
/// Basé sur l'ObjectId (GUID) fourni par l'IDP externe.
/// </summary>
/// <remarks>
/// <para>
/// Le PrincipalId encapsule le GUID fourni par l'Identity Provider (Azure AD, Okta, Keycloak).
/// Il garantit que l'identifiant n'est jamais vide et fournit des conversions implicites.
/// </para>
/// <example>
/// <code>
/// // Création depuis un GUID
/// var id = PrincipalId.Create(Guid.NewGuid());
/// 
/// // Parsing depuis une chaîne
/// var id = PrincipalId.Parse("550e8400-e29b-41d4-a716-446655440000");
/// 
/// // Conversion implicite
/// Guid guid = id;
/// PrincipalId fromGuid = someGuid;
/// </code>
/// </example>
/// </remarks>
public readonly record struct PrincipalId : IEquatable<PrincipalId>
{
    /// <summary>
    /// Valeur du GUID (ObjectId de l'IDP).
    /// </summary>
    public Guid Value { get; }

    private PrincipalId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PrincipalId ne peut pas être un GUID vide.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Crée un PrincipalId à partir d'un GUID.
    /// </summary>
    /// <param name="objectId">ObjectId provenant de l'IDP.</param>
    /// <returns>Instance de PrincipalId.</returns>
    /// <exception cref="ArgumentException">Si le GUID est vide.</exception>
    public static PrincipalId Create(Guid objectId) => new(objectId);

    /// <summary>
    /// Parse une chaîne en PrincipalId.
    /// </summary>
    /// <param name="value">Chaîne représentant un GUID.</param>
    /// <returns>Instance de PrincipalId.</returns>
    /// <exception cref="ArgumentException">Si le format est invalide ou le GUID vide.</exception>
    public static PrincipalId Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("PrincipalId ne peut pas être vide.", nameof(value));
        }

        if (!Guid.TryParse(value, out var guid))
        {
            throw new ArgumentException($"Format ObjectId invalide : {value}", nameof(value));
        }

        return new PrincipalId(guid);
    }

    /// <summary>
    /// Tente de parser une chaîne en PrincipalId.
    /// </summary>
    /// <param name="value">Chaîne à parser.</param>
    /// <param name="principalId">PrincipalId résultant si succès.</param>
    /// <returns>True si le parsing a réussi.</returns>
    public static bool TryParse(string? value, out PrincipalId principalId)
    {
        if (!string.IsNullOrWhiteSpace(value) &&
            Guid.TryParse(value, out var guid) &&
            guid != Guid.Empty)
        {
            principalId = new PrincipalId(guid);
            return true;
        }

        principalId = default;
        return false;
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Retourne la représentation OpenFGA du principal.
    /// Format : "user:{guid}" ou "group:{guid}" selon le contexte.
    /// </summary>
    /// <param name="type">Type de principal.</param>
    /// <returns>Chaîne formatée pour OpenFGA.</returns>
    public string ToOpenFgaFormat(PrincipalType type) => type switch
    {
        PrincipalType.User => $"user:{Value}",
        PrincipalType.Group => $"group:{Value}",
        PrincipalType.ServiceAccount => $"serviceaccount:{Value}",
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    /// <summary>
    /// Conversion implicite vers Guid.
    /// </summary>
    public static implicit operator Guid(PrincipalId id) => id.Value;

    /// <summary>
    /// Conversion implicite depuis Guid.
    /// </summary>
    public static implicit operator PrincipalId(Guid guid) => Create(guid);
}
