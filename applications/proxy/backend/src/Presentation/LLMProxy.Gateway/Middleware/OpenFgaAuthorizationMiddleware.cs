using System.Security.Claims;
using LLMProxy.Infrastructure.Authorization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Middleware d'autorisation OpenFGA pour les requêtes API Gateway.
/// </summary>
/// <remarks>
/// <para>
/// Ce middleware s'exécute après l'authentification et vérifie les autorisations
/// ReBAC basées sur les attributs de la requête.
/// </para>
/// <para>
/// Il extrait automatiquement :
/// - L'identifiant utilisateur depuis les claims JWT
/// - Le tenant ID depuis le header X-Tenant-ID ou la route
/// - Les permissions requises depuis les métadonnées de l'endpoint
/// </para>
/// </remarks>
public sealed class OpenFgaAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<OpenFgaAuthorizationMiddleware> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="OpenFgaAuthorizationMiddleware"/>.
    /// </summary>
    /// <param name="next">Délégué de requête suivant.</param>
    /// <param name="logger">Logger pour les traces.</param>
    public OpenFgaAuthorizationMiddleware(
        RequestDelegate next,
        ILogger<OpenFgaAuthorizationMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Traite la requête HTTP et vérifie les autorisations.
    /// </summary>
    /// <param name="context">Contexte HTTP.</param>
    /// <param name="authorizationService">Service d'autorisation injecté.</param>
    /// <returns>Tâche représentant l'opération asynchrone.</returns>
    public async Task InvokeAsync(
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(authorizationService);

        // Récupérer les métadonnées d'autorisation de l'endpoint
        var endpoint = context.GetEndpoint();
        var authMetadata = endpoint?.Metadata.GetMetadata<FgaAuthorizeAttribute>();

        // Si pas de métadonnées d'autorisation, passer au suivant
        if (authMetadata is null)
        {
            await _next(context);
            return;
        }

        // Vérifier que l'utilisateur est authentifié
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Tentative d'accès à une ressource protégée sans authentification");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Non authentifié" });
            return;
        }

        // Extraire les informations d'autorisation
        var userId = ExtractUserId(context);
        var objectId = ExtractObjectId(context, authMetadata);

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Identifiant utilisateur non trouvé dans les claims");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Identifiant utilisateur invalide" });
            return;
        }

        if (string.IsNullOrEmpty(objectId))
        {
            _logger.LogWarning(
                "Identifiant d'objet non trouvé pour la vérification {ObjectType}",
                authMetadata.ObjectType);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Identifiant de ressource manquant" });
            return;
        }

        // Vérifier l'autorisation
        var request = new AuthorizationRequest(
            userId,
            authMetadata.Relation,
            authMetadata.ObjectType,
            objectId);

        var result = await authorizationService.CheckAsync(request, context.RequestAborted);

        if (result.IsDenied)
        {
            _logger.LogWarning(
                "Autorisation refusée: {UserId} {Relation} {ObjectType}:{ObjectId} - {Reason}",
                userId,
                authMetadata.Relation,
                authMetadata.ObjectType,
                objectId,
                result.Reason);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Accès interdit",
                message = result.Reason
            });
            return;
        }

        _logger.LogDebug(
            "Autorisation accordée: {UserId} {Relation} {ObjectType}:{ObjectId}",
            userId,
            authMetadata.Relation,
            authMetadata.ObjectType,
            objectId);

        // Stocker les informations d'autorisation dans le contexte pour usage ultérieur
        context.Items["AuthorizedUserId"] = userId;
        context.Items["AuthorizedObjectId"] = objectId;
        context.Items["AuthorizedRelation"] = authMetadata.Relation;

        await _next(context);
    }

    /// <summary>
    /// Extrait l'identifiant utilisateur depuis les claims JWT.
    /// </summary>
    private static string? ExtractUserId(HttpContext context)
    {
        // Essayer plusieurs claims standards
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub")
            ?? context.User.FindFirstValue(ClaimTypes.Email)
            ?? context.User.FindFirstValue("email");

        return userId;
    }

    /// <summary>
    /// Extrait l'identifiant de l'objet depuis la requête.
    /// </summary>
    private static string? ExtractObjectId(HttpContext context, FgaAuthorizeAttribute metadata)
    {
        // 1. Essayer le paramètre de route spécifié
        if (!string.IsNullOrEmpty(metadata.ObjectIdRouteParameter))
        {
            if (context.Request.RouteValues.TryGetValue(metadata.ObjectIdRouteParameter, out var routeValue))
            {
                return routeValue?.ToString();
            }
        }

        // 2. Essayer le header X-Tenant-ID pour les ressources tenant
        if (metadata.ObjectType == "tenant")
        {
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader))
            {
                return tenantHeader.FirstOrDefault();
            }
        }

        // 3. Essayer les paramètres de route communs
        var commonRouteParams = new[] { "id", "tenantId", "providerId", "apiKeyId", "routeId" };
        foreach (var param in commonRouteParams)
        {
            if (context.Request.RouteValues.TryGetValue(param, out var value))
            {
                return value?.ToString();
            }
        }

        // 4. Essayer le query string
        if (context.Request.Query.TryGetValue(metadata.ObjectIdRouteParameter ?? "id", out var queryValue))
        {
            return queryValue.FirstOrDefault();
        }

        return null;
    }
}
