namespace LLMProxy.Infrastructure.LLMProviders.Configuration;

/// <summary>
/// Configuration globale des providers LLM.
/// </summary>
public sealed class LLMProvidersOptions
{
    /// <summary>
    /// Section de configuration dans appsettings.json.
    /// </summary>
    public const string SectionName = "LLMProviders";

    /// <summary>
    /// Liste des providers configurés.
    /// </summary>
    public List<LLMProviderConfiguration> Providers { get; set; } = new();

    /// <summary>
    /// Configuration du routage.
    /// </summary>
    public RoutingOptions Routing { get; set; } = new();

    /// <summary>
    /// Configuration des retries.
    /// </summary>
    public RetryPolicyOptions Retry { get; set; } = new();

    /// <summary>
    /// Configuration du circuit breaker.
    /// </summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Timeout par défaut pour les requêtes (en secondes).
    /// </summary>
    public int DefaultTimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Nombre maximum de tentatives par défaut.
    /// </summary>
    public int DefaultMaxRetries { get; set; } = 3;
}

/// <summary>
/// Configuration du routage des requêtes.
/// </summary>
public sealed class RoutingOptions
{
    /// <summary>
    /// Stratégie de routage.
    /// </summary>
    public RoutingStrategy Strategy { get; init; } = RoutingStrategy.PriorityWithFallback;

    /// <summary>
    /// Intervalle de health check.
    /// </summary>
    public TimeSpan HealthCheckInterval { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Nombre d'échecs avant failover.
    /// </summary>
    public int FailoverThreshold { get; init; } = 3;

    /// <summary>
    /// Durée de blacklist après failover.
    /// </summary>
    public TimeSpan BlacklistDuration { get; init; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Stratégies de routage disponibles.
/// </summary>
public enum RoutingStrategy
{
    /// <summary>
    /// Utilise le provider le plus prioritaire, failover si échec.
    /// </summary>
    PriorityWithFallback = 0,

    /// <summary>
    /// Round-robin entre les providers actifs.
    /// </summary>
    RoundRobin = 1,

    /// <summary>
    /// Random parmi les providers actifs.
    /// </summary>
    Random = 2,

    /// <summary>
    /// Choisit le provider avec la latence la plus basse.
    /// </summary>
    LeastLatency = 3,

    /// <summary>
    /// Choisit le provider avec le moins de charge.
    /// </summary>
    LeastLoad = 4
}
