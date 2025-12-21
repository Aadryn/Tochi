using Microsoft.AspNetCore.Http;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Implémentation du service d'extraction de clés API depuis les en-têtes HTTP.
/// </summary>
/// <remarks>
/// Ce service extrait les clés API des requêtes HTTP en recherchant dans les en-têtes
/// Authorization (Bearer) et X-API-Key, dans cet ordre de priorité.
/// Les query parameters ne sont PAS supportés pour des raisons de sécurité
/// (éviter le logging des clés dans les logs serveur).
/// Conforme à ADR-005 (SRP), ADR-009 (Fail Fast) et ADR-027 (Defensive Programming).
/// </remarks>
public class HeaderApiKeyExtractor : IApiKeyExtractor
{
    /// <summary>
    /// Extrait la clé API depuis le contexte HTTP.
    /// Recherche dans Authorization (Bearer) puis X-API-Key headers.
    /// </summary>
    /// <param name="context">Contexte HTTP de la requête.</param>
    /// <returns>Clé API extraite, ou null si non trouvée.</returns>
    public string? ExtractApiKey(HttpContext context)
    {
        Guard.AgainstNull(context, nameof(context));

        // Try Authorization header first (Bearer token format)
        if (context.Request.Headers.TryGetValue(HttpHeaderConstants.Authorization, out var authHeader))
        {
            var headerValue = authHeader.ToString();
            if (headerValue.StartsWith(HttpHeaderConstants.Bearer, StringComparison.OrdinalIgnoreCase))
            {
                return headerValue.Substring(HttpHeaderConstants.Bearer.Length).Trim();
            }
        }

        // Try X-API-Key header
        if (context.Request.Headers.TryGetValue(HttpHeaderConstants.ApiKey, out var apiKeyHeader))
        {
            return apiKeyHeader.ToString();
        }

        // Query parameter NOT supported for security reasons
        // API keys must be sent via headers only to prevent logging in server logs

        return null;
    }
}
