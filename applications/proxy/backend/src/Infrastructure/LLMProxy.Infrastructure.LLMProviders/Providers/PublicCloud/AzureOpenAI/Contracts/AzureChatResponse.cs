namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.AzureOpenAI.Contracts;

/// <summary>
/// Représente une réponse de chat d'Azure OpenAI.
/// </summary>
internal sealed record AzureChatResponse
{
    /// <summary>
    /// Identifiant unique de la complétion.
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Timestamp de création (Unix epoch).
    /// </summary>
    public long Created { get; init; }
    
    /// <summary>
    /// Choix de complétion générés.
    /// </summary>
    public AzureChoice[]? Choices { get; init; }
    
    /// <summary>
    /// Statistiques d'utilisation des tokens.
    /// </summary>
    public AzureUsage? Usage { get; init; }
}
