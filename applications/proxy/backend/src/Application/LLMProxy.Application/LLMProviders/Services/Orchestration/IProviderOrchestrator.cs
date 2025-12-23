using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.LLMProviders.Services.Orchestration;

/// <summary>
/// Orchestrateur intelligent de providers LLM.
/// Point d'entrée principal pour l'exécution des requêtes LLM avec routing et failover.
/// Conforme à ADR-032 (Circuit Breaker) et ADR-035 (Single Responsibility).
/// </summary>
public interface IProviderOrchestrator
{
    /// <summary>
    /// Exécute une requête de complétion avec routing intelligent et failover automatique.
    /// </summary>
    /// <param name="request">Requête LLM normalisée.</param>
    /// <param name="context">Contexte d'exécution optionnel.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Réponse LLM normalisée.</returns>
    Task<OrchestratorResult<LLMResponse>> ExecuteCompletionAsync(
        LLMRequest request,
        ExecutionContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exécute une requête de streaming avec routing intelligent et failover automatique.
    /// </summary>
    /// <param name="request">Requête LLM normalisée.</param>
    /// <param name="context">Contexte d'exécution optionnel.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Stream de chunks de réponse.</returns>
    IAsyncEnumerable<OrchestratorResult<LLMResponse>> ExecuteStreamingAsync(
        LLMRequest request,
        ExecutionContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exécute une requête d'embeddings avec routing intelligent et failover automatique.
    /// </summary>
    /// <param name="request">Requête d'embeddings.</param>
    /// <param name="context">Contexte d'exécution optionnel.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Vecteurs d'embeddings.</returns>
    Task<OrchestratorResult<EmbeddingResponse>> ExecuteEmbeddingAsync(
        EmbeddingRequest request,
        ExecutionContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtient l'état de santé de tous les providers.
    /// </summary>
    Task<IReadOnlyDictionary<ProviderType, ProviderHealthStatus>> GetProvidersHealthAsync(
        CancellationToken cancellationToken = default);
}
