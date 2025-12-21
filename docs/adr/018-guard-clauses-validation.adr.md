# 18. Guard Clauses et Validation

Date: 2025-12-21

## Statut

Accepté

## Contexte

La validation des données est souvent :
- **Dispersée** : Validations répétées à plusieurs endroits
- **Tardive** : Erreurs détectées loin de la source
- **Inconsistante** : Règles de validation différentes selon le contexte
- **Mélangée** : Validation technique et métier mélangées

Exemple de problèmes :

```csharp
// ❌ PROBLÈME : Validation dispersée et tardive
public class TenantService
{
    public async Task CreateTenant(string name, string email)
    {
        // ... 20 lignes de code ...
        
        _context.Tenants.Add(new Tenant 
        { 
            Name = name,    // Peut être null ou vide !
            Email = email   // Peut être invalide !
        });
        
        await _context.SaveChangesAsync();
        // Exception SQL si Name est null (contrainte BDD)
    }
}
```

## Décision

**Utiliser des Guard Clauses en début de méthode et une validation structurée à chaque niveau.**

### 1. Guard Clauses avec ArgumentNullException.ThrowIfNull

```csharp
public class TenantService
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public TenantService(ITenantRepository repository, IUnitOfWork unitOfWork)
    {
        // Guards dans le constructeur
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }
    
    public async Task<Tenant> GetByIdAsync(Guid tenantId, CancellationToken ct)
    {
        // Guards en début de méthode
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
        
        var tenant = await _repository.GetByIdAsync(tenantId, ct);
        
        // Guard sur le résultat
        if (tenant is null)
            throw new TenantNotFoundException(tenantId);
        
        return tenant;
    }
}
```

### 2. Classe Guard réutilisable

```csharp
// Common/Guard.cs
namespace LLMProxy.Domain.Common;

public static class Guard
{
    public static void AgainstNull<T>(T? value, string parameterName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
    }
    
    public static void AgainstNullOrEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException($"{parameterName} cannot be null or empty", parameterName);
    }
    
    public static void AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be null or whitespace", parameterName);
    }
    
    public static void AgainstEmptyGuid(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"{parameterName} cannot be empty GUID", parameterName);
    }
    
    public static void AgainstNegative(int value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, value, 
                $"{parameterName} cannot be negative");
    }
    
    public static void AgainstNegativeOrZero(int value, string parameterName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, value, 
                $"{parameterName} must be positive");
    }
    
    public static void AgainstOutOfRange(int value, int min, int max, string parameterName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(parameterName, value, 
                $"{parameterName} must be between {min} and {max}");
    }
    
    public static void AgainstInvalidEmail(string? email, string parameterName)
    {
        AgainstNullOrWhiteSpace(email, parameterName);
        
        if (!EmailRegex.IsMatch(email!))
            throw new ArgumentException($"{parameterName} is not a valid email", parameterName);
    }
    
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
}

// Utilisation
public async Task CreateTenant(string name, string email, CancellationToken ct)
{
    Guard.AgainstNullOrWhiteSpace(name, nameof(name));
    Guard.AgainstInvalidEmail(email, nameof(email));
    
    // Code métier...
}
```

### 3. Validation avec FluentValidation

```csharp
// Application/Tenants/Commands/CreateTenant/CreateTenantCommandValidator.cs
public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Le nom du tenant est obligatoire")
            .MinimumLength(3)
                .WithMessage("Le nom doit contenir au moins 3 caractères")
            .MaximumLength(100)
                .WithMessage("Le nom ne peut pas dépasser 100 caractères")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$")
                .WithMessage("Le nom ne peut contenir que des lettres, chiffres, espaces, tirets et underscores");
        
        RuleFor(x => x.ContactEmail)
            .NotEmpty()
                .WithMessage("L'email de contact est obligatoire")
            .EmailAddress()
                .WithMessage("L'email n'est pas valide")
            .MaximumLength(255)
                .WithMessage("L'email ne peut pas dépasser 255 caractères");
        
        RuleFor(x => x.Plan)
            .IsInEnum()
                .WithMessage("Le plan sélectionné n'est pas valide");
    }
}
```

### 4. Pipeline de validation avec MediatR

```csharp
// Application/Common/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any())
            return await next();
        
        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));
        
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();
        
        if (failures.Any())
            throw new ValidationException(failures);
        
        return await next();
    }
}

// Enregistrement
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddValidatorsFromAssemblyContaining<CreateTenantCommandValidator>();
```

### 5. Validation dans les Value Objects

```csharp
// Domain/ValueObjects/Email.cs
public sealed class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public string Value { get; }
    
    private Email(string value)
    {
        Value = value;
    }
    
    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty");
        
        var trimmed = value.Trim().ToLowerInvariant();
        
        if (trimmed.Length > 255)
            throw new DomainException("Email cannot exceed 255 characters");
        
        if (!EmailRegex.IsMatch(trimmed))
            throw new DomainException($"Invalid email format: {value}");
        
        return new Email(trimmed);
    }
    
    public static bool TryCreate(string value, out Email? email)
    {
        try
        {
            email = Create(value);
            return true;
        }
        catch
        {
            email = null;
            return false;
        }
    }
    
    // Conversion implicite pour faciliter l'usage
    public static implicit operator string(Email email) => email.Value;
    
    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Email email && Equals(email);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}

// Utilisation
var email = Email.Create("user@example.com"); // Valide à la création
```

### 6. Validation dans les Entities

```csharp
// Domain/Entities/Tenant.cs
public class Tenant : Entity
{
    public string Name { get; private set; }
    public Email ContactEmail { get; private set; }
    public TenantStatus Status { get; private set; }
    
    private Tenant() { } // Pour EF Core
    
    public static Tenant Create(string name, string contactEmail)
    {
        // Guards de base
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        
        // Validation métier
        if (name.Length < 3)
            throw new DomainException("Tenant name must be at least 3 characters");
        
        if (name.Length > 100)
            throw new DomainException("Tenant name cannot exceed 100 characters");
        
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            ContactEmail = Email.Create(contactEmail), // Validation via Value Object
            Status = TenantStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void Rename(string newName)
    {
        Guard.AgainstNullOrWhiteSpace(newName, nameof(newName));
        
        if (newName.Length < 3 || newName.Length > 100)
            throw new DomainException("Tenant name must be between 3 and 100 characters");
        
        Name = newName.Trim();
        ModifiedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        if (Status == TenantStatus.Suspended)
            throw new DomainException("Cannot activate a suspended tenant");
        
        if (Status == TenantStatus.Active)
            throw new DomainException("Tenant is already active");
        
        Status = TenantStatus.Active;
        ModifiedAt = DateTime.UtcNow;
    }
}
```

### 7. Niveaux de validation

| Niveau | Responsabilité | Exemple |
|--------|----------------|---------|
| **API** | Format de requête | JSON valide, champs requis |
| **Application** | Règles applicatives | FluentValidation, format email |
| **Domain** | Invariants métier | Statut valide pour transition |
| **Infrastructure** | Contraintes techniques | Unicité en BDD |

```csharp
// Exemple de validation à chaque niveau

// 1. API - Model binding ASP.NET Core
[HttpPost]
public async Task<IActionResult> Create(
    [FromBody] CreateTenantRequest request) // Validation automatique
{
    // 2. Application - FluentValidation via Pipeline
    var command = new CreateTenantCommand(request.Name, request.ContactEmail);
    var id = await _mediator.Send(command); // ValidationBehavior s'exécute
    return Ok(id);
}

// 3. Domain - Dans l'Entity
public static Tenant Create(string name, string email)
{
    // Validation des invariants métier
    if (name.Length < 3) throw new DomainException("...");
    // ...
}

// 4. Infrastructure - Contraintes BDD
modelBuilder.Entity<Tenant>(entity =>
{
    entity.HasIndex(e => e.Name).IsUnique();
    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
});
```

## Conséquences

### Positives

- **Fail Fast** : Erreurs détectées immédiatement à leur source
- **Clarté** : Les préconditions sont visibles en début de méthode
- **Cohérence** : Validation centralisée et réutilisable
- **Séparation** : Chaque niveau valide ce qui le concerne
- **Sécurité** : Impossible de créer des objets invalides

### Négatives

- **Verbosité** : Plus de code de validation
  - *Mitigation* : Guards et Validators réutilisables
- **Messages d'erreur** : Doivent être maintenus et traduits
  - *Mitigation* : Centraliser les messages dans des ressources

### Neutres

- La validation est un investissement qui paie sur la durée

## Alternatives considérées

### Option A : Validation uniquement en BDD

- **Description** : S'appuyer sur les contraintes de base de données
- **Avantages** : Simple, source unique de vérité
- **Inconvénients** : Erreurs tardives, messages cryptiques
- **Raison du rejet** : Fail Fast est préférable

### Option B : Data Annotations uniquement

- **Description** : Utiliser [Required], [StringLength], etc.
- **Avantages** : Simple, intégré à ASP.NET
- **Inconvénients** : Limité pour les règles complexes, mélange concerns
- **Raison du rejet** : FluentValidation plus puissant et testable

## Références

- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [Guard Clauses - Ardalis](https://github.com/ardalis/GuardClauses)
- [Validation in DDD - Vladimir Khorikov](https://enterprisecraftsmanship.com/posts/validation-and-ddd/)
