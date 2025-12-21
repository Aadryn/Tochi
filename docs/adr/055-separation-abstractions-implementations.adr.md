# 055. Séparation des Abstractions et des Implémentations dans des Projets Distincts

Date: 2025-12-21

## Statut

Accepté

## Contexte

Dans une architecture .NET moderne respectant les principes de Clean Architecture et Dependency Inversion, il est crucial de bien séparer les abstractions (interfaces, modèles, enums, constantes) des implémentations concrètes. Cette séparation permet de :

1. **Réduire les dépendances circulaires** : Les projets clients peuvent référencer uniquement les abstractions sans être couplés aux implémentations
2. **Faciliter les tests** : Les tests peuvent dépendre uniquement des contrats (interfaces) sans nécessiter les implémentations réelles
3. **Améliorer la réutilisabilité** : Les abstractions peuvent être partagées entre plusieurs projets sans imposer de dépendances lourdes
4. **Respecter le principe d'inversion de dépendance (DIP)** : Les modules de haut niveau ne dépendent pas des modules de bas niveau, mais tous deux dépendent d'abstractions
5. **Permettre le remplacement des implémentations** : Facilite le changement d'implémentation sans impacter les consommateurs

Actuellement, le projet mélange parfois abstractions et implémentations dans le même projet (ex: `LLMProxy.Infrastructure.Security` contient à la fois `IHashService` et `Sha256HashService`), ce qui crée des couplages inutiles.

## Décision

**Nous adoptons la convention suivante pour TOUS les projets .NET du système** :

### Convention de Nommage

```
{ProjectName}.csproj                  → Implémentations concrètes UNIQUEMENT
{ProjectName}.Abstractions.csproj     → Abstractions (interfaces, models, enums, constants) UNIQUEMENT
```

### Règles de Placement

#### Projet `.Abstractions.csproj` (Contrats)

**DOIT contenir** :
- ✅ **Interfaces** (services, repositories, factories, etc.)
- ✅ **Modèles de données partagés** (DTOs, requests, responses, results)
- ✅ **Enums** partagés
- ✅ **Constantes** publiques
- ✅ **Exceptions métier** personnalisées
- ✅ **Value Objects** (si réutilisés en dehors du domaine)
- ✅ **Attributs personnalisés** (annotations, metadata)

**NE DOIT PAS contenir** :
- ❌ Implémentations concrètes de classes
- ❌ Logique métier exécutable
- ❌ Dépendances vers des bibliothèques d'implémentation (Entity Framework, Dapper, etc.)
- ❌ Code lié à une technologie spécifique

**Dépendances autorisées** :
- ✅ Autres projets `.Abstractions`
- ✅ Bibliothèques de base .NET (`System.*`)
- ✅ Packages d'annotations légères (`System.ComponentModel.Annotations`, `FluentValidation.Abstractions`)

#### Projet `.csproj` (Implémentations)

**DOIT contenir** :
- ✅ Implémentations concrètes des interfaces
- ✅ Logique métier exécutable
- ✅ Accès aux bibliothèques tierces (EF Core, Dapper, Npgsql, etc.)
- ✅ Configuration spécifique à la technologie

**DOIT référencer** :
- ✅ Le projet `.Abstractions` correspondant
- ✅ Les dépendances nécessaires à l'implémentation

### Exemples Concrets

#### Infrastructure - Sécurité

**AVANT (Structure Actuelle - À Corriger)** :
```
LLMProxy.Infrastructure.Security.csproj
  ├── IHashService.cs                    ❌ Interface mélangée
  ├── Sha256HashService.cs               ✅ Implémentation
  ├── IApiKeyExtractor.cs                ❌ Interface mélangée
  ├── HeaderApiKeyExtractor.cs           ✅ Implémentation
  ├── IApiKeyValidator.cs                ❌ Interface mélangée
  ├── ApiKeyValidator.cs                 ✅ Implémentation
  ├── ApiKeyValidationResult.cs          ❌ Model mélangé
  ├── IApiKeyAuthenticator.cs            ❌ Interface mélangée
  ├── ApiKeyAuthenticator.cs             ✅ Implémentation
  └── ApiKeyAuthenticationResult.cs      ❌ Model mélangé
```

**APRÈS (Structure Correcte)** :
```
LLMProxy.Infrastructure.Security.Abstractions.csproj
  ├── IHashService.cs                    ✅ Interface pure
  ├── IApiKeyExtractor.cs                ✅ Interface pure
  ├── IApiKeyValidator.cs                ✅ Interface pure
  ├── ApiKeyValidationResult.cs          ✅ Result model
  ├── IApiKeyAuthenticator.cs            ✅ Interface pure
  └── ApiKeyAuthenticationResult.cs      ✅ Result model

LLMProxy.Infrastructure.Security.csproj
  ├── Sha256HashService.cs               ✅ Implémentation concrète
  ├── HeaderApiKeyExtractor.cs           ✅ Implémentation concrète
  ├── ApiKeyValidator.cs                 ✅ Implémentation concrète
  └── ApiKeyAuthenticator.cs             ✅ Implémentation concrète
  └── (références) → LLMProxy.Infrastructure.Security.Abstractions
```

#### Infrastructure - Caching

**Structure Recommandée** :
```
LLMProxy.Infrastructure.Caching.Abstractions.csproj
  ├── ICacheService.cs
  ├── CacheOptions.cs
  └── CacheKeys.cs (constantes)

LLMProxy.Infrastructure.Caching.csproj
  ├── RedisCacheService.cs
  ├── InMemoryCacheService.cs
  └── (références) → LLMProxy.Infrastructure.Caching.Abstractions
                  → StackExchange.Redis (pour Redis)
```

#### Application - Services

**Structure Recommandée** :
```
LLMProxy.Application.Abstractions.csproj
  ├── Services/
  │   ├── IQuotaService.cs
  │   ├── QuotaCheckResult.cs
  │   ├── ITokenCounterService.cs
  │   └── ISecretService.cs
  └── DTOs/
      ├── QuotaCheckRequest.cs
      └── QuotaCheckResponse.cs

LLMProxy.Application.csproj
  ├── Services/
  │   ├── QuotaService.cs
  │   ├── TokenCounterService.cs
  │   └── SecretService.cs
  └── (références) → LLMProxy.Application.Abstractions
                  → LLMProxy.Domain
```

### Graphe de Dépendances

```
┌─────────────────────────────────────────────┐
│  LLMProxy.Gateway.csproj                    │
│  (API, Middleware, Controllers)             │
└────────────┬────────────────────────────────┘
             │ référence
             ▼
┌─────────────────────────────────────────────┐
│  LLMProxy.Application.Abstractions.csproj   │
│  (IQuotaService, ITokenCounter, DTOs)       │
└─────────────────────────────────────────────┘
             ▲
             │ référence
┌────────────┴────────────────────────────────┐
│  LLMProxy.Application.csproj                │
│  (QuotaService, implémentations)            │
└────────────┬────────────────────────────────┘
             │ référence
             ▼
┌─────────────────────────────────────────────┐
│  LLMProxy.Infrastructure.*.Abstractions     │
│  (IHashService, ICacheService, etc.)        │
└─────────────────────────────────────────────┘
             ▲
             │ implémente
┌────────────┴────────────────────────────────┐
│  LLMProxy.Infrastructure.*.csproj           │
│  (Sha256HashService, Redis, PostgreSQL)     │
└─────────────────────────────────────────────┘
```

### Enregistrement dans DI (Program.cs)

```csharp
// ✅ CORRECT : Référencer uniquement les abstractions dans les signatures
builder.Services.AddSingleton<IHashService, Sha256HashService>();
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IQuotaService, QuotaService>();

// ❌ INCORRECT : Exposer le type concret
builder.Services.AddSingleton<Sha256HashService>(); // Violation DIP
```

## Conséquences

### Positives

- ✅ **Dépendances réduites** : Les projets clients (Gateway, Application) peuvent référencer uniquement les abstractions, réduisant drastiquement le graphe de dépendances
- ✅ **Tests isolés** : Les tests unitaires peuvent mocker facilement les interfaces sans dépendre des implémentations concrètes et de leurs dépendances (Redis, PostgreSQL, etc.)
- ✅ **Compilation incrémentale plus rapide** : Les changements dans les implémentations ne déclenchent pas de recompilation des projets consommateurs s'ils dépendent uniquement des abstractions
- ✅ **Découplage technologique** : Facilite le remplacement d'une technologie (Redis → Memcached) sans impacter les consommateurs
- ✅ **Respecte DIP (SOLID)** : Dépendance stricte vers les abstractions, jamais vers les implémentations
- ✅ **Versionning indépendant** : Les abstractions peuvent évoluer à un rythme différent des implémentations
- ✅ **Packaging NuGet facilité** : Les `.Abstractions` peuvent être publiés comme packages légers sans dépendances lourdes
- ✅ **Maintenabilité améliorée** : Séparation claire des responsabilités (contrat vs implémentation)

### Négatives

- ⚠️ **Plus de projets à gérer** : Chaque module fonctionnel aura 2 projets au lieu d'un seul
  - **Mitigation** : Organisation claire dans la solution (dossiers `Core/`, `Infrastructure/`, `Application/`)
- ⚠️ **Légère complexité initiale** : Les développeurs doivent comprendre où placer chaque type
  - **Mitigation** : Règles claires documentées dans cet ADR + revues de code strictes
- ⚠️ **Références de projets supplémentaires** : Les projets d'implémentation doivent référencer leur `.Abstractions`
  - **Mitigation** : Automatisation via templates de projet .NET

### Neutres

- **Impact sur les performances** : Aucun (compilation uniquement)
- **Migration existante** : Nécessite refactoring des projets actuels (tâche planifiable)

## Alternatives Considérées

### Option A: Tout dans un seul projet

- **Description** : Garder interfaces et implémentations dans le même projet (structure actuelle)
- **Avantages** :
  - Moins de projets à gérer
  - Simplicité apparente pour les débutants
- **Inconvénients** :
  - ❌ Viole le principe DIP (clients couplés aux implémentations)
  - ❌ Tests obligés de dépendre de toutes les bibliothèques d'implémentation
  - ❌ Compilation lente (changement d'implémentation → recompilation de tous les consommateurs)
  - ❌ Impossible de distribuer uniquement les contrats (NuGet packages lourds)
- **Raison du rejet** : Ne respecte pas les principes SOLID et Clean Architecture, crée un couplage fort

### Option B: Projet `Contracts` centralisé

- **Description** : Un seul projet `LLMProxy.Contracts` contenant toutes les abstractions du système
- **Avantages** :
  - Un seul projet d'abstractions à gérer
  - Visibilité globale des contrats
- **Inconvénients** :
  - ❌ Violation de la cohésion (abstractions non liées mélangées)
  - ❌ Dépendances circulaires potentielles (Contracts → Domain, Domain → Contracts)
  - ❌ Difficult de versionner indépendamment les modules
  - ❌ Package NuGet monolithique (tout ou rien)
- **Raison du rejet** : Crée un point de couplage central contraire à la modularité

### Option C: Sous-dossiers dans le même projet

- **Description** : Organisation en sous-dossiers `Abstractions/` et `Implementations/` dans chaque projet
- **Avantages** :
  - Moins de fichiers `.csproj` à gérer
  - Organisation visuelle claire
- **Inconvénients** :
  - ❌ Les consommateurs doivent quand même référencer le projet complet (pas de réduction de dépendances)
  - ❌ Impossible d'empêcher techniquement la référence aux implémentations concrètes
  - ❌ Tests obligés de charger toutes les dépendances
- **Raison du rejet** : Organisation cosmétique sans bénéfice réel sur le couplage

## Références

- [ADR-005: Principes SOLID](./005-principes-solid.adr.md) - Dependency Inversion Principle
- [ADR-006: Onion Architecture](./006-onion-architecture.adr.md) - Dépendances vers l'intérieur
- [ADR-014: Dependency Injection](./014-dependency-injection.adr.md) - Injection via interfaces
- [Microsoft Docs - .NET Application Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/)
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
