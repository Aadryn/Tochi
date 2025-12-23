namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Représente une réponse de streaming Cohere.
/// </summary>
internal sealed record CohereStreamResponse
{
    /// <summary>
    /// Identifiant de la réponse.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Raison de fin de génération.
    /// </summary>
    public string? FinishReason { get; init; }

    /// <summary>
    /// Métadonnées de la réponse.
    /// </summary>
    public CohereMeta? Meta { get; init; }
}
