using FluentAssertions;
using LLMProxy.Application.Common.NullObjects;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LLMProxy.Application.Tests.Common.NullObjects;

/// <summary>
/// Tests unitaires pour NullLoggerFactory.
/// Valide le pattern Null Object pour ILoggerFactory.
/// </summary>
public sealed class NullLoggerFactoryTests
{
    [Fact]
    public void Instance_Should_ReturnSingletonInstance()
    {
        // Arrange & Act
        var instance1 = NullLoggerFactory.Instance;
        var instance2 = NullLoggerFactory.Instance;

        // Assert
        instance1.Should().BeSameAs(instance2, "NullLoggerFactory doit être un singleton");
        instance1.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Category1")]
    [InlineData("Category2")]
    [InlineData("Microsoft.AspNetCore")]
    [InlineData("LLMProxy.Application")]
    [InlineData("")]
    public void CreateLogger_Should_AlwaysReturnNullLoggerInstance(string categoryName)
    {
        // Arrange
        var factory = NullLoggerFactory.Instance;

        // Act
        var logger = factory.CreateLogger(categoryName);

        // Assert
        logger.Should().BeSameAs(NullLogger.Instance, 
            "CreateLogger doit toujours retourner la même instance de NullLogger");
    }

    [Fact]
    public void CreateLogger_WithMultipleCalls_Should_ReturnSameInstance()
    {
        // Arrange
        var factory = NullLoggerFactory.Instance;

        // Act
        var logger1 = factory.CreateLogger("Category1");
        var logger2 = factory.CreateLogger("Category2");
        var logger3 = factory.CreateLogger("Category1"); // Même catégorie que logger1

        // Assert
        logger1.Should().BeSameAs(logger2, "Tous les loggers doivent être la même instance");
        logger1.Should().BeSameAs(logger3, "Tous les loggers doivent être la même instance");
    }

    [Fact]
    public void AddProvider_Should_DoNothing_WithoutException()
    {
        // Arrange
        var factory = NullLoggerFactory.Instance;
        var provider = new MockLoggerProvider();

        // Act
        Action action = () => factory.AddProvider(provider);

        // Assert
        action.Should().NotThrow("AddProvider ne doit jamais lever d'exception");
    }

    [Fact]
    public void AddProvider_WithNull_Should_DoNothing_WithoutException()
    {
        // Arrange
        var factory = NullLoggerFactory.Instance;

        // Act
        Action action = () => factory.AddProvider(null!);

        // Assert
        action.Should().NotThrow("AddProvider accepte null sans erreur");
    }

    [Fact]
    public void Dispose_Should_DoNothing_WithoutException()
    {
        // Arrange
        var factory = NullLoggerFactory.Instance;

        // Act
        Action action = () => factory.Dispose();

        // Assert
        action.Should().NotThrow("Dispose ne doit jamais lever d'exception");
    }

    [Fact]
    public void Dispose_MultipleTimes_Should_BeIdempotent()
    {
        // Arrange
        var factory = NullLoggerFactory.Instance;

        // Act
        Action action = () =>
        {
            factory.Dispose();
            factory.Dispose();
            factory.Dispose();
        };

        // Assert
        action.Should().NotThrow("Dispose peut être appelé plusieurs fois sans erreur");
    }

    [Fact]
    public void CreateLogger_AfterDispose_Should_StillWork()
    {
        // Arrange
        var factory = NullLoggerFactory.Instance;
        factory.Dispose();

        // Act
        var logger = factory.CreateLogger("TestCategory");

        // Assert
        logger.Should().BeSameAs(NullLogger.Instance, 
            "CreateLogger fonctionne même après Dispose (comportement neutre)");
    }

    [Fact]
    public void UsingStatement_Should_WorkCorrectly()
    {
        // Arrange
        ILogger? createdLogger = null;

        // Act
        Action action = () =>
        {
            using (var factory = NullLoggerFactory.Instance)
            {
                createdLogger = factory.CreateLogger("TestCategory");
            }
        };

        // Assert
        action.Should().NotThrow("using statement avec NullLoggerFactory fonctionne correctement");
        createdLogger.Should().BeSameAs(NullLogger.Instance);
    }

    /// <summary>
    /// Mock de ILoggerProvider pour tester AddProvider.
    /// </summary>
    private sealed class MockLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => NullLogger.Instance;
        public void Dispose() { }
    }
}
