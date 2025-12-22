using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using MediatR;

namespace LLMProxy.Application.Tenants.Commands;

/// <summary>
/// Gestionnaire de la commande de mise à jour des paramètres d'un tenant.
/// </summary>
/// <remarks>
/// Récupère le tenant depuis la base de données, applique les nouveaux paramètres,
/// et persiste les modifications.
/// </remarks>
public class UpdateTenantSettingsCommandHandler : IRequestHandler<UpdateTenantSettingsCommand, Result<TenantDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="UpdateTenantSettingsCommandHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">L'unité de travail pour l'accès aux dépôts de données.</param>
    public UpdateTenantSettingsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Traite la commande de mise à jour des paramètres du tenant.
    /// </summary>
    /// <param name="request">La commande contenant les nouveaux paramètres.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération.</param>
    /// <returns>Résultat contenant le DTO du tenant mis à jour ou une erreur.</returns>
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
            return Result<TenantDto>.Failure(result.Error);
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
