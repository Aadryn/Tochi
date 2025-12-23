# LLMProxy Authorization Service

**Encapsulation d'OpenFGA - Service d'autorisation style Azure RBAC**

## 🎯 Concept

Ce service est une **encapsulation d'OpenFGA** avec une API .NET servant de **couche d'intermédiation**.

```
┌─────────────────────────────────────────────────────────────────┐
│                         IDP Externe                              │
│              (Azure AD, Okta, Keycloak, etc.)                   │
│                                                                  │
│   Source de vérité : Users, Groups, ServiceAccounts             │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ JWT (ObjectId)
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Authorization Service                         │
│                                                                  │
│   ┌──────────────────────────────────────────────────────────┐  │
│   │                    API .NET                               │  │
│   │              (Couche d'Intermédiation)                    │  │
│   │                                                           │  │
│   │  • Validation des données                                │  │
│   │  • Vérification anti-doublons                            │  │
│   │  • Contraintes métier                                    │  │
│   │  • Cohérence des tuples                                  │  │
│   └──────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│   ┌──────────────────────────────────────────────────────────┐  │
│   │                     OpenFGA                               │  │
│   │           (Source de Vérité Autorisations)                │  │
│   │                                                           │  │
│   │  Stocke : assignments, memberships (copie), hierarchy    │  │
│   └──────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│   ┌──────────────────────────────────────────────────────────┐  │
│   │                      Redis (Cache)                        │  │
│   └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## ✨ Principes

### 1. IDP = Maître des Identités

L'IDP externe est la **source de vérité** pour les identités :
- Users (email, nom, attributs)
- Groups (nom, membres)
- ServiceAccounts (credentials)

L'application Authorization :
- ❌ Ne crée PAS d'utilisateurs
- ❌ Ne stocke PAS les attributs utilisateur
- ✅ Référence les principals par ObjectId (GUID)

### 2. OpenFGA = Maître des Autorisations

OpenFGA stocke tout ce qui concerne les autorisations :
- Role assignments (qui a quel rôle sur quel scope)
- Group memberships (copie synchronisée depuis IDP)
- Scope hierarchy (relations parent/enfant)

### 3. API .NET = Couche d'Intermédiation

L'API .NET n'est **PAS un simple proxy**. Elle ajoute :

| Responsabilité | Description |
|----------------|-------------|
| Validation | Format ObjectId, scope URL REST valide |
| Anti-doublons | Vérifier avant Write qu'un tuple n'existe pas déjà |
| Contraintes | Un user ne peut avoir qu'un seul rôle par scope |
| Cohérence | Créer les tuples `parent` automatiquement |
| Audit | Logger les opérations d'écriture |

### 4. Scopes au Format URL REST

```
api.llmproxy.com
api.llmproxy.com/organizations/org-123
api.llmproxy.com/organizations/org-123/tenants/tenant-456
```

### 5. ObjectId (GUID) pour les Principals

Tous les principals utilisent l'ObjectId (GUID) de l'IDP :
```
user:550e8400-e29b-41d4-a716-446655440000
group:661e9500-f30c-52e5-b827-557766551111
serviceaccount:772fa611-g41d-63f6-c938-668877662222
```

## 📁 Structure du Projet

```
authorization/
├── README.md
├── docker-compose.yml
├── src/
│   ├── Authorization.Domain/           # Value Objects, DTOs
│   ├── Authorization.Application/      # Services (intermédiation)
│   └── Authorization.API/              # Controllers REST
├── tests/
│   ├── Authorization.Application.Tests/
│   └── Authorization.API.Tests/
├── infrastructure/
│   └── openfga/
│       ├── authorization-model.fga     # Modèle d'autorisation
│       └── seed-data.json              # Données initiales
└── migrations/
    └── *.json                          # Migrations de tuples
```

## 🔐 Modèle OpenFGA

```fga
model
  schema 1.1

type user

type group
  relations
    define member: [user, group#member]

type serviceaccount

type scope
  relations
    define parent: [scope]
    define owner: [user, group#member, serviceaccount] or owner from parent
    define contributor: [user, group#member, serviceaccount] or contributor from parent or owner
    define reader: [user, group#member, serviceaccount] or reader from parent or contributor
    define can_read: reader
    define can_write: contributor
    define can_delete: owner
    define can_manage: owner
```

## 📡 API Endpoints

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| `POST` | `/api/v1/check` | Vérifier une permission |
| `POST` | `/api/v1/assignments` | Assigner un rôle |
| `DELETE` | `/api/v1/assignments` | Révoquer un rôle |
| `GET` | `/api/v1/assignments/principal/{id}` | Lister par principal |
| `GET` | `/api/v1/assignments/scope/{scope}` | Lister par scope |
| `POST` | `/api/v1/groups/{id}/members` | Ajouter membre au groupe |
| `DELETE` | `/api/v1/groups/{id}/members/{memberId}` | Retirer membre |
| `GET` | `/api/v1/permissions/{principalId}` | Lister permissions |

### Exemples

**Vérifier une permission :**
```bash
curl -X POST http://localhost:5100/api/v1/check \
  -H "Content-Type: application/json" \
  -d '{
    "principalId": "550e8400-e29b-41d4-a716-446655440000",
    "permission": "can_write",
    "scope": "api.llmproxy.com/organizations/org-123/tenants/tenant-456"
  }'

# Response
{ "allowed": true }
```

**Assigner un rôle :**
```bash
curl -X POST http://localhost:5100/api/v1/assignments \
  -H "Content-Type: application/json" \
  -d '{
    "principalId": "550e8400-e29b-41d4-a716-446655440000",
    "principalType": "user",
    "role": "contributor",
    "scope": "api.llmproxy.com/organizations/org-123"
  }'

# Response
{ "principalId": "550e8400-...", "role": "contributor", "scope": "..." }
```

## 🚀 Démarrage Rapide

```bash
cd authorization

# Démarrer OpenFGA et Redis
docker-compose up -d

# Build
dotnet build

# Tests
dotnet test

# Démarrer l'API
dotnet run --project src/Authorization.API
```

## 🐳 Docker Compose

```yaml
services:
  openfga:
    image: openfga/openfga:latest
    ports:
      - "8080:8080"   # HTTP API
      - "8081:8081"   # gRPC
      - "3000:3000"   # Playground
    command: run

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
```

## 🔗 Intégration Backend

Le backend LLMProxy utilise un SDK client :

```csharp
// Vérifier permission avant opération
var allowed = await _authService.CheckAsync(new CheckPermissionRequest(
    PrincipalId.Create(userId),
    "can_write",
    Scope.Parse($"api.llmproxy.com/organizations/{orgId}/tenants/{tenantId}")));

if (!allowed)
    return Forbid();
```

## 📚 Références

- [ADR-060 : Authorization Service](../docs/adr/060-authorization-azure-rbac-style.adr.md)
- [OpenFGA Documentation](https://openfga.dev/docs)
- [OpenFGA .NET SDK](https://github.com/openfga/dotnet-sdk)
