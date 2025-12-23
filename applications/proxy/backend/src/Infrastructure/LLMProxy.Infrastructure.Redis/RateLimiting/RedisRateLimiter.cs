using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LLMProxy.Infrastructure.Redis.RateLimiting;

/// <summary>
/// Implémentation distribuée du rate limiting avec Redis.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Cette implémentation utilise Redis pour stocker les compteurs de rate limiting
/// de manière distribuée, garantissant la cohérence entre toutes les instances de l'application.
/// </para>
/// <para>
/// <strong>Scripts Lua atomiques :</strong> Tous les algorithmes utilisent des scripts Lua
/// exécutés côté Redis pour garantir l'atomicité des opérations (lecture + écriture + expiration).
/// </para>
/// <para>
/// <strong>Algorithmes supportés :</strong>
/// <list type="bullet">
/// <item><description><strong>Sliding Window</strong> : Fenêtre glissante avec Sorted Sets (ZSET)</description></item>
/// <item><description><strong>Token Bucket</strong> : Seau de tokens avec Hash (HSET)</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class RedisRateLimiter : IRateLimiter
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateLimiter> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="RedisRateLimiter"/>.
    /// </summary>
    /// <param name="redis">Multiplexeur de connexion Redis.</param>
    /// <param name="logger">Logger pour diagnostics.</param>
    /// <exception cref="ArgumentNullException">Si un paramètre est null.</exception>
    public RedisRateLimiter(
        IConnectionMultiplexer redis,
        ILogger<RedisRateLimiter> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<RateLimitResult> CheckSlidingWindowAsync(
        string key,
        int limit,
        TimeSpan window,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(window.TotalMilliseconds);

        var db = _redis.GetDatabase();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowStart = now - (long)window.TotalMilliseconds;

        // Script Lua atomique pour Sliding Window avec Sorted Set
        const string script = @"
            local key = KEYS[1]
            local now = tonumber(ARGV[1])
            local window_start = tonumber(ARGV[2])
            local limit = tonumber(ARGV[3])
            local window_ms = tonumber(ARGV[4])
            
            -- Supprimer les entrées expirées (hors de la fenêtre)
            redis.call('ZREMRANGEBYSCORE', key, '-inf', window_start)
            
            -- Compter les requêtes dans la fenêtre glissante
            local count = redis.call('ZCARD', key)
            
            if count < limit then
                -- Ajouter la requête actuelle avec timestamp comme score et membre unique
                redis.call('ZADD', key, now, now .. '-' .. math.random())
                -- Définir expiration pour cleanup automatique
                redis.call('PEXPIRE', key, window_ms)
                return {1, count + 1, limit}
            else
                -- Limite atteinte
                return {0, count, limit}
            end
        ";

        try
        {
            var scriptResult = await db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { key },
                new RedisValue[] { now, windowStart, limit, (long)window.TotalMilliseconds });
            var result = (RedisResult[])(scriptResult!);

            var allowed = (int)result[0]! == 1;
            var current = (int)result[1]!;
            var maximum = (int)result[2]!;

            if (!allowed)
            {
                _logger.LogDebug(
                    "Sliding Window rate limit exceeded for key {Key}. Current: {Current}/{Limit}",
                    key,
                    current,
                    maximum);
            }

            return new RateLimitResult
            {
                IsAllowed = allowed,
                CurrentCount = current,
                Limit = maximum,
                Window = window,
                RetryAfter = allowed ? null : TimeSpan.FromMilliseconds(
                    (long)window.TotalMilliseconds - (now - windowStart))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking sliding window rate limit for key {Key}", key);
            // En cas d'erreur Redis, autoriser la requête pour éviter un deny-of-service
            return new RateLimitResult
            {
                IsAllowed = true,
                CurrentCount = 0,
                Limit = limit,
                Window = window
            };
        }
    }

    /// <inheritdoc />
    public async Task<RateLimitResult> CheckTokenBucketAsync(
        string key,
        int capacity,
        int tokensPerInterval,
        TimeSpan interval,
        int tokensRequired = 1,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokensPerInterval);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(interval.TotalMilliseconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokensRequired);

        var db = _redis.GetDatabase();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Script Lua atomique pour Token Bucket avec Hash
        const string script = @"
            local key = KEYS[1]
            local capacity = tonumber(ARGV[1])
            local tokens_per_interval = tonumber(ARGV[2])
            local interval_ms = tonumber(ARGV[3])
            local tokens_required = tonumber(ARGV[4])
            local now = tonumber(ARGV[5])
            
            -- Récupérer l'état actuel du bucket
            local bucket = redis.call('HMGET', key, 'tokens', 'last_refill')
            local tokens = tonumber(bucket[1]) or capacity
            local last_refill = tonumber(bucket[2]) or now
            
            -- Calculer les tokens à ajouter depuis la dernière recharge
            local elapsed = now - last_refill
            local intervals = math.floor(elapsed / interval_ms)
            
            if intervals > 0 then
                local new_tokens = intervals * tokens_per_interval
                tokens = math.min(capacity, tokens + new_tokens)
                last_refill = last_refill + (intervals * interval_ms)
            end
            
            -- Vérifier si assez de tokens disponibles
            if tokens >= tokens_required then
                tokens = tokens - tokens_required
                redis.call('HMSET', key, 'tokens', tokens, 'last_refill', last_refill)
                redis.call('PEXPIRE', key, interval_ms * 2)
                return {1, tokens, capacity}
            else
                -- Pas assez de tokens
                return {0, tokens, capacity}
            end
        ";

        try
        {
            var scriptResult = await db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { key },
                new RedisValue[] {
                    capacity,
                    tokensPerInterval,
                    (long)interval.TotalMilliseconds,
                    tokensRequired,
                    now
                });
            var result = (RedisResult[])(scriptResult!);

            var allowed = (int)result[0]! == 1;
            var remainingTokens = (int)result[1]!;
            var maxTokens = (int)result[2]!;

            if (!allowed)
            {
                _logger.LogDebug(
                    "Token Bucket rate limit exceeded for key {Key}. Remaining tokens: {Remaining}/{Capacity}",
                    key,
                    remainingTokens,
                    maxTokens);
            }

            return new RateLimitResult
            {
                IsAllowed = allowed,
                CurrentCount = maxTokens - remainingTokens,
                Limit = maxTokens,
                Window = interval,
                RemainingTokens = remainingTokens,
                RetryAfter = allowed ? null : interval
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking token bucket rate limit for key {Key}", key);
            // En cas d'erreur Redis, autoriser la requête
            return new RateLimitResult
            {
                IsAllowed = true,
                CurrentCount = 0,
                Limit = capacity,
                Window = interval,
                RemainingTokens = capacity
            };
        }
    }

    /// <inheritdoc />
    public async Task<long> IncrementAsync(
        string key,
        long increment,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            var db = _redis.GetDatabase();
            var newValue = await db.StringIncrementAsync(key, increment);

            _logger.LogDebug("Incremented counter {Key} by {Increment} to {NewValue}", key, increment, newValue);

            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing counter for key {Key}", key);
            return 0;
        }
    }
}
