using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using LLMProxy.Gateway.Middleware;
using LLMProxy.Infrastructure.Security.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NFluent;
using NSubstitute;
using Xunit;

namespace LLMProxy.Gateway.Tests.Middleware;

/// <summary>
/// Tests unitaires pour le middleware d'authentification par clé API.
/// </summary>
public class ApiKeyAuthenticationMiddlewareTests
{
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IApiKeyExtractor _apiKeyExtractor;
    private readonly IApiKeyAuthenticator _apiKeyAuthenticator;
    private readonly RequestDelegate _next;
    private readonly DefaultHttpContext _httpContext;

    public ApiKeyAuthenticationMiddlewareTests()
    {
        _logger = Substitute.For<ILogger<ApiKeyAuthenticationMiddleware>>();
        _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _apiKeyExtractor = Substitute.For<IApiKeyExtractor>();
        _apiKeyAuthenticator = Substitute.For<IApiKeyAuthenticator>();
        _next = Substitute.For<RequestDelegate>();
        _httpContext = new DefaultHttpContext();
    }

    private ApiKeyAuthenticationMiddleware CreateMiddleware()
    {
        return new ApiKeyAuthenticationMiddleware(
            _next,
            _logger,
            _serviceScopeFactory,
            _apiKeyExtractor,
            _apiKeyAuthenticator);
    }

    [Fact]
    public async Task InvokeAsync_HealthEndpoint_BypassesAuthentication()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _httpContext.Request.Path = "/health";

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        await _next.Received(1).Invoke(_httpContext);
        _apiKeyExtractor.DidNotReceive().ExtractApiKey(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task InvokeAsync_MissingApiKey_Returns401()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _httpContext.Request.Path = "/api/proxy";
        _httpContext.Response.Body = new MemoryStream();
        _apiKeyExtractor.ExtractApiKey(_httpContext).Returns((string?)null);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Check.That(_httpContext.Response.StatusCode).IsEqualTo(401);
        await _next.DidNotReceive().Invoke(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task InvokeAsync_ValidApiKey_AuthenticatesAndCallsNext()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _httpContext.Request.Path = "/api/proxy";
        _httpContext.Response.Body = new MemoryStream();

        var apiKey = "test-api-key-12345";
        var tenantId = Guid.NewGuid();

        // Créer User via factory
        var userResult = User.Create(tenantId, "test@example.com", "Test User", UserRole.User);
        Check.That(userResult.IsSuccess).IsTrue();
        var user = userResult.Value;

        // Créer ApiKey via factory
        var apiKeyResult = user.CreateApiKey("Test Key", DateTime.UtcNow.AddYears(1));
        Check.That(apiKeyResult.IsSuccess).IsTrue();
        var apiKeyEntity = apiKeyResult.Value;

        var authResult = ApiKeyAuthenticationResult.Success(apiKeyEntity, user);

        _apiKeyExtractor.ExtractApiKey(_httpContext).Returns(apiKey);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var serviceScope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(typeof(IUnitOfWork)).Returns(unitOfWork);
        serviceScope.ServiceProvider.Returns(serviceProvider);
        _serviceScopeFactory.CreateScope().Returns(serviceScope);

        _apiKeyAuthenticator.AuthenticateAsync(apiKey, unitOfWork, Arg.Any<CancellationToken>())
            .Returns(authResult);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Check.That(_httpContext.Items["UserId"]).IsEqualTo(user.Id);
        Check.That(_httpContext.Items["TenantId"]).IsEqualTo(user.TenantId);
        Check.That(_httpContext.Items["ApiKeyId"]).IsEqualTo(apiKeyEntity.Id);
        Check.That(_httpContext.Items["UserRole"]).IsEqualTo("User");
        await _next.Received(1).Invoke(_httpContext);
    }

    [Fact]
    public async Task InvokeAsync_FailedAuthentication_Returns401()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _httpContext.Request.Path = "/api/proxy";
        _httpContext.Response.Body = new MemoryStream();

        var apiKey = "invalid-api-key";
        var authResult = ApiKeyAuthenticationResult.Failure("Invalid API key", 401);

        _apiKeyExtractor.ExtractApiKey(_httpContext).Returns(apiKey);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var serviceScope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(typeof(IUnitOfWork)).Returns(unitOfWork);
        serviceScope.ServiceProvider.Returns(serviceProvider);
        _serviceScopeFactory.CreateScope().Returns(serviceScope);

        _apiKeyAuthenticator.AuthenticateAsync(apiKey, unitOfWork, Arg.Any<CancellationToken>())
            .Returns(authResult);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Check.That(_httpContext.Response.StatusCode).IsEqualTo(401);
        await _next.DidNotReceive().Invoke(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task InvokeAsync_RevokedApiKey_Returns401()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _httpContext.Request.Path = "/api/proxy";
        _httpContext.Response.Body = new MemoryStream();

        var apiKey = "revoked-api-key";
        var authResult = ApiKeyAuthenticationResult.Failure("API key has been revoked", 401);

        _apiKeyExtractor.ExtractApiKey(_httpContext).Returns(apiKey);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var serviceScope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(typeof(IUnitOfWork)).Returns(unitOfWork);
        serviceScope.ServiceProvider.Returns(serviceProvider);
        _serviceScopeFactory.CreateScope().Returns(serviceScope);

        _apiKeyAuthenticator.AuthenticateAsync(apiKey, unitOfWork, Arg.Any<CancellationToken>())
            .Returns(authResult);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Check.That(_httpContext.Response.StatusCode).IsEqualTo(401);
        await _next.DidNotReceive().Invoke(Arg.Any<HttpContext>());
    }

    [Fact]
    public async Task InvokeAsync_ExpiredApiKey_Returns401()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _httpContext.Request.Path = "/api/proxy";
        _httpContext.Response.Body = new MemoryStream();

        var apiKey = "expired-api-key";
        var authResult = ApiKeyAuthenticationResult.Failure("API key has expired", 401);

        _apiKeyExtractor.ExtractApiKey(_httpContext).Returns(apiKey);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var serviceScope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(typeof(IUnitOfWork)).Returns(unitOfWork);
        serviceScope.ServiceProvider.Returns(serviceProvider);
        _serviceScopeFactory.CreateScope().Returns(serviceScope);

        _apiKeyAuthenticator.AuthenticateAsync(apiKey, unitOfWork, Arg.Any<CancellationToken>())
            .Returns(authResult);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Check.That(_httpContext.Response.StatusCode).IsEqualTo(401);
        await _next.DidNotReceive().Invoke(Arg.Any<HttpContext>());
    }
}
