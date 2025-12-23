using System.Text.Json.Serialization;

namespace LLMProxy.Application.ApiTransformation.Ollama.Contracts;

/// <summary>
/// Réponse de liste des modèles au format Ollama API (endpoint /api/tags).
/// </summary>
public sealed record OllamaTagsResponse
{
    /// <summary>
    /// Liste des modèles disponibles.
    /// </summary>
    [JsonPropertyName("models")]
    public required IReadOnlyList<OllamaModelInfo> Models { get; init; }
}

/// <summary>
/// Information sur un modèle au format Ollama.
/// </summary>
public sealed record OllamaModelInfo
{
    /// <summary>
    /// Nom du modèle.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Nom du modèle de base.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Date de modification (ISO 8601).
    /// </summary>
    [JsonPropertyName("modified_at")]
    public required string ModifiedAt { get; init; }

    /// <summary>
    /// Taille du modèle en octets.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; init; }

    /// <summary>
    /// Digest du modèle.
    /// </summary>
    [JsonPropertyName("digest")]
    public required string Digest { get; init; }

    /// <summary>
    /// Détails du modèle.
    /// </summary>
    [JsonPropertyName("details")]
    public OllamaModelDetails? Details { get; init; }
}

/// <summary>
/// Détails d'un modèle Ollama.
/// </summary>
public sealed record OllamaModelDetails
{
    /// <summary>
    /// Format du modèle.
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; init; }

    /// <summary>
    /// Famille du modèle.
    /// </summary>
    [JsonPropertyName("family")]
    public string? Family { get; init; }

    /// <summary>
    /// Familles parentes.
    /// </summary>
    [JsonPropertyName("families")]
    public IReadOnlyList<string>? Families { get; init; }

    /// <summary>
    /// Nombre de paramètres.
    /// </summary>
    [JsonPropertyName("parameter_size")]
    public string? ParameterSize { get; init; }

    /// <summary>
    /// Type de quantisation.
    /// </summary>
    [JsonPropertyName("quantization_level")]
    public string? QuantizationLevel { get; init; }
}
