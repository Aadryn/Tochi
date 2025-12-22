using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.Tenants.Queries;

/// <summary>
/// Gestionnaire pour la requête GetAllTenantsQuery
/// </summary>
public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, Result<IEnumerable<TenantDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllTenantsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<TenantDto>>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenantsResult = await _unitOfWork.Tenants.GetAllAsync(includeInactive: false, cancellationToken);
        if (tenantsResult.IsFailure)
        {
            return Result<IEnumerable<TenantDto>>.Failure(tenantsResult.Error);
        }
        
        var dtos = tenantsResult.Value.Select(t => new TenantDto
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            IsActive = t.IsActive,
            Settings = new TenantSettingsDto
            {
                MaxUsers = t.Settings.MaxUsers,
                MaxProviders = t.Settings.MaxProviders,
                EnableAuditLogging = t.Settings.EnableAuditLogging,
                AuditRetentionDays = t.Settings.AuditRetentionDays,
                EnableResponseCache = t.Settings.EnableResponseCache
            },
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt ?? DateTime.MinValue
        });

        return Result<IEnumerable<TenantDto>>.Success(dtos);
    }
}
