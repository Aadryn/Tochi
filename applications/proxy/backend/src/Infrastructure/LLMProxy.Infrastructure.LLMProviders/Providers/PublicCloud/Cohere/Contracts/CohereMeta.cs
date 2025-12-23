namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Représente les métadonnées d'une réponse Cohere.
/// </summary>
internal sealed record CohereMeta
{
    /// <summary>
    /// Information sur les tokens utilisés.
    /// </summary>
    public CohereTokens? Tokens { get; init; }
}
