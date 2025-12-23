namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Représente un delta de message dans un streaming Cohere.
/// </summary>
internal sealed record CohereMessageDelta
{
    /// <summary>
    /// Delta du contenu du message.
    /// </summary>
    public CohereContentDelta? Content { get; init; }
}
