namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.OpenAI.Contracts;

/// <summary>
/// Représente un chunk de streaming de l'API OpenAI.
/// </summary>
internal sealed record OpenAIStreamChunk
{
    /// <summary>
    /// Identifiant du stream.
    /// </summary>
    public string? Id { get; init; }
    
    /// <summary>
    /// Modèle utilisé.
    /// </summary>
    public string? Model { get; init; }
    
    /// <summary>
    /// Choix de streaming.
    /// </summary>
    public OpenAIStreamChoice[]? Choices { get; init; }
}
