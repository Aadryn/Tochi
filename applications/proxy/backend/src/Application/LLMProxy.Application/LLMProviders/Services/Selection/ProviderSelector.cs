using LLMProxy.Domain.Entities;
using LLMProxy.Domain.LLM;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.LLMProviders.Services.Selection;

/// <summary>
/// Implémentation du service de sélection de providers LLM.
/// </summary>
/// <remarks>
/// Cette classe implémente la logique de sélection et de scoring des providers en fonction
/// de critères utilisateur et de métriques de performance (si disponibles).
/// </remarks>
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
