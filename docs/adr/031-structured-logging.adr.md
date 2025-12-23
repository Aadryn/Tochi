# 31. Structured Logging

Date: 2025-12-21

## Statut

Accepté

## Contexte

Le logging traditionnel avec string concatenation pose problème :
- **Recherche difficile** : Grep sur du texte brut
- **Pas de structure** : Impossible de filtrer par propriété
- **Performance** : String interpolation même si niveau désactivé
- **Corrélation** : Difficile de suivre une requête

```csharp
// ❌ LOGGING NON STRUCTURÉ : String concatenation
_logger.LogInformation($"Processing request {requestId} for tenant {tenantId}");
_logger.LogError($"Failed to process request: " + exception.Message);

// Résultat dans les logs :
// "Processing request 123e4567-e89b-12d3-a456-426614174000 for tenant abc..."
// Impossible de filtrer par tenantId !
```

## Décision

**Utiliser le logging structuré avec des paramètres nommés pour permettre l'analyse, la recherche et la corrélation des logs.**

### 1. Logging structuré de base

```csharp
/// <summary>
/// Exemples de logging structuré correct.
/// </summary>
public class LlmRequestService
{
    private readonly ILogger<LlmRequestService> _logger;
    
    // ✅ STRUCTURÉ : Paramètres nommés entre {}
    public async Task<LlmResponse> ProcessRequestAsync(
        LlmRequest request, 
        CancellationToken ct)
    {
        // Les propriétés sont indexables et searchables
        _logger.LogInformation(
            "Processing LLM request {RequestId} for tenant {TenantId} using model {Model}",
            request.Id,
            request.TenantId,
            request.Model);
        
        try
        {
            var response = await _provider.SendAsync(request, ct);
            
            _logger.LogInformation(
                "LLM request {RequestId} completed in {DurationMs}ms with {TokenCount} tokens",
                request.Id,
                response.DurationMs,
                response.TotalTokens);
            
            return response;
        }
        catch (Exception ex)
        {
            // L'exception est passée en premier paramètre
            _logger.LogError(
                ex,
                "LLM request {RequestId} failed for tenant {TenantId}: {ErrorMessage}",
                request.Id,
                request.TenantId,
                ex.Message);
            
            throw;
        }
    }
}
```

### 2. Message Templates constants

```csharp
/// <summary>
/// Templates de messages centralisés (évite typos, facilite la recherche).
/// </summary>
public static class LogMessages
{
    // Tenant
    public const string TenantCreated = 
        "Tenant {TenantId} created with name {TenantName}";
    public const string TenantActivated = 
        "Tenant {TenantId} activated from status {PreviousStatus}";
    public const string TenantDeactivated = 
        "Tenant {TenantId} deactivated: {Reason}";
    public const string TenantQuotaExceeded = 
        "Tenant {TenantId} exceeded quota: {CurrentUsage}/{MonthlyQuota}";
    
    // API Key
    public const string ApiKeyValidated = 
        "API key {ApiKeyId} validated for tenant {TenantId}";
    public const string ApiKeyValidationFailed = 
        "API key validation failed: {Reason}";
    public const string ApiKeyExpired = 
        "API key {ApiKeyId} has expired at {ExpirationDate}";
    
    // LLM Request
    public const string LlmRequestStarted = 
        "LLM request {RequestId} started for tenant {TenantId} using model {Model}";
    public const string LlmRequestCompleted = 
        "LLM request {RequestId} completed in {DurationMs}ms ({InputTokens} in, {OutputTokens} out)";
    public const string LlmRequestFailed = 
        "LLM request {RequestId} failed: {ErrorType} - {ErrorMessage}";
    
    // Performance
    public const string SlowOperation = 
        "Slow operation detected: {OperationName} took {DurationMs}ms (threshold: {ThresholdMs}ms)";
}

// Utilisation
_logger.LogInformation(
    LogMessages.TenantCreated,
    tenant.Id,
    tenant.Name);
```

### 3. Scopes pour corrélation

```csharp
/// <summary>
/// Utilisation des scopes pour corréler les logs.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Générer ou récupérer le correlation ID
        var correlationId = context.Request.Headers["X-Correlation-ID"]
            .FirstOrDefault() ?? Guid.NewGuid().ToString();
        
        // Extraire le tenant ID du token
        var tenantId = context.User.FindFirstValue("tenant_id");
        
        // Créer un scope qui ajoute ces propriétés à TOUS les logs
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["TenantId"] = tenantId ?? "anonymous",
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method
        }))
        {
            _logger.LogInformation("Request started");
            
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _next(context);
                
                _logger.LogInformation(
                    "Request completed with status {StatusCode} in {DurationMs}ms",
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Request failed after {DurationMs}ms",
                    stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }
    }
}
```

### 4. High-performance logging avec LoggerMessage

```csharp
/// <summary>
/// Logging haute performance avec source generators.
/// </summary>
public static partial class LoggerExtensions
{
    // Source generator crée le code optimisé
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Tenant {TenantId} created with name {TenantName}")]
    public static partial void LogTenantCreated(
        this ILogger logger,
        Guid tenantId,
        string tenantName);
    
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Tenant {TenantId} quota usage at {UsagePercent}%")]
    public static partial void LogTenantQuotaWarning(
        this ILogger logger,
        Guid tenantId,
        int usagePercent);
    
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "LLM request {RequestId} failed: {ErrorMessage}")]
    public static partial void LogLlmRequestFailed(
        this ILogger logger,
        Exception exception,
        Guid requestId,
        string errorMessage);
    
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Cache {CacheOperation} for key {CacheKey}: {Result}")]
    public static partial void LogCacheOperation(
        this ILogger logger,
        string cacheOperation,
        string cacheKey,
        string result);
}

// Utilisation (zéro allocation si niveau désactivé)
_logger.LogTenantCreated(tenant.Id, tenant.Name);
_logger.LogLlmRequestFailed(ex, request.Id, ex.Message);
```

### 5. Enrichissement contextuel

```csharp
/// <summary>
/// Enrichir les logs avec le contexte applicatif.
/// </summary>
public class TenantContextEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public Dictionary<string, object> GetEnrichmentProperties()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext is null)
            return new Dictionary<string, object>();
        
        var properties = new Dictionary<string, object>
        {
            ["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            ["MachineName"] = Environment.MachineName,
            ["ThreadId"] = Environment.CurrentManagedThreadId
        };
        
        // Ajouter infos utilisateur si authentifié
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            properties["UserId"] = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
            properties["TenantId"] = httpContext.User.FindFirstValue("tenant_id") ?? "Unknown";
        }
        
        // Ajouter infos request
        properties["ClientIP"] = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        properties["UserAgent"] = httpContext.Request.Headers.UserAgent.ToString();
        
        return properties;
    }
}

// Configuration Serilog avec enrichissement
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "LLMProxy.Gateway")
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();
```

### 6. Log Levels appropriés

```csharp
/// <summary>
/// Guide d'utilisation des log levels.
/// </summary>
public class LogLevelGuidelines
{
    private readonly ILogger<LogLevelGuidelines> _logger;
    
    public void DemonstrateLogLevels()
    {
        // TRACE : Diagnostic très détaillé (données sensibles OK en dev)
        _logger.LogTrace(
            "Token validation details: {TokenPayload}",
            tokenPayload);
        
        // DEBUG : Informations utiles pour le debugging
        _logger.LogDebug(
            "Cache lookup for key {CacheKey} returned {Result}",
            key, result);
        
        // INFORMATION : Événements normaux du flux applicatif
        _logger.LogInformation(
            "Tenant {TenantId} created with name {TenantName}",
            tenant.Id, tenant.Name);
        
        // WARNING : Situation anormale mais gérée
        _logger.LogWarning(
            "Tenant {TenantId} quota at {UsagePercent}% - approaching limit",
            tenantId, usagePercent);
        
        // ERROR : Erreur qui empêche une opération
        _logger.LogError(
            ex,
            "Failed to process LLM request {RequestId}",
            requestId);
        
        // CRITICAL : Erreur système critique (app down)
        _logger.LogCritical(
            ex,
            "Database connection lost - application shutting down");
    }
}
```

### 7. Logging d'opérations avec timing

```csharp
/// <summary>
/// Helper pour logger les opérations avec timing.
/// </summary>
public class OperationLogger
{
    private readonly ILogger _logger;
    
    public async Task<T> LogOperationAsync<T>(
        string operationName,
        Func<Task<T>> operation,
        object? context = null)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogDebug(
            "Starting operation {OperationName} with context {@Context}",
            operationName,
            context);
        
        try
        {
            var result = await operation();
            
            stopwatch.Stop();
            
            _logger.LogInformation(
                "Operation {OperationName} completed in {DurationMs}ms",
                operationName,
                stopwatch.ElapsedMilliseconds);
            
            // Warning si trop lent
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning(
                    LogMessages.SlowOperation,
                    operationName,
                    stopwatch.ElapsedMilliseconds,
                    1000);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(
                ex,
                "Operation {OperationName} failed after {DurationMs}ms",
                operationName,
                stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }
}

// Utilisation
var result = await _operationLogger.LogOperationAsync(
    "GetTenantWithApiKeys",
    () => _repository.GetTenantWithApiKeysAsync(tenantId, ct),
    new { TenantId = tenantId });
```

### 8. Configuration des logs par environnement

```csharp
// appsettings.json (Production)
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning",
        "LLMProxy": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq:5341"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithEnvironmentName"],
    "Properties": {
      "Application": "LLMProxy.Gateway"
    }
  }
}

// appsettings.Development.json (plus verbeux)
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}      {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

### 9. Éviter les pièges courants

```csharp
public class LoggingBestPractices
{
    private readonly ILogger<LoggingBestPractices> _logger;
    
    public void CommonMistakes()
    {
        var tenantId = Guid.NewGuid();
        var tenantName = "Acme Corp";
        
        // ❌ ÉVITER : String interpolation (allocation inutile si niveau désactivé)
        _logger.LogDebug($"Tenant {tenantId} created: {tenantName}");
        
        // ✅ CORRECT : Message template
        _logger.LogDebug("Tenant {TenantId} created: {TenantName}", tenantId, tenantName);
        
        // ❌ ÉVITER : ToString() explicite
        _logger.LogInformation("Processing tenant {TenantId}", tenantId.ToString());
        
        // ✅ CORRECT : Laisser le formateur gérer
        _logger.LogInformation("Processing tenant {TenantId}", tenantId);
        
        // ❌ ÉVITER : Concaténation
        _logger.LogError("Error: " + ex.Message);
        
        // ✅ CORRECT : Exception en premier paramètre
        _logger.LogError(ex, "Error processing request");
        
        // ❌ ÉVITER : Logger des données sensibles
        _logger.LogDebug("API key: {ApiKey}", apiKey);
        
        // ✅ CORRECT : Masquer les données sensibles
        _logger.LogDebug("API key validated: {ApiKeyPrefix}****", apiKey[..8]);
        
        // ❌ ÉVITER : @ pour les objets simples
        _logger.LogInformation("Tenant {@TenantId}", tenantId); // Guid n'a pas besoin de @
        
        // ✅ CORRECT : @ pour les objets complexes (sérialisation)
        _logger.LogInformation("Request details: {@Request}", new { request.Model, request.TenantId });
    }
}
```

## Conséquences

### Positives

- **Recherche** : Filtrer par TenantId, RequestId, etc.
- **Corrélation** : Suivre une requête à travers les services
- **Performance** : Pas d'allocation si niveau désactivé
- **Analyse** : Métriques et alertes sur les propriétés
- **Debugging** : Contexte riche pour investigation

### Négatives

- **Verbosité** : Plus de paramètres à passer
  - *Mitigation* : LoggerMessage source generator
- **Discipline** : Équipe doit suivre les conventions
  - *Mitigation* : Code review, analyseurs statiques

### Neutres

- Le structured logging est le standard moderne pour les applications distribuées

## Alternatives considérées

### Option A : Logging texte simple

- **Description** : Console.WriteLine ou string concatenation
- **Avantages** : Simple
- **Inconvénients** : Pas searchable, pas de corrélation
- **Raison du rejet** : Ne scale pas

### Option B : Logging en base de données

- **Description** : Stocker les logs en DB relationnelle
- **Avantages** : SQL pour recherche
- **Inconvénients** : Performance, volume
- **Raison du rejet** : Solutions spécialisées plus adaptées (Seq, ELK)

## Références

- [Structured Logging - Serilog](https://github.com/serilog/serilog/wiki/Structured-Data)
- [High-performance logging - Microsoft](https://docs.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging)
- [Logging Best Practices - Microsoft](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/loggermessage)
- [Seq](https://datalust.co/seq)
