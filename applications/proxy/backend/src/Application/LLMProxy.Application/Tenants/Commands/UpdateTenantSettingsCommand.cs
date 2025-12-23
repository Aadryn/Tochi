using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;

namespace LLMProxy.Application.Tenants.Commands;

/// <summary>
/// Commande de mise à jour des paramètres d'un tenant.
/// </summary>
/// <remarks>
/// Permet de modifier la configuration d'un tenant existant incluant les limites d'utilisateurs,
/// de fournisseurs, et les paramètres d'audit et de cache.
/// </remarks>
public record UpdateTenantSettingsCommand : ICommand<TenantDto>
{
    /// <summary>
    /// Obtient ou définit l'identifiant unique du tenant à mettre à jour.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Obtient ou initialise le nombre maximum d'utilisateurs autorisés pour le tenant.
    /// </summary>
    public int MaxUsers { get; init; }

    /// <summary>
    /// Obtient ou initialise le nombre maximum de fournisseurs LLM autorisés pour le tenant.
    /// </summary>
    public int MaxProviders { get; init; }

    /// <summary>
    /// Obtient ou initialise si la journalisation d'audit est activée pour le tenant.
    /// </summary>
    public bool EnableAuditLogging { get; init; }

    /// <summary>
    /// Obtient ou initialise la durée de rétention des journaux d'audit en jours.
    /// </summary>
    public int AuditRetentionDays { get; init; }

    /// <summary>
    /// Obtient ou initialise si le cache de réponses est activé pour le tenant.
    /// </summary>
    public bool EnableResponseCache { get; init; }
}
