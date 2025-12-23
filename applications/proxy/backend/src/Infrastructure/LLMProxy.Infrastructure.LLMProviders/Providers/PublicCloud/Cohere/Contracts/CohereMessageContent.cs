namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Contenu de message Cohere.
/// </summary>
internal sealed record CohereMessageContent
{
    /// <summary>
    /// Blocs de contenu.
    /// </summary>
    public List<CohereContentBlock>? Content { get; init; }
}
