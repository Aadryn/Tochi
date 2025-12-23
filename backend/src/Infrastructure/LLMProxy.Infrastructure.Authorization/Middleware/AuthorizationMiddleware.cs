// <copyright file="AuthorizationMiddleware.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Security.Claims;
using LLMProxy.Infrastructure.Authorization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.Authorization.Middleware;

/// <summary>
/// Middleware d'autorisation basé sur les permissions.
/// </summary>
/// <remarks>
/// <para>
/// Ce middleware vérifie automatiquement les permissions pour chaque requête
/// en fonction des attributs [RequirePermission] sur les endpoints.
/// </para>
/// <para>
/// Si l'endpoint n'a pas d'attribut [RequirePermission], la requête passe sans vérification.
/// </para>
/// </remarks>
public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    /// <summary>
    /// Initialise une nouvelle instance du middleware.
    /// </summary>
    /// <param name="next">Délégué de requête suivant.</param>
    /// <param name="logger">Logger.</param>
    public AuthorizationMiddleware(
        RequestDelegate next,
        ILogger<AuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Traite la requête et vérifie les autorisations si nécessaire.
    /// </summary>
    /// <param name="context">Contexte HTTP.</param>
    /// <param name="authorizationService">Service d'autorisation.</param>
    public async Task InvokeAsync(
        HttpContext context,
        IAuthorizationService authorizationService)
    {
        // Récupérer les métadonnées de l'endpoint
        var endpoint = context.GetEndpoint();

        if (endpoint is null)
        {
            await _next(context);
            return;
        }

        // Chercher l'attribut RequirePermission
        var requirePermission = endpoint.Metadata.GetMetadata<RequirePermissionAttribute>();

        if (requirePermission is null)
        {
            // Pas d'exigence de permission, continuer
            await _next(context);
            return;
        }

        // Vérifier l'authentification
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Accès refusé: utilisateur non authentifié");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = "Authentication required"
            });
            return;
        }

        // Extraire l'identifiant utilisateur
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Accès refusé: identifiant utilisateur manquant");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = "User identifier claim not found"
            });
            return;
        }

        // Résoudre le scope (objectId)
        var objectId = ResolveObjectId(context, requirePermission);

        if (string.IsNullOrEmpty(objectId))
        {
            _logger.LogWarning(
                "Accès refusé: impossible de résoudre l'identifiant d'objet pour {ObjectType}",
                requirePermission.ObjectType);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "BadRequest",
                message = $"Unable to resolve {requirePermission.ObjectType} identifier from request"
            });
            return;
        }

        // Vérifier l'autorisation
        var result = await authorizationService.CheckAsync(
            userId,
            requirePermission.Relation,
            requirePermission.ObjectType,
            objectId,
            context.RequestAborted);

        if (!result.IsAllowed)
        {
            _logger.LogWarning(
                "Accès refusé: {UserId} n'a pas la permission {Relation} sur {ObjectType}:{ObjectId}",
                userId,
                requirePermission.Relation,
                requirePermission.ObjectType,
                objectId);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Forbidden",
                message = result.Reason ?? "Access denied"
            });
            return;
        }

        _logger.LogDebug(
            "Autorisation accordée: {UserId} {Relation} {ObjectType}:{ObjectId}",
            userId,
            requirePermission.Relation,
            requirePermission.ObjectType,
            objectId);

        await _next(context);
    }

    /// <summary>
    /// Résout l'identifiant d'objet à partir de la requête.
    /// </summary>
    private static string? ResolveObjectId(
        HttpContext context,
        RequirePermissionAttribute attribute)
    {
        // 1. D'abord chercher dans les route values
        if (!string.IsNullOrEmpty(attribute.RouteParameter))
        {
            if (context.Request.RouteValues.TryGetValue(attribute.RouteParameter, out var routeValue))
            {
                return routeValue?.ToString();
            }
        }

        // 2. Chercher un paramètre standard basé sur le type d'objet
        var candidateNames = new[]
        {
            $"{attribute.ObjectType}Id",
            $"{attribute.ObjectType}_id",
            "id",
            "Id"
        };

        foreach (var name in candidateNames)
        {
            if (context.Request.RouteValues.TryGetValue(name, out var value))
            {
                return value?.ToString();
            }

            if (context.Request.Query.TryGetValue(name, out var queryValue))
            {
                return queryValue.FirstOrDefault();
            }
        }

        // 3. Pour tenant, chercher dans le header ou claim
        if (attribute.ObjectType.Equals("tenant", StringComparison.OrdinalIgnoreCase))
        {
            // Header X-Tenant-Id
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader))
            {
                return tenantHeader.FirstOrDefault();
            }

            // Claim tenant_id
            var tenantClaim = context.User.FindFirst("tenant_id")
                ?? context.User.FindFirst("tid");

            if (tenantClaim is not null)
            {
                return tenantClaim.Value;
            }
        }

        return null;
    }
}

/// <summary>
/// Attribut pour spécifier les exigences de permission sur un endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : Attribute
{
    /// <summary>
    /// Relation requise (ex: "can_read", "can_write", "operator").
    /// </summary>
    public string Relation { get; }

    /// <summary>
    /// Type d'objet sur lequel la permission est vérifiée (ex: "tenant", "provider").
    /// </summary>
    public string ObjectType { get; }

    /// <summary>
    /// Nom du paramètre de route contenant l'identifiant d'objet.
    /// Si non spécifié, sera déduit automatiquement.
    /// </summary>
    public string? RouteParameter { get; set; }

    /// <summary>
    /// Initialise une nouvelle instance de l'attribut.
    /// </summary>
    /// <param name="relation">Relation requise.</param>
    /// <param name="objectType">Type d'objet.</param>
    public RequirePermissionAttribute(string relation, string objectType)
    {
        Relation = relation;
        ObjectType = objectType;
    }
}
