namespace LLMProxy.Infrastructure.Redis.Idempotency;

/// <summary>
/// Store pour cacher les réponses HTTP idempotentes.
/// Permet de rejouer la même réponse si une requête est exécutée plusieurs fois avec la même clé d'idempotence.
/// Conforme à ADR-022 (Idempotence).
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Récupère une réponse HTTP précédemment cachée par clé d'idempotence.
    /// </summary>
    /// <param name="idempotencyKey">Clé d'idempotence unique (UUID fourni par le client).</param>
    /// <param name="ct">Jeton d'annulation pour stopper l'opération.</param>
    /// <returns>La réponse cachée si elle existe, sinon null.</returns>
    Task<CachedResponse?> GetAsync(string idempotencyKey, CancellationToken ct = default);

    /// <summary>
    /// Stocke une réponse HTTP avec une durée de vie limitée (TTL).
    /// La réponse sera automatiquement supprimée après expiration du TTL.
    /// </summary>
    /// <param name="idempotencyKey">Clé d'idempotence unique.</param>
    /// <param name="response">Réponse HTTP à cacher.</param>
    /// <param name="ttl">Durée de vie du cache (Time To Live).</param>
    /// <param name="ct">Jeton d'annulation pour stopper l'opération.</param>
    Task SetAsync(
        string idempotencyKey,
        CachedResponse response,
        TimeSpan ttl,
        CancellationToken ct = default);
}
