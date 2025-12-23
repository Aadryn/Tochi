using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.LLM;

/// <summary>
/// Représente les capacités et métadonnées d'un provider LLM.
/// </summary>
public sealed record ProviderCapabilities
{
    /// <summary>
    /// Type du provider.
    /// </summary>
    public required ProviderType Type { get; init; }

    /// <summary>
    /// Nom d'affichage du provider.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Version de l'API du provider.
    /// </summary>
    public string? ApiVersion { get; init; }

    /// <summary>
    /// Capacités supportées par le provider.
    /// </summary>
    public ModelCapabilities SupportedCapabilities { get; init; }

    /// <summary>
    /// Indique si le provider est en bonne santé.
    /// </summary>
    public bool IsHealthy { get; init; } = true;

    /// <summary>
    /// Latence moyenne des requêtes (ms).
    /// </summary>
    public int? AverageLatencyMs { get; init; }

    /// <summary>
    /// Limite de requêtes par minute.
    /// </summary>
    public int? RateLimitPerMinute { get; init; }

    /// <summary>
    /// Limite de tokens par minute.
    /// </summary>
    public int? TokenLimitPerMinute { get; init; }

    /// <summary>
    /// Nombre maximum de tokens par requête.
    /// </summary>
    public int? MaxTokensPerRequest { get; init; }

    /// <summary>
    /// Indique si le streaming est supporté.
    /// </summary>
    public bool SupportsStreaming { get; init; } = true;

    /// <summary>
    /// Indique si les embeddings sont supportés.
    /// </summary>
    public bool SupportsEmbeddings { get; init; }

    /// <summary>
    /// Indique si l'appel de fonctions est supporté.
    /// </summary>
    public bool SupportsFunctionCalling { get; init; }

    /// <summary>
    /// Indique si la vision est supportée.
    /// </summary>
    public bool SupportsVision { get; init; }

    /// <summary>
    /// Régions géographiques disponibles.
    /// </summary>
    public IReadOnlyList<string>? Regions { get; init; }

    /// <summary>
    /// Date de dernière vérification de santé.
    /// </summary>
    public DateTimeOffset? LastHealthCheck { get; init; }

    /// <summary>
    /// Métadonnées additionnelles spécifiques au provider.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
