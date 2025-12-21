---
id: 016
title: Créer classe PublicEndpoints pour constants de paths publics
concerns: constants, explicit-over-implicit
type: refactoring
priority: minor
effort: small
risk: very-low
value: medium
dependencies: []
status: to-do
created: 2025-12-21
---

# Créer Classe PublicEndpoints pour Constants de Paths Publics

## 🎯 Objectif

Créer une classe statique `PublicEndpoints` contenant les constants pour tous les paths publics (endpoints ne nécessitant pas d'authentification) pour éliminer les magic strings et rendre le code explicite.

**Amélioration visée :**
- **Explicit over Implicit** (ADR-016): Noms constants vs strings hardcodées
- **Maintenabilité**: Changement centralisé
- **Typos impossibles**: Erreur compilation si mauvais constant

## 📊 Contexte

### Problème Identifié

- **Type** : Violation ADR-016 (Explicit over Implicit)
- **Localisation** : `ApiKeyAuthenticationMiddleware.cs:30`, autres middlewares
- **Description Factuelle** : Magic strings "/health", "/metrics", "/swagger" hardcodées partout
- **Preuve** :

```csharp
// ❌ IMPLICITE : Magic string
if (context.Request.Path.StartsWithSegments("/health"))
{
    await _next(context);
    return;
}
```

## 🔧 Implémentation

**CRÉER :**
```csharp
// src/Core/LLMProxy.Domain/Common/PublicEndpoints.cs
namespace LLMProxy.Domain.Common;

/// <summary>
/// Paths des endpoints publics (sans authentification requise)
/// </summary>
public static class PublicEndpoints
{
    /// <summary>
    /// Endpoint de health check
    /// </summary>
    public const string Health = "/health";
    
    /// <summary>
    /// Endpoint de métriques Prometheus
    /// </summary>
    public const string Metrics = "/metrics";
    
    /// <summary>
    /// Endpoint de documentation Swagger/OpenAPI
    /// </summary>
    public const string Swagger = "/swagger";
    
    /// <summary>
    /// Vérifie si un path est un endpoint public
    /// </summary>
    public static bool IsPublicEndpoint(PathString path)
    {
        return path.StartsWithSegments(Health)
            || path.StartsWithSegments(Metrics)
            || path.StartsWithSegments(Swagger);
    }
}
```

**UTILISER :**
```csharp
// ✅ EXPLICITE : Constant nommé
if (PublicEndpoints.IsPublicEndpoint(context.Request.Path))
{
    await _next(context);
    return;
}

// Ou individuellement
if (context.Request.Path.StartsWithSegments(PublicEndpoints.Health))
{
    await _next(context);
    return;
}
```

## ✅ Critères de Validation

- [ ] Classe PublicEndpoints créée
- [ ] Tous magic strings remplacés
- [ ] Build réussit
- [ ] Tests middlewares passent

## 📈 Métriques

**Avant :** Magic strings: 5+ occurrences  
**Après :** Constants: 1 fichier centralisé

## 🔗 Références

- `docs/adr/016-explicit-over-implicit.adr.md`


##  COMPLÉTÉ VIA TASK 009

HttpConstants.PublicEndpoints créé avec Health = "/health"

