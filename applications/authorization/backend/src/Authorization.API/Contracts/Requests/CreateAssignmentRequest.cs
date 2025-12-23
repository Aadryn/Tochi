using System.ComponentModel.DataAnnotations;

namespace Authorization.API.Contracts.Requests;

/// <summary>
/// Requête de création d'assignation de rôle.
/// </summary>
public sealed record CreateAssignmentRequest
{
    /// <summary>
    /// ID du principal (utilisateur, groupe ou service account).
    /// </summary>
    [Required]
    public required Guid PrincipalId { get; init; }

    /// <summary>
    /// Type de principal.
    /// </summary>
    /// <example>user</example>
    [Required]
    public required string PrincipalType { get; init; }

    /// <summary>
    /// Identifiant du rôle à assigner.
    /// </summary>
    /// <example>owner</example>
    [Required]
    public required string RoleId { get; init; }

    /// <summary>
    /// Scope sur lequel assigner le rôle.
    /// </summary>
    /// <example>api.llmproxy.com/organizations/org-123/tenants/tenant-456</example>
    [Required]
    public required string Scope { get; init; }

    /// <summary>
    /// Date d'expiration optionnelle de l'assignation.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    /// Justification de l'assignation (pour audit).
    /// </summary>
    [MaxLength(1000)]
    public string? Justification { get; init; }
}
