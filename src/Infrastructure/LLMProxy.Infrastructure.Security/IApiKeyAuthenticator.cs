using LLMProxy.Domain.Interfaces;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Service d'authentification par clé API.
/// </summary>
/// <remarks>
/// Cette interface définit le contrat pour authentifier un utilisateur
/// via une clé API fournie dans les en-têtes HTTP.
/// Conforme à ADR-014 (Dependency Injection) et ADR-023 (Result Pattern).
/// </remarks>
public interface IApiKeyAuthenticator
{
    /// <summary>
    /// Authentifie un utilisateur via une clé API brute.
    /// </summary>
    /// <param name="rawApiKey">Clé API en clair (non hachée).</param>
    /// <param name="unitOfWork">Unité de travail pour accéder au repository.</param>
    /// <param name="cancellationToken">Jeton d'annulation de l'opération.</param>
    /// <returns>
    /// Résultat de l'authentification contenant la clé API et l'utilisateur si succès,
    /// ou un message d'erreur et code de statut si échec.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await authenticator.AuthenticateAsync(
    ///     "sk-abc123...",
    ///     unitOfWork,
    ///     cancellationToken);
    /// 
    /// if (result.IsAuthenticated)
    /// {
    ///     Console.WriteLine($"Utilisateur {result.User.Email} authentifié");
    /// }
    /// </code>
    /// </example>
    Task<ApiKeyAuthenticationResult> AuthenticateAsync(
        string rawApiKey,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default);
}
