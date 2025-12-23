namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.AzureOpenAI.Contracts;

/// <summary>
/// Représente un vecteur d'embedding généré.
/// </summary>
internal sealed record AzureEmbeddingData
{
    /// <summary>
    /// Index de l'embedding dans la requête.
    /// </summary>
    public int Index { get; init; }
    
    /// <summary>
    /// Vecteur d'embedding.
    /// </summary>
    public required float[] Embedding { get; init; }
}
