namespace LLMProxy.Infrastructure.LLMProviders.Providers.Local.Ollama.Contracts;

/// <summary>
/// Représente un chunk de streaming Ollama.
/// </summary>
internal sealed record OllamaStreamChunk
{
    /// <summary>
    /// Modèle utilisé.
    /// </summary>
    public string? Model { get; init; }
    
    /// <summary>
    /// Message partiel.
    /// </summary>
    public OllamaMessage? Message { get; init; }
    
    /// <summary>
    /// Indique si le streaming est terminé.
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
