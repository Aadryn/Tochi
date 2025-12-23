using LLMProxy.Application.Configuration.RateLimiting;

namespace LLMProxy.Application.Interfaces;

/// <summary>
/// Service de récupération des configurations de rate limiting pour un tenant.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Cette interface définit le contrat pour charger la configuration de rate limiting
/// d'un tenant. L'implémentation peut provenir de multiples sources :
/// <list type="bullet">
/// <item><description>Base de données (PostgreSQL avec cache Redis)</description></item>
/// <item><description>Fichier de configuration (appsettings.json)</description></item>
/// <item><description>Service distant (API de configuration centralisée)</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Exemple d'utilisation dans un middleware :</strong>
/// </para>
/// <code>
/// public class RateLimitingMiddleware
/// {
///     private readonly IRateLimitConfigurationService _configService;
///     
///     public async Task InvokeAsync(HttpContext context)
///     {
///         var tenantId = context.GetTenantId();
///         if (tenantId.HasValue)
///         {
///             var config = await _configService.GetConfigurationAsync(tenantId.Value);
///             
///             // Appliquer les limites configurées
///             var result = await CheckRateLimits(config);
///             if (!result.IsAllowed)
///             {
///                 context.Response.StatusCode = 429;
///                 return;
///             }
///         }
///         
///         await _next(context);
///     }
/// }
/// </code>
/// </para>
/// </remarks>
public interface IRateLimitConfigurationService
{
    /// <summary>
    /// Récupère la configuration de rate limiting pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant unique du tenant.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns>Configuration de rate limiting avec toutes les limites applicables.</returns>
    /// <remarks>
    /// <para>
    /// Cette méthode DOIT être rapide (< 10ms) car elle est appelée à chaque requête.
    /// L'implémentation DOIT utiliser un cache (Redis ou mémoire) pour éviter les
    /// lectures répétées en base de données.
    /// </para>
    /// <para>
    /// <strong>Pattern de cache recommandé :</strong>
    /// </para>
    /// <code>
    /// public async Task&lt;TenantRateLimitConfiguration&gt; GetConfigurationAsync(
    ///     Guid tenantId, 
    ///     CancellationToken ct = default)
    /// {
    ///     // 1. Chercher en cache
    ///     var cacheKey = $"ratelimit:config:{tenantId}";
    ///     var cached = await _cache.GetAsync&lt;TenantRateLimitConfiguration&gt;(cacheKey, ct);
    ///     if (cached != null)
    ///         return cached;
    ///     
    ///     // 2. Charger depuis DB
    ///     var config = await LoadFromDatabaseAsync(tenantId, ct);
    ///     
    ///     // 3. Mettre en cache (TTL court : 1 minute)
    ///     await _cache.SetAsync(cacheKey, config, TimeSpan.FromMinutes(1), ct);
    ///     
    ///     return config;
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// Si le tenant n'existe pas ou n'a pas de configuration personnalisée,
    /// retourner une configuration par défaut (valeurs standards).
    /// </para>
    /// </remarks>
    Task<TenantRateLimitConfiguration> GetConfigurationAsync(
        Guid tenantId,
        CancellationToken ct = default);
}
