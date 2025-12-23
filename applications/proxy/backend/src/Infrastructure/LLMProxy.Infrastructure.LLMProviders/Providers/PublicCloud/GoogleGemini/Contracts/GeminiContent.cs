namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

/// <summary>
/// Représente un contenu dans une conversation Gemini.
/// </summary>
internal sealed record GeminiContent
{
    /// <summary>
    /// Rôle du contenu (user, model).
    /// </summary>
    public string? Role { get; init; }
    
    /// <summary>
    /// Parties du contenu.
    /// </summary>
    public required List<GeminiPart> Parts { get; init; }
}
