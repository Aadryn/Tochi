using Authorization.Domain.ValueObjects;
using OpenFga.Sdk.Client;

namespace Authorization.Infrastructure.OpenFGA.Services;

/// <summary>
/// Fournisseur de clients OpenFGA par tenant (multi-tenant).
/// </summary>
public interface IOpenFgaStoreProvider
{
    /// <summary>
    /// Obtient un client OpenFGA configuré pour un tenant spécifique.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Client OpenFGA configuré avec le bon store.</returns>
    Task<OpenFgaClient> GetClientAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crée un nouveau store pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Identifiant du store créé.</returns>
    Task<string> CreateStoreAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère l'identifiant du store pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Identifiant du store ou null si inexistant.</returns>
    Task<string?> GetStoreIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initialise le modèle d'autorisation pour un store.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task InitializeModelAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un store (pour nettoyage).
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task DeleteStoreAsync(TenantId tenantId, CancellationToken cancellationToken = default);
}
