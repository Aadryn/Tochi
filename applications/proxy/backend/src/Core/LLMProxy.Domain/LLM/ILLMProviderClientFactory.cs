using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.LLM;

/// <summary>
/// Factory pour créer des instances de ILLMProviderClient.
/// Conforme au Factory Pattern (ADR-034).
/// </summary>
public interface ILLMProviderClientFactory
{
    /// <summary>
    /// Crée un client pour le provider spécifié.
    /// </summary>
    /// <param name="providerType">Type du provider.</param>
    /// <returns>Instance du client provider.</returns>
    /// <exception cref="NotSupportedException">
    /// Levée si le provider n'est pas supporté.
    /// </exception>
    ILLMProviderClient CreateClient(ProviderType providerType);

    /// <summary>
    /// Crée un client pour un provider nommé.
    /// </summary>
    /// <param name="providerName">Nom unique du provider configuré.</param>
    /// <returns>Instance du client provider.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Levée si le provider n'est pas trouvé.
    /// </exception>
    ILLMProviderClient CreateClient(string providerName);

    /// <summary>
    /// Récupère tous les clients configurés et actifs.
    /// </summary>
    /// <returns>Liste des clients disponibles.</returns>
    IReadOnlyList<ILLMProviderClient> GetAllClients();

    /// <summary>
    /// Vérifie si un type de provider est supporté.
    /// </summary>
    /// <param name="providerType">Type du provider.</param>
    /// <returns>True si le provider est supporté.</returns>
    bool IsSupported(ProviderType providerType);

    /// <summary>
    /// Récupère les types de providers supportés.
    /// </summary>
    /// <returns>Liste des types supportés.</returns>
    IReadOnlyList<ProviderType> GetSupportedTypes();
}
