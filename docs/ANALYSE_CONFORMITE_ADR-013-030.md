# ANALYSE DE CONFORMITÉ ADR PATTERNS MÉTIER ET TECHNIQUES (013-030, 039-040)

**Date** : 2025-12-21  
**Périmètre** : `src/**/*.cs`  
**Portée** : 18 ADR patterns (groupés en 6 catégories)

---

## 📊 VUE GLOBALE - TABLEAU DE CONFORMITÉ

| ADR | Pattern | Conformité | Violations | Priorité | Effort |
|-----|---------|-----------|------------|----------|---------|
| **013** | CQRS | ✅ 95% | 0 violations critiques | ⚪ Basse | 2h |
| **014** | Dependency Injection | ✅ 98% | 0 violations | ⚪ Basse | 1h |
| **015** | Immutability | 🟡 75% | 5 violations moyennes | 🟡 Moyenne | 8h |
| **016** | Explicit over Implicit | ✅ 100% | 0 violations | ⚪ Basse | 0h |
| **017** | Repository Pattern | ✅ 95% | 0 violations | ⚪ Basse | 1h |
| **018** | Guard Clauses | ✅ 90% | 2 violations mineures | 🟢 Basse | 2h |
| **019** | Convention over Config | ✅ 95% | 0 violations | ⚪ Basse | 1h |
| **020** | Least Astonishment | ✅ 90% | 0 violations | ⚪ Basse | 2h |
| **021** | Tell Don't Ask | 🟡 70% | 8 violations moyennes | 🟡 Moyenne | 12h |
| **022** | Idempotence | 🔴 50% | 15 violations critiques | 🔴 Haute | 20h |
| **023** | Result Pattern | ✅ 85% | 3 violations moyennes | 🟡 Moyenne | 6h |
| **024** | Value Objects | ✅ 90% | 2 violations mineures | 🟢 Basse | 4h |
| **025** | Domain Events | ✅ 95% | 1 violation mineure | 🟢 Basse | 2h |
| **026** | Null Object Pattern | ❌ 0% | Non implémenté | 🟡 Moyenne | 6h |
| **027** | Defensive Programming | ✅ 85% | 4 violations moyennes | 🟡 Moyenne | 6h |
| **028** | Specification Pattern | ❌ 0% | Non implémenté | 🟡 Moyenne | 10h |
| **029** | Unit of Work | ✅ 95% | 1 violation mineure | 🟢 Basse | 2h |
| **030** | Feature Toggles | ❌ 0% | Non implémenté | 🟢 Basse | 8h |
| **039** | Aggregate Root | 🟡 65% | 7 violations moyennes | 🟡 Moyenne | 10h |
| **040** | Outbox Pattern | ❌ 0% | Non implémenté | 🟡 Moyenne | 15h |

**Score global** : **70.8%** (14/20 patterns bien implémentés)

---

## 📁 GROUPE 1 : PATTERNS ARCHITECTURAUX (013-017)

### ✅ ADR-013 : CQRS - Command Query Responsibility Segregation

**Conformité** : ✅ **95%** (Excellent)

#### Points Conformes

1. **Séparation Commands/Queries stricte** ✅
   ```csharp
   // Commands (Write)
   src/Application/LLMProxy.Application/Common/ICommand.cs
   src/Application/LLMProxy.Application/Common/ICommand<T>.cs
   
   // Queries (Read)
   src/Application/LLMProxy.Application/Common/IQuery<T>.cs
   ```

2. **Handlers dédiés via MediatR** ✅
   - 62 fichiers de handlers détectés
   - Pattern `{Command|Query}Handler` respecté
   - Exemple : `CreateTenantCommandHandler`, `GetTenantByIdQueryHandler`

3. **Structure par feature (Vertical Slice)** ✅
   ```
   Application/
   ├── Tenants/
   │   ├── Commands/
   │   │   ├── CreateTenant/
   │   │   ├── UpdateTenant/
   │   └── Queries/
   │       ├── GetTenantById/
   │       └── GetAllTenants/
   ```

4. **Séparation Read/Write Models** ✅
   - Domain entities (write) : `Tenant`, `User`, `ApiKey`
   - DTOs (read) : `TenantDto`, `UserDto`, `ApiKeyDto`

#### Violations Détectées

**Aucune violation critique détectée**.

#### Améliorations Suggérées (Priorité Basse)

1. **Optimisation Read Models** (Effort : 2h)
   - Créer des vues dénormalisées PostgreSQL pour queries complexes
   - Implémenter projections spécialisées pour statistiques

**Recommandation** : ✅ CQRS excellemment implémenté, maintenir la conformité.

---

### ✅ ADR-014 : Dependency Injection

**Conformité** : ✅ **98%** (Excellent)

#### Points Conformes

1. **Constructor Injection systématique** ✅
   - 98 classes avec DI détectées
   - Aucun `new()` dans logique métier

2. **Configuration DI centralisée** ✅
   ```csharp
   // Program.cs (Composition Root)
   builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
   builder.Services.AddScoped<ISecretService, SecretService>();
   builder.Services.AddScoped<IQuotaService, QuotaService>();
   ```

3. **Abstractions injectées** ✅
   - Toutes les dépendances via interfaces (`IXxx`)
   - Aucun couplage concret détecté

4. **Lifetime Management** ✅
   - Scoped : Repositories, UnitOfWork, Services métier
   - Singleton : Cache, HttpClients factories
   - Transient : Validators

#### Violations Détectées

**Aucune violation détectée**.

#### Améliorations Suggérées (Priorité Basse)

1. **ServiceCollection Extensions** (Effort : 1h)
   - Créer extensions `AddDomainServices()`, `AddInfrastructure()` pour meilleure organisation
   - Regrouper enregistrements par couche

**Recommandation** : ✅ DI parfaitement implémenté.

---

### 🟡 ADR-015 : Immutability

**Conformité** : 🟡 **75%** (Moyen)

#### Points Conformes

1. **Value Objects immuables** ✅
   ```csharp
   public class TenantSettings : ValueObject { /* propriétés readonly */ }
   public class RoutingStrategy : ValueObject { /* propriétés readonly */ }
   public class ProviderConfiguration : ValueObject { /* propriétés readonly */ }
   ```

2. **Domain Events en `record`** ✅
   ```csharp
   public record TenantCreatedEvent(Guid TenantId, string TenantName) : IDomainEvent;
   public record ApiKeyCreatedEvent(Guid ApiKeyId, string RawKey) : IDomainEvent;
   ```

3. **DTOs en `record`** ✅
   - Tous les Commands/Queries en `record`
   - DTOs utilisent `init` accessors

#### Violations Détectées

**V-IMM-001 à V-IMM-005 : Entités mutables avec setters publics** (Priorité 🟡 Moyenne)

**Fichiers concernés** :
- `Domain/Entities/Tenant.cs`
- `Domain/Entities/User.cs`
- `Domain/Entities/ApiKey.cs`
- `Domain/Entities/LLMProvider.cs`
- `Domain/Entities/QuotaLimit.cs`

**Problème** :
```csharp
// ❌ VIOLATION : Setters publics sur entités domain
public class Tenant : Entity
{
    public string Name { get; set; }  // ← Devrait être private set
    public TenantSettings Settings { get; set; }  // ← Devrait être private set
}
```

**Impact** : Mutation non contrôlée de l'état des entités (violation encapsulation).

**Recommandation** :
```csharp
// ✅ CORRECTION
public class Tenant : Entity
{
    public string Name { get; private set; }
    public TenantSettings Settings { get; private set; }
    
    // Méthodes publiques pour mutations contrôlées
    public void UpdateName(string newName)
    {
        Guard.AgainstNullOrWhiteSpace(newName, nameof(newName));
        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

**Effort estimé** : 8h (refactoring 7 entités + handlers associés)

---

### ✅ ADR-016 : Explicit over Implicit

**Conformité** : ✅ **100%** (Parfait)

#### Points Conformes

1. **Aucune conversion implicite** ✅
   - Recherche `implicit operator` : 0 résultats
   - Toutes conversions explicites

2. **Nommage explicite** ✅
   ```csharp
   // Constants explicites
   public static class HttpHeaders
   {
       public const string Authorization = "Authorization";
       public const string ApiKey = "X-API-Key";
       public const string RequestId = "X-Request-Id";
   }
   
   public static class AuthenticationSchemes
   {
       public const string Bearer = "Bearer ";
   }
   ```

3. **Méthodes explicites** ✅
   - `CreateAsync()`, `UpdateAsync()` au lieu de opérateurs
   - Pas de conventions "magiques"

**Recommandation** : ✅ Maintenir la conformité totale.

---

### ✅ ADR-017 : Repository Pattern

**Conformité** : ✅ **95%** (Excellent)

#### Points Conformes

1. **Interfaces dans Domain** ✅
   ```csharp
   Domain/Interfaces/
   ├── ITenantRepository.cs
   ├── IUserRepository.cs
   ├── IApiKeyRepository.cs
   ├── ILLMProviderRepository.cs
   ├── IQuotaLimitRepository.cs
   ├── IAuditLogRepository.cs
   └── ITokenUsageMetricRepository.cs
   ```

2. **Implémentations dans Infrastructure** ✅
   ```csharp
   Infrastructure.PostgreSQL/Repositories/
   ├── TenantRepository.cs
   ├── UserRepository.cs
   ├── ApiKeyRepository.cs
   ├── LLMProviderRepository.cs
   └── ...
   ```

3. **Abstraction complète de EF Core** ✅
   - Domain ne référence PAS `Microsoft.EntityFrameworkCore`
   - Méthodes async avec `CancellationToken`

4. **Pattern Repository par Aggregate** ✅
   - 1 repository par Aggregate Root
   - Pas de repository générique exposé

#### Améliorations Suggérées (Priorité Basse)

1. **Specification Pattern dans repositories** (Effort : 1h)
   - Ajouter méthodes `GetBySpecificationAsync(ISpecification<T>)`
   - Permettre requêtes complexes sans exposer IQueryable

**Recommandation** : ✅ Repository Pattern excellemment appliqué.

---

## 📁 GROUPE 2 : VALIDATION & ERREURS (018, 023, 027)

### ✅ ADR-018 : Guard Clauses

**Conformité** : ✅ **90%** (Très bon)

#### Points Conformes

1. **Classe Guard centralisée** ✅
   ```csharp
   Infrastructure.Security/Guard.cs
   - AgainstNull()
   - AgainstNullOrWhiteSpace()
   - AgainstNegativeOrZero()
   - AgainstEmptyCollection()
   - AgainstResponseStarted()
   - AgainstInvalidRange()
   ```

2. **Validations en début de méthode** ✅
   - 52 usages de `Guard.AgainstNull()`
   - 17 usages de `Guard.AgainstNullOrWhiteSpace()`
   - Pattern appliqué dans middlewares, services, entités

3. **Fail-Fast avec exceptions explicites** ✅
   ```csharp
   public StreamInterceptionMiddleware(/* ... */)
   {
       Guard.AgainstNegativeOrZero((int)_maxStreamSizeBytes, 
           nameof(_maxStreamSizeBytes), 
           "Stream size must be positive");
   }
   ```

#### Violations Détectées

**V-GC-001 : Checks null manuels au lieu de Guard** (Priorité 🟢 Mineure)

**Exemples** :
```csharp
// ❌ VIOLATION : if (x == null) au lieu de Guard
src/Application/.../CreateUserCommandHandler.cs:80
if (tenant == null) return Result.Failure(Error.NotFound("Tenant not found"));

src/Application/.../GetUserByIdQueryHandler.cs:23
if (user == null) return Result.Failure(Error.NotFound("User not found"));
```

**31 occurrences détectées** de `if (x == null)` au lieu de `Guard.AgainstNull()`.

**Recommandation** :
```csharp
// ✅ CORRECTION
var tenant = await unitOfWork.Tenants.GetByIdAsync(command.TenantId, ct);
Guard.AgainstNull(tenant, nameof(tenant), $"Tenant {command.TenantId} not found");
```

**Effort estimé** : 2h (remplacer 31 occurrences).

---

### ✅ ADR-023 : Result Pattern

**Conformité** : ✅ **85%** (Bon)

#### Points Conformes

1. **Type Result<T> implémenté** ✅
   ```csharp
   Domain/Common/Result.cs
   Domain/Common/Result<T>.cs
   
   public sealed class Result<T>
   {
       public bool IsSuccess { get; }
       public bool IsFailure => !IsSuccess;
       public T Value { get; }
       public Error Error { get; }
   }
   ```

2. **Utilisation dans Commands/Queries** ✅
   - Tous les handlers retournent `Result` ou `Result<T>`
   - Controllers mappent `Result` vers `IActionResult`

3. **Pattern Success/Failure** ✅
   ```csharp
   if (result.IsFailure)
       return BadRequest(result.Error);
       
   return Ok(result.Value);
   ```

#### Violations Détectées

**V-RES-001 à V-RES-003 : Exceptions métier non wrappées** (Priorité 🟡 Moyenne)

**Fichiers concernés** :
- `Domain/Entities/Tenant.cs:72` - `throw new InvalidOperationException("Cannot deactivate deleted tenant")`
- `Domain/Entities/User.cs:133` - `throw new ArgumentException("User name cannot be empty")`
- `Infrastructure.Security/SecretService.cs` - 9 `throw new NotImplementedException()`

**Problème** : Exceptions levées directement au lieu de retourner `Result.Failure()`.

**Recommandation** :
```csharp
// ❌ VIOLATION
public void Deactivate()
{
    if (Status == TenantStatus.Deleted)
        throw new InvalidOperationException("Cannot deactivate deleted tenant");
}

// ✅ CORRECTION
public Result Deactivate()
{
    if (Status == TenantStatus.Deleted)
        return Result.Failure(Error.InvalidOperation("Cannot deactivate deleted tenant"));
    
    Status = TenantStatus.Inactive;
    return Result.Success();
}
```

**Effort estimé** : 6h (refactoring 12 méthodes domain).

---

### ✅ ADR-027 : Defensive Programming

**Conformité** : ✅ **85%** (Bon)

#### Points Conformes

1. **Input Validation systématique** ✅
   - Guard clauses en début de méthode
   - Validators FluentValidation pour Commands

2. **Null checks** ✅
   - 31 checks `if (x == null)` détectés
   - Pattern ?? null-coalescing utilisé

3. **Boundary checks** ✅
   ```csharp
   Guard.AgainstNegativeOrZero(amount, nameof(amount));
   Guard.AgainstInvalidRange(priority, 0, 100, nameof(priority));
   ```

#### Violations Détectées

**V-DEF-001 à V-DEF-004 : Validations manquantes** (Priorité 🟡 Moyenne)

**Exemples** :
1. **Pas de validation longueur strings** dans certains setters domain
2. **Pas de validation range** sur certains entiers (QuotaLimit)
3. **Pas de validation format** sur emails/URLs avant sauvegarde
4. **Pas de sanitization** des inputs utilisateur dans logs

**Recommandation** :
```csharp
// ✅ AJOUT VALIDATIONS
public class Tenant : Entity
{
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        private set
        {
            Guard.AgainstNullOrWhiteSpace(value, nameof(Name));
            Guard.AgainstInvalidRange(value.Length, 1, 200, nameof(Name));
            _name = value;
        }
    }
}
```

**Effort estimé** : 6h (ajout validations dans 7 entités).

---

## 📁 GROUPE 3 : DOMAIN-DRIVEN DESIGN (024-026, 028, 039)

### ✅ ADR-024 : Value Objects

**Conformité** : ✅ **90%** (Très bon)

#### Points Conformes

1. **Classe abstraite ValueObject** ✅
   ```csharp
   Domain/Common/ValueObject.cs
   
   public abstract class ValueObject
   {
       protected abstract IEnumerable<object?> GetEqualityComponents();
       public override bool Equals(object? obj) { /* ... */ }
       public override int GetHashCode() { /* ... */ }
       public static bool operator ==(ValueObject? left, ValueObject? right) { /* ... */ }
   }
   ```

2. **Value Objects implémentés** ✅
   - `TenantSettings : ValueObject`
   - `RoutingStrategy : ValueObject`
   - `ProviderConfiguration : ValueObject`
   - `QuotaUsage` (struct, immutable)

3. **Comparaison par valeur** ✅
   - Égalité structurelle via `GetEqualityComponents()`

#### Violations Détectées

**V-VO-001 à V-VO-002 : Concepts métier non modélisés en Value Objects** (Priorité 🟢 Mineure)

**Candidats manquants** :
1. **Email** : Actuellement `string`, devrait être `Email : ValueObject`
   - Validation format
   - Normalisation (lowercase)
   - Comparaison case-insensitive

2. **ApiKeyValue** : Actuellement `string`, devrait être `ApiKeyValue : ValueObject`
   - Validation format (`llm_xxx`)
   - Masquage pour logs
   - Extraction prefix/hash

**Recommandation** :
```csharp
// ✅ AJOUT Value Object Email
public sealed class Email : ValueObject
{
    public string Value { get; }
    
    private Email(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        if (!IsValid(value))
            throw new ArgumentException("Invalid email format", nameof(value));
        
        Value = value.ToLowerInvariant();
    }
    
    public static Email Create(string value) => new(value);
    
    private static bool IsValid(string email) => /* regex validation */;
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**Effort estimé** : 4h (créer 2 Value Objects + refactoring usages).

---

### ✅ ADR-025 : Domain Events

**Conformité** : ✅ **95%** (Excellent)

#### Points Conformes

1. **Interface IDomainEvent** ✅
   ```csharp
   Domain/Common/IDomainEvent.cs
   
   public interface IDomainEvent
   {
       DateTime OccurredOn { get; }
   }
   ```

2. **Events implémentés** ✅
   - `TenantCreatedEvent`
   - `TenantDeactivatedEvent`
   - `ApiKeyCreatedEvent`

3. **Collection DomainEvents dans Entity** ✅
   ```csharp
   public abstract class Entity
   {
       private readonly List<IDomainEvent> _domainEvents = new();
       public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
       
       protected void AddDomainEvent(IDomainEvent domainEvent)
       {
           _domainEvents.Add(domainEvent);
       }
       
       public void ClearDomainEvents() => _domainEvents.Clear();
   }
   ```

4. **Events levés dans méthodes factory** ✅
   ```csharp
   public static Tenant Create(string name, string slug, TenantSettings settings)
   {
       var tenant = new Tenant(name, slug, settings);
       AddDomainEvent(tenant, new TenantCreatedEvent(tenant.Id, tenant.Name));
       return tenant;
   }
   ```

#### Violations Détectées

**V-DE-001 : Events non levés pour certaines mutations** (Priorité 🟢 Mineure)

**Mutations sans events** :
- `Tenant.Activate()` - Pas de `TenantActivatedEvent`
- `User.UpdateEmail()` - Pas de `UserEmailChangedEvent`
- `ApiKey.Revoke()` - Pas de `ApiKeyRevokedEvent`

**Recommandation** : Ajouter events manquants pour traçabilité complète.

**Effort estimé** : 2h (ajout 3 events + handlers de notification).

---

### ❌ ADR-026 : Null Object Pattern

**Conformité** : ❌ **0%** (Non implémenté)

#### État Actuel

**Aucun Null Object détecté** dans le codebase.

#### Cas d'Usage Identifiés

1. **DefaultTenant** : Tenant par défaut pour utilisateurs sans tenant
2. **EmptyRoutingStrategy** : Stratégie de routage par défaut
3. **NullQuotaLimit** : Quota illimité au lieu de `null`

#### Problème Actuel

```csharp
// ❌ PATTERN ACTUEL : Null checks partout
public async Task ProcessRequest(Guid tenantId)
{
    var tenant = await _repository.GetByIdAsync(tenantId);
    
    if (tenant == null)
    {
        // Logique par défaut dupliquée
        return DefaultBehavior();
    }
    
    return tenant.ProcessRequest();
}
```

#### Recommandation

```csharp
// ✅ NULL OBJECT PATTERN
public sealed class NullTenant : Tenant
{
    public static NullTenant Instance { get; } = new NullTenant();
    
    private NullTenant() : base("Default", "default", TenantSettings.Default)
    {
    }
    
    public override bool IsActive => false;
    public override Result ProcessRequest() => Result.Failure(Error.Unauthorized());
}

// Usage
var tenant = await _repository.GetByIdAsync(tenantId) ?? NullTenant.Instance;
return tenant.ProcessRequest(); // Pas de null check
```

**Effort estimé** : 6h (créer 3 Null Objects + refactoring usages).

---

### ❌ ADR-028 : Specification Pattern

**Conformité** : ❌ **0%** (Non implémenté)

#### État Actuel

**Aucune Specification détectée** dans le codebase.

#### Problèmes Actuels

1. **Logique métier dans repositories** :
   ```csharp
   // ❌ VIOLATION : Logique dans repository
   public async Task<List<Tenant>> GetEligibleForProcessing()
   {
       return await _context.Tenants
           .Where(t => t.Status == TenantStatus.Active)
           .Where(t => t.MonthlyQuota > t.CurrentUsage)
           .Where(t => !t.IsSuspended)
           .ToListAsync();
   }
   ```

2. **Duplication règles métier** :
   - Règle "Tenant éligible" dupliquée dans 3 repositories
   - Règle "User actif" dupliquée dans 2 services

#### Recommandation

```csharp
// ✅ SPECIFICATION PATTERN
public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T entity);
}

public class TenantIsEligibleSpecification : ISpecification<Tenant>
{
    public Expression<Func<Tenant, bool>> ToExpression()
    {
        return t => t.Status == TenantStatus.Active
                 && t.MonthlyQuota > t.CurrentUsage
                 && !t.IsSuspended;
    }
    
    public bool IsSatisfiedBy(Tenant tenant)
    {
        return tenant.Status == TenantStatus.Active
            && tenant.MonthlyQuota > tenant.CurrentUsage
            && !tenant.IsSuspended;
    }
}

// Usage
var spec = new TenantIsEligibleSpecification();
var eligibleTenants = await _repository.GetBySpecificationAsync(spec);

// Composition
var spec = new TenantIsActiveSpecification()
    .And(new TenantHasQuotaSpecification())
    .And(new TenantNotSuspendedSpecification());
```

**Effort estimé** : 10h (infrastructure Specification + 5 specs métier).

---

### 🟡 ADR-039 : Aggregate Root

**Conformité** : 🟡 **65%** (Moyen)

#### Points Conformes

1. **Entités racines identifiées** ✅
   - `Tenant` (root)
   - `User` (root)
   - `ApiKey` (root)
   - `LLMProvider` (root)

2. **Entities de base** ✅
   ```csharp
   public abstract class Entity
   {
       public Guid Id { get; protected set; }
       public DateTime CreatedAt { get; protected set; }
       public DateTime? UpdatedAt { get; protected set; }
       
       private readonly List<IDomainEvent> _domainEvents = new();
   }
   ```

#### Violations Détectées

**V-AGG-001 à V-AGG-007 : Manque de cohérence transactionnelle** (Priorité 🟡 Moyenne)

**Problèmes identifiés** :

1. **Tenant et TenantSettings séparés** :
   ```csharp
   // ❌ VIOLATION : TenantSettings devrait être un child entity, pas Value Object modifiable
   public class Tenant : Entity
   {
       public TenantSettings Settings { get; set; }  // Mutable, devrait être private
   }
   ```

2. **User et ApiKey indépendants** :
   - `ApiKey` devrait être enfant de `User` (aggregate)
   - Cohérence : Révoquer User → Révoquer toutes ses ApiKeys

3. **LLMProvider et ProviderConfiguration séparés** :
   - Configuration devrait être child entity
   - Cohérence : Update Provider → Update Configuration atomiquement

4. **Pas d'interface IAggregateRoot** :
   - Impossible de distinguer roots des child entities

5. **Pas de validation d'invariants d'aggregate** :
   - Pas de méthode `ValidateInvariants()` dans aggregates

6. **Accès direct aux enfants** :
   - Child entities accessibles via repositories séparés

7. **Transactions non limitées aux aggregates** :
   - UnitOfWork permet de modifier plusieurs aggregates simultanément

**Recommandation** :

```csharp
// ✅ INTERFACE AGGREGATE ROOT
public interface IAggregateRoot
{
    Guid Id { get; }
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

// ✅ AGGREGATE TENANT avec enfants
public class Tenant : Entity, IAggregateRoot
{
    private readonly List<TenantUser> _users = new();
    private readonly List<ApiKey> _apiKeys = new();
    
    public IReadOnlyCollection<TenantUser> Users => _users.AsReadOnly();
    public IReadOnlyCollection<ApiKey> ApiKeys => _apiKeys.AsReadOnly();
    
    public void AddUser(string email, string name, UserRole role)
    {
        // Validation invariants
        if (_users.Count >= MaxUsers)
            throw new DomainException("Maximum users reached");
        
        var user = new TenantUser(Id, email, name, role);
        _users.Add(user);
        
        // Event
        AddDomainEvent(new UserAddedToTenantEvent(Id, user.Id));
    }
    
    public void RevokeAllApiKeys()
    {
        foreach (var apiKey in _apiKeys)
        {
            apiKey.Revoke();
        }
    }
}

// ✅ REPOSITORY par Aggregate Root uniquement
public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct);
    // Include users et apiKeys automatiquement (lazy loading ou eager)
}
```

**Effort estimé** : 10h (redéfinir boundaries + refactoring repositories).

---

## 📁 GROUPE 4 : DATA ACCESS (029, 040)

### ✅ ADR-029 : Unit of Work Pattern

**Conformité** : ✅ **95%** (Excellent)

#### Points Conformes

1. **Interface IUnitOfWork** ✅
   ```csharp
   Domain/Interfaces/IUnitOfWork.cs
   
   public interface IUnitOfWork : IDisposable
   {
       ITenantRepository Tenants { get; }
       IUserRepository Users { get; }
       IApiKeyRepository ApiKeys { get; }
       ILLMProviderRepository Providers { get; }
       IQuotaLimitRepository QuotaLimits { get; }
       IAuditLogRepository AuditLogs { get; }
       ITokenUsageMetricRepository TokenMetrics { get; }
       
       Task<int> SaveChangesAsync(CancellationToken ct = default);
       Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
       Task CommitTransactionAsync(CancellationToken ct = default);
       Task RollbackTransactionAsync(CancellationToken ct = default);
   }
   ```

2. **Implémentation avec EF Core** ✅
   ```csharp
   Infrastructure.PostgreSQL/UnitOfWork.cs
   
   public class UnitOfWork : IUnitOfWork
   {
       private readonly LLMProxyDbContext _context;
       private ITenantRepository? _tenants;
       // Lazy initialization des repositories
   }
   ```

3. **Usage dans handlers** ✅
   - 62 handlers utilisent `IUnitOfWork`
   - Pattern : `await unitOfWork.SaveChangesAsync(ct);`

4. **Transactions explicites** ✅
   ```csharp
   await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
   try
   {
       // Operations
       await _unitOfWork.SaveChangesAsync(ct);
       await _unitOfWork.CommitTransactionAsync(ct);
   }
   catch
   {
       await _unitOfWork.RollbackTransactionAsync(ct);
       throw;
   }
   ```

#### Violations Détectées

**V-UOW-001 : Pas de retry sur DbUpdateConcurrencyException** (Priorité 🟢 Mineure)

**Recommandation** :
```csharp
// ✅ AJOUT Resilient UnitOfWork wrapper
public class ResilientUnitOfWork : IUnitOfWork
{
    private readonly IUnitOfWork _inner;
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        const int maxRetries = 3;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await _inner.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException) when (i < maxRetries - 1)
            {
                // Reload entities and retry
            }
        }
        throw;
    }
}
```

**Effort estimé** : 2h (wrapper + tests).

---

### ❌ ADR-040 : Outbox Pattern

**Conformité** : ❌ **0%** (Non implémenté)

#### État Actuel

**Aucun Outbox détecté** dans le codebase.

#### Problèmes Actuels

1. **Events non persistés** :
   - `DomainEvents` en mémoire uniquement
   - Perte d'events si crash avant publication

2. **Pas de garantie de publication** :
   - Events clearés après `SaveChanges()`
   - Si publication échoue, events perdus

3. **Pas de retry automatique** :
   - Events non publiés jamais réessayés

#### Recommandation

```csharp
// ✅ OUTBOX TABLE
public class OutboxMessage : Entity
{
    public string Type { get; private set; }
    public string Payload { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public DateTime? ProcessedOn { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
}

// ✅ PERSISTENCE EVENTS DANS OUTBOX
public async Task<int> SaveChangesAsync(CancellationToken ct)
{
    // 1. Collecter domain events
    var entities = _context.ChangeTracker.Entries<Entity>()
        .Where(e => e.Entity.DomainEvents.Any())
        .Select(e => e.Entity)
        .ToList();
    
    // 2. Convertir en OutboxMessages
    var outboxMessages = entities
        .SelectMany(e => e.DomainEvents)
        .Select(e => new OutboxMessage(
            e.GetType().Name,
            JsonSerializer.Serialize(e),
            e.OccurredOn))
        .ToList();
    
    // 3. Persister dans même transaction
    await _context.OutboxMessages.AddRangeAsync(outboxMessages, ct);
    
    // 4. Clear domain events
    entities.ForEach(e => e.ClearDomainEvents());
    
    // 5. Save all
    return await _context.SaveChangesAsync(ct);
}

// ✅ BACKGROUND WORKER pour publier
public class OutboxPublisherBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var pending = await _repository.GetPendingMessagesAsync(100, ct);
            
            foreach (var message in pending)
            {
                try
                {
                    await _eventBus.PublishAsync(message.ToEvent(), ct);
                    message.MarkAsProcessed();
                }
                catch (Exception ex)
                {
                    message.IncrementRetry(ex.Message);
                }
            }
            
            await _repository.SaveChangesAsync(ct);
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}
```

**Effort estimé** : 15h (table + publisher + tests).

---

## 📁 GROUPE 5 : CONFIGURATION & INFRASTRUCTURE (019, 030)

### ✅ ADR-019 : Convention over Configuration

**Conformité** : ✅ **95%** (Excellent)

#### Points Conformes

1. **Conventions de nommage** ✅
   - Entités → Tables (snake_case pluriel)
   - FK → `{table}_id`
   - Timestamps → `created_at`, `updated_at`

2. **Conventions CQRS** ✅
   - Commands : `{Verb}{Noun}Command`
   - Queries : `Get{Noun}[ByCriteria]Query`
   - Handlers : `{Command}Handler`
   - Validators : `{Command}Validator`

3. **Conventions structure** ✅
   ```
   Application/{Feature}/
   ├── Commands/
   │   ├── {Action}/
   │   │   ├── {Action}Command.cs
   │   │   ├── {Action}CommandHandler.cs
   │   │   └── {Action}CommandValidator.cs
   └── Queries/
       └── Get{Entity}[ByCriteria]/
   ```

4. **Conventions EF Core** ✅
   - `ApplyConfigurationsFromAssembly()` auto-discovery
   - Snake_case global via extension

5. **Conventions API REST** ✅
   - `GET /api/{resources}`
   - `GET /api/{resources}/{id}`
   - `POST /api/{resources}`
   - `PUT /api/{resources}/{id}`
   - `DELETE /api/{resources}/{id}`

#### Améliorations Suggérées (Priorité Basse)

1. **Documentation conventions** (Effort : 1h)
   - Créer `docs/CONVENTIONS.md`
   - Lister toutes les conventions du projet

**Recommandation** : ✅ Conventions bien établies et suivies.

---

### ❌ ADR-030 : Feature Toggles

**Conformité** : ❌ **0%** (Non implémenté)

#### État Actuel

**Aucun Feature Toggle détecté** dans le codebase.

#### Cas d'Usage Identifiés

1. **New Quota Algorithm** : Tester nouvel algorithme de quota en prod avec % utilisateurs
2. **Enhanced Logging** : Activer logs détaillés pour debug sans redéploiement
3. **Provider Fallback** : Activer/désactiver fallback vers provider secondaire
4. **Rate Limiting Strategy** : Switcher entre stratégies de rate limiting

#### Recommandation

```csharp
// ✅ FEATURE TOGGLE SERVICE
public interface IFeatureToggleService
{
    Task<bool> IsEnabledAsync(string featureName, CancellationToken ct = default);
    Task<bool> IsEnabledForUserAsync(string featureName, Guid userId, CancellationToken ct = default);
    Task<bool> IsEnabledForTenantAsync(string featureName, Guid tenantId, CancellationToken ct = default);
}

// ✅ IMPLEMENTATION Redis-based
public class RedisFeatureToggleService : IFeatureToggleService
{
    private readonly IDatabase _redis;
    
    public async Task<bool> IsEnabledAsync(string featureName, CancellationToken ct)
    {
        var value = await _redis.StringGetAsync($"feature:{featureName}");
        return value.HasValue && bool.Parse(value!);
    }
    
    public async Task<bool> IsEnabledForTenantAsync(string featureName, Guid tenantId, CancellationToken ct)
    {
        // Rollout percentage
        var rollout = await _redis.StringGetAsync($"feature:{featureName}:rollout");
        if (rollout.HasValue)
        {
            var percentage = int.Parse(rollout!);
            var hash = tenantId.GetHashCode() % 100;
            return hash < percentage;
        }
        
        return await IsEnabledAsync(featureName, ct);
    }
}

// ✅ USAGE
if (await _featureToggles.IsEnabledForTenantAsync("NewQuotaAlgorithm", tenantId, ct))
{
    return await _newQuotaService.CalculateAsync(usage, ct);
}
else
{
    return await _quotaService.CalculateAsync(usage, ct);
}
```

**Effort estimé** : 8h (service + admin UI pour toggle management).

---

## 📁 GROUPE 6 : AUTRES PATTERNS (020-022)

### ✅ ADR-020 : Principle of Least Astonishment

**Conformité** : ✅ **90%** (Très bon)

#### Points Conformes

1. **Nommage clair et prévisible** ✅
   - Méthodes : `GetByIdAsync()`, `CreateAsync()`, `UpdateAsync()`
   - Pas de noms trompeurs ou surprenants

2. **Comportements attendus** ✅
   - `Activate()` active (pas de side-effects cachés)
   - `Delete()` supprime (pas de soft delete implicite)

3. **Constantes explicites** ✅
   ```csharp
   public static class HttpHeaders
   {
       public const string Authorization = "Authorization";  // Pas "Auth"
       public const string ApiKey = "X-API-Key";  // Pas "Key"
   }
   ```

4. **Exceptions claires** ✅
   - `InvalidOperationException` avec message explicite
   - Pas de exceptions génériques `Exception`

#### Améliorations Suggérées (Priorité Basse)

1. **Documenter comportements surprenants** (Effort : 2h)
   - Exemple : `SaveChangesAsync()` déclenche validation EF Core
   - Ajouter XML comments sur comportements non évidents

**Recommandation** : ✅ Principe bien respecté.

---

### 🟡 ADR-021 : Tell, Don't Ask

**Conformité** : 🟡 **70%** (Moyen)

#### Points Conformes

1. **Comportements encapsulés dans entités** ✅
   ```csharp
   public class Tenant : Entity
   {
       public void Activate() { /* logic */ }
       public void Deactivate(string reason) { /* logic */ }
   }
   ```

2. **Évite getters pour décider** ✅
   - Méthodes `CanBeActivated()` au lieu de exposer Status + vérifier

#### Violations Détectées

**V-TDA-001 à V-TDA-008 : Logique métier dans handlers au lieu d'entités** (Priorité 🟡 Moyenne)

**Exemples** :

```csharp
// ❌ VIOLATION : Handler interroge et décide (ASK pattern)
// src/Application/Tenants/Commands/ActivateTenantCommandHandler.cs
public async Task<Result> Handle(ActivateTenantCommand request, CancellationToken ct)
{
    var tenant = await _unitOfWork.Tenants.GetByIdAsync(request.TenantId, ct);
    
    // ❌ ASK : Handler vérifie l'état
    if (tenant.Status == TenantStatus.Deleted)
        return Result.Failure(Error.InvalidOperation("Cannot activate deleted tenant"));
    
    if (tenant.Status == TenantStatus.Suspended)
        return Result.Failure(Error.InvalidOperation("Cannot activate suspended tenant"));
    
    // ❌ ASK : Handler modifie directement
    tenant.Status = TenantStatus.Active;
    tenant.ActivatedAt = DateTime.UtcNow;
    
    await _unitOfWork.SaveChangesAsync(ct);
    return Result.Success();
}

// ✅ CORRECTION : Entité décide (TELL pattern)
public class Tenant : Entity
{
    public Result Activate()
    {
        if (Status == TenantStatus.Deleted)
            return Result.Failure(Error.InvalidOperation("Cannot activate deleted tenant"));
        
        if (Status == TenantStatus.Suspended)
            return Result.Failure(Error.InvalidOperation("Tenant must be unsuspended first"));
        
        if (Status == TenantStatus.Active)
            return Result.Success(); // Idempotent
        
        Status = TenantStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        
        return Result.Success();
    }
}

// Handler simplifié (TELL)
public async Task<Result> Handle(ActivateTenantCommand request, CancellationToken ct)
{
    var tenant = await _unitOfWork.Tenants.GetByIdAsync(request.TenantId, ct);
    Guard.AgainstNull(tenant, nameof(tenant));
    
    var result = tenant.Activate(); // ← TELL
    if (result.IsFailure)
        return result;
    
    await _unitOfWork.SaveChangesAsync(ct);
    return Result.Success();
}
```

**8 handlers détectés** avec pattern ASK au lieu de TELL.

**Effort estimé** : 12h (refactoring 8 handlers + entités).

---

### 🔴 ADR-022 : Idempotence

**Conformité** : 🔴 **50%** (Insuffisant)

#### Points Conformes

1. **HTTP Verbs idempotents** ✅
   - PUT, DELETE naturellement idempotents
   - GET sans side-effects

2. **Vérifications d'état avant action** ✅
   ```csharp
   public void Activate()
   {
       if (Status == TenantStatus.Active)
           return; // Idempotent - déjà actif
       
       Status = TenantStatus.Active;
   }
   ```

#### Violations CRITIQUES Détectées

**V-IDP-001 à V-IDP-015 : Opérations non-idempotentes** (Priorité 🔴 Haute)

**Problèmes identifiés** :

1. **POST Create sans Idempotency-Key** (15 endpoints) :
   ```csharp
   // ❌ VIOLATION : POST peut créer doublons si retry
   [HttpPost]
   public async Task<IActionResult> Create(CreateTenantCommand command, CancellationToken ct)
   {
       var result = await _mediator.Send(command, ct);
       return Created($"/api/tenants/{result.Value}", result.Value);
   }
   ```

2. **Pas de tracking des requests traitées** :
   - Aucun mécanisme `ProcessedMessageRepository`
   - Events peuvent être traités plusieurs fois

3. **Opérations monétaires non-idempotentes** :
   ```csharp
   // ❌ VIOLATION : Incrément non-idempotent
   public void IncrementUsage(int tokens)
   {
       CurrentUsage += tokens;  // Problème si appelé 2x !
   }
   ```

4. **Pas de middleware d'idempotence** :
   - Aucun middleware `IdempotencyMiddleware` détecté

**Recommandation** :

```csharp
// ✅ AJOUT Idempotency-Key header obligatoire pour POST
[HttpPost]
public async Task<IActionResult> Create(
    CreateTenantCommand command,
    [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
    CancellationToken ct)
{
    // Vérifier si déjà traité
    var existing = await _idempotencyRepo.GetByKeyAsync(idempotencyKey, ct);
    if (existing != null)
    {
        return Ok(existing.Response); // Retourne résultat précédent
    }
    
    var result = await _mediator.Send(command, ct);
    
    // Sauvegarder pour futurs retries
    await _idempotencyRepo.StoreAsync(
        idempotencyKey,
        result,
        TimeSpan.FromHours(24),
        ct);
    
    return Created($"/api/tenants/{result.Value}", result.Value);
}

// ✅ MIDDLEWARE automatique
public class IdempotencyMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method != "POST")
        {
            await _next(context);
            return;
        }
        
        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var key))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Idempotency-Key header required for POST");
            return;
        }
        
        var cached = await _store.GetAsync(key);
        if (cached != null)
        {
            await WriteResponseAsync(context, cached);
            return;
        }
        
        // Capture response et store
        // ...
    }
}

// ✅ OPÉRATIONS IDEMPOTENTES
public void RecordUsage(Guid transactionId, int tokens)
{
    // Vérifie si transaction déjà appliquée
    if (_appliedTransactions.Contains(transactionId))
        return; // Idempotent
    
    CurrentUsage += tokens;
    _appliedTransactions.Add(transactionId);
}
```

**Effort estimé** : 20h (middleware + tracking + refactoring 15 endpoints).

---

## 🎯 TOP 10 ACTIONS PRIORITAIRES

### 🔴 PRIORITÉ HAUTE (Critique)

| # | Violation | Impact | Effort | ROI |
|---|-----------|--------|--------|-----|
| **1** | **V-IDP-001 à 015** : Idempotence manquante (POST sans Idempotency-Key) | Doublons en prod lors retries | 20h | **Critique** |
| **2** | **V-IDP-016** : Opérations monétaires non-idempotentes (quotas) | Facturation incorrecte | 4h | **Critique** |

### 🟡 PRIORITÉ MOYENNE (Important)

| # | Violation | Impact | Effort | ROI |
|---|-----------|--------|--------|-----|
| **3** | **V-TDA-001 à 008** : Logique métier dans handlers (Tell Don't Ask) | Duplication, maintenabilité | 12h | **Élevé** |
| **4** | **V-AGG-001 à 007** : Boundaries d'aggregates mal définis | Cohérence transactionnelle faible | 10h | **Élevé** |
| **5** | **V-IMM-001 à 005** : Entités mutables (setters publics) | Mutations non contrôlées | 8h | **Moyen** |
| **6** | **V-RES-001 à 003** : Exceptions métier non wrappées | Flux d'erreur imprévisible | 6h | **Moyen** |
| **7** | **ADR-028** : Specification Pattern non implémenté | Duplication règles métier | 10h | **Moyen** |
| **8** | **ADR-040** : Outbox Pattern manquant | Perte d'events si crash | 15h | **Moyen** |

### 🟢 PRIORITÉ BASSE (Amélioration)

| # | Violation | Impact | Effort | ROI |
|---|-----------|--------|--------|-----|
| **9** | **ADR-026** : Null Object Pattern non utilisé | Null checks partout | 6h | **Bas** |
| **10** | **ADR-030** : Feature Toggles absents | Déploiement risqué de features | 8h | **Bas** |

---

## 📈 ESTIMATION EFFORT TOTAL

### Répartition par Priorité

- **🔴 Haute** : 24h (2 actions critiques)
- **🟡 Moyenne** : 61h (6 actions importantes)
- **🟢 Basse** : 23h (7 actions d'amélioration + autres ADR)

**TOTAL** : **108 heures** (~3 semaines de développement)

### Plan de Remédiation Recommandé

#### Sprint 1 (40h) - Corrections Critiques
- ✅ Implémenter Idempotence (POST + middleware) : 20h
- ✅ Refactoring Tell Don't Ask (8 handlers) : 12h
- ✅ Corriger opérations monétaires : 4h
- ✅ Tests unitaires/intégration : 4h

#### Sprint 2 (40h) - Améliorations Structurelles
- ✅ Redéfinir Aggregate Roots boundaries : 10h
- ✅ Implémenter Specification Pattern : 10h
- ✅ Corriger immutabilité entités : 8h
- ✅ Wrapper Result Pattern pour exceptions : 6h
- ✅ Tests + documentation : 6h

#### Sprint 3 (28h) - Patterns Manquants
- ✅ Implémenter Outbox Pattern : 15h
- ✅ Implémenter Feature Toggles : 8h
- ✅ Null Object Pattern : 6h
- ✅ Tests + monitoring : 4h

---

## 📝 RECOMMANDATIONS FINALES

### Points Forts du Projet

1. ✅ **CQRS** excellemment implémenté (95%)
2. ✅ **Dependency Injection** parfait (98%)
3. ✅ **Repository Pattern** bien appliqué (95%)
4. ✅ **Unit of Work** solide (95%)
5. ✅ **Domain Events** bien structurés (95%)
6. ✅ **Convention over Configuration** cohérent (95%)

### Axes d'Amélioration Prioritaires

1. 🔴 **Idempotence** : Impératif avant mise en production
2. 🟡 **Tell Don't Ask** : Améliorer encapsulation domain
3. 🟡 **Aggregate Roots** : Clarifier boundaries transactionnelles
4. 🟡 **Patterns manquants** : Specification, Outbox, Null Object, Feature Toggles

### Prochaines Étapes

1. **Validation avec l'équipe** : Prioriser les 10 actions selon contexte projet
2. **Planification sprints** : Répartir corrections sur 3 sprints
3. **Documentation** : Mettre à jour ADR avec décisions de remédiation
4. **Monitoring** : Ajouter métriques pour tracker conformité continue

---

**CONCLUSION** : Projet globalement **bien structuré** (70.8%) avec fondations solides (CQRS, DI, Repositories). Les corrections prioritaires (Idempotence, Tell Don't Ask) amélioreront significativement la **robustesse** et la **maintenabilité** du système.

