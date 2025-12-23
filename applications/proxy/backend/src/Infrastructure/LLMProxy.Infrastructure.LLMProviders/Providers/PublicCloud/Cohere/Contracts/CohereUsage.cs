namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Informations d'utilisation Cohere.
/// </summary>
internal sealed record CohereUsage
{
    /// <summary>
    /// Tokens utilisés.
    /// </summary>
    public CohereTokens? Tokens { get; init; }
}
