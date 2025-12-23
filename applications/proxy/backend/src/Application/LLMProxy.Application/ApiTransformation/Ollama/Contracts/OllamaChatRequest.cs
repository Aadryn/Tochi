using System.Text.Json.Serialization;

namespace LLMProxy.Application.ApiTransformation.Ollama.Contracts;

/// <summary>
/// Requête de chat au format Ollama API.
/// </summary>
public sealed record OllamaChatRequest
{
    /// <summary>
    /// Nom du modèle à utiliser.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Liste des messages de la conversation.
    /// </summary>
    [JsonPropertyName("messages")]
    public required IReadOnlyList<OllamaMessage> Messages { get; init; }

    /// <summary>
    /// Activer le streaming (true par défaut dans Ollama).
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; init; } = true;

    /// <summary>
    /// Format de sortie (json pour mode JSON).
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; init; }

    /// <summary>
    /// Options de génération.
    /// </summary>
    [JsonPropertyName("options")]
    public OllamaOptions? Options { get; init; }

    /// <summary>
    /// Durée de conservation du modèle en mémoire.
    /// </summary>
    [JsonPropertyName("keep_alive")]
    public string? KeepAlive { get; init; }
}

/// <summary>
/// Message au format Ollama API.
/// </summary>
public sealed record OllamaMessage
{
    /// <summary>
    /// Rôle du message (system, user, assistant).
    /// </summary>
    [JsonPropertyName("role")]
    public required string Role { get; init; }

    /// <summary>
    /// Contenu du message.
    /// </summary>
    [JsonPropertyName("content")]
    public required string Content { get; init; }

    /// <summary>
    /// Images encodées en base64 (pour modèles vision).
    /// </summary>
    [JsonPropertyName("images")]
    public IReadOnlyList<string>? Images { get; init; }
}

/// <summary>
/// Options de génération Ollama.
/// </summary>
public sealed record OllamaOptions
{
    /// <summary>
    /// Température de génération.
    /// </summary>
    [JsonPropertyName("temperature")]
    public decimal? Temperature { get; init; }

    /// <summary>
    /// Top-k sampling.
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; init; }

    /// <summary>
    /// Top-p (nucleus sampling).
    /// </summary>
    [JsonPropertyName("top_p")]
    public decimal? TopP { get; init; }

    /// <summary>
    /// Nombre de tokens à prédire.
    /// </summary>
    [JsonPropertyName("num_predict")]
    public int? NumPredict { get; init; }

    /// <summary>
    /// Séquences d'arrêt.
    /// </summary>
    [JsonPropertyName("stop")]
    public IReadOnlyList<string>? Stop { get; init; }

    /// <summary>
    /// Pénalité de répétition.
    /// </summary>
    [JsonPropertyName("repeat_penalty")]
    public decimal? RepeatPenalty { get; init; }

    /// <summary>
    /// Seed pour la reproductibilité.
    /// </summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; init; }
}
