namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Représente un delta de modification dans un streaming Cohere.
/// </summary>
internal sealed record CohereDelta
{
    /// <summary>
    /// Delta du message.
    /// </summary>
    public CohereMessageDelta? Message { get; init; }
}
