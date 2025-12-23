using LLMProxy.Domain.Entities;

namespace LLMProxy.Application.LLMProviders.Services.Orchestration;

/// <summary>
/// Résultat d'une opération orchestrée.
/// </summary>
/// <typeparam name="T">Type du résultat.</typeparam>
public sealed record OrchestratorResult<T>
{
    /// <summary>
    /// Indique si l'opération a réussi.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Résultat de l'opération (si succès).
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Provider utilisé pour l'exécution.
    /// </summary>
    public ProviderType? Provider { get; init; }

    /// <summary>
    /// Modèle utilisé.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Erreur rencontrée (si échec).
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Code d'erreur (si échec).
    /// </summary>
    public OrchestratorErrorCode? ErrorCode { get; init; }

    /// <summary>
    /// Métriques d'exécution.
    /// </summary>
    public ExecutionMetrics? Metrics { get; init; }
}
