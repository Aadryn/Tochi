namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.OpenAI.Contracts;

/// <summary>
/// Représente les informations d'un modèle OpenAI.
/// </summary>
internal sealed record OpenAIModelInfo
{
    /// <summary>
    /// Identifiant du modèle.
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Propriétaire du modèle.
    /// </summary>
    public string? OwnedBy { get; init; }
    
    /// <summary>
    /// Date de création (Unix epoch).
    /// </summary>
    public long Created { get; init; }
}
