namespace LLMProxy.Application.Authorization;

/// <summary>
/// Attribut marquant une requête MediatR comme nécessitant une vérification d'autorisation.
/// </summary>
/// <remarks>
/// <para>
/// Cet attribut est utilisé en conjonction avec <see cref="AuthorizationBehavior{TRequest, TResponse}"/>
/// pour effectuer une vérification d'autorisation ReBAC avant l'exécution du handler.
/// </para>
/// <para>
/// La requête doit implémenter <see cref="IAuthorizedRequest"/> pour fournir les informations
/// nécessaires à la vérification d'autorisation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [RequirePermission("can_view", "tenant")]
/// public record GetTenantQuery(string TenantId) : IRequest&lt;TenantDto&gt;, IAuthorizedRequest
/// {
///     public string GetUserId() => CurrentUser.Id;
///     public string GetObjectId() => TenantId;
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute : Attribute
{
    /// <summary>
    /// Initialise une nouvelle instance de <see cref="RequirePermissionAttribute"/>.
    /// </summary>
    /// <param name="relation">Relation requise (ex: "can_view", "can_modify", "admin").</param>
    /// <param name="objectType">Type d'objet sur lequel la relation est vérifiée (ex: "tenant", "provider").</param>
    public RequirePermissionAttribute(string relation, string objectType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relation);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);

        Relation = relation;
        ObjectType = objectType;
    }

    /// <summary>
    /// Relation requise pour accéder à la ressource.
    /// </summary>
    /// <example>can_view, can_modify, can_delete, admin, operator, viewer</example>
    public string Relation { get; }

    /// <summary>
    /// Type d'objet sur lequel la relation est vérifiée.
    /// </summary>
    /// <example>tenant, provider, api_key, route, config, stats, audit_log</example>
    public string ObjectType { get; }
}
