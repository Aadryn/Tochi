using System.Collections.Concurrent;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.LLMProviders.Services.Failover;

/// <summary>
/// Implémentation du gestionnaire de failover.
/// </summary>
/// <remarks>
/// Coordonne les tentatives d'exécution sur plusieurs providers avec gestion automatique
/// de blacklist basée sur le nombre d'échecs consécutifs.
/// </remarks>
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
