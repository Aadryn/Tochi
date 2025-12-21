using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Entities;

/// <summary>
/// Représente une entrée de journal d'audit pour la conformité et le monitoring
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
