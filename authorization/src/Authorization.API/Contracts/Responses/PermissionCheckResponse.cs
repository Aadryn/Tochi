namespace Authorization.API.Contracts.Responses;

/// <summary>
/// Réponse à une vérification de permission.
/// </summary>
public sealed record PermissionCheckResponse
{
    /// <summary>
    /// Indique si la permission est accordée.
    /// </summary>
    public required bool Allowed { get; init; }

    /// <summary>
    /// ID du principal vérifié.
    /// </summary>
    public required string PrincipalId { get; init; }

    /// <summary>
    /// Type du principal.
    /// </summary>
    public required string PrincipalType { get; init; }

    /// <summary>
    /// Permission vérifiée.
    /// </summary>
    public required string Permission { get; init; }

    /// <summary>
    /// Scope sur lequel la permission a été vérifiée.
    /// </summary>
    public required string Scope { get; init; }

    /// <summary>
    /// Rôle qui accorde la permission (si accordée).
    /// </summary>
    public string? GrantingRole { get; init; }

    /// <summary>
    /// Scope sur lequel le rôle est assigné (peut être parent du scope demandé).
    /// </summary>
    public string? GrantingScope { get; init; }

    /// <summary>
    /// Indique si le résultat provient du cache.
    /// </summary>
    public bool FromCache { get; init; }

    /// <summary>
    /// Durée de la vérification en millisecondes.
    /// </summary>
    public double DurationMs { get; init; }
}
