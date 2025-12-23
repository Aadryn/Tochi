# Feature Flags (ADR-030)

## Vue d'Ensemble

Le système de **Feature Flags** (ou Feature Toggles) permet d'activer ou désactiver des fonctionnalités de manière dynamique, sans nécessiter un redéploiement de l'application.

### Bénéfices

✅ **Déploiement Canary** : Activer progressivement une nouvelle fonctionnalité (10% → 50% → 100% des tenants)  
✅ **A/B Testing** : Comparer deux variantes d'une fonctionnalité avec des groupes utilisateurs distincts  
✅ **Rollback Instantané** : Désactiver une fonctionnalité problématique en quelques secondes  
✅ **Activation Ciblée** : Activer pour des tenants/utilisateurs spécifiques (beta testing, premium features)  
✅ **Séparation Déploiement/Activation** : Déployer le code, activer la fonctionnalité plus tard

### Architecture

```
┌─────────────────────────────────────────┐
│ IFeatureFlags (Domain Interface)        │
│ - IsEnabled(name)                       │
│ - IsEnabled(name, context)              │
│ - IsEnabledAsync(name, context, ct)     │
└─────────────────────────────────────────┘
              ↓ implémente
┌─────────────────────────────────────────┐
│ ConfigurationFeatureFlags               │
│ (Lecture depuis appsettings.json)       │
│                                         │
│ Avantages :                             │
│ - Simple, rapide                        │
│ - Aucune dépendance externe             │
│ - Configuration versionnée              │
│                                         │
│ Limitations :                           │
│ - Nécessite redéploiement               │
│ - Pas de scoping tenant/user            │
└─────────────────────────────────────────┘
```

## Configuration

### appsettings.json

```json
{
  "FeatureFlags": {
    "llm_use_optimized_provider": false,
    "llm_enable_streaming": true,
    "quota_enhanced_tracking": false,
    "ui_dark_mode": true,
    "experimental_ai_caching": false
  }
}
```

### Variables d'Environnement

Pour override runtime (sans modifier appsettings.json) :

```bash
# Windows
set FeatureFlags__llm_use_optimized_provider=true

# Linux/macOS
export FeatureFlags__llm_use_optimized_provider=true

# Docker
docker run -e FeatureFlags__llm_use_optimized_provider=true ...
```

## Utilisation

### 1. Vérification Simple (Global)

```csharp
using LLMProxy.Domain.Common;
using LLMProxy.Domain.Interfaces;

public class MyService
{
    private readonly IFeatureFlags _featureFlags;

    public MyService(IFeatureFlags featureFlags)
    {
        _featureFlags = featureFlags;
    }

    public void DoSomething()
    {
        if (_featureFlags.IsEnabled(FeatureNames.Ui_DarkMode))
        {
            // Utiliser le thème sombre
        }
        else
        {
            // Utiliser le thème clair
        }
    }
}
```

### 2. Vérification avec Contexte (Tenant-Specific)

```csharp
public async Task<Response> ProcessRequest(Guid tenantId)
{
    var context = new FeatureContext
    {
        TenantId = tenantId
    };

    if (_featureFlags.IsEnabled(FeatureNames.Quota_EnhancedTracking, context))
    {
        // Activer métriques détaillées pour ce tenant
        await TrackDetailedMetrics(tenantId);
    }
    else
    {
        // Métriques standard uniquement
        await TrackBasicMetrics(tenantId);
    }
}
```

### 3. Vérification Async (User + Tenant + Env)

```csharp
public async Task<IActionResult> ExecuteExperimentalFeature(
    Guid tenantId, 
    Guid userId,
    CancellationToken ct)
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

    var isEnabled = await _featureFlags.IsEnabledAsync(
        FeatureNames.Experimental_AiCaching,
        context,
        ct);

    if (isEnabled)
    {
        return await ExecuteWithCaching(request, ct);
    }
    else
    {
        return await ExecuteWithoutCaching(request, ct);
    }
}
```

## Feature Flags Disponibles

### LLM Providers

| Flag | Description | Use Case | Défaut |
|------|-------------|----------|--------|
| `llm_use_optimized_provider` | Provider LLM avec cache local et batching | Canary deployment nouveau provider | `false` |
| `llm_enable_streaming` | Activer streaming des réponses LLM | Améliorer UX temps réel | `true` |
| `llm_use_new_token_counter` | Nouveau algorithme de comptage tokens | Tester précision comptage | `false` |

### Quotas

| Flag | Description | Use Case | Défaut |
|------|-------------|----------|--------|
| `quota_enhanced_tracking` | Métriques détaillées par tenant/user | Offrir à clients enterprise | `false` |
| `quota_realtime_alerts` | Alertes temps réel dépassement quotas | Réduire incidents | `false` |

### API

| Flag | Description | Use Case | Défaut |
|------|-------------|----------|--------|
| `api_enable_rate_limiting_v2` | Nouveau rate limiter (sliding window) | Migration progressive | `false` |
| `api_new_authentication_flow` | Nouveau flow OAuth2/OIDC | Tester avec beta testers | `false` |

### UI

| Flag | Description | Use Case | Défaut |
|------|-------------|----------|--------|
| `ui_new_dashboard` | Nouveau dashboard redesigné | A/B testing engagement | `false` |
| `ui_dark_mode` | Thème sombre | Préférence utilisateur | `true` |

### Experimental

| Flag | Description | Use Case | Défaut |
|------|-------------|----------|--------|
| `experimental_ai_caching` | Cache sémantique des prompts | Tester cost reduction | `false` |
| `experimental_prompt_compression` | Compression intelligente prompts | Réduire tokens | `false` |

## Scénarios d'Utilisation

### Scénario 1 : Déploiement Canary

**Objectif** : Déployer le nouveau provider LLM optimisé progressivement

**Étape 1** : Déployer le code avec flag désactivé
```json
{ "llm_use_optimized_provider": false }
```

**Étape 2** : Activer pour 10% des tenants (via Redis - future implémentation)
```csharp
await SetRolloutPercentage("llm_use_optimized_provider", 10);
```

**Étape 3** : Monitorer métriques (latence, coût, erreurs)
- Si OK → Augmenter à 50% après 24h
- Si KO → Rollback instantané à 0%

**Étape 4** : Rollout complet
```json
{ "llm_use_optimized_provider": true }
```

### Scénario 2 : A/B Testing

**Objectif** : Comparer nouveau dashboard vs ancien

**Setup** :
```csharp
await SetRolloutPercentage("ui_new_dashboard", 50); // 50% utilisateurs
```

**Mesure** :
- Groupe A (ancien) : Baseline engagement
- Groupe B (nouveau) : Comparer temps session, clics, conversions

**Décision** :
- Si Groupe B > Groupe A → Activer pour tous
- Sinon → Rollback et itérer sur le design

### Scénario 3 : Feature Premium

**Objectif** : Activer enhanced tracking uniquement pour tenants Enterprise

**Code** :
```csharp
// Vérifier si tenant a plan Enterprise
var context = new FeatureContext { TenantId = tenantId };

if (_featureFlags.IsEnabled(FeatureNames.Quota_EnhancedTracking, context))
{
    // Afficher métriques détaillées
}
```

**Configuration** :
```csharp
// Pour chaque tenant Enterprise
await SetTenantFlag("quota_enhanced_tracking", enterpriseTenantId, true);
```

### Scénario 4 : Rollback d'Urgence

**Contexte** : Nouveau rate limiter V2 cause des erreurs 429 excessives

**Action** :
```bash
# Désactiver instantanément (via variable d'env ou Redis)
export FeatureFlags__api_enable_rate_limiting_v2=false

# Ou via appsettings.json
{ "api_enable_rate_limiting_v2": false }

# Redémarrer l'application
```

**Résultat** : Retour au rate limiter V1 stable en moins de 30 secondes

## Tests

### Tests Unitaires

18 tests couvrent tous les scénarios de `ConfigurationFeatureFlags` :

```bash
dotnet test tests/LLMProxy.Infrastructure.Configuration.Tests
```

**Couverture** :
- ✅ Validation arguments (null checks)
- ✅ Feature activée/désactivée
- ✅ Parsing case-insensitive (`True`, `true`, `TRUE`)
- ✅ Valeurs invalides (default à `false`)
- ✅ Context ignoré (Configuration statique)
- ✅ Async et CancellationToken

### Tests Fonctionnels

Tester via API Controller :

```bash
# 1. Démarrer le Gateway
dotnet run --project src/Presentation/LLMProxy.Gateway

# 2. Tester endpoint simple
curl http://localhost:5000/api/featureflagsexample/simple
# → {"theme":"dark","message":"Dark mode activated"}

# 3. Tester avec tenant
curl http://localhost:5000/api/featureflagsexample/tenant/00000000-0000-0000-0000-000000000001
# → {"enabled":false,"message":"Standard metrics only"}

# 4. Lister toutes les features
curl http://localhost:5000/api/featureflagsexample/status
# → {"totalFeatures":12,"enabledCount":2,"disabledCount":10,...}
```

## Bonnes Pratiques

### ✅ À Faire

1. **Nommer selon convention** : `{domaine}_{feature}` en snake_case
   ```csharp
   public const string Quota_EnhancedTracking = "quota_enhanced_tracking";
   ```

2. **Défaut à `false` (fail-safe)** : Si flag absent, désactivé par défaut
   ```csharp
   return _configuration.GetValue<bool>($"FeatureFlags:{name}", false);
   ```

3. **Documenter chaque flag** : Cas d'usage, impact, rollback
   ```csharp
   /// <summary>
   /// Active l'utilisation du provider LLM optimisé.
   /// </summary>
   /// <remarks>
   /// <strong>Cas d'usage</strong> : Déploiement canary.
   /// <strong>Impact</strong> : Performance +20%, Coût -15%.
   /// <strong>Rollback</strong> : Immédiat si problème.
   /// </remarks>
   public const string Llm_UseOptimizedProvider = "llm_use_optimized_provider";
   ```

4. **Logger les décisions** : Tracer quand un flag change le comportement
   ```csharp
   _logger.LogInformation(
       "Feature {FeatureName} is {Status} for tenant {TenantId}",
       featureName,
       isEnabled ? "enabled" : "disabled",
       tenantId);
   ```

5. **Nettoyer les flags obsolètes** : Supprimer flags après rollout complet (100%)
   - Supprimer constante dans `FeatureNames`
   - Supprimer vérifications dans le code
   - Supprimer configuration

### ❌ À Éviter

1. **Logique métier dans les flags** : Flag = on/off, pas de calculs
   ```csharp
   // ❌ MAUVAIS
   if (_featureFlags.IsEnabled("calculate_discount_v2"))
   {
       discount = price * 0.20 * (isPremium ? 1.5 : 1.0);
   }
   
   // ✅ BON
   if (_featureFlags.IsEnabled("enhanced_discount"))
   {
       discount = _discountCalculator.CalculateEnhanced(price, isPremium);
   }
   ```

2. **Flags imbriqués excessifs** : Max 1 niveau de nesting
   ```csharp
   // ❌ MAUVAIS (illisible)
   if (_featureFlags.IsEnabled("feature_a"))
   {
       if (_featureFlags.IsEnabled("feature_b"))
       {
           if (_featureFlags.IsEnabled("feature_c")) { ... }
       }
   }
   
   // ✅ BON (flag composite)
   if (_featureFlags.IsEnabled("feature_abc_combo"))
   {
       // Logique combinée
   }
   ```

3. **Flags sans documentation** : Impossible de comprendre l'impact
   ```csharp
   // ❌ MAUVAIS
   public const string Flag1 = "flag_1";
   
   // ✅ BON
   /// <summary>Active le nouveau dashboard.</summary>
   /// <remarks>
   /// <strong>Impact</strong> : UX redesigné, +30% engagement attendu.
   /// </remarks>
   public const string Ui_NewDashboard = "ui_new_dashboard";
   ```

4. **Flags permanents** : Feature flags = temporaire
   - Supprimer après rollout complet
   - Si permanent → Refactorer en configuration

## Roadmap

### Version 1 (Actuelle) : Configuration Statique

✅ Lecture depuis `appsettings.json`  
✅ Override via variables d'environnement  
✅ Interface `IFeatureFlags`  
✅ 12 feature flags initiaux  
✅ Tests unitaires (18 tests)  
✅ Exemple d'utilisation (5 patterns)  

### Version 2 (Future) : Redis Dynamique

⏳ `RedisFeatureFlags` : Flags stockés dans Redis  
⏳ Scoping par tenant : `feature_flags:{name}:tenants:{tenantId}`  
⏳ Scoping par user : `feature_flags:{name}:users:{userId}`  
⏳ Rollout percentage : `feature_flags:{name}:rollout_percentage` (0-100)  
⏳ Changements temps réel (sans redémarrage)  

### Version 3 (Future) : Cache Local

⏳ `CachedFeatureFlags` : Decorator avec cache mémoire  
⏳ TTL configurable (défaut 60s)  
⏳ Réduction appels Redis (1000/s → 17/s)  
⏳ Invalidation manuelle via événement  

## Références

- **ADR-030** : [docs/adr/030-feature-toggles.adr.md](../../../docs/adr/030-feature-toggles.adr.md)
- **Code Domain** : [src/Core/LLMProxy.Domain/Interfaces/IFeatureFlags.cs](../../../src/Core/LLMProxy.Domain/Interfaces/IFeatureFlags.cs)
- **Code Infrastructure** : [src/Infrastructure/LLMProxy.Infrastructure.Configuration/FeatureFlags/ConfigurationFeatureFlags.cs](../../../src/Infrastructure/LLMProxy.Infrastructure.Configuration/FeatureFlags/ConfigurationFeatureFlags.cs)
- **Tests** : [tests/LLMProxy.Infrastructure.Configuration.Tests/FeatureFlags/ConfigurationFeatureFlagsTests.cs](../../../tests/LLMProxy.Infrastructure.Configuration.Tests/FeatureFlags/ConfigurationFeatureFlagsTests.cs)
- **Exemple** : [src/Presentation/LLMProxy.Gateway/Controllers/FeatureFlagsExampleController.cs](../../../src/Presentation/LLMProxy.Gateway/Controllers/FeatureFlagsExampleController.cs)

## Support

Pour questions ou suggestions :
- **Documentation ADR** : Consultez ADR-030 pour contexte et décisions
- **Issues GitHub** : Ouvrir un ticket avec label `feature-flags`
- **Code Review** : Vérifier implémentation dans `Infrastructure.Configuration`
