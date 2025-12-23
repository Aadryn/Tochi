using LLMProxy.Application.Configuration.RateLimiting;
using LLMProxy.Application.Interfaces;
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Services.RateLimiting;

/// <summary>
/// Service de configuration de rate limiting avec persistance PostgreSQL et cache Redis.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling et ADR-042 Distributed Cache Strategy.
/// </para>
/// <para>
/// Ce service implémente une stratégie de cache à deux niveaux :
/// <list type="number">
/// <item><description>Vérification du cache Redis (TTL : 1 minute)</description></item>
/// <item><description>Chargement depuis PostgreSQL si absent du cache</description></item>
/// <item><description>Fallback sur configuration par défaut si non trouvé en DB</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Stratégie de cache :</strong>
/// <list type="bullet">
/// <item><description>Clé Redis : <c>ratelimit:config:{tenantId}</c></description></item>
/// <item><description>TTL : 1 minute pour permettre les mises à jour fréquentes</description></item>
/// <item><description>Invalidation automatique lors des mises à jour</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class DatabaseRateLimitConfigurationService : IRateLimitConfigurationService
{
    private readonly ITenantRateLimitConfigurationRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DatabaseRateLimitConfigurationService> _logger;

    /// <summary>
    /// Préfixe des clés de cache pour les configurations de rate limiting.
    /// </summary>
    private const string CacheKeyPrefix = "ratelimit:config:";

    /// <summary>
    /// Durée de vie du cache (1 minute).
    /// </summary>
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Initialise une nouvelle instance du service.
    /// </summary>
    /// <param name="repository">Repository pour la persistance des configurations.</param>
    /// <param name="cacheService">Service de cache Redis.</param>
    /// <param name="logger">Logger pour la traçabilité.</param>
    public DatabaseRateLimitConfigurationService(
        ITenantRateLimitConfigurationRepository repository,
        ICacheService cacheService,
        ILogger<DatabaseRateLimitConfigurationService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TenantRateLimitConfiguration> GetConfigurationAsync(
        Guid tenantId,
        CancellationToken ct = default)
    {
        var cacheKey = GetCacheKey(tenantId);

        // 1. Vérifier le cache Redis
        try
        {
            var cached = await _cacheService.GetAsync<TenantRateLimitConfiguration>(cacheKey, ct);
            if (cached != null)
            {
                _logger.LogDebug(
                    "Configuration rate limiting trouvée en cache pour le tenant {TenantId}",
                    tenantId);
                return cached;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Erreur lors de la lecture du cache Redis pour le tenant {TenantId}. Fallback sur DB.",
                tenantId);
        }

        // 2. Charger depuis PostgreSQL
        var config = await _repository.GetByTenantIdAsync(tenantId, ct);

        // 3. Si non trouvé, utiliser configuration par défaut
        if (config == null)
        {
            _logger.LogDebug(
                "Configuration rate limiting non trouvée en DB pour le tenant {TenantId}. Utilisation des valeurs par défaut.",
                tenantId);
            config = CreateDefaultConfiguration(tenantId);
        }
        else
        {
            _logger.LogDebug(
                "Configuration rate limiting chargée depuis PostgreSQL pour le tenant {TenantId}",
                tenantId);
        }

        // 4. Mettre en cache (TTL : 1 minute)
        try
        {
            await _cacheService.SetAsync(cacheKey, config, CacheTtl, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Erreur lors de la mise en cache Redis pour le tenant {TenantId}. La configuration reste fonctionnelle.",
                tenantId);
        }

        return config;
    }

    /// <summary>
    /// Met à jour la configuration de rate limiting pour un tenant.
    /// </summary>
    /// <param name="config">Nouvelle configuration à persister.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns>Configuration mise à jour.</returns>
    /// <remarks>
    /// Cette méthode invalide automatiquement le cache après mise à jour.
    /// </remarks>
    public async Task<TenantRateLimitConfiguration> UpdateConfigurationAsync(
        TenantRateLimitConfiguration config,
        CancellationToken ct = default)
    {
        // 1. Persister en base de données
        var updated = await _repository.UpsertAsync(config, ct);

        _logger.LogInformation(
            "Configuration rate limiting mise à jour pour le tenant {TenantId}",
            config.TenantId);

        // 2. Invalider le cache
        await InvalidateCacheAsync(config.TenantId, ct);

        return updated;
    }

    /// <summary>
    /// Supprime la configuration de rate limiting pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <remarks>
    /// Après suppression, le tenant utilisera la configuration par défaut.
    /// </remarks>
    public async Task DeleteConfigurationAsync(Guid tenantId, CancellationToken ct = default)
    {
        // 1. Supprimer de la base de données
        await _repository.DeleteAsync(tenantId, ct);

        _logger.LogInformation(
            "Configuration rate limiting supprimée pour le tenant {TenantId}. Retour aux valeurs par défaut.",
            tenantId);

        // 2. Invalider le cache
        await InvalidateCacheAsync(tenantId, ct);
    }

    /// <summary>
    /// Invalide le cache pour un tenant spécifique.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="ct">Token d'annulation.</param>
    public async Task InvalidateCacheAsync(Guid tenantId, CancellationToken ct = default)
    {
        var cacheKey = GetCacheKey(tenantId);

        try
        {
            await _cacheService.RemoveAsync(cacheKey, ct);

            _logger.LogDebug(
                "Cache rate limiting invalidé pour le tenant {TenantId}",
                tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Erreur lors de l'invalidation du cache Redis pour le tenant {TenantId}",
                tenantId);
        }
    }

    /// <summary>
    /// Génère la clé de cache pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <returns>Clé de cache formatée.</returns>
    private static string GetCacheKey(Guid tenantId) => $"{CacheKeyPrefix}{tenantId}";

    /// <summary>
    /// Crée la configuration par défaut pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <returns>Configuration par défaut.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Limites par défaut :</strong>
    /// <list type="bullet">
    /// <item><description>Globales : 1000 req/min, 100k tokens/min</description></item>
    /// <item><description>Par API Key : 100 req/min, 10k tokens/min</description></item>
    /// <item><description>Chat Completions : 60 req/min (coût élevé)</description></item>
    /// <item><description>Embeddings : 1000 req/min (coût faible)</description></item>
    /// <item><description>Images : 10 req/min (coût très élevé)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private static TenantRateLimitConfiguration CreateDefaultConfiguration(Guid tenantId)
    {
        return new TenantRateLimitConfiguration
        {
            TenantId = tenantId,
            GlobalLimit = new GlobalLimit
            {
                RequestsPerMinute = 1000,
                RequestsPerDay = 100_000,
                TokensPerMinute = 100_000,
                TokensPerDay = 10_000_000
            },
            ApiKeyLimit = new ApiKeyLimit
            {
                RequestsPerMinute = 100,
                TokensPerMinute = 10_000
            },
            EndpointLimits = new Dictionary<string, EndpointLimit>
            {
                // Chat Completions : Limite plus basse (coût élevé)
                ["/v1/chat/completions"] = new EndpointLimit
                {
                    RequestsPerMinute = 60,
                    TokensPerMinute = 100_000,
                    BurstCapacity = 120
                },

                // Embeddings : Limite plus haute (coût faible)
                ["/v1/embeddings"] = new EndpointLimit
                {
                    RequestsPerMinute = 1000,
                    TokensPerMinute = 500_000,
                    BurstCapacity = 2000
                },

                // Completions (legacy) : Même limite que chat
                ["/v1/completions"] = new EndpointLimit
                {
                    RequestsPerMinute = 60,
                    TokensPerMinute = 100_000,
                    BurstCapacity = 120
                },

                // Images : Limite basse (coût très élevé)
                ["/v1/images/generations"] = new EndpointLimit
                {
                    RequestsPerMinute = 10,
                    TokensPerMinute = 0, // Pas de concept de tokens pour les images
                    BurstCapacity = 20
                }
            }
        };
    }
}
