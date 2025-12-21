using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Représente les métriques d'utilisation de tokens agrégées par période
/// </summary>
public class TokenUsageMetric : Entity
{
    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? ProviderId { get; private set; }
    
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public MetricPeriod Period { get; private set; }
    
    public long TotalRequests { get; private set; }
    public long SuccessfulRequests { get; private set; }
    public long FailedRequests { get; private set; }
    
    public long TotalInputTokens { get; private set; }
    public long TotalOutputTokens { get; private set; }
    public long TotalTokens { get; private set; }
    
    public long TotalDurationMs { get; private set; }
    public double AverageDurationMs { get; private set; }
    
    public decimal EstimatedCost { get; private set; }

    private TokenUsageMetric() { }

    public static Result<TokenUsageMetric> Create(
        Guid tenantId,
        Guid? userId,
        Guid? providerId,
        DateTime periodStart,
        MetricPeriod period)
    {
        try
        {
            Guard.AgainstEmptyGuid(tenantId, nameof(tenantId), "Invalid tenant ID.");
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<TokenUsageMetric>(ex.Message);
        }

        var metric = new TokenUsageMetric
        {
            TenantId = tenantId,
            UserId = userId,
            ProviderId = providerId,
            PeriodStart = periodStart,
            PeriodEnd = CalculatePeriodEnd(periodStart, period),
            Period = period
        };
        
        return Result.Success(metric);
    }

    public void IncrementUsage(long inputTokens, long outputTokens, int durationMs, bool isSuccess, decimal cost = 0)
    {
        TotalRequests++;
        if (isSuccess)
            SuccessfulRequests++;
        else
            FailedRequests++;

        TotalInputTokens += inputTokens;
        TotalOutputTokens += outputTokens;
        TotalTokens += inputTokens + outputTokens;
        
        TotalDurationMs += durationMs;
        AverageDurationMs = TotalRequests > 0 ? (double)TotalDurationMs / TotalRequests : 0;
        
        EstimatedCost += cost;
        
        MarkAsModified();
    }

    private static DateTime CalculatePeriodEnd(DateTime start, MetricPeriod period)
    {
        return period switch
        {
            MetricPeriod.Minute => start.AddMinutes(1),
            MetricPeriod.Hour => start.AddHours(1),
            MetricPeriod.Day => start.AddDays(1),
            MetricPeriod.Month => start.AddMonths(1),
            _ => throw new ArgumentException("Invalid metric period")
        };
    }
}
