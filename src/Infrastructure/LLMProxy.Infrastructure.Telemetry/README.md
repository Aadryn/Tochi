# LLMProxy.Infrastructure.Telemetry

Infrastructure de télémétrie et logging structuré pour LLMProxy.

## Vue d'ensemble

Ce projet fournit des extensions de logging optimisées utilisant les **LoggerMessage Source Generators** de .NET pour obtenir une performance maximale avec zéro allocation lorsque le niveau de log est désactivé.

### Bénéfices

- **Performance** : ~85% de réduction des allocations par rapport au logging traditionnel avec interpolation de chaînes
- **Type-safety** : Vérification à la compilation des paramètres de log
- **Maintenabilité** : Templates de messages centralisés (DRY)
- **Observabilité** : Propriétés structurées indexables pour requêtes dans systèmes de monitoring

## Architecture

```
LoggerMessageAttribute (source generator)
    ↓ génère au build
Méthodes optimisées (zero-allocation)
    ↓ utilisées via
_logger.TenantCreated(id, name)
```

### Ancien vs Nouveau

**❌ Ancien (avec allocations) :**
```csharp
_logger.LogInformation("Tenant {TenantId} created with name '{TenantName}'", tenantId, tenantName);
// → Allocation de string à chaque appel
// → Boxing des Guid (value type → object)
// → Allocation du tableau params object[]
// → Résultat: ~500 bytes alloués
```

**✅ Nouveau (zero-allocation) :**
```csharp
_logger.TenantCreated(tenantId, tenantName);
// → 0 bytes si niveau Information désactivé
// → ~24 bytes si niveau activé (aucun boxing)
// → Template compilé une fois au démarrage
```

## LoggerExtensions disponibles

### 1. TenantLoggerExtensions

Logging structuré pour les opérations Tenant.

```csharp
using LLMProxy.Infrastructure.Telemetry.Logging;

// Création réussie
_logger.TenantCreated(tenantId, tenantName);

// Activation/Désactivation
_logger.TenantActivated(tenantId, previousStatus);
_logger.TenantDeactivated(tenantId, reason);

// Quota
_logger.TenantQuotaExceeded(tenantId, currentUsage, monthlyQuota);
_logger.TenantUsageRecorded(tenantId, tokensUsed, remainingQuota);
_logger.TenantQuotaReset(tenantId, newQuota);

// Mise à jour et suppression
_logger.TenantUpdated(tenantId, updatedFields);
_logger.TenantDeleted(tenantId, tenantName);

// Erreurs
_logger.TenantCreationFailed(exception, tenantName, errorMessage);
_logger.TenantUpdateFailed(exception, tenantId, errorMessage);
_logger.TenantDeletionFailed(exception, tenantId, errorMessage);
```

**EventIds : 1001-1011**

### 2. UserLoggerExtensions

Logging structuré pour les opérations User.

```csharp
using LLMProxy.Infrastructure.Telemetry.Logging;

// Création et modification
_logger.UserCreated(userId, email, tenantId);
_logger.UserUpdated(userId, updatedFields);
_logger.UserDeleted(userId, email);

// Permissions
_logger.UserPermissionChanged(userId, permission, isGranted);

// Erreurs
_logger.UserCreationFailed(exception, email, tenantId, errorMessage);
```

**EventIds : 2001-2005**

### 3. ApiKeyLoggerExtensions

Logging structuré pour les opérations API Key.

```csharp
using LLMProxy.Infrastructure.Telemetry.Logging;

// Cycle de vie
_logger.ApiKeyCreated(apiKeyId, tenantId, scope);
_logger.ApiKeyValidated(tenantId);
_logger.ApiKeyRevoked(apiKeyId, tenantId, reason);

// Erreurs et expiration
_logger.ApiKeyValidationFailed(tenantId, reason);
_logger.ApiKeyExpired(apiKeyId, tenantId, expirationDate);
_logger.ApiKeyCreationFailed(exception, tenantId, errorMessage);
```

**EventIds : 3001-3006**

### 4. QuotaLoggerExtensions

Logging structuré pour les opérations Quota.

```csharp
using LLMProxy.Infrastructure.Telemetry.Logging;

// Opérations quota
_logger.QuotaUsageRecorded(tenantId, tokensUsed, remainingQuota);
_logger.QuotaLimitReached(tenantId, currentUsage, monthlyQuota);
_logger.QuotaReset(tenantId, newQuota);

// Alertes
_logger.QuotaThresholdWarning(tenantId, usagePercentage, threshold);
_logger.QuotaLimitIncreased(tenantId, oldLimit, newLimit);

// Erreurs
_logger.QuotaRecordingFailed(exception, tenantId, errorMessage);
```

**EventIds : 4001-4006**

### 5. LlmLoggerExtensions

Logging structuré pour les requêtes LLM.

```csharp
using LLMProxy.Infrastructure.Telemetry.Logging;

// Cycle de vie requête
_logger.LlmRequestStarted(requestId, tenantId, model, provider);
_logger.LlmRequestCompleted(requestId, durationMs, tokenCount, promptTokens, completionTokens);
_logger.LlmRequestFailed(exception, requestId, tenantId, errorMessage);

// Opérations avancées
_logger.LlmProviderSwitched(requestId, fromProvider, toProvider, reason);
_logger.LlmRateLimited(requestId, tenantId, retryAfterSeconds);
_logger.LlmRequestTimeout(requestId, tenantId, timeoutMs);
_logger.LlmCacheHit(requestId, tenantId, cacheKey);
_logger.LlmStreamingStarted(requestId, tenantId, model);
```

**EventIds : 5001-5008**

## Convention EventId

Les EventIds sont organisés par domaine pour faciliter le filtrage dans les systèmes de monitoring :

- **1000-1999** : Opérations Tenant
- **2000-2999** : Opérations User
- **3000-3999** : Opérations API Key
- **4000-4999** : Opérations Quota
- **5000-5999** : Opérations LLM

## Niveaux de log

- **Debug** : Opérations verboses (validation API key, enregistrement quota)
- **Information** : Événements métier (création, mise à jour, activation)
- **Warning** : Alertes importantes (quota dépassé, clé expirée, désactivation)
- **Error** : Échecs avec exceptions (création échouée, mise à jour échouée)

## Format des messages

### Nombres formatés

Utilisez `:N0` pour les nombres avec séparateurs de milliers :

```csharp
_logger.TenantQuotaExceeded(tenantId, 150000, 100000);
// → "Tenant {id} quota exceeded: 150,000/100,000 tokens"
```

### Durées

Utilisez `:N0` pour les millisecondes :

```csharp
_logger.LlmRequestCompleted(requestId, 1234, 5000, 3000, 2000);
// → "LLM request {id} completed in 1,234ms with 5,000 tokens"
```

### Pourcentages

Utilisez `:F1` pour un décimal :

```csharp
_logger.QuotaThresholdWarning(tenantId, 85.3, 80.0);
// → "Quota threshold warning for tenant {id}: 85.3% of 80.0% threshold"
```

## Gestion des exceptions

Les méthodes d'erreur prennent **toujours l'exception en premier paramètre** (convention ADR-031) :

```csharp
try
{
    // opération
}
catch (Exception ex)
{
    _logger.TenantCreationFailed(
        ex,                    // ← Exception en premier
        tenantName,
        "Detailed error message"
    );
}
```

## Propriétés structurées

Toutes les méthodes capturent des propriétés structurées indexables :

```csharp
_logger.TenantCreated(tenantId, tenantName);

// Propriétés structurées capturées :
// - TenantId (Guid)
// - TenantName (string)
// - EventId (1001)
// - LogLevel (Information)
// - Timestamp (auto)
```

Ces propriétés sont requêtables dans les systèmes de monitoring (Elasticsearch, Application Insights, etc.) :

```
// Exemple requête Elasticsearch
logs where TenantId == "xxx" AND EventId >= 1000 AND EventId < 2000
```

## Performances

### Benchmarks (estimés ADR-031)

| Scénario | Ancien | Nouveau | Gain |
|----------|--------|---------|------|
| Log désactivé | ~500 bytes | 0 bytes | **100%** |
| Log activé | ~500 bytes | ~24 bytes | **95%** |
| Exécution | ~150 ns | ~20 ns | **87%** |

### Optimisations appliquées

- ✅ **Zero-allocation** quand niveau de log désactivé (early exit)
- ✅ **Aucun boxing** des value types (Guid, int, long, DateTime)
- ✅ **Templates compilés** une seule fois au démarrage
- ✅ **Paramètres typés** (vérification compile-time)
- ✅ **Inlining agressif** par le JIT (.NET 9+)

## Migration depuis ancien code

### Avant (string interpolation)

```csharp
_logger.LogInformation("Tenant {TenantId} created with name '{TenantName}'", tenantId, tenantName);
_logger.LogWarning("Tenant slug {Slug} already exists", slug);
_logger.LogError(ex, "Error creating tenant with slug {Slug}", slug);
```

### Après (LoggerMessage)

```csharp
using LLMProxy.Infrastructure.Telemetry.Logging;

_logger.TenantCreated(tenantId, tenantName);
_logger.TenantCreationFailed(
    new InvalidOperationException($"Slug '{slug}' already exists"),
    tenantName,
    $"Tenant with slug '{slug}' already exists"
);
_logger.TenantCreationFailed(ex, tenantName, "An error occurred while creating the tenant");
```

## Ajout de nouvelles méthodes

Pour ajouter une nouvelle méthode de logging :

1. **Choisir le fichier** approprié (TenantLoggerExtensions, UserLoggerExtensions, etc.)

2. **Ajouter la méthode** avec `[LoggerMessage]` :

```csharp
/// <summary>
/// Log l'opération spécifique.
/// </summary>
/// <param name="logger">Logger instance.</param>
/// <param name="param1">Description paramètre 1.</param>
[LoggerMessage(
    EventId = 1012,  // ← Incrémenter dans la bonne plage
    Level = LogLevel.Information,
    Message = "Message avec {Param1} formaté"
)]
public static partial void OperationName(
    this ILogger logger,
    TypeParam1 param1);
```

3. **Respecter les conventions** :
   - EventId dans la bonne plage (1xxx pour Tenant, 2xxx pour User, etc.)
   - Exception toujours en premier paramètre si présente
   - Noms de paramètres en PascalCase dans le message
   - XML doc complète en français

4. **Build** → Le source generator crée automatiquement l'implémentation

## Tests

Les tests unitaires utilisent `Microsoft.Extensions.Logging.Testing` (FakeLogger) :

```csharp
using Microsoft.Extensions.Logging.Testing;

var fakeLogger = new FakeLogger<MyService>();
var service = new MyService(fakeLogger);

service.DoSomething();

// Vérifier les logs capturés
var logRecord = fakeLogger.Collector.GetSnapshot().Single();
Assert.Equal(LogLevel.Information, logRecord.Level);
Assert.Equal(1001, logRecord.Id.Id);
```

## Références

- [ADR-031 : Structured Logging](../../docs/adr/031-structured-logging.adr.md)
- [LoggerMessage Source Generators (.NET)](https://learn.microsoft.com/en-us/dotnet/core/extensions/logger-message-generator)
- [High-performance logging (.NET)](https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging)

## Changelog

### v1.0.0 (2025-12-22)

- ✅ Création des 5 LoggerExtensions (Tenant, User, ApiKey, Quota, Llm)
- ✅ 36 méthodes de logging structuré
- ✅ Migration de 12+ occurrences dans Application et Security
- ✅ EventId ranges définis par domaine
- ✅ Documentation complète
