namespace LLMProxy.Application.LLMProviders.Services.Orchestration;

/// <summary>
/// Métriques d'exécution d'une requête.
/// </summary>
public sealed record ExecutionMetrics
{
    /// <summary>
    /// Durée totale de l'exécution.
    /// </summary>
    public TimeSpan TotalDuration { get; init; }

    /// <summary>
    /// Nombre de providers essayés.
    /// </summary>
    public int ProvidersAttempted { get; init; }

    /// <summary>
    /// Tokens d'entrée consommés.
    /// </summary>
    public int? InputTokens { get; init; }

    /// <summary>
    /// Tokens de sortie générés.
    /// </summary>
    public int? OutputTokens { get; init; }

    /// <summary>
    /// Latence du premier token (streaming).
    /// </summary>
    public TimeSpan? TimeToFirstToken { get; init; }
}
