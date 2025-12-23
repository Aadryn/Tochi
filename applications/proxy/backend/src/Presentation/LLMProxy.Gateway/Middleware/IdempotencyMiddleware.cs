using LLMProxy.Infrastructure.Redis.Idempotency;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Middleware garantissant l'idempotence des requêtes POST et PATCH.
/// Vérifie la présence du header Idempotency-Key et rejoue les réponses déjà traitées.
/// Conforme à ADR-022 (Idempotence).
/// </summary>
public sealed partial class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IIdempotencyStore _store;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    private const string IdempotencyKeyHeader = "Idempotency-Key";
    private static readonly string[] IdempotentMethods = { "POST", "PATCH" };

    /// <summary>
    /// Initialise une nouvelle instance du middleware d'idempotence.
    /// </summary>
    /// <param name="next">Délégué vers le prochain middleware dans le pipeline.</param>
    /// <param name="store">Store Redis pour cacher les réponses idempotentes.</param>
    /// <param name="logger">Logger pour tracer les événements d'idempotence.</param>
    public IdempotencyMiddleware(
        RequestDelegate next,
        IIdempotencyStore store,
        ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _store = store;
        _logger = logger;
    }

    /// <summary>
    /// Traite une requête HTTP en vérifiant l'idempotence.
    /// </summary>
    /// <param name="context">Contexte HTTP de la requête actuelle.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // Ne traiter que POST et PATCH (GET/PUT/DELETE sont naturellement idempotents)
        if (!IdempotentMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Vérifier présence du header Idempotency-Key
        if (!context.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var keyHeader))
        {
            LogIdempotencyKeyMissing(_logger, context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "idempotency_key_required",
                message = $"Header '{IdempotencyKeyHeader}' is required for {context.Request.Method} requests"
            });
            return;
        }

        var idempotencyKey = keyHeader.ToString();

        // Vérifier si la requête a déjà été traitée (cache hit)
        var cachedResponse = await _store.GetAsync(idempotencyKey, context.RequestAborted);
        if (cachedResponse is not null)
        {
            // Rejouer la réponse cachée (idempotence)
            LogIdempotencyReplay(_logger, idempotencyKey, cachedResponse.StatusCode);
            context.Response.StatusCode = cachedResponse.StatusCode;
            context.Response.ContentType = cachedResponse.ContentType;
            await context.Response.WriteAsync(cachedResponse.Body, context.RequestAborted);
            return;
        }

        // Capturer la réponse originale pour la mettre en cache
        var originalBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        // Exécuter le reste du pipeline HTTP
        await _next(context);

        // Lire la réponse générée
        memoryStream.Position = 0;
        var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

        // Créer l'objet réponse cachée
        var response = new CachedResponse(
            context.Response.StatusCode,
            context.Response.ContentType ?? "application/json",
            responseBody,
            DateTime.UtcNow
        );

        // Stocker dans Redis avec TTL de 24h
        await _store.SetAsync(idempotencyKey, response, TimeSpan.FromHours(24), context.RequestAborted);

        LogIdempotencyCached(_logger, idempotencyKey, context.Response.StatusCode);

        // Écrire la réponse originale au client
        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(originalBody, context.RequestAborted);
        context.Response.Body = originalBody;
    }

    /// <summary>
    /// Log une requête POST/PATCH sans header Idempotency-Key (erreur 400).
    /// EventId 6001 - Niveau Warning.
    /// </summary>
    [LoggerMessage(
        EventId = 6001,
        Level = LogLevel.Warning,
        Message = "Idempotency-Key header missing for {Method} {Path}")]
    private static partial void LogIdempotencyKeyMissing(ILogger logger, string method, string path);

    /// <summary>
    /// Log le rejeu d'une réponse cachée (requête déjà traitée).
    /// EventId 6002 - Niveau Information.
    /// </summary>
    [LoggerMessage(
        EventId = 6002,
        Level = LogLevel.Information,
        Message = "Idempotency replay for key {IdempotencyKey} - Status {StatusCode}")]
    private static partial void LogIdempotencyReplay(ILogger logger, string idempotencyKey, int statusCode);

    /// <summary>
    /// Log la mise en cache d'une nouvelle réponse idempotente.
    /// EventId 6003 - Niveau Debug.
    /// </summary>
    [LoggerMessage(
        EventId = 6003,
        Level = LogLevel.Debug,
        Message = "Idempotency response cached for key {IdempotencyKey} - Status {StatusCode}")]
    private static partial void LogIdempotencyCached(ILogger logger, string idempotencyKey, int statusCode);
}
