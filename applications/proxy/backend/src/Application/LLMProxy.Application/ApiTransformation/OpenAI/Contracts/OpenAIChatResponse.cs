using System.Text.Json.Serialization;

namespace LLMProxy.Application.ApiTransformation.OpenAI.Contracts;

/// <summary>
/// Réponse de chat au format OpenAI API.
/// </summary>
public sealed record OpenAIChatResponse
{
    /// <summary>
    /// Identifiant unique de la réponse.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Type d'objet (chat.completion).
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "chat.completion";

    /// <summary>
    /// Timestamp de création (Unix epoch).
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; init; }

    /// <summary>
    /// Modèle utilisé.
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Liste des choix de réponse.
    /// </summary>
    [JsonPropertyName("choices")]
    public required IReadOnlyList<OpenAIChatChoice> Choices { get; init; }

    /// <summary>
    /// Utilisation des tokens.
    /// </summary>
    [JsonPropertyName("usage")]
    public OpenAIUsage? Usage { get; init; }

    /// <summary>
    /// Identifiant système.
    /// </summary>
    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; init; }
}

/// <summary>
/// Choix de réponse au format OpenAI.
/// </summary>
public sealed record OpenAIChatChoice
{
    /// <summary>
    /// Index du choix.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; init; }

    /// <summary>
    /// Message de réponse.
    /// </summary>
    [JsonPropertyName("message")]
    public required OpenAIMessage Message { get; init; }

    /// <summary>
    /// Raison de fin de génération.
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }
}

/// <summary>
/// Utilisation des tokens au format OpenAI.
/// </summary>
public sealed record OpenAIUsage
{
    /// <summary>
    /// Tokens en entrée (prompt).
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; init; }

    /// <summary>
    /// Tokens en sortie (completion).
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; init; }

    /// <summary>
    /// Total de tokens.
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; init; }
}
