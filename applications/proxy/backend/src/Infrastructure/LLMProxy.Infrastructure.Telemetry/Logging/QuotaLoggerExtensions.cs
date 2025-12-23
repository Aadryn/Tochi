using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.Telemetry.Logging;

/// <summary>
/// Extensions de logging structuré pour les opérations sur les Quotas (ADR-031).
/// </summary>
public static partial class QuotaLoggerExtensions
{
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Debug,
        Message = "Quota usage recorded for tenant {TenantId}: {TokensUsed:N0} tokens used, {RemainingQuota:N0} remaining")]
    public static partial void QuotaUsageRecorded(
        this ILogger logger,
        Guid tenantId,
        long tokensUsed,
        long remainingQuota);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Warning,
        Message = "Quota limit reached for tenant {TenantId}: {CurrentUsage:N0}/{MonthlyQuota:N0} tokens")]
    public static partial void QuotaLimitReached(
        this ILogger logger,
        Guid tenantId,
        long currentUsage,
        long monthlyQuota);

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Information,
        Message = "Quota reset for tenant {TenantId}, previous usage: {PreviousUsage:N0} tokens")]
    public static partial void QuotaReset(
        this ILogger logger,
        Guid tenantId,
        long previousUsage);

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Warning,
        Message = "Quota threshold warning for tenant {TenantId}: {UsagePercentage:F1}% used ({CurrentUsage:N0}/{MonthlyQuota:N0})")]
    public static partial void QuotaThresholdWarning(
        this ILogger logger,
        Guid tenantId,
        double usagePercentage,
        long currentUsage,
        long monthlyQuota);

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Information,
        Message = "Quota limit increased for tenant {TenantId}: {OldLimit:N0} → {NewLimit:N0} tokens")]
    public static partial void QuotaLimitIncreased(
        this ILogger logger,
        Guid tenantId,
        long oldLimit,
        long newLimit);

    [LoggerMessage(
        EventId = 4006,
        Level = LogLevel.Error,
        Message = "Failed to record quota usage for tenant {TenantId}: {ErrorMessage}")]
    public static partial void QuotaRecordingFailed(
        this ILogger logger,
        Exception exception,
        Guid tenantId,
        string errorMessage);
}
