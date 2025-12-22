using LLMProxy.Application.Interfaces;
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;

namespace LLMProxy.Application.Services.RateLimiting;

/// <summary>
/// Service de rate limiting basé sur les tokens LLM consommés.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Ce service étend le rate limiting classique (par requête) en ajoutant
/// une dimension basée sur les tokens LLM réellement consommés. Cela permet
/// une facturation et un contrôle de quota plus précis.
/// </para>
/// <para>
/// <strong>Workflow typique :</strong>
/// </para>
/// <code>
/// // 1. Avant d'appeler le provider LLM (pré-validation)
/// var estimatedTokens = EstimateTokenCount(request);
/// var canProceed = await _tokenLimiter.CheckTokenLimitAsync(
///     tenantId, 
///     endpoint, 
///     estimatedTokens);
/// 
/// if (!canProceed.IsAllowed)
///     return StatusCode(429, "Token quota exceeded");
/// 
/// // 2. Appeler le provider LLM
/// var response = await _llmProvider.CallAsync(request);
/// 
/// // 3. Après l'appel (enregistrement réel)
/// var actualTokens = response.Usage.TotalTokens;
/// await _tokenLimiter.RecordTokenUsageAsync(
///     tenantId, 
///     endpoint, 
///     actualTokens);
/// </code>
/// </para>
/// <para>
/// <strong>Différence entre CheckTokenLimitAsync et RecordTokenUsageAsync :</strong>
/// <list type="bullet">
/// <item><description>CheckTokenLimitAsync : Vérification préalable avec ESTIMATION (bloquant si quota dépassé)</description></item>
/// <item><description>RecordTokenUsageAsync : Enregistrement post-appel avec VALEUR RÉELLE (non bloquant, incrémente compteur)</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class TokenBasedRateLimiter
{
    private readonly IRateLimiter _rateLimiter;
    private readonly IRateLimitConfigurationService _configService;

    /// <summary>
    /// Initialise une nouvelle instance du service de rate limiting par tokens.
    /// </summary>
    /// <param name="rateLimiter">Service de rate limiting Redis.</param>
    /// <param name="configService">Service de configuration des limites.</param>
    public TokenBasedRateLimiter(
        IRateLimiter rateLimiter,
        IRateLimitConfigurationService configService)
    {
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    /// <summary>
    /// Vérifie si un tenant peut consommer un nombre estimé de tokens.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="endpoint">Endpoint cible (ex: /v1/chat/completions).</param>
    /// <param name="estimatedTokens">Nombre estimé de tokens nécessaires.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns>Résultat de la vérification (autorisé ou rejeté).</returns>
    /// <remarks>
    /// <para>
    /// Cette méthode DOIT être appelée AVANT d'invoquer le provider LLM.
    /// Elle utilise une ESTIMATION du nombre de tokens pour éviter les dépassements.
    /// </para>
    /// <para>
    /// L'estimation peut être calculée avec :
    /// <list type="bullet">
    /// <item><description>Comptage approximatif : mots × 1.3 (pour textes anglais)</description></item>
    /// <item><description>Bibliothèque tiktoken (encodage exact selon modèle)</description></item>
    /// <item><description>Valeur max_tokens de la requête (limite supérieure)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public async Task<RateLimitResult> CheckTokenLimitAsync(
        Guid tenantId,
        string endpoint,
        int estimatedTokens,
        CancellationToken ct = default)
    {
        if (estimatedTokens <= 0)
            throw new ArgumentException("Le nombre de tokens estimés doit être positif", nameof(estimatedTokens));

        // Charger configuration
        var config = await _configService.GetConfigurationAsync(tenantId, ct);

        // Chercher limite spécifique à l'endpoint
        if (!config.EndpointLimits.TryGetValue(endpoint, out var endpointLimit))
        {
            endpointLimit = new Configuration.RateLimiting.EndpointLimit();
        }

        // Vérifier limite de tokens avec Token Bucket
        var key = $"ratelimit:tenant:{tenantId}:endpoint:{endpoint}:tokens";
        
        return await _rateLimiter.CheckTokenBucketAsync(
            key,
            capacity: endpointLimit.TokensPerMinute * 2, // Burst = 2× limite/min
            tokensPerInterval: endpointLimit.TokensPerMinute,
            interval: TimeSpan.FromMinutes(1),
            tokensRequired: estimatedTokens);
    }

    /// <summary>
    /// Enregistre le nombre réel de tokens consommés après un appel LLM.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="endpoint">Endpoint appelé.</param>
    /// <param name="actualTokens">Nombre réel de tokens consommés (prompt + completion).</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns>Tâche asynchrone.</returns>
    /// <remarks>
    /// <para>
    /// Cette méthode DOIT être appelée APRÈS la réponse du provider LLM.
    /// Elle enregistre la consommation réelle pour :
    /// <list type="bullet">
    /// <item><description>Facturation précise (usage.total_tokens)</description></item>
    /// <item><description>Statistiques de consommation</description></item>
    /// <item><description>Agrégations mensuelles</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// L'enregistrement est NON BLOQUANT : même si le quota est dépassé,
    /// la requête a déjà été exécutée. Cette méthode sert uniquement à la traçabilité.
    /// </para>
    /// </remarks>
    public async Task RecordTokenUsageAsync(
        Guid tenantId,
        string endpoint,
        int actualTokens,
        CancellationToken ct = default)
    {
        if (actualTokens <= 0)
            throw new ArgumentException("Le nombre de tokens réels doit être positif", nameof(actualTokens));

        // Enregistrer pour statistiques mensuelles
        var monthKey = $"usage:tenant:{tenantId}:month:{DateTime.UtcNow:yyyy-MM}:tokens";
        await _rateLimiter.IncrementAsync(monthKey, actualTokens);

        // Enregistrer par endpoint pour analyse détaillée
        var endpointKey = $"usage:tenant:{tenantId}:endpoint:{endpoint}:month:{DateTime.UtcNow:yyyy-MM}:tokens";
        await _rateLimiter.IncrementAsync(endpointKey, actualTokens);

        // Enregistrer total journalier (pour limites quotidiennes)
        var dayKey = $"usage:tenant:{tenantId}:day:{DateTime.UtcNow:yyyy-MM-dd}:tokens";
        await _rateLimiter.IncrementAsync(dayKey, actualTokens);
    }

    /// <summary>
    /// Récupère l'usage total de tokens pour un tenant sur le mois en cours.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="ct">Token d'annulation.</param>
    /// <returns>Nombre total de tokens consommés ce mois-ci.</returns>
    /// <remarks>
    /// <para>
    /// Utile pour :
    /// <list type="bullet">
    /// <item><description>Afficher un dashboard de consommation</description></item>
    /// <item><description>Alertes de dépassement de quota</description></item>
    /// <item><description>Facturation mensuelle</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public async Task<long> GetMonthlyTokenUsageAsync(
        Guid tenantId,
        CancellationToken ct = default)
    {
        var monthKey = $"usage:tenant:{tenantId}:month:{DateTime.UtcNow:yyyy-MM}:tokens";
        
        // IncrementAsync avec 0 retourne la valeur actuelle
        var result = await _rateLimiter.IncrementAsync(monthKey, 0);
        
        return result;
    }
}
