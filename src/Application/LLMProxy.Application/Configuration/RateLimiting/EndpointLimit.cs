namespace LLMProxy.Application.Configuration.RateLimiting;

/// <summary>
/// Configuration de rate limiting pour un endpoint spécifique.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Permet de définir des limites différentes selon les endpoints (ex: /chat/completions vs /embeddings).
/// Les embeddings peuvent avoir une limite plus élevée car moins coûteux en calcul.
/// </para>
/// <para>
/// <strong>Exemple de configuration :</strong>
/// </para>
/// <code>
/// var chatLimit = new EndpointLimit
/// {
///     RequestsPerMinute = 60,        // 1 req/seconde
///     TokensPerMinute = 100_000,     // 100k tokens/minute
///     BurstCapacity = 120            // Permet 2 minutes en burst
/// };
/// 
/// var embeddingsLimit = new EndpointLimit
/// {
///     RequestsPerMinute = 1000,      // Embeddings plus rapides
///     TokensPerMinute = 500_000,
///     BurstCapacity = 2000
/// };
/// </code>
/// </para>
/// </remarks>
public sealed class EndpointLimit
{
    /// <summary>
    /// Nombre maximum de requêtes autorisées par minute pour cet endpoint.
    /// </summary>
    /// <value>
    /// Valeur par défaut : 100 requêtes/minute.
    /// </value>
    public int RequestsPerMinute { get; set; } = 100;

    /// <summary>
    /// Nombre maximum de tokens LLM autorisés par minute pour cet endpoint.
    /// </summary>
    /// <value>
    /// Valeur par défaut : 50 000 tokens/minute.
    /// </value>
    public int TokensPerMinute { get; set; } = 50_000;

    /// <summary>
    /// Capacité de burst (nombre maximum de tokens pouvant être consommés instantanément).
    /// </summary>
    /// <value>
    /// Valeur par défaut : 200 (2× RequestsPerMinute pour permettre des bursts courts).
    /// </value>
    /// <remarks>
    /// <para>
    /// Utilisé avec l'algorithme Token Bucket pour permettre des rafales de requêtes
    /// tout en maintenant un débit moyen respecté.
    /// </para>
    /// </remarks>
    public int BurstCapacity { get; set; } = 200;
}
