using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.LLM;

/// <summary>
/// Interface abstraite pour les clients de providers LLM.
/// Chaque provider (OpenAI, Anthropic, etc.) implémente cette interface.
/// Conforme à ADR-034 (Encapsulation des Bibliothèques Tierces).
/// </summary>
/// <remarks>
/// <para>
/// Cette interface définit le contrat commun pour tous les providers LLM.
/// Les implémentations concrètes sont dans Infrastructure.LLMProviders.
/// </para>
/// <para>
/// Le code métier ne doit dépendre que de cette interface, jamais des
/// implémentations spécifiques des providers.
/// </para>
/// </remarks>
public interface ILLMProviderClient : IDisposable
{
    /// <summary>
    /// Type du provider.
    /// </summary>
    ProviderType Type { get; }

    /// <summary>
    /// Nom d'affichage du provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Récupère les capacités et métadonnées du provider.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Capacités du provider.</returns>
    Task<ProviderCapabilities> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Liste les modèles disponibles chez ce provider.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des modèles disponibles.</returns>
    Task<IReadOnlyList<LLMModel>> ListModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si le provider est en bonne santé.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>True si le provider est disponible.</returns>
    Task<bool> IsHealthyAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Effectue une complétion de chat.
    /// </summary>
    /// <param name="request">Requête de complétion.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Réponse du modèle.</returns>
    Task<LLMResponse> ChatCompletionAsync(
        LLMRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Effectue une complétion de chat en streaming.
    /// </summary>
    /// <param name="request">Requête de complétion.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Stream de chunks de réponse.</returns>
    IAsyncEnumerable<LLMResponse> ChatCompletionStreamAsync(
        LLMRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Génère des embeddings pour les textes fournis.
    /// </summary>
    /// <param name="request">Requête d'embeddings.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Réponse contenant les embeddings.</returns>
    Task<EmbeddingResponse> EmbeddingsAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default);
}
