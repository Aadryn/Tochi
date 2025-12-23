namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Constantes pour les en-têtes HTTP utilisés dans l'authentification par clé API.
/// </summary>
/// <remarks>
/// Cette classe contient les noms des en-têtes HTTP utilisés pour extraire les clés API.
/// Classe interne pour éviter la pollution du namespace public et garantir l'encapsulation.
/// Conforme à ADR-016 (Explicit over Implicit).
/// </remarks>
internal static class HttpHeaderConstants
{
    /// <summary>
    /// Nom de l'en-tête HTTP Authorization standard.
    /// </summary>
    /// <remarks>
    /// Utilisé avec le schéma "Bearer" : Authorization: Bearer sk-abc123...
    /// Conforme à la spécification RFC 6750 (OAuth 2.0 Bearer Token).
    /// </remarks>
    public const string Authorization = "Authorization";

    /// <summary>
    /// Nom de l'en-tête HTTP personnalisé pour les clés API.
    /// </summary>
    /// <remarks>
    /// En-tête alternatif si Authorization n'est pas utilisé : X-API-Key: sk-abc123...
    /// Convention courante pour les APIs REST.
    /// </remarks>
    public const string ApiKey = "X-API-Key";

    /// <summary>
    /// Préfixe du schéma Bearer dans l'en-tête Authorization.
    /// </summary>
    /// <remarks>
    /// Format attendu : "Bearer sk-abc123..."
    /// Le préfixe inclut l'espace final pour simplifier l'extraction.
    /// </remarks>
    public const string Bearer = "Bearer ";
}
