using System.Security.Claims;
using Authorization.Application.Services.Authorization;
using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.Idp.Sync;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authorization.Infrastructure.Idp.Middleware;

/// <summary>
/// Middleware pour la synchronisation JIT (Just-In-Time) des groupes utilisateur.
/// Synchronise les appartenances aux groupes lors de la première requête après authentification.
/// </summary>
public class JitSyncMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JitSyncMiddleware> _logger;
    private readonly IdpConfiguration _configuration;

    /// <summary>
    /// Initialise une nouvelle instance du middleware.
    /// </summary>
    public JitSyncMiddleware(
        RequestDelegate next,
        ILogger<JitSyncMiddleware> logger,
        IOptions<IdpConfiguration> configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration.Value;
    }

    /// <summary>
    /// Traite la requête et synchronise les groupes si nécessaire.
    /// </summary>
    public async Task InvokeAsync(
        HttpContext context,
        IIdpSyncService syncService,
        IRbacAuthorizationService authorizationService)
    {
        // Ignorer si JIT désactivé
        if (!_configuration.EnableJitSync)
        {
            await _next(context);
            return;
        }

        // Ignorer si pas authentifié
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        try
        {
            await SyncUserGroupsAsync(context, syncService);
        }
        catch (Exception ex)
        {
            // Log mais ne bloque pas la requête
            _logger.LogWarning(
                ex,
                "JIT sync failed for user {UserId}, continuing with existing permissions",
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

        await _next(context);
    }

    /// <summary>
    /// Synchronise les groupes de l'utilisateur courant.
    /// </summary>
    private async Task SyncUserGroupsAsync(HttpContext context, IIdpSyncService syncService)
    {
        // Extraire les informations du token JWT
        var subjectClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirst("sub");

        var tenantClaim = context.User.FindFirst("tenant_id")
            ?? context.User.FindFirst("tid");

        if (subjectClaim is null || tenantClaim is null)
        {
            _logger.LogDebug("Missing subject or tenant claim, skipping JIT sync");
            return;
        }

        // Parser les claims en Value Objects
        if (!PrincipalId.TryParse(subjectClaim.Value, out var userId))
        {
            _logger.LogDebug("Invalid subject claim format '{Subject}', skipping JIT sync", subjectClaim.Value);
            return;
        }

        var tenantId = TenantId.Create(tenantClaim.Value);

        // Vérifier si une sync récente a eu lieu (via header custom)
        var lastSyncHeader = context.Request.Headers["X-Last-Group-Sync"].FirstOrDefault();
        if (!string.IsNullOrEmpty(lastSyncHeader) &&
            DateTime.TryParse(lastSyncHeader, out var lastSync) &&
            (DateTime.UtcNow - lastSync).TotalMinutes < 5)
        {
            _logger.LogDebug("Skipping JIT sync, last sync was {Minutes} minutes ago", (DateTime.UtcNow - lastSync).TotalMinutes);
            return;
        }

        // Synchroniser les groupes
        _logger.LogDebug("Starting JIT sync for user {UserId} in tenant {TenantId}", userId, tenantId);

        var result = await syncService.SyncUserGroupsAsync(tenantId, userId, context.RequestAborted);

        if (result.Success)
        {
            _logger.LogInformation(
                "JIT sync completed for user {UserId}: +{Added} -{Removed} groups",
                userId,
                result.AddedCount,
                result.RemovedCount);

            // Ajouter un header pour éviter les syncs répétées
            context.Response.Headers["X-Last-Group-Sync"] = DateTime.UtcNow.ToString("O");
        }
        else
        {
            _logger.LogWarning(
                "JIT sync failed for user {UserId}: {Error}",
                userId,
                result.ErrorMessage);
        }
    }
}

/// <summary>
/// Extensions pour enregistrer le middleware JIT.
/// </summary>
public static class JitSyncMiddlewareExtensions
{
    /// <summary>
    /// Ajoute le middleware de synchronisation JIT.
    /// </summary>
    public static IApplicationBuilder UseJitSync(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JitSyncMiddleware>();
    }
}
