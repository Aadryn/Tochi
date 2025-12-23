namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

/// <summary>
/// Représente une réponse de l'API Gemini.
/// </summary>
internal sealed record GeminiResponse
{
    /// <summary>
    /// Candidats de réponse.
    /// </summary>
    public GeminiCandidate[]? Candidates { get; init; }
    
    /// <summary>
    /// Métadonnées d'utilisation.
    /// </summary>
    public GeminiUsageMetadata? UsageMetadata { get; init; }
}
