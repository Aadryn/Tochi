namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.OpenAI.Contracts;

/// <summary>
/// Représente un choix dans un chunk de streaming.
/// </summary>
internal sealed record OpenAIStreamChoice
{
    /// <summary>
    /// Delta de contenu.
    /// </summary>
    public OpenAIDelta? Delta { get; init; }
    
    /// <summary>
    /// Raison de fin de génération.
    /// </summary>
    public string? FinishReason { get; init; }
}
