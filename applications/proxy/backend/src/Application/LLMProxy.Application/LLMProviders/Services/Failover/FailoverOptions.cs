namespace LLMProxy.Application.LLMProviders.Services.Failover;

/// <summary>
/// Options de configuration du failover.
/// </summary>
/// <remarks>
/// Configure les comportements du système de failover, incluant les seuils de blacklist,
/// les durées et les timeouts.
/// </remarks>
public sealed class FailoverOptions
{
    /// <summary>
    /// Nombre d'échecs avant blacklist d'un provider.
    /// </summary>
    /// <remarks>
    /// Par défaut 3. Lorsque ce nombre d'échecs consécutifs est atteint, le provider est automatiquement blacklisté.
    /// </remarks>
    public int FailuresBeforeBlacklist { get; init; } = 3;

    /// <summary>
    /// Durée du blacklist.
    /// </summary>
    /// <remarks>
    /// Par défaut 5 minutes. Durée pendant laquelle un provider blacklisté ne sera pas utilisé.
    /// </remarks>
    public TimeSpan BlacklistDuration { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Timeout par provider.
    /// </summary>
    /// <remarks>
    /// Par défaut 30 secondes. Temps maximum alloué pour une tentative sur un provider avant timeout.
    /// </remarks>
    public TimeSpan ProviderTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
