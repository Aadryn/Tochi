using FluentValidation;

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
            .NotEmpty().WithMessage("Tenant ID is required");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
    }
}
