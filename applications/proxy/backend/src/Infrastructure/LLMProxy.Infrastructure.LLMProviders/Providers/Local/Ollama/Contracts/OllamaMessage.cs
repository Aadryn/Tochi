namespace LLMProxy.Infrastructure.LLMProviders.Providers.Local.Ollama.Contracts;

/// <summary>
/// Représente un message dans une conversation Ollama.
/// </summary>
internal sealed record OllamaMessage
{
    /// <summary>
    /// Rôle du message (system, user, assistant).
    /// </summary>
    public required string Role { get; init; }
    
    /// <summary>
    /// Contenu du message.
    /// </summary>
    public required string Content { get; init; }
    
    /// <summary>
    /// Images encodées en base64 (pour modèles multimodaux).
    /// </summary>
    public List<string>? Images { get; init; }
}
