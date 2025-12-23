using LLMProxy.Domain.Entities;

namespace LLMProxy.Application.LLMProviders.Services.Failover;

/// <summary>
/// Tentative de failover sur un provider.
/// </summary>
/// <remarks>
/// Capture les détails d'une tentative individuelle d'exécution sur un provider LLM,
/// incluant le résultat, la durée et les erreurs rencontrées.
/// </remarks>
public sealed record FailoverAttempt
{
    /// <summary>
    /// Type du provider tenté.
    /// </summary>
    public required ProviderType ProviderType { get; init; }

    /// <summary>
    /// Indique si la tentative a réussi.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Exception rencontrée (si échec).
    /// </summary>
    /// <remarks>
    /// Null si Success est true.
    /// </remarks>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Durée de la tentative.
    /// </summary>
    /// <remarks>
    /// Temps écoulé depuis le début de l'appel au provider jusqu'à la réponse ou l'erreur.
    /// </remarks>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Timestamp de la tentative.
    /// </summary>
    /// <remarks>
    /// Heure UTC à laquelle la tentative a été initiée.
    /// </remarks>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
