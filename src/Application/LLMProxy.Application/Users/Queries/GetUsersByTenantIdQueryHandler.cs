using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.Users.Queries;

/// <summary>
/// Gestionnaire pour la requête GetUsersByTenantIdQuery
/// </summary>
public class GetUsersByTenantIdQueryHandler : IRequestHandler<GetUsersByTenantIdQuery, Result<IEnumerable<UserDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUsersByTenantIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(GetUsersByTenantIdQuery request, CancellationToken cancellationToken)
    {
        var usersResult = await _unitOfWork.Users.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (usersResult.IsFailure)
        {
            return Result.Failure<IEnumerable<UserDto>>(usersResult.Error);
        }
        
        var dtos = usersResult.Value.Select(u => new UserDto
        {
            Id = u.Id,
            TenantId = u.TenantId,
            Email = u.Email,
            Name = u.Name,
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt ?? DateTime.MinValue
        });

        return Result.Success(dtos);
    }
}
