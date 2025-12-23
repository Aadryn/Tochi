namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.OpenAI.Contracts;

/// <summary>
/// Représente la réponse de la liste des modèles disponibles.
/// </summary>
internal sealed record OpenAIModelsResponse
{
    /// <summary>
    /// Liste des modèles disponibles.
    /// </summary>
    public List<OpenAIModelInfo>? Data { get; init; }
}
