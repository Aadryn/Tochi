# 19. Convention over Configuration

Date: 2025-12-21

## Statut

Accepté

## Contexte

La configuration excessive crée des frictions :
- **Boilerplate massif** : Fichiers de configuration verbeux pour chaque aspect
- **Courbe d'apprentissage** : Comprendre toutes les options possibles
- **Erreurs de configuration** : Typos, valeurs incorrectes, options oubliées
- **Incohérence** : Chaque développeur configure différemment
- **Maintenance** : Synchroniser configuration et code

Exemple de sur-configuration :

```csharp
// ❌ PROBLÈME : Configuration explicite pour chaque détail
services.AddDbContext<AppDbContext>(options =>
{
    options.TableNamingConvention = TableNamingConvention.SnakeCase;
    options.ColumnNamingConvention = ColumnNamingConvention.SnakeCase;
    options.PrimaryKeyColumnName = "id";
    options.ForeignKeyPattern = "{navigation}_{property}";
    options.CreatedAtColumnName = "created_at";
    options.UpdatedAtColumnName = "updated_at";
    // ... 50 lignes de config
});
```

## Décision

**Adopter des conventions standard et ne configurer que les exceptions.**

### 1. Conventions de nommage

```csharp
// ✅ CONVENTION : Les noms suivent des patterns prévisibles

// Entités → Tables (pluriel, snake_case)
public class Tenant → tenants
public class ApiKey → api_keys
public class UserRole → user_roles

// Propriétés → Colonnes (snake_case)
public Guid Id → id
public string ContactEmail → contact_email
public DateTime CreatedAt → created_at

// FK → {table_singulier}_id
public Guid TenantId → tenant_id

// Navigation → sans suffixe
public Tenant Tenant { get; set; }
public ICollection<User> Users { get; set; }
```

### 2. Structure des dossiers

```
// ✅ CONVENTION : Structure prévisible
src/
├── Core/
│   └── {Project}.Domain/           # Entités, interfaces, value objects
├── Application/
│   └── {Project}.Application/      # Commands, Queries, DTOs, Handlers
├── Infrastructure/
│   └── {Project}.Infrastructure.{Tech}/  # PostgreSQL, Redis, etc.
└── Presentation/
    └── {Project}.Api/              # Controllers, Middleware

tests/
└── {Project}.{Layer}.Tests/        # Tests par couche
```

### 3. Conventions de nommage des fichiers

```
// ✅ CONVENTION : Nom du fichier = Nom du type
Tenant.cs                           # Contient class Tenant
ITenantRepository.cs                # Contient interface ITenantRepository
CreateTenantCommand.cs              # Contient record CreateTenantCommand
CreateTenantCommandHandler.cs       # Contient class CreateTenantCommandHandler
CreateTenantCommandValidator.cs     # Contient class CreateTenantCommandValidator
```

### 4. Conventions CQRS

```csharp
// ✅ CONVENTION : Nommage des Commands et Queries

// Commands : Verbe + Nom + "Command"
public record CreateTenantCommand : IRequest<Guid>;
public record UpdateTenantCommand : IRequest;
public record DeleteTenantCommand : IRequest;
public record ActivateTenantCommand : IRequest;

// Queries : "Get" + Nom + [Critère] + "Query"
public record GetTenantByIdQuery : IRequest<TenantDto>;
public record GetAllTenantsQuery : IRequest<IReadOnlyList<TenantDto>>;
public record GetTenantsByStatusQuery : IRequest<IReadOnlyList<TenantDto>>;

// Handlers : {Command|Query}Handler
public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>;
public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto>;

// Validators : {Command}Validator
public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>;
```

### 5. Configuration EF Core par convention

```csharp
// ✅ CONVENTION : Configuration minimale, conventions maximales
public class LLMProxyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applique toutes les configurations IEntityTypeConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Convention : snake_case pour PostgreSQL
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName()?.ToSnakeCase());
            
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName()?.ToSnakeCase());
            }
        }
    }
}

// Configuration spécifique uniquement pour les exceptions
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // Seulement ce qui diffère des conventions
        builder.HasIndex(t => t.Name).IsUnique();
        builder.Property(t => t.Name).HasMaxLength(100);
    }
}
```

### 6. Conventions d'API REST

```csharp
// ✅ CONVENTION : Routes prévisibles

// Collection : GET /api/{resources}
[HttpGet]
public Task<IReadOnlyList<TenantDto>> GetAll();

// Item : GET /api/{resources}/{id}
[HttpGet("{id:guid}")]
public Task<TenantDto> GetById(Guid id);

// Create : POST /api/{resources}
[HttpPost]
public Task<IActionResult> Create(CreateTenantRequest request);

// Update : PUT /api/{resources}/{id}
[HttpPut("{id:guid}")]
public Task<IActionResult> Update(Guid id, UpdateTenantRequest request);

// Delete : DELETE /api/{resources}/{id}
[HttpDelete("{id:guid}")]
public Task<IActionResult> Delete(Guid id);

// Actions : POST /api/{resources}/{id}/{action}
[HttpPost("{id:guid}/activate")]
public Task<IActionResult> Activate(Guid id);
```

### 7. Conventions de tests

```csharp
// ✅ CONVENTION : Nommage des tests
public class TenantTests
{
    // Pattern : MethodName_Scenario_ExpectedBehavior
    [Fact]
    public void Create_WithValidData_ShouldCreateTenant() { }
    
    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException() { }
    
    [Fact]
    public void Activate_WhenSuspended_ShouldThrowDomainException() { }
    
    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnTenant() { }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull() { }
}
```

### 8. Quand configurer explicitement ?

```csharp
// ✅ CONFIGURER uniquement les exceptions aux conventions

// Exception : Nom de table différent
builder.ToTable("legacy_customers"); // Table existante avec nom non conventionnel

// Exception : Colonne avec nom spécifique
builder.Property(x => x.Email)
    .HasColumnName("email_address"); // Nom de colonne legacy

// Exception : Relation non standard
builder.HasMany(x => x.Orders)
    .WithOne()
    .HasForeignKey("customer_ref"); // FK non conventionnelle

// Exception : Comportement spécifique au provider
builder.Property(x => x.Metadata)
    .HasColumnType("jsonb"); // Type PostgreSQL spécifique
```

## Conséquences

### Positives

- **Productivité** : Moins de code de configuration à écrire
- **Cohérence** : Tous les développeurs suivent les mêmes conventions
- **Prévisibilité** : Structure et nommage prévisibles
- **Onboarding** : Nouveaux développeurs opérationnels rapidement
- **Maintenance** : Moins de configuration à maintenir

### Négatives

- **Rigidité perçue** : Les conventions peuvent sembler contraignantes
  - *Mitigation* : Les exceptions sont toujours possibles quand justifiées
- **Apprentissage initial** : Connaître les conventions
  - *Mitigation* : Documentation des conventions du projet

### Neutres

- Les conventions doivent être documentées et partagées avec l'équipe

## Alternatives considérées

### Option A : Configuration explicite totale

- **Description** : Tout configurer explicitement
- **Avantages** : Contrôle total, pas d'ambiguïté
- **Inconvénients** : Verbeux, erreurs de configuration, incohérence
- **Raison du rejet** : Le coût de configuration dépasse les bénéfices

### Option B : Pas de convention

- **Description** : Chacun organise comme il veut
- **Avantages** : Liberté totale
- **Inconvénients** : Chaos, incohérence, maintenance difficile
- **Raison du rejet** : L'absence de convention nuit à la collaboration

## Références

- [Convention over Configuration - Wikipedia](https://en.wikipedia.org/wiki/Convention_over_configuration)
- [Ruby on Rails - Convention over Configuration](https://rubyonrails.org/doctrine#convention-over-configuration)
- [ASP.NET Core Conventions](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/application-model)
