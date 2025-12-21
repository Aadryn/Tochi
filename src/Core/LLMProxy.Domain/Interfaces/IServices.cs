using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Service for managing quota usage in real-time (Port)
/// Hybrid implementation: Redis for speed, PostgreSQL for persistence
/// </summary>
public interface IQuotaService
{
    /// <summary>
    /// Check if user has quota available for the request
    /// </summary>
    Task<QuotaCheckResult> CheckQuotaAsync(Guid userId, QuotaType quotaType, long amount = 1, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Increment quota usage for a user
    /// </summary>
    Task<QuotaUsage> IncrementUsageAsync(Guid userId, QuotaType quotaType, long amount = 1, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get current quota usage for a user
    /// </summary>
    Task<QuotaUsage?> GetUsageAsync(Guid userId, QuotaType quotaType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all quota usages for a user
    /// </summary>
    Task<IEnumerable<QuotaUsage>> GetAllUsagesAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reset quota usage for a specific window (called by background job)
    /// </summary>
    Task ResetExpiredQuotasAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sync Redis quota data to PostgreSQL for persistence
    /// </summary>
    Task SyncQuotaToDatabaseAsync(CancellationToken cancellationToken = default);
}

public class QuotaCheckResult
{
    public bool IsAllowed { get; init; }
    public QuotaUsage? Usage { get; init; }
    public string? DenialReason { get; init; }
    
    public static QuotaCheckResult Allow(QuotaUsage usage) => new()
    {
        IsAllowed = true,
        Usage = usage
    };
    
    public static QuotaCheckResult Deny(string reason, QuotaUsage? usage = null) => new()
    {
        IsAllowed = false,
        DenialReason = reason,
        Usage = usage
    };
}

/// <summary>
/// Service for caching LLM responses (Port)
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate cache key for LLM request (semantic or exact)
    /// </summary>
    string GenerateCacheKey(string endpoint, string requestBody, bool semantic = false);
}

/// <summary>
/// Service for managing secrets across different environments (Port)
/// </summary>
public interface ISecretService
{
    Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
    Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default);
    Task<bool> DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for token counting (Port)
/// </summary>
public interface ITokenCounterService
{
    /// <summary>
    /// Estimate token count for a text using local tokenizer
    /// </summary>
    int EstimateTokens(string text, string model);
    
    /// <summary>
    /// Parse actual token count from LLM response
    /// </summary>
    (long inputTokens, long outputTokens) ParseTokensFromResponse(string responseBody, ProviderType providerType);
}
