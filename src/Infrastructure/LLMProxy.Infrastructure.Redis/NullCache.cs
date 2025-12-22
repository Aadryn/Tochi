using Microsoft.Extensions.Caching.Distributed;

namespace LLMProxy.Infrastructure.Redis;

/// <summary>
/// Null Object représentant un cache désactivé (no-op).
/// Conforme à ADR-026 (Null Object Pattern).
/// </summary>
/// <remarks>
/// Ce cache fictif ne stocke rien et retourne toujours des cache misses.
/// Utilisé lorsque Redis est indisponible ou en mode dégradé.
/// Pattern Singleton : une seule instance partagée via <see cref="Instance"/>.
/// </remarks>
public sealed class NullCache : IDistributedCache
{
    /// <summary>
    /// Instance singleton du cache désactivé.
    /// </summary>
    /// <remarks>
    /// Utiliser cette instance au lieu de créer de nouvelles instances.
    /// Économise la mémoire et garantit un comportement cohérent.
    /// </remarks>
    public static NullCache Instance { get; } = new NullCache();

    /// <summary>
    /// Constructeur privé pour empêcher l'instanciation externe.
    /// </summary>
    /// <remarks>
    /// Conforme au pattern Singleton : seule l'instance statique <see cref="Instance"/> peut être utilisée.
    /// </remarks>
    private NullCache() { }

    /// <summary>
    /// Retourne toujours null (cache miss).
    /// </summary>
    /// <param name="key">Clé de cache (ignorée).</param>
    /// <returns>Toujours null.</returns>
    public byte[]? Get(string key) => null;

    /// <summary>
    /// Retourne toujours null (cache miss) de manière asynchrone.
    /// </summary>
    /// <param name="key">Clé de cache (ignorée).</param>
    /// <param name="token">Token d'annulation (ignoré).</param>
    /// <returns>Toujours null.</returns>
    public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        return Task.FromResult<byte[]?>(null);
    }

    /// <summary>
    /// Ne stocke rien (no-op).
    /// </summary>
    /// <param name="key">Clé de cache (ignorée).</param>
    /// <param name="value">Valeur à stocker (ignorée).</param>
    /// <param name="options">Options de cache (ignorées).</param>
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        // No-op : ne fait rien
    }

    /// <summary>
    /// Ne stocke rien (no-op) de manière asynchrone.
    /// </summary>
    /// <param name="key">Clé de cache (ignorée).</param>
    /// <param name="value">Valeur à stocker (ignorée).</param>
    /// <param name="options">Options de cache (ignorées).</param>
    /// <param name="token">Token d'annulation (ignoré).</param>
    /// <returns>Tâche complétée immédiatement.</returns>
    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ne rafraîchit rien (no-op).
    /// </summary>
    /// <param name="key">Clé de cache (ignorée).</param>
    public void Refresh(string key)
    {
        // No-op : ne fait rien
    }

    /// <summary>
    /// Ne rafraîchit rien (no-op) de manière asynchrone.
    /// </summary>
    /// <param name="key">Clé de cache (ignorée).</param>
    /// <param name="token">Token d'annulation (ignoré).</param>
    /// <returns>Tâche complétée immédiatement.</returns>
    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ne supprime rien (no-op).
    /// </summary>
    /// <param name="key">Clé de cache (ignorée).</param>
    public void Remove(string key)
    {
        // No-op : ne fait rien
    }

    /// <summary>
    /// Ne supprime rien (no-op) de manière asynchrone.
    /// </summary>
    /// <param name="key">Clé de cache (ignorée).</param>
    /// <param name="token">Token d'annulation (ignoré).</param>
    /// <returns>Tâche complétée immédiatement.</returns>
    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }
}
