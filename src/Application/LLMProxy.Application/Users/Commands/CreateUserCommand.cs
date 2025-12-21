using FluentValidation;
using LLMProxy.Application.Common;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Users.Commands;

// ==================== Command ====================
public record CreateUserCommand : ICommand<UserDto>
{
    public Guid TenantId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public UserRole Role { get; init; } = UserRole.User;
}

// ==================== Validator ====================
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
    }
}

// ==================== Handler ====================
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify tenant exists
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(request.TenantId, cancellationToken);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
                return Result.Failure<UserDto>("Tenant not found.");
            }

            if (!tenant.IsActive)
            {
                _logger.LogWarning("Tenant {TenantId} is not active", request.TenantId);
                return Result.Failure<UserDto>("Cannot create user for inactive tenant.");
            }

            // Check if email already exists for this tenant
            if (await _unitOfWork.Users.EmailExistsAsync(request.TenantId, request.Email, cancellationToken))
            {
                _logger.LogWarning("User with email {Email} already exists in tenant {TenantId}", request.Email, request.TenantId);
                return Result.Failure<UserDto>($"User with email '{request.Email}' already exists.");
            }

            // Create user
            var userResult = User.Create(request.TenantId, request.Email, request.Name, request.Role);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("Failed to create user: {Error}", userResult.Error);
                return Result.Failure<UserDto>(userResult.Error!);
            }

            var user = userResult.Value;

            // Save to database
            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} created successfully for tenant {TenantId}", user.Id, request.TenantId);

            var dto = new UserDto
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

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email {Email}", request.Email);
            return Result.Failure<UserDto>("An error occurred while creating the user.");
        }
    }
}
