using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.LLMProviders.Services.Failover;

/// <summary>
/// Gère le failover automatique entre providers LLM.
/// Conforme à ADR-032 (Circuit Breaker Pattern) et ADR-034 (Encapsulation).
/// </summary>
/// <remarks>
/// Ce service coordonne les tentatives sur plusieurs providers avec gestion automatique
/// de blacklist, retry et circuit breaker pour assurer la résilience du système.
/// </remarks>
public interface IFailoverManager
{
    /// <summary>
    /// Exécute une requête avec failover automatique sur les providers alternatifs.
    /// </summary>
    /// <typeparam name="TResult">Type du résultat.</typeparam>
    /// <param name="providers">Liste ordonnée des providers à essayer.</param>
    /// <param name="operation">Opération à exécuter sur chaque provider.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Résultat de l'opération avec détails des tentatives effectuées.</returns>
    /// <remarks>
    /// La méthode essaie les providers dans l'ordre jusqu'à obtenir un succès ou épuiser la liste.
    /// Les providers blacklistés sont automatiquement filtrés, sauf si tous le sont.
    /// </remarks>
    Task<FailoverResult<TResult>> ExecuteWithFailoverAsync<TResult>(
        IReadOnlyList<ILLMProviderClient> providers,
        Func<ILLMProviderClient, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie si un provider est actuellement blacklisté.
    /// </summary>
    /// <param name="providerType">Type du provider.</param>
    /// <returns>True si le provider est blacklisté.</returns>
    /// <remarks>
    /// Cette méthode nettoie automatiquement les blacklists expirés lors de la vérification.
    /// </remarks>
    bool IsBlacklisted(ProviderType providerType);

    /// <summary>
    /// Blackliste un provider pour une durée spécifiée.
    /// </summary>
    /// <param name="providerType">Type du provider.</param>
    /// <param name="duration">Durée du blacklist.</param>
    /// <param name="reason">Raison du blacklist.</param>
    /// <remarks>
    /// Le provider ne sera plus utilisé pour les nouvelles requêtes jusqu'à expiration du blacklist.
    /// </remarks>
    void BlacklistProvider(ProviderType providerType, TimeSpan duration, string reason);

    /// <summary>
    /// Retire un provider de la blacklist.
    /// </summary>
    /// <param name="providerType">Type du provider.</param>
    /// <remarks>
    /// Réinitialise également le compteur d'échecs pour ce provider.
    /// </remarks>
    void UnblacklistProvider(ProviderType providerType);

    /// <summary>
    /// Récupère les informations de blacklist pour tous les providers.
    /// </summary>
    /// <returns>Dictionnaire des providers blacklistés avec leurs informations.</returns>
    /// <remarks>
    /// Cette méthode nettoie automatiquement les entrées expirées avant de retourner les résultats.
    /// </remarks>
    IReadOnlyDictionary<ProviderType, BlacklistInfo> GetBlacklistedProviders();
}
