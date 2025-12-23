namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.OpenAI.Contracts;

/// <summary>
/// Représente une requête d'embedding vers l'API OpenAI.
/// </summary>
internal sealed record OpenAIEmbeddingRequest
{
    /// <summary>
    /// Identifiant du modèle d'embedding.
    /// </summary>
    public required string Model { get; init; }
    
    /// <summary>
    /// Textes à transformer en embeddings.
    /// </summary>
    public required List<string> Input { get; init; }
    
    /// <summary>
    /// Format d'encodage (float, base64).
    /// </summary>
    public string? EncodingFormat { get; init; }
    
    /// <summary>
    /// Nombre de dimensions souhaitées.
    /// </summary>
    public int? Dimensions { get; init; }
}
