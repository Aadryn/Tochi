# 30. Feature Toggles (Feature Flags)

Date: 2025-12-21

## Statut

Accepté

## Contexte

Le déploiement de nouvelles fonctionnalités présente des risques :
- **Big bang releases** : Tout ou rien, risque élevé
- **Rollback difficile** : Redéployer pour désactiver
- **Tests en production** : Impossible sans feature flags
- **Releases progressives** : Canary, A/B testing impossibles

```csharp
// ❌ SANS FEATURE FLAG : Code conditionnel en dur
public class LlmService
{
    public async Task<LlmResponse> SendRequestAsync(LlmRequest request)
    {
        // Nouvelle fonctionnalité ? Déployer et croiser les doigts...
        var response = await _newOptimizedProvider.SendAsync(request);
        return response;
    }
}
```

## Décision

**Utiliser des Feature Toggles pour contrôler l'activation des fonctionnalités de manière dynamique, sans redéploiement.**

### 1. Interface Feature Flag

```csharp
/// <summary>
/// Interface pour vérifier l'état des feature flags.
/// </summary>
public interface IFeatureFlags
{
    /// <summary>
    /// Vérifie si une fonctionnalité est activée.
    /// </summary>
    bool IsEnabled(string featureName);
    
    /// <summary>
    /// Vérifie si une fonctionnalité est activée pour un contexte donné.
    /// </summary>
    bool IsEnabled(string featureName, FeatureContext context);
    
    /// <summary>
    /// Version async pour les vérifications nécessitant I/O.
    /// </summary>
    Task<bool> IsEnabledAsync(string featureName, CancellationToken ct = default);
}

/// <summary>
/// Contexte pour les feature flags (tenant, user, etc.).
/// </summary>
public record FeatureContext
{
    public Guid? TenantId { get; init; }
    public Guid? UserId { get; init; }
    public string? Environment { get; init; }
    public decimal? PercentageRollout { get; init; }
    public Dictionary<string, string> CustomAttributes { get; init; } = new();
}
```

### 2. Constantes des feature flags

```csharp
/// <summary>
/// Noms des feature flags - centralisés.
/// </summary>
public static class FeatureNames
{
    // Format: {domaine}_{fonctionnalité}
    
    // LLM Provider
    public const string Llm_UseOptimizedProvider = "llm_use_optimized_provider";
    public const string Llm_EnableStreaming = "llm_enable_streaming";
    public const string Llm_UseNewTokenCounter = "llm_use_new_token_counter";
    
    // Quotas
    public const string Quota_EnhancedTracking = "quota_enhanced_tracking";
    public const string Quota_RealTimeAlerts = "quota_realtime_alerts";
    
    // API
    public const string Api_EnableRateLimitingV2 = "api_enable_rate_limiting_v2";
    public const string Api_NewAuthenticationFlow = "api_new_authentication_flow";
    
    // UI
    public const string Ui_NewDashboard = "ui_new_dashboard";
    public const string Ui_DarkMode = "ui_dark_mode";
    
    // Expérimental
    public const string Experimental_AiCaching = "experimental_ai_caching";
}
```

### 3. Implémentation simple (configuration)

```csharp
/// <summary>
/// Feature flags basés sur la configuration.
/// </summary>
public class ConfigurationFeatureFlags : IFeatureFlags
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationFeatureFlags> _logger;
    
    public ConfigurationFeatureFlags(
        IConfiguration configuration,
        ILogger<ConfigurationFeatureFlags> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public bool IsEnabled(string featureName)
    {
        var value = _configuration[$"FeatureFlags:{featureName}"];
        var isEnabled = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        
        _logger.LogDebug(
            "Feature flag {FeatureName} is {Status}", 
            featureName, 
            isEnabled ? "enabled" : "disabled");
        
        return isEnabled;
    }
    
    public bool IsEnabled(string featureName, FeatureContext context)
    {
        // Version simple : ignore le contexte
        return IsEnabled(featureName);
    }
    
    public Task<bool> IsEnabledAsync(string featureName, CancellationToken ct)
    {
        return Task.FromResult(IsEnabled(featureName));
    }
}

// appsettings.json
/*
{
  "FeatureFlags": {
    "llm_use_optimized_provider": "true",
    "llm_enable_streaming": "false",
    "experimental_ai_caching": "false"
  }
}
*/
```

### 4. Implémentation avec pourcentage de rollout

```csharp
/// <summary>
/// Feature flags avec rollout progressif.
/// </summary>
public class AdvancedFeatureFlags : IFeatureFlags
{
    private readonly IFeatureFlagRepository _repository;
    private readonly ILogger<AdvancedFeatureFlags> _logger;
    
    public bool IsEnabled(string featureName, FeatureContext context)
    {
        var flag = _repository.GetFlag(featureName);
        
        if (flag is null || !flag.IsGloballyEnabled)
            return false;
        
        // Vérifier les overrides par tenant
        if (context.TenantId.HasValue)
        {
            var tenantOverride = flag.TenantOverrides
                .FirstOrDefault(o => o.TenantId == context.TenantId);
            
            if (tenantOverride is not null)
                return tenantOverride.IsEnabled;
        }
        
        // Vérifier le rollout progressif
        if (flag.RolloutPercentage < 100)
        {
            return IsInRolloutGroup(context, flag.RolloutPercentage);
        }
        
        return true;
    }
    
    /// <summary>
    /// Détermine si le contexte fait partie du groupe de rollout.
    /// </summary>
    private bool IsInRolloutGroup(FeatureContext context, int percentage)
    {
        // Utiliser un hash stable pour consistance
        var identifier = context.UserId?.ToString() 
            ?? context.TenantId?.ToString() 
            ?? Guid.NewGuid().ToString();
        
        var hash = Math.Abs(identifier.GetHashCode());
        var bucket = hash % 100;
        
        return bucket < percentage;
    }
}

/// <summary>
/// Entité Feature Flag en base.
/// </summary>
public class FeatureFlag
{
    public string Name { get; set; } = default!;
    public bool IsGloballyEnabled { get; set; }
    public int RolloutPercentage { get; set; } = 100;
    public DateTime? EnabledFrom { get; set; }
    public DateTime? EnabledUntil { get; set; }
    public List<TenantFeatureOverride> TenantOverrides { get; set; } = new();
}

public class TenantFeatureOverride
{
    public Guid TenantId { get; set; }
    public bool IsEnabled { get; set; }
}
```

### 5. Utilisation dans les services

```csharp
public class LlmService
{
    private readonly IFeatureFlags _featureFlags;
    private readonly ILlmProvider _defaultProvider;
    private readonly ILlmProvider _optimizedProvider;
    
    /// <summary>
    /// Envoie une requête LLM avec feature flag pour le provider.
    /// </summary>
    public async Task<LlmResponse> SendRequestAsync(
        LlmRequest request,
        CancellationToken ct)
    {
        var context = new FeatureContext { TenantId = request.TenantId };
        
        // Choisir le provider selon le feature flag
        var provider = _featureFlags.IsEnabled(
            FeatureNames.Llm_UseOptimizedProvider, 
            context)
            ? _optimizedProvider
            : _defaultProvider;
        
        var response = await provider.SendAsync(request, ct);
        
        // Feature flag pour le streaming
        if (_featureFlags.IsEnabled(FeatureNames.Llm_EnableStreaming, context))
        {
            response = await EnhanceWithStreamingAsync(response, ct);
        }
        
        return response;
    }
}
```

### 6. Attribute pour feature flags

```csharp
/// <summary>
/// Attribute pour conditionner un endpoint par feature flag.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class FeatureGateAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _featureName;
    
    public FeatureGateAttribute(string featureName)
    {
        _featureName = featureName;
    }
    
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        var featureFlags = context.HttpContext.RequestServices
            .GetRequiredService<IFeatureFlags>();
        
        if (!featureFlags.IsEnabled(_featureName))
        {
            context.Result = new NotFoundResult();
            return;
        }
        
        await next();
    }
}

// Utilisation
[ApiController]
[Route("api/v2/dashboard")]
[FeatureGate(FeatureNames.Ui_NewDashboard)]
public class DashboardV2Controller : ControllerBase
{
    [HttpGet]
    public IActionResult GetDashboard()
    {
        // Accessible seulement si feature flag activé
        return Ok(new { version = "v2" });
    }
}
```

### 7. Feature flags dans Blazor/UI

```csharp
/// <summary>
/// Service pour les feature flags côté UI.
/// </summary>
public class UiFeatureService
{
    private readonly IFeatureFlags _featureFlags;
    private readonly AuthenticationStateProvider _authStateProvider;
    
    /// <summary>
    /// Vérifie si une fonctionnalité UI est activée pour l'utilisateur courant.
    /// </summary>
    public async Task<bool> IsEnabledForCurrentUserAsync(string featureName)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        var context = new FeatureContext
        {
            UserId = user.GetUserId(),
            TenantId = user.GetTenantId()
        };
        
        return _featureFlags.IsEnabled(featureName, context);
    }
}

// Dans un composant Blazor
@inject UiFeatureService FeatureService

@if (_showNewDashboard)
{
    <NewDashboard />
}
else
{
    <LegacyDashboard />
}

@code {
    private bool _showNewDashboard;
    
    protected override async Task OnInitializedAsync()
    {
        _showNewDashboard = await FeatureService
            .IsEnabledForCurrentUserAsync(FeatureNames.Ui_NewDashboard);
    }
}
```

### 8. Administration des feature flags

```csharp
/// <summary>
/// API d'administration des feature flags.
/// </summary>
[ApiController]
[Route("api/admin/feature-flags")]
[Authorize(Roles = "Admin")]
public class FeatureFlagsAdminController : ControllerBase
{
    private readonly IFeatureFlagRepository _repository;
    
    /// <summary>
    /// Liste tous les feature flags.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<FeatureFlagDto>>> GetAll(
        CancellationToken ct)
    {
        var flags = await _repository.GetAllAsync(ct);
        return Ok(flags.Select(f => f.ToDto()));
    }
    
    /// <summary>
    /// Active/désactive un feature flag.
    /// </summary>
    [HttpPut("{name}")]
    public async Task<ActionResult> UpdateFlag(
        string name,
        [FromBody] UpdateFeatureFlagRequest request,
        CancellationToken ct)
    {
        var flag = await _repository.GetByNameAsync(name, ct);
        
        if (flag is null)
            return NotFound();
        
        flag.IsGloballyEnabled = request.IsEnabled;
        flag.RolloutPercentage = request.RolloutPercentage;
        
        await _repository.SaveAsync(flag, ct);
        
        return NoContent();
    }
    
    /// <summary>
    /// Override pour un tenant spécifique.
    /// </summary>
    [HttpPut("{name}/tenants/{tenantId}")]
    public async Task<ActionResult> SetTenantOverride(
        string name,
        Guid tenantId,
        [FromBody] TenantOverrideRequest request,
        CancellationToken ct)
    {
        var flag = await _repository.GetByNameAsync(name, ct);
        
        if (flag is null)
            return NotFound();
        
        flag.SetTenantOverride(tenantId, request.IsEnabled);
        
        await _repository.SaveAsync(flag, ct);
        
        return NoContent();
    }
}
```

### 9. Tests avec feature flags

```csharp
/// <summary>
/// Fake feature flags pour les tests.
/// </summary>
public class TestFeatureFlags : IFeatureFlags
{
    private readonly Dictionary<string, bool> _flags = new();
    
    public TestFeatureFlags Enable(string featureName)
    {
        _flags[featureName] = true;
        return this;
    }
    
    public TestFeatureFlags Disable(string featureName)
    {
        _flags[featureName] = false;
        return this;
    }
    
    public bool IsEnabled(string featureName)
    {
        return _flags.TryGetValue(featureName, out var value) && value;
    }
    
    public bool IsEnabled(string featureName, FeatureContext context)
    {
        return IsEnabled(featureName);
    }
    
    public Task<bool> IsEnabledAsync(string featureName, CancellationToken ct)
    {
        return Task.FromResult(IsEnabled(featureName));
    }
}

/// <summary>
/// Tests du service avec feature flags.
/// </summary>
public class LlmServiceTests
{
    [Fact]
    public async Task Should_Use_Optimized_Provider_When_Flag_Enabled()
    {
        // Arrange
        var featureFlags = new TestFeatureFlags()
            .Enable(FeatureNames.Llm_UseOptimizedProvider);
        
        var defaultProvider = Substitute.For<ILlmProvider>();
        var optimizedProvider = Substitute.For<ILlmProvider>();
        
        optimizedProvider
            .SendAsync(Arg.Any<LlmRequest>(), Arg.Any<CancellationToken>())
            .Returns(new LlmResponse { Success = true });
        
        var service = new LlmService(featureFlags, defaultProvider, optimizedProvider);
        
        // Act
        await service.SendRequestAsync(new LlmRequest(), CancellationToken.None);
        
        // Assert
        await optimizedProvider.Received(1).SendAsync(
            Arg.Any<LlmRequest>(), 
            Arg.Any<CancellationToken>());
        
        await defaultProvider.DidNotReceive().SendAsync(
            Arg.Any<LlmRequest>(), 
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Should_Use_Default_Provider_When_Flag_Disabled()
    {
        // Arrange
        var featureFlags = new TestFeatureFlags()
            .Disable(FeatureNames.Llm_UseOptimizedProvider);
        
        var defaultProvider = Substitute.For<ILlmProvider>();
        var optimizedProvider = Substitute.For<ILlmProvider>();
        
        var service = new LlmService(featureFlags, defaultProvider, optimizedProvider);
        
        // Act
        await service.SendRequestAsync(new LlmRequest(), CancellationToken.None);
        
        // Assert
        await defaultProvider.Received(1).SendAsync(
            Arg.Any<LlmRequest>(), 
            Arg.Any<CancellationToken>());
    }
}
```

## Conséquences

### Positives

- **Déploiement sécurisé** : Activer progressivement
- **Rollback instantané** : Désactiver sans redéployer
- **Tests en production** : Canary releases, A/B testing
- **Customisation** : Features par tenant
- **Trunk-based development** : Code incomplet derrière flag

### Négatives

- **Dette technique** : Flags à nettoyer après release
  - *Mitigation* : Processus de cleanup, dates d'expiration
- **Complexité** : Combinaisons de flags à tester
  - *Mitigation* : Limiter le nombre de flags actifs
- **Conditionnels** : Code parsemé de `if (flag)`
  - *Mitigation* : Strategy pattern plutôt que if/else

### Neutres

- Les feature flags sont un outil puissant mais à utiliser avec modération

## Alternatives considérées

### Option A : Branches de release longues

- **Description** : Feature branches mergées à la release
- **Avantages** : Pas de flags dans le code
- **Inconvénients** : Merge hell, intégration tardive
- **Raison du rejet** : Contre trunk-based development

### Option B : Feature flags en dur (#if DEBUG)

- **Description** : Compilation conditionnelle
- **Avantages** : Simple
- **Inconvénients** : Nécessite recompilation
- **Raison du rejet** : Pas de contrôle runtime

### Option C : Service externe (LaunchDarkly, Split)

- **Description** : SaaS de feature management
- **Avantages** : UI riche, analytics, targeting avancé
- **Inconvénients** : Coût, dépendance externe
- **Raison du rejet** : Acceptable pour grands projets

## Références

- [Feature Toggles - Martin Fowler](https://martinfowler.com/articles/feature-toggles.html)
- [Microsoft Feature Management](https://docs.microsoft.com/en-us/azure/azure-app-configuration/concept-feature-management)
- [LaunchDarkly](https://launchdarkly.com/)
