// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - Extensions Serilog
// ADR-031 : Structured Logging
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using LLMProxy.Gateway.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace LLMProxy.Gateway.Extensions;

/// <summary>
/// Extensions pour la configuration de Serilog dans l'application.
/// Fournit une configuration structurée avec logs JSON, enrichissement contextuel
/// et multiples sinks (Console, File, OpenTelemetry).
/// </summary>
/// <remarks>
/// <para>
/// Cette classe implémente les recommandations de l'ADR-031 (Structured Logging)
/// en configurant Serilog comme provider de logging principal avec :
/// </para>
/// <list type="bullet">
/// <item><description>Logs JSON structurés (CompactJsonFormatter)</description></item>
/// <item><description>Enrichissement automatique (TenantId, RequestId, TraceId)</description></item>
/// <item><description>Rolling file logs avec rotation quotidienne</description></item>
/// <item><description>Filtrage par niveau selon le namespace</description></item>
/// </list>
/// </remarks>
public static class SerilogExtensions
{
    /// <summary>
    /// Configure Serilog comme provider de logging pour l'application.
    /// </summary>
    /// <param name="builder">Le builder de l'application web.</param>
    /// <returns>Le builder configuré pour permettre le chaînage.</returns>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.ConfigureSerilog();
    /// </code>
    /// </example>
    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                // Lecture de la configuration depuis appsettings.json
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                
                // Enrichissement contextuel de base
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "LLMProxy.Gateway")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                
                // Enrichisseurs personnalisés (TenantId, ApiKey, LLMContext)
                .Enrich.With<TenantIdEnricher>()
                .Enrich.With<ApiKeyEnricher>()
                .Enrich.With<LLMContextEnricher>()
                
                // Niveaux minimum par namespace
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Yarp", LogEventLevel.Information)
                
                // Sinks selon l'environnement
                .ConfigureSinks(context);
        });

        return builder;
    }

    /// <summary>
    /// Configure les sinks Serilog selon l'environnement d'exécution.
    /// </summary>
    /// <param name="configuration">La configuration Serilog.</param>
    /// <param name="context">Le contexte de l'hôte.</param>
    private static void ConfigureSinks(
        this LoggerConfiguration configuration,
        HostBuilderContext context)
    {
        var isDevelopment = context.HostingEnvironment.IsDevelopment();
        var logsPath = context.Configuration["Serilog:LogsPath"] ?? "logs";

        // Console : toujours actif, format différent selon environnement
        if (isDevelopment)
        {
            // En développement : format lisible avec couleurs
            configuration.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}      {Message:lj}{NewLine}{Exception}");
        }
        else
        {
            // En production : JSON compact pour agrégation
            configuration.WriteTo.Console(new CompactJsonFormatter());
        }

        // File : logs JSON avec rotation quotidienne
        configuration.WriteTo.File(
            formatter: new CompactJsonFormatter(),
            path: Path.Combine(logsPath, "llmproxy-.json"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            fileSizeLimitBytes: 100 * 1024 * 1024, // 100 MB
            rollOnFileSizeLimit: true,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1));

        // Fichier d'erreurs séparé pour debugging rapide
        configuration.WriteTo.File(
            formatter: new CompactJsonFormatter(),
            path: Path.Combine(logsPath, "llmproxy-errors-.json"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 90,
            restrictedToMinimumLevel: LogEventLevel.Error);
    }

    /// <summary>
    /// Ajoute le middleware de logging des requêtes HTTP Serilog.
    /// </summary>
    /// <param name="app">L'application web.</param>
    /// <returns>L'application configurée pour permettre le chaînage.</returns>
    /// <remarks>
    /// Ce middleware enrichit les logs avec les informations de requête HTTP
    /// et génère un log structuré pour chaque requête avec :
    /// <list type="bullet">
    /// <item><description>Méthode HTTP et chemin</description></item>
    /// <item><description>Code de statut de réponse</description></item>
    /// <item><description>Durée de traitement</description></item>
    /// <item><description>Informations contextuelles (TenantId, UserId, etc.)</description></item>
    /// </list>
    /// </remarks>
    public static IApplicationBuilder UseSerilogRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            // Format du message pour chaque requête
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            // Enrichissement supplémentaire du log de requête
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                // Informations de base de la requête
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());

                // Identifiants de corrélation
                if (httpContext.Request.Headers.TryGetValue("X-Request-Id", out var requestId))
                {
                    diagnosticContext.Set("RequestId", requestId.ToString());
                }

                if (httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
                {
                    diagnosticContext.Set("CorrelationId", correlationId.ToString());
                }

                // Informations du tenant et utilisateur (depuis le contexte)
                if (httpContext.Items.TryGetValue("TenantId", out var tenantId) && tenantId is Guid tid)
                {
                    diagnosticContext.Set("TenantId", tid);
                }

                if (httpContext.Items.TryGetValue("UserId", out var userId) && userId is Guid uid)
                {
                    diagnosticContext.Set("UserId", uid);
                }

                // Taille de la réponse
                diagnosticContext.Set("ResponseContentLength", httpContext.Response.ContentLength ?? 0);
            };

            // Personnalisation du niveau de log selon le statut HTTP
            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex is not null)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 400)
                    return LogEventLevel.Warning;

                // Requêtes lentes (> 3 secondes)
                if (elapsed > 3000)
                    return LogEventLevel.Warning;

                return LogEventLevel.Information;
            };
        });
    }
}
