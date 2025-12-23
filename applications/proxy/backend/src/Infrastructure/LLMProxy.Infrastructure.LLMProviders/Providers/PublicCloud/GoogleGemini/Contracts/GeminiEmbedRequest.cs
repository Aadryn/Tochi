namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.GoogleGemini.Contracts;

/// <summary>
/// Requête d'embedding pour Gemini.
/// </summary>
internal sealed record GeminiEmbedRequest
{
    /// <summary>
    /// Identifiant du modèle.
    /// </summary>
    public required string Model { get; init; }
    
    /// <summary>
    /// Contenu à transformer en embedding.
    /// </summary>
    public required GeminiContent Content { get; init; }
}
