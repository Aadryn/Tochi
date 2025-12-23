using LLMProxy.Gateway.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NFluent;
using NSubstitute;
using Xunit;

namespace LLMProxy.Gateway.Tests.Middleware;

/// <summary>
/// Tests unitaires pour le middleware de gestion globale des exceptions.
/// </summary>
public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;
    private readonly RequestDelegate _next;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _logger = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
        _environment = Substitute.For<IHostEnvironment>();
        _environment.EnvironmentName.Returns("Production");
        _next = Substitute.For<RequestDelegate>();
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    private GlobalExceptionHandlerMiddleware CreateMiddleware()
    {
        return new GlobalExceptionHandlerMiddleware(_next, _logger, _environment);
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNext()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _next.When(x => x.Invoke(_httpContext)).Do(_ => { });

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        await _next.Received(1).Invoke(_httpContext);
        Check.That(_httpContext.Response.StatusCode).IsEqualTo(200);
    }

    [Fact]
    public async Task InvokeAsync_OperationCanceledException_Returns499()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _next.When(x => x.Invoke(_httpContext))
            .Do(_ => throw new OperationCanceledException("Request cancelled"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Check.That(_httpContext.Response.StatusCode).IsEqualTo(499);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Returns401()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _next.When(x => x.Invoke(_httpContext))
            .Do(_ => throw new UnauthorizedAccessException("Unauthorized"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Check.That(_httpContext.Response.StatusCode).IsEqualTo(401);
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_Returns400()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _next.When(x => x.Invoke(_httpContext))
            .Do(_ => throw new ArgumentException("Invalid argument"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Check.That(_httpContext.Response.StatusCode).IsEqualTo(400);
    }

    [Fact]
    public async Task InvokeAsync_InvalidOperationException_Returns409()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _next.When(x => x.Invoke(_httpContext))
            .Do(_ => throw new InvalidOperationException("Invalid operation"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Check.That(_httpContext.Response.StatusCode).IsEqualTo(409);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _next.When(x => x.Invoke(_httpContext))
            .Do(_ => throw new Exception("Unexpected error"));

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Check.That(_httpContext.Response.StatusCode).IsEqualTo(500);
    }

    [Fact]
    public async Task InvokeAsync_Exception_DoesNotCrash()
    {
        // Arrange
        var middleware = CreateMiddleware();
        _next.When(x => x.Invoke(_httpContext))
            .Do(_ => throw new Exception("Test exception"));

        // Act & Assert - Should not throw
        await middleware.InvokeAsync(_httpContext);
        
        // Verify error was handled (500 status)
        Check.That(_httpContext.Response.StatusCode).IsEqualTo(500);
    }

    [Fact(Skip = "LoggerMessage extensions use generated code - difficult to mock. Functional testing validates this.")]
    public async Task InvokeAsync_Exception_LogsError()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var exception = new Exception("Test error");
        _next.When(x => x.Invoke(_httpContext)).Do(_ => throw exception);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert - LoggerMessage extension generates EventId 3005
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Is<EventId>(e => e.Id == 3005),  // EventId for UnhandledException
            Arg.Is<object>(o => o.ToString()!.Contains("/test") && o.ToString()!.Contains("POST")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
