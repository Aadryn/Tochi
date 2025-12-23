using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Interface définissant un service de rate limiting.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Cette interface définit les opérations de base pour limiter le nombre de requêtes
/// provenant d'une source donnée (IP, tenant, API key, endpoint).
/// </para>
/// <para>
/// Deux algorithmes principaux sont supportés :
/// <list type="bullet">
/// <item><description><strong>Sliding Window</strong> : Fenêtre glissante qui lisse le trafic et évite les bursts aux limites de fenêtres</description></item>
/// <item><description><strong>Token Bucket</strong> : Seau de tokens régénérés permettant des bursts contrôlés</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IRateLimiter
{
    /// <summary>
    /// Vérifie si une requête peut être autorisée selon l'algorithme Sliding Window.
    /// </summary>
    /// <param name="key">Clé unique identifiant la source (format : "ratelimit:{type}:{id}").</param>
    /// <param name="limit">Nombre maximum de requêtes autorisées dans la fenêtre.</param>
    /// <param name="window">Durée de la fenêtre glissante.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns>Résultat de la vérification avec les informations de limite.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Algorithme Sliding Window :</strong>
    /// </para>
    /// <para>
    /// La fenêtre glisse continuellement dans le temps, évitant les bursts possibles
    /// aux limites de fenêtres fixes. Par exemple, avec une limite de 100 req/min :
    /// </para>
    /// <code>
    /// Time: ─────────────────────────────────────────►
    ///       |←───── 1 minute ─────►|
    ///                |←───── 1 minute ─────►|
    ///                        |←───── 1 minute ─────►|
    /// </code>
    /// <para>
    /// À tout moment, seules les 100 dernières requêtes dans les 60 dernières secondes sont comptées.
    /// </para>
    /// <para>
    /// <strong>Exemple d'utilisation :</strong>
    /// </para>
    /// <code>
    /// var result = await rateLimiter.CheckSlidingWindowAsync(
    ///     key: "ratelimit:ip:192.168.1.1",
    ///     limit: 100,
    ///     window: TimeSpan.FromMinutes(1));
    /// 
    /// if (!result.IsAllowed)
    /// {
    ///     // Requête bloquée - trop de requêtes dans la fenêtre
    ///     return StatusCode(429, "Too Many Requests");
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    Task<RateLimitResult> CheckSlidingWindowAsync(
        string key,
        int limit,
        TimeSpan window,
        CancellationToken ct = default);

    /// <summary>
    /// Vérifie si une requête peut être autorisée selon l'algorithme Token Bucket.
    /// </summary>
    /// <param name="key">Clé unique identifiant la source.</param>
    /// <param name="capacity">Capacité maximale du seau (nombre de tokens max).</param>
    /// <param name="tokensPerInterval">Nombre de tokens ajoutés à chaque intervalle.</param>
    /// <param name="interval">Durée de l'intervalle de régénération.</param>
    /// <param name="tokensRequired">Nombre de tokens nécessaires pour cette requête (défaut : 1).</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns>Résultat de la vérification avec le nombre de tokens restants.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Algorithme Token Bucket :</strong>
    /// </para>
    /// <para>
    /// Un seau contient des tokens qui sont régénérés à intervalle régulier.
    /// Chaque requête consomme un ou plusieurs tokens. Cet algorithme permet
    /// des bursts contrôlés tout en garantissant un débit moyen.
    /// </para>
    /// <code>
    /// ┌─────────────────┐
    /// │   Token Bucket  │
    /// │                 │  ← Tokens ajoutés régulièrement
    /// │  ○ ○ ○ ○ ○     │
    /// │  ○ ○ ○         │  Capacité max : 10
    /// │  ○             │  Rate : 1 token/seconde
    /// └────────┬───────┘
    ///          │
    ///          ▼ 1+ tokens consommés par requête
    /// </code>
    /// <para>
    /// <strong>Exemple d'utilisation :</strong>
    /// </para>
    /// <code>
    /// // Rate limiting basé sur les tokens LLM consommés
    /// var result = await rateLimiter.CheckTokenBucketAsync(
    ///     key: "ratelimit:tokens:tenant-guid",
    ///     capacity: 100_000,        // 100k tokens max en burst
    ///     tokensPerInterval: 10_000, // 10k tokens ajoutés par minute
    ///     interval: TimeSpan.FromMinutes(1),
    ///     tokensRequired: estimatedTokens);  // Tokens estimés pour cette requête
    /// 
    /// if (!result.IsAllowed)
    /// {
    ///     // Pas assez de tokens disponibles
    ///     return StatusCode(429, $"Token quota exceeded. Retry after {result.RetryAfter}");
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    Task<RateLimitResult> CheckTokenBucketAsync(
        string key,
        int capacity,
        int tokensPerInterval,
        TimeSpan interval,
        int tokensRequired = 1,
        CancellationToken ct = default);

    /// <summary>
    /// Incrémente un compteur atomiquement (utilisé pour les statistiques d'usage).
    /// </summary>
    /// <param name="key">Clé du compteur.</param>
    /// <param name="increment">Valeur à ajouter au compteur.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns>Nouvelle valeur du compteur après incrémentation.</returns>
    /// <remarks>
    /// <para>
    /// Utilisé principalement pour enregistrer les tokens réellement consommés
    /// après une requête LLM, pour la facturation et le monitoring.
    /// </para>
    /// <para>
    /// <strong>Exemple d'utilisation :</strong>
    /// </para>
    /// <code>
    /// // Après complétion d'une requête LLM
    /// var key = $"usage:tokens:{tenantId}:{DateTime.UtcNow:yyyy-MM}";
    /// await rateLimiter.IncrementAsync(key, actualTokensConsumed);
    /// </code>
    /// </para>
    /// </remarks>
    Task<long> IncrementAsync(
        string key,
        long increment,
        CancellationToken ct = default);
}
