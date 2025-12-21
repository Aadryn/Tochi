using LLMProxy.Gateway.Middleware;
using LLMProxy.Gateway.Configuration;
using LLMProxy.Infrastructure.Redis;
using LLMProxy.Infrastructure.Security;
using LLMProxy.Infrastructure.LLMProviders;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==================== Configuration ====================
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// ==================== Services ====================

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("LLMProxy.Gateway"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("LLMProxy.*")
        .AddConsoleExporter()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("LLMProxy.*")
        .AddConsoleExporter()
        .AddOtlpExporter());

// Add Authentication
builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    })
    .AddCertificate("Certificate", options =>
    {
        options.AllowedCertificateTypes = Microsoft.AspNetCore.Authentication.Certificate.CertificateTypes.All;
    });

// Add Authorization
builder.Services.AddAuthorization();

// Add Health Checks
builder.Services.AddHealthChecks();

// Configure Rate Limiting (ADR-041)
var rateLimitOptions = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingOptions>() 
    ?? throw new InvalidOperationException("RateLimiting configuration is missing");
builder.Services.AddSingleton(rateLimitOptions);

builder.Services.AddRateLimiter(options =>
{
    // ═══════════════════════════════════════════════════════════════
    // GLOBAL RATE LIMITER - Protection infrastructure
    // Token Bucket : permet bursts contrôlés tout en limitant débit moyen
    // ═══════════════════════════════════════════════════════════════
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetTokenBucketLimiter("global", key =>
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = rateLimitOptions.Global.PermitLimit,
                ReplenishmentPeriod = rateLimitOptions.Global.Window,
                TokensPerPeriod = rateLimitOptions.Global.PermitLimit,
                AutoReplenishment = true,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    // ═══════════════════════════════════════════════════════════════
    // PER-TENANT RATE LIMITER - Équité multi-tenant
    // Fixed Window : quotas stricts par tenant
    // ═══════════════════════════════════════════════════════════════
    options.AddPolicy("per-tenant", context =>
    {
        var tenantId = context.Items["TenantId"] as Guid?;
        if (tenantId == null)
        {
            return RateLimitPartition.GetNoLimiter<string>("anonymous");
        }

        return RateLimitPartition.GetFixedWindowLimiter(tenantId.ToString()!, key =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitOptions.PerTenant.PermitLimit,
                Window = rateLimitOptions.PerTenant.Window,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    // ═══════════════════════════════════════════════════════════════
    // PER-USER RATE LIMITER - Protection abus utilisateur
    // Sliding Window : lisse le trafic, évite bursts aux limites de fenêtres
    // ═══════════════════════════════════════════════════════════════
    options.AddPolicy("per-user", context =>
    {
        var userId = context.Items["UserId"] as Guid?;
        if (userId == null)
        {
            return RateLimitPartition.GetNoLimiter<string>("anonymous");
        }

        return RateLimitPartition.GetSlidingWindowLimiter(userId.ToString()!, key =>
            new SlidingWindowRateLimiterOptions
            {
                PermitLimit = rateLimitOptions.PerUser.PermitLimit,
                Window = rateLimitOptions.PerUser.Window,
                SegmentsPerWindow = rateLimitOptions.PerUser.SegmentsPerWindow,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    // ═══════════════════════════════════════════════════════════════
    // PER-IP RATE LIMITER - Protection DDoS
    // Fixed Window : limite par adresse IP client
    // ═══════════════════════════════════════════════════════════════
    options.AddPolicy("per-ip", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        return RateLimitPartition.GetFixedWindowLimiter(ip, key =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitOptions.PerIp.PermitLimit,
                Window = rateLimitOptions.PerIp.Window,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    // ═══════════════════════════════════════════════════════════════
    // CONCURRENCY LIMITER - Protection ressources serveur
    // Limite connexions simultanées
    // ═══════════════════════════════════════════════════════════════
    options.AddPolicy("concurrency", context =>
    {
        return RateLimitPartition.GetConcurrencyLimiter("concurrent-requests", key =>
            new ConcurrencyLimiterOptions
            {
                PermitLimit = rateLimitOptions.Concurrency.PermitLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = rateLimitOptions.Concurrency.QueueLimit
            });
    });

    // ═══════════════════════════════════════════════════════════════
    // REJECTION HANDLER - Réponse 429 Too Many Requests
    // Conforme à RFC 6585
    // ═══════════════════════════════════════════════════════════════
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        // Ajouter header Retry-After si disponible
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString("F0");
        }

        // Réponse JSON structurée
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too Many Requests",
            message = "Rate limit exceeded. Please retry after the specified delay.",
            retryAfterSeconds = retryAfter.TotalSeconds
        }, cancellationToken);
    };
});

// Add Infrastructure services
builder.Services.AddRedisInfrastructure(builder.Configuration);
builder.Services.AddSecurityInfrastructure(builder.Configuration);
builder.Services.AddLLMProviderInfrastructure();

// Add Hash Service
builder.Services.AddSingleton<LLMProxy.Infrastructure.Security.IHashService, LLMProxy.Infrastructure.Security.Sha256HashService>();
builder.Services.AddSingleton<LLMProxy.Infrastructure.Security.IApiKeyExtractor, LLMProxy.Infrastructure.Security.HeaderApiKeyExtractor>();
builder.Services.AddSingleton<LLMProxy.Infrastructure.Security.IApiKeyValidator, LLMProxy.Infrastructure.Security.ApiKeyValidator>();
builder.Services.AddSingleton<LLMProxy.Infrastructure.Security.IApiKeyAuthenticator, LLMProxy.Infrastructure.Security.ApiKeyAuthenticator>();

// Add Database (PostgreSQL)
builder.Services.AddDbContext<LLMProxy.Infrastructure.PostgreSQL.LLMProxyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Add Repositories
builder.Services.AddScoped<LLMProxy.Domain.Interfaces.IUnitOfWork, LLMProxy.Infrastructure.PostgreSQL.UnitOfWork>();

// Add Application services (MediatR)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(LLMProxy.Application.Tenants.Commands.CreateTenantCommand).Assembly));

var app = builder.Build();

// ==================== Middleware Pipeline ====================

// Global exception handler MUST be first to catch all exceptions
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Custom middleware for request logging and metrics
app.UseMiddleware<RequestLoggingMiddleware>();

// Authentication MUST come before authorization and quota checks
// This prevents unauthenticated requests from consuming resources
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

// Enrich logging context with business metadata (TenantId, UserId, ApiKeyId)
// MUST be after ApiKeyAuthenticationMiddleware to access authenticated context
app.UseMiddleware<LogContextEnrichmentMiddleware>();

// Rate Limiting (ADR-041) MUST be before QuotaEnforcement
// Order: Auth → RateLimit → Quota → Routing
app.UseRateLimiter();

// Add rate limit headers to responses
app.UseMiddleware<RateLimitHeadersMiddleware>();

app.UseMiddleware<QuotaEnforcementMiddleware>();

// Standard middleware
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapHealthChecks("/health");

// YARP Reverse Proxy
app.MapReverseProxy(proxyPipeline =>
{
    // Add custom transforms for request/response manipulation
    proxyPipeline.Use((context, next) =>
    {
        // Add tracing headers
        context.Request.Headers["X-Request-Id"] = Guid.NewGuid().ToString();
        context.Request.Headers["X-Forwarded-By"] = "LLMProxy";
        
        return next();
    });
    
    // Stream interception middleware for metrics extraction
    proxyPipeline.UseMiddleware<StreamInterceptionMiddleware>();
});

app.Run();
