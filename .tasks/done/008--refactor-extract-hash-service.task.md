---
id: 008
title: Extraire hashing vers IHashService injectable
concerns: architecture, testabilité, réutilisabilité
type: refactoring
priority: minor
effort: small
risk: low
value: medium
dependencies: []
status: to-do
created: 2025-12-21
---

# Extraire hashing vers IHashService injectable

## 🎯 Objectif

Extraire la logique de hashing (`ComputeHash`) du middleware ApiKeyAuthentication vers un service injectable `IHashService` pour améliorer testabilité et réutilisabilité.

**Amélioration visée :**
- **Testabilité** : Mocker facilement le hashing dans les tests
- **Réutilisabilité** : Service partageable entre middlewares/services
- **Modularisation** : Responsabilité unique (SRP)
- **Maintenabilité** : Changement d'algorithme centralisé

**Bénéfice mesurable :** 
- Service réutilisable dans 3+ endroits
- Tests unitaires simplifiés (mock hash)

## 📊 Contexte

### Problème Identifié

- **Type** : Architecture / Testabilité
- **Localisation** : `src/Presentation/LLMProxy.Gateway/Middleware/ApiKeyAuthenticationMiddleware.cs:120-127`
- **Description Factuelle** : La méthode `ComputeHash` est une méthode privée du middleware, rendant difficile le test unitaire isolé et empêchant la réutilisation dans d'autres composants.
- **Impact Actuel** : 
  - Tests doivent passer par le middleware complet (pas d'isolation)
  - Duplication potentielle si autre composant besoin de hash
  - Changement d'algorithme SHA256 nécessite modification middleware
- **Preuve** :

```csharp
// ❌ Méthode privée dans middleware
private string ComputeHash(string input)
{
    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(input);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash);
}
```

**Violation Piliers :**
- **Modularisation** : Responsabilité de hashing mélangée avec authentification
- **Mutualisation** : Code potentiellement dupliqué ailleurs

### Conformité Standards

**Instructions Applicables :**
- `.github/instructions/csharp.standards.instructions.md` - SOLID (SRP, DIP)

**Citation :**
> **Single Responsibility Principle (SRP)** : Une classe ne doit avoir qu'une seule raison de changer

**Vérification de Conformité :**
- [x] Améliore SRP (séparation responsabilités)
- [x] Améliore DIP (dépendance sur abstraction)
- [x] Aucun standard violé

## 🔧 Implémentation

### Approche de Refactoring

**Stratégie :** 
1. Créer interface `IHashService`
2. Créer implémentation `Sha256HashService`
3. Injecter dans middleware via DI
4. Remplacer appel méthode privée par service

**Principe appliqué :**
- **SRP** : Service dédié au hashing
- **DIP** : Dépendance sur interface
- **OCP** : Facile d'ajouter autres algorithmes

### Fichiers à Modifier

- `src/Core/LLMProxy.Domain/Interfaces/IHashService.cs` (nouveau)
- `src/Infrastructure/LLMProxy.Infrastructure.Security/Sha256HashService.cs` (nouveau)
- `src/Infrastructure/LLMProxy.Infrastructure.Security/ServiceCollectionExtensions.cs` (modifier)
- `src/Presentation/LLMProxy.Gateway/Middleware/ApiKeyAuthenticationMiddleware.cs` (modifier)

### Modifications Détaillées

#### Étape 1 : Créer interface IHashService

**Fichier : `src/Core/LLMProxy.Domain/Interfaces/IHashService.cs`**

```csharp
namespace LLMProxy.Domain.Interfaces;

/// <summary>
/// Service de hashing cryptographique pour sécuriser les données sensibles
/// </summary>
public interface IHashService
{
    /// <summary>
    /// Calcule le hash SHA-256 d'une chaîne de caractères
    /// </summary>
    /// <param name="input">Données à hasher (ex: API key, password)</param>
    /// <returns>Hash encodé en Base64</returns>
    /// <exception cref="ArgumentNullException">Si input est null</exception>
    /// <example>
    /// <code>
    /// var hash = hashService.ComputeHash("my-api-key-123");
    /// // Résultat: "dGVzdC1oYXNo..." (Base64)
    /// </code>
    /// </example>
    string ComputeHash(string input);

    /// <summary>
    /// Vérifie si une valeur correspond à un hash donné
    /// </summary>
    /// <param name="input">Valeur à vérifier</param>
    /// <param name="hash">Hash attendu</param>
    /// <returns>True si correspondance, false sinon</returns>
    bool VerifyHash(string input, string hash);
}
```

**Validation :**
- [ ] Interface créée dans Domain (architecture propre)
- [ ] Documentation XML complète
- [ ] Méthodes simples et focalisées

#### Étape 2 : Implémenter Sha256HashService

**Fichier : `src/Infrastructure/LLMProxy.Infrastructure.Security/Sha256HashService.cs`**

```csharp
using LLMProxy.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Implémentation du service de hashing utilisant SHA-256
/// </summary>
/// <remarks>
/// SHA-256 est un algorithme de hashing cryptographique sécurisé adapté pour:
/// - Stockage de clés API
/// - Vérification d'intégrité
/// - Tokens de session
/// 
/// ⚠️ Ne PAS utiliser pour hashing de passwords (utiliser bcrypt/argon2 à la place)
/// </remarks>
public class Sha256HashService : IHashService
{
    /// <summary>
    /// Calcule le hash SHA-256 d'une chaîne
    /// </summary>
    /// <param name="input">Données à hasher</param>
    /// <returns>Hash encodé en Base64</returns>
    /// <exception cref="ArgumentNullException">Si input est null</exception>
    public string ComputeHash(string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input), "Input cannot be null");

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Vérifie si une valeur correspond au hash attendu
    /// </summary>
    /// <param name="input">Valeur à vérifier</param>
    /// <param name="hash">Hash attendu (Base64)</param>
    /// <returns>True si correspondance, false sinon</returns>
    public bool VerifyHash(string input, string hash)
    {
        if (input == null || hash == null)
            return false;

        var computedHash = ComputeHash(input);
        return computedHash.Equals(hash, StringComparison.Ordinal);
    }
}
```

**Validation :**
- [ ] Implémentation créée dans Infrastructure.Security
- [ ] Même comportement que méthode originale
- [ ] Gestion null propre

#### Étape 3 : Enregistrer dans DI

**Fichier : `src/Infrastructure/LLMProxy.Infrastructure.Security/ServiceCollectionExtensions.cs`**

```csharp
using LLMProxy.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LLMProxy.Infrastructure.Security;

/// <summary>
/// Extensions pour enregistrer les services de sécurité dans DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Enregistre tous les services du module Security
    /// </summary>
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        // ✅ NOUVEAU - Hash service
        services.AddSingleton<IHashService, Sha256HashService>();
        
        // Existants
        services.AddSingleton<ISecretService, SecretService>();

        return services;
    }
}
```

**Dans `Program.cs` du Gateway :**
```csharp
// Add infrastructure services
builder.Services.AddSecurityServices();  // ✅ Enregistre IHashService
```

**Validation :**
- [ ] Service enregistré en Singleton (sans état)
- [ ] Gateway Program.cs appelle AddSecurityServices

#### Étape 4 : Injecter dans ApiKeyAuthenticationMiddleware

**État actuel (AVANT) :**
```csharp
public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
    private readonly IApiKeyRepository _repository;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyAuthenticationMiddleware> logger,
        IApiKeyRepository repository)
    {
        _next = next;
        _logger = logger;
        _repository = repository;
    }

    // ...

    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
```

**État cible (APRÈS) :**
```csharp
using LLMProxy.Domain.Interfaces;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
    private readonly IApiKeyRepository _repository;
    private readonly IHashService _hashService;  // ✅ NOUVEAU

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyAuthenticationMiddleware> logger,
        IApiKeyRepository repository,
        IHashService hashService)  // ✅ NOUVEAU
    {
        _next = next;
        _logger = logger;
        _repository = repository;
        _hashService = hashService;
    }

    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        // ...

        var apiKey = ExtractApiKey(context);
        if (string.IsNullOrEmpty(apiKey))
        {
            // ... error handling
        }

        // ✅ Utiliser service au lieu de méthode privée
        var keyHash = _hashService.ComputeHash(apiKey);
        
        var apiKeyEntity = await _repository.GetByKeyHashAsync(keyHash, cancellationToken);

        // ... reste du code
    }

    // ✅ SUPPRIMER méthode privée ComputeHash
}
```

**Validation :**
- [ ] Build réussit
- [ ] Comportement identique
- [ ] Tests passent (avec mock IHashService)

#### Étape 5 : Créer tests unitaires pour HashService

**Fichier : `tests/LLMProxy.Infrastructure.Security.Tests/Sha256HashServiceTests.cs`**

```csharp
using LLMProxy.Infrastructure.Security;
using NFluent;

namespace LLMProxy.Infrastructure.Security.Tests;

public class Sha256HashServiceTests
{
    private readonly Sha256HashService _hashService;

    public Sha256HashServiceTests()
    {
        _hashService = new Sha256HashService();
    }

    [Fact]
    public void ComputeHash_WithValidInput_ShouldReturnBase64Hash()
    {
        // Arrange
        var input = "test-api-key-123";

        // Act
        var hash = _hashService.ComputeHash(input);

        // Assert
        Check.That(hash).IsNotEmpty();
        Check.That(hash).Matches(@"^[A-Za-z0-9+/=]+$"); // Base64 pattern
    }

    [Fact]
    public void ComputeHash_WithSameInput_ShouldReturnSameHash()
    {
        // Arrange
        var input = "same-input";

        // Act
        var hash1 = _hashService.ComputeHash(input);
        var hash2 = _hashService.ComputeHash(input);

        // Assert
        Check.That(hash1).IsEqualTo(hash2);
    }

    [Fact]
    public void ComputeHash_WithDifferentInputs_ShouldReturnDifferentHashes()
    {
        // Arrange
        var input1 = "key1";
        var input2 = "key2";

        // Act
        var hash1 = _hashService.ComputeHash(input1);
        var hash2 = _hashService.ComputeHash(input2);

        // Assert
        Check.That(hash1).IsNotEqualTo(hash2);
    }

    [Fact]
    public void ComputeHash_WithNullInput_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Check.ThatCode(() => _hashService.ComputeHash(null!))
            .Throws<ArgumentNullException>();
    }

    [Fact]
    public void VerifyHash_WithMatchingHash_ShouldReturnTrue()
    {
        // Arrange
        var input = "test-value";
        var hash = _hashService.ComputeHash(input);

        // Act
        var result = _hashService.VerifyHash(input, hash);

        // Assert
        Check.That(result).IsTrue();
    }

    [Fact]
    public void VerifyHash_WithNonMatchingHash_ShouldReturnFalse()
    {
        // Arrange
        var input = "test-value";
        var wrongHash = _hashService.ComputeHash("different-value");

        // Act
        var result = _hashService.VerifyHash(input, wrongHash);

        // Assert
        Check.That(result).IsFalse();
    }
}
```

**Validation :**
- [ ] 6+ tests créés
- [ ] Coverage 100% sur HashService
- [ ] Tests passent

### Considérations Techniques

**Points d'Attention :**
- SHA-256 OK pour API keys, **PAS pour passwords** (utiliser bcrypt/argon2)
- Service Singleton OK (sans état)
- Base64 encoding pour compatibilité stockage string

**Bonnes Pratiques :**
- Interface dans Domain, implémentation dans Infrastructure
- Documentation claire sur use cases appropriés
- Tests unitaires complets

**Pièges à Éviter :**
- Ne pas utiliser MD5 ou SHA1 (deprecated)
- Ne pas oublier using sur SHA256.Create()
- Ne pas hasher passwords avec SHA-256 (trop rapide = bruteforce facile)

## ✅ Critères de Validation

### Tests de Non-Régression

**Tests Obligatoires :**
- [ ] Middleware fonctionne identiquement avec service
- [ ] Même hashes générés qu'avant
- [ ] Tests middleware passent (mock IHashService)
- [ ] Tests HashService passent (100% coverage)

**Validation Fonctionnelle :**
- [ ] API key authentication fonctionne
- [ ] Hash stockés en DB compatibles

### Amélioration des Piliers

**Piliers Améliorés :**
- [x] **Modularisation** : Service dédié au hashing (SRP)
- [x] **Testabilité** : Facile de mocker IHashService
- [x] **Réutilisabilité** : Service utilisable ailleurs
- [x] **Maintenabilité** : Changement algo centralisé

**Piliers Non Dégradés :**
- [x] Performance identique
- [x] Sécurité maintenue (même algo)
- [x] Fonctionnalité préservée

### Conformité et Documentation

- [x] Respecte SRP (SOLID)
- [x] Respecte DIP (SOLID)
- [ ] Documentation XML complète
- [ ] Tests unitaires créés
- [ ] Git commit : `refactor(security): extract hashing to IHashService`

### Plan de Rollback

**En cas de problème :**
1. `git revert <commit-hash>`
2. Vérifier comportement middleware restored

## 📈 Métriques d'Amélioration

**Avant Refactoring :**
- Service hash réutilisable : Non
- Testabilité middleware : Difficile (méthode privée)
- Modularité : Faible (couplage fort)
- Respect SRP : Non (2 responsabilités)

**Après Refactoring (attendu) :**
- Service hash réutilisable : Oui (3+ endroits potentiels)
- Testabilité middleware : Facile (mock service)
- Modularité : Élevée (séparation claire)
- Respect SRP : Oui (1 responsabilité)

**Bénéfice Mesurable :**
- Réutilisabilité : +++ (service injectable partout)
- Testabilité : ++ (mock facile)
- Maintenabilité : ++ (changement algo centralisé)

## 🔗 Références

**SOLID Principles :**
- [Single Responsibility Principle](https://en.wikipedia.org/wiki/Single_responsibility_principle)
- [Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)

**Cryptography :**
- [SHA-256 Overview](https://en.wikipedia.org/wiki/SHA-2)
- [.NET Cryptography](https://learn.microsoft.com/en-us/dotnet/standard/security/cryptography-model)

**Instructions Projet :**
- `.github/instructions/csharp.standards.instructions.md` - SOLID principles

**Patterns :**
- Service Layer Pattern
- Dependency Injection Pattern


##  TRACKING

Début: 2025-12-21T06:27:13.1813474Z


Fin: 2025-12-21T06:28:03.6582789Z
Durée: 00:00:50

##  VALIDATION

- [x] IHashService créé
- [x] Sha256HashService implémenté avec Guard
- [x] Middleware utilise injection au lieu de méthode statique
- [x] Singleton enregistré dans DI
- [x] Build sans warning

