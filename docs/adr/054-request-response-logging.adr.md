# 54. Request/Response Logging pour l'observabilité

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM traite des requêtes sensibles nécessitant un audit complet. Sans logging approprié :
- Impossible de debugger les problèmes
- Pas de traçabilité pour compliance
- Difficulté à analyser les patterns d'usage
- Pas de preuves en cas de litige

```csharp
// ❌ SANS LOGGING : Aucune traçabilité
[HttpPost]
public async Task<IActionResult> Chat(ChatRequest request)
{
    // Pas de log de la requête entrante
    var result = await _service.ProcessAsync(request);
    // Pas de log de la réponse
    return Ok(result);
    // Si erreur → Impossible de savoir ce qui s'est passé
}

// ❌ LOGGING DANGEREUX : Données sensibles exposées
_logger.LogInformation("Request: {@Request}", request); // Log le contenu complet !
_logger.LogInformation("API Key: {ApiKey}", apiKey);    // Log la clé API !
```

## Décision

**Implémenter un logging structuré des requêtes/réponses avec masquage des données sensibles.**

### 1. Middleware de Request/Response Logging

```csharp
/// <summary>
/// Middleware de logging des requêtes et réponses HTTP.
/// </summary>
public sealed class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly RequestLoggingOptions _options;
    private readonly ISensitiveDataMasker _masker;
    
    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger,
        IOptions<RequestLoggingOptions> options,
        ISensitiveDataMasker masker)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
        _masker = masker;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Générer ou récupérer le Request ID
        var requestId = context.Request.Headers["X-Request-Id"].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");
        context.Items["RequestId"] = requestId;
        context.Response.Headers["X-Request-Id"] = requestId;
        
        var stopwatch = Stopwatch.StartNew();
        
        // Capturer le body de la requête si configuré
        string? requestBody = null;
        if (_options.LogRequestBody && ShouldLogBody(context.Request))
        {
            requestBody = await CaptureRequestBodyAsync(context.Request);
        }
        
        // Log de début de requête
        LogRequestStart(context, requestId, requestBody);
        
        // Capturer la réponse
        var originalBodyStream = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        
        Exception? exception = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            
            // Capturer le body de la réponse si configuré
            string? responseBodyContent = null;
            if (_options.LogResponseBody && ShouldLogBody(context.Response))
            {
                responseBodyContent = await CaptureResponseBodyAsync(responseBody);
            }
            
            // Log de fin de requête
            LogRequestEnd(context, requestId, stopwatch.Elapsed, responseBodyContent, exception);
            
            // Copier la réponse vers le stream original
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
    
    private void LogRequestStart(HttpContext context, string requestId, string? body)
    {
        var request = context.Request;
        
        // Masquer les headers sensibles
        var maskedHeaders = MaskSensitiveHeaders(request.Headers);
        
        _logger.LogInformation(
            "HTTP {Method} {Path} started. " +
            "RequestId: {RequestId}, " +
            "ClientIP: {ClientIP}, " +
            "UserAgent: {UserAgent}, " +
            "ContentLength: {ContentLength}",
            request.Method,
            request.Path + request.QueryString,
            requestId,
            GetClientIp(context),
            request.Headers.UserAgent.ToString(),
            request.ContentLength);
        
        if (body is not null && _options.LogRequestBody)
        {
            var maskedBody = _masker.MaskJson(body);
            _logger.LogDebug(
                "Request body for {RequestId}: {Body}",
                requestId,
                maskedBody);
        }
    }
    
    private void LogRequestEnd(
        HttpContext context,
        string requestId,
        TimeSpan duration,
        string? responseBody,
        Exception? exception)
    {
        var statusCode = context.Response.StatusCode;
        var logLevel = GetLogLevel(statusCode, exception);
        
        if (exception is not null)
        {
            _logger.Log(logLevel,
                exception,
                "HTTP {Method} {Path} failed. " +
                "RequestId: {RequestId}, " +
                "StatusCode: {StatusCode}, " +
                "Duration: {Duration}ms, " +
                "Error: {ErrorMessage}",
                context.Request.Method,
                context.Request.Path,
                requestId,
                statusCode,
                duration.TotalMilliseconds,
                exception.Message);
        }
        else
        {
            _logger.Log(logLevel,
                "HTTP {Method} {Path} completed. " +
                "RequestId: {RequestId}, " +
                "StatusCode: {StatusCode}, " +
                "Duration: {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                requestId,
                statusCode,
                duration.TotalMilliseconds);
        }
        
        if (responseBody is not null && _options.LogResponseBody)
        {
            var maskedBody = _masker.MaskJson(responseBody);
            _logger.LogDebug(
                "Response body for {RequestId}: {Body}",
                requestId,
                TruncateIfNeeded(maskedBody));
        }
    }
    
    private static LogLevel GetLogLevel(int statusCode, Exception? exception)
    {
        return (statusCode, exception) switch
        {
            (_, not null) => LogLevel.Error,
            (>= 500, _) => LogLevel.Error,
            (>= 400, _) => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }
    
    private Dictionary<string, string> MaskSensitiveHeaders(IHeaderDictionary headers)
    {
        var result = new Dictionary<string, string>();
        
        foreach (var header in headers)
        {
            var key = header.Key.ToLowerInvariant();
            var value = _options.SensitiveHeaders.Contains(key)
                ? _masker.MaskValue(header.Value.ToString())
                : header.Value.ToString();
            
            result[header.Key] = value;
        }
        
        return result;
    }
    
    private async Task<string> CaptureRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        
        using var reader = new StreamReader(
            request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        
        return TruncateIfNeeded(body);
    }
    
    private async Task<string> CaptureResponseBodyAsync(MemoryStream responseBody)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseBody, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);
        
        return body;
    }
    
    private string TruncateIfNeeded(string content)
    {
        if (content.Length <= _options.MaxBodyLogLength)
            return content;
        
        return content[.._options.MaxBodyLogLength] + $"... [truncated, total: {content.Length} chars]";
    }
    
    private bool ShouldLogBody(HttpRequest request)
    {
        if (_options.ExcludedPaths.Any(p => request.Path.StartsWithSegments(p)))
            return false;
        
        var contentType = request.ContentType?.ToLowerInvariant() ?? "";
        return contentType.Contains("json") || contentType.Contains("text");
    }
    
    private bool ShouldLogBody(HttpResponse response)
    {
        var contentType = response.ContentType?.ToLowerInvariant() ?? "";
        return contentType.Contains("json") || contentType.Contains("text");
    }
    
    private static string GetClientIp(HttpContext context)
    {
        // Vérifier les headers de proxy
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }
        
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
```

### 2. Service de Masquage des Données Sensibles

```csharp
/// <summary>
/// Service de masquage des données sensibles.
/// </summary>
public interface ISensitiveDataMasker
{
    string MaskValue(string value);
    string MaskJson(string json);
}

public sealed class SensitiveDataMasker : ISensitiveDataMasker
{
    private readonly SensitiveDataOptions _options;
    private static readonly Regex ApiKeyRegex = new(
        @"sk-[a-zA-Z0-9]{20,}",
        RegexOptions.Compiled);
    private static readonly Regex JwtRegex = new(
        @"eyJ[a-zA-Z0-9_-]+\.eyJ[a-zA-Z0-9_-]+\.[a-zA-Z0-9_-]+",
        RegexOptions.Compiled);
    
    public SensitiveDataMasker(IOptions<SensitiveDataOptions> options)
    {
        _options = options.Value;
    }
    
    public string MaskValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        
        if (value.Length <= 8)
            return "***";
        
        // Garder les 4 premiers et derniers caractères
        return $"{value[..4]}...{value[^4..]}";
    }
    
    public string MaskJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return json;
        
        try
        {
            var doc = JsonDocument.Parse(json);
            var masked = MaskJsonElement(doc.RootElement);
            return JsonSerializer.Serialize(masked, new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }
        catch
        {
            // Si pas du JSON valide, masquer les patterns sensibles
            return MaskSensitivePatterns(json);
        }
    }
    
    private object? MaskJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => MaskJsonObject(element),
            JsonValueKind.Array => MaskJsonArray(element),
            JsonValueKind.String => MaskStringValue(element.GetString()),
            _ => element.Clone()
        };
    }
    
    private Dictionary<string, object?> MaskJsonObject(JsonElement obj)
    {
        var result = new Dictionary<string, object?>();
        
        foreach (var property in obj.EnumerateObject())
        {
            var key = property.Name.ToLowerInvariant();
            
            if (_options.SensitiveFields.Contains(key))
            {
                // Masquer complètement les champs sensibles
                result[property.Name] = "***MASKED***";
            }
            else if (_options.PartiallyMaskedFields.Contains(key))
            {
                // Masquage partiel
                var value = property.Value.ValueKind == JsonValueKind.String
                    ? MaskValue(property.Value.GetString() ?? "")
                    : "***";
                result[property.Name] = value;
            }
            else
            {
                result[property.Name] = MaskJsonElement(property.Value);
            }
        }
        
        return result;
    }
    
    private List<object?> MaskJsonArray(JsonElement array)
    {
        return array.EnumerateArray()
            .Select(MaskJsonElement)
            .ToList();
    }
    
    private string MaskStringValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? "";
        
        // Masquer les API keys
        value = ApiKeyRegex.Replace(value, m => MaskValue(m.Value));
        
        // Masquer les JWT
        value = JwtRegex.Replace(value, m => MaskValue(m.Value));
        
        return value;
    }
    
    private string MaskSensitivePatterns(string text)
    {
        text = ApiKeyRegex.Replace(text, m => MaskValue(m.Value));
        text = JwtRegex.Replace(text, m => MaskValue(m.Value));
        return text;
    }
}

/// <summary>
/// Options de masquage.
/// </summary>
public sealed class SensitiveDataOptions
{
    public const string SectionName = "SensitiveData";
    
    /// <summary>
    /// Champs à masquer complètement.
    /// </summary>
    public HashSet<string> SensitiveFields { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "secret",
        "apikey",
        "api_key",
        "authorization",
        "token",
        "accesstoken",
        "access_token",
        "refreshtoken",
        "refresh_token",
        "private_key",
        "privatekey",
        "credit_card",
        "creditcard",
        "cvv",
        "ssn"
    };
    
    /// <summary>
    /// Champs à masquer partiellement.
    /// </summary>
    public HashSet<string> PartiallyMaskedFields { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "email",
        "phone",
        "x-api-key"
    };
}
```

### 3. Options de Configuration

```csharp
/// <summary>
/// Options de logging des requêtes.
/// </summary>
public sealed class RequestLoggingOptions
{
    public const string SectionName = "RequestLogging";
    
    /// <summary>
    /// Logger le body des requêtes.
    /// </summary>
    public bool LogRequestBody { get; set; } = true;
    
    /// <summary>
    /// Logger le body des réponses.
    /// </summary>
    public bool LogResponseBody { get; set; } = true;
    
    /// <summary>
    /// Taille maximale du body à logger.
    /// </summary>
    public int MaxBodyLogLength { get; set; } = 10_000;
    
    /// <summary>
    /// Paths à exclure du logging détaillé.
    /// </summary>
    public HashSet<string> ExcludedPaths { get; set; } = new()
    {
        "/health",
        "/ready",
        "/metrics",
        "/_internal"
    };
    
    /// <summary>
    /// Headers sensibles à masquer.
    /// </summary>
    public HashSet<string> SensitiveHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "authorization",
        "x-api-key",
        "cookie",
        "set-cookie"
    };
}
```

### 4. Logging Structuré avec Enrichissement

```csharp
/// <summary>
/// Enrichisseur de logs avec contexte de requête.
/// </summary>
public sealed class RequestContextEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return;
        
        var requestId = context.Items["RequestId"]?.ToString();
        if (requestId is not null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("RequestId", requestId));
        }
        
        var tenantId = context.Items["TenantId"]?.ToString();
        if (tenantId is not null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("TenantId", tenantId));
        }
        
        var userId = context.User?.FindFirst("sub")?.Value;
        if (userId is not null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("UserId", userId));
        }
        
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("Path", context.Request.Path.ToString()));
        
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("Method", context.Request.Method));
    }
}

/// <summary>
/// Configuration Serilog avec enrichissement.
/// </summary>
public static class LoggingConfiguration
{
    public static IHostBuilder UseStructuredLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.With<RequestContextEnricher>()
                .WriteTo.Console(new JsonFormatter())
                .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341");
        });
    }
}
```

### 5. Audit Trail pour Compliance

```csharp
/// <summary>
/// Service d'audit trail pour les opérations sensibles.
/// </summary>
public interface IAuditService
{
    Task LogAuditEventAsync(AuditEvent auditEvent, CancellationToken ct = default);
}

public sealed class AuditService : IAuditService
{
    private readonly IAuditRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TimeProvider _timeProvider;
    
    public async Task LogAuditEventAsync(AuditEvent auditEvent, CancellationToken ct = default)
    {
        var context = _httpContextAccessor.HttpContext;
        
        auditEvent = auditEvent with
        {
            Timestamp = _timeProvider.GetUtcNow(),
            RequestId = context?.Items["RequestId"]?.ToString(),
            IpAddress = GetClientIp(context),
            UserAgent = context?.Request.Headers.UserAgent.ToString()
        };
        
        await _repository.SaveAsync(auditEvent, ct);
    }
}

/// <summary>
/// Événement d'audit.
/// </summary>
public sealed record AuditEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; init; }
    public required string Action { get; init; }
    public required string Resource { get; init; }
    public Guid? ResourceId { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? UserId { get; init; }
    public string? RequestId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public AuditResult Result { get; init; }
    public Dictionary<string, object>? Details { get; init; }
}

public enum AuditResult
{
    Success,
    Failure,
    Denied
}

/// <summary>
/// Attribut pour auditer automatiquement les actions.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AuditAttribute : Attribute, IAsyncActionFilter
{
    public required string Action { get; init; }
    public required string Resource { get; init; }
    
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var auditService = context.HttpContext.RequestServices
            .GetRequiredService<IAuditService>();
        
        var resourceId = context.ActionArguments
            .FirstOrDefault(a => a.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
            .Value as Guid?;
        
        var executedContext = await next();
        
        var result = executedContext.Exception is null
            ? AuditResult.Success
            : AuditResult.Failure;
        
        await auditService.LogAuditEventAsync(new AuditEvent
        {
            Action = Action,
            Resource = Resource,
            ResourceId = resourceId,
            Result = result,
            Details = new Dictionary<string, object>
            {
                ["StatusCode"] = context.HttpContext.Response.StatusCode
            }
        });
    }
}

/// <summary>
/// Exemple d'utilisation.
/// </summary>
[ApiController]
[Route("admin/api-keys")]
public class ApiKeyAdminController : ControllerBase
{
    [HttpPost]
    [Audit(Action = "Create", Resource = "ApiKey")]
    public async Task<IActionResult> Create(CreateApiKeyRequest request)
    {
        // ...
    }
    
    [HttpDelete("{id}")]
    [Audit(Action = "Delete", Resource = "ApiKey")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // ...
    }
}
```

### 6. Configuration appsettings.json

```json
{
  "RequestLogging": {
    "LogRequestBody": true,
    "LogResponseBody": true,
    "MaxBodyLogLength": 10000,
    "ExcludedPaths": ["/health", "/ready", "/metrics"],
    "SensitiveHeaders": ["authorization", "x-api-key", "cookie"]
  },
  "SensitiveData": {
    "SensitiveFields": ["password", "secret", "apikey", "token"],
    "PartiallyMaskedFields": ["email", "phone"]
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

## Conséquences

### Positives

- **Observabilité** : Visibilité complète sur le trafic
- **Compliance** : Audit trail pour réglementations
- **Debug** : Reproduction facilitée des problèmes
- **Sécurité** : Données sensibles protégées

### Négatives

- **Performance** : Overhead de capture des bodies
  - *Mitigation* : Limiter la taille, exclure certains paths
- **Stockage** : Volume de logs important
  - *Mitigation* : Rétention courte, sampling

### Neutres

- Standard observabilité
- Compatible avec tous les outils de log

## Alternatives considérées

### Option A : Logging applicatif uniquement

- **Description** : Logs dans le code métier
- **Avantages** : Contrôle total
- **Inconvénients** : Oublis, inconsistances
- **Raison du rejet** : Pas systématique

### Option B : Logging au niveau reverse proxy

- **Description** : nginx/traefik logs
- **Avantages** : Pas de code
- **Inconvénients** : Pas d'accès au contexte applicatif
- **Raison du rejet** : Insuffisant

## Références

- [Serilog](https://serilog.net/)
- [ASP.NET Core Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
- [GDPR Logging Requirements](https://gdpr-info.eu/)
