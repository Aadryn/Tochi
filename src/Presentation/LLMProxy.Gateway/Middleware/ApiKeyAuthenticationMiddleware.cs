using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.Security;
using LLMProxy.Gateway.Constants;
using LLMProxy.Gateway.Extensions;

namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Middleware for API Key authentication
/// Validates API keys from Authorization header or query parameter
/// </summary>
public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IApiKeyExtractor _apiKeyExtractor;
    private readonly IApiKeyAuthenticator _apiKeyAuthenticator;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next, 
        ILogger<ApiKeyAuthenticationMiddleware> logger,
        IServiceScopeFactory serviceScopeFactory,
        IApiKeyExtractor apiKeyExtractor,
        IApiKeyAuthenticator apiKeyAuthenticator)
    {
        _next = next;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _apiKeyExtractor = apiKeyExtractor;
        _apiKeyAuthenticator = apiKeyAuthenticator;
    }

    /// <summary>
    /// Invoque le middleware d'authentification par clé API.
    /// </summary>
    /// <param name="context">Contexte HTTP de la requête en cours.</param>
    /// <param name="cancellationToken">Token d'annulation pour interrompre l'opération.</param>
    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        // Skip authentication for health checks and other public endpoints
        if (context.Request.Path.StartsWithSegments(PublicEndpoints.Health))
        {
            await _next(context);
            return;
        }

        var apiKey = _apiKeyExtractor.ExtractApiKey(context);

        try
        {
            Guard.AgainstNullOrWhiteSpace(apiKey, nameof(apiKey), "API key is missing");
        }
        catch (ArgumentException)
        {
            _logger.LogRequestWithoutApiKey(context.Request.Path);
            Guard.AgainstResponseStarted(context.Response);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API key is required" });
            return;
        }

        // Authenticate API key via orchestrator
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        var authResult = await _apiKeyAuthenticator.AuthenticateAsync(apiKey!, unitOfWork, cancellationToken);
        
        if (!authResult.IsAuthenticated)
        {
            Guard.AgainstResponseStarted(context.Response);
            context.Response.StatusCode = authResult.StatusCode;
            await context.Response.WriteAsJsonAsync(new { error = authResult.ErrorMessage });
            return;
        }
        
        // Store user context in HttpContext.Items for downstream use
        context.Items["UserId"] = authResult.User!.Id;
        context.Items["TenantId"] = authResult.User!.TenantId;
        context.Items["ApiKeyId"] = authResult.ApiKey!.Id;
        context.Items["UserRole"] = authResult.User.Role.ToString();

        await _next(context);
    }
}
