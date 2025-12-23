namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

/// <summary>
/// Représente une partie de contenu dans Gemini.
/// </summary>
internal sealed record GeminiPart
{
    /// <summary>
    /// Texte de la partie.
    /// </summary>
    public string? Text { get; init; }
}
