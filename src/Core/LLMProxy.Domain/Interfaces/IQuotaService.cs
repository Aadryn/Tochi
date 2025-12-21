using LLMProxy.Domain.Entities;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Service de gestion des quotas en temps réel (Port en Architecture Hexagonale)
/// Implémentation hybride : Redis pour la rapidité, PostgreSQL pour la persistance
/// </summary>
public interface IQuotaService
{
    /// <summary>
    /// Vérifie si l'utilisateur dispose de quota disponible pour la requête
    /// </summary>
    Task<QuotaCheckResult> CheckQuotaAsync(Guid userId, QuotaType quotaType, long amount = 1, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Incrémente l'utilisation du quota pour un utilisateur
    /// </summary>
    Task<QuotaUsage> IncrementUsageAsync(Guid userId, QuotaType quotaType, long amount = 1, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtient l'utilisation actuelle du quota pour un utilisateur
    /// </summary>
    Task<QuotaUsage?> GetUsageAsync(Guid userId, QuotaType quotaType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtient toutes les utilisations de quotas pour un utilisateur
    /// </summary>
    Task<IEnumerable<QuotaUsage>> GetAllUsagesAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Réinitialise les quotas expirés pour une fenêtre spécifique (appelé par tâche planifiée)
    /// </summary>
    Task ResetExpiredQuotasAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Synchronise les données de quota de Redis vers PostgreSQL pour persistance
    /// </summary>
    Task SyncQuotaToDatabaseAsync(CancellationToken cancellationToken = default);
}
