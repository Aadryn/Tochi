// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// LLMProxy - Métriques OpenTelemetry pour les opérations LLM
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace LLMProxy.Infrastructure.Telemetry.Metrics;

/// <summary>
/// Métriques OpenTelemetry dédiées aux opérations LLM.
/// Fournit des compteurs, histogrammes et jauges pour l'observabilité métier.
/// </summary>
/// <remarks>
/// <para>
/// Cette classe expose 7 métriques clés pour le monitoring LLM :
/// </para>
/// <list type="bullet">
/// <item><description><b>llmproxy.request.duration</b> : Latence des requêtes par provider/model</description></item>
/// <item><description><b>llmproxy.request.count</b> : Nombre de requêtes par tenant/provider/status</description></item>
/// <item><description><b>llmproxy.tokens.input</b> : Tokens consommés en entrée</description></item>
/// <item><description><b>llmproxy.tokens.output</b> : Tokens générés en sortie</description></item>
/// <item><description><b>llmproxy.cost.estimated</b> : Coût estimé en centimes</description></item>
/// <item><description><b>llmproxy.provider.availability</b> : Disponibilité des providers</description></item>
/// <item><description><b>llmproxy.ratelimit.remaining</b> : Quota restant par tenant</description></item>
/// </list>
/// <para>
/// Les métriques sont automatiquement exportées vers le collecteur OTLP configuré.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Enregistrer une requête réussie
/// LLMMetrics.RecordRequest(
///     provider: "openai",
///     model: "gpt-4",
///     tenantId: tenantGuid,
///     durationMs: 1500,
///     inputTokens: 150,
///     outputTokens: 500,
///     success: true);
/// </code>
/// </example>
public sealed class LLMMetrics : IDisposable
{
    /// <summary>
    /// Nom du Meter pour les métriques LLM.
    /// </summary>
    public const string MeterName = "LLMProxy.LLM";

    /// <summary>
    /// Version du Meter.
    /// </summary>
    public const string MeterVersion = "2.0.0";

    private readonly Meter _meter;

    // ═══════════════════════════════════════════════════════════════
    // INSTRUMENTS DE MÉTRIQUES
    // ═══════════════════════════════════════════════════════════════

    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long> _requestCount;
    private readonly Counter<long> _inputTokens;
    private readonly Counter<long> _outputTokens;
    private readonly Counter<double> _estimatedCost;
    private readonly ObservableGauge<int> _providerAvailability;
    private readonly ObservableGauge<int> _rateLimitRemaining;

    // État interne pour les jauges observables
    private readonly Dictionary<string, bool> _providerStatus = new();
    private readonly Dictionary<string, int> _tenantRateLimits = new();
    private readonly object _lock = new();

    /// <summary>
    /// Instance singleton des métriques LLM.
    /// </summary>
    public static LLMMetrics Instance { get; } = new();

    /// <summary>
    /// Obtient le Meter pour l'enregistrement auprès d'OpenTelemetry.
    /// </summary>
    public Meter Meter => _meter;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="LLMMetrics"/>.
    /// </summary>
    private LLMMetrics()
    {
        _meter = new Meter(MeterName, MeterVersion);

        // ═══ HISTOGRAM : Durée des requêtes ═══
        _requestDuration = _meter.CreateHistogram<double>(
            name: "llmproxy.request.duration",
            unit: "ms",
            description: "Durée des requêtes LLM en millisecondes");

        // ═══ COUNTER : Nombre de requêtes ═══
        _requestCount = _meter.CreateCounter<long>(
            name: "llmproxy.request.count",
            unit: "{request}",
            description: "Nombre total de requêtes LLM");

        // ═══ COUNTER : Tokens en entrée ═══
        _inputTokens = _meter.CreateCounter<long>(
            name: "llmproxy.tokens.input",
            unit: "{token}",
            description: "Nombre total de tokens en entrée");

        // ═══ COUNTER : Tokens en sortie ═══
        _outputTokens = _meter.CreateCounter<long>(
            name: "llmproxy.tokens.output",
            unit: "{token}",
            description: "Nombre total de tokens générés en sortie");

        // ═══ COUNTER : Coût estimé ═══
        _estimatedCost = _meter.CreateCounter<double>(
            name: "llmproxy.cost.estimated",
            unit: "cents",
            description: "Coût estimé des requêtes LLM en centimes (EUR)");

        // ═══ GAUGE : Disponibilité des providers ═══
        _providerAvailability = _meter.CreateObservableGauge<int>(
            name: "llmproxy.provider.availability",
            observeValues: ObserveProviderAvailability,
            unit: "{status}",
            description: "Disponibilité des providers LLM (1=disponible, 0=indisponible)");

        // ═══ GAUGE : Quota restant par tenant ═══
        _rateLimitRemaining = _meter.CreateObservableGauge<int>(
            name: "llmproxy.ratelimit.remaining",
            observeValues: ObserveRateLimitRemaining,
            unit: "{request}",
            description: "Nombre de requêtes restantes avant rate limit");
    }

    // ═══════════════════════════════════════════════════════════════
    // MÉTHODES D'ENREGISTREMENT DES MÉTRIQUES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Enregistre une requête LLM avec toutes ses métriques.
    /// </summary>
    /// <param name="provider">Nom du provider (openai, anthropic, etc.).</param>
    /// <param name="model">Nom du modèle utilisé.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="durationMs">Durée de la requête en millisecondes.</param>
    /// <param name="inputTokens">Nombre de tokens en entrée.</param>
    /// <param name="outputTokens">Nombre de tokens en sortie.</param>
    /// <param name="success">Indique si la requête a réussi.</param>
    /// <param name="estimatedCostCents">Coût estimé en centimes (optionnel).</param>
    /// <param name="isStreaming">Indique si la requête utilisait le streaming.</param>
    public void RecordRequest(
        string provider,
        string model,
        Guid tenantId,
        double durationMs,
        int inputTokens,
        int outputTokens,
        bool success,
        double? estimatedCostCents = null,
        bool isStreaming = false)
    {
        var tags = new TagList
        {
            { "provider", provider },
            { "model", model },
            { "tenant_id", tenantId.ToString() },
            { "status", success ? "success" : "error" },
            { "streaming", isStreaming.ToString().ToLowerInvariant() }
        };

        // Durée de la requête
        _requestDuration.Record(durationMs, tags);

        // Compteur de requêtes
        _requestCount.Add(1, tags);

        // Tokens (seulement si succès)
        if (success)
        {
            var tokenTags = new TagList
            {
                { "provider", provider },
                { "model", model },
                { "tenant_id", tenantId.ToString() }
            };

            _inputTokens.Add(inputTokens, tokenTags);
            _outputTokens.Add(outputTokens, tokenTags);

            // Coût estimé
            if (estimatedCostCents.HasValue)
            {
                _estimatedCost.Add(estimatedCostCents.Value, tokenTags);
            }
        }
    }

    /// <summary>
    /// Enregistre uniquement la durée d'une requête.
    /// </summary>
    /// <param name="provider">Nom du provider.</param>
    /// <param name="model">Nom du modèle.</param>
    /// <param name="durationMs">Durée en millisecondes.</param>
    /// <param name="success">Statut de succès.</param>
    public void RecordDuration(string provider, string model, double durationMs, bool success)
    {
        var tags = new TagList
        {
            { "provider", provider },
            { "model", model },
            { "status", success ? "success" : "error" }
        };

        _requestDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Enregistre les tokens consommés.
    /// </summary>
    /// <param name="provider">Nom du provider.</param>
    /// <param name="model">Nom du modèle.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="inputTokens">Tokens en entrée.</param>
    /// <param name="outputTokens">Tokens en sortie.</param>
    public void RecordTokens(
        string provider,
        string model,
        Guid tenantId,
        int inputTokens,
        int outputTokens)
    {
        var tags = new TagList
        {
            { "provider", provider },
            { "model", model },
            { "tenant_id", tenantId.ToString() }
        };

        _inputTokens.Add(inputTokens, tags);
        _outputTokens.Add(outputTokens, tags);
    }

    /// <summary>
    /// Enregistre le coût estimé d'une requête.
    /// </summary>
    /// <param name="provider">Nom du provider.</param>
    /// <param name="model">Nom du modèle.</param>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="costCents">Coût en centimes.</param>
    public void RecordCost(string provider, string model, Guid tenantId, double costCents)
    {
        var tags = new TagList
        {
            { "provider", provider },
            { "model", model },
            { "tenant_id", tenantId.ToString() }
        };

        _estimatedCost.Add(costCents, tags);
    }

    // ═══════════════════════════════════════════════════════════════
    // MÉTHODES DE MISE À JOUR DES JAUGES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Met à jour le statut de disponibilité d'un provider.
    /// </summary>
    /// <param name="provider">Nom du provider.</param>
    /// <param name="isAvailable">Indique si le provider est disponible.</param>
    public void UpdateProviderAvailability(string provider, bool isAvailable)
    {
        lock (_lock)
        {
            _providerStatus[provider] = isAvailable;
        }
    }

    /// <summary>
    /// Met à jour le quota restant pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="remainingRequests">Nombre de requêtes restantes.</param>
    public void UpdateRateLimitRemaining(Guid tenantId, int remainingRequests)
    {
        lock (_lock)
        {
            _tenantRateLimits[tenantId.ToString()] = remainingRequests;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // OBSERVATEURS POUR LES JAUGES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Observe la disponibilité de tous les providers enregistrés.
    /// </summary>
    private IEnumerable<Measurement<int>> ObserveProviderAvailability()
    {
        lock (_lock)
        {
            foreach (var (provider, isAvailable) in _providerStatus)
            {
                yield return new Measurement<int>(
                    isAvailable ? 1 : 0,
                    new TagList { { "provider", provider } });
            }
        }
    }

    /// <summary>
    /// Observe le quota restant pour tous les tenants enregistrés.
    /// </summary>
    private IEnumerable<Measurement<int>> ObserveRateLimitRemaining()
    {
        lock (_lock)
        {
            foreach (var (tenantId, remaining) in _tenantRateLimits)
            {
                yield return new Measurement<int>(
                    remaining,
                    new TagList { { "tenant_id", tenantId } });
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _meter.Dispose();
    }
}
