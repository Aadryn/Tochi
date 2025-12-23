namespace Authorization.Domain.ValueObjects;

/// <summary>
/// Types de principals supportés par le système d'autorisation.
/// </summary>
/// <remarks>
/// <para>
/// Les principals sont les entités qui peuvent recevoir des autorisations.
/// Ils sont tous gérés par l'IDP externe et synchronisés dans le système.
/// </para>
/// </remarks>
public enum PrincipalType
{
    /// <summary>
    /// Utilisateur humain authentifié via l'IDP.
    /// </summary>
    User = 1,

    /// <summary>
    /// Groupe d'utilisateurs défini dans l'IDP.
    /// Les membres du groupe héritent des permissions assignées au groupe.
    /// </summary>
    Group = 2,

    /// <summary>
    /// Compte de service (machine-to-machine) pour les applications.
    /// </summary>
    ServiceAccount = 3
}

/// <summary>
/// Extensions pour le type PrincipalType.
/// </summary>
public static class PrincipalTypeExtensions
{
    /// <summary>
    /// Convertit le type en préfixe OpenFGA.
    /// </summary>
    /// <param name="type">Type de principal.</param>
    /// <returns>Préfixe pour les tuples OpenFGA.</returns>
    public static string ToOpenFgaPrefix(this PrincipalType type) => type switch
    {
        PrincipalType.User => "user",
        PrincipalType.Group => "group",
        PrincipalType.ServiceAccount => "serviceaccount",
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    /// <summary>
    /// Parse une chaîne en PrincipalType.
    /// </summary>
    /// <param name="value">Chaîne représentant le type.</param>
    /// <returns>Type de principal.</returns>
    public static PrincipalType ParsePrincipalType(string value) =>
        value.ToLowerInvariant() switch
        {
            "user" => PrincipalType.User,
            "group" => PrincipalType.Group,
            "serviceaccount" or "service_account" => PrincipalType.ServiceAccount,
            _ => throw new ArgumentException($"Type de principal inconnu : {value}", nameof(value))
        };
}
