namespace LLMProxy.Infrastructure.Authorization;

/// <summary>
/// Configuration du service d'autorisation OpenFGA.
/// </summary>
/// <remarks>
/// Ces paramètres sont chargés depuis la section "OpenFga" du fichier de configuration.
/// </remarks>
public sealed class OpenFgaConfiguration
{
    /// <summary>
    /// Nom de la section de configuration dans appsettings.json.
    /// </summary>
    public const string SectionName = "OpenFga";

    /// <summary>
    /// URL de l'API OpenFGA.
    /// </summary>
    /// <example>http://localhost:8080</example>
    public string ApiUrl { get; set; } = "http://localhost:8080";

    /// <summary>
    /// Identifiant du store OpenFGA.
    /// </summary>
    /// <remarks>
    /// Obtenu lors de l'initialisation via le script init-openfga.sh
    /// ou via la variable d'environnement OPENFGA_STORE_ID.
    /// </remarks>
    public string StoreId { get; set; } = string.Empty;

    /// <summary>
    /// Identifiant du modèle d'autorisation OpenFGA.
    /// </summary>
    /// <remarks>
    /// Obtenu lors de l'initialisation via le script init-openfga.sh
    /// ou via la variable d'environnement OPENFGA_MODEL_ID.
    /// </remarks>
    public string AuthorizationModelId { get; set; } = string.Empty;

    /// <summary>
    /// Timeout des requêtes HTTP vers OpenFGA (en secondes).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Nombre maximum de tentatives en cas d'échec.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Active ou désactive le service d'autorisation.
    /// </summary>
    /// <remarks>
    /// Si désactivé, toutes les vérifications retournent "Autorisé".
    /// Utile pour le développement ou les tests.
    /// </remarks>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Mode de fallback si OpenFGA n'est pas disponible.
    /// </summary>
    public FallbackMode FallbackMode { get; set; } = FallbackMode.Deny;

    /// <summary>
    /// Valide la configuration et lève une exception si invalide.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Levée si des paramètres requis sont manquants.
    /// </exception>
    public void Validate()
    {
        if (Enabled && string.IsNullOrWhiteSpace(StoreId))
        {
            throw new InvalidOperationException(
                "OpenFga.StoreId est requis quand OpenFga.Enabled est true. " +
                "Exécutez init-openfga.sh pour initialiser le store.");
        }

        if (Enabled && string.IsNullOrWhiteSpace(ApiUrl))
        {
            throw new InvalidOperationException("OpenFga.ApiUrl est requis.");
        }
    }
}

/// <summary>
/// Mode de fallback si OpenFGA n'est pas disponible.
/// </summary>
public enum FallbackMode
{
    /// <summary>
    /// Refuse toutes les requêtes si OpenFGA est indisponible.
    /// </summary>
    Deny = 0,

    /// <summary>
    /// Autorise toutes les requêtes si OpenFGA est indisponible.
    /// </summary>
    /// <remarks>
    /// ⚠️ À utiliser uniquement en environnement de développement.
    /// </remarks>
    Allow = 1
}
