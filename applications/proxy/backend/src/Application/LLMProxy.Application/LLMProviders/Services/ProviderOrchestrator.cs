using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.LLMProviders.Services;

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

/// <summary>
/// Codes d'erreur de l'orchestrateur.
/// </summary>
public enum OrchestratorErrorCode
{
    /// <summary>
    /// Aucun provider disponible.
    /// </summary>
    NoProviderAvailable,

    /// <summary>
    /// Aucun modèle compatible trouvé.
    /// </summary>
    NoCompatibleModel,

    /// <summary>
    /// Tous les providers ont échoué.
    /// </summary>
    AllProvidersFailed,

    /// <summary>
    /// Requête invalide.
    /// </summary>
    InvalidRequest,

    /// <summary>
    /// Timeout global dépassé.
    /// </summary>
    GlobalTimeout,

    /// <summary>
    /// Opération annulée.
    /// </summary>
    Cancelled
}

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

/// <summary>
/// Implémentation de l'orchestrateur de providers.
/// </summary>
public sealed class ProviderOrchestrator : IProviderOrchestrator
{
    private readonly ILLMProviderClientFactory _clientFactory;
    private readonly IProviderSelector _providerSelector;
    private readonly IFailoverManager _failoverManager;
    private readonly ILogger<ProviderOrchestrator> _logger;

    /// <summary>
    /// Constructeur.
    /// </summary>
    public ProviderOrchestrator(
        ILLMProviderClientFactory clientFactory,
        IProviderSelector providerSelector,
        IFailoverManager failoverManager,
        ILogger<ProviderOrchestrator> logger)
    {
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _providerSelector = providerSelector ?? throw new ArgumentNullException(nameof(providerSelector));
        _failoverManager = failoverManager ?? throw new ArgumentNullException(nameof(failoverManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<OrchestratorResult<LLMResponse>> ExecuteCompletionAsync(
        LLMRequest request,
        ExecutionContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        var correlationId = context?.CorrelationId ?? Guid.NewGuid().ToString("N")[..8];

        _logger.LogInformation(
            "[{CorrelationId}] Début exécution complétion: Model={Model}, MaxTokens={MaxTokens}",
            correlationId,
            request.Model,
            request.MaxTokens);

        try
        {
            // Obtenir les providers éligibles
            var providers = GetEligibleProviders(request, context);

            if (providers.Count == 0)
            {
                return CreateErrorResult<LLMResponse>(
                    OrchestratorErrorCode.NoProviderAvailable,
                    "Aucun provider disponible pour cette requête",
                    startTime);
            }

            // Créer les clients
            var clients = providers
                .Select(p => _clientFactory.CreateClient(p))
                .ToList();

            // Exécuter avec failover
            var failoverResult = await _failoverManager.ExecuteWithFailoverAsync(
                clients,
                async (client, ct) => await client.ChatCompletionAsync(request, ct),
                cancellationToken);

            if (failoverResult.Success && failoverResult.Result is not null)
            {
                return new OrchestratorResult<LLMResponse>
                {
                    Success = true,
                    Data = failoverResult.Result,
                    Provider = failoverResult.SuccessfulProvider,
                    Model = request.Model.ToString(),
                    Metrics = CreateMetrics(startTime, failoverResult.Attempts.Count, failoverResult.Result.Usage)
                };
            }

            return CreateErrorResult<LLMResponse>(
                OrchestratorErrorCode.AllProvidersFailed,
                BuildFailureMessage(failoverResult.Attempts),
                startTime,
                failoverResult.Attempts.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("[{CorrelationId}] Opération annulée", correlationId);
            return CreateErrorResult<LLMResponse>(
                OrchestratorErrorCode.Cancelled,
                "Opération annulée par l'utilisateur",
                startTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] Erreur inattendue", correlationId);
            return CreateErrorResult<LLMResponse>(
                OrchestratorErrorCode.AllProvidersFailed,
                $"Erreur inattendue: {ex.Message}",
                startTime);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<OrchestratorResult<LLMResponse>> ExecuteStreamingAsync(
        LLMRequest request,
        ExecutionContext? context = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        var correlationId = context?.CorrelationId ?? Guid.NewGuid().ToString("N")[..8];

        _logger.LogInformation(
            "[{CorrelationId}] Début exécution streaming: Model={Model}",
            correlationId,
            request.Model);

        // Obtenir les providers éligibles
        var providers = GetEligibleProviders(request, context);

        if (providers.Count == 0)
        {
            yield return CreateErrorResult<LLMResponse>(
                OrchestratorErrorCode.NoProviderAvailable,
                "Aucun provider disponible pour cette requête",
                startTime);
            yield break;
        }

        var firstChunkReceived = false;
        var chunkCount = 0;
        Exception? lastException = null;
        var success = false;

        foreach (var providerType in providers)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield return CreateErrorResult<LLMResponse>(
                    OrchestratorErrorCode.Cancelled,
                    "Opération annulée",
                    startTime);
                yield break;
            }

            if (_failoverManager.IsBlacklisted(providerType))
                continue;

            var client = _clientFactory.CreateClient(providerType);

            // Variable pour stocker les chunks avant yield
            var chunks = new List<OrchestratorResult<LLMResponse>>();

            try
            {
                await foreach (var chunk in client.ChatCompletionStreamAsync(request, cancellationToken))
                {
                    if (!firstChunkReceived)
                    {
                        firstChunkReceived = true;
                        _logger.LogDebug(
                            "[{CorrelationId}] Premier chunk reçu après {Duration}ms",
                            correlationId,
                            (DateTimeOffset.UtcNow - startTime).TotalMilliseconds);
                    }

                    chunkCount++;
                    chunks.Add(new OrchestratorResult<LLMResponse>
                    {
                        Success = true,
                        Data = chunk,
                        Provider = providerType,
                        Model = request.Model.ToString()
                    });
                }

                success = true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(
                    ex,
                    "[{CorrelationId}] Échec streaming sur {Provider}, failover...",
                    correlationId,
                    providerType);
                continue;
            }

            // Yield les chunks après le try-catch
            foreach (var chunk in chunks)
            {
                yield return chunk;
            }

            if (success)
            {
                _logger.LogInformation(
                    "[{CorrelationId}] Streaming terminé: {ChunkCount} chunks via {Provider}",
                    correlationId,
                    chunkCount,
                    providerType);
                yield break;
            }
        }

        // Tous les providers ont échoué
        if (!success)
        {
            yield return CreateErrorResult<LLMResponse>(
                OrchestratorErrorCode.AllProvidersFailed,
                lastException?.Message ?? "Tous les providers ont échoué",
                startTime);
        }
    }

    /// <inheritdoc />
    public async Task<OrchestratorResult<EmbeddingResponse>> ExecuteEmbeddingAsync(
        EmbeddingRequest request,
        ExecutionContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        var correlationId = context?.CorrelationId ?? Guid.NewGuid().ToString("N")[..8];

        _logger.LogInformation(
            "[{CorrelationId}] Début exécution embedding: Model={Model}, Inputs={Count}",
            correlationId,
            request.Model,
            request.Inputs.Count);

        try
        {
            // Sélectionner les providers supportant les embeddings
            var criteria = new SelectionCriteria
            {
                PreferredProviders = context?.ForceProvider is not null
                    ? [context.ForceProvider.Value]
                    : null
            };

            var providers = _providerSelector.Select(criteria);

            if (providers.Count == 0)
            {
                return CreateErrorResult<EmbeddingResponse>(
                    OrchestratorErrorCode.NoProviderAvailable,
                    "Aucun provider ne supporte les embeddings",
                    startTime);
            }

            var clients = providers
                .Select(p => _clientFactory.CreateClient(p))
                .ToList();

            var failoverResult = await _failoverManager.ExecuteWithFailoverAsync(
                clients,
                async (client, ct) => await client.EmbeddingsAsync(request, ct),
                cancellationToken);

            if (failoverResult.Success && failoverResult.Result is not null)
            {
                return new OrchestratorResult<EmbeddingResponse>
                {
                    Success = true,
                    Data = failoverResult.Result,
                    Provider = failoverResult.SuccessfulProvider,
                    Model = request.Model.ToString(),
                    Metrics = new ExecutionMetrics
                    {
                        TotalDuration = DateTimeOffset.UtcNow - startTime,
                        ProvidersAttempted = failoverResult.Attempts.Count,
                        InputTokens = failoverResult.Result.Usage.TotalTokens
                    }
                };
            }

            return CreateErrorResult<EmbeddingResponse>(
                OrchestratorErrorCode.AllProvidersFailed,
                BuildFailureMessage(failoverResult.Attempts),
                startTime,
                failoverResult.Attempts.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CreateErrorResult<EmbeddingResponse>(
                OrchestratorErrorCode.Cancelled,
                "Opération annulée",
                startTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] Erreur embedding", correlationId);
            return CreateErrorResult<EmbeddingResponse>(
                OrchestratorErrorCode.AllProvidersFailed,
                ex.Message,
                startTime);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<ProviderType, ProviderHealthStatus>> GetProvidersHealthAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<ProviderType, ProviderHealthStatus>();
        var blacklisted = _failoverManager.GetBlacklistedProviders();
        var supportedTypes = _clientFactory.GetSupportedTypes();

        foreach (var providerType in supportedTypes)
        {
            var isBlacklisted = blacklisted.ContainsKey(providerType);
            var client = _clientFactory.CreateClient(providerType);

            try
            {
                var isHealthy = await client.IsHealthyAsync(cancellationToken);

                results[providerType] = new ProviderHealthStatus
                {
                    IsHealthy = isHealthy && !isBlacklisted,
                    IsBlacklisted = isBlacklisted,
                    LastChecked = DateTimeOffset.UtcNow,
                    LastError = isBlacklisted ? blacklisted[providerType].Reason : null
                };
            }
            catch (Exception ex)
            {
                results[providerType] = new ProviderHealthStatus
                {
                    IsHealthy = false,
                    IsBlacklisted = isBlacklisted,
                    LastChecked = DateTimeOffset.UtcNow,
                    LastError = ex.Message
                };
            }
        }

        return results;
    }

    private IReadOnlyList<ProviderType> GetEligibleProviders(
        LLMRequest request,
        ExecutionContext? context)
    {
        // Si provider forcé
        if (context?.ForceProvider is not null)
        {
            return [context.ForceProvider.Value];
        }

        var criteria = new SelectionCriteria();

        return _providerSelector.Select(criteria);
    }

    private static OrchestratorResult<T> CreateErrorResult<T>(
        OrchestratorErrorCode errorCode,
        string message,
        DateTimeOffset startTime,
        int providersAttempted = 0)
    {
        return new OrchestratorResult<T>
        {
            Success = false,
            Error = message,
            ErrorCode = errorCode,
            Metrics = new ExecutionMetrics
            {
                TotalDuration = DateTimeOffset.UtcNow - startTime,
                ProvidersAttempted = providersAttempted
            }
        };
    }

    private static ExecutionMetrics CreateMetrics(
        DateTimeOffset startTime,
        int providersAttempted,
        TokenUsage? usage)
    {
        return new ExecutionMetrics
        {
            TotalDuration = DateTimeOffset.UtcNow - startTime,
            ProvidersAttempted = providersAttempted,
            InputTokens = usage?.InputTokens,
            OutputTokens = usage?.OutputTokens
        };
    }

    private static string BuildFailureMessage(IReadOnlyList<FailoverAttempt> attempts)
    {
        if (attempts.Count == 0)
            return "Aucun provider n'a pu être contacté";

        var messages = attempts
            .Where(a => !a.Success && a.Exception is not null)
            .Select(a => $"{a.ProviderType}: {a.Exception!.Message}");

        return $"Échec sur tous les providers: {string.Join("; ", messages)}";
    }
}
