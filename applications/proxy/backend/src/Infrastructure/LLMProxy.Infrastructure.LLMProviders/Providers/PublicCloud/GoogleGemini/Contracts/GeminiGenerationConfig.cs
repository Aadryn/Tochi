namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

/// <summary>
/// Configuration de génération pour Gemini.
/// </summary>
internal sealed record GeminiGenerationConfig
{
    /// <summary>
    /// Température de génération.
    /// </summary>
    public float? Temperature { get; init; }
    
    /// <summary>
    /// Nombre maximum de tokens en sortie.
    /// </summary>
    public int? MaxOutputTokens { get; init; }
    
    /// <summary>
    /// Paramètre top-p.
    /// </summary>
    public float? TopP { get; init; }
    
    /// <summary>
    /// Paramètre top-k.
    /// </summary>
    public int? TopK { get; init; }
    
    /// <summary>
    /// Séquences d'arrêt.
    /// </summary>
    public List<string>? StopSequences { get; init; }
    
    /// <summary>
    /// Type MIME de la réponse.
    /// </summary>
    public string? ResponseMimeType { get; init; }
}
