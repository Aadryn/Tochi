namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.OpenAI.Contracts;

/// <summary>
/// Représente une réponse d'embedding de l'API OpenAI.
/// </summary>
internal sealed record OpenAIEmbeddingResponse
{
    /// <summary>
    /// Modèle utilisé pour générer les embeddings.
    /// </summary>
    public required string Model { get; init; }
    
    /// <summary>
    /// Données d'embedding générées.
    /// </summary>
    public required List<OpenAIEmbeddingData> Data { get; init; }
    
    /// <summary>
    /// Statistiques d'utilisation des tokens.
    /// </summary>
    public required OpenAIUsage Usage { get; init; }
}
