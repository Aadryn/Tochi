using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace LLMProxy.Infrastructure.Redis;

public class QuotaService : IQuotaService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public QuotaService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task<QuotaCheckResult> CheckQuotaAsync(Guid userId, QuotaType quotaType, long amount = 1, CancellationToken cancellationToken = default)
    {
        var key = GetQuotaKey(userId, quotaType);
        var limitKey = GetLimitKey(userId, quotaType);

        // Get quota limit from Redis (or load from DB if not cached)
        var limitData = await _db.StringGetAsync(limitKey);
        if (limitData.IsNullOrEmpty)
        {
            return QuotaCheckResult.Deny("Quota limit not configured");
        }

        var limit = JsonSerializer.Deserialize<QuotaLimitCache>(limitData!, _jsonOptions);
        if (limit == null) return QuotaCheckResult.Deny("Failed to deserialize quota limit");

        // Get current usage
        var currentUsage = await _db.StringGetAsync(key);
        var used = currentUsage.HasValue ? long.Parse(currentUsage!) : 0;

        var quotaUsage = new QuotaUsage
        {
            UserId = userId,
            QuotaType = quotaType,
            CurrentUsage = used,
            Limit = limit.MaxValue,
            WindowStart = DateTime.UtcNow,
            WindowEnd = DateTime.UtcNow.Add(CalculateExpiration(limit.Period))
        };

        // Check if quota would be exceeded
        if ((used + amount) > limit.MaxValue)
        {
            return QuotaCheckResult.Deny($"Quota exceeded: {used + amount}/{limit.MaxValue}", quotaUsage);
        }

        return QuotaCheckResult.Allow(quotaUsage);
    }

    public async Task<QuotaUsage> IncrementUsageAsync(Guid userId, QuotaType quotaType, long amount = 1, CancellationToken cancellationToken = default)
    {
        var key = GetQuotaKey(userId, quotaType);
        var limitKey = GetLimitKey(userId, quotaType);

        // Get TTL from limit configuration
        var limitData = await _db.StringGetAsync(limitKey);
        if (limitData.IsNullOrEmpty)
        {
            throw new InvalidOperationException($"Quota limit not configured for user {userId} and type {quotaType}");
        }

        var limit = JsonSerializer.Deserialize<QuotaLimitCache>(limitData!, _jsonOptions);
        if (limit == null)
        {
            throw new InvalidOperationException("Failed to deserialize quota limit");
        }

        // Increment usage atomically
        var newUsage = await _db.StringIncrementAsync(key, amount);

        // Set expiration based on period (if not already set)
        var ttl = await _db.KeyTimeToLiveAsync(key);
        if (!ttl.HasValue)
        {
            var expiration = CalculateExpiration(limit.Period);
            await _db.KeyExpireAsync(key, expiration);
        }

        return new QuotaUsage
        {
            UserId = userId,
            QuotaType = quotaType,
            CurrentUsage = newUsage,
            Limit = limit.MaxValue,
            WindowStart = DateTime.UtcNow,
            WindowEnd = DateTime.UtcNow.Add(CalculateExpiration(limit.Period))
        };
    }

    public async Task<QuotaUsage?> GetUsageAsync(Guid userId, QuotaType quotaType, CancellationToken cancellationToken = default)
    {
        var key = GetQuotaKey(userId, quotaType);
        var limitKey = GetLimitKey(userId, quotaType);

        var usageTask = _db.StringGetAsync(key);
        var limitTask = _db.StringGetAsync(limitKey);

        await Task.WhenAll(usageTask, limitTask);

        var used = usageTask.Result.HasValue ? long.Parse(usageTask.Result!) : 0;
        var limitData = limitTask.Result;

        if (limitData.IsNullOrEmpty)
        {
            return null;
        }

        var limit = JsonSerializer.Deserialize<QuotaLimitCache>(limitData!, _jsonOptions);
        if (limit == null) return null;

        return new QuotaUsage
        {
            UserId = userId,
            QuotaType = quotaType,
            CurrentUsage = used,
            Limit = limit.MaxValue,
            WindowStart = DateTime.UtcNow,
            WindowEnd = DateTime.UtcNow.Add(CalculateExpiration(limit.Period))
        };
    }

    public async Task<IEnumerable<QuotaUsage>> GetAllUsagesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var usages = new List<QuotaUsage>();
        
        foreach (QuotaType quotaType in Enum.GetValues<QuotaType>())
        {
            var usage = await GetUsageAsync(userId, quotaType, cancellationToken);
            if (usage != null)
            {
                usages.Add(usage);
            }
        }
        
        return usages;
    }

    public async Task ResetExpiredQuotasAsync(CancellationToken cancellationToken = default)
    {
        // Redis automatically expires keys based on TTL, so this is mostly a no-op
        // Could scan for expired keys and clean up, but Redis handles this automatically
        await Task.CompletedTask;
    }

    public async Task SyncQuotaToDatabaseAsync(CancellationToken cancellationToken = default)
    {
        // Sync all quota data from Redis to PostgreSQL for persistence
        // This ensures quota data survives Redis restarts
        try
        {
            // Note: Full implementation would require:
            // 1. IUnitOfWork/IQuotaRepository injection
            // 2. Scan all quota keys in Redis
            // 3. For each quota, create/update Quota entity in database
            // 4. Batch save to optimize performance
            
            // Example pattern:
            // var server = _redis.GetServer(_redis.GetEndPoints().First());
            // var keys = server.Keys(pattern: "quota:*");
            // foreach (var key in keys)
            // {
            //     // Parse userId and quotaType from key
            //     // Get usage from Redis
            //     // Save to database via unitOfWork.Quotas
            // }
            
            await Task.CompletedTask;
        }
        catch (Exception)
        {
            // Failed to sync - continue without throwing
            // Production implementation should log this error
            await Task.CompletedTask;
        }
    }

    public async Task ResetQuotaAsync(Guid userId, QuotaType quotaType, CancellationToken cancellationToken = default)
    {
        var key = GetQuotaKey(userId, quotaType);
        await _db.KeyDeleteAsync(key);
    }

    public async Task SetQuotaLimitAsync(Guid userId, QuotaType quotaType, QuotaPeriod period, long maxValue, CancellationToken cancellationToken = default)
    {
        var limitKey = GetLimitKey(userId, quotaType);
        
        var limitCache = new QuotaLimitCache
        {
            UserId = userId,
            QuotaType = quotaType,
            Period = period,
            MaxValue = maxValue
        };

        var json = JsonSerializer.Serialize(limitCache, _jsonOptions);
        await _db.StringSetAsync(limitKey, json, TimeSpan.FromDays(30)); // Cache for 30 days
    }

    public async Task<bool> TryConsumeQuotaAsync(Guid userId, QuotaType quotaType, long amount, CancellationToken cancellationToken = default)
    {
        // Use Lua script for atomic check-and-increment
        var script = @"
            local key = KEYS[1]
            local limitKey = KEYS[2]
            local amount = tonumber(ARGV[1])
            
            local limitData = redis.call('GET', limitKey)
            if not limitData then
                return -1
            end
            
            local current = tonumber(redis.call('GET', key) or '0')
            local limit = tonumber(cjson.decode(limitData).maxValue)
            
            if (current + amount) <= limit then
                return redis.call('INCRBY', key, amount)
            else
                return -2
            end
        ";

        var keys = new RedisKey[] { GetQuotaKey(userId, quotaType), GetLimitKey(userId, quotaType) };
        var values = new RedisValue[] { amount };

        var result = await _db.ScriptEvaluateAsync(script, keys, values);
        
        if (result.IsNull || (long)result == -1)
        {
            throw new InvalidOperationException("Quota limit not configured");
        }

        return (long)result != -2;
    }

    private static string GetQuotaKey(Guid userId, QuotaType quotaType)
    {
        var now = DateTime.UtcNow;
        return $"quota:{userId}:{quotaType}:{now:yyyyMMddHH}";
    }

    private static string GetLimitKey(Guid userId, QuotaType quotaType)
    {
        return $"quota_limit:{userId}:{quotaType}";
    }

    private static TimeSpan CalculateExpiration(QuotaPeriod period)
    {
        return period switch
        {
            QuotaPeriod.Minute => TimeSpan.FromMinutes(1),
            QuotaPeriod.Hour => TimeSpan.FromHours(1),
            QuotaPeriod.Day => TimeSpan.FromDays(1),
            QuotaPeriod.Month => TimeSpan.FromDays(30),
            _ => throw new ArgumentException($"Unknown quota period: {period}")
        };
    }

    private class QuotaLimitCache
    {
        public Guid UserId { get; set; }
        public QuotaType QuotaType { get; set; }
        public QuotaPeriod Period { get; set; }
        public long MaxValue { get; set; }
    }
}
