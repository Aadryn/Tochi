using FluentValidation;

namespace LLMProxy.Application.Tenants.Commands;

/// <summary>
/// Validateur pour la commande de mise à jour des paramètres d'un tenant.
/// </summary>
/// <remarks>
/// Valide que l'identifiant du tenant est fourni, que les limites d'utilisateurs et de fournisseurs
/// sont positives, et que la rétention d'audit est non négative.
/// </remarks>
public class UpdateTenantSettingsCommandValidator : AbstractValidator<UpdateTenantSettingsCommand>
{
    /// <summary>
    /// Initialise une nouvelle instance de <see cref="UpdateTenantSettingsCommandValidator"/>.
    /// </summary>
    public UpdateTenantSettingsCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MaxUsers).GreaterThan(0);
        RuleFor(x => x.MaxProviders).GreaterThan(0);
        RuleFor(x => x.AuditRetentionDays).GreaterThanOrEqualTo(0);
    }
}
