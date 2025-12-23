using LLMProxy.Domain.Common;

namespace LLMProxy.Infrastructure.PostgreSQL.Entities;

/// <summary>
/// Entité EF Core pour les limites spécifiques par endpoint.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// Permet de définir des limites différenciées par endpoint API.
/// </para>
/// <para>
/// Cette entité est mappée vers la table <c>configuration.endpoint_limits</c>.
/// </para>
/// </remarks>
public sealed class EndpointLimitEntity : Entity
{
    /// <summary>
    /// Identifiant de la configuration parent.
    /// </summary>
    /// <remarks>
    /// Foreign key vers <see cref="TenantRateLimitConfigurationEntity"/>.
    /// </remarks>
    public Guid TenantRateLimitConfigurationId { get; set; }

    /// <summary>
    /// Chemin de l'endpoint.
    /// </summary>
    /// <remarks>
    /// Exemple : <c>/v1/chat/completions</c>, <c>/v1/embeddings</c>.
    /// Le chemin doit être unique par configuration parent.
    /// </remarks>
    public string EndpointPath { get; set; } = string.Empty;

    /// <summary>
    /// Nombre maximum de requêtes par minute pour cet endpoint.
    /// </summary>
    /// <remarks>Valeur par défaut : 100 requêtes/minute.</remarks>
    public int RequestsPerMinute { get; set; } = 100;

    /// <summary>
    /// Nombre maximum de tokens par minute pour cet endpoint.
    /// </summary>
    /// <remarks>Valeur par défaut : 50 000 tokens/minute.</remarks>
    public int TokensPerMinute { get; set; } = 50_000;

    /// <summary>
    /// Capacité de burst (dépassement temporaire autorisé).
    /// </summary>
    /// <remarks>
    /// Permet d'absorber des pics de trafic temporaires.
    /// Valeur par défaut : 200 requêtes.
    /// </remarks>
    public int BurstCapacity { get; set; } = 200;

    /// <summary>
    /// Navigation property vers la configuration parent.
    /// </summary>
    public TenantRateLimitConfigurationEntity Configuration { get; set; } = null!;

    /// <summary>
    /// Constructeur par défaut pour EF Core.
    /// </summary>
    private EndpointLimitEntity()
    {
    }

    /// <summary>
    /// Crée une nouvelle limite d'endpoint.
    /// </summary>
    /// <param name="configurationId">Identifiant de la configuration parent.</param>
    /// <param name="endpointPath">Chemin de l'endpoint.</param>
    /// <param name="requestsPerMinute">Limite de requêtes par minute.</param>
    /// <param name="tokensPerMinute">Limite de tokens par minute.</param>
    /// <param name="burstCapacity">Capacité de burst.</param>
    /// <returns>Nouvelle instance de limite d'endpoint.</returns>
    public static EndpointLimitEntity Create(
        Guid configurationId,
        string endpointPath,
        int requestsPerMinute = 100,
        int tokensPerMinute = 50_000,
        int burstCapacity = 200)
    {
        return new EndpointLimitEntity
        {
            TenantRateLimitConfigurationId = configurationId,
            EndpointPath = endpointPath,
            RequestsPerMinute = requestsPerMinute,
            TokensPerMinute = tokensPerMinute,
            BurstCapacity = burstCapacity
        };
    }
}
