using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.Users.Commands;

/// <summary>
/// Gestionnaire pour la commande de mise à jour d'utilisateur.
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<UserDto>(userResult.Error);
        }

        var user = userResult.Value;
        user.UpdateName(request.Name);
        var userRole = Enum.Parse<UserRole>(request.Role, true);
        user.UpdateRole(userRole);

        var updateResult = await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        if (updateResult.IsFailure)
        {
            return Result.Failure<UserDto>(updateResult.Error);
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
