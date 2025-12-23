# Tâche 027 - Implémenter ADR-028 : Specification Pattern

**Statut** : À faire  
**Priorité** : 🟡 MOYENNE (P3)  
**Conformité cible** : ADR-028 de 0% → 90%  
**Dépendances** : Aucune

## CONTEXTE

**Analyse ADR-028** : `docs/ANALYSE_CONFORMITE_ADR-013-030.md` (lignes 658-720)  
**ADR** : `docs/adr/028-specification-pattern.adr.md`

**Conformité actuelle** : **0%** (pattern non implémenté)

**Problème identifié** :
- 🟡 **Règles métier dupliquées** : Logique de validation répétée dans plusieurs handlers
- 🟡 **Violation DRY** : Même condition métier écrite 5+ fois
- 🟡 **Testabilité faible** : Difficile de tester les règles métier isolément
- 🟡 **Maintenabilité** : Modification d'une règle = changement dans N fichiers

**Cas d'usage identifiés** (docs/ANALYSE_CONFORMITE_ADR-013-030.md lignes 665-670) :
1. **TenantIsEligibleSpecification** : `tenant.IsActive && tenant.MonthlyQuota > tenant.CurrentUsage && !tenant.IsSuspended`
2. **UserHasPermissionSpecification** : Validation des permissions basée sur rôles
3. **QuotaIsAvailableSpecification** : Vérification quota restant > demandé

**Exemples de duplication détectés** :
```csharp
// ❌ V-SPEC-001 : Validation tenant répétée (5 endroits différents)
// Dans CreateUserCommandHandler
if (!tenant.IsActive || tenant.IsSuspended || tenant.CurrentUsage >= tenant.MonthlyQuota)
{
    return Error.Validation("Tenant not eligible");
}

// Dans ProcessRequestCommandHandler (MÊME LOGIQUE)
if (!tenant.IsActive || tenant.IsSuspended || tenant.CurrentUsage >= tenant.MonthlyQuota)
{
    return Error.Validation("Tenant cannot process request");
}

// Dans UpdateTenantCommandHandler (ENCORE LA MÊME)
if (!tenant.IsActive || tenant.IsSuspended)
{
    return Error.Validation("Tenant inactive");
}

// ❌ V-SPEC-002 : Validation quota dupliquée (3 endroits)
// Dans RecordUsageCommandHandler
var remaining = quota.Limit - quota.CurrentUsage;
if (remaining < tokens)
{
    return Error.QuotaExceeded();
}

// Dans CheckQuotaQueryHandler (MÊME LOGIQUE)
if ((quota.CurrentUsage + tokens) > quota.Limit)
{
    return Error.QuotaExceeded();
}
```

**Risques sans Specification Pattern** :
- 🟡 **Violations DRY** : Logique métier dupliquée
- 🟡 **Bugs incohérents** : Règle modifiée à un endroit mais oubliée ailleurs
- 🟡 **Tests incomplets** : Impossible de tester règles métier isolément
- 🟡 **Compréhension difficile** : Règles métier noyées dans le code

## OBJECTIF

Implémenter le Specification Pattern pour centraliser et réutiliser les règles métier complexes.

**Spécifications ADR-028** :
- Créer interface `ISpecification<T>` avec méthode `IsSatisfiedBy(T entity)`
- Implémenter spécifications métier réutilisables
- Combiner spécifications avec opérateurs logiques (And, Or, Not)
- Support LINQ avec `ToExpression()` pour EF Core

## CRITÈRES DE SUCCÈS

### Fonctionnels
- [ ] `ISpecification<T>` interface créée
- [ ] `TenantIsEligibleSpecification` implémentée
- [ ] `QuotaIsAvailableSpecification` implémentée
- [ ] Opérateurs logiques (And, Or, Not) implémentés
- [ ] Handlers CQRS refactorés (élimination duplications)

### Techniques
- [ ] Support in-memory (`IsSatisfiedBy`) pour validation
- [ ] Support EF Core (`ToExpression()`) pour requêtes
- [ ] Spécifications composables (chaînage And/Or)
- [ ] Tests unitaires : au moins 15 scénarios

### Qualité
- [ ] **Build** : 0 erreurs, 0 warnings
- [ ] **Tests** : 15+ nouveaux tests Specification
  - TenantIsEligibleSpecification scenarios
  - QuotaIsAvailableSpecification scenarios
  - Logical operators (And, Or, Not)
  - Composition chaining
  - EF Core expression generation
  - Handler refactoring validation
- [ ] Tests existants : 100% passing (non-régression)
- [ ] Documentation README.md mise à jour

## ÉTAPES D'IMPLÉMENTATION

### 1. Créer ISpecification<T> interface (1h)

**Fichier** : `src/Core/LLMProxy.Domain/Specifications/ISpecification.cs`

**Création** : Interface générique pour spécifications.

```csharp
using System.Linq.Expressions;

namespace LLMProxy.Domain.Specifications;

/// <summary>
/// Spécification pour encapsuler une règle métier réutilisable.
/// Conforme à ADR-028 (Specification Pattern).
/// </summary>
/// <typeparam name="T">Type d'entité sur laquelle la spécification s'applique.</typeparam>
/// <remarks>
/// Permet de centraliser les règles métier complexes et de les combiner.
/// Support à la fois l'évaluation in-memory (<see cref="IsSatisfiedBy"/>) et les requêtes EF Core (<see cref="ToExpression"/>).
/// </remarks>
public interface ISpecification<T>
{
    /// <summary>
    /// Vérifie si une entité satisfait la spécification (évaluation in-memory).
    /// </summary>
    /// <param name="entity">Entité à tester.</param>
    /// <returns><c>true</c> si l'entité satisfait la règle métier, sinon <c>false</c>.</returns>
    /// <remarks>
    /// Utilisé pour valider des entités déjà chargées en mémoire.
    /// Pour les requêtes EF Core, utiliser <see cref="ToExpression"/> à la place.
    /// </remarks>
    bool IsSatisfiedBy(T entity);

    /// <summary>
    /// Convertit la spécification en expression LINQ pour requêtes EF Core.
    /// </summary>
    /// <returns>Expression LINQ représentant la règle métier.</returns>
    /// <remarks>
    /// Permet d'utiliser la spécification dans des requêtes EF Core :
    /// <code>
    /// var spec = new TenantIsEligibleSpecification();
    /// var eligibleTenants = await context.Tenants
    ///     .Where(spec.ToExpression())
    ///     .ToListAsync();
    /// </code>
    /// </remarks>
    Expression<Func<T, bool>> ToExpression();
}
```

**Action** : Créer dossier `Specifications/` et interface `ISpecification<T>`.

---

### 2. Créer CompositeSpecification<T> classe de base (1h30)

**Fichier** : `src/Core/LLMProxy.Domain/Specifications/CompositeSpecification.cs`

**Création** : Classe abstraite avec opérateurs logiques.

```csharp
using System.Linq.Expressions;

namespace LLMProxy.Domain.Specifications;

/// <summary>
/// Classe de base pour spécifications composables avec opérateurs logiques.
/// Conforme à ADR-028 (Specification Pattern).
/// </summary>
/// <typeparam name="T">Type d'entité.</typeparam>
/// <remarks>
/// Fournit les opérateurs And, Or, Not pour combiner des spécifications.
/// Les classes dérivées doivent implémenter <see cref="ToExpression"/>.
/// </remarks>
public abstract class CompositeSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// Vérifie si une entité satisfait la spécification.
    /// </summary>
    /// <remarks>
    /// Implémentation par défaut utilisant <see cref="ToExpression"/>.
    /// Peut être surchargée pour optimiser l'évaluation in-memory.
    /// </remarks>
    public virtual bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    /// <summary>
    /// Convertit la spécification en expression LINQ.
    /// </summary>
    /// <remarks>
    /// Doit être implémentée par les classes dérivées.
    /// </remarks>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Combine deux spécifications avec l'opérateur logique AND.
    /// </summary>
    /// <param name="other">Autre spécification à combiner.</param>
    /// <returns>Nouvelle spécification représentant (this AND other).</returns>
    /// <remarks>
    /// Exemple :
    /// <code>
    /// var spec = new TenantIsActiveSpecification()
    ///     .And(new TenantHasQuotaSpecification());
    /// </code>
    /// </remarks>
    public ISpecification<T> And(ISpecification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }

    /// <summary>
    /// Combine deux spécifications avec l'opérateur logique OR.
    /// </summary>
    /// <param name="other">Autre spécification à combiner.</param>
    /// <returns>Nouvelle spécification représentant (this OR other).</returns>
    public ISpecification<T> Or(ISpecification<T> other)
    {
        return new OrSpecification<T>(this, other);
    }

    /// <summary>
    /// Inverse la spécification avec l'opérateur logique NOT.
    /// </summary>
    /// <returns>Nouvelle spécification représentant (NOT this).</returns>
    public ISpecification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}
```

**Action** : Créer classe abstraite `CompositeSpecification<T>`.

---

### 3. Créer opérateurs logiques (1h30)

**Fichiers** :
- `src/Core/LLMProxy.Domain/Specifications/AndSpecification.cs`
- `src/Core/LLMProxy.Domain/Specifications/OrSpecification.cs`
- `src/Core/LLMProxy.Domain/Specifications/NotSpecification.cs`

**Création AndSpecification** :

```csharp
using System.Linq.Expressions;

namespace LLMProxy.Domain.Specifications;

/// <summary>
/// Spécification combinant deux spécifications avec l'opérateur AND.
/// Conforme à ADR-028 (Specification Pattern).
/// </summary>
internal sealed class AndSpecification<T> : CompositeSpecification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override bool IsSatisfiedBy(T entity)
    {
        return _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();

        // Combiner les expressions avec AND
        var parameter = Expression.Parameter(typeof(T), "x");
        var combined = Expression.AndAlso(
            Expression.Invoke(leftExpr, parameter),
            Expression.Invoke(rightExpr, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}
```

**Création OrSpecification** : (même structure, remplacer `AndAlso` par `OrElse`)

**Création NotSpecification** : (inverser avec `Expression.Not`)

**Action** : Créer les 3 classes d'opérateurs logiques.

---

### 4. Créer TenantIsEligibleSpecification (1h)

**Fichier** : `src/Core/LLMProxy.Domain/Specifications/Tenants/TenantIsEligibleSpecification.cs`

**Création** : Spécification métier pour tenant éligible.

```csharp
using LLMProxy.Domain.Entities;
using System.Linq.Expressions;

namespace LLMProxy.Domain.Specifications.Tenants;

/// <summary>
/// Spécification déterminant si un tenant est éligible pour traiter des requêtes.
/// Conforme à ADR-028 (Specification Pattern).
/// </summary>
/// <remarks>
/// Un tenant est éligible si :
/// - Il est actif (<see cref="Tenant.IsActive"/> = true)
/// - Il n'est pas suspendu (logique métier à définir)
/// - Il a du quota disponible (usage actuel inférieur à la limite)
/// 
/// Cette règle métier centralisée évite la duplication dans les handlers.
/// </remarks>
public sealed class TenantIsEligibleSpecification : CompositeSpecification<Tenant>
{
    public override Expression<Func<Tenant, bool>> ToExpression()
    {
        // Règle métier : tenant actif ET pas suspendu ET quota disponible
        return tenant => tenant.IsActive 
                      && tenant.DeactivatedAt == null;
        // Note: Logique quota à ajouter selon modèle de données
    }

    public override bool IsSatisfiedBy(Tenant tenant)
    {
        if (tenant == null)
            return false;

        return tenant.IsActive 
            && tenant.DeactivatedAt == null;
    }
}
```

**Action** : Créer spécification `TenantIsEligibleSpecification`.

---

### 5. Créer QuotaIsAvailableSpecification (1h)

**Fichier** : `src/Core/LLMProxy.Domain/Specifications/Quotas/QuotaIsAvailableSpecification.cs`

**Création** : Spécification métier pour quota disponible.

```csharp
using LLMProxy.Domain.Entities;
using System.Linq.Expressions;

namespace LLMProxy.Domain.Specifications.Quotas;

/// <summary>
/// Spécification déterminant si un quota a suffisamment de capacité disponible.
/// Conforme à ADR-028 (Specification Pattern).
/// </summary>
/// <remarks>
/// Paramétrable avec le nombre de tokens demandés.
/// Utilisée pour valider les requêtes avant traitement.
/// </remarks>
public sealed class QuotaIsAvailableSpecification : CompositeSpecification<QuotaUsage>
{
    private readonly long _tokensRequested;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="QuotaIsAvailableSpecification"/>.
    /// </summary>
    /// <param name="tokensRequested">Nombre de tokens demandés.</param>
    public QuotaIsAvailableSpecification(long tokensRequested)
    {
        _tokensRequested = tokensRequested;
    }

    public override Expression<Func<QuotaUsage, bool>> ToExpression()
    {
        // Règle métier : usage actuel + demandé <= limite
        return quota => (quota.CurrentUsage + _tokensRequested) <= quota.Limit;
    }

    public override bool IsSatisfiedBy(QuotaUsage quota)
    {
        if (quota == null)
            return false;

        var remaining = quota.Limit - quota.CurrentUsage;
        return remaining >= _tokensRequested;
    }
}
```

**Action** : Créer spécification `QuotaIsAvailableSpecification`.

---

### 6. Refactorer handlers CQRS (2h)

**Fichiers à modifier** :
- `src/Application/LLMProxy.Application/Tenants/Commands/*.cs`
- `src/Application/LLMProxy.Application/Quotas/Commands/*.cs`

**Changements** :

```csharp
// ❌ AVANT : Logique métier dupliquée dans handler
public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);
    
    // Validation métier dupliquée dans 5 handlers
    if (!tenant.IsActive || tenant.IsSuspended || tenant.CurrentUsage >= tenant.MonthlyQuota)
    {
        return Result.Failure(Error.Validation("Tenant not eligible"));
    }
    
    // Logique création utilisateur...
}

// ✅ APRÈS : Spécification réutilisable
public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);
    
    var spec = new TenantIsEligibleSpecification();
    if (!spec.IsSatisfiedBy(tenant))
    {
        return Result.Failure(Error.Validation("Tenant not eligible"));
    }
    
    // Logique création utilisateur...
}
```

**Action** : Identifier et refactorer 5+ handlers utilisant règles métier dupliquées.

---

### 7. Créer tests unitaires (2h)

**Fichier** : `tests/LLMProxy.Domain.Tests/Specifications/SpecificationTests.cs`

**Création** : Tests pour spécifications et opérateurs.

```csharp
using LLMProxy.Domain.Entities;
using LLMProxy.Domain.Specifications.Tenants;
using LLMProxy.Domain.Specifications.Quotas;
using Xunit;

namespace LLMProxy.Domain.Tests.Specifications;

public sealed class SpecificationTests
{
    [Fact]
    public void TenantIsEligibleSpecification_ActiveTenant_ShouldSatisfy()
    {
        // ARRANGE
        var tenant = CreateActiveTenant();
        var spec = new TenantIsEligibleSpecification();

        // ACT
        var result = spec.IsSatisfiedBy(tenant);

        // ASSERT
        Assert.True(result);
    }

    [Fact]
    public void TenantIsEligibleSpecification_InactiveTenant_ShouldNotSatisfy()
    {
        // ARRANGE
        var tenant = CreateActiveTenant();
        tenant.Deactivate(); // Rend le tenant inactif
        var spec = new TenantIsEligibleSpecification();

        // ACT
        var result = spec.IsSatisfiedBy(tenant);

        // ASSERT
        Assert.False(result);
    }

    [Fact]
    public void QuotaIsAvailableSpecification_SufficientQuota_ShouldSatisfy()
    {
        // ARRANGE
        var quota = new QuotaUsage
        {
            CurrentUsage = 500,
            Limit = 1000
        };
        var spec = new QuotaIsAvailableSpecification(tokensRequested: 300);

        // ACT
        var result = spec.IsSatisfiedBy(quota);

        // ASSERT
        Assert.True(result);
    }

    [Fact]
    public void QuotaIsAvailableSpecification_InsufficientQuota_ShouldNotSatisfy()
    {
        // ARRANGE
        var quota = new QuotaUsage
        {
            CurrentUsage = 900,
            Limit = 1000
        };
        var spec = new QuotaIsAvailableSpecification(tokensRequested: 200);

        // ACT
        var result = spec.IsSatisfiedBy(quota);

        // ASSERT
        Assert.False(result);
    }

    [Fact]
    public void AndSpecification_BothSatisfied_ShouldSatisfy()
    {
        // ARRANGE
        var tenant = CreateActiveTenant();
        var spec1 = new TenantIsActiveSpecification();
        var spec2 = new TenantNotDeactivatedSpecification();
        var combined = spec1.And(spec2);

        // ACT
        var result = combined.IsSatisfiedBy(tenant);

        // ASSERT
        Assert.True(result);
    }

    [Fact]
    public void OrSpecification_OneSatisfied_ShouldSatisfy()
    {
        // ARRANGE
        var tenant = CreateInactiveTenant();
        var spec1 = new TenantIsActiveSpecification(); // False
        var spec2 = new TenantNotDeactivatedSpecification(); // True
        var combined = spec1.Or(spec2);

        // ACT
        var result = combined.IsSatisfiedBy(tenant);

        // ASSERT
        Assert.True(result);
    }

    [Fact]
    public void NotSpecification_Satisfied_ShouldInvert()
    {
        // ARRANGE
        var tenant = CreateActiveTenant();
        var spec = new TenantIsActiveSpecification();
        var inverted = spec.Not();

        // ACT
        var result = inverted.IsSatisfiedBy(tenant);

        // ASSERT
        Assert.False(result); // Active tenant → NOT Active = False
    }

    [Fact]
    public void ToExpression_ShouldWorkWithEFCore()
    {
        // ARRANGE
        var spec = new TenantIsEligibleSpecification();
        var expression = spec.ToExpression();

        // ACT
        var compiled = expression.Compile();
        var tenant = CreateActiveTenant();
        var result = compiled(tenant);

        // ASSERT
        Assert.True(result);
        Assert.NotNull(expression); // Expression utilisable dans EF Core
    }

    private Tenant CreateActiveTenant()
    {
        var result = Tenant.Create("Test Tenant", "test-tenant");
        return result.Value;
    }

    private Tenant CreateInactiveTenant()
    {
        var tenant = CreateActiveTenant();
        tenant.Deactivate();
        return tenant;
    }
}
```

**Action** : Créer 15+ tests couvrant tous les scénarios.

---

### 8. Mettre à jour README.md (30 min)

**Fichier** : `README.md`

**Ajout** : Section "Specification Pattern (ADR-028)".

```markdown
### Specification Pattern (ADR-028)

**Centralisation des règles métier** avec spécifications réutilisables et composables.

**Spécifications disponibles :**

| Spécification | Usage | Règle métier |
|---------------|-------|--------------|
| `TenantIsEligibleSpecification` | Validation tenant | Active ET non suspendu ET quota disponible |
| `QuotaIsAvailableSpecification` | Validation quota | Usage + demandé ≤ limite |

**Avantages :**
- ✅ Élimine duplication (DRY)
- ✅ Règles métier testables isolément
- ✅ Composition avec opérateurs logiques (And, Or, Not)
- ✅ Support EF Core (ToExpression) et in-memory (IsSatisfiedBy)

**Exemple d'utilisation :**

```csharp
// ❌ AVANT : Logique métier dupliquée
if (!tenant.IsActive || tenant.IsSuspended || tenant.CurrentUsage >= tenant.MonthlyQuota)
{
    return Error.Validation("Tenant not eligible");
}

// ✅ APRÈS : Spécification réutilisable
var spec = new TenantIsEligibleSpecification();
if (!spec.IsSatisfiedBy(tenant))
{
    return Error.Validation("Tenant not eligible");
}
```

**Composition de spécifications :**

```csharp
// Combiner plusieurs règles métier
var spec = new TenantIsActiveSpecification()
    .And(new TenantHasQuotaSpecification())
    .Or(new TenantIsPremiumSpecification());

var isEligible = spec.IsSatisfiedBy(tenant);
```

**Support EF Core :**

```csharp
// Utiliser spécification dans requête EF Core
var spec = new TenantIsEligibleSpecification();
var eligibleTenants = await context.Tenants
    .Where(spec.ToExpression())
    .ToListAsync();
```
```

**Action** : Documenter Specification Pattern et usage.

---

### 9. Build, test et validation (1h)

**Commandes** :

```powershell
# Build
dotnet build --no-restore

# Tests
dotnet test --no-build --no-restore

# Validation : Vérifier sortie
# - 0 errors, 0 warnings
# - Tous tests passing (102 anciens + 15 nouveaux = 117 total)
```

**Action** :
1. Compiler sans erreurs ni warnings
2. Exécuter tests (100% passing)
3. Valider refactoring handlers

---

### 10. Commit et merge (30 min)

**Commits atomiques** :

```powershell
# Commit 1: Interface
git add src/Core/LLMProxy.Domain/Specifications/ISpecification.cs
git commit -m "feat(specification): Add ISpecification<T> interface

- Generic interface for business rules encapsulation
- IsSatisfiedBy for in-memory validation
- ToExpression for EF Core queries

ADR-028 conformity: Interface created"

# Commit 2: CompositeSpecification
git add src/Core/LLMProxy.Domain/Specifications/CompositeSpecification.cs
git commit -m "feat(specification): Add CompositeSpecification base class

- Abstract base with logical operators (And, Or, Not)
- Default IsSatisfiedBy implementation
- Composable specifications

ADR-028 conformity: Base class created"

# Commit 3: Logical operators
git add src/Core/LLMProxy.Domain/Specifications/AndSpecification.cs \
       src/Core/LLMProxy.Domain/Specifications/OrSpecification.cs \
       src/Core/LLMProxy.Domain/Specifications/NotSpecification.cs
git commit -m "feat(specification): Add logical operators (And, Or, Not)

- AndSpecification combines two specs with AND
- OrSpecification combines two specs with OR
- NotSpecification inverts a spec with NOT
- Expression composition for EF Core

ADR-028 conformity: Operators created"

# Commit 4: TenantIsEligibleSpecification
git add src/Core/LLMProxy.Domain/Specifications/Tenants/TenantIsEligibleSpecification.cs
git commit -m "feat(specification): Add TenantIsEligibleSpecification

- Business rule: active AND not deactivated
- Eliminates duplication across 5 handlers
- Supports in-memory and EF Core

ADR-028 conformity: Tenant specification created"

# Commit 5: QuotaIsAvailableSpecification
git add src/Core/LLMProxy.Domain/Specifications/Quotas/QuotaIsAvailableSpecification.cs
git commit -m "feat(specification): Add QuotaIsAvailableSpecification

- Business rule: (usage + requested) <= limit
- Parameterized with tokens requested
- Centralized quota validation logic

ADR-028 conformity: Quota specification created"

# Commit 6: Refactor handlers
git add src/Application/LLMProxy.Application/**/*.cs
git commit -m "refactor(specification): Replace duplicated business rules with specifications

- Removed 5+ duplicated tenant validation checks
- Removed 3+ duplicated quota validation checks
- Use TenantIsEligibleSpecification in handlers
- Use QuotaIsAvailableSpecification in handlers

ADR-028 conformity: Duplication eliminated"

# Commit 7: Tests
git add tests/LLMProxy.Domain.Tests/Specifications/SpecificationTests.cs
git commit -m "test(specification): Add comprehensive Specification tests

- 15+ tests covering all specifications
- Logical operators validation (And, Or, Not)
- Composition chaining tests
- EF Core expression generation validation

ADR-028 conformity: Test coverage complete"

# Commit 8: Documentation
git add README.md
git commit -m "docs(specification): Document Specification Pattern usage

- Table of available specifications
- Before/After code examples
- Composition examples (And, Or, Not)
- EF Core integration documentation

ADR-028 conformity: Documentation complete"
```

**Merge** :

```powershell
git checkout main
git merge --no-ff feature/027--implement-adr-028-specification-pattern -m "Merge feature/027 - Implement ADR-028 Specification Pattern"
git branch -d feature/027--implement-adr-028-specification-pattern
```

**Action** : 8 commits atomiques, merge, supprimer feature branch.

---

## RÉFÉRENCE ADR

**ADR-028** : `docs/adr/028-specification-pattern.adr.md`

**Principes clés** :
1. **Encapsulation** : Règle métier complexe = une classe de spécification
2. **Réutilisation** : Même règle utilisée dans N handlers sans duplication
3. **Composition** : Combiner spécifications simples en règles complexes
4. **Testabilité** : Tester règles métier isolément (unit tests)
5. **EF Core** : Utiliser `ToExpression()` pour requêtes base de données

**Spécifications recommandées** :
- **TenantIsEligibleSpecification** : Validation complète tenant
- **QuotaIsAvailableSpecification** : Vérification quota disponible
- **UserHasPermissionSpecification** : Contrôle accès basé rôles

**Bénéfices** :
- Élimine violations DRY
- Améliore testabilité (règles métier isolées)
- Facilite maintenance (1 endroit à changer)
- Supporte requêtes complexes (EF Core)

---

## DURÉE ESTIMÉE

**Total** : 12h  
- ISpecification interface : 1h
- CompositeSpecification : 1h30
- Opérateurs logiques : 1h30
- TenantIsEligibleSpecification : 1h
- QuotaIsAvailableSpecification : 1h
- Refactoring handlers : 2h
- Tests unitaires : 2h
- README.md : 30 min
- Build/test/validation : 1h
- Commits/merge : 30 min

---

## NOTES

**Impacts sur architecture** :
- Centralisation règles métier dans `Domain/Specifications/`
- Simplification handlers CQRS (moins de logique métier)
- Amélioration testabilité globale

**Trade-offs** :
- ✅ Élimine duplication DRY
- ✅ Règles métier explicites et documentées
- ✅ Testabilité isolée améliorée
- ⚠️ Légère complexité initiale (comprendre pattern)
- ⚠️ Fichiers supplémentaires (1 spécification = 1 fichier)

**Éviter abus** :
- Ne pas créer spécification pour condition triviale (`x == null`)
- Seulement pour règles métier complexes réutilisées 3+ fois
- Garder spécifications simples (1 règle métier par classe)

**Complémentarité** :
- Fonctionne avec Result Pattern (ADR-023) pour validation
- Complète Value Objects (ADR-024) pour encapsulation
- Utilise Null Object (ADR-026) pour éviter null checks


## TRACKING
Début: 2025-12-22T08:40:36.4517858Z


Fin: 2025-12-22T08:48:15.6714292Z
Durée: 00:07:39

