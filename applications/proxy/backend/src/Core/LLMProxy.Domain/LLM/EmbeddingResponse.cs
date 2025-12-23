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

/// <summary>
/// Représente un vecteur d'embedding.
/// </summary>
public sealed record Embedding
{
    /// <summary>
    /// Index dans la liste des inputs.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// Vecteur d'embedding.
    /// </summary>
    public required IReadOnlyList<float> Vector { get; init; }

    /// <summary>
    /// Texte original (optionnel).
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Dimension du vecteur.
    /// </summary>
    public int Dimensions => Vector.Count;
}
