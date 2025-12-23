# ADR-055. Intégration OpenFGA pour Autorisation ReBAC Hiérarchique

Date: 2025-12-22
Statut: **Proposé**

## Contexte

Le système LLMProxy nécessite un contrôle d'accès fin sur toutes les ressources :
- **Multi-tenant** : Isolation stricte entre tenants
- **Hiérarchique** : Organization → Tenant → Resources
- **Granulaire** : Permissions différenciées (view, operate, manage)
- **Relationnel** : Droits basés sur les relations entre entités

Les approches traditionnelles (RBAC) ne permettent pas cette flexibilité.

## Décision

Adopter **OpenFGA** comme moteur d'autorisation avec un modèle **ReBAC (Relationship-Based Access Control)** hiérarchique.

### Modèle d'Autorisation

```fga
model
  schema 1.1

type user

type organization
  relations
    define admin: [user]
    define member: [user] or admin

type tenant
  relations
    define organization: [organization]
    define owner: [user]
    define admin: [user] or owner or admin from organization
    define operator: [user] or admin
    define viewer: [user] or operator
    define can_manage: admin
    define can_operate: operator
    define can_view: viewer
```

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    LLMProxy Gateway                          │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐    ┌─────────────────────────────────┐ │
│  │ JWT Token       │───▶│ AuthorizationMiddleware         │ │
│  │ (Identity)      │    │ - Extract user/tenant from JWT  │ │
│  └─────────────────┘    │ - Call OpenFGA Check API        │ │
│                         │ - Cache decisions (Redis)        │ │
│                         └─────────────────────────────────┘ │
│                                       │                      │
│                                       ▼                      │
│                         ┌─────────────────────────────────┐ │
│                         │ OpenFGA Server                   │ │
│                         │ - Store: PostgreSQL              │ │
│                         │ - Model: ReBAC hiérarchique      │ │
│                         └─────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Ressources Protégées

| Type | Relations | Héritage |
|------|-----------|----------|
| `organization` | admin, member | - |
| `tenant` | owner, admin, operator, viewer | organization.admin |
| `provider` | can_use, can_configure, can_delete | tenant.* |
| `api_key` | owner, can_use, can_revoke | tenant.* |
| `route` | can_view, can_modify, can_delete | tenant.* |
| `config` | can_view, can_modify | tenant.* |
| `stats` | can_view, can_export | tenant.* |
| `audit_log` | can_view | tenant.admin |

## Conséquences

### Positives
- Autorisation ultra-fine basée sur les relations réelles
- Héritage automatique (organization → tenant → resources)
- Scalabilité horizontale (OpenFGA stateless)
- Standard CNCF, interopérable
- Playground pour tester les règles

### Négatives
- Composant supplémentaire à déployer (OpenFGA server)
- Latence additionnelle (1-5ms par check, cacheable)
- Courbe d'apprentissage du langage FGA
- Synchronisation tuples à maintenir

## Alternatives Considérées

### Alternative 1 : RBAC Classique (ASP.NET Core Policies)
- Avantages : Simple, natif, pas de dépendance externe
- Inconvénients : Pas de relations, pas de hiérarchie, explosion des rôles
- Raison du rejet : Complexité multi-tenant ingérable

### Alternative 2 : Casbin
- Avantages : Polyvalent (RBAC, ABAC, ReBAC), Go/C# SDK
- Inconvénients : Moins mature en ReBAC, moins de tooling
- Raison du rejet : OpenFGA plus adapté au modèle ReBAC pur

### Alternative 3 : OPA (Open Policy Agent)
- Avantages : Très flexible, Rego puissant
- Inconvénients : Complexité Rego, pas natif ReBAC
- Raison du rejet : Surcoût d'apprentissage pour ReBAC

## Références

- [OpenFGA Documentation](https://openfga.dev/docs)
- [Zanzibar: Google's Consistent, Global Authorization System](https://research.google/pubs/pub48190/)
- [OpenFGA .NET SDK](https://github.com/openfga/dotnet-sdk)
