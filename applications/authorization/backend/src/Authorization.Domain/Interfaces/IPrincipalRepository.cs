using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects;

namespace Authorization.Domain.Interfaces;

/// <summary>
/// Repository pour la gestion des principals synchronisés depuis l'IDP.
/// </summary>
public interface IPrincipalRepository
{
    /// <summary>
    /// Récupère un principal par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant du principal.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Principal trouvé ou null.</returns>
    Task<Principal?> GetByIdAsync(
        PrincipalId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère tous les principals d'un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="type">Filtre optionnel par type.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des principals.</returns>
    Task<IReadOnlyList<Principal>> GetAllAsync(
        TenantId tenantId,
        PrincipalType? type = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recherche des principals par email ou nom d'affichage.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="searchTerm">Terme de recherche.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des principals correspondants.</returns>
    Task<IReadOnlyList<Principal>> SearchAsync(
        TenantId tenantId,
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un principal existe.
    /// </summary>
    /// <param name="id">Identifiant du principal.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si le principal existe.</returns>
    Task<bool> ExistsAsync(
        PrincipalId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute un nouveau principal.
    /// </summary>
    /// <param name="principal">Principal à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task AddAsync(Principal principal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour un principal existant.
    /// </summary>
    /// <param name="principal">Principal à mettre à jour.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task UpdateAsync(Principal principal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un principal.
    /// </summary>
    /// <param name="id">Identifiant du principal.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task DeleteAsync(
        PrincipalId id,
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour en masse les principals depuis l'IDP.
    /// </summary>
    /// <param name="principals">Principals à synchroniser.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Nombre de principals créés, mis à jour et supprimés.</returns>
    Task<(int Created, int Updated, int Deleted)> SyncBatchAsync(
        IEnumerable<Principal> principals,
        TenantId tenantId,
        CancellationToken cancellationToken = default);
}
