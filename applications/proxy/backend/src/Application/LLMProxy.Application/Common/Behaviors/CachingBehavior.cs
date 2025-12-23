using LLMProxy.Domain.Interfaces;
using LLMProxy.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LLMProxy.Application.Common.Behaviors;

/// <summary>
/// Behavior MediatR pour gérer le cache des requêtes (Query uniquement).
/// Conforme à ADR-042 (Distributed Cache Strategy).
/// </summary>
/// <remarks>
/// Ce behavior intercepte toutes les requêtes implémentant <see cref="ICachedQuery{TResponse}"/>
/// et vérifie le cache Redis avant d'exécuter le handler.
/// En cas de cache hit, renvoie directement la réponse mise en cache.
/// En cas de cache miss, exécute le handler et met en cache la réponse.
/// </remarks>
/// <typeparam name="TRequest">Type de la requête (doit implémenter ICachedQuery)</typeparam>
/// <typeparam name="TResponse">Type de la réponse</typeparam>
public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery<TResponse>
    where TResponse : class
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cacheService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Intercepte la requête pour vérifier le cache avant exécution.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Générer la clé de cache basée sur le type de requête et ses paramètres
        var cacheKey = GenerateCacheKey(request);

        // Tenter de récupérer depuis le cache
        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);

        if (cachedResponse is not null)
        {
            _logger.LogDebug(
                "Cache HIT pour {RequestType} avec clé {CacheKey}",
                typeof(TRequest).Name,
                cacheKey);

            return cachedResponse;
        }

        _logger.LogDebug(
            "Cache MISS pour {RequestType} avec clé {CacheKey}",
            typeof(TRequest).Name,
            cacheKey);

        // Exécuter le handler (accès DB)
        var response = await next();

        // Mettre en cache la réponse si elle n'est pas null
        if (response is not null)
        {
            var expiration = request.CacheExpiration ?? TimeSpan.FromMinutes(5);

            await _cacheService.SetAsync(
                cacheKey,
                response,
                expiration,
                cancellationToken);

            _logger.LogDebug(
                "Réponse mise en cache pour {RequestType} avec TTL {Expiration}",
                typeof(TRequest).Name,
                expiration);
        }

        return response!;
    }

    /// <summary>
    /// Génère une clé de cache unique basée sur le type et les paramètres de la requête.
    /// </summary>
    /// <remarks>
    /// Format : {RequestTypeName}:{Hash(RequestJSON)}
    /// Exemple : GetTenantByIdQuery:a3f2b8c1...
    /// </remarks>
    private static string GenerateCacheKey(TRequest request)
    {
        var requestType = typeof(TRequest).Name;

        // Sérialiser la requête pour générer un hash unique des paramètres
        var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Hash simple pour garder la clé courte
        var hash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(requestJson)))
            .Substring(0, 16);

        return $"{requestType}:{hash}";
    }
}
