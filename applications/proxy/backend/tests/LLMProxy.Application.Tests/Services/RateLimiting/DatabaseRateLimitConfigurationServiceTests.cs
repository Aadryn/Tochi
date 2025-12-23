using FluentAssertions;
using LLMProxy.Application.Configuration.RateLimiting;
using LLMProxy.Application.Interfaces;
using LLMProxy.Application.Services.RateLimiting;
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace LLMProxy.Application.Tests.Services.RateLimiting;

/// <summary>
/// Tests unitaires pour <see cref="DatabaseRateLimitConfigurationService"/>.
/// </summary>
/// <remarks>
/// Conforme à l'ADR-041 Rate Limiting et ADR-042 Distributed Cache Strategy.
/// </remarks>
public sealed class DatabaseRateLimitConfigurationServiceTests
{
    private readonly ITenantRateLimitConfigurationRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DatabaseRateLimitConfigurationService> _logger;
    private readonly DatabaseRateLimitConfigurationService _sut;

    public DatabaseRateLimitConfigurationServiceTests()
    {
        _repository = Substitute.For<ITenantRateLimitConfigurationRepository>();
        _cacheService = Substitute.For<ICacheService>();
        _logger = Substitute.For<ILogger<DatabaseRateLimitConfigurationService>>();
        _sut = new DatabaseRateLimitConfigurationService(_repository, _cacheService, _logger);
    }

    #region GetConfigurationAsync

    [Fact]
    public async Task GetConfigurationAsync_WhenCacheHit_ReturnsFromCache()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cachedConfig = CreateConfig(tenantId);
        
        _cacheService
            .GetAsync<TenantRateLimitConfiguration>($"ratelimit:config:{tenantId}", Arg.Any<CancellationToken>())
            .Returns(cachedConfig);

        // Act
        var result = await _sut.GetConfigurationAsync(tenantId);

        // Assert
        result.Should().BeEquivalentTo(cachedConfig);
        await _repository.DidNotReceive().GetByTenantIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetConfigurationAsync_WhenCacheMissAndDbHit_LoadsFromDbAndCaches()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbConfig = CreateConfig(tenantId);
        var cacheKey = $"ratelimit:config:{tenantId}";

        _cacheService
            .GetAsync<TenantRateLimitConfiguration>(cacheKey, Arg.Any<CancellationToken>())
            .Returns((TenantRateLimitConfiguration?)null);

        _repository
            .GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(dbConfig);

        // Act
        var result = await _sut.GetConfigurationAsync(tenantId);

        // Assert
        result.Should().BeEquivalentTo(dbConfig);
        await _cacheService.Received(1).SetAsync(
            cacheKey,
            Arg.Any<TenantRateLimitConfiguration>(),
            TimeSpan.FromMinutes(1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetConfigurationAsync_WhenCacheMissAndDbMiss_ReturnsDefault()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        _cacheService
            .GetAsync<TenantRateLimitConfiguration>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((TenantRateLimitConfiguration?)null);

        _repository
            .GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns((TenantRateLimitConfiguration?)null);

        // Act
        var result = await _sut.GetConfigurationAsync(tenantId);

        // Assert
        result.Should().NotBeNull();
        result.TenantId.Should().Be(tenantId);
        result.GlobalLimit.RequestsPerMinute.Should().Be(1000); // Valeur par défaut
        result.ApiKeyLimit.RequestsPerMinute.Should().Be(100);  // Valeur par défaut
    }

    [Fact]
    public async Task GetConfigurationAsync_WhenCacheThrows_FallsBackToDb()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbConfig = CreateConfig(tenantId);

        _cacheService
            .GetAsync<TenantRateLimitConfiguration>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Redis unavailable"));

        _repository
            .GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(dbConfig);

        // Act
        var result = await _sut.GetConfigurationAsync(tenantId);

        // Assert
        result.Should().BeEquivalentTo(dbConfig);
    }

    #endregion

    #region UpdateConfigurationAsync

    [Fact]
    public async Task UpdateConfigurationAsync_UpsertsToDatabaseAndInvalidatesCache()
    {
        // Arrange
        var config = CreateConfig(Guid.NewGuid());
        var cacheKey = $"ratelimit:config:{config.TenantId}";

        _repository
            .UpsertAsync(config, Arg.Any<CancellationToken>())
            .Returns(config);

        // Act
        var result = await _sut.UpdateConfigurationAsync(config);

        // Assert
        result.Should().BeEquivalentTo(config);
        await _repository.Received(1).UpsertAsync(config, Arg.Any<CancellationToken>());
        await _cacheService.Received(1).RemoveAsync(cacheKey, Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteConfigurationAsync

    [Fact]
    public async Task DeleteConfigurationAsync_DeletesFromDatabaseAndInvalidatesCache()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cacheKey = $"ratelimit:config:{tenantId}";

        // Act
        await _sut.DeleteConfigurationAsync(tenantId);

        // Assert
        await _repository.Received(1).DeleteAsync(tenantId, Arg.Any<CancellationToken>());
        await _cacheService.Received(1).RemoveAsync(cacheKey, Arg.Any<CancellationToken>());
    }

    #endregion

    #region InvalidateCacheAsync

    [Fact]
    public async Task InvalidateCacheAsync_RemovesCacheEntry()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var cacheKey = $"ratelimit:config:{tenantId}";

        // Act
        await _sut.InvalidateCacheAsync(tenantId);

        // Assert
        await _cacheService.Received(1).RemoveAsync(cacheKey, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvalidateCacheAsync_WhenCacheThrows_DoesNotRethrow()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        _cacheService
            .RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Redis unavailable"));

        // Act
        var act = async () => await _sut.InvalidateCacheAsync(tenantId);

        // Assert
        await act.Should().NotThrowAsync();
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
