// <copyright file="HttpAuthorizationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net.Http.Json;
using System.Text.Json;
using LLMProxy.Infrastructure.Authorization.Abstractions;
using LLMProxy.Infrastructure.Authorization.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LLMProxy.Infrastructure.Authorization.Services;

/// <summary>
/// Implémentation du service d'autorisation via appels HTTP vers l'application Authorization externe.
/// </summary>
/// <remarks>
/// <para>
/// Cette implémentation délègue toutes les opérations à l'application Authorization autonome
/// qui encapsule OpenFGA. Elle remplace l'accès direct à OpenFGA par des appels REST.
/// </para>
/// <para>
/// Avantages de cette approche (voir ADR-060) :
/// - Encapsulation : OpenFGA est un détail d'implémentation
/// - Scalabilité : L'application Authorization peut être déployée indépendamment
/// - Maintenance : Les mises à jour OpenFGA n'impactent pas les clients
/// </para>
/// </remarks>
public sealed class HttpAuthorizationService : IAuthorizationService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly AuthorizationClientConfiguration _config;
    private readonly ILogger<HttpAuthorizationService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="HttpAuthorizationService"/>.
    /// </summary>
    /// <param name="httpClient">Client HTTP configuré avec Polly.</param>
    /// <param name="cache">Cache mémoire pour les résultats.</param>
    /// <param name="options">Configuration du client.</param>
    /// <param name="logger">Logger.</param>
    public HttpAuthorizationService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<AuthorizationClientConfiguration> options,
        ILogger<HttpAuthorizationService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _config = options.Value;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        _config.Validate();

        _logger.LogInformation(
            "Service Authorization HTTP initialisé - URL: {BaseUrl}, Cache: {CacheDuration}s",
            _config.BaseUrl,
            _config.CacheDurationSeconds);
    }

    /// <inheritdoc />
    public async Task<AuthorizationResult> CheckAsync(
        AuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await CheckAsync(
            request.GetFullUser(),
            request.Relation,
            request.ObjectType,
            request.ObjectId,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuthorizationResult> CheckAsync(
        string userId,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
        {
            _logger.LogDebug("Service d'autorisation désactivé - autorisation accordée par défaut");
            return AuthorizationResult.Allowed;
        }

        // Vérifier le cache
        var cacheKey = BuildCacheKey("check", userId, relation, objectType, objectId);

        if (_config.CacheDurationSeconds > 0 && _cache.TryGetValue(cacheKey, out AuthorizationResult? cachedResult) && cachedResult is not null)
        {
            _logger.LogDebug("Résultat trouvé en cache: {CacheKey} = {Result}", cacheKey, cachedResult.IsAllowed);
            return cachedResult;
        }

        try
        {
            _logger.LogDebug(
                "Vérification autorisation: {UserId} {Relation} {ObjectType}:{ObjectId}",
                userId, relation, objectType, objectId);

            var request = new PermissionCheckRequest(userId, relation, objectType, objectId);
            var response = await _httpClient.PostAsJsonAsync(
                "/api/v1/permissions/check",
                request,
                _jsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Erreur API Authorization: {StatusCode} - {Reason}",
                    response.StatusCode,
                    response.ReasonPhrase);

                // En cas d'erreur, refuser par sécurité
                return AuthorizationResult.Denied("Authorization service unavailable");
            }

            var checkResponse = await response.Content.ReadFromJsonAsync<PermissionCheckResponse>(
                _jsonOptions,
                cancellationToken);

            var result = checkResponse?.IsAllowed == true
                ? AuthorizationResult.Allowed
                : AuthorizationResult.Denied(checkResponse?.Reason ?? "Access denied");

            // Mettre en cache
            if (_config.CacheDurationSeconds > 0)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_config.CacheDurationSeconds)
                };
                _cache.Set(cacheKey, result, cacheOptions);
            }

            _logger.LogDebug(
                "Autorisation {Result}: {UserId} {Relation} {ObjectType}:{ObjectId}",
                result.IsAllowed ? "accordée" : "refusée",
                userId, relation, objectType, objectId);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur de communication avec l'API Authorization");
            return AuthorizationResult.Denied("Authorization service communication error");
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken != cancellationToken)
        {
            _logger.LogError(ex, "Timeout lors de l'appel à l'API Authorization");
            return AuthorizationResult.Denied("Authorization service timeout");
        }
    }

    /// <inheritdoc />
    public async Task AddRelationAsync(
        string userId,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
        {
            _logger.LogDebug("Service d'autorisation désactivé - opération ignorée");
            return;
        }

        try
        {
            _logger.LogInformation(
                "Ajout relation: {UserId} {Relation} {ObjectType}:{ObjectId}",
                userId, relation, objectType, objectId);

            var request = new RelationRequest(userId, relation, objectType, objectId);
            var response = await _httpClient.PostAsJsonAsync(
                "/api/v1/relations",
                request,
                _jsonOptions,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            // Invalider le cache pour cet utilisateur/objet
            InvalidateCache(userId, objectType, objectId);

            _logger.LogDebug("Relation ajoutée avec succès");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Erreur lors de l'ajout de relation: {UserId} {Relation} {ObjectType}:{ObjectId}",
                userId, relation, objectType, objectId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RemoveRelationAsync(
        string userId,
        string relation,
        string objectType,
        string objectId,
        CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
        {
            _logger.LogDebug("Service d'autorisation désactivé - opération ignorée");
            return;
        }

        try
        {
            _logger.LogInformation(
                "Suppression relation: {UserId} {Relation} {ObjectType}:{ObjectId}",
                userId, relation, objectType, objectId);

            var request = new RelationRequest(userId, relation, objectType, objectId);
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/relations")
            {
                Content = JsonContent.Create(request, options: _jsonOptions)
            };

            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Invalider le cache
            InvalidateCache(userId, objectType, objectId);

            _logger.LogDebug("Relation supprimée avec succès");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Erreur lors de la suppression de relation: {UserId} {Relation} {ObjectType}:{ObjectId}",
                userId, relation, objectType, objectId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListObjectsAsync(
        string userId,
        string relation,
        string objectType,
        CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
        {
            _logger.LogDebug("Service d'autorisation désactivé - liste vide retournée");
            return Array.Empty<string>();
        }

        // Vérifier le cache
        var cacheKey = BuildCacheKey("list", userId, relation, objectType, "*");

        if (_config.CacheDurationSeconds > 0 && _cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cachedList))
        {
            _logger.LogDebug("Liste trouvée en cache: {CacheKey}", cacheKey);
            return cachedList!;
        }

        try
        {
            _logger.LogDebug(
                "Liste objets: {UserId} {Relation} {ObjectType}",
                userId, relation, objectType);

            var response = await _httpClient.GetAsync(
                $"/api/v1/relations/objects?userId={Uri.EscapeDataString(userId)}&relation={Uri.EscapeDataString(relation)}&objectType={Uri.EscapeDataString(objectType)}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Erreur API Authorization lors du listage: {StatusCode}",
                    response.StatusCode);
                return Array.Empty<string>();
            }

            var listResponse = await response.Content.ReadFromJsonAsync<ListObjectsResponse>(
                _jsonOptions,
                cancellationToken);

            var objects = listResponse?.ObjectIds ?? Array.Empty<string>();

            // Mettre en cache
            if (_config.CacheDurationSeconds > 0)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_config.CacheDurationSeconds)
                };
                _cache.Set(cacheKey, objects, cacheOptions);
            }

            _logger.LogDebug("Objets listés: {Count} résultats", objects.Count);

            return objects;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur lors du listage des objets");
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Construit une clé de cache unique.
    /// </summary>
    private static string BuildCacheKey(string operation, string userId, string relation, string objectType, string objectId)
    {
        return $"authz:{operation}:{userId}:{relation}:{objectType}:{objectId}";
    }

    /// <summary>
    /// Invalide les entrées de cache liées à un utilisateur/objet.
    /// </summary>
    private void InvalidateCache(string userId, string objectType, string objectId)
    {
        // Note: IMemoryCache ne supporte pas l'invalidation par pattern
        // On pourrait utiliser un IDistributedCache avec Redis pour cette fonctionnalité
        // Pour l'instant, on laisse le cache expirer naturellement
        _logger.LogDebug(
            "Cache invalidation demandée (expiration naturelle): {UserId} {ObjectType}:{ObjectId}",
            userId, objectType, objectId);
    }

    #region DTOs internes

    /// <summary>
    /// Requête de vérification de permission.
    /// </summary>
    private record PermissionCheckRequest(
        string UserId,
        string Relation,
        string ObjectType,
        string ObjectId);

    /// <summary>
    /// Réponse de vérification de permission.
    /// </summary>
    private record PermissionCheckResponse(
        bool IsAllowed,
        string? Reason);

    /// <summary>
    /// Requête pour ajouter/supprimer une relation.
    /// </summary>
    private record RelationRequest(
        string UserId,
        string Relation,
        string ObjectType,
        string ObjectId);

    /// <summary>
    /// Réponse du listage d'objets.
    /// </summary>
    private record ListObjectsResponse(
        IReadOnlyList<string> ObjectIds);

    #endregion
}
