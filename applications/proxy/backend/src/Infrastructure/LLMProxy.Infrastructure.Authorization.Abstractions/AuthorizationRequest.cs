namespace LLMProxy.Infrastructure.Authorization.Abstractions;

/// <summary>
/// Représente une requête de vérification d'autorisation ReBAC.
/// Cette classe encapsule les informations nécessaires pour vérifier
/// si un utilisateur a une relation spécifique avec un objet.
/// </summary>
/// <remarks>
/// Basé sur le modèle OpenFGA (Zanzibar-like) où les autorisations sont
/// définies par des relations entre utilisateurs et objets.
/// </remarks>
public sealed class AuthorizationRequest
{
    /// <summary>
    /// Initialise une nouvelle instance de <see cref="AuthorizationRequest"/>.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur effectuant la requête.</param>
    /// <param name="relation">Relation à vérifier (ex: "can_view", "can_modify", "admin").</param>
    /// <param name="objectType">Type de l'objet cible (ex: "tenant", "provider", "api_key").</param>
    /// <param name="objectId">Identifiant de l'objet cible.</param>
    public AuthorizationRequest(string userId, string relation, string objectType, string objectId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(relation);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectId);

        UserId = userId;
        Relation = relation;
        ObjectType = objectType;
        ObjectId = objectId;
    }

    /// <summary>
    /// Identifiant de l'utilisateur effectuant la requête.
    /// Format attendu : "user:{email}" ou "user:{id}".
    /// </summary>
    public string UserId { get; }

    /// <summary>
    /// Relation à vérifier entre l'utilisateur et l'objet.
    /// </summary>
    /// <example>
    /// Exemples de relations :
    /// - "admin" : droits d'administration
    /// - "can_view" : droits de lecture
    /// - "can_modify" : droits de modification
    /// - "can_delete" : droits de suppression
    /// - "owner" : propriétaire de la ressource
    /// </example>
    public string Relation { get; }

    /// <summary>
    /// Type de l'objet cible défini dans le modèle d'autorisation.
    /// </summary>
    /// <example>
    /// Types disponibles dans LLMProxy :
    /// - "organization"
    /// - "tenant"
    /// - "provider"
    /// - "api_key"
    /// - "route"
    /// - "config"
    /// - "stats"
    /// - "audit_log"
    /// </example>
    public string ObjectType { get; }

    /// <summary>
    /// Identifiant unique de l'objet cible.
    /// </summary>
    public string ObjectId { get; }

    /// <summary>
    /// Construit la représentation complète de l'objet au format OpenFGA.
    /// </summary>
    /// <returns>Format "type:id" (ex: "tenant:tenant-123").</returns>
    public string GetFullObject() => $"{ObjectType}:{ObjectId}";

    /// <summary>
    /// Construit la représentation complète de l'utilisateur au format OpenFGA.
    /// </summary>
    /// <returns>Format "user:id" si non préfixé, sinon retourne UserId tel quel.</returns>
    public string GetFullUser() => UserId.Contains(':') ? UserId : $"user:{UserId}";
}
