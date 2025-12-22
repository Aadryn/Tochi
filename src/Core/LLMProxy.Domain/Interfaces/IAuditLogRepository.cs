using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Repository pour l'agrégat AuditLog (Port dans l'architecture hexagonale).
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    /// <summary>
    /// Récupère les logs d'audit d'un tenant avec filtrage temporel optionnel.
    /// </summary>
    Task<Result<IReadOnlyList<AuditLog>>> GetByTenantIdAsync(Guid tenantId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les logs d'audit d'un utilisateur avec filtrage temporel optionnel.
    /// </summary>
    Task<Result<IReadOnlyList<AuditLog>>> GetByUserIdAsync(Guid userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime les logs d'audit plus anciens qu'un seuil donné.
    /// </summary>
    Task<Result<int>> DeleteOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default);
}
