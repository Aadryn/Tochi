using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Users.Commands;

/// <summary>
/// Gestionnaire de la commande de création d'un utilisateur.
/// </summary>
/// <remarks>
/// Vérifie l'existence et l'activité du tenant, l'unicité de l'email, crée l'utilisateur et le persiste en base.
/// Journalise les avertissements et erreurs durant le processus.
/// </remarks>
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="CreateUserCommandHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">L'unité de travail pour l'accès aux dépôts de données.</param>
    /// <param name="logger">Le service de journalisation.</param>
    public CreateUserCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Traite la commande de création de l'utilisateur.
    /// </summary>
    /// <param name="request">La commande contenant les informations de l'utilisateur à créer.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération.</param>
    /// <returns>Résultat contenant le DTO de l'utilisateur créé ou une erreur.</returns>
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenantValidation = await ValidateTenant(request.TenantId, cancellationToken);
            if (tenantValidation.IsFailure)
            {
                return Result.Failure<UserDto>(tenantValidation.Error!);
            }

            if (await _unitOfWork.Users.EmailExistsAsync(request.TenantId, request.Email, cancellationToken))
            {
                _logger.LogWarning("User with email {Email} already exists in tenant {TenantId}", request.Email, request.TenantId);
                return Result.Failure<UserDto>($"User with email '{request.Email}' already exists.");
            }

            var userResult = User.Create(request.TenantId, request.Email, request.Name, request.Role);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("Failed to create user: {Error}", userResult.Error);
                return Result.Failure<UserDto>(userResult.Error!);
            }

            var user = userResult.Value;
            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} created successfully for tenant {TenantId}", user.Id, request.TenantId);

            return Result.Success(MapToDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email {Email}", request.Email);
            return Result.Failure<UserDto>("An error occurred while creating the user.");
        }
    }

    private async Task<Domain.Common.Result> ValidateTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", tenantId);
            return Domain.Common.Result.Failure("Tenant not found.");
        }

        if (!tenant.IsActive)
        {
            _logger.LogWarning("Tenant {TenantId} is not active", tenantId);
            return Domain.Common.Result.Failure("Cannot create user for inactive tenant.");
        }

        return Domain.Common.Result.Success();
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt ?? DateTime.MinValue
        };
    }
}
