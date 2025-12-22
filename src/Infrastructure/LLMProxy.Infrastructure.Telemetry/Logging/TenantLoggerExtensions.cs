using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.Telemetry.Logging;

/// <summary>
/// Extensions de logging structuré pour les opérations sur les Tenants (ADR-031).
/// </summary>
/// <remarks>
/// <para>
/// Utilise LoggerMessage source generators pour performance optimale :
/// </para>
/// <list type="bullet">
/// <item><description><strong>Zero allocation</strong> si log level désactivé</description></item>
/// <item><description><strong>Compile-time safety</strong> des paramètres</description></item>
/// <item><description><strong>Templates constants</strong> évitent typos</description></item>
/// <item><description><strong>Structured properties</strong> indexables (TenantId, etc.)</description></item>
/// </list>
/// <para>
/// <strong>Usage</strong> :
/// </para>
/// <code>
/// // Au lieu de :
/// _logger.LogInformation("Tenant {TenantId} created", tenantId);
/// 
/// // Utiliser :
/// _logger.TenantCreated(tenantId, tenantName);
/// </code>
/// <para>
/// <strong>Performance</strong> : ~85% réduction allocations vs string interpolation.
/// </para>
/// </remarks>
public static partial class TenantLoggerExtensions
{
    // ═══════════════════════════════════════════════════════════════
    // CRÉATION & ACTIVATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Log la création d'un nouveau tenant.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="tenantId">ID du tenant créé.</param>
    /// <param name="tenantName">Nom du tenant.</param>
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Tenant {TenantId} created with name '{TenantName}'")]
    public static partial void TenantCreated(
        this ILogger logger,
        Guid tenantId,
        string tenantName);

    /// <summary>
    /// Log l'activation d'un tenant.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="tenantId">ID du tenant activé.</param>
    /// <param name="previousStatus">Statut précédent (ex: Pending, Suspended).</param>
    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Tenant {TenantId} activated from status '{PreviousStatus}'")]
    public static partial void TenantActivated(
        this ILogger logger,
        Guid tenantId,
        string previousStatus);

    /// <summary>
    /// Log la désactivation d'un tenant.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="tenantId">ID du tenant désactivé.</param>
    /// <param name="reason">Raison de la désactivation.</param>
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Tenant {TenantId} deactivated: {Reason}")]
    public static partial void TenantDeactivated(
        this ILogger logger,
        Guid tenantId,
        string reason);

    // ═══════════════════════════════════════════════════════════════
    // QUOTAS & USAGE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Log le dépassement de quota d'un tenant.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="tenantId">ID du tenant.</param>
    /// <param name="currentUsage">Usage actuel (tokens).</param>
    /// <param name="monthlyQuota">Quota mensuel autorisé.</param>
    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Warning,
        Message = "Tenant {TenantId} quota exceeded: {CurrentUsage:N0}/{MonthlyQuota:N0} tokens")]
    public static partial void TenantQuotaExceeded(
        this ILogger logger,
        Guid tenantId,
        long currentUsage,
        long monthlyQuota);

    /// <summary>
    /// Log l'enregistrement d'usage de tokens pour un tenant.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="tenantId">ID du tenant.</param>
    /// <param name="tokensUsed">Nombre de tokens utilisés.</param>
    /// <param name="remainingQuota">Quota restant.</param>
    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Debug,
        Message = "Tenant {TenantId} used {TokensUsed:N0} tokens, {RemainingQuota:N0} remaining")]
    public static partial void TenantUsageRecorded(
        this ILogger logger,
        Guid tenantId,
        long tokensUsed,
        long remainingQuota);

    /// <summary>
    /// Log la réinitialisation du quota mensuel d'un tenant.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="tenantId">ID du tenant.</param>
    /// <param name="previousUsage">Usage avant réinitialisation.</param>
    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Information,
        Message = "Tenant {TenantId} quota reset, previous usage was {PreviousUsage:N0} tokens")]
    public static partial void TenantQuotaReset(
        this ILogger logger,
        Guid tenantId,
        long previousUsage);

    // ═══════════════════════════════════════════════════════════════
    // MISE À JOUR & SUPPRESSION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Log la mise à jour d'un tenant.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="tenantId">ID du tenant mis à jour.</param>
    /// <param name="changedFields">Champs modifiés (ex: "Name, MonthlyQuota").</param>
    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Information,
        Message = "Tenant {TenantId} updated: {ChangedFields}")]
    public static partial void TenantUpdated(
        this ILogger logger,
        Guid tenantId,
        string changedFields);

    /// <summary>
    /// Log la suppression d'un tenant.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="tenantId">ID du tenant supprimé.</param>
    /// <param name="tenantName">Nom du tenant (pour audit).</param>
    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Warning,
        Message = "Tenant {TenantId} '{TenantName}' deleted")]
    public static partial void TenantDeleted(
        this ILogger logger,
        Guid tenantId,
        string tenantName);

    // ═══════════════════════════════════════════════════════════════
    // ERREURS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Log l'échec de création d'un tenant.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="exception">Exception levée.</param>
    /// <param name="tenantName">Nom du tenant qui devait être créé.</param>
    /// <param name="errorMessage">Message d'erreur descriptif.</param>
    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Error,
        Message = "Failed to create tenant '{TenantName}': {ErrorMessage}")]
    public static partial void TenantCreationFailed(
        this ILogger logger,
        Exception exception,
        string tenantName,
        string errorMessage);

    /// <summary>
    /// Log l'échec de mise à jour d'un tenant.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="exception">Exception levée.</param>
    /// <param name="tenantId">ID du tenant.</param>
    /// <param name="errorMessage">Message d'erreur descriptif.</param>
    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Error,
        Message = "Failed to update tenant {TenantId}: {ErrorMessage}")]
    public static partial void TenantUpdateFailed(
        this ILogger logger,
        Exception exception,
        Guid tenantId,
        string errorMessage);

    /// <summary>
    /// Log l'échec de suppression d'un tenant.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="exception">Exception levée.</param>
    /// <param name="tenantId">ID du tenant.</param>
    /// <param name="errorMessage">Message d'erreur descriptif.</param>
    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Error,
        Message = "Failed to delete tenant {TenantId}: {ErrorMessage}")]
    public static partial void TenantDeletionFailed(
        this ILogger logger,
        Exception exception,
        Guid tenantId,
        string errorMessage);
}
