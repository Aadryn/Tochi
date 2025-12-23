namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Représente une réponse d'embedding Cohere.
/// </summary>
internal sealed record CohereEmbedResponse
{
    /// <summary>
    /// Résultats des embeddings.
    /// </summary>
    public CohereEmbeddingResult? Embeddings { get; init; }

    /// <summary>
    /// Métadonnées de la réponse.
    /// </summary>
    public CohereEmbedMeta? Meta { get; init; }
}
