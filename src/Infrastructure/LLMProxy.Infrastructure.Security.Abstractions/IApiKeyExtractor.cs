using Microsoft.AspNetCore.Http;

namespace LLMProxy.Infrastructure.Security.Abstractions;

/// <summary>
/// Service d'extraction des clés API depuis les en-têtes HTTP.
/// </summary>
/// <remarks>
/// Cette interface définit le contrat pour extraire les clés API
/// des requêtes HTTP entrantes, en respectant les bonnes pratiques de sécurité.
/// Conforme à ADR-005 (Single Responsibility Principle).
/// </remarks>
public interface IApiKeyExtractor
{
    /// <summary>
    /// Extrait la clé API des en-têtes HTTP de la requête.
    /// </summary>
    /// <param name="context">Contexte HTTP de la requête contenant les en-têtes.</param>
    /// <returns>
    /// Clé API extraite si présente dans les en-têtes, sinon null.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Tente d'extraire la clé API dans l'ordre de priorité suivant :
    /// </para>
    /// <list type="number">
    /// <item><description>En-tête Authorization avec schéma Bearer (standard OAuth 2.0)</description></item>
    /// <item><description>En-tête personnalisé X-API-Key (convention REST commune)</description></item>
    /// </list>
    /// <para>
    /// IMPORTANT : Les query parameters ne sont PAS supportés pour des raisons de sécurité.
    /// Raisons : éviter le logging des clés API dans les logs serveur, historique navigateur,
    /// et logs de proxy/CDN qui capturent souvent les URLs complètes.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var apiKey = _extractor.ExtractApiKey(httpContext);
    /// if (apiKey == null)
    /// {
    ///     _logger.LogWarning("Aucune clé API trouvée dans la requête");
    ///     return Results.Unauthorized();
    /// }
    /// </code>
    /// </example>
    string? ExtractApiKey(HttpContext context);
}
