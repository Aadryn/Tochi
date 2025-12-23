namespace LLMProxy.Domain.LLM;

/// <summary>
/// Représente un modèle LLM disponible chez un provider.
/// </summary>
public sealed record LLMModel
{
    /// <summary>
    /// Identifiant unique du modèle.
    /// </summary>
    public required ModelIdentifier Id { get; init; }

    /// <summary>
    /// Nom d'affichage du modèle.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description du modèle.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Propriétaire/créateur du modèle.
    /// </summary>
    public string? OwnedBy { get; init; }

    /// <summary>
    /// Date de création/mise à jour.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    /// Longueur de contexte maximale en tokens.
    /// </summary>
    public int? ContextLength { get; init; }

    /// <summary>
    /// Longueur de contexte maximale en tokens (alias pour compatibilité).
    /// </summary>
    /// <remarks>
    /// Alias pour <see cref="ContextLength"/> maintenu pour compatibilité avec le code existant.
    /// </remarks>
    public int? MaxContextLength
    {
        get => ContextLength;
        init => ContextLength = value;
    }

    /// <summary>
    /// Dimension des vecteurs d'embeddings (pour les modèles d'embeddings).
    /// </summary>
    public int? OutputDimension { get; init; }

    /// <summary>
    /// Capacités supportées par le modèle.
    /// </summary>
    public ModelCapabilities Capabilities { get; init; } = ModelCapabilities.ChatCompletion;

    /// <summary>
    /// Indique si le modèle est disponible.
    /// </summary>
    public bool IsAvailable { get; init; } = true;

    /// <summary>
    /// Indique si le modèle est déprécié.
    /// </summary>
    public bool IsDeprecated { get; init; }

    /// <summary>
    /// Prix par million de tokens d'entrée (en USD).
    /// </summary>
    public decimal? InputPricePerMillion { get; init; }

    /// <summary>
    /// Prix par million de tokens de sortie (en USD).
    /// </summary>
    public decimal? OutputPricePerMillion { get; init; }

    /// <summary>
    /// Métadonnées additionnelles spécifiques au provider.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
