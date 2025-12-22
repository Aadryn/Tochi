using LLMProxy.Application.Interfaces;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LLMProxy.Presentation.Middleware;

/// <summary>
/// Middleware ASP.NET Core pour appliquer le rate limiting sur toutes les requêtes.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Ce middleware implémente une stratégie de rate limiting à 4 niveaux :
/// <list type="number">
/// <item><description><strong>Niveau IP</strong> : Limite par adresse IP (protection DDoS)</description></item>
/// <item><description><strong>Niveau Tenant</strong> : Limites globales du tenant (quota)</description></item>
/// <item><description><strong>Niveau API Key</strong> : Limites par clé d'API (distribution équitable)</description></item>
/// <item><description><strong>Niveau Endpoint</strong> : Limites spécifiques par opération</description></item>
/// </list>
/// </para>
/// <para>
/// Les vérifications sont effectuées en cascade : si un niveau rejette la requête,
/// les niveaux suivants ne sont pas vérifiés (optimisation).
/// </para>
/// <para>
/// <strong>Headers HTTP ajoutés à la réponse :</strong>
/// <list type="bullet">
/// <item><description>X-RateLimit-Limit : Limite maximale</description></item>
/// <item><description>X-RateLimit-Remaining : Requêtes restantes</description></item>
/// <item><description>X-RateLimit-Reset : Timestamp de réinitialisation (Unix epoch)</description></item>
/// <item><description>Retry-After : Secondes à attendre avant de réessayer (si rejeté)</description></item>
/// </list>
/// </para>
/// <para>
/// En cas de rejet, retourne HTTP 429 Too Many Requests avec corps JSON :
/// <code>
/// {
///   "error": {
///     "message": "Rate limit exceeded",
///     "type": "rate_limit_error",
///     "code": "rate_limit_exceeded"
///   }
/// }
/// </code>
/// </para>
/// </remarks>
public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimiter _rateLimiter;
    private readonly IRateLimitConfigurationService _configService;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    /// <summary>
    /// Initialise une nouvelle instance du middleware de rate limiting.
    /// </summary>
    /// <param name="next">Prochain middleware dans le pipeline.</param>
    /// <param name="rateLimiter">Service de rate limiting (Redis).</param>
    /// <param name="configService">Service de configuration des limites par tenant.</param>
    /// <param name="logger">Logger pour traçabilité.</param>
    public RateLimitingMiddleware(
        RequestDelegate next,
        IRateLimiter rateLimiter,
        IRateLimitConfigurationService configService,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Traite une requête HTTP en appliquant le rate limiting.
    /// </summary>
    /// <param name="context">Contexte de la requête HTTP.</param>
    /// <returns>Tâche asynchrone.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Extraire les identifiants depuis le contexte
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var tenantId = ExtractTenantId(context);
        var apiKey = ExtractApiKey(context);
        var endpoint = context.Request.Path.Value ?? "/";

        _logger.LogDebug(
            "Rate limiting check - IP: {IP}, Tenant: {TenantId}, ApiKey: {ApiKey}, Endpoint: {Endpoint}",
            ipAddress, tenantId, apiKey, endpoint);

        // Niveau 1 : Vérification par IP (protection DDoS)
        var ipResult = await CheckIpRateLimitAsync(ipAddress);
        if (!ipResult.IsAllowed)
        {
            await RejectRequestAsync(context, ipResult, "IP", ipAddress);
            LogRateLimitExceeded("IP", ipAddress, stopwatch.ElapsedMilliseconds);
            return;
        }

        // Si tenant non identifié, autoriser (géré par authentification)
        if (!tenantId.HasValue)
        {
            await _next(context);
            return;
        }

        // Charger configuration du tenant
        var config = await _configService.GetConfigurationAsync(tenantId.Value, context.RequestAborted);

        // Niveau 2 : Vérification globale du tenant
        var tenantResult = await CheckTenantGlobalLimitAsync(tenantId.Value, config);
        if (!tenantResult.IsAllowed)
        {
            await RejectRequestAsync(context, tenantResult, "Tenant", tenantId.ToString()!);
            LogRateLimitExceeded("Tenant", tenantId.ToString()!, stopwatch.ElapsedMilliseconds);
            return;
        }

        // Niveau 3 : Vérification par API Key (si présente)
        if (!string.IsNullOrEmpty(apiKey))
        {
            var apiKeyResult = await CheckApiKeyLimitAsync(tenantId.Value, apiKey, config);
            if (!apiKeyResult.IsAllowed)
            {
                await RejectRequestAsync(context, apiKeyResult, "ApiKey", apiKey);
                LogRateLimitExceeded("ApiKey", apiKey, stopwatch.ElapsedMilliseconds);
                return;
            }
        }

        // Niveau 4 : Vérification par endpoint
        var endpointResult = await CheckEndpointLimitAsync(tenantId.Value, endpoint, config);
        if (!endpointResult.IsAllowed)
        {
            await RejectRequestAsync(context, endpointResult, "Endpoint", endpoint);
            LogRateLimitExceeded("Endpoint", endpoint, stopwatch.ElapsedMilliseconds);
            return;
        }

        // Toutes les vérifications passées : ajouter les headers informatifs
        AddRateLimitHeaders(context.Response, endpointResult);

        _logger.LogDebug(
            "Rate limiting passed - Tenant: {TenantId}, Endpoint: {Endpoint}, Duration: {Duration}ms",
            tenantId, endpoint, stopwatch.ElapsedMilliseconds);

        await _next(context);
    }

    /// <summary>
    /// Vérifie la limite de rate par adresse IP.
    /// </summary>
    private async Task<RateLimitResult> CheckIpRateLimitAsync(string ipAddress)
    {
        // Limite IP : 100 requêtes/minute (protection DDoS basique)
        var key = $"ratelimit:ip:{ipAddress}";
        return await _rateLimiter.CheckSlidingWindowAsync(key, limit: 100, window: TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Vérifie la limite globale du tenant.
    /// </summary>
    private async Task<RateLimitResult> CheckTenantGlobalLimitAsync(
        Guid tenantId,
        Application.Configuration.RateLimiting.TenantRateLimitConfiguration config)
    {
        // Utiliser Token Bucket pour permettre les bursts
        var key = $"ratelimit:tenant:{tenantId}:global";
        return await _rateLimiter.CheckTokenBucketAsync(
            key,
            capacity: config.GlobalLimit.RequestsPerMinute * 2, // Burst = 2× limite/min
            tokensPerInterval: config.GlobalLimit.RequestsPerMinute,
            interval: TimeSpan.FromMinutes(1),
            tokensRequired: 1);
    }

    /// <summary>
    /// Vérifie la limite par API Key.
    /// </summary>
    private async Task<RateLimitResult> CheckApiKeyLimitAsync(
        Guid tenantId,
        string apiKey,
        Application.Configuration.RateLimiting.TenantRateLimitConfiguration config)
    {
        var key = $"ratelimit:tenant:{tenantId}:apikey:{apiKey}";
        return await _rateLimiter.CheckTokenBucketAsync(
            key,
            capacity: config.ApiKeyLimit.RequestsPerMinute * 2,
            tokensPerInterval: config.ApiKeyLimit.RequestsPerMinute,
            interval: TimeSpan.FromMinutes(1),
            tokensRequired: 1);
    }

    /// <summary>
    /// Vérifie la limite par endpoint.
    /// </summary>
    private async Task<RateLimitResult> CheckEndpointLimitAsync(
        Guid tenantId,
        string endpoint,
        Application.Configuration.RateLimiting.TenantRateLimitConfiguration config)
    {
        // Chercher configuration spécifique à l'endpoint
        if (!config.EndpointLimits.TryGetValue(endpoint, out var endpointLimit))
        {
            // Utiliser limite par défaut
            endpointLimit = new Application.Configuration.RateLimiting.EndpointLimit();
        }

        var key = $"ratelimit:tenant:{tenantId}:endpoint:{endpoint}";
        return await _rateLimiter.CheckTokenBucketAsync(
            key,
            capacity: endpointLimit.BurstCapacity,
            tokensPerInterval: endpointLimit.RequestsPerMinute,
            interval: TimeSpan.FromMinutes(1),
            tokensRequired: 1);
    }

    /// <summary>
    /// Rejette une requête avec HTTP 429 et headers appropriés.
    /// </summary>
    private async Task RejectRequestAsync(
        HttpContext context,
        RateLimitResult result,
        string limitType,
        string identifier)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";

        // Ajouter headers de rate limiting
        AddRateLimitHeaders(context.Response, result);

        // Corps de la réponse conforme à l'API OpenAI
        var errorResponse = new
        {
            error = new
            {
                message = $"Rate limit exceeded for {limitType}: {identifier}",
                type = "rate_limit_error",
                code = "rate_limit_exceeded"
            }
        };

        await context.Response.WriteAsJsonAsync(errorResponse, context.RequestAborted);
    }

    /// <summary>
    /// Ajoute les headers HTTP de rate limiting à la réponse.
    /// </summary>
    private void AddRateLimitHeaders(HttpResponse response, RateLimitResult result)
    {
        response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
        response.Headers["X-RateLimit-Remaining"] = Math.Max(0, result.Limit - result.CurrentCount).ToString();
        
        // Reset = maintenant + window
        var resetTimestamp = DateTimeOffset.UtcNow.Add(result.Window).ToUnixTimeSeconds();
        response.Headers["X-RateLimit-Reset"] = resetTimestamp.ToString();

        // Retry-After si rejeté
        if (!result.IsAllowed && result.RetryAfter.HasValue)
        {
            response.Headers["Retry-After"] = ((int)result.RetryAfter.Value.TotalSeconds).ToString();
        }
    }

    /// <summary>
    /// Extrait le Tenant ID depuis le contexte HTTP.
    /// </summary>
    private Guid? ExtractTenantId(HttpContext context)
    {
        // Priorité 1 : Claims de l'utilisateur authentifié
        var tenantClaim = context.User.FindFirst("tenant_id");
        if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            return tenantId;

        // Priorité 2 : Header X-Tenant-Id
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue) &&
            Guid.TryParse(headerValue.ToString(), out tenantId))
            return tenantId;

        return null;
    }

    /// <summary>
    /// Extrait l'API Key depuis le contexte HTTP.
    /// </summary>
    private string? ExtractApiKey(HttpContext context)
    {
        // Priorité 1 : Header Authorization (Bearer token)
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return authHeader.Substring(7);

        // Priorité 2 : Header X-Api-Key
        if (context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
            return apiKey.ToString();

        return null;
    }

    /// <summary>
    /// Log un dépassement de rate limit.
    /// </summary>
    private void LogRateLimitExceeded(string limitType, string identifier, long durationMs)
    {
        _logger.LogWarning(
            "Rate limit exceeded - Type: {LimitType}, Identifier: {Identifier}, Duration: {Duration}ms",
            limitType, identifier, durationMs);
    }
}
