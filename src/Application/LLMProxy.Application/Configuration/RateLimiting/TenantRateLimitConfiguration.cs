namespace LLMProxy.Application.Configuration.RateLimiting;

/// <summary>
/// Configuration complète de rate limiting pour un tenant.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Cette classe centralise toutes les configurations de rate limiting applicables à un tenant :
/// limites globales, limites par endpoint, et limites par API Key.
/// </para>
/// <para>
/// <strong>Architecture multi-niveau :</strong>
/// </para>
/// <code>
/// Niveau 1: IP          → 100 req/min (protection DDoS)
/// Niveau 2: Tenant      → Configuration globale (RequestsPerMinute, TokensPerDay)
/// Niveau 3: API Key     → Configuration par clé (RequestsPerMinute, TokensPerMinute)
/// Niveau 4: Endpoint    → Configuration par endpoint (/chat, /embeddings, etc.)
/// </code>
/// <para>
/// <strong>Exemple de configuration complète :</strong>
/// </para>
/// <code>
/// var config = new TenantRateLimitConfiguration
/// {
///     TenantId = Guid.Parse("..."),
///     
///     GlobalLimit = new GlobalLimit
///     {
///         RequestsPerMinute = 1000,
///         RequestsPerDay = 100_000,
///         TokensPerMinute = 100_000,
///         TokensPerDay = 10_000_000
///     },
///     
///     ApiKeyLimit = new ApiKeyLimit
///     {
///         RequestsPerMinute = 100,
///         TokensPerMinute = 10_000
///     },
///     
///     EndpointLimits = new Dictionary&lt;string, EndpointLimit&gt;
///     {
///         ["/v1/chat/completions"] = new EndpointLimit
///         {
///             RequestsPerMinute = 60,
///             TokensPerMinute = 100_000,
///             BurstCapacity = 120
///         },
///         ["/v1/embeddings"] = new EndpointLimit
///         {
///             RequestsPerMinute = 1000,
///             TokensPerMinute = 500_000,
///             BurstCapacity = 2000
///         }
///     }
/// };
/// </code>
/// </para>
/// </remarks>
public sealed class TenantRateLimitConfiguration
{
    /// <summary>
    /// Identifiant unique du tenant concerné par cette configuration.
    /// </summary>
    public required Guid TenantId { get; set; }

    /// <summary>
    /// Limites par endpoint (clé = chemin de l'endpoint, ex: "/v1/chat/completions").
    /// </summary>
    /// <value>
    /// Dictionnaire vide par défaut. Les endpoints non configurés utilisent les limites globales.
    /// </value>
    public Dictionary<string, EndpointLimit> EndpointLimits { get; set; } = new();

    /// <summary>
    /// Limite globale du tenant (tous endpoints confondus).
    /// </summary>
    /// <value>
    /// Instance par défaut avec valeurs standard si non spécifié.
    /// </value>
    public GlobalLimit GlobalLimit { get; set; } = new();

    /// <summary>
    /// Limite par API Key (granularité fine au niveau de chaque clé).
    /// </summary>
    /// <value>
    /// Instance par défaut avec valeurs standard si non spécifié.
    /// </value>
    public ApiKeyLimit ApiKeyLimit { get; set; } = new();
}
