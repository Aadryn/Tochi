using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LLMProxy.Infrastructure.Authorization.Tests;

/// <summary>
/// Tests unitaires pour <see cref="OpenFgaConfiguration"/>.
/// </summary>
public class OpenFgaConfigurationTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var config = new OpenFgaConfiguration();

        // Assert
        Check.That(config.ApiUrl).IsEqualTo("http://localhost:8080");
        Check.That(config.StoreId).IsEmpty();
        Check.That(config.AuthorizationModelId).IsEmpty();
        Check.That(config.TimeoutSeconds).IsEqualTo(10);
        Check.That(config.MaxRetries).IsEqualTo(3);
        Check.That(config.Enabled).IsTrue();
        Check.That(config.FallbackMode).IsEqualTo(FallbackMode.Deny);
    }

    [Fact]
    public void Validate_WhenEnabledAndNoStoreId_ThrowsException()
    {
        // Arrange
        var config = new OpenFgaConfiguration
        {
            Enabled = true,
            StoreId = ""
        };

        // Act & Assert
        Check.ThatCode(() => config.Validate())
            .Throws<InvalidOperationException>()
            .WithMessage("OpenFga.StoreId est requis quand OpenFga.Enabled est true. Exécutez init-openfga.sh pour initialiser le store.");
    }

    [Fact]
    public void Validate_WhenEnabledWithStoreId_DoesNotThrow()
    {
        // Arrange
        var config = new OpenFgaConfiguration
        {
            Enabled = true,
            StoreId = "01HTXYZ123456789ABCDEFGHIJ",
            ApiUrl = "http://localhost:8080"
        };

        // Act & Assert
        Check.ThatCode(() => config.Validate()).DoesNotThrow();
    }

    [Fact]
    public void Validate_WhenDisabled_DoesNotThrowEvenWithoutStoreId()
    {
        // Arrange
        var config = new OpenFgaConfiguration
        {
            Enabled = false,
            StoreId = ""
        };

        // Act & Assert
        Check.ThatCode(() => config.Validate()).DoesNotThrow();
    }

    [Fact]
    public void Validate_WhenEnabledAndNoApiUrl_ThrowsException()
    {
        // Arrange
        var config = new OpenFgaConfiguration
        {
            Enabled = true,
            StoreId = "01HTXYZ123456789ABCDEFGHIJ",
            ApiUrl = ""
        };

        // Act & Assert
        Check.ThatCode(() => config.Validate())
            .Throws<InvalidOperationException>()
            .WithMessage("OpenFga.ApiUrl est requis.");
    }

    [Theory]
    [InlineData(FallbackMode.Deny)]
    [InlineData(FallbackMode.Allow)]
    public void FallbackMode_CanBeSet(FallbackMode mode)
    {
        // Arrange
        var config = new OpenFgaConfiguration();

        // Act
        config.FallbackMode = mode;

        // Assert
        Check.That(config.FallbackMode).IsEqualTo(mode);
    }
}
