using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.LLMProviders.Services;

/// <summary>
/// Sélectionne le meilleur provider LLM selon des critères de sélection.
/// Conforme à ADR-034 (Encapsulation des Bibliothèques Tierces).
/// </summary>
public interface IProviderSelector
{
    /// <summary>
    /// Sélectionne les types de providers disponibles selon les critères.
    /// Retourne une liste ordonnée par préférence.
    /// </summary>
    /// <param name="criteria">Critères de sélection optionnels.</param>
    /// <returns>Liste ordonnée des types de providers.</returns>
    IReadOnlyList<ProviderType> Select(SelectionCriteria? criteria = null);

    /// <summary>
    /// Sélectionne le meilleur provider pour une requête donnée.
    /// </summary>
    /// <param name="request">Requête LLM à traiter.</param>
    /// <param name="availableProviders">Liste des providers disponibles.</param>
    /// <param name="criteria">Critères de sélection optionnels.</param>
    /// <returns>Le provider sélectionné ou null si aucun ne convient.</returns>
    ILLMProviderClient? SelectBestProvider(
        LLMRequest request,
        IReadOnlyList<ILLMProviderClient> availableProviders,
        SelectionCriteria? criteria = null);

    /// <summary>
    /// Sélectionne les providers par ordre de préférence pour le failover.
    /// </summary>
    /// <param name="request">Requête LLM à traiter.</param>
    /// <param name="availableProviders">Liste des providers disponibles.</param>
    /// <param name="criteria">Critères de sélection optionnels.</param>
    /// <returns>Liste ordonnée des providers par préférence.</returns>
    IReadOnlyList<ILLMProviderClient> SelectProvidersOrdered(
        LLMRequest request,
        IReadOnlyList<ILLMProviderClient> availableProviders,
        SelectionCriteria? criteria = null);
}

/// <summary>
/// Critères de sélection d'un provider.
/// </summary>
public sealed record SelectionCriteria
{
    /// <summary>
    /// Types de providers préférés (dans l'ordre de préférence).
    /// </summary>
    public IReadOnlyList<ProviderType>? PreferredProviders { get; init; }

    /// <summary>
    /// Capacités requises du provider.
    /// </summary>
    public ModelCapabilities? RequiredCapabilities { get; init; }

    /// <summary>
    /// Modèle spécifique requis.
    /// </summary>
    public string? RequiredModel { get; init; }

    /// <summary>
    /// Priorité au provider le moins cher.
    /// </summary>
    public bool PreferCheapest { get; init; }

    /// <summary>
    /// Priorité au provider le plus rapide.
    /// </summary>
    public bool PreferFastest { get; init; }

    /// <summary>
    /// Exclure certains providers.
    /// </summary>
    public IReadOnlyList<ProviderType>? ExcludedProviders { get; init; }

    /// <summary>
    /// Nombre minimum de tokens de contexte requis.
    /// </summary>
    public int? MinContextLength { get; init; }
}

/// <summary>
/// Implémentation du sélecteur de providers.
/// </summary>
public sealed class ProviderSelector : IProviderSelector
{
    private readonly ILogger<ProviderSelector> _logger;
    private readonly IProviderMetricsService? _metricsService;

    /// <summary>
    /// Constructeur.
    /// </summary>
    public ProviderSelector(
        ILogger<ProviderSelector> logger,
        IProviderMetricsService? metricsService = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metricsService = metricsService;
    }

    /// <inheritdoc />
    public IReadOnlyList<ProviderType> Select(SelectionCriteria? criteria = null)
    {
        // Liste par défaut de tous les providers supportés, ordonnés par fiabilité
        var allProviders = new List<ProviderType>
        {
            ProviderType.OpenAI,
            ProviderType.Anthropic,
            ProviderType.AzureOpenAI,
            ProviderType.AWSBedrock,
            ProviderType.GoogleGemini,
            ProviderType.Mistral,
            ProviderType.Cohere,
            ProviderType.HuggingFace,
            ProviderType.Ollama,
            ProviderType.VLLM
        };

        if (criteria == null)
        {
            return allProviders;
        }

        var result = allProviders.AsEnumerable();

        // Exclure les providers non désirés
        if (criteria.ExcludedProviders?.Count > 0)
        {
            result = result.Where(p => !criteria.ExcludedProviders.Contains(p));
        }

        // Si providers préférés spécifiés, les mettre en premier
        if (criteria.PreferredProviders?.Count > 0)
        {
            var preferred = criteria.PreferredProviders.Where(p => result.Contains(p)).ToList();
            var others = result.Where(p => !criteria.PreferredProviders.Contains(p)).ToList();
            result = preferred.Concat(others);
        }

        var finalList = result.ToList();

        _logger.LogDebug(
            "Sélection de providers: {Count} disponibles, premiers: {FirstProviders}",
            finalList.Count,
            string.Join(", ", finalList.Take(3)));

        return finalList;
    }

    /// <inheritdoc />
    public ILLMProviderClient? SelectBestProvider(
        LLMRequest request,
        IReadOnlyList<ILLMProviderClient> availableProviders,
        SelectionCriteria? criteria = null)
    {
        var orderedProviders = SelectProvidersOrdered(request, availableProviders, criteria);
        return orderedProviders.FirstOrDefault();
    }

    /// <inheritdoc />
    public IReadOnlyList<ILLMProviderClient> SelectProvidersOrdered(
        LLMRequest request,
        IReadOnlyList<ILLMProviderClient> availableProviders,
        SelectionCriteria? criteria = null)
    {
        if (availableProviders.Count == 0)
        {
            _logger.LogWarning("Aucun provider disponible pour la sélection");
            return Array.Empty<ILLMProviderClient>();
        }

        var candidates = availableProviders.AsEnumerable();

        // Appliquer les filtres
        candidates = ApplyFilters(candidates, criteria);

        // Trier par score
        var scoredProviders = candidates
            .Select(p => new ScoredProvider(p, CalculateScore(p, request, criteria)))
            .OrderByDescending(sp => sp.Score)
            .ToList();

        _logger.LogDebug(
            "Sélection de providers: {Count} candidats, meilleur score: {Score}",
            scoredProviders.Count,
            scoredProviders.FirstOrDefault()?.Score ?? 0);

        return scoredProviders.Select(sp => sp.Provider).ToList();
    }

    private IEnumerable<ILLMProviderClient> ApplyFilters(
        IEnumerable<ILLMProviderClient> providers,
        SelectionCriteria? criteria)
    {
        if (criteria == null)
            return providers;

        // Exclure les providers non désirés
        if (criteria.ExcludedProviders?.Count > 0)
        {
            providers = providers.Where(p => !criteria.ExcludedProviders.Contains(p.Type));
        }

        // Filtrer par providers préférés (si spécifiés, ne garder que ceux-là)
        if (criteria.PreferredProviders?.Count > 0)
        {
            var filtered = providers.Where(p => criteria.PreferredProviders.Contains(p.Type)).ToList();
            if (filtered.Count > 0)
            {
                providers = filtered;
            }
        }

        return providers;
    }

    private double CalculateScore(
        ILLMProviderClient provider,
        LLMRequest request,
        SelectionCriteria? criteria)
    {
        double score = 100.0; // Score de base

        // Bonus pour provider préféré
        if (criteria?.PreferredProviders?.Count > 0)
        {
            var preferenceIndex = criteria.PreferredProviders
                .ToList()
                .IndexOf(provider.Type);

            if (preferenceIndex >= 0)
            {
                // Plus haut dans la liste = meilleur score
                score += (criteria.PreferredProviders.Count - preferenceIndex) * 10;
            }
        }

        // Bonus pour métriques si disponibles
        if (_metricsService != null)
        {
            var metrics = _metricsService.GetProviderMetrics(provider.Type);
            if (metrics != null)
            {
                // Bonus pour taux de succès élevé
                score += metrics.SuccessRate * 20;

                // Malus pour latence élevée
                if (criteria?.PreferFastest == true && metrics.AverageLatencyMs > 0)
                {
                    score -= Math.Min(metrics.AverageLatencyMs / 100, 30);
                }
            }
        }

        // Bonus selon le type de provider (fiabilité connue)
        score += GetProviderReliabilityBonus(provider.Type);

        return score;
    }

    private static double GetProviderReliabilityBonus(ProviderType type)
    {
        return type switch
        {
            ProviderType.OpenAI => 15,
            ProviderType.Anthropic => 15,
            ProviderType.AzureOpenAI => 12,
            ProviderType.AWSBedrock => 12,
            ProviderType.GoogleGemini => 10,
            ProviderType.Mistral => 8,
            ProviderType.Cohere => 8,
            ProviderType.HuggingFace => 5,
            ProviderType.Ollama => 3,
            ProviderType.VLLM => 3,
            _ => 0
        };
    }

    private sealed record ScoredProvider(ILLMProviderClient Provider, double Score);
}

/// <summary>
/// Service de métriques des providers (optionnel).
/// </summary>
public interface IProviderMetricsService
{
    /// <summary>
    /// Récupère les métriques d'un provider.
    /// </summary>
    ProviderMetrics? GetProviderMetrics(ProviderType type);

    /// <summary>
    /// Enregistre une métrique.
    /// </summary>
    void RecordMetric(ProviderType type, TimeSpan latency, bool success);
}

/// <summary>
/// Métriques d'un provider.
/// </summary>
public sealed record ProviderMetrics
{
    /// <summary>
    /// Taux de succès (0.0 à 1.0).
    /// </summary>
    public required double SuccessRate { get; init; }

    /// <summary>
    /// Latence moyenne en millisecondes.
    /// </summary>
    public required double AverageLatencyMs { get; init; }

    /// <summary>
    /// Nombre d'appels effectués.
    /// </summary>
    public required int TotalCalls { get; init; }

    /// <summary>
    /// Dernière mise à jour des métriques.
    /// </summary>
    public required DateTimeOffset LastUpdated { get; init; }
}
