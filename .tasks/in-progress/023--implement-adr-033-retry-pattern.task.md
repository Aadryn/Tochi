# Tâche 023 - Implémenter ADR-033 : Retry Pattern avec Exponential Backoff

**Statut** : À faire  
**Priorité** : 🔴 CRITIQUE (P0)  
**Conformité cible** : ADR-033 de 0% → 95%  
**Dépendances** : Task 022 (Circuit Breaker) ✅ complétée

## CONTEXTE

**Analyse ADR-033** : `docs/ANALYSE_CONFORMITE_ADR-031-041.md` (lignes 402-510)

**Conformité actuelle** : **0%** (aucune politique retry configurée)

**Problème identifié** :
- Aucune gestion automatique des échecs transitoires (erreurs réseau, timeouts)
- Appels LLM échouent immédiatement sans retry
- Utilisateurs voient erreurs 5xx pour pannes temporaires
- Pas d'exponential backoff (risque thundering herd)
- Circuit Breaker seul ne suffit pas (erreurs transitoires !== pannes prolongées)

**Risques sans retry** :
- 🔴 **CRITIQUE** : Échecs transitoires perçus comme pannes définitives
- 🔴 Expérience utilisateur dégradée (erreurs évitables)
- 🔴 Surcharge providers LLM lors retry synchrone sans backoff
- 🟡 Logs pollués par erreurs transitoires

## OBJECTIF

Implémenter le pattern Retry avec Exponential Backoff sur les HttpClients LLM pour gérer automatiquement les échecs transitoires.

**Spécifications ADR-033** :
- Retry avec exponential backoff (1s, 2s, 4s...)
- Jitter aléatoire pour éviter thundering herd
- MaxRetryAttempts configurable (défaut: 3)
- Retry uniquement sur erreurs transitoires :
  - 429 Too Many Requests
  - 503 Service Unavailable
  - 408 Request Timeout
  - `HttpRequestException` (erreurs réseau)
  - `TaskCanceledException` (timeouts)
- Ne PAS retry sur erreurs client (4xx sauf 408, 429)
- Logs structurés des tentatives (EventIds 5010-5012)

## CRITÈRES DE SUCCÈS

### Fonctionnels
- [ ] Retry configuré sur les 3 providers (OpenAI, Anthropic, Ollama)
- [ ] Exponential backoff : 1s → 2s → 4s (max 3 tentatives)
- [ ] Jitter activé (UseJitter = true)
- [ ] Retry uniquement sur codes HTTP transitoires (429, 503, 408)
- [ ] Retry sur exceptions réseau (`HttpRequestException`, `TaskCanceledException`)
- [ ] Pas de retry sur 4xx (sauf 408, 429) ou 2xx

### Techniques
- [ ] Utiliser `config.Retry` de `AddStandardResilienceHandler` (Polly v8)
- [ ] Configuration centralisée dans `appsettings.json` (section `RetryPolicy`)
- [ ] Logs structurés avec LoggerMessage :
  - EventId 5010 : OnRetry (Information)
  - EventId 5011 : RetryExhausted (Warning)
  - EventId 5012 : RetrySkipped (Debug)
- [ ] Métriques retry exposées (nombre tentatives, durée totale)

### Qualité
- [ ] **Build** : 0 erreurs, 0 warnings
- [ ] **Tests** : Créer 5+ tests unitaires
  - Retry réussi après 2 tentatives
  - Retry échoue après 3 tentatives (max)
  - Backoff exponentiel vérifié (1s, 2s, 4s)
  - Jitter ajoute variation aléatoire
  - Pas de retry sur 400 Bad Request
  - Pas de retry sur 200 OK
- [ ] Tests existants : 100% passing (non-régression)
- [ ] Documentation README.md mise à jour

## ÉTAPES D'IMPLÉMENTATION

### 1. Créer configuration Retry (30 min)

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Configuration/RetryPolicyOptions.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace LLMProxy.Infrastructure.LLMProviders.Configuration;

/// <summary>
/// Options de configuration pour la politique de retry avec exponential backoff.
/// Conforme à ADR-033 (Retry Pattern & Exponential Backoff).
/// </summary>
public sealed class RetryPolicyOptions
{
    /// <summary>
    /// Nombre maximum de tentatives (1 appel initial + N retries).
    /// Défaut : 3 (soit 1 appel + 2 retries).
    /// </summary>
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Délai initial avant le premier retry.
    /// Défaut : 1 seconde.
    /// </summary>
    [Required]
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Activer le jitter (variation aléatoire) pour éviter thundering herd.
    /// Défaut : true.
    /// </summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>
    /// Type de backoff (Exponential recommandé pour ADR-033).
    /// Défaut : Exponential (1s, 2s, 4s, 8s...).
    /// </summary>
    [Required]
    public string BackoffType { get; set; } = "Exponential";
}
```

**Action** : Créer le fichier avec validation DataAnnotations.

---

### 2. Enrichir HttpClientCircuitBreakerExtensions (1h)

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Resilience/HttpClientCircuitBreakerExtensions.cs`

**Modification** : Renommer en `HttpClientResilienceExtensions.cs` et ajouter configuration retry.

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;

namespace LLMProxy.Infrastructure.LLMProviders.Resilience;

/// <summary>
/// Extensions pour configurer les politiques de résilience (Circuit Breaker + Retry) avec Polly.
/// Conforme à ADR-032 (Circuit Breaker) et ADR-033 (Retry Pattern).
/// </summary>
public static partial class HttpClientResilienceExtensions
{
    /// <summary>
    /// Ajoute les politiques de résilience (Circuit Breaker + Retry + Timeout) à un HttpClient.
    /// </summary>
    /// <param name="builder">Builder HttpClient à configurer.</param>
    /// <param name="providerName">Nom du provider LLM (pour logs et isolation).</param>
    /// <param name="circuitBreakerOptions">Options circuit breaker.</param>
    /// <param name="retryOptions">Options retry policy.</param>
    /// <param name="logger">Logger pour traçabilité.</param>
    public static void AddResiliencePolicies(
        this IHttpClientBuilder builder,
        string providerName,
        Configuration.CircuitBreakerOptions circuitBreakerOptions,
        Configuration.RetryPolicyOptions retryOptions,
        ILogger logger)
    {
        builder.AddStandardResilienceHandler(config =>
        {
            // ═══ CIRCUIT BREAKER ═══
            config.CircuitBreaker.FailureRatio = circuitBreakerOptions.FailureThreshold;
            config.CircuitBreaker.MinimumThroughput = circuitBreakerOptions.MinimumThroughput;
            config.CircuitBreaker.SamplingDuration = circuitBreakerOptions.SamplingDuration;
            config.CircuitBreaker.BreakDuration = circuitBreakerOptions.DurationOfBreak;

            config.CircuitBreaker.OnOpened = args =>
            {
                LogCircuitBreakerOpened(logger, providerName, circuitBreakerOptions.DurationOfBreak.TotalSeconds);
                return ValueTask.CompletedTask;
            };

            config.CircuitBreaker.OnClosed = args =>
            {
                LogCircuitBreakerClosed(logger, providerName);
                return ValueTask.CompletedTask;
            };

            config.CircuitBreaker.OnHalfOpened = args =>
            {
                LogCircuitBreakerHalfOpened(logger, providerName);
                return ValueTask.CompletedTask;
            };

            // ═══ RETRY WITH EXPONENTIAL BACKOFF ═══
            config.Retry.MaxRetryAttempts = retryOptions.MaxRetryAttempts;
            config.Retry.Delay = retryOptions.InitialDelay;
            config.Retry.BackoffType = retryOptions.BackoffType == "Exponential" 
                ? Polly.DelayBackoffType.Exponential 
                : Polly.DelayBackoffType.Constant;
            config.Retry.UseJitter = retryOptions.UseJitter;

            // Retry uniquement sur erreurs transitoires
            config.Retry.ShouldHandle = new HttpClientResiliencePredicates()
                .HandleTransientHttpErrors() // 408, 429, 5xx, HttpRequestException, TaskCanceledException
                .Build();

            config.Retry.OnRetry = args =>
            {
                LogRetryAttempt(
                    logger,
                    providerName,
                    args.AttemptNumber,
                    retryOptions.MaxRetryAttempts,
                    args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString() ?? "Unknown");
                return ValueTask.CompletedTask;
            };

            // ═══ TIMEOUT ═══
            // Déjà configuré via HttpClient.Timeout dans ServiceCollectionExtensions
        });
    }

    // ═══ LOGGING CIRCUIT BREAKER ═══
    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Warning,
        Message = "Circuit breaker OPENED for provider {ProviderName} - Blocking requests for {DurationSeconds}s")]
    private static partial void LogCircuitBreakerOpened(ILogger logger, string providerName, double durationSeconds);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Information,
        Message = "Circuit breaker CLOSED for provider {ProviderName} - Resuming normal operation")]
    private static partial void LogCircuitBreakerClosed(ILogger logger, string providerName);

    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Information,
        Message = "Circuit breaker HALF-OPENED for provider {ProviderName} - Testing recovery with limited requests")]
    private static partial void LogCircuitBreakerHalfOpened(ILogger logger, string providerName);

    // ═══ LOGGING RETRY ═══
    [LoggerMessage(
        EventId = 5010,
        Level = LogLevel.Information,
        Message = "Retry attempt {AttemptNumber}/{MaxAttempts} for provider {ProviderName} - Reason: {FailureReason}")]
    private static partial void LogRetryAttempt(
        ILogger logger,
        string providerName,
        int attemptNumber,
        int maxAttempts,
        string failureReason);
}
```

**Action** :
1. Renommer `HttpClientCircuitBreakerExtensions.cs` → `HttpClientResilienceExtensions.cs`
2. Renommer méthode `AddCircuitBreakerPolicy` → `AddResiliencePolicies`
3. Ajouter paramètre `RetryPolicyOptions`
4. Configurer `config.Retry` avec backoff exponentiel
5. Ajouter logging retry (EventId 5010)

---

### 3. Mettre à jour ServiceCollectionExtensions (30 min)

**Fichier** : `src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/ServiceCollectionExtensions.cs`

**Modification** : Bind `RetryPolicy` depuis configuration et passer aux HttpClients.

```csharp
public static IServiceCollection AddLLMProviderInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Bind Circuit Breaker Options
    var circuitBreakerOptions = configuration.GetSection("CircuitBreaker").Get<CircuitBreakerOptions>() 
        ?? new CircuitBreakerOptions();
    services.AddSingleton(circuitBreakerOptions);

    // Bind Retry Policy Options
    var retryOptions = configuration.GetSection("RetryPolicy").Get<RetryPolicyOptions>() 
        ?? new RetryPolicyOptions();
    services.AddSingleton(retryOptions);

    // Register token counter
    services.AddSingleton<ITokenCounterService, SharpTokenCounterService>();

    // Configure HttpClients avec résilience
    ConfigureHttpClientsWithResilience(services, circuitBreakerOptions, retryOptions);

    return services;
}

private static void ConfigureHttpClientsWithResilience(
    IServiceCollection services,
    CircuitBreakerOptions circuitBreakerOptions,
    RetryPolicyOptions retryOptions)
{
    var logger = services.BuildServiceProvider().GetRequiredService<ILogger<object>>();

    // OpenAI
    services.AddHttpClient("OpenAI", client =>
    {
        client.BaseAddress = new Uri("https://api.openai.com/");
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddResiliencePolicies("OpenAI", circuitBreakerOptions, retryOptions, logger);

    // Anthropic
    services.AddHttpClient("Anthropic", client =>
    {
        client.BaseAddress = new Uri("https://api.anthropic.com/");
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddResiliencePolicies("Anthropic", circuitBreakerOptions, retryOptions, logger);

    // Ollama
    services.AddHttpClient("Ollama", client =>
    {
        client.BaseAddress = new Uri("http://localhost:11434/");
        client.Timeout = TimeSpan.FromSeconds(60);
    })
    .AddResiliencePolicies("Ollama", circuitBreakerOptions, retryOptions, logger);
}
```

**Action** :
1. Bind `RetryPolicy` depuis configuration
2. Passer `retryOptions` à `AddResiliencePolicies`
3. Mettre à jour les 3 HttpClients

---

### 4. Ajouter configuration appsettings.json (10 min)

**Fichier** : `src/Presentation/LLMProxy.Gateway/appsettings.json`

**Ajout** :

```json
"RetryPolicy": {
  "MaxRetryAttempts": 3,
  "InitialDelay": "00:00:01",
  "UseJitter": true,
  "BackoffType": "Exponential"
}
```

**Position** : Après section `CircuitBreaker`.

**Action** : Ajouter la section entre `CircuitBreaker` et `RateLimiting`.

---

### 5. Créer tests unitaires (2h)

**Fichier** : `tests/LLMProxy.Gateway.Tests/Resilience/RetryPolicyTests.cs`

```csharp
using System.Net;
using Microsoft.Extensions.Http.Resilience;
using NFluent;
using Xunit;

namespace LLMProxy.Gateway.Tests.Resilience;

/// <summary>
/// Tests pour le Retry Pattern avec Exponential Backoff.
/// Conforme à ADR-033 (Retry Pattern & Exponential Backoff).
/// </summary>
public sealed class RetryPolicyTests
{
    [Fact]
    public void RetryPolicyOptions_Should_HaveCorrectDefaults()
    {
        // Arrange & Act
        var options = new RetryPolicyOptions();

        // Assert
        Check.That(options.MaxRetryAttempts).IsEqualTo(3);
        Check.That(options.InitialDelay).IsEqualTo(TimeSpan.FromSeconds(1));
        Check.That(options.UseJitter).IsTrue();
        Check.That(options.BackoffType).IsEqualTo("Exponential");
    }

    [Fact]
    public async Task Retry_Should_SucceedAfter2Attempts()
    {
        // Test : Simule 2 échecs puis succès
        // Vérifier : 3 appels totaux (1 initial + 2 retries)
    }

    [Fact]
    public async Task Retry_Should_ExhaustAfter3Attempts()
    {
        // Test : Simule échecs constants
        // Vérifier : 4 appels totaux (1 initial + 3 retries) puis échec final
    }

    [Fact]
    public async Task Retry_Should_UseExponentialBackoff()
    {
        // Test : Mesurer délais entre tentatives
        // Vérifier : ~1s, ~2s, ~4s (avec tolérance jitter)
    }

    [Fact]
    public async Task Retry_Should_AddJitterVariation()
    {
        // Test : Exécuter 10 retries identiques
        // Vérifier : Délais varient légèrement (jitter actif)
    }

    [Fact]
    public async Task Retry_ShouldNot_RetryOn400BadRequest()
    {
        // Test : Retourne 400
        // Vérifier : 1 seul appel (pas de retry sur erreur client)
    }

    [Fact]
    public async Task Retry_Should_RetryOn503ServiceUnavailable()
    {
        // Test : Retourne 503
        // Vérifier : Retry actif (erreur transitoire)
    }
}
```

**Action** : Créer le fichier avec 6+ tests couvrant tous les scénarios.

---

### 6. Mettre à jour README.md (30 min)

**Fichier** : `README.md`

**Ajout** : Section "Retry Pattern" après "Circuit Breaker".

```markdown
### Retry Pattern avec Exponential Backoff (ADR-033)

Le proxy implémente une politique de retry automatique pour gérer les échecs transitoires (erreurs réseau, timeouts temporaires).

**Configuration** (`appsettings.json`) :

```json
"RetryPolicy": {
  "MaxRetryAttempts": 3,
  "InitialDelay": "00:00:01",
  "UseJitter": true,
  "BackoffType": "Exponential"
}
```

**Comportement** :
- **Tentatives** : 1 appel initial + 3 retries max
- **Backoff** : Exponentiel (1s, 2s, 4s, 8s...)
- **Jitter** : Variation aléatoire pour éviter thundering herd
- **Retry sur** : 429, 503, 408, `HttpRequestException`, `TaskCanceledException`
- **Pas de retry** : 2xx, 4xx (sauf 408, 429)

**Logs** :
- `[5010]` Retry attempt X/Y for provider {name} (Information)
```

**Action** : Documenter configuration et comportement.

---

### 7. Build, test et validation (1h)

**Commandes** :

```powershell
# Build
dotnet build --no-restore

# Tests
dotnet test --no-build --no-restore

# Validation : Vérifier sortie
# - 0 errors, 0 warnings
# - Tous tests passing (anciens + 6 nouveaux)
```

**Action** :
1. Compiler sans erreurs ni warnings
2. Exécuter tests (100% passing)
3. Vérifier logs structurés (EventIds 5010-5012)

---

### 8. Commit et merge (30 min)

**Commit atomique** :

```powershell
git add -A
git commit -m "feat(resilience): Add retry pattern with exponential backoff

- Created RetryPolicyOptions.cs configuration class
- Renamed HttpClientCircuitBreakerExtensions -> HttpClientResilienceExtensions
- Added retry configuration with exponential backoff and jitter
- Updated ServiceCollectionExtensions to bind RetryPolicy
- Added appsettings.json RetryPolicy section
- Created 6 unit tests for retry scenarios
- Updated README.md with retry documentation

ADR-033 conformity: 0% -> 95%
Build: 0 errors, 0 warnings
Tests: 78+ passed (72 existing + 6 new retry tests)"
```

**Merge** :

```powershell
git checkout main
git merge --no-ff feature/023--implement-adr-033-retry-pattern -m "Merge feature/023 - Implement ADR-033 Retry Pattern"
git branch -d feature/023--implement-adr-033-retry-pattern
```

**Action** : Commit, merge, supprimer feature branch.

---

## RÉFÉRENCE ADR

**ADR-033** : `docs/adr/033-retry-pattern-backoff.adr.md`

**Principes clés** :
1. Retry uniquement sur erreurs **transitoires** (temporaires, récupérables)
2. Exponential backoff pour éviter surcharge (1s, 2s, 4s, 8s...)
3. Jitter aléatoire pour éviter thundering herd (tous clients retry en même temps)
4. MaxRetryAttempts limité (3-5 max) pour éviter boucles infinies
5. Pas de retry sur erreurs **permanentes** (400 Bad Request, 401 Unauthorized)

**Erreurs transitoires** :
- 429 Too Many Requests (rate limit dépassé temporairement)
- 503 Service Unavailable (maintenance ou surcharge temporaire)
- 408 Request Timeout (timeout réseau)
- `HttpRequestException` (erreur réseau, DNS, connexion)
- `TaskCanceledException` (timeout HttpClient)

**Erreurs permanentes (PAS de retry)** :
- 400 Bad Request (requête invalide)
- 401 Unauthorized (authentification échouée)
- 403 Forbidden (pas de permission)
- 404 Not Found (ressource inexistante)
- 2xx Success (requête réussie)

---

## DURÉE ESTIMÉE

**Total** : 6h  
- Configuration options : 30 min
- Extension retry : 1h
- ServiceCollectionExtensions : 30 min
- appsettings.json : 10 min
- Tests unitaires : 2h
- README.md : 30 min
- Build/test/validation : 1h
- Commit/merge : 30 min

---

## NOTES

**Synergie avec ADR-032** :
- Circuit Breaker = protection contre pannes **prolongées** (provider down pendant minutes/heures)
- Retry = gestion échecs **transitoires** (timeout ponctuel, surcharge temporaire)
- Complémentaires : Circuit Breaker stoppe appels si trop d'échecs, Retry gère récupération rapide

**Anti-pattern à éviter** :
- Retry sur erreurs permanentes (400, 401) → Gaspille ressources
- Retry sans backoff → Thundering herd (tous clients retry simultanément)
- MaxRetryAttempts trop élevé (>5) → Latence utilisateur inacceptable
- Retry sans jitter → Patterns d'accès synchronisés (pic charge)

**Métriques à exposer** :
- Nombre total de retries par provider
- Durée cumulée des retries
- Taux de succès après retry (X tentatives pour succès)
- Distribution des délais (P50, P95, P99)


## TRACKING
Début: 2025-12-21T23:53:41.3254029Z

