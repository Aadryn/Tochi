using FluentValidation;
using LLMProxy.Application.Common;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Application.LLMProviders.Commands;

/// <summary>
/// Validateur pour la commande de création de fournisseur LLM.
/// </summary>
public class CreateProviderCommandValidator : AbstractValidator<CreateProviderCommand>
{
    public CreateProviderCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage(ValidationMessages.Required("Tenant ID"));
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.Required("Name"))
            .MaximumLength(200).WithMessage(ValidationMessages.MaxLength("Name", 200));
        
        RuleFor(x => x.ProviderType)
            .NotEmpty().WithMessage(ValidationMessages.Required("Provider type"))
            .Must(BeValidProviderType).WithMessage(ValidationMessages.InvalidValue("provider type"));
        
        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage(ValidationMessages.Required("Base URL"))
            .Must(BeValidUrl).WithMessage(ValidationMessages.InvalidFormat("URL"));
        
        RuleFor(x => x.RoutingStrategy)
            .NotEmpty().WithMessage(ValidationMessages.Required("Routing strategy"))
            .Must(BeValidRoutingStrategy).WithMessage(ValidationMessages.InvalidValue("routing strategy"));
        
        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be greater than or equal to 0");
        
        RuleFor(x => x.TimeoutSeconds)
            .GreaterThan(0).WithMessage(ValidationMessages.MustBePositive("Timeout"))
            .LessThanOrEqualTo(300).WithMessage("Timeout must not exceed 300 seconds");
        
        RuleFor(x => x.MaxRetries)
            .GreaterThanOrEqualTo(0).WithMessage("Max retries must be greater than or equal to 0")
            .LessThanOrEqualTo(10).WithMessage("Max retries must not exceed 10");
    }

    private bool BeValidProviderType(string providerType) => Enum.TryParse<ProviderType>(providerType, true, out _);
    private bool BeValidRoutingStrategy(string strategy) => 
        strategy.Equals("Path", StringComparison.OrdinalIgnoreCase) ||
        strategy.Equals("Header", StringComparison.OrdinalIgnoreCase) ||
        strategy.Equals("Subdomain", StringComparison.OrdinalIgnoreCase) ||
        strategy.Equals("UserConfig", StringComparison.OrdinalIgnoreCase);
    private bool BeValidUrl(string url) => Uri.TryCreate(url, UriKind.Absolute, out _);
}
