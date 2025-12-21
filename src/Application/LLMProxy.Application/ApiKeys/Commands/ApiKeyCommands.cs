using FluentValidation;
using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.ApiKeys.Commands;

public record CreateApiKeyCommand : ICommand<ApiKeyDto>
{
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime? ExpiresAt { get; init; }
}

public class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ExpiresAt)
            .Must(expiry => !expiry.HasValue || expiry.Value > DateTime.UtcNow)
            .When(x => x.ExpiresAt.HasValue)
            .WithMessage("Expiration date must be in the future");
    }
}

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

public record RevokeApiKeyCommand : ICommand
{
    public Guid ApiKeyId { get; init; }
}

public class RevokeApiKeyCommandHandler : IRequestHandler<RevokeApiKeyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RevokeApiKeyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
    {
        var apiKey = await _unitOfWork.ApiKeys.GetByIdAsync(request.ApiKeyId, cancellationToken);
        if (apiKey == null)
        {
            return Result.Failure($"API Key with ID {request.ApiKeyId} not found");
        }

        apiKey.Revoke();
        await _unitOfWork.ApiKeys.UpdateAsync(apiKey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public record DeleteApiKeyCommand : ICommand
{
    public Guid ApiKeyId { get; init; }
}

public class DeleteApiKeyCommandHandler : IRequestHandler<DeleteApiKeyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteApiKeyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteApiKeyCommand request, CancellationToken cancellationToken)
    {
        var apiKey = await _unitOfWork.ApiKeys.GetByIdAsync(request.ApiKeyId, cancellationToken);
        if (apiKey == null)
        {
            return Result.Failure($"API Key with ID {request.ApiKeyId} not found");
        }

        await _unitOfWork.ApiKeys.DeleteAsync(request.ApiKeyId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
