using System.Text.Json.Serialization;

namespace LLMProxy.Application.ApiTransformation.OpenAI.Contracts;

/// <summary>
/// Message au format OpenAI API.
/// </summary>
public sealed record OpenAIMessage
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
    /// Nom de l'auteur (optionnel).
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
