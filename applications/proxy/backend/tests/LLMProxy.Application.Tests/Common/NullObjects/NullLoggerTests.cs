using FluentAssertions;
using LLMProxy.Application.Common.NullObjects;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LLMProxy.Application.Tests.Common.NullObjects;

/// <summary>
/// Tests unitaires pour NullLogger.
/// Valide le pattern Null Object pour ILogger.
/// </summary>
public sealed class NullLoggerTests
{
    [Fact]
    public void Instance_Should_ReturnSingletonInstance()
    {
        // Arrange & Act
        var instance1 = NullLogger.Instance;
        var instance2 = NullLogger.Instance;

        // Assert
        instance1.Should().BeSameAs(instance2, "NullLogger doit être un singleton");
        instance1.Should().NotBeNull();
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    [InlineData(LogLevel.None)]
    public void IsEnabled_Should_AlwaysReturnTrue(LogLevel logLevel)
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act
        var isEnabled = logger.IsEnabled(logLevel);

        // Assert
        isEnabled.Should().BeTrue("NullLogger accepte tous les niveaux de log");
    }

    [Fact]
    public void Log_Should_DoNothing_WithoutException()
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act
        Action action = () => logger.LogInformation("Test message");

        // Assert
        action.Should().NotThrow("NullLogger ne doit jamais lever d'exception");
    }

    [Fact]
    public void Log_WithException_Should_DoNothing_WithoutException()
    {
        // Arrange
        var logger = NullLogger.Instance;
        var exception = new InvalidOperationException("Test exception");

        // Act
        Action action = () => logger.LogError(exception, "Test error");

        // Assert
        action.Should().NotThrow("NullLogger ne doit jamais lever d'exception");
    }

    [Fact]
    public void BeginScope_Should_ReturnNullScope()
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act
        using var scope = logger.BeginScope("Test scope");

        // Assert
        scope.Should().NotBeNull("BeginScope retourne un NullScope valide");
    }

    [Fact]
    public void BeginScope_Dispose_Should_DoNothing_WithoutException()
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act
        Action action = () =>
        {
            using var scope = logger.BeginScope("Test scope");
        };

        // Assert
        action.Should().NotThrow("NullScope.Dispose() ne doit jamais lever d'exception");
    }

    [Fact]
    public void BeginScope_Should_ReturnSameSingletonInstance()
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act
        using var scope1 = logger.BeginScope("Scope 1");
        using var scope2 = logger.BeginScope("Scope 2");

        // Assert
        scope1.Should().BeSameAs(scope2, "NullScope doit être un singleton");
    }

    [Fact]
    public void MultipleLogCalls_Should_WorkWithoutException()
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act
        Action action = () =>
        {
            logger.LogTrace("Trace message");
            logger.LogDebug("Debug message");
            logger.LogInformation("Info message");
            logger.LogWarning("Warning message");
            logger.LogError("Error message");
            logger.LogCritical("Critical message");
        };

        // Assert
        action.Should().NotThrow("NullLogger supporte des appels multiples sans erreur");
    }

    [Fact]
    public void Log_WithComplexState_Should_DoNothing_WithoutException()
    {
        // Arrange
        var logger = NullLogger.Instance;
        var state = new Dictionary<string, object>
        {
            ["UserId"] = Guid.NewGuid(),
            ["Timestamp"] = DateTime.UtcNow,
            ["Data"] = new { Name = "Test", Count = 42 }
        };

        // Act
        Action action = () => logger.Log(
            LogLevel.Information,
            new EventId(100, "TestEvent"),
            state,
            null,
            (s, e) => "Test formatter");

        // Assert
        action.Should().NotThrow("NullLogger accepte tout type de state sans erreur");
    }
}
