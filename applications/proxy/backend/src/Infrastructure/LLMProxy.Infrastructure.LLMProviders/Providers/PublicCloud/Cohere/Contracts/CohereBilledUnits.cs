namespace LLMProxy.Infrastructure.LLMProviders.Providers.PublicCloud.Cohere.Contracts;

/// <summary>
/// Représente les unités facturées pour une requête Cohere.
/// </summary>
internal sealed record CohereBilledUnits
{
    /// <summary>
    /// Nombre de tokens d'entrée facturés.
    /// </summary>
    public int? InputTokens { get; init; }
}
