using FluentAssertions;
using LLMProxy.Application.Configuration.RateLimiting;
using LLMProxy.Application.Interfaces;
using NSubstitute;

namespace LLMProxy.Application.Tests.Repositories;

/// <summary>
/// Tests unitaires pour valider le contrat de <see cref="ITenantRateLimitConfigurationRepository"/>.
/// </summary>
/// <remarks>
/// <para>
/// Ces tests valident le comportement attendu de l'interface repository.
/// Les tests d'intégration avec PostgreSQL réel seront dans un projet séparé.
/// Conforme à l'ADR-041 Rate Limiting.
/// </para>
/// </remarks>
public sealed class TenantRateLimitConfigurationRepositoryContractTests
{
    private readonly ITenantRateLimitConfigurationRepository _mockRepository;

    public TenantRateLimitConfigurationRepositoryContractTests()
    {
        _mockRepository = Substitute.For<ITenantRateLimitConfigurationRepository>();
    }

    #region GetByTenantIdAsync

    [Fact]
    public async Task GetByTenantIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _mockRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns((TenantRateLimitConfiguration?)null);

        // Act
        var result = await _mockRepository.GetByTenantIdAsync(tenantId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTenantIdAsync_WhenFound_ReturnsConfiguration()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = CreateConfig(tenantId);
        _mockRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(config);

        // Act
        var result = await _mockRepository.GetByTenantIdAsync(tenantId);

        // Assert
        result.Should().NotBeNull();
        result!.TenantId.Should().Be(tenantId);
        result.GlobalLimit.RequestsPerMinute.Should().Be(500);
        result.ApiKeyLimit.RequestsPerMinute.Should().Be(50);
    }

    [Fact]
    public async Task GetByTenantIdAsync_WithEndpointLimits_ReturnsAllData()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var config = CreateConfig(tenantId);
        _mockRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(config);

        // Act
        var result = await _mockRepository.GetByTenantIdAsync(tenantId);

        // Assert
        result.Should().NotBeNull();
        result!.EndpointLimits.Should().ContainKey("/v1/chat/completions");
        result.EndpointLimits["/v1/chat/completions"].RequestsPerMinute.Should().Be(30);
    }

    #endregion

    #region UpsertAsync

    [Fact]
    public async Task UpsertAsync_InsertsOrUpdatesConfiguration()
    {
        // Arrange
        var config = CreateConfig(Guid.NewGuid());
        _mockRepository.UpsertAsync(config, Arg.Any<CancellationToken>())
            .Returns(config);

        // Act
        var result = await _mockRepository.UpsertAsync(config);

        // Assert
        result.Should().NotBeNull();
        result.TenantId.Should().Be(config.TenantId);
        await _mockRepository.Received(1).UpsertAsync(config, Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_RemovesConfiguration()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        await _mockRepository.DeleteAsync(tenantId);

        // Assert
        await _mockRepository.Received(1).DeleteAsync(tenantId, Arg.Any<CancellationToken>());
    }

    #endregion

    #region ExistsAsync

    [Fact]
    public async Task ExistsAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _mockRepository.ExistsAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _mockRepository.ExistsAsync(tenantId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenNotExists_ReturnsFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _mockRepository.ExistsAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _mockRepository.ExistsAsync(tenantId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helpers

    private static TenantRateLimitConfiguration CreateConfig(Guid tenantId)
    {
        return new TenantRateLimitConfiguration
        {
            TenantId = tenantId,
            GlobalLimit = new GlobalLimit
            {
                RequestsPerMinute = 500,
                RequestsPerDay = 50_000,
                TokensPerMinute = 50_000,
                TokensPerDay = 5_000_000
            },
            ApiKeyLimit = new ApiKeyLimit
            {
                RequestsPerMinute = 50,
                TokensPerMinute = 5_000
            },
            EndpointLimits = new Dictionary<string, EndpointLimit>
            {
                ["/v1/chat/completions"] = new EndpointLimit
                {
                    RequestsPerMinute = 30,
                    TokensPerMinute = 50_000,
                    BurstCapacity = 60
                }
            }
        };
    }

    #endregion
}
