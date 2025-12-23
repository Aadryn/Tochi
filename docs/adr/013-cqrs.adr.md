# 13. CQRS - Command Query Responsibility Segregation

Date: 2025-12-21

## Statut

Accepté

## Contexte

Dans une architecture traditionnelle, le même modèle sert pour les lectures et les écritures :
- **Conflits d'optimisation** : Optimiser pour les lectures dégrade les écritures et vice-versa
- **Modèles surchargés** : Le modèle accumule des propriétés pour tous les cas d'usage
- **Complexité des requêtes** : Les lectures nécessitent des jointures complexes sur le modèle d'écriture
- **Scalabilité limitée** : Impossible de scaler lectures et écritures indépendamment

Exemple typique de problème :

```csharp
// ❌ PROBLÈME : Même modèle pour tout
public class Order
{
    public Guid Id { get; set; }
    public Customer Customer { get; set; }      // Nécessaire pour écriture
    public List<OrderItem> Items { get; set; }  // Nécessaire pour écriture
    public decimal Total { get; set; }          // Calculé
    public string CustomerName { get; set; }    // Dénormalisé pour lecture rapide
    public int ItemCount { get; set; }          // Dénormalisé pour lecture rapide
    // Le modèle grossit pour satisfaire tous les besoins...
}
```

## Décision

**Adopter CQRS : séparer les opérations de lecture (Queries) des opérations d'écriture (Commands).**

### Principe fondamental

```
┌──────────────────────────────────────────────────────────────┐
│                         Client                                │
└─────────────────────────┬────────────────────────────────────┘
                          │
          ┌───────────────┴───────────────┐
          │                               │
          ▼                               ▼
┌─────────────────────┐       ┌─────────────────────┐
│      Commands       │       │       Queries       │
│  (Write Operations) │       │  (Read Operations)  │
└──────────┬──────────┘       └──────────┬──────────┘
           │                             │
           ▼                             ▼
┌─────────────────────┐       ┌─────────────────────┐
│  Command Handlers   │       │   Query Handlers    │
│  (Business Logic)   │       │  (Read Optimized)   │
└──────────┬──────────┘       └──────────┬──────────┘
           │                             │
           ▼                             ▼
┌─────────────────────┐       ┌─────────────────────┐
│    Write Model      │       │    Read Model       │
│  (Domain Entities)  │       │  (DTOs, ViewModels) │
└──────────┬──────────┘       └──────────┬──────────┘
           │                             │
           ▼                             ▼
┌─────────────────────┐       ┌─────────────────────┐
│   Write Database    │       │   Read Database     │
│   (Normalized)      │       │  (Denormalized/     │
│                     │       │   Optimized Views)  │
└─────────────────────┘       └─────────────────────┘
```

### 1. Définition des Commands

Les Commands **modifient l'état** du système. Elles sont impératives.

```csharp
// Command = Intention de modifier l'état
public record CreateTenantCommand(
    string Name,
    string ContactEmail,
    TenantPlan Plan
) : IRequest<Guid>;

public record ActivateTenantCommand(Guid TenantId) : IRequest;

public record UpdateTenantQuotaCommand(
    Guid TenantId,
    int NewRequestLimit,
    int NewTokenLimit
) : IRequest;

// Les Commands ne retournent pas de données (ou juste l'ID créé)
// ✅ OK : IRequest<Guid> pour Create (retourne l'ID)
// ✅ OK : IRequest (void) pour Update/Delete
// ❌ NON : IRequest<TenantDto> (c'est une Query déguisée)
```

### 2. Command Handlers

Les handlers exécutent la logique métier et modifient l'état.

```csharp
public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        // Validation métier
        if (await _repository.ExistsByNameAsync(request.Name, ct))
            throw new TenantNameAlreadyExistsException(request.Name);
        
        // Création de l'entité
        var tenant = Tenant.Create(
            request.Name, 
            request.ContactEmail,
            request.Plan);
        
        // Persistance
        await _repository.AddAsync(tenant, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return tenant.Id;
    }
}
```

### 3. Définition des Queries

Les Queries **lisent l'état** sans le modifier. Elles sont interrogatives.

```csharp
// Query = Demande de données
public record GetTenantByIdQuery(Guid TenantId) : IRequest<TenantDto>;

public record GetAllTenantsQuery(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    TenantStatus? Status = null
) : IRequest<PagedResult<TenantSummaryDto>>;

public record GetTenantUsageStatsQuery(
    Guid TenantId,
    DateTime From,
    DateTime To
) : IRequest<TenantUsageStatsDto>;
```

### 4. Query Handlers

Les handlers de query sont optimisés pour la lecture.

```csharp
public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto>
{
    private readonly IDbConnection _connection; // Accès direct SQL pour performance
    
    public async Task<TenantDto> Handle(GetTenantByIdQuery request, CancellationToken ct)
    {
        const string sql = @"
            SELECT 
                t.id,
                t.name,
                t.contact_email,
                t.status,
                t.created_at,
                COUNT(u.id) as user_count,
                COUNT(ak.id) as api_key_count
            FROM tenants t
            LEFT JOIN users u ON u.tenant_id = t.id
            LEFT JOIN api_keys ak ON ak.tenant_id = t.id
            WHERE t.id = @TenantId
            GROUP BY t.id, t.name, t.contact_email, t.status, t.created_at";
        
        var result = await _connection.QuerySingleOrDefaultAsync<TenantDto>(
            sql, 
            new { request.TenantId });
        
        return result ?? throw new TenantNotFoundException(request.TenantId);
    }
}

public class GetAllTenantsQueryHandler 
    : IRequestHandler<GetAllTenantsQuery, PagedResult<TenantSummaryDto>>
{
    private readonly IDbConnection _connection;
    
    public async Task<PagedResult<TenantSummaryDto>> Handle(
        GetAllTenantsQuery request, 
        CancellationToken ct)
    {
        // Query optimisée avec pagination, filtres
        // Pas besoin de charger les entités complètes
        var sql = BuildOptimizedQuery(request);
        var items = await _connection.QueryAsync<TenantSummaryDto>(sql, request);
        var total = await _connection.ExecuteScalarAsync<int>(countSql, request);
        
        return new PagedResult<TenantSummaryDto>(items, total, request.Page, request.PageSize);
    }
}
```

### 5. Structure des dossiers (Vertical Slice + CQRS)

```
src/Application/LLMProxy.Application/
├── Tenants/
│   ├── Commands/
│   │   ├── CreateTenant/
│   │   │   ├── CreateTenantCommand.cs
│   │   │   ├── CreateTenantCommandHandler.cs
│   │   │   └── CreateTenantCommandValidator.cs
│   │   ├── ActivateTenant/
│   │   └── UpdateTenantQuota/
│   ├── Queries/
│   │   ├── GetTenantById/
│   │   │   ├── GetTenantByIdQuery.cs
│   │   │   ├── GetTenantByIdQueryHandler.cs
│   │   │   └── TenantDto.cs
│   │   └── GetAllTenants/
│   │       ├── GetAllTenantsQuery.cs
│   │       ├── GetAllTenantsQueryHandler.cs
│   │       └── TenantSummaryDto.cs
│   └── Events/
│       └── TenantCreatedEvent.cs
├── Users/
│   ├── Commands/
│   └── Queries/
```

### 6. Utilisation dans les Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    // Commands (POST, PUT, DELETE)
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateTenantRequest request, 
        CancellationToken ct)
    {
        var command = new CreateTenantCommand(
            request.Name, 
            request.ContactEmail, 
            request.Plan);
        
        var tenantId = await _mediator.Send(command, ct);
        
        return CreatedAtAction(nameof(GetById), new { id = tenantId }, null);
    }
    
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateTenantCommand(id), ct);
        return NoContent();
    }
    
    // Queries (GET)
    [HttpGet("{id}")]
    public async Task<ActionResult<TenantDto>> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetTenantByIdQuery(id);
        var tenant = await _mediator.Send(query, ct);
        return Ok(tenant);
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<TenantSummaryDto>>> GetAll(
        [FromQuery] GetAllTenantsQuery query,
        CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}
```

### 7. CQRS simplifié vs CQRS complet

**CQRS simplifié (adopté ici)** :
- Même base de données pour lectures et écritures
- Modèles séparés (Commands/Queries) mais stockage partagé
- Queries peuvent utiliser Dapper pour performance

**CQRS complet (optionnel, plus tard si nécessaire)** :
- Bases de données séparées (Write DB + Read DB)
- Synchronisation via événements (Event Sourcing)
- Eventual consistency entre les modèles

## Conséquences

### Positives

- **Optimisation ciblée** : Queries optimisées sans impacter les Commands
- **Simplicité des modèles** : Chaque côté a son modèle adapté
- **Scalabilité** : Lectures et écritures peuvent scaler indépendamment
- **Testabilité** : Commands et Queries testables isolément
- **Clarté** : Intention claire (lecture vs modification)

### Négatives

- **Plus de code** : Duplication apparente (DTOs queries vs entities)
  - *Mitigation* : La clarté compense le code supplémentaire
- **Synchronisation** : En CQRS complet, gérer l'eventual consistency
  - *Mitigation* : Commencer par CQRS simplifié (même BDD)
- **Courbe d'apprentissage** : Pattern moins familier
  - *Mitigation* : Formation équipe, exemples clairs

### Neutres

- CQRS simplifié est un bon point de départ ; évoluer vers CQRS complet si besoin

## Alternatives considérées

### Option A : Modèle unique (CRUD)

- **Description** : Même modèle pour toutes les opérations
- **Avantages** : Simple, familier
- **Inconvénients** : Modèles surchargés, optimisation difficile
- **Raison du rejet** : Ne scale pas avec la complexité du domaine

### Option B : CQRS + Event Sourcing immédiat

- **Description** : Implémenter Event Sourcing dès le départ
- **Avantages** : Audit complet, replay d'événements
- **Inconvénients** : Complexité massive, overkill pour ce projet
- **Raison du rejet** : YAGNI - commencer simple, évoluer si nécessaire

## Références

- [CQRS - Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [CQRS Documents - Greg Young](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf)
- [MediatR - Jimmy Bogard](https://github.com/jbogard/MediatR)
