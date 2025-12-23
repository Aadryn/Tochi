namespace Authorization.Infrastructure.Idp.AzureAd;

/// <summary>
/// Configuration pour l'intégration Azure AD (Entra ID).
/// </summary>
public class AzureAdConfiguration
{
    /// <summary>
    /// Nom de la section de configuration.
    /// </summary>
    public const string SectionName = "AzureAd";

    /// <summary>
    /// Identifiant du tenant Azure AD.
    /// </summary>
    public required string TenantId { get; set; }

    /// <summary>
    /// Identifiant de l'application (client ID).
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// Secret client pour l'authentification.
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    /// URL de l'instance Azure AD.
    /// </summary>
    public string Instance { get; set; } = "https://login.microsoftonline.com/";

    /// <summary>
    /// URL de l'API Graph.
    /// </summary>
    public string GraphApiUrl => "https://graph.microsoft.com/v1.0";
}
