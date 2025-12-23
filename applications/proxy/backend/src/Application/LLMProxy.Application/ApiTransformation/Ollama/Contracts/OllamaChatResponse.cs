using System.Text.Json.Serialization;

namespace LLMProxy.Application.ApiTransformation.Ollama.Contracts;

/// <summary>
/// Réponse de chat au format Ollama API.
/// </summary>
public sealed record OllamaChatResponse
{
    /// <summary>
    /// Nom du modèle utilisé.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Timestamp de création (ISO 8601).
    /// </summary>
    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; init; }

    /// <summary>
    /// Message de réponse.
    /// </summary>
    [JsonPropertyName("message")]
    public required OllamaMessage Message { get; init; }

    /// <summary>
    /// Indique si le streaming est terminé.
    /// </summary>
    [JsonPropertyName("done")]
    public bool Done { get; init; }

    /// <summary>
    /// Raison de fin (si done=true).
    /// </summary>
    [JsonPropertyName("done_reason")]
    public string? DoneReason { get; init; }

    /// <summary>
    /// Durée totale en nanosecondes.
    /// </summary>
    [JsonPropertyName("total_duration")]
    public long? TotalDuration { get; init; }

    /// <summary>
    /// Durée de chargement en nanosecondes.
    /// </summary>
    [JsonPropertyName("load_duration")]
    public long? LoadDuration { get; init; }

    /// <summary>
    /// Nombre de tokens évalués (prompt).
    /// </summary>
    [JsonPropertyName("prompt_eval_count")]
    public int? PromptEvalCount { get; init; }

    /// <summary>
    /// Durée d'évaluation du prompt en nanosecondes.
    /// </summary>
    [JsonPropertyName("prompt_eval_duration")]
    public long? PromptEvalDuration { get; init; }

    /// <summary>
    /// Nombre de tokens générés.
    /// </summary>
    [JsonPropertyName("eval_count")]
    public int? EvalCount { get; init; }

    /// <summary>
    /// Durée de génération en nanosecondes.
    /// </summary>
    [JsonPropertyName("eval_duration")]
    public long? EvalDuration { get; init; }
}
