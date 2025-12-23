namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Informations de tokens Cohere.
/// </summary>
internal sealed record CohereTokens
{
    /// <summary>
    /// Tokens d'entrée.
    /// </summary>
    public int? InputTokens { get; init; }

    /// <summary>
    /// Tokens de sortie.
    /// </summary>
    public int? OutputTokens { get; init; }
}
