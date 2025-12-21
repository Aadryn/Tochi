---
id: 017
title: Créer classe HttpHeaders pour constants de header names
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

# Créer Classe HttpHeaders pour Constants de Header Names

## 🎯 Objectif

Créer une classe statique `HttpHeaders` contenant les constants pour tous les noms de headers HTTP utilisés dans le projet pour éliminer les magic strings.

**Amélioration visée :**
- **Explicit over Implicit** (ADR-016): Noms constants vs strings
- **IntelliSense**: Autocomplete des headers
- **Typos impossibles**: Erreur compilation

## 📊 Contexte

### Problème Identifié

- **Type** : Violation ADR-016
- **Localisation** : `ApiKeyAuthenticationMiddleware.cs:143-153`
- **Preuve** :

```csharp
// ❌ IMPLICITE : Magic strings
context.Request.Headers.TryGetValue("Authorization", out var authHeader)
context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader)
context.Request.Headers.TryGetValue("X-Request-Id", out var requestId)
```

## 🔧 Implémentation

**CRÉER :**
```csharp
// src/Core/LLMProxy.Domain/Common/HttpHeaders.cs
namespace LLMProxy.Domain.Common;

/// <summary>
/// Noms des headers HTTP utilisés dans l'application
/// </summary>
public static class HttpHeaders
{
    /// <summary>
    /// Header d'authentification standard (Bearer token)
    /// </summary>
    public const string Authorization = "Authorization";
    
    /// <summary>
    /// Header personnalisé pour clé API
    /// </summary>
    public const string ApiKey = "X-API-Key";
    
    /// <summary>
    /// Header de traçabilité pour corrélation logs
    /// </summary>
    public const string RequestId = "X-Request-Id";
    
    /// <summary>
    /// Header Content-Type
    /// </summary>
    public const string ContentType = "Content-Type";
    
    /// <summary>
    /// Header User-Agent
    /// </summary>
    public const string UserAgent = "User-Agent";
}
```

**UTILISER :**
```csharp
// ✅ EXPLICITE
if (context.Request.Headers.TryGetValue(HttpHeaders.Authorization, out var authHeader))
{
    // ...
}

if (context.Request.Headers.TryGetValue(HttpHeaders.ApiKey, out var apiKeyHeader))
{
    // ...
}
```

## ✅ Critères de Validation

- [ ] Classe HttpHeaders créée
- [ ] Tous magic strings headers remplacés
- [ ] Build réussit

## 📈 Métriques

**Avant :** Magic strings headers: 10+ occurrences  
**Après :** Constants: 1 fichier centralisé

## 🔗 Références

- `docs/adr/016-explicit-over-implicit.adr.md`


##  COMPLÉTÉ VIA TASK 009

HttpConstants.HttpHeaders créé avec Authorization, ApiKey, RequestId

