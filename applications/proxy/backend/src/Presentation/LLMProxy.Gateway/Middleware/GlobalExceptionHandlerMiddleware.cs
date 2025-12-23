using System.Net;
using System.Text.Json;
using LLMProxy.Infrastructure.Security;
using LLMProxy.Gateway.Extensions;

namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Middleware global pour la gestion centralisée des exceptions
/// Capture toutes les exceptions non gérées et retourne des réponses structurées au client
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initialise une nouvelle instance du middleware de gestion globale des exceptions
    /// </summary>
    /// <param name="next">Délégué vers le prochain middleware dans le pipeline</param>
    /// <param name="logger">Logger pour enregistrer les exceptions</param>
    /// <param name="environment">Environnement d'exécution de l'application</param>
    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Invoque le middleware de gestion des exceptions
    /// </summary>
    /// <param name="context">Contexte HTTP de la requête en cours</param>
    /// <param name="cancellationToken">Token d'annulation pour interrompre l'opération</param>
    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context, nameof(context));

        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
        {
            // Client a annulé la requête - ne pas logger comme erreur
            _logger.LogRequestCancelled(context.Request.Path);
            
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogUnauthorizedAccess(ex, context.Request.Path);
            await HandleExceptionAsync(context, ex, HttpStatusCode.Unauthorized, "Accès non autorisé");
        }
        catch (ArgumentException ex)
        {
            _logger.LogInvalidArgument(ex, context.Request.Path);
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, "Requête invalide");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogInvalidOperation(ex, context.Request.Path);
            await HandleExceptionAsync(context, ex, HttpStatusCode.Conflict, "Opération invalide");
        }
        catch (Exception ex)
        {
            _logger.LogUnhandledException(ex,
                context.Request.Path,
                context.Request.Method,
                context.Response.StatusCode);
            
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Gère une exception en retournant une réponse structurée au client
    /// </summary>
    /// <param name="context">Contexte HTTP de la requête</param>
    /// <param name="exception">Exception à gérer</param>
    /// <param name="statusCode">Code de statut HTTP à retourner</param>
    /// <param name="message">Message d'erreur pour le client</param>
    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        HttpStatusCode statusCode,
        string message)
    {
        Guard.AgainstResponseStarted(context.Response);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Message = message,
                Type = exception.GetType().Name,
                StatusCode = (int)statusCode,
                RequestId = context.Items.TryGetValue("RequestId", out var reqId) ? reqId?.ToString() : null,
                Timestamp = DateTime.UtcNow
            }
        };

        // Inclure la stack trace uniquement en développement
        if (_environment.IsDevelopment())
        {
            response.Error.Details = exception.Message;
            response.Error.StackTrace = exception.StackTrace;
        }

        var jsonOptions = LLMProxy.Infrastructure.Redis.Common.JsonSerializerOptionsFactory.CreateDefault();
        jsonOptions.WriteIndented = _environment.IsDevelopment();

        var json = JsonSerializer.Serialize(response, jsonOptions);

        await context.Response.WriteAsync(json);
    }
}
