using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Interface pour vérifier l'état des feature flags (ADR-030).
/// </summary>
/// <remarks>
/// <para>
/// Les feature flags permettent d'activer/désactiver des fonctionnalités de manière dynamique,
/// sans redéploiement. Ils supportent plusieurs scénarios :
/// </para>
/// <list type="bullet">
/// <item><description>Activation globale (tous les tenants/users)</description></item>
/// <item><description>Activation par tenant (canary deployments)</description></item>
/// <item><description>Activation par user (beta testeurs)</description></item>
/// <item><description>Rollout progressif (pourcentage de traffic)</description></item>
/// <item><description>Activation par environnement (dev/staging/prod)</description></item>
/// </list>
/// <para>
/// Conforme aux ADR suivants :
/// - ADR-030 (Feature Toggles) : Contrôle dynamique des fonctionnalités
/// - ADR-014 (Dependency Injection) : Interface abstraite pour implémentations multiples
/// - ADR-009 (Fail Fast) : Validation rapide du contexte
/// </para>
/// </remarks>
/// <example>
/// Utilisation simple :
/// <code>
/// // Vérification globale
/// if (_featureFlags.IsEnabled(FeatureNames.Llm_UseOptimizedProvider))
/// {
///     return await _optimizedProvider.SendAsync(request, ct);
/// }
/// 
/// // Vérification avec contexte tenant
/// var context = new FeatureContext { TenantId = request.TenantId };
/// if (await _featureFlags.IsEnabledAsync(FeatureNames.Quota_EnhancedTracking, context, ct))
/// {
///     await _enhancedTracker.TrackAsync(request, ct);
/// }
/// </code>
/// </example>
public interface IFeatureFlags
{
    /// <summary>
    /// Vérifie si une fonctionnalité est activée globalement.
    /// </summary>
    /// <param name="featureName">Nom de la fonctionnalité (utiliser constantes <see cref="FeatureNames"/>).</param>
    /// <returns><c>true</c> si la fonctionnalité est activée globalement, sinon <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Si <paramref name="featureName"/> est vide.</exception>
    /// <remarks>
    /// Cette méthode vérifie uniquement l'activation globale, sans contexte tenant/user.
    /// Pour une vérification contextuelle, utiliser <see cref="IsEnabled(string, FeatureContext)"/>.
    /// </remarks>
    bool IsEnabled(string featureName);

    /// <summary>
    /// Vérifie si une fonctionnalité est activée pour un contexte donné (tenant, user, environnement).
    /// </summary>
    /// <param name="featureName">Nom de la fonctionnalité.</param>
    /// <param name="context">Contexte d'évaluation (tenant, user, attributs personnalisés).</param>
    /// <returns><c>true</c> si la fonctionnalité est activée pour ce contexte, sinon <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Si <paramref name="featureName"/> est vide.</exception>
    /// <exception cref="ArgumentNullException">Si <paramref name="context"/> est null.</exception>
    /// <remarks>
    /// <para>
    /// L'évaluation suit cette logique :
    /// </para>
    /// <list type="number">
    /// <item><description>Vérifier activation spécifique au tenant (si TenantId fourni)</description></item>
    /// <item><description>Vérifier activation spécifique au user (si UserId fourni)</description></item>
    /// <item><description>Vérifier rollout percentage (si configuré)</description></item>
    /// <item><description>Fallback sur activation globale</description></item>
    /// </list>
    /// </remarks>
    bool IsEnabled(string featureName, FeatureContext context);

    /// <summary>
    /// Vérifie de manière asynchrone si une fonctionnalité est activée pour un contexte donné.
    /// </summary>
    /// <param name="featureName">Nom de la fonctionnalité.</param>
    /// <param name="context">Contexte d'évaluation.</param>
    /// <param name="cancellationToken">Token d'annulation pour opérations I/O.</param>
    /// <returns>Tâche retournant <c>true</c> si activée, sinon <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Si <paramref name="featureName"/> est vide.</exception>
    /// <exception cref="ArgumentNullException">Si <paramref name="context"/> est null.</exception>
    /// <remarks>
    /// Utilisez cette méthode quand la vérification nécessite des opérations I/O
    /// (lecture Redis, base de données, appel distant).
    /// Pour une vérification purement en mémoire/cache, préférer <see cref="IsEnabled(string, FeatureContext)"/>.
    /// </remarks>
    Task<bool> IsEnabledAsync(string featureName, FeatureContext context, CancellationToken cancellationToken = default);
}
