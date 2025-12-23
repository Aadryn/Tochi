namespace Authorization.Infrastructure.OpenFGA.Configuration;

/// <summary>
/// Options de configuration pour OpenFGA.
/// </summary>
public sealed class OpenFgaOptions
{
    /// <summary>
    /// Section de configuration.
    /// </summary>
    public const string SectionName = "OpenFGA";

    /// <summary>
    /// URL de l'API OpenFGA.
    /// </summary>
    /// <example>http://openfga:8080</example>
    public string ApiUrl { get; set; } = "http://localhost:8080";

    /// <summary>
    /// Identifiant du store par défaut (null pour multi-tenant).
    /// </summary>
    public string? DefaultStoreId { get; set; }

    /// <summary>
    /// Préfixe pour les noms de stores (multi-tenant).
    /// </summary>
    public string StorePrefix { get; set; } = "authz";

    /// <summary>
    /// Timeout des requêtes en secondes.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Nombre maximum de tentatives en cas d'échec.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Délai entre les tentatives (en millisecondes).
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Active le cache des vérifications de permissions.
    /// </summary>
    public bool EnableCache { get; set; } = true;

    /// <summary>
    /// Durée de vie du cache (en secondes).
    /// </summary>
    public int CacheTtlSeconds { get; set; } = 60;

    /// <summary>
    /// Chemin du modèle d'autorisation (.fga).
    /// </summary>
    public string? AuthorizationModelPath { get; set; }
}
