using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.LLMProviders.Commands;

/// <summary>
/// Gestionnaire pour la commande de création de fournisseur LLM.
/// </summary>
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

        return dto;
    }
}
