namespace LLMProxy.Infrastructure.LLMProviders.Providers.Local.Ollama.Contracts;

/// <summary>
/// Représente une requête de chat vers Ollama.
/// </summary>
internal sealed record OllamaChatRequest
{
    /// <summary>
    /// Nom du modèle Ollama à utiliser.
    /// </summary>
    public required string Model { get; init; }
    
    /// <summary>
    /// Liste des messages de la conversation.
    /// </summary>
    public required List<OllamaMessage> Messages { get; init; }
    
    /// <summary>
    /// Options de génération.
    /// </summary>
    public OllamaOptions? Options { get; init; }
    
    /// <summary>
    /// Indique si le streaming est activé.
    /// </summary>
    public bool Stream { get; init; }
}
