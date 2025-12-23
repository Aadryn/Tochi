# 53. CORS Policy pour les requêtes cross-origin

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM peut être appelé depuis des applications web tierces. Sans CORS configuré :
- Requêtes bloquées par les navigateurs
- Applications frontend ne peuvent pas intégrer
- Développeurs frustrés
- Ou pire : CORS trop permissif = faille de sécurité

```csharp
// ❌ SANS CORS : Requêtes bloquées
// Erreur console: 
// "Access to fetch at 'https://api.llmproxy.com/v1/chat' from origin 
// 'https://myapp.com' has been blocked by CORS policy"

// ❌ CORS TROP PERMISSIF : Dangereux !
app.UseCors(policy => policy
    .AllowAnyOrigin()      // DANGER: N'importe quel site peut appeler l'API
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());  // ERREUR: AllowAnyOrigin + AllowCredentials interdit
```

## Décision

**Configurer CORS de manière stricte avec whitelist d'origines et policies par endpoint.**

### 1. Configuration des Policies CORS

```csharp
/// <summary>
/// Configuration CORS dans Program.cs.
/// </summary>
public static class CorsConfiguration
{
    public const string DefaultPolicy = "DefaultCors";
    public const string PublicApiPolicy = "PublicApi";
    public const string AdminPolicy = "AdminOnly";
    public const string StreamingPolicy = "Streaming";
    
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>() ?? new CorsOptions();
        
        services.AddCors(corsOptions =>
        {
            // ═══════════════════════════════════════════════════════════
            // Policy par défaut - Origines whitelistées
            // ═══════════════════════════════════════════════════════════
            corsOptions.AddPolicy(DefaultPolicy, policy =>
            {
                if (options.AllowedOrigins.Any())
                {
                    policy.WithOrigins(options.AllowedOrigins.ToArray())
                          .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                          .WithHeaders(
                              "Content-Type",
                              "Authorization",
                              "X-API-Key",
                              "X-Request-Id",
                              "X-Correlation-Id")
                          .WithExposedHeaders(
                              "X-Request-Id",
                              "X-RateLimit-Limit",
                              "X-RateLimit-Remaining",
                              "X-RateLimit-Reset")
                          .SetPreflightMaxAge(TimeSpan.FromHours(1));
                    
                    if (options.AllowCredentials)
                    {
                        policy.AllowCredentials();
                    }
                }
                else
                {
                    // Pas d'origines configurées = pas de CORS
                    policy.SetIsOriginAllowed(_ => false);
                }
            });
            
            // ═══════════════════════════════════════════════════════════
            // Policy API publique - Plus restrictive
            // ═══════════════════════════════════════════════════════════
            corsOptions.AddPolicy(PublicApiPolicy, policy =>
            {
                policy.WithOrigins(options.PublicApiOrigins.ToArray())
                      .WithMethods("POST") // Seulement POST pour chat
                      .WithHeaders("Content-Type", "X-API-Key")
                      .WithExposedHeaders("X-Request-Id")
                      .SetPreflightMaxAge(TimeSpan.FromHours(24));
            });
            
            // ═══════════════════════════════════════════════════════════
            // Policy Admin - Très restrictive
            // ═══════════════════════════════════════════════════════════
            corsOptions.AddPolicy(AdminPolicy, policy =>
            {
                policy.WithOrigins(options.AdminOrigins.ToArray())
                      .WithMethods("GET", "POST", "PUT", "DELETE")
                      .WithHeaders("Content-Type", "Authorization")
                      .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
            
            // ═══════════════════════════════════════════════════════════
            // Policy Streaming - SSE spécifique
            // ═══════════════════════════════════════════════════════════
            corsOptions.AddPolicy(StreamingPolicy, policy =>
            {
                policy.WithOrigins(options.AllowedOrigins.ToArray())
                      .WithMethods("POST", "GET")
                      .WithHeaders("Content-Type", "X-API-Key", "Accept")
                      .WithExposedHeaders("Content-Type")
                      .SetPreflightMaxAge(TimeSpan.FromHours(1));
            });
        });
        
        return services;
    }
    
    public static IApplicationBuilder UseCorsConfiguration(
        this IApplicationBuilder app)
    {
        // Policy par défaut
        return app.UseCors(DefaultPolicy);
    }
}
```

### 2. Options de Configuration

```csharp
/// <summary>
/// Options de configuration CORS.
/// </summary>
public sealed class CorsOptions
{
    public const string SectionName = "Cors";
    
    /// <summary>
    /// Origines autorisées pour l'API principale.
    /// </summary>
    /// <example>["https://app.example.com", "https://dashboard.example.com"]</example>
    public List<string> AllowedOrigins { get; set; } = new();
    
    /// <summary>
    /// Origines autorisées pour l'API publique (plus large).
    /// </summary>
    public List<string> PublicApiOrigins { get; set; } = new();
    
    /// <summary>
    /// Origines autorisées pour l'interface admin.
    /// </summary>
    public List<string> AdminOrigins { get; set; } = new();
    
    /// <summary>
    /// Autoriser les credentials (cookies, auth headers).
    /// </summary>
    public bool AllowCredentials { get; set; } = false;
    
    /// <summary>
    /// Mode développement : autoriser localhost.
    /// </summary>
    public bool AllowLocalhost { get; set; } = false;
}

/// <summary>
/// Configuration dans appsettings.json.
/// </summary>
/*
{
  "Cors": {
    "AllowedOrigins": [
      "https://app.example.com",
      "https://dashboard.example.com"
    ],
    "PublicApiOrigins": [
      "https://app.example.com",
      "https://partner1.com",
      "https://partner2.com"
    ],
    "AdminOrigins": [
      "https://admin.example.com"
    ],
    "AllowCredentials": true,
    "AllowLocalhost": false
  }
}
*/
```

### 3. Application des Policies par Controller

```csharp
/// <summary>
/// Controller avec policy CORS spécifique.
/// </summary>
[ApiController]
[Route("v1/chat")]
[EnableCors(CorsConfiguration.PublicApiPolicy)]
public class ChatController : ControllerBase
{
    /// <summary>
    /// Chat completion - Accessible depuis les origines publiques.
    /// </summary>
    [HttpPost("completions")]
    public async Task<IActionResult> CreateCompletion(
        [FromBody] ChatCompletionRequest request)
    {
        // ...
    }
    
    /// <summary>
    /// Streaming - Policy dédiée.
    /// </summary>
    [HttpPost("completions/stream")]
    [EnableCors(CorsConfiguration.StreamingPolicy)]
    public async Task StreamCompletion(
        [FromBody] ChatCompletionRequest request)
    {
        // ...
    }
}

/// <summary>
/// Controller admin avec policy restrictive.
/// </summary>
[ApiController]
[Route("admin")]
[EnableCors(CorsConfiguration.AdminPolicy)]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants()
    {
        // ...
    }
}

/// <summary>
/// Controller interne sans CORS.
/// </summary>
[ApiController]
[Route("_internal")]
[DisableCors]
public class InternalController : ControllerBase
{
    // Accessible uniquement depuis le réseau interne
    [HttpGet("metrics")]
    public IActionResult GetMetrics()
    {
        // ...
    }
}
```

### 4. Validation Dynamique des Origines

```csharp
/// <summary>
/// Service de validation dynamique des origines.
/// </summary>
public interface IOriginValidator
{
    Task<bool> IsAllowedAsync(string origin, CancellationToken ct = default);
}

public sealed class DatabaseOriginValidator : IOriginValidator
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    
    public async Task<bool> IsAllowedAsync(string origin, CancellationToken ct = default)
    {
        var cacheKey = $"cors:origin:{origin}";
        
        if (_cache.TryGetValue(cacheKey, out bool isAllowed))
        {
            return isAllowed;
        }
        
        // Vérifier si l'origine est enregistrée pour un tenant
        isAllowed = await _tenantRepository.HasAllowedOriginAsync(origin, ct);
        
        _cache.Set(cacheKey, isAllowed, CacheDuration);
        
        return isAllowed;
    }
}

/// <summary>
/// Configuration avec validation dynamique.
/// </summary>
public static class DynamicCorsConfiguration
{
    public static IServiceCollection AddDynamicCors(
        this IServiceCollection services)
    {
        services.AddSingleton<IOriginValidator, DatabaseOriginValidator>();
        
        services.AddCors(options =>
        {
            options.AddPolicy("Dynamic", policy =>
            {
                policy.SetIsOriginAllowedToAllowWildcardSubdomains()
                      .SetIsOriginAllowed(origin =>
                      {
                          // Validation synchrone avec cache
                          using var scope = services.BuildServiceProvider().CreateScope();
                          var validator = scope.ServiceProvider
                              .GetRequiredService<IOriginValidator>();
                          
                          return validator.IsAllowedAsync(origin).GetAwaiter().GetResult();
                      })
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });
        
        return services;
    }
}
```

### 5. Gestion des Subdomains Wildcard

```csharp
/// <summary>
/// Policy avec wildcard pour subdomains.
/// </summary>
public static class WildcardCorsExtensions
{
    public static CorsPolicyBuilder WithWildcardOrigins(
        this CorsPolicyBuilder builder,
        params string[] patterns)
    {
        // Patterns comme "https://*.example.com"
        return builder.SetIsOriginAllowed(origin =>
        {
            var originUri = new Uri(origin);
            
            foreach (var pattern in patterns)
            {
                if (IsWildcardMatch(originUri, pattern))
                {
                    return true;
                }
            }
            
            return false;
        });
    }
    
    private static bool IsWildcardMatch(Uri origin, string pattern)
    {
        // Pattern: https://*.example.com
        if (!pattern.Contains("*"))
        {
            return origin.ToString().TrimEnd('/') == pattern.TrimEnd('/');
        }
        
        var patternUri = new Uri(pattern.Replace("*", "wildcard"));
        
        // Vérifier le scheme
        if (origin.Scheme != patternUri.Scheme)
            return false;
        
        // Vérifier le port
        if (origin.Port != patternUri.Port)
            return false;
        
        // Vérifier le host avec wildcard
        var patternHost = patternUri.Host.Replace("wildcard", "*");
        var regex = "^" + Regex.Escape(patternHost).Replace("\\*", "[a-zA-Z0-9-]+") + "$";
        
        return Regex.IsMatch(origin.Host, regex);
    }
}

/// <summary>
/// Exemple d'utilisation.
/// </summary>
services.AddCors(options =>
{
    options.AddPolicy("Partners", policy =>
    {
        policy.WithWildcardOrigins(
                "https://*.partner1.com",
                "https://*.partner2.com",
                "https://app.example.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### 6. Middleware de Logging CORS

```csharp
/// <summary>
/// Middleware pour logger les requêtes CORS bloquées.
/// </summary>
public sealed class CorsLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorsLoggingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var origin = context.Request.Headers.Origin.ToString();
        
        if (!string.IsNullOrEmpty(origin))
        {
            context.Response.OnCompleted(() =>
            {
                var hasAcaoHeader = context.Response.Headers
                    .ContainsKey("Access-Control-Allow-Origin");
                
                if (!hasAcaoHeader)
                {
                    _logger.LogWarning(
                        "CORS blocked request from origin {Origin} to {Path}. " +
                        "Method: {Method}, Status: {Status}",
                        origin,
                        context.Request.Path,
                        context.Request.Method,
                        context.Response.StatusCode);
                }
                else
                {
                    _logger.LogDebug(
                        "CORS allowed request from origin {Origin} to {Path}",
                        origin,
                        context.Request.Path);
                }
                
                return Task.CompletedTask;
            });
        }
        
        await _next(context);
    }
}
```

### 7. Tests CORS

```csharp
/// <summary>
/// Tests de configuration CORS.
/// </summary>
public sealed class CorsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public CorsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Theory]
    [InlineData("https://app.example.com")]
    [InlineData("https://dashboard.example.com")]
    public async Task AllowedOrigin_ShouldReturnCorsHeaders(string origin)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/v1/chat/completions");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type, X-API-Key");
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");
        response.Headers.GetValues("Access-Control-Allow-Origin")
            .Should().ContainSingle(origin);
    }
    
    [Theory]
    [InlineData("https://malicious-site.com")]
    [InlineData("https://attacker.com")]
    public async Task DisallowedOrigin_ShouldNotReturnCorsHeaders(string origin)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/v1/chat/completions");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        response.Headers.Should().NotContainKey("Access-Control-Allow-Origin");
    }
    
    [Fact]
    public async Task PreflightRequest_ShouldReturnAllowedMethods()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/v1/chat/completions");
        request.Headers.Add("Origin", "https://app.example.com");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        response.Headers.GetValues("Access-Control-Allow-Methods")
            .Should().Contain("POST");
    }
    
    [Fact]
    public async Task InternalEndpoint_ShouldNotHaveCors()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/_internal/metrics");
        request.Headers.Add("Origin", "https://app.example.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        response.Headers.Should().NotContainKey("Access-Control-Allow-Origin");
    }
}
```

### 8. Configuration Environnement

```json
// appsettings.Development.json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "https://localhost:7001"
    ],
    "AllowLocalhost": true,
    "AllowCredentials": true
  }
}

// appsettings.Production.json
{
  "Cors": {
    "AllowedOrigins": [
      "https://app.llmproxy.com",
      "https://dashboard.llmproxy.com"
    ],
    "PublicApiOrigins": [
      "https://app.llmproxy.com",
      "https://*.partner-domain.com"
    ],
    "AdminOrigins": [
      "https://admin.llmproxy.com"
    ],
    "AllowLocalhost": false,
    "AllowCredentials": true
  }
}
```

## Conséquences

### Positives

- **Sécurité** : Seules les origines autorisées accèdent
- **Flexibilité** : Policies par endpoint
- **Monitoring** : Logging des requêtes bloquées
- **Standards** : Respect des specs CORS

### Négatives

- **Configuration** : Maintenance de la whitelist
  - *Mitigation* : Configuration dynamique en DB
- **Debugging** : Erreurs CORS parfois obscures
  - *Mitigation* : Logging détaillé

### Neutres

- Standard W3C
- Géré par tous les navigateurs modernes

## Alternatives considérées

### Option A : AllowAnyOrigin

- **Description** : Autoriser toutes les origines
- **Avantages** : Simple
- **Inconvénients** : Faille de sécurité majeure
- **Raison du rejet** : Inacceptable

### Option B : CORS géré par reverse proxy

- **Description** : nginx/cloudflare gère CORS
- **Avantages** : Pas de code
- **Inconvénients** : Moins de contrôle fin
- **Raison du rejet** : Besoin de policies par endpoint

## Références

- [MDN CORS](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS)
- [ASP.NET Core CORS](https://docs.microsoft.com/en-us/aspnet/core/security/cors)
- [OWASP CORS](https://owasp.org/www-community/attacks/CORS_OriginHeaderScrutiny)
