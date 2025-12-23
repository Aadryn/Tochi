# LLM Proxy

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](.)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![Vue.js 3](https://img.shields.io/badge/Vue.js-3.x-4FC08D)](https://vuejs.org/)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

> Un proxy intelligent pour les API de modèles de langage (LLM) avec multi-tenant, multi-provider et gestion avancée des quotas.

## 📋 Fonctionnalités

- **Multi-Provider** : OpenAI, Anthropic, Azure OpenAI, Ollama, Cohere, Mistral
- **Multi-Format API** : Support des formats OpenAI et Ollama côté client
- **Multi-Tenant** : Isolation complète par tenant avec configuration indépendante
- **Gestion des Quotas** : Rate limiting par utilisateur/tenant avec Redis
- **Streaming** : Support complet du streaming SSE
- **Observabilité** : OpenTelemetry, métriques Prometheus, traces distribuées
- **Sécurité** : Authentification API Key, intégration Keycloak, JWT
- **Administration** : API REST + Interface d'administration Vue.js

## 🏗️ Architecture

```
                            ┌─────────────────────────────┐
                            │         Clients             │
                            │  (OpenAI SDK / Ollama CLI)  │
                            └─────────────┬───────────────┘
                                          │
                            ┌─────────────▼───────────────┐
                            │     LLM Proxy Gateway       │
                            │   (YARP Reverse Proxy)      │
                            │  ┌─────────────────────┐    │
                            │  │ API Format Detection│    │
                            │  │ Rate Limiting       │    │
                            │  │ Authentication      │    │
                            │  │ Load Balancing      │    │
                            │  └─────────────────────┘    │
                            └─────────────┬───────────────┘
                                          │
                   ┌──────────────────────┼──────────────────────┐
                   │                      │                      │
        ┌──────────▼──────────┐ ┌────────▼────────┐ ┌──────────▼──────────┐
        │      OpenAI         │ │    Anthropic    │ │       Ollama        │
        │    (gpt-4o)         │ │   (claude-3)    │ │    (llama3.1)       │
        └─────────────────────┘ └─────────────────┘ └─────────────────────┘
```

Le projet suit une **Architecture Hexagonale** (Ports & Adapters) avec CQRS et les principes SOLID.

Pour plus de détails, voir [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## 🚀 Démarrage Rapide

### Prérequis

- Docker & Docker Compose
- .NET 9 SDK
- Node.js 20+
- PowerShell 7+ (Windows) ou Bash (Linux/macOS)

### Installation

```bash
# 1. Cloner le repository
git clone https://github.com/your-org/llm-proxy.git
cd llm-proxy

# 2. Démarrer l'environnement de développement
docker compose -f .environments/docker-compose.yml up -d

# 3. Démarrer le backend
cd applications/proxy/backend
dotnet restore
dotnet build
dotnet run --project src/Presentation/LLMProxy.Gateway

# 4. Démarrer le frontend (dans un autre terminal)
cd applications/proxy/frontend
npm install
npm run dev
```

### Accès aux Services

| Service | URL | Description |
|---------|-----|-------------|
| Gateway API | http://localhost:5000 | API proxy LLM |
| Admin API | http://localhost:5001 | API d'administration |
| Admin UI | http://localhost:3000 | Interface d'administration |
| Keycloak | http://localhost:8080 | Identity Provider |
| Grafana | http://localhost:3001 | Dashboards de monitoring |
| Jaeger | http://localhost:16686 | Traces distribuées |

## 📁 Structure du Projet

```
llm-proxy/
├── .environments/          # Configuration Docker (postgres, redis, keycloak...)
├── applications/
│   ├── proxy/              # Application principale
│   │   ├── backend/        # Solution .NET 9
│   │   │   ├── src/
│   │   │   │   ├── Core/           # Domain Layer
│   │   │   │   ├── Application/    # Application Layer
│   │   │   │   ├── Infrastructure/ # Infrastructure Layer
│   │   │   │   └── Presentation/   # Gateway & Admin API
│   │   │   └── tests/              # Tests unitaires
│   │   └── frontend/       # Vue.js 3 + PrimeVue
│   └── authorization/      # Service d'autorisation (Azure RBAC style)
├── docs/
│   ├── adr/                # Architecture Decision Records (59+ ADRs)
│   └── ...                 # Documentation technique
├── k8s/                    # Manifestes Kubernetes
└── scripts/                # Scripts d'automatisation
```

## 🔧 Configuration

### Variables d'Environnement

Créez un fichier `.env` à la racine :

```env
# Base de données
POSTGRES_HOST=localhost
POSTGRES_PORT=15432
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your-password
POSTGRES_DB=development

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379

# Keycloak
KEYCLOAK_URL=http://localhost:8080
KEYCLOAK_REALM=development
KEYCLOAK_CLIENT_ID=llm-proxy
```

Voir [.env.example](.env.example) pour un exemple complet.

## 🧪 Tests

```bash
# Backend - Tests unitaires
cd applications/proxy/backend
dotnet test

# Frontend - Tests unitaires
cd applications/proxy/frontend
npm run test:unit

# Frontend - Tests E2E
npm run test:e2e
```

## 📖 Documentation

- [Architecture](docs/ARCHITECTURE.md) - Vue d'ensemble de l'architecture
- [Database](docs/DATABASE.md) - Schéma et migrations
- [ADRs](docs/adr/) - Architecture Decision Records (59+ décisions documentées)
- [Feature Flags](docs/FEATURE_FLAGS.md) - Configuration des feature toggles

## 🔐 Sécurité

- Authentification par API Key avec hachage SHA-256
- Support JWT via Keycloak
- Rate limiting configurable par utilisateur/tenant
- Audit logging complet avec JSONB metadata
- Isolation multi-tenant stricte

## 📊 Observabilité

- **Métriques** : Prometheus + Grafana dashboards
- **Traces** : OpenTelemetry → Jaeger
- **Logs** : Structured logging avec Serilog

## 🤝 Contribution

1. Fork le repository
2. Créer une feature branch (`git checkout -b feature/amazing-feature`)
3. Commit avec messages conventionnels (`git commit -m 'feat(scope): add amazing feature'`)
4. Push sur la branche (`git push origin feature/amazing-feature`)
5. Ouvrir une Pull Request

## 📜 Licence

Ce projet est sous licence MIT - voir le fichier [LICENSE](LICENSE) pour plus de détails.

---

*Développé avec ❤️ pour simplifier l'intégration des LLM en entreprise.*
