using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Represents an audit log entry for compliance and monitoring
/// </summary>
public class AuditLog : Entity
{
    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? ApiKeyId { get; private set; }
    public Guid? ProviderId { get; private set; }
    
    // Request information
    public string RequestId { get; private set; }
    public string Endpoint { get; private set; }
    public string HttpMethod { get; private set; }
    public string? RequestPath { get; private set; }
    public int StatusCode { get; private set; }
    
    // Content (may be anonymized based on tenant settings)
    public string? RequestBody { get; private set; }
    public string? ResponseBody { get; private set; }
    public bool IsAnonymized { get; private set; }
    
    // Metrics
    public long RequestTokens { get; private set; }
    public long ResponseTokens { get; private set; }
    public long TotalTokens { get; private set; }
    public int DurationMs { get; private set; }
    
    // Metadata
    public string? ClientIp { get; private set; }
    public string? UserAgent { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; }
    
    // Error information
    public bool IsError { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorStackTrace { get; private set; }

    private AuditLog() 
    { 
        RequestId = string.Empty;
        Endpoint = string.Empty;
        HttpMethod = string.Empty;
        Metadata = new(); 
    }

    private AuditLog(
        Guid tenantId,
        Guid? userId,
        Guid? apiKeyId,
        Guid? providerId,
        string requestId,
        string endpoint,
        string httpMethod,
        string? requestPath,
        int statusCode,
        string? requestBody,
        string? responseBody,
        bool isAnonymized,
        long requestTokens,
        long responseTokens,
        int durationMs,
        string? clientIp,
        string? userAgent)
    {
        TenantId = tenantId;
        UserId = userId;
        ApiKeyId = apiKeyId;
        ProviderId = providerId;
        RequestId = requestId ?? throw new ArgumentNullException(nameof(requestId));
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        HttpMethod = httpMethod ?? throw new ArgumentNullException(nameof(httpMethod));
        RequestPath = requestPath;
        StatusCode = statusCode;
        RequestBody = requestBody;
        ResponseBody = responseBody;
        IsAnonymized = isAnonymized;
        RequestTokens = requestTokens;
        ResponseTokens = responseTokens;
        TotalTokens = requestTokens + responseTokens;
        DurationMs = durationMs;
        ClientIp = clientIp;
        UserAgent = userAgent;
        Metadata = new Dictionary<string, string>();
        IsError = statusCode >= 400;
    }

    public static Result<AuditLog> Create(
        Guid tenantId,
        Guid? userId,
        Guid? apiKeyId,
        Guid? providerId,
        string requestId,
        string endpoint,
        string httpMethod,
        string? requestPath,
        int statusCode,
        string? requestBody,
        string? responseBody,
        bool isAnonymized,
        long requestTokens,
        long responseTokens,
        int durationMs,
        string? clientIp,
        string? userAgent)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<AuditLog>("Invalid tenant ID.");

        if (string.IsNullOrWhiteSpace(requestId))
            return Result.Failure<AuditLog>("Request ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(endpoint))
            return Result.Failure<AuditLog>("Endpoint cannot be empty.");

        if (string.IsNullOrWhiteSpace(httpMethod))
            return Result.Failure<AuditLog>("HTTP method cannot be empty.");

        var auditLog = new AuditLog(
            tenantId, userId, apiKeyId, providerId, requestId, endpoint, httpMethod,
            requestPath, statusCode, requestBody, responseBody, isAnonymized,
            requestTokens, responseTokens, durationMs, clientIp, userAgent);
        
        return Result.Success(auditLog);
    }

    public void SetError(string errorMessage, string? stackTrace = null)
    {
        IsError = true;
        ErrorMessage = errorMessage;
        ErrorStackTrace = stackTrace;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMetadata(string key, string value)
    {
        Metadata[key] = value;
    }
}

/// <summary>
/// Represents token usage metrics aggregated by time period
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
        if (tenantId == Guid.Empty)
            return Result.Failure<TokenUsageMetric>("Invalid tenant ID.");

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
        
        UpdatedAt = DateTime.UtcNow;
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

public enum MetricPeriod
{
    Minute = 0,
    Hour = 1,
    Day = 2,
    Month = 3
}
