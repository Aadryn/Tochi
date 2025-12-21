# 41. Rate Limiting et Throttling

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM expose des APIs coûteuses aux providers externes (OpenAI, Anthropic, etc.). Sans limitation :
- Un client peut monopoliser les ressources
- Les coûts explosent
- Les providers en amont peuvent bloquer le service
- Les autres clients sont impactés (noisy neighbor)

```csharp
// ❌ SANS RATE LIMITING : Aucune protection
public async Task<IActionResult> ChatCompletion([FromBody] ChatRequest request)
{
    // N'importe qui peut envoyer des milliers de requêtes
    // Aucune limite par tenant, par IP, par API key
    // Coûts non maîtrisés
    return await _llmService.CompleteAsync(request);
}
```

### Problèmes sans Rate Limiting

```
┌─────────────────────────────────────────────────────────────────┐
│                        SANS RATE LIMITING                       │
│                                                                 │
│  Client A ────────────────►  ┌─────────┐  ────────────►  API    │
│  (1000 req/s)               │         │                Provider │
│                              │  Proxy  │                        │
│  Client B ──────►            │  LLM    │  Client B attend       │
│  (légitime)                  │         │  indéfiniment          │
│                              └─────────┘                        │
│                                                                 │
│  Résultat :                                                     │
│  • Client A monopolise                                          │
│  • Client B dégradé                                             │
│  • Coûts non contrôlés                                          │
│  • Provider peut bloquer                                        │
└─────────────────────────────────────────────────────────────────┘
```

## Décision

**Implémenter un système de Rate Limiting multi-niveau avec différentes stratégies selon le contexte.**

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      RATE LIMITING MULTI-NIVEAU                 │
│                                                                 │
│  Niveau 1: Global                                               │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  IP Rate Limiter (protection DDoS)                         │ │
│  │  • 100 req/min par IP                                      │ │
│  │  • Sliding Window                                          │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                  │
│                              ▼                                  │
│  Niveau 2: Tenant                                               │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Tenant Rate Limiter (fair share)                          │ │
│  │  • Configurable par tenant                                 │ │
│  │  • Token Bucket                                            │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                  │
│                              ▼                                  │
│  Niveau 3: API Key                                              │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  API Key Rate Limiter (granular)                           │ │
│  │  • Limites par clé                                         │ │
│  │  • Fixed Window                                            │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                  │
│                              ▼                                  │
│  Niveau 4: Endpoint                                             │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Endpoint Rate Limiter (par opération)                     │ │
│  │  • /chat/completions: 60 req/min                           │ │
│  │  • /embeddings: 1000 req/min                               │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### 1. Algorithmes de Rate Limiting

```csharp
/// <summary>
/// Algorithmes de rate limiting disponibles.
/// </summary>

// ═══════════════════════════════════════════════════════════════
// FIXED WINDOW
// ═══════════════════════════════════════════════════════════════
// Simple : compteur reset à intervalle fixe
// Inconvénient : burst possible aux limites de fenêtres
//
// |──────────────|──────────────|──────────────|
// |  Window 1    |  Window 2    |  Window 3    |
// |  100 req     |  100 req     |  100 req     |
// |──────────────|──────────────|──────────────|
// 
// Burst possible : 100 req fin Window 1 + 100 req début Window 2

// ═══════════════════════════════════════════════════════════════
// SLIDING WINDOW
// ═══════════════════════════════════════════════════════════════
// Fenêtre glissante : lisse le trafic
// Plus précis, évite les bursts aux limites
//
// Time: ─────────────────────────────────────────►
//       |←───── 1 minute ─────►|
//                |←───── 1 minute ─────►|
//                        |←───── 1 minute ─────►|

// ═══════════════════════════════════════════════════════════════
// TOKEN BUCKET
// ═══════════════════════════════════════════════════════════════
// Seau avec tokens régénérés
// Permet des bursts contrôlés
//
// ┌─────────────────┐
// │   Token Bucket  │
// │                 │  ← Tokens ajoutés régulièrement
// │  ○ ○ ○ ○ ○     │
// │  ○ ○ ○         │  Capacité max : 10
// │  ○             │  Rate : 1 token/seconde
// └────────┬───────┘
//          │
//          ▼ 1 token consommé par requête

// ═══════════════════════════════════════════════════════════════
// LEAKY BUCKET
// ═══════════════════════════════════════════════════════════════
// Seau qui fuit à débit constant
// Lisse parfaitement le trafic sortant
//
// Requêtes ──► ┌───────────┐ ──► Traitement
//              │           │     (débit constant)
//              │  Queue    │
//              │           │
//              └───────────┘
```

### 2. Implémentation avec Redis (distribuée)

```csharp
/// <summary>
/// Service de rate limiting distribué avec Redis.
/// </summary>
public sealed class RedisRateLimiter : IRateLimiter
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateLimiter> _logger;
    
    /// <summary>
    /// Sliding Window Rate Limiter.
    /// </summary>
    public async Task<RateLimitResult> CheckSlidingWindowAsync(
        string key,
        int limit,
        TimeSpan window,
        CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowStart = now - (long)window.TotalMilliseconds;
        
        // Script Lua pour atomicité
        const string script = @"
            local key = KEYS[1]
            local now = tonumber(ARGV[1])
            local window_start = tonumber(ARGV[2])
            local limit = tonumber(ARGV[3])
            local window_ms = tonumber(ARGV[4])
            
            -- Supprimer les entrées expirées
            redis.call('ZREMRANGEBYSCORE', key, '-inf', window_start)
            
            -- Compter les requêtes dans la fenêtre
            local count = redis.call('ZCARD', key)
            
            if count < limit then
                -- Ajouter la requête actuelle
                redis.call('ZADD', key, now, now .. '-' .. math.random())
                redis.call('PEXPIRE', key, window_ms)
                return {1, count + 1, limit}
            else
                return {0, count, limit}
            end
        ";
        
        var result = (RedisResult[])await db.ScriptEvaluateAsync(
            script,
            new RedisKey[] { key },
            new RedisValue[] { now, windowStart, limit, (long)window.TotalMilliseconds });
        
        var allowed = (int)result[0] == 1;
        var current = (int)result[1];
        var maximum = (int)result[2];
        
        return new RateLimitResult
        {
            IsAllowed = allowed,
            CurrentCount = current,
            Limit = maximum,
            Window = window,
            RetryAfter = allowed ? null : TimeSpan.FromMilliseconds(
                (long)window.TotalMilliseconds - (now - windowStart))
        };
    }
    
    /// <summary>
    /// Token Bucket Rate Limiter.
    /// </summary>
    public async Task<RateLimitResult> CheckTokenBucketAsync(
        string key,
        int capacity,
        int tokensPerInterval,
        TimeSpan interval,
        int tokensRequired = 1,
        CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        const string script = @"
            local key = KEYS[1]
            local capacity = tonumber(ARGV[1])
            local tokens_per_interval = tonumber(ARGV[2])
            local interval_ms = tonumber(ARGV[3])
            local tokens_required = tonumber(ARGV[4])
            local now = tonumber(ARGV[5])
            
            local bucket = redis.call('HMGET', key, 'tokens', 'last_refill')
            local tokens = tonumber(bucket[1]) or capacity
            local last_refill = tonumber(bucket[2]) or now
            
            -- Calculer les tokens à ajouter
            local elapsed = now - last_refill
            local intervals = math.floor(elapsed / interval_ms)
            local new_tokens = intervals * tokens_per_interval
            tokens = math.min(capacity, tokens + new_tokens)
            
            if tokens >= tokens_required then
                tokens = tokens - tokens_required
                redis.call('HMSET', key, 'tokens', tokens, 'last_refill', now)
                redis.call('PEXPIRE', key, interval_ms * 2)
                return {1, tokens, capacity}
            else
                return {0, tokens, capacity}
            end
        ";
        
        var result = (RedisResult[])await db.ScriptEvaluateAsync(
            script,
            new RedisKey[] { key },
            new RedisValue[] { capacity, tokensPerInterval, 
                (long)interval.TotalMilliseconds, tokensRequired, now });
        
        var allowed = (int)result[0] == 1;
        var remainingTokens = (int)result[1];
        var maxTokens = (int)result[2];
        
        return new RateLimitResult
        {
            IsAllowed = allowed,
            CurrentCount = maxTokens - remainingTokens,
            Limit = maxTokens,
            Window = interval,
            RemainingTokens = remainingTokens,
            RetryAfter = allowed ? null : interval
        };
    }
}

/// <summary>
/// Résultat d'une vérification de rate limit.
/// </summary>
public sealed record RateLimitResult
{
    public bool IsAllowed { get; init; }
    public int CurrentCount { get; init; }
    public int Limit { get; init; }
    public TimeSpan Window { get; init; }
    public int? RemainingTokens { get; init; }
    public TimeSpan? RetryAfter { get; init; }
}
```

### 3. Configuration par tenant

```csharp
/// <summary>
/// Configuration des rate limits par tenant.
/// </summary>
public sealed class TenantRateLimitConfiguration
{
    public Guid TenantId { get; set; }
    
    /// <summary>
    /// Limites par endpoint.
    /// </summary>
    public Dictionary<string, EndpointLimit> EndpointLimits { get; set; } = new();
    
    /// <summary>
    /// Limite globale du tenant.
    /// </summary>
    public GlobalLimit GlobalLimit { get; set; } = new();
    
    /// <summary>
    /// Limite par API Key.
    /// </summary>
    public ApiKeyLimit ApiKeyLimit { get; set; } = new();
}

public sealed class EndpointLimit
{
    public int RequestsPerMinute { get; set; }
    public int TokensPerMinute { get; set; }
    public int BurstCapacity { get; set; }
}

public sealed class GlobalLimit
{
    public int RequestsPerMinute { get; set; } = 1000;
    public int RequestsPerDay { get; set; } = 100_000;
    public int TokensPerMinute { get; set; } = 100_000;
    public int TokensPerDay { get; set; } = 10_000_000;
}

public sealed class ApiKeyLimit
{
    public int RequestsPerMinute { get; set; } = 100;
    public int TokensPerMinute { get; set; } = 10_000;
}

/// <summary>
/// Service de configuration des rate limits.
/// </summary>
public interface IRateLimitConfigurationService
{
    Task<TenantRateLimitConfiguration> GetConfigurationAsync(
        Guid tenantId, 
        CancellationToken ct = default);
}
```

### 4. Middleware ASP.NET Core

```csharp
/// <summary>
/// Middleware de rate limiting.
/// </summary>
public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimiter _rateLimiter;
    private readonly IRateLimitConfigurationService _configService;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Extraire les identifiants
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var tenantId = context.GetTenantId();
        var apiKeyId = context.GetApiKeyId();
        var endpoint = context.Request.Path.Value ?? "/";
        
        // 2. Niveau 1 : IP Rate Limiting (protection DDoS)
        var ipResult = await CheckIpRateLimitAsync(clientIp);
        if (!ipResult.IsAllowed)
        {
            await WriteRateLimitResponseAsync(context, ipResult, "IP");
            return;
        }
        
        // 3. Niveau 2 : Tenant Rate Limiting
        if (tenantId.HasValue)
        {
            var config = await _configService.GetConfigurationAsync(tenantId.Value);
            
            var tenantResult = await CheckTenantRateLimitAsync(tenantId.Value, config);
            if (!tenantResult.IsAllowed)
            {
                await WriteRateLimitResponseAsync(context, tenantResult, "Tenant");
                return;
            }
            
            // 4. Niveau 3 : API Key Rate Limiting
            if (apiKeyId.HasValue)
            {
                var apiKeyResult = await CheckApiKeyRateLimitAsync(
                    apiKeyId.Value, config.ApiKeyLimit);
                if (!apiKeyResult.IsAllowed)
                {
                    await WriteRateLimitResponseAsync(context, apiKeyResult, "ApiKey");
                    return;
                }
            }
            
            // 5. Niveau 4 : Endpoint Rate Limiting
            if (config.EndpointLimits.TryGetValue(endpoint, out var endpointLimit))
            {
                var endpointResult = await CheckEndpointRateLimitAsync(
                    tenantId.Value, endpoint, endpointLimit);
                if (!endpointResult.IsAllowed)
                {
                    await WriteRateLimitResponseAsync(context, endpointResult, "Endpoint");
                    return;
                }
            }
        }
        
        // Ajouter les headers de rate limit
        AddRateLimitHeaders(context, ipResult);
        
        await _next(context);
    }
    
    private async Task<RateLimitResult> CheckIpRateLimitAsync(string clientIp)
    {
        var key = $"ratelimit:ip:{clientIp}";
        return await _rateLimiter.CheckSlidingWindowAsync(
            key,
            limit: 100,
            window: TimeSpan.FromMinutes(1));
    }
    
    private async Task<RateLimitResult> CheckTenantRateLimitAsync(
        Guid tenantId,
        TenantRateLimitConfiguration config)
    {
        var key = $"ratelimit:tenant:{tenantId}";
        return await _rateLimiter.CheckTokenBucketAsync(
            key,
            capacity: config.GlobalLimit.RequestsPerMinute,
            tokensPerInterval: config.GlobalLimit.RequestsPerMinute / 60,
            interval: TimeSpan.FromSeconds(1));
    }
    
    private async Task<RateLimitResult> CheckApiKeyRateLimitAsync(
        Guid apiKeyId,
        ApiKeyLimit limit)
    {
        var key = $"ratelimit:apikey:{apiKeyId}";
        return await _rateLimiter.CheckSlidingWindowAsync(
            key,
            limit: limit.RequestsPerMinute,
            window: TimeSpan.FromMinutes(1));
    }
    
    private async Task<RateLimitResult> CheckEndpointRateLimitAsync(
        Guid tenantId,
        string endpoint,
        EndpointLimit limit)
    {
        var key = $"ratelimit:endpoint:{tenantId}:{endpoint}";
        return await _rateLimiter.CheckTokenBucketAsync(
            key,
            capacity: limit.BurstCapacity,
            tokensPerInterval: limit.RequestsPerMinute / 60,
            interval: TimeSpan.FromSeconds(1));
    }
    
    private void AddRateLimitHeaders(HttpContext context, RateLimitResult result)
    {
        context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = 
            (result.Limit - result.CurrentCount).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = 
            DateTimeOffset.UtcNow.Add(result.Window).ToUnixTimeSeconds().ToString();
    }
    
    private async Task WriteRateLimitResponseAsync(
        HttpContext context,
        RateLimitResult result,
        string limitType)
    {
        _logger.LogWarning(
            "Rate limit exceeded for {LimitType}. Current: {Current}, Limit: {Limit}",
            limitType,
            result.CurrentCount,
            result.Limit);
        
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";
        
        if (result.RetryAfter.HasValue)
        {
            context.Response.Headers["Retry-After"] = 
                ((int)result.RetryAfter.Value.TotalSeconds).ToString();
        }
        
        var response = new
        {
            error = new
            {
                message = $"Rate limit exceeded ({limitType})",
                type = "rate_limit_error",
                code = "rate_limit_exceeded",
                param = limitType.ToLowerInvariant(),
                limit = result.Limit,
                current = result.CurrentCount,
                retry_after_seconds = result.RetryAfter?.TotalSeconds
            }
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

### 5. Rate Limiting par tokens consommés (pour LLM)

```csharp
/// <summary>
/// Rate limiting basé sur les tokens consommés (pas juste les requêtes).
/// </summary>
public sealed class TokenBasedRateLimiter
{
    private readonly IRateLimiter _rateLimiter;
    
    /// <summary>
    /// Vérifie si le tenant peut consommer le nombre de tokens estimé.
    /// </summary>
    public async Task<RateLimitResult> CheckTokenLimitAsync(
        Guid tenantId,
        int estimatedTokens,
        CancellationToken ct = default)
    {
        var key = $"ratelimit:tokens:{tenantId}";
        
        // Token bucket avec tokens consommés comme coût
        return await _rateLimiter.CheckTokenBucketAsync(
            key,
            capacity: 100_000,        // 100k tokens max en burst
            tokensPerInterval: 10_000, // 10k tokens/minute
            interval: TimeSpan.FromMinutes(1),
            tokensRequired: estimatedTokens);
    }
    
    /// <summary>
    /// Enregistre les tokens réellement consommés après la requête.
    /// </summary>
    public async Task RecordActualTokensAsync(
        Guid tenantId,
        int actualTokens,
        CancellationToken ct = default)
    {
        // Mise à jour des métriques pour facturation/monitoring
        var key = $"usage:tokens:{tenantId}:{DateTime.UtcNow:yyyy-MM}";
        
        await _rateLimiter.IncrementAsync(key, actualTokens);
    }
}
```

### 6. Configuration ASP.NET Core

```csharp
/// <summary>
/// Configuration du rate limiting.
/// </summary>
public static class RateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IRateLimiter, RedisRateLimiter>();
        services.AddScoped<IRateLimitConfigurationService, RateLimitConfigurationService>();
        services.AddScoped<TokenBasedRateLimiter>();
        
        return services;
    }
}

/// <summary>
/// Configuration dans Program.cs.
/// </summary>
public static class RateLimitingApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder app)
    {
        // Rate limiting global (avant auth)
        app.UseMiddleware<RateLimitingMiddleware>();
        
        return app;
    }
}

// Program.cs
app.UseRateLimiting();
app.UseAuthentication();
app.UseAuthorization();
```

## Conséquences

### Positives

- **Protection** : Pas de monopolisation des ressources
- **Équité** : Chaque tenant a sa part équitable
- **Coûts** : Maîtrise des dépenses vers les providers
- **Stabilité** : Service stable sous charge

### Négatives

- **Complexité** : Configuration multi-niveau
  - *Mitigation* : Valeurs par défaut sensées
- **Latence** : Vérification Redis à chaque requête
  - *Mitigation* : Redis local/proche, cache court
- **Faux positifs** : Requêtes légitimes bloquées
  - *Mitigation* : Token bucket pour bursts

### Neutres

- Headers standards (X-RateLimit-*)
- Compatible avec les conventions de l'industrie

## Alternatives considérées

### Option A : Rate limiting en mémoire

- **Description** : Compteurs en mémoire applicative
- **Avantages** : Simple, rapide
- **Inconvénients** : Pas distribué, perdu au restart
- **Raison du rejet** : Incohérent en cluster

### Option B : API Gateway (Kong, NGINX)

- **Description** : Rate limiting au niveau gateway
- **Avantages** : Découplé de l'application
- **Inconvénients** : Moins flexible, config séparée
- **Raison du rejet** : Besoin de logique métier (par tenant)

## Références

- [Rate Limiting - Microsoft](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [Token Bucket Algorithm](https://en.wikipedia.org/wiki/Token_bucket)
- [Stripe Rate Limiting](https://stripe.com/docs/rate-limits)
