namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

/// <summary>
/// Réponse batch d'embeddings Gemini.
/// </summary>
internal sealed record GeminiBatchEmbeddingResponse
{
    /// <summary>
    /// Embeddings générés.
    /// </summary>
    public required List<GeminiEmbedding> Embeddings { get; init; }
}
