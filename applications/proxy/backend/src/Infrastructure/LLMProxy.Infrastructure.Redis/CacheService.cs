using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.Redis.Common;
using LLMProxy.Infrastructure.Security.Abstractions;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace LLMProxy.Infrastructure.Redis;

/// <summary>
/// Service de cache distribué utilisant Redis pour stocker les réponses LLM.
/// </summary>
/// <remarks>
/// Implémente le cache sémantique et exact pour optimiser les appels répétitifs aux fournisseurs LLM.
/// Supporte également le verrouillage distribué et les opérations atomiques d'incrémentation.
/// </remarks>
public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly IHashService _hashService;
    private static readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptionsFactory.CreateDefault();

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="CacheService"/>.
    /// </summary>
    /// <param name="redis">Multiplexeur de connexion Redis pour gérer les connexions au serveur.</param>
    /// <param name="hashService">Service de hachage cryptographique pour générer les clés de cache.</param>
    /// <exception cref="ArgumentNullException">
    /// Levée si <paramref name="redis"/> ou <paramref name="hashService"/> est <c>null</c>.
    /// </exception>
    public CacheService(IConnectionMultiplexer redis, IHashService hashService)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        _db = redis.GetDatabase();
    }

    /// <summary>
    /// Récupère une valeur du cache Redis de manière asynchrone.
    /// </summary>
    /// <typeparam name="T">Type de l'objet à récupérer. Doit être une classe (contrainte <c>where T : class</c>).</typeparam>
    /// <param name="key">Clé unique identifiant la valeur dans le cache Redis.</param>
    /// <param name="cancellationToken">Jeton d'annulation optionnel pour interrompre l'opération.</param>
    /// <returns>
    /// L'objet désérialisé de type <typeparamref name="T"/> si la clé existe, sinon <c>null</c>.
    /// </returns>
    /// <exception cref="JsonException">
    /// Levée si la désérialisation JSON échoue en raison d'un format invalide.
    /// </exception>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var value = await _db.StringGetAsync(key);
        
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
    }

    /// <summary>
    /// Stocke une valeur dans le cache Redis de manière asynchrone.
    /// </summary>
    /// <typeparam name="T">Type de l'objet à stocker. Doit être une classe (contrainte <c>where T : class</c>).</typeparam>
    /// <param name="key">Clé unique pour identifier la valeur dans le cache.</param>
    /// <param name="value">Objet à sérialiser et stocker dans Redis.</param>
    /// <param name="expiration">
    /// Durée de vie optionnelle de la clé. Si <c>null</c>, la clé n'expire jamais.
    /// </param>
    /// <param name="cancellationToken">Jeton d'annulation optionnel pour interrompre l'opération.</param>
    /// <returns>Tâche représentant l'opération asynchrone de stockage.</returns>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        await _db.StringSetAsync(key, json, expiration);
    }

    /// <summary>
    /// Vérifie si une clé existe dans le cache Redis.
    /// </summary>
    /// <param name="key">Clé à vérifier dans Redis.</param>
    /// <param name="cancellationToken">Jeton d'annulation optionnel pour interrompre l'opération.</param>
    /// <returns>
    /// <c>true</c> si la clé existe dans Redis, sinon <c>false</c>.
    /// </returns>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _db.KeyExistsAsync(key);
    }

    /// <summary>
    /// Supprime une clé du cache Redis de manière asynchrone.
    /// </summary>
    /// <param name="key">Clé à supprimer du cache.</param>
    /// <param name="cancellationToken">Jeton d'annulation optionnel pour interrompre l'opération.</param>
    /// <returns>Tâche représentant l'opération asynchrone de suppression.</returns>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _db.KeyDeleteAsync(key);
    }

    /// <summary>
    /// Supprime toutes les clés correspondant à un motif spécifique dans Redis.
    /// </summary>
    /// <param name="pattern">
    /// Motif de recherche Redis (ex: <c>"llm_cache:*"</c> pour toutes les clés commençant par "llm_cache:").
    /// Utilise la syntaxe de motifs Redis avec caractères génériques (* et ?).
    /// </param>
    /// <param name="cancellationToken">Jeton d'annulation optionnel pour interrompre l'opération.</param>
    /// <returns>Tâche représentant l'opération asynchrone de suppression par motif.</returns>
    /// <remarks>
    /// Cette opération scanne le serveur Redis et peut être coûteuse sur de grandes bases de données.
    /// Utilisez avec précaution en production.
    /// </remarks>
    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var endpoints = _redis.GetEndPoints();
        var server = _redis.GetServer(endpoints.First());

        var keys = server.Keys(pattern: pattern).ToArray();
        
        if (keys.Length > 0)
        {
            await _db.KeyDeleteAsync(keys);
        }
    }

    /// <summary>
    /// Génère une clé de cache unique pour les requêtes LLM.
    /// </summary>
    /// <param name="endpoint">Point de terminaison de l'API LLM (ex: "/v1/chat/completions").</param>
    /// <param name="requestBody">Corps de la requête JSON à hasher.</param>
    /// <param name="semantic">
    /// Si <c>true</c>, génère une clé de cache sémantique (basée uniquement sur le contenu).
    /// Si <c>false</c> (par défaut), génère une clé exacte incluant l'endpoint.
    /// </param>
    /// <returns>
    /// Clé de cache formatée, soit <c>"llm_cache:{endpoint}:{hash}"</c> pour le cache sémantique,
    /// soit <c>"llm_cache_exact:{hash}"</c> pour le cache exact.
    /// </returns>
    public string GenerateCacheKey(string endpoint, string requestBody, bool semantic = false)
    {
        if (semantic)
        {
            // Create a semantic hash of the request body
            var bodyHash = _hashService.ComputeSha256Hash(requestBody);
            return $"llm_cache:{endpoint}:{bodyHash}";
        }
        else
        {
            // Create exact hash for deterministic caching
            var exactHash = _hashService.ComputeSha256Hash($"{endpoint}:{requestBody}");
            return $"llm_cache_exact:{exactHash}";
        }
    }

    // Helper methods for backward compatibility
    private string GenerateCacheKeyInternal(string prefix, params object[] parts)
    {
        var keyBuilder = new StringBuilder(prefix);
        
        foreach (var part in parts)
        {
            keyBuilder.Append(':');
            keyBuilder.Append(part?.ToString() ?? "null");
        }

        return keyBuilder.ToString();
    }

    /// <summary>
    /// Récupère une valeur du cache ou l'initialise en exécutant une fonction factory si absente.
    /// </summary>
    /// <typeparam name="T">Type de l'objet à récupérer ou créer.</typeparam>
    /// <param name="key">Clé du cache Redis.</param>
    /// <param name="factory">Fonction asynchrone à exécuter pour générer la valeur si elle n'existe pas dans le cache.</param>
    /// <param name="expiration">Durée de vie optionnelle de la clé si créée. Si <c>null</c>, pas d'expiration.</param>
    /// <param name="cancellationToken">Jeton d'annulation optionnel.</param>
    /// <returns>
    /// La valeur récupérée du cache ou générée par la fonction factory.
    /// </returns>
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        // Try to get from cache first (use workaround for value types)
        var cached = await GetAsync<object>(key, cancellationToken);
        
        if (cached != null && cached is T typedValue)
        {
            return typedValue;
        }

        // Not in cache, execute factory
        var value = await factory();
        
        if (value != null)
        {
            await SetAsync<object>(key, value, expiration, cancellationToken);
        }

        return value;
    }

    /// <summary>
    /// Incrémente atomiquement une valeur numérique dans Redis.
    /// </summary>
    /// <param name="key">Clé de la valeur à incrémenter.</param>
    /// <param name="value">Valeur d'incrémentation (par défaut <c>1</c>).</param>
    /// <param name="expiration">Durée de vie optionnelle à appliquer si la clé n'a pas déjà d'expiration.</param>
    /// <param name="cancellationToken">Jeton d'annulation optionnel.</param>
    /// <returns>
    /// La nouvelle valeur après incrémentation.
    /// </returns>
    /// <remarks>
    /// Si la clé n'existe pas, elle est créée avec la valeur initiale égale à <paramref name="value"/>.
    /// L'opération est atomique et thread-safe.
    /// </remarks>
    public async Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var newValue = await _db.StringIncrementAsync(key, value);
        
        if (expiration.HasValue)
        {
            var ttl = await _db.KeyTimeToLiveAsync(key);
            if (!ttl.HasValue)
            {
                await _db.KeyExpireAsync(key, expiration);
            }
        }

        return newValue;
    }

    /// <summary>
    /// Décrémente atomiquement une valeur numérique dans Redis.
    /// </summary>
    /// <param name="key">Clé de la valeur à décrémenter.</param>
    /// <param name="value">Valeur de décrémentation (par défaut <c>1</c>).</param>
    /// <param name="cancellationToken">Jeton d'annulation optionnel.</param>
    /// <returns>
    /// La nouvelle valeur après décrémentation.
    /// </returns>
    /// <remarks>
    /// Si la clé n'existe pas, elle est créée avec la valeur initiale <c>0</c> puis décrémentée.
    /// L'opération est atomique et thread-safe.
    /// </remarks>
    public async Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        return await _db.StringDecrementAsync(key, value);
    }

    /// <summary>
    /// Acquiert un verrou distribué dans Redis.
    /// </summary>
    /// <param name="lockKey">Clé unique identifiant le verrou.</param>
    /// <param name="lockValue">Valeur unique (généralement un GUID) pour identifier le propriétaire du verrou.</param>
    /// <param name="expiration">Durée maximale du verrou avant expiration automatique.</param>
    /// <param name="cancellationToken">Jeton d'annulation optionnel.</param>
    /// <returns>
    /// <c>true</c> si le verrou a été acquis avec succès, <c>false</c> si déjà détenu par un autre processus.
    /// </returns>
    /// <remarks>
    /// Utilise l'opération atomique SET NX (SET if Not eXists) de Redis pour garantir l'exclusivité.
    /// Le verrou expire automatiquement après <paramref name="expiration"/> pour éviter les blocages permanents.
    /// </remarks>
    public async Task<bool> AcquireLockAsync(string lockKey, string lockValue, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        return await _db.StringSetAsync(lockKey, lockValue, expiration, When.NotExists);
    }

    /// <summary>
    /// Libère un verrou distribué dans Redis de manière sécurisée.
    /// </summary>
    /// <param name="lockKey">Clé du verrou à libérer.</param>
    /// <param name="lockValue">Valeur unique du verrou pour vérifier la propriété.</param>
    /// <param name="cancellationToken">Jeton d'annulation optionnel.</param>
    /// <returns>
    /// <c>true</c> si le verrou a été libéré avec succès, <c>false</c> si la valeur ne correspond pas
    /// (verrou détenu par un autre processus ou déjà expiré).
    /// </returns>
    /// <remarks>
    /// Utilise un script Lua pour garantir l'atomicité de l'opération vérification-puis-suppression.
    /// Seul le propriétaire légitime du verrou (même <paramref name="lockValue"/>) peut le libérer.
    /// </remarks>
    public async Task<bool> ReleaseLockAsync(string lockKey, string lockValue, CancellationToken cancellationToken = default)
    {
        // Use Lua script to ensure atomic check-and-delete
        var script = @"
            if redis.call('GET', KEYS[1]) == ARGV[1] then
                return redis.call('DEL', KEYS[1])
            else
                return 0
            end
        ";

        var result = await _db.ScriptEvaluateAsync(script, new RedisKey[] { lockKey }, new RedisValue[] { lockValue });
        return (int)result == 1;
    }
}
