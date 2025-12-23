# 55. Implémentation d'OpenFGA pour l'autorisation ReBAC

Date: 2025-01-07
Statut: Accepté

## Contexte

LLMProxy est un proxy multi-tenant pour les API LLM nécessitant un contrôle d'accès fin aux ressources. Les exigences sont :

1. **Multi-tenancy** : Isolation stricte entre tenants
2. **Hiérarchie des permissions** : Organisation → Tenant → Ressources
3. **Flexibilité** : Relations personnalisables sans modification du code
4. **Performance** : Latence minimale pour les vérifications d'autorisation
5. **Auditabilité** : Traçabilité des décisions d'autorisation

## Décision

Nous adoptons **OpenFGA** comme moteur d'autorisation ReBAC (Relationship-Based Access Control).

### Architecture Implémentée

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Layer                         │
│  ┌───────────────────┐  ┌────────────────────────────────────┐  │
│  │ RequirePermission │  │    AuthorizationBehavior<T,R>      │  │
│  │    Attribute      │──│         (MediatR Pipeline)         │  │
│  └───────────────────┘  └────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│                      Infrastructure Layer                        │
│  ┌───────────────────┐  ┌────────────────────────────────────┐  │
│  │IAuthorizationSvc  │──│   OpenFgaAuthorizationService      │  │
│  │   (Abstraction)   │  │        (Implementation)            │  │
│  └───────────────────┘  └──────────────┬─────────────────────┘  │
├─────────────────────────────────────────┼───────────────────────┤
│                     Presentation Layer  │                        │
│  ┌───────────────────┐  ┌───────────────┴────────────────────┐  │
│  │ FgaAuthorize      │──│ OpenFgaAuthorizationMiddleware     │  │
│  │   Attribute       │  │      (ASP.NET Core)                │  │
│  └───────────────────┘  └────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│                        External Services                         │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                    OpenFGA Server                          │  │
│  │    ┌─────────────┐  ┌─────────────┐  ┌──────────────┐     │  │
│  │    │ HTTP :8080  │  │ gRPC :8081  │  │ Playground   │     │  │
│  │    │             │  │             │  │   :3001      │     │  │
│  │    └─────────────┘  └─────────────┘  └──────────────┘     │  │
│  │                           │                                │  │
│  │              ┌────────────┴───────────┐                   │  │
│  │              │   PostgreSQL Datastore │                   │  │
│  │              └────────────────────────┘                   │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Modèle d'Autorisation ReBAC

```
Organization
    ├── admin: [user]
    └── member: [user] or admin
           │
           ▼
       Tenant
           ├── organization: [organization]
           ├── owner: [user]
           ├── admin: [user] or owner or admin from organization
           ├── operator: [user] or admin
           ├── viewer: [user] or operator
           ├── can_manage: admin
           ├── can_operate: operator
           └── can_view: viewer
                  │
        ┌────────┼────────┬───────────┬─────────┬────────┐
        ▼        ▼        ▼           ▼         ▼        ▼
    Provider  API Key   Route      Config    Stats  Audit Log
```

### Projets Créés

| Projet | Rôle |
|--------|------|
| `LLMProxy.Infrastructure.Authorization.Abstractions` | Interfaces et DTOs |
| `LLMProxy.Infrastructure.Authorization` | Implémentation OpenFGA |
| `LLMProxy.Infrastructure.Authorization.Tests` | Tests unitaires |

### Fichiers Clés

| Fichier | Description |
|---------|-------------|
| `IAuthorizationService.cs` | Abstraction du service d'autorisation |
| `OpenFgaAuthorizationService.cs` | Implémentation OpenFGA |
| `AuthorizationBehavior.cs` | Pipeline MediatR |
| `RequirePermissionAttribute.cs` | Attribut pour requêtes MediatR |
| `FgaAuthorizeAttribute.cs` | Attribut pour endpoints ASP.NET Core |
| `OpenFgaAuthorizationMiddleware.cs` | Middleware HTTP |
| `model.fga` | Modèle d'autorisation OpenFGA |

## Conséquences

### Positives

1. **Autorisation déclarative** : Permissions définies en configuration, pas dans le code
2. **Relations héritées** : Les admins d'organisation héritent automatiquement des droits sur les tenants
3. **Granularité fine** : Contrôle précis par ressource et relation
4. **Performance** : OpenFGA optimisé pour les vérifications d'autorisation à haute fréquence
5. **Testabilité** : Modèle d'autorisation testable indépendamment
6. **Standard CNCF** : Projet mature de la Cloud Native Computing Foundation

### Négatives

1. **Complexité opérationnelle** : Service externe supplémentaire à maintenir
2. **Latence réseau** : Chaque vérification nécessite un appel à OpenFGA
3. **Courbe d'apprentissage** : Modèle ReBAC moins intuitif que RBAC classique
4. **Dépendance externe** : Mode dégradé nécessaire si OpenFGA indisponible

### Mitigations

- **Health checks** : Surveillance de la disponibilité d'OpenFGA
- **Fallback mode** : Configuration pour Allow/Deny si OpenFGA indisponible
- **Caching** (futur) : Cache des résultats d'autorisation fréquents
- **Documentation** : Exemples et guides d'utilisation fournis

## Alternatives Considérées

### 1. Authorization Policies ASP.NET Core Natif
- **Avantages** : Intégré, pas de dépendance externe
- **Inconvénients** : Pas de modèle relationnel, moins flexible
- **Raison du rejet** : Ne supporte pas ReBAC nativement

### 2. OPA (Open Policy Agent)
- **Avantages** : Très flexible, langage Rego puissant
- **Inconvénients** : Complexité du langage Rego, pas orienté ReBAC
- **Raison du rejet** : Courbe d'apprentissage plus élevée

### 3. Casbin
- **Avantages** : Léger, supporté en .NET
- **Inconvénients** : Moins de fonctionnalités ReBAC, moins mature
- **Raison du rejet** : OpenFGA plus adapté aux relations hiérarchiques

## Références

- [OpenFGA Documentation](https://openfga.dev/docs)
- [OpenFGA .NET SDK](https://github.com/openfga/dotnet-sdk)
- [Zanzibar Paper (Google)](https://research.google/pubs/pub48190/)
- [CNCF OpenFGA](https://www.cncf.io/projects/openfga/)
