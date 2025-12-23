namespace LLMProxy.Domain.Entities.Routing;

/// <summary>
/// Configuration de l'affinité de session.
/// </summary>
public class SessionAffinityConfig
{
    /// <summary>
    /// Indique si l'affinité est activée.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Politique d'affinité.
    /// </summary>
    public string Policy { get; set; } = "Cookie";

    /// <summary>
    /// Nom du cookie ou header d'affinité.
    /// </summary>
    public string AffinityKeyName { get; set; } = ".Yarp.Affinity";
}
