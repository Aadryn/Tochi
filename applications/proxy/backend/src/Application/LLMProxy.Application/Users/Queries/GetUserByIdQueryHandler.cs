using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.Users.Queries;

/// <summary>
/// Gestionnaire pour la requête GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userResult = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result<UserDto>.Failure(userResult.Error);
        }

        var user = userResult.Value;
        var dto = new UserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt ?? DateTime.MinValue
        };

        return dto;
    }
}
