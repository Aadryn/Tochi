# 8. Hexagonal Architecture (Ports and Adapters)

Date: 2025-12-21

## Statut

Accepté

## Contexte

Les applications modernes interagissent avec de nombreux systèmes externes :
- Bases de données (PostgreSQL, Redis)
- APIs tierces (OpenAI, Anthropic, Azure)
- Files de messages (RabbitMQ, Kafka)
- Services d'authentification (OAuth, LDAP)
- Interfaces utilisateur (Web, API, CLI)

L'architecture traditionnelle crée des dépendances fortes vers ces systèmes :
- **Couplage technologique** : Le code métier appelle directement EF Core, HttpClient, etc.
- **Tests difficiles** : Impossible de tester sans les vrais systèmes externes
- **Rigidité** : Changer de provider (ex: OpenAI → Anthropic) impacte tout le code
- **Vendor lock-in** : Dépendance excessive à des technologies spécifiques

## Décision

**Adopter l'Hexagonal Architecture (Ports and Adapters) pour isoler le cœur métier des détails d'infrastructure.**

### Principe fondamental

L'application est vue comme un **hexagone** avec :
- **Le cœur (Domain)** : Logique métier pure, sans dépendance externe
- **Les Ports** : Interfaces définissant les interactions (entrantes et sortantes)
- **Les Adapters** : Implémentations concrètes des ports

```
                    ┌─────────────────────┐
                    │   REST API Adapter   │
                    │   (Driving/Primary)  │
                    └──────────┬──────────┘
                               │
                    ┌──────────▼──────────┐
                    │     Input Port       │
                    │  (ITenantService)    │
                    └──────────┬──────────┘
                               │
        ┌──────────────────────┼──────────────────────┐
        │                      │                       │
        │            ┌─────────▼─────────┐            │
        │            │                    │            │
        │            │   DOMAIN CORE      │            │
        │            │   (Business Logic) │            │
        │            │                    │            │
        │            └─────────┬─────────┘            │
        │                      │                       │
        └──────────────────────┼──────────────────────┘
                               │
                    ┌──────────▼──────────┐
                    │    Output Port       │
                    │ (ITenantRepository)  │
                    └──────────┬──────────┘
                               │
                    ┌──────────▼──────────┐
                    │  PostgreSQL Adapter  │
                    │  (Driven/Secondary)  │
                    └─────────────────────┘
```

### Types de Ports

#### Ports Primaires (Driving/Input)

Exposent les cas d'utilisation de l'application. Appelés par l'extérieur.

```csharp
// Port primaire : Ce que l'application OFFRE
namespace LLMProxy.Domain.Ports.Input;

public interface ITenantManagement
{
    Task<TenantId> CreateTenantAsync(CreateTenantRequest request, CancellationToken ct);
    Task<Tenant> GetTenantAsync(TenantId id, CancellationToken ct);
    Task ActivateTenantAsync(TenantId id, CancellationToken ct);
    Task SuspendTenantAsync(TenantId id, string reason, CancellationToken ct);
}
```

#### Ports Secondaires (Driven/Output)

Définissent ce dont l'application a BESOIN. Implémentés par l'infrastructure.

```csharp
// Port secondaire : Ce dont l'application A BESOIN
namespace LLMProxy.Domain.Ports.Output;

public interface ITenantRepository
{
    Task<Tenant?> FindByIdAsync(TenantId id, CancellationToken ct);
    Task SaveAsync(Tenant tenant, CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct);
}

public interface ILLMProvider
{
    Task<CompletionResponse> CompleteAsync(CompletionRequest request, CancellationToken ct);
    Task<EmbeddingResponse> EmbedAsync(EmbeddingRequest request, CancellationToken ct);
}

public interface INotificationService
{
    Task NotifyTenantCreatedAsync(Tenant tenant, CancellationToken ct);
}
```

### Types d'Adapters

#### Adapters Primaires (Driving)

Appellent les ports primaires. Ce sont les points d'entrée de l'application.

```csharp
// Adapter primaire : REST API
namespace LLMProxy.Adapters.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly ITenantManagement _tenantManagement; // Port primaire
    
    public TenantsController(ITenantManagement tenantManagement)
    {
        _tenantManagement = tenantManagement;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateTenantRequest request, CancellationToken ct)
    {
        var tenantId = await _tenantManagement.CreateTenantAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = tenantId }, null);
    }
}
```

#### Adapters Secondaires (Driven)

Implémentent les ports secondaires. Ce sont les connexions vers l'extérieur.

```csharp
// Adapter secondaire : PostgreSQL
namespace LLMProxy.Adapters.PostgreSQL.Repositories;

public class PostgresTenantRepository : ITenantRepository
{
    private readonly LLMProxyDbContext _context;
    
    public PostgresTenantRepository(LLMProxyDbContext context)
    {
        _context = context;
    }
    
    public async Task<Tenant?> FindByIdAsync(TenantId id, CancellationToken ct)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id.Value, ct);
    }
    
    public async Task SaveAsync(Tenant tenant, CancellationToken ct)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(ct);
    }
}
```

```csharp
// Adapter secondaire : OpenAI
namespace LLMProxy.Adapters.OpenAI;

public class OpenAIProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIOptions _options;
    
    public async Task<CompletionResponse> CompleteAsync(
        CompletionRequest request, 
        CancellationToken ct)
    {
        // Appel HTTP vers OpenAI API
        var response = await _httpClient.PostAsJsonAsync(
            "https://api.openai.com/v1/chat/completions",
            MapToOpenAIRequest(request),
            ct);
            
        var openAIResponse = await response.Content
            .ReadFromJsonAsync<OpenAIChatResponse>(ct);
            
        return MapToCompletionResponse(openAIResponse);
    }
}
```

### Structure du projet

```
src/
├── Core/
│   └── LLMProxy.Domain/
│       ├── Entities/                    # Modèle de domaine
│       ├── ValueObjects/
│       ├── Ports/
│       │   ├── Input/                   # Ports primaires
│       │   │   ├── ITenantManagement.cs
│       │   │   └── ILLMGateway.cs
│       │   └── Output/                  # Ports secondaires
│       │       ├── ITenantRepository.cs
│       │       ├── ILLMProvider.cs
│       │       └── ICacheService.cs
│       └── Services/                    # Implémentation des ports primaires
│           └── TenantManagementService.cs
│
├── Adapters/
│   ├── LLMProxy.Adapters.Api/           # Adapter primaire : REST API
│   │   └── Controllers/
│   ├── LLMProxy.Adapters.PostgreSQL/    # Adapter secondaire : BDD
│   │   └── Repositories/
│   ├── LLMProxy.Adapters.Redis/         # Adapter secondaire : Cache
│   ├── LLMProxy.Adapters.OpenAI/        # Adapter secondaire : LLM
│   └── LLMProxy.Adapters.Anthropic/     # Adapter secondaire : LLM
│
└── Composition/
    └── LLMProxy.Bootstrap/              # Composition root, DI
```

### Avantage clé : Interchangeabilité

```csharp
// Configuration DI - Changer d'adapter = changer une ligne
services.AddScoped<ILLMProvider, OpenAIProvider>();      // Production avec OpenAI
// services.AddScoped<ILLMProvider, AnthropicProvider>(); // Alternative
// services.AddScoped<ILLMProvider, FakeLLMProvider>();   // Tests

services.AddScoped<ITenantRepository, PostgresTenantRepository>();  // Production
// services.AddScoped<ITenantRepository, InMemoryTenantRepository>(); // Tests
```

## Conséquences

### Positives

- **Testabilité totale** : Domain testable avec des fake adapters
- **Indépendance technologique** : Changer de BDD/provider = nouveau adapter
- **Clarté des contrats** : Les ports documentent explicitement les interactions
- **Évolutivité** : Ajouter un nouveau provider (ex: Gemini) = nouvel adapter
- **Isolation des erreurs** : Problème d'adapter ≠ problème de domain

### Négatives

- **Indirection supplémentaire** : Plus d'interfaces et de mapping
  - *Mitigation* : IDE moderne avec "Go to Implementation"
- **Mapping entre modèles** : Conversion Domain ↔ Adapter models
  - *Mitigation* : Utiliser des records pour les mappings simples
- **Surcoût initial** : Plus de structure à mettre en place
  - *Mitigation* : Templates de projet et conventions claires

### Neutres

- Similaire à Onion Architecture ; l'hexagonale met l'accent sur les ports/adapters

## Alternatives considérées

### Option A : Dépendances directes

- **Description** : Le code métier utilise directement EF Core, HttpClient, etc.
- **Avantages** : Moins de code, plus direct
- **Inconvénients** : Couplage fort, tests difficiles, changement de techno coûteux
- **Raison du rejet** : Inacceptable pour un projet de cette envergure

### Option B : Abstraction partielle

- **Description** : Abstraire uniquement certains systèmes externes
- **Avantages** : Moins de travail initial
- **Inconvénients** : Incohérence, dette technique future
- **Raison du rejet** : Mieux vaut une approche cohérente dès le départ

## Références

- [Hexagonal Architecture - Alistair Cockburn](https://alistair.cockburn.us/hexagonal-architecture/)
- [Ports and Adapters Pattern - Wikipedia](https://en.wikipedia.org/wiki/Hexagonal_architecture_(software))
- [DDD, Hexagonal, Onion, Clean, CQRS - Herberto Graça](https://herbertograca.com/2017/11/16/explicit-architecture-01-ddd-hexagonal-onion-clean-cqrs-how-i-put-it-all-together/)
