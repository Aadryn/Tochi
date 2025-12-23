// <copyright file="OktaClient.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Authorization.Infrastructure.Idp.Models;
using Microsoft.Extensions.Logging;
using Okta.Sdk.Api;
using Okta.Sdk.Client;
using Okta.Sdk.Model;

namespace Authorization.Infrastructure.Idp.Okta;

/// <summary>
/// Client pour l'intégration avec Okta.
/// Implémente la récupération des utilisateurs, groupes et comptes de service.
/// </summary>
/// <remarks>
/// <para>
/// Ce client utilise le SDK Okta .NET v7.x qui fournit une API basée sur
/// les classes Api (UserApi, GroupApi, ApplicationApi).
/// </para>
/// <para>
/// L'API Okta utilise un système de pagination par curseur avec le paramètre "after".
/// Les méthodes List* retournent des IOktaCollectionClient pour la pagination.
/// </para>
/// </remarks>
public class OktaClient : IIdpClient
{
    private readonly OktaConfiguration _configuration;
    private readonly ILogger<OktaClient> _logger;
    private readonly Configuration _oktaConfig;

    /// <inheritdoc />
    public string IdpSource => "okta";

    /// <summary>
    /// Initialise une nouvelle instance du client Okta.
    /// </summary>
    /// <param name="configuration">Configuration Okta.</param>
    /// <param name="logger">Logger.</param>
    public OktaClient(
        OktaConfiguration configuration,
        ILogger<OktaClient> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _oktaConfig = new Configuration
        {
            OktaDomain = _configuration.Domain,
            Token = _configuration.ApiToken
        };
    }

    /// <inheritdoc />
    public async Task<IdpUser?> GetUserByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userApi = new UserApi(_oktaConfig);
            var user = await userApi.GetUserAsync(userId, cancellationToken: cancellationToken);

            return user is null ? null : MapToIdpUser(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur {UserId} depuis Okta", userId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IdpUser?> GetUserByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userApi = new UserApi(_oktaConfig);
            var collectionClient = userApi.ListUsers(q: externalId, limit: 1, cancellationToken: cancellationToken);

            await foreach (var user in collectionClient)
            {
                return MapToIdpUser(user);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche de l'utilisateur par external ID {ExternalId} depuis Okta", externalId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IdpGroup?> GetGroupByIdAsync(
        string groupId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var groupApi = new GroupApi(_oktaConfig);
            var group = await groupApi.GetGroupAsync(groupId, cancellationToken: cancellationToken);

            return group is null ? null : MapToIdpGroup(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du groupe {GroupId} depuis Okta", groupId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IdpUser>> GetGroupMembersAsync(
        string groupId,
        CancellationToken cancellationToken = default)
    {
        var members = new List<IdpUser>();

        try
        {
            var groupApi = new GroupApi(_oktaConfig);
            var collectionClient = groupApi.ListGroupUsers(groupId, cancellationToken: cancellationToken);

            await foreach (var user in collectionClient)
            {
                members.Add(MapToIdpUser(user));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des membres du groupe {GroupId} depuis Okta", groupId);
        }

        return members;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IdpGroup>> GetUserGroupsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var groups = new List<IdpGroup>();

        try
        {
            var userApi = new UserApi(_oktaConfig);
            var collectionClient = userApi.ListUserGroups(userId, cancellationToken: cancellationToken);

            await foreach (var group in collectionClient)
            {
                groups.Add(MapToIdpGroup(group));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des groupes de l'utilisateur {UserId} depuis Okta", userId);
        }

        return groups;
    }

    /// <inheritdoc />
    public async Task<IdpServiceAccount?> GetServiceAccountAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var appApi = new ApplicationApi(_oktaConfig);
            var app = await appApi.GetApplicationAsync(clientId, cancellationToken: cancellationToken);

            if (app is null)
            {
                return null;
            }

            // Parser l'ID en Guid
            var objectId = Guid.TryParse(app.Id, out var id)
                ? id
                : CreateDeterministicGuid(app.Id ?? clientId);

            return new IdpServiceAccount(
                ObjectId: objectId,
                ClientId: clientId,
                DisplayName: app.Label ?? $"Service Account {clientId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du service account {ClientId} depuis Okta", clientId);
            return null;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IdpUser> ListUsersAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var userApi = new UserApi(_oktaConfig);
        var hasError = false;

        var collectionClient = userApi.ListUsers(limit: 200, cancellationToken: cancellationToken);

        IAsyncEnumerator<User>? enumerator = null;

        try
        {
            enumerator = collectionClient.GetAsyncEnumerator(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'initialisation de la liste des utilisateurs Okta");
            hasError = true;
        }

        if (hasError || enumerator is null)
        {
            yield break;
        }

        try
        {
            while (true)
            {
                bool hasNext;

                try
                {
                    hasNext = await enumerator.MoveNextAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'itération des utilisateurs Okta");
                    break;
                }

                if (!hasNext)
                {
                    break;
                }

                yield return MapToIdpUser(enumerator.Current);
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IdpGroup> ListGroupsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var groupApi = new GroupApi(_oktaConfig);
        var hasError = false;

        var collectionClient = groupApi.ListGroups(limit: 200, cancellationToken: cancellationToken);

        IAsyncEnumerator<Group>? enumerator = null;

        try
        {
            enumerator = collectionClient.GetAsyncEnumerator(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'initialisation de la liste des groupes Okta");
            hasError = true;
        }

        if (hasError || enumerator is null)
        {
            yield break;
        }

        try
        {
            while (true)
            {
                bool hasNext;

                try
                {
                    hasNext = await enumerator.MoveNextAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'itération des groupes Okta");
                    break;
                }

                if (!hasNext)
                {
                    break;
                }

                yield return MapToIdpGroup(enumerator.Current);
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }
    }

    /// <summary>
    /// Mappe un utilisateur Okta vers le modèle IDP.
    /// </summary>
    /// <param name="user">Utilisateur Okta.</param>
    /// <returns>Utilisateur IDP.</returns>
    private static IdpUser MapToIdpUser(User user)
    {
        // Parser l'ID en Guid, sinon générer un Guid déterministe
        var objectId = Guid.TryParse(user.Id, out var id)
            ? id
            : CreateDeterministicGuid(user.Id ?? string.Empty);

        var displayName = BuildDisplayName(user.Profile?.FirstName, user.Profile?.LastName);

        return new IdpUser(
            ObjectId: objectId,
            Email: user.Profile?.Email ?? string.Empty,
            DisplayName: displayName,
            UserPrincipalName: user.Profile?.Login,
            IsEnabled: user.Status == UserStatus.ACTIVE);
    }

    /// <summary>
    /// Mappe un groupe Okta vers le modèle IDP.
    /// </summary>
    /// <param name="group">Groupe Okta.</param>
    /// <returns>Groupe IDP.</returns>
    private static IdpGroup MapToIdpGroup(Group group)
    {
        // Parser l'ID en Guid, sinon générer un Guid déterministe
        var objectId = Guid.TryParse(group.Id, out var id)
            ? id
            : CreateDeterministicGuid(group.Id ?? string.Empty);

        return new IdpGroup(
            ObjectId: objectId,
            Name: group.Profile?.Name ?? "Unknown",
            Description: group.Profile?.Description);
    }

    /// <summary>
    /// Construit le nom d'affichage à partir du prénom et nom.
    /// </summary>
    private static string BuildDisplayName(string? firstName, string? lastName)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            parts.Add(firstName);
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            parts.Add(lastName);
        }

        return parts.Count > 0 ? string.Join(" ", parts) : "Unknown";
    }

    /// <summary>
    /// Crée un Guid déterministe à partir d'une chaîne.
    /// Utile quand l'IDP utilise des IDs non-Guid.
    /// </summary>
    /// <param name="input">Chaîne source.</param>
    /// <returns>Guid déterministe basé sur le hash MD5.</returns>
    private static Guid CreateDeterministicGuid(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
