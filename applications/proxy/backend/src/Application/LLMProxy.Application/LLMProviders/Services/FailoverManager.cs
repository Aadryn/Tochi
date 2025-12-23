using System.Collections.Concurrent;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.LLMProviders.Services;

/// <summary>
/// Gère le failover automatique entre providers LLM.
/// Conforme à ADR-032 (Circuit Breaker Pattern) et ADR-034 (Encapsulation).
/// </summary>
public interface IFailoverManager
{
    /// <summary>
    /// Exécute une requête avec failover automatique sur les providers alternatifs.
    /// </summary>
    /// <typeparam name="TResult">Type du résultat.</typeparam>
    /// <param name="providers">Liste ordonnée des providers à essayer.</param>
    /// <param name="operation">Opération à exécuter sur chaque provider.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Résultat de l'opération.</returns>
    Task<FailoverResult<TResult>> ExecuteWithFailoverAsync<TResult>(
        IReadOnlyList<ILLMProviderClient> providers,
        Func<ILLMProviderClient, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un provider est actuellement blacklisté.
    /// </summary>
    /// <param name="providerType">Type du provider.</param>
    /// <returns>True si le provider est blacklisté.</returns>
    bool IsBlacklisted(ProviderType providerType);

    /// <summary>
    /// Blackliste un provider pour une durée spécifiée.
    /// </summary>
    /// <param name="providerType">Type du provider.</param>
    /// <param name="duration">Durée du blacklist.</param>
    /// <param name="reason">Raison du blacklist.</param>
    void BlacklistProvider(ProviderType providerType, TimeSpan duration, string reason);

    /// <summary>
    /// Retire un provider de la blacklist.
    /// </summary>
    /// <param name="providerType">Type du provider.</param>
    void UnblacklistProvider(ProviderType providerType);

    /// <summary>
    /// Récupère les informations de blacklist pour tous les providers.
    /// </summary>
    IReadOnlyDictionary<ProviderType, BlacklistInfo> GetBlacklistedProviders();
}

/// <summary>
/// Résultat d'une opération avec failover.
/// </summary>
/// <typeparam name="T">Type du résultat.</typeparam>
public sealed record FailoverResult<T>
{
    /// <summary>
    /// Indique si l'opération a réussi.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Résultat de l'opération (si succès).
    /// </summary>
    public T? Result { get; init; }

    /// <summary>
    /// Provider qui a réussi l'opération.
    /// </summary>
    public ProviderType? SuccessfulProvider { get; init; }

    /// <summary>
    /// Liste des providers essayés avec leurs erreurs.
    /// </summary>
    public required IReadOnlyList<FailoverAttempt> Attempts { get; init; }

    /// <summary>
    /// Durée totale du failover.
    /// </summary>
    public TimeSpan TotalDuration { get; init; }
}

/// <summary>
/// Tentative de failover sur un provider.
/// </summary>
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
    public Exception? Exception { get; init; }

    /// <summary>
    /// Durée de la tentative.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Timestamp de la tentative.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Informations de blacklist d'un provider.
/// </summary>
public sealed record BlacklistInfo
{
    /// <summary>
    /// Date de mise en blacklist.
    /// </summary>
    public required DateTimeOffset BlacklistedAt { get; init; }

    /// <summary>
    /// Date d'expiration du blacklist.
    /// </summary>
    public required DateTimeOffset ExpiresAt { get; init; }

    /// <summary>
    /// Raison du blacklist.
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Nombre d'échecs ayant mené au blacklist.
    /// </summary>
    public int FailureCount { get; init; }
}

/// <summary>
/// Implémentation du gestionnaire de failover.
/// </summary>
public sealed class FailoverManager : IFailoverManager
{
    private readonly ILogger<FailoverManager> _logger;
    private readonly ConcurrentDictionary<ProviderType, BlacklistInfo> _blacklist = new();
    private readonly ConcurrentDictionary<ProviderType, int> _failureCounts = new();
    private readonly FailoverOptions _options;

    /// <summary>
    /// Constructeur.
    /// </summary>
    public FailoverManager(
        ILogger<FailoverManager> logger,
        FailoverOptions? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new FailoverOptions();
    }

    /// <inheritdoc />
    public async Task<FailoverResult<TResult>> ExecuteWithFailoverAsync<TResult>(
        IReadOnlyList<ILLMProviderClient> providers,
        Func<ILLMProviderClient, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        if (providers.Count == 0)
        {
            return new FailoverResult<TResult>
            {
                Success = false,
                Attempts = Array.Empty<FailoverAttempt>()
            };
        }

        var attempts = new List<FailoverAttempt>();
        var startTime = DateTimeOffset.UtcNow;

        // Filtrer les providers blacklistés
        var eligibleProviders = providers
            .Where(p => !IsBlacklisted(p.Type))
            .ToList();

        if (eligibleProviders.Count == 0)
        {
            _logger.LogWarning(
                "Tous les providers sont blacklistés, tentative avec le premier provider de la liste");
            eligibleProviders = providers.Take(1).ToList();
        }

        foreach (var provider in eligibleProviders)
        {
            var attemptStart = DateTimeOffset.UtcNow;

            try
            {
                _logger.LogDebug(
                    "Tentative d'exécution sur provider {Provider}",
                    provider.Type);

                var result = await operation(provider, cancellationToken);

                // Succès - réinitialiser le compteur d'échecs
                _failureCounts.TryRemove(provider.Type, out _);

                var attempt = new FailoverAttempt
                {
                    ProviderType = provider.Type,
                    Success = true,
                    Duration = DateTimeOffset.UtcNow - attemptStart
                };
                attempts.Add(attempt);

                _logger.LogInformation(
                    "Exécution réussie sur provider {Provider} après {AttemptCount} tentative(s)",
                    provider.Type,
                    attempts.Count);

                return new FailoverResult<TResult>
                {
                    Success = true,
                    Result = result,
                    SuccessfulProvider = provider.Type,
                    Attempts = attempts,
                    TotalDuration = DateTimeOffset.UtcNow - startTime
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Opération annulée sur provider {Provider}", provider.Type);
                throw;
            }
            catch (Exception ex)
            {
                var attempt = new FailoverAttempt
                {
                    ProviderType = provider.Type,
                    Success = false,
                    Exception = ex,
                    Duration = DateTimeOffset.UtcNow - attemptStart
                };
                attempts.Add(attempt);

                _logger.LogWarning(
                    ex,
                    "Échec sur provider {Provider}: {Message}",
                    provider.Type,
                    ex.Message);

                // Incrémenter le compteur d'échecs
                var failureCount = _failureCounts.AddOrUpdate(
                    provider.Type,
                    1,
                    (_, count) => count + 1);

                // Blacklister si seuil atteint
                if (failureCount >= _options.FailuresBeforeBlacklist)
                {
                    BlacklistProvider(
                        provider.Type,
                        _options.BlacklistDuration,
                        $"Échecs consécutifs: {failureCount}. Dernière erreur: {ex.Message}");
                }
            }
        }

        _logger.LogError(
            "Toutes les tentatives de failover ont échoué ({AttemptCount} providers essayés)",
            attempts.Count);

        return new FailoverResult<TResult>
        {
            Success = false,
            Attempts = attempts,
            TotalDuration = DateTimeOffset.UtcNow - startTime
        };
    }

    /// <inheritdoc />
    public bool IsBlacklisted(ProviderType providerType)
    {
        if (!_blacklist.TryGetValue(providerType, out var info))
            return false;

        // Vérifier si le blacklist a expiré
        if (DateTimeOffset.UtcNow >= info.ExpiresAt)
        {
            UnblacklistProvider(providerType);
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public void BlacklistProvider(ProviderType providerType, TimeSpan duration, string reason)
    {
        var info = new BlacklistInfo
        {
            BlacklistedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(duration),
            Reason = reason,
            FailureCount = _failureCounts.GetValueOrDefault(providerType, 0)
        };

        _blacklist.AddOrUpdate(providerType, info, (_, _) => info);

        _logger.LogWarning(
            "Provider {Provider} blacklisté jusqu'à {ExpiresAt}: {Reason}",
            providerType,
            info.ExpiresAt,
            reason);
    }

    /// <inheritdoc />
    public void UnblacklistProvider(ProviderType providerType)
    {
        if (_blacklist.TryRemove(providerType, out var info))
        {
            _failureCounts.TryRemove(providerType, out _);

            _logger.LogInformation(
                "Provider {Provider} retiré de la blacklist (était blacklisté pour: {Reason})",
                providerType,
                info.Reason);
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<ProviderType, BlacklistInfo> GetBlacklistedProviders()
    {
        // Nettoyer les entrées expirées
        var expiredProviders = _blacklist
            .Where(kvp => DateTimeOffset.UtcNow >= kvp.Value.ExpiresAt)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var provider in expiredProviders)
        {
            UnblacklistProvider(provider);
        }

        return _blacklist.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}

/// <summary>
/// Options de configuration du failover.
/// </summary>
public sealed class FailoverOptions
{
    /// <summary>
    /// Nombre d'échecs avant blacklist d'un provider.
    /// </summary>
    public int FailuresBeforeBlacklist { get; init; } = 3;

    /// <summary>
    /// Durée du blacklist.
    /// </summary>
    public TimeSpan BlacklistDuration { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Timeout par provider.
    /// </summary>
    public TimeSpan ProviderTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
