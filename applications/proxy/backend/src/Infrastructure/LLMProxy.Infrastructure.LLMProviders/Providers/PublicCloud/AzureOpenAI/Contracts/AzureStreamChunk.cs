namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.AzureOpenAI.Contracts;

/// <summary>
/// Représente un chunk de streaming d'Azure OpenAI.
/// </summary>
internal sealed record AzureStreamChunk
{
    /// <summary>
    /// Identifiant du stream.
    /// </summary>
    public string? Id { get; init; }
    
    /// <summary>
    /// Choix de streaming.
    /// </summary>
    public AzureStreamChoice[]? Choices { get; init; }
}
