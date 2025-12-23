using LLMProxy.Application.Configuration.RateLimiting;

namespace LLMProxy.Application.Interfaces;

/// <summary>
/// Repository pour la gestion des configurations de rate limiting par tenant.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-017 Repository Pattern et ADR-041 Rate Limiting.
/// </para>
/// <para>
/// Cette interface définit les opérations CRUD pour les configurations de rate limiting.
/// L'implémentation utilise PostgreSQL comme stockage persistant.
/// </para>
/// </remarks>
public interface ITenantRateLimitConfigurationRepository
{
    /// <summary>
    /// Récupère la configuration de rate limiting pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant unique du tenant.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns>
    /// La configuration du tenant si elle existe, <c>null</c> sinon.
    /// </returns>
    /// <remarks>
    /// Cette méthode retourne <c>null</c> si aucune configuration n'existe pour le tenant.
    /// Le service appelant doit alors utiliser une configuration par défaut.
    /// </remarks>
    Task<TenantRateLimitConfiguration?> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken ct = default);

    /// <summary>
    /// Crée ou met à jour la configuration d'un tenant.
    /// </summary>
    /// <param name="config">Configuration à persister.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns>Configuration persistée avec éventuel ID généré.</returns>
    /// <remarks>
    /// <para>
    /// Si une configuration existe déjà pour le tenant, elle est mise à jour.
    /// Sinon, une nouvelle configuration est créée (upsert).
    /// </para>
    /// <para>
    /// Les limites par endpoint sont entièrement remplacées à chaque appel.
    /// </para>
    /// </remarks>
    Task<TenantRateLimitConfiguration> UpsertAsync(
        TenantRateLimitConfiguration config,
        CancellationToken ct = default);

    /// <summary>
    /// Supprime la configuration d'un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <remarks>
    /// Après suppression, le tenant utilisera la configuration par défaut du système.
    /// La suppression cascade automatiquement vers les <c>EndpointLimits</c> associées.
    /// </remarks>
    Task DeleteAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Vérifie si une configuration existe pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns><c>true</c> si une configuration existe, <c>false</c> sinon.</returns>
    Task<bool> ExistsAsync(Guid tenantId, CancellationToken ct = default);
}
