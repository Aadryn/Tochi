namespace Authorization.Infrastructure.Idp;

/// <summary>
/// Types de fournisseurs d'identité supportés.
/// </summary>
public enum IdpProviderType
{
    /// <summary>
    /// Keycloak (développement/on-premise).
    /// </summary>
    Keycloak,

    /// <summary>
    /// Azure Active Directory / Entra ID.
    /// </summary>
    AzureAd,

    /// <summary>
    /// Okta.
    /// </summary>
    Okta
}
