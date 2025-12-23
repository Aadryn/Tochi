namespace LLMProxy.Application.Common.Interfaces;

/// <summary>
/// Marker interface pour indiquer qu'une Query peut être mise en cache.
/// Conforme à ADR-042 (Distributed Cache Strategy).
/// </summary>
/// <remarks>
/// Les Queries implémentant cette interface seront automatiquement mises en cache
/// par le <see cref="Behaviors.CachingBehavior{TRequest,TResponse}"/>.
/// 
/// ATTENTION : Ne jamais utiliser sur :
/// - Les Commands (POST/PUT/DELETE)
/// - Les données sensibles (tokens, secrets)
/// - Les données temps réel (statistiques instantanées)
/// </remarks>
/// <typeparam name="TResponse">Type de réponse de la Query</typeparam>
public interface ICachedQuery<out TResponse>
{
    /// <summary>
    /// Durée de vie du cache pour cette requête.
    /// </summary>
    /// <remarks>
    /// Recommandations :
    /// - Données statiques (ex: Providers) : 30-60 minutes
    /// - Données stables (ex: Tenants) : 15-30 minutes
    /// - Données dynamiques (ex: Users) : 1-5 minutes
    /// - Valeur par défaut si null : 5 minutes
    /// </remarks>
    TimeSpan? CacheExpiration => TimeSpan.FromMinutes(5);
}
