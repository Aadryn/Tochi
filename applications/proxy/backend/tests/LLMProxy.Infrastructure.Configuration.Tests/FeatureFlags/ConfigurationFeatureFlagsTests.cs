using LLMProxy.Domain.Common;
using LLMProxy.Infrastructure.Configuration.FeatureFlags;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.Configuration.Tests.FeatureFlags;

/// <summary>
/// Tests pour <see cref="ConfigurationFeatureFlags"/>.
/// </summary>
public sealed class ConfigurationFeatureFlagsTests : IDisposable
{
    private readonly ILogger<ConfigurationFeatureFlags> _logger;

    public ConfigurationFeatureFlagsTests()
    {
        _logger = Substitute.For<ILogger<ConfigurationFeatureFlags>>();
    }

    public void Dispose()
    {
        // Cleanup si nécessaire
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Check.ThatCode(() => new ConfigurationFeatureFlags(null!, _logger))
            .Throws<ArgumentNullException>()
            .WithProperty("ParamName", "configuration");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string>());

        // Act & Assert
        Check.ThatCode(() => new ConfigurationFeatureFlags(configuration, null!))
            .Throws<ArgumentNullException>()
            .WithProperty("ParamName", "logger");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsEnabled_WithInvalidFeatureName_ThrowsArgumentException(string? invalidName)
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string>());
        var sut = new ConfigurationFeatureFlags(configuration, _logger);

        // Act & Assert
        Check.ThatCode(() => sut.IsEnabled(invalidName!))
            .Throws<ArgumentException>();
    }

    [Fact]
    public void IsEnabled_WhenFeatureNotConfigured_ReturnsFalse()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string>());
        var sut = new ConfigurationFeatureFlags(configuration, _logger);

        // Act
        var result = sut.IsEnabled("non_existent_feature");

        // Assert
        Check.That(result).IsFalse();
    }

    [Fact]
    public void IsEnabled_WhenFeatureEnabledInConfiguration_ReturnsTrue()
    {
        // Arrange
        var settings = new Dictionary<string, string>
        {
            ["FeatureFlags:test_feature"] = "true"
        };
        var configuration = BuildConfiguration(settings);
        var sut = new ConfigurationFeatureFlags(configuration, _logger);

        // Act
        var result = sut.IsEnabled("test_feature");

        // Assert
        Check.That(result).IsTrue();
    }

    [Fact]
    public void IsEnabled_WhenFeatureDisabledInConfiguration_ReturnsFalse()
    {
        // Arrange
        var settings = new Dictionary<string, string>
        {
            ["FeatureFlags:test_feature"] = "false"
        };
        var configuration = BuildConfiguration(settings);
        var sut = new ConfigurationFeatureFlags(configuration, _logger);

        // Act
        var result = sut.IsEnabled("test_feature");

        // Assert
        Check.That(result).IsFalse();
    }

    [Theory]
    [InlineData("True")]
    [InlineData("TRUE")]
    [InlineData("true")]
    public void IsEnabled_WithCaseInsensitiveTrueValue_ReturnsTrue(string trueValue)
    {
        // Arrange
        var settings = new Dictionary<string, string>
        {
            ["FeatureFlags:test_feature"] = trueValue
        };
        var configuration = BuildConfiguration(settings);
        var sut = new ConfigurationFeatureFlags(configuration, _logger);

        // Act
        var result = sut.IsEnabled("test_feature");

        // Assert
        Check.That(result).IsTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("1")]
    [InlineData("yes")]
    public void IsEnabled_WithInvalidBooleanValue_ReturnsFalse(string invalidValue)
    {
        // Arrange
        var settings = new Dictionary<string, string>
        {
            ["FeatureFlags:test_feature"] = invalidValue
        };
        var configuration = BuildConfiguration(settings);
        var sut = new ConfigurationFeatureFlags(configuration, _logger);

        // Act
        var result = sut.IsEnabled("test_feature");

        // Assert
        Check.That(result).IsFalse();
    }

    [Fact]
    public void IsEnabledWithContext_IgnoresContext_ReturnsGlobalValue()
    {
        // Arrange
        var settings = new Dictionary<string, string>
        {
            ["FeatureFlags:tenant_feature"] = "true"
        };
        var configuration = BuildConfiguration(settings);
        var sut = new ConfigurationFeatureFlags(configuration, _logger);

        var context = new FeatureContext
        {
            TenantId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Environment = "Production"
        };

        // Act
        var result = sut.IsEnabled("tenant_feature", context);

        // Assert
        Check.That(result).IsTrue();
    }

    [Fact]
    public async Task IsEnabledAsync_ReturnsSameAsSync()
    {
        // Arrange
        var settings = new Dictionary<string, string>
        {
            ["FeatureFlags:async_feature"] = "true"
        };
        var configuration = BuildConfiguration(settings);
        var sut = new ConfigurationFeatureFlags(configuration, _logger);

        var context = new FeatureContext();

        // Act
        var syncResult = sut.IsEnabled("async_feature", context);
        var asyncResult = await sut.IsEnabledAsync("async_feature", context);

        // Assert
        Check.That(asyncResult).IsEqualTo(syncResult);
    }

    [Fact]
    public async Task IsEnabledAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var settings = new Dictionary<string, string>
        {
            ["FeatureFlags:test"] = "true"
        };
        var configuration = BuildConfiguration(settings);
        var sut = new ConfigurationFeatureFlags(configuration, _logger);

        using var cts = new CancellationTokenSource();

        // Act
        var result = await sut.IsEnabledAsync("test", FeatureContext.Empty, cts.Token);

        // Assert
        Check.That(result).IsTrue();
    }

    [Fact]
    public void IsEnabled_WithMultipleFeatures_ReturnsCorrectValues()
    {
        // Arrange
        var settings = new Dictionary<string, string>
        {
            ["FeatureFlags:feature_a"] = "true",
            ["FeatureFlags:feature_b"] = "false",
            ["FeatureFlags:feature_c"] = "true"
        };
        var configuration = BuildConfiguration(settings);
        var sut = new ConfigurationFeatureFlags(configuration, _logger);

        // Act & Assert
        Check.That(sut.IsEnabled("feature_a")).IsTrue();
        Check.That(sut.IsEnabled("feature_b")).IsFalse();
        Check.That(sut.IsEnabled("feature_c")).IsTrue();
        Check.That(sut.IsEnabled("feature_d")).IsFalse(); // Not configured
    }

    /// <summary>
    /// Construit une IConfiguration à partir d'un dictionnaire de settings.
    /// </summary>
    private static IConfiguration BuildConfiguration(Dictionary<string, string> settings)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();
    }
}
