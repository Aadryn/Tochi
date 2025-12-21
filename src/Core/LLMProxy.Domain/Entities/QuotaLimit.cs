using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Represents quota limits for users (requests and tokens)
/// </summary>
public class QuotaLimit : Entity
{
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public QuotaType QuotaType { get; private set; }
    public long Limit { get; private set; }
    public QuotaPeriod Period { get; private set; }
    public bool IsEnabled { get; private set; }
    
    // Navigation
    public User User { get; private set; } = null!;

    private QuotaLimit() { } // EF Core

    private QuotaLimit(Guid userId, Guid tenantId, QuotaType quotaType, long limit, QuotaPeriod period)
    {
        UserId = userId;
        TenantId = tenantId;
        QuotaType = quotaType;
        Limit = limit;
        Period = period;
        IsEnabled = true;
    }

    public static Result<QuotaLimit> Create(Guid userId, Guid tenantId, QuotaType quotaType, long limit, QuotaPeriod period)
    {
        if (userId == Guid.Empty)
            return Result.Failure<QuotaLimit>("Invalid user ID.");

        if (tenantId == Guid.Empty)
            return Result.Failure<QuotaLimit>("Invalid tenant ID.");

        if (limit < 0)
            return Result.Failure<QuotaLimit>("Quota limit cannot be negative.");

        var quotaLimit = new QuotaLimit(userId, tenantId, quotaType, limit, period);
        
        return Result.Success(quotaLimit);
    }

    public Result UpdateLimit(long newLimit)
    {
        if (newLimit < 0)
            return Result.Failure("Quota limit cannot be negative.");

        Limit = newLimit;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success();
    }

    public Result Enable()
    {
        if (IsEnabled)
            return Result.Failure("Quota is already enabled.");

        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success();
    }

    public Result Disable()
    {
        if (!IsEnabled)
            return Result.Failure("Quota is already disabled.");

        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success();
    }
}

public enum QuotaType
{
    RequestsPerMinute = 0,
    RequestsPerHour = 1,
    RequestsPerDay = 2,
    TokensPerMinute = 10,
    TokensPerHour = 11,
    TokensPerDay = 12,
    TokensPerMonth = 13
}

public enum QuotaPeriod
{
    Minute = 0,
    Hour = 1,
    Day = 2,
    Month = 3
}

/// <summary>
/// Represents current quota usage (stored in Redis for performance)
/// </summary>
public class QuotaUsage
{
    public Guid UserId { get; set; }
    public QuotaType QuotaType { get; set; }
    public long CurrentUsage { get; set; }
    public long Limit { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }

    public bool IsExceeded => CurrentUsage >= Limit;
    public long Remaining => Math.Max(0, Limit - CurrentUsage);
    public double PercentageUsed => Limit > 0 ? (double)CurrentUsage / Limit * 100 : 0;
}
