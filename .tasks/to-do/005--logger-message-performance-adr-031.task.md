# Tâche 005 - LoggerMessage Performance (ADR-031)

**Créée le** : 2025-12-21  
**Criticité** : 🟢 MINEUR  
**Priorité** : P3 (OPTIONNELLE)  
**Effort estimé** : 2 heures  
**Risque** : FAIBLE

---

## OBJECTIF

Remplacer l'interpolation de chaînes dans les logs par **LoggerMessage** avec source generators pour améliorer les performances (3-6x plus rapide), conformément à **ADR-031 (Structured Logging)**.

**Situation actuelle** :
```csharp
_logger.LogWarning("Revoked API key used: {KeyId}", apiKey.Id);
_logger.LogInformation("HTTP {Method} {Path} completed", method, path);
```

**Problème** : Allocation de strings à chaque log, boxing/unboxing, performance sous-optimale en haute charge.

**Impact** : Performance - réduction CPU et allocations mémoire en production

---

## CRITÈRES DE SUCCÈS

- [ ] **Classe LoggerExtensions créée** avec méthodes LoggerMessage
- [ ] **Source generators activés** (C# 10+, .NET 6+)
- [ ] **Tous les logs critiques convertis** :
  - [ ] ApiKeyValidator logs (revoked, expired, inactive user)
  - [ ] RequestLoggingMiddleware logs (request start/end)
  - [ ] GlobalExceptionHandlerMiddleware logs (exceptions)
  - [ ] ApiKeyAuthenticator logs (authentication events)
- [ ] **EventIds uniques** : Chaque message a un EventId distinct
- [ ] **Build : 0 errors, 0 warnings**
- [ ] **Tests : 100% passed**
- [ ] **Logs validés** : Même output qu'avant (rétro-compatible)

---

## SPÉCIFICATIONS TECHNIQUES

### LoggerMessage Pattern

**Avant** (interpolation) :
```csharp
_logger.LogWarning("Revoked API key used: {KeyId}", apiKey.Id);
```

**Après** (LoggerMessage) :
```csharp
public static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Revoked API key used: {KeyId}")]
    public static partial void LogRevokedKeyUsage(
        this ILogger logger, Guid keyId);
}

// Utilisation
_logger.LogRevokedKeyUsage(apiKey.Id);
```

### Bénéfices

**Performance** :
- 3-6x plus rapide que string interpolation
- Pas d'allocation de chaînes
- Pas de boxing/unboxing
- Code généré au compile-time

**Qualité** :
- EventIds centralisés
- Type-safety (compile-time checks)
- IntelliSense amélioré

### EventIds Convention

```
1xxx : Security/Authentication
  1001 : Revoked API key used
  1002 : Expired API key used
  1003 : Inactive user
  1004 : Authentication success
  1005 : Authentication failure

2xxx : Request/Response
  2001 : Request started
  2002 : Request completed
  2003 : Request failed

3xxx : Exceptions
  3001 : Unhandled exception
  3002 : Operation cancelled
  3003 : Unauthorized access
```

---

## PLAN D'EXÉCUTION

### ÉTAPE 1 : Créer feature branch (2 min)

```powershell
git checkout -b feature/005--logger-message-performance
```

### ÉTAPE 2 : Créer LoggerExtensions.Security.cs (30 min)

Fichier : `src/Infrastructure/LLMProxy.Infrastructure.Security/LoggerExtensions.Security.cs`

```csharp
namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Extensions de logging haute performance pour le module Security.
/// Utilise LoggerMessage avec source generators pour 3-6x amélioration performance.
/// </summary>
public static partial class SecurityLoggerExtensions
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Revoked API key used: {KeyId}")]
    public static partial void LogRevokedKeyUsage(
        this ILogger logger, Guid keyId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Expired API key used: {KeyId}")]
    public static partial void LogExpiredKeyUsage(
        this ILogger logger, Guid keyId);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Inactive user for API key: {UserId}")]
    public static partial void LogInactiveUser(
        this ILogger logger, Guid userId);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Information,
        Message = "API key authenticated successfully for user: {UserId}")]
    public static partial void LogAuthenticationSuccess(
        this ILogger logger, Guid userId);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Warning,
        Message = "Authentication failed: {Reason}")]
    public static partial void LogAuthenticationFailure(
        this ILogger logger, string reason);
}
```

### ÉTAPE 3 : Créer LoggerExtensions.Gateway.cs (30 min)

Fichier : `src/Presentation/LLMProxy.Gateway/Extensions/LoggerExtensions.Gateway.cs`

```csharp
namespace LLMProxy.Gateway.Extensions;

public static partial class GatewayLoggerExtensions
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "HTTP {Method} {Path} started - RequestId: {RequestId}")]
    public static partial void LogRequestStarted(
        this ILogger logger, string method, string path, Guid requestId);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "HTTP {Method} {Path} completed in {DurationMs}ms with status {StatusCode} - RequestId: {RequestId}")]
    public static partial void LogRequestCompleted(
        this ILogger logger, string method, string path, double durationMs, int statusCode, Guid requestId);

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Error,
        Message = "Unhandled exception in request {Method} {Path}: {ExceptionMessage}")]
    public static partial void LogUnhandledException(
        this ILogger logger, string method, string path, string exceptionMessage, Exception exception);
}
```

### ÉTAPE 4 : Modifier ApiKeyValidator.cs (15 min)

Remplacer les appels _logger.LogXXX par extensions :

```csharp
// AVANT
_logger.LogWarning("Revoked API key used: {KeyId}", apiKey.Id);

// APRÈS
_logger.LogRevokedKeyUsage(apiKey.Id);
```

### ÉTAPE 5 : Modifier RequestLoggingMiddleware.cs (15 min)

```csharp
// AVANT
_logger.LogInformation("HTTP {Method} {Path} started - RequestId: {RequestId}", ...);

// APRÈS  
_logger.LogRequestStarted(context.Request.Method, context.Request.Path, requestId);
```

### ÉTAPE 6 : Modifier GlobalExceptionHandlerMiddleware.cs (15 min)

```csharp
// AVANT
_logger.LogError(ex, "Unhandled exception");

// APRÈS
_logger.LogUnhandledException(method, path, ex.Message, ex);
```

### ÉTAPE 7 : Build et tests (5 min)

```powershell
dotnet build --no-restore
dotnet test --no-build
```

### ÉTAPE 8 : Validation performance (10 min)

- Comparer allocations avant/après (dotnet-counters)
- Vérifier format logs identique

### ÉTAPE 9 : Commit et merge (3 min)

```powershell
git add .
git commit -m "perf(logging): Use LoggerMessage source generators (ADR-031)

Replace string interpolation with LoggerMessage for high-performance logging:
- Security module: 5 methods (revoked, expired, inactive, success, failure)
- Gateway module: 3 methods (request start/end, exceptions)

Benefits:
- 3-6x performance improvement
- Reduced memory allocations
- Type-safe compile-time checks
- Centralized EventIds

EventId ranges:
- 1xxx: Security/Authentication
- 2xxx: Request/Response
- 3xxx: Exceptions

ADR-031: Structured Logging - 100% compliant (performance optimized)
Tests: XX/XX passing
Build: 0 errors, 0 warnings"

git checkout main
git merge --no-ff feature/005--logger-message-performance
git branch -d feature/005--logger-message-performance
```

---

## DÉPENDANCES

- **Requis** : C# 10+ (.NET 6+) pour source generators
- **Note** : Projet déjà en .NET 9.0 ✅

---

## RÉFÉRENCES

- **ADR-031** : Structured Logging
- **Rapport** : `docs/ANALYSE_CONFORMITE_ADR.md` (Problème m2)
- **Microsoft Docs** : https://learn.microsoft.com/en-us/dotnet/core/extensions/logger-message-generator

---

_Conforme à : ADR-031 (Structured Logging - performance optimized)_
