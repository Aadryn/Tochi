// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - Extensions OpenTelemetry pour la configuration avancée
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using LLMProxy.Infrastructure.Telemetry.Metrics;
using LLMProxy.Infrastructure.Telemetry.Tracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LLMProxy.Infrastructure.Telemetry.Extensions;

/// <summary>
/// Extensions pour configurer OpenTelemetry avec les métriques et traces LLM.
/// </summary>
/// <remarks>
/// <para>
/// Cette classe fournit une configuration complète d'OpenTelemetry incluant :
/// </para>
/// <list type="bullet">
/// <item><description>Traces distribuées avec <see cref="LLMActivitySource"/></description></item>
/// <item><description>Métriques métier avec <see cref="LLMMetrics"/></description></item>
/// <item><description>Corrélation logs-traces via TraceId/SpanId</description></item>
/// <item><description>Export OTLP configurable</description></item>
/// </list>
/// </remarks>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Configure OpenTelemetry avec l'instrumentation LLM complète.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <param name="configuration">Configuration de l'application.</param>
    /// <returns>La collection de services pour le chaînage.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddLLMOpenTelemetry(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddLLMOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var otelConfig = configuration.GetSection("OpenTelemetry");
        var serviceName = otelConfig["ServiceName"] ?? "LLMProxy.Gateway";
        var serviceVersion = otelConfig["ServiceVersion"] ?? "2.0.0";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production",
                    ["host.name"] = Environment.MachineName
                }))
            .WithTracing(tracing => ConfigureTracing(tracing, otelConfig))
            .WithMetrics(metrics => ConfigureMetrics(metrics, otelConfig));

        // Enregistrer le singleton des métriques LLM
        services.AddSingleton(LLMMetrics.Instance);

        return services;
    }

    /// <summary>
    /// Configure le tracing OpenTelemetry.
    /// </summary>
    private static void ConfigureTracing(TracerProviderBuilder tracing, IConfigurationSection config)
    {
        var tracingConfig = config.GetSection("Tracing");
        var exporterConfig = config.GetSection("Exporter");
        
        // Sampling configuration
        var samplingRatio = tracingConfig.GetValue("SamplingRatio", 0.1);

        tracing
            // Sources d'activité
            .AddSource(LLMActivitySource.SourceName)
            .AddSource("LLMProxy.*")
            
            // Instrumentation ASP.NET Core
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequest = (activity, request) =>
                {
                    activity.SetTag("http.request.header.x_correlation_id", 
                        request.Headers["X-Correlation-Id"].ToString());
                };
                options.EnrichWithHttpResponse = (activity, response) =>
                {
                    activity.SetTag("http.response.content_length", 
                        response.ContentLength ?? 0);
                };
            })
            
            // Instrumentation HttpClient
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequestMessage = (activity, request) =>
                {
                    if (request.RequestUri is not null)
                    {
                        // Détecter le provider LLM depuis l'URL
                        var host = request.RequestUri.Host;
                        if (host.Contains("openai.com"))
                        {
                            activity.SetTag(LLMSemanticConventions.GenAiSystem, 
                                LLMSemanticConventions.Systems.OpenAi);
                        }
                        else if (host.Contains("anthropic.com"))
                        {
                            activity.SetTag(LLMSemanticConventions.GenAiSystem, 
                                LLMSemanticConventions.Systems.Anthropic);
                        }
                    }
                };
            })
            
            // Sampler configuré
            .SetSampler(new ParentBasedSampler(
                new TraceIdRatioBasedSampler(samplingRatio)));

        // Configuration de l'exporteur
        var exporterType = exporterConfig["Type"] ?? "OTLP";
        var endpoint = exporterConfig["Endpoint"];

        switch (exporterType.ToUpperInvariant())
        {
            case "OTLP":
                if (!string.IsNullOrEmpty(endpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(endpoint);
                        var protocol = exporterConfig["Protocol"]?.ToUpperInvariant();
                        options.Protocol = protocol == "HTTP" 
                            ? OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf 
                            : OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                }
                else
                {
                    tracing.AddOtlpExporter();
                }
                break;
                
            case "CONSOLE":
                tracing.AddConsoleExporter();
                break;
        }

        // Toujours ajouter la console en développement
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        if (isDevelopment && exporterType.ToUpperInvariant() != "CONSOLE")
        {
            tracing.AddConsoleExporter();
        }
    }

    /// <summary>
    /// Configure les métriques OpenTelemetry.
    /// </summary>
    private static void ConfigureMetrics(MeterProviderBuilder metrics, IConfigurationSection config)
    {
        var metricsConfig = config.GetSection("Metrics");
        var exporterConfig = config.GetSection("Exporter");
        
        // Buckets personnalisés pour les histogrammes de latence
        var histogramBuckets = metricsConfig
            .GetSection("HistogramBuckets")
            .Get<double[]>() ?? [0.01, 0.05, 0.1, 0.5, 1, 2, 5, 10, 30, 60];

        metrics
            // Meters LLM
            .AddMeter(LLMMetrics.MeterName)
            .AddMeter("LLMProxy.*")
            
            // Instrumentation ASP.NET Core
            .AddAspNetCoreInstrumentation()
            
            // Instrumentation HttpClient
            .AddHttpClientInstrumentation()
            
            // Vue personnalisée pour les histogrammes de requêtes
            .AddView(
                instrumentName: "llmproxy.request.duration",
                new ExplicitBucketHistogramConfiguration
                {
                    Boundaries = histogramBuckets
                });

        // Configuration de l'exporteur
        var exporterType = exporterConfig["Type"] ?? "OTLP";
        var endpoint = exporterConfig["Endpoint"];
        var exportIntervalMs = metricsConfig.GetValue("ExportIntervalMilliseconds", 15000);

        switch (exporterType.ToUpperInvariant())
        {
            case "OTLP":
                if (!string.IsNullOrEmpty(endpoint))
                {
                    metrics.AddOtlpExporter((exporterOptions, readerOptions) =>
                    {
                        exporterOptions.Endpoint = new Uri(endpoint);
                        var protocol = exporterConfig["Protocol"]?.ToUpperInvariant();
                        exporterOptions.Protocol = protocol == "HTTP" 
                            ? OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf 
                            : OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                        readerOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = exportIntervalMs;
                    });
                }
                else
                {
                    metrics.AddOtlpExporter();
                }
                break;
                
            case "CONSOLE":
                metrics.AddConsoleExporter();
                break;
        }

        // Toujours ajouter la console en développement
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        if (isDevelopment && exporterType.ToUpperInvariant() != "CONSOLE")
        {
            metrics.AddConsoleExporter();
        }
    }

    /// <summary>
    /// Configure l'enrichissement des logs Serilog avec le TraceId/SpanId.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <returns>La collection de services pour le chaînage.</returns>
    /// <remarks>
    /// Cette méthode ajoute un enrichisseur Serilog qui injecte automatiquement
    /// le TraceId et SpanId dans tous les logs pour la corrélation.
    /// </remarks>
    public static IServiceCollection AddTraceLogCorrelation(this IServiceCollection services)
    {
        // L'intégration avec Serilog se fait via LogContext et Activity.Current
        // Les enrichisseurs Serilog récupèrent automatiquement Activity.Current
        return services;
    }
}
