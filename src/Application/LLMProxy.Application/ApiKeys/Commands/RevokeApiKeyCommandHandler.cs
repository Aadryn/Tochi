using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.ApiKeys.Commands;

/// <summary>
/// Gestionnaire pour la commande de révocation de clé API.
/// </summary>
public class RevokeApiKeyCommandHandler : IRequestHandler<RevokeApiKeyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RevokeApiKeyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
    {
        var apiKeyResult = await _unitOfWork.ApiKeys.GetByIdAsync(request.ApiKeyId, cancellationToken);
        if (apiKeyResult.IsFailure)
        {
            return apiKeyResult.Error;
        }

        var apiKey = apiKeyResult.Value;
        apiKey.Revoke();
        
        var updateResult = await _unitOfWork.ApiKeys.UpdateAsync(apiKey, cancellationToken);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
