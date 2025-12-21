---
id: 019
title: Créer classe ApiKeyConstants pour magic values API key
concerns: constants, explicit-over-implicit
type: refactoring
priority: minor
effort: small
risk: very-low
value: low
dependencies: []
status: to-do
created: 2025-12-21
---

# Créer Classe ApiKeyConstants pour Magic Values API Key

## 🎯 Objectif

Créer une classe statique `ApiKeyConstants` contenant les constants pour les valeurs magiques liées aux clés API (longueur prefix, format, etc.).

**Amélioration visée :**
- **Explicit over Implicit** (ADR-016): Constants nommés vs magic numbers
- **Maintenabilité**: Changement centralisé
- **Documentation**: Valeurs documentées

## 📊 Contexte

### Problème Identifié

- **Type** : Violation ADR-016
- **Localisation** : `ApiKeyAuthenticationMiddleware.cs:53`
- **Preuve** :

```csharp
// ❌ IMPLICITE : Magic number 12
var prefix = apiKey.Length > 12 ? apiKey.Substring(0, 12) : apiKey;

// ❌ IMPLICITE : Format non documenté
// Pourquoi 12 ? Quel format attendu ?
```

## 🔧 Implémentation

**CRÉER :**
```csharp
// src/Core/LLMProxy.Domain/Common/ApiKeyConstants.cs
namespace LLMProxy.Domain.Common;

/// <summary>
/// Constants liées aux clés API
/// </summary>
public static class ApiKeyConstants
{
    /// <summary>
    /// Longueur du préfixe de clé API pour optimisation lookup BDD
    /// </summary>
    /// <remarks>
    /// Le préfixe "sk_live_XXX" ou "sk_test_XXX" permet un index BDD
    /// pour accélérer la recherche initiale avant vérification hash complet.
    /// </remarks>
    public const int PrefixLength = 12;
    
    /// <summary>
    /// Longueur minimale d'une clé API valide
    /// </summary>
    /// <remarks>
    /// Format: "sk_live_" (8 chars) + 32 chars minimum = 40 chars
    /// </remarks>
    public const int MinLength = 40;
    
    /// <summary>
    /// Longueur maximale d'une clé API
    /// </summary>
    public const int MaxLength = 100;
    
    /// <summary>
    /// Pattern regex du format attendu
    /// </summary>
    /// <remarks>
    /// Formats valides:
    /// - sk_live_XXXXX (production)
    /// - sk_test_XXXXX (test/staging)
    /// Où XXXXX = 32 caractères alphanumériques minimum
    /// </remarks>
    public const string FormatPattern = @"^sk_(live|test)_[A-Za-z0-9]{32,}$";
    
    /// <summary>
    /// Préfixe pour clés de production
    /// </summary>
    public const string LivePrefix = "sk_live_";
    
    /// <summary>
    /// Préfixe pour clés de test
    /// </summary>
    public const string TestPrefix = "sk_test_";
}
```

**UTILISER :**
```csharp
// ✅ EXPLICITE : Constant documenté
var prefix = apiKey.Length > ApiKeyConstants.PrefixLength 
    ? apiKey.Substring(0, ApiKeyConstants.PrefixLength) 
    : apiKey;

// Validation
if (apiKey.Length < ApiKeyConstants.MinLength)
{
    return ValidationResult.Failure($"API key must be at least {ApiKeyConstants.MinLength} characters");
}
```

## ✅ Critères de Validation

- [ ] Classe ApiKeyConstants créée
- [ ] Tous magic numbers remplacés
- [ ] Build réussit

## 📈 Métriques

**Avant :** Magic number 12: 3+ occurrences  
**Après :** Constant documenté: 1 fichier

## 🔗 Références

- `docs/adr/016-explicit-over-implicit.adr.md`


##  COMPLÉTÉ VIA TASK 009

HttpConstants.AuthenticationSchemes créé avec Bearer prefix

