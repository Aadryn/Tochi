namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.OpenAI.Contracts;

/// <summary>
/// Représente une réponse de chat completion de l'API OpenAI.
/// </summary>
internal sealed record OpenAIChatCompletionResponse
{
    /// <summary>
    /// Identifiant unique de la complétion.
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Modèle utilisé pour générer la réponse.
    /// </summary>
    public required string Model { get; init; }
    
    /// <summary>
    /// Timestamp de création (Unix epoch).
    /// </summary>
    public long Created { get; init; }
    
    /// <summary>
    /// Choix de complétion générés.
    /// </summary>
    public OpenAIChoice[]? Choices { get; init; }
    
    /// <summary>
    /// Statistiques d'utilisation des tokens.
    /// </summary>
    public OpenAIUsage? Usage { get; init; }
}
