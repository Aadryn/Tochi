namespace LLMProxy.Domain.Common;

/// <summary>
/// Résultat d'une vérification de rate limit.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Ce record immuable encapsule toutes les informations nécessaires pour
/// décider si une requête doit être autorisée ou rejetée, ainsi que les
/// métadonnées pour les headers de réponse HTTP (X-RateLimit-*, Retry-After).
/// </para>
/// <para>
/// <strong>Exemple d'utilisation :</strong>
/// </para>
/// <code>
/// var result = await rateLimiter.CheckSlidingWindowAsync("key", 100, TimeSpan.FromMinutes(1));
/// 
/// if (!result.IsAllowed)
/// {
///     // Ajouter headers HTTP standard
///     context.Response.StatusCode = 429;
///     context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
///     context.Response.Headers["X-RateLimit-Remaining"] = "0";
///     context.Response.Headers["Retry-After"] = ((int)result.RetryAfter.Value.TotalSeconds).ToString();
///     
///     return new { error = "Rate limit exceeded" };
/// }
/// </code>
/// </para>
/// </remarks>
public sealed record RateLimitResult
{
    /// <summary>
    /// Indique si la requête est autorisée selon les limites configurées.
    /// </summary>
    /// <value>
    /// <c>true</c> si la requête peut être traitée; <c>false</c> si elle doit être rejetée avec HTTP 429.
    /// </value>
    public required bool IsAllowed { get; init; }

    /// <summary>
    /// Nombre actuel de requêtes ou tokens consommés dans la fenêtre courante.
    /// </summary>
    /// <value>
    /// Valeur entre 0 et <see cref="Limit"/>. Utilisé pour calculer <c>X-RateLimit-Remaining</c>.
    /// </value>
    public required int CurrentCount { get; init; }

    /// <summary>
    /// Limite maximale configurée pour cette fenêtre.
    /// </summary>
    /// <value>
    /// Nombre maximum de requêtes autorisées dans <see cref="Window"/>. Correspond au header <c>X-RateLimit-Limit</c>.
    /// </value>
    public required int Limit { get; init; }

    /// <summary>
    /// Durée de la fenêtre de rate limiting.
    /// </summary>
    /// <value>
    /// Pour Sliding Window : durée de la fenêtre glissante.
    /// Pour Token Bucket : intervalle de régénération des tokens.
    /// </value>
    public required TimeSpan Window { get; init; }

    /// <summary>
    /// Nombre de tokens restants dans le bucket (algorithme Token Bucket uniquement).
    /// </summary>
    /// <value>
    /// <c>null</c> pour Sliding Window. Pour Token Bucket : nombre de tokens disponibles (≤ capacity).
    /// </value>
    /// <remarks>
    /// <para>
    /// Permet de calculer combien de requêtes supplémentaires peuvent être effectuées
    /// immédiatement sans attendre la régénération.
    /// </para>
    /// </remarks>
    public int? RemainingTokens { get; init; }

    /// <summary>
    /// Durée suggérée avant de réessayer après un rejet (HTTP 429).
    /// </summary>
    /// <value>
    /// <c>null</c> si <see cref="IsAllowed"/> est <c>true</c>. Sinon, durée minimale avant que la requête puisse réussir.
    /// </value>
    /// <remarks>
    /// <para>
    /// Utilisé pour définir le header HTTP <c>Retry-After</c> en secondes.
    /// </para>
    /// <para>
    /// <strong>Exemple :</strong>
    /// </para>
    /// <code>
    /// if (!result.IsAllowed &amp;&amp; result.RetryAfter.HasValue)
    /// {
    ///     var seconds = (int)result.RetryAfter.Value.TotalSeconds;
    ///     context.Response.Headers["Retry-After"] = seconds.ToString();
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public TimeSpan? RetryAfter { get; init; }
}
