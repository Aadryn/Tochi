using Authorization.API.Configuration;
using Authorization.API.Middleware;
using Authorization.Application;
using Authorization.Infrastructure.OpenFGA;
using Authorization.Infrastructure.PostgreSQL;
using Authorization.Infrastructure.Redis;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// ===== Configuration =====
builder.Services.Configure<JwtConfiguration>(
    builder.Configuration.GetSection(JwtConfiguration.SectionName));

// ===== Infrastructure Layers =====
builder.Services.AddOpenFgaAuthorization(builder.Configuration);
builder.Services.AddPostgreSqlAudit(builder.Configuration);
builder.Services.AddRedisCache(builder.Configuration);

// ===== Application Layer =====
builder.Services.AddAuthorizationApplication();

// ===== API =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

// ===== Authentication (JWT depuis IDP) =====
var jwtConfig = builder.Configuration
    .GetSection(JwtConfiguration.SectionName)
    .Get<JwtConfiguration>();

if (jwtConfig != null)
{
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = jwtConfig.Authority;
            options.Audience = jwtConfig.Audience;
            options.RequireHttpsMetadata = jwtConfig.RequireHttpsMetadata;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience
            };
        });
}
else
{
    // Fallback pour le développement sans IDP
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer");
}

builder.Services.AddAuthorization();

// ===== Health Checks =====
var healthChecksBuilder = builder.Services.AddHealthChecks();

var dbConnectionString = builder.Configuration.GetConnectionString("AuthorizationDb");
if (!string.IsNullOrEmpty(dbConnectionString))
{
    healthChecksBuilder.AddNpgSql(dbConnectionString, name: "postgresql");
}

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    healthChecksBuilder.AddRedis(redisConnectionString, name: "redis");
}

// ===== OpenTelemetry =====
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "Authorization.API",
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter());

// ===== Build Application =====
var app = builder.Build();

// ===== Middleware Pipeline =====
app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Authorization API v1");
        options.RoutePrefix = string.Empty; // Swagger à la racine
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint("/metrics");

// ===== Database Migration =====
if (args.Contains("--migrate") || app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetService<Authorization.Infrastructure.PostgreSQL.Data.AuthorizationAuditDbContext>();

    if (dbContext != null)
    {
        app.Logger.LogInformation("Applying database migrations...");
        await dbContext.Database.EnsureCreatedAsync();
        app.Logger.LogInformation("Database migrations applied.");
    }
}

app.Run();
