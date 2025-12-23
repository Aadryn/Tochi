using Authorization.Infrastructure.Idp.Models;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Authorization.Infrastructure.Idp.AzureAd;

/// <summary>
/// Client pour l'intégration avec Azure AD (Entra ID) via Microsoft Graph.
/// </summary>
public class AzureAdClient : IIdpClient
{
    private readonly AzureAdConfiguration _configuration;
    private readonly ILogger<AzureAdClient> _logger;
    private readonly GraphServiceClient _graphClient;

    /// <inheritdoc />
    public string IdpSource => "azure-ad";

    /// <summary>
    /// Initialise une nouvelle instance du client Azure AD.
    /// </summary>
    public AzureAdClient(
        AzureAdConfiguration configuration,
        ILogger<AzureAdClient> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Créer les credentials avec client secret
        var credential = new ClientSecretCredential(
            _configuration.TenantId,
            _configuration.ClientId,
            _configuration.ClientSecret);

        _graphClient = new GraphServiceClient(credential);
    }

    /// <inheritdoc />
    public async Task<IdpUser?> GetUserByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _graphClient.Users[userId]
                .GetAsync(cancellationToken: cancellationToken);

            return user is null ? null : MapToIdpUser(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {UserId} from Azure AD", userId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IdpUser?> GetUserByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        // Dans Azure AD, l'externalId correspond généralement au userPrincipalName ou objectId
        return await GetUserByIdAsync(externalId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IdpGroup?> GetGroupByIdAsync(
        string groupId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _graphClient.Groups[groupId]
                .GetAsync(cancellationToken: cancellationToken);

            return group is null ? null : MapToIdpGroup(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching group {GroupId} from Azure AD", groupId);
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
            var response = await _graphClient.Groups[groupId].Members
                .GetAsync(cancellationToken: cancellationToken);

            if (response?.Value is null)
            {
                return members;
            }

            foreach (var member in response.Value)
            {
                if (member is User user)
                {
                    members.Add(MapToIdpUser(user));
                }
            }

            // Gérer la pagination
            var pageIterator = PageIterator<DirectoryObject, DirectoryObjectCollectionResponse>
                .CreatePageIterator(
                    _graphClient,
                    response,
                    item =>
                    {
                        if (item is User user)
                        {
                            members.Add(MapToIdpUser(user));
                        }
                        return true;
                    });

            await pageIterator.IterateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching members for group {GroupId}", groupId);
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
            var response = await _graphClient.Users[userId].MemberOf
                .GetAsync(cancellationToken: cancellationToken);

            if (response?.Value is null)
            {
                return groups;
            }

            foreach (var member in response.Value)
            {
                if (member is Group group)
                {
                    groups.Add(MapToIdpGroup(group));
                }
            }

            // Gérer la pagination
            var pageIterator = PageIterator<DirectoryObject, DirectoryObjectCollectionResponse>
                .CreatePageIterator(
                    _graphClient,
                    response,
                    item =>
                    {
                        if (item is Group group)
                        {
                            groups.Add(MapToIdpGroup(group));
                        }
                        return true;
                    });

            await pageIterator.IterateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching groups for user {UserId}", userId);
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
            // Les service principals sont les applications dans Azure AD
            var response = await _graphClient.ServicePrincipals
                .GetAsync(config =>
                {
                    config.QueryParameters.Filter = $"appId eq '{clientId}'";
                }, cancellationToken);

            var servicePrincipal = response?.Value?.FirstOrDefault();
            if (servicePrincipal is null)
            {
                return null;
            }

            return new IdpServiceAccount(
                ObjectId: Guid.TryParse(servicePrincipal.Id, out var id) ? id : Guid.Empty,
                ClientId: servicePrincipal.AppId ?? clientId,
                DisplayName: servicePrincipal.DisplayName ?? $"Service Account {clientId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching service account {ClientId}", clientId);
            return null;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IdpUser> ListUsersAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        UserCollectionResponse? response;

        try
        {
            response = await _graphClient.Users
                .GetAsync(config =>
                {
                    config.QueryParameters.Top = 100;
                    config.QueryParameters.Select = ["id", "userPrincipalName", "displayName", "mail", "accountEnabled"];
                }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing users from Azure AD");
            yield break;
        }

        if (response?.Value is null)
        {
            yield break;
        }

        foreach (var user in response.Value)
        {
            yield return MapToIdpUser(user);
        }

        // Pagination
        while (!string.IsNullOrEmpty(response.OdataNextLink))
        {
            UserCollectionResponse? nextResponse = null;
            var shouldBreak = false;
            
            try
            {
                nextResponse = await _graphClient.Users
                    .WithUrl(response.OdataNextLink)
                    .GetAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user pagination");
                shouldBreak = true;
            }

            if (shouldBreak || nextResponse?.Value is null)
            {
                break;
            }

            response = nextResponse;
            foreach (var user in response.Value)
            {
                yield return MapToIdpUser(user);
            }
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IdpGroup> ListGroupsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        GroupCollectionResponse? response;

        try
        {
            response = await _graphClient.Groups
                .GetAsync(config =>
                {
                    config.QueryParameters.Top = 100;
                    config.QueryParameters.Select = ["id", "displayName", "description"];
                }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing groups from Azure AD");
            yield break;
        }

        if (response?.Value is null)
        {
            yield break;
        }

        foreach (var group in response.Value)
        {
            yield return MapToIdpGroup(group);
        }

        // Pagination
        while (!string.IsNullOrEmpty(response.OdataNextLink))
        {
            GroupCollectionResponse? nextResponse = null;
            var shouldBreak = false;
            
            try
            {
                nextResponse = await _graphClient.Groups
                    .WithUrl(response.OdataNextLink)
                    .GetAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during group pagination");
                shouldBreak = true;
            }

            if (shouldBreak || nextResponse?.Value is null)
            {
                break;
            }

            response = nextResponse;
            foreach (var group in response.Value)
            {
                yield return MapToIdpGroup(group);
            }
        }
    }

    /// <summary>
    /// Mappe un utilisateur Graph vers le modèle IDP.
    /// </summary>
    private static IdpUser MapToIdpUser(User user)
    {
        return new IdpUser(
            ObjectId: Guid.TryParse(user.Id, out var id) ? id : Guid.Empty,
            Email: user.Mail ?? user.UserPrincipalName ?? string.Empty,
            DisplayName: user.DisplayName ?? "Unknown",
            UserPrincipalName: user.UserPrincipalName,
            IsEnabled: user.AccountEnabled ?? true);
    }

    /// <summary>
    /// Mappe un groupe Graph vers le modèle IDP.
    /// </summary>
    private static IdpGroup MapToIdpGroup(Group group)
    {
        return new IdpGroup(
            ObjectId: Guid.TryParse(group.Id, out var id) ? id : Guid.Empty,
            Name: group.DisplayName ?? "Unknown",
            Description: group.Description);
    }
}
