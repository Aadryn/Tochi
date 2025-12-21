using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.ApiKeys.Commands;

/// <summary>
/// Gestionnaire pour la commande de création de clé API.
/// </summary>
public class CreateApiKeyCommandHandler : IRequestHandler<CreateApiKeyCommand, Result<ApiKeyDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateApiKeyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ApiKeyDto>> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        // Verify user exists
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<ApiKeyDto>($"User with ID {request.UserId} not found");
        }

        // Generate API key - use the overload that returns ApiKey directly
        var plainKey = ApiKey.GenerateKey();
        var apiKey = ApiKey.Create(
            request.UserId,
            request.TenantId,
            request.Name,
            plainKey,
            request.ExpiresAt
        );

        await _unitOfWork.ApiKeys.AddAsync(apiKey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new ApiKeyDto
        {
            Id = apiKey.Id,
            UserId = apiKey.UserId,
            TenantId = apiKey.TenantId,
            Name = apiKey.Name,
            KeyPrefix = apiKey.KeyPrefix,
            IsRevoked = apiKey.IsRevoked(),
            ExpiresAt = apiKey.ExpiresAt,
            LastUsedAt = apiKey.LastUsedAt,
            CreatedAt = apiKey.CreatedAt,
            UpdatedAt = apiKey.UpdatedAt ?? DateTime.MinValue
        };

        return Result.Success(dto);
    }
}
