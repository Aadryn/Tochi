// <copyright file="KeycloakClient.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Authorization.Infrastructure.Idp.Keycloak.Contracts;
using Authorization.Infrastructure.Idp.Models;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Idp.Keycloak;

/// <summary>
/// Client pour interagir avec l'API Admin de Keycloak.
/// Utilise les credentials client pour l'authentification.
/// </summary>
public sealed class KeycloakClient : IIdpClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakConfiguration _config;
    private readonly ILogger<KeycloakClient> _logger;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <inheritdoc/>
    public string IdpSource => "keycloak";

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="KeycloakClient"/>.
    /// </summary>
    /// <param name="httpClient">Client HTTP configuré.</param>
    /// <param name="config">Configuration Keycloak.</param>
    /// <param name="logger">Logger pour le diagnostic.</param>
    public KeycloakClient(
        HttpClient httpClient,
        KeycloakConfiguration config,
        ILogger<KeycloakClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IdpUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var url = $"{_config.AdminApiUrl}/users/{userId}";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                _logger.LogWarning(
                    "Échec récupération utilisateur {UserId}: {StatusCode}",
                    userId,
                    response.StatusCode);
                return null;
            }

            var user = await response.Content.ReadFromJsonAsync<KeycloakUserDto>(JsonOptions, cancellationToken);
            return user?.ToIdpUser();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur HTTP lors de la récupération de l'utilisateur {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IdpUser?> GetUserByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        // Keycloak: recherche par email ou username
        var url = $"{_config.AdminApiUrl}/users?email={Uri.EscapeDataString(externalId)}&exact=true";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserDto>>(JsonOptions, cancellationToken);

            if (users == null || users.Count == 0)
            {
                // Essayer par username
                url = $"{_config.AdminApiUrl}/users?username={Uri.EscapeDataString(externalId)}&exact=true";
                response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                users = await response.Content.ReadFromJsonAsync<List<KeycloakUserDto>>(JsonOptions, cancellationToken);
            }

            return users?.FirstOrDefault()?.ToIdpUser();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur HTTP lors de la recherche de l'utilisateur par {ExternalId}", externalId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IdpGroup?> GetGroupByIdAsync(string groupId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var url = $"{_config.AdminApiUrl}/groups/{groupId}";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                return null;
            }

            var group = await response.Content.ReadFromJsonAsync<KeycloakGroupDto>(JsonOptions, cancellationToken);
            return group?.ToIdpGroup();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur HTTP lors de la récupération du groupe {GroupId}", groupId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IdpUser>> GetGroupMembersAsync(string groupId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var members = new List<IdpUser>();
        var url = $"{_config.AdminApiUrl}/groups/{groupId}/members";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserDto>>(JsonOptions, cancellationToken);

            if (users != null)
            {
                foreach (var user in users)
                {
                    var idpUser = user.ToIdpUser();
                    if (idpUser != null)
                    {
                        members.Add(idpUser);
                    }
                }
            }

            return members;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur HTTP lors de la récupération des membres du groupe {GroupId}", groupId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IdpGroup>> GetUserGroupsAsync(string userId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var url = $"{_config.AdminApiUrl}/users/{userId}/groups";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var groups = await response.Content.ReadFromJsonAsync<List<KeycloakGroupDto>>(JsonOptions, cancellationToken);

            return groups?
                .Select(g => g.ToIdpGroup())
                .Where(g => g != null)
                .Cast<IdpGroup>()
                .ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur HTTP lors de la récupération des groupes de l'utilisateur {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IdpServiceAccount?> GetServiceAccountAsync(string clientId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        // Keycloak: les service accounts sont des clients avec serviceAccountsEnabled=true
        var url = $"{_config.AdminApiUrl}/clients?clientId={Uri.EscapeDataString(clientId)}";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var clients = await response.Content.ReadFromJsonAsync<List<KeycloakClientDto>>(JsonOptions, cancellationToken);
            var client = clients?.FirstOrDefault(c => c.ServiceAccountsEnabled == true);

            if (client == null)
            {
                return null;
            }

            return new IdpServiceAccount(
                Guid.Parse(client.Id),
                client.ClientId,
                client.Name ?? client.ClientId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur HTTP lors de la récupération du service account {ClientId}", clientId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<IdpUser> ListUsersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var first = 0;
        const int max = 100;

        while (!cancellationToken.IsCancellationRequested)
        {
            var url = $"{_config.AdminApiUrl}/users?first={first}&max={max}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserDto>>(JsonOptions, cancellationToken);

            if (users == null || users.Count == 0)
            {
                yield break;
            }

            foreach (var user in users)
            {
                var idpUser = user.ToIdpUser();
                if (idpUser != null)
                {
                    yield return idpUser;
                }
            }

            if (users.Count < max)
            {
                yield break;
            }

            first += max;
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<IdpGroup> ListGroupsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var first = 0;
        const int max = 100;

        while (!cancellationToken.IsCancellationRequested)
        {
            var url = $"{_config.AdminApiUrl}/groups?first={first}&max={max}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var groups = await response.Content.ReadFromJsonAsync<List<KeycloakGroupDto>>(JsonOptions, cancellationToken);

            if (groups == null || groups.Count == 0)
            {
                yield break;
            }

            foreach (var group in groups)
            {
                var idpGroup = group.ToIdpGroup();
                if (idpGroup != null)
                {
                    yield return idpGroup;
                }
            }

            if (groups.Count < max)
            {
                yield break;
            }

            first += max;
        }
    }

    /// <summary>
    /// S'assure que le client est authentifié avec un token valide.
    /// </summary>
    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-1))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
            return;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check après acquisition du lock
            if (_accessToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-1))
            {
                return;
            }

            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _config.ClientId,
                ["client_secret"] = _config.ClientSecret
            };

            var response = await _httpClient.PostAsync(
                _config.TokenUrl,
                new FormUrlEncodedContent(tokenRequest),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions, cancellationToken);

            _accessToken = tokenResponse?.AccessToken
                ?? throw new InvalidOperationException("Token response did not contain access_token");

            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);

            _logger.LogDebug("Token Keycloak renouvelé, expire dans {ExpiresIn}s", tokenResponse.ExpiresIn);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _tokenLock.Dispose();
    }
}
