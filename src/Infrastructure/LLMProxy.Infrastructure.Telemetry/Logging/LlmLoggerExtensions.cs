using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.Telemetry.Logging;

/// <summary>
/// Extensions de logging structuré pour les opérations LLM (ADR-031).
/// </summary>
public static partial class LlmLoggerExtensions
{
    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Information,
        Message = "LLM request {RequestId} started for tenant {TenantId} using model '{Model}' with provider '{Provider}'")]
    public static partial void LlmRequestStarted(
        this ILogger logger,
        Guid requestId,
        Guid tenantId,
        string model,
        string provider);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Information,
        Message = "LLM request {RequestId} completed in {DurationMs:N0}ms with {TokenCount:N0} tokens (prompt: {PromptTokens:N0}, completion: {CompletionTokens:N0})")]
    public static partial void LlmRequestCompleted(
        this ILogger logger,
        Guid requestId,
        long durationMs,
        long tokenCount,
        long promptTokens,
        long completionTokens);

    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Error,
        Message = "LLM request {RequestId} failed for tenant {TenantId} with provider '{Provider}': {ErrorMessage}")]
    public static partial void LlmRequestFailed(
        this ILogger logger,
        Exception exception,
        Guid requestId,
        Guid tenantId,
        string provider,
        string errorMessage);

    [LoggerMessage(
        EventId = 5004,
        Level = LogLevel.Warning,
        Message = "LLM provider switched from '{PreviousProvider}' to '{NewProvider}' for request {RequestId}: {Reason}")]
    public static partial void LlmProviderSwitched(
        this ILogger logger,
        string previousProvider,
        string newProvider,
        Guid requestId,
        string reason);

    [LoggerMessage(
        EventId = 5005,
        Level = LogLevel.Warning,
        Message = "LLM request {RequestId} rate limited by provider '{Provider}', retrying in {RetryAfterMs:N0}ms")]
    public static partial void LlmRequestRateLimited(
        this ILogger logger,
        Guid requestId,
        string provider,
        long retryAfterMs);

    [LoggerMessage(
        EventId = 5006,
        Level = LogLevel.Warning,
        Message = "LLM request {RequestId} timeout after {TimeoutMs:N0}ms with provider '{Provider}'")]
    public static partial void LlmRequestTimeout(
        this ILogger logger,
        Guid requestId,
        string provider,
        long timeoutMs);

    [LoggerMessage(
        EventId = 5007,
        Level = LogLevel.Debug,
        Message = "LLM request {RequestId} cached response used (cache hit for key '{CacheKey}')")]
    public static partial void LlmRequestCacheHit(
        this ILogger logger,
        Guid requestId,
        string cacheKey);

    [LoggerMessage(
        EventId = 5008,
        Level = LogLevel.Information,
        Message = "LLM streaming response started for request {RequestId} (chunk size: {ChunkSizeBytes:N0} bytes)")]
    public static partial void LlmStreamingStarted(
        this ILogger logger,
        Guid requestId,
        long chunkSizeBytes);
}
