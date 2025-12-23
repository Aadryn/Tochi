using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.LLMProviders.Commands;

/// <summary>
/// Gestionnaire pour la commande de mise à jour de fournisseur LLM.
/// </summary>
public class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, Result<LLMProviderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProviderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LLMProviderDto>> Handle(UpdateProviderCommand request, CancellationToken cancellationToken)
    {
        var providerResult = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);
        if (providerResult.IsFailure)
        {
            return Result<LLMProviderDto>.Failure(providerResult.Error);
        }

        var provider = providerResult.Value;
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

        var updateResult = await _unitOfWork.Providers.UpdateAsync(provider, cancellationToken);
        if (updateResult.IsFailure)
        {
            return Result<LLMProviderDto>.Failure(updateResult.Error);
        }
        
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
