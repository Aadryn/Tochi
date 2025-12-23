namespace LLMProxy.Domain.LLM;

/// <summary>
/// Requête normalisée pour une complétion de chat LLM.
/// Représente le format interne indépendant des APIs des providers.
/// </summary>
public sealed record LLMRequest
{
    /// <summary>
    /// Identifiant du modèle à utiliser.
    /// </summary>
    public required ModelIdentifier Model { get; init; }

    /// <summary>
    /// Liste des messages de la conversation.
    /// </summary>
    public required IReadOnlyList<LLMMessage> Messages { get; init; }

    /// <summary>
    /// Température de génération (0.0 = déterministe, 2.0 = très créatif).
    /// Null pour utiliser la valeur par défaut du modèle.
    /// </summary>
    public decimal? Temperature { get; init; }

    /// <summary>
    /// Nombre maximum de tokens à générer.
    /// Null pour utiliser la valeur par défaut du modèle.
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Top-p (nucleus sampling). Alternative à temperature.
    /// </summary>
    public decimal? TopP { get; init; }

    /// <summary>
    /// Top-k sampling. Limite le vocabulaire aux k tokens les plus probables.
    /// Utilisé par Cohere, Google Gemini et autres providers.
    /// </summary>
    public int? TopK { get; init; }

    /// <summary>
    /// Nombre de complétions à générer.
    /// </summary>
    public int? N { get; init; } = 1;

    /// <summary>
    /// Séquences qui arrêtent la génération.
    /// </summary>
    public IReadOnlyList<string>? StopSequences { get; init; }

    /// <summary>
    /// Pénalité de présence (-2.0 à 2.0).
    /// </summary>
    public decimal? PresencePenalty { get; init; }

    /// <summary>
    /// Pénalité de fréquence (-2.0 à 2.0).
    /// </summary>
    public decimal? FrequencyPenalty { get; init; }

    /// <summary>
    /// Identifiant utilisateur pour le tracking.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Seed pour la reproductibilité des résultats.
    /// </summary>
    public int? Seed { get; init; }

    /// <summary>
    /// Métadonnées personnalisées pour le tracking/audit.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Indique si le streaming est demandé.
    /// </summary>
    public bool Stream { get; init; }

    /// <summary>
    /// Format de réponse demandé.
    /// </summary>
    public ResponseFormat? ResponseFormat { get; init; }

    /// <summary>
    /// Timeout spécifique pour cette requête.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}

/// <summary>
/// Format de réponse demandé.
/// </summary>
public enum ResponseFormat
{
    /// <summary>
    /// Texte libre (défaut).
    /// </summary>
    Text = 0,

    /// <summary>
    /// JSON structuré.
    /// </summary>
    Json = 1
}
