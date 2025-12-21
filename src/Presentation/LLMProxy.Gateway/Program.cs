using LLMProxy.Gateway.Middleware;
using LLMProxy.Infrastructure.Redis;
using LLMProxy.Infrastructure.Security;
using LLMProxy.Infrastructure.LLMProviders;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

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
