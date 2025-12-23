namespace Authorization.API.Configuration;

/// <summary>
/// Options de configuration pour l'authentification JWT.
/// </summary>
public sealed class JwtConfiguration
{
    /// <summary>
    /// Section de configuration dans appsettings.json.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// URL de l'autorité (IDP - Keycloak).
    /// </summary>
    /// <example>http://localhost:8180/realms/llmproxy</example>
    public required string Authority { get; init; }

    /// <summary>
    /// Issuer attendu dans le token JWT.
    /// </summary>
    /// <example>http://localhost:8180/realms/llmproxy</example>
    public required string Issuer { get; init; }

    /// <summary>
    /// Audience attendue dans le token JWT.
    /// </summary>
    /// <example>authorization-api</example>
    public required string Audience { get; init; }

    /// <summary>
    /// Indique si HTTPS est requis pour les métadonnées.
    /// À désactiver uniquement en développement.
    /// </summary>
    public bool RequireHttpsMetadata { get; init; } = true;
}
