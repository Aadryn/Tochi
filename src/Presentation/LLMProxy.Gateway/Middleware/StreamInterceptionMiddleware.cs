using System.Text;
using System.Text.Json;
using LLMProxy.Domain.Interfaces;
using LLMProxy.Domain.Entities;
using LLMProxy.Gateway.Extensions;
using LLMProxy.Infrastructure.Security;
using Microsoft.Extensions.Configuration;

namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Middleware pour intercepter les réponses streaming afin d'extraire les métriques et transformer le contenu.
/// Supporte le format Server-Sent Events (SSE) utilisé par OpenAI et APIs similaires.
/// </summary>
public class StreamInterceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<StreamInterceptionMiddleware> _logger;
    private readonly ITokenCounterService _tokenCounter;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly long _maxStreamSizeBytes;

    /// <summary>
    /// Initialise une nouvelle instance du middleware d'interception de streaming.
    /// </summary>
    /// <param name="next">Délégué vers le prochain middleware dans le pipeline.</param>
    /// <param name="logger">Logger pour enregistrer les événements.</param>
    /// <param name="tokenCounter">Service de comptage de tokens.</param>
    /// <param name="serviceScopeFactory">Factory pour créer des scopes de services.</param>
    /// <param name="configuration">Configuration de l'application.</param>
    public StreamInterceptionMiddleware(
        RequestDelegate next, 
        ILogger<StreamInterceptionMiddleware> logger,
        ITokenCounterService tokenCounter,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _tokenCounter = tokenCounter;
        _serviceScopeFactory = serviceScopeFactory;
        
        // Lire la limite de taille depuis la configuration (10 MB par défaut)
        _maxStreamSizeBytes = configuration.GetValue<long>("StreamInterception:MaxStreamSizeBytes", 10_485_760);
        
        Guard.AgainstNegativeOrZero((int)_maxStreamSizeBytes, nameof(_maxStreamSizeBytes), 
            "La taille maximale du stream doit être positive");
    }

    /// <summary>
    /// Invoque le middleware pour traiter la requête HTTP.
    /// </summary>
    /// <param name="context">Contexte HTTP de la requête en cours.</param>
    /// <param name="cancellationToken">Token d'annulation pour interrompre l'opération.</param>
    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context, nameof(context));
        
        // Check if this is a streaming request (typically indicated by "stream": true in request body or Accept header)
        var isStreaming = await IsStreamingRequest(context);

        if (!isStreaming)
        {
            await _next(context);
            return;
        }

        _logger.LogStreamingRequestIntercepted(context.Request.Path);

        // Store original response body stream
        var originalBodyStream = context.Response.Body;

        try
        {
            // Create a new memory stream to capture the response with size limit
            using var responseBody = new MemoryStream((int)_maxStreamSizeBytes);
            context.Response.Body = responseBody;

            await _next(context);

            // Vérifier que la taille du stream ne dépasse pas la limite
            if (responseBody.Length > _maxStreamSizeBytes)
            {
                _logger.LogError(
                    "Stream size exceeded limit: {ActualSize} bytes > {MaxSize} bytes for path {Path}",
                    responseBody.Length,
                    _maxStreamSizeBytes,
                    context.Request.Path);
                
                Guard.AgainstResponseStarted(context.Response);
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                await context.Response.WriteAsync($"Response stream size exceeded maximum allowed size of {_maxStreamSizeBytes} bytes");
                return;
            }

            // Reset stream position
            responseBody.Seek(0, SeekOrigin.Begin);

            // Process the streaming response
            await ProcessStreamingResponse(context, responseBody, originalBodyStream);
        }
        finally
        {
            // Restore original stream
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task<bool> IsStreamingRequest(HttpContext context)
    {
        // Check if request body contains "stream": true
        if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
        {
            // Enable buffering so we can read the body multiple times
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // Reset for next middleware

            return body.Contains("\"stream\"") && body.Contains("true");
        }

        return false;
    }

    private async Task ProcessStreamingResponse(HttpContext context, Stream responseStream, Stream originalStream)
    {
        using var reader = new StreamReader(responseStream);
        using var writer = new StreamWriter(originalStream, leaveOpen: true) { AutoFlush = true };

        // var tokenCount = 0;  // Unused - token counting happens server-side
        var chunks = new List<string>();

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            // Parse SSE format: "data: {json}"
            if (line.StartsWith("data: "))
            {
                var data = line.Substring(6);
                
                if (data == "[DONE]")
                {
                    await writer.WriteLineAsync(line);
                    break;
                }

                try
                {
                    // Parse JSON and extract content
                    using var jsonDoc = JsonDocument.Parse(data);
                    var root = jsonDoc.RootElement;
                    
                    // Extract content from choices[0].delta.content or similar structure
                    if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                    {
                        var choice = choices[0];
                        if (choice.TryGetProperty("delta", out var delta) && 
                            delta.TryGetProperty("content", out var content))
                        {
                            var contentText = content.GetString();
                            if (!string.IsNullOrEmpty(contentText))
                            {
                                chunks.Add(contentText);
                            }
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogStreamingChunkParseError(ex, data);
                }
                catch (Exception ex)
                {
                    _logger.LogStreamingChunkProcessingError(ex);
                }
            }

            // Forward the line to the client
            await writer.WriteLineAsync(line);
        }

        // After streaming is complete, log metrics
        var fullResponse = string.Join("", chunks);
        long outputTokens = 0;
        long inputTokens = 0;
        
        try
        {
            // Estimate tokens from collected content
            // Default to "gpt-3.5-turbo" model for token estimation
            var model = "gpt-3.5-turbo";
            outputTokens = _tokenCounter.EstimateTokens(fullResponse, model);
            
            // Try to get input tokens from request if available
            if (context.Items.TryGetValue("RequestBody", out var requestBodyObj) && requestBodyObj is string requestBody)
            {
                inputTokens = _tokenCounter.EstimateTokens(requestBody, model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogStreamingTokenCountError(ex);
        }
        
        _logger.LogInformation(
            "Streaming completed: {Path} | Chunks: {ChunkCount} | InputTokens: {InputTokens} | OutputTokens: {OutputTokens} | RequestId: {RequestId}",
            context.Request.Path,
            chunks.Count,
            inputTokens,
            outputTokens,
            context.Items.TryGetValue("RequestId", out var reqId) ? reqId : "unknown");

        // Save audit log and update token usage metrics
        await SaveMetricsAsync(context, inputTokens, outputTokens, fullResponse);
    }
    
    private async Task SaveMetricsAsync(HttpContext context, long inputTokens, long outputTokens, string responseContent)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            var userId = context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is Guid uid ? uid : (Guid?)null;
            var tenantId = context.Items.TryGetValue("TenantId", out var tenantIdObj) && tenantIdObj is Guid tid ? tid : (Guid?)null;
            var apiKeyId = context.Items.TryGetValue("ApiKeyId", out var apiKeyIdObj) && apiKeyIdObj is Guid akid ? akid : (Guid?)null;
            
            if (tenantId.HasValue && userId.HasValue)
            {
                // Create audit log
                var auditLogResult = AuditLog.Create(
                    tenantId.Value,
                    userId,
                    apiKeyId,
                    null, // providerId - would need to be extracted from routing
                    context.Items.TryGetValue("RequestId", out var reqId) ? reqId?.ToString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString(),
                    context.Request.Path.Value ?? "/",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Response.StatusCode,
                    null, // request body
                    responseContent.Length > 10000 ? responseContent.Substring(0, 10000) : responseContent, // truncate large responses
                    false, // isAnonymized
                    inputTokens,
                    outputTokens,
                    (int)(context.Items.TryGetValue("RequestDuration", out var duration) && duration is int d ? d : 0),
                    context.Connection.RemoteIpAddress?.ToString(),
                    context.Request.Headers["User-Agent"].ToString()
                );
                
                if (auditLogResult.IsSuccess)
                {
                    await unitOfWork.AuditLogs.AddAsync(auditLogResult.Value);
                    await unitOfWork.SaveChangesAsync();
                    
                    // RequestId from HttpContext.Items
                    var requestIdObj = context.Items["RequestId"];
                    var requestId = requestIdObj switch
                    {
                        Guid g => g,
                        string s => Guid.Parse(s),
                        _ => throw new InvalidOperationException("RequestId not found or invalid type")
                    };
                    _logger.LogAuditLogSaved(requestId);
                }
                else
                {
                    _logger.LogAuditLogCreationError(auditLogResult.Error ?? "Unknown error");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogMetricsSaveError(ex);
        }
    }
}
