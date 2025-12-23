namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Représente un événement de streaming Cohere.
/// </summary>
internal sealed record CohereStreamEvent
{
    /// <summary>
    /// Type de l'événement de streaming.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Delta contenant les modifications progressives.
    /// </summary>
    public CohereDelta? Delta { get; init; }

    /// <summary>
    /// Réponse finale de streaming.
    /// </summary>
    public CohereStreamResponse? Response { get; init; }
}
