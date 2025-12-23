# 52. Security Headers pour la protection HTTP

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM exposé sur Internet est vulnérable aux attaques web. Sans headers de sécurité :
- XSS (Cross-Site Scripting)
- Clickjacking
- MIME sniffing
- Man-in-the-middle
- Information disclosure

```csharp
// ❌ SANS SECURITY HEADERS : Réponses non protégées
[HttpGet("data")]
public IActionResult GetData()
{
    // La réponse ne contient aucune protection
    // Vulnérable à de nombreuses attaques
    return Ok(new { data = "sensitive" });
}

// Headers de réponse (dangereux):
// HTTP/1.1 200 OK
// Content-Type: application/json
// Server: Kestrel            ← Information disclosure !
// X-Powered-By: ASP.NET      ← Information disclosure !
// (aucun header de sécurité)
```

## Décision

**Ajouter systématiquement les headers de sécurité HTTP recommandés (OWASP).**

### 1. Middleware de Security Headers

```csharp
/// <summary>
/// Middleware ajoutant les headers de sécurité HTTP.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;
    
    public SecurityHeadersMiddleware(
        RequestDelegate next,
        IOptions<SecurityHeadersOptions> options)
    {
        _next = next;
        _options = options.Value;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Ajouter les headers AVANT le traitement
        AddSecurityHeaders(context.Response.Headers);
        
        // Supprimer les headers révélateurs
        context.Response.OnStarting(() =>
        {
            RemoveInsecureHeaders(context.Response.Headers);
            return Task.CompletedTask;
        });
        
        await _next(context);
    }
    
    private void AddSecurityHeaders(IHeaderDictionary headers)
    {
        // ═══════════════════════════════════════════════════════════
        // 1. X-Content-Type-Options
        // ═══════════════════════════════════════════════════════════
        // Empêche le MIME type sniffing
        headers["X-Content-Type-Options"] = "nosniff";
        
        // ═══════════════════════════════════════════════════════════
        // 2. X-Frame-Options
        // ═══════════════════════════════════════════════════════════
        // Protection contre le clickjacking
        headers["X-Frame-Options"] = _options.FrameOptions; // DENY ou SAMEORIGIN
        
        // ═══════════════════════════════════════════════════════════
        // 3. X-XSS-Protection
        // ═══════════════════════════════════════════════════════════
        // Deprecated mais encore supporté par certains navigateurs
        headers["X-XSS-Protection"] = "1; mode=block";
        
        // ═══════════════════════════════════════════════════════════
        // 4. Strict-Transport-Security (HSTS)
        // ═══════════════════════════════════════════════════════════
        // Force HTTPS pour toutes les requêtes futures
        if (_options.EnableHsts)
        {
            var maxAge = (int)_options.HstsMaxAge.TotalSeconds;
            var hstsValue = $"max-age={maxAge}";
            
            if (_options.HstsIncludeSubdomains)
                hstsValue += "; includeSubDomains";
            
            if (_options.HstsPreload)
                hstsValue += "; preload";
            
            headers["Strict-Transport-Security"] = hstsValue;
        }
        
        // ═══════════════════════════════════════════════════════════
        // 5. Content-Security-Policy (CSP)
        // ═══════════════════════════════════════════════════════════
        // Contrôle strict des sources de contenu
        if (!string.IsNullOrEmpty(_options.ContentSecurityPolicy))
        {
            headers["Content-Security-Policy"] = _options.ContentSecurityPolicy;
        }
        
        // ═══════════════════════════════════════════════════════════
        // 6. Referrer-Policy
        // ═══════════════════════════════════════════════════════════
        // Contrôle les informations envoyées dans le header Referer
        headers["Referrer-Policy"] = _options.ReferrerPolicy;
        
        // ═══════════════════════════════════════════════════════════
        // 7. Permissions-Policy (anciennement Feature-Policy)
        // ═══════════════════════════════════════════════════════════
        // Désactive les fonctionnalités non nécessaires
        if (!string.IsNullOrEmpty(_options.PermissionsPolicy))
        {
            headers["Permissions-Policy"] = _options.PermissionsPolicy;
        }
        
        // ═══════════════════════════════════════════════════════════
        // 8. Cache-Control pour contenu sensible
        // ═══════════════════════════════════════════════════════════
        if (_options.NoCacheForApiResponses)
        {
            headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
            headers["Pragma"] = "no-cache";
            headers["Expires"] = "0";
        }
    }
    
    private void RemoveInsecureHeaders(IHeaderDictionary headers)
    {
        // Supprimer les headers qui révèlent des informations
        headers.Remove("Server");
        headers.Remove("X-Powered-By");
        headers.Remove("X-AspNet-Version");
        headers.Remove("X-AspNetMvc-Version");
    }
}
```

### 2. Configuration des Options

```csharp
/// <summary>
/// Options de configuration des headers de sécurité.
/// </summary>
public sealed class SecurityHeadersOptions
{
    public const string SectionName = "SecurityHeaders";
    
    // X-Frame-Options
    public string FrameOptions { get; set; } = "DENY";
    
    // HSTS
    public bool EnableHsts { get; set; } = true;
    public TimeSpan HstsMaxAge { get; set; } = TimeSpan.FromDays(365);
    public bool HstsIncludeSubdomains { get; set; } = true;
    public bool HstsPreload { get; set; } = false;
    
    // CSP
    public string? ContentSecurityPolicy { get; set; }
    
    // Referrer-Policy
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";
    
    // Permissions-Policy
    public string? PermissionsPolicy { get; set; }
    
    // Cache
    public bool NoCacheForApiResponses { get; set; } = true;
}

/// <summary>
/// Configuration par défaut pour une API.
/// </summary>
public static class SecurityHeadersDefaults
{
    public static SecurityHeadersOptions ForApi()
    {
        return new SecurityHeadersOptions
        {
            FrameOptions = "DENY",
            EnableHsts = true,
            HstsMaxAge = TimeSpan.FromDays(365),
            HstsIncludeSubdomains = true,
            HstsPreload = false, // Activer seulement après validation
            ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'",
            ReferrerPolicy = "no-referrer",
            PermissionsPolicy = "accelerometer=(), camera=(), geolocation=(), gyroscope=(), " +
                               "magnetometer=(), microphone=(), payment=(), usb=()",
            NoCacheForApiResponses = true
        };
    }
    
    public static SecurityHeadersOptions ForWebApp()
    {
        return new SecurityHeadersOptions
        {
            FrameOptions = "SAMEORIGIN",
            EnableHsts = true,
            HstsMaxAge = TimeSpan.FromDays(365),
            ContentSecurityPolicy = BuildWebAppCsp(),
            ReferrerPolicy = "strict-origin-when-cross-origin",
            PermissionsPolicy = "camera=(), microphone=(), geolocation=()",
            NoCacheForApiResponses = false
        };
    }
    
    private static string BuildWebAppCsp()
    {
        return string.Join("; ", new[]
        {
            "default-src 'self'",
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'", // Blazor
            "style-src 'self' 'unsafe-inline'",
            "img-src 'self' data: https:",
            "font-src 'self'",
            "connect-src 'self' wss:",
            "frame-ancestors 'self'",
            "base-uri 'self'",
            "form-action 'self'"
        });
    }
}
```

### 3. Enregistrement et Configuration

```csharp
/// <summary>
/// Extensions pour l'enregistrement des security headers.
/// </summary>
public static class SecurityHeadersExtensions
{
    public static IServiceCollection AddSecurityHeaders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SecurityHeadersOptions>(
            configuration.GetSection(SecurityHeadersOptions.SectionName));
        
        return services;
    }
    
    public static IServiceCollection AddSecurityHeaders(
        this IServiceCollection services,
        Action<SecurityHeadersOptions> configure)
    {
        services.Configure(configure);
        return services;
    }
    
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
    
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder app,
        SecurityHeadersOptions options)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>(Options.Create(options));
    }
}

/// <summary>
/// Configuration dans Program.cs.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configurer les security headers
        builder.Services.AddSecurityHeaders(options =>
        {
            options.FrameOptions = "DENY";
            options.EnableHsts = true;
            options.HstsMaxAge = TimeSpan.FromDays(365);
            options.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
            options.ReferrerPolicy = "no-referrer";
        });
        
        var app = builder.Build();
        
        // IMPORTANT: UseSecurityHeaders doit être AVANT tout autre middleware
        app.UseSecurityHeaders();
        
        // ... reste de la configuration
        
        app.Run();
    }
}
```

### 4. Headers Spécifiques par Endpoint

```csharp
/// <summary>
/// Attribut pour personnaliser les headers par endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SecurityHeadersAttribute : Attribute, IAsyncActionFilter
{
    public string? ContentSecurityPolicy { get; set; }
    public string? FrameOptions { get; set; }
    public bool AllowCaching { get; set; }
    
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var response = context.HttpContext.Response;
        
        response.OnStarting(() =>
        {
            if (ContentSecurityPolicy is not null)
            {
                response.Headers["Content-Security-Policy"] = ContentSecurityPolicy;
            }
            
            if (FrameOptions is not null)
            {
                response.Headers["X-Frame-Options"] = FrameOptions;
            }
            
            if (AllowCaching)
            {
                response.Headers.Remove("Cache-Control");
                response.Headers.Remove("Pragma");
                response.Headers.Remove("Expires");
            }
            
            return Task.CompletedTask;
        });
        
        await next();
    }
}

/// <summary>
/// Exemple d'utilisation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PublicDataController : ControllerBase
{
    // Endpoint avec CSP relaxé pour embed
    [HttpGet("widget")]
    [SecurityHeaders(
        FrameOptions = "ALLOW-FROM https://trusted-site.com",
        ContentSecurityPolicy = "frame-ancestors https://trusted-site.com")]
    public IActionResult GetWidget()
    {
        return Ok(new { widget = "data" });
    }
    
    // Endpoint avec caching autorisé
    [HttpGet("static")]
    [SecurityHeaders(AllowCaching = true)]
    [ResponseCache(Duration = 3600)]
    public IActionResult GetStatic()
    {
        return Ok(new { version = "1.0" });
    }
}
```

### 5. Content Security Policy Builder

```csharp
/// <summary>
/// Builder fluent pour Content Security Policy.
/// </summary>
public sealed class CspBuilder
{
    private readonly Dictionary<string, HashSet<string>> _directives = new();
    
    public CspBuilder DefaultSrc(params string[] sources)
        => AddDirective("default-src", sources);
    
    public CspBuilder ScriptSrc(params string[] sources)
        => AddDirective("script-src", sources);
    
    public CspBuilder StyleSrc(params string[] sources)
        => AddDirective("style-src", sources);
    
    public CspBuilder ImgSrc(params string[] sources)
        => AddDirective("img-src", sources);
    
    public CspBuilder FontSrc(params string[] sources)
        => AddDirective("font-src", sources);
    
    public CspBuilder ConnectSrc(params string[] sources)
        => AddDirective("connect-src", sources);
    
    public CspBuilder FrameAncestors(params string[] sources)
        => AddDirective("frame-ancestors", sources);
    
    public CspBuilder FrameSrc(params string[] sources)
        => AddDirective("frame-src", sources);
    
    public CspBuilder BaseUri(params string[] sources)
        => AddDirective("base-uri", sources);
    
    public CspBuilder FormAction(params string[] sources)
        => AddDirective("form-action", sources);
    
    public CspBuilder UpgradeInsecureRequests()
        => AddDirective("upgrade-insecure-requests", Array.Empty<string>());
    
    public CspBuilder BlockAllMixedContent()
        => AddDirective("block-all-mixed-content", Array.Empty<string>());
    
    private CspBuilder AddDirective(string directive, string[] sources)
    {
        if (!_directives.TryGetValue(directive, out var existing))
        {
            existing = new HashSet<string>();
            _directives[directive] = existing;
        }
        
        foreach (var source in sources)
        {
            existing.Add(source);
        }
        
        return this;
    }
    
    public string Build()
    {
        var parts = _directives.Select(kvp =>
        {
            var sources = string.Join(" ", kvp.Value);
            return string.IsNullOrEmpty(sources)
                ? kvp.Key
                : $"{kvp.Key} {sources}";
        });
        
        return string.Join("; ", parts);
    }
    
    // Sources prédéfinies
    public static class Sources
    {
        public const string Self = "'self'";
        public const string None = "'none'";
        public const string UnsafeInline = "'unsafe-inline'";
        public const string UnsafeEval = "'unsafe-eval'";
        public const string Data = "data:";
        public const string Blob = "blob:";
        public const string Https = "https:";
        public const string Wss = "wss:";
    }
}

/// <summary>
/// Exemple d'utilisation du builder.
/// </summary>
public static class CspExamples
{
    public static string BuildApiCsp()
    {
        return new CspBuilder()
            .DefaultSrc(CspBuilder.Sources.None)
            .FrameAncestors(CspBuilder.Sources.None)
            .Build();
        // Résultat: "default-src 'none'; frame-ancestors 'none'"
    }
    
    public static string BuildWebAppCsp()
    {
        return new CspBuilder()
            .DefaultSrc(CspBuilder.Sources.Self)
            .ScriptSrc(CspBuilder.Sources.Self, CspBuilder.Sources.UnsafeInline)
            .StyleSrc(CspBuilder.Sources.Self, CspBuilder.Sources.UnsafeInline)
            .ImgSrc(CspBuilder.Sources.Self, CspBuilder.Sources.Data, CspBuilder.Sources.Https)
            .FontSrc(CspBuilder.Sources.Self)
            .ConnectSrc(CspBuilder.Sources.Self, CspBuilder.Sources.Wss)
            .FrameAncestors(CspBuilder.Sources.Self)
            .BaseUri(CspBuilder.Sources.Self)
            .FormAction(CspBuilder.Sources.Self)
            .UpgradeInsecureRequests()
            .Build();
    }
}
```

### 6. Tests des Security Headers

```csharp
/// <summary>
/// Tests pour vérifier les headers de sécurité.
/// </summary>
public sealed class SecurityHeadersTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public SecurityHeadersTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Response_ShouldContainXContentTypeOptions()
    {
        var response = await _client.GetAsync("/health");
        
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options")
            .Should().ContainSingle("nosniff");
    }
    
    [Fact]
    public async Task Response_ShouldContainXFrameOptions()
    {
        var response = await _client.GetAsync("/health");
        
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options")
            .Should().ContainSingle("DENY");
    }
    
    [Fact]
    public async Task Response_ShouldContainStrictTransportSecurity()
    {
        var response = await _client.GetAsync("/health");
        
        response.Headers.Should().ContainKey("Strict-Transport-Security");
        var hsts = response.Headers.GetValues("Strict-Transport-Security").First();
        hsts.Should().Contain("max-age=");
    }
    
    [Fact]
    public async Task Response_ShouldContainContentSecurityPolicy()
    {
        var response = await _client.GetAsync("/health");
        
        response.Headers.Should().ContainKey("Content-Security-Policy");
    }
    
    [Fact]
    public async Task Response_ShouldNotContainServerHeader()
    {
        var response = await _client.GetAsync("/health");
        
        response.Headers.Should().NotContainKey("Server");
    }
    
    [Fact]
    public async Task Response_ShouldNotContainXPoweredByHeader()
    {
        var response = await _client.GetAsync("/health");
        
        response.Headers.Should().NotContainKey("X-Powered-By");
    }
    
    [Theory]
    [InlineData("/v1/chat/completions")]
    [InlineData("/admin/tenants")]
    public async Task ApiEndpoints_ShouldHaveNoCacheHeaders(string endpoint)
    {
        _client.DefaultRequestHeaders.Add("X-API-Key", "test-key");
        
        var response = await _client.GetAsync(endpoint);
        
        response.Headers.CacheControl?.NoStore.Should().BeTrue();
    }
}
```

### 7. Configuration appsettings.json

```json
{
  "SecurityHeaders": {
    "FrameOptions": "DENY",
    "EnableHsts": true,
    "HstsMaxAge": "365.00:00:00",
    "HstsIncludeSubdomains": true,
    "HstsPreload": false,
    "ContentSecurityPolicy": "default-src 'none'; frame-ancestors 'none'",
    "ReferrerPolicy": "no-referrer",
    "PermissionsPolicy": "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()",
    "NoCacheForApiResponses": true
  }
}
```

## Conséquences

### Positives

- **Protection XSS** : CSP bloque scripts malveillants
- **Anti-clickjacking** : X-Frame-Options protège contre embed
- **HTTPS forcé** : HSTS élimine downgrade attacks
- **Discrétion** : Pas d'info sur stack technique

### Négatives

- **Compatibilité** : CSP strict peut casser des features
  - *Mitigation* : Mode report-only d'abord
- **Configuration** : Réglages fins nécessaires
  - *Mitigation* : Presets par type d'app

### Neutres

- Standards OWASP respectés
- Compatible tous navigateurs modernes

## Alternatives considérées

### Option A : Reverse proxy uniquement

- **Description** : Headers ajoutés par nginx/cloudflare
- **Avantages** : Pas de code
- **Inconvénients** : Pas de contrôle fin
- **Raison du rejet** : Defense in depth

### Option B : Sans headers

- **Description** : Faire confiance au réseau
- **Avantages** : Simple
- **Inconvénients** : Vulnérable
- **Raison du rejet** : Non acceptable

## Références

- [OWASP Secure Headers](https://owasp.org/www-project-secure-headers/)
- [MDN HTTP Headers](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers)
- [Content Security Policy](https://content-security-policy.com/)
