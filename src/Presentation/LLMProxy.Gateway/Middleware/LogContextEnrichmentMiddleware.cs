using LLMProxy.Infrastructure.Security;

namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Enrichit le contexte de logging avec les informations métier (TenantId, UserId, ApiKeyId).
/// </summary>
/// <remarks>
/// <para>
/// Ce middleware doit être placé APRÈS <see cref="ApiKeyAuthenticationMiddleware"/> pour avoir accès
/// au contexte utilisateur authentifié stocké dans <c>HttpContext.Items</c>.
/// </para>
/// <para>
/// Utilise <see cref="ILogger.BeginScope"/> pour propager automatiquement ces propriétés
/// à tous les logs de la requête en cours. Le scope est automatiquement libéré à la fin
/// de la requête grâce au <c>using</c>.
/// </para>
/// <para>
/// <strong>Ordre du pipeline middleware</strong> :
/// <list type="number">
/// <item><description><see cref="GlobalExceptionHandlerMiddleware"/></description></item>
/// <item><description><see cref="RequestLoggingMiddleware"/></description></item>
/// <item><description><see cref="ApiKeyAuthenticationMiddleware"/> ← Popule HttpContext.Items</description></item>
/// <item><description><see cref="LogContextEnrichmentMiddleware"/> ← LIT HttpContext.Items, enrichit LogContext</description></item>
/// <item><description><c>QuotaEnforcementMiddleware</c></description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Configuration dans Program.cs</strong> :</para>
/// <code>
/// app.UseMiddleware&lt;GlobalExceptionHandlerMiddleware&gt;();
/// app.UseMiddleware&lt;RequestLoggingMiddleware&gt;();
/// app.UseMiddleware&lt;ApiKeyAuthenticationMiddleware&gt;();
/// app.UseMiddleware&lt;LogContextEnrichmentMiddleware&gt;(); // NOUVEAU
/// app.UseMiddleware&lt;QuotaEnforcementMiddleware&gt;();
/// </code>
/// <para><strong>Résultat dans les logs</strong> :</para>
/// <para>Avant (sans contexte) :</para>
/// <code>
/// [2025-12-21 16:00:00 WRN] Revoked API key used: a1b2c3d4
/// </code>
/// <para>Après (avec contexte automatique) :</para>
/// <code>
/// [2025-12-21 16:00:00 WRN] Revoked API key used: a1b2c3d4
///   RequestId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
///   TenantId: 7c9e6679-7425-40de-944b-e07fc1f90ae7
///   UserId: 5f3a8d2b-1c4e-4a9f-9b8d-3e7f2c1a5b9d
///   ApiKeyId: a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d
/// </code>
/// </example>
public class LogContextEnrichmentMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogContextEnrichmentMiddleware> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="LogContextEnrichmentMiddleware"/>.
    /// </summary>
    /// <param name="next">Délégué vers le middleware suivant dans le pipeline.</param>
    /// <param name="logger">Logger pour diagnostics internes du middleware.</param>
    public LogContextEnrichmentMiddleware(
        RequestDelegate next,
        ILogger<LogContextEnrichmentMiddleware> logger)
    {
        Guard.AgainstNull(next, nameof(next));
        Guard.AgainstNull(logger, nameof(logger));

        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Traite la requête HTTP en enrichissant le contexte de logging avec les métadonnées métier.
    /// </summary>
    /// <param name="context">Contexte de la requête HTTP en cours.</param>
    /// <param name="cancellationToken">Jeton d'annulation pour supporter l'annulation de requête.</param>
    /// <returns>Tâche asynchrone représentant le traitement de la requête.</returns>
    /// <remarks>
    /// <para>
    /// Les propriétés suivantes sont extraites de <c>HttpContext.Items</c> et ajoutées au scope de logging :
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>RequestId</strong> : Toujours présent (créé par <see cref="RequestLoggingMiddleware"/>)</description></item>
    /// <item><description><strong>TenantId</strong> : Disponible après authentification réussie</description></item>
    /// <item><description><strong>UserId</strong> : Disponible après authentification réussie</description></item>
    /// <item><description><strong>ApiKeyId</strong> : Disponible après authentification par API key</description></item>
    /// </list>
    /// <para>
    /// Le scope est actif pour TOUS les logs émis durant le traitement de la requête,
    /// y compris dans les middlewares suivants et les contrôleurs/services.
    /// </para>
    /// </remarks>
    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context, nameof(context));

        // Extraire les métadonnées de contexte depuis HttpContext.Items
        var requestId = context.Items["RequestId"] as Guid?;
        var tenantId = context.Items["TenantId"] as Guid?;
        var userId = context.Items["UserId"] as Guid?;
        var apiKeyId = context.Items["ApiKeyId"] as Guid?;

        // Créer un dictionnaire pour le scope de logging
        var logScope = new Dictionary<string, object?>
        {
            ["RequestId"] = requestId,
            ["TenantId"] = tenantId,
            ["UserId"] = userId,
            ["ApiKeyId"] = apiKeyId
        };

        // Enrichir le contexte de logging avec un scope
        // Le scope sera automatiquement libéré à la fin du using
        using (_logger.BeginScope(logScope))
        {
            _logger.LogDebug(
                "Log context enriched - RequestId: {RequestId}, TenantId: {TenantId}, UserId: {UserId}, ApiKeyId: {ApiKeyId}",
                requestId, tenantId, userId, apiKeyId);

            await _next(context);
        }
    }
}
