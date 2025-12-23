namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.OpenAI.Contracts;

/// <summary>
/// Représente un message dans une conversation OpenAI.
/// </summary>
internal sealed record OpenAIMessage
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
    /// Nom de l'auteur du message.
    /// </summary>
    public string? Name { get; init; }
}
