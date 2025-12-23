using LLMProxy.Domain.Entities;
using Microsoft.Extensions.Logging;
using LLMProxy.Infrastructure.Security.Abstractions;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Implémentation du service de validation de clés API.
/// </summary>
/// <remarks>
/// Ce service valide les règles métier liées aux clés API :
/// révocation, expiration et statut de l'utilisateur associé.
/// Conforme à ADR-005 (Single Responsibility), ADR-009 (Fail Fast)
/// et ADR-023 (Result Pattern).
/// </remarks>
public class ApiKeyValidator : IApiKeyValidator
{
    private readonly ILogger<ApiKeyValidator> _logger;

    /// <summary>
    /// Initialise une nouvelle instance du validateur de clés API.
    /// </summary>
    /// <param name="logger">Logger pour enregistrer les événements.</param>
    public ApiKeyValidator(ILogger<ApiKeyValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Valide une clé API (vérification révocation, expiration, utilisateur actif).
    /// </summary>
    /// <param name="apiKey">Clé API à valider.</param>
    /// <param name="user">Utilisateur associé à la clé.</param>
    /// <returns>Résultat de la validation.</returns>
    public ApiKeyValidationResult ValidateApiKey(ApiKey apiKey, User user)
    {
        Guard.AgainstNull(apiKey, nameof(apiKey));
        Guard.AgainstNull(user, nameof(user));

        // Check if key is revoked
        if (apiKey.IsRevoked())
        {
            _logger.LogRevokedKeyUsage(apiKey.Id);
            return ApiKeyValidationResult.Failure("API key has been revoked");
        }

        // Check if key is expired
        if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
        {
            _logger.LogExpiredKeyUsage(apiKey.Id);
            return ApiKeyValidationResult.Failure("API key has expired");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _logger.LogInactiveUser(user.Id);
            return ApiKeyValidationResult.Failure("User account is inactive");
        }

        return ApiKeyValidationResult.Success();
    }
}
