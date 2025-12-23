namespace LLMProxy.Infrastructure.LLMProviders.Providers.Local.Ollama.Contracts;

/// <summary>
/// Représente la réponse de la liste des modèles Ollama.
/// </summary>
internal sealed record OllamaModelsResponse
{
    /// <summary>
    /// Liste des modèles disponibles.
    /// </summary>
    public List<OllamaModelInfo>? Models { get; init; }
}
