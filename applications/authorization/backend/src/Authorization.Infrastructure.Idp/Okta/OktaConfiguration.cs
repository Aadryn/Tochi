namespace Authorization.Infrastructure.Idp.Okta;

/// <summary>
/// Configuration pour l'intégration Okta.
/// </summary>
public class OktaConfiguration
{
    /// <summary>
    /// Nom de la section de configuration.
    /// </summary>
    public const string SectionName = "Okta";

    /// <summary>
    /// URL du domaine Okta (ex: https://dev-123456.okta.com).
    /// </summary>
    public required string Domain { get; set; }

    /// <summary>
    /// Token API pour l'authentification.
    /// </summary>
    public required string ApiToken { get; set; }

    /// <summary>
    /// URL de base de l'API.
    /// </summary>
    public string ApiUrl => $"{Domain.TrimEnd('/')}/api/v1";
}
