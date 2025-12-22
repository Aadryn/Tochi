using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LLMProxy.Gateway.Controllers;

/// <summary>
/// Contrôleur de démonstration pour l'utilisation des Feature Flags (ADR-030).
/// </summary>
/// <remarks>
/// <para>
/// Ce contrôleur illustre différents patterns d'utilisation des feature flags :
/// </para>
/// <list type="bullet">
/// <item><description>Vérification simple (enabled/disabled)</description></item>
/// <item><description>Vérification avec contexte (tenant/user-specific)</description></item>
/// <item><description>Vérification async pour intégrations externes (Redis)</description></item>
/// </list>
/// <para>
/// <strong>Use Cases Illustrés</strong> :
/// </para>
/// <list type="bullet">
/// <item><description>Canary deployment : Activer nouvelles features pour 10% des tenants</description></item>
/// <item><description>Beta testing : Activer pour utilisateurs spécifiques</description></item>
/// <item><description>Feature de développement : Désactiver en production</description></item>
/// <item><description>Rollback instantané : Désactiver feature problématique sans redéployer</description></item>
/// </list>
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public sealed class FeatureFlagsExampleController : ControllerBase
{
    private readonly IFeatureFlags _featureFlags;
    private readonly ILogger<FeatureFlagsExampleController> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="FeatureFlagsExampleController"/>.
    /// </summary>
    public FeatureFlagsExampleController(
        IFeatureFlags featureFlags,
        ILogger<FeatureFlagsExampleController> logger)
    {
        _featureFlags = featureFlags;
        _logger = logger;
    }

    /// <summary>
    /// Exemple 1 : Vérification simple (enabled/disabled).
    /// </summary>
    /// <remarks>
    /// Use case : Feature globale on/off sans scoping.
    /// </remarks>
    [HttpGet("simple")]
    public IActionResult SimpleCheck()
    {
        // Vérifier si le dark mode est activé globalement
        var darkModeEnabled = _featureFlags.IsEnabled(FeatureNames.Ui_DarkMode);

        if (darkModeEnabled)
        {
            _logger.LogInformation("Dark mode is enabled, using dark theme");
            return Ok(new { theme = "dark", message = "Dark mode activated" });
        }
        else
        {
            _logger.LogInformation("Dark mode is disabled, using light theme");
            return Ok(new { theme = "light", message = "Light mode activated" });
        }
    }

    /// <summary>
    /// Exemple 2 : Vérification avec contexte tenant.
    /// </summary>
    /// <remarks>
    /// Use case : Canary deployment - activer feature pour 10% des tenants.
    /// </remarks>
    [HttpGet("tenant/{tenantId:guid}")]
    public IActionResult TenantSpecificCheck(Guid tenantId)
    {
        var context = new FeatureContext
        {
            TenantId = tenantId
        };

        // Vérifier si l'enhanced tracking est activé pour ce tenant
        var enhancedTrackingEnabled = _featureFlags.IsEnabled(
            FeatureNames.Quota_EnhancedTracking,
            context);

        if (enhancedTrackingEnabled)
        {
            _logger.LogInformation(
                "Enhanced tracking enabled for tenant {TenantId}",
                tenantId);
            return Ok(new
            {
                tenantId,
                feature = "enhanced_tracking",
                enabled = true,
                message = "Detailed metrics available"
            });
        }
        else
        {
            _logger.LogInformation(
                "Enhanced tracking disabled for tenant {TenantId}",
                tenantId);
            return Ok(new
            {
                tenantId,
                feature = "enhanced_tracking",
                enabled = false,
                message = "Standard metrics only"
            });
        }
    }

    /// <summary>
    /// Exemple 3 : Vérification async avec contexte complet.
    /// </summary>
    /// <remarks>
    /// Use case : Beta testing - activer pour utilisateurs spécifiques + tenant + environnement.
    /// </remarks>
    [HttpGet("async/{tenantId:guid}/{userId:guid}")]
    public async Task<IActionResult> AsyncCheck(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var context = new FeatureContext
        {
            TenantId = tenantId,
            UserId = userId,
            Environment = "Production",
            CustomAttributes = new Dictionary<string, string>
            {
                ["Region"] = "EU",
                ["PlanTier"] = "Enterprise"
            }
        };

        // Vérifier si l'AI caching est activé (async pour Redis future)
        var aiCachingEnabled = await _featureFlags.IsEnabledAsync(
            FeatureNames.Experimental_AiCaching,
            context,
            cancellationToken);

        if (aiCachingEnabled)
        {
            _logger.LogInformation(
                "AI Caching enabled for user {UserId} in tenant {TenantId}",
                userId,
                tenantId);
            return Ok(new
            {
                tenantId,
                userId,
                feature = "ai_caching",
                enabled = true,
                message = "Semantic cache activated - improved performance",
                estimatedCostReduction = "30%"
            });
        }
        else
        {
            _logger.LogInformation(
                "AI Caching disabled for user {UserId} in tenant {TenantId}",
                userId,
                tenantId);
            return Ok(new
            {
                tenantId,
                userId,
                feature = "ai_caching",
                enabled = false,
                message = "Standard processing - no cache"
            });
        }
    }

    /// <summary>
    /// Exemple 4 : Basculement conditionnel de provider LLM.
    /// </summary>
    /// <remarks>
    /// Use case : Rollout progressif nouveau provider avec rollback instantané.
    /// </remarks>
    [HttpGet("provider-selection/{tenantId:guid}")]
    public IActionResult ProviderSelection(Guid tenantId)
    {
        var context = new FeatureContext
        {
            TenantId = tenantId
        };

        // Vérifier si on utilise le provider optimisé
        var useOptimizedProvider = _featureFlags.IsEnabled(
            FeatureNames.Llm_UseOptimizedProvider,
            context);

        if (useOptimizedProvider)
        {
            // Nouveau provider avec cache local et batching
            _logger.LogInformation(
                "Using optimized LLM provider for tenant {TenantId}",
                tenantId);
            return Ok(new
            {
                provider = "OptimizedProvider",
                features = new[] { "local_cache", "request_batching", "smart_retry" },
                expectedLatencyReduction = "20%",
                expectedCostReduction = "15%"
            });
        }
        else
        {
            // Provider standard (stable)
            _logger.LogInformation(
                "Using standard LLM provider for tenant {TenantId}",
                tenantId);
            return Ok(new
            {
                provider = "StandardProvider",
                features = new[] { "basic_retry" },
                status = "stable"
            });
        }
    }

    /// <summary>
    /// Exemple 5 : Lister tous les feature flags activés.
    /// </summary>
    /// <remarks>
    /// Use case : Debugging, observabilité, support client.
    /// </remarks>
    [HttpGet("status")]
    public IActionResult GetAllFeaturesStatus()
    {
        // Récupérer tous les noms de features via reflection
        var allFeatureNames = typeof(FeatureNames)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => new
            {
                Name = (string)f.GetValue(null)!,
                Enabled = _featureFlags.IsEnabled((string)f.GetValue(null)!)
            })
            .OrderBy(f => f.Name)
            .ToList();

        var enabledCount = allFeatureNames.Count(f => f.Enabled);
        var disabledCount = allFeatureNames.Count - enabledCount;

        return Ok(new
        {
            totalFeatures = allFeatureNames.Count,
            enabledCount,
            disabledCount,
            features = allFeatureNames
        });
    }
}
