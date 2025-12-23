using System.Text.Json.Serialization;

namespace LLMProxy.Application.ApiTransformation.OpenAI.Contracts;

/// <summary>
/// Réponse de liste des modèles au format OpenAI API.
/// </summary>
public sealed record OpenAIModelsResponse
{
    /// <summary>
    /// Type d'objet (list).
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "list";

    /// <summary>
    /// Liste des modèles disponibles.
    /// </summary>
    [JsonPropertyName("data")]
    public required IReadOnlyList<OpenAIModelInfo> Data { get; init; }
}

/// <summary>
/// Information sur un modèle au format OpenAI.
/// </summary>
public sealed record OpenAIModelInfo
{
    /// <summary>
    /// Identifiant du modèle.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Type d'objet (model).
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "model";

    /// <summary>
    /// Timestamp de création (Unix epoch).
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; init; }

    /// <summary>
    /// Propriétaire du modèle.
    /// </summary>
    [JsonPropertyName("owned_by")]
    public required string OwnedBy { get; init; }
}
