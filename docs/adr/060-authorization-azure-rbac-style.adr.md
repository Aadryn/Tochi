# ADR-060 : Authorization Service - Encapsulation OpenFGA

**Date** : 2025-01-08
**Statut** : Accepté
**Décideurs** : Équipe Architecture
**Dernière mise à jour** : 2025-12-23

## Contexte

Le système LLMProxy nécessite un système d'autorisation granulaire style Azure RBAC. Nous avons choisi **OpenFGA** comme solution d'autorisation.

L'application Authorization est une **encapsulation d'OpenFGA** avec une API .NET servant de couche d'intermédiation.

## Décision

### Architecture : Séparation des Responsabilités

```
┌─────────────────────────────────────────────────────────────────┐
│                         IDP Externe                              │
│              (Azure AD, Okta, Keycloak, etc.)                   │
│                                                                  │
│   Source de vérité pour : Users, Groups, ServiceAccounts        │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ JWT (ObjectId, claims)
                              │ + Sync (JIT, Batch, Webhook)
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Authorization Service                         │
│                   (Encapsulation OpenFGA)                        │
│                                                                  │
│   ┌──────────────────────────────────────────────────────────┐  │
│   │                    API .NET                               │  │
│   │              (Couche d'Intermédiation)                    │  │
│   │                                                           │  │
│   │  • Validation des données avant écriture                 │  │
│   │  • Vérification des doublons                             │  │
│   │  • Contraintes métier (délégation hiérarchique)          │  │
│   │  • Cohérence des tuples                                  │  │
│   │  • Audit (logs + PostgreSQL)                             │  │
│   └──────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│   ┌──────────────────────────────────────────────────────────┐  │
│   │                     OpenFGA                               │  │
│   │           (Source de Vérité Autorisations)                │  │
│   │           (1 Store par Tenant - Multi-tenant)             │  │
│   │                                                           │  │
│   │  • Role assignments (avec expiration optionnelle)        │  │
│   │  • Custom roles                                          │  │
│   │  • Group memberships (copie depuis IDP)                  │  │
│   │  • Scope hierarchy (explicite)                           │  │
│   │  • Permissions granulaires (resource:action)             │  │
│   └──────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│   ┌──────────────────────────────────────────────────────────┐  │
│   │                      Redis                                │  │
│   │                (Cache Permissions)                        │  │
│   └──────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│   ┌──────────────────────────────────────────────────────────┐  │
│   │                   PostgreSQL                              │  │
│   │              (Audit Table uniquement)                     │  │
│   └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Séparation des Sources de Vérité

| Donnée | Source de Vérité | Stockage OpenFGA |
|--------|------------------|------------------|
| Users (identité) | **IDP** | Non (référence par ObjectId) |
| Groups (identité) | **IDP** | Non (référence par ObjectId) |
| ServiceAccounts | **IDP** | Non (référence par ObjectId) |
| Group memberships | **IDP** | Oui (copie synchronisée) |
| Role assignments | **OpenFGA** | Oui |
| Custom roles | **OpenFGA** | Oui |
| Scope hierarchy | **OpenFGA** | Oui |
| Audit trail | **PostgreSQL** | Non |

---

## Décisions de Cadrage (2025-12-23)

### 1. Gestion des Rôles

**Décision** : Rôles de base fixes + Rôles custom via API

| Type | Rôles | Gestion |
|------|-------|---------|
| **Base (fixes)** | `owner`, `contributor`, `reader` | Code (non modifiables) |
| **Custom** | `data-scientist`, `auditor`, etc. | API (CRUD par admin) |

Les rôles custom héritent des permissions des rôles de base ou définissent leurs propres permissions.

### 2. Synchronisation des Groupes IDP

**Décision** : JIT + Batch + Webhook (les 3 stratégies)

| Stratégie | Déclencheur | Latence |
|-----------|-------------|---------|
| **JIT** | Premier accès utilisateur | Temps réel |
| **Batch** | Cron job (toutes les 15 min) | 0-15 min |
| **Webhook** | Événement IDP (création/modification groupe) | Temps réel |

La révocation est **immédiate** : retrait du groupe dans l'IDP = perte des permissions héritées.

### 3. Multi-Rôles par Scope

**Décision** : Autorisé

Un utilisateur PEUT avoir plusieurs rôles sur le même scope :
```
user:alice → reader sur scope:org-123
user:alice → contributor sur scope:org-123  ← AUTORISÉ
```

Les permissions sont cumulatives (union des permissions des rôles).

### 4. Expiration des Assignations

**Décision** : Optionnelle

```csharp
public sealed record AssignRoleRequest(
    PrincipalId PrincipalId,
    string Role,
    Scope Scope,
    DateTimeOffset? ExpiresAt = null  // Optionnel
);
```

Un **Cleanup Job** (background service) purge les assignations expirées toutes les 5 minutes.

### 5. Audit

**Décision** : Logs applicatifs + Table PostgreSQL dédiée

| Cible | Données |
|-------|---------|
| **Logs (Serilog)** | Toutes opérations (Check, Write, Delete) |
| **PostgreSQL** | Opérations d'écriture uniquement (audit trail formel) |

```sql
CREATE TABLE authorization_audit (
    id BIGSERIAL PRIMARY KEY,
    timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    operation VARCHAR(50) NOT NULL,  -- 'ASSIGN', 'REVOKE', 'CREATE_SCOPE', etc.
    principal_id UUID NOT NULL,       -- Qui a fait l'action
    target_principal_id UUID,         -- Sur qui
    role VARCHAR(100),
    scope TEXT,
    details JSONB,
    correlation_id UUID
);
```

### 6. Gestion des Scopes

**Décision** : Scopes explicites (doivent être créés avant assignation)

```
POST /scopes
{
  "path": "api.llmproxy.com/organizations/org-123/tenants/tenant-456"
}
```

L'API :
1. Valide le format URL REST
2. Vérifie que le parent existe (ou le crée automatiquement)
3. Crée le scope dans OpenFGA
4. Permet ensuite les assignations sur ce scope

### 7. Multi-Tenancy

**Décision** : Multi-tenant avec isolation par Store OpenFGA

| Tenant | Store OpenFGA |
|--------|---------------|
| `tenant-abc` | `store-abc` |
| `tenant-xyz` | `store-xyz` |

Chaque tenant a son propre store, garantissant une isolation complète des données d'autorisation.

### 8. Permissions Granulaires

**Décision** : Oui, format `resource:action`

```
prompts:create
prompts:read
prompts:update
prompts:delete
models:read
models:configure
routes:read
routes:create
statistics:read
```

Les rôles agrègent des permissions :
```
reader → prompts:read, models:read, routes:read, statistics:read
contributor → reader + prompts:create, prompts:update
owner → contributor + prompts:delete, models:configure, routes:create
```

### 9. Délégation d'Administration

**Décision** : Hiérarchique (Owner délègue uniquement rôles ≤ sien)

```csharp
public async Task ValidateDelegationAsync(
    PrincipalId delegator,
    string roleToAssign,
    Scope scope)
{
    var delegatorRole = await GetHighestRoleAsync(delegator, scope);
    
    if (GetRoleLevel(roleToAssign) > GetRoleLevel(delegatorRole))
        throw new ForbiddenException(
            $"Cannot assign role '{roleToAssign}' (level {GetRoleLevel(roleToAssign)}) " +
            $"when your highest role is '{delegatorRole}' (level {GetRoleLevel(delegatorRole)})");
}

// Niveaux de rôles
// owner = 3, contributor = 2, reader = 1
```

### 10. Révocation et Propagation

**Décision** : Immédiate via OpenFGA

- Retrait d'un groupe dans l'IDP → Mise à jour immédiate des tuples OpenFGA
- Pas de grace period
- Les permissions héritées disparaissent automatiquement

---

## Principes Fondamentaux

### PRINCIPE 1 : IDP = Maître des Identités

L'IDP externe (Azure AD, Okta, Keycloak) est la **source de vérité** pour :
- Users (email, nom, attributs)
- Groups (nom, membres)
- ServiceAccounts (credentials)

L'application Authorization :
- ❌ Ne crée PAS d'utilisateurs
- ❌ Ne stocke PAS les attributs utilisateur
- ✅ Référence les principals par leur ObjectId (GUID)
- ✅ Synchronise les group memberships depuis l'IDP (JIT + Batch + Webhook)

### PRINCIPE 2 : OpenFGA = Maître des Autorisations

OpenFGA stocke tout ce qui concerne les autorisations :
- Role assignments (qui a quel rôle sur quel scope, avec expiration optionnelle)
- Custom roles (définitions de rôles personnalisés)
- Group memberships (copie pour les checks OpenFGA)
- Scope hierarchy (relations parent/enfant, explicites)
- Permissions granulaires (resource:action)

### PRINCIPE 3 : API .NET = Couche d'Intermédiation

L'API .NET n'est PAS un simple proxy. Elle ajoute :

```csharp
// Vérifications avant écriture
public async Task<RoleAssignmentDto> AssignAsync(AssignRoleRequest request)
{
    // 1. Valider que le principal existe (via JWT ou cache)
    await ValidatePrincipalExistsAsync(request.PrincipalId);
    
    // 2. Vérifier qu'un assignment identique n'existe pas déjà
    var exists = await CheckDuplicateAssignmentAsync(request);
    if (exists)
        throw new ConflictException("Role assignment already exists");
    
    // 3. Valider le scope (format, existence dans hiérarchie)
    await ValidateScopeAsync(request.Scope);
    
    // 4. Valider le rôle (base ou custom existant)
    await ValidateRoleAsync(request.Role);
    
    // 5. Valider la délégation (caller peut-il assigner ce rôle ?)
    await ValidateDelegationAsync(callerId, request.Role, request.Scope);
    
    // 6. Écrire dans OpenFGA
    await _openFga.WriteTuplesAsync([...]);
    
    // 7. Audit (PostgreSQL + logs)
    await _audit.LogAssignmentAsync(callerId, request);
}
```

| Responsabilité | Description |
|----------------|-------------|
| Validation données | Format ObjectId, scope URL REST valide |
| Anti-doublons | Vérifier avant Write qu'un tuple identique n'existe pas |
| Contraintes métier | Délégation hiérarchique (owner → contributor → reader) |
| Cohérence | Créer les tuples `parent` lors de la création de scopes |
| Audit | Logger les opérations d'écriture (logs + PostgreSQL) |

### PRINCIPE 4 : Scopes au Format URL REST

```
api.llmproxy.com
api.llmproxy.com/organizations/org-123
api.llmproxy.com/organizations/org-123/tenants/tenant-456
```

Dans OpenFGA :
```
scope:api.llmproxy.com
scope:api.llmproxy.com/organizations/org-123
scope:api.llmproxy.com/organizations/org-123/tenants/tenant-456
```

### PRINCIPE 5 : ObjectId (GUID) pour les Principals

Tous les principals utilisent l'**ObjectId (GUID)** de l'IDP :
```
user:550e8400-e29b-41d4-a716-446655440000
group:661e9500-f30c-52e5-b827-557766551111
serviceaccount:772fa611-g41d-63f6-c938-668877662222
```

---

## API Endpoints

| API Endpoint | OpenFGA Operation | Vérifications API .NET |
|--------------|-------------------|------------------------|
| `POST /check` | `Check` | Validation format |
| `POST /assignments` | `WriteTuples` | Anti-doublon, validation scope/rôle, délégation |
| `DELETE /assignments` | `DeleteTuples` | Vérification existence, délégation |
| `GET /assignments` | `Read` | Filtrage, pagination |
| `POST /groups/{id}/members` | `WriteTuples` | Vérification groupe IDP |
| `POST /scopes` | `WriteTuples` (parent) | Validation hiérarchie |
| `POST /roles` | `WriteTuples` | Création rôle custom |
| `GET /roles` | `Read` | Liste rôles (base + custom) |
| `GET /permissions` | Calcul | Liste permissions resource:action |
| `GET /audit` | PostgreSQL | Historique audit trail |

---

## Modèle OpenFGA

```fga
model
  schema 1.1

type user

type group
  relations
    define member: [user, group#member]

type serviceaccount

type role
  relations
    define assignee: [user, group#member, serviceaccount]
    define parent_role: [role]
    define has_permission: assignee or has_permission from parent_role

type permission
  relations
    define granted_by: [role#has_permission]

type scope
  relations
    define parent: [scope]
    
    # Rôles de base
    define owner: [user, group#member, serviceaccount] or owner from parent
    define contributor: [user, group#member, serviceaccount] or contributor from parent or owner
    define reader: [user, group#member, serviceaccount] or reader from parent or contributor
    
    # Rôles custom
    define custom_role: [role#assignee]
    
    # Permissions (actions)
    define can_read: reader or custom_role
    define can_write: contributor or custom_role
    define can_delete: owner
    define can_manage: owner
    define can_delegate: owner
```

---

## Exemples de Tuples OpenFGA

### Hiérarchie des scopes

```json
{ "user": "scope:api.llmproxy.com/organizations/org-123", "relation": "parent", "object": "scope:api.llmproxy.com" }
{ "user": "scope:api.llmproxy.com/organizations/org-123/tenants/tenant-456", "relation": "parent", "object": "scope:api.llmproxy.com/organizations/org-123" }
```

### Group membership

```json
{ "user": "user:550e8400-e29b-41d4-a716-446655440000", "relation": "member", "object": "group:admin-group-id" }
```

### Role assignment (via groupe)

```json
{ "user": "group:admin-group-id#member", "relation": "owner", "object": "scope:api.llmproxy.com" }
```

### Role assignment (direct)

```json
{ "user": "user:772fa611-g41d-63f6-c938-668877662222", "relation": "contributor", "object": "scope:api.llmproxy.com/organizations/org-123" }
```

---

## Flux de Vérification

```json
// Request API
POST /api/v1/check
{
  "principalId": "550e8400-e29b-41d4-a716-446655440000",
  "permission": "can_write",
  "scope": "api.llmproxy.com/organizations/org-123/tenants/tenant-456"
}

// OpenFGA Check (interne)
{
  "user": "user:550e8400-e29b-41d4-a716-446655440000",
  "relation": "can_write",
  "object": "scope:api.llmproxy.com/organizations/org-123/tenants/tenant-456"
}

// Response
{
  "allowed": true
}
```

---

## Structure du Projet

```
authorization/
├── src/
│   ├── Authorization.Domain/           # Value Objects, DTOs
│   ├── Authorization.Application/      # Services (façade OpenFGA)
│   └── Authorization.API/              # Controllers REST
├── tests/
│   ├── Authorization.Application.Tests/
│   └── Authorization.API.Tests/
├── infrastructure/
│   └── openfga/
│       ├── authorization-model.fga     # Modèle d'autorisation
│       └── seed-data.json              # Données initiales
├── migrations/
│   └── *.json                          # Migrations de tuples
└── jobs/
    └── ExpirationCleanupJob.cs         # Cleanup assignations expirées
```

---

## Conséquences

### Positives

1. **Simplicité** : OpenFGA = source unique pour autorisations
2. **Performance** : Check < 10ms, optimisé pour l'autorisation
3. **Cohérence** : Pas de désynchronisation entre 2 sources
4. **Scalabilité** : OpenFGA scale horizontalement
5. **Multi-tenancy** : Isolation par store OpenFGA
6. **Flexibilité** : Rôles custom + permissions granulaires
7. **Audit complet** : Logs + PostgreSQL

### Négatives

1. **Dépendance** : Forte dépendance à OpenFGA
2. **Requêtes complexes** : Pas de SQL pour reporting (sauf audit)
3. **Debugging** : Tuples moins lisibles que des tables relationnelles
4. **Complexité** : 3 stratégies de sync IDP à maintenir

### Mitigations

- **Dépendance** : OpenFGA est open-source, peut être self-hosted
- **Reporting** : Table audit PostgreSQL pour les besoins de reporting
- **Debugging** : OpenFGA Playground pour visualiser les tuples
- **Sync** : Prioriser JIT, Batch en fallback, Webhook optionnel

---

## Estimation Révisée

| Composant | Effort | Notes |
|-----------|--------|-------|
| Domain Layer (DTOs, Value Objects) | 4h | Inchangé |
| Infrastructure OpenFGA + Redis | 10h | +2h pour multi-tenant |
| Infrastructure PostgreSQL (Audit) | 4h | Nouveau |
| Application Layer | 14h | +6h (custom roles, expiration, délégation) |
| API Layer (Controllers) | 8h | +2h (endpoints custom roles, permissions) |
| Sync IDP (JIT + Batch + Webhook) | 12h | Nouveau (3 stratégies) |
| Cleanup Job (expiration) | 4h | Nouveau |
| Tests | 10h | +4h (couverture étendue) |
| Backend SDK (Client) | 4h | Inchangé |
| **TOTAL** | **70h** | +36h vs estimation initiale |

---

## Références

- [OpenFGA Documentation](https://openfga.dev/docs)
- [OpenFGA .NET SDK](https://github.com/openfga/dotnet-sdk)
- [Azure RBAC](https://docs.microsoft.com/azure/role-based-access-control/)
- [Google Zanzibar](https://research.google/pubs/pub48190/)
