namespace LLMProxy.Domain.LLM;

/// <summary>
/// Réponse normalisée d'une requête d'embeddings.
/// </summary>
public sealed record EmbeddingResponse
{
    /// <summary>
    /// Modèle qui a généré les embeddings.
    /// </summary>
    public required ModelIdentifier Model { get; init; }

    /// <summary>
    /// Liste des embeddings générés.
    /// </summary>
    public required IReadOnlyList<Embedding> Embeddings { get; init; }

    /// <summary>
    /// Utilisation des tokens.
    /// </summary>
    public TokenUsage Usage { get; init; } = TokenUsage.Empty;

    /// <summary>
    /// Nom du provider ayant généré les embeddings.
    /// </summary>
    public string? ProviderName { get; init; }

    /// <summary>
    /// Durée de génération.
    /// </summary>
    public TimeSpan? Duration { get; init; }
}
