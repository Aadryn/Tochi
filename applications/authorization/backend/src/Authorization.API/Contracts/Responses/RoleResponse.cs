namespace Authorization.API.Contracts.Responses;

/// <summary>
/// Réponse représentant une définition de rôle.
/// </summary>
public sealed record RoleResponse
{
    /// <summary>
    /// Identifiant unique du rôle.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Nom d'affichage du rôle.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description du rôle.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Indique si c'est un rôle prédéfini (built-in).
    /// </summary>
    public required bool IsBuiltIn { get; init; }

    /// <summary>
    /// Permissions accordées par ce rôle.
    /// </summary>
    public required IReadOnlyList<string> Permissions { get; init; }

    /// <summary>
    /// Scopes sur lesquels ce rôle peut être assigné.
    /// </summary>
    public required IReadOnlyList<string> AssignableScopes { get; init; }
}
