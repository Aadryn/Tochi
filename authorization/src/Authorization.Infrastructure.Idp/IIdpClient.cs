// <copyright file="IIdpClient.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Authorization.Infrastructure.Idp.Models;

namespace Authorization.Infrastructure.Idp;

/// <summary>
/// Interface abstraite pour interagir avec un Identity Provider externe.
/// Supporte Azure AD, Okta et Keycloak.
/// </summary>
/// <remarks>
/// L'IDP est la source de vérité pour les identités (utilisateurs, groupes, service accounts).
/// L'application Authorization ne crée JAMAIS de principals - elle synchronise les memberships.
/// </remarks>
public interface IIdpClient
{
    /// <summary>
    /// Obtient l'identifiant de l'IDP (azure-ad, okta, keycloak).
    /// </summary>
    string IdpSource { get; }

    /// <summary>
    /// Récupère un utilisateur par son ObjectId.
    /// </summary>
    /// <param name="userId">Identifiant unique de l'utilisateur dans l'IDP.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>L'utilisateur trouvé ou null si inexistant.</returns>
    Task<IdpUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère un utilisateur par son identifiant externe (email, UPN).
    /// </summary>
    /// <param name="externalId">Email ou User Principal Name.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>L'utilisateur trouvé ou null si inexistant.</returns>
    Task<IdpUser?> GetUserByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère un groupe par son ObjectId.
    /// </summary>
    /// <param name="groupId">Identifiant unique du groupe dans l'IDP.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le groupe trouvé ou null si inexistant.</returns>
    Task<IdpGroup?> GetGroupByIdAsync(string groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les membres d'un groupe.
    /// </summary>
    /// <param name="groupId">Identifiant du groupe.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des utilisateurs membres du groupe.</returns>
    Task<IReadOnlyList<IdpUser>> GetGroupMembersAsync(string groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les groupes auxquels appartient un utilisateur.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des groupes de l'utilisateur.</returns>
    Task<IReadOnlyList<IdpGroup>> GetUserGroupsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère un service account par son client_id.
    /// </summary>
    /// <param name="clientId">Client ID OAuth2 du service account.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Le service account trouvé ou null si inexistant.</returns>
    Task<IdpServiceAccount?> GetServiceAccountAsync(string clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Liste tous les utilisateurs de l'IDP (pour synchronisation batch).
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Stream asynchrone d'utilisateurs.</returns>
    IAsyncEnumerable<IdpUser> ListUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Liste tous les groupes de l'IDP (pour synchronisation batch).
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Stream asynchrone de groupes.</returns>
    IAsyncEnumerable<IdpGroup> ListGroupsAsync(CancellationToken cancellationToken = default);
}
