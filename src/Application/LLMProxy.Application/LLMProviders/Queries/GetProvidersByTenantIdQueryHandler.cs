using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.LLMProviders.Queries;

/// <summary>
/// Gestionnaire pour la requête GetProvidersByTenantIdQuery
/// </summary>
public class GetProvidersByTenantIdQueryHandler : IRequestHandler<GetProvidersByTenantIdQuery, Result<IEnumerable<LLMProviderDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProvidersByTenantIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<LLMProviderDto>>> Handle(GetProvidersByTenantIdQuery request, CancellationToken cancellationToken)
    {
        var providersResult = await _unitOfWork.Providers.GetByTenantIdAsync(request.TenantId, true, cancellationToken);
        if (providersResult.IsFailure)
        {
            return Result.Failure<IEnumerable<LLMProviderDto>>(providersResult.Error);
        }
        
        var dtos = providersResult.Value.Select(p => new LLMProviderDto
        {
            Id = p.Id,
            TenantId = p.TenantId,
            Name = p.Name,
            ProviderType = p.Type.ToString(),
            BaseUrl = p.BaseUrl,
            RoutingStrategy = p.RoutingStrategy.Method.ToString(),
            Priority = p.Priority,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt ?? DateTime.MinValue
        });

        return Result.Success(dtos);
    }
}
