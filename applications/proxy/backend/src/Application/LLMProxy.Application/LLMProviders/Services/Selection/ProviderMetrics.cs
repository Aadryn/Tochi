namespace LLMProxy.Application.LLMProviders.Services.Selection;

/// <summary>
/// Métriques de performance d'un provider.
/// </summary>
/// <remarks>
/// Ces métriques permettent d'évaluer la fiabilité et la performance d'un provider au fil du temps.
/// Elles sont utilisées pour optimiser la sélection des providers.
/// </remarks>
public sealed record ProviderMetrics
{
    /// <summary>
    /// Taux de succès (0.0 à 1.0).
    /// </summary>
    /// <remarks>
    /// Un taux de 1.0 signifie que tous les appels ont réussi.
    /// Un taux de 0.0 signifie que tous les appels ont échoué.
    /// </remarks>
    public required double SuccessRate { get; init; }

    /// <summary>
    /// Latence moyenne en millisecondes.
    /// </summary>
    /// <remarks>
    /// Calculée comme la moyenne de toutes les latences observées.
    /// </remarks>
    public required double AverageLatencyMs { get; init; }

    /// <summary>
    /// Nombre total d'appels effectués.
    /// </summary>
    /// <remarks>
    /// Utilisé pour pondérer les métriques (plus il y a d'appels, plus les métriques sont fiables).
    /// </remarks>
    public required int TotalCalls { get; init; }

    /// <summary>
    /// Dernière mise à jour des métriques.
    /// </summary>
    /// <remarks>
    /// Permet de déterminer la fraîcheur des métriques.
    /// </remarks>
    public required DateTimeOffset LastUpdated { get; init; }
}
