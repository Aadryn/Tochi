using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Interfaces;

/// <summary>
/// Repository pour la gestion des assignations de rôles.
/// </summary>
public interface IRoleAssignmentRepository
{
    /// <summary>
    /// Récupère une assignation par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant de l'assignation.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Assignation trouvée ou null.</returns>
    Task<RoleAssignment?> GetByIdAsync(
        RoleAssignmentId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère toutes les assignations d'un principal.
    /// </summary>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="activeOnly">Filtrer uniquement les assignations actives.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des assignations.</returns>
    Task<IReadOnlyList<RoleAssignment>> GetByPrincipalAsync(
        PrincipalId principalId,
        TenantId tenantId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère toutes les assignations sur un scope spécifique.
    /// </summary>
    /// <param name="scope">Scope de l'assignation.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="activeOnly">Filtrer uniquement les assignations actives.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des assignations.</returns>
    Task<IReadOnlyList<RoleAssignment>> GetByScopeAsync(
        Scope scope,
        TenantId tenantId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère toutes les assignations pour un rôle spécifique.
    /// </summary>
    /// <param name="roleId">Identifiant du rôle.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="activeOnly">Filtrer uniquement les assignations actives.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des assignations.</returns>
    Task<IReadOnlyList<RoleAssignment>> GetByRoleAsync(
        RoleId roleId,
        TenantId tenantId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si une assignation identique existe déjà.
    /// </summary>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="roleId">Identifiant du rôle.</param>
    /// <param name="scope">Scope de l'assignation.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si une assignation active identique existe.</returns>
    Task<bool> ExistsAsync(
        PrincipalId principalId,
        RoleId roleId,
        Scope scope,
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les assignations expirées.
    /// </summary>
    /// <param name="batchSize">Nombre maximum d'assignations à retourner.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des assignations expirées non encore traitées.</returns>
    Task<IReadOnlyList<RoleAssignment>> GetExpiredAssignmentsAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute une nouvelle assignation.
    /// </summary>
    /// <param name="assignment">Assignation à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task AddAsync(RoleAssignment assignment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour une assignation existante.
    /// </summary>
    /// <param name="assignment">Assignation à mettre à jour.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task UpdateAsync(RoleAssignment assignment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime physiquement une assignation (soft delete via Revoke() préféré).
    /// </summary>
    /// <param name="id">Identifiant de l'assignation.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task DeleteAsync(
        RoleAssignmentId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Révoque toutes les assignations d'un principal.
    /// </summary>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="revokedBy">Principal effectuant la révocation.</param>
    /// <param name="reason">Raison de la révocation.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Nombre d'assignations révoquées.</returns>
    Task<int> RevokeAllByPrincipalAsync(
        PrincipalId principalId,
        TenantId tenantId,
        PrincipalId revokedBy,
        string? reason = null,
        CancellationToken cancellationToken = default);
}
