namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Service de mise en cache des réponses LLM (Port en Architecture Hexagonale)
/// Permet de réduire les coûts et la latence en évitant les requêtes dupliquées
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Récupère une valeur du cache par sa clé
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Stocke une valeur dans le cache avec expiration optionnelle
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Vérifie si une clé existe dans le cache
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Supprime une entrée du cache par sa clé
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Supprime toutes les entrées correspondant au pattern (ex: "user:123:*")
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Génère une clé de cache pour une requête LLM (sémantique ou exacte)
    /// </summary>
    string GenerateCacheKey(string endpoint, string requestBody, bool semantic = false);
}
