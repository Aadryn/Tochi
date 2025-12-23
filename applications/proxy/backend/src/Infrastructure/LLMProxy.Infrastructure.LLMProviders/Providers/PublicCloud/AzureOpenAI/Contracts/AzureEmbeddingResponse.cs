namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.AzureOpenAI.Contracts;

/// <summary>
/// Représente une réponse d'embedding d'Azure OpenAI.
/// </summary>
internal sealed record AzureEmbeddingResponse
{
    /// <summary>
    /// Données d'embedding générées.
    /// </summary>
    public required List<AzureEmbeddingData> Data { get; init; }
    
    /// <summary>
    /// Statistiques d'utilisation des tokens.
    /// </summary>
    public required AzureUsage Usage { get; init; }
}
