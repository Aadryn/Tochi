namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Requête de chat Cohere.
/// </summary>
internal sealed record CohereChatRequest
{
    /// <summary>
    /// Modèle à utiliser.
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Messages de la conversation.
    /// </summary>
    public required List<CohereMessage> Messages { get; init; }

    /// <summary>
    /// Préambule optionnel.
    /// </summary>
    public string? Preamble { get; init; }

    /// <summary>
    /// Température (0-1).
    /// </summary>
    public float? Temperature { get; init; }

    /// <summary>
    /// Nombre maximum de tokens à générer.
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Nucleus sampling (Top-P).
    /// </summary>
    public float? P { get; init; }

    /// <summary>
    /// Top-K sampling.
    /// </summary>
    public int? K { get; init; }

    /// <summary>
    /// Séquences d'arrêt.
    /// </summary>
    public List<string>? StopSequences { get; init; }

    /// <summary>
    /// Pénalité de fréquence.
    /// </summary>
    public float? FrequencyPenalty { get; init; }

    /// <summary>
    /// Pénalité de présence.
    /// </summary>
    public float? PresencePenalty { get; init; }

    /// <summary>
    /// Mode streaming.
    /// </summary>
    public bool Stream { get; init; }
}
