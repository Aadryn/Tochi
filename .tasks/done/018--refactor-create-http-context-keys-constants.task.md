---
id: 018
title: Créer classe HttpContextKeys pour constants de Context.Items
concerns: constants, explicit-over-implicit, type-safety
type: refactoring
priority: minor
effort: small
risk: very-low
value: medium
dependencies: []
status: to-do
created: 2025-12-21
---

# Créer Classe HttpContextKeys pour Constants de Context.Items

## 🎯 Objectif

Créer une classe statique `HttpContextKeys` avec méthodes typesafe pour accéder aux `HttpContext.Items` et éliminer les magic strings + améliorer type safety.

**Amélioration visée :**
- **Explicit over Implicit** (ADR-016): Constants nommés
- **Type Safety**: Méthodes typesafe Get/Set
- **Typos impossibles**: Erreur compilation

## 📊 Contexte

### Problème Identifié

- **Type** : Violation ADR-016, manque type safety
- **Localisation** : `ApiKeyAuthenticationMiddleware.cs:105-108`
- **Preuve** :

```csharp
// ❌ IMPLICITE + UNSAFE : Magic strings + casting manuel
context.Items["UserId"] = user.Id;
context.Items["TenantId"] = user.TenantId;
context.Items["ApiKeyId"] = validKey.Id;
context.Items["UserRole"] = user.Role.ToString();

// Lecture
var userId = context.Items["UserId"] as Guid?; // Risque de typo
```

## 🔧 Implémentation

**CRÉER :**
```csharp
// src/Core/LLMProxy.Domain/Common/HttpContextKeys.cs
using Microsoft.AspNetCore.Http;

namespace LLMProxy.Domain.Common;

/// <summary>
/// Clés et méthodes typesafe pour HttpContext.Items
/// </summary>
public static class HttpContextKeys
{
    // Constants privés
    private const string UserIdKey = "UserId";
    private const string TenantIdKey = "TenantId";
    private const string ApiKeyIdKey = "ApiKeyId";
    private const string UserRoleKey = "UserRole";
    
    /// <summary>
    /// Définit l'ID utilisateur dans le contexte
    /// </summary>
    public static void SetUserId(this HttpContext context, Guid userId)
    {
        Guard.AgainstNull(context, nameof(context));
        Guard.AgainstEmptyGuid(userId, nameof(userId));
        context.Items[UserIdKey] = userId;
    }
    
    /// <summary>
    /// Récupère l'ID utilisateur depuis le contexte
    /// </summary>
    public static Guid? GetUserId(this HttpContext context)
    {
        Guard.AgainstNull(context, nameof(context));
        return context.Items[UserIdKey] as Guid?;
    }
    
    /// <summary>
    /// Définit l'ID tenant dans le contexte
    /// </summary>
    public static void SetTenantId(this HttpContext context, Guid tenantId)
    {
        Guard.AgainstNull(context, nameof(context));
        Guard.AgainstEmptyGuid(tenantId, nameof(tenantId));
        context.Items[TenantIdKey] = tenantId;
    }
    
    /// <summary>
    /// Récupère l'ID tenant depuis le contexte
    /// </summary>
    public static Guid? GetTenantId(this HttpContext context)
    {
        Guard.AgainstNull(context, nameof(context));
        return context.Items[TenantIdKey] as Guid?;
    }
    
    /// <summary>
    /// Définit l'ID de la clé API dans le contexte
    /// </summary>
    public static void SetApiKeyId(this HttpContext context, Guid apiKeyId)
    {
        Guard.AgainstNull(context, nameof(context));
        Guard.AgainstEmptyGuid(apiKeyId, nameof(apiKeyId));
        context.Items[ApiKeyIdKey] = apiKeyId;
    }
    
    /// <summary>
    /// Récupère l'ID de la clé API depuis le contexte
    /// </summary>
    public static Guid? GetApiKeyId(this HttpContext context)
    {
        Guard.AgainstNull(context, nameof(context));
        return context.Items[ApiKeyIdKey] as Guid?;
    }
    
    /// <summary>
    /// Définit le rôle utilisateur dans le contexte
    /// </summary>
    public static void SetUserRole(this HttpContext context, string userRole)
    {
        Guard.AgainstNull(context, nameof(context));
        Guard.AgainstNullOrWhiteSpace(userRole, nameof(userRole));
        context.Items[UserRoleKey] = userRole;
    }
    
    /// <summary>
    /// Récupère le rôle utilisateur depuis le contexte
    /// </summary>
    public static string? GetUserRole(this HttpContext context)
    {
        Guard.AgainstNull(context, nameof(context));
        return context.Items[UserRoleKey] as string;
    }
}
```

**UTILISER :**
```csharp
// ✅ EXPLICITE + TYPE-SAFE : Extension methods

// Écriture
context.SetUserId(user.Id);
context.SetTenantId(user.TenantId);
context.SetApiKeyId(validKey.Id);
context.SetUserRole(user.Role.ToString());

// Lecture
var userId = context.GetUserId(); // Guid? typé
if (userId.HasValue)
{
    Guard.AgainstEmptyGuid(userId.Value, nameof(userId));
    // ...
}
```

## ✅ Critères de Validation

- [ ] Classe HttpContextKeys créée avec extension methods
- [ ] Tous accès Context.Items remplacés
- [ ] Guards intégrés dans setters
- [ ] Build réussit
- [ ] Tests passent

## 📈 Métriques

**Avant :**
- Magic strings Context.Items: 15+ occurrences
- Type safety: 0% (casting manuel)

**Après :**
- Constants privés + extension methods
- Type safety: 100% (méthodes typées)

## 🔗 Références

- `docs/adr/016-explicit-over-implicit.adr.md`
- `docs/adr/018-guard-clauses-validation.adr.md`


##  PEUT ÊTRE FAIT SI NÉCESSAIRE

Actuellement: clés HttpContext.Items en string literals (acceptable pour l'instant)

