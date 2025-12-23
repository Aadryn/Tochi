using System.Text.Json.Serialization;

namespace LLMProxy.Application.ApiTransformation.OpenAI.Contracts;

/// <summary>
/// Requête de chat au format OpenAI API.
/// </summary>
public sealed record OpenAIChatRequest
{
    /// <summary>
    /// Identifiant du modèle à utiliser.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Liste des messages de la conversation.
    /// </summary>
    [JsonPropertyName("messages")]
    public required IReadOnlyList<OpenAIMessage> Messages { get; init; }

    /// <summary>
    /// Température de génération (0.0 à 2.0).
    /// </summary>
    [JsonPropertyName("temperature")]
    public decimal? Temperature { get; init; }

    /// <summary>
    /// Nombre maximum de tokens à générer.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Top-p (nucleus sampling).
    /// </summary>
    [JsonPropertyName("top_p")]
    public decimal? TopP { get; init; }

    /// <summary>
    /// Nombre de complétions à générer.
    /// </summary>
    [JsonPropertyName("n")]
    public int? N { get; init; }

    /// <summary>
    /// Activer le streaming.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; init; }

    /// <summary>
    /// Séquences d'arrêt.
    /// </summary>
    [JsonPropertyName("stop")]
    public IReadOnlyList<string>? Stop { get; init; }

    /// <summary>
    /// Pénalité de présence.
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public decimal? PresencePenalty { get; init; }

    /// <summary>
    /// Pénalité de fréquence.
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public decimal? FrequencyPenalty { get; init; }

    /// <summary>
    /// Identifiant utilisateur.
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; init; }

    /// <summary>
    /// Seed pour la reproductibilité.
    /// </summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; init; }
}
