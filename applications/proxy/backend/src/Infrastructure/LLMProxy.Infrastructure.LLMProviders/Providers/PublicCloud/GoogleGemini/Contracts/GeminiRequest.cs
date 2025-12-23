namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

/// <summary>
/// Représente une requête vers l'API Gemini.
/// </summary>
internal sealed record GeminiRequest
{
    /// <summary>
    /// Contenus de la conversation.
    /// </summary>
    public required List<GeminiContent> Contents { get; init; }
    
    /// <summary>
    /// Instruction système optionnelle.
    /// </summary>
    public GeminiContent? SystemInstruction { get; init; }
    
    /// <summary>
    /// Configuration de génération.
    /// </summary>
    public GeminiGenerationConfig? GenerationConfig { get; init; }
    
    /// <summary>
    /// Paramètres de sécurité.
    /// </summary>
    public List<GeminiSafetySetting>? SafetySettings { get; init; }
}
