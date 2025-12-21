using FluentValidation;
using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.Tenants.Commands;

public record UpdateTenantSettingsCommand : ICommand<TenantDto>
{
    public Guid TenantId { get; set; }
    public int MaxUsers { get; init; }
    public int MaxProviders { get; init; }
    public bool EnableAuditLogging { get; init; }
    public int AuditRetentionDays { get; init; }
    public bool EnableResponseCache { get; init; }
}

public class UpdateTenantSettingsCommandValidator : AbstractValidator<UpdateTenantSettingsCommand>
{
    public UpdateTenantSettingsCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.MaxUsers).GreaterThan(0);
        RuleFor(x => x.MaxProviders).GreaterThan(0);
        RuleFor(x => x.AuditRetentionDays).GreaterThanOrEqualTo(0);
    }
}

public class UpdateTenantSettingsCommandHandler : IRequestHandler<UpdateTenantSettingsCommand, Result<TenantDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTenantSettingsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TenantDto>> Handle(UpdateTenantSettingsCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            return Result.Failure<TenantDto>($"Tenant with ID {request.TenantId} not found");
        }

        var newSettings = new Domain.Entities.TenantSettings(
            request.MaxUsers,
            request.MaxProviders,
            request.EnableAuditLogging,
            request.AuditRetentionDays,
            request.EnableResponseCache
        );

        var result = tenant.UpdateSettings(newSettings);
        if (result.IsFailure)
        {
            return Result.Failure<TenantDto>(result.Error!);
        }

        await _unitOfWork.Tenants.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
}
