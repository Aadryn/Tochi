using FluentValidation;
using LLMProxy.Application.Common;

namespace LLMProxy.Application.ApiKeys.Commands;

/// <summary>
/// Validateur pour la commande de création de clé API.
/// </summary>
public class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(ValidationMessages.Required("User ID"));
        
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage(ValidationMessages.Required("Tenant ID"));
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Required("Name"))
            .MaximumLength(200).WithMessage(ValidationMessages.MaxLength("Name", 200));
        
        RuleFor(x => x.ExpiresAt)
            .Must(expiry => !expiry.HasValue || expiry.Value > DateTime.UtcNow)
            .When(x => x.ExpiresAt.HasValue)
            .WithMessage(ValidationMessages.MustBeFuture("Expiration date"));
    }
}
