using Authorization.Domain.ValueObjects;

namespace Authorization.Infrastructure.Redis.Services;

/// <summary>
/// Interface pour le service de cache des permissions.
/// </summary>
public interface IPermissionCacheService
{
    /// <summary>
    /// Récupère un résultat de vérification de permission du cache.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="permission">Permission à vérifier.</param>
    /// <param name="scope">Scope de la permission.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Résultat mis en cache ou null si non trouvé.</returns>
    Task<bool?> GetPermissionCheckAsync(
        TenantId tenantId,
        PrincipalId principalId,
        Permission permission,
        Scope scope,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stocke un résultat de vérification de permission dans le cache.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="permission">Permission vérifiée.</param>
    /// <param name="scope">Scope de la permission.</param>
    /// <param name="allowed">Résultat de la vérification.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task SetPermissionCheckAsync(
        TenantId tenantId,
        PrincipalId principalId,
        Permission permission,
        Scope scope,
        bool allowed,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalide le cache des permissions pour un principal.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task InvalidatePrincipalAsync(
        TenantId tenantId,
        PrincipalId principalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalide le cache des permissions pour un scope.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="scope">Scope à invalider.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task InvalidateScopeAsync(
        TenantId tenantId,
        Scope scope,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalide tout le cache pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task InvalidateTenantAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);
}
