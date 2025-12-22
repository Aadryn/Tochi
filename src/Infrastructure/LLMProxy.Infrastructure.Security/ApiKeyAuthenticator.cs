using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using LLMProxy.Infrastructure.Security.Abstractions;
using LLMProxy.Infrastructure.Telemetry.Logging;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Implémentation du service d'authentification par clé API.
/// </summary>
/// <remarks>
/// Ce service orchestre l'authentification complète d'une clé API :
/// - Extraction du préfixe pour optimiser la recherche en base
/// - Vérification du hash SHA-256 de la clé complète
/// - Validation métier (révocation, expiration, utilisateur actif)
/// Conforme à ADR-005 (SRP), ADR-014 (DI), ADR-023 (Result Pattern).
/// </remarks>
public class ApiKeyAuthenticator : IApiKeyAuthenticator
{
    private readonly IHashService _hashService;
    private readonly IApiKeyValidator _apiKeyValidator;
    private readonly ILogger<ApiKeyAuthenticator> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de l'authentificateur de clés API.
    /// </summary>
    public ApiKeyAuthenticator(
        IHashService hashService,
        IApiKeyValidator apiKeyValidator,
        ILogger<ApiKeyAuthenticator> logger)
    {
        _hashService = hashService;
        _apiKeyValidator = apiKeyValidator;
        _logger = logger;
    }

    /// <summary>
    /// Authentifie une requête via sa clé API.
    /// </summary>
    public async Task<ApiKeyAuthenticationResult> AuthenticateAsync(
        string rawApiKey,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNullOrWhiteSpace(rawApiKey, nameof(rawApiKey));
        Guard.AgainstNull(unitOfWork, nameof(unitOfWork));

        try
        {
            // Extract prefix to optimize lookup
            var prefix = rawApiKey.Length > 12 ? rawApiKey.Substring(0, 12) : rawApiKey;
            var apiKeyEntityResult = await unitOfWork.ApiKeys.GetByKeyPrefixAsync(prefix);

            if (apiKeyEntityResult.IsFailure)
            {
                _logger.ApiKeyValidationFailed(
                    Guid.Empty,
                    "Invalid API key prefix");
                return ApiKeyAuthenticationResult.Failure("Invalid API key");
            }

            var apiKeyEntity = apiKeyEntityResult.Value;
            
            // Verify full key hash
            var keyHash = _hashService.ComputeSha256Hash(rawApiKey);
            var validKeyResult = await unitOfWork.ApiKeys.GetByKeyHashAsync(keyHash);

            if (validKeyResult.IsFailure || validKeyResult.Value.Id != apiKeyEntity.Id)
            {
                _logger.ApiKeyValidationFailed(
                    apiKeyEntity.TenantId,
                    "Invalid API key hash");
                return ApiKeyAuthenticationResult.Failure("Invalid API key");
            }

            var validKey = validKeyResult.Value;
            
            // Get user
            var userResult = await unitOfWork.Users.GetByIdAsync(validKey.UserId);
            if (userResult.IsFailure)
            {
                _logger.ApiKeyValidationFailed(
                    validKey.TenantId,
                    "User not found");
                return ApiKeyAuthenticationResult.Failure("User not found");
            }

            var user = userResult.Value;

            // Validate API key (revocation, expiration, user active)
            var validationResult = _apiKeyValidator.ValidateApiKey(validKey, user);
            if (!validationResult.IsValid)
            {
                return ApiKeyAuthenticationResult.Failure(
                    validationResult.ErrorMessage ?? "Validation failed",
                    validationResult.StatusCode);
            }

            _logger.ApiKeyValidated(user.TenantId);

            return ApiKeyAuthenticationResult.Success(validKey, user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument during authentication");
            return ApiKeyAuthenticationResult.Failure("Invalid request", 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during API key authentication");
            return ApiKeyAuthenticationResult.Failure("Authentication error", 500);
        }
    }
}
