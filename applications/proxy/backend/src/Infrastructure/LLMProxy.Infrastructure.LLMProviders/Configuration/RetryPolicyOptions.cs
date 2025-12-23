using System.ComponentModel.DataAnnotations;

namespace LLMProxy.Infrastructure.LLMProviders.Configuration;

/// <summary>
/// Options de configuration pour la politique de retry avec exponential backoff.
/// Conforme à ADR-033 (Retry Pattern, Exponential Backoff).
/// </summary>
public sealed class RetryPolicyOptions
{
    /// <summary>
    /// Nombre maximum de tentatives (1 appel initial + N retries).
    /// Défaut : 3 (soit 1 appel + 2 retries).
    /// </summary>
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Délai initial avant le premier retry.
    /// Défaut : 1 seconde.
    /// </summary>
    [Required]
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Activer le jitter (variation aléatoire) pour éviter thundering herd.
    /// Défaut : true.
    /// </summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>
    /// Type de backoff (Exponential recommandé pour ADR-033).
    /// Défaut : Exponential (1s, 2s, 4s, 8s...).
    /// </summary>
    [Required]
    public string BackoffType { get; set; } = "Exponential";
}
