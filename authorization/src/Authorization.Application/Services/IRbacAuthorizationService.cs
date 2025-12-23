using Authorization.Domain.ValueObjects;

namespace Authorization.Application.Services;

/// <summary>
/// Service principal d'autorisation.
/// Point d'entrée pour toutes les opérations d'autorisation.
/// </summary>
public interface IRbacAuthorizationService
{
    /// <summary>
    /// Vérifie si un principal a une permission sur un scope.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="principalType">Type du principal.</param>
    /// <param name="permission">Permission à vérifier.</param>
    /// <param name="scope">Scope de la vérification.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si autorisé, false sinon.</returns>
    Task<bool> CheckPermissionAsync(
        TenantId tenantId,
        PrincipalId principalId,
        PrincipalType principalType,
        Permission permission,
        Scope scope,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigne un rôle à un principal sur un scope.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="actorId">Identifiant du principal qui effectue l'opération.</param>
    /// <param name="actorType">Type du principal acteur.</param>
    /// <param name="targetPrincipalId">Identifiant du principal cible.</param>
    /// <param name="targetPrincipalType">Type du principal cible.</param>
    /// <param name="roleId">Identifiant du rôle à assigner.</param>
    /// <param name="scope">Scope de l'assignation.</param>
    /// <param name="expiresAt">Date d'expiration optionnelle.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Identifiant de l'assignation créée.</returns>
    Task<RoleAssignmentId> AssignRoleAsync(
        TenantId tenantId,
        PrincipalId actorId,
        PrincipalType actorType,
        PrincipalId targetPrincipalId,
        PrincipalType targetPrincipalType,
        RoleId roleId,
        Scope scope,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Révoque une assignation de rôle.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="actorId">Identifiant du principal qui effectue l'opération.</param>
    /// <param name="actorType">Type du principal acteur.</param>
    /// <param name="assignmentId">Identifiant de l'assignation à révoquer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task RevokeRoleAsync(
        TenantId tenantId,
        PrincipalId actorId,
        PrincipalType actorType,
        RoleAssignmentId assignmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Liste les assignations de rôles pour un principal.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des assignations.</returns>
    Task<IReadOnlyList<RoleAssignmentInfo>> ListRoleAssignmentsAsync(
        TenantId tenantId,
        PrincipalId principalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Liste les principaux qui ont accès à un scope.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="scope">Scope à vérifier.</param>
    /// <param name="permission">Permission optionnelle à filtrer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des principaux avec leurs rôles.</returns>
    Task<IReadOnlyList<PrincipalAccessInfo>> ListPrincipalsWithAccessAsync(
        TenantId tenantId,
        Scope scope,
        Permission? permission = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Informations sur une assignation de rôle.
/// </summary>
public record RoleAssignmentInfo(
    RoleAssignmentId Id,
    RoleId RoleId,
    string RoleName,
    Scope Scope,
    DateTime AssignedAt,
    PrincipalId? AssignedBy,
    DateTime? ExpiresAt);

/// <summary>
/// Informations sur l'accès d'un principal.
/// </summary>
public record PrincipalAccessInfo(
    PrincipalId PrincipalId,
    PrincipalType PrincipalType,
    string DisplayName,
    RoleId RoleId,
    string RoleName,
    Scope Scope);
