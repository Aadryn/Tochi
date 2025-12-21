using FluentValidation;
using LLMProxy.Domain.Entities;

namespace LLMProxy.Application.LLMProviders.Commands;

/// <summary>
/// Validateur pour la commande de création de fournisseur LLM.
/// </summary>
public class CreateProviderCommandValidator : AbstractValidator<CreateProviderCommand>
{
    public CreateProviderCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProviderType).NotEmpty().Must(BeValidProviderType).WithMessage("Invalid provider type");
        RuleFor(x => x.BaseUrl).NotEmpty().Must(BeValidUrl).WithMessage("Invalid URL format");
        RuleFor(x => x.RoutingStrategy).NotEmpty().Must(BeValidRoutingStrategy).WithMessage("Invalid routing strategy");
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TimeoutSeconds).GreaterThan(0).LessThanOrEqualTo(300);
        RuleFor(x => x.MaxRetries).GreaterThanOrEqualTo(0).LessThanOrEqualTo(10);
    }

    private bool BeValidProviderType(string providerType) => Enum.TryParse<ProviderType>(providerType, true, out _);
    private bool BeValidRoutingStrategy(string strategy) => 
        strategy.Equals("Path", StringComparison.OrdinalIgnoreCase) ||
        strategy.Equals("Header", StringComparison.OrdinalIgnoreCase) ||
        strategy.Equals("Subdomain", StringComparison.OrdinalIgnoreCase) ||
        strategy.Equals("UserConfig", StringComparison.OrdinalIgnoreCase);
    private bool BeValidUrl(string url) => Uri.TryCreate(url, UriKind.Absolute, out _);
}
