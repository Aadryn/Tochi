namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.OpenAI.Contracts;

/// <summary>
/// Représente un choix de complétion dans une réponse OpenAI.
/// </summary>
internal sealed record OpenAIChoice
{
    /// <summary>
    /// Message généré.
    /// </summary>
    public OpenAIMessage? Message { get; init; }
    
    /// <summary>
    /// Raison de fin de génération (stop, length, content_filter).
    /// </summary>
    public string? FinishReason { get; init; }
}
