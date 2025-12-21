# Architecture du Projet LLM Proxy

## 📐 Vue d'ensemble

Le projet suit une **Architecture Hexagonale** (Ports & Adapters) avec les principes **SOLID**, **DRY**, **KISS**, et **YAGNI**.

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Presentation Layer                           │
│                                                                       │
│  ┌──────────────────────┐          ┌──────────────────────┐         │
│  │   Gateway (YARP)     │          │     Admin API        │         │
│  │   - Reverse Proxy    │          │   - REST Endpoints   │         │
│  │   - Middlewares      │          │   - CRUD Operations  │         │
│  │   - Streaming        │          │                      │         │
│  └──────────────────────┘          └──────────────────────┘         │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        Application Layer                             │
│                                                                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────┐  │
│  │   Commands   │  │    Queries   │  │   DTOs / Validators      │  │
│  │   (CQRS)     │  │   (CQRS)     │  │   (FluentValidation)     │  │
│  └──────────────┘  └──────────────┘  └──────────────────────────┘  │
│                                                                       │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │              MediatR Pipeline Behaviors                       │   │
│  │   - Validation - Logging - Transaction - Exception Handling  │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                          Domain Layer (Core)                         │
│                                                                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────────┐    │
│  │    Entities     │  │  Value Objects  │  │   Aggregates     │    │
│  │   - Tenant      │  │   - Settings    │  │   - Tenant       │    │
│  │   - User        │  │   - Config      │  │                  │    │
│  │   - ApiKey      │  │   - Strategy    │  │                  │    │
│  │   - Provider    │  │                 │  │                  │    │
│  │   - QuotaLimit  │  └─────────────────┘  └──────────────────┘    │
│  │   - AuditLog    │                                                │
│  └─────────────────┘                                                │
│                                                                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────────┐    │
│  │   Interfaces    │  │  Domain Events  │  │  Domain Services │    │
│  │   (Ports)       │  │                 │  │                  │    │
│  └─────────────────┘  └─────────────────┘  └──────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      Infrastructure Layer                            │
│                                                                       │
│  ┌───────────────┐  ┌──────────────┐  ┌──────────────────────┐     │
│  │  PostgreSQL   │  │    Redis     │  │   LLM Providers      │     │
│  │  - Repos      │  │  - Cache     │  │   - OpenAI Client    │     │
│  │  - EF Core    │  │  - Quota     │  │   - Anthropic Client │     │
│  │  - UnitOfWork │  │  - Session   │  │   - Ollama Client    │     │
│  └───────────────┘  └──────────────┘  └──────────────────────┘     │
│                                                                       │
│  ┌───────────────┐  ┌──────────────┐  ┌──────────────────────┐     │
│  │   Security    │  │  Telemetry   │  │   External Services  │     │
│  │  - JWT        │  │  - OTel      │  │   - KeyVault         │     │
│  │  - ApiKey     │  │  - Metrics   │  │   - SMTP             │     │
│  │  - Cert Auth  │  │  - Tracing   │  │                      │     │
│  └───────────────┘  └──────────────┘  └──────────────────────┘     │
└─────────────────────────────────────────────────────────────────────┘
```

## 🏗️ Structure des Dossiers

```
LLMProxy/
├── src/
│   ├── Core/                              # ❤️ Cœur du domaine (sans dépendances)
│   │   └── LLMProxy.Domain/
│   │       ├── Common/                    # Base classes (Entity, ValueObject, Result)
│   │       ├── Entities/                  # Entités métier
│   │       │   ├── Tenant.cs             # Agrégat racine pour tenant
│   │       │   ├── User.cs               # Entité utilisateur
│   │       │   ├── ApiKey.cs             # Entité API key
│   │       │   ├── LLMProvider.cs        # Configuration provider
│   │       │   ├── QuotaLimit.cs         # Limites de quota
│   │       │   └── AuditLog.cs           # Journal d'audit
│   │       └── Interfaces/                # Ports (abstractions)
│   │           ├── IRepositories.cs       # Contrats de repository
│   │           └── IServices.cs           # Contrats de services
│   │
│   ├── Application/                       # 🎯 Logique applicative (Use Cases)
│   │   └── LLMProxy.Application/
│   │       ├── Common/                    # CQRS base, DTOs
│   │       ├── Tenants/
│   │       │   ├── Commands/              # Commandes (Create, Update, Delete)
│   │       │   └── Queries/               # Requêtes (Get, List)
│   │       └── Users/
│   │           ├── Commands/
│   │           └── Queries/
│   │
│   ├── Infrastructure/                    # 🔌 Adaptateurs (implémentations)
│   │   ├── LLMProxy.Infrastructure.PostgreSQL/
│   │   │   ├── Configurations/            # EF Core configurations
│   │   │   ├── Repositories/              # Implémentations repositories
│   │   │   ├── LLMProxyDbContext.cs       # DbContext
│   │   │   └── UnitOfWork.cs              # Unit of Work pattern
│   │   │
│   │   ├── LLMProxy.Infrastructure.Redis/
│   │   │   ├── Services/                  # Cache, quota services
│   │   │   └── Configuration/
│   │   │
│   │   ├── LLMProxy.Infrastructure.Security/
│   │   │   ├── Authentication/            # JWT, ApiKey, Certificate
│   │   │   └── Secrets/                   # KeyVault, HashiCorp Vault
│   │   │
│   │   ├── LLMProxy.Infrastructure.Telemetry/
│   │   │   ├── OpenTelemetry/            # Tracing, metrics
│   │   │   └── Logging/
│   │   │
│   │   └── LLMProxy.Infrastructure.LLMProviders/
│   │       ├── OpenAI/                    # Client OpenAI
│   │       ├── Anthropic/                 # Client Anthropic
│   │       ├── Ollama/                    # Client Ollama
│   │       ├── Polly/                     # Résilience policies
│   │       └── TokenCounter/              # SharpToken integration
│   │
│   └── Presentation/                      # 🌐 Interfaces utilisateur
│       ├── LLMProxy.Gateway/              # API Gateway (YARP)
│       │   ├── Middleware/                # Custom middlewares
│       │   │   ├── RequestLoggingMiddleware.cs
│       │   │   ├── ApiKeyAuthenticationMiddleware.cs
│       │   │   ├── QuotaEnforcementMiddleware.cs
│       │   │   └── StreamInterceptionMiddleware.cs
│       │   ├── Program.cs                 # Configuration & pipeline
│       │   └── appsettings.json           # Configuration YARP
│       │
│       └── LLMProxy.Admin.API/            # Admin REST API
│           ├── Controllers/               # REST endpoints
│           ├── Program.cs
│           └── appsettings.json
│
├── tests/
│   ├── LLMProxy.Domain.Tests/            # 🧪 Tests unitaires domaine
│   │   └── Entities/
│   │       └── TenantTests.cs            # Tests TDD pour Tenant
│   │
│   ├── LLMProxy.Application.Tests/        # Tests use cases
│   └── LLMProxy.Integration.Tests/        # Tests d'intégration
│
├── docker/                                # 🐳 Dockerfiles
│   ├── Gateway.Dockerfile
│   └── Admin.Dockerfile
│
├── kubernetes/                            # ☸️ Manifests K8s
│   ├── deployment.yaml
│   ├── service.yaml
│   └── configmap.yaml
│
├── docs/                                  # 📚 Documentation
│   ├── architecture/
│   ├── api/
│   └── deployment/
│
├── docker-compose.yml                     # Orchestration services
├── LLMProxy.sln                           # Solution .NET
├── README.md                              # Présentation projet
├── GETTING_STARTED.md                     # Guide démarrage
└── .gitignore

```

## 🎭 Principes Appliqués

### SOLID

- **S** - Single Responsibility: Chaque classe a une seule raison de changer
  - `Tenant` gère uniquement la logique de tenant
  - `TenantRepository` gère uniquement la persistance
  - `CreateTenantCommand` gère uniquement la création

- **O** - Open/Closed: Ouvert à l'extension, fermé à la modification
  - Interfaces (`IRepository`) permettent l'extension
  - Nouveaux providers via nouvelles implémentations

- **L** - Liskov Substitution: Les implémentations sont interchangeables
  - Tout `ISecretService` peut remplacer un autre

- **I** - Interface Segregation: Interfaces spécifiques
  - Pas d'interface monolithique, mais des contrats ciblés

- **D** - Dependency Inversion: Dépendre des abstractions
  - Application dépend de `IUnitOfWork`, pas de `EfCoreUnitOfWork`

### YAGNI (You Aren't Gonna Need It)

- Pas de sur-ingénierie
- Implémenter uniquement ce qui est nécessaire maintenant
- Les TODOs marquent les extensions futures

### KISS (Keep It Simple, Stupid)

- Code simple et lisible
- Pas de patterns complexes inutiles
- Nommage clair et explicite

### DRY (Don't Repeat Yourself)

- Logique partagée dans classes de base (`Entity`, `ValueObject`)
- Réutilisation via composition et héritage approprié

## 🔄 Flux de Données (Exemple: Créer un Tenant)

```
1. HTTP Request
   ↓
2. Gateway Controller/Endpoint
   ↓
3. MediatR Command (CreateTenantCommand)
   ↓
4. FluentValidation Validator
   ↓
5. Command Handler
   ↓
6. Domain Entity (Tenant.Create)
   ↓
7. Repository (ITenantRepository)
   ↓
8. EF Core / PostgreSQL
   ↓
9. Domain Events (si nécessaire)
   ↓
10. Response DTO
    ↓
11. HTTP Response
```

## 🧩 Patterns Utilisés

| Pattern | Où | Pourquoi |
|---------|-----|----------|
| **Repository** | Infrastructure | Abstraction de la persistance |
| **Unit of Work** | Infrastructure | Gestion transactionnelle |
| **CQRS** | Application | Séparation lecture/écriture |
| **Mediator** | Application | Découplage handlers |
| **Strategy** | Domain | Routing configurable |
| **Builder** | Domain | Construction objets complexes |
| **Factory** | Domain | Création entités validées |
| **Value Object** | Domain | Immutabilité concepts métier |
| **Specification** | Domain | Logique de filtrage réutilisable |

## 🔐 Sécurité

- **Multi-niveaux**: API Key → JWT → Certificate
- **Secrets**: Azure KeyVault, HashiCorp Vault, chiffrement DB
- **Audit**: Tous les appels loggés avec anonymisation configurable

## 📊 Observabilité

- **OpenTelemetry**: Tracing distribué
- **Prometheus**: Métriques temps réel
- **Grafana**: Dashboards
- **Jaeger**: Visualisation traces

## 🚀 Déploiement

- **Docker**: Images multi-stage optimisées
- **Kubernetes**: Manifests prêts pour prod
- **Cloud-agnostic**: On-premise, Azure, AWS, GCP

## 📈 Prochaines Implémentations

1. ✅ Compléter tous les repositories
2. ✅ Implémenter QuotaService avec Redis
3. ✅ Ajouter TokenCounterService (SharpToken)
4. ✅ Créer Admin API complète
5. ✅ Implémenter politiques Polly (retry, circuit breaker)
6. ✅ Ajouter support semantic cache
7. ✅ Créer React Admin UI (micro frontend)
8. ✅ Tests d'intégration complets
9. ✅ Documentation API (Swagger/OpenAPI)
10. ✅ CI/CD pipelines

---

**Créé avec ❤️ en suivant les meilleures pratiques .NET et les principes Clean Architecture**
