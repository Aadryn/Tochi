// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - MediatR Pipeline Behavior pour les métriques de performance
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.Diagnostics;
using System.Diagnostics.Metrics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Common.Behaviors;

/// <summary>
/// Pipeline Behavior MediatR pour collecter des métriques de performance sur chaque requête.
/// Enregistre la durée, le taux de succès/échec et expose les données via OpenTelemetry.
/// </summary>
/// <remarks>
/// <para>
/// Ce behavior collecte les métriques suivantes :
/// </para>
/// <list type="bullet">
/// <item><description><b>llmproxy.cqrs.duration</b> : Histogramme de la durée des requêtes</description></item>
/// <item><description><b>llmproxy.cqrs.count</b> : Compteur de requêtes par type et statut</description></item>
/// </list>
/// <para>
/// Les métriques sont taggées par :
/// </para>
/// <list type="bullet">
/// <item><description><c>request_type</c> : Nom du type de requête</description></item>
/// <item><description><c>category</c> : "Command" ou "Query"</description></item>
/// <item><description><c>status</c> : "success" ou "error"</description></item>
/// </list>
/// </remarks>
/// <typeparam name="TRequest">Type de la requête MediatR.</typeparam>
/// <typeparam name="TResponse">Type de la réponse attendue.</typeparam>
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly Meter Meter = new("LLMProxy.CQRS", "1.0.0");

    private static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>(
        name: "llmproxy.cqrs.duration",
        unit: "ms",
        description: "Durée des requêtes CQRS en millisecondes");

    private static readonly Counter<long> RequestCount = Meter.CreateCounter<long>(
        name: "llmproxy.cqrs.count",
        unit: "{request}",
        description: "Nombre de requêtes CQRS traitées");

    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="PerformanceBehavior{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="logger">Logger pour les alertes de performance.</param>
    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var category = DetermineCategory(requestName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();
            RecordMetrics(requestName, category, "success", stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch
        {
            stopwatch.Stop();
            RecordMetrics(requestName, category, "error", stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    /// <summary>
    /// Détermine la catégorie de la requête (Command ou Query) basée sur son nom.
    /// </summary>
    /// <param name="requestName">Nom du type de requête.</param>
    /// <returns>Catégorie de la requête.</returns>
    private static string DetermineCategory(string requestName)
    {
        if (requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
        {
            return "Command";
        }

        if (requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
        {
            return "Query";
        }

        return "Unknown";
    }

    /// <summary>
    /// Enregistre les métriques de performance pour une requête.
    /// </summary>
    /// <param name="requestName">Nom de la requête.</param>
    /// <param name="category">Catégorie (Command/Query).</param>
    /// <param name="status">Statut de l'exécution (success/error).</param>
    /// <param name="durationMs">Durée en millisecondes.</param>
    private static void RecordMetrics(string requestName, string category, string status, long durationMs)
    {
        var tags = new TagList
        {
            { "request_type", requestName },
            { "category", category },
            { "status", status }
        };

        RequestDuration.Record(durationMs, tags);
        RequestCount.Add(1, tags);
    }
}
