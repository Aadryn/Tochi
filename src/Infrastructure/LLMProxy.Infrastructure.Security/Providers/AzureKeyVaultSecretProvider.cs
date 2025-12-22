using LLMProxy.Infrastructure.Security.Abstractions.Providers;

namespace LLMProxy.Infrastructure.Security.Providers;

/// <summary>
/// Fournisseur de secrets basé sur Azure Key Vault.
/// </summary>
/// <remarks>
/// Nécessite l'installation du package Azure.Security.KeyVault.Secrets et une configuration appropriée.
/// Implémentation à compléter avec SecretClient et DefaultAzureCredential.
/// </remarks>
internal class AzureKeyVaultSecretProvider : ISecretProvider
{
    /// <summary>
    /// Récupère un secret depuis Azure Key Vault.
    /// </summary>
    /// <param name="secretName">Nom du secret à récupérer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Valeur du secret.</returns>
    /// <exception cref="NotImplementedException">Azure Key Vault integration non implémentée.</exception>
    public Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        // TODO: Implémenter avec Azure.Security.KeyVault.Secrets
        // var client = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential());
        // var secret = await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
        // return secret.Value.Value;
        
        throw new NotImplementedException(
            "Azure KeyVault integration not yet implemented. Install Azure.Security.KeyVault.Secrets package."
        );
    }

    /// <summary>
    /// Définit ou met à jour un secret dans Azure Key Vault.
    /// </summary>
    /// <param name="secretName">Nom du secret à créer ou mettre à jour.</param>
    /// <param name="secretValue">Valeur du secret.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    /// <exception cref="NotImplementedException">Azure Key Vault integration non implémentée.</exception>
    public Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default)
    {
        // TODO: Implémenter avec Azure.Security.KeyVault.Secrets
        throw new NotImplementedException("Azure KeyVault integration not yet implemented.");
    }

    /// <summary>
    /// Supprime un secret d'Azure Key Vault.
    /// </summary>
    /// <param name="secretName">Nom du secret à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    /// <exception cref="NotImplementedException">Azure Key Vault integration non implémentée.</exception>
    public Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        // TODO: Implémenter avec Azure.Security.KeyVault.Secrets
        throw new NotImplementedException("Azure KeyVault integration not yet implemented.");
    }
}
