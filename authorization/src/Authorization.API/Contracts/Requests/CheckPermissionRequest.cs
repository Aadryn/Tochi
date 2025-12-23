using System.ComponentModel.DataAnnotations;

namespace Authorization.API.Contracts.Requests;

/// <summary>
/// Requête de vérification de permission.
/// </summary>
public sealed record CheckPermissionRequest
{
    /// <summary>
    /// Permission à vérifier.
    /// </summary>
    /// <example>prompts:read</example>
    [Required]
    public required string Permission { get; init; }

    /// <summary>
    /// Scope sur lequel vérifier la permission.
    /// </summary>
    /// <example>api.llmproxy.com/organizations/org-123/tenants/tenant-456</example>
    [Required]
    public required string Scope { get; init; }

    /// <summary>
    /// ID du principal pour lequel vérifier (optionnel).
    /// Si non spécifié, utilise le principal de l'appelant.
    /// </summary>
    public Guid? PrincipalId { get; init; }

    /// <summary>
    /// Type de principal (optionnel, déduit si non spécifié).
    /// </summary>
    /// <example>user</example>
    public string? PrincipalType { get; init; }
}
