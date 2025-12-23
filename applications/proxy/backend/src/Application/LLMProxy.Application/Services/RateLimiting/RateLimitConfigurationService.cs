using LLMProxy.Application.Configuration.RateLimiting;
using LLMProxy.Application.Interfaces;

namespace LLMProxy.Application.Services.RateLimiting;

/// <summary>
/// Implémentation par défaut du service de configuration de rate limiting.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Cette implémentation fournit des configurations par défaut pour tous les tenants.
/// Dans un scénario de production, cette classe devrait être remplacée par une version
/// qui charge les configurations depuis une base de données avec cache Redis.
/// </para>
/// <para>
/// <strong>Configuration par défaut appliquée :</strong>
/// <list type="bullet">
/// <item><description>Limites globales : 1000 req/min, 100k tokens/min</description></item>
/// <item><description>Limites par API Key : 100 req/min, 10k tokens/min</description></item>
/// <item><description>Limites par endpoint : 100 req/min, 50k tokens/min</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class RateLimitConfigurationService : IRateLimitConfigurationService
{
    /// <summary>
    /// Récupère la configuration de rate limiting pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant unique du tenant.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns>Configuration par défaut pour tous les tenants.</returns>
    /// <remarks>
    /// <para>
    /// Cette implémentation retourne la même configuration pour tous les tenants.
    /// Dans une version de production, remplacer par :
    /// </para>
    /// <code>
    /// // 1. Vérifier cache Redis
    /// var cacheKey = $"ratelimit:config:{tenantId}";
    /// var cached = await _redisCache.GetAsync&lt;TenantRateLimitConfiguration&gt;(cacheKey, ct);
    /// if (cached != null)
    ///     return cached;
    /// 
    /// // 2. Charger depuis PostgreSQL
    /// var config = await _dbContext.TenantRateLimitConfigs
    ///     .Include(c => c.EndpointLimits)
    ///     .FirstOrDefaultAsync(c => c.TenantId == tenantId, ct);
    /// 
    /// // 3. Si non trouvé, utiliser config par défaut
    /// if (config == null)
    ///     config = CreateDefaultConfiguration(tenantId);
    /// 
    /// // 4. Mettre en cache (TTL : 1 minute)
    /// await _redisCache.SetAsync(cacheKey, config, TimeSpan.FromMinutes(1), ct);
    /// 
    /// return config;
    /// </code>
    /// </para>
    /// </remarks>
    public Task<TenantRateLimitConfiguration> GetConfigurationAsync(
        Guid tenantId,
        CancellationToken ct = default)
    {
        // Configuration par défaut pour tous les tenants
        // TODO: Remplacer par chargement depuis DB + cache Redis en production
        var config = new TenantRateLimitConfiguration
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

        return Task.FromResult(config);
    }
}
