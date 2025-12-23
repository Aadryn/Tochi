using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.Tenants.Commands;

/// <summary>
/// Gestionnaire pour la commande DeactivateTenantCommand
/// </summary>
public class DeactivateTenantCommandHandler : IRequestHandler<DeactivateTenantCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateTenantCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenantResult = await _unitOfWork.Tenants.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenantResult.IsFailure)
        {
            return tenantResult.Error;
        }

        var tenant = tenantResult.Value;
        tenant.Deactivate();
        
        var updateResult = await _unitOfWork.Tenants.UpdateAsync(tenant, cancellationToken);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
