namespace LLMProxy.Application.Configuration.RateLimiting;

/// <summary>
/// Configuration de rate limiting global pour un tenant.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Définit les limites maximales absolues pour un tenant, tous endpoints confondus.
/// Ces limites servent de plafond global pour éviter les abus et maîtriser les coûts.
/// </para>
/// <para>
/// <strong>Exemple de configuration :</strong>
/// </para>
/// <code>
/// var globalLimit = new GlobalLimit
/// {
///     RequestsPerMinute = 1000,      // 16.67 req/s maximum
///     RequestsPerDay = 100_000,      // Quota journalier
///     TokensPerMinute = 100_000,     // 100k tokens/minute
///     TokensPerDay = 10_000_000      // 10M tokens/jour pour facturation
/// };
/// </code>
/// </para>
/// </remarks>
public sealed class GlobalLimit
{
    /// <summary>
    /// Nombre maximum de requêtes autorisées par minute (tous endpoints confondus).
    /// </summary>
    /// <value>
    /// Valeur par défaut : 1000 requêtes/minute.
    /// </value>
    public int RequestsPerMinute { get; set; } = 1000;

    /// <summary>
    /// Nombre maximum de requêtes autorisées par jour (quota journalier).
    /// </summary>
    /// <value>
    /// Valeur par défaut : 100 000 requêtes/jour.
    /// </value>
    public int RequestsPerDay { get; set; } = 100_000;

    /// <summary>
    /// Nombre maximum de tokens LLM consommés par minute.
    /// </summary>
    /// <value>
    /// Valeur par défaut : 100 000 tokens/minute.
    /// </value>
    public int TokensPerMinute { get; set; } = 100_000;

    /// <summary>
    /// Nombre maximum de tokens LLM consommés par jour (pour facturation et quotas).
    /// </summary>
    /// <value>
    /// Valeur par défaut : 10 000 000 tokens/jour.
    /// </value>
    public int TokensPerDay { get; set; } = 10_000_000;
}
