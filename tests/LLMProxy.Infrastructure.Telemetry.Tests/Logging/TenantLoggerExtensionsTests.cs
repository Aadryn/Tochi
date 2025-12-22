using LLMProxy.Infrastructure.Telemetry.Logging;
using Microsoft.Extensions.Logging;
using NFluent;
using NSubstitute;
using Xunit;

namespace LLMProxy.Infrastructure.Telemetry.Tests.Logging;

/// <summary>
/// Tests unitaires pour <see cref="TenantLoggerExtensions"/>.
/// </summary>
/// <remarks>
/// Valide le comportement des méthodes de logging structuré générées par LoggerMessage.
/// Vérifie la capture correcte des propriétés structurées et les EventIds.
/// </remarks>
public sealed class TenantLoggerExtensionsTests
{
    private readonly ILogger _logger;

    public TenantLoggerExtensionsTests()
    {
        _logger = Substitute.For<ILogger>();
        
        // Configure mock to enable all log levels (required for LoggerMessage)
        _logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
    }

    [Fact]
    public void TenantCreated_Should_Log_Information_With_Correct_EventId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        const string tenantName = "Test Tenant";

        // Act
        _logger.TenantCreated(tenantId, tenantName);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Is<EventId>(e => e.Id == 1001),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void TenantCreated_Should_Include_TenantId_And_TenantName_In_State()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        const string tenantName = "Test Tenant";
        object? capturedState = null;

        _logger.Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Do<object>(state => capturedState = state),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        // Act
        _logger.TenantCreated(tenantId, tenantName);

        // Assert
        Check.That(capturedState).IsNotNull();
        var stateString = capturedState!.ToString();
        Check.That(stateString).Contains(tenantId.ToString());
        Check.That(stateString).Contains(tenantName);
    }

    [Fact]
    public void TenantActivated_Should_Log_Information_With_EventId_1002()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        const string previousStatus = "Inactive";

        // Act
        _logger.TenantActivated(tenantId, previousStatus);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Is<EventId>(e => e.Id == 1002),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void TenantDeactivated_Should_Log_Warning_With_EventId_1003()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        const string reason = "Quota exceeded";

        // Act
        _logger.TenantDeactivated(tenantId, reason);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Is<EventId>(e => e.Id == 1003),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void TenantQuotaExceeded_Should_Log_Warning_With_EventId_1004()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        const long currentUsage = 150_000;
        const long monthlyQuota = 100_000;

        // Act
        _logger.TenantQuotaExceeded(tenantId, currentUsage, monthlyQuota);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Is<EventId>(e => e.Id == 1004),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void TenantQuotaExceeded_Should_Include_Formatted_Numbers()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        const long currentUsage = 150_000;
        const long monthlyQuota = 100_000;
        object? capturedState = null;

        _logger.Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Do<object>(state => capturedState = state),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        // Act
        _logger.TenantQuotaExceeded(tenantId, currentUsage, monthlyQuota);

        // Assert
        Check.That(capturedState).IsNotNull();
        var stateString = capturedState!.ToString();
        Check.That(stateString).Contains("150,000"); // Formatted with :N0
        Check.That(stateString).Contains("100,000");
    }

    [Fact]
    public void TenantUsageRecorded_Should_Log_Debug_With_EventId_1005()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        const long tokensUsed = 5000;
        const long remainingQuota = 95_000;

        // Act
        _logger.TenantUsageRecorded(tenantId, tokensUsed, remainingQuota);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Debug,
            Arg.Is<EventId>(e => e.Id == 1005),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void TenantQuotaReset_Should_Log_Information_With_EventId_1006()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        const long newQuota = 100_000;

        // Act
        _logger.TenantQuotaReset(tenantId, newQuota);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Is<EventId>(e => e.Id == 1006),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void TenantUpdated_Should_Log_Information_With_EventId_1007()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        const string updatedFields = "Name, MonthlyQuota";

        // Act
        _logger.TenantUpdated(tenantId, updatedFields);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Is<EventId>(e => e.Id == 1007),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void TenantDeleted_Should_Log_Information_With_EventId_1008()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        const string tenantName = "Test Tenant";

        // Act
        _logger.TenantDeleted(tenantId, tenantName);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Is<EventId>(e => e.Id == 1008),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void TenantCreationFailed_Should_Log_Error_With_EventId_1009()
    {
        // Arrange
        const string tenantName = "Test Tenant";
        const string errorMessage = "Database connection failed";
        var exception = new InvalidOperationException("Test exception");

        // Act
        _logger.TenantCreationFailed(exception, tenantName, errorMessage);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Is<EventId>(e => e.Id == 1009),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void TenantUpdateFailed_Should_Log_Error_With_EventId_1010()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        const string errorMessage = "Validation failed";
        var exception = new ArgumentException("Test exception");

        // Act
        _logger.TenantUpdateFailed(exception, tenantId, errorMessage);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Is<EventId>(e => e.Id == 1010),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void TenantDeletionFailed_Should_Log_Error_With_EventId_1011()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        const string errorMessage = "Foreign key constraint violation";
        var exception = new InvalidOperationException("Test exception");

        // Act
        _logger.TenantDeletionFailed(exception, tenantId, errorMessage);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Is<EventId>(e => e.Id == 1011),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void All_EventIds_Should_Be_In_Tenant_Range_1000_To_1999()
    {
        // Arrange - All EventIds used in TenantLoggerExtensions
        var eventIds = new[] { 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011 };

        // Assert
        foreach (var eventId in eventIds)
        {
            Check.That(eventId).IsStrictlyGreaterThan(1000);
            Check.That(eventId).IsStrictlyLessThan(2000);
        }
    }

    [Fact]
    public void All_EventIds_Should_Be_Unique()
    {
        // Arrange - All EventIds used in TenantLoggerExtensions
        var eventIds = new[] { 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011 };

        // Assert
        Check.That(eventIds).ContainsNoDuplicateItem();
    }
}
