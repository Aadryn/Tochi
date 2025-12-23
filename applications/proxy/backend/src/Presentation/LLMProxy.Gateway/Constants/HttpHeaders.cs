namespace LLMProxy.Gateway.Constants;

/// <summary>
/// Constantes pour les en-têtes HTTP utilisés dans l'application
/// </summary>
public static class HttpHeaders
{
    /// <summary>
    /// En-tête standard d'autorisation (Bearer token, API key, etc.)
    /// </summary>
    public const string Authorization = "Authorization";

    /// <summary>
    /// En-tête personnalisé pour les clés API
    /// </summary>
    public const string ApiKey = "X-API-Key";

    /// <summary>
    /// En-tête pour l'identifiant unique de requête
    /// </summary>
    public const string RequestId = "X-Request-Id";
}
