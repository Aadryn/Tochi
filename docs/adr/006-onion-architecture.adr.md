# 6. Onion Architecture

Date: 2025-12-21

## Statut

Accepté

## Contexte

L'architecture traditionnelle en couches (N-tier) présente des limitations :
- **Dépendances descendantes** : UI → Business → Data Access → Database
- **Couplage fort à l'infrastructure** : Le domaine métier dépend de la base de données
- **Testabilité limitée** : Difficile de tester le métier sans la base de données
- **Rigidité** : Changer de technologie d'infrastructure impacte le cœur métier

Ces problèmes deviennent critiques quand :
- Le domaine métier est complexe et doit être protégé
- Les technologies d'infrastructure évoluent (changement de BDD, cloud, etc.)
- Les tests automatisés sont essentiels
- Plusieurs interfaces (API, Web, CLI) consomment le même domaine

## Décision

**Adopter l'Onion Architecture pour structurer l'application.**

### Principe fondamental

**Les dépendances pointent vers l'intérieur.** Le domaine métier est au centre et ne dépend de rien.

```
┌─────────────────────────────────────────────────────────┐
│                    Infrastructure                        │
│  ┌─────────────────────────────────────────────────┐    │
│  │                  Application                     │    │
│  │  ┌─────────────────────────────────────────┐    │    │
│  │  │              Domain Services             │    │    │
│  │  │  ┌─────────────────────────────────┐    │    │    │
│  │  │  │         Domain Model            │    │    │    │
│  │  │  │    (Entities, Value Objects)    │    │    │    │
│  │  │  └─────────────────────────────────┘    │    │    │
│  │  └─────────────────────────────────────────┘    │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

### Structure des couches

#### 1. Domain Model (Centre)

Entités, Value Objects, Enums du domaine. **Aucune dépendance externe.**

```
src/Core/LLMProxy.Domain/
├── Entities/
│   ├── Tenant.cs
│   ├── User.cs
│   └── ApiKey.cs
├── ValueObjects/
│   ├── Email.cs
│   └── ApiKeyHash.cs
├── Enums/
│   └── TenantStatus.cs
└── Interfaces/
    ├── IRepository.cs
    └── ITenantRepository.cs
```

```csharp
// Domain Model : AUCUNE dépendance externe
namespace LLMProxy.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; private set; }
    public TenantStatus Status { get; private set; }
    
    // Logique métier encapsulée
    public void Activate()
    {
        if (Status == TenantStatus.Suspended)
            throw new DomainException("Cannot activate suspended tenant");
        Status = TenantStatus.Active;
    }
}
```

#### 2. Domain Services

Services métier utilisant les entités. Définition des interfaces (ports).

```
src/Core/LLMProxy.Domain/
└── Services/
    └── ITenantDomainService.cs
```

#### 3. Application Layer

Cas d'utilisation, orchestration, CQRS, DTOs.

```
src/Application/LLMProxy.Application/
├── Tenants/
│   ├── Commands/
│   │   └── CreateTenantCommand.cs
│   ├── Queries/
│   │   └── GetTenantQuery.cs
│   └── DTOs/
│       └── TenantDto.cs
└── Common/
    └── IUnitOfWork.cs
```

```csharp
// Application : Dépend du Domain, pas de l'Infrastructure
namespace LLMProxy.Application.Tenants.Commands;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _repository; // Interface du Domain
    private readonly IUnitOfWork _unitOfWork;       // Interface du Domain
    
    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        var tenant = new Tenant(request.Name);
        await _repository.AddAsync(tenant, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return tenant.Id;
    }
}
```

#### 4. Infrastructure Layer (Extérieur)

Implémentations concrètes : BDD, cache, APIs externes, etc.

```
src/Infrastructure/
├── LLMProxy.Infrastructure.PostgreSQL/
│   ├── Repositories/
│   │   └── TenantRepository.cs      # Implémente ITenantRepository
│   └── LLMProxyDbContext.cs
├── LLMProxy.Infrastructure.Redis/
│   └── CacheService.cs              # Implémente ICacheService
└── LLMProxy.Infrastructure.Security/
    └── SecretService.cs             # Implémente ISecretService
```

```csharp
// Infrastructure : Implémente les interfaces du Domain
namespace LLMProxy.Infrastructure.PostgreSQL.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly LLMProxyDbContext _context;
    
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Tenants.FindAsync(id, ct);
    }
}
```

### Règle des dépendances

```
Presentation → Application → Domain ← Infrastructure
                    ↑                      │
                    └──────────────────────┘
```

- **Domain** : Aucune dépendance (centre)
- **Application** : Dépend de Domain uniquement
- **Infrastructure** : Dépend de Domain (implémente ses interfaces)
- **Presentation** : Dépend de Application

### Configuration de l'injection de dépendances

```csharp
// Program.cs - Composition Root
services.AddScoped<ITenantRepository, TenantRepository>();
services.AddScoped<ICacheService, RedisCacheService>();
services.AddScoped<ISecretService, VaultSecretService>();
```

## Conséquences

### Positives

- **Domaine protégé** : Le cœur métier est isolé des détails techniques
- **Testabilité maximale** : Domain et Application testables sans infrastructure
- **Flexibilité infrastructure** : Changer de BDD/cache sans toucher au métier
- **Indépendance des frameworks** : Le domaine ne connaît pas EF Core, MediatR, etc.
- **Évolutivité** : Ajouter des interfaces (API GraphQL, CLI) sans dupliquer le métier

### Négatives

- **Complexité structurelle** : Plus de projets et de fichiers
  - *Mitigation* : Structure claire et conventions de nommage
- **Indirection** : Navigation indirecte via interfaces
  - *Mitigation* : Outils IDE (Go to Implementation)
- **Mapping** : Nécessité de mapper entre couches (Entity ↔ DTO)
  - *Mitigation* : Utiliser AutoMapper ou mappings explicites simples

### Neutres

- Courbe d'apprentissage pour les développeurs habitués au N-tier classique

## Alternatives considérées

### Option A : Architecture N-Tier classique

- **Description** : UI → Business → DAL → Database
- **Avantages** : Simple, familière
- **Inconvénients** : Domaine couplé à l'infrastructure, difficile à tester
- **Raison du rejet** : Ne protège pas suffisamment le domaine métier

### Option B : Pas d'architecture formelle

- **Description** : Code organisé librement
- **Avantages** : Flexibilité maximale initialement
- **Inconvénients** : Dégradation rapide, couplage anarchique
- **Raison du rejet** : Intenable à moyen/long terme

## Références

- [The Onion Architecture - Jeffrey Palermo](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/)
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design - Eric Evans](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)
