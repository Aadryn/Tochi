# 📚 Index de la Documentation LLM Proxy

Bienvenue dans la documentation technique du projet LLM Proxy. Ce document centralise les liens vers toute la documentation disponible.

## 🚀 Démarrage

| Document | Description |
|----------|-------------|
| [README.md](../README.md) | Vue d'ensemble du projet et démarrage rapide |
| [.env.example](../.env.example) | Variables d'environnement à configurer |

## 🏗️ Architecture

| Document | Description |
|----------|-------------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | Architecture hexagonale, structure des couches |
| [DATABASE.md](DATABASE.md) | Schéma de base de données et migrations |
| [FEATURE_FLAGS.md](FEATURE_FLAGS.md) | Configuration des feature toggles |

## 📐 Architecture Decision Records (ADRs)

Le projet contient **62 ADRs** documentant toutes les décisions architecturales majeures.

### Principes Fondamentaux (001-012)

| ADR | Titre |
|-----|-------|
| [ADR-001](adr/001-un-seul-type-par-fichier-csharp.adr.md) | Un seul type par fichier C# |
| [ADR-002](adr/002-principe-kiss.adr.md) | Principe KISS |
| [ADR-003](adr/003-principe-dry.adr.md) | Principe DRY |
| [ADR-004](adr/004-principe-yagni.adr.md) | Principe YAGNI |
| [ADR-005](adr/005-principes-solid.adr.md) | Principes SOLID |
| [ADR-006](adr/006-onion-architecture.adr.md) | Onion Architecture |
| [ADR-007](adr/007-vertical-slice-architecture.adr.md) | Vertical Slice Architecture |
| [ADR-008](adr/008-hexagonal-architecture.adr.md) | Hexagonal Architecture |
| [ADR-009](adr/009-principe-fail-fast.adr.md) | Principe Fail-Fast |
| [ADR-010](adr/010-separation-of-concerns.adr.md) | Separation of Concerns |
| [ADR-011](adr/011-composition-over-inheritance.adr.md) | Composition over Inheritance |
| [ADR-012](adr/012-law-of-demeter.adr.md) | Law of Demeter |

### Patterns & Pratiques (013-030)

| ADR | Titre |
|-----|-------|
| [ADR-013](adr/013-cqrs.adr.md) | CQRS (Command Query Responsibility Segregation) |
| [ADR-014](adr/014-dependency-injection.adr.md) | Dependency Injection |
| [ADR-015](adr/015-immutability.adr.md) | Immutability |
| [ADR-016](adr/016-explicit-over-implicit.adr.md) | Explicit over Implicit |
| [ADR-017](adr/017-repository-pattern.adr.md) | Repository Pattern |
| [ADR-018](adr/018-unit-of-work.adr.md) | Unit of Work |
| [ADR-019](adr/019-factory-pattern.adr.md) | Factory Pattern |
| [ADR-020](adr/020-strategy-pattern.adr.md) | Strategy Pattern |
| [ADR-021](adr/021-decorator-pattern.adr.md) | Decorator Pattern |
| [ADR-022](adr/022-idempotence.adr.md) | Idempotence |
| [ADR-023](adr/023-result-pattern.adr.md) | Result Pattern |
| [ADR-024](adr/024-value-objects.adr.md) | Value Objects |
| [ADR-025](adr/025-domain-events.adr.md) | Domain Events |
| [ADR-026](adr/026-null-object-pattern.adr.md) | Null Object Pattern |
| [ADR-027](adr/027-debug-assertions.adr.md) | Debug Assertions |
| [ADR-028](adr/028-specification-pattern.adr.md) | Specification Pattern |
| [ADR-029](adr/029-options-pattern.adr.md) | Options Pattern |
| [ADR-030](adr/030-feature-toggles.adr.md) | Feature Toggles |

### Logging & Observabilité (031-035)

| ADR | Titre |
|-----|-------|
| [ADR-031](adr/031-structured-logging.adr.md) | Structured Logging |
| [ADR-032](adr/032-circuit-breaker.adr.md) | Circuit Breaker |
| [ADR-033](adr/033-retry-pattern.adr.md) | Retry Pattern |
| [ADR-034](adr/034-timeout-pattern.adr.md) | Timeout Pattern |
| [ADR-035](adr/035-bulkhead-pattern.adr.md) | Bulkhead Pattern |

### API & Infrastructure (036-045)

| ADR | Titre |
|-----|-------|
| [ADR-036](adr/036-cache-aside-pattern.adr.md) | Cache-Aside Pattern |
| [ADR-037](adr/037-api-versioning.adr.md) | API Versioning |
| [ADR-038](adr/038-health-checks.adr.md) | Health Checks |
| [ADR-039](adr/039-configuration-management.adr.md) | Configuration Management |
| [ADR-040](adr/040-outbox-pattern.adr.md) | Outbox Pattern |
| [ADR-041](adr/041-rate-limiting.adr.md) | Rate Limiting |
| [ADR-042](adr/042-input-validation.adr.md) | Input Validation |
| [ADR-043](adr/043-exception-handling.adr.md) | Exception Handling |
| [ADR-044](adr/044-async-await.adr.md) | Async/Await Best Practices |
| [ADR-045](adr/045-resource-disposal.adr.md) | Resource Disposal |

### Infrastructure & Déploiement (046-060)

| ADR | Titre |
|-----|-------|
| [ADR-046](adr/046-database-connection-pooling.adr.md) | Database Connection Pooling |
| [ADR-047](adr/047-secret-management.adr.md) | Secret Management |
| [ADR-048](adr/048-graceful-shutdown.adr.md) | Graceful Shutdown |
| [ADR-049](adr/049-database-migrations-strategy.adr.md) | Database Migrations Strategy |
| [ADR-050](adr/050-api-documentation-openapi.adr.md) | API Documentation (OpenAPI) |
| [ADR-051](adr/051-testing-strategy.adr.md) | Testing Strategy |
| [ADR-052](adr/052-security-headers.adr.md) | Security Headers |
| [ADR-053](adr/053-cors-policy.adr.md) | CORS Policy |
| [ADR-054](adr/054-request-response-logging.adr.md) | Request/Response Logging |
| [ADR-055](adr/055-separation-abstractions-implementations.adr.md) | Separation Abstractions/Implementations |
| [ADR-056](adr/056-admin-ui-react-fluent-vite.adr.md) | Admin UI (React + Fluent) |
| [ADR-057](adr/057-multi-provider-llm-abstraction.adr.md) | Multi-Provider LLM Abstraction |
| [ADR-058](adr/058-caching-via-mediatr-pipeline-behaviors.adr.md) | Caching via MediatR Pipeline |
| [ADR-059](adr/059-frontend-internationalization-strategy.adr.md) | Frontend Internationalization |
| [ADR-060](adr/060-authorization-azure-rbac-style.adr.md) | Authorization (Azure RBAC Style) |

## 📊 Rapports d'Analyse

| Document | Description |
|----------|-------------|
| [ANALYSE_CONFORMITE_ADR.md](analysis/ANALYSE_CONFORMITE_ADR.md) | Analyse globale de conformité aux ADRs |
| [SYNTHESE_GLOBALE_CONFORMITE_ADR.md](analysis/SYNTHESE_GLOBALE_CONFORMITE_ADR.md) | Synthèse de conformité |
| [RAPPORT_VIOLATIONS_ADR_CRITIQUES.md](analysis/RAPPORT_VIOLATIONS_ADR_CRITIQUES.md) | Violations critiques identifiées |

### Analyses Détaillées par ADR

- [ANALYSE_CONFORMITE_ADR-002.md](analysis/ANALYSE_CONFORMITE_ADR-002.md) - Conformité ADR-002 (KISS)
- [ANALYSE_CONFORMITE_ADR-003.md](analysis/ANALYSE_CONFORMITE_ADR-003.md) - Conformité ADR-003 (DRY)
- [ANALYSE_CONFORMITE_ADR-004.md](analysis/ANALYSE_CONFORMITE_ADR-004.md) - Conformité ADR-004 (YAGNI)
- [ANALYSE_CONFORMITE_ADR-005.md](analysis/ANALYSE_CONFORMITE_ADR-005.md) - Conformité ADR-005 (SOLID)
- [ANALYSE_CONFORMITE_ADR-006-012.md](analysis/ANALYSE_CONFORMITE_ADR-006-012.md) - Conformité ADR 006-012
- [ANALYSE_CONFORMITE_ADR-013-030.md](analysis/ANALYSE_CONFORMITE_ADR-013-030.md) - Conformité ADR 013-030
- [ANALYSE_CONFORMITE_ADR-031-041.md](analysis/ANALYSE_CONFORMITE_ADR-031-041.md) - Conformité ADR 031-041

## 🛠️ Développement

| Document | Description |
|----------|-------------|
| [NEXT_STEPS.md](NEXT_STEPS.md) | Prochaines étapes de développement |
| [.github/instructions/](../.github/instructions/) | Instructions Copilot (70+ fichiers) |
| [.tasks/](../.tasks/) | Système de gestion des tâches |

## 🔗 Liens Utiles

### Backend

- [Solution .NET](../applications/proxy/backend/LLMProxy.sln)
- [Domain README](../applications/proxy/backend/src/Core/LLMProxy.Domain/README.md)
- [Application README](../applications/proxy/backend/src/Application/LLMProxy.Application/README.md)

### Frontend

- [Frontend README](../applications/proxy/frontend/README.md)

### Infrastructure

- [Docker Compose](../.environments/docker-compose.yml)
- [Kubernetes Manifests](../k8s/)

---

*Dernière mise à jour : 2025-01-27*
