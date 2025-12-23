namespace LLMProxy.Domain.LLM;

/// <summary>
/// Requête normalisée pour la génération d'embeddings.
/// </summary>
public sealed record EmbeddingRequest
{
    /// <summary>
    /// Identifiant du modèle d'embeddings à utiliser.
    /// </summary>
    public required ModelIdentifier Model { get; init; }

    /// <summary>
    /// Textes à vectoriser.
    /// </summary>
    public required IReadOnlyList<string> Inputs { get; init; }

    /// <summary>
    /// Format d'encodage des embeddings.
    /// </summary>
    public EmbeddingEncoding Encoding { get; init; } = EmbeddingEncoding.Float;

    /// <summary>
    /// Dimensions souhaitées (si le modèle le supporte).
    /// </summary>
    public int? Dimensions { get; init; }

    /// <summary>
    /// Identifiant utilisateur pour le tracking.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Métadonnées personnalisées pour le tracking/audit.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Timeout spécifique pour cette requête.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}

/// <summary>
/// Format d'encodage des embeddings.
/// </summary>
public enum EmbeddingEncoding
{
    /// <summary>
    /// Tableau de flottants (défaut).
    /// </summary>
    Float = 0,

    /// <summary>
    /// Base64 encodé.
    /// </summary>
    Base64 = 1
}
