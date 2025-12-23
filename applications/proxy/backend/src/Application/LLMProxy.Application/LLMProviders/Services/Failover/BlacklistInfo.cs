namespace LLMProxy.Application.LLMProviders.Services.Failover;

/// <summary>
/// Informations de blacklist d'un provider.
/// </summary>
/// <remarks>
/// Capture les détails d'un provider actuellement blacklisté, incluant la raison,
/// la durée et le nombre d'échecs ayant mené au blacklist.
/// </remarks>
public sealed record BlacklistInfo
{
    /// <summary>
    /// Date de mise en blacklist.
    /// </summary>
    /// <remarks>
    /// Heure UTC à laquelle le provider a été blacklisté.
    /// </remarks>
    public required DateTimeOffset BlacklistedAt { get; init; }

    /// <summary>
    /// Date d'expiration du blacklist.
    /// </summary>
    /// <remarks>
    /// Heure UTC après laquelle le provider sera automatiquement retiré du blacklist.
    /// </remarks>
    public required DateTimeOffset ExpiresAt { get; init; }

    /// <summary>
    /// Raison du blacklist.
    /// </summary>
    /// <remarks>
    /// Description de l'erreur ou du problème ayant entraîné le blacklist.
    /// </remarks>
    public required string Reason { get; init; }

    /// <summary>
    /// Nombre d'échecs ayant mené au blacklist.
    /// </summary>
    /// <remarks>
    /// Compteur d'échecs consécutifs avant que le seuil de blacklist ne soit atteint.
    /// </remarks>
    public int FailureCount { get; init; }
}
