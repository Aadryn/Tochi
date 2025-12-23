namespace LLMProxy.Infrastructure.LLMProviders.Providers.Local.Ollama.Contracts;

/// <summary>
/// Requête d'embedding vers Ollama.
/// </summary>
internal sealed record OllamaEmbeddingRequest
{
    /// <summary>
    /// Nom du modèle d'embedding.
    /// </summary>
    public required string Model { get; init; }
    
    /// <summary>
    /// Texte à transformer en embedding.
    /// </summary>
    public required string Prompt { get; init; }
}
