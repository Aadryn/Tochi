using FluentValidation;
using LLMProxy.Application.Common;

namespace LLMProxy.Application.Users.Commands;

/// <summary>
/// Validateur pour la commande de création d'un utilisateur.
/// </summary>
/// <remarks>
/// Valide que le tenant existe, que l'email est valide et unique, et que le nom respecte les contraintes de longueur.
/// </remarks>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    /// <summary>
    /// Initialise une nouvelle instance de <see cref="CreateUserCommandValidator"/>.
    /// </summary>
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage(ValidationMessages.Required("Tenant ID"));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.Required("Email"))
            .EmailAddress().WithMessage(ValidationMessages.InvalidFormat("email"))
            .MaximumLength(255).WithMessage(ValidationMessages.MaxLength("Email", 255));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Required("Name"))
            .MaximumLength(100).WithMessage(ValidationMessages.MaxLength("Name", 100));
    }
}
