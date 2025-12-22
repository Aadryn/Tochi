using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.LLMProviders.Commands;

/// <summary>
/// Gestionnaire pour la commande de suppression de fournisseur LLM.
/// </summary>
public class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProviderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
    {
        var providerResult = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken);
        if (providerResult.IsFailure)
        {
            return providerResult.Error;
        }

        var deleteResult = await _unitOfWork.Providers.DeleteAsync(request.ProviderId, cancellationToken);
        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
