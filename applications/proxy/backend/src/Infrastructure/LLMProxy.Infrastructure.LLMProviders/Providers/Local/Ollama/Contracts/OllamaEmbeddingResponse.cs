namespace LLMProxy.Infrastructure.LLMProviders.Providers.Local.Ollama.Contracts;

/// <summary>
/// Réponse d'embedding d'Ollama.
/// </summary>
internal sealed record OllamaEmbeddingResponse
{
    /// <summary>
    /// Vecteur d'embedding généré.
    /// </summary>
    public float[]? Embedding { get; init; }
}
