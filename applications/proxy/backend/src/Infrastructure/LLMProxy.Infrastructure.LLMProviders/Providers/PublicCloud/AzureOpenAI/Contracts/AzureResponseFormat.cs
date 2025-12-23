namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.AzureOpenAI.Contracts;

/// <summary>
/// Représente le format de réponse souhaité.
/// </summary>
internal sealed record AzureResponseFormat
{
    /// <summary>
    /// Type de format (text, json_object).
    /// </summary>
    public required string Type { get; init; }
}
