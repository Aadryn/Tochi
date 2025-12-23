using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.Redis.Common;
using StackExchange.Redis;
using System.Text.Json;

namespace LLMProxy.Infrastructure.Redis;

/// <summary>
/// Service de gestion des quotas utilisateurs basé sur Redis.
/// </summary>
/// <remarks>
/// Implémente la vérification, l'incrémentation et la gestion des limites de quotas en temps réel
/// avec stockage distribué Redis pour des performances élevées et expiration automatique des fenêtres.
/// </remarks>
public class QuotaService : IQuotaService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private static readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptionsFactory.CreateDefault();

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="QuotaService"/>.
    /// </summary>
    /// <param name="redis">Le multiplexeur de connexions Redis.</param>
    public QuotaService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    /// <summary>
    /// Vérifie si un quota peut être consommé sans dépasser la limite.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="quotaType">Type de quota à vérifier.</param>
    /// <param name="amount">Montant à consommer (par défaut 1).</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Résultat de vérification indiquant autorisation ou refus avec détails.</returns>
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

    /// <summary>
    /// Incrémente l'utilisation d'un quota de manière atomique.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="quotaType">Type de quota à incrémenter.</param>
    /// <param name="amount">Montant à ajouter (par défaut 1).</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>État du quota après incrémentation.</returns>
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

    /// <summary>
    /// Récupère l'utilisation actuelle d'un quota.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="quotaType">Type de quota à consulter.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>État actuel du quota ou null si non configuré.</returns>
    public async Task<QuotaUsage?> GetUsageAsync(Guid userId, QuotaType quotaType, CancellationToken cancellationToken = default)
    {
        var key = GetQuotaKey(userId, quotaType);
        var limitKey = GetLimitKey(userId, quotaType);

        var usageTask = _db.StringGetAsync(key);
        var limitTask = _db.StringGetAsync(limitKey);

        var results = await Task.WhenAll(usageTask, limitTask);

        var usageValue = results[0];
        var limitValue = results[1];

        var used = usageValue.HasValue ? long.Parse(usageValue!) : 0;
        var limitData = limitValue;

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

    /// <summary>
    /// Récupère l'utilisation actuelle d'un quota ou retourne un quota illimité.
    /// Conforme à ADR-026 (Null Object Pattern).
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="quotaType">Type de quota.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>État actuel du quota ou quota illimité si non configuré.</returns>
    /// <remarks>
    /// Élimine les null checks dans le code appelant.
    /// Retourne un quota illimité (limit = long.MaxValue) si aucun quota configuré.
    /// </remarks>
    public async Task<QuotaUsage> GetUsageOrUnlimitedAsync(Guid userId, QuotaType quotaType, CancellationToken cancellationToken = default)
    {
        var usage = await GetUsageAsync(userId, quotaType, cancellationToken);
        
        if (usage == null)
        {
            // Aucun quota configuré - retourner quota illimité (Null Object Pattern)
            return new QuotaUsage
            {
                UserId = userId,
                QuotaType = quotaType,
                CurrentUsage = 0,
                Limit = long.MaxValue,
                WindowStart = DateTime.UtcNow,
                WindowEnd = DateTime.MaxValue
            };
        }

        return usage;
    }

    /// <summary>
    /// Récupère l'ensemble des quotas d'un utilisateur.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Collection de tous les quotas configurés pour l'utilisateur.</returns>
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

    /// <summary>
    /// Réinitialise les quotas expirés (opération automatique sous Redis).
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    public async Task ResetExpiredQuotasAsync(CancellationToken cancellationToken = default)
    {
        // Redis automatically expires keys based on TTL, so this is mostly a no-op
        // Could scan for expired keys and clean up, but Redis handles this automatically
        await Task.CompletedTask;
    }

    /// <summary>
    /// Synchronise les données de quotas Redis vers PostgreSQL pour persistance.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
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

    /// <summary>
    /// Réinitialise un quota spécifique en supprimant sa clé Redis.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="quotaType">Type de quota à réinitialiser.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    public async Task ResetQuotaAsync(Guid userId, QuotaType quotaType, CancellationToken cancellationToken = default)
    {
        var key = GetQuotaKey(userId, quotaType);
        await _db.KeyDeleteAsync(key);
    }

    /// <summary>
    /// Définit la limite d'un quota utilisateur.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="quotaType">Type de quota à configurer.</param>
    /// <param name="period">Période de la fenêtre de quota.</param>
    /// <param name="maxValue">Valeur maximale autorisée.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
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

    /// <summary>
    /// Tente de consommer un quota de manière atomique (vérification + incrémentation).
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="quotaType">Type de quota à consommer.</param>
    /// <param name="amount">Montant à consommer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>True si consommation réussie, false si quota dépassé.</returns>
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
}
