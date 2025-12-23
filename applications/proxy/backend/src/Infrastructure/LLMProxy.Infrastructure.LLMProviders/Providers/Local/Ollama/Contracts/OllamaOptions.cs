namespace LLMProxy.Infrastructure.LLMProviders.Providers.Local.Ollama.Contracts;

/// <summary>
/// Options de génération pour Ollama.
/// </summary>
internal sealed record OllamaOptions
{
    /// <summary>
    /// Température de génération.
    /// </summary>
    public decimal? Temperature { get; init; }
    
    /// <summary>
    /// Paramètre top-p.
    /// </summary>
    public decimal? TopP { get; init; }
    
    /// <summary>
    /// Nombre de tokens à prédire.
    /// </summary>
    public int? NumPredict { get; init; }
    
    /// <summary>
    /// Séquences d'arrêt.
    /// </summary>
    public List<string>? Stop { get; init; }
    
    /// <summary>
    /// Seed pour génération déterministe.
    /// </summary>
    public int? Seed { get; init; }
}
