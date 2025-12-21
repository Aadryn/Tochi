namespace LLMProxy.Infrastructure.Security.Providers;

/// <summary>
/// Contrat pour les fournisseurs de secrets (Strategy Pattern - ADR-005 SRP).
/// </summary>
/// <remarks>
/// Permet de déléguer la récupération, le stockage et la suppression de secrets
/// à différentes implémentations spécialisées (variables d'environnement, Azure KeyVault,
/// HashiCorp Vault, base de données chiffrée).
/// </remarks>
public interface ISecretProvider
{
    /// <summary>
    /// Récupère un secret depuis le fournisseur.
    /// </summary>
    /// <param name="secretName">Nom du secret à récupérer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Valeur du secret ou null si introuvable.</returns>
    Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Définit ou met à jour un secret dans le fournisseur.
    /// </summary>
    /// <param name="secretName">Nom du secret à créer ou mettre à jour.</param>
    /// <param name="secretValue">Valeur du secret.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un secret du fournisseur.
    /// </summary>
    /// <param name="secretName">Nom du secret à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default);
}
