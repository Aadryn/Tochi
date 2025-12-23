namespace LLMProxy.Infrastructure.LLMProviders.Providers.Local.Ollama.Contracts;

/// <summary>
/// Représente une réponse de chat d'Ollama.
/// </summary>
internal sealed record OllamaChatResponse
{
    /// <summary>
    /// Modèle utilisé.
    /// </summary>
    public string? Model { get; init; }
    
    /// <summary>
    /// Message généré.
    /// </summary>
    public OllamaMessage? Message { get; init; }
    
    /// <summary>
    /// Indique si la génération est terminée.
    /// </summary>
    public bool Done { get; init; }
    
    /// <summary>
    /// Nombre de tokens évalués dans le prompt.
    /// </summary>
    public int? PromptEvalCount { get; init; }
    
    /// <summary>
    /// Nombre de tokens générés.
    /// </summary>
    public int? EvalCount { get; init; }
}
