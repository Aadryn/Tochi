namespace LLMProxy.Gateway.Middleware;

/// <summary>
/// Attribut pour marquer un endpoint comme nécessitant une autorisation OpenFGA.
/// </summary>
/// <remarks>
/// Cet attribut est utilisé par <see cref="OpenFgaAuthorizationMiddleware"/>
/// pour déterminer les permissions requises pour accéder à un endpoint.
/// </remarks>
/// <example>
/// <code>
/// [FgaAuthorize("can_view", "tenant", ObjectIdRouteParameter = "tenantId")]
/// [HttpGet("tenants/{tenantId}")]
/// public async Task&lt;IActionResult&gt; GetTenant(string tenantId)
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class FgaAuthorizeAttribute : Attribute
{
    /// <summary>
    /// Initialise une nouvelle instance de <see cref="FgaAuthorizeAttribute"/>.
    /// </summary>
    /// <param name="relation">Relation requise (ex: "can_view", "can_modify", "admin").</param>
    /// <param name="objectType">Type d'objet OpenFGA (ex: "tenant", "provider").</param>
    public FgaAuthorizeAttribute(string relation, string objectType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relation);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);

        Relation = relation;
        ObjectType = objectType;
    }

    /// <summary>
    /// Relation requise pour accéder à l'endpoint.
    /// </summary>
    /// <example>can_view, can_modify, can_delete, admin, operator, viewer</example>
    public string Relation { get; }

    /// <summary>
    /// Type d'objet OpenFGA sur lequel vérifier la relation.
    /// </summary>
    /// <example>tenant, provider, api_key, route, config, stats, audit_log</example>
    public string ObjectType { get; }

    /// <summary>
    /// Nom du paramètre de route contenant l'identifiant de l'objet.
    /// </summary>
    /// <remarks>
    /// Si non spécifié, le middleware essaiera de détecter automatiquement
    /// l'identifiant depuis la route ou les headers.
    /// </remarks>
    /// <example>tenantId, providerId, id</example>
    public string? ObjectIdRouteParameter { get; set; }

    /// <summary>
    /// Indique si l'autorisation doit être vérifiée au niveau du tenant parent.
    /// </summary>
    /// <remarks>
    /// Utile pour les ressources enfants (providers, api_keys) où l'autorisation
    /// doit être vérifiée sur le tenant parent plutôt que sur la ressource elle-même.
    /// </remarks>
    public bool CheckTenantLevel { get; set; }

    /// <summary>
    /// Message d'erreur personnalisé en cas de refus d'accès.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
