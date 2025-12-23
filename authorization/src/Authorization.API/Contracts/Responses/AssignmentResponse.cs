namespace Authorization.API.Contracts.Responses;

/// <summary>
/// Réponse représentant une assignation de rôle.
/// </summary>
public sealed record AssignmentResponse
{
    /// <summary>
    /// Identifiant unique de l'assignation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// ID du principal à qui le rôle est assigné.
    /// </summary>
    public required string PrincipalId { get; init; }

    /// <summary>
    /// Type du principal.
    /// </summary>
    public required string PrincipalType { get; init; }

    /// <summary>
    /// Identifiant du rôle assigné.
    /// </summary>
    public required string RoleId { get; init; }

    /// <summary>
    /// Nom d'affichage du rôle.
    /// </summary>
    public required string RoleName { get; init; }

    /// <summary>
    /// Scope sur lequel le rôle est assigné.
    /// </summary>
    public required string Scope { get; init; }

    /// <summary>
    /// Date de création de l'assignation.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// ID du principal qui a créé l'assignation.
    /// </summary>
    public required string CreatedBy { get; init; }

    /// <summary>
    /// Date d'expiration de l'assignation (null si permanente).
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    /// Indique si l'assignation est active.
    /// </summary>
    public bool IsActive => ExpiresAt == null || ExpiresAt > DateTimeOffset.UtcNow;
}
