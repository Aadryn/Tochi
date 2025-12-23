namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Représente un delta de contenu dans un streaming Cohere.
/// </summary>
internal sealed record CohereContentDelta
{
    /// <summary>
    /// Texte du delta.
    /// </summary>
    public string? Text { get; init; }
}
