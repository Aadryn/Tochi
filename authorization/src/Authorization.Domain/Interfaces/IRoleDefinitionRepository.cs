using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Interfaces;

/// <summary>
/// Repository pour la gestion des définitions de rôles.
/// </summary>
public interface IRoleDefinitionRepository
{
    /// <summary>
    /// Récupère un rôle par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant du rôle.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Définition de rôle trouvée ou null.</returns>
    Task<RoleDefinition?> GetByIdAsync(
        RoleId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère tous les rôles d'un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="includeBuiltIn">Inclure les rôles de base.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des définitions de rôles.</returns>
    Task<IReadOnlyList<RoleDefinition>> GetAllAsync(
        TenantId tenantId,
        bool includeBuiltIn = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les rôles de base d'un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des rôles de base.</returns>
    Task<IReadOnlyList<RoleDefinition>> GetBuiltInRolesAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les rôles personnalisés d'un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des rôles personnalisés.</returns>
    Task<IReadOnlyList<RoleDefinition>> GetCustomRolesAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un rôle avec ce nom existe déjà.
    /// </summary>
    /// <param name="name">Nom du rôle.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="excludeId">Identifiant à exclure (pour les mises à jour).</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si un rôle avec ce nom existe.</returns>
    Task<bool> ExistsByNameAsync(
        string name,
        TenantId tenantId,
        RoleId? excludeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute une nouvelle définition de rôle.
    /// </summary>
    /// <param name="roleDefinition">Définition à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task AddAsync(RoleDefinition roleDefinition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour une définition de rôle existante.
    /// </summary>
    /// <param name="roleDefinition">Définition à mettre à jour.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task UpdateAsync(RoleDefinition roleDefinition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime une définition de rôle (personnalisés uniquement).
    /// </summary>
    /// <param name="id">Identifiant du rôle.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task DeleteAsync(
        RoleId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initialise les rôles de base pour un nouveau tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des rôles créés.</returns>
    Task<IReadOnlyList<RoleDefinition>> InitializeBuiltInRolesAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);
}
