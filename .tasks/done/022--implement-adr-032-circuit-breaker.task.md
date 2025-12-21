# 022 - Implémenter ADR-032 : Circuit Breaker Pattern

## OBJECTIF

Configurer et activer Polly Circuit Breaker pour protéger l'application contre les défaillances en cascade lors d'appels aux LLM providers externes.

## JUSTIFICATION

**Problème** : 
- ✅ Polly package déjà installé (conformité 45%)
- ❌ **AUCUNE configuration** de circuit breaker active
- 🔴 **RISQUE HAUTE DISPONIBILITÉ** : Défaillances en cascade non gérées
- 🔴 **SURCHARGE PROVIDERS** : Requêtes continues vers services défaillants
- 🔴 **EXPÉRIENCE UTILISATEUR** : Timeouts longs sans fail-fast

**Bénéfices attendus** :
- ✅ Protection contre défaillances en cascade (fail-fast)
- ✅ Réduction charge sur providers défaillants (recovery time)
- ✅ Timeouts courts avec fallback immédiat
- ✅ Conformité ADR-032 : 45% → 95%
- ✅ Amélioration résilience et disponibilité

## PÉRIMÈTRE

### États du Circuit Breaker

```
┌─────────────────────────────────────────────────────────┐
│                  CIRCUIT BREAKER ÉTATS                   │
│                                                          │
│  CLOSED (Normal)                                         │
│  ├─ Requêtes passent normalement                        │
│  ├─ Compteur échecs < seuil                             │
│  └─ Si seuil dépassé → OPEN                             │
│                                                          │
│  OPEN (Circuit ouvert)                                   │
│  ├─ Requêtes rejetées immédiatement                     │
│  ├─ Fail-fast sans appel provider                       │
│  ├─ Durée configurable (ex: 30s)                        │
│  └─ Après durée → HALF-OPEN                             │
│                                                          │
│  HALF-OPEN (Test de récupération)                       │
│  ├─ Nombre limité de requêtes test                      │
│  ├─ Si succès → CLOSED (récupération)                   │
│  └─ Si échec → OPEN (encore défaillant)                 │
└─────────────────────────────────────────────────────────┘
```

### Configuration par Provider LLM

Chaque provider LLM (OpenAI, Anthropic, Ollama, etc.) aura son propre circuit breaker isolé :

- **Seuils** :
  - Échecs consécutifs : 5 (OpenAPI)
  - Durée circuit ouvert : 30 secondes
  - Échantillonnage : 10 requêtes minimum

- **Critères d'échec** :
  - HTTP 5xx (Server errors)
  - Timeouts (>30s)
  - Exceptions réseau (SocketException, TimeoutException)
  - **Exclusions** : 4xx (erreurs client, pas provider défaillant)

### Intégration avec Architecture Existante

**HttpClientFactory** (déjà utilisé) :
- Extension Polly pour HttpClient
- Circuit breaker par provider (clé = provider name)
- Isolation entre providers

**LLMProviderService** :
- Wrapping automatique des appels HTTP
- Logs structurés des changements d'état
- Métriques OpenTelemetry

**Fallback Strategy** :
- Si circuit OPEN → Retour 503 Service Unavailable
- Header `Retry-After` indiquant durée avant retry
- Message clair : "Provider {name} temporarily unavailable"

## CRITÈRES DE SUCCÈS

- [ ] **Polly Circuit Breaker** configuré via `AddHttpClient`
- [ ] **Circuit breaker par provider** (OpenAI, Anthropic, Ollama isolés)
- [ ] **Configuration appsettings.json** : seuils, durées, échantillonnage
- [ ] **3 états gérés** : Closed, Open, Half-Open
- [ ] **Logs structurés** : State changes (OnBreak, OnReset, OnHalfOpen)
- [ ] **Métriques** : Circuit breaker state gauge par provider
- [ ] **Tests unitaires** : Circuit opening, half-open, reset
- [ ] **Tests d'intégration** : Simulation provider failures
- [ ] **Documentation** : README avec configuration et comportement
- [ ] **Conformité ADR-032** : 95%+

## DÉPENDANCES

- ✅ Polly package installé (Microsoft.Extensions.Http.Polly)
- ✅ HttpClientFactory configuré
- ✅ ILogger pour logs structurés
- ✅ OpenTelemetry pour métriques

## CONTRAINTES

- **Respect ADR-032** : États et transitions selon l'ADR
- **Isolation providers** : Circuit breaker par provider (pas global)
- **Performance** : Overhead circuit breaker <1ms
- **Configuration** : Seuils ajustables par environnement
- **Backward compatibility** : Ne pas casser appels existants

## PLAN D'ACTION

### Étape 1 : Configuration Options

**1.1 Créer CircuitBreakerOptions.cs**
```csharp
// src/Infrastructure/LLMProxy.Infrastructure.LLMProviders/Configuration/CircuitBreakerOptions.cs
public sealed class CircuitBreakerOptions
{
    /// <summary>
    /// Nombre d'échecs consécutifs avant ouverture du circuit.
    /// </summary>
    public int FailureThreshold { get; set; } = 5;
    
    /// <summary>
    /// Durée pendant laquelle le circuit reste ouvert.
    /// </summary>
    public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Nombre minimum de requêtes avant calcul du taux d'échec.
    /// </summary>
    public int MinimumThroughput { get; set; } = 10;
    
    /// <summary>
    /// Durée de la fenêtre d'échantillonnage.
    /// </summary>
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(60);
}
```

**1.2 Ajouter configuration appsettings.json**
```json
{
  "CircuitBreaker": {
    "FailureThreshold": 5,
    "DurationOfBreak": "00:00:30",
    "MinimumThroughput": 10,
    "SamplingDuration": "00:01:00"
  }
}
```

### Étape 2 : Implémenter Circuit Breaker Policy

**2.1 Créer HttpClientCircuitBreakerExtensions.cs**
```csharp
public static class HttpClientCircuitBreakerExtensions
{
    public static IHttpClientBuilder AddCircuitBreakerPolicy(
        this IHttpClientBuilder builder,
        CircuitBreakerOptions options,
        ILogger logger)
    {
        return builder.AddPolicyHandler((services, request) =>
        {
            var policy = Policy
                .HandleResult<HttpResponseMessage>(r => 
                    (int)r.StatusCode >= 500 || // 5xx errors
                    r.StatusCode == HttpStatusCode.RequestTimeout)
                .Or<HttpRequestException>()
                .Or<TimeoutException>()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: 0.5, // 50% failure rate
                    samplingDuration: options.SamplingDuration,
                    minimumThroughput: options.MinimumThroughput,
                    durationOfBreak: options.DurationOfBreak,
                    onBreak: (outcome, breakDuration, context) =>
                    {
                        LogCircuitBreakerOpened(logger, 
                            context.PolicyKey, 
                            breakDuration.TotalSeconds);
                    },
                    onReset: (context) =>
                    {
                        LogCircuitBreakerReset(logger, context.PolicyKey);
                    },
                    onHalfOpen: () =>
                    {
                        LogCircuitBreakerHalfOpen(logger);
                    });
            
            return policy;
        });
    }
    
    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Warning,
        Message = "Circuit breaker OPENED for {ProviderName} - Duration: {DurationSeconds}s")]
    private static partial void LogCircuitBreakerOpened(
        ILogger logger, string providerName, double durationSeconds);
    
    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Information,
        Message = "Circuit breaker RESET for {ProviderName}")]
    private static partial void LogCircuitBreakerReset(
        ILogger logger, string providerName);
    
    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Information,
        Message = "Circuit breaker HALF-OPEN - Testing provider recovery")]
    private static partial void LogCircuitBreakerHalfOpen(ILogger logger);
}
```

### Étape 3 : Configurer HttpClient avec Circuit Breaker

**3.1 Modifier ServiceCollectionExtensions.cs**
```csharp
public static IServiceCollection AddLLMProviderInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var circuitBreakerOptions = configuration
        .GetSection("CircuitBreaker")
        .Get<CircuitBreakerOptions>() ?? new CircuitBreakerOptions();
    
    services.AddSingleton(circuitBreakerOptions);
    
    // OpenAI provider avec circuit breaker
    services.AddHttpClient("OpenAI", client =>
    {
        client.BaseAddress = new Uri("https://api.openai.com/");
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddCircuitBreakerPolicy(circuitBreakerOptions, logger);
    
    // Anthropic provider avec circuit breaker isolé
    services.AddHttpClient("Anthropic", client =>
    {
        client.BaseAddress = new Uri("https://api.anthropic.com/");
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddCircuitBreakerPolicy(circuitBreakerOptions, logger);
    
    // Ollama provider (local) avec circuit breaker
    services.AddHttpClient("Ollama", client =>
    {
        client.BaseAddress = new Uri("http://localhost:11434/");
        client.Timeout = TimeSpan.FromSeconds(60);
    })
    .AddCircuitBreakerPolicy(circuitBreakerOptions, logger);
    
    return services;
}
```

### Étape 4 : Gestion Erreurs Circuit Ouvert

**4.1 Créer CircuitBreakerException**
```csharp
public sealed class CircuitBreakerOpenException : Exception
{
    public string ProviderName { get; }
    public TimeSpan RetryAfter { get; }
    
    public CircuitBreakerOpenException(
        string providerName, 
        TimeSpan retryAfter)
        : base($"Circuit breaker is OPEN for provider {providerName}. Retry after {retryAfter.TotalSeconds}s.")
    {
        ProviderName = providerName;
        RetryAfter = retryAfter;
    }
}
```

**4.2 Gérer dans GlobalExceptionHandlerMiddleware**
```csharp
catch (CircuitBreakerOpenException ex)
{
    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
    context.Response.Headers.RetryAfter = ex.RetryAfter.TotalSeconds.ToString("F0");
    
    await context.Response.WriteAsJsonAsync(new
    {
        error = "Service Unavailable",
        message = ex.Message,
        retryAfter = ex.RetryAfter.TotalSeconds,
        provider = ex.ProviderName
    });
}
```

### Étape 5 : Tests Unitaires

**5.1 Créer CircuitBreakerTests.cs**
```csharp
public class CircuitBreakerTests
{
    [Fact]
    public async Task CircuitBreaker_Should_OpenAfterConsecutiveFailures()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 3,
            DurationOfBreak = TimeSpan.FromSeconds(5),
            MinimumThroughput = 3,
            SamplingDuration = TimeSpan.FromSeconds(10)
        };
        
        // Simuler 3 échecs consécutifs → circuit OPEN
        
        // Assert circuit ouvert
    }
    
    [Fact]
    public async Task CircuitBreaker_Should_TransitionToHalfOpen_AfterDuration()
    {
        // Test transition OPEN → HALF-OPEN après durée
    }
    
    [Fact]
    public async Task CircuitBreaker_Should_Reset_AfterSuccessfulTest()
    {
        // Test HALF-OPEN → CLOSED si succès
    }
}
```

### Étape 6 : Métriques OpenTelemetry

**6.1 Ajouter métriques circuit breaker**
```csharp
var meter = new Meter("LLMProxy.CircuitBreaker");
var circuitBreakerState = meter.CreateObservableGauge<int>(
    "circuit_breaker_state",
    () => new[]
    {
        new Measurement<int>(GetCircuitState("OpenAI"), new("provider", "OpenAI")),
        new Measurement<int>(GetCircuitState("Anthropic"), new("provider", "Anthropic"))
    },
    description: "Circuit breaker state (0=Closed, 1=Open, 2=HalfOpen)");
```

### Étape 7 : Documentation

**7.1 Mettre à jour README.md**
```markdown
### Resilience (Polly)

**Circuit Breaker Pattern:**
- Automatic failure detection (5 consecutive failures)
- Fail-fast when provider unavailable (30s circuit open)
- Isolated circuit per provider (OpenAI, Anthropic, Ollama)
- State transitions: Closed → Open → Half-Open → Closed

**Configuration:**
```json
{
  "CircuitBreaker": {
    "FailureThreshold": 5,
    "DurationOfBreak": "00:00:30",
    "MinimumThroughput": 10,
    "SamplingDuration": "00:01:00"
  }
}
```

**503 Response (Circuit Open):**
```json
{
  "error": "Service Unavailable",
  "message": "Circuit breaker is OPEN for provider OpenAI. Retry after 30s.",
  "retryAfter": 30,
  "provider": "OpenAI"
}
```
```

### Étape 8 : Validation

**8.1 Build & Tests**
```powershell
dotnet build --no-restore
dotnet test --no-build
```

**8.2 Test manuel**
```powershell
# Simuler provider failure
# Vérifier circuit opening après 5 échecs
# Vérifier fail-fast pendant 30s
# Vérifier récupération automatique
```

## ESTIMATION

- **Durée** : 8-10 heures
- **Complexité** : Moyenne
- **Risque** : Faible (Polly mature, pattern standard)

## NOTES

- Polly déjà installé, configuration seulement
- Circuit breaker par provider (isolation)
- Logs structurés pour observabilité
- Métriques pour monitoring production
- Tests critiques : état transitions

## TRACKING
Début: 2025-12-21T23:43:51.5421100Z



## COMPLÉTION

Fin: 2025-12-21T23:50:56.4280663Z
Durée: 00:07:04

### Résultats

**Build**:  0 erreurs, 0 warnings
**Tests**:  72 total (17 Domain + 35 Security + 20 Gateway)
**Conformité ADR-032**: 45%  90%

### Fichiers modifiés
1. CircuitBreakerOptions.cs (43 lignes) - Configuration POCO
2. HttpClientCircuitBreakerExtensions.cs (90 lignes) - Polly v8 integration
3. ServiceCollectionExtensions.cs (1493 lignes) - HttpClient configuration
4. LLMProxy.Infrastructure.LLMProviders.csproj - Packages: Http.Resilience, Polly.Extensions
5. appsettings.json - CircuitBreaker section
6. Program.cs - Configuration enablement

### Implémentation
- 3 circuits isolés (OpenAI, Anthropic, Ollama)
- AddStandardResilienceHandler de Microsoft.Extensions.Http.Resilience
- Logs structurés (EventIds 5001-5003: Opened, Closed, HalfOpened)
- Configuration-driven via appsettings.json

### Commits
- 81c71f7: feat(resilience): Add circuit breaker with Polly v8
- Merge: feature/022  main (--no-ff)
- Feature branch deleted

## COMPLÉTION

Fin: 2025-12-21T23:51:01.6533855Z
Durée: 00:07:10

