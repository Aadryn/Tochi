using StackExchange.Redis;
using System.Text.Json;

namespace LLMProxy.Infrastructure.Redis.Idempotency;

/// <summary>
/// Implémentation Redis du store d'idempotence.
/// Stocke les réponses HTTP dans Redis avec un TTL pour permettre le rejeu des requêtes idempotentes.
/// Conforme à ADR-022 (Idempotence).
/// </summary>
public sealed class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly IDatabase _redis;
    private const string KeyPrefix = "idempotency:";

    /// <summary>
    /// Initialise une nouvelle instance du store d'idempotence Redis.
    /// </summary>
    /// <param name="redis">Connexion multiplexeur Redis (injectée par DI).</param>
    public RedisIdempotencyStore(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    /// <inheritdoc />
    public async Task<CachedResponse?> GetAsync(string idempotencyKey, CancellationToken ct = default)
    {
        var key = GetRedisKey(idempotencyKey);
        var value = await _redis.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<CachedResponse>(value!);
    }

    /// <inheritdoc />
    public async Task SetAsync(
        string idempotencyKey,
        CachedResponse response,
        TimeSpan ttl,
        CancellationToken ct = default)
    {
        var key = GetRedisKey(idempotencyKey);
        var value = JsonSerializer.Serialize(response);

        await _redis.StringSetAsync(key, value, ttl);
    }

    /// <summary>
    /// Génère la clé Redis complète en ajoutant le préfixe standard.
    /// Permet d'isoler les clés d'idempotence des autres données Redis.
    /// </summary>
    /// <param name="idempotencyKey">Clé d'idempotence brute fournie par le client.</param>
    /// <returns>Clé Redis avec préfixe (ex: "idempotency:550e8400-...").</returns>
    private static string GetRedisKey(string idempotencyKey)
        => $"{KeyPrefix}{idempotencyKey}";
}
