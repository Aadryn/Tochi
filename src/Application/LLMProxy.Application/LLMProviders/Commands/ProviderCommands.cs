using FluentValidation;
using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.LLMProviders.Commands;

public record CreateProviderCommand : ICommand<LLMProviderDto>
{
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ProviderType { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string ApiKeySecretName { get; init; } = string.Empty;
    public string RoutingStrategy { get; init; } = string.Empty;
    public int Priority { get; init; }
    public Dictionary<string, string> CustomHeaders { get; init; } = new();
    public int TimeoutSeconds { get; init; } = 30;
    public int MaxRetries { get; init; } = 3;
}

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

public class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, Result<LLMProviderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProviderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LLMProviderDto>> Handle(CreateProviderCommand request, CancellationToken cancellationToken)
    {
        // Parse routing strategy
        var routingStrategy = request.RoutingStrategy.ToLowerInvariant() switch
        {
            "path" => RoutingStrategy.ByPath("/"),
            "header" => RoutingStrategy.ByHeader("X-Provider"),
            "subdomain" => RoutingStrategy.BySubdomain("api"),
            "userconfig" => RoutingStrategy.ByUser(),
            _ => RoutingStrategy.ByUser()
        };

        var provider = LLMProvider.Create(
            request.TenantId,
            request.Name,
            Enum.Parse<ProviderType>(request.ProviderType, true),
            request.BaseUrl,
            request.ApiKeySecretName,
            routingStrategy,
            request.Priority,
            request.CustomHeaders,
            request.TimeoutSeconds,
            request.MaxRetries
        );

        await _unitOfWork.Providers.AddAsync(provider, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new LLMProviderDto
        {
            Id = provider.Id,
            TenantId = provider.TenantId,
            Name = provider.Name,
            ProviderType = provider.Type.ToString(),
            BaseUrl = provider.BaseUrl,
            RoutingStrategy = provider.RoutingStrategy.Method.ToString(),
            Priority = provider.Priority,
            IsActive = provider.IsActive,
            CreatedAt = provider.CreatedAt,
            UpdatedAt = provider.UpdatedAt ?? DateTime.MinValue
        };

        return Result.Success(dto);
    }
}

public record UpdateProviderCommand : ICommand<LLMProviderDto>
{
    public Guid ProviderId { get; set; }
    public string? BaseUrl { get; init; }
    public int? Priority { get; init; }
    public Dictionary<string, string>? CustomHeaders { get; init; }
    public int? TimeoutSeconds { get; init; }
    public int? MaxRetries { get; init; }
}

public class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, Result<LLMProviderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProviderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LLMProviderDto>> Handle(UpdateProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider == null)
        {
            return Result.Failure<LLMProviderDto>($"Provider with ID {request.ProviderId} not found");
        }

        // Update only provided properties
        if (request.BaseUrl != null) provider.UpdateBaseUrl(request.BaseUrl);
        if (request.Priority.HasValue) provider.UpdatePriority(request.Priority.Value);
        if (request.CustomHeaders != null) provider.UpdateCustomHeaders(request.CustomHeaders);
        if (request.TimeoutSeconds.HasValue || request.MaxRetries.HasValue)
        {
            provider.UpdateConfiguration(
                request.TimeoutSeconds ?? provider.Configuration.TimeoutSeconds,
                request.MaxRetries ?? provider.Configuration.MaxRetries
            );
        }

        await _unitOfWork.Providers.UpdateAsync(provider, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new LLMProviderDto
        {
            Id = provider.Id,
            TenantId = provider.TenantId,
            Name = provider.Name,
            ProviderType = provider.Type.ToString(),
            BaseUrl = provider.BaseUrl,
            RoutingStrategy = provider.RoutingStrategy.Method.ToString(),
            Priority = provider.Priority,
            IsActive = provider.IsActive,
            CreatedAt = provider.CreatedAt,
            UpdatedAt = provider.UpdatedAt ?? DateTime.MinValue
        };

        return Result.Success(dto);
    }
}

public record DeleteProviderCommand : ICommand
{
    public Guid ProviderId { get; init; }
}

public class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProviderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider == null)
        {
            return Result.Failure($"Provider with ID {request.ProviderId} not found");
        }

        await _unitOfWork.Providers.DeleteAsync(request.ProviderId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
