using Authorization.Domain.ValueObjects;

namespace Authorization.Infrastructure.OpenFGA.Services;

/// <summary>
/// Interface pour l'interaction avec OpenFGA.
/// </summary>
public interface IOpenFgaService
{
    /// <summary>
    /// Vérifie si un principal a une relation avec un objet.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="principalType">Type du principal.</param>
    /// <param name="relation">Relation à vérifier (ex: owner, contributor, reader).</param>
    /// <param name="objectType">Type d'objet (ex: scope).</param>
    /// <param name="objectId">Identifiant de l'objet.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si la relation existe.</returns>
    Task<bool> CheckAsync(
        TenantId tenantId,
        PrincipalId principalId,
        PrincipalType principalType,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Écrit un tuple de relation dans OpenFGA.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="user">Utilisateur au format OpenFGA (ex: user:guid).</param>
    /// <param name="relation">Relation.</param>
    /// <param name="objectType">Type d'objet.</param>
    /// <param name="objectId">Identifiant de l'objet.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task WriteAsync(
        TenantId tenantId,
        string user,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un tuple de relation dans OpenFGA.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="user">Utilisateur au format OpenFGA.</param>
    /// <param name="relation">Relation.</param>
    /// <param name="objectType">Type d'objet.</param>
    /// <param name="objectId">Identifiant de l'objet.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task DeleteAsync(
        TenantId tenantId,
        string user,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Liste les objets auxquels un utilisateur a accès.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="principalId">Identifiant du principal.</param>
    /// <param name="principalType">Type du principal.</param>
    /// <param name="relation">Relation recherchée.</param>
    /// <param name="objectType">Type d'objets à lister.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des identifiants d'objets.</returns>
    Task<IReadOnlyList<string>> ListObjectsAsync(
        TenantId tenantId,
        PrincipalId principalId,
        PrincipalType principalType,
        string relation,
        string objectType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Liste les utilisateurs ayant une relation avec un objet.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="relation">Relation recherchée.</param>
    /// <param name="objectType">Type d'objet.</param>
    /// <param name="objectId">Identifiant de l'objet.</param>
    /// <param name="userType">Type d'utilisateurs à lister (user, group).</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des utilisateurs au format OpenFGA.</returns>
    Task<IReadOnlyList<string>> ListUsersAsync(
        TenantId tenantId,
        string relation,
        string objectType,
        string objectId,
        string userType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crée un nouveau store pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Identifiant du store créé.</returns>
    Task<string> CreateStoreAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère l'identifiant du store pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Identifiant du store ou null si inexistant.</returns>
    Task<string?> GetStoreIdAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initialise le modèle d'autorisation pour un store.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task InitializeModelAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);
}
