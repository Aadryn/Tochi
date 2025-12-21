using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.Redis.Common;
using LLMProxy.Infrastructure.Security;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace LLMProxy.Infrastructure.Redis;

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly IHashService _hashService;
    private static readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptionsFactory.CreateDefault();

    public CacheService(IConnectionMultiplexer redis, IHashService hashService)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var value = await _db.StringGetAsync(key);
        
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        await _db.StringSetAsync(key, json, expiration);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _db.KeyExistsAsync(key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _db.KeyDeleteAsync(key);
    }

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

    public async Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        return await _db.StringDecrementAsync(key, value);
    }

    public async Task<bool> AcquireLockAsync(string lockKey, string lockValue, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        return await _db.StringSetAsync(lockKey, lockValue, expiration, When.NotExists);
    }

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
