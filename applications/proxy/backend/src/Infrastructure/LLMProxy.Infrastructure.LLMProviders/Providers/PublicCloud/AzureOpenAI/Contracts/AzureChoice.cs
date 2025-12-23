namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.AzureOpenAI.Contracts;

/// <summary>
/// Représente un choix de complétion dans une réponse Azure.
/// </summary>
internal sealed record AzureChoice
{
    /// <summary>
    /// Message généré.
    /// </summary>
    public AzureMessage? Message { get; init; }
    
    /// <summary>
    /// Raison de fin de génération (stop, length, content_filter).
    /// </summary>
    public string? FinishReason { get; init; }
}
