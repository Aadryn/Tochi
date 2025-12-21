using FluentValidation;

namespace LLMProxy.Application.Tenants.Commands;

/// <summary>
/// Validateur pour la commande CreateTenantCommand
/// </summary>
public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required")
            .MaximumLength(100).WithMessage("Tenant name must not exceed 100 characters");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Tenant slug is required")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens")
            .MaximumLength(50).WithMessage("Slug must not exceed 50 characters");
    }
}
