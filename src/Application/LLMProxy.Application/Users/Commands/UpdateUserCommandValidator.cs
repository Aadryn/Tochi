using FluentValidation;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Application.Users.Commands;

/// <summary>
/// Validateur pour la commande de mise à jour d'utilisateur.
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).NotEmpty().Must(BeValidRole).WithMessage("Invalid role. Valid roles: User, Admin, TenantAdmin");
    }

    private bool BeValidRole(string role)
    {
        return Enum.TryParse<UserRole>(role, true, out _);
    }
}
