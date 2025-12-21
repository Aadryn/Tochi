using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.Tenants.Commands;

public record DeactivateTenantCommand : ICommand
{
    public Guid TenantId { get; init; }
}

public class DeactivateTenantCommandHandler : IRequestHandler<DeactivateTenantCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateTenantCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            return Result.Failure($"Tenant with ID {request.TenantId} not found");
        }

        tenant.Deactivate();
        await _unitOfWork.Tenants.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
