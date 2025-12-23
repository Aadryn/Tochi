---
id: 020
title: Utiliser HttpStatusCode enum au lieu de magic numbers
concerns: constants, explicit-over-implicit, http
type: refactoring
priority: minor
effort: small
risk: very-low
value: low
dependencies: []
status: to-do
created: 2025-12-21
---

# Utiliser HttpStatusCode Enum au Lieu de Magic Numbers

## 🎯 Objectif

Remplacer tous les magic numbers HTTP status codes (401, 500, etc.) par l'enum `HttpStatusCode` de .NET pour rendre le code explicite.

**Amélioration visée :**
- **Explicit over Implicit** (ADR-016): Enum nommé vs magic numbers
- **Lisibilité**: `HttpStatusCode.Unauthorized` vs `401`
- **IntelliSense**: Autocomplete des status codes

## 📊 Contexte

### Problème Identifié

- **Type** : Violation ADR-016
- **Localisation** : Tous middlewares
- **Preuve** :

```csharp
// ❌ IMPLICITE : Magic numbers
context.Response.StatusCode = 401; // Qu'est-ce que 401 ?
context.Response.StatusCode = 500;
context.Response.StatusCode = 429;
```

## 🔧 Implémentation

**AJOUTER using :**
```csharp
using System.Net;
```

**REMPLACER :**
```csharp
// ✅ EXPLICITE : Enum nommé
context.Response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401
context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests; // 429
context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
context.Response.StatusCode = (int)HttpStatusCode.OK; // 200
```

**Principaux status codes à remplacer :**
- `200` → `HttpStatusCode.OK`
- `400` → `HttpStatusCode.BadRequest`
- `401` → `HttpStatusCode.Unauthorized`
- `403` → `HttpStatusCode.Forbidden`
- `404` → `HttpStatusCode.NotFound`
- `429` → `HttpStatusCode.TooManyRequests`
- `500` → `HttpStatusCode.InternalServerError`
- `503` → `HttpStatusCode.ServiceUnavailable`

## ✅ Critères de Validation

- [ ] using System.Net ajouté dans tous middlewares
- [ ] Tous magic numbers status codes remplacés
- [ ] Build réussit
- [ ] Tests passent

## 📈 Métriques

**Avant :** Magic numbers status: 20+ occurrences  
**Après :** Enum HttpStatusCode: Explicite partout

## 🔗 Références

- `docs/adr/016-explicit-over-implicit.adr.md`
- Microsoft docs: `System.Net.HttpStatusCode`


##  EN PLACE

Codes statut HTTP utilisés de manière cohérente (401, 400, 409, 413, 499, 500)

