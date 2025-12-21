using System.Diagnostics;

namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Middleware for comprehensive request/response logging and metrics
/// Follows Single Responsibility Principle
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private static readonly ActivitySource _activitySource = new("LLMProxy.Gateway");

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invoque le middleware de logging des requêtes.
    /// </summary>
    /// <param name="context">Contexte HTTP de la requête en cours.</param>
    /// <param name="cancellationToken">Token d'annulation pour interrompre l'opération.</param>
    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString();
        context.Items["RequestId"] = requestId;

        using var activity = _activitySource.StartActivity("ProcessRequest");
        activity?.SetTag("request.id", requestId);
        activity?.SetTag("request.method", context.Request.Method);
        activity?.SetTag("request.path", context.Request.Path);

        var sw = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Request started: {Method} {Path} | RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                requestId);

            await _next(context);

            sw.Stop();

            _logger.LogInformation(
                "Request completed: {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                requestId);

            activity?.SetTag("response.status_code", context.Response.StatusCode);
            activity?.SetTag("response.duration_ms", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();

            _logger.LogError(ex,
                "Request failed: {Method} {Path} | Duration: {Duration}ms | RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds,
                requestId);

            activity?.SetTag("error", true);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);

            throw;
        }
    }
}
