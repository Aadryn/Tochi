namespace Authorization.Infrastructure.Idp;

/// <summary>
/// Factory pour créer et sélectionner le client IDP approprié.
/// </summary>
public interface IIdpClientFactory
{
    /// <summary>
    /// Crée un client IDP pour le fournisseur spécifié.
    /// </summary>
    /// <param name="providerType">Type de fournisseur.</param>
    /// <returns>Client IDP.</returns>
    IIdpClient GetClient(IdpProviderType providerType);

    /// <summary>
    /// Retourne le client IDP par défaut configuré.
    /// </summary>
    /// <returns>Client IDP par défaut.</returns>
    IIdpClient GetDefaultClient();
}
