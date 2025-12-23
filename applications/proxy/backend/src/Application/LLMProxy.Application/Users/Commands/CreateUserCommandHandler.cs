using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.Telemetry.Logging;
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
                return Result<UserDto>.Failure(tenantValidation.Error);
            }

            var emailExistsResult = await _unitOfWork.Users.EmailExistsAsync(request.TenantId, request.Email, cancellationToken);
            if (emailExistsResult.IsFailure)
                return Result<UserDto>.Failure(emailExistsResult.Error);
            
            if (emailExistsResult.Value)
            {
                _logger.UserCreationFailed(
                    new InvalidOperationException($"Email '{request.Email}' already exists in tenant '{request.TenantId}'"),
                    request.Email,
                    request.TenantId,
                    $"User with email '{request.Email}' already exists");
                return Error.User.EmailAlreadyExists(request.Email);
            }

            var userResult = User.Create(request.TenantId, request.Email, request.Name, request.Role);
            if (userResult.IsFailure)
            {
                _logger.UserCreationFailed(
                    new InvalidOperationException(userResult.Error.Message),
                    request.Email,
                    request.TenantId,
                    userResult.Error.Message);
                return Result<UserDto>.Failure(userResult.Error);
            }

            var user = userResult.Value;
            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.UserCreated(user.Id, user.Email, user.TenantId);

            return MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.UserCreationFailed(ex, request.Email, request.TenantId, "An error occurred while creating the user");
            return new Error("User.CreationError", "An error occurred while creating the user.");
        }
    }

    private async Task<Domain.Common.Result> ValidateTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenantResult = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        
        if (tenantResult.IsFailure)
        {
            _logger.LogWarning("Tenant {TenantId} not found", tenantId);
            return tenantResult.Error;
        }

        var tenant = tenantResult.Value;
        if (!tenant.IsActive)
        {
            _logger.LogWarning("Tenant {TenantId} is not active", tenantId);
            return Error.Tenant.Inactive(tenantId);
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
