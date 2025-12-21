using LLMProxy.Domain.Common;
using System.Security.Cryptography;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Represents an API key for authentication
/// </summary>
public class ApiKey : Entity
{
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string KeyHash { get; private set; } // Store hash, never plain text
    public string KeyPrefix { get; private set; } // First 8 chars for identification (e.g., "sk_live_abcd1234")
    public bool IsActive { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    
    // Navigation
    public User User { get; private set; } = null!;
    public Tenant Tenant { get; private set; } = null!;

    private ApiKey() 
    {
        Name = string.Empty;
        KeyHash = string.Empty;
        KeyPrefix = string.Empty;
    } // EF Core

    private ApiKey(Guid userId, Guid tenantId, string name, string keyHash, string keyPrefix, DateTime? expiresAt)
    {
        UserId = userId;
        TenantId = tenantId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        KeyHash = keyHash ?? throw new ArgumentNullException(nameof(keyHash));
        KeyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
        ExpiresAt = expiresAt;
        IsActive = true;
    }

    public static Result<ApiKey> Create(Guid userId, Guid tenantId, string name, DateTime? expiresAt = null)
    {
        if (userId == Guid.Empty)
            return Result.Failure<ApiKey>("Invalid user ID.");

        if (tenantId == Guid.Empty)
            return Result.Failure<ApiKey>("Invalid tenant ID.");

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ApiKey>("API key name cannot be empty.");

        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
            return Result.Failure<ApiKey>("Expiration date must be in the future.");

        // Generate a secure random key
        var rawKey = GenerateSecureKey();
        var keyHash = HashKey(rawKey);
        var keyPrefix = rawKey.Substring(0, Math.Min(16, rawKey.Length));

        var apiKey = new ApiKey(userId, tenantId, name, keyHash, keyPrefix, expiresAt);
        
        // Store the raw key in a domain event so it can be returned once (never stored)
        apiKey.AddDomainEvent(new ApiKeyCreatedEvent(apiKey.Id, rawKey));
        
        return Result.Success(apiKey);
    }

    // Overload for when a plain key is provided (for command handlers)
    public static ApiKey Create(Guid userId, Guid tenantId, string name, string plainKey, DateTime? expiresAt = null)
    {
        var keyHash = HashKey(plainKey);
        var keyPrefix = plainKey.Substring(0, Math.Min(16, plainKey.Length));

        var apiKey = new ApiKey(userId, tenantId, name, keyHash, keyPrefix, expiresAt);
        
        // Store the raw key in a domain event so it can be returned once (never stored)
        apiKey.AddDomainEvent(new ApiKeyCreatedEvent(apiKey.Id, plainKey));
        
        return apiKey;
    }

    public static string GenerateKey()
    {
        return GenerateSecureKey();
    }

    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    public bool IsRevoked() => RevokedAt.HasValue;

    public bool IsValid() => IsActive && !IsExpired() && !IsRevoked();

    public Result Revoke()
    {
        if (IsRevoked())
            return Result.Failure("API key is already revoked.");

        IsActive = false;
        RevokedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success();
    }

    public void UpdateLastUsed()
    {
        LastUsedAt = DateTime.UtcNow;
    }

    public bool VerifyKey(string rawKey)
    {
        if (string.IsNullOrWhiteSpace(rawKey))
            return false;

        var hash = HashKey(rawKey);
        return KeyHash == hash;
    }

    private static string GenerateSecureKey()
    {
        const string prefix = "llm_";
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return prefix + Convert.ToBase64String(randomBytes).Replace("+", "").Replace("/", "").Replace("=", "");
    }

    private static string HashKey(string rawKey)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToBase64String(hashBytes);
    }
}

public record ApiKeyCreatedEvent(Guid ApiKeyId, string RawKey) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
