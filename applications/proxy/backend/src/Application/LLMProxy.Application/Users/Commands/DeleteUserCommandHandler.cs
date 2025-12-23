using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.Users.Commands;

/// <summary>
/// Gestionnaire pour la commande de suppression d'utilisateur.
/// </summary>
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (userResult.IsFailure)
        {
            return userResult.Error;
        }

        var deleteResult = await _unitOfWork.Users.DeleteAsync(request.UserId, cancellationToken);
        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
