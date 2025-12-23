# 7. Vertical Slice Architecture

Date: 2025-12-21

## Statut

Accepté

## Contexte

L'architecture en couches horizontales (Onion, Clean) peut mener à :
- **Dispersion du code** : Une fonctionnalité répartie sur 5+ projets/dossiers
- **Couplage horizontal** : Les classes d'une couche dépendent les unes des autres
- **Abstractions génériques** : `IRepository<T>` utilisé partout, même quand inutile
- **Modifications en cascade** : Ajouter un champ = modifier 4 fichiers dans 4 couches

Exemple de dispersion pour "Créer un Tenant" :
```
Controllers/TenantController.cs      → Endpoint
Services/ITenantService.cs           → Interface
Services/TenantService.cs            → Implémentation
Repositories/ITenantRepository.cs    → Interface
Repositories/TenantRepository.cs     → Implémentation
DTOs/CreateTenantDto.cs              → DTO entrée
DTOs/TenantResponseDto.cs            → DTO sortie
Validators/CreateTenantValidator.cs  → Validation
```

## Décision

**Adopter la Vertical Slice Architecture pour organiser le code par fonctionnalité (feature) plutôt que par couche technique.**

### Principe fondamental

**Chaque fonctionnalité est autonome et contient tout son code.**

```
Organisation horizontale (couches) :     Organisation verticale (slices) :
                                         
├── Controllers/                         ├── Features/
│   ├── TenantController.cs              │   ├── Tenants/
│   ├── UserController.cs                │   │   ├── CreateTenant/
│   └── ApiKeyController.cs              │   │   │   ├── CreateTenantCommand.cs
├── Services/                            │   │   │   ├── CreateTenantHandler.cs
│   ├── TenantService.cs                 │   │   │   └── CreateTenantEndpoint.cs
│   ├── UserService.cs                   │   │   ├── GetTenant/
│   └── ApiKeyService.cs                 │   │   │   ├── GetTenantQuery.cs
├── Repositories/                        │   │   │   ├── GetTenantHandler.cs
│   ├── TenantRepository.cs              │   │   │   └── GetTenantEndpoint.cs
│   ├── UserRepository.cs                │   │   └── DeleteTenant/
│   └── ApiKeyRepository.cs              │   │       └── ...
├── DTOs/                                │   ├── Users/
│   └── ...                              │   │   └── ...
```

### Structure d'une slice

Chaque slice contient **tout** ce qui est nécessaire à la fonctionnalité :

```
Features/Tenants/CreateTenant/
├── CreateTenantCommand.cs       # Requête (DTO entrée)
├── CreateTenantHandler.cs       # Logique métier
├── CreateTenantResponse.cs      # DTO sortie (si différent)
├── CreateTenantValidator.cs     # Validation FluentValidation
└── CreateTenantEndpoint.cs      # Endpoint API (optionnel)
```

### Exemple complet d'une slice

```csharp
// Features/Tenants/CreateTenant/CreateTenantCommand.cs
namespace LLMProxy.Application.Features.Tenants.CreateTenant;

public record CreateTenantCommand(string Name, string ContactEmail) : IRequest<CreateTenantResponse>;

public record CreateTenantResponse(Guid Id, string Name);
```

```csharp
// Features/Tenants/CreateTenant/CreateTenantHandler.cs
namespace LLMProxy.Application.Features.Tenants.CreateTenant;

public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, CreateTenantResponse>
{
    private readonly LLMProxyDbContext _context; // Accès direct au DbContext
    
    public CreateTenantHandler(LLMProxyDbContext context)
    {
        _context = context;
    }
    
    public async Task<CreateTenantResponse> Handle(
        CreateTenantCommand request, 
        CancellationToken ct)
    {
        var tenant = new Tenant(request.Name, request.ContactEmail);
        
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(ct);
        
        return new CreateTenantResponse(tenant.Id, tenant.Name);
    }
}
```

```csharp
// Features/Tenants/CreateTenant/CreateTenantValidator.cs
namespace LLMProxy.Application.Features.Tenants.CreateTenant;

public class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
            
        RuleFor(x => x.ContactEmail)
            .NotEmpty()
            .EmailAddress();
    }
}
```

### Cohabitation avec Onion Architecture

Vertical Slice et Onion ne sont **pas mutuellement exclusifs** :

```
src/
├── Core/
│   └── LLMProxy.Domain/           # Entités partagées (Onion)
├── Application/
│   └── LLMProxy.Application/
│       ├── Features/              # Vertical Slices
│       │   ├── Tenants/
│       │   │   ├── CreateTenant/
│       │   │   └── GetTenant/
│       │   └── Users/
│       │       └── ...
│       └── Common/                # Code partagé entre slices
│           ├── Behaviors/
│           └── Exceptions/
└── Infrastructure/                # Implémentations (Onion)
```

### Règles d'application

1. **Isolation des slices** : Une slice ne doit pas dépendre d'une autre slice
2. **Code partagé minimal** : Extraire dans `Common/` uniquement si 3+ slices utilisent
3. **Pas de repository générique** : Chaque slice accède aux données comme elle le souhaite
4. **DTOs spécifiques** : Chaque slice a ses propres DTOs (pas de partage par défaut)

### Quand partager du code ?

```csharp
// ❌ ÉVITER : Partage prématuré
public interface ITenantService
{
    Task<Tenant> GetByIdAsync(Guid id);
    Task<Tenant> CreateAsync(CreateTenantDto dto);
    Task UpdateAsync(Guid id, UpdateTenantDto dto);
    Task DeleteAsync(Guid id);
}

// ✅ PRÉFÉRER : Chaque slice autonome
// CreateTenant/CreateTenantHandler.cs → Accès direct DbContext
// GetTenant/GetTenantHandler.cs → Accès direct DbContext
// UpdateTenant/UpdateTenantHandler.cs → Accès direct DbContext
```

**Partager uniquement** :
- Entités du Domain (partagées par définition)
- Cross-cutting concerns (logging, validation pipeline, etc.)
- Infrastructure technique (DbContext, services externes)

## Conséquences

### Positives

- **Cohésion forte** : Tout le code d'une feature au même endroit
- **Modifications localisées** : Changer une feature = modifier 1 dossier
- **Couplage faible** : Les slices sont indépendantes entre elles
- **Simplicité** : Pas besoin de repository/service si la slice est simple
- **Onboarding facilité** : Comprendre une feature = lire un dossier
- **Suppression facile** : Supprimer une feature = supprimer un dossier

### Négatives

- **Duplication potentielle** : Code similaire dans plusieurs slices
  - *Mitigation* : Acceptable tant que les slices évoluent indépendamment ; extraire si vraiment identique
- **Pas de vue d'ensemble** : Difficile de voir "tous les endpoints Tenant"
  - *Mitigation* : Documentation API (Swagger) et conventions de nommage
- **Discipline requise** : Résister à la tentation de partager prématurément
  - *Mitigation* : Revues de code pour maintenir l'isolation

### Neutres

- Compatible avec CQRS/MediatR qui encouragent naturellement les slices

## Alternatives considérées

### Option A : Architecture en couches pure

- **Description** : Organisation stricte par couche technique
- **Avantages** : Familière, séparation claire des responsabilités techniques
- **Inconvénients** : Dispersion des features, couplage horizontal
- **Raison du rejet** : Rend les modifications plus coûteuses et risquées

### Option B : Microservices

- **Description** : Un service par fonctionnalité/domaine
- **Avantages** : Isolation maximale, déploiement indépendant
- **Inconvénients** : Complexité opérationnelle massive, overkill pour ce projet
- **Raison du rejet** : Prématuré ; Vertical Slice offre les bénéfices sans la complexité

## Références

- [Vertical Slice Architecture - Jimmy Bogard](https://www.jimmybogard.com/vertical-slice-architecture/)
- [CQRS Documents - Greg Young](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf)
- [Feature Folders - Steve Smith](https://ardalis.com/feature-folders/)
