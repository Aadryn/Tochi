using System.Diagnostics;
using System.Text.RegularExpressions;
using LLMProxy.Gateway.Extensions;

namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Middleware de logging structuré des requêtes et réponses HTTP
/// Conforme à ADR-054 (Request/Response Logging) et ADR-031 (Structured Logging)
/// </summary>
/// <remarks>
/// Fonctionnalités :
/// - Génère un RequestId unique pour chaque requête (correlation)
/// - Log le début de chaque requête (méthode, path, querystring sanitisé)
/// - Log la fin de chaque requête (status code, durée)
/// - Masque les données sensibles (API keys, Authorization headers)
/// - Mesure la durée d'exécution avec précision (Stopwatch)
/// - Support OpenTelemetry via ActivitySource
/// </remarks>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private static readonly ActivitySource _activitySource = new("LLMProxy.Gateway");

    /// <summary>
    /// Initialise une nouvelle instance du middleware de logging des requêtes
    /// </summary>
    /// <param name="next">Delegate vers le prochain middleware</param>
    /// <param name="logger">Logger pour les logs structurés</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Traite la requête HTTP avec logging structuré et masquage des données sensibles
    /// </summary>
    /// <param name="context">Contexte HTTP de la requête en cours</param>
    /// <param name="cancellationToken">Token d'annulation pour interrompre l'opération</param>
    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Générer RequestId unique pour correlation (format compact)
        var requestId = Guid.NewGuid().ToString("N");
        context.Items["RequestId"] = requestId;

        using var activity = _activitySource.StartActivity("ProcessRequest");
        activity?.SetTag("request.id", requestId);
        activity?.SetTag("request.method", context.Request.Method);
        activity?.SetTag("request.path", context.Request.Path);

        var sw = Stopwatch.StartNew();

        // Masquer données sensibles dans querystring et headers
        var sanitizedQueryString = SanitizeQueryString(context.Request.QueryString);
        var sanitizedHeaders = SanitizeHeaders(context.Request.Headers);

        try
        {
            // Log début de requête avec données sanitisées
            _logger.LogRequestStarted(
                context.Request.Method,
                $"{context.Request.Path}{sanitizedQueryString}",
                Guid.Parse(requestId));

            await _next(context);

            sw.Stop();

            // Déterminer niveau de log selon status code
            var level = context.Response.StatusCode >= 500 ? LogLevel.Error
                      : context.Response.StatusCode >= 400 ? LogLevel.Warning
                      : LogLevel.Information;

            if (level <= LogLevel.Information)
            {
                _logger.LogRequestCompleted(
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds,
                    Guid.Parse(requestId));
            }

            activity?.SetTag("response.status_code", context.Response.StatusCode);
            activity?.SetTag("response.duration_ms", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();

            _logger.LogRequestError(ex,
                context.Request.Method,
                context.Request.Path,
                Guid.Parse(requestId));

            activity?.SetTag("error", true);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);

            throw;
        }
    }

    /// <summary>
    /// Masque les paramètres sensibles dans le QueryString (API keys, tokens)
    /// </summary>
    /// <param name="queryString">QueryString original de la requête</param>
    /// <returns>QueryString avec valeurs sensibles masquées</returns>
    private static string SanitizeQueryString(QueryString queryString)
    {
        if (!queryString.HasValue)
        {
            return string.Empty;
        }

        var original = queryString.Value ?? string.Empty;

        // Masquer apikey= dans querystring (si présent)
        if (original.Contains("apikey=", StringComparison.OrdinalIgnoreCase))
        {
            return Regex.Replace(
                original,
                @"(apikey=)([^&]+)",
                "$1***MASKED***",
                RegexOptions.IgnoreCase);
        }

        return original;
    }

    /// <summary>
    /// Masque les headers sensibles (X-API-Key, Authorization, etc.)
    /// Conforme à ADR-054 : Protection des secrets dans les logs
    /// </summary>
    /// <param name="headers">Headers HTTP originaux de la requête</param>
    /// <returns>Dictionnaire avec headers sensibles masqués</returns>
    private static Dictionary<string, string> SanitizeHeaders(IHeaderDictionary headers)
    {
        var sanitized = new Dictionary<string, string>();

        foreach (var header in headers)
        {
            var key = header.Key;
            var value = header.Value.ToString();

            // Masquer X-API-Key (garder 4 premiers + 4 derniers caractères pour debug)
            if (key.Equals("X-API-Key", StringComparison.OrdinalIgnoreCase))
            {
                if (value.Length > 8)
                {
                    sanitized[key] = $"{value[..4]}***{value[^4..]}";
                }
                else
                {
                    sanitized[key] = "***MASKED***";
                }
            }
            // Masquer Authorization complètement (bearer tokens, basic auth)
            else if (key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                sanitized[key] = "***MASKED***";
            }
            // Headers non sensibles (User-Agent, Accept, Content-Type, etc.)
            else
            {
                sanitized[key] = value;
            }
        }

        return sanitized;
    }
}
