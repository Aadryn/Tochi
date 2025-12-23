# Tâche 067 - Application Authorization Autonome (Azure RBAC Style)

## PRIORITÉ
🔴 **P1 - CRITIQUE** (Fondation sécurité)

## OBJECTIF

Créer une **application Authorization totalement autonome et découplée** du backend LLMProxy, inspirée du modèle Azure RBAC.

## PRINCIPES FONDAMENTAUX

### 1. Application Découplée

```
┌─────────────────────────────────────────────────────────────────────┐
│  ARCHITECTURE DÉCOUPLÉE                                             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  /workspaces/proxy/                                                 │
│  ├── backend/           ← Backend LLMProxy existant                │
│  ├── frontend/          ← Frontend Vue.js existant                 │
│  └── authorization/     ← NOUVELLE APP AUTONOME                    │
│                                                                     │
│  L'application Authorization :                                      │
│  • Est déployée séparément                                         │
│  • A sa propre base de données                                     │
│  • A son propre cache Redis                                        │
│  • Peut être utilisée par d'autres applications                    │
│  • N'a AUCUNE dépendance vers backend/                             │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 2. IDP Externe pour les Principals

```
┌─────────────────────────────────────────────────────────────────────┐
│  GESTION DES IDENTITÉS                                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  IDP Externe (Azure AD, Okta, Keycloak, etc.)                      │
│  ├── Gère les utilisateurs, groupes, service accounts             │
│  ├── Émet les tokens JWT                                           │
│  └── Source de vérité pour les identités                           │
│                                                                     │
│  Application Authorization                                          │
│  ├── ❌ Ne crée PAS de comptes utilisateurs                        │
│  ├── ❌ Ne gère PAS les mots de passe                              │
│  ├── ❌ Ne gère PAS l'authentification                             │
│  ├── ✅ Synchronise les principals depuis l'IDP (ObjectId)         │
│  ├── ✅ Attribue des rôles aux principals                          │
│  └── ✅ Vérifie les permissions                                    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 3. Format de Scope Style URL REST

```
┌─────────────────────────────────────────────────────────────────────┐
│  FORMAT DES SCOPES                                                  │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Structure : {domain}/{resource}/{id}/{sub-resource}/{id}/...      │
│                                                                     │
│  Exemples :                                                         │
│  • api.llmproxy.com                           (racine)             │
│  • api.llmproxy.com/organizations             (collection)         │
│  • api.llmproxy.com/organizations/org-123     (instance)           │
│  • api.llmproxy.com/organizations/org-123/tenants                  │
│  • api.llmproxy.com/organizations/org-123/tenants/tenant-456       │
│  • api.llmproxy.com/organizations/org-123/tenants/tenant-456/providers │
│  • api.llmproxy.com/organizations/org-123/tenants/tenant-456/providers/openai-1 │
│                                                                     │
│  Héritage : Permission sur parent → s'applique aux enfants         │
│  • Permission sur api.llmproxy.com/organizations/org-123           │
│  • → S'applique à tous les tenants/providers/routes de org-123    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### 4. ObjectId (GUID) pour les Principals

```
┌─────────────────────────────────────────────────────────────────────┐
│  IDENTIFICATION DES PRINCIPALS                                      │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Chaque principal de l'IDP a un ObjectId unique (GUID)             │
│                                                                     │
│  User :                                                             │
│    ObjectId : 550e8400-e29b-41d4-a716-446655440000                 │
│    ExternalId : john@example.com (depuis IDP)                      │
│    Type : user                                                      │
│                                                                     │
│  Group :                                                            │
│    ObjectId : 661e9500-f30c-52e5-b827-557766551111                 │
│    ExternalId : data-scientists (depuis IDP)                       │
│    Type : group                                                     │
│                                                                     │
│  ServiceAccount :                                                   │
│    ObjectId : 772fa611-g41d-63f6-c938-668877662222                 │
│    ExternalId : llmproxy-gateway (depuis IDP)                      │
│    Type : service_account                                          │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

## STRUCTURE DE L'APPLICATION

```
authorization/
├── README.md
├── Authorization.sln
├── docker-compose.yml
├── Dockerfile
│
├── src/
│   ├── Authorization.Domain/
│   │   ├── Authorization.Domain.csproj
│   │   ├── ValueObjects/
│   │   │   ├── PrincipalId.cs         # Wrapper sur GUID (ObjectId)
│   │   │   ├── PrincipalType.cs
│   │   │   └── Scope.cs               # Format URL REST
│   │   └── DTOs/
│   │       ├── CheckPermissionRequest.cs
│   │       ├── CheckPermissionResponse.cs
│   │       ├── RoleAssignmentDto.cs
│   │       └── AssignRoleRequest.cs
│   │
│   ├── Authorization.Application/
│   │   ├── Authorization.Application.csproj
│   │   ├── Services/
│   │   │   ├── IAuthorizationService.cs
│   │   │   ├── AuthorizationService.cs
│   │   │   ├── IRoleAssignmentService.cs
│   │   │   ├── RoleAssignmentService.cs
│   │   │   ├── IGroupService.cs
│   │   │   ├── GroupService.cs
│   │   │   ├── IScopeService.cs
│   │   │   └── ScopeService.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── Authorization.Infrastructure/
│   │   ├── Authorization.Infrastructure.csproj
│   │   ├── OpenFga/
│   │   │   ├── IOpenFgaService.cs
│   │   │   ├── OpenFgaService.cs
│   │   │   ├── OpenFgaConfiguration.cs
│   │   │   └── TupleBuilder.cs
│   │   ├── Caching/
│   │   │   ├── IPermissionCache.cs
│   │   │   └── PermissionCacheService.cs
│   │   ├── Health/
│   │   │   └── OpenFgaHealthCheck.cs
│   │   └── DependencyInjection.cs
│   │
│   └── Authorization.API/
│       ├── Authorization.API.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── Controllers/
│       │   ├── CheckController.cs
│       │   ├── AssignmentsController.cs
│       │   ├── GroupsController.cs
│       │   └── PermissionsController.cs
│       └── Middleware/
│           └── JwtValidationMiddleware.cs
│
├── tests/
│   ├── Authorization.Application.Tests/
│   └── Authorization.API.Tests/
│
├── infrastructure/
│   └── openfga/
│       ├── authorization-model.fga
│       └── seed-data.json
│
└── migrations/
    └── *.json                          # Migrations de tuples OpenFGA
```

**Note** : PostgreSQL utilisé UNIQUEMENT pour l'audit trail - OpenFGA stocke les autorisations.

## DÉCISIONS DE CADRAGE (2025-12-23)

| # | Question | Décision |
|---|----------|----------|
| 1 | Rôles | Base fixes + Custom via API |
| 2 | Sync groupes IDP | JIT + Batch + Webhook |
| 3 | Multi-rôles par scope | Autorisé (cumulatif) |
| 4 | Expiration | Optionnelle + Cleanup Job |
| 5 | Audit | Logs + PostgreSQL |
| 6 | Gestion scopes | Explicite (créer avant assigner) |
| 7 | Multi-tenancy | 1 store OpenFGA / tenant |
| 8 | Permissions | Granulaires (resource:action) |
| 9 | Délégation | Hiérarchique (≤ son rôle) |
| 10 | Révocation | Immédiate via OpenFGA |

## SOUS-TÂCHES

| ID | Titre | Effort | Dépendances |
|----|-------|--------|-------------|
| 067.1 | Domain Layer (DTOs, Value Objects) | 4h | - |
| 067.2 | Infrastructure OpenFGA + Redis + PostgreSQL Audit | 14h | 067.1 |
| 067.3 | Application Layer (Façade OpenFGA + Logique métier) | 14h | 067.1, 067.2 |
| 067.4 | API Layer (Controllers, JWT) | 8h | 067.3 |
| 067.5 | IDP Integration (JIT + Batch + Webhook) | 12h | 067.3 |
| 067.6 | Backend SDK (Client) | 4h | 067.4 |
| 067.7 | Cleanup Job (Expiration) | 4h | 067.3 |
| 067.8 | Tests complets | 10h | 067.1-067.7 |

**Total estimé** : 70h

## CRITÈRES DE SUCCÈS

- [ ] Application dans `/authorization/` totalement autonome
- [ ] Aucune dépendance vers `/backend/`
- [ ] Scopes au format URL REST (explicites)
- [ ] Principals synchronisés depuis IDP (JIT + Batch + Webhook)
- [ ] ObjectId (GUID) pour tous les principals
- [ ] Multi-tenant avec isolation par store OpenFGA
- [ ] Rôles custom via API
- [ ] Permissions granulaires (resource:action)
- [ ] Délégation hiérarchique
- [ ] Expiration optionnelle avec cleanup job
- [ ] Audit trail complet (logs + PostgreSQL)
- [ ] Révocation immédiate
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests : >80% couverture
- [ ] Documentation API complète

## ADR ASSOCIÉ

Voir `docs/adr/060-authorization-azure-rbac-style.adr.md`

## TRACKING
