using LLMProxy.Domain.Entities;

namespace LLMProxy.Infrastructure.Security.Abstractions;

/// <summary>
/// Service de validation métier des clés API.
/// </summary>
/// <remarks>
/// Cette interface définit le contrat pour valider les règles métier
/// liées aux clés API (révocation, expiration, utilisateur actif).
/// Conforme à ADR-005 (Single Responsibility Principle) et ADR-023 (Result Pattern).
/// </remarks>
public interface IApiKeyValidator
{
    /// <summary>
    /// Valide une clé API selon les règles métier.
    /// </summary>
    /// <param name="apiKey">Clé API à valider.</param>
    /// <param name="user">Utilisateur associé à la clé API.</param>
    /// <returns>
    /// Résultat de validation indiquant si la clé est valide,
    /// avec un message d'erreur détaillé en cas d'échec.
    /// </returns>
    /// <remarks>
    /// Valide les règles suivantes :
    /// <list type="bullet">
    /// <item><description>La clé ne doit pas être révoquée</description></item>
    /// <item><description>La clé ne doit pas être expirée</description></item>
    /// <item><description>L'utilisateur associé doit être actif</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = validator.ValidateApiKey(apiKey, user);
    /// if (!result.IsValid)
    /// {
    ///     _logger.LogWarning("Validation échouée: {Error}", result.ErrorMessage);
    ///     return Unauthorized(result.ErrorMessage);
    /// }
    /// </code>
    /// </example>
    ApiKeyValidationResult ValidateApiKey(ApiKey apiKey, User user);
}
