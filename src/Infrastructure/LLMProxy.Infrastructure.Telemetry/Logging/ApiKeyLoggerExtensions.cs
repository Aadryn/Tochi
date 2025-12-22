using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.Telemetry.Logging;

/// <summary>
/// Extensions de logging structuré pour les opérations sur les API Keys (ADR-031).
/// </summary>
public static partial class ApiKeyLoggerExtensions
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Information,
        Message = "API Key {ApiKeyId} created for tenant {TenantId} with scope '{Scope}'")]
    public static partial void ApiKeyCreated(
        this ILogger logger,
        Guid apiKeyId,
        Guid tenantId,
        string scope);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Debug,
        Message = "API Key validated successfully for tenant {TenantId}")]
    public static partial void ApiKeyValidated(
        this ILogger logger,
        Guid tenantId);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Warning,
        Message = "API Key {ApiKeyId} revoked for tenant {TenantId}: {Reason}")]
    public static partial void ApiKeyRevoked(
        this ILogger logger,
        Guid apiKeyId,
        Guid tenantId,
        string reason);

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Warning,
        Message = "API Key validation failed for tenant {TenantId}: {Reason}")]
    public static partial void ApiKeyValidationFailed(
        this ILogger logger,
        Guid tenantId,
        string reason);

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Warning,
        Message = "API Key {ApiKeyId} expired for tenant {TenantId} (expired at {ExpirationDate})")]
    public static partial void ApiKeyExpired(
        this ILogger logger,
        Guid apiKeyId,
        Guid tenantId,
        DateTimeOffset expirationDate);

    [LoggerMessage(
        EventId = 3006,
        Level = LogLevel.Error,
        Message = "Failed to create API Key for tenant {TenantId}: {ErrorMessage}")]
    public static partial void ApiKeyCreationFailed(
        this ILogger logger,
        Exception exception,
        Guid tenantId,
        string errorMessage);
}
