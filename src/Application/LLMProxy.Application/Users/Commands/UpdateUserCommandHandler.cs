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
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<UserDto>($"User with ID {request.UserId} not found");
        }

        // Update user properties (you would add methods to User entity for this)
        user.UpdateName(request.Name);
        var userRole = Enum.Parse<UserRole>(request.Role, true);
        user.UpdateRole(userRole);

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
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
