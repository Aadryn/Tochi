// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - MediatR Pipeline Behavior pour le logging des requêtes
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Common.Behaviors;

/// <summary>
/// Pipeline Behavior MediatR pour logger automatiquement toutes les requêtes.
/// Enregistre le début, la fin, et les erreurs de chaque commande/query.
/// </summary>
/// <remarks>
/// <para>
/// Ce behavior s'exécute pour TOUTES les requêtes MediatR et fournit :
/// </para>
/// <list type="bullet">
/// <item><description>Log du début de traitement avec le nom de la requête</description></item>
/// <item><description>Log de fin avec la durée d'exécution</description></item>
/// <item><description>Log d'erreur en cas d'exception</description></item>
/// <item><description>Alerte pour les requêtes lentes (&gt; 500ms)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Enregistrement automatique dans le pipeline MediatR
/// services.AddMediatR(cfg =>
/// {
///     cfg.RegisterServicesFromAssembly(assembly);
///     cfg.AddBehavior(typeof(IPipelineBehavior&lt;,&gt;), typeof(LoggingBehavior&lt;,&gt;));
/// });
/// </code>
/// </example>
/// <typeparam name="TRequest">Type de la requête MediatR.</typeparam>
/// <typeparam name="TResponse">Type de la réponse attendue.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Seuil en millisecondes au-delà duquel une requête est considérée comme lente.
    /// </summary>
    private const int SlowRequestThresholdMs = 500;

    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="LoggingBehavior{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="logger">Logger pour enregistrer les informations.</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
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
        var requestId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Début traitement {RequestName} [RequestId={RequestId}]",
            requestName,
            requestId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > SlowRequestThresholdMs)
            {
                _logger.LogWarning(
                    "Requête lente {RequestName} terminée en {ElapsedMs}ms [RequestId={RequestId}]",
                    requestName,
                    elapsedMs,
                    requestId);
            }
            else
            {
                _logger.LogInformation(
                    "Fin traitement {RequestName} en {ElapsedMs}ms [RequestId={RequestId}]",
                    requestName,
                    elapsedMs,
                    requestId);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Erreur traitement {RequestName} après {ElapsedMs}ms [RequestId={RequestId}]",
                requestName,
                stopwatch.ElapsedMilliseconds,
                requestId);

            throw;
        }
    }
}
