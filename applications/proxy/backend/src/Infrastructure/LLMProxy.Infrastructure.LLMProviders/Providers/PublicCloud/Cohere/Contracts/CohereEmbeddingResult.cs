namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Représente le résultat d'un embedding Cohere.
/// </summary>
internal sealed record CohereEmbeddingResult
{
    /// <summary>
    /// Tableaux d'embeddings en virgule flottante.
    /// </summary>
    public float[][]? Float { get; init; }
}
