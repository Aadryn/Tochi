# 23. Result Pattern

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les exceptions pour le contrôle de flux posent problème :
- **Performance** : Les exceptions sont coûteuses (stack trace)
- **Lisibilité** : Try/catch dispersés dans le code
- **Flux imprévisible** : Difficile de suivre le chemin d'exécution
- **Perte d'information** : L'exception ne porte pas toujours le contexte

```csharp
// ❌ PROBLÉMATIQUE : Exceptions pour cas métier prévisibles
public User GetUser(Guid id)
{
    var user = _repository.Find(id);
    if (user == null)
        throw new UserNotFoundException(id); // Exception pour cas normal
    return user;
}

// Appelant doit catcher
try
{
    var user = _service.GetUser(id);
    // ...
}
catch (UserNotFoundException ex)
{
    return NotFound(ex.Message);
}
catch (ValidationException ex)
{
    return BadRequest(ex.Errors);
}
```

## Décision

**Utiliser le Result Pattern pour les opérations qui peuvent échouer de manière prévisible, en réservant les exceptions aux cas vraiment exceptionnels.**

### 1. Type Result générique

```csharp
/// <summary>
/// Représente le résultat d'une opération qui peut échouer.
/// </summary>
public sealed class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    
    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Résultat avec valeur en cas de succès.
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public Error Error { get; }
    
    private Result(bool isSuccess, T value, Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static Result<T> Failure(Error error) => new(false, default!, error);
    
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}

/// <summary>
/// Représente une erreur avec code et message.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    
    // Erreurs prédéfinies par domaine
    public static class User
    {
        public static Error NotFound(Guid id) => 
            new("User.NotFound", $"User with ID {id} was not found");
        
        public static Error EmailAlreadyExists(string email) => 
            new("User.EmailExists", $"Email {email} is already registered");
        
        public static Error InvalidCredentials => 
            new("User.InvalidCredentials", "Invalid email or password");
    }
    
    public static class Tenant
    {
        public static Error NotFound(Guid id) => 
            new("Tenant.NotFound", $"Tenant with ID {id} was not found");
        
        public static Error Inactive(Guid id) => 
            new("Tenant.Inactive", $"Tenant {id} is not active");
        
        public static Error QuotaExceeded => 
            new("Tenant.QuotaExceeded", "Monthly quota has been exceeded");
    }
}
```

### 2. Utilisation dans les services

```csharp
public class UserService
{
    private readonly IUserRepository _repository;
    
    /// <summary>
    /// Récupère un utilisateur par ID.
    /// </summary>
    public async Task<Result<User>> GetByIdAsync(
        Guid id, 
        CancellationToken ct)
    {
        var user = await _repository.FindAsync(id, ct);
        
        if (user is null)
            return Error.User.NotFound(id);
        
        return user;
    }
    
    /// <summary>
    /// Crée un nouvel utilisateur.
    /// </summary>
    public async Task<Result<User>> CreateAsync(
        CreateUserCommand command, 
        CancellationToken ct)
    {
        // Validation métier
        var existingUser = await _repository.FindByEmailAsync(
            command.Email, ct);
        
        if (existingUser is not null)
            return Error.User.EmailAlreadyExists(command.Email);
        
        // Création
        var user = User.Create(command.Email, command.Name);
        await _repository.AddAsync(user, ct);
        
        return user;
    }
    
    /// <summary>
    /// Authentifie un utilisateur.
    /// </summary>
    public async Task<Result<AuthToken>> AuthenticateAsync(
        string email, 
        string password, 
        CancellationToken ct)
    {
        var user = await _repository.FindByEmailAsync(email, ct);
        
        if (user is null)
            return Error.User.InvalidCredentials;
        
        if (!user.VerifyPassword(password))
            return Error.User.InvalidCredentials;
        
        if (!user.IsActive)
            return new Error("User.Inactive", "Account is not active");
        
        var token = _tokenService.GenerateToken(user);
        
        return token;
    }
}
```

### 3. Chaînage de Results (Railway Oriented)

```csharp
public static class ResultExtensions
{
    /// <summary>
    /// Exécute une fonction si le résultat est un succès.
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> func)
    {
        var result = await resultTask;
        
        if (result.IsFailure)
            return Result<TOut>.Failure(result.Error);
        
        return await func(result.Value);
    }
    
    /// <summary>
    /// Transforme la valeur si succès.
    /// </summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper)
    {
        var result = await resultTask;
        
        if (result.IsFailure)
            return Result<TOut>.Failure(result.Error);
        
        return mapper(result.Value);
    }
    
    /// <summary>
    /// Exécute une action sur la valeur si succès.
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task> action)
    {
        var result = await resultTask;
        
        if (result.IsSuccess)
            await action(result.Value);
        
        return result;
    }
    
    /// <summary>
    /// Matche le résultat pour produire une valeur.
    /// </summary>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess 
            ? onSuccess(result.Value) 
            : onFailure(result.Error);
    }
}
```

### 4. Pipeline de traitement

```csharp
public class OrderService
{
    /// <summary>
    /// Crée une commande avec pipeline de validation.
    /// </summary>
    public async Task<Result<Order>> CreateOrderAsync(
        CreateOrderCommand command,
        CancellationToken ct)
    {
        // Pipeline Railway-Oriented
        return await ValidateCommand(command)
            .BindAsync(cmd => GetTenantAsync(cmd.TenantId, ct))
            .BindAsync(tenant => CheckTenantQuotaAsync(tenant, ct))
            .BindAsync(tenant => CreateOrderForTenantAsync(tenant, command, ct))
            .TapAsync(order => SendConfirmationEmailAsync(order, ct));
    }
    
    private Result<CreateOrderCommand> ValidateCommand(CreateOrderCommand cmd)
    {
        if (cmd.Items.Count == 0)
            return new Error("Order.NoItems", "Order must have at least one item");
        
        if (cmd.Items.Any(i => i.Quantity <= 0))
            return new Error("Order.InvalidQuantity", "Quantity must be positive");
        
        return cmd;
    }
    
    private async Task<Result<Tenant>> GetTenantAsync(
        Guid tenantId, 
        CancellationToken ct)
    {
        var tenant = await _tenantRepository.FindAsync(tenantId, ct);
        
        if (tenant is null)
            return Error.Tenant.NotFound(tenantId);
        
        return tenant;
    }
    
    private async Task<Result<Tenant>> CheckTenantQuotaAsync(
        Tenant tenant, 
        CancellationToken ct)
    {
        var usage = await _quotaService.GetUsageAsync(tenant.Id, ct);
        
        if (usage >= tenant.MonthlyQuota)
            return Error.Tenant.QuotaExceeded;
        
        return tenant;
    }
}
```

### 5. Utilisation dans les contrôleurs

```csharp
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(
        Guid id, 
        CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        
        return result.Match(
            onSuccess: user => Ok(user.ToDto()),
            onFailure: error => error.Code switch
            {
                "User.NotFound" => NotFound(error),
                _ => BadRequest(error)
            });
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        var command = request.ToCommand();
        var result = await _userService.CreateAsync(command, ct);
        
        return result.Match(
            onSuccess: user => CreatedAtAction(
                nameof(GetUser), 
                new { id = user.Id }, 
                user.ToDto()),
            onFailure: error => error.Code switch
            {
                "User.EmailExists" => Conflict(error),
                _ => BadRequest(error)
            });
    }
}

// Extension pour réponses standardisées
public static class ControllerExtensions
{
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess?.Invoke(result.Value) 
                ?? controller.Ok(result.Value);
        }
        
        return result.Error.Code switch
        {
            var c when c.EndsWith(".NotFound") => 
                controller.NotFound(new { result.Error.Code, result.Error.Message }),
            var c when c.Contains("Validation") => 
                controller.BadRequest(new { result.Error.Code, result.Error.Message }),
            var c when c.Contains("Unauthorized") => 
                controller.Unauthorized(new { result.Error.Code, result.Error.Message }),
            _ => controller.BadRequest(new { result.Error.Code, result.Error.Message })
        };
    }
}
```

### 6. Result avec validation multiple

```csharp
/// <summary>
/// Résultat avec plusieurs erreurs possibles.
/// </summary>
public sealed class ValidationResult<T>
{
    public bool IsSuccess => !Errors.Any();
    public bool IsFailure => Errors.Any();
    public T Value { get; }
    public IReadOnlyList<Error> Errors { get; }
    
    private ValidationResult(T value, IEnumerable<Error> errors)
    {
        Value = value;
        Errors = errors.ToList();
    }
    
    public static ValidationResult<T> Success(T value) => 
        new(value, Enumerable.Empty<Error>());
    
    public static ValidationResult<T> WithErrors(IEnumerable<Error> errors) => 
        new(default!, errors);
}

public class CreateUserValidator
{
    public ValidationResult<CreateUserCommand> Validate(CreateUserCommand command)
    {
        var errors = new List<Error>();
        
        if (string.IsNullOrWhiteSpace(command.Email))
            errors.Add(new Error("Validation.Email.Required", "Email is required"));
        else if (!IsValidEmail(command.Email))
            errors.Add(new Error("Validation.Email.Invalid", "Email format is invalid"));
        
        if (string.IsNullOrWhiteSpace(command.Name))
            errors.Add(new Error("Validation.Name.Required", "Name is required"));
        else if (command.Name.Length < 2)
            errors.Add(new Error("Validation.Name.TooShort", "Name must be at least 2 characters"));
        
        if (command.Password.Length < 8)
            errors.Add(new Error("Validation.Password.TooShort", "Password must be at least 8 characters"));
        
        if (errors.Any())
            return ValidationResult<CreateUserCommand>.WithErrors(errors);
        
        return ValidationResult<CreateUserCommand>.Success(command);
    }
}
```

### 7. Quand utiliser Exception vs Result

```csharp
public class TenantService
{
    // ✅ RESULT : Cas métier prévisible (tenant non trouvé)
    public async Task<Result<Tenant>> GetByIdAsync(
        Guid id, 
        CancellationToken ct)
    {
        var tenant = await _repository.FindAsync(id, ct);
        
        if (tenant is null)
            return Error.Tenant.NotFound(id);
        
        return tenant;
    }
    
    // ✅ EXCEPTION : Bug de programmation (argument null)
    public async Task<Result<Tenant>> CreateAsync(
        CreateTenantCommand command,
        CancellationToken ct)
    {
        // Exception pour les bugs (violation de contrat)
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.Name);
        
        // Result pour la logique métier
        var existing = await _repository.FindByNameAsync(command.Name, ct);
        if (existing is not null)
            return new Error("Tenant.NameExists", $"Tenant {command.Name} already exists");
        
        var tenant = Tenant.Create(command.Name);
        await _repository.AddAsync(tenant, ct);
        
        return tenant;
    }
    
    // ✅ EXCEPTION : Infrastructure (base de données down)
    // Les exceptions d'infrastructure remontent naturellement
    // et sont gérées globalement (middleware)
}
```

## Conséquences

### Positives

- **Explicite** : Le type de retour indique que l'opération peut échouer
- **Performance** : Pas de coût d'exception pour les cas normaux
- **Lisibilité** : Flux linéaire sans try/catch
- **Composition** : Chaînage facile avec Bind/Map
- **Exhaustivité** : Le compilateur force à gérer le cas d'erreur

### Négatives

- **Verbosité** : Plus de code pour les types Result
  - *Mitigation* : Bibliothèques comme FluentResults ou ErrorOr
- **Courbe d'apprentissage** : Pattern moins connu
  - *Mitigation* : Documentation et exemples
- **Interopérabilité** : Les bibliothèques externes utilisent des exceptions
  - *Mitigation* : Wrapper les appels externes

### Neutres

- Le Result Pattern vient de la programmation fonctionnelle (Either/Option)

## Alternatives considérées

### Option A : Exceptions partout

- **Description** : Utiliser uniquement des exceptions
- **Avantages** : Standard .NET, moins de types
- **Inconvénients** : Performance, flux imprévisible
- **Raison du rejet** : Les exceptions doivent rester exceptionnelles

### Option B : Tuple (success, value, error)

- **Description** : Retourner un tuple au lieu d'un type dédié
- **Avantages** : Pas de type custom
- **Inconvénients** : Pas de méthodes helper, moins expressif
- **Raison du rejet** : Le type Result offre plus de fonctionnalités

### Option C : Bibliothèque externe (FluentResults, ErrorOr)

- **Description** : Utiliser une bibliothèque existante
- **Avantages** : Fonctionnalités riches, communauté
- **Inconvénients** : Dépendance externe
- **Raison du rejet** : Acceptable, mais un type simple suffit souvent

## Références

- [Railway Oriented Programming - Scott Wlaschin](https://fsharpforfunandprofit.com/rop/)
- [FluentResults](https://github.com/altmann/FluentResults)
- [ErrorOr](https://github.com/amantinband/error-or)
- [Functional C# - Enrico Buonanno](https://www.manning.com/books/functional-programming-in-c-sharp)
