namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.OpenAI.Contracts;

/// <summary>
/// Représente le format de réponse souhaité.
/// </summary>
internal sealed record OpenAIResponseFormat
{
    /// <summary>
    /// Type de format (text, json_object).
    /// </summary>
    public required string Type { get; init; }
}
