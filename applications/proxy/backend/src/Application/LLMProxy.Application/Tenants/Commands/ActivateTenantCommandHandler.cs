using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.Tenants.Commands;

/// <summary>
/// Gestionnaire pour la commande ActivateTenantCommand
/// </summary>
public class ActivateTenantCommandHandler : IRequestHandler<ActivateTenantCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivateTenantCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenantResult = await _unitOfWork.Tenants.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenantResult.IsFailure)
        {
            return tenantResult.Error;
        }

        var tenant = tenantResult.Value;
        tenant.Activate();
        
        var updateResult = await _unitOfWork.Tenants.UpdateAsync(tenant, cancellationToken);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
