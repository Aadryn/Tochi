namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

/// <summary>
/// Requête batch pour les embeddings Gemini.
/// </summary>
internal sealed record GeminiBatchEmbeddingRequest
{
    /// <summary>
    /// Liste des requêtes d'embedding.
    /// </summary>
    public required List<GeminiEmbedRequest> Requests { get; init; }
}
