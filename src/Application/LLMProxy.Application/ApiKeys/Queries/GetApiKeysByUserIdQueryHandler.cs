using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.ApiKeys.Queries;

/// <summary>
/// Gestionnaire pour la requête GetApiKeysByUserIdQuery
/// </summary>
public class GetApiKeysByUserIdQueryHandler : IRequestHandler<GetApiKeysByUserIdQuery, Result<IEnumerable<ApiKeyDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetApiKeysByUserIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<ApiKeyDto>>> Handle(GetApiKeysByUserIdQuery request, CancellationToken cancellationToken)
    {
        var apiKeys = await _unitOfWork.ApiKeys.GetByUserIdAsync(request.UserId, cancellationToken);
        var dtos = apiKeys.Select(k => new ApiKeyDto
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
