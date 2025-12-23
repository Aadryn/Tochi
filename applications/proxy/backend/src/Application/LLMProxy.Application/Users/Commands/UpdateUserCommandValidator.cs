using FluentValidation;
using LLMProxy.Application.Common;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Application.Users.Commands;

/// <summary>
/// Validateur pour la commande de mise à jour d'utilisateur.
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(ValidationMessages.Required("User ID"));
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Required("Name"))
            .MaximumLength(200).WithMessage(ValidationMessages.MaxLength("Name", 200));
        
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage(ValidationMessages.Required("Role"))
            .Must(BeValidRole).WithMessage(ValidationMessages.InvalidValue("role", "User, Admin, TenantAdmin"));
    }

    private bool BeValidRole(string role)
    {
        return Enum.TryParse<UserRole>(role, true, out _);
    }
}
