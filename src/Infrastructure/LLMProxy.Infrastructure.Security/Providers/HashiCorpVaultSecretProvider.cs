using LLMProxy.Infrastructure.Security.Abstractions.Providers;

namespace LLMProxy.Infrastructure.Security.Providers;

/// <summary>
/// Fournisseur de secrets basé sur HashiCorp Vault.
/// </summary>
/// <remarks>
/// Nécessite l'installation du package VaultSharp et une configuration appropriée.
/// Implémentation à compléter avec VaultClient et authentification.
/// </remarks>
internal class HashiCorpVaultSecretProvider : ISecretProvider
{
    /// <summary>
    /// Récupère un secret depuis HashiCorp Vault.
    /// </summary>
    /// <param name="secretName">Nom du secret à récupérer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Valeur du secret.</returns>
    /// <exception cref="NotImplementedException">HashiCorp Vault integration non implémentée.</exception>
    public Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        // TODO: Implémenter avec VaultSharp
        // var vaultClient = new VaultClient(...);
        // var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(secretName);
        // return secret.Data.Data[secretName].ToString();
        
        throw new NotImplementedException(
            "HashiCorp Vault integration not yet implemented. Install VaultSharp package."
        );
    }

    /// <summary>
    /// Définit ou met à jour un secret dans HashiCorp Vault.
    /// </summary>
    /// <param name="secretName">Nom du secret à créer ou mettre à jour.</param>
    /// <param name="secretValue">Valeur du secret.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    /// <exception cref="NotImplementedException">HashiCorp Vault integration non implémentée.</exception>
    public Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default)
    {
        // TODO: Implémenter avec VaultSharp
        throw new NotImplementedException("HashiCorp Vault integration not yet implemented.");
    }

    /// <summary>
    /// Supprime un secret de HashiCorp Vault.
    /// </summary>
    /// <param name="secretName">Nom du secret à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    /// <exception cref="NotImplementedException">HashiCorp Vault integration non implémentée.</exception>
    public Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        // TODO: Implémenter avec VaultSharp
        throw new NotImplementedException("HashiCorp Vault integration not yet implemented.");
    }
}
