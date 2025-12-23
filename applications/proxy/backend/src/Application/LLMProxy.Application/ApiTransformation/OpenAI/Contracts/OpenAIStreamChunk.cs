using System.Text.Json.Serialization;

namespace LLMProxy.Application.ApiTransformation.OpenAI.Contracts;

/// <summary>
/// Chunk de streaming au format OpenAI API (Server-Sent Events).
/// </summary>
public sealed record OpenAIStreamChunk
{
    /// <summary>
    /// Identifiant unique du chunk.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Type d'objet (chat.completion.chunk).
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "chat.completion.chunk";

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
    public required IReadOnlyList<OpenAIStreamChoice> Choices { get; init; }

    /// <summary>
    /// Identifiant système.
    /// </summary>
    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; init; }
}

/// <summary>
/// Choix de réponse streaming au format OpenAI.
/// </summary>
public sealed record OpenAIStreamChoice
{
    /// <summary>
    /// Index du choix.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; init; }

    /// <summary>
    /// Delta du message.
    /// </summary>
    [JsonPropertyName("delta")]
    public required OpenAIDelta Delta { get; init; }

    /// <summary>
    /// Raison de fin de génération (null si pas fini).
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }
}

/// <summary>
/// Delta de contenu pour le streaming.
/// </summary>
public sealed record OpenAIDelta
{
    /// <summary>
    /// Rôle (uniquement dans le premier chunk).
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; init; }

    /// <summary>
    /// Contenu incrémental.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; init; }
}
