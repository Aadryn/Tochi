using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using LLMProxy.Infrastructure.Security.Abstractions;

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
            var apiKeyEntity = await unitOfWork.ApiKeys.GetByKeyPrefixAsync(prefix);

            if (apiKeyEntity == null)
            {
                _logger.LogWarning("Invalid API key prefix: {Prefix}", prefix);
                return ApiKeyAuthenticationResult.Failure("Invalid API key");
            }

            // Verify full key hash
            var keyHash = _hashService.ComputeSha256Hash(rawApiKey);
            var validKey = await unitOfWork.ApiKeys.GetByKeyHashAsync(keyHash);

            if (validKey == null || validKey.Id != apiKeyEntity.Id)
            {
                _logger.LogWarning("Invalid API key hash for user {UserId}", apiKeyEntity.UserId);
                return ApiKeyAuthenticationResult.Failure("Invalid API key");
            }

            // Get user
            var user = await unitOfWork.Users.GetByIdAsync(validKey.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found for API key: {UserId}", validKey.UserId);
                return ApiKeyAuthenticationResult.Failure("User not found");
            }

            // Validate API key (revocation, expiration, user active)
            var validationResult = _apiKeyValidator.ValidateApiKey(validKey, user);
            if (!validationResult.IsValid)
            {
                return ApiKeyAuthenticationResult.Failure(
                    validationResult.ErrorMessage ?? "Validation failed",
                    validationResult.StatusCode);
            }

            _logger.LogInformation("API key authenticated: User={UserId}, Tenant={TenantId}",
                user.Id, user.TenantId);

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
