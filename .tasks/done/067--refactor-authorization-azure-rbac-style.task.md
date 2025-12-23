# Tâche 067 - Refondre le Système d'Autorisation Style Azure RBAC

## PRIORITÉ
🔴 **P1 - CRITIQUE** (Fondation sécurité)

## OBJECTIF

Refondre le système d'autorisation OpenFGA pour adopter un modèle inspiré d'Azure RBAC avec :
1. **Scope** : Hiérarchie de ressources avec héritage automatique
2. **Permission** : Actions granulaires sur les ressources
3. **Principal** : Utilisateurs, Groupes, Service Accounts

Extraire le composant d'autorisation dans une **API autonome** (Authorization.API).

## CONTEXTE

### Modèle Azure RBAC (Référence)

```
┌─────────────────────────────────────────────────────────────────┐
│                    Azure RBAC Model                              │
├─────────────────────────────────────────────────────────────────┤
│  Role Assignment = Principal + Role Definition + Scope          │
│                                                                  │
│  Principal Types:                                                │
│  ├── User (utilisateur individuel)                              │
│  ├── Group (groupe d'utilisateurs)                              │
│  ├── Service Principal (application/service)                    │
│  └── Managed Identity (identité gérée)                          │
│                                                                  │
│  Scope Hierarchy (héritage descendant):                         │
│  ├── Management Group                                           │
│  │   └── Subscription                                           │
│  │       └── Resource Group                                     │
│  │           └── Resource                                       │
│                                                                  │
│  Role Definition:                                                │
│  ├── Name (ex: "Reader", "Contributor", "Owner")                │
│  ├── Permissions[] (Actions + NotActions + DataActions)         │
│  └── AssignableScopes[]                                         │
└─────────────────────────────────────────────────────────────────┘
```

### Mapping vers LLMProxy

```
Azure Concept          → LLMProxy Concept
─────────────────────────────────────────
Management Group       → Platform (niveau global)
Subscription           → Organization
Resource Group         → Tenant
Resource               → Provider, Route, ApiKey, Config, etc.

Principal Types        → LLMProxy Implementation (ObjectId = GUID)
───────────────────────────────────────────────────────────────────
User                   → ObjectId: 550e8400-e29b-41d4-a716-446655440000
                         ExternalId: john@example.com (pour sync IDP)
Group                  → ObjectId: 661e9500-f30c-52e5-b827-557766551111
                         ExternalId: data-team (nom humain)
ServiceAccount         → ObjectId: 772fa611-g41d-63f6-c938-668877662222
                         ExternalId: gateway-service (nom du service)

RÈGLE FONDAMENTALE : Tous les principals sont identifiés par leur ObjectId (GUID),
jamais par un format textuel comme "user:email" ou "group:name".
```

### État Actuel

Le système actuel utilise OpenFGA avec un modèle ReBAC simple :
- Relations directes (user → resource)
- Héritage limité via `from` keyword
- Pas de concept de groupe ou service account
- Pas de rôles prédéfinis modulables

**Fichiers existants :**
- `LLMProxy.Infrastructure.Authorization/OpenFgaAuthorizationService.cs`
- `LLMProxy.Infrastructure.Authorization.Abstractions/IAuthorizationService.cs`
- `infrastructure/openfga/model.fga`

## ARCHITECTURE CIBLE

### 1. Hiérarchie des Scopes (avec héritage)

```
Platform (global)
└── Organization
    └── Tenant
        ├── Provider
        ├── Route
        ├── ApiKey
        ├── Config
        ├── Stats
        └── AuditLog
```

**Règle d'héritage** : Une permission accordée à un scope s'applique automatiquement à toutes les ressources enfants de ce scope.

### 2. Modèle de Données

```csharp
// Principal - Qui demande l'accès
public record Principal(
    PrincipalType Type,      // User, Group, ServiceAccount
    string Id,               // Identifiant unique
    string DisplayName       // Nom affiché
);

public enum PrincipalType
{
    User,
    Group,
    ServiceAccount
}

// Scope - Où s'applique la permission
public record Scope(
    ScopeType Type,          // Platform, Organization, Tenant, Resource
    string Id,               // Identifiant du scope
    string? ParentScopeId    // Pour l'héritage
);

public enum ScopeType
{
    Platform,      // /*
    Organization,  // /organizations/{orgId}
    Tenant,        // /organizations/{orgId}/tenants/{tenantId}
    Resource       // /organizations/{orgId}/tenants/{tenantId}/{resourceType}/{resourceId}
}

// Permission - Quelle action est autorisée
public record Permission(
    string Action,           // Ex: "read", "write", "delete", "admin"
    string ResourceType      // Ex: "provider", "route", "apikey", "*" (wildcard)
);

// Role Definition - Template de permissions réutilisable
public record RoleDefinition(
    string Id,
    string Name,             // Ex: "Reader", "Contributor", "Owner"
    string Description,
    IReadOnlyList<Permission> Permissions,
    IReadOnlyList<ScopeType> AssignableScopes
);

// Role Assignment - Attribution d'un rôle à un principal sur un scope
public record RoleAssignment(
    string Id,
    Principal Principal,
    RoleDefinition Role,
    Scope Scope,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? ExpiresAt       // Support des assignments temporaires
);
```

### 3. Rôles Prédéfinis

| Rôle | Permissions | Description |
|------|-------------|-------------|
| **Platform.Admin** | `*:*` | Super-admin global |
| **Organization.Owner** | `*:*` sur org | Propriétaire d'organisation |
| **Organization.Admin** | `read,write,delete:*` sur org | Admin d'organisation |
| **Tenant.Owner** | `*:*` sur tenant | Propriétaire de tenant |
| **Tenant.Admin** | `read,write,delete:*` sur tenant | Admin de tenant |
| **Tenant.Operator** | `read,write:provider,route,config` | Opérateur |
| **Tenant.Reader** | `read:*` sur tenant | Lecteur |
| **Provider.User** | `read,use:provider` | Utilisateur de provider |
| **ApiKey.Owner** | `read,revoke:apikey` | Propriétaire de clé API |

### 4. API Authorization (Nouvelle API Autonome)

```
LLMProxy.Authorization.API/
├── Controllers/
│   ├── V1/
│   │   ├── RolesController.cs           # CRUD Role Definitions
│   │   ├── AssignmentsController.cs     # CRUD Role Assignments
│   │   ├── PrincipalsController.cs      # Gestion des Principals
│   │   ├── CheckController.cs           # Vérification d'autorisation
│   │   └── ScopesController.cs          # Hiérarchie des scopes
│   └── Internal/
│       └── SyncController.cs            # Synchro avec OpenFGA
├── Services/
│   ├── AuthorizationEngine.cs           # Moteur de décision
│   ├── ScopeHierarchyResolver.cs        # Résolution héritage scopes
│   ├── PermissionCalculator.cs          # Calcul permissions effectives
│   └── OpenFgaSyncService.cs            # Sync vers OpenFGA
├── Models/
│   ├── Requests/
│   │   ├── CheckPermissionRequest.cs
│   │   ├── CreateAssignmentRequest.cs
│   │   └── CreateRoleRequest.cs
│   └── Responses/
│       ├── PermissionCheckResponse.cs
│       ├── EffectivePermissionsResponse.cs
│       └── RoleAssignmentResponse.cs
└── Program.cs
```

### 5. Endpoints API

```http
# === Gestion des Rôles ===
GET    /api/v1/roles                          # Liste des rôles
GET    /api/v1/roles/{roleId}                 # Détail d'un rôle
POST   /api/v1/roles                          # Créer un rôle custom
PUT    /api/v1/roles/{roleId}                 # Modifier un rôle custom
DELETE /api/v1/roles/{roleId}                 # Supprimer un rôle custom

# === Gestion des Assignments ===
GET    /api/v1/assignments                    # Liste des assignments (filtrable)
GET    /api/v1/assignments/{assignmentId}     # Détail d'un assignment
POST   /api/v1/assignments                    # Créer un assignment
DELETE /api/v1/assignments/{assignmentId}     # Supprimer un assignment

# === Gestion des Principals ===
GET    /api/v1/principals                     # Liste des principals
GET    /api/v1/principals/{principalId}       # Détail d'un principal
POST   /api/v1/principals/groups              # Créer un groupe
PUT    /api/v1/principals/groups/{groupId}/members  # Gérer membres du groupe

# === Vérification d'Autorisation ===
POST   /api/v1/check                          # Vérifier une permission
POST   /api/v1/check/batch                    # Vérification batch
GET    /api/v1/effective-permissions          # Permissions effectives d'un principal

# === Hiérarchie des Scopes ===
GET    /api/v1/scopes                         # Arborescence des scopes
GET    /api/v1/scopes/{scopeId}/children      # Enfants d'un scope
GET    /api/v1/scopes/{scopeId}/assignments   # Assignments sur un scope
```

### 6. Nouveau Modèle OpenFGA

```fga
model
  schema 1.1

# Types de principals
type user
type group
  relations
    define member: [user, group#member]
type service_account

# Platform - niveau global
type platform
  relations
    define admin: [user, group#member, service_account]

# Organization
type organization
  relations
    define platform: [platform]
    define owner: [user, group#member, service_account]
    define admin: [user, group#member, service_account] or owner or admin from platform
    define member: [user, group#member, service_account] or admin

# Tenant
type tenant
  relations
    define organization: [organization]
    define owner: [user, group#member, service_account]
    define admin: [user, group#member, service_account] or owner or admin from organization
    define operator: [user, group#member, service_account] or admin
    define reader: [user, group#member, service_account] or operator

# Resources avec héritage du tenant
type provider
  relations
    define tenant: [tenant]
    define can_read: reader from tenant
    define can_write: operator from tenant
    define can_delete: admin from tenant
    define can_use: can_read

type route
  relations
    define tenant: [tenant]
    define can_read: reader from tenant
    define can_write: operator from tenant
    define can_delete: admin from tenant

type api_key
  relations
    define tenant: [tenant]
    define owner: [user, service_account]
    define can_read: owner or reader from tenant
    define can_revoke: owner or admin from tenant

type config
  relations
    define tenant: [tenant]
    define can_read: reader from tenant
    define can_write: admin from tenant

type stats
  relations
    define tenant: [tenant]
    define can_read: reader from tenant
    define can_export: operator from tenant

type audit_log
  relations
    define tenant: [tenant]
    define can_read: admin from tenant
```

## IMPLÉMENTATION

### Phase 1 : Domain Layer (Modèles)
1. Créer `LLMProxy.Domain.Authorization/` avec les entités
2. Définir les Value Objects (Principal, Scope, Permission)
3. Créer les RoleDefinition et RoleAssignment

### Phase 2 : Infrastructure Layer
1. Créer `LLMProxy.Infrastructure.Authorization.PostgreSQL/`
2. Tables : `principals`, `groups`, `group_members`, `role_definitions`, `role_assignments`
3. Mettre à jour `OpenFgaAuthorizationService` pour le nouveau modèle

### Phase 3 : Application Layer
1. Créer handlers CQRS pour les opérations
2. Implémenter `AuthorizationEngine` avec résolution d'héritage
3. Créer `PermissionCalculator` pour les permissions effectives

### Phase 4 : Authorization.API
1. Créer le nouveau projet `LLMProxy.Authorization.API`
2. Implémenter les contrôleurs
3. Configurer OpenAPI/Swagger
4. Ajouter health checks et métriques

### Phase 5 : Migration et Intégration
1. Migrer les données existantes
2. Mettre à jour Admin.API et Gateway pour utiliser Authorization.API
3. Tests d'intégration end-to-end

## CRITÈRES DE SUCCÈS

- [ ] Modèle de données Principal/Scope/Permission implémenté
- [ ] Support des groupes avec membres
- [ ] Support des service accounts
- [ ] Héritage de permissions sur les scopes enfants
- [ ] 10+ rôles prédéfinis disponibles
- [ ] API Authorization autonome déployable
- [ ] Endpoints CRUD pour roles et assignments
- [ ] Endpoint de vérification avec latence < 50ms
- [ ] Synchronisation avec OpenFGA fonctionnelle
- [ ] Migration des données existantes
- [ ] Tests unitaires (>80% coverage)
- [ ] Tests d'intégration
- [ ] Documentation OpenAPI complète
- [ ] Build : 0 erreurs, 0 warnings

## DÉPENDANCES

- OpenFGA Server opérationnel
- PostgreSQL pour stockage des assignments
- Redis pour cache des permissions

## ESTIMATION

**Effort** : 40h (refonte majeure)
**Complexité** : Très haute

## RÉFÉRENCES

- [Azure RBAC Documentation](https://docs.microsoft.com/azure/role-based-access-control/)
- [OpenFGA Documentation](https://openfga.dev/docs)
- ADR-055 (Architecture Authorization actuelle)
- Modèle actuel : `infrastructure/openfga/model.fga`

## NOTES

Cette tâche est une refonte majeure du système d'autorisation. Elle devrait être décomposée en sous-tâches si nécessaire.

Propositions de subdivision :
- 067.1 : Domain Layer + Infrastructure (modèles de données)
- 067.2 : Application Layer (AuthorizationEngine)
- 067.3 : Authorization.API (nouvelle API)
- 067.4 : Migration et Intégration

## TRACKING
