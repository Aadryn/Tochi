namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

/// <summary>
/// Représente un candidat de réponse Gemini.
/// </summary>
internal sealed record GeminiCandidate
{
    /// <summary>
    /// Contenu généré.
    /// </summary>
    public GeminiContent? Content { get; init; }
    
    /// <summary>
    /// Raison de fin de génération.
    /// </summary>
    public string? FinishReason { get; init; }
}
