namespace LLMProxy.Domain.Common;

/// <summary>
/// Constantes centralisées des noms de feature flags (ADR-030).
/// </summary>
/// <remarks>
/// <para>
/// Cette classe centralise tous les noms de feature flags pour éviter :
/// </para>
/// <list type="bullet">
/// <item><description><strong>Typos</strong> : Erreurs de frappe détectées à la compilation</description></item>
/// <item><description><strong>Duplications</strong> : Nom utilisé de manière cohérente partout</description></item>
/// <item><description><strong>Recherche difficile</strong> : Un seul endroit pour trouver tous les flags</description></item>
/// <item><description><strong>Documentation dispersée</strong> : Documentation centralisée ici</description></item>
/// </list>
/// <para>
/// <strong>Convention de nommage</strong> : <c>{domaine}_{fonctionnalité}</c> en snake_case lowercase.
/// </para>
/// <para>
/// <strong>Lifecycle</strong> : Nettoyer les flags obsolètes après déploiement stable (pas de dette technique).
/// </para>
/// </remarks>
/// <example>
/// Utilisation avec IFeatureFlags :
/// <code>
/// if (_featureFlags.IsEnabled(FeatureNames.Llm_UseOptimizedProvider))
/// {
///     // Code de la nouvelle fonctionnalité
/// }
/// </code>
/// </example>
public static class FeatureNames
{
    // ═══════════════════════════════════════════════════════════════
    // LLM PROVIDERS - Gestion des providers de modèles de langage
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Active l'utilisation du provider LLM optimisé (cache local, batching).
    /// </summary>
    /// <remarks>
    /// <strong>Cas d'usage</strong> : Déploiement canary d'un nouveau provider avec meilleures performances.
    /// <strong>Impact</strong> : Performance (+20% latence), coût (-15% tokens).
    /// <strong>Rollback</strong> : Immédiat si problème détecté.
    /// </remarks>
    public const string Llm_UseOptimizedProvider = "llm_use_optimized_provider";

    /// <summary>
    /// Active le streaming des réponses LLM (chunks progressifs).
    /// </summary>
    /// <remarks>
    /// <strong>Cas d'usage</strong> : Améliorer UX avec affichage progressif des réponses.
    /// <strong>Impact</strong> : UX (meilleure perception vitesse), architecture (SSE/WebSocket requis).
    /// <strong>Prérequis</strong> : Client doit supporter streaming.
    /// </remarks>
    public const string Llm_EnableStreaming = "llm_enable_streaming";

    /// <summary>
    /// Active le nouveau compteur de tokens (BPE tokenizer optimisé).
    /// </summary>
    /// <remarks>
    /// <strong>Cas d'usage</strong> : Migration vers tokenizer plus précis et performant.
    /// <strong>Impact</strong> : Précision quotas (+5% exactitude), performance (+30% vitesse).
    /// <strong>Validation</strong> : Comparer avec ancien compteur pendant rollout.
    /// </remarks>
    public const string Llm_UseNewTokenCounter = "llm_use_new_token_counter";

    // ═══════════════════════════════════════════════════════════════
    // QUOTAS - Gestion des quotas de consommation
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Active le tracking enrichi des quotas (métriques détaillées, attribution par user).
    /// </summary>
    /// <remarks>
    /// <strong>Cas d'usage</strong> : Analytics avancés pour optimisation consommation.
    /// <strong>Impact</strong> : Observabilité (+50% métriques), storage (+10% Redis).
    /// <strong>Coût</strong> : Légère latence (+2ms par requête).
    /// </remarks>
    public const string Quota_EnhancedTracking = "quota_enhanced_tracking";

    /// <summary>
    /// Active les alertes temps réel lors de dépassement de quotas (webhooks).
    /// </summary>
    /// <remarks>
    /// <strong>Cas d'usage</strong> : Notifier tenant avant blocage (80%, 95%, 100%).
    /// <strong>Impact</strong> : UX (proactif), infrastructure (webhooks requis).
    /// <strong>Prérequis</strong> : Tenant doit configurer webhook URL.
    /// </remarks>
    public const string Quota_RealtimeAlerts = "quota_realtime_alerts";

    // ═══════════════════════════════════════════════════════════════
    // API - Fonctionnalités de l'API Gateway
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Active la v2 du rate limiting (algorithme sliding window + Redis).
    /// </summary>
    /// <remarks>
    /// <strong>Cas d'usage</strong> : Migration vers algorithme plus précis que fixed window.
    /// <strong>Impact</strong> : Précision (élimine burst exploitation), Redis (+ operations).
    /// <strong>Migration</strong> : Rollout progressif, comparer métriques v1 vs v2.
    /// </remarks>
    public const string Api_EnableRateLimitingV2 = "api_enable_rate_limiting_v2";

    /// <summary>
    /// Active le nouveau flow d'authentification (OAuth2 + JWT refresh tokens).
    /// </summary>
    /// <remarks>
    /// <strong>Cas d'usage</strong> : Migration API Keys → OAuth2 pour meilleure sécurité.
    /// <strong>Impact</strong> : Sécurité (+rotation tokens), compatibilité (breaking change).
    /// <strong>Migration</strong> : Support dual auth pendant période de transition.
    /// </remarks>
    public const string Api_NewAuthenticationFlow = "api_new_authentication_flow";

    // ═══════════════════════════════════════════════════════════════
    // UI - Fonctionnalités de l'interface utilisateur
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Active le nouveau tableau de bord (refonte UI/UX).
    /// </summary>
    /// <remarks>
    /// <strong>Cas d'usage</strong> : A/B testing nouveau dashboard vs ancien.
    /// <strong>Impact</strong> : UX (nouveau design), engagement (mesurer métriques).
    /// <strong>Rollout</strong> : 50/50 split pour comparer performances.
    /// </remarks>
    public const string Ui_NewDashboard = "ui_new_dashboard";

    /// <summary>
    /// Active le mode sombre (dark theme).
    /// </summary>
    /// <remarks>
    /// <strong>Cas d'usage</strong> : Préférence utilisateur, accessibilité.
    /// <strong>Impact</strong> : UX (confort visuel), aucun impact fonctionnel.
    /// <strong>Activation</strong> : Par user (préférence sauvegardée).
    /// </remarks>
    public const string Ui_DarkMode = "ui_dark_mode";

    // ═══════════════════════════════════════════════════════════════
    // EXPERIMENTAL - Fonctionnalités en phase expérimentale
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Active le cache sémantique des prompts (embedding similarity).
    /// </summary>
    /// <remarks>
    /// <strong>Cas d'usage</strong> : Expérimentation cache intelligent (réduire appels LLM).
    /// <strong>Impact</strong> : Performance (hit rate à mesurer), coût (−% tokens si efficace).
    /// <strong>Risque</strong> : Précision (faux positifs si similarité trop agressive).
    /// <strong>Statut</strong> : Expérimental, metrics intensives requises.
    /// </remarks>
    public const string Experimental_AiCaching = "experimental_ai_caching";

    /// <summary>
    /// Active la compression automatique des prompts (réduction tokens).
    /// </summary>
    /// <remarks>
    /// <strong>Cas d'usage</strong> : Optimiser coûts en compressant prompts longs.
    /// <strong>Impact</strong> : Coût (−20% tokens estimé), qualité (à valider).
    /// <strong>Risque</strong> : Dégradation qualité réponses si compression trop agressive.
    /// <strong>Statut</strong> : Expérimental, validation manuelle requise.
    /// </remarks>
    public const string Experimental_PromptCompression = "experimental_prompt_compression";
}
