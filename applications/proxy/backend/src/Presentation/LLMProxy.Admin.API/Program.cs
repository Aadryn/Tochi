using LLMProxy.Admin.API.Configuration;
using LLMProxy.Application.Extensions;
using LLMProxy.Infrastructure.PostgreSQL;
using LLMProxy.Infrastructure.Redis;
using LLMProxy.Infrastructure.Security;
using LLMProxy.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("llmproxy-admin-api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        // .AddEntityFrameworkCoreInstrumentation()  // Requires OpenTelemetry.Instrumentation.EntityFrameworkCore package
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
        }));

// ═══════════════════════════════════════════════════════════════
// APPLICATION LAYER (ADR-007 Vertical Slice, ADR-013 CQRS)
// ═══════════════════════════════════════════════════════════════
builder.Services.AddApplicationServices();

// Database & Infrastructure
builder.Services.AddDbContext<LLMProxyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? builder.Configuration.GetConnectionString("PostgreSQL")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddRedisInfrastructure(builder.Configuration);
builder.Services.AddSecurityInfrastructure(builder.Configuration);

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TenantAdmin", policy => policy.RequireRole("Admin", "TenantAdmin"));
});

// Add API Versioning avec namespace convention (ADR-037)
builder.Services.AddApiVersioningWithNamespaceConvention();

// Add Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Configuration de base
    c.SwaggerDoc("v1", new() 
    { 
        Title = "LLM Proxy Admin API", 
        Version = "v1",
        Description = @"API d'administration du proxy LLM permettant la gestion des tenants, providers, utilisateurs et API keys.

**Fonctionnalités principales :**
- Gestion multi-tenant
- Configuration des fournisseurs LLM (OpenAI, Anthropic, etc.)
- Gestion des utilisateurs et permissions
- Génération et rotation des API keys

**Sécurité :**
- Authentification JWT Bearer obligatoire
- Rate limiting par IP (100 req/min)
- Health checks Kubernetes-ready",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Support LLM Proxy",
            Email = "support@llmproxy.io"
        }
    });

    // XML Documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Sécurité Bearer Token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 

Entrez 'Bearer' [espace] suivi de votre token dans le champ ci-dessous.

Exemple : `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Tags pour regroupement fonctionnel
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        return new[] { controllerName ?? "Unknown" };
    });

    c.DocInclusionPredicate((name, api) => true);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ═══════════════════════════════════════════════════════════════
// HEALTH CHECKS (ADR-038)
// Liveness, Readiness et Monitoring endpoints pour Kubernetes
// ═══════════════════════════════════════════════════════════════
builder.Services.AddHealthChecks()
    // ═══ LIVENESS : Vérifie que le processus est vivant ═══
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Admin API is alive"), tags: new[] { "live" })
    
    // ═══ READINESS : Vérifie les dépendances critiques ═══
    // PostgreSQL (CRITIQUE : app non fonctionnelle sans DB)
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? builder.Configuration.GetConnectionString("PostgreSQL") ?? throw new InvalidOperationException("PostgreSQL connection string not configured"),
        name: "database",
        healthQuery: "SELECT 1;",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "ready", "db" })
    
    // Redis (DÉGRADÉ : app reste fonctionnelle sans cache)
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection string not configured"),
        name: "redis",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
        tags: new[] { "ready", "cache" })
    
    // ═══ INFRASTRUCTURE : Disk et Memory ═══
    .AddDiskStorageHealthCheck(
        options => options.AddDrive("C:\\", 1024), // 1GB minimum
        name: "disk",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
        tags: new[] { "ready", "infrastructure" })
    
    .AddProcessAllocatedMemoryHealthCheck(
        maximumMegabytesAllocated: 1024, // 1GB max
        name: "memory",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
        tags: new[] { "live", "infrastructure" });

// ═══════════════════════════════════════════════════════════════
// RATE LIMITING (ADR-041)
// Protection contre abus et DDoS
// ═══════════════════════════════════════════════════════════════
var rateLimitOptions = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingOptions>() 
    ?? throw new InvalidOperationException("RateLimiting configuration is missing");
builder.Services.AddSingleton(rateLimitOptions);

builder.Services.AddRateLimiter(options =>
{
    // ═══════════════════════════════════════════════════════════════
    // GLOBAL RATE LIMITER - Protection infrastructure
    // Fixed Window : quotas stricts, prévisibles
    // ═══════════════════════════════════════════════════════════════
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter("global", key =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitOptions.Global.PermitLimit,
                Window = rateLimitOptions.Global.Window,
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
    // REJECTION BEHAVIOR - Réponse HTTP 429 Too Many Requests
    // ═══════════════════════════════════════════════════════════════
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        TimeSpan? retryAfter = null;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue))
        {
            retryAfter = retryAfterValue;
            context.HttpContext.Response.Headers.RetryAfter = retryAfterValue.TotalSeconds.ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            message = "Rate limit exceeded. Please try again later.",
            retryAfter = retryAfter?.TotalSeconds
        }, cancellationToken);
    };
});

var app = builder.Build();

// ═══════════════════════════════════════════════════════════════
// HEALTH CHECKS ENDPOINTS (ADR-038)
// ═══════════════════════════════════════════════════════════════
// Liveness Probe : Vérifie que le processus est actif
// Utilisé par Kubernetes pour restart si échec
app.MapHealthChecks("/health/live", HealthCheckOptionsHelper.CreateLivenessOptions());

// Readiness Probe : Vérifie que toutes les dépendances sont disponibles
// Utilisé par Kubernetes pour le load balancing
app.MapHealthChecks("/health/ready", HealthCheckOptionsHelper.CreateReadinessOptions());

// Monitoring détaillé (tous les checks)
app.MapHealthChecks("/health", HealthCheckOptionsHelper.CreateMonitoringOptions());

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactFrontend");
app.UseRateLimiter(); // ADR-041: Rate Limiting AVANT Authentication
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
