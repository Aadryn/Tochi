using LLMProxy.Domain.Entities;

namespace LLMProxy.Application.LLMProviders.Services.Selection;

/// <summary>
/// Service de métriques des providers (optionnel).
/// </summary>
/// <remarks>
/// Ce service permet de collecter et récupérer des métriques de performance pour chaque provider,
/// telles que le taux de succès et la latence moyenne. Ces métriques sont utilisées par le
/// ProviderSelector pour optimiser la sélection des providers.
/// </remarks>
public interface IProviderMetricsService
{
    /// <summary>
    /// Récupère les métriques d'un provider.
    /// </summary>
    /// <param name="type">Type de provider pour lequel récupérer les métriques.</param>
    /// <returns>Les métriques du provider, ou null si aucune métrique n'est disponible.</returns>
    ProviderMetrics? GetProviderMetrics(ProviderType type);

    /// <summary>
    /// Enregistre une métrique pour un provider.
    /// </summary>
    /// <param name="type">Type de provider.</param>
    /// <param name="latency">Latence de l'appel.</param>
    /// <param name="success">Indique si l'appel a réussi.</param>
    /// <remarks>
    /// Cette méthode met à jour les statistiques globales du provider (taux de succès, latence moyenne).
    /// </remarks>
    void RecordMetric(ProviderType type, TimeSpan latency, bool success);
}
