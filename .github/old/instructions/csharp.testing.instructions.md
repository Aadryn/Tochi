---
applyTo: "**/*.Tests/**/*.cs"
---

# Tests .NET - Règles de Développement avec xUnit, NFluent, NSubstitute, Bogus et bUnit

## 0. RÈGLES ABSOLUES (NON NÉGOCIABLES)
- L'ensemble des tests doit systématiquement être parallélisable pour optimiser le temps d'exécution.
- Toujours suivre le pattern AAA (Arrange-Act-Assert) avec une séparation visuelle claire entre les sections.
- Ne jamais utiliser du code obsolète ou des bibliothèques non approuvées.
- Toujours tester avec des interfaces plutot qu'avec des implémentations concrètes.
- Toujours veiller à l'isolation des tests (pas de dépendance entre tests), pour permettre l'exécution parallèle systématique.
- Toujours nettoyer les ressources externes (fichiers, bases de données, services) après chaque test.
- Toujours nommer les tests de manière descriptive pour refléter le scénario testé.
- Toujours ajouter des commentaires XML complet pour expliquer le but de chaque test.
- Toujours analyser et réflechir à des scénarios pertinents pour maximiser la couverture de code et détecter les régressions, qu'ils soient positifs ou négatifs. Nominaux et aux limites.
- Toujours utiliser Bogus pour générer des données de test réalistes et variées, multilingues si nécessaire, pour couvrir un maximum de cas.
- Use clear assertions that verify the outcome expressed by the test name
- Avoid using multiple assertions in one test method. In this case, prefer multiple tests.
- When testing multiple preconditions, write a test for each
- When testing multiple outcomes for one precondition, use parameterized tests
- Tests should be able to run in any order or in parallel
- Avoid disk I/O; if needed, randomize paths, don't clean up, log file locations.
- Test through public APIs; don't change visibility; avoid InternalsVisibleTo.
- Require tests for new/changed public APIs.
Assert specific values and edge cases, not vague outcomes.
### 0.1. Frameworks et Librairies OBLIGATOIRES

**TOUJOURS utiliser ces outils et UNIQUEMENT ces outils :**

```xml
<!-- *.Tests.csproj -->
<ItemGroup>
    <!-- Framework de test -->
    <PackageReference Include="xunit" Version="2.6.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.5" />
    
    <!-- Assertions fluides -->
    <PackageReference Include="NFluent" Version="3.0.3" />
    
    <!-- Mocks et Stubs -->
    <PackageReference Include="NSubstitute" Version="5.1.0" />
    
    <!-- Génération de données de test -->
    <PackageReference Include="Bogus" Version="35.3.0" />
    
    <!-- Tests de composants Blazor (si nécessaire) -->
    <PackageReference Include="bunit" Version="1.28.9" />
    <PackageReference Include="bunit.web" Version="1.28.9" />
</ItemGroup>
```

### ❌ INTERDICTIONS ABSOLUES

```csharp
// INTERDIT - NUnit
[Test]
public void MyTest() { }

// INTERDIT - MSTest
[TestMethod]
public void MyTest() { }

// INTERDIT - FluentAssertions (utiliser NFluent à la place)
result.Should().Be(expected);

// INTERDIT - Moq (utiliser NSubstitute à la place)
var mock = new Mock<IService>();

// INTERDIT - AutoFixture (utiliser Bogus à la place)
var fixture = new Fixture();
```

### ✅ OBLIGATOIRE - xUnit, NFluent, NSubstitute, Bogus

```csharp
// BON - xUnit + NFluent
[Fact]
public void MyTest()
{
    var result = 42;
    Check.That(result).IsEqualTo(42);
}

// BON - NSubstitute pour les mocks
var service = Substitute.For<IService>();
service.GetData().Returns("test");

// BON - Bogus pour les données de test
var faker = new Faker<User>()
    .RuleFor(u => u.Name, f => f.Name.FullName())
    .RuleFor(u => u.Email, f => f.Internet.Email());
```

### 0.2. Mutation Testing - Qualité des Tests (CRITIQUE)

**RÈGLE : Le mutation testing avec Stryker.NET valide la qualité réelle des tests**

#### Objectifs de Mutation Score

| Score | Niveau | Action |
|-------|--------|--------|
| **≥ 85%** | ⭐⭐⭐⭐⭐ EXCELLENT | Production Ready - Qualité optimale |
| **75-85%** | ⭐⭐⭐⭐ BON | Acceptable avec justification des gaps |
| **65-75%** | ⭐⭐⭐ MOYEN | Amélioration nécessaire |
| **< 65%** | ⭐⭐ INSUFFISANT | Tests critiques manquants - BLOQUANT |

#### Que Tester (Priorités Business)

**✅ TOUJOURS TESTER - Priorité HAUTE**
```csharp
// Logique métier et calculs
public decimal CalculateDiscount(Order order) { /* TESTER */ }

// Validations et règles métier  
public Result<User> ValidateUser(UserInput input) { /* TESTER */ }

// Transformations de données critiques
public UserDto MapToDto(User user) { /* TESTER */ }

// Error handling business
public Result<T> Handle(Command cmd) {
    if (condition) return Result.Failure("DOM-001", "Message exact"); // TESTER message EXACT
}

// Conditions et branches logiques
if (user.IsAdmin || user.HasPermission("X")) { /* TESTER les DEUX branches */ }

// Messages d'erreur EXACTS (pas juste "non null")
❌ BAD:  result.Error.Should().NotBeNull();
✅ GOOD: result.Error.Code.Should().Be("DOM-USER-001");
✅ GOOD: result.Error.Message.Should().Contain("Email invalide");
```

**⚠️ ACCEPTABLE DE NE PAS TESTER - Priorité BASSE**
```csharp
// Logging informatif (LogInformation, LogDebug, LogTrace)
_logger.LogInformation("User {Id} created", userId); // OK de ne pas tester

// Enregistrement DI basique
services.AddScoped<IUserService, UserService>(); // OK de ne pas tester

// Properties auto-implemented simples
public string Name { get; set; } // OK de ne pas tester

// Code généré automatiquement
[GeneratedCode] public class X { } // OK de ne pas tester
```

**✅ TESTER UNIQUEMENT SI CRITIQUE**
```csharp
// Logs d'erreur avec données sensibles
_logger.LogError("Failed {Email}", SanitizeEmail(email)); // TESTER la sanitization

// Logs de décision métier
_logger.LogWarning("User {Id} exceeded quota", id); // TESTER si business-critical

// DI avec configuration complexe
services.AddScoped<ICache>(sp => new RedisCache(options)); // TESTER si logique
```

#### Interpréter les Résultats Stryker

**Mutants Survivants (Survived)**
```
Statement mutation sur logging → ACCEPTABLE (priorité basse)
Logical mutation sur condition → CRITIQUE (test manquant)
Equality mutation sur ==       → CRITIQUE (branche non testée)
String mutation sur message    → IMPORTANT (assertion trop générique)
```

**Actions selon type de mutant :**
```csharp
// MUTANT: Logical mutation (condition inversée)
if (user.IsActive) { /* code */ }
// → Mutation: if (!user.IsActive) { /* code */ }
// ✅ ACTION: Tester EXPLICITEMENT les deux cas (IsActive=true ET IsActive=false)

// MUTANT: Equality mutation (== vers !=)
if (status == "Approved") { /* code */ }
// → Mutation: if (status != "Approved") { /* code */ }
// ✅ ACTION: Tester les deux branches avec assertions précises

// MUTANT: String mutation (message d'erreur)
return Result.Failure("DOM-001", "Invalid email format");
// → Mutation: return Result.Failure("DOM-001", "");
// ✅ ACTION: Vérifier le message EXACT, pas juste qu'il existe

// MUTANT: Statement mutation (logging)
_logger.LogInformation("User created");
// → Mutation: (ligne supprimée)
// ⚠️ ACTION: ACCEPTABLE de ne pas tester (log informatif)
```

**Timeouts (< 3% acceptable)**
```
Timeout = ALERTE PERFORMANCE
- Vérifier regex complexes dans validators
- Vérifier boucles potentiellement infinies
- Ajouter tests de performance avec BenchmarkDotNet
- Si > 5% timeouts → PROBLÈME DE DESIGN à corriger
```

**NoCoverage (< 5% acceptable)**
```
NoCoverage = Code non exécuté par aucun test
- Si dead code → SUPPRIMER (YAGNI)
- Si defensive coding (null check sur DI) → ACCEPTABLE
- Si logique métier → AJOUTER TESTS immédiatement
```

#### Assertions Précises (Anti-Pattern du Test Faible)

**❌ MAUVAIS - Assertions génériques**
```csharp
result.IsFailure.Should().BeTrue();              // Trop vague
result.Error.Should().NotBeNull();               // Trop vague
result.Error.Message.Should().NotBeEmpty();      // Trop vague
exception.Should().BeOfType<Exception>();         // Trop générique
```

**✅ BON - Assertions spécifiques**
```csharp
// Vérifier code ET message ET données
Check.That(result.IsFailure).IsTrue();
Check.That(result.Error.Code).IsEqualTo("DOM-RESOURCE-005");
Check.That(result.Error.Message).Contains("Email invalide");
Check.That(result.Error.Message).Contains("format");

// Exceptions avec détails
var ex = Check.ThatCode(() => sut.Method()).Throws<DomainException>();
Check.That(ex.Value.ErrorCode).IsEqualTo("DOM-001");
Check.That(ex.Value.Message).Contains("constraint violated");
```

#### Commande Stryker Recommandée

```bash
# Exécution standard
cd tests/Project.Units.Tests
dotnet stryker --project "Project.csproj" \
  --reporter html --reporter json --reporter progress \
  --verbosity info

# Avec threshold pour CI/CD
dotnet stryker --project "Project.csproj" \
  --reporter html --reporter json \
  --threshold-high 85 --threshold-low 75 --threshold-break 65
```

#### Checklist Anti-Mutants (Prévention dès l'Écriture)

**Basée sur l'analyse de 175 mutants survivants réels du projet**

Cette checklist permet d'écrire des tests résistants aux mutations dès le premier essai, évitant de longues phases de refactorisation. Les 5 patterns ci-dessous représentent **78% des mutants survivants** analysés.

##### #1 - String Mutations (41% des survivants) ⚠️ CRITIQUE

**Problème** : Tests vérifient l'existence d'un message sans vérifier son contenu exact.

```csharp
// ❌ FAIBLE - Le mutant "message = ''" survit
[Fact]
public void Validate_WithInvalidEmail_ReturnsError()
{
    var result = _validator.Validate(new User { Email = "invalid" });
    
    Check.That(result.IsFailure).IsTrue();
    Check.That(result.Error).IsNotNull();           // Mutation survit !
    Check.That(result.Error.Message).IsNotEmpty();  // Mutation survit !
}

// ✅ RÉSISTANT - Vérifie le contenu exact
[Fact]
public void Validate_WithInvalidEmail_ReturnsEmailFormatError()
{
    var result = _validator.Validate(new User { Email = "invalid" });
    
    Check.That(result.IsFailure).IsTrue();
    Check.That(result.Error.Code).IsEqualTo("DOM-USER-005");              // Exact
    Check.That(result.Error.Message).Contains("Email invalide");          // Contenu
    Check.That(result.Error.Message).Contains("format attendu");          // Détail
}
```

**Règle** : Toujours vérifier :
1. Le code d'erreur exact (`ErrorCode`)
2. Les mots-clés critiques du message (pas juste "non vide")
3. Les données contextuelles si présentes

**Format StandardISÉ des Codes d'Erreur : `APP-LAYER-DOMAIN-NNNN`**

Pour garantir la cohérence et la traçabilité des erreurs dans toute l'application, utiliser systématiquement ce format :

- **APP** : Identifiant de l'application (ex: BIL, CRM, AUTH, JANUS)
- **LAYER** : Couche architecturale du projet
  - `H` : Hosting (applications exécutables, endpoints, API)
  - `D` : Domain (logique métier, entités, règles)
  - `I` : Infrastructure (accès données, services externes)
  - `T` : Tools (utilitaires, analyseurs, helpers)
- **DOMAIN** : Sous-domaine fonctionnel/technique (AUTH, PAY, INV, API, DB, USER, RESOURCE...)
- **NNNN** : Numéro séquentiel (0001-9999)

**Exemples :**
```csharp
// Erreurs Hosting (endpoints, API, validation requêtes)
"JANUS-H-API-0001"      // Request validation failed
"JANUS-H-AUTH-0003"     // Unauthorized access

// Erreurs Domain (logique métier, règles business)
"JANUS-D-USER-0010"     // Email invalide
"JANUS-D-ALLOC-0015"    // Conflit d'allocation

// Erreurs Infrastructure (DB, services externes)
"JANUS-I-DB-0007"       // Connexion base échouée
"JANUS-I-API-0012"      // Timeout API externe

// Erreurs Tools (helpers, utilitaires)
"JANUS-T-CACHE-0005"    // Cache invalidation failed
"JANUS-T-LOG-0008"      // Logging configuration error
```

**Builder Pattern pour Construction Fluent :**
```csharp
using System;

/// <summary>
/// Enumération des couches architecturales.
/// </summary>
public enum ErrorLayer
{
    /// <summary>Couche Hosting (applications exécutables, endpoints, API).</summary>
    Hosting,
    /// <summary>Couche Domain (logique métier, entités, règles business).</summary>
    Domain,
    /// <summary>Couche Infrastructure (accès données, services externes).</summary>
    Infrastructure,
    /// <summary>Couche Tools (utilitaires, analyseurs, helpers).</summary>
    Tools
}

/// <summary>
/// Builder fluent pour construire des codes d'erreur standardisés au format APP-LAYER-DOMAIN-NNNN.
/// </summary>
/// <example>
/// <code>
/// var code = ErrorCodeBuilder
///     .Create("JANUS")
///     .DomainLayer()
///     .InDomain("USER")
///     .WithNumber(1)
///     .Build(); // "JANUS-D-USER-0001"
/// </code>
/// </example>
public sealed class ErrorCodeBuilder
{
    private string? _app;
    private ErrorLayer? _layer;
    private string? _domain;
    private int? _number;

    private ErrorCodeBuilder() { }

    /// <summary>
    /// Point d'entrée principal : crée un builder avec l'identifiant d'application.
    /// </summary>
    /// <param name="app">Code application (ex: "JANUS", "BIL", "CRM")</param>
    public static ErrorCodeBuilder Create(string app)
    {
        return new ErrorCodeBuilder().ForApp(app);
    }

    /// <summary>
    /// Définit l'application (préfixe du code).
    /// </summary>
    public ErrorCodeBuilder ForApp(string app)
    {
        if (string.IsNullOrWhiteSpace(app))
            throw new ArgumentException("App code must not be null or empty.", nameof(app));

        _app = app.Trim().ToUpperInvariant();
        return this;
    }

    /// <summary>
    /// Définit la couche (H / D / I / T).
    /// </summary>
    public ErrorCodeBuilder WithLayer(ErrorLayer layer)
    {
        _layer = layer;
        return this;
    }

    /// <summary>
    /// Raccourci : couche Hosting ("H").
    /// </summary>
    public ErrorCodeBuilder HostingLayer() => WithLayer(ErrorLayer.Hosting);

    /// <summary>
    /// Raccourci : couche Domain ("D").
    /// </summary>
    public ErrorCodeBuilder DomainLayer() => WithLayer(ErrorLayer.Domain);

    /// <summary>
    /// Raccourci : couche Infrastructure ("I").
    /// </summary>
    public ErrorCodeBuilder InfrastructureLayer() => WithLayer(ErrorLayer.Infrastructure);

    /// <summary>
    /// Raccourci : couche Tools ("T").
    /// </summary>
    public ErrorCodeBuilder ToolsLayer() => WithLayer(ErrorLayer.Tools);

    /// <summary>
    /// Définit le domaine fonctionnel/technique.
    /// </summary>
    /// <param name="domain">Code domaine (ex: "AUTH", "PAY", "USER", "RESOURCE")</param>
    public ErrorCodeBuilder InDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Domain must not be null or empty.", nameof(domain));

        _domain = domain.Trim().ToUpperInvariant();
        return this;
    }

    /// <summary>
    /// Définit le numéro séquentiel (0-9999).
    /// </summary>
    public ErrorCodeBuilder WithNumber(int number)
    {
        if (number < 0 || number > 9999)
            throw new ArgumentOutOfRangeException(nameof(number), "Number must be between 0 and 9999.");

        _number = number;
        return this;
    }

    /// <summary>
    /// Construit la chaîne de code finale : "APP-LAYER-DOMAIN-NNNN".
    /// </summary>
    public string Build()
    {
        if (string.IsNullOrWhiteSpace(_app))
            throw new InvalidOperationException("App is not set. Call ForApp() first.");

        if (!_layer.HasValue)
            throw new InvalidOperationException("Layer is not set. Call WithLayer()/HostingLayer()/DomainLayer()/InfrastructureLayer()/ToolsLayer().");

        if (string.IsNullOrWhiteSpace(_domain))
            throw new InvalidOperationException("Domain is not set. Call InDomain().");

        if (!_number.HasValue)
            throw new InvalidOperationException("Number is not set. Call WithNumber().");

        var layerCode = _layer switch
        {
            ErrorLayer.Hosting => "H",
            ErrorLayer.Domain => "D",
            ErrorLayer.Infrastructure => "I",
            ErrorLayer.Tools => "T",
            _ => throw new InvalidOperationException("Unknown layer.")
        };

        return $"{_app}-{layerCode}-{_domain}-{_number.Value:0000}";
    }
}
```

**Utilisation dans les Tests :**
```csharp
[Fact]
public void Validate_WithInvalidEmail_ReturnsStandardErrorCode()
{
    var result = _validator.Validate(new User { Email = "invalid" });
    
    var expectedCode = ErrorCodeBuilder
        .Create("JANUS")
        .DomainLayer()
        .InDomain("USER")
        .WithNumber(5)
        .Build(); // "JANUS-D-USER-0005"
    
    Check.That(result.Error.Code).IsEqualTo(expectedCode);
}
```

**Avantages :**
- ✅ Format cohérent et normalisé dans toute l'application
- ✅ Traçabilité immédiate (couche + domaine)
- ✅ Facile à prononcer et recopier pour support utilisateur
- ✅ Recherche rapide dans les logs et documentation
- ✅ Construction type-safe avec validation à la compilation

##### #2 - Equality Mutations (10% des survivants) ⚠️ BOUNDARY CONDITIONS

**Problème** : Tests ne vérifient pas les valeurs limites des comparaisons.

```csharp
// Code source
public bool IsValidStatus(int statusCode) => statusCode >= 200 && statusCode < 300;

// ❌ FAIBLE - Ne teste qu'une valeur au milieu
[Fact]
public void IsValidStatus_With250_ReturnsTrue()
{
    var result = IsValidStatus(250);  // Mutation >= vers > survit
    Check.That(result).IsTrue();
}

// ✅ RÉSISTANT - Teste les boundaries (limite-1, limite, limite+1)
[Theory]
[InlineData(199, false)]  // Juste avant la limite basse
[InlineData(200, true)]   // Limite basse exacte (>=)
[InlineData(201, true)]   // Juste après la limite basse
[InlineData(299, true)]   // Juste avant la limite haute
[InlineData(300, false)]  // Limite haute exacte (<)
[InlineData(301, false)]  // Juste après la limite haute
public void IsValidStatus_WithBoundaryValues_ReturnsExpectedResult(int status, bool expected)
{
    var result = IsValidStatus(status);
    Check.That(result).IsEqualTo(expected);
}
```

**Règle** : Pour toute comparaison (`>=`, `>`, `<=`, `<`, `==`) :
- Tester : **Boundary-1**, **Boundary**, **Boundary+1**
- Exemple : `x >= 100` → Tester 99, 100, 101

##### #3 - Logical Mutations (6% des survivants) ⚠️ TABLE DE VÉRITÉ

**Problème** : Tests ne couvrent pas toutes les combinaisons logiques `&&` et `||`.

```csharp
// Code source
public bool CanAccess(User user) => user.IsActive && user.HasPermission("Read");

// ❌ FAIBLE - Teste uniquement le cas "tout vrai"
[Fact]
public void CanAccess_WithActiveUserAndPermission_ReturnsTrue()
{
    var user = new User { IsActive = true, HasPermission = true };
    Check.That(CanAccess(user)).IsTrue();  // Mutation && vers || survit
}

// ✅ RÉSISTANT - Table de vérité complète pour &&
[Theory]
[InlineData(true,  true,  true)]   // TT → true  (cas nominal)
[InlineData(true,  false, false)]  // TF → false (actif MAIS pas de permission)
[InlineData(false, true,  false)]  // FT → false (permission MAIS inactif)
[InlineData(false, false, false)]  // FF → false (ni actif ni permission)
public void CanAccess_WithVariousCombinations_ReturnsExpectedResult(
    bool isActive, bool hasPermission, bool expected)
{
    var user = new User { IsActive = isActive };
    if (hasPermission) user.Permissions.Add("Read");
    
    Check.That(CanAccess(user)).IsEqualTo(expected);
}
```

**Règles** :
- Pour `A && B` : Tester **au minimum** (T,T), (T,F), (F,T)
- Pour `A || B` : Tester **au minimum** (T,*), (F,T), (F,F)
- Idéal : Couvrir les 4 combinaisons pour être exhaustif

##### #4 - Boolean Mutations (5% des survivants) ⚠️ CHEMINS IF/ELSE

**Problème** : Tests vérifient qu'une action se produit mais pas qu'elle NE se produit PAS dans le cas inverse.

```csharp
// Code source
public void Process(Request request)
{
    if (request.IsUrgent)
    {
        _notifier.SendUrgentAlert(request);
    }
    // ... suite du traitement
}

// ❌ FAIBLE - Vérifie seulement IsUrgent = true
[Fact]
public void Process_WithUrgentRequest_SendsAlert()
{
    var request = new Request { IsUrgent = true };
    
    _sut.Process(request);
    
    _notifier.Received(1).SendUrgentAlert(request);  // Mutation true→false survit
}

// ✅ RÉSISTANT - Teste EXPLICITEMENT les deux branches
[Fact]
public void Process_WithUrgentRequest_SendsAlert()
{
    var request = new Request { IsUrgent = true };
    _sut.Process(request);
    _notifier.Received(1).SendUrgentAlert(request);
}

[Fact]
public void Process_WithNonUrgentRequest_DoesNotSendAlert()
{
    var request = new Request { IsUrgent = false };  // Branche inverse
    _sut.Process(request);
    _notifier.DidNotReceive().SendUrgentAlert(Arg.Any<Request>());  // Vérifie NON appelé
}
```

**Règle** : Pour chaque condition booléenne `if (x)`, créer 2 tests :
1. `MethodName_WhenConditionTrue_ExpectedBehavior`
2. `MethodName_WhenConditionFalse_ExpectedBehavior`

##### #5 - Null Coalescing Mutations (6% des survivants) ⚠️ FALLBACK

**Problème** : Tests ne vérifient pas les valeurs de fallback des opérateurs `??` et `?.`.

```csharp
// Code source
public string GetUserName(User user) => user?.Name ?? "Anonymous";

// ❌ FAIBLE - Ne teste que le cas "user non null"
[Fact]
public void GetUserName_WithValidUser_ReturnsName()
{
    var user = new User { Name = "John" };
    Check.That(GetUserName(user)).IsEqualTo("John");  // Mutation ?? survit
}

// ✅ RÉSISTANT - Teste les 3 cas (non-null, null user, null name)
[Fact]
public void GetUserName_WithValidUser_ReturnsName()
{
    var user = new User { Name = "John" };
    Check.That(GetUserName(user)).IsEqualTo("John");
}

[Fact]
public void GetUserName_WithNullUser_ReturnsAnonymous()
{
    Check.That(GetUserName(null)).IsEqualTo("Anonymous");  // Teste le fallback
}

[Fact]
public void GetUserName_WithUserButNullName_ReturnsAnonymous()
{
    var user = new User { Name = null };
    Check.That(GetUserName(user)).IsEqualTo("Anonymous");  // Teste ?. + ??
}
```

**Règle** : Pour chaque opérateur `??` ou `?.`, créer au moins 2 tests :
1. Test avec valeur **non-null** (utilise la valeur)
2. Test avec valeur **null** (utilise le fallback)

#### Récapitulatif des Patterns Anti-Mutants

| Pattern | % Survivants | Action Préventive | Impact |
|---------|--------------|-------------------|--------|
| **String mutations** | 41% | Vérifier code ET contenu exact | ⚠️⚠️⚠️ CRITIQUE |
| **Equality mutations** | 10% | Tester Boundary-1, Boundary, Boundary+1 | ⚠️⚠️ IMPORTANT |
| **Logical mutations** | 6% | Table de vérité complète (&&, \|\|) | ⚠️ MOYEN |
| **Boolean mutations** | 5% | Tester True ET False explicitement | ⚠️ MOYEN |
| **Null coalescing** | 6% | Tester valeur ET fallback | ⚠️ MOYEN |

**Application de cette checklist = Prévention de 62% des mutants survivants** (hors Statement mutations acceptables comme le logging).

### 0.3. Organisation des Tests (OBLIGATOIRE)

**RÈGLE ABSOLUE : Un fichier source ↔ Un fichier de test avec la même arborescence**

```
Fichier source                              →  Fichier de test
─────────────────────────────────────────────────────────────────────────
src/ADP.Application.Services/               →  tests/ADP.Application.Services.Tests/
├── Services/                               →  ├── Services/
│   ├── UserService.cs                      →  │   ├── UserServiceTests.cs
│   └── ClaimsService.cs                    →  │   └── ClaimsServiceTests.cs
└── DTOs/                                   →  └── DTOs/
    └── UserDto.cs                          →      └── UserDtoTests.cs (si logique)

src/ADP.Domain.Core/                        →  tests/ADP.Domain.Core.Tests/
├── Entities/                               →  ├── Entities/
│   ├── User.cs                             →  │   ├── UserTests.cs
│   └── Order.cs                            →  │   └── OrderTests.cs
└── Interfaces/                             →  └── Interfaces/
    └── IRepository.cs                      →      └── (pas de test pour interfaces)

src/ADP.Demo.WebApi/                        →  tests/ADP.Demo.WebApi.Tests/
├── Controllers/                            →  ├── Controllers/
│   ├── UsersController.cs                  →  │   ├── UsersControllerTests.cs
│   └── OrdersController.cs                 →  │   └── OrdersControllerTests.cs
└── Middleware/                             →  └── Middleware/
    └── ExceptionMiddleware.cs              →      └── ExceptionMiddlewareTests.cs

src/ADP.Blazor.WebApp/                      →  tests/ADP.Blazor.WebApp.Tests/
├── Components/                             →  ├── Components/
│   ├── Atoms/                              →  │   ├── Atoms/
│   │   └── Buttons/                        →  │   │   └── Buttons/
│   │       ├── PrimaryButton.razor         →  │   │       ├── PrimaryButtonTests.cs
│   │       ├── PrimaryButton.razor.cs      →  │   │       └── (testé via .razor)
│   │       └── PrimaryButton.razor.css     →  │   │           
│   ├── Molecules/                          →  │   ├── Molecules/
│   │   └── Forms/                          →  │   │   └── Forms/
│   │       └── SearchBox.razor             →  │   │       └── SearchBoxTests.cs
│   ├── Organisms/                          →  │   ├── Organisms/
│   │   └── Tables/                         →  │   │   └── Tables/
│   │       └── UsersTable.razor            →  │   │       └── UsersTableTests.cs
│   └── Pages/                              →  │   └── Pages/
│       └── Areas/                          →  │       └── Areas/
│           └── Admin/                      →  │           └── Admin/
│               └── Users/                  →  │               └── Users/
│                   └── List/               →  │                   └── List/
│                       └── UsersList.razor →  │                       └── UsersListTests.cs
└── Program.cs                              →  └── (tests d'intégration séparés)
```

**Structure COMPLÈTE des projets de test :**

```
tests/
├── ADP.Application.Services.Tests/
│   ├── ADP.Application.Services.Tests.csproj
│   ├── Services/                          ← MÊME arborescence que src/
│   │   ├── UserServiceTests.cs            ← Pour UserService.cs
│   │   ├── ClaimsServiceTests.cs          ← Pour ClaimsService.cs
│   │   └── OrderServiceTests.cs           ← Pour OrderService.cs
│   ├── DTOs/                              ← MÊME arborescence que src/
│   │   └── UserDtoTests.cs                ← Pour UserDto.cs (si validation)
│   ├── Fixtures/                          ← Dossier SUPPLÉMENTAIRE pour helpers
│   │   ├── UserFixtures.cs
│   │   └── TestDataGenerator.cs
│   └── Helpers/                           ← Dossier SUPPLÉMENTAIRE pour helpers
│       └── TestHelper.cs
│
├── ADP.Domain.Core.Tests/
│   ├── ADP.Domain.Core.Tests.csproj
│   ├── Entities/                          ← MÊME arborescence que src/
│   │   ├── UserTests.cs                   ← Pour User.cs
│   │   ├── OrderTests.cs                  ← Pour Order.cs
│   │   └── ProductTests.cs                ← Pour Product.cs
│   └── Fixtures/                          ← Dossier SUPPLÉMENTAIRE
│       └── EntityFixtures.cs
│
├── ADP.Demo.WebApi.Tests/
│   ├── ADP.Demo.WebApi.Tests.csproj
│   ├── Controllers/                       ← MÊME arborescence que src/
│   │   ├── UsersControllerTests.cs        ← Pour UsersController.cs
│   │   └── OrdersControllerTests.cs       ← Pour OrdersController.cs
│   ├── Middleware/                        ← MÊME arborescence que src/
│   │   └── ExceptionMiddlewareTests.cs    ← Pour ExceptionMiddleware.cs
│   ├── Integration/                       ← Dossier SUPPLÉMENTAIRE pour tests d'intégration
│   │   ├── UsersApiTests.cs
│   │   └── CustomWebApplicationFactory.cs
│   └── Fixtures/                          ← Dossier SUPPLÉMENTAIRE
│       └── ApiFixtures.cs
│
└── ADP.Blazor.WebApp.Tests/
    ├── ADP.Blazor.WebApp.Tests.csproj
    ├── Components/                        ← MÊME arborescence que src/
    │   ├── Atoms/
    │   │   ├── Buttons/
    │   │   │   ├── PrimaryButtonTests.cs       ← Pour PrimaryButton.razor
    │   │   │   ├── SecondaryButtonTests.cs     ← Pour SecondaryButton.razor
    │   │   │   └── IconButtonTests.cs          ← Pour IconButton.razor
    │   │   └── Inputs/
    │   │       ├── TextFieldTests.cs           ← Pour TextField.razor
    │   │       └── NumberFieldTests.cs         ← Pour NumberField.razor
    │   ├── Molecules/
    │   │   └── Forms/
    │   │       ├── SearchBoxTests.cs           ← Pour SearchBox.razor
    │   │       └── FormFieldTests.cs           ← Pour FormField.razor
    │   ├── Organisms/
    │   │   ├── Forms/
    │   │   │   └── LoginFormTests.cs           ← Pour LoginForm.razor
    │   │   └── Tables/
    │   │       └── UsersTableTests.cs          ← Pour UsersTable.razor
    │   └── Pages/
    │       └── Areas/
    │           └── Administration/
    │               └── Users/
    │                   └── List/
    │                       └── UsersListTests.cs  ← Pour UsersList.razor
    ├── Fixtures/                          ← Dossier SUPPLÉMENTAIRE
    │   └── ComponentFixtures.cs
    └── Helpers/                           ← Dossier SUPPLÉMENTAIRE
        └── BunitTestContext.cs
```

**Règles de correspondance OBLIGATOIRES :**

1. **Un fichier source = Un fichier de test**
   ```
   UserService.cs → UserServiceTests.cs
   UsersController.cs → UsersControllerTests.cs
   PrimaryButton.razor → PrimaryButtonTests.cs
   ```

2. **Même arborescence de dossiers**
   ```
   src/Project/Services/User/UserService.cs
   tests/Project.Tests/Services/User/UserServiceTests.cs
   ```

3. **Nommage : `{ClassName}Tests.cs`**
   ```
   ✅ UserService.cs → UserServiceTests.cs
   ✅ OrderValidator.cs → OrderValidatorTests.cs
   ❌ UserService.cs → TestUserService.cs (INCORRECT)
   ❌ UserService.cs → User_Tests.cs (INCORRECT)
   ```

4. **Composants Blazor : Tester le .razor, pas le .razor.cs**
   ```
   PrimaryButton.razor     ← Fichier principal à tester
   PrimaryButton.razor.cs  ← Code-behind (testé via .razor)
   PrimaryButton.razor.css ← Pas de test
   →
   PrimaryButtonTests.cs   ← UN SEUL fichier de test
   ```

5. **Dossiers supplémentaires autorisés :**
   - `Fixtures/` - Données de test réutilisables
   - `Helpers/` - Utilitaires de test
   - `Integration/` - Tests d'intégration API
   - `Builders/` - Builder pattern pour fixtures

**Exemple complet de correspondance :**

```
src/ADP.Blazor.WebApp/Components/
└── Pages/
    └── Areas/
        └── Administration/
            └── Users/
                └── List/
                    ├── UsersList.razor
                    ├── UsersList.razor.cs
                    └── UsersList.razor.css

tests/ADP.Blazor.WebApp.Tests/Components/
└── Pages/
    └── Areas/
        └── Administration/
            └── Users/
                └── List/
                    └── UsersListTests.cs  ← UN SEUL fichier de test
```

### 0.3. Convention de Nommage (OBLIGATOIRE)

**TOUJOURS suivre cette convention :**

1. **Fichiers de test** : `{ClasseTestée}Tests.cs`
   ```
   UserService.cs → UserServiceTests.cs
   UsersController.cs → UsersControllerTests.cs
   PrimaryButton.razor → PrimaryButtonTests.cs
   ```

2. **Méthodes de test** : `{MethodeTested}_{Scenario}_{ExpectedResult}`
   ```csharp
   [Fact]
   public void GetUser_WithValidId_ReturnsUser() { }
   
   [Fact]
   public void GetUser_WithInvalidId_ThrowsNotFoundException() { }
   
   [Fact]
   public void CreateUser_WithDuplicateEmail_ThrowsDuplicateException() { }
   ```

3. **Classes de fixtures** : `{Domaine}Fixtures.cs`
   ```
   UserFixtures.cs
   OrderFixtures.cs
   ComponentFixtures.cs
   ```

### 0.4. Exemples de Correspondance Fichier Source ↔ Fichier Test

**🎯 RÈGLE D'OR : Même structure de dossiers + Même nom de fichier + Suffixe "Tests"**

```
Structure source                    →  Structure test
═══════════════════════════════════════════════════════════════════════════
src/Projet/                         →  tests/Projet.Tests/
  ├── Dossier1/                     →    ├── Dossier1/           ✅ MÊME dossier
  │   └── ClasseA.cs                →    │   └── ClasseATests.cs ✅ MÊME nom + Tests
  └── Dossier2/                     →    └── Dossier2/           ✅ MÊME dossier
      ├── Sous/                     →        ├── Sous/           ✅ MÊME sous-dossier
      │   └── ClasseB.cs            →        │   └── ClasseBTests.cs
      └── ClasseC.cs                →        └── ClasseCTests.cs
```

**Services (Application Layer) :**
```
📁 src/ADP.Application.Services/Services/
   ├── 📄 UserService.cs
   ├── 📄 ClaimsService.cs
   ├── 📄 OrderService.cs
   └── 📄 EmailService.cs

📁 tests/ADP.Application.Services.Tests/Services/
   ├── 📄 UserServiceTests.cs        ← Teste UserService.cs
   ├── 📄 ClaimsServiceTests.cs      ← Teste ClaimsService.cs
   ├── 📄 OrderServiceTests.cs       ← Teste OrderService.cs
   └── 📄 EmailServiceTests.cs       ← Teste EmailService.cs
```

**Entities (Domain Layer) :**
```
📁 src/ADP.Domain.Core/Entities/
   ├── 📄 User.cs
   ├── 📄 Order.cs
   ├── 📄 Product.cs
   └── 📁 ValueObjects/
       ├── 📄 Email.cs
       └── 📄 Money.cs

📁 tests/ADP.Domain.Core.Tests/Entities/
   ├── 📄 UserTests.cs               ← Teste User.cs
   ├── 📄 OrderTests.cs              ← Teste Order.cs
   ├── 📄 ProductTests.cs            ← Teste Product.cs
   └── 📁 ValueObjects/
       ├── 📄 EmailTests.cs          ← Teste Email.cs
       └── 📄 MoneyTests.cs          ← Teste Money.cs
```

**Controllers (API Layer) :**
```
📁 src/ADP.Demo.WebApi/Controllers/
   ├── 📄 UsersController.cs
   ├── 📄 OrdersController.cs
   └── 📁 V2/
       └── 📄 UsersController.cs

📁 tests/ADP.Demo.WebApi.Tests/Controllers/
   ├── 📄 UsersControllerTests.cs    ← Teste UsersController.cs
   ├── 📄 OrdersControllerTests.cs   ← Teste OrdersController.cs
   └── 📁 V2/
       └── 📄 UsersControllerTests.cs ← Teste V2/UsersController.cs
```

**Composants Blazor (Atoms) :**
```
📁 src/ADP.Blazor.WebApp/Components/Atoms/Buttons/
   ├── 📄 PrimaryButton.razor
   ├── 📄 PrimaryButton.razor.cs
   ├── 📄 PrimaryButton.razor.css
   ├── 📄 SecondaryButton.razor
   ├── 📄 SecondaryButton.razor.cs
   └── 📄 IconButton.razor

📁 tests/ADP.Blazor.WebApp.Tests/Components/Atoms/Buttons/
   ├── 📄 PrimaryButtonTests.cs      ← Teste PrimaryButton.razor + .razor.cs
   ├── 📄 SecondaryButtonTests.cs    ← Teste SecondaryButton.razor + .razor.cs
   └── 📄 IconButtonTests.cs         ← Teste IconButton.razor
```

**Composants Blazor (Pages avec arborescence complexe) :**
```
📁 src/ADP.Blazor.WebApp/Components/Pages/Areas/Administration/Users/
   ├── 📁 List/
   │   ├── 📄 UsersList.razor
   │   ├── 📄 UsersList.razor.cs
   │   └── 📄 UsersList.razor.css
   ├── 📁 Edit/
   │   ├── 📄 UsersEdit.razor
   │   └── 📄 UsersEdit.razor.cs
   └── 📁 Create/
       ├── 📄 UsersCreate.razor
       └── 📄 UsersCreate.razor.cs

📁 tests/ADP.Blazor.WebApp.Tests/Components/Pages/Areas/Administration/Users/
   ├── 📁 List/
   │   └── 📄 UsersListTests.cs      ← Teste UsersList.razor
   ├── 📁 Edit/
   │   └── 📄 UsersEditTests.cs      ← Teste UsersEdit.razor
   └── 📁 Create/
       └── 📄 UsersCreateTests.cs    ← Teste UsersCreate.razor
```

**✅ BON - Respecte l'arborescence :**
```
src/ADP.Application.Services/Services/Orders/OrderService.cs
tests/ADP.Application.Services.Tests/Services/Orders/OrderServiceTests.cs
```

**❌ MAUVAIS - Ne respecte PAS l'arborescence :**
```
src/ADP.Application.Services/Services/Orders/OrderService.cs
tests/ADP.Application.Services.Tests/Services/OrderServiceTests.cs  ← Dossier Orders/ manquant
```

**❌ MAUVAIS - Plusieurs fichiers de test pour un fichier source :**
```
src/ADP.Application.Services/Services/UserService.cs
tests/ADP.Application.Services.Tests/Services/UserServiceTests.cs
tests/ADP.Application.Services.Tests/Services/UserServiceIntegrationTests.cs  ← INCORRECT
```

**✅ BON - Un seul fichier avec des catégories :**
```csharp
// UserServiceTests.cs
public class UserServiceTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void GetUser_WithValidId_ReturnsUser() { }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task GetUser_WithDatabase_ReturnsUser() { }
}
```

### 0.5. Pattern AAA (Arrange-Act-Assert) - OBLIGATOIRE

**TOUS les tests DOIVENT suivre le pattern AAA avec séparation visuelle :**

```csharp
[Fact]
public void GetUser_WithValidId_ReturnsUser()
{
    // Arrange
    var userId = 1;
    var expectedUser = UserFixtures.CreateValidUser(userId);
    var repository = Substitute.For<IUserRepository>();
    repository.GetByIdAsync(userId).Returns(expectedUser);
    var service = new UserService(repository);

    // Act
    var result = await service.GetUserAsync(userId);

    // Assert
    Check.That(result).IsNotNull();
    Check.That(result.Id).IsEqualTo(userId);
    Check.That(result.Name).IsEqualTo(expectedUser.Name);
}
```

**❌ INTERDIT - Mélanger les sections :**
```csharp
[Fact]
public void BadTest()
{
    var service = new UserService(); // Arrange
    var result = service.GetUser(1); // Act
    var expected = new User(); // Arrange - MAUVAIS : après Act
    Check.That(result).IsEqualTo(expected); // Assert
}
```

## 1. xUnit - Framework de Test

### 1.1. Attributs xUnit Obligatoires

**[Fact] - Test simple sans paramètres :**
```csharp
[Fact]
public void Add_TwoNumbers_ReturnsSum()
{
    // Arrange
    var calculator = new Calculator();

    // Act
    var result = calculator.Add(2, 3);

    // Assert
    Check.That(result).IsEqualTo(5);
}
```

**[Theory] + [InlineData] - Tests paramétrés :**
```csharp
[Theory]
[InlineData(2, 3, 5)]
[InlineData(0, 0, 0)]
[InlineData(-1, 1, 0)]
[InlineData(10, -5, 5)]
public void Add_VariousInputs_ReturnsExpectedSum(int a, int b, int expected)
{
    // Arrange
    var calculator = new Calculator();

    // Act
    var result = calculator.Add(a, b);

    // Assert
    Check.That(result).IsEqualTo(expected);
}
```

**[Theory] + [MemberData] - Données complexes :**
```csharp
public class UserServiceTests
{
    [Theory]
    [MemberData(nameof(GetUserTestData))]
    public void CreateUser_VariousScenarios_ReturnsExpectedResult(
        User user, bool shouldSucceed, string expectedMessage)
    {
        // Arrange
        var service = new UserService();

        // Act & Assert
        if (shouldSucceed)
        {
            var result = service.CreateUser(user);
            Check.That(result).IsNotNull();
        }
        else
        {
            Check.ThatCode(() => service.CreateUser(user))
                .Throws<ValidationException>()
                .WithMessage(expectedMessage);
        }
    }

    public static IEnumerable<object[]> GetUserTestData()
    {
        yield return new object[] 
        { 
            new User { Name = "John", Email = "john@test.com" }, 
            true, 
            null 
        };
        yield return new object[] 
        { 
            new User { Name = "", Email = "invalid" }, 
            false, 
            "Invalid user data" 
        };
    }
}
```

### 1.2. Fixtures et Partage de Contexte

**IClassFixture - Partage d'instance entre tests d'une classe :**
```csharp
public class DatabaseFixture : IDisposable
{
    public DbContext Context { get; private set; }

    public DatabaseFixture()
    {
        Context = new TestDbContext();
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}

public class UserRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Arrange
        var repository = new UserRepository(_fixture.Context);
        
        // Act & Assert
        // ...
    }
}
```

**ICollectionFixture - Partage entre plusieurs classes de test :**
```csharp
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // Cette classe n'a pas de code, elle sert uniquement à définir la collection
}

[Collection("Database collection")]
public class UserRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}

[Collection("Database collection")]
public class OrderRepositoryTests
{
    private readonly DatabaseFixture _fixture;

    public OrderRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}
```

### 1.3. Tests Asynchrones

**TOUJOURS utiliser async/await correctement :**
```csharp
[Fact]
public async Task GetUserAsync_WithValidId_ReturnsUser()
{
    // Arrange
    var repository = Substitute.For<IUserRepository>();
    repository.GetByIdAsync(1).Returns(Task.FromResult(new User { Id = 1 }));
    var service = new UserService(repository);

    // Act
    var result = await service.GetUserAsync(1);

    // Assert
    Check.That(result).IsNotNull();
    Check.That(result.Id).IsEqualTo(1);
}
```

## 2. NFluent - Assertions Fluides

### 2.1. Assertions de Base

**Check.That - Assertions simples :**
```csharp
[Fact]
public void NFluentBasicAssertions()
{
    // Égalité
    Check.That(42).IsEqualTo(42);
    Check.That("hello").IsEqualTo("hello");
    
    // Null checks
    Check.That(obj).IsNotNull();
    Check.That(obj).IsNull();
    
    // Booléens
    Check.That(true).IsTrue();
    Check.That(false).IsFalse();
    
    // Comparaisons numériques
    Check.That(10).IsGreaterThan(5);
    Check.That(5).IsLessThan(10);
    Check.That(10).IsGreaterOrEqualThan(10);
    Check.That(5).IsLessOrEqualThan(5);
    
    // Types
    Check.That(obj).IsInstanceOf<User>();
    Check.That(obj).IsNotInstanceOf<Admin>();
}
```

### 2.2. Assertions sur Collections

```csharp
[Fact]
public void NFluentCollectionAssertions()
{
    var list = new List<int> { 1, 2, 3, 4, 5 };
    
    // Taille
    Check.That(list).HasSize(5);
    Check.That(list).IsNotEmpty();
    Check.That(emptyList).IsEmpty();
    
    // Contenu
    Check.That(list).Contains(3);
    Check.That(list).ContainsExactly(1, 2, 3, 4, 5);
    Check.That(list).ContainsOnlyElementsThatMatch(x => x > 0);
    
    // Ordre
    Check.That(list).IsInAscendingOrder();
    
    // Sous-ensembles
    Check.That(list).ContainsOnlyElementsOfType<int>();
}
```

### 2.3. Assertions sur Strings

```csharp
[Fact]
public void NFluentStringAssertions()
{
    var text = "Hello World";
    
    Check.That(text).StartsWith("Hello");
    Check.That(text).EndsWith("World");
    Check.That(text).Contains("lo Wo");
    Check.That(text).HasSize(11);
    Check.That(text).Matches("Hello.*");
    Check.That(text).IsNotEmpty();
    
    // Insensible à la casse
    Check.That(text).IsEqualIgnoringCase("hello world");
}
```

### 2.4. Assertions sur Exceptions

```csharp
[Fact]
public void NFluentExceptionAssertions()
{
    // Vérifier qu'une exception est levée
    Check.ThatCode(() => service.ThrowException())
        .Throws<InvalidOperationException>();
    
    // Vérifier le message d'exception
    Check.ThatCode(() => service.ThrowException())
        .Throws<InvalidOperationException>()
        .WithMessage("Invalid operation");
    
    // Vérifier qu'aucune exception n'est levée
    Check.ThatCode(() => service.ValidOperation())
        .DoesNotThrow();
    
    // Vérifier une propriété de l'exception
    Check.ThatCode(() => service.ThrowCustomException())
        .Throws<CustomException>()
        .And
        .WhichMember(e => e.ErrorCode)
        .IsEqualTo(404);
}
```

### 2.5. Assertions sur Objets

```csharp
[Fact]
public void NFluentObjectAssertions()
{
    var user = new User { Id = 1, Name = "John", Email = "john@test.com" };
    
    // Propriétés
    Check.That(user.Id).IsEqualTo(1);
    Check.That(user.Name).IsEqualTo("John");
    
    // Objets complexes
    var expectedUser = new User { Id = 1, Name = "John", Email = "john@test.com" };
    Check.That(user).HasFieldsWithSameValues(expectedUser);
    
    // Propriété spécifique
    Check.That(user).HasFieldsEqualToThose(new { Id = 1, Name = "John" });
}
```

## 3. NSubstitute - Mocks et Stubs

### 3.1. Création de Substituts

```csharp
[Fact]
public void CreateSubstitutes()
{
    // Interface
    var service = Substitute.For<IUserService>();
    
    // Classe abstraite
    var repository = Substitute.For<BaseRepository>();
    
    // Multiple interfaces
    var handler = Substitute.For<IRequestHandler, IDisposable>();
    
    // Délégué
    var func = Substitute.For<Func<int, string>>();
}
```

### 3.2. Configuration des Retours

```csharp
[Fact]
public void ConfigureReturns()
{
    var service = Substitute.For<IUserService>();
    
    // Retour simple
    service.GetUser(1).Returns(new User { Id = 1, Name = "John" });
    
    // Retour avec fonction
    service.GetUser(Arg.Any<int>()).Returns(x => new User 
    { 
        Id = (int)x[0], 
        Name = $"User{x[0]}" 
    });
    
    // Retours multiples (séquence)
    service.GetStatus()
        .Returns("Pending", "Processing", "Completed");
    
    // Retour asynchrone
    service.GetUserAsync(1).Returns(Task.FromResult(new User { Id = 1 }));
    
    // Retour avec délai
    service.GetUserAsync(1).Returns(async x => 
    {
        await Task.Delay(100);
        return new User { Id = 1 };
    });
}
```

### 3.3. Arguments Matchers

```csharp
[Fact]
public void ArgumentMatchers()
{
    var service = Substitute.For<IUserService>();
    
    // N'importe quelle valeur
    service.GetUser(Arg.Any<int>()).Returns(new User());
    
    // Condition spécifique
    service.GetUser(Arg.Is<int>(x => x > 0)).Returns(new User());
    
    // Prédicat
    service.GetUser(Arg.Is<int>(x => x % 2 == 0)).Returns(new User());
    
    // Capture d'argument
    int capturedId = 0;
    service.GetUser(Arg.Do<int>(x => capturedId = x));
    
    // Valeur spécifique
    service.GetUser(42).Returns(new User { Id = 42 });
}
```

### 3.4. Vérification des Appels

```csharp
[Fact]
public void VerifyCalls()
{
    var service = Substitute.For<IUserService>();
    
    // Appeler la méthode
    service.GetUser(1);
    service.GetUser(2);
    
    // Vérifier qu'elle a été appelée
    service.Received(1).GetUser(1);
    service.Received(2).GetUser(Arg.Any<int>());
    
    // Vérifier qu'elle n'a PAS été appelée
    service.DidNotReceive().DeleteUser(Arg.Any<int>());
    
    // Vérifier l'ordre des appels
    Received.InOrder(() =>
    {
        service.GetUser(1);
        service.GetUser(2);
    });
}
```

### 3.5. Exceptions et Callbacks

```csharp
[Fact]
public void ThrowExceptions()
{
    var service = Substitute.For<IUserService>();
    
    // Lever une exception
    service.When(x => x.DeleteUser(Arg.Any<int>()))
        .Do(x => throw new InvalidOperationException("Cannot delete"));
    
    // Callback avant retour
    service.GetUser(Arg.Any<int>())
        .Returns(x => new User())
        .AndDoes(x => Console.WriteLine($"Getting user {x[0]}"));
}
```

### 3.6. Propriétés et Événements

```csharp
[Fact]
public void PropertiesAndEvents()
{
    var service = Substitute.For<IUserService>();
    
    // Propriétés
    service.CurrentUser.Returns(new User { Name = "John" });
    
    // Vérifier l'accès à une propriété
    var user = service.CurrentUser;
    var x = service.Received().CurrentUser;
    
    // Événements
    service.UserCreated += Raise.EventWith(new UserEventArgs { UserId = 1 });
}
```

## 4. Bogus - Génération de Données de Test

### 4.1. Faker de Base

```csharp
[Fact]
public void BasicFaker()
{
    var faker = new Faker();
    
    // Données aléatoires
    var name = faker.Name.FullName();
    var email = faker.Internet.Email();
    var address = faker.Address.FullAddress();
    var phone = faker.Phone.PhoneNumber();
    var date = faker.Date.Past();
    var lorem = faker.Lorem.Paragraph();
    var number = faker.Random.Int(1, 100);
}
```

### 4.2. Faker Typé (Recommandé)

```csharp
public class UserFixtures
{
    private static readonly Faker<User> _userFaker = new Faker<User>()
        .RuleFor(u => u.Id, f => f.IndexFaker)
        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
        .RuleFor(u => u.LastName, f => f.Name.LastName())
        .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
        .RuleFor(u => u.DateOfBirth, f => f.Date.Past(50, DateTime.Now.AddYears(-18)))
        .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
        .RuleFor(u => u.Address, f => f.Address.FullAddress())
        .RuleFor(u => u.IsActive, f => f.Random.Bool(0.8f)) // 80% actifs
        .RuleFor(u => u.CreatedAt, f => f.Date.Past(2));

    public static User CreateValidUser(int? id = null)
    {
        var user = _userFaker.Generate();
        if (id.HasValue)
            user.Id = id.Value;
        return user;
    }

    public static List<User> CreateValidUsers(int count)
    {
        return _userFaker.Generate(count);
    }

    public static User CreateInactiveUser()
    {
        return new Faker<User>()
            .RuleFor(u => u.IsActive, false)
            .Generate();
    }
}

[Fact]
public void UseUserFixtures()
{
    // Arrange
    var user = UserFixtures.CreateValidUser();
    var users = UserFixtures.CreateValidUsers(10);

    // Act & Assert
    Check.That(user).IsNotNull();
    Check.That(users).HasSize(10);
}
```

### 4.3. Règles Avancées

```csharp
public class OrderFixtures
{
    private static readonly Faker<OrderItem> _orderItemFaker = new Faker<OrderItem>()
        .RuleFor(o => o.ProductName, f => f.Commerce.ProductName())
        .RuleFor(o => o.Quantity, f => f.Random.Int(1, 10))
        .RuleFor(o => o.Price, f => f.Random.Decimal(1, 1000));

    private static readonly Faker<Order> _orderFaker = new Faker<Order>()
        .RuleFor(o => o.Id, f => f.IndexFaker)
        .RuleFor(o => o.OrderNumber, f => f.Random.AlphaNumeric(10).ToUpper())
        .RuleFor(o => o.CustomerId, f => f.Random.Int(1, 1000))
        .RuleFor(o => o.OrderDate, f => f.Date.Recent(30))
        .RuleFor(o => o.Status, f => f.PickRandom<OrderStatus>())
        .RuleFor(o => o.Items, f => _orderItemFaker.Generate(f.Random.Int(1, 5)))
        .RuleFor(o => o.TotalAmount, (f, o) => o.Items.Sum(i => i.Price * i.Quantity));

    public static Order CreateValidOrder() => _orderFaker.Generate();
    
    public static List<Order> CreateValidOrders(int count) => _orderFaker.Generate(count);
}
```

### 4.4. Locales et Seed

```csharp
[Fact]
public void LocalesAndSeed()
{
    // Locale française
    var fakerFr = new Faker("fr");
    var nameFr = fakerFr.Name.FullName(); // Nom français
    
    // Seed pour reproductibilité
    var faker1 = new Faker { Random = new Randomizer(12345) };
    var faker2 = new Faker { Random = new Randomizer(12345) };
    
    Check.That(faker1.Name.FullName()).IsEqualTo(faker2.Name.FullName());
}
```

### 4.5. Bogus avec Builder Pattern

```csharp
public class UserBuilder
{
    private readonly Faker<User> _faker;
    private int? _id;
    private string _email;
    private bool? _isActive;

    public UserBuilder()
    {
        _faker = new Faker<User>()
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.IsActive, f => f.Random.Bool());
    }

    public UserBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder IsActive()
    {
        _isActive = true;
        return this;
    }

    public UserBuilder IsInactive()
    {
        _isActive = false;
        return this;
    }

    public User Build()
    {
        var user = _faker.Generate();
        
        if (_id.HasValue) user.Id = _id.Value;
        if (_email != null) user.Email = _email;
        if (_isActive.HasValue) user.IsActive = _isActive.Value;
        
        return user;
    }
}

[Fact]
public void UseUserBuilder()
{
    // Arrange
    var user = new UserBuilder()
        .WithId(1)
        .WithEmail("test@example.com")
        .IsActive()
        .Build();

    // Assert
    Check.That(user.Id).IsEqualTo(1);
    Check.That(user.Email).IsEqualTo("test@example.com");
    Check.That(user.IsActive).IsTrue();
}
```

## 5. bUnit - Tests de Composants Blazor

### 5.1. Configuration de Base

```csharp
public class ComponentTestBase : TestContext
{
    protected IStringLocalizer<SharedResources> Localizer { get; private set; }

    public ComponentTestBase()
    {
        // Services communs
        Services.AddMudServices();
        Services.AddLocalization();
        
        // Mock du localizer
        Localizer = Substitute.For<IStringLocalizer<SharedResources>>();
        Services.AddSingleton(Localizer);
        
        // Mock de IJSRuntime
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
```

### 5.2. Tests de Composants Atoms

```csharp
public class PrimaryButtonTests : ComponentTestBase
{
    [Fact]
    public void Render_WithText_DisplaysCorrectly()
    {
        // Arrange
        Localizer["Common.Save"].Returns(new LocalizedString("Common.Save", "Enregistrer"));

        // Act
        var cut = RenderComponent<PrimaryButton>(parameters => parameters
            .Add(p => p.TextKey, "Common.Save"));

        // Assert
        cut.MarkupMatches(@"
            <button class=""mud-button-root mud-button mud-button-filled mud-button-filled-primary"">
                Enregistrer
            </button>");
    }

    [Fact]
    public void Click_WhenEnabled_TriggersOnClick()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<PrimaryButton>(parameters => parameters
            .Add(p => p.OnClick, () => clicked = true));

        // Act
        cut.Find("button").Click();

        // Assert
        Check.That(clicked).IsTrue();
    }

    [Fact]
    public void Click_WhenDisabled_DoesNotTriggerOnClick()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<PrimaryButton>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.OnClick, () => clicked = true));

        // Act
        var button = cut.Find("button");
        Check.ThatCode(() => button.Click()).Throws<ElementDisabledException>();

        // Assert
        Check.That(clicked).IsFalse();
    }
}
```

### 5.3. Tests de Composants Molecules

```csharp
public class SearchBoxTests : ComponentTestBase
{
    [Fact]
    public void Render_WithPlaceholder_DisplaysCorrectly()
    {
        // Arrange
        Localizer["Common.Search"].Returns(new LocalizedString("Common.Search", "Rechercher"));

        // Act
        var cut = RenderComponent<SearchBox>();

        // Assert
        var input = cut.Find("input");
        Check.That(input.GetAttribute("placeholder")).IsEqualTo("Rechercher");
    }

    [Fact]
    public async Task Search_WithText_TriggersOnSearch()
    {
        // Arrange
        string searchedText = null;
        var cut = RenderComponent<SearchBox>(parameters => parameters
            .Add(p => p.OnSearch, text => searchedText = text));

        // Act
        var input = cut.Find("input");
        await cut.InvokeAsync(() => input.Input("test search"));
        var button = cut.Find("button");
        button.Click();

        // Assert
        Check.That(searchedText).IsEqualTo("test search");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Search_WithEmptyText_DoesNotTriggerOnSearch(string searchText)
    {
        // Arrange
        var searchTriggered = false;
        var cut = RenderComponent<SearchBox>(parameters => parameters
            .Add(p => p.OnSearch, _ => searchTriggered = true));

        // Act
        var input = cut.Find("input");
        await cut.InvokeAsync(() => input.Input(searchText));
        var button = cut.Find("button");
        button.Click();

        // Assert
        Check.That(searchTriggered).IsFalse();
    }
}
```

### 5.4. Tests de Composants Organisms

```csharp
public class UsersTableTests : ComponentTestBase
{
    [Fact]
    public void Render_WithUsers_DisplaysAllRows()
    {
        // Arrange
        var users = UserFixtures.CreateValidUsers(5);
        Localizer["Users.ColumnName"].Returns(new LocalizedString("Users.ColumnName", "Nom"));
        Localizer["Users.ColumnEmail"].Returns(new LocalizedString("Users.ColumnEmail", "Email"));

        // Act
        var cut = RenderComponent<UsersTable>(parameters => parameters
            .Add(p => p.Items, users));

        // Assert
        var rows = cut.FindAll("tbody tr");
        Check.That(rows).HasSize(5);
    }

    [Fact]
    public void Render_WithEmptyList_DisplaysNoDataMessage()
    {
        // Arrange
        Localizer["Common.NoData"].Returns(new LocalizedString("Common.NoData", "Aucune donnée"));

        // Act
        var cut = RenderComponent<UsersTable>(parameters => parameters
            .Add(p => p.Items, new List<User>()));

        // Assert
        var noDataElement = cut.Find(".no-data");
        Check.That(noDataElement.TextContent).Contains("Aucune donnée");
    }

    [Fact]
    public async Task DeleteButton_Click_TriggersOnDelete()
    {
        // Arrange
        var user = UserFixtures.CreateValidUser(1);
        int? deletedUserId = null;
        var cut = RenderComponent<UsersTable>(parameters => parameters
            .Add(p => p.Items, new List<User> { user })
            .Add(p => p.OnDelete, id => deletedUserId = id));

        // Act
        var deleteButton = cut.Find("button.delete-button");
        await cut.InvokeAsync(() => deleteButton.Click());

        // Assert
        Check.That(deletedUserId).IsEqualTo(1);
    }

    [Fact]
    public async Task SortColumn_Click_SortsData()
    {
        // Arrange
        var users = new List<User>
        {
            UserFixtures.CreateValidUser(1).With(u => u.Name = "Charlie"),
            UserFixtures.CreateValidUser(2).With(u => u.Name = "Alice"),
            UserFixtures.CreateValidUser(3).With(u => u.Name = "Bob")
        };
        var cut = RenderComponent<UsersTable>(parameters => parameters
            .Add(p => p.Items, users));

        // Act
        var nameColumn = cut.Find("th.sortable-name");
        await cut.InvokeAsync(() => nameColumn.Click());

        // Assert
        var firstRowName = cut.Find("tbody tr:first-child td:first-child").TextContent;
        Check.That(firstRowName).IsEqualTo("Alice");
    }
}
```

### 5.5. Tests de Pages Blazor

```csharp
public class UsersListPageTests : ComponentTestBase
{
    private IUserService _userService;

    public UsersListPageTests()
    {
        _userService = Substitute.For<IUserService>();
        Services.AddSingleton(_userService);
    }

    [Fact]
    public async Task OnInitialized_LoadsUsers()
    {
        // Arrange
        var users = UserFixtures.CreateValidUsers(3);
        _userService.GetAllAsync().Returns(users);

        // Act
        var cut = RenderComponent<UsersListPage>();
        await Task.Delay(100); // Attendre le chargement

        // Assert
        await _userService.Received(1).GetAllAsync();
        var rows = cut.FindAll("tbody tr");
        Check.That(rows).HasSize(3);
    }

    [Fact]
    public async Task OnInitialized_WithError_DisplaysErrorMessage()
    {
        // Arrange
        _userService.GetAllAsync().Returns(Task.FromException<List<User>>(
            new Exception("Database error")));
        Localizer["Errors.LoadFailed"].Returns(
            new LocalizedString("Errors.LoadFailed", "Erreur de chargement"));

        // Act
        var cut = RenderComponent<UsersListPage>();
        await Task.Delay(100);

        // Assert
        var alert = cut.Find(".mud-alert-error");
        Check.That(alert.TextContent).Contains("Erreur de chargement");
    }

    [Fact]
    public async Task CreateButton_Click_NavigatesToCreatePage()
    {
        // Arrange
        var navManager = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<UsersListPage>();

        // Act
        var createButton = cut.Find("button.create-button");
        await cut.InvokeAsync(() => createButton.Click());

        // Assert
        Check.That(navManager.Uri).EndsWith("/admin/users/create");
    }
}
```

### 5.6. Tests avec JSInterop

```csharp
public class ComponentWithJSTests : ComponentTestBase
{
    [Fact]
    public void OnAfterRender_CallsJavaScript()
    {
        // Arrange
        var jsInterop = JSInterop.SetupVoid("initializeComponent");

        // Act
        var cut = RenderComponent<ComponentWithJS>();

        // Assert
        jsInterop.VerifyInvoke("initializeComponent");
    }

    [Fact]
    public async Task MethodThatUsesJS_ReturnsExpectedValue()
    {
        // Arrange
        JSInterop.Setup<string>("getWindowLocation")
            .SetResult("https://example.com");
        var cut = RenderComponent<ComponentWithJS>();

        // Act
        var location = await cut.InvokeAsync(() => cut.Instance.GetLocation());

        // Assert
        Check.That(location).IsEqualTo("https://example.com");
    }
}
```

## 6. Tests d'Intégration

### 6.1. WebApplicationFactory

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remplacer la base de données par une base en mémoire
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);
            
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });
            
            // Initialiser la base de test
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private void SeedTestData(AppDbContext context)
    {
        var users = UserFixtures.CreateValidUsers(10);
        context.Users.AddRange(users);
        context.SaveChanges();
    }
}

public class UsersApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UsersApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_ReturnsSuccessAndUsers()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        Check.That(users).IsNotNull();
        Check.That(users).HasSize(10);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreated()
    {
        // Arrange
        var newUser = new CreateUserDto
        {
            Name = "Test User",
            Email = "test@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", newUser);

        // Assert
        Check.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        var createdUser = await response.Content.ReadFromJsonAsync<UserDto>();
        Check.That(createdUser.Name).IsEqualTo(newUser.Name);
    }
}
```

### 6.2. Tests de Base de Données

```csharp
public class UserRepositoryIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var user = UserFixtures.CreateValidUser(1);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Check.That(result).IsNotNull();
        Check.That(result.Id).IsEqualTo(1);
    }

    [Fact]
    public async Task CreateAsync_WithValidUser_AddsToDatabase()
    {
        // Arrange
        var user = UserFixtures.CreateValidUser();

        // Act
        var result = await _repository.CreateAsync(user);
        var savedUser = await _context.Users.FindAsync(result.Id);

        // Assert
        Check.That(savedUser).IsNotNull();
        Check.That(savedUser.Name).IsEqualTo(user.Name);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

## 7. Bonnes Pratiques

### 7.1. Tests FIRST

**Fast** - Tests rapides
```csharp
// ✅ BON - Test unitaire rapide
[Fact]
public void Add_TwoNumbers_ReturnsSum()
{
    var result = 2 + 3;
    Check.That(result).IsEqualTo(5);
}

// ❌ MAUVAIS - Test lent avec Thread.Sleep
[Fact]
public void SlowTest()
{
    Thread.Sleep(5000); // ÉVITER
    // ...
}
```

**Independent** - Tests indépendants
```csharp
// ✅ BON - Chaque test crée ses propres données
[Fact]
public void Test1()
{
    var user = UserFixtures.CreateValidUser();
    // ...
}

[Fact]
public void Test2()
{
    var user = UserFixtures.CreateValidUser();
    // ...
}

// ❌ MAUVAIS - Tests qui dépendent de l'ordre d'exécution
private User _sharedUser; // ÉVITER

[Fact]
public void Test1_CreatesUser()
{
    _sharedUser = new User();
}

[Fact]
public void Test2_UsesUser()
{
    // Dépend de Test1 - MAUVAIS
    Check.That(_sharedUser).IsNotNull();
}
```

**Repeatable** - Tests reproductibles
```csharp
// ✅ BON - Utiliser Bogus avec seed pour reproductibilité
[Fact]
public void RepeatableTest()
{
    var faker = new Faker { Random = new Randomizer(12345) };
    var name = faker.Name.FullName();
    Check.That(name).IsEqualTo("Alice Smith"); // Toujours le même
}

// ❌ MAUVAIS - Utiliser DateTime.Now
[Fact]
public void UnreliableTest()
{
    var result = CalculateAge(DateTime.Now); // ÉVITER
    Check.That(result).IsEqualTo(25); // Changera avec le temps
}
```

**Self-Validating** - Tests auto-validants
```csharp
// ✅ BON - Assert clair avec NFluent
[Fact]
public void SelfValidatingTest()
{
    var result = service.Process();
    Check.That(result.IsValid).IsTrue();
    Check.That(result.Errors).IsEmpty();
}

// ❌ MAUVAIS - Sans assertion
[Fact]
public void NoAssertionTest()
{
    service.Process(); // Pas d'assertion - MAUVAIS
}
```

**Timely** - Tests écrits à temps (TDD)
```csharp
// 1. RED - Écrire le test qui échoue
[Fact]
public void CalculateDiscount_WithVipCustomer_Returns20Percent()
{
    // Arrange
    var customer = new Customer { IsVip = true };
    var calculator = new DiscountCalculator();

    // Act
    var discount = calculator.Calculate(customer, 100);

    // Assert
    Check.That(discount).IsEqualTo(20);
}

// 2. GREEN - Implémenter le minimum pour passer
// 3. REFACTOR - Améliorer le code
```

### 7.2. Un Concept par Test

```csharp
// ✅ BON - Tests séparés pour chaque cas
[Fact]
public void Divide_TwoNumbers_ReturnsQuotient()
{
    var result = 10 / 2;
    Check.That(result).IsEqualTo(5);
}

[Fact]
public void Divide_ByZero_ThrowsException()
{
    Check.ThatCode(() => 10 / 0).Throws<DivideByZeroException>();
}

// ❌ MAUVAIS - Tester plusieurs concepts dans un test
[Fact]
public void Divide_TestMultipleScenarios()
{
    // MAUVAIS - Trop de responsabilités
    Check.That(10 / 2).IsEqualTo(5);
    Check.That(20 / 4).IsEqualTo(5);
    Check.ThatCode(() => 10 / 0).Throws<DivideByZeroException>();
}
```

### 7.3. Tests Lisibles et Maintenables

```csharp
// ✅ BON - Nom descriptif et AAA clair
[Fact]
public void CreateOrder_WithInvalidItems_ThrowsValidationException()
{
    // Arrange - Données de test claires
    var order = new Order
    {
        Items = new List<OrderItem>() // Liste vide
    };
    var validator = new OrderValidator();

    // Act - Action claire
    var act = () => validator.Validate(order);

    // Assert - Vérification claire
    Check.ThatCode(act)
        .Throws<ValidationException>()
        .WithMessage("Order must contain at least one item");
}

// ❌ MAUVAIS - Nom vague, logique confuse
[Fact]
public void Test1()
{
    var o = new Order { Items = new List<OrderItem>() };
    var v = new OrderValidator();
    try { v.Validate(o); } catch (Exception e) { Check.That(e).IsInstanceOf<ValidationException>(); }
}
```

### 7.4. Éviter la Logique dans les Tests

```csharp
// ✅ BON - Pas de logique, données explicites
[Theory]
[InlineData(1, 10, 10)]
[InlineData(2, 10, 20)]
[InlineData(5, 10, 50)]
public void Multiply_VariousInputs_ReturnsProduct(int a, int b, int expected)
{
    var result = a * b;
    Check.That(result).IsEqualTo(expected);
}

// ❌ MAUVAIS - Logique dans le test
[Theory]
[InlineData(1, 10)]
[InlineData(2, 10)]
[InlineData(5, 10)]
public void Multiply_VariousInputs_ReturnsProduct(int a, int b)
{
    var expected = a * b; // ÉVITER - Logique dans le test
    var result = Multiply(a, b);
    Check.That(result).IsEqualTo(expected);
}
```

### 7.5. Fixtures Réutilisables

```csharp
// ✅ BON - Fixtures centralisées et réutilisables
public static class OrderFixtures
{
    public static Order CreateValidOrder() => new Faker<Order>()
        .RuleFor(o => o.Id, f => f.IndexFaker)
        .RuleFor(o => o.CustomerId, f => f.Random.Int(1, 1000))
        .RuleFor(o => o.Items, f => CreateOrderItems(3))
        .Generate();

    public static Order CreateEmptyOrder() => new Order { Items = new List<OrderItem>() };

    public static Order CreateOrderWithAmount(decimal amount) => new Order
    {
        TotalAmount = amount,
        Items = CreateOrderItems(1)
    };

    private static List<OrderItem> CreateOrderItems(int count) => new Faker<OrderItem>()
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 10))
        .RuleFor(i => i.Price, f => f.Random.Decimal(1, 100))
        .Generate(count);
}

// Utilisation
[Fact]
public void ProcessOrder_WithValidOrder_Succeeds()
{
    var order = OrderFixtures.CreateValidOrder();
    // ...
}
```

### 7.6. Tests de Validation

```csharp
public class CreateUserDtoValidatorTests
{
    private readonly CreateUserDtoValidator _validator;

    public CreateUserDtoValidatorTests()
    {
        _validator = new CreateUserDtoValidator();
    }

    [Fact]
    public void Validate_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "P@ssw0rd123"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Check.That(result.IsValid).IsTrue();
        Check.That(result.Errors).IsEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidName_ReturnsError(string name)
    {
        // Arrange
        var dto = new CreateUserDto { Name = name };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Check.That(result.IsValid).IsFalse();
        Check.That(result.Errors).HasSize(1);
        Check.That(result.Errors[0].PropertyName).IsEqualTo("Name");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Validate_WithInvalidEmail_ReturnsError(string email)
    {
        // Arrange
        var dto = new CreateUserDto { Email = email };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Check.That(result.IsValid).IsFalse();
        Check.That(result.Errors).Contains(e => e.PropertyName == "Email");
    }
}
```

### 7.7. Tests de Performance

```csharp
public class PerformanceTests
{
    [Fact]
    public void ProcessLargeDataSet_CompletesInReasonableTime()
    {
        // Arrange
        var data = Enumerable.Range(1, 10000).ToList();
        var processor = new DataProcessor();
        var stopwatch = Stopwatch.StartNew();

        // Act
        processor.Process(data);
        stopwatch.Stop();

        // Assert
        Check.That(stopwatch.ElapsedMilliseconds).IsLessThan(1000); // < 1 seconde
    }
}
```

### 7.8. Tests Paramétrés Complexes

```csharp
public class ComplexTheoryTests
{
    [Theory]
    [MemberData(nameof(GetValidationTestData))]
    public void ValidateUser_VariousScenarios_ReturnsExpectedResult(
        User user, bool expectedValid, string expectedError)
    {
        // Arrange
        var validator = new UserValidator();

        // Act
        var result = validator.Validate(user);

        // Assert
        Check.That(result.IsValid).IsEqualTo(expectedValid);
        if (!expectedValid)
        {
            Check.That(result.Errors).Contains(expectedError);
        }
    }

    public static IEnumerable<object[]> GetValidationTestData()
    {
        yield return new object[]
        {
            new User { Name = "John", Email = "john@test.com" },
            true,
            null
        };
        yield return new object[]
        {
            new User { Name = "", Email = "john@test.com" },
            false,
            "Name is required"
        };
        yield return new object[]
        {
            new User { Name = "John", Email = "invalid-email" },
            false,
            "Email is invalid"
        };
    }
}
```

## 8. Organisation et Exécution des Tests

### 8.1. Catégories de Tests (Traits)

```csharp
// Tests unitaires rapides
[Trait("Category", "Unit")]
[Fact]
public void UnitTest() { }

// Tests d'intégration plus lents
[Trait("Category", "Integration")]
[Fact]
public void IntegrationTest() { }

// Tests UI avec bUnit
[Trait("Category", "UI")]
[Fact]
public void UITest() { }

// Exécuter uniquement les tests unitaires :
// dotnet test --filter "Category=Unit"
```

### 8.2. Tests à Ignorer Temporairement

```csharp
[Fact(Skip = "En attente de correction du bug #1234")]
public void TemporarilyDisabledTest()
{
    // Test désactivé temporairement
}
```

### 8.3. Parallel Execution

```csharp
// Désactiver le parallélisme pour une collection
[Collection("Sequential")]
public class SequentialTests
{
    // Ces tests s'exécutent séquentiellement
}

// Activer le parallélisme (par défaut dans xUnit)
// Les tests dans des classes différentes s'exécutent en parallèle
```

## 9. Checklist - Tests de Qualité

### ✅ Avant de Commit

1. **Structure**
   - [ ] Tous les tests suivent le pattern AAA (Arrange-Act-Assert)
   - [ ] Noms de tests descriptifs : `{Method}_{Scenario}_{ExpectedResult}`
   - [ ] Un seul concept par test
   - [ ] Tests organisés dans la bonne structure de dossiers

2. **Assertions**
   - [ ] Utilise NFluent pour toutes les assertions
   - [ ] Assertions claires et spécifiques
   - [ ] Messages d'erreur compréhensibles

3. **Données de Test**
   - [ ] Utilise Bogus pour générer les données
   - [ ] Fixtures réutilisables dans le dossier `Fixtures/`
   - [ ] Pas de valeurs magiques en dur

4. **Mocks et Stubs**
   - [ ] Utilise NSubstitute uniquement
   - [ ] Mocks configurés clairement dans Arrange
   - [ ] Vérifications d'appels explicites

5. **Tests Blazor (bUnit)**
   - [ ] TestContext configuré correctement
   - [ ] Services mockés (IStringLocalizer, services métier)
   - [ ] JSInterop mocké si nécessaire
   - [ ] Tests de rendering et d'interactions

6. **Performance**
   - [ ] Tests unitaires < 100ms
   - [ ] Tests d'intégration < 1s
   - [ ] Pas de Thread.Sleep sauf si absolument nécessaire

7. **Couverture**
   - [ ] Tous les cas nominaux testés
   - [ ] Tous les cas d'erreur testés
   - [ ] Cas limites couverts

8. **Exécution**
   - [ ] Tous les tests passent
   - [ ] Pas de tests ignorés sans raison valable
   - [ ] Tests indépendants (ordre d'exécution n'importe pas)

### 🚀 Commandes Utiles

```bash
# Exécuter tous les tests
dotnet test

# Exécuter avec couverture de code
dotnet test --collect:"XPlat Code Coverage"

# Exécuter uniquement les tests unitaires
dotnet test --filter "Category=Unit"

# Exécuter avec verbosité détaillée
dotnet test --logger "console;verbosity=detailed"

# Exécuter les tests d'un projet spécifique
dotnet test tests/ADP.Application.Services.Tests/

# Exécuter un test spécifique
dotnet test --filter "FullyQualifiedName~UserServiceTests.GetUser_WithValidId_ReturnsUser"
```

## 10. Anti-Patterns à Éviter

### ❌ Tests qui Testent le Framework

```csharp
// MAUVAIS - Teste Entity Framework, pas votre code
[Fact]
public void DbContext_CanSaveEntity()
{
    var context = new AppDbContext();
    var user = new User { Name = "Test" };
    context.Users.Add(user);
    context.SaveChanges();
    
    Check.That(context.Users.Count()).IsEqualTo(1);
}
```

### ❌ Tests avec Dépendances Externes

```csharp
// MAUVAIS - Dépend d'une vraie API
[Fact]
public async Task GetWeather_ReturnsData()
{
    var client = new HttpClient();
    var response = await client.GetAsync("https://api.weather.com/data");
    // ...
}
```

### ❌ Tests Fragiles

```csharp
// MAUVAIS - Teste l'implémentation, pas le comportement
[Fact]
public void ProcessOrder_CallsRepositorySaveExactlyOnce()
{
    var repo = Substitute.For<IRepository>();
    var service = new OrderService(repo);
    
    service.ProcessOrder(order);
    
    // Trop couplé à l'implémentation
    repo.Received(1).Save(Arg.Any<Order>());
}
```

### ❌ Tests avec Logique Complexe

```csharp
// MAUVAIS - Trop de logique dans le test
[Fact]
public void ComplexTest()
{
    var users = GetUsers();
    var result = new List<User>();
    
    foreach (var user in users)
    {
        if (user.IsActive && user.Age > 18)
        {
            result.Add(user);
        }
    }
    
    Check.That(result).HasSize(expected); // Comment calculer expected?
}
```

---

## Résumé - Stack de Test Obligatoire

| Outil | Usage | Alternatives Interdites |
|-------|-------|------------------------|
| **xUnit** | Framework de test | ❌ NUnit, MSTest |
| **NFluent** | Assertions fluides | ❌ FluentAssertions, Assert.* |
| **NSubstitute** | Mocks et stubs | ❌ Moq, FakeItEasy |
| **Bogus** | Données de test | ❌ AutoFixture, données en dur |
| **bUnit** | Tests composants Blazor | ❌ Tests manuels |

**Toujours suivre :**
1. Pattern AAA (Arrange-Act-Assert)
2. Convention de nommage : `{Method}_{Scenario}_{ExpectedResult}`
3. Tests FIRST (Fast, Independent, Repeatable, Self-Validating, Timely)
4. TDD quand possible (Red-Green-Refactor)
5. Fixtures réutilisables avec Bogus
6. Mocks clairs avec NSubstitute
7. Assertions expressives avec NFluent
8. Tests Blazor structurés avec bUnit
