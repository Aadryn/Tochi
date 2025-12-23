using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.LLMProviders.Queries;

/// <summary>
/// Gestionnaire pour la requête GetProviderByIdQuery
/// </summary>
public class GetProviderByIdQueryHandler : IRequestHandler<GetProviderByIdQuery, Result<LLMProviderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProviderByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LLMProviderDto>> Handle(GetProviderByIdQuery request, CancellationToken cancellationToken)
    {
        var providerResult = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);
        if (providerResult.IsFailure)
        {
            return Result<LLMProviderDto>.Failure(providerResult.Error);
        }

        var provider = providerResult.Value;
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
