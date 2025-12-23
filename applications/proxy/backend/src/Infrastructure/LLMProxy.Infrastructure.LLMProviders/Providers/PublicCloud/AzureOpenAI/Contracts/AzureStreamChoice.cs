namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.AzureOpenAI.Contracts;

/// <summary>
/// Représente un choix dans un chunk de streaming.
/// </summary>
internal sealed record AzureStreamChoice
{
    /// <summary>
    /// Delta de contenu.
    /// </summary>
    public AzureDelta? Delta { get; init; }
    
    /// <summary>
    /// Raison de fin de génération.
    /// </summary>
    public string? FinishReason { get; init; }
}
