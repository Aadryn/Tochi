# Value Objects - Domain-Driven Design

## Vue d'ensemble

Ce dossier contient les **Value Objects** du domain model, conformément à [ADR-024](../../../docs/adr/024-value-objects.adr.md). Les Value Objects encapsulent des concepts métier avec validation intégrée, garantissent l'immutabilité et éliminent la "primitive obsession".

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│                   Domain Layer                            │
│                                                            │
│  ┌────────────────────────────────────────────────────┐  │
│  │           ValueObject (Base Class)                 │  │
│  │   - Equality based on components                   │  │
│  │   - Immutable by design                            │  │
│  │   - Operators ==, !=                               │  │
│  └──────────────┬─────────────────────────────────────┘  │
│                 │                                          │
│  ┌──────────────▼──────────┐  ┌────────────────────┐     │
│  │    Email                │  │   Slug             │     │
│  │  - RFC validation       │  │  - URL-safe        │     │
│  │  - Lowercase normalized │  │  - Max 100 chars   │     │
│  │  - Max 255 chars        │  │  - Alphanumeric    │     │
│  └─────────────────────────┘  └────────────────────┘     │
│                                                            │
└──────────────────────────────────────────────────────────┘
```

## Principes fondamentaux

### 1. Immutabilité

Les Value Objects sont **immuables** - une fois créés, leur état ne peut pas changer :

```csharp
// ✅ Immutable
public sealed class Email : ValueObject
{
    public string Value { get; } // Propriété read-only
    
    private Email(string value) // Constructeur privé
    {
        Value = value.ToLowerInvariant();
    }
    
    // Pas de setters, pas de méthodes mutantes
}
```

**Pourquoi ?**
- Thread-safety automatique
- Pas d'effets de bord
- Hashcode stable (utilisable comme clé dictionary)
- Raisonnement plus simple sur le code

### 2. Validation dans la création

Toute validation est effectuée dans la méthode factory `Create()` qui retourne `Result<T>` :

```csharp
public static Result<Email> Create(string email)
{
    // 1. Validation null/empty
    if (string.IsNullOrWhiteSpace(email))
        return Error.Validation.Required(nameof(email));
    
    // 2. Validation longueur
    if (email.Length > 255)
        return Error.Validation.TooLong(nameof(email), 255);
    
    // 3. Validation format
    if (!email.Contains('@') || !email.Contains('.'))
        return Error.Validation.InvalidEmail(email);
    
    // 4. Création garantie valide
    return new Email(email);
}
```

**Invariant garanti** : Si un `Email` existe, il est **toujours valide**.

### 3. Égalité structurelle

Deux Value Objects sont égaux si leurs composants sont égaux :

```csharp
var email1 = Email.Create("test@example.com").Value;
var email2 = Email.Create("TEST@EXAMPLE.COM").Value;

// ✅ True - comparaison par valeur (normalized lowercase)
Console.WriteLine(email1 == email2);

// ❌ False pour entités (comparaison par ID)
var user1 = new User(id: Guid.NewGuid(), ...);
var user2 = new User(id: Guid.NewGuid(), ...);
Console.WriteLine(user1 == user2); // False même si mêmes données
```

### 4. Pas d'identité

Contrairement aux **Entities**, les Value Objects n'ont pas d'identité propre :

| Concept | Identité | Égalité | Mutable | Exemple |
|---------|----------|---------|---------|---------|
| **Entity** | ID unique | Par ID | Oui | User, Tenant |
| **Value Object** | Aucune | Par valeur | Non | Email, Slug |

## Structure d'un Value Object

```csharp
using LLMProxy.Domain.Common;

namespace LLMProxy.Domain.ValueObjects;

/// <summary>
/// Value Object représentant [CONCEPT MÉTIER].
/// </summary>
/// <remarks>
/// [INVARIANTS ET RÈGLES MÉTIER]
/// Conforme à ADR-024 (Value Objects).
/// </remarks>
public sealed class MyValueObject : ValueObject
{
    /// <summary>
    /// Obtient la valeur [normalisée/validée].
    /// </summary>
    public string Value { get; }
    
    // Constructeur PRIVÉ - force utilisation de Create()
    private MyValueObject(string value)
    {
        Value = value;
    }
    
    /// <summary>
    /// Crée une instance après validation.
    /// </summary>
    /// <param name="value">Valeur à valider.</param>
    /// <returns>Résultat contenant le Value Object ou une erreur.</returns>
    public static Result<MyValueObject> Create(string value)
    {
        // VALIDATIONS MÉTIER
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation.Required(nameof(value));
        
        if (value.Length > 100)
            return Error.Validation.TooLong(nameof(value), 100);
        
        // Validation métier spécifique
        if (!IsValid(value))
            return new Error("MyValueObject.Invalid", "...");
        
        return new MyValueObject(value);
    }
    
    // Égalité basée sur les composants
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    // Conversion implicite vers string (optionnel)
    public static implicit operator string(MyValueObject vo) => vo.Value;
    
    // ToString pour debug/logs
    public override string ToString() => Value;
    
    // Méthode de validation privée
    private static bool IsValid(string value)
    {
        // Logique de validation
        return true;
    }
}
```

## Value Objects existants

### Email

**Concept métier** : Adresse email valide et normalisée.

**Invariants** :
- Format valide (contient @ et .)
- Maximum 255 caractères
- Stocké en lowercase pour comparaisons cohérentes

**Utilisation** :
```csharp
// Création avec validation
var emailResult = Email.Create("Test@Example.COM");
if (emailResult.IsFailure)
{
    Console.WriteLine($"Erreur: {emailResult.Error.Message}");
    return;
}

var email = emailResult.Value;
Console.WriteLine(email.Value); // "test@example.com" (normalized)

// Conversion implicite
string emailString = email; // "test@example.com"

// Égalité
var email2 = Email.Create("test@example.com").Value;
Console.WriteLine(email == email2); // True
```

**Validation** :
```csharp
Email.Create("")                    // ❌ Error.Validation.Required
Email.Create("a".PadRight(300))     // ❌ Error.Validation.TooLong
Email.Create("invalid")             // ❌ Error.Validation.InvalidEmail
Email.Create("test@example.com")    // ✅ Success
```

### Slug

**Concept métier** : Identifiant URL-safe pour ressources (tenants, etc.).

**Invariants** :
- Format URL-safe (alphanumeric + tirets)
- Maximum 100 caractères
- Pas d'espaces, caractères spéciaux

**Utilisation** :
```csharp
var slugResult = Slug.Create("my-company-2024");
if (slugResult.IsFailure)
    return slugResult.Error;

var slug = slugResult.Value;
Console.WriteLine(slug.Value); // "my-company-2024"

// Utilisation dans URL
var url = $"/api/tenants/{slug}"; // Conversion implicite
```

**Validation** :
```csharp
Slug.Create("")                     // ❌ Error.Validation.Required
Slug.Create("a".PadRight(150))      // ❌ Error.Validation.TooLong
Slug.Create("My Slug!")             // ❌ Format invalide
Slug.Create("my-valid-slug")        // ✅ Success
```

## Utilisation dans les Entities

Les Entities utilisent des Value Objects au lieu de primitives :

```csharp
// ❌ AVANT - Primitive Obsession
public class User
{
    public string Email { get; private set; } // Pas de validation garantie
    
    public void UpdateEmail(string newEmail)
    {
        // Validation dispersée dans l'application
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentException(...);
        
        Email = newEmail.ToLowerInvariant();
    }
}

// ✅ APRÈS - Value Object
public class User
{
    public Email Email { get; private set; } // Toujours valide
    
    public Result UpdateEmail(Email newEmail)
    {
        // Validation déjà faite à la création du Value Object
        Email = newEmail;
        return Result.Success();
    }
}

// Utilisation
var emailResult = Email.Create("test@example.com");
if (emailResult.IsFailure)
    return emailResult.Error;

var updateResult = user.UpdateEmail(emailResult.Value);
```

**Avantages** :
- Validation centralisée dans le Value Object
- Type safety : impossible de passer une string invalide
- Pas de duplication de code de validation
- Intention claire dans le code

## Tests unitaires

### Tester la validation

```csharp
using NFluent;
using Xunit;

public class EmailTests
{
    [Fact]
    public void Create_ValidEmail_ReturnsSuccess()
    {
        // Act
        var result = Email.Create("test@example.com");
        
        // Assert
        Check.That(result.IsSuccess).IsTrue();
        Check.That(result.Value.Value).IsEqualTo("test@example.com");
    }
    
    [Fact]
    public void Create_EmptyEmail_ReturnsValidationError()
    {
        // Act
        var result = Email.Create("");
        
        // Assert
        Check.That(result.IsFailure).IsTrue();
        Check.That(result.Error.Code).IsEqualTo("Validation.Required");
    }
    
    [Fact]
    public void Create_TooLongEmail_ReturnsValidationError()
    {
        // Arrange
        var longEmail = new string('a', 300) + "@example.com";
        
        // Act
        var result = Email.Create(longEmail);
        
        // Assert
        Check.That(result.IsFailure).IsTrue();
        Check.That(result.Error.Code).IsEqualTo("Validation.TooLong");
    }
    
    [Fact]
    public void Create_InvalidFormat_ReturnsValidationError()
    {
        // Act
        var result = Email.Create("notanemail");
        
        // Assert
        Check.That(result.IsFailure).IsTrue();
        Check.That(result.Error.Code).IsEqualTo("Validation.InvalidEmail");
    }
}
```

### Tester l'égalité

```csharp
public class EmailEqualityTests
{
    [Fact]
    public void Equals_SameValueDifferentCase_ReturnsTrue()
    {
        // Arrange
        var email1 = Email.Create("Test@Example.COM").Value;
        var email2 = Email.Create("test@example.com").Value;
        
        // Assert
        Check.That(email1).IsEqualTo(email2);
        Check.That(email1 == email2).IsTrue();
    }
    
    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com").Value;
        var email2 = Email.Create("test2@example.com").Value;
        
        // Assert
        Check.That(email1).IsNotEqualTo(email2);
        Check.That(email1 != email2).IsTrue();
    }
    
    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHashCode()
    {
        // Arrange
        var email1 = Email.Create("test@example.com").Value;
        var email2 = Email.Create("TEST@EXAMPLE.COM").Value;
        
        // Assert
        Check.That(email1.GetHashCode()).IsEqualTo(email2.GetHashCode());
    }
}
```

### Tester l'immutabilité

```csharp
public class EmailImmutabilityTests
{
    [Fact]
    public void Email_IsImmutable_CannotBeModified()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var originalValue = email.Value;
        
        // Act
        // Aucune méthode pour modifier l'email
        // email.Value = "new"; // ❌ Compilation error - no setter
        
        // Assert
        Check.That(email.Value).IsEqualTo(originalValue);
    }
    
    [Fact]
    public void Email_UsedAsDictionaryKey_StaysStable()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var dict = new Dictionary<Email, int> { [email] = 42 };
        
        // Act & Assert
        Check.That(dict[email]).IsEqualTo(42);
        Check.That(dict.ContainsKey(email)).IsTrue();
    }
}
```

## Bonnes pratiques

### ✅ À FAIRE

1. **Constructeur privé + méthode factory publique**
   ```csharp
   private Email(string value) { ... }
   public static Result<Email> Create(string value) { ... }
   ```

2. **Propriétés read-only uniquement**
   ```csharp
   public string Value { get; } // ✅ Pas de setter
   ```

3. **Validation complète dans Create()**
   ```csharp
   public static Result<Email> Create(string value)
   {
       // Toutes les validations ici
       if (validation fails) return Error;
       return new Email(value);
   }
   ```

4. **Utiliser Value Objects dans les Entities**
   ```csharp
   public class User
   {
       public Email Email { get; private set; } // ✅ Value Object
   }
   ```

5. **Implémenter GetEqualityComponents()**
   ```csharp
   protected override IEnumerable<object> GetEqualityComponents()
   {
       yield return Value;
       yield return OtherComponent; // Si plusieurs composants
   }
   ```

### ❌ À ÉVITER

1. **Constructeur public**
   ```csharp
   // ❌ Permet de créer instances invalides
   public Email(string value) { Value = value; }
   ```

2. **Propriétés mutables**
   ```csharp
   // ❌ Brise l'immutabilité
   public string Value { get; set; }
   ```

3. **Validation dans le setter**
   ```csharp
   // ❌ Pas de setters dans Value Objects
   public string Value 
   { 
       get => _value;
       set => _value = Validate(value);
   }
   ```

4. **Exposer des primitives partout**
   ```csharp
   // ❌ Primitive Obsession
   public class User
   {
       public string Email { get; set; } // Devrait être Email
   }
   ```

5. **Logique métier dans les setters d'Entity**
   ```csharp
   // ❌ Duplication de validation
   public void UpdateEmail(string email)
   {
       if (!email.Contains('@')) throw...; // Validation déjà dans Email.Create
   }
   ```

## Patterns avancés

### Value Objects multi-composants

```csharp
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    
    private Address(string street, string city, string postalCode)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
    }
    
    public static Result<Address> Create(string street, string city, string postalCode)
    {
        // Validations...
        return new Address(street, city, postalCode);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
    }
}
```

### Opérations sur Value Objects

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return new Error("Money.NegativeAmount", "...");
        
        if (string.IsNullOrWhiteSpace(currency))
            return Error.Validation.Required(nameof(currency));
        
        return new Money(amount, currency);
    }
    
    // Opérations mathématiques retournent nouveaux Value Objects
    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
            return new Error("Money.CurrencyMismatch", "...");
        
        return Create(Amount + other.Amount, Currency);
    }
    
    public Result<Money> Multiply(decimal factor)
    {
        return Create(Amount * factor, Currency);
    }
}
```

## Migration progressive

**Étape 1** : Créer le Value Object
```csharp
public sealed class Email : ValueObject { ... }
```

**Étape 2** : Utiliser dans les nouvelles Entities
```csharp
public class NewUser
{
    public Email Email { get; private set; }
}
```

**Étape 3** : Migrer les Entities existantes
```csharp
// Avant
public class User
{
    public string Email { get; private set; }
}

// Après
public class User
{
    public Email Email { get; private set; }
    
    // Backward compatibility temporaire
    [Obsolete("Use Email property instead")]
    public string EmailValue => Email.Value;
}
```

**Étape 4** : Mettre à jour les appels
```csharp
// Avant
var user = new User { Email = "test@example.com" };

// Après
var emailResult = Email.Create("test@example.com");
if (emailResult.IsFailure) return emailResult.Error;
var user = new User { Email = emailResult.Value };
```

## Références

- **ADR-024** : [Value Objects](../../../docs/adr/024-value-objects.adr.md)
- **ADR-023** : [Result Pattern](../../../docs/adr/023-result-pattern.adr.md)
- **ADR-015** : [Immutability](../../../docs/adr/015-immutability.adr.md)
- **DDD** : Eric Evans - "Domain-Driven Design"

---

**Date de création** : 2025-12-22  
**Dernière mise à jour** : 2025-12-22  
**Auteur** : LLMProxy Team
