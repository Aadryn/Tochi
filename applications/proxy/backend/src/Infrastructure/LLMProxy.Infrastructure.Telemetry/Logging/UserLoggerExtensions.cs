using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.Telemetry.Logging;

/// <summary>
/// Extensions de logging structuré pour les opérations sur les Users (ADR-031).
/// </summary>
public static partial class UserLoggerExtensions
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "User {UserId} created with email '{Email}' for tenant {TenantId}")]
    public static partial void UserCreated(
        this ILogger logger,
        Guid userId,
        string email,
        Guid tenantId);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "User {UserId} updated: {ChangedFields}")]
    public static partial void UserUpdated(
        this ILogger logger,
        Guid userId,
        string changedFields);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Warning,
        Message = "User {UserId} '{Email}' deleted from tenant {TenantId}")]
    public static partial void UserDeleted(
        this ILogger logger,
        Guid userId,
        string email,
        Guid tenantId);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Information,
        Message = "User {UserId} permission changed: {Permission} = {IsGranted}")]
    public static partial void UserPermissionChanged(
        this ILogger logger,
        Guid userId,
        string permission,
        bool isGranted);

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Error,
        Message = "Failed to create user '{Email}' for tenant {TenantId}: {ErrorMessage}")]
    public static partial void UserCreationFailed(
        this ILogger logger,
        Exception exception,
        string email,
        Guid tenantId,
        string errorMessage);
}
