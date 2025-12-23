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

/// <summary>
/// Capacités d'un modèle LLM.
/// </summary>
[Flags]
public enum ModelCapabilities
{
    /// <summary>
    /// Aucune capacité spécifique.
    /// </summary>
    None = 0,

    /// <summary>
    /// Complétion de chat (messages).
    /// </summary>
    ChatCompletion = 1 << 0,

    /// <summary>
    /// Complétion de texte (legacy).
    /// </summary>
    TextCompletion = 1 << 1,

    /// <summary>
    /// Génération d'embeddings.
    /// </summary>
    Embeddings = 1 << 2,

    /// <summary>
    /// Support du streaming.
    /// </summary>
    Streaming = 1 << 3,

    /// <summary>
    /// Appel de fonctions/outils.
    /// </summary>
    FunctionCalling = 1 << 4,

    /// <summary>
    /// Analyse d'images (vision).
    /// </summary>
    Vision = 1 << 5,

    /// <summary>
    /// Génération d'images.
    /// </summary>
    ImageGeneration = 1 << 6,

    /// <summary>
    /// Analyse audio (speech-to-text).
    /// </summary>
    Audio = 1 << 7,

    /// <summary>
    /// Sortie JSON structurée garantie.
    /// </summary>
    JsonMode = 1 << 8
}
