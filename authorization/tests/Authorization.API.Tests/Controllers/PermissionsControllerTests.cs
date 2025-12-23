using Authorization.API.Contracts.Requests;
using Authorization.API.Contracts.Responses;
using Authorization.API.Controllers;
using Authorization.Application.Services;
using Authorization.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NFluent;
using NSubstitute;
using System.Security.Claims;
using Xunit;

namespace Authorization.API.Tests.Controllers;

/// <summary>
/// Tests unitaires pour <see cref="PermissionsController"/>.
/// Tests la logique du contrôleur en isolation.
/// </summary>
public class PermissionsControllerTests
{
    private readonly IRbacAuthorizationService _authorizationService;
    private readonly ILogger<PermissionsController> _logger;
    private readonly PermissionsController _sut;

    public PermissionsControllerTests()
    {
        _authorizationService = Substitute.For<IRbacAuthorizationService>();
        _logger = Substitute.For<ILogger<PermissionsController>>();

        _sut = new PermissionsController(_authorizationService, _logger);

        // Setup default authenticated user with correct claim types
        SetupAuthenticatedUser(
            tenantId: "tenant-1",
            principalId: Guid.NewGuid().ToString(),
            principalType: "user");
    }

    private void SetupAuthenticatedUser(string tenantId, string principalId, string principalType)
    {
        var claims = new List<Claim>
        {
            // Azure AD / Microsoft claims
            new("http://schemas.microsoft.com/identity/claims/tenantid", tenantId),
            new("http://schemas.microsoft.com/identity/claims/objectidentifier", principalId),
            new("principal_type", principalType),
            // Fallbacks
            new("tid", tenantId),
            new("oid", principalId)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    #region CheckPermission - Basic Tests

    [Fact]
    public async Task CheckPermission_WhenAllowed_ReturnsOkWithAllowedTrue()
    {
        // Arrange
        var request = new CheckPermissionRequest
        {
            Permission = "prompts:read",
            Scope = "api.llmproxy.com/projects/main"
        };

        _authorizationService.CheckPermissionAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<PrincipalType>(),
            Arg.Any<Permission>(),
            Arg.Any<Scope>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act
        var result = await _sut.CheckPermission(request, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Check.That(okResult).IsNotNull();
        Check.That(okResult!.StatusCode).IsEqualTo(StatusCodes.Status200OK);

        var response = okResult.Value as PermissionCheckResponse;
        Check.That(response).IsNotNull();
        Check.That(response!.Allowed).IsTrue();
        Check.That(response.Permission).IsEqualTo("prompts:read");
        Check.That(response.Scope).IsEqualTo("api.llmproxy.com/projects/main");
    }

    [Fact]
    public async Task CheckPermission_WhenDenied_ReturnsOkWithAllowedFalse()
    {
        // Arrange
        var request = new CheckPermissionRequest
        {
            Permission = "models:delete",
            Scope = "api.llmproxy.com/production"
        };

        _authorizationService.CheckPermissionAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<PrincipalType>(),
            Arg.Any<Permission>(),
            Arg.Any<Scope>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        // Act
        var result = await _sut.CheckPermission(request, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Check.That(okResult).IsNotNull();

        var response = okResult!.Value as PermissionCheckResponse;
        Check.That(response).IsNotNull();
        Check.That(response!.Allowed).IsFalse();
    }

    #endregion

    #region CheckPermission - Permission Parsing

    [Theory]
    [InlineData("prompts:read")]
    [InlineData("models:write")]
    [InlineData("configurations:admin")]
    public async Task CheckPermission_ParsesPermissionCorrectly(string permissionStr)
    {
        // Arrange
        var request = new CheckPermissionRequest
        {
            Permission = permissionStr,
            Scope = "api.llmproxy.com"
        };

        Permission? capturedPermission = null;
        _authorizationService.CheckPermissionAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<PrincipalType>(),
            Arg.Do<Permission>(p => capturedPermission = p),
            Arg.Any<Scope>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act
        await _sut.CheckPermission(request, CancellationToken.None);

        // Assert
        Check.That(capturedPermission).IsNotNull();
        Check.That(capturedPermission!.Value).IsEqualTo(permissionStr);
    }

    #endregion

    #region CheckPermission - Scope Parsing

    [Theory]
    [InlineData("api.llmproxy.com")]
    [InlineData("api.llmproxy.com/projects")]
    [InlineData("api.llmproxy.com/projects/main/prompts")]
    [InlineData("api.llmproxy.com/tenants/acme/users")]
    public async Task CheckPermission_ParsesScopeCorrectly(string scopeStr)
    {
        // Arrange
        var request = new CheckPermissionRequest
        {
            Permission = "prompts:read",
            Scope = scopeStr
        };

        Scope? capturedScope = null;
        _authorizationService.CheckPermissionAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<PrincipalType>(),
            Arg.Any<Permission>(),
            Arg.Do<Scope>(s => capturedScope = s),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act
        await _sut.CheckPermission(request, CancellationToken.None);

        // Assert
        Check.That(capturedScope).IsNotNull();
        Check.That(capturedScope!.Path).IsNotEmpty();
    }

    #endregion

    #region CheckPermission - Explicit Principal

    [Fact]
    public async Task CheckPermission_WithExplicitPrincipalId_UsesProvidedId()
    {
        // Arrange
        var explicitPrincipalId = Guid.NewGuid();
        var request = new CheckPermissionRequest
        {
            Permission = "prompts:read",
            Scope = "api.llmproxy.com/projects",
            PrincipalId = explicitPrincipalId,
            PrincipalType = "user"
        };

        PrincipalId? capturedPrincipalId = null;
        _authorizationService.CheckPermissionAsync(
            Arg.Any<TenantId>(),
            Arg.Do<PrincipalId>(p => capturedPrincipalId = p),
            Arg.Any<PrincipalType>(),
            Arg.Any<Permission>(),
            Arg.Any<Scope>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act
        await _sut.CheckPermission(request, CancellationToken.None);

        // Assert
        Check.That(capturedPrincipalId).IsNotNull();
        Check.That(capturedPrincipalId!.Value.Value).IsEqualTo(explicitPrincipalId);
    }

    [Theory]
    [InlineData("user", PrincipalType.User)]
    [InlineData("group", PrincipalType.Group)]
    [InlineData("service_account", PrincipalType.ServiceAccount)]
    public async Task CheckPermission_WithExplicitPrincipalType_UsesProvidedType(
        string principalTypeStr,
        PrincipalType expectedType)
    {
        // Arrange
        var request = new CheckPermissionRequest
        {
            Permission = "prompts:read",
            Scope = "api.llmproxy.com/projects",
            PrincipalId = Guid.NewGuid(),
            PrincipalType = principalTypeStr
        };

        PrincipalType? capturedType = null;
        _authorizationService.CheckPermissionAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Do<PrincipalType>(t => capturedType = t),
            Arg.Any<Permission>(),
            Arg.Any<Scope>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act
        await _sut.CheckPermission(request, CancellationToken.None);

        // Assert
        Check.That(capturedType).IsEqualTo(expectedType);
    }

    #endregion

    #region Response Structure

    [Fact]
    public async Task CheckPermission_ReturnsCorrectResponseStructure()
    {
        // Arrange
        var request = new CheckPermissionRequest
        {
            Permission = "prompts:read",
            Scope = "api.llmproxy.com/projects/main"
        };

        _authorizationService.CheckPermissionAsync(
            Arg.Any<TenantId>(),
            Arg.Any<PrincipalId>(),
            Arg.Any<PrincipalType>(),
            Arg.Any<Permission>(),
            Arg.Any<Scope>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act
        var result = await _sut.CheckPermission(request, CancellationToken.None);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as PermissionCheckResponse;

        Check.That(response).IsNotNull();
        Check.That(response!.PrincipalId).IsNotNull().And.IsNotEmpty();
        Check.That(response.PrincipalType).IsNotNull().And.IsNotEmpty();
        Check.That(response.Permission).IsEqualTo("prompts:read");
        Check.That(response.Scope).IsEqualTo("api.llmproxy.com/projects/main");
        Check.That(response.DurationMs).IsStrictlyPositive();
    }

    #endregion
}
