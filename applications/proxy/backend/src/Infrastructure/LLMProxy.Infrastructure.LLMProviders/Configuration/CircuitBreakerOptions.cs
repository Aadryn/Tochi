using System.ComponentModel.DataAnnotations;

namespace LLMProxy.Infrastructure.LLMProviders.Configuration;

/// <summary>
/// Options de configuration pour le pattern Circuit Breaker.
/// Conforme à ADR-032 (Circuit Breaker Pattern).
/// </summary>
public sealed class CircuitBreakerOptions
{
    /// <summary>
    /// Taux d'échec (0.0 à 1.0) déclenchant l'ouverture du circuit.
    /// Exemple : 0.5 = 50% d'échecs
    /// </summary>
    [Range(0.0, 1.0)]
    public double FailureThreshold { get; set; } = 0.5;

    /// <summary>
    /// Durée minimale pendant laquelle le circuit reste ouvert avant de passer en Half-Open.
    /// Permet au service défaillant de récupérer.
    /// </summary>
    [Required]
    public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Nombre minimum de requêtes dans la fenêtre d'échantillonnage avant calcul du taux d'échec.
    /// Évite ouverture prématurée sur faible volume.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int MinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Durée de la fenêtre d'échantillonnage pour calculer le taux d'échec.
    /// </summary>
    [Required]
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Nombre de requêtes autorisées en état Half-Open pour tester la récupération.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PermittedNumberOfCallsInHalfOpenState { get; set; } = 3;
}
