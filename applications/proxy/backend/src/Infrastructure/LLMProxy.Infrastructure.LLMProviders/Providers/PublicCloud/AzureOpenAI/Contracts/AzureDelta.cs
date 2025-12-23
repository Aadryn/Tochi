namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.AzureOpenAI.Contracts;

/// <summary>
/// Représente un delta de contenu dans un stream.
/// </summary>
internal sealed record AzureDelta
{
    /// <summary>
    /// Fragment de contenu incrémental.
    /// </summary>
    public string? Content { get; init; }
}
