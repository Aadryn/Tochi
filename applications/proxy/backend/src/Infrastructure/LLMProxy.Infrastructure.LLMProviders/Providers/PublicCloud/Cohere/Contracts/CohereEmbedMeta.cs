namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Représente les métadonnées d'une réponse d'embedding Cohere.
/// </summary>
internal sealed record CohereEmbedMeta
{
    /// <summary>
    /// Unités facturées pour l'embedding.
    /// </summary>
    public CohereBilledUnits? BilledUnits { get; init; }
}
