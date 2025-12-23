# 36. Cross-Cutting Concerns comme Services Autonomes

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les préoccupations transversales (cross-cutting concerns) sont souvent intégrées directement dans l'application principale :
- **Quota Management** : Logique de quotas dispersée dans les handlers
- **Metering** : Comptage des tokens mélangé au code métier
- **Feature Flipping** : Conditions if/else partout
- **Authentication/Authorization** : Middleware couplé à l'app
- **Audit** : Logging dispersé sans cohérence
- **Observability** : Métriques ad-hoc sans standardisation

Problèmes de cette approche :
- **Réutilisabilité nulle** : Chaque app réimplémente
- **Couplage fort** : Difficile d'évoluer indépendamment
- **Tests complexes** : Dépendances entremêlées
- **Duplication** : Même logique dans Gateway, Admin API, Workers

```csharp
// ❌ CROSS-CUTTING CONCERNS INTÉGRÉS : Tout mélangé
public class ProcessLlmRequestHandler
{
    public async Task<Result> Handle(LlmRequest request)
    {
        // Auth - couplé
        if (!await _authService.ValidateTokenAsync(request.Token))
            return Result.Unauthorized();
        
        // Quota - couplé
        var usage = await _quotaService.GetUsageAsync(request.TenantId);
        if (usage >= await _quotaService.GetLimitAsync(request.TenantId))
            return Result.QuotaExceeded();
        
        // Feature flag - couplé
        if (!await _featureService.IsEnabledAsync("streaming", request.TenantId))
            return Result.FeatureDisabled();
        
        // Audit - couplé
        await _auditService.LogAsync("llm_request_started", request);
        
        // Métriques - couplé
        _metrics.IncrementRequestCount(request.TenantId);
        
        // ENFIN le code métier (10% du handler)
        var response = await _llmProvider.SendAsync(request);
        
        // Encore du cross-cutting
        await _quotaService.IncrementAsync(request.TenantId, response.Tokens);
        await _auditService.LogAsync("llm_request_completed", response);
        _metrics.RecordLatency(request.TenantId, response.Duration);
        
        return Result.Success(response);
    }
}
```

## Décision

**Concevoir les cross-cutting concerns comme des services autonomes ou des frameworks réutilisables, découplés de l'application principale.**

### 1. Architecture cible

```
┌─────────────────────────────────────────────────────────────────────┐
│                    APPLICATIONS MÉTIER                               │
│                                                                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │
│  │   Gateway    │  │  Admin API   │  │   Workers    │               │
│  │   (LLM)      │  │              │  │              │               │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘               │
│         │                 │                 │                        │
└─────────┼─────────────────┼─────────────────┼────────────────────────┘
          │                 │                 │
          ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────────────┐
│              SERVICES AUTONOMES (CROSS-CUTTING)                      │
│                                                                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │
│  │   Quota      │  │  Metering    │  │  Feature     │               │
│  │   Service    │  │  Service     │  │  Flags       │               │
│  │   (gRPC)     │  │  (gRPC)      │  │  (gRPC)      │               │
│  └──────────────┘  └──────────────┘  └──────────────┘               │
│                                                                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │
│  │   Auth       │  │  Audit       │  │ Observability│               │
│  │   Service    │  │  Service     │  │  Collector   │               │
│  │  (OIDC/JWT)  │  │  (Events)    │  │  (OTLP)      │               │
│  └──────────────┘  └──────────────┘  └──────────────┘               │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### 2. Quota Service - Service Autonome

```csharp
// ═══════════════════════════════════════════════════════════════
// QUOTA SERVICE - Application autonome (gRPC)
// Projet: LLMProxy.Services.Quota
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Service gRPC pour la gestion des quotas.
/// Déployé indépendamment, scalable horizontalement.
/// </summary>
public sealed class QuotaGrpcService : QuotaService.QuotaServiceBase
{
    private readonly IQuotaRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<QuotaGrpcService> _logger;
    
    /// <summary>
    /// Vérifie si un tenant peut consommer des ressources.
    /// </summary>
    public override async Task<CheckQuotaResponse> CheckQuota(
        CheckQuotaRequest request,
        ServerCallContext context)
    {
        var tenantId = Guid.Parse(request.TenantId);
        var quotaType = request.QuotaType;
        var requestedAmount = request.RequestedAmount;
        
        // Vérifier en cache d'abord (Redis)
        var cacheKey = $"quota:{tenantId}:{quotaType}";
        var cached = await _cache.GetAsync<QuotaStatus>(cacheKey);
        
        if (cached is null)
        {
            cached = await _repository.GetQuotaStatusAsync(tenantId, quotaType);
            await _cache.SetAsync(cacheKey, cached, TimeSpan.FromMinutes(1));
        }
        
        var allowed = cached.CurrentUsage + requestedAmount <= cached.Limit;
        
        return new CheckQuotaResponse
        {
            Allowed = allowed,
            CurrentUsage = cached.CurrentUsage,
            Limit = cached.Limit,
            RemainingQuota = cached.Limit - cached.CurrentUsage,
            ResetAt = cached.ResetAt?.ToTimestamp()
        };
    }
    
    /// <summary>
    /// Incrémente l'usage d'un quota.
    /// </summary>
    public override async Task<IncrementQuotaResponse> IncrementQuota(
        IncrementQuotaRequest request,
        ServerCallContext context)
    {
        var tenantId = Guid.Parse(request.TenantId);
        
        // Mise à jour atomique (Redis INCRBY ou DB avec lock)
        var newUsage = await _repository.IncrementUsageAsync(
            tenantId,
            request.QuotaType,
            request.Amount);
        
        // Invalider le cache
        var cacheKey = $"quota:{tenantId}:{request.QuotaType}";
        await _cache.RemoveAsync(cacheKey);
        
        _logger.LogInformation(
            "Quota incremented for tenant {TenantId}: {QuotaType} += {Amount}",
            tenantId, request.QuotaType, request.Amount);
        
        return new IncrementQuotaResponse
        {
            NewUsage = newUsage,
            Success = true
        };
    }
}

// ═══════════════════════════════════════════════════════════════
// CLIENT SDK - Framework réutilisable
// Projet: LLMProxy.Sdk.Quota
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Client SDK pour consommer le Quota Service.
/// Utilisable par toutes les applications.
/// </summary>
public sealed class QuotaClient : IQuotaClient
{
    private readonly QuotaService.QuotaServiceClient _grpcClient;
    private readonly ILogger<QuotaClient> _logger;
    
    public async Task<QuotaCheckResult> CheckAsync(
        Guid tenantId,
        string quotaType,
        long requestedAmount,
        CancellationToken ct = default)
    {
        var request = new CheckQuotaRequest
        {
            TenantId = tenantId.ToString(),
            QuotaType = quotaType,
            RequestedAmount = requestedAmount
        };
        
        var response = await _grpcClient.CheckQuotaAsync(
            request, 
            cancellationToken: ct);
        
        return new QuotaCheckResult
        {
            Allowed = response.Allowed,
            CurrentUsage = response.CurrentUsage,
            Limit = response.Limit,
            Remaining = response.RemainingQuota
        };
    }
    
    public async Task IncrementAsync(
        Guid tenantId,
        string quotaType,
        long amount,
        CancellationToken ct = default)
    {
        var request = new IncrementQuotaRequest
        {
            TenantId = tenantId.ToString(),
            QuotaType = quotaType,
            Amount = amount
        };
        
        await _grpcClient.IncrementQuotaAsync(request, cancellationToken: ct);
    }
}

/// <summary>
/// Extension pour DI - Utilisable par n'importe quelle app.
/// </summary>
public static class QuotaClientExtensions
{
    public static IServiceCollection AddQuotaClient(
        this IServiceCollection services,
        string serviceUrl)
    {
        services.AddGrpcClient<QuotaService.QuotaServiceClient>(options =>
        {
            options.Address = new Uri(serviceUrl);
        });
        
        services.AddSingleton<IQuotaClient, QuotaClient>();
        
        return services;
    }
}
```

### 3. Feature Flags Service - Service Autonome

```csharp
// ═══════════════════════════════════════════════════════════════
// FEATURE FLAGS SERVICE
// Projet: LLMProxy.Services.FeatureFlags
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Service gRPC pour les feature flags.
/// </summary>
public sealed class FeatureFlagsGrpcService : FeatureFlagsService.FeatureFlagsServiceBase
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IDistributedCache _cache;
    
    /// <summary>
    /// Vérifie si une feature est activée.
    /// </summary>
    public override async Task<IsEnabledResponse> IsEnabled(
        IsEnabledRequest request,
        ServerCallContext context)
    {
        var cacheKey = $"feature:{request.FeatureKey}:{request.TenantId}";
        
        var cached = await _cache.GetAsync<bool?>(cacheKey);
        if (cached.HasValue)
        {
            return new IsEnabledResponse { Enabled = cached.Value };
        }
        
        var flag = await _repository.GetFlagAsync(request.FeatureKey);
        
        if (flag is null)
        {
            return new IsEnabledResponse { Enabled = false };
        }
        
        var enabled = EvaluateFlag(flag, request);
        
        await _cache.SetAsync(cacheKey, enabled, TimeSpan.FromMinutes(5));
        
        return new IsEnabledResponse { Enabled = enabled };
    }
    
    private bool EvaluateFlag(FeatureFlag flag, IsEnabledRequest request)
    {
        // Désactivé globalement
        if (!flag.GloballyEnabled)
            return false;
        
        // Vérifier la whitelist tenant
        if (flag.EnabledForTenants.Contains(request.TenantId))
            return true;
        
        // Vérifier le rollout percentage
        if (flag.RolloutPercentage > 0)
        {
            var hash = GetConsistentHash(request.TenantId, request.FeatureKey);
            return hash % 100 < flag.RolloutPercentage;
        }
        
        return flag.DefaultValue;
    }
}

// ═══════════════════════════════════════════════════════════════
// CLIENT SDK
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Client avec cache local pour performance.
/// </summary>
public sealed class FeatureFlagsClient : IFeatureFlagsClient
{
    private readonly FeatureFlagsService.FeatureFlagsServiceClient _grpcClient;
    private readonly IMemoryCache _localCache;
    
    public async Task<bool> IsEnabledAsync(
        string featureKey,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var cacheKey = $"feature:{featureKey}:{tenantId}";
        
        // Cache local (quelques secondes)
        if (_localCache.TryGetValue(cacheKey, out bool cached))
            return cached;
        
        var response = await _grpcClient.IsEnabledAsync(
            new IsEnabledRequest
            {
                FeatureKey = featureKey,
                TenantId = tenantId.ToString()
            },
            cancellationToken: ct);
        
        _localCache.Set(cacheKey, response.Enabled, TimeSpan.FromSeconds(30));
        
        return response.Enabled;
    }
    
    /// <summary>
    /// Raccourci pour usage dans du code métier.
    /// </summary>
    public async Task<T> WhenEnabledAsync<T>(
        string featureKey,
        Guid tenantId,
        Func<Task<T>> whenEnabled,
        Func<Task<T>> whenDisabled,
        CancellationToken ct = default)
    {
        return await IsEnabledAsync(featureKey, tenantId, ct)
            ? await whenEnabled()
            : await whenDisabled();
    }
}
```

### 4. Audit Service - Event-Driven

```csharp
// ═══════════════════════════════════════════════════════════════
// AUDIT SERVICE - Consomme des événements
// Projet: LLMProxy.Services.Audit
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Worker qui consomme les événements d'audit.
/// Découplé via message queue (RabbitMQ, Kafka, Azure Service Bus).
/// </summary>
public sealed class AuditEventConsumer : BackgroundService
{
    private readonly IMessageConsumer<AuditEvent> _consumer;
    private readonly IAuditRepository _repository;
    private readonly ILogger<AuditEventConsumer> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (var auditEvent in _consumer.ConsumeAsync(ct))
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    TenantId = auditEvent.TenantId,
                    UserId = auditEvent.UserId,
                    Action = auditEvent.Action,
                    EntityType = auditEvent.EntityType,
                    EntityId = auditEvent.EntityId,
                    OldValues = auditEvent.OldValues,
                    NewValues = auditEvent.NewValues,
                    IpAddress = auditEvent.IpAddress,
                    UserAgent = auditEvent.UserAgent,
                    Timestamp = auditEvent.Timestamp
                };
                
                await _repository.InsertAsync(auditLog, ct);
                
                _logger.LogDebug(
                    "Audit log saved: {Action} on {EntityType} by {UserId}",
                    auditEvent.Action,
                    auditEvent.EntityType,
                    auditEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process audit event");
                // Dead letter queue pour retry
            }
        }
    }
}

// ═══════════════════════════════════════════════════════════════
// AUDIT SDK - Publisher
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Publisher d'événements d'audit (fire and forget).
/// </summary>
public sealed class AuditPublisher : IAuditPublisher
{
    private readonly IMessagePublisher<AuditEvent> _publisher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    /// <summary>
    /// Publie un événement d'audit de manière asynchrone.
    /// Ne bloque pas le flux principal.
    /// </summary>
    public async Task PublishAsync(
        string action,
        string entityType,
        Guid? entityId = null,
        object? oldValues = null,
        object? newValues = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        var auditEvent = new AuditEvent
        {
            TenantId = GetTenantId(httpContext),
            UserId = GetUserId(httpContext),
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues is not null 
                ? JsonSerializer.Serialize(oldValues) 
                : null,
            NewValues = newValues is not null 
                ? JsonSerializer.Serialize(newValues) 
                : null,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent,
            Timestamp = DateTimeOffset.UtcNow
        };
        
        // Publish sans await (fire and forget)
        _ = _publisher.PublishAsync(auditEvent);
    }
}
```

### 5. Metering Service - Haute Performance

```csharp
// ═══════════════════════════════════════════════════════════════
// METERING SERVICE - Optimisé pour le volume
// Projet: LLMProxy.Services.Metering
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Service de metering avec agrégation.
/// Gère des millions d'événements par jour.
/// </summary>
public sealed class MeteringGrpcService : MeteringService.MeteringServiceBase
{
    private readonly IRedisConnection _redis;
    private readonly IMeteringRepository _repository;
    private readonly ILogger<MeteringGrpcService> _logger;
    
    /// <summary>
    /// Enregistre un usage (batch optimisé).
    /// </summary>
    public override async Task<RecordUsageResponse> RecordUsage(
        RecordUsageRequest request,
        ServerCallContext context)
    {
        var tenantId = request.TenantId;
        var timestamp = DateTimeOffset.UtcNow;
        
        // Agrégation en Redis (INCRBY atomique)
        var redisKey = $"metering:{tenantId}:{timestamp:yyyy-MM-dd-HH}";
        
        var batch = _redis.CreateBatch();
        
        batch.HashIncrementAsync(redisKey, "requests", 1);
        batch.HashIncrementAsync(redisKey, "input_tokens", request.InputTokens);
        batch.HashIncrementAsync(redisKey, "output_tokens", request.OutputTokens);
        batch.HashIncrementAsync(redisKey, $"model:{request.Model}", 1);
        batch.KeyExpireAsync(redisKey, TimeSpan.FromDays(7));
        
        batch.Execute();
        
        return new RecordUsageResponse { Success = true };
    }
    
    /// <summary>
    /// Récupère les métriques agrégées.
    /// </summary>
    public override async Task<GetUsageResponse> GetUsage(
        GetUsageRequest request,
        ServerCallContext context)
    {
        var tenantId = request.TenantId;
        var from = request.From.ToDateTimeOffset();
        var to = request.To.ToDateTimeOffset();
        
        // Agréger depuis Redis (données récentes)
        // + depuis la DB (données historiques)
        var usage = await _repository.GetAggregatedUsageAsync(
            Guid.Parse(tenantId),
            from,
            to);
        
        return new GetUsageResponse
        {
            TotalRequests = usage.TotalRequests,
            TotalInputTokens = usage.TotalInputTokens,
            TotalOutputTokens = usage.TotalOutputTokens,
            ByModel = { usage.ByModel.Select(m => new ModelUsage
            {
                Model = m.Key,
                RequestCount = m.Value
            })}
        };
    }
}

/// <summary>
/// Worker qui flush les données Redis vers la DB (périodique).
/// </summary>
public sealed class MeteringFlushWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // Flush toutes les heures
            await Task.Delay(TimeSpan.FromHours(1), ct);
            
            // Lire les données Redis des heures précédentes
            // et les persister en DB pour l'historique
            await FlushOldDataToDatabaseAsync(ct);
        }
    }
}
```

### 6. Observability - Collecteur OTLP

```csharp
// ═══════════════════════════════════════════════════════════════
// OBSERVABILITY - Framework/SDK partagé
// Projet: LLMProxy.Sdk.Observability
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Configuration standardisée d'observabilité.
/// Réutilisable par toutes les applications.
/// </summary>
public static class ObservabilityExtensions
{
    public static IServiceCollection AddLlmProxyObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var otlpEndpoint = configuration["Observability:OtlpEndpoint"];
        
        // OpenTelemetry - Traces
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = 
                        configuration["Environment"] ?? "development",
                    ["service.version"] = 
                        Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0"
                }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddGrpcClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddSource(serviceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(serviceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                }));
        
        // Structured Logging vers OTLP
        services.AddLogging(logging =>
        {
            logging.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName));
                options.AddOtlpExporter(otlp =>
                {
                    otlp.Endpoint = new Uri(otlpEndpoint);
                });
            });
        });
        
        return services;
    }
}

/// <summary>
/// Métriques métier standardisées.
/// </summary>
public sealed class LlmProxyMetrics
{
    private readonly Meter _meter;
    
    // Compteurs
    public Counter<long> RequestsTotal { get; }
    public Counter<long> TokensProcessed { get; }
    public Counter<long> ErrorsTotal { get; }
    
    // Histogrammes
    public Histogram<double> RequestDuration { get; }
    public Histogram<long> TokensPerRequest { get; }
    
    // Gauges
    public ObservableGauge<int> ActiveConnections { get; }
    
    public LlmProxyMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create("LLMProxy.Gateway");
        
        RequestsTotal = _meter.CreateCounter<long>(
            "llmproxy.requests.total",
            description: "Total LLM requests processed");
        
        TokensProcessed = _meter.CreateCounter<long>(
            "llmproxy.tokens.processed",
            description: "Total tokens processed");
        
        ErrorsTotal = _meter.CreateCounter<long>(
            "llmproxy.errors.total",
            description: "Total errors");
        
        RequestDuration = _meter.CreateHistogram<double>(
            "llmproxy.request.duration",
            unit: "ms",
            description: "Request duration in milliseconds");
        
        TokensPerRequest = _meter.CreateHistogram<long>(
            "llmproxy.tokens.per_request",
            description: "Tokens per request");
    }
    
    public void RecordRequest(
        string tenantId,
        string model,
        string provider,
        long inputTokens,
        long outputTokens,
        double durationMs,
        bool success)
    {
        var tags = new TagList
        {
            { "tenant_id", tenantId },
            { "model", model },
            { "provider", provider },
            { "status", success ? "success" : "error" }
        };
        
        RequestsTotal.Add(1, tags);
        TokensProcessed.Add(inputTokens + outputTokens, tags);
        RequestDuration.Record(durationMs, tags);
        TokensPerRequest.Record(inputTokens + outputTokens, tags);
        
        if (!success)
            ErrorsTotal.Add(1, tags);
    }
}
```

### 7. Auth Service - OIDC/JWT Standard

```csharp
// ═══════════════════════════════════════════════════════════════
// AUTH - Service externe (Keycloak, Auth0, ou custom)
// Configuration dans les apps clientes
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Configuration Auth standardisée pour toutes les apps.
/// </summary>
public static class AuthExtensions
{
    public static IServiceCollection AddLlmProxyAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authConfig = configuration.GetSection("Auth");
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Service d'authentification externe
                options.Authority = authConfig["Authority"]; // https://auth.llmproxy.com
                options.Audience = authConfig["Audience"];   // llmproxy-api
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
        
        // Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("TenantAdmin", policy =>
                policy.RequireClaim("role", "tenant_admin"));
            
            options.AddPolicy("ApiAccess", policy =>
                policy.RequireClaim("scope", "api:access"));
            
            options.AddPolicy("SameTenant", policy =>
                policy.AddRequirements(new SameTenantRequirement()));
        });
        
        services.AddSingleton<IAuthorizationHandler, SameTenantHandler>();
        
        return services;
    }
}

/// <summary>
/// Handler custom pour vérifier l'appartenance au tenant.
/// </summary>
public sealed class SameTenantHandler : AuthorizationHandler<SameTenantRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameTenantRequirement requirement)
    {
        var userTenantId = context.User.FindFirstValue("tenant_id");
        var routeTenantId = _httpContextAccessor.HttpContext?
            .GetRouteValue("tenantId")?.ToString();
        
        if (userTenantId == routeTenantId)
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
```

### 8. Utilisation dans l'application principale

```csharp
// ═══════════════════════════════════════════════════════════════
// GATEWAY - Code métier PROPRE
// Utilise les SDKs des services autonomes
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Handler métier PROPRE - sans cross-cutting concerns intégrés.
/// </summary>
public sealed class ProcessLlmRequestHandler
{
    private readonly ILlmProvider _llmProvider;
    
    // Les cross-cutting concerns sont dans des middlewares/interceptors
    // PAS dans le handler
    
    public async Task<Result<LlmResponse>> Handle(
        LlmRequest request,
        CancellationToken ct)
    {
        // UNIQUEMENT la logique métier
        var response = await _llmProvider.SendAsync(request, ct);
        
        return Result.Success(response);
    }
}

/// <summary>
/// Middleware qui orchestre les cross-cutting concerns.
/// </summary>
public sealed class LlmRequestMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IQuotaClient _quotaClient;
    private readonly IFeatureFlagsClient _featureFlags;
    private readonly IAuditPublisher _audit;
    private readonly LlmProxyMetrics _metrics;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = context.User.GetTenantId();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // 1. Quota check (appel gRPC vers Quota Service)
            var quotaCheck = await _quotaClient.CheckAsync(
                tenantId, "tokens", estimatedTokens);
            
            if (!quotaCheck.Allowed)
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsJsonAsync(new { error = "Quota exceeded" });
                return;
            }
            
            // 2. Feature flag (appel gRPC vers Feature Service)
            if (!await _featureFlags.IsEnabledAsync("llm_gateway", tenantId))
            {
                context.Response.StatusCode = 403;
                return;
            }
            
            // 3. Exécuter le handler métier
            await _next(context);
            
            // 4. Post-processing (async, fire and forget)
            var response = context.Items["LlmResponse"] as LlmResponse;
            
            if (response is not null)
            {
                // Incrémenter quota (async)
                _ = _quotaClient.IncrementAsync(
                    tenantId, "tokens", response.TotalTokens);
                
                // Audit (async via message queue)
                _ = _audit.PublishAsync(
                    "llm_request_completed",
                    "LlmRequest",
                    newValues: response);
                
                // Métriques
                _metrics.RecordRequest(
                    tenantId.ToString(),
                    response.Model,
                    response.Provider,
                    response.InputTokens,
                    response.OutputTokens,
                    stopwatch.ElapsedMilliseconds,
                    success: true);
            }
        }
        catch (Exception ex)
        {
            _metrics.RecordRequest(
                tenantId.ToString(), "", "", 0, 0,
                stopwatch.ElapsedMilliseconds,
                success: false);
            
            throw;
        }
    }
}
```

### 9. Déploiement des services

```yaml
# docker-compose.yml - Services autonomes
version: '3.8'

services:
  # Service Quota (autonome)
  quota-service:
    image: llmproxy/quota-service:latest
    ports:
      - "5001:5001"
    environment:
      - REDIS_URL=redis:6379
      - DATABASE_URL=postgresql://postgres:password@db:5432/quotas
    depends_on:
      - redis
      - db

  # Service Feature Flags (autonome)
  feature-flags-service:
    image: llmproxy/feature-flags-service:latest
    ports:
      - "5002:5002"
    environment:
      - REDIS_URL=redis:6379
      - DATABASE_URL=postgresql://postgres:password@db:5432/features

  # Service Audit (worker)
  audit-service:
    image: llmproxy/audit-service:latest
    environment:
      - RABBITMQ_URL=amqp://rabbitmq:5672
      - DATABASE_URL=postgresql://postgres:password@db:5432/audit
    depends_on:
      - rabbitmq
      - db

  # Service Metering (haute performance)
  metering-service:
    image: llmproxy/metering-service:latest
    ports:
      - "5003:5003"
    environment:
      - REDIS_URL=redis:6379
    deploy:
      replicas: 3  # Scalable horizontalement

  # Application principale (Gateway)
  gateway:
    image: llmproxy/gateway:latest
    ports:
      - "8080:8080"
    environment:
      - QUOTA_SERVICE_URL=http://quota-service:5001
      - FEATURE_FLAGS_URL=http://feature-flags-service:5002
      - METERING_SERVICE_URL=http://metering-service:5003
      - AUTH_AUTHORITY=https://auth.llmproxy.com
    depends_on:
      - quota-service
      - feature-flags-service
      - metering-service
```

## Conséquences

### Positives

- **Réutilisabilité** : Un service Quota utilisable par N applications
- **Scalabilité** : Chaque service scale indépendamment
- **Maintenance** : Équipes peuvent évoluer en parallèle
- **Tests** : Services testables isolément
- **Technologie** : Chaque service peut utiliser la stack optimale

### Négatives

- **Latence** : Appels réseau supplémentaires
  - *Mitigation* : Cache local, gRPC performant
- **Complexité opérationnelle** : Plus de services à déployer
  - *Mitigation* : Kubernetes, orchestration
- **Cohérence** : Transactions distribuées plus complexes
  - *Mitigation* : Event sourcing, saga pattern

### Neutres

- Approche standard dans les architectures microservices
- Peut commencer avec des libs/SDKs avant d'extraire en services

## Alternatives considérées

### Option A : Bibliothèques partagées uniquement

- **Description** : NuGet packages sans services autonomes
- **Avantages** : Pas de latence réseau, plus simple
- **Inconvénients** : Pas de scalabilité indépendante
- **Raison du rejet** : Limitant pour la croissance

### Option B : Tout dans le monolithe

- **Description** : Cross-cutting concerns intégrés
- **Avantages** : Simple au début
- **Inconvénients** : Couplage, duplication, pas réutilisable
- **Raison du rejet** : Dette technique importante

## Références

- [Microservices - Martin Fowler](https://martinfowler.com/articles/microservices.html)
- [Cross-Cutting Concerns](https://en.wikipedia.org/wiki/Cross-cutting_concern)
- [OpenTelemetry](https://opentelemetry.io/)
- [gRPC](https://grpc.io/)
- [Feature Flags - LaunchDarkly](https://launchdarkly.com/blog/feature-flags-best-practices/)
