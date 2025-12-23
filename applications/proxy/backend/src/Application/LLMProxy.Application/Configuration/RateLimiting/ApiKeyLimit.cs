namespace LLMProxy.Application.Configuration.RateLimiting;

/// <summary>
/// Configuration de rate limiting pour une API Key.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Permet de limiter chaque API Key individuellement pour garantir une répartition
/// équitable des ressources et éviter qu'une clé monopolise le quota du tenant.
/// </para>
/// <para>
/// <strong>Exemple de configuration :</strong>
/// </para>
/// <code>
/// var apiKeyLimit = new ApiKeyLimit
/// {
///     RequestsPerMinute = 100,       // 100 req/min par clé
///     TokensPerMinute = 10_000       // 10k tokens/min par clé
/// };
/// </code>
/// </para>
/// </remarks>
public sealed class ApiKeyLimit
{
    /// <summary>
    /// Nombre maximum de requêtes autorisées par minute pour une API Key.
    /// </summary>
    /// <value>
    /// Valeur par défaut : 100 requêtes/minute par clé.
    /// </value>
    public int RequestsPerMinute { get; set; } = 100;

    /// <summary>
    /// Nombre maximum de tokens LLM consommés par minute pour une API Key.
    /// </summary>
    /// <value>
    /// Valeur par défaut : 10 000 tokens/minute par clé.
    /// </value>
    public int TokensPerMinute { get; set; } = 10_000;
}
