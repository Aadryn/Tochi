namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.AzureOpenAI.Contracts;

/// <summary>
/// Représente les statistiques d'utilisation des tokens.
/// </summary>
internal sealed record AzureUsage
{
    /// <summary>
    /// Nombre de tokens utilisés dans le prompt.
    /// </summary>
    public int PromptTokens { get; init; }
    
    /// <summary>
    /// Nombre de tokens générés dans la complétion.
    /// </summary>
    public int CompletionTokens { get; init; }
}
