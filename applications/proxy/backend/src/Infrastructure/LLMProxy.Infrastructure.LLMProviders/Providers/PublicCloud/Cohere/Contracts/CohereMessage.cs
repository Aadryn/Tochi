namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Message Cohere.
/// </summary>
internal sealed record CohereMessage
{
    /// <summary>
    /// Rôle (user, assistant, system).
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Contenu du message.
    /// </summary>
    public required string Content { get; init; }
}
