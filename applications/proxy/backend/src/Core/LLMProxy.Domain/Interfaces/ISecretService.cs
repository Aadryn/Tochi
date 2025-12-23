namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Service de gestion des secrets pour différents environnements (Port en Architecture Hexagonale)
/// Encapsule l'accès à Azure Key Vault ou tout autre gestionnaire de secrets
/// </summary>
public interface ISecretService
{
    /// <summary>
    /// Récupère la valeur d'un secret par son nom
    /// </summary>
    Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stocke ou met à jour un secret
    /// </summary>
    Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Supprime un secret par son nom
    /// </summary>
    Task<bool> DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default);
}
