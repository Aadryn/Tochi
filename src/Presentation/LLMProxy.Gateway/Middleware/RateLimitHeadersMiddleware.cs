using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Middleware pour enrichir les réponses avec headers de rate limiting.
/// Fournit transparence sur quotas restants et reset times.
/// Conforme à ADR-041 (Rate Limiting et Throttling).
/// </summary>
public sealed partial class RateLimitHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitHeadersMiddleware> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="RateLimitHeadersMiddleware"/>.
    /// </summary>
    /// <param name="next">Prochain middleware dans le pipeline.</param>
    /// <param name="logger">Logger pour traçabilité.</param>
    public RateLimitHeadersMiddleware(RequestDelegate next, ILogger<RateLimitHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Traite la requête HTTP et ajoute les headers de rate limiting.
    /// </summary>
    /// <param name="context">Contexte HTTP de la requête.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Ajouter headers informatifs sur rate limiting
        // Note: Les métadonnées détaillées du limiter ne sont pas directement accessibles
        // Cette implémentation ajoute des headers génériques
        // Une implémentation complète nécessiterait un tracking custom Redis

        if (!context.Response.Headers.ContainsKey("X-RateLimit-Policy"))
        {
            // Identifier quelle politique a été appliquée (basé sur présence de metadata)
            var tenantId = context.Items["TenantId"];
            var userId = context.Items["UserId"];
            
            string appliedPolicy;
            if (userId != null)
            {
                appliedPolicy = "per-user,per-tenant,global";
            }
            else if (tenantId != null)
            {
                appliedPolicy = "per-tenant,global";
            }
            else
            {
                appliedPolicy = "per-ip,global";
            }

            context.Response.Headers["X-RateLimit-Policy"] = appliedPolicy;
        }

        // Log si rate limit approché ou dépassé
        if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
        {
            LogRateLimitExceeded(
                _logger, 
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                context.Items["TenantId"]?.ToString() ?? "anonymous",
                context.Items["UserId"]?.ToString() ?? "anonymous");
        }
    }

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Warning,
        Message = "Rate limit exceeded - IP: {IpAddress}, TenantId: {TenantId}, UserId: {UserId}")]
    private static partial void LogRateLimitExceeded(
        ILogger logger,
        string ipAddress,
        string tenantId,
        string userId);
}
