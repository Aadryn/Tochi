using FluentValidation;
using LLMProxy.Application.Common;
using LLMProxy.Application.Tenants.Commands;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Tenants.Queries;

// ==================== Query ====================
public record GetTenantByIdQuery : IQuery<TenantDto>
{
    public Guid TenantId { get; init; }
}

// ==================== Validator ====================
public class GetTenantByIdQueryValidator : AbstractValidator<GetTenantByIdQuery>
{
    public GetTenantByIdQueryValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");
    }
}

// ==================== Handler ====================
public class GetTenantByIdQueryHandler : IQueryHandler<GetTenantByIdQuery, TenantDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetTenantByIdQueryHandler> _logger;

    public GetTenantByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetTenantByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(request.TenantId, cancellationToken);

            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
                return Result.Failure<TenantDto>("Tenant not found.");
            }

            var dto = new TenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                IsActive = tenant.IsActive,
                Settings = new TenantSettingsDto
                {
                    MaxUsers = tenant.Settings.MaxUsers,
                    MaxProviders = tenant.Settings.MaxProviders,
                    EnableAuditLogging = tenant.Settings.EnableAuditLogging,
                    AuditRetentionDays = tenant.Settings.AuditRetentionDays,
                    EnableResponseCache = tenant.Settings.EnableResponseCache
                },
                CreatedAt = tenant.CreatedAt,
                UpdatedAt = tenant.UpdatedAt ?? DateTime.MinValue
            };

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant {TenantId}", request.TenantId);
            return Result.Failure<TenantDto>("An error occurred while retrieving the tenant.");
        }
    }
}
