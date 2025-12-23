namespace LLMProxy.Application.Authorization;

/// <summary>
/// Interface que les requêtes MediatR doivent implémenter pour supporter l'autorisation ReBAC.
/// </summary>
/// <remarks>
/// Cette interface fournit les informations nécessaires au <see cref="AuthorizationBehavior{TRequest, TResponse}"/>
/// pour effectuer les vérifications d'autorisation.
/// </remarks>
public interface IAuthorizedRequest
{
    /// <summary>
    /// Obtient l'identifiant de l'utilisateur effectuant la requête.
    /// </summary>
    /// <returns>
    /// Identifiant utilisateur au format attendu par OpenFGA (ex: "user:email@example.com").
    /// </returns>
    string GetUserId();

    /// <summary>
    /// Obtient l'identifiant de l'objet sur lequel vérifier l'autorisation.
    /// </summary>
    /// <returns>
    /// Identifiant de l'objet cible (ex: "tenant-123", "provider-456").
    /// </returns>
    string GetObjectId();
}
