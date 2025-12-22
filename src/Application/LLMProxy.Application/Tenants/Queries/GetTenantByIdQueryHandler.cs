using LLMProxy.Application.Common;
using LLMProxy.Application.Tenants.Commands;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Tenants.Queries;

/// <summary>
/// Gestionnaire de la requête de récupération d'un tenant par identifiant.
/// </summary>
/// <remarks>
/// Recherche le tenant dans la base de données et construit un DTO complet avec ses paramètres.
/// Journalise les avertissements si le tenant est introuvable et les erreurs en cas d'exception.
/// </remarks>
public class GetTenantByIdQueryHandler : IQueryHandler<GetTenantByIdQuery, TenantDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetTenantByIdQueryHandler> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="GetTenantByIdQueryHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">L'unité de travail pour l'accès aux dépôts de données.</param>
    /// <param name="logger">Le service de journalisation.</param>
    public GetTenantByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetTenantByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Traite la requête de récupération du tenant.
    /// </summary>
    /// <param name="request">La requête contenant l'identifiant du tenant.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération.</param>
    /// <returns>Résultat contenant le DTO du tenant ou une erreur si introuvable ou exception.</returns>
    public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantResult = await _unitOfWork.Tenants.GetByIdAsync(request.TenantId, cancellationToken);

            if (tenantResult.IsFailure)
            {
                _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
                return Result<TenantDto>.Failure(tenantResult.Error);
            }

            var tenant = tenantResult.Value;
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

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant {TenantId}", request.TenantId);
            return new Error("Tenant.QueryError", "An error occurred while retrieving the tenant.");
        }
    }
}
