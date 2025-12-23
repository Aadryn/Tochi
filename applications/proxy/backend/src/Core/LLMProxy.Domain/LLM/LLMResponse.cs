namespace LLMProxy.Domain.LLM;

/// <summary>
/// Réponse normalisée d'une complétion de chat LLM.
/// Représente le format interne indépendant des APIs des providers.
/// </summary>
public sealed record LLMResponse
{
    /// <summary>
    /// Identifiant unique de la réponse.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Modèle qui a généré la réponse.
    /// </summary>
    public required ModelIdentifier Model { get; init; }

    /// <summary>
    /// Contenu textuel de la réponse.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Raison de fin de génération.
    /// </summary>
    public FinishReason FinishReason { get; init; } = FinishReason.Stop;

    /// <summary>
    /// Utilisation des tokens.
    /// </summary>
    public TokenUsage Usage { get; init; } = TokenUsage.Empty;

    /// <summary>
    /// Timestamp de création de la réponse.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Nom du provider ayant généré la réponse.
    /// </summary>
    public string? ProviderName { get; init; }

    /// <summary>
    /// Durée de génération côté provider.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// Métadonnées additionnelles de la réponse.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Indique si la réponse est un chunk de streaming.
    /// </summary>
    public bool IsStreamChunk { get; init; }

    /// <summary>
    /// Index du chunk dans le stream (si streaming).
    /// </summary>
    public int? StreamIndex { get; init; }
}
