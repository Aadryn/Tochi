namespace LLMProxy.Application.LLMProviders.Services.Orchestration;

/// <summary>
/// État de santé d'un provider.
/// </summary>
public sealed record ProviderHealthStatus
{
    /// <summary>
    /// Indique si le provider est disponible.
    /// </summary>
    public required bool IsHealthy { get; init; }

    /// <summary>
    /// Indique si le provider est blacklisté.
    /// </summary>
    public bool IsBlacklisted { get; init; }

    /// <summary>
    /// Latence moyenne récente.
    /// </summary>
    public TimeSpan? AverageLatency { get; init; }

    /// <summary>
    /// Taux de succès récent (0-1).
    /// </summary>
    public double? SuccessRate { get; init; }

    /// <summary>
    /// Dernière erreur rencontrée.
    /// </summary>
    public string? LastError { get; init; }

    /// <summary>
    /// Timestamp de la dernière vérification.
    /// </summary>
    public DateTimeOffset LastChecked { get; init; }
}
