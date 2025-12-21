namespace LLMProxy.Gateway.Constants;

/// <summary>
/// Constantes pour les endpoints publics qui ne nécessitent pas d'authentification.
/// </summary>
public static class PublicEndpoints
{
    /// <summary>
    /// Endpoint de vérification de santé de l'application.
    /// </summary>
    public const string Health = "/health";
}

/// <summary>
/// Constantes pour les en-têtes HTTP utilisés dans l'application.
/// </summary>
public static class HttpHeaders
{
    /// <summary>
    /// En-tête standard d'autorisation (Bearer token, API key, etc.).
    /// </summary>
    public const string Authorization = "Authorization";

    /// <summary>
    /// En-tête personnalisé pour les clés API.
    /// </summary>
    public const string ApiKey = "X-API-Key";

    /// <summary>
    /// En-tête pour l'identifiant unique de requête.
    /// </summary>
    public const string RequestId = "X-Request-Id";
}

/// <summary>
/// Constantes pour les préfixes de schémas d'authentification.
/// </summary>
public static class AuthenticationSchemes
{
    /// <summary>
    /// Préfixe Bearer pour les tokens JWT.
    /// </summary>
    public const string Bearer = "Bearer ";
}
