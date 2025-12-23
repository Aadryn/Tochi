namespace LLMProxy.Infrastructure.LLMProviders.Providers.Local.Ollama.Contracts;

/// <summary>
/// Informations sur un modèle Ollama.
/// </summary>
internal sealed record OllamaModelInfo
{
    /// <summary>
    /// Nom du modèle.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Taille du modèle en octets.
    /// </summary>
    public long Size { get; init; }
    
    /// <summary>
    /// Hash du modèle.
    /// </summary>
    public string? Digest { get; init; }
    
    /// <summary>
    /// Date de dernière modification.
    /// </summary>
    public DateTime? ModifiedAt { get; init; }
    
    /// <summary>
    /// Détails du modèle.
    /// </summary>
    public OllamaModelDetails? Details { get; init; }
}
