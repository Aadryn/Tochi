namespace LLMProxy.Domain.Entities.Routing;

/// <summary>
/// Configuration du health check actif.
/// </summary>
public class HealthCheckConfig
{
    /// <summary>
    /// Indique si le health check est activé.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Intervalle entre les vérifications.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Timeout de chaque vérification.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Chemin de l'endpoint de health.
    /// </summary>
    public string Path { get; set; } = "/health";

    /// <summary>
    /// Politique de détection de pannes.
    /// </summary>
    public string Policy { get; set; } = "ConsecutiveFailures";
}
