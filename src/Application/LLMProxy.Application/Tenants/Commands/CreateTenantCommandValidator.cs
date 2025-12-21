using FluentValidation;
using LLMProxy.Application.Common;

namespace LLMProxy.Application.Tenants.Commands;

/// <summary>
/// Validateur pour la commande CreateTenantCommand
/// </summary>
public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Required("Tenant name"))
            .MaximumLength(100).WithMessage(ValidationMessages.MaxLength("Tenant name", 100));

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage(ValidationMessages.Required("Tenant slug"))
            .Matches(@"^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens")
            .MaximumLength(50).WithMessage(ValidationMessages.MaxLength("Slug", 50));
    }
}
