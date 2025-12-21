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
