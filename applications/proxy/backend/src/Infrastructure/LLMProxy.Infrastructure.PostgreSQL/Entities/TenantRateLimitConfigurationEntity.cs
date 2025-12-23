using LLMProxy.Domain.Common;

namespace LLMProxy.Infrastructure.PostgreSQL.Entities;

/// <summary>
/// Entité EF Core pour la configuration de rate limiting par tenant.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// Stockage PostgreSQL avec cache Redis (ADR-042).
/// </para>
/// <para>
/// Cette entité est mappée vers la table <c>configuration.tenant_ratelimit_configurations</c>.
/// </para>
/// </remarks>
public sealed class TenantRateLimitConfigurationEntity : Entity
{
    /// <summary>
    /// Identifiant du tenant concerné.
    /// </summary>
    /// <remarks>
    /// Référence vers la table <c>tenants.tenants</c>.
    /// Une seule configuration par tenant (index unique).
    /// </remarks>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Requêtes par minute (limite globale).
    /// </summary>
    /// <remarks>Valeur par défaut : 1000 requêtes/minute.</remarks>
    public int GlobalRequestsPerMinute { get; set; } = 1000;

    /// <summary>
    /// Requêtes par jour (limite globale).
    /// </summary>
    /// <remarks>Valeur par défaut : 100 000 requêtes/jour.</remarks>
    public int GlobalRequestsPerDay { get; set; } = 100_000;

    /// <summary>
    /// Tokens par minute (limite globale).
    /// </summary>
    /// <remarks>Valeur par défaut : 100 000 tokens/minute.</remarks>
    public int GlobalTokensPerMinute { get; set; } = 100_000;

    /// <summary>
    /// Tokens par jour (limite globale).
    /// </summary>
    /// <remarks>Valeur par défaut : 10 000 000 tokens/jour.</remarks>
    public int GlobalTokensPerDay { get; set; } = 10_000_000;

    /// <summary>
    /// Requêtes par minute pour une API Key.
    /// </summary>
    /// <remarks>Valeur par défaut : 100 requêtes/minute.</remarks>
    public int ApiKeyRequestsPerMinute { get; set; } = 100;

    /// <summary>
    /// Tokens par minute pour une API Key.
    /// </summary>
    /// <remarks>Valeur par défaut : 10 000 tokens/minute.</remarks>
    public int ApiKeyTokensPerMinute { get; set; } = 10_000;

    /// <summary>
    /// Limites spécifiques par endpoint.
    /// </summary>
    /// <remarks>
    /// Navigation property vers <see cref="EndpointLimitEntity"/>.
    /// Relation 1-N avec cascade delete.
    /// </remarks>
    public ICollection<EndpointLimitEntity> EndpointLimits { get; set; } = new List<EndpointLimitEntity>();

    /// <summary>
    /// Constructeur par défaut pour EF Core.
    /// </summary>
    private TenantRateLimitConfigurationEntity()
    {
    }

    /// <summary>
    /// Crée une nouvelle configuration de rate limiting pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <returns>Nouvelle instance de configuration avec valeurs par défaut.</returns>
    public static TenantRateLimitConfigurationEntity CreateDefault(Guid tenantId)
    {
        return new TenantRateLimitConfigurationEntity
        {
            TenantId = tenantId
        };
    }

    /// <summary>
    /// Crée une configuration personnalisée de rate limiting.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="globalRequestsPerMinute">Limite globale de requêtes par minute.</param>
    /// <param name="globalRequestsPerDay">Limite globale de requêtes par jour.</param>
    /// <param name="globalTokensPerMinute">Limite globale de tokens par minute.</param>
    /// <param name="globalTokensPerDay">Limite globale de tokens par jour.</param>
    /// <param name="apiKeyRequestsPerMinute">Limite de requêtes par minute pour une API Key.</param>
    /// <param name="apiKeyTokensPerMinute">Limite de tokens par minute pour une API Key.</param>
    /// <returns>Nouvelle instance de configuration personnalisée.</returns>
    public static TenantRateLimitConfigurationEntity Create(
        Guid tenantId,
        int globalRequestsPerMinute,
        int globalRequestsPerDay,
        int globalTokensPerMinute,
        int globalTokensPerDay,
        int apiKeyRequestsPerMinute,
        int apiKeyTokensPerMinute)
    {
        return new TenantRateLimitConfigurationEntity
        {
            TenantId = tenantId,
            GlobalRequestsPerMinute = globalRequestsPerMinute,
            GlobalRequestsPerDay = globalRequestsPerDay,
            GlobalTokensPerMinute = globalTokensPerMinute,
            GlobalTokensPerDay = globalTokensPerDay,
            ApiKeyRequestsPerMinute = apiKeyRequestsPerMinute,
            ApiKeyTokensPerMinute = apiKeyTokensPerMinute
        };
    }

    /// <summary>
    /// Met à jour la configuration de rate limiting.
    /// </summary>
    /// <param name="globalRequestsPerMinute">Limite globale de requêtes par minute.</param>
    /// <param name="globalRequestsPerDay">Limite globale de requêtes par jour.</param>
    /// <param name="globalTokensPerMinute">Limite globale de tokens par minute.</param>
    /// <param name="globalTokensPerDay">Limite globale de tokens par jour.</param>
    /// <param name="apiKeyRequestsPerMinute">Limite de requêtes par minute pour une API Key.</param>
    /// <param name="apiKeyTokensPerMinute">Limite de tokens par minute pour une API Key.</param>
    /// <remarks>
    /// Met à jour automatiquement la date de modification via <see cref="Entity.MarkAsModified"/>.
    /// </remarks>
    public void Update(
        int globalRequestsPerMinute,
        int globalRequestsPerDay,
        int globalTokensPerMinute,
        int globalTokensPerDay,
        int apiKeyRequestsPerMinute,
        int apiKeyTokensPerMinute)
    {
        GlobalRequestsPerMinute = globalRequestsPerMinute;
        GlobalRequestsPerDay = globalRequestsPerDay;
        GlobalTokensPerMinute = globalTokensPerMinute;
        GlobalTokensPerDay = globalTokensPerDay;
        ApiKeyRequestsPerMinute = apiKeyRequestsPerMinute;
        ApiKeyTokensPerMinute = apiKeyTokensPerMinute;
        MarkAsModified();
    }
}
