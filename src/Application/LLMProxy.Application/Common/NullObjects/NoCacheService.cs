using LLMProxy.Domain.Interfaces;

namespace LLMProxy.Application.Common.NullObjects;

/// <summary>
/// Null Object pour ICacheService - Service de cache désactivé.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-026 Null Object Pattern.
/// </para>
/// <para>
/// Utilisé lorsque le cache est désactivé (ex: environnement de développement, tests).
/// Toutes les opérations retournent des valeurs neutres sans erreur :
/// - GetAsync retourne null (cache miss)
/// - SetAsync, RemoveAsync, etc. ne font rien
/// - ExistsAsync retourne false
/// </para>
/// <para>
/// Évite les vérifications null partout dans le code applicatif.
/// </para>
/// </remarks>
public sealed class NoCacheService : ICacheService
{
    /// <summary>
    /// Instance singleton du service de cache désactivé.
    /// </summary>
    public static readonly NoCacheService Instance = new();

    private NoCacheService() { }

    /// <summary>
    /// Retourne toujours null (comportement neutre - cache miss).
    /// </summary>
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) 
        where T : class
    {
        return Task.FromResult<T?>(null);
    }

    /// <summary>
    /// Ne fait rien (comportement neutre - cache désactivé).
    /// </summary>
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) 
        where T : class
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retourne toujours false (comportement neutre - aucune clé n'existe).
    /// </summary>
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    /// <summary>
    /// Ne fait rien (comportement neutre - rien à supprimer).
    /// </summary>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ne fait rien (comportement neutre - aucun pattern à supprimer).
    /// </summary>
    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Génère une clé de cache standard (pour compatibilité API).
    /// </summary>
    /// <remarks>
    /// Même si le cache est désactivé, cette méthode retourne une clé valide
    /// pour maintenir la cohérence de l'interface.
    /// </remarks>
    public string GenerateCacheKey(string endpoint, string requestBody, bool semantic = false)
    {
        var prefix = semantic ? "semantic" : "exact";
        var hash = ComputeSimpleHash(requestBody);
        return $"{prefix}:{endpoint}:{hash}";
    }

    private static string ComputeSimpleHash(string input)
    {
        // Hash simple pour la clé (cache désactivé donc pas critique)
        return input.GetHashCode().ToString("X8");
    }
}
