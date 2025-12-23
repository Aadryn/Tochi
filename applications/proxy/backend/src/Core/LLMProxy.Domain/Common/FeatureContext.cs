namespace LLMProxy.Domain.Common;

/// <summary>
/// Contexte pour l'évaluation des feature flags (ADR-030).
/// </summary>
/// <remarks>
/// <para>
/// Ce contexte permet de cibler finement l'activation des fonctionnalités selon :
/// </para>
/// <list type="bullet">
/// <item><description><strong>Tenant</strong> : Activer pour certains tenants uniquement (canary)</description></item>
/// <item><description><strong>User</strong> : Activer pour beta testeurs spécifiques</description></item>
/// <item><description><strong>Environnement</strong> : Activer uniquement en dev/staging/prod</description></item>
/// <item><description><strong>Attributs personnalisés</strong> : Critères métier complexes (région, plan tarifaire, etc.)</description></item>
/// </list>
/// <para>
/// Utilise un <c>record</c> pour immutabilité et comparaison par valeur.
/// </para>
/// </remarks>
/// <example>
/// Contexte basique avec tenant :
/// <code>
/// var context = new FeatureContext 
/// { 
///     TenantId = tenantId 
/// };
/// </code>
/// 
/// Contexte enrichi avec attributs personnalisés :
/// <code>
/// var context = new FeatureContext
/// {
///     TenantId = tenantId,
///     UserId = userId,
///     Environment = "production",
///     CustomAttributes = new Dictionary&lt;string, string&gt;
///     {
///         ["region"] = "eu-west",
///         ["plan"] = "enterprise",
///         ["beta_tester"] = "true"
///     }
/// };
/// </code>
/// </example>
public record FeatureContext
{
    /// <summary>
    /// Identifiant du tenant pour lequel vérifier l'activation.
    /// </summary>
    /// <remarks>
    /// Si fourni, permet de cibler l'activation par tenant (canary deployments, rollout progressif).
    /// Exemple : Activer la nouvelle fonctionnalité pour 10% des tenants, puis 50%, puis 100%.
    /// </remarks>
    public Guid? TenantId { get; init; }

    /// <summary>
    /// Identifiant de l'utilisateur pour lequel vérifier l'activation.
    /// </summary>
    /// <remarks>
    /// Si fourni, permet de cibler l'activation par user (beta testeurs, early adopters).
    /// Exemple : Activer la fonctionnalité expérimentale pour une liste de users spécifiques.
    /// </remarks>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Environnement d'exécution (Development, Staging, Production).
    /// </summary>
    /// <remarks>
    /// Permet de limiter l'activation à certains environnements.
    /// Exemple : Activer uniquement en staging pour validation, puis en production.
    /// </remarks>
    public string? Environment { get; init; }

    /// <summary>
    /// Attributs personnalisés pour critères métier complexes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Permet d'implémenter des règles métier avancées :
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Région géographique</strong> : ["region"] = "eu-west"</description></item>
    /// <item><description><strong>Plan tarifaire</strong> : ["plan"] = "enterprise"</description></item>
    /// <item><description><strong>Version client</strong> : ["client_version"] = "2.5.0"</description></item>
    /// <item><description><strong>Flags métier</strong> : ["beta_tester"] = "true"</description></item>
    /// </list>
    /// <para>
    /// Les clés sont sensibles à la casse. Convention : snake_case lowercase.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// CustomAttributes = new Dictionary&lt;string, string&gt;
    /// {
    ///     ["region"] = "eu-west",
    ///     ["plan"] = "enterprise",
    ///     ["client_version"] = "2.5.0"
    /// };
    /// </code>
    /// </example>
    public Dictionary<string, string> CustomAttributes { get; init; } = new();

    /// <summary>
    /// Crée un contexte vide (aucun scoping).
    /// </summary>
    /// <remarks>
    /// Utilisé pour vérification globale quand aucun contexte tenant/user n'est disponible.
    /// </remarks>
    public static FeatureContext Empty => new();
}
