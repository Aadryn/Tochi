# 43. Gestion des Exceptions et Error Handling

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un système distribué comme un proxy LLM peut rencontrer de nombreuses erreurs :
- Erreurs métier (quota dépassé, clé invalide)
- Erreurs techniques (timeout, connexion perdue)
- Erreurs externes (provider LLM indisponible)
- Erreurs de validation (input invalide)

Sans stratégie cohérente :
- Les exceptions sont gérées de manière inconsistante
- Les logs sont incomplets ou trop verbeux
- Les clients reçoivent des messages d'erreur incohérents
- Les informations sensibles peuvent fuiter

```csharp
// ❌ MAUVAISE GESTION : Incohérente, dangereuse
public async Task<IActionResult> ChatCompletion([FromBody] ChatRequest request)
{
    try
    {
        // Logique...
        return Ok(response);
    }
    catch (Exception ex)
    {
        // Log pas structuré
        Console.WriteLine($"Error: {ex}");
        
        // Stack trace exposée au client !
        return BadRequest(new { error = ex.ToString() });
    }
}
```

## Décision

**Adopter une stratégie de gestion des exceptions structurée avec hiérarchie d'exceptions métier, middleware global, et réponses d'erreur standardisées.**

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    HIÉRARCHIE D'EXCEPTIONS                      │
│                                                                 │
│  ┌────────────────────────────────────────────────────────┐    │
│  │              DomainException (abstract)                │    │
│  │              • Message                                  │    │
│  │              • ErrorCode                                │    │
│  │              • HttpStatusCode                           │    │
│  └──────────────────────┬─────────────────────────────────┘    │
│                          │                                      │
│     ┌────────────────────┼────────────────────┐                │
│     │                    │                    │                 │
│     ▼                    ▼                    ▼                 │
│  ┌──────────┐    ┌──────────────┐    ┌────────────────┐        │
│  │Validation│    │   Business   │    │   Technical    │        │
│  │Exception │    │  Exception   │    │   Exception    │        │
│  └──────────┘    └──────────────┘    └────────────────┘        │
│       │                  │                    │                 │
│       ▼                  ▼                    ▼                 │
│  InvalidInput       QuotaExceeded      ProviderUnavailable     │
│  RequiredField      TenantInactive     DatabaseError           │
│  InvalidFormat      ApiKeyRevoked      TimeoutException        │
└─────────────────────────────────────────────────────────────────┘
```

### 1. Hiérarchie d'Exceptions Métier

```csharp
/// <summary>
/// Exception de base du domaine.
/// Toutes les exceptions métier en héritent.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Code d'erreur unique pour identification.
    /// </summary>
    public string ErrorCode { get; }
    
    /// <summary>
    /// Code HTTP suggéré pour la réponse API.
    /// </summary>
    public virtual int HttpStatusCode => 400;
    
    /// <summary>
    /// Données additionnelles pour le debugging.
    /// </summary>
    public IReadOnlyDictionary<string, object> Details { get; }
    
    protected DomainException(
        string errorCode,
        string message,
        IReadOnlyDictionary<string, object>? details = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Details = details ?? new Dictionary<string, object>();
    }
}

// ═══════════════════════════════════════════════════════════════
// EXCEPTIONS DE VALIDATION
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Exception pour les erreurs de validation.
/// </summary>
public sealed class ValidationException : DomainException
{
    public IReadOnlyList<ValidationError> Errors { get; }
    
    public ValidationException(IReadOnlyList<ValidationError> errors)
        : base(
            "VALIDATION_ERROR",
            "One or more validation errors occurred",
            new Dictionary<string, object> { ["errors"] = errors })
    {
        Errors = errors;
    }
    
    public ValidationException(string field, string message)
        : this(new[] { new ValidationError(field, message) })
    {
    }
}

public sealed record ValidationError(string Field, string Message);

// ═══════════════════════════════════════════════════════════════
// EXCEPTIONS MÉTIER
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Exception quand une ressource n'est pas trouvée.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public override int HttpStatusCode => 404;
    
    public NotFoundException(string resourceType, object resourceId)
        : base(
            "NOT_FOUND",
            $"{resourceType} with identifier '{resourceId}' was not found",
            new Dictionary<string, object>
            {
                ["resourceType"] = resourceType,
                ["resourceId"] = resourceId
            })
    {
    }
}

/// <summary>
/// Exception quand le quota est dépassé.
/// </summary>
public sealed class QuotaExceededException : DomainException
{
    public override int HttpStatusCode => 429;
    
    public QuotaExceededException(
        Guid tenantId,
        string quotaType,
        long current,
        long limit)
        : base(
            "QUOTA_EXCEEDED",
            $"Quota '{quotaType}' exceeded: {current}/{limit}",
            new Dictionary<string, object>
            {
                ["tenantId"] = tenantId,
                ["quotaType"] = quotaType,
                ["currentUsage"] = current,
                ["limit"] = limit
            })
    {
    }
}

/// <summary>
/// Exception quand le tenant est inactif.
/// </summary>
public sealed class TenantInactiveException : DomainException
{
    public override int HttpStatusCode => 403;
    
    public TenantInactiveException(Guid tenantId)
        : base(
            "TENANT_INACTIVE",
            "Tenant is not active",
            new Dictionary<string, object> { ["tenantId"] = tenantId })
    {
    }
}

/// <summary>
/// Exception quand une clé API est invalide.
/// </summary>
public sealed class InvalidApiKeyException : DomainException
{
    public override int HttpStatusCode => 401;
    
    public InvalidApiKeyException(string reason)
        : base(
            "INVALID_API_KEY",
            reason)
    {
    }
}

/// <summary>
/// Exception quand une clé API est révoquée.
/// </summary>
public sealed class ApiKeyRevokedException : DomainException
{
    public override int HttpStatusCode => 401;
    
    public ApiKeyRevokedException(string keyPrefix)
        : base(
            "API_KEY_REVOKED",
            $"API key '{keyPrefix}...' has been revoked",
            new Dictionary<string, object> { ["keyPrefix"] = keyPrefix })
    {
    }
}

/// <summary>
/// Exception quand l'accès est refusé.
/// </summary>
public sealed class ForbiddenException : DomainException
{
    public override int HttpStatusCode => 403;
    
    public ForbiddenException(string reason)
        : base("FORBIDDEN", reason)
    {
    }
}

// ═══════════════════════════════════════════════════════════════
// EXCEPTIONS TECHNIQUES
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Exception quand un provider LLM est indisponible.
/// </summary>
public sealed class ProviderUnavailableException : DomainException
{
    public override int HttpStatusCode => 503;
    
    public ProviderUnavailableException(
        string provider,
        string reason,
        Exception? innerException = null)
        : base(
            "PROVIDER_UNAVAILABLE",
            $"Provider '{provider}' is currently unavailable: {reason}",
            new Dictionary<string, object>
            {
                ["provider"] = provider,
                ["reason"] = reason
            },
            innerException)
    {
    }
}

/// <summary>
/// Exception quand un timeout se produit.
/// </summary>
public sealed class RequestTimeoutException : DomainException
{
    public override int HttpStatusCode => 504;
    
    public RequestTimeoutException(
        string operation,
        TimeSpan timeout)
        : base(
            "REQUEST_TIMEOUT",
            $"Operation '{operation}' timed out after {timeout.TotalSeconds}s",
            new Dictionary<string, object>
            {
                ["operation"] = operation,
                ["timeoutSeconds"] = timeout.TotalSeconds
            })
    {
    }
}

/// <summary>
/// Exception quand le rate limit est atteint.
/// </summary>
public sealed class RateLimitExceededException : DomainException
{
    public override int HttpStatusCode => 429;
    
    public TimeSpan? RetryAfter { get; }
    
    public RateLimitExceededException(
        string limitType,
        int current,
        int limit,
        TimeSpan? retryAfter = null)
        : base(
            "RATE_LIMIT_EXCEEDED",
            $"Rate limit '{limitType}' exceeded: {current}/{limit}",
            new Dictionary<string, object>
            {
                ["limitType"] = limitType,
                ["current"] = current,
                ["limit"] = limit,
                ["retryAfterSeconds"] = retryAfter?.TotalSeconds
            })
    {
        RetryAfter = retryAfter;
    }
}
```

### 2. Middleware de Gestion Globale

```csharp
/// <summary>
/// Middleware de gestion globale des exceptions.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;
    
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
        var (statusCode, errorResponse) = exception switch
        {
            // Exceptions métier - loggées en Warning
            DomainException domainEx => HandleDomainException(context, domainEx),
            
            // Exceptions d'annulation - pas d'erreur
            OperationCanceledException => HandleCancellationException(context),
            
            // Exceptions techniques - loggées en Error
            _ => HandleUnexpectedException(context, exception)
        };
        
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsJsonAsync(errorResponse);
    }
    
    private (int, ErrorResponse) HandleDomainException(
        HttpContext context,
        DomainException exception)
    {
        // Log Warning avec contexte
        _logger.LogWarning(
            exception,
            "Domain exception occurred. Code: {ErrorCode}, Path: {Path}, TraceId: {TraceId}",
            exception.ErrorCode,
            context.Request.Path,
            Activity.Current?.Id ?? context.TraceIdentifier);
        
        var response = new ErrorResponse
        {
            Error = new ErrorDetails
            {
                Code = exception.ErrorCode,
                Message = exception.Message,
                Type = GetErrorType(exception),
                Details = exception.Details,
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier
            }
        };
        
        // Ajouter header Retry-After si applicable
        if (exception is RateLimitExceededException rateLimit && rateLimit.RetryAfter.HasValue)
        {
            context.Response.Headers["Retry-After"] = 
                ((int)rateLimit.RetryAfter.Value.TotalSeconds).ToString();
        }
        
        return (exception.HttpStatusCode, response);
    }
    
    private (int, ErrorResponse) HandleCancellationException(HttpContext context)
    {
        _logger.LogDebug(
            "Request cancelled. Path: {Path}, TraceId: {TraceId}",
            context.Request.Path,
            Activity.Current?.Id ?? context.TraceIdentifier);
        
        return (499, new ErrorResponse
        {
            Error = new ErrorDetails
            {
                Code = "REQUEST_CANCELLED",
                Message = "The request was cancelled",
                Type = "client_error",
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier
            }
        });
    }
    
    private (int, ErrorResponse) HandleUnexpectedException(
        HttpContext context,
        Exception exception)
    {
        // Log Error avec stack trace complète
        _logger.LogError(
            exception,
            "Unexpected exception occurred. Path: {Path}, Method: {Method}, TraceId: {TraceId}",
            context.Request.Path,
            context.Request.Method,
            Activity.Current?.Id ?? context.TraceIdentifier);
        
        // Ne pas exposer les détails en production
        var message = _environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred. Please try again later.";
        
        var response = new ErrorResponse
        {
            Error = new ErrorDetails
            {
                Code = "INTERNAL_ERROR",
                Message = message,
                Type = "server_error",
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier
            }
        };
        
        // Ajouter la stack trace en développement seulement
        if (_environment.IsDevelopment())
        {
            response.Error.DebugInfo = new DebugInfo
            {
                ExceptionType = exception.GetType().Name,
                StackTrace = exception.StackTrace
            };
        }
        
        return (500, response);
    }
    
    private static string GetErrorType(DomainException exception) => exception switch
    {
        ValidationException => "validation_error",
        NotFoundException => "not_found",
        InvalidApiKeyException or ApiKeyRevokedException => "authentication_error",
        ForbiddenException or TenantInactiveException => "authorization_error",
        QuotaExceededException or RateLimitExceededException => "rate_limit_error",
        ProviderUnavailableException or RequestTimeoutException => "provider_error",
        _ => "error"
    };
}
```

### 3. Modèles de Réponse d'Erreur

```csharp
/// <summary>
/// Format de réponse d'erreur standardisé.
/// Inspiré du format OpenAI pour compatibilité.
/// </summary>
public sealed class ErrorResponse
{
    [JsonPropertyName("error")]
    public required ErrorDetails Error { get; init; }
}

public sealed class ErrorDetails
{
    /// <summary>
    /// Code d'erreur machine-readable.
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; init; }
    
    /// <summary>
    /// Message human-readable.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }
    
    /// <summary>
    /// Type d'erreur (validation_error, rate_limit_error, etc.).
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }
    
    /// <summary>
    /// Paramètre concerné (pour les erreurs de validation).
    /// </summary>
    [JsonPropertyName("param")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Param { get; init; }
    
    /// <summary>
    /// Détails supplémentaires.
    /// </summary>
    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, object>? Details { get; init; }
    
    /// <summary>
    /// ID de trace pour le support.
    /// </summary>
    [JsonPropertyName("trace_id")]
    public required string TraceId { get; init; }
    
    /// <summary>
    /// Informations de debug (développement seulement).
    /// </summary>
    [JsonPropertyName("debug")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DebugInfo? DebugInfo { get; set; }
}

public sealed class DebugInfo
{
    [JsonPropertyName("exception_type")]
    public string? ExceptionType { get; init; }
    
    [JsonPropertyName("stack_trace")]
    public string? StackTrace { get; init; }
}
```

### 4. Exception Filters pour les Controllers

```csharp
/// <summary>
/// Filter d'exception pour enrichir les logs avec le contexte controller.
/// </summary>
public sealed class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;
    
    public void OnException(ExceptionContext context)
    {
        // Enrichir avec les informations du controller
        var controllerName = context.ActionDescriptor
            .RouteValues["controller"];
        var actionName = context.ActionDescriptor
            .RouteValues["action"];
        
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["Controller"] = controllerName ?? "Unknown",
            ["Action"] = actionName ?? "Unknown",
            ["ModelState"] = context.ModelState.IsValid ? "Valid" : "Invalid"
        }))
        {
            // Le middleware global gère le reste
            // Ce filter ajoute juste du contexte
        }
    }
}
```

### 5. Problem Details (RFC 7807)

```csharp
/// <summary>
/// Format Problem Details (RFC 7807) alternatif.
/// </summary>
public sealed class ProblemDetailsFactory : IProblemDetailsFactory
{
    public ProblemDetails CreateFromException(
        DomainException exception,
        HttpContext context)
    {
        return new ProblemDetails
        {
            Status = exception.HttpStatusCode,
            Type = $"https://api.llmproxy.com/errors/{exception.ErrorCode.ToLowerInvariant()}",
            Title = GetTitle(exception),
            Detail = exception.Message,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier,
                ["errorCode"] = exception.ErrorCode
            }
        };
    }
    
    private static string GetTitle(DomainException exception) => exception switch
    {
        ValidationException => "Validation Failed",
        NotFoundException => "Resource Not Found",
        QuotaExceededException => "Quota Exceeded",
        RateLimitExceededException => "Rate Limit Exceeded",
        _ => "An Error Occurred"
    };
}
```

### 6. Validation avec FluentValidation

```csharp
/// <summary>
/// Pipeline de validation MediatR.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();
        
        var context = new ValidationContext<TRequest>(request);
        
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();
        
        if (failures.Any())
        {
            var errors = failures
                .Select(f => new ValidationError(f.PropertyName, f.ErrorMessage))
                .ToList();
            
            throw new ValidationException(errors);
        }
        
        return await next();
    }
}

/// <summary>
/// Exemple de validateur.
/// </summary>
public sealed class CreateTenantCommandValidator 
    : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");
        
        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Slug is required")
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens");
        
        RuleFor(x => x.BillingEmail)
            .NotEmpty()
            .WithMessage("Billing email is required")
            .EmailAddress()
            .WithMessage("Invalid email format");
    }
}
```

### 7. Configuration

```csharp
/// <summary>
/// Configuration de la gestion des exceptions.
/// </summary>
public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddExceptionHandling(
        this IServiceCollection services)
    {
        // Filter pour les controllers
        services.AddControllers(options =>
        {
            options.Filters.Add<ApiExceptionFilter>();
        });
        
        // Problem Details
        services.AddProblemDetails();
        
        return services;
    }
    
    public static IApplicationBuilder UseExceptionHandling(
        this IApplicationBuilder app)
    {
        // Middleware en premier dans le pipeline
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        
        return app;
    }
}
```

## Conséquences

### Positives

- **Cohérence** : Même format d'erreur partout
- **Sécurité** : Pas de fuite d'informations sensibles
- **Debuggabilité** : TraceId pour corrélation
- **DX** : Messages d'erreur clairs pour les clients

### Négatives

- **Verbosité** : Beaucoup de classes d'exceptions
  - *Mitigation* : Hiérarchie claire, génération possible
- **Maintenance** : Codes d'erreur à documenter
  - *Mitigation* : Documentation auto-générée

### Neutres

- Compatible avec les standards (RFC 7807)
- Format inspiré d'OpenAI pour familiarité

## Alternatives considérées

### Option A : Exceptions génériques

- **Description** : Utiliser Exception standard
- **Avantages** : Simple
- **Inconvénients** : Pas de sémantique, difficile à gérer
- **Raison du rejet** : Pas assez expressif

### Option B : Result Pattern partout

- **Description** : Jamais d'exceptions, que des Result
- **Avantages** : Flux explicite
- **Inconvénients** : Verbeux, pas idiomatique partout
- **Raison du rejet** : Hybride préféré (Result + exceptions)

## Références

- [RFC 7807 - Problem Details](https://www.rfc-editor.org/rfc/rfc7807)
- [Exception Handling in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
- [OpenAI Error Handling](https://platform.openai.com/docs/guides/error-codes)
