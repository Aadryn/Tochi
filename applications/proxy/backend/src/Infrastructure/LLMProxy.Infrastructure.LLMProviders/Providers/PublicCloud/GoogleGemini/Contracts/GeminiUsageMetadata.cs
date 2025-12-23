namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

/// <summary>
/// Métadonnées d'utilisation des tokens.
/// </summary>
internal sealed record GeminiUsageMetadata
{
    /// <summary>
    /// Nombre de tokens dans le prompt.
    /// </summary>
    public int? PromptTokenCount { get; init; }
    
    /// <summary>
    /// Nombre de tokens dans les candidats.
    /// </summary>
    public int? CandidatesTokenCount { get; init; }
    
    /// <summary>
    /// Nombre total de tokens.
    /// </summary>
    public int? TotalTokenCount { get; init; }
}
