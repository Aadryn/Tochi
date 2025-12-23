# 45. Configuration Management et Options Pattern

Date: 2025-12-21

## Statut

Accepté

## Contexte

Un proxy LLM nécessite de nombreuses configurations :
- Connexions aux providers (clés API, endpoints)
- Connexions infrastructure (Redis, PostgreSQL)
- Paramètres métier (quotas, timeouts, rate limits)
- Feature flags
- Logging et monitoring

Sans stratégie cohérente :
- Configuration éparpillée dans le code
- Pas de validation au démarrage
- Difficile de changer en runtime
- Secrets exposés

```csharp
// ❌ MAUVAISE CONFIGURATION : Hardcodé et éparpillé
public class LlmService
{
    private readonly string _openAiKey = "sk-xxx"; // ❌ Secret hardcodé !
    private readonly int _timeout = 30;            // ❌ Magic number
    
    public async Task<Response> CallAsync()
    {
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeout);
        // ...
    }
}
```

## Décision

**Adopter le pattern Options de .NET avec validation, strongly-typed configuration, et hiérarchie de sources.**

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    HIÉRARCHIE DE CONFIGURATION                  │
│                    (ordre de priorité croissant)                │
│                                                                 │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  1. appsettings.json (défauts)                            │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                  │
│                              ▼                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  2. appsettings.{Environment}.json (par environnement)    │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                  │
│                              ▼                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  3. Environment Variables (override système)              │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                  │
│                              ▼                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  4. User Secrets (développement local)                    │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                  │
│                              ▼                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  5. Azure Key Vault / AWS Secrets Manager (production)    │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                  │
│                              ▼                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  6. Command Line Arguments (runtime override)             │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### 1. Classes de Configuration Strongly-Typed

```csharp
/// <summary>
/// Configuration des providers LLM.
/// </summary>
public sealed class LlmProvidersOptions
{
    public const string SectionName = "LlmProviders";
    
    public OpenAiOptions OpenAi { get; set; } = new();
    public AnthropicOptions Anthropic { get; set; } = new();
    public AzureOpenAiOptions AzureOpenAi { get; set; } = new();
    
    /// <summary>
    /// Provider par défaut si non spécifié.
    /// </summary>
    public string DefaultProvider { get; set; } = "OpenAi";
    
    /// <summary>
    /// Timeout global pour les appels LLM.
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(60);
    
    /// <summary>
    /// Nombre de retries par défaut.
    /// </summary>
    public int DefaultRetryCount { get; set; } = 3;
}

public sealed class OpenAiOptions
{
    /// <summary>
    /// Clé API OpenAI.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// URL de base de l'API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    
    /// <summary>
    /// Organisation ID (optionnel).
    /// </summary>
    public string? OrganizationId { get; set; }
    
    /// <summary>
    /// Modèle par défaut.
    /// </summary>
    public string DefaultModel { get; set; } = "gpt-4";
    
    /// <summary>
    /// Timeout spécifique pour ce provider.
    /// </summary>
    public TimeSpan? Timeout { get; set; }
}

public sealed class AnthropicOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    public string DefaultModel { get; set; } = "claude-3-opus-20240229";
    public string ApiVersion { get; set; } = "2024-01-01";
}

public sealed class AzureOpenAiOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2024-02-15-preview";
}
```

### 2. Configuration Infrastructure

```csharp
/// <summary>
/// Configuration de la base de données.
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre max de connexions dans le pool.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;
    
    /// <summary>
    /// Timeout de connexion.
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Timeout des commandes.
    /// </summary>
    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Activer les logs SQL détaillés (dev only).
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; }
}

/// <summary>
/// Configuration Redis.
/// </summary>
public sealed class RedisOptions
{
    public const string SectionName = "Redis";
    
    public string ConnectionString { get; set; } = "localhost:6379";
    
    /// <summary>
    /// Préfixe pour toutes les clés.
    /// </summary>
    public string KeyPrefix { get; set; } = "llmproxy";
    
    /// <summary>
    /// TTL par défaut du cache.
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(15);
    
    /// <summary>
    /// Timeout de connexion.
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Timeout des opérations.
    /// </summary>
    public TimeSpan SyncTimeout { get; set; } = TimeSpan.FromSeconds(5);
}
```

### 3. Configuration Métier

```csharp
/// <summary>
/// Configuration des quotas par défaut.
/// </summary>
public sealed class QuotaOptions
{
    public const string SectionName = "Quotas";
    
    public long DefaultMonthlyTokens { get; set; } = 1_000_000;
    public long DefaultMonthlyRequests { get; set; } = 10_000;
    public long DefaultDailyTokens { get; set; } = 100_000;
    public long DefaultDailyRequests { get; set; } = 1_000;
    
    /// <summary>
    /// Pourcentage d'alerte avant limite.
    /// </summary>
    public int AlertThresholdPercent { get; set; } = 80;
}

/// <summary>
/// Configuration du rate limiting.
/// </summary>
public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";
    
    /// <summary>
    /// Rate limit par IP (protection DDoS).
    /// </summary>
    public int IpRequestsPerMinute { get; set; } = 100;
    
    /// <summary>
    /// Rate limit par tenant par défaut.
    /// </summary>
    public int TenantRequestsPerMinute { get; set; } = 1000;
    
    /// <summary>
    /// Rate limit par API Key par défaut.
    /// </summary>
    public int ApiKeyRequestsPerMinute { get; set; } = 100;
    
    /// <summary>
    /// Intervalle de la fenêtre glissante.
    /// </summary>
    public TimeSpan WindowSize { get; set; } = TimeSpan.FromMinutes(1);
}
```

### 4. Validation avec DataAnnotations

```csharp
/// <summary>
/// Configuration avec validation intégrée.
/// </summary>
public sealed class OpenAiOptions : IValidatableObject
{
    [Required(ErrorMessage = "OpenAI API Key is required")]
    [RegularExpression(@"^sk-[a-zA-Z0-9]{48}$", 
        ErrorMessage = "Invalid OpenAI API Key format")]
    public string ApiKey { get; set; } = string.Empty;
    
    [Required]
    [Url(ErrorMessage = "BaseUrl must be a valid URL")]
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    
    [Range(1, 300, ErrorMessage = "Timeout must be between 1 and 300 seconds")]
    public int TimeoutSeconds { get; set; } = 60;
    
    /// <summary>
    /// Validation custom complexe.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (ApiKey.StartsWith("sk-") && BaseUrl.Contains("azure"))
        {
            yield return new ValidationResult(
                "OpenAI API key format is not compatible with Azure endpoint",
                new[] { nameof(ApiKey), nameof(BaseUrl) });
        }
    }
}

/// <summary>
/// Validateur personnalisé pour les options.
/// </summary>
public class OpenAiOptionsValidator : IValidateOptions<OpenAiOptions>
{
    private readonly ILogger<OpenAiOptionsValidator> _logger;
    
    public ValidateOptionsResult Validate(string? name, OpenAiOptions options)
    {
        var failures = new List<string>();
        
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            failures.Add("OpenAI API Key is required");
        }
        else if (!options.ApiKey.StartsWith("sk-"))
        {
            failures.Add("OpenAI API Key must start with 'sk-'");
        }
        
        if (options.TimeoutSeconds < 1 || options.TimeoutSeconds > 300)
        {
            failures.Add("Timeout must be between 1 and 300 seconds");
        }
        
        if (failures.Any())
        {
            _logger.LogError(
                "OpenAI configuration validation failed: {Failures}",
                string.Join(", ", failures));
            
            return ValidateOptionsResult.Fail(failures);
        }
        
        return ValidateOptionsResult.Success;
    }
}
```

### 5. Validation au Démarrage

```csharp
/// <summary>
/// Extension pour valider les options au démarrage.
/// </summary>
public static class OptionsValidationExtensions
{
    public static IServiceCollection AddValidatedOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart(); // ← Validation au démarrage !
        
        return services;
    }
}

/// <summary>
/// Configuration dans Program.cs.
/// </summary>
public static void ConfigureOptions(
    IServiceCollection services,
    IConfiguration configuration)
{
    // Options avec validation au démarrage
    services.AddValidatedOptions<LlmProvidersOptions>(
        configuration, LlmProvidersOptions.SectionName);
    
    services.AddValidatedOptions<DatabaseOptions>(
        configuration, DatabaseOptions.SectionName);
    
    services.AddValidatedOptions<RedisOptions>(
        configuration, RedisOptions.SectionName);
    
    // Validateur custom
    services.AddSingleton<IValidateOptions<OpenAiOptions>, OpenAiOptionsValidator>();
}
```

### 6. Injection des Options

```csharp
/// <summary>
/// Patterns d'injection des options.
/// </summary>

// IOptions<T> - Singleton, valeur lue une fois au démarrage
public class SingletonService
{
    private readonly LlmProvidersOptions _options;
    
    public SingletonService(IOptions<LlmProvidersOptions> options)
    {
        _options = options.Value; // Snapshot fixe
    }
}

// IOptionsSnapshot<T> - Scoped, relue à chaque requête
public class ScopedService
{
    private readonly LlmProvidersOptions _options;
    
    public ScopedService(IOptionsSnapshot<LlmProvidersOptions> options)
    {
        _options = options.Value; // Peut changer entre requêtes
    }
}

// IOptionsMonitor<T> - Singleton avec notification de changements
public class MonitoredService : IDisposable
{
    private readonly IOptionsMonitor<LlmProvidersOptions> _optionsMonitor;
    private readonly IDisposable? _changeListener;
    private LlmProvidersOptions _currentOptions;
    
    public MonitoredService(IOptionsMonitor<LlmProvidersOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
        _currentOptions = optionsMonitor.CurrentValue;
        
        _changeListener = optionsMonitor.OnChange(OnOptionsChanged);
    }
    
    private void OnOptionsChanged(LlmProvidersOptions newOptions)
    {
        _currentOptions = newOptions;
        // Réagir au changement (reconfigurer clients, etc.)
    }
    
    public void Dispose()
    {
        _changeListener?.Dispose();
    }
}
```

### 7. Configuration Dynamique avec Reload

```csharp
/// <summary>
/// Support du rechargement de configuration sans redémarrage.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddReloadableConfiguration(
        this IConfigurationBuilder builder,
        IHostEnvironment environment)
    {
        return builder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", 
                optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
    }
}

/// <summary>
/// Service qui réagit aux changements de configuration.
/// </summary>
public sealed class DynamicConfigurationService : IHostedService
{
    private readonly IOptionsMonitor<RateLimitOptions> _rateLimitOptions;
    private readonly IRateLimiter _rateLimiter;
    private IDisposable? _changeListener;
    
    public Task StartAsync(CancellationToken ct)
    {
        _changeListener = _rateLimitOptions.OnChange(OnRateLimitConfigChanged);
        return Task.CompletedTask;
    }
    
    private void OnRateLimitConfigChanged(RateLimitOptions newOptions)
    {
        // Reconfigurer le rate limiter sans redémarrage
        _rateLimiter.Configure(new RateLimitConfiguration
        {
            IpLimit = newOptions.IpRequestsPerMinute,
            TenantLimit = newOptions.TenantRequestsPerMinute
        });
    }
    
    public Task StopAsync(CancellationToken ct)
    {
        _changeListener?.Dispose();
        return Task.CompletedTask;
    }
}
```

### 8. Secrets Management

```csharp
/// <summary>
/// Configuration des secrets selon l'environnement.
/// </summary>
public static class SecretsConfigurationExtensions
{
    public static IConfigurationBuilder AddSecrets(
        this IConfigurationBuilder builder,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            // User Secrets pour le développement local
            builder.AddUserSecrets<Program>();
        }
        else
        {
            // Azure Key Vault pour la production
            var keyVaultUri = Environment.GetEnvironmentVariable("KEYVAULT_URI");
            if (!string.IsNullOrEmpty(keyVaultUri))
            {
                var credential = new DefaultAzureCredential();
                builder.AddAzureKeyVault(new Uri(keyVaultUri), credential);
            }
        }
        
        return builder;
    }
}

/// <summary>
/// Configuration Program.cs complète.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddReloadableConfiguration(builder.Environment)
    .AddSecrets(builder.Environment);

// Enregistrer les options avec validation
builder.Services.AddValidatedOptions<LlmProvidersOptions>(
    builder.Configuration, 
    LlmProvidersOptions.SectionName);
```

### 9. appsettings.json Structure

```json
{
  "LlmProviders": {
    "DefaultProvider": "OpenAi",
    "DefaultTimeout": "00:01:00",
    "DefaultRetryCount": 3,
    "OpenAi": {
      "ApiKey": "",
      "BaseUrl": "https://api.openai.com/v1",
      "DefaultModel": "gpt-4",
      "Timeout": "00:00:60"
    },
    "Anthropic": {
      "ApiKey": "",
      "BaseUrl": "https://api.anthropic.com",
      "DefaultModel": "claude-3-opus-20240229",
      "ApiVersion": "2024-01-01"
    }
  },
  "Database": {
    "ConnectionString": "",
    "MaxPoolSize": 100,
    "ConnectionTimeout": "00:00:30",
    "CommandTimeout": "00:00:30",
    "EnableSensitiveDataLogging": false
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "KeyPrefix": "llmproxy",
    "DefaultExpiration": "00:15:00",
    "ConnectTimeout": "00:00:05"
  },
  "Quotas": {
    "DefaultMonthlyTokens": 1000000,
    "DefaultMonthlyRequests": 10000,
    "AlertThresholdPercent": 80
  },
  "RateLimiting": {
    "IpRequestsPerMinute": 100,
    "TenantRequestsPerMinute": 1000,
    "ApiKeyRequestsPerMinute": 100,
    "WindowSize": "00:01:00"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### 10. Environment Variables Mapping

```csharp
/// <summary>
/// Mapping des variables d'environnement.
/// Convention : Section__Propriété__SousPropriété
/// </summary>

// Exemples de variables d'environnement :
// LLMPROVIDERS__OPENAI__APIKEY=sk-xxx
// LLMPROVIDERS__DEFAULTTIMEOUT=00:02:00
// DATABASE__CONNECTIONSTRING=Host=prod-db;...
// REDIS__CONNECTIONSTRING=redis.prod.internal:6379

// Docker Compose exemple
/*
services:
  gateway:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - LLMPROVIDERS__OPENAI__APIKEY=${OPENAI_API_KEY}
      - DATABASE__CONNECTIONSTRING=Host=postgres;Database=llmproxy;Username=app
      - REDIS__CONNECTIONSTRING=redis:6379
*/
```

## Conséquences

### Positives

- **Type-safety** : Erreurs de typo détectées à la compilation
- **Validation** : Erreurs détectées au démarrage
- **Flexibilité** : Configuration par environnement
- **Sécurité** : Secrets séparés du code

### Négatives

- **Verbosité** : Beaucoup de classes de configuration
  - *Mitigation* : Source generators possibles
- **Overhead** : Validation à chaque démarrage
  - *Mitigation* : Négligeable (<100ms)

### Neutres

- Pattern standard .NET
- Compatible avec tous les providers de secrets

## Alternatives considérées

### Option A : Configuration statique

- **Description** : Valeurs hardcodées ou constantes
- **Avantages** : Simple
- **Inconvénients** : Pas flexible, pas sécurisé pour les secrets
- **Raison du rejet** : Inadapté aux environnements multiples

### Option B : Base de données pour la config

- **Description** : Stocker la configuration en DB
- **Avantages** : UI d'administration facile
- **Inconvénients** : Dépendance DB au démarrage
- **Raison du rejet** : Complexité accrue, config bootstrap problématique

## Références

- [Options Pattern - Microsoft](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Safe Storage of Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
