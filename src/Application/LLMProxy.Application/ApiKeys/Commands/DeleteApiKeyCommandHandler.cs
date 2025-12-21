using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.ApiKeys.Commands;

/// <summary>
/// Gestionnaire pour la commande de suppression de clé API.
/// </summary>
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
