namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Représente une requête d'embedding Cohere.
/// </summary>
internal sealed record CohereEmbedRequest
{
    /// <summary>
    /// Identifiant du modèle d'embedding.
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Liste des textes à convertir en embeddings.
    /// </summary>
    public required List<string> Texts { get; init; }

    /// <summary>
    /// Type d'entrée (search_query, search_document, classification, clustering).
    /// </summary>
    public required string InputType { get; init; }

    /// <summary>
    /// Types d'embeddings à retourner (float, int8, uint8, binary, ubinary).
    /// </summary>
    public required string[] EmbeddingTypes { get; init; }
}
