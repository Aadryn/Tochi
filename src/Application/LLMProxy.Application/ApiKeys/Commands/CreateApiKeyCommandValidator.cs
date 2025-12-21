using FluentValidation;

namespace LLMProxy.Application.ApiKeys.Commands;

/// <summary>
/// Validateur pour la commande de création de clé API.
/// </summary>
public class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ExpiresAt)
            .Must(expiry => !expiry.HasValue || expiry.Value > DateTime.UtcNow)
            .When(x => x.ExpiresAt.HasValue)
            .WithMessage("Expiration date must be in the future");
    }
}
