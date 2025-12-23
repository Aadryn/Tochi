namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Bloc de contenu Cohere.
/// </summary>
internal sealed record CohereContentBlock
{
    /// <summary>
    /// Type de contenu.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Texte.
    /// </summary>
    public string? Text { get; init; }
}
