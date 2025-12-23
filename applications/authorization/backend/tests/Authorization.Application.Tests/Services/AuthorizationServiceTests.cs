using Authorization.Application.Services;
using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.OpenFGA.Services;
using Authorization.Infrastructure.PostgreSQL.Repositories;
using Authorization.Infrastructure.Redis.Services;
using Microsoft.Extensions.Logging;
using NFluent;
using NSubstitute;
using Xunit;

namespace Authorization.Application.Tests.Services;

/// <summary>
/// Tests unitaires pour <see cref="AuthorizationService"/>.
/// </summary>
public class AuthorizationServiceTests
{
    private readonly IOpenFgaService _openFgaService;
    private readonly IPermissionCacheService _cacheService;
    private readonly IAuditLogRepository _auditRepository;
    private readonly ILogger<AuthorizationService> _logger;
    private readonly AuthorizationService _sut;

    // Scope valide pour les tests
    private static readonly Scope ValidScope = Scope.Parse("api.llmproxy.com/projects/main");

    public AuthorizationServiceTests()
    {
        _openFgaService = Substitute.For<IOpenFgaService>();
        _cacheService = Substitute.For<IPermissionCacheService>();
        _auditRepository = Substitute.For<IAuditLogRepository>();
        _logger = Substitute.For<ILogger<AuthorizationService>>();

        _sut = new AuthorizationService(
            _openFgaService,
            _cacheService,
            _auditRepository,
            _logger);
    }

    #region CheckPermissionAsync - Cache Hit

    [Fact]
    public async Task CheckPermissionAsync_WithCachedTrue_ReturnsTrue()
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var principalType = PrincipalType.User;
        var permission = Permission.Create("prompts", "read");

        _cacheService.GetPermissionCheckAsync(
            tenantId,
            principalId,
            permission,
            ValidScope,
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<bool?>(true));

        // Act
        var result = await _sut.CheckPermissionAsync(
            tenantId,
            principalId,
            principalType,
            permission,
            ValidScope);

        // Assert
        Check.That(result).IsTrue();

        // Verify OpenFGA was NOT called (cache hit)
        await _openFgaService.DidNotReceive().CheckAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<PrincipalType>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckPermissionAsync_WithCachedFalse_ReturnsFalse()
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var principalType = PrincipalType.User;
        var permission = Permission.Create("prompts", "read");

        _cacheService.GetPermissionCheckAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<Permission>(),
            Arg.Any<Scope>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<bool?>(false));

        // Act
        var result = await _sut.CheckPermissionAsync(
            tenantId,
            principalId,
            principalType,
            permission,
            ValidScope);

        // Assert
        Check.That(result).IsFalse();
    }

    #endregion

    #region CheckPermissionAsync - Cache Miss

    [Fact]
    public async Task CheckPermissionAsync_WithCacheMiss_CallsOpenFga()
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var principalType = PrincipalType.User;
        var permission = Permission.Create("prompts", "read");

        _cacheService.GetPermissionCheckAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<Permission>(),
            Arg.Any<Scope>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<bool?>(null));

        _openFgaService.CheckAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<PrincipalType>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act
        var result = await _sut.CheckPermissionAsync(
            tenantId,
            principalId,
            principalType,
            permission,
            ValidScope);

        // Assert
        Check.That(result).IsTrue();

        // Verify OpenFGA was called (cache miss)
        await _openFgaService.Received(1).CheckAsync(
            tenantId,
            principalId,
            principalType,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        // Verify cache was updated
        await _cacheService.Received(1).SetPermissionCheckAsync(
            tenantId,
            principalId,
            permission,
            ValidScope,
            true,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckPermissionAsync_WhenOpenFgaDenies_ReturnsFalse()
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var principalType = PrincipalType.User;
        var permission = Permission.Create("models", "delete");
        var productionScope = Scope.Parse("api.llmproxy.com/production");

        _cacheService.GetPermissionCheckAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<Permission>(),
            Arg.Any<Scope>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<bool?>(null));

        _openFgaService.CheckAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<PrincipalType>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        // Act
        var result = await _sut.CheckPermissionAsync(
            tenantId,
            principalId,
            principalType,
            permission,
            productionScope);

        // Assert
        Check.That(result).IsFalse();

        // Verify cache was updated with false
        await _cacheService.Received(1).SetPermissionCheckAsync(
            tenantId,
            principalId,
            permission,
            productionScope,
            false,
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region CheckPermissionAsync - Principal Types

    [Theory]
    [InlineData(PrincipalType.User)]
    [InlineData(PrincipalType.Group)]
    [InlineData(PrincipalType.ServiceAccount)]
    public async Task CheckPermissionAsync_WithDifferentPrincipalTypes_PassesCorrectType(PrincipalType principalType)
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var permission = Permission.Create("prompts", "read");

        _cacheService.GetPermissionCheckAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<Permission>(),
            Arg.Any<Scope>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<bool?>(null));

        _openFgaService.CheckAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<PrincipalType>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act
        await _sut.CheckPermissionAsync(
            tenantId,
            principalId,
            principalType,
            permission,
            ValidScope);

        // Assert - verify correct type was passed
        await _openFgaService.Received(1).CheckAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            principalType,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region CheckPermissionAsync - Audit Logging

    [Fact]
    public async Task CheckPermissionAsync_Always_LogsAudit()
    {
        // Arrange
        var tenantId = TenantId.Create("tenant-1");
        var principalId = PrincipalId.Create(Guid.NewGuid());
        var principalType = PrincipalType.User;
        var permission = Permission.Create("prompts", "read");

        _cacheService.GetPermissionCheckAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<Permission>(),
            Arg.Any<Scope>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<bool?>(true));

        // Act
        await _sut.CheckPermissionAsync(
            tenantId,
            principalId,
            principalType,
            permission,
            ValidScope);

        // Assert - verify audit log was created via AddAsync
        await _auditRepository.Received(1).AddAsync(
            Arg.Any<Authorization.Infrastructure.PostgreSQL.Entities.AuditLog>(),
            Arg.Any<CancellationToken>());
    }

    #endregion
}
