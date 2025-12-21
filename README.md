# LLM Proxy - Multi-Tenant Gateway

**Status:** ✅ **Production Ready** | **Build:** ✅ All Tests Passing | **Migration:** ✅ Created

Enterprise-grade reverse proxy for LLM providers with comprehensive security, monitoring, and quota management.

---

## 🚀 Quick Start

```powershell
# Prerequisites: Docker Desktop running

# 1. Automated setup (starts infrastructure, applies migration)
.\setup.ps1

# 2. Start both services
.\start-services.ps1

# 3. Test the system
.\test.ps1

# 4. Follow detailed steps in NEXT_STEPS.md
```

**Manual Setup:**
```powershell
docker-compose up -d
dotnet ef database update --project src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL --startup-project src\Presentation\LLMProxy.Admin.API
dotnet run --project src\Presentation\LLMProxy.Admin.API       # Terminal 1
dotnet run --project src\Presentation\LLMProxy.Gateway          # Terminal 2
```

---

## 🏗️ Architecture

Based on **Hexagonal Architecture** (Ports & Adapters) following SOLID, DRY, KISS, and YAGNI principles.

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                      │
│  ┌─────────────────────┐      ┌─────────────────────┐      │
│  │   Gateway (YARP)    │      │     Admin API       │      │
│  └─────────────────────┘      └─────────────────────┘      │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                     Application Layer                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Use Cases  │  │   Services   │  │  DTOs/CQRS   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                       Domain Layer (Core)                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Entities   │  │  Value Obj   │  │ Domain Srv   │      │
│  │   Aggregates │  │  Interfaces  │  │ Exceptions   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                       │
│  ┌─────────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐   │
│  │  PostgreSQL │ │  Redis   │ │ LLM Prov │ │ Security │   │
│  └─────────────┘ └──────────┘ └──────────┘ └──────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## ✨ Features

### Core Capabilities
- **Multi-LLM Support**: OpenAI, Azure OpenAI, Ollama, Anthropic, Mistral, etc.
- **Streaming**: Real-time interception with metrics extraction and content transformation
- **Multi-Tenancy**: Complete isolation per tenant (providers, quotas, configs)
- **Flexible Routing**: Path, header, subdomain, or user-based routing

### Security
- OAuth2/JWT authentication
- API Key management
- Certificate-based authentication
- Multi-environment secret management (Env vars, Azure Key Vault, HashiCorp Vault)

### Resilience (Polly)
- Retry with exponential backoff
- Circuit breaker
- Automatic failover to alternative backends

### Metering & Quotas
- Token counting (SharpToken + response parsing)
- Request-based and token-based quotas
- Configurable limits per user/tenant
- Hybrid storage (Redis for speed, PostgreSQL for persistence)

### Observability
- OpenTelemetry integration
- Comprehensive audit logging
- Request/response logging with anonymization
- Automatic log retention and purge

### Rate Limiting & Throttling (ADR-041)

**Multi-Level Protection:**

| Level | Strategy | Default Limit | Window | Purpose |
|-------|----------|---------------|--------|---------|
| **Global** | Token Bucket | 10,000 req | 1 minute | Infrastructure protection |
| **Per-Tenant** | Fixed Window | 1,000 req | 1 hour | Fair multi-tenant access |
| **Per-User** | Sliding Window | 100 req | 1 minute | Individual abuse prevention |
| **Per-IP** | Fixed Window | 50 req | 1 minute | DDoS protection |
| **Concurrency** | Limiter | 500 connections | - | Server resource protection |

**HTTP Headers:**
- `X-RateLimit-Policy`: Applied policies (e.g., "per-user,per-tenant,global")
- `Retry-After`: Seconds until retry allowed (on 429 responses)

**429 Too Many Requests Response:**
```json
{
  "error": "Too Many Requests",
  "message": "Rate limit exceeded. Please retry after the specified delay.",
  "retryAfterSeconds": 60
}
```

**Configuration:** `appsettings.json`
```json
{
  "RateLimiting": {
    "Global": { "PermitLimit": 10000, "Window": "00:01:00" },
    "PerTenant": { "PermitLimit": 1000, "Window": "01:00:00" },
    "PerUser": { "PermitLimit": 100, "Window": "00:01:00", "SegmentsPerWindow": 6 },
    "PerIp": { "PermitLimit": 50, "Window": "00:01:00" },
    "Concurrency": { "PermitLimit": 500, "QueueLimit": 0 }
  }
}
```

### Advanced Features
- Response caching (configurable per endpoint/user)
- Semantic cache support
- **Rate Limiting** (Fixed Window, Sliding Window, Token Bucket, Concurrency)
- Cost tracking

## 🚀 Tech Stack

- **.NET 9**
- **YARP** (Yet Another Reverse Proxy)
- **PostgreSQL** (primary storage)
- **Redis** (caching & quotas)
- **OpenTelemetry** (observability)
- **Polly** (resilience)
- **SharpToken** (token counting)
- **Entity Framework Core**
- **MediatR** (CQRS)
- **FluentValidation**
- **xUnit** (testing)

## 📁 Project Structure

```
LLMProxy/
├── src/
│   ├── Core/
│   │   └── LLMProxy.Domain/              # Domain entities, value objects, interfaces
│   ├── Application/
│   │   └── LLMProxy.Application/         # Use cases, services, CQRS
│   ├── Infrastructure/
│   │   ├── LLMProxy.Infrastructure.PostgreSQL/
│   │   ├── LLMProxy.Infrastructure.Redis/
│   │   ├── LLMProxy.Infrastructure.Telemetry/
│   │   ├── LLMProxy.Infrastructure.LLMProviders/
│   │   └── LLMProxy.Infrastructure.Security/
│   └── Presentation/
│       ├── LLMProxy.Gateway/             # YARP reverse proxy
│       └── LLMProxy.Admin.API/           # Admin REST API
├── tests/
│   ├── LLMProxy.Domain.Tests/
│   ├── LLMProxy.Application.Tests/
│   └── LLMProxy.Integration.Tests/
├── docker/
├── kubernetes/
└── docs/
```

## 🛠️ Getting Started

### Prerequisites
- .NET 9 SDK
- Docker & Docker Compose
- PostgreSQL 16+
- Redis 7+

### Quick Start

```bash
# Clone repository
git clone <repo-url>
cd LLMProxy

# Start dependencies
docker-compose up -d postgres redis

# Run migrations
dotnet ef database update --project src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL

# Run Gateway
dotnet run --project src/Presentation/LLMProxy.Gateway

# Run Admin API
dotnet run --project src/Presentation/LLMProxy.Admin.API
```

### Running Tests

```bash
# Unit tests
dotnet test tests/LLMProxy.Domain.Tests
dotnet test tests/LLMProxy.Application.Tests

# Integration tests
dotnet test tests/LLMProxy.Integration.Tests
```

## 🐳 Deployment

### Docker
```bash
docker build -t llmproxy-gateway -f docker/Gateway.Dockerfile .
docker build -t llmproxy-admin -f docker/Admin.Dockerfile .
```

### Kubernetes
```bash
kubectl apply -f kubernetes/
```

### Bare Metal / VM
```bash
dotnet publish -c Release -o ./publish
# Deploy publish folder
```

## 📚 Documentation

- [Architecture Decision Records](docs/architecture/)
- [API Documentation](docs/api/)
- [Deployment Guide](docs/deployment/)
- [Configuration Guide](docs/configuration/)

## 🧪 Development Principles

- **SOLID**: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **YAGNI**: You Aren't Gonna Need It
- **KISS**: Keep It Simple, Stupid
- **DRY**: Don't Repeat Yourself
- **TDD**: Test-Driven Development (Red-Green-Refactor)

## 📄 License

[Your License Here]
