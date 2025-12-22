using LLMProxy.Domain.Entities.Routing;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Interface de repository pour les clusters de proxy.
/// </summary>
/// <remarks>
/// <para>
/// Fournit les opérations CRUD pour les entités <see cref="ProxyCluster"/>
/// et leurs destinations. Utilisé par le provider de configuration YARP dynamique.
/// </para>
/// </remarks>
public interface IProxyClusterRepository
{
    /// <summary>
    /// Récupère tous les clusters actifs avec leurs destinations.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des clusters actifs avec destinations.</returns>
    Task<IReadOnlyList<ProxyCluster>> GetAllActiveWithDestinationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère tous les clusters d'un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des clusters du tenant.</returns>
    Task<IReadOnlyList<ProxyCluster>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère un cluster par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant du cluster.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le cluster ou null si non trouvé.</returns>
    Task<ProxyCluster?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère un cluster par son ClusterId YARP.
    /// </summary>
    /// <param name="clusterId">Identifiant de cluster YARP.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le cluster ou null si non trouvé.</returns>
    Task<ProxyCluster?> GetByClusterIdAsync(string clusterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère un cluster avec ses destinations.
    /// </summary>
    /// <param name="id">Identifiant du cluster.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le cluster avec destinations ou null.</returns>
    Task<ProxyCluster?> GetByIdWithDestinationsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute un nouveau cluster.
    /// </summary>
    /// <param name="cluster">Cluster à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task AddAsync(ProxyCluster cluster, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour un cluster existant.
    /// </summary>
    /// <param name="cluster">Cluster à mettre à jour.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task UpdateAsync(ProxyCluster cluster, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un cluster et ses destinations.
    /// </summary>
    /// <param name="id">Identifiant du cluster à supprimer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un ClusterId existe déjà.
    /// </summary>
    /// <param name="clusterId">Identifiant de cluster YARP.</param>
    /// <param name="excludeId">Identifiant à exclure (pour les mises à jour).</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si le ClusterId existe.</returns>
    Task<bool> ExistsAsync(string clusterId, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un cluster est référencé par des routes.
    /// </summary>
    /// <param name="clusterId">Identifiant de cluster YARP.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si des routes référencent ce cluster.</returns>
    Task<bool> IsReferencedByRoutesAsync(string clusterId, CancellationToken cancellationToken = default);
}
