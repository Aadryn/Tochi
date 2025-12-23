using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.API.Middleware;

/// <summary>
/// Middleware de gestion globale des exceptions.
/// Convertit les exceptions en réponses ProblemDetails standardisées.
/// </summary>
public sealed class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initialise le middleware.
    /// </summary>
    /// <param name="next">Délégué suivant dans le pipeline.</param>
    /// <param name="logger">Logger pour les erreurs.</param>
    /// <param name="environment">Environnement d'exécution.</param>
    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Invoque le middleware.
    /// </summary>
    /// <param name="context">Contexte HTTP.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = MapException(exception);

        _logger.LogError(
            exception,
            "Unhandled exception: {Message}. TraceId: {TraceId}",
            exception.Message,
            context.TraceIdentifier);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        // En développement, inclure la stack trace
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = exception.ToString();
        }

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode StatusCode, string Title, string Detail) MapException(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException ex => (
                HttpStatusCode.Forbidden,
                "Accès refusé",
                ex.Message),

            ArgumentNullException ex => (
                HttpStatusCode.BadRequest,
                "Paramètre manquant",
                $"Le paramètre '{ex.ParamName}' est requis."),

            ArgumentException ex => (
                HttpStatusCode.BadRequest,
                "Paramètre invalide",
                ex.Message),

            InvalidOperationException ex => (
                HttpStatusCode.Conflict,
                "Opération invalide",
                ex.Message),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Ressource non trouvée",
                "La ressource demandée n'existe pas."),

            OperationCanceledException => (
                HttpStatusCode.ServiceUnavailable,
                "Opération annulée",
                "L'opération a été annulée."),

            _ => (
                HttpStatusCode.InternalServerError,
                "Erreur interne",
                "Une erreur inattendue s'est produite.")
        };
    }
}
