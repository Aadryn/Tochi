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

        var chunks = await ProcessAndForwardStreamLines(reader, writer);
        var (inputTokens, outputTokens) = await EstimateTokenCounts(context, chunks);
        
        LogStreamingCompletion(context, chunks, inputTokens, outputTokens);
        await SaveMetricsAsync(context, inputTokens, outputTokens, string.Join("", chunks));
    }

    private async Task<List<string>> ProcessAndForwardStreamLines(StreamReader reader, StreamWriter writer)
    {
        var chunks = new List<string>();
        string? line;
        
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: "))
            {
                var content = ExtractContentFromStreamLine(line);
                if (content == "[DONE]")
                {
                    await writer.WriteLineAsync(line);
                    break;
                }
                
                if (!string.IsNullOrEmpty(content))
                {
                    chunks.Add(content);
                }
            }

            await writer.WriteLineAsync(line);
        }
        
        return chunks;
    }

    private string? ExtractContentFromStreamLine(string line)
    {
        var data = line.Substring(6);
        
        if (data == "[DONE]")
        {
            return "[DONE]";
        }

        try
        {
            using var jsonDoc = JsonDocument.Parse(data);
            var root = jsonDoc.RootElement;
            
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var choice = choices[0];
                if (choice.TryGetProperty("delta", out var delta) && 
                    delta.TryGetProperty("content", out var content))
                {
                    return content.GetString();
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
        
        return null;
    }

    private Task<(long inputTokens, long outputTokens)> EstimateTokenCounts(HttpContext context, List<string> chunks)
    {
        long inputTokens = 0;
        long outputTokens = 0;
        const string defaultModel = "gpt-3.5-turbo";
        
        try
        {
            var fullResponse = string.Join("", chunks);
            outputTokens = _tokenCounter.EstimateTokens(fullResponse, defaultModel);
            
            if (context.Items.TryGetValue("RequestBody", out var requestBodyObj) && requestBodyObj is string requestBody)
            {
                inputTokens = _tokenCounter.EstimateTokens(requestBody, defaultModel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogStreamingTokenCountError(ex);
        }
        
        return Task.FromResult((inputTokens, outputTokens));
    }

    private void LogStreamingCompletion(HttpContext context, List<string> chunks, long inputTokens, long outputTokens)
    {
        _logger.LogInformation(
            "Streaming completed: {Path} | Chunks: {ChunkCount} | InputTokens: {InputTokens} | OutputTokens: {OutputTokens} | RequestId: {RequestId}",
            context.Request.Path,
            chunks.Count,
            inputTokens,
            outputTokens,
            context.Items.TryGetValue("RequestId", out var reqId) ? reqId : "unknown");
    }
    
    private async Task SaveMetricsAsync(HttpContext context, long inputTokens, long outputTokens, string responseContent)
    {
        try
        {
            var (userId, tenantId, apiKeyId) = ExtractContextIds(context);
            
            if (!tenantId.HasValue || !userId.HasValue)
            {
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            var auditLogResult = CreateAuditLog(context, tenantId.Value, userId, apiKeyId, inputTokens, outputTokens, responseContent);
            
            if (auditLogResult.IsSuccess)
            {
                await unitOfWork.AuditLogs.AddAsync(auditLogResult.Value);
                await unitOfWork.SaveChangesAsync();
                
                var requestId = GetRequestId(context);
                _logger.LogAuditLogSaved(requestId);
            }
            else
            {
                _logger.LogAuditLogCreationError(auditLogResult.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogMetricsSaveError(ex);
        }
    }

    private (Guid? userId, Guid? tenantId, Guid? apiKeyId) ExtractContextIds(HttpContext context)
    {
        var userId = context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is Guid uid ? uid : (Guid?)null;
        var tenantId = context.Items.TryGetValue("TenantId", out var tenantIdObj) && tenantIdObj is Guid tid ? tid : (Guid?)null;
        var apiKeyId = context.Items.TryGetValue("ApiKeyId", out var apiKeyIdObj) && apiKeyIdObj is Guid akid ? akid : (Guid?)null;
        
        return (userId, tenantId, apiKeyId);
    }

    private Domain.Common.Result<AuditLog> CreateAuditLog(HttpContext context, Guid tenantId, Guid? userId, Guid? apiKeyId, 
        long inputTokens, long outputTokens, string responseContent)
    {
        var requestId = context.Items.TryGetValue("RequestId", out var reqId) 
            ? reqId?.ToString() ?? Guid.NewGuid().ToString() 
            : Guid.NewGuid().ToString();
        
        var truncatedResponse = responseContent.Length > 10000 
            ? responseContent.Substring(0, 10000) 
            : responseContent;
        
        var duration = context.Items.TryGetValue("RequestDuration", out var durationObj) && durationObj is int d 
            ? d 
            : 0;

        return AuditLog.Create(
            tenantId,
            userId,
            apiKeyId,
            null,
            requestId,
            context.Request.Path.Value ?? "/",
            context.Request.Method,
            context.Request.Path.Value,
            context.Response.StatusCode,
            null,
            truncatedResponse,
            false,
            inputTokens,
            outputTokens,
            duration,
            context.Connection.RemoteIpAddress?.ToString(),
            context.Request.Headers["User-Agent"].ToString()
        );
    }

    private Guid GetRequestId(HttpContext context)
    {
        var requestIdObj = context.Items["RequestId"];
        return requestIdObj switch
        {
            Guid g => g,
            string s => Guid.Parse(s),
            _ => throw new InvalidOperationException("RequestId not found or invalid type")
        };
    }
}
