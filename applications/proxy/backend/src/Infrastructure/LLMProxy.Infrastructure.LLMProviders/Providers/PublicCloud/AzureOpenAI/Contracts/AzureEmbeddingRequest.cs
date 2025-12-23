namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.AzureOpenAI.Contracts;

/// <summary>
/// Représente une requête d'embedding vers Azure OpenAI.
/// </summary>
internal sealed record AzureEmbeddingRequest
{
    /// <summary>
    /// Textes à transformer en embeddings.
    /// </summary>
    public required List<string> Input { get; init; }
}
