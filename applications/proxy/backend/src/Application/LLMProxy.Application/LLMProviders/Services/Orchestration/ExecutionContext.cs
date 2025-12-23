using LLMProxy.Domain.Entities;

namespace LLMProxy.Application.LLMProviders.Services.Orchestration;

/// <summary>
/// Contexte d'exécution pour personnaliser le comportement.
/// </summary>
public sealed record ExecutionContext
{
    /// <summary>
    /// Force l'utilisation d'un provider spécifique.
    /// </summary>
    public ProviderType? ForceProvider { get; init; }

    /// <summary>
    /// Force l'utilisation d'un modèle spécifique.
    /// </summary>
    public string? ForceModel { get; init; }

    /// <summary>
    /// Désactive le failover.
    /// </summary>
    public bool DisableFailover { get; init; }

    /// <summary>
    /// Timeout personnalisé.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Nombre maximum de providers à essayer.
    /// </summary>
    public int? MaxProvidersToTry { get; init; }

    /// <summary>
    /// Identifiant de corrélation pour le tracing.
    /// </summary>
    public string? CorrelationId { get; init; }
}
