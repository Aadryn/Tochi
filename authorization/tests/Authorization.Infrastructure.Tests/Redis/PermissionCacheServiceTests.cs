using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.Redis.Configuration;
using Authorization.Infrastructure.Redis.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NFluent;
using NSubstitute;
using Xunit;

namespace Authorization.Infrastructure.Tests.Redis;

/// <summary>
/// Tests unitaires pour <see cref="PermissionCacheService"/>.
/// </summary>
public class PermissionCacheServiceTests
{
    private readonly IDistributedCache _cache;
    private readonly IOptions<RedisCacheOptions> _options;
    private readonly ILogger<PermissionCacheService> _logger;
    private readonly PermissionCacheService _sut;

    // Scope valide pour les tests
    private static readonly Scope ValidScope = Scope.Parse("api.llmproxy.com/projects/main");

    public PermissionCacheServiceTests()
    {
        _cache = Substitute.For<IDistributedCache>();
        _logger = Substitute.For<ILogger<PermissionCacheService>>();

        var options = new RedisCacheOptions
        {
            Enabled = true,
            PermissionCheckTtlSeconds = 300
        };
        _options = Options.Create(options);

        _sut = new PermissionCacheService(_cache, _options, _logger);
    }

    #region GetPermissionCheckAsync

    [Fact]
    public async Task GetPermissionCheckAsync_WhenCacheDisabled_ReturnsNull()
    {
        // Arrange
        var options = Options.Create(new RedisCacheOptions { Enabled = false });
        var sut = new PermissionCacheService(_cache, options, _logger);

        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var permission = Permission.Create("prompts", "read");

        // Act
        var result = await sut.GetPermissionCheckAsync(tenantId, principalId, permission, ValidScope);

        // Assert
        Check.That(result).IsNull();

        // Verify cache was not accessed
        await _cache.DidNotReceive().GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPermissionCheckAsync_WithCacheMiss_ReturnsNull()
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var permission = Permission.Create("prompts", "read");

        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<byte[]?>(null));

        // Act
        var result = await _sut.GetPermissionCheckAsync(tenantId, principalId, permission, ValidScope);

        // Assert
        Check.That(result).IsNull();
    }

    [Fact]
    public async Task GetPermissionCheckAsync_WithCachedTrue_ReturnsTrue()
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var permission = Permission.Create("prompts", "read");

        // "1" encoded as UTF8 bytes
        var cachedValue = System.Text.Encoding.UTF8.GetBytes("1");
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<byte[]?>(cachedValue));

        // Act
        var result = await _sut.GetPermissionCheckAsync(tenantId, principalId, permission, ValidScope);

        // Assert
        Check.That(result).IsEqualTo(true);
    }

    [Fact]
    public async Task GetPermissionCheckAsync_WithCachedFalse_ReturnsFalse()
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var permission = Permission.Create("prompts", "read");

        // "0" encoded as UTF8 bytes
        var cachedValue = System.Text.Encoding.UTF8.GetBytes("0");
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<byte[]?>(cachedValue));

        // Act
        var result = await _sut.GetPermissionCheckAsync(tenantId, principalId, permission, ValidScope);

        // Assert
        Check.That(result).IsEqualTo(false);
    }

    [Fact]
    public async Task GetPermissionCheckAsync_OnException_ReturnsNull()
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var permission = Permission.Create("prompts", "read");

        _cache.When(c => c.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("Redis connection failed"));

        // Act
        var result = await _sut.GetPermissionCheckAsync(tenantId, principalId, permission, ValidScope);

        // Assert - gracefully handles exception
        Check.That(result).IsNull();
    }

    #endregion

    #region SetPermissionCheckAsync

    [Fact]
    public async Task SetPermissionCheckAsync_WhenCacheDisabled_DoesNothing()
    {
        // Arrange
        var options = Options.Create(new RedisCacheOptions { Enabled = false });
        var sut = new PermissionCacheService(_cache, options, _logger);

        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var permission = Permission.Create("prompts", "read");

        // Act
        await sut.SetPermissionCheckAsync(tenantId, principalId, permission, ValidScope, true);

        // Assert
        await _cache.DidNotReceive().SetAsync(
            Arg.Any<string>(),
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetPermissionCheckAsync_WithTrue_CachesCorrectValue()
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var permission = Permission.Create("prompts", "read");

        // Act
        await _sut.SetPermissionCheckAsync(tenantId, principalId, permission, ValidScope, true);

        // Assert
        await _cache.Received(1).SetAsync(
            Arg.Any<string>(),
            Arg.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == "1"),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetPermissionCheckAsync_WithFalse_CachesCorrectValue()
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var permission = Permission.Create("prompts", "read");

        // Act
        await _sut.SetPermissionCheckAsync(tenantId, principalId, permission, ValidScope, false);

        // Assert
        await _cache.Received(1).SetAsync(
            Arg.Any<string>(),
            Arg.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == "0"),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Key Generation

    [Fact]
    public async Task GetPermissionCheckAsync_GeneratesCorrectKey()
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-123");
        var principalId = PrincipalId.Create(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
        var permission = Permission.Create("prompts", "read");
        var scopeWithPath = Scope.Parse("api.llmproxy.com/projects/main");

        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<byte[]?>(null));

        // Act
        await _sut.GetPermissionCheckAsync(tenantId, principalId, permission, scopeWithPath);

        // Assert - verify key format: perm:{tenant}:{principal}:{permission}:{scope}
        await _cache.Received(1).GetAsync(
            Arg.Is<string>(k => k.StartsWith("perm:tenant-123:") && k.Contains("prompts:read")),
            Arg.Any<CancellationToken>());
    }

    #endregion
}
