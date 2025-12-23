using Authorization.Infrastructure.PostgreSQL.Entities;

namespace Authorization.Infrastructure.PostgreSQL.Repositories;

/// <summary>
/// Interface pour le repository des logs d'audit.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Enregistre une entrée d'audit.
    /// </summary>
    /// <param name="auditLog">Entrée d'audit à enregistrer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enregistre plusieurs entrées d'audit en lot.
    /// </summary>
    /// <param name="auditLogs">Entrées d'audit à enregistrer.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task AddRangeAsync(IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les logs d'audit pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="from">Date de début (optionnelle).</param>
    /// <param name="to">Date de fin (optionnelle).</param>
    /// <param name="operationType">Type d'opération (optionnel).</param>
    /// <param name="actorId">Identifiant de l'acteur (optionnel).</param>
    /// <param name="result">Résultat (optionnel).</param>
    /// <param name="skip">Nombre d'éléments à ignorer.</param>
    /// <param name="take">Nombre d'éléments à retourner.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste paginée des logs d'audit.</returns>
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetByTenantAsync(
        string tenantId,
        DateTime? from = null,
        DateTime? to = null,
        AuditOperationType? operationType = null,
        string? actorId = null,
        AuditResult? result = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les logs d'audit par identifiant de corrélation.
    /// </summary>
    /// <param name="correlationId">Identifiant de corrélation.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Liste des logs d'audit correspondants.</returns>
    Task<IReadOnlyList<AuditLog>> GetByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les statistiques d'audit pour un tenant.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="from">Date de début.</param>
    /// <param name="to">Date de fin.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Statistiques d'audit.</returns>
    Task<AuditStatistics> GetStatisticsAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime les logs d'audit antérieurs à une date donnée.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="before">Date limite.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Nombre de logs supprimés.</returns>
    Task<int> PurgeOldLogsAsync(
        string tenantId,
        DateTime before,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistiques d'audit agrégées.
/// </summary>
public record AuditStatistics
{
    /// <summary>
    /// Nombre total d'opérations.
    /// </summary>
    public int TotalOperations { get; init; }

    /// <summary>
    /// Nombre de vérifications de permissions.
    /// </summary>
    public int PermissionChecks { get; init; }

    /// <summary>
    /// Nombre de permissions accordées.
    /// </summary>
    public int PermissionsGranted { get; init; }

    /// <summary>
    /// Nombre de permissions refusées.
    /// </summary>
    public int PermissionsDenied { get; init; }

    /// <summary>
    /// Nombre d'assignations de rôles.
    /// </summary>
    public int RoleAssignments { get; init; }

    /// <summary>
    /// Nombre de révocations de rôles.
    /// </summary>
    public int RoleRevocations { get; init; }

    /// <summary>
    /// Nombre d'erreurs.
    /// </summary>
    public int Errors { get; init; }

    /// <summary>
    /// Durée moyenne des opérations en millisecondes.
    /// </summary>
    public double AverageDurationMs { get; init; }

    /// <summary>
    /// Taux de cache hit pour les vérifications de permissions.
    /// </summary>
    public double CacheHitRate { get; init; }
}
