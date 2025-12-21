namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Énumération des types de fournisseurs de secrets supportés.
/// </summary>
/// <remarks>
/// Détermine où les secrets sont stockés et récupérés par le SecretService.
/// </remarks>
internal enum SecretProviderType
{
    /// <summary>
    /// Variables d'environnement système ou configuration appsettings.json.
    /// </summary>
    EnvironmentVariable,

    /// <summary>
    /// Azure Key Vault (nécessite Azure.Security.KeyVault.Secrets package).
    /// </summary>
    AzureKeyVault,

    /// <summary>
    /// HashiCorp Vault (nécessite VaultSharp package).
    /// </summary>
    HashiCorpVault,

    /// <summary>
    /// Stockage en base de données avec chiffrement AES-256.
    /// </summary>
    EncryptedDatabase
}
