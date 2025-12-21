using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Tenants.Commands;

/// <summary>
/// Gestionnaire pour la commande CreateTenantCommand
/// </summary>
public class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, TenantDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateTenantCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TenantDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if slug already exists
            if (await _unitOfWork.Tenants.SlugExistsAsync(request.Slug, cancellationToken))
            {
                _logger.LogWarning("Tenant slug {Slug} already exists", request.Slug);
                return Result.Failure<TenantDto>($"Tenant with slug '{request.Slug}' already exists.");
            }

            // Map settings
            TenantSettings? settings = null;
            if (request.Settings != null)
            {
                settings = new TenantSettings(
                    request.Settings.MaxUsers,
                    request.Settings.MaxProviders,
                    request.Settings.EnableAuditLogging,
                    request.Settings.AuditRetentionDays,
                    request.Settings.EnableResponseCache
                );
            }

            // Create tenant
            var tenantResult = Tenant.Create(request.Name, request.Slug, settings);
            if (tenantResult.IsFailure)
            {
                _logger.LogWarning("Failed to create tenant: {Error}", tenantResult.Error);
                return Result.Failure<TenantDto>(tenantResult.Error!);
            }

            var tenant = tenantResult.Value;

            // Save to database
            await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tenant {TenantId} created successfully with slug {Slug}", tenant.Id, tenant.Slug);

            // Map to DTO
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
            _logger.LogError(ex, "Error creating tenant with slug {Slug}", request.Slug);
            return Result.Failure<TenantDto>("An error occurred while creating the tenant.");
        }
    }
}
