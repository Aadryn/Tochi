using LLMProxy.Domain.Entities.Routing;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Interface de repository pour les routes de proxy.
/// </summary>
/// <remarks>
/// <para>
/// Fournit les opérations CRUD pour les entités <see cref="ProxyRoute"/>.
/// Utilisé par le provider de configuration YARP dynamique.
/// </para>
/// </remarks>
public interface IProxyRouteRepository
{
    /// <summary>
    /// Récupère toutes les routes actives.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des routes actives.</returns>
    Task<IReadOnlyList<ProxyRoute>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère toutes les routes d'un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des routes du tenant.</returns>
    Task<IReadOnlyList<ProxyRoute>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère une route par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant de la route.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La route ou null si non trouvée.</returns>
    Task<ProxyRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère une route par son RouteId YARP.
    /// </summary>
    /// <param name="routeId">Identifiant de route YARP.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>La route ou null si non trouvée.</returns>
    Task<ProxyRoute?> GetByRouteIdAsync(string routeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute une nouvelle route.
    /// </summary>
    /// <param name="route">Route à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task AddAsync(ProxyRoute route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour une route existante.
    /// </summary>
    /// <param name="route">Route à mettre à jour.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task UpdateAsync(ProxyRoute route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime une route.
    /// </summary>
    /// <param name="id">Identifiant de la route à supprimer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un RouteId existe déjà.
    /// </summary>
    /// <param name="routeId">Identifiant de route YARP.</param>
    /// <param name="excludeId">Identifiant à exclure (pour les mises à jour).</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si le RouteId existe.</returns>
    Task<bool> ExistsAsync(string routeId, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
