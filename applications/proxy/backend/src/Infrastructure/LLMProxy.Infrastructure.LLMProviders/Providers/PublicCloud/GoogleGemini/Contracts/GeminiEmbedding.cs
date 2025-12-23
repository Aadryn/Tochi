namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

/// <summary>
/// Représente un embedding Gemini.
/// </summary>
internal sealed record GeminiEmbedding
{
    /// <summary>
    /// Valeurs du vecteur d'embedding.
    /// </summary>
    public required float[] Values { get; init; }
}
