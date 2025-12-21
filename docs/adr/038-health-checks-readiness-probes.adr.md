# 38. Health Checks et Readiness Probes

Date: 2025-12-21

## Statut

Accepté

## Contexte

Dans un environnement conteneurisé (Kubernetes, Docker Swarm), l'orchestrateur doit savoir :
- **Liveness** : L'application est-elle vivante ? (sinon → restart)
- **Readiness** : L'application peut-elle recevoir du trafic ? (sinon → retirer du load balancer)
- **Startup** : L'application a-t-elle fini de démarrer ? (sinon → attendre)

Sans health checks :
- L'orchestrateur ne sait pas si l'app fonctionne
- Le traffic est routé vers des instances non prêtes
- Les problèmes de dépendances passent inaperçus
- Pas de monitoring proactif

```csharp
// ❌ SANS HEALTH CHECK : Aucune visibilité
// Kubernetes ne sait pas si l'app fonctionne
// Le load balancer envoie du traffic vers une instance cassée
```

## Décision

**Implémenter des health checks granulaires distinguant liveness, readiness et startup, avec vérification des dépendances critiques.**

### 1. Types de health checks

```csharp
/// <summary>
/// Les trois types de health checks Kubernetes :
/// 
/// ┌─────────────────────────────────────────────────────────────┐
/// │                    STARTUP PROBE                            │
/// │  "L'application a-t-elle fini de démarrer ?"                │
/// │                                                              │
/// │  • Vérifié au démarrage uniquement                          │
/// │  • Si échec prolongé → kill + restart                       │
/// │  • Permet un démarrage lent (migrations, warmup)            │
/// └─────────────────────────────────────────────────────────────┘
///                           │
///                           ▼ (une fois OK)
/// ┌─────────────────────────────────────────────────────────────┐
/// │                    LIVENESS PROBE                           │
/// │  "L'application est-elle vivante ?"                         │
/// │                                                              │
/// │  • Vérifié périodiquement                                   │
/// │  • Si échec → kill + restart                                │
/// │  • Détecte les deadlocks, OOM, crashes silencieux           │
/// │  • DOIT être léger (pas de dépendances externes)            │
/// └─────────────────────────────────────────────────────────────┘
///                           │
///                           │ (en parallèle)
/// ┌─────────────────────────────────────────────────────────────┐
/// │                   READINESS PROBE                           │
/// │  "L'application peut-elle recevoir du trafic ?"             │
/// │                                                              │
/// │  • Vérifié périodiquement                                   │
/// │  • Si échec → retrait du Service (pas de traffic)           │
/// │  • Vérifie les dépendances (DB, Redis, providers)           │
/// │  • Peut fluctuer (dépendance temporairement down)           │
/// └─────────────────────────────────────────────────────────────┘
/// </summary>
```

### 2. Configuration ASP.NET Core

```csharp
/// <summary>
/// Configuration des health checks.
/// </summary>
public static class HealthCheckExtensions
{
    public static IServiceCollection AddLlmProxyHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            // ═══ CHECKS DE BASE ═══
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy(),
                tags: new[] { "live" })
            
            // ═══ DATABASE ═══
            .AddNpgSql(
                configuration.GetConnectionString("Database")!,
                name: "database",
                healthQuery: "SELECT 1;",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready", "db" })
            
            // ═══ REDIS ═══
            .AddRedis(
                configuration.GetConnectionString("Redis")!,
                name: "redis",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready", "cache" })
            
            // ═══ LLM PROVIDERS ═══
            .AddCheck<OpenAiHealthCheck>(
                "openai",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready", "llm" })
            
            .AddCheck<AnthropicHealthCheck>(
                "anthropic",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready", "llm" })
            
            // ═══ MESSAGE QUEUE ═══
            .AddRabbitMQ(
                configuration.GetConnectionString("RabbitMQ")!,
                name: "rabbitmq",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready", "messaging" })
            
            // ═══ SERVICES INTERNES ═══
            .AddCheck<QuotaServiceHealthCheck>(
                "quota-service",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready", "internal" })
            
            // ═══ DISK SPACE ═══
            .AddDiskStorageHealthCheck(
                options => options.AddDrive("C:\\", 1024), // 1GB minimum
                name: "disk",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready", "infrastructure" })
            
            // ═══ MEMORY ═══
            .AddProcessAllocatedMemoryHealthCheck(
                maximumMegabytesAllocated: 1024, // 1GB max
                name: "memory",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "live", "infrastructure" });
        
        return services;
    }
    
    public static IApplicationBuilder UseLlmProxyHealthChecks(
        this IApplicationBuilder app)
    {
        // Liveness : Vérifie uniquement que l'app répond
        // Utilisé par Kubernetes pour savoir s'il faut restart
        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteMinimalResponse,
            AllowCachingResponses = false
        });
        
        // Readiness : Vérifie toutes les dépendances
        // Utilisé par Kubernetes pour le load balancing
        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteDetailedResponse,
            AllowCachingResponses = false
        });
        
        // Startup : Vérifie que l'app a fini de démarrer
        // Utilisé par Kubernetes pendant le démarrage
        app.UseHealthChecks("/health/startup", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteDetailedResponse,
            AllowCachingResponses = false
        });
        
        // Endpoint détaillé pour monitoring (protégé)
        app.UseHealthChecks("/health/details", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteFullDetailedResponse,
            AllowCachingResponses = false
        }).RequireAuthorization("HealthCheckPolicy");
        
        return app;
    }
}
```

### 3. Health Check personnalisé pour LLM Provider

```csharp
/// <summary>
/// Health check pour OpenAI avec circuit breaker awareness.
/// </summary>
public sealed class OpenAiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ICircuitBreakerState _circuitBreaker;
    private readonly ILogger<OpenAiHealthCheck> _logger;
    
    public OpenAiHealthCheck(
        IHttpClientFactory httpClientFactory,
        ICircuitBreakerState circuitBreaker,
        ILogger<OpenAiHealthCheck> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OpenAI");
        _circuitBreaker = circuitBreaker;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["provider"] = "OpenAI",
            ["endpoint"] = "https://api.openai.com"
        };
        
        try
        {
            // Vérifier l'état du circuit breaker d'abord
            if (_circuitBreaker.IsOpen)
            {
                data["circuit_state"] = "open";
                data["reason"] = "Circuit breaker is open";
                
                return HealthCheckResult.Degraded(
                    "OpenAI circuit breaker is open",
                    data: data);
            }
            
            // Appel léger à l'API (models endpoint)
            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            
            var response = await _httpClient.GetAsync(
                "/v1/models",
                cts.Token);
            
            data["status_code"] = (int)response.StatusCode;
            data["circuit_state"] = "closed";
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy(
                    "OpenAI is reachable",
                    data: data);
            }
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Problème de configuration, pas de panne
                return HealthCheckResult.Degraded(
                    "OpenAI authentication failed - check API key",
                    data: data);
            }
            
            return HealthCheckResult.Unhealthy(
                $"OpenAI returned {response.StatusCode}",
                data: data);
        }
        catch (TaskCanceledException)
        {
            data["error"] = "timeout";
            
            return HealthCheckResult.Degraded(
                "OpenAI health check timed out",
                data: data);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "OpenAI health check failed");
            
            data["error"] = ex.Message;
            
            return HealthCheckResult.Unhealthy(
                "Cannot reach OpenAI",
                exception: ex,
                data: data);
        }
    }
}
```

### 4. Health Check pour service interne gRPC

```csharp
/// <summary>
/// Health check pour le Quota Service (gRPC).
/// </summary>
public sealed class QuotaServiceHealthCheck : IHealthCheck
{
    private readonly Health.HealthClient _healthClient;
    private readonly ILogger<QuotaServiceHealthCheck> _logger;
    
    public QuotaServiceHealthCheck(
        Health.HealthClient healthClient,
        ILogger<QuotaServiceHealthCheck> logger)
    {
        _healthClient = healthClient;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["service"] = "QuotaService",
            ["protocol"] = "gRPC"
        };
        
        try
        {
            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            
            var response = await _healthClient.CheckAsync(
                new HealthCheckRequest { Service = "QuotaService" },
                cancellationToken: cts.Token);
            
            data["grpc_status"] = response.Status.ToString();
            
            return response.Status switch
            {
                HealthCheckResponse.Types.ServingStatus.Serving =>
                    HealthCheckResult.Healthy("QuotaService is healthy", data: data),
                
                HealthCheckResponse.Types.ServingStatus.NotServing =>
                    HealthCheckResult.Unhealthy("QuotaService is not serving", data: data),
                
                _ => HealthCheckResult.Degraded(
                    $"QuotaService status: {response.Status}", data: data)
            };
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "QuotaService health check failed");
            
            data["error"] = ex.Status.Detail;
            data["grpc_code"] = ex.StatusCode.ToString();
            
            return HealthCheckResult.Unhealthy(
                $"Cannot reach QuotaService: {ex.Status.Detail}",
                exception: ex,
                data: data);
        }
    }
}
```

### 5. Response Writers

```csharp
/// <summary>
/// Writers pour les réponses de health check.
/// </summary>
public static class HealthCheckResponseWriters
{
    /// <summary>
    /// Réponse minimale pour liveness (rapide).
    /// </summary>
    public static Task WriteMinimalResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "text/plain";
        
        var status = report.Status switch
        {
            HealthStatus.Healthy => "Healthy",
            HealthStatus.Degraded => "Degraded",
            HealthStatus.Unhealthy => "Unhealthy",
            _ => "Unknown"
        };
        
        return context.Response.WriteAsync(status);
    }
    
    /// <summary>
    /// Réponse détaillée pour readiness.
    /// </summary>
    public static async Task WriteDetailedResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description
            })
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
    
    /// <summary>
    /// Réponse complète avec données (pour monitoring).
    /// </summary>
    public static async Task WriteFullDetailedResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTimeOffset.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                tags = e.Value.Tags,
                data = e.Value.Data,
                exception = e.Value.Exception?.Message
            })
        };
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        await context.Response.WriteAsJsonAsync(response, options);
    }
}
```

### 6. Configuration Kubernetes

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: llmproxy-gateway
spec:
  replicas: 3
  template:
    spec:
      containers:
        - name: gateway
          image: llmproxy/gateway:latest
          ports:
            - containerPort: 8080
          
          # Startup Probe : Attendre que l'app démarre
          startupProbe:
            httpGet:
              path: /health/startup
              port: 8080
            initialDelaySeconds: 5
            periodSeconds: 5
            failureThreshold: 30  # 30 * 5s = 2.5 minutes max pour démarrer
            successThreshold: 1
          
          # Liveness Probe : Vérifier que l'app est vivante
          livenessProbe:
            httpGet:
              path: /health/live
              port: 8080
            initialDelaySeconds: 0  # Commence après startupProbe OK
            periodSeconds: 10
            failureThreshold: 3     # 3 échecs = restart
            successThreshold: 1
            timeoutSeconds: 5
          
          # Readiness Probe : Vérifier que l'app peut recevoir du trafic
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 8080
            initialDelaySeconds: 0
            periodSeconds: 5
            failureThreshold: 3     # 3 échecs = retrait du Service
            successThreshold: 1
            timeoutSeconds: 10      # Plus long car vérifie les dépendances
          
          resources:
            requests:
              memory: "256Mi"
              cpu: "100m"
            limits:
              memory: "1Gi"
              cpu: "500m"
```

### 7. Health Check avec dégradation gracieuse

```csharp
/// <summary>
/// Agrégateur de health checks avec logique de dégradation.
/// </summary>
public sealed class GracefulDegradationHealthCheck : IHealthCheck
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<GracefulDegradationHealthCheck> _logger;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var report = await _healthCheckService.CheckHealthAsync(
            check => check.Tags.Contains("llm"),
            cancellationToken);
        
        var results = report.Entries;
        var healthyProviders = results
            .Where(r => r.Value.Status == HealthStatus.Healthy)
            .Select(r => r.Key)
            .ToList();
        
        var unhealthyProviders = results
            .Where(r => r.Value.Status != HealthStatus.Healthy)
            .Select(r => r.Key)
            .ToList();
        
        var data = new Dictionary<string, object>
        {
            ["healthy_providers"] = healthyProviders,
            ["unhealthy_providers"] = unhealthyProviders,
            ["total_providers"] = results.Count,
            ["available_providers"] = healthyProviders.Count
        };
        
        // Au moins un provider doit être disponible
        if (healthyProviders.Count == 0)
        {
            _logger.LogCritical("All LLM providers are unavailable!");
            
            return HealthCheckResult.Unhealthy(
                "No LLM providers available",
                data: data);
        }
        
        // Mode dégradé si certains providers sont down
        if (unhealthyProviders.Count > 0)
        {
            _logger.LogWarning(
                "Some LLM providers unavailable: {Providers}",
                string.Join(", ", unhealthyProviders));
            
            return HealthCheckResult.Degraded(
                $"Operating with reduced capacity ({healthyProviders.Count}/{results.Count} providers)",
                data: data);
        }
        
        return HealthCheckResult.Healthy(
            "All LLM providers available",
            data: data);
    }
}
```

### 8. UI Health Check Dashboard

```csharp
/// <summary>
/// Configuration du Health Check UI.
/// </summary>
public static class HealthCheckUiExtensions
{
    public static IServiceCollection AddHealthCheckUi(
        this IServiceCollection services)
    {
        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(30);
            options.MaximumHistoryEntriesPerEndpoint(50);
            
            options.AddHealthCheckEndpoint(
                "Gateway",
                "http://localhost:8080/health/ready");
            
            options.AddHealthCheckEndpoint(
                "Admin API",
                "http://localhost:8081/health/ready");
            
            options.AddHealthCheckEndpoint(
                "Quota Service",
                "http://localhost:5001/health/ready");
        })
        .AddInMemoryStorage();
        
        return services;
    }
    
    public static IApplicationBuilder UseHealthCheckUi(
        this IApplicationBuilder app)
    {
        app.UseHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-ui-api";
        });
        
        return app;
    }
}
```

### 9. Métriques de health check

```csharp
/// <summary>
/// Publisher de métriques basé sur les health checks.
/// </summary>
public sealed class HealthCheckMetricsPublisher : IHealthCheckPublisher
{
    private readonly ILogger<HealthCheckMetricsPublisher> _logger;
    private readonly Gauge<int> _healthGauge;
    private readonly Counter<long> _healthCheckCounter;
    
    public HealthCheckMetricsPublisher(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("LLMProxy.Health");
        
        _healthGauge = meter.CreateGauge<int>(
            "health.status",
            description: "Health status (0=unhealthy, 1=degraded, 2=healthy)");
        
        _healthCheckCounter = meter.CreateCounter<long>(
            "health.checks.total",
            description: "Total health checks executed");
    }
    
    public Task PublishAsync(
        HealthReport report,
        CancellationToken cancellationToken)
    {
        // Metric globale
        var statusValue = report.Status switch
        {
            HealthStatus.Healthy => 2,
            HealthStatus.Degraded => 1,
            _ => 0
        };
        
        _healthGauge.Record(statusValue, 
            new KeyValuePair<string, object?>("aggregate", "overall"));
        
        // Metrics par check
        foreach (var entry in report.Entries)
        {
            var checkStatus = entry.Value.Status switch
            {
                HealthStatus.Healthy => 2,
                HealthStatus.Degraded => 1,
                _ => 0
            };
            
            _healthGauge.Record(checkStatus,
                new KeyValuePair<string, object?>("check", entry.Key));
            
            _healthCheckCounter.Add(1,
                new KeyValuePair<string, object?>("check", entry.Key),
                new KeyValuePair<string, object?>("status", entry.Value.Status.ToString()));
        }
        
        return Task.CompletedTask;
    }
}

// Registration
services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(5);
    options.Period = TimeSpan.FromSeconds(30);
});

services.AddSingleton<IHealthCheckPublisher, HealthCheckMetricsPublisher>();
```

## Conséquences

### Positives

- **Résilience** : Kubernetes restart automatique si unhealthy
- **Load balancing** : Traffic routé uniquement vers instances ready
- **Monitoring** : Visibilité sur l'état des dépendances
- **Debugging** : Diagnostic facilité des problèmes

### Négatives

- **Overhead** : Appels périodiques vers les dépendances
  - *Mitigation* : Checks légers, intervalles adaptés
- **Faux positifs** : Dépendance temporairement slow
  - *Mitigation* : Seuils de failure threshold adaptés

### Neutres

- Standard Kubernetes
- Supporté nativement par ASP.NET Core

## Alternatives considérées

### Option A : Health check unique

- **Description** : Un seul endpoint /health
- **Avantages** : Simple
- **Inconvénients** : Pas de distinction live/ready
- **Raison du rejet** : Kubernetes a besoin de la distinction

### Option B : Pas de health check

- **Description** : Kubernetes utilise le port TCP
- **Avantages** : Zero config
- **Inconvénients** : Pas de vérification des dépendances
- **Raison du rejet** : Insuffisant pour la production

## Références

- [ASP.NET Core Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Kubernetes Probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)
- [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
- [Health Check UI](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks#healthchecksui)
