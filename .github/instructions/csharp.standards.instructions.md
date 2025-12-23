---
description: C# coding standards - naming, formatting, best practices, and patterns
name: CSharp_Coding_Standards
applyTo: "**/backend/**/*.cs"
---

# Standards C# - Conventions et Bonnes Pratiques

Guide complet des standards de codage C# : nommage, formatage, patterns, et principes SOLID.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** de noms abrégés ou cryptiques (`str`, `mgr`, `cnt`)
- **Ne crée jamais** de méthode >20 lignes sans extraire
- **N'utilise jamais** de nombres magiques (constantes nommées obligatoires)
- **Ne laisse jamais** de code commenté ou mort
- **N'utilise jamais** de `var` quand le type n'est pas évident
- **Ne mélange jamais** plusieurs niveaux d'abstraction dans une méthode
- **N'ignore jamais** les warnings du compilateur

## ✅ À FAIRE

- **Nomme toujours** explicitement (PascalCase classes, camelCase variables, _camelCase champs privés)
- **Garde toujours** les méthodes courtes et focalisées (<20 lignes idéalement)
- **Utilise toujours** des constantes nommées pour les valeurs littérales
- **Préfère toujours** la composition à l'héritage
- **Utilise toujours** les expressions switch C# modernes
- **Applique toujours** early return pour réduire l'imbrication
- **Traite toujours** tous les warnings comme des erreurs (zléro warning)

## 🎯 Principes Fondamentaux (OBLIGATOIRES)

**RESPECTER systématiquement ces 5 principes :**

1. ✅ **LISIBILITÉ** : Le code est lu 10 fois plus qu'il n'est écrit
   - Noms explicites et descriptifs
   - Fonctions courtes (< 20 lignes idéalement)
   - Un seul niveau d'abstraction par fonction

2. ✅ **SIMPLICITÉ** : La solution la plus simple qui fonctionne
   - Éviter la sur-ingénierie
   - Préférer la composition à l'héritage
   - Pas de code spéculatif ("on en aura peut-être besoin")

3. ✅ **COHÉRENCE** : Appliquer les mêmes règles partout
   - Conventions de nommage uniformes
   - Structure de fichiers identique
   - Patterns similaires pour problèmes similaires

4. ✅ **MAINTENABILITÉ** : Faciliter les modifications futures
   - Code auto-documenté
   - Couplage faible, cohésion forte
   - Tests unitaires systématiques

5. ✅ **PERFORMANCE** : Optimiser selon les besoins réels
   - Mesurer avant d'optimiser
   - Éviter les allocations inutiles
   - Préférer les structures de données appropriées

## 📝 Conventions de Nommage

### Règles Générales

**TOUJOURS respecter la casse appropriée :**

| Type | Convention | Exemple |
|------|-----------|---------|
| **Namespace** | PascalCase | `GroupeAdp.GenAi.Domain` |
| **Class** | PascalCase | `UserService` |
| **Interface** | I + PascalCase | `IUserRepository` |
| **Method** | PascalCase | `GetUserById` |
| **Property** | PascalCase | `FirstName` |
| **Field (private)** | _camelCase | `_userRepository` |
| **Field (const)** | PascalCase | `MaxRetryCount` |
| **Parameter** | camelCase | `userId` |
| **Local variable** | camelCase | `userName` |
| **Enum** | PascalCase | `OrderStatus` |
| **Enum member** | PascalCase | `Pending`, `Confirmed` |

### Classes et Interfaces

```csharp
// ✅ BON - Noms clairs et descriptifs
public class UserAuthenticationService { }
public interface IEmailNotificationService { }
public record UserRegistrationRequest(string Email, string Password);
public abstract class BaseRepository<TEntity> { }

// ❌ MAUVAIS - Noms vagues ou abrégés
public class UsrAuthSvc { }  // Abréviations
public interface EmailService { }  // Manque le préfixe I
public class Manager { }  // Trop générique
public class Helper { }  // Trop vague
```

**Règles de nommage des classes :**
- Noms singuliers (sauf collections : `Users`, `Orders`)
- Suffixes explicites pour patterns :
  - `Service` : Logique métier
  - `Repository` : Accès aux données
  - `Controller` : Point d'entrée API
  - `Validator` : Validation de données
  - `Factory` : Création d'objets complexes
  - `Handler` : Traitement de commandes/requêtes
  - `Provider` : Fournisseur de services
  - `Manager` : Coordination de plusieurs services (à éviter si possible)

### Méthodes

```csharp
// ✅ BON - Verbes d'action clairs
public async Task<User> GetUserByIdAsync(Guid userId) { }
public bool ValidateEmail(string email) { }
public void SendNotification(string recipient, string message) { }
public IEnumerable<Product> FilterActiveProducts() { }
public decimal CalculateTotalPrice(IEnumerable<OrderLine> lines) { }

// ❌ MAUVAIS - Noms ambigus
public async Task<User> User(Guid id) { }  // Pas de verbe
public bool Check(string email) { }  // Trop vague
public void Do() { }  // Aucune information
public IEnumerable<Product> Products() { }  // Devrait être GetProducts ou similaire
```

**Conventions pour méthodes :**
- **Query methods** : `Get`, `Find`, `Search`, `Query`, `List`
  - `GetById` : Doit retourner un résultat (exception si absent)
  - `FindById` : Peut retourner `null`
  - `Search` : Avec critères multiples
  - `List` : Retourne collection (peut être vide)

- **Command methods** : `Create`, `Update`, `Delete`, `Add`, `Remove`, `Save`
  - Verbes d'action explicites
  - Retournent résultat ou void

- **Boolean methods** : `Is`, `Has`, `Can`, `Should`, `Contains`
  ```csharp
  public bool IsValid() { }
  public bool HasPermission(string permission) { }
  public bool CanExecute() { }
  public bool ContainsItem(string itemId) { }
  ```

- **Async methods** : Toujours suffixe `Async`
  ```csharp
  // ✅ BON
  public async Task<User> GetUserAsync(Guid id) { }
  public async Task SendEmailAsync(string to, string subject) { }
  
  // ❌ MAUVAIS
  public async Task<User> GetUser(Guid id) { }  // Manque Async
  ```

### Propriétés et Champs

```csharp
// ✅ BON - Champs privés avec underscore
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly int _maxRetryCount = 3;
    
    // Propriétés publiques PascalCase
    public string ServiceName { get; init; }
    public int MaxConnections { get; set; }
    
    // Propriétés calculées
    public bool IsActive => _status == ServiceStatus.Running;
    public int TotalUsers => _userRepository.Count();
}

// ❌ MAUVAIS
public class UserService
{
    private IUserRepository userRepository;  // Manque underscore
    private readonly ILogger<UserService> m_logger;  // Notation hongroise obsolète
    public string serviceName;  // Devrait être propriété
    private string _PublicData;  // Private avec PascalCase incohérent
}
```

### Paramètres et Variables Locales

```csharp
// ✅ BON - camelCase descriptif
public User CreateUser(string email, string firstName, string lastName, DateTime birthDate)
{
    var normalizedEmail = email.ToLowerInvariant().Trim();
    var hashedPassword = _passwordHasher.Hash(temporaryPassword);
    var newUser = new User
    {
        Email = normalizedEmail,
        FirstName = firstName,
        LastName = lastName,
        BirthDate = birthDate
    };
    
    return newUser;
}

// ❌ MAUVAIS
public User CreateUser(string e, string fn, string ln, DateTime bd)  // Abréviations
{
    var temp = e.ToLowerInvariant().Trim();  // Nom vague
    var x = _passwordHasher.Hash(temporaryPassword);  // Nom inutile
    var u = new User { /* ... */ };  // Abréviation
    
    return u;
}
```

### Enums

```csharp
// ✅ BON - Enum et membres PascalCase
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

public enum UserRole
{
    None = 0,
    Guest = 1,
    User = 2,
    Administrator = 3,
    SuperAdministrator = 4
}

// ❌ MAUVAIS
public enum orderStatus  // Devrait être PascalCase
{
    pending,  // Devrait être PascalCase
    CONFIRMED,  // Devrait être PascalCase, pas UPPER_CASE
    shipped_items  // Pas de underscore
}
```

**Règles pour enums :**
- Toujours spécifier les valeurs numériques explicitement
- Commencer à 0 pour la valeur par défaut
- Valeur 0 = état neutre ou invalide (`None`, `Unknown`, `NotSet`)
- Utiliser `[Flags]` pour enums combinables (avec valeurs puissance de 2)

```csharp
// ✅ BON - Enum avec flags
[Flags]
public enum FilePermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4,
    Delete = 8,
    ReadWrite = Read | Write,
    FullControl = Read | Write | Execute | Delete
}
```

## 📐 Formatage et Structure

### Organisation des Fichiers

**RÈGLE ABSOLUE : 1 classe publique = 1 fichier**

```
GroupeAdp.GenAi.Domain/
├── Entities/
│   ├── User.cs              // 1 classe User
│   └── Order.cs             // 1 classe Order
├── ValueObjects/
│   ├── Email.cs             // 1 record Email
│   └── Address.cs           // 1 record Address
└── Services/
    └── UserService.cs       // 1 classe UserService
```

**Ordre des membres dans une classe (strict) :**

```csharp
public class ExampleClass
{
    // 1. CHAMPS CONSTANTS (const)
    private const int MaxRetryCount = 3;
    public const string DefaultCulture = "fr-FR";
    
    // 2. CHAMPS STATIQUES (static fields)
    private static readonly ILogger _staticLogger = LoggerFactory.Create();
    
    // 3. CHAMPS D'INSTANCE (instance fields)
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ExampleClass> _logger;
    private int _retryCount;
    
    // 4. CONSTRUCTEURS (constructors)
    public ExampleClass(IUserRepository userRepository, ILogger<ExampleClass> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // 5. PROPRIÉTÉS (properties)
    public string Name { get; init; }
    public bool IsActive { get; private set; }
    public int Count => _items.Count;
    
    // 6. MÉTHODES PUBLIQUES (public methods)
    public async Task<User> GetUserAsync(Guid id)
    {
        // ...
    }
    
    public bool ValidateUser(User user)
    {
        // ...
    }
    
    // 7. MÉTHODES PROTÉGÉES (protected methods)
    protected virtual void OnUserValidated(User user)
    {
        // ...
    }
    
    // 8. MÉTHODES PRIVÉES (private methods)
    private bool IsValidEmail(string email)
    {
        // ...
    }
    
    private async Task<bool> CheckDatabaseAsync()
    {
        // ...
    }
    
    // 9. CLASSES IMBRIQUÉES (nested classes) - à éviter sauf exceptions
    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
```

### Indentation et Espacement

```csharp
// ✅ BON - Indentation 4 espaces, accolades sur nouvelle ligne
public class UserService
{
    public async Task<User> GetUserAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ID cannot be empty", nameof(id));
        }
        
        var user = await _repository.FindByIdAsync(id);
        
        if (user is null)
        {
            return null;
        }
        
        return user;
    }
}

// ❌ MAUVAIS - Mélange de styles
public class UserService {  // Accolade sur même ligne
  public async Task<User> GetUserAsync(Guid id) {  // 2 espaces
      if(id == Guid.Empty) {  // Pas d'espace avant parenthèse
          throw new ArgumentException("ID cannot be empty",nameof(id));  // Pas d'espace après virgule
      }
      var user=await _repository.FindByIdAsync(id);  // Pas d'espaces autour =
      if(user is null) return null;  // If sur une ligne
      return user;
  }
}
```

**Règles d'espacement :**
- 4 espaces (pas de tabulations)
- Accolades sur nouvelle ligne (style Allman)
- Ligne vide entre méthodes
- Ligne vide après déclarations de variables si suivi de code
- Espace après virgule : `Method(a, b, c)`
- Espace autour opérateurs : `x = y + z`
- Pas d'espace avant parenthèse méthode : `Method()`
- Espace après mots-clés : `if (`, `for (`, `while (`

### Longueur des Lignes et Méthodes

```csharp
// ✅ BON - Ligne < 120 caractères, méthode courte
public async Task<User> CreateUserAsync(string email, string password)
{
    ValidateEmail(email);
    ValidatePassword(password);
    
    var hashedPassword = await _passwordHasher.HashAsync(password);
    var user = new User
    {
        Email = email,
        PasswordHash = hashedPassword,
        CreatedAt = DateTime.UtcNow
    };
    
    await _repository.AddAsync(user);
    return user;
}

// ❌ MAUVAIS - Ligne trop longue, méthode trop complexe
public async Task<User> CreateUserAsync(string email, string password, string firstName, string lastName, DateTime? birthDate, string phoneNumber, string address, string city, string postalCode, string country)
{
    // Méthode de 50+ lignes avec trop de responsabilités
    // Validation + hachage + création + envoi email + logging + audit...
}
```

**Règles de longueur :**
- Ligne max : **120 caractères**
- Méthode : **< 20 lignes** (idéalement < 15)
- Paramètres : **< 4 paramètres** (sinon créer objet)
- Classe : **< 300 lignes** (sinon découper)

**Si méthode trop longue, découper :**
```csharp
// ✅ BON - Méthode complexe découpée
public async Task<User> CreateUserAsync(CreateUserRequest request)
{
    ValidateRequest(request);
    
    var user = await CreateUserEntityAsync(request);
    await SendWelcomeEmailAsync(user);
    await LogUserCreationAsync(user);
    
    return user;
}

private void ValidateRequest(CreateUserRequest request)
{
    ValidateEmail(request.Email);
    ValidatePassword(request.Password);
    ValidateRequiredFields(request);
}

private async Task<User> CreateUserEntityAsync(CreateUserRequest request)
{
    var hashedPassword = await _passwordHasher.HashAsync(request.Password);
    var user = new User
    {
        Email = request.Email,
        PasswordHash = hashedPassword,
        FirstName = request.FirstName,
        LastName = request.LastName,
        CreatedAt = DateTime.UtcNow
    };
    
    await _repository.AddAsync(user);
    return user;
}
```

## 🏗️ Principes SOLID

### Single Responsibility Principle (SRP)

**Une classe = une seule raison de changer**

```csharp
// ❌ MAUVAIS - Trop de responsabilités
public class UserService
{
    public void CreateUser(User user) { }
    public void SendEmail(string to, string subject) { }  // Devrait être EmailService
    public void LogActivity(string message) { }  // Devrait être ILogger
    public void ValidateUser(User user) { }  // Devrait être UserValidator
    public byte[] GeneratePdfReport(User user) { }  // Devrait être ReportGenerator
}

// ✅ BON - Responsabilités séparées
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IUserValidator _validator;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;
    
    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        _validator.Validate(request);  // Validation déléguée
        
        var user = new User { /* ... */ };
        await _repository.AddAsync(user);
        
        _logger.LogInformation("User created: {UserId}", user.Id);  // Log délégué
        await _emailService.SendWelcomeEmailAsync(user);  // Email délégué
        
        return user;
    }
}

public class UserValidator : IUserValidator
{
    public void Validate(CreateUserRequest request)
    {
        // Logique de validation uniquement
    }
}

public class EmailService : IEmailService
{
    public async Task SendWelcomeEmailAsync(User user)
    {
        // Logique d'envoi email uniquement
    }
}
```

### Open/Closed Principle (OCP)

**Ouvert à l'extension, fermé à la modification**

```csharp
// ❌ MAUVAIS - Modification nécessaire pour ajouter un type
public class PriceCalculator
{
    public decimal Calculate(Product product, string customerType)
    {
        if (customerType == "Regular")
            return product.Price;
        else if (customerType == "Premium")
            return product.Price * 0.9m;
        else if (customerType == "VIP")
            return product.Price * 0.8m;
        
        return product.Price;
    }
}

// ✅ BON - Extension sans modification
public interface IPricingStrategy
{
    decimal CalculatePrice(Product product);
}

public class RegularPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(Product product) => product.Price;
}

public class PremiumPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(Product product) => product.Price * 0.9m;
}

public class VipPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(Product product) => product.Price * 0.8m;
}

public class PriceCalculator
{
    public decimal Calculate(Product product, IPricingStrategy strategy)
    {
        return strategy.CalculatePrice(product);
    }
}
```

### Liskov Substitution Principle (LSP)

**Les sous-types doivent être substituables à leurs types de base**

```csharp
// ❌ MAUVAIS - Viole LSP
public class Bird
{
    public virtual void Fly() { /* vole */ }
}

public class Penguin : Bird
{
    public override void Fly()
    {
        throw new NotSupportedException("Penguins cannot fly");  // ❌ Viole le contrat
    }
}

// ✅ BON - Respecte LSP
public abstract class Bird
{
    public abstract void Move();
}

public class FlyingBird : Bird
{
    public override void Move() => Fly();
    
    protected virtual void Fly() { /* vole */ }
}

public class Penguin : Bird
{
    public override void Move() => Swim();
    
    protected void Swim() { /* nage */ }
}
```

### Interface Segregation Principle (ISP)

**Interfaces spécifiques plutôt qu'interface générale**

```csharp
// ❌ MAUVAIS - Interface trop large
public interface IUserService
{
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<User> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> SearchAsync(string query);
    Task SendEmailAsync(Guid userId, string message);
    Task<byte[]> GenerateReportAsync(Guid userId);
    Task ValidateAsync(User user);
}

// ✅ BON - Interfaces ségrégées
public interface IUserRepository
{
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> SearchAsync(string query);
}

public interface IUserNotificationService
{
    Task SendEmailAsync(Guid userId, string message);
}

public interface IUserReportService
{
    Task<byte[]> GenerateReportAsync(Guid userId);
}

public interface IUserValidator
{
    Task ValidateAsync(User user);
}
```

### Dependency Inversion Principle (DIP)

**Dépendre d'abstractions, pas d'implémentations**

```csharp
// ❌ MAUVAIS - Dépend d'implémentation concrète
public class UserService
{
    private readonly SqlUserRepository _repository;  // ❌ Couplage fort
    private readonly SmtpEmailSender _emailSender;   // ❌ Couplage fort
    
    public UserService()
    {
        _repository = new SqlUserRepository();  // ❌ Création directe
        _emailSender = new SmtpEmailSender();   // ❌ Création directe
    }
}

// ✅ BON - Dépend d'abstractions
public class UserService
{
    private readonly IUserRepository _repository;     // ✅ Interface
    private readonly IEmailService _emailService;     // ✅ Interface
    private readonly ILogger<UserService> _logger;    // ✅ Interface
    
    public UserService(
        IUserRepository repository,
        IEmailService emailService,
        ILogger<UserService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

## 🎯 Patterns et Bonnes Pratiques

### Null Safety

```csharp
// ✅ BON - Null safety avec nullable reference types
public class UserService
{
    private readonly IUserRepository _repository;
    
    public UserService(IUserRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
    
    public async Task<User?> FindUserAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;
        
        return await _repository.FindByEmailAsync(email);
    }
    
    public async Task<User> GetUserAsync(Guid id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        var user = await _repository.GetByIdAsync(id);
        
        if (user is null)
            throw new UserNotFoundException(id);
        
        return user;
    }
}

// ✅ BON - Pattern matching avec null
public string GetDisplayName(User? user)
{
    return user switch
    {
        null => "Guest",
        { FirstName: not null, LastName: not null } => $"{user.FirstName} {user.LastName}",
        { FirstName: not null } => user.FirstName,
        { Email: not null } => user.Email,
        _ => "Unknown"
    };
}

// ✅ BON - Null coalescing
public string GetUserName(User? user)
{
    return user?.FullName ?? user?.Email ?? "Unknown";
}
```

### Exception Handling

```csharp
// ✅ BON - Exceptions spécifiques et informatives
public async Task<User> GetUserAsync(Guid userId)
{
    if (userId == Guid.Empty)
    {
        throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }
    
    try
    {
        var user = await _repository.GetByIdAsync(userId);
        
        if (user is null)
        {
            throw new UserNotFoundException(userId);
        }
        
        return user;
    }
    catch (DbException ex)
    {
        _logger.LogError(ex, "Database error while retrieving user {UserId}", userId);
        throw new DataAccessException("Failed to retrieve user from database", ex);
    }
}

// ❌ MAUVAIS - Mauvaise gestion des exceptions
public async Task<User> GetUserAsync(Guid userId)
{
    try
    {
        return await _repository.GetByIdAsync(userId);
    }
    catch (Exception ex)  // ❌ Trop large
    {
        Console.WriteLine(ex.Message);  // ❌ Console.WriteLine
        return null;  // ❌ Masque l'erreur
    }
}

// ✅ BON - Custom exceptions
public class UserNotFoundException : Exception
{
    public Guid UserId { get; }
    
    public UserNotFoundException(Guid userId)
        : base($"User with ID {userId} was not found")
    {
        UserId = userId;
    }
    
    public UserNotFoundException(Guid userId, Exception innerException)
        : base($"User with ID {userId} was not found", innerException)
    {
        UserId = userId;
    }
}
```

### Async/Await

```csharp
// ✅ BON - Async correctement utilisé
public async Task<User> CreateUserAsync(CreateUserRequest request)
{
    ValidateRequest(request);  // Synchrone si validation rapide
    
    var hashedPassword = await _passwordHasher.HashAsync(request.Password);
    var user = new User
    {
        Email = request.Email,
        PasswordHash = hashedPassword
    };
    
    await _repository.AddAsync(user);
    await _emailService.SendWelcomeEmailAsync(user);
    
    return user;
}

// ❌ MAUVAIS - Anti-patterns async
public async Task<User> GetUserAsync(Guid id)
{
    return await _repository.GetByIdAsync(id).ConfigureAwait(false);  // ❌ Inutile dans app moderne
}

public async Task<User> CreateUserAsync(User user)
{
    var result = _repository.AddAsync(user).Result;  // ❌ .Result bloque
    return result;
}

public async Task<User> FindUserAsync(Guid id)
{
    await Task.Delay(0);  // ❌ Async inutile
    return _users.FirstOrDefault(u => u.Id == id);
}

// ✅ BON - Task.WhenAll pour parallélisme
public async Task<UserDetails> GetUserDetailsAsync(Guid userId)
{
    var userTask = _userRepository.GetByIdAsync(userId);
    var ordersTask = _orderRepository.GetByUserIdAsync(userId);
    var preferencesTask = _preferencesRepository.GetByUserIdAsync(userId);
    
    await Task.WhenAll(userTask, ordersTask, preferencesTask);
    
    return new UserDetails
    {
        User = await userTask,
        Orders = await ordersTask,
        Preferences = await preferencesTask
    };
}
```

### LINQ et Collections

```csharp
// ✅ BON - LINQ lisible et performant
public IEnumerable<Product> GetActiveExpensiveProducts(IEnumerable<Product> products)
{
    return products
        .Where(p => p.IsActive)
        .Where(p => p.Price > 100)
        .OrderByDescending(p => p.Price)
        .Take(10)
        .ToList();  // Matérialiser si nécessaire
}

// ✅ BON - Préférer méthode LINQ à boucle si lisible
var activeUsers = users.Where(u => u.IsActive).ToList();
var userNames = users.Select(u => u.FullName).ToList();
var hasAdmin = users.Any(u => u.Role == UserRole.Administrator);

// ❌ MAUVAIS - Boucle moins lisible
var activeUsers = new List<User>();
foreach (var user in users)
{
    if (user.IsActive)
        activeUsers.Add(user);
}

// ✅ BON - FirstOrDefault vs First
var user = users.FirstOrDefault(u => u.Id == userId);  // Retourne null si absent
if (user is not null)
{
    // Traiter
}

// ❌ MAUVAIS
try
{
    var user = users.First(u => u.Id == userId);  // Exception si absent
}
catch (InvalidOperationException)
{
    // Gestion d'erreur
}

// ✅ BON - Éviter ToList() inutile
public IEnumerable<User> GetActiveUsers()
{
    return _context.Users
        .Where(u => u.IsActive);  // Pas de ToList(), exécution différée
}

// ✅ BON - ToList() nécessaire si multiples itérations
public void ProcessUsers()
{
    var users = _context.Users
        .Where(u => u.IsActive)
        .ToList();  // Matérialiser une seule fois
    
    foreach (var user in users)
    {
        // Première itération
    }
    
    foreach (var user in users)
    {
        // Seconde itération sans re-requête
    }
}
```

### Using et Dispose

```csharp
// ✅ BON - Using declaration (C# 8+)
public async Task ProcessFileAsync(string filePath)
{
    using var stream = File.OpenRead(filePath);
    using var reader = new StreamReader(stream);
    
    var content = await reader.ReadToEndAsync();
    ProcessContent(content);
    
    // Dispose automatique en fin de scope
}

// ✅ BON - Using statement classique
public void ProcessFile(string filePath)
{
    using (var stream = File.OpenRead(filePath))
    using (var reader = new StreamReader(stream))
    {
        var content = reader.ReadToEnd();
        ProcessContent(content);
    }
}

// ✅ BON - IAsyncDisposable (C# 8+)
public async Task ProcessDatabaseAsync()
{
    await using var connection = await _connectionFactory.CreateAsync();
    await using var command = connection.CreateCommand();
    
    // Utilisation
    
    // DisposeAsync automatique
}

// ❌ MAUVAIS - Pas de using pour IDisposable
public void ProcessFile(string filePath)
{
    var stream = File.OpenRead(filePath);  // ❌ Fuite de ressource
    var reader = new StreamReader(stream);
    
    var content = reader.ReadToEnd();
    ProcessContent(content);
    
    // stream et reader non disposés
}
```

### String Manipulation

```csharp
// ✅ BON - StringBuilder pour concaténations multiples
public string BuildReport(IEnumerable<Order> orders)
{
    var sb = new StringBuilder();
    sb.AppendLine("Order Report");
    sb.AppendLine("=============");
    
    foreach (var order in orders)
    {
        sb.AppendLine($"Order {order.Id}: {order.TotalAmount:C}");
    }
    
    return sb.ToString();
}

// ❌ MAUVAIS - Concaténation en boucle
public string BuildReport(IEnumerable<Order> orders)
{
    string report = "Order Report\n";
    report += "=============\n";  // ❌ Nouvelle string à chaque +=
    
    foreach (var order in orders)
    {
        report += $"Order {order.Id}: {order.TotalAmount:C}\n";  // ❌ Très inefficace
    }
    
    return report;
}

// ✅ BON - String interpolation
var message = $"User {user.Name} (ID: {user.Id}) logged in at {DateTime.Now:yyyy-MM-dd HH:mm}";

// ✅ BON - Verbatim strings pour chemins
var path = @"C:\Users\Documents\file.txt";
var sql = @"
    SELECT *
    FROM Users
    WHERE IsActive = 1
";

// ✅ BON - Raw string literals (C# 11+)
var json = """
    {
        "name": "John Doe",
        "email": "john@example.com"
    }
    """;
```

### Value Objects et Records

```csharp
// ✅ BON - Record pour Value Objects
public record Email
{
    public string Value { get; init; }
    
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));
        
        if (!IsValidEmail(value))
            throw new ArgumentException($"Invalid email format: {value}", nameof(value));
        
        Value = value.ToLowerInvariant().Trim();
    }
    
    private static bool IsValidEmail(string email)
    {
        // Validation
        return true;
    }
    
    public override string ToString() => Value;
}

// ✅ BON - Record positional (C# 9+)
public record Money(decimal Amount, string Currency)
{
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        
        return this with { Amount = Amount + other.Amount };
    }
}

// ✅ BON - Record inheritance
public record Person(string FirstName, string LastName);
public record Employee(string FirstName, string LastName, string EmployeeId) : Person(FirstName, LastName);
```

## 🚫 Anti-Patterns à Éviter

### Magic Numbers et Strings

```csharp
// ❌ MAUVAIS - Magic numbers
public bool CanWithdraw(decimal amount)
{
    return amount <= 1000 && _balance >= amount;  // Qu'est-ce que 1000 ?
}

if (user.Status == 2)  // Qu'est-ce que 2 ?
{
    // ...
}

// ✅ BON - Constantes nommées
private const decimal MaxWithdrawalAmount = 1000m;

public bool CanWithdraw(decimal amount)
{
    return amount <= MaxWithdrawalAmount && _balance >= amount;
}

public enum UserStatus
{
    Inactive = 0,
    Active = 1,
    Suspended = 2,
    Deleted = 3
}

if (user.Status == UserStatus.Suspended)
{
    // ...
}
```

### God Classes

```csharp
// ❌ MAUVAIS - Classe qui fait tout
public class ApplicationManager
{
    public void CreateUser() { }
    public void UpdateUser() { }
    public void DeleteUser() { }
    public void SendEmail() { }
    public void GenerateReport() { }
    public void ProcessPayment() { }
    public void ValidateData() { }
    public void LogActivity() { }
    // ... 50 autres méthodes
}

// ✅ BON - Responsabilités séparées
public class UserService { /* gestion utilisateurs */ }
public class EmailService { /* envoi emails */ }
public class ReportService { /* génération rapports */ }
public class PaymentService { /* traitement paiements */ }
public class ValidationService { /* validation données */ }
```

### Primitive Obsession

```csharp
// ❌ MAUVAIS - Primitives partout
public class User
{
    public string Email { get; set; }  // Juste string, pas de validation
    public string PhoneNumber { get; set; }
    public decimal Salary { get; set; }  // Quelle devise ?
}

public void SendEmail(string to, string subject, string body)
{
    // Aucune garantie que 'to' est un email valide
}

// ✅ BON - Value objects
public class User
{
    public Email Email { get; init; }
    public PhoneNumber PhoneNumber { get; init; }
    public Money Salary { get; init; }
}

public void SendEmail(Email to, EmailSubject subject, EmailBody body)
{
    // Types garantissent la validité
}
```

### Anemic Domain Model

```csharp
// ❌ MAUVAIS - Modèle anémique (seulement des getters/setters)
public class Order
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderLine> Lines { get; set; }
}

public class OrderService
{
    public void AddLine(Order order, OrderLine line)
    {
        order.Lines.Add(line);
        order.Total += line.Price * line.Quantity;
    }
    
    public void ConfirmOrder(Order order)
    {
        if (order.Lines.Count == 0)
            throw new InvalidOperationException();
        
        order.Status = OrderStatus.Confirmed;
    }
}

// ✅ BON - Rich domain model
public class Order
{
    public Guid Id { get; init; }
    public decimal Total { get; private set; }
    public OrderStatus Status { get; private set; }
    private readonly List<OrderLine> _lines = new();
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();
    
    public void AddLine(OrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        
        _lines.Add(line);
        Total += line.Price * line.Quantity;
    }
    
    public void Confirm()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException("Cannot confirm order without lines");
        
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm order in status {Status}");
        
        Status = OrderStatus.Confirmed;
    }
}
```

## ✅ Checklist Validation Code

Avant de commiter du code, VÉRIFIER :

### Nommage
- [ ] Classes, méthodes, propriétés en PascalCase
- [ ] Paramètres et variables en camelCase
- [ ] Champs privés avec underscore `_fieldName`
- [ ] Interfaces avec préfixe `I`
- [ ] Méthodes async avec suffixe `Async`
- [ ] Noms descriptifs et explicites (pas d'abréviations)

### Structure
- [ ] 1 classe publique = 1 fichier
- [ ] Ordre des membres respecté (constantes, champs, constructeur, propriétés, méthodes)
- [ ] Méthodes < 20 lignes
- [ ] Paramètres < 4 (sinon objet)
- [ ] Classes < 300 lignes

### Principes SOLID
- [ ] Chaque classe a une seule responsabilité
- [ ] Dépendances injectées (pas de `new` dans constructeur)
- [ ] Interfaces ségrégées (petites et spécifiques)
- [ ] Couplage faible, cohésion forte

### Qualité
- [ ] Null safety (nullable reference types)
- [ ] Exceptions spécifiques (pas de `Exception` générique)
- [ ] Using/Dispose pour ressources
- [ ] Async/await correct (pas de `.Result` ou `.Wait()`)
- [ ] LINQ lisible et performant

### Documentation
- [ ] Commentaires XML sur membres publics
- [ ] Documentation en français
- [ ] Exemples pour APIs complexes
- [ ] Pas de code commenté (supprimer)

### Tests
- [ ] Tests unitaires pour toute logique métier
- [ ] Couverture > 80%
- [ ] Tests nommés explicitement
- [ ] Arrange-Act-Assert respecté

## 📚 Ressources

### Documentation Officielle
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [C# Programming Guide](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/)

### Livres Recommandés
- **Clean Code** - Robert C. Martin
- **Refactoring** - Martin Fowler
- **Domain-Driven Design** - Eric Evans
- **Patterns of Enterprise Application Architecture** - Martin Fowler
