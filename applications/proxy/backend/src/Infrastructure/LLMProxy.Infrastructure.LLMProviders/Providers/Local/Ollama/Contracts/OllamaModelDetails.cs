namespace LLMProxy.Infrastructure.LLMProviders.Providers.Local.Ollama.Contracts;

/// <summary>
/// Détails d'un modèle Ollama.
/// </summary>
internal sealed record OllamaModelDetails
{
    /// <summary>
    /// Taille des paramètres (ex: "7B", "13B").
    /// </summary>
    public string? ParameterSize { get; init; }
    
    /// <summary>
    /// Niveau de quantization (ex: "Q4_0", "Q8_0").
    /// </summary>
    public string? QuantizationLevel { get; init; }
}
