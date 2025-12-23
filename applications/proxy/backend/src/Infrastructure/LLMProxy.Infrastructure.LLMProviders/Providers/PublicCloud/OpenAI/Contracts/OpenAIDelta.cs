namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.OpenAI.Contracts;

/// <summary>
/// Représente un delta de contenu dans un stream.
/// </summary>
internal sealed record OpenAIDelta
{
    /// <summary>
    /// Fragment de contenu incrémental.
    /// </summary>
    public string? Content { get; init; }
}
