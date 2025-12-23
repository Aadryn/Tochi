namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

/// <summary>
/// Paramètre de sécurité pour Gemini.
/// </summary>
internal sealed record GeminiSafetySetting
{
    /// <summary>
    /// Catégorie de sécurité.
    /// </summary>
    public required string Category { get; init; }
    
    /// <summary>
    /// Seuil de blocage.
    /// </summary>
    public required string Threshold { get; init; }
}
