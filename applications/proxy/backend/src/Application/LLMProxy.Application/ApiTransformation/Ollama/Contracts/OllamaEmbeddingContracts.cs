using System.Text.Json.Serialization;

namespace LLMProxy.Application.ApiTransformation.Ollama.Contracts;

/// <summary>
/// Requête d'embedding au format Ollama API.
/// </summary>
public sealed record OllamaEmbeddingRequest
{
    /// <summary>
    /// Nom du modèle à utiliser.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Texte(s) à encoder. Peut être une chaîne ou une liste.
    /// </summary>
    [JsonPropertyName("input")]
    public required object Input { get; init; }

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
/// Réponse d'embedding au format Ollama API.
/// </summary>
public sealed record OllamaEmbeddingResponse
{
    /// <summary>
    /// Nom du modèle utilisé.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Liste des vecteurs d'embedding.
    /// </summary>
    [JsonPropertyName("embeddings")]
    public required IReadOnlyList<IReadOnlyList<float>> Embeddings { get; init; }

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
    /// Nombre de tokens évalués.
    /// </summary>
    [JsonPropertyName("prompt_eval_count")]
    public int? PromptEvalCount { get; init; }
}
