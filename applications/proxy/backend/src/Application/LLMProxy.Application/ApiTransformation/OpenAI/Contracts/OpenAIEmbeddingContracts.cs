using System.Text.Json.Serialization;

namespace LLMProxy.Application.ApiTransformation.OpenAI.Contracts;

/// <summary>
/// Requête d'embedding au format OpenAI API.
/// </summary>
public sealed record OpenAIEmbeddingRequest
{
    /// <summary>
    /// Identifiant du modèle à utiliser.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Texte(s) à encoder. Peut être une chaîne ou une liste.
    /// </summary>
    [JsonPropertyName("input")]
    public required object Input { get; init; }

    /// <summary>
    /// Format de sortie (float ou base64).
    /// </summary>
    [JsonPropertyName("encoding_format")]
    public string? EncodingFormat { get; init; }

    /// <summary>
    /// Dimensions de l'embedding (optionnel).
    /// </summary>
    [JsonPropertyName("dimensions")]
    public int? Dimensions { get; init; }

    /// <summary>
    /// Identifiant utilisateur.
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; init; }
}

/// <summary>
/// Réponse d'embedding au format OpenAI API.
/// </summary>
public sealed record OpenAIEmbeddingResponse
{
    /// <summary>
    /// Type d'objet (list).
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "list";

    /// <summary>
    /// Liste des embeddings.
    /// </summary>
    [JsonPropertyName("data")]
    public required IReadOnlyList<OpenAIEmbeddingData> Data { get; init; }

    /// <summary>
    /// Modèle utilisé.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Utilisation des tokens.
    /// </summary>
    [JsonPropertyName("usage")]
    public required OpenAIEmbeddingUsage Usage { get; init; }
}

/// <summary>
/// Données d'un embedding au format OpenAI.
/// </summary>
public sealed record OpenAIEmbeddingData
{
    /// <summary>
    /// Type d'objet (embedding).
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "embedding";

    /// <summary>
    /// Index de l'embedding.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; init; }

    /// <summary>
    /// Vecteur d'embedding.
    /// </summary>
    [JsonPropertyName("embedding")]
    public required IReadOnlyList<float> Embedding { get; init; }
}

/// <summary>
/// Utilisation des tokens pour embeddings.
/// </summary>
public sealed record OpenAIEmbeddingUsage
{
    /// <summary>
    /// Tokens en entrée.
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; init; }

    /// <summary>
    /// Total de tokens.
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; init; }
}
