using FluentValidation;
using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.Users.Queries;

public record GetUserByIdQuery : IQuery<UserDto>
{
    public Guid UserId { get; init; }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<UserDto>($"User with ID {request.UserId} not found");
        }

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

        return Result.Success(dto);
    }
}

public record GetUsersByTenantIdQuery : IQuery<IEnumerable<UserDto>>
{
    public Guid TenantId { get; init; }
}

public class GetUsersByTenantIdQueryHandler : IRequestHandler<GetUsersByTenantIdQuery, Result<IEnumerable<UserDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUsersByTenantIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(GetUsersByTenantIdQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetByTenantIdAsync(request.TenantId, cancellationToken);
        var dtos = users.Select(u => new UserDto
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
