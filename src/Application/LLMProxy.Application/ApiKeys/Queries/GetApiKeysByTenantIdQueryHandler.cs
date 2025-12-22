using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.ApiKeys.Queries;

/// <summary>
/// Gestionnaire pour la requête GetApiKeysByTenantIdQuery
/// </summary>
public class GetApiKeysByTenantIdQueryHandler : IRequestHandler<GetApiKeysByTenantIdQuery, Result<IEnumerable<ApiKeyDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetApiKeysByTenantIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<ApiKeyDto>>> Handle(GetApiKeysByTenantIdQuery request, CancellationToken cancellationToken)
    {
        var apiKeysResult = await _unitOfWork.ApiKeys.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (apiKeysResult.IsFailure)
        {
            return Result.Failure<IEnumerable<ApiKeyDto>>(apiKeysResult.Error);
        }
        
        var dtos = apiKeysResult.Value.Select(k => new ApiKeyDto
        {
            Id = k.Id,
            UserId = k.UserId,
            TenantId = k.TenantId,
            Name = k.Name,
            KeyPrefix = k.KeyPrefix,
            IsRevoked = k.IsRevoked(),
            ExpiresAt = k.ExpiresAt,
            LastUsedAt = k.LastUsedAt,
            CreatedAt = k.CreatedAt,
            UpdatedAt = k.UpdatedAt ?? DateTime.MinValue
        });

        return Result.Success(dtos);
    }
}
