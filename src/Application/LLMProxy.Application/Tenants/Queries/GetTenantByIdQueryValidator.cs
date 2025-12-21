using FluentValidation;

namespace LLMProxy.Application.Tenants.Queries;

/// <summary>
/// Validateur pour la requête de récupération d'un tenant par identifiant.
/// </summary>
/// <remarks>
/// Valide que l'identifiant du tenant est fourni et non vide.
/// </remarks>
public class GetTenantByIdQueryValidator : AbstractValidator<GetTenantByIdQuery>
{
    /// <summary>
    /// Initialise une nouvelle instance de <see cref="GetTenantByIdQueryValidator"/>.
    /// </summary>
    public GetTenantByIdQueryValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");
    }
}
