using LLMProxy.Domain.Interfaces;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Middleware for enforcing quota limits (requests and tokens)
/// Integrates with Redis for real-time quota tracking
/// </summary>
public class QuotaEnforcementMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<QuotaEnforcementMiddleware> _logger;
    private readonly IQuotaService _quotaService;

    public QuotaEnforcementMiddleware(
        RequestDelegate next, 
        ILogger<QuotaEnforcementMiddleware> logger,
        IQuotaService quotaService)
    {
        _next = next;
        _logger = logger;
        _quotaService = quotaService;
    }

    /// <summary>
    /// Invoque le middleware de vérification des quotas.
    /// </summary>
    /// <param name="context">Contexte HTTP de la requête en cours.</param>
    /// <param name="cancellationToken">Token d'annulation pour interrompre l'opération.</param>
    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        // Skip quota check for public endpoints
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        // Get user ID from context (set by authentication middleware)
        if (!context.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
        {
            // No user context, skip quota (authentication middleware will handle)
            await _next(context);
            return;
        }

        try
        {
            // Check request quota (per hour by default)
            var quotaCheck = await _quotaService.CheckQuotaAsync(userId, QuotaType.RequestsPerHour);
            if (!quotaCheck.IsAllowed)
            {
                _logger.LogWarning("Quota exceeded for user {UserId}: {Reason}", userId, quotaCheck.DenialReason);
                context.Response.StatusCode = 429; // Too Many Requests
                context.Response.Headers["X-RateLimit-Limit"] = quotaCheck.Usage?.Limit.ToString() ?? "0";
                context.Response.Headers["X-RateLimit-Remaining"] = quotaCheck.Usage?.Remaining.ToString() ?? "0";
                context.Response.Headers["X-RateLimit-Reset"] = quotaCheck.Usage?.WindowEnd.ToString("o") ?? "";
                context.Response.Headers["Retry-After"] = ((int)(quotaCheck.Usage?.WindowEnd.Subtract(DateTime.UtcNow).TotalSeconds ?? 60)).ToString();
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Rate limit exceeded",
                    message = quotaCheck.DenialReason,
                    limit = quotaCheck.Usage?.Limit,
                    remaining = 0,
                    reset = quotaCheck.Usage?.WindowEnd
                });
                return;
            }

            // Increment request quota
            await _quotaService.IncrementUsageAsync(userId, QuotaType.RequestsPerHour, 1);
            
            // Add quota headers to response
            if (quotaCheck.Usage != null)
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["X-RateLimit-Limit"] = quotaCheck.Usage.Limit.ToString();
                    context.Response.Headers["X-RateLimit-Remaining"] = quotaCheck.Usage.Remaining.ToString();
                    context.Response.Headers["X-RateLimit-Reset"] = quotaCheck.Usage.WindowEnd.ToString("o");
                    return Task.CompletedTask;
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking quota for user {UserId}", userId);
            // Don't block request on quota check errors, just log
        }

        await _next(context);

        // Token quota checking is done in StreamInterceptionMiddleware after counting tokens
    }
}
