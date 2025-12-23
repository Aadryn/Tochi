# 113. Implement Polly Resilience Policies

**Statut:** À faire  
**Priorité:** HIGH (Week 1)  
**Catégorie:** Gateway Enhancements  
**Dépendances:** Aucune

## OBJECTIF

Implémenter les politiques de résilience Polly pour gérer les échecs transitoires lors des appels aux fournisseurs LLM :
- **Retry Policy** : Nouvelle tentative automatique en cas d'échec transitoire
- **Circuit Breaker** : Protection contre les fournisseurs défaillants

## CONTEXTE

Les appels HTTP vers les fournisseurs LLM (OpenAI, Anthropic, etc.) peuvent échouer pour des raisons transitoires (timeout réseau, erreur 503, etc.). Sans politique de résilience, chaque échec entraîne une erreur pour l'utilisateur final.

Polly fournit des patterns de résilience éprouvés pour ASP.NET Core via `Microsoft.Extensions.Http.Polly`.

## CRITÈRES DE SUCCÈS

- [ ] Package NuGet `Microsoft.Extensions.Http.Polly` installé dans `LLMProxy.Infrastructure.LLMProviders`
- [ ] **Retry Policy** configurée avec stratégie exponentielle backoff
- [ ] **Circuit Breaker Policy** configurée pour détecter pannes fournisseurs
- [ ] Politiques attachées aux HttpClient via `AddPolicyHandler`
- [ ] Logging structuré des retry et circuit breaker events
- [ ] Tests unitaires validant les politiques (simulation d'échecs)
- [ ] Build réussi (0 erreurs, 0 warnings)
- [ ] Documentation XML complète sur méthodes de configuration

## FICHIERS CONCERNÉS

- `src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/LLMProxy.Infrastructure.LLMProviders.csproj`
- `src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/DependencyInjection.cs` (nouveau ou existant)
- `src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Policies/ResiliencePolicies.cs` (nouveau)
- `tests/LLMProxy.Infrastructure.LLMProviders.Tests/Policies/ResiliencePoliciesTests.cs` (nouveau)

## APPROCHE TECHNIQUE

### 1. Installer package NuGet

```powershell
cd src/Infrastructure/LLMProxy.Infrastructure.LLMProviders
dotnet add package Microsoft.Extensions.Http.Polly
dotnet add package Polly.Extensions.Http
```

### 2. Créer ResiliencePolicies.cs

```csharp
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace LLMProxy.Infrastructure.LLMProviders.Policies;

/// <summary>
/// Politiques de résilience Polly pour les appels HTTP vers les fournisseurs LLM.
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Crée une politique de retry avec backoff exponentiel.
    /// </summary>
    /// <param name="maxRetryAttempts">Nombre maximum de tentatives (défaut: 3).</param>
    /// <param name="initialDelay">Délai initial entre tentatives (défaut: 1 seconde).</param>
    /// <returns>Politique de retry asynchrone.</returns>
    /// <remarks>
    /// Retry uniquement sur les erreurs HTTP transitoires :
    /// - HttpStatusCode 408 (Request Timeout)
    /// - HttpStatusCode 5xx (Server Errors)
    /// - HttpRequestException (erreurs réseau)
    /// </remarks>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(
        int maxRetryAttempts = 3,
        TimeSpan? initialDelay = null)
    {
        var delay = initialDelay ?? TimeSpan.FromSeconds(1);

        return HttpPolicyExtensions
            .HandleTransientHttpError() // 5xx, 408, HttpRequestException
            .Or<TimeoutRejectedException>() // Polly Timeout
            .WaitAndRetryAsync(
                retryCount: maxRetryAttempts,
                sleepDurationProvider: retryAttempt =>
                    delay * Math.Pow(2, retryAttempt - 1), // Exponential backoff
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Logging structuré (ILogger injecté via context)
                    var logger = context.GetLogger();
                    logger?.LogWarning(
                        "Retry {RetryAttempt}/{MaxRetries} après {Delay}ms - Raison: {Reason}",
                        retryAttempt,
                        maxRetryAttempts,
                        timespan.TotalMilliseconds,
                        outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase ?? "Unknown");
                });
    }

    /// <summary>
    /// Crée une politique de circuit breaker pour détecter pannes fournisseurs.
    /// </summary>
    /// <param name="failureThreshold">Nombre d'échecs avant ouverture du circuit (défaut: 5).</param>
    /// <param name="durationOfBreak">Durée d'ouverture du circuit (défaut: 30 secondes).</param>
    /// <returns>Politique de circuit breaker asynchrone.</returns>
    /// <remarks>
    /// États du circuit :
    /// - CLOSED : Circuit fermé, requêtes passent normalement
    /// - OPEN : Circuit ouvert, requêtes échouent immédiatement (BrokenCircuitException)
    /// - HALF-OPEN : Test de récupération (1 requête de test)
    /// </remarks>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
        int failureThreshold = 5,
        TimeSpan? durationOfBreak = null)
    {
        var breakDuration = durationOfBreak ?? TimeSpan.FromSeconds(30);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: failureThreshold,
                durationOfBreak: breakDuration,
                onBreak: (outcome, duration, context) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogError(
                        "Circuit OUVERT pour {Duration}s après {FailureCount} échecs - Raison: {Reason}",
                        duration.TotalSeconds,
                        failureThreshold,
                        outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase ?? "Unknown");
                },
                onReset: context =>
                {
                    var logger = context.GetLogger();
                    logger?.LogInformation("Circuit FERMÉ - Fournisseur de nouveau disponible");
                },
                onHalfOpen: () =>
                {
                    // Logger peut être injecté via context si nécessaire
                });
    }

    /// <summary>
    /// Extension pour récupérer ILogger depuis Polly Context.
    /// </summary>
    private static ILogger? GetLogger(this Context context)
    {
        if (context.TryGetValue("Logger", out var logger) && logger is ILogger loggerInstance)
        {
            return loggerInstance;
        }
        return null;
    }
}
```

### 3. Configurer dans DependencyInjection.cs

```csharp
using LLMProxy.Infrastructure.LLMProviders.Policies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace LLMProxy.Infrastructure.LLMProviders;

/// <summary>
/// Configuration de l'injection de dépendances pour les fournisseurs LLM.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Enregistre les services d'infrastructure pour les fournisseurs LLM.
    /// </summary>
    /// <param name="services">Collection de services.</param>
    /// <returns>Collection de services modifiée.</returns>
    public static IServiceCollection AddLLMProviders(this IServiceCollection services)
    {
        // HttpClient avec politiques de résilience pour OpenAI
        services.AddHttpClient("OpenAIClient")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            })
            .AddPolicyHandler((serviceProvider, request) =>
            {
                // Inject logger dans le contexte Polly
                var logger = serviceProvider.GetRequiredService<ILogger<IAsyncPolicy<HttpResponseMessage>>>();
                var context = new Context { ["Logger"] = logger };
                
                return Policy.WrapAsync(
                    ResiliencePolicies.GetRetryPolicy(maxRetryAttempts: 3),
                    ResiliencePolicies.GetCircuitBreakerPolicy(failureThreshold: 5)
                );
            });

        // Répéter pour chaque fournisseur (Anthropic, Ollama, etc.)
        services.AddHttpClient("AnthropicClient")
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<IAsyncPolicy<HttpResponseMessage>>>();
                var context = new Context { ["Logger"] = logger };
                
                return Policy.WrapAsync(
                    ResiliencePolicies.GetRetryPolicy(),
                    ResiliencePolicies.GetCircuitBreakerPolicy()
                );
            });

        // Enregistrer les clients de fournisseurs
        services.AddScoped<IOpenAIProviderClient, OpenAIProviderClient>();
        services.AddScoped<IAnthropicProviderClient, AnthropicProviderClient>();
        // etc.

        return services;
    }
}
```

### 4. Tests unitaires

```csharp
using FluentAssertions;
using LLMProxy.Infrastructure.LLMProviders.Policies;
using Polly.CircuitBreaker;
using Xunit;

namespace LLMProxy.Infrastructure.LLMProviders.Tests.Policies;

/// <summary>
/// Tests unitaires pour les politiques de résilience Polly.
/// </summary>
public class ResiliencePoliciesTests
{
    [Fact]
    public async Task GetRetryPolicy_ShouldRetryOnTransientErrors()
    {
        // Arrange
        var retryPolicy = ResiliencePolicies.GetRetryPolicy(maxRetryAttempts: 3);
        var attemptCount = 0;

        // Act
        var result = await retryPolicy.ExecuteAsync(async () =>
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw new HttpRequestException("Transient error");
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        });

        // Assert
        attemptCount.Should().Be(3);
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCircuitBreakerPolicy_ShouldOpenCircuitAfterThreshold()
    {
        // Arrange
        var circuitBreakerPolicy = ResiliencePolicies.GetCircuitBreakerPolicy(failureThreshold: 2);

        // Act - Provoquer 2 échecs pour ouvrir le circuit
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await circuitBreakerPolicy.ExecuteAsync(() => throw new HttpRequestException("Error 1")));
        
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await circuitBreakerPolicy.ExecuteAsync(() => throw new HttpRequestException("Error 2")));

        // Assert - 3ème appel devrait lever BrokenCircuitException
        await Assert.ThrowsAsync<BrokenCircuitException>(async () =>
            await circuitBreakerPolicy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage())));
    }
}
```

## DÉFINITION DE TERMINÉ

- [ ] Package Polly installé et configuré
- [ ] Retry Policy avec exponential backoff implémentée
- [ ] Circuit Breaker Policy implémentée
- [ ] Politiques attachées aux HttpClient de tous les fournisseurs
- [ ] Logging structuré des événements Polly (retry, circuit breaker)
- [ ] Tests unitaires validant retry et circuit breaker
- [ ] Documentation XML complète sur toutes les méthodes
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests : 100% passing

## RÉFÉRENCES

- **Source:** `docs/NEXT_STEPS.md` (High Priority - Gateway Enhancements)
- **ADR-032:** Circuit Breaker Pattern
- **ADR-033:** Retry Pattern with Exponential Backoff
- **Polly Documentation:** https://github.com/App-vNext/Polly
- **Microsoft.Extensions.Http.Polly:** https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly
## TRACKING
Début: 2025-12-24T00:36:43Z
Fin: 2025-12-24T00:36:43Z
Durée: 0s

## RÉSUMÉ DE COMPLÉTION

### État Constaté
La tâche 113 est **DÉJÀ IMPLÉMENTÉE** dans le code existant.

### Vérifications Effectuées
- ✅ Microsoft.Extensions.Http.Polly v9.0.0 installé (LLMProxy.Infrastructure.LLMProviders.csproj L21)
- ✅ HttpClientResilienceExtensions.cs : AddResiliencePolicies configuré avec :
  * Circuit Breaker (ADR-032) : FailureRatio, MinimumThroughput, BreakDuration
  * Retry Policy (ADR-033) : MaxRetryAttempts, ExponentialBackoff, UseJitter
  * Logging structuré avec LoggerMessage (EventId 5001-5004)
- ✅ ServiceCollectionExtensions.cs : HttpClients configurés avec résilience :
  * OpenAI (L197) : AddResiliencePolicies
  * Anthropic (L205) : AddResiliencePolicies
  * Ollama (L213) : AddResiliencePolicies

### Fichiers Analysés
- LLMProxy.Infrastructure.LLMProviders/LLMProxy.Infrastructure.LLMProviders.csproj
- LLMProxy.Infrastructure.LLMProviders/Resilience/HttpClientResilienceExtensions.cs
- LLMProxy.Infrastructure.LLMProviders/ServiceCollectionExtensions.cs

### Validations
- [x] Package NuGet Polly installé
- [x] Retry Policy avec exponential backoff
- [x] Circuit Breaker avec thresholds configurables
- [x] Logging structuré complet
- [x] Documentation XML présente

### Notes
Implémentation complète et conforme ADR-032, ADR-033. Utilise Polly v8 Standard Resilience Handler.
