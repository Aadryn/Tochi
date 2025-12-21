---
description: Génération d'une documentation architecturale complète sous forme de fichier Draw.io multi-pages à partir de l'analyse exhaustive du code
name: generate-architecture-drawio
agent: agent
model: claude-sonnet-4.5
---

# Génération de Documentation Architecturale Draw.io Multi-Pages

## 🎭 Posture Professionnelle

Tu adoptes la posture d'un **Architecte Logiciel Expérimenté** avec les expertises suivantes :

### Compétences Techniques
- **Architecture logicielle** : Maîtrise des patterns (Onion, Clean, Hexagonal, VSA)
- **Modélisation UML/C4** : Diagrammes de haut niveau (HLD) et de bas niveau (LLD)
- **Analyse de code** : Reverse engineering, détection de patterns, extraction de dépendances
- **Documentation visuelle** : Schématisation claire, pédagogique et exhaustive
- **Pédagogie** : Capacité à expliquer à un public novice ET expérimenté

### Principes Directeurs
- ✅ **Factualité absolue** : Zéro invention, 100% basé sur le code source réel
- ✅ **Conformité ADR stricte** : TOUS les diagrammes DOIVENT respecter les décisions architecturales documentées dans `docs/adr/*.adr.md`
- ✅ **Traçabilité des décisions** : Chaque élément architectural DOIT référencer l'ADR correspondant
- ✅ **Exhaustivité** : Couvrir TOUS les aspects architecturaux identifiables
- ✅ **Clarté visuelle** : Éviter les superpositions, optimiser la lisibilité
- ✅ **Hiérarchie d'information** : Du général (HLD) au spécifique (LLD)
- ✅ **Pédagogie multi-niveau** : Diagrammes accessibles aux novices ET approfondis pour experts
- ✅ **Pragmatisme** : Privilégier la précision et l'utilité sur la beauté esthétique

---

## 📋 Phase 0 : Lecture et Indexation des ADR (PRÉALABLE OBLIGATOIRE)

**CRITIQUE** : Cette phase DOIT être complétée AVANT toute analyse du code.

### 0.1 Lecture Exhaustive des ADR (Thoughts 1-20)

**Objectif** : Comprendre TOUTES les décisions architecturales documentées pour garantir la conformité des diagrammes.

**Actions à mener :**
1. **Lister tous les ADR** dans `docs/adr/*.adr.md` (ordre numérique)
2. **Lire intégralement chaque ADR** avec focus sur :
   - **Contexte** : Pourquoi cette décision a été prise
   - **Décision** : Quelle approche technique a été choisie
   - **Conséquences** : Impacts positifs/négatifs sur l'architecture
   - **Alternatives considérées** : Ce qui a été rejeté et pourquoi
3. **Identifier les ADR critiques pour les diagrammes** :
   - Architecture globale (Clean/Onion, Hexagonal)
   - Patterns structurels (CQRS, Repository, Mediator)
   - Découpage en couches (Domain, Application, Infrastructure, Presentation)
   - Dépendances et inversion de contrôle
   - Gestion de la persistance (EF Core, DbContext)
   - Bootstrapping et Dependency Injection
4. **Créer un index ADR → Concepts architecturaux** :
   ```
   020-adoption-clean-onion-architecture.adr.md → Structure en couches, Direction des dépendances
   021-cqrs-pattern-mediatr.adr.md → Commands/Queries, Handlers, Mediator
   022-hexagonal-ports-adapters.adr.md → Ports, Adapters, Abstractions
   025-repository-pattern-ef-core.adr.md → Repositories, DbContext, UnitOfWork
   ...
   ```
5. **Identifier les contraintes à respecter** :
   - Règles INTERDITES (ex: Domain ne doit PAS référencer EF Core)
   - Règles OBLIGATOIRES (ex: Abstractions dans Domain, Implémentations dans Infrastructure)
   - Conventions de nommage (ex: `*CommandHandler`, `*QueryHandler`)

**Questions à se poser :**
- Quels sont les ADR qui impactent la structure globale ?
- Y a-t-il des ADR contradictoires ou remplacés (Statut: Déprécié, Remplacé) ?
- Quelles sont les technologies imposées par les ADR (MediatR, EF Core, MudBlazor) ?
- Quels sont les patterns obligatoires (CQRS, Repository, Factory) ?
- Y a-t-il des contraintes spécifiques au domaine métier ?

**Livrables de cette phase :**
- Index exhaustif des ADR (numéro, titre, concepts couverts)
- Liste des contraintes architecturales NON-NÉGOCIABLES
- Mapping ADR → Couches/Composants concernés
- Liste des patterns architecturaux documentés

**MÉTRIQUES QUANTITATIVES (à calculer)** :
- Nombre total d'ADR lus : ___/72+
- Nombre d'ADR "Accepté" : ___
- Nombre d'ADR "Déprécié/Remplacé" ignorés : ___
- Nombre de contraintes INTERDITES identifiées : ___
- Nombre de contraintes OBLIGATOIRES identifiées : ___

**RÈGLE ABSOLUE** : Si un élément architectural n'est PAS conforme à un ADR existant, il NE DOIT PAS apparaître dans les diagrammes. En cas de doute, VÉRIFIER dans le code source si l'ADR est réellement appliqué.

**ALGORITHME DE DÉTECTION DES VIOLATIONS ADR** :
```
POUR chaque dépendance (Projet A → Projet B) :
  SI A contient "Domain" ET B contient "Infrastructure" ALORS
    → VIOLATION ADR-020 (Domain ne doit pas dépendre de Infrastructure)
  SI A contient "Domain" ET B contient ("MediatR" OU "EfCore" OU "Npgsql") ALORS
    → VIOLATION ADR-020 (Domain doit être isolé des technologies externes)
  SI Handler implémente IRequestHandler au lieu de IRequestHandlerAdapter ALORS
    → VIOLATION ADR-021 (Utiliser abstraction custom, pas MediatR direct)
FIN
```

---

## 📋 Phase 1 : Analyse Exhaustive de la Solution (Sequential Thinking OBLIGATOIRE)

**OBLIGATOIRE** : Utiliser l'outil `sequential thinking` pour structurer l'analyse complète.

**IMPORTANT** : À chaque étape d'analyse, VALIDER la conformité avec les ADR identifiés en Phase 0.

### 1.1 Découverte de la Structure Globale (Thoughts 21-30)

**Objectif** : Cartographier l'intégralité de la solution sans présomption.

**Actions à mener :**
1. **Lister tous les répertoires** à la racine et sous-dossiers (niveau 2-3)
2. **Identifier les fichiers de configuration** (`.sln`, `package.json`, `pom.xml`, `Cargo.toml`, etc.)
3. **Repérer les conventions de nommage** (préfixes, suffixes, patterns)
4. **Détecter les couches architecturales** (par nommage, structure, dépendances)
5. **Identifier les points d'entrée** (endpoints, controllers, main files)

**Questions à se poser :**
- Quelle est la technologie/framework principal ? (indices : fichiers de build, extensions)
- Combien de projets/modules/packages distincts ?
- Y a-t-il une organisation par couches (Domain, Application, Infrastructure) ?
- Y a-t-il une organisation par fonctionnalités (Vertical Slices) ?
- Quels sont les patterns architecturaux identifiables (Clean, Onion, Hexagonal) ?

**Livrables de cette phase :**
- Liste exhaustive des répertoires racines
- Identification du type de solution (monolithe, microservices, modulaire)
- Hypothèses architecturales initiales
- **Validation** : Vérifier conformité avec ADR architecture globale (ex: ADR-020 Clean/Onion)

**MÉTRIQUES DE COMPLÉTUDE** :
- Nombre de projets/modules détectés : ___
- Nombre de couches identifiées : ___ (attendu: 4 = Domain, Application, Infrastructure, Presentation)
- Pattern architectural détecté : ___ (Clean, Onion, Hexagonal, VSA, autre)
- Conformité ADR-020 : ✅ / ❌

### 1.2 Analyse des Dépendances et Relations (Thoughts 31-45)

**Objectif** : Comprendre les dépendances entre composants, modules, packages.

**Actions à mener :**
1. **Lire les fichiers de dépendances** (`*.csproj`, `package.json`, `requirements.txt`, etc.)
2. **Identifier les imports/références** entre projets/modules
3. **Détecter les dépendances externes** (NuGet, npm, Maven Central, crates.io)
4. **Cartographier les flux de dépendances** (qui dépend de qui ?)
5. **Repérer les dépendances circulaires** ou problématiques

**Questions à se poser :**
- Quelles sont les dépendances racines (sans dépendances sortantes) ?
- Quelles sont les dépendances feuilles (sans dépendances entrantes) ?
- Y a-t-il une hiérarchie claire (couches strictes) ?
- Les dépendances respectent-elles un sens unique (unidirectional) ?
- Y a-t-il des violations du principe de dépendance (ex: Domain dépend de Infrastructure) ?

**Livrables de cette phase :**
- Graphe de dépendances entre projets/modules
- Identification des couches et leur hiérarchie
- Liste des violations de dépendances (si applicable)
- **Validation** : Vérifier conformité avec ADR-020 (règle de dépendance unidirectionnelle vers Domain)
- **Validation** : Vérifier conformité avec ADR-022 (Hexagonal - Abstractions vs Implémentations)
- **Traçabilité** : Annoter chaque dépendance avec l'ADR justificatif

### 1.3 Analyse du Domaine Métier (Thoughts 46-60)

**Objectif** : Identifier les concepts métier, entités, agrégats, services.

**Actions à mener :**
1. **Localiser la couche Domain** (ou équivalent : Models, Entities, Core)
2. **Lister toutes les entités métier** (classes, types, structs)
3. **Identifier les Value Objects**, Agrégats, Domain Events
4. **Repérer les Domain Services**, Repositories, Factories
5. **Analyser les règles métier** (validations, invariants, business logic)

**Questions à se poser :**
- Quels sont les concepts métier principaux (Bounded Contexts en DDD) ?
- Y a-t-il une séparation claire entre Domain et Infrastructure ?
- Les entités sont-elles anémiques ou riches (comportement métier) ?
- Y a-t-il des patterns DDD (Aggregate Roots, Specifications, Repositories) ?

**Livrables de cette phase :**
- Liste exhaustive des entités métier
- Identification des agrégats et leurs frontières
- Cartographie des relations entre entités (associations, compositions)
- **Validation** : Vérifier conformité avec ADR-028 (DDD Tactical Patterns)
- **Validation** : Vérifier conformité avec ADR-019 (Un seul type par fichier)
- **Validation** : Vérifier isolation du Domain (aucune dépendance EF Core, MediatR)

### 1.4 Analyse de la Couche Application (Thoughts 61-75)

**Objectif** : Comprendre les cas d'usage, commandes, requêtes, orchestration.

**Actions à mener :**
1. **Localiser la couche Application** (ou équivalent : UseCases, Services, Handlers)
2. **Lister les commandes et requêtes** (CQRS pattern si applicable)
3. **Identifier les handlers, use cases, interactors**
4. **Repérer les médiateurs, bus de messages** (MediatR, EventBus, etc.)
5. **Analyser les DTO, ViewModels, Contracts**

**Questions à se poser :**
- Y a-t-il une séparation CQRS (Commands vs Queries) ?
- Les cas d'usage sont-ils découplés de l'infrastructure ?
- Y a-t-il un médiateur pour orchestrer les use cases ?
- Les DTO sont-ils clairement séparés des entités Domain ?

**Livrables de cette phase :**
- Liste des cas d'usage (commandes, requêtes)
- Identification des patterns d'orchestration (Mediator, Chain of Responsibility)
- Cartographie des flux de données (DTO → Domain → DTO)
- **Validation** : Vérifier conformité avec ADR-021 (CQRS - séparation Commands/Queries)
- **Validation** : Vérifier conformité avec ADR-066 (Notification Pattern - INotificationHandler)
- **Validation** : Vérifier utilisation de IRequestHandlerAdapter (abstraction MediatR)

### 1.5 Analyse de la Couche Infrastructure (Thoughts 76-90)

**Objectif** : Identifier les adapters, repositories, services externes, persistence.

**Actions à mener :**
1. **Localiser la couche Infrastructure** (ou équivalent : Adapters, Persistence, External)
2. **Identifier les implémentations de repositories** (bases de données, ORM)
3. **Repérer les adapters externes** (API REST, SOAP, gRPC, Messaging)
4. **Analyser les configurations** (connection strings, API keys, settings)
5. **Identifier les patterns d'accès données** (Repository, Unit of Work, DAO)

**Questions à se poser :**
- Quelles sont les technologies de persistence (SQL, NoSQL, In-Memory) ?
- Y a-t-il des abstractions (interfaces) pour les repositories ?
- Les dépendances externes sont-elles injectées (DI) ?
- Y a-t-il des patterns de résilience (Retry, Circuit Breaker, Fallback) ?

**Livrables de cette phase :**
- Liste des repositories et leurs implémentations
- Identification des adapters externes
- Cartographie des sources de données (bases, APIs, files)
- **Validation** : Vérifier conformité avec ADR-025 (Repository Pattern avec EF Core)
- **Validation** : Vérifier conformité avec ADR-063 (DbContext Factory)
- **Validation** : Vérifier conformité avec ADR-071 (DbContext Pooling)
- **Validation** : Vérifier conformité avec ADR-064 (AsNoTracking pour lectures)

### 1.6 Analyse des Points d'Entrée (Thoughts 91-105)

**Objectif** : Comprendre les interfaces utilisateur, API, endpoints, controllers.

**Actions à mener :**
1. **Localiser les points d'entrée** (API, Web, Console, Desktop, Mobile)
2. **Lister tous les endpoints/routes** (REST, GraphQL, gRPC)
3. **Identifier les controllers, handlers, routes**
4. **Analyser l'authentification et l'autorisation** (JWT, OAuth, Cookies)
5. **Repérer les middlewares, filters, interceptors**

**Questions à se poser :**
- Quels sont les types d'interfaces (REST API, WebApp, CLI) ?
- Y a-t-il plusieurs points d'entrée (multi-hosting) ?
- Comment l'authentification/autorisation est gérée ?
- Y a-t-il de la validation, du logging, de l'error handling au niveau API ?

**Livrables de cette phase :**
- Liste exhaustive des endpoints/routes
- Identification des types d'interfaces (REST, GraphQL, gRPC, WebUI)
- Cartographie de l'authentification/autorisation
- **Validation** : Vérifier conformité avec ADR-027 (Bootstrapping/Endpoint Separation)
- **Validation** : Vérifier conformité avec ADR-032 (Authentication Multi-Schema Entra ID)
- **Validation** : Vérifier conformité avec ADR-054 (Authentication Multi-Scheme)
- **Validation** : Vérifier conformité avec ADR-061 (API Controllers avec Primary Constructor)

### 1.7 Analyse des Flux de Données (Thoughts 106-120)

**Objectif** : Tracer les flux de bout en bout (requête → réponse).

**Actions à mener :**
1. **Choisir 3-5 cas d'usage représentatifs** (lecture, écriture, complexe)
2. **Tracer le flux complet** : API → Application → Domain → Infrastructure → DB
3. **Identifier les transformations de données** (DTO → Entity → DTO)
4. **Repérer les points de validation, logging, error handling**
5. **Analyser les transactions et la cohérence** (Unit of Work, ACID, Eventual Consistency)

**Questions à se poser :**
- Comment une requête HTTP est traitée de bout en bout ?
- Quelles sont les transformations de données à chaque couche ?
- Où sont gérées les erreurs (try/catch, middleware, result pattern) ?
- Comment la cohérence transactionnelle est garantie ?

**Livrables de cette phase :**
- Diagrammes de séquence pour 3-5 cas d'usage représentatifs
- Identification des points de transformation de données
- Liste des mécanismes de gestion d'erreurs
- **Validation** : Vérifier conformité avec ADR-067 (Manual Object Mapping - pas d'AutoMapper)
- **Validation** : Vérifier conformité avec ADR-031 (Global Exception Handling Middleware)
- **Validation** : Vérifier conformité avec ADR-070 (ExecutionResult Pattern)

---

## 📋 Phase 2 : Planification des Diagrammes (Sequential Thinking OBLIGATOIRE)

**OBLIGATOIRE** : Utiliser `sequential thinking` pour planifier les diagrammes avant création.

**CRITIQUE** : Chaque diagramme planifié DOIT être validé contre les ADR identifiés en Phase 0.

### 2.1 Structuration des Pages Draw.io (Thoughts 121-130)

**Objectif** : Définir la structure hiérarchique des pages du fichier Draw.io unique.

**Règles de structuration :**
1. **Une page = Un niveau d'abstraction** (pas de mélange HLD/LLD sur une même page)
2. **Une page = Un public cible** (novice, intermédiaire, expert)
3. **Une page = Un type de diagramme** (flux, séquence, dépendances, architecture)
4. **Maximum 15-20 pages** (éviter la surcharge cognitive)

**Structure recommandée (à adapter selon la solution) :**

#### Niveau 1 : Vue d'Ensemble (Public : Novice + Intermédiaire)
- **Page 1 : Context Diagram (C4 Level 1)**
  - Système dans son environnement
  - Acteurs externes (utilisateurs, systèmes externes)
  - Relations de haut niveau

- **Page 2 : Container Diagram (C4 Level 2)**
  - Découpage en conteneurs (API, WebApp, Database, Services)
  - Technologies principales
  - Protocoles de communication

#### Niveau 2 : Architecture Logicielle (Public : Intermédiaire + Expert)
- **Page 3 : Architecture en Couches (Layered Architecture)**
  - Présentation des couches (Presentation, Application, Domain, Infrastructure)
  - Dépendances entre couches
  - Principe de dépendance unidirectionnelle

- **Page 4 : Architecture Onion/Clean/Hexagonal (si applicable)**
  - Noyau Domain au centre
  - Couches concentriques (Application, Infrastructure, Presentation)
  - Ports et Adapters (si Hexagonal)

- **Page 5 : Découpage Fonctionnel (Vertical Slices - si applicable)**
  - Organisation par fonctionnalités
  - Tranches verticales traversant toutes les couches
  - Découplage entre features

#### Niveau 3 : Domaine Métier (Public : Expert + Développeurs)
- **Page 6 : Diagramme de Domaine (Entités + Agrégats)**
  - Toutes les entités métier
  - Relations entre entités (associations, compositions, héritages)
  - Agrégats et leurs frontières (si DDD)

- **Page 7 : Bounded Contexts (si DDD applicable)**
  - Identification des contextes bornés
  - Relations entre contextes (Shared Kernel, Customer/Supplier, etc.)
  - Ubiquitous Language par contexte

#### Niveau 4 : Couche Application (Public : Expert + Développeurs)
- **Page 8 : CQRS Pattern (si applicable)**
  - Séparation Commands vs Queries
  - Handlers pour chaque commande/requête
  - Médiateur (MediatR, EventBus)

- **Page 9 : Cas d'Usage (Use Cases)**
  - Liste exhaustive des cas d'usage
  - Regroupement par fonctionnalité
  - Dépendances entre use cases

#### Niveau 5 : Dépendances et Techniques (Public : Expert + DevOps)
- **Page 10 : Graphe de Dépendances (Projets/Modules)**
  - Tous les projets/modules
  - Dépendances entre projets (flèches unidirectionnelles)
  - Identification des couches (couleurs)

- **Page 11 : Dépendances Externes (NuGet/npm/Maven)**
  - Packages externes utilisés
  - Versions majeures
  - Identification des dépendances critiques

- **Page 12 : Infrastructure Technique**
  - Bases de données (schémas, tables principales)
  - Services externes (APIs, Message Queues, Cache)
  - Configuration et secrets

#### Niveau 6 : Flux et Séquences (Public : Expert + Développeurs)
- **Page 13 : Flux de Lecture (Query Flow)**
  - Diagramme de séquence : API → Application → Domain → Infrastructure → DB
  - Exemple concret de requête en lecture
  - Transformations de données (DTO → Entity → DTO)

- **Page 14 : Flux d'Écriture (Command Flow)**
  - Diagramme de séquence : API → Application → Domain → Infrastructure → DB
  - Exemple concret de commande en écriture
  - Gestion de la transaction et de la cohérence

- **Page 15 : Flux Complexe (si applicable)**
  - Diagramme de séquence pour un cas d'usage complexe
  - Orchestration de plusieurs services
  - Gestion d'erreurs, retries, compensations

#### Niveau 7 : Patterns et Détails (Public : Expert)
- **Page 16 : Patterns de Conception Identifiés**
  - Repository Pattern
  - Factory Pattern
  - Strategy Pattern
  - Decorator Pattern
  - (Liste tous les patterns détectés dans le code)

- **Page 17 : Gestion des Erreurs et Résilience**
  - Try/Catch vs Result Pattern
  - Error Handling Middleware
  - Retry Policies, Circuit Breaker
  - Logging et Monitoring

- **Page 18 : Authentification et Autorisation**
  - Mécanisme d'authentification (JWT, Cookies, OAuth)
  - Gestion des rôles et permissions
  - Flux d'authentification (login, refresh token)

#### Niveau 8 : Déploiement et Runtime (Public : DevOps + Ops)
- **Page 19 : Architecture de Déploiement**
  - Environnements (Dev, Test, Staging, Prod)
  - Infrastructure (VM, Containers, Cloud)
  - CI/CD Pipeline (si identifiable dans le code)

- **Page 20 : Observabilité et Monitoring**
  - Logging (Serilog, NLog, Log4j, etc.)
  - Metrics (Prometheus, Application Insights, etc.)
  - Tracing (OpenTelemetry, Jaeger, etc.)

**Livrables de cette phase :**
- Structure complète des pages Draw.io (titres, objectifs, public cible)
- Liste des diagrammes à créer par page
- Priorisation des pages (critiques en premier)
- **Mapping ADR → Pages** : Pour chaque page, lister les ADR référencés

**PRIORISATION PRAGMATIQUE (Ordre de création recommandé)** :
```
PRIORITÉ CRITIQUE (créer en premier) :
  1. Page 3 - Architecture en Couches (ADR-020, ADR-027) → Fondamental
  2. Page 10 - Graphe de Dépendances (ADR-020, ADR-022, ADR-023) → Vérification conformité
  3. Page 8 - CQRS Pattern (ADR-021, ADR-066) → Pattern central

PRIORITÉ HAUTE :
  4. Page 13 - Flux de Lecture (ADR-021, ADR-025, ADR-067)
  5. Page 14 - Flux d'Écriture (ADR-021, ADR-025, ADR-070)
  6. Page 6 - Diagramme de Domaine (ADR-028)

PRIORITÉ MOYENNE :
  7-12. Pages techniques (Infrastructure, Auth, Observabilité)

PRIORITÉ BASSE (si temps disponible) :
  13-20. Pages de contexte et détails secondaires
```

**CRITÈRES DE VALEUR PAR PAGE** :
- **Valeur Pédagogique** : Aide à comprendre l'architecture (0-10)
- **Valeur Technique** : Utile pour développeurs (0-10)
- **Effort de Création** : Temps estimé (Faible/Moyen/Élevé)
- **Couverture ADR** : Nombre d'ADR référencés
  ```
  Page 3 (Architecture en Couches) → ADR-020, ADR-027
  Page 4 (Onion/Clean) → ADR-020, ADR-022
  Page 8 (CQRS) → ADR-021, ADR-066
  Page 10 (Graphe Dépendances) → ADR-020, ADR-022, ADR-023
  ...
  ```
- **Validation** : Vérifier qu'AUCUN diagramme ne contredit un ADR existant

### 2.2 Identification des Éléments Visuels (Thoughts 131-140)

**Objectif** : Définir les éléments visuels (rectangles, flèches, groupes) pour chaque diagramme.

**Palette visuelle standardisée (à utiliser systématiquement) :**

#### Formes
- **Rectangle arrondi** : Composant, Service, Module, Projet
- **Rectangle** : Entité, Classe, Table
- **Cylindre** : Base de données, Stockage persistant
- **Nuage** : Service externe, API tierce, Cloud
- **Acteur (bonhomme)** : Utilisateur, Système externe actif
- **Losange** : Décision, Condition (dans flux)
- **Cercle** : Événement, Point d'entrée/sortie

#### Couleurs (Palette cohérente)
- **Bleu clair** (#E3F2FD) : Couche Presentation / UI
- **Vert clair** (#E8F5E9) : Couche Application / Use Cases
- **Jaune clair** (#FFF9C4) : Couche Domain / Core
- **Orange clair** (#FFE0B2) : Couche Infrastructure / Adapters
- **Gris clair** (#ECEFF1) : Services externes, dépendances tierces
- **Rouge clair** (#FFCDD2) : Erreurs, alertes, violations

#### Flèches
- **Flèche pleine** (→) : Dépendance forte, appel synchrone
- **Flèche pointillée** (⇢) : Dépendance faible, appel asynchrone
- **Flèche bidirectionnelle** (↔) : Communication bidirectionnelle
- **Flèche avec label** : Préciser le type de relation (implements, uses, calls)

#### Groupes et Conteneurs
- **Rectangle avec bordure épaisse** : Couche architecturale, Bounded Context
- **Rectangle avec fond transparent** : Namespace, Package, Module logique
- **Zone colorée** : Regroupement visuel (ex: tous les repositories ensemble)

**Livrables de cette phase :**
- Palette visuelle définie (formes, couleurs, flèches)
- Conventions de représentation documentées
- Exemples de représentation pour chaque type d'élément
- **Légende ADR** : Convention pour annoter les éléments avec références ADR
  - Format : `[ADR-XXX]` en annotation ou note
  - Exemple : Rectangle "Domain Layer" avec note "Conformité: ADR-020, ADR-022"

---

## 📋 Phase 3 : Génération du Fichier Draw.io (Execution)

**RÈGLE CRITIQUE** : Chaque élément architectural généré DOIT être traçable vers :
1. Le code source (fichiers, classes, méthodes)
2. L'ADR justificatif (si applicable)

**Annotations obligatoires dans les diagrammes :**
- Titre de page avec liste des ADR couverts (ex: "Page 8: CQRS Pattern [ADR-021, ADR-066]")
- Notes explicatives référençant les ADR (ex: "Séparation Commands/Queries imposée par ADR-021")
- Annotations sur éléments critiques (ex: Flèche de dépendance avec "Conforme ADR-020")

### 3.1 Création du Fichier Draw.io Unique

**IMPORTANT** : Le fichier Draw.io est un XML structuré. Utiliser le format suivant :

```xml
<mxfile host="app.diagrams.net" modified="2025-12-01T20:00:00.000Z" agent="AI Architecture Generator" version="24.0.0" type="device" pages="20">
  <diagram id="page1" name="1. Context Diagram (C4)">
    <mxGraphModel dx="1422" dy="794" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1169" pageHeight="827" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />
        <!-- ELEMENTS HERE -->
      </root>
    </mxGraphModel>
  </diagram>
  <diagram id="page2" name="2. Container Diagram (C4)">
    <!-- ... -->
  </diagram>
  <!-- ... autres pages ... -->
</mxfile>
```

**Règles de génération XML :**
1. **Un fichier unique** avec attribut `pages="N"` où N = nombre de pages
2. **Une balise `<diagram>` par page** avec `id` unique et `name` descriptif
3. **Format de page** : `pageWidth="1169"` `pageHeight="827"` (A4 paysage, standard Draw.io)
4. **Grid activé** : `grid="1" gridSize="10"` pour alignement propre
5. **Namespace mxGraph** : Respecter la structure `<mxGraphModel><root><mxCell>...</mxCell></root></mxGraphModel>`

### 3.2 Génération des Pages HLD (High-Level Design)

**Objectif** : Créer les pages de vue d'ensemble (Pages 1-5).

**Instructions détaillées par page :**

#### Page 1 : Context Diagram (C4 Level 1)
- **ADR couverts** : ADR-027 (Bootstrapping/Endpoint Separation), ADR-032 (Authentication)
- **Éléments à représenter :**
  - Système principal (rectangle bleu, centré, taille ~300x200px)
  - Acteurs externes (bonhomme, 3-6 acteurs positionnés autour)
  - Systèmes externes (nuages gris, 2-4 systèmes)
  - Flèches avec labels (ex: "utilise API REST", "envoie emails via SMTP")
  - **Annotation ADR** : Note en bas de page listant les ADR appliqués

- **Positionnement (éviter superpositions) :**
  - Système principal : Centre (x=500, y=350)
  - Acteurs : Haut-gauche et haut-droite (y=100-150)
  - Systèmes externes : Bas et côtés (y=600-700)
  - Espacement minimal : 150px entre éléments

- **Code XML exemple :**
```xml
<!-- Système principal - Centre de la page -->
<mxCell id="system-main" value="Système GenAI&#xa;(Application Blazor)" style="rounded=1;whiteSpace=wrap;html=1;fillColor=#E3F2FD;strokeColor=#1565C0;fontSize=16;fontStyle=1" vertex="1" parent="1">
  <mxGeometry x="450" y="300" width="300" height="150" as="geometry" />
</mxCell>

<!-- Acteur utilisateur - Haut gauche -->
<mxCell id="user1" value="Utilisateur&#xa;Final" style="shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;html=1;outlineConnect=0;" vertex="1" parent="1">
  <mxGeometry x="200" y="100" width="60" height="120" as="geometry" />
</mxCell>

<!-- Flèche avec label -->
<mxCell id="arrow1" value="Accède via HTTPS" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=0.5;exitY=1;exitDx=0;exitDy=0;entryX=0.25;entryY=0;entryDx=0;entryDy=0;" edge="1" parent="1" source="user1" target="system-main">
  <mxGeometry relative="1" as="geometry" />
</mxCell>

<!-- Note ADR - Bas de page -->
<mxCell id="note-adr" value="Architecture Decision Records appliqués:&#xa;- ADR-027: Bootstrapping/Endpoint Separation&#xa;- ADR-032: Authentication Multi-Schema Entra ID" style="text;html=1;strokeColor=#d6b656;fillColor=#fff2cc;align=left;verticalAlign=top;whiteSpace=wrap;rounded=1;fontSize=11;" vertex="1" parent="1">
  <mxGeometry x="50" y="700" width="400" height="80" as="geometry" />
</mxCell>
```

**ALGORITHME DE CALCUL DES POSITIONS (Grille NxM)** :
```python
PAGE_WIDTH = 1169
PAGE_HEIGHT = 827
MARGIN = 50
SPACING = 50

def calculate_grid_layout(nb_elements, element_width, element_height):
    # Calculer nombre de colonnes optimal
    cols = ceil(sqrt(nb_elements))
    rows = ceil(nb_elements / cols)
    
    # Calculer espacement
    cell_width = (PAGE_WIDTH - 2*MARGIN - (cols-1)*SPACING) / cols
    cell_height = (PAGE_HEIGHT - 2*MARGIN - (rows-1)*SPACING) / rows
    
    positions = []
    for i in range(nb_elements):
        col = i % cols
        row = i // cols
        x = MARGIN + col * (cell_width + SPACING)
        y = MARGIN + row * (cell_height + SPACING)
        positions.append((x, y, cell_width, cell_height))
    
    return positions
```

#### Page 2 : Container Diagram (C4 Level 2)
- **Éléments à représenter :**
  - Conteneurs applicatifs (WebApp, API, Worker - rectangles arrondis)
  - Bases de données (cylindres)
  - Services externes (nuages)
  - Protocoles de communication (labels sur flèches)

- **Positionnement :**
  - Layout en grille : 3 colonnes × 2-3 lignes
  - Espacement : 250px horizontal, 200px vertical
  - Bases de données : Bas de page (y=600-700)

#### Pages 3-5 : Architecture en Couches
- **ADR couverts** : ADR-020 (Clean/Onion), ADR-022 (Hexagonal Ports/Adapters), ADR-027 (Bootstrapping)
- **Représentation en couches horizontales** :
  - Chaque couche = rectangle avec fond coloré + bordure épaisse
  - Hauteur par couche : 150-200px
  - Largeur : 100% de la page (x=50 à x=1100)
  - Espacement vertical : 50px entre couches

**TEMPLATE XML RÉUTILISABLE - COUCHE ARCHITECTURALE** :
```xml
<!-- Couche Presentation (y=50) -->
<mxCell id="layer-presentation" value="" style="rounded=0;whiteSpace=wrap;html=1;fillColor=#E3F2FD;strokeColor=#1565C0;strokeWidth=3;dashed=0;" vertex="1" parent="1">
  <mxGeometry x="50" y="50" width="1069" height="150" as="geometry" />
</mxCell>
<mxCell id="layer-presentation-label" value="Presentation Layer (Hostings)" style="text;html=1;strokeColor=none;fillColor=none;align=left;verticalAlign=top;whiteSpace=wrap;rounded=0;fontSize=18;fontStyle=1;" vertex="1" parent="1">
  <mxGeometry x="70" y="60" width="300" height="30" as="geometry" />
</mxCell>

<!-- Couche Application (y=250) -->
<mxCell id="layer-application" value="" style="rounded=0;whiteSpace=wrap;html=1;fillColor=#E8F5E9;strokeColor=#2E7D32;strokeWidth=3;" vertex="1" parent="1">
  <mxGeometry x="50" y="250" width="1069" height="150" as="geometry" />
</mxCell>
<mxCell id="layer-application-label" value="Application Layer (Handlers, Use Cases)" style="text;html=1;strokeColor=none;fillColor=none;align=left;verticalAlign=top;whiteSpace=wrap;rounded=0;fontSize=18;fontStyle=1;" vertex="1" parent="1">
  <mxGeometry x="70" y="260" width="400" height="30" as="geometry" />
</mxCell>

<!-- Couche Domain (y=450) -->
<mxCell id="layer-domain" value="" style="rounded=0;whiteSpace=wrap;html=1;fillColor=#FFF9C4;strokeColor=#F57F17;strokeWidth=3;" vertex="1" parent="1">
  <mxGeometry x="50" y="450" width="1069" height="150" as="geometry" />
</mxCell>
<mxCell id="layer-domain-label" value="Domain Layer (Entities, Aggregates, DTOs)" style="text;html=1;strokeColor=none;fillColor=none;align=left;verticalAlign=top;whiteSpace=wrap;rounded=0;fontSize=18;fontStyle=1;" vertex="1" parent="1">
  <mxGeometry x="70" y="460" width="450" height="30" as="geometry" />
</mxCell>

<!-- Couche Infrastructure (y=650) -->
<mxCell id="layer-infrastructure" value="" style="rounded=0;whiteSpace=wrap;html=1;fillColor=#FFE0B2;strokeColor=#E65100;strokeWidth=3;" vertex="1" parent="1">
  <mxGeometry x="50" y="650" width="1069" height="150" as="geometry" />
</mxCell>
<mxCell id="layer-infrastructure-label" value="Infrastructure Layer (Repositories, Adapters, EF Core)" style="text;html=1;strokeColor=none;fillColor=none;align=left;verticalAlign=top;whiteSpace=wrap;rounded=0;fontSize=18;fontStyle=1;" vertex="1" parent="1">
  <mxGeometry x="70" y="660" width="500" height="30" as="geometry" />
</mxCell>

<!-- Flèche de dépendance Domain ← Application -->
<mxCell id="dep-app-domain" value="Dépend de" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=0.5;exitY=1;exitDx=0;exitDy=0;entryX=0.5;entryY=0;entryDx=0;entryDy=0;strokeWidth=2;endArrow=block;endFill=0;" edge="1" parent="1" source="layer-application" target="layer-domain">
  <mxGeometry relative="1" as="geometry" />
</mxCell>
<mxCell id="dep-app-domain-note" value="Conforme ADR-020" style="edgeLabel;html=1;align=center;verticalAlign=middle;resizable=0;points=[];fontSize=11;fontColor=#2E7D32;fontStyle=1" vertex="1" connectable="0" parent="dep-app-domain">
  <mxGeometry x="0.1" y="2" relative="1" as="geometry">
    <mxPoint as="offset" />
  </mxGeometry>
</mxCell>
```

- **Contenu par couche :**
  - Liste des composants principaux (5-10 par couche)
  - Flèches descendantes uniquement (respect dépendances)
  - **CRITIQUE - Conformité ADR-020** :
    - ❌ INTERDIRE toute flèche de Domain vers Infrastructure
    - ✅ Domain définit interfaces, Infrastructure les implémente
    - ✅ Annotations sur flèches: "Dépendance vers abstraction (ADR-020)"
  - **Annotations textuelles** :
    - "Règle: Dépendances pointent vers le centre (ADR-020)"
    - "Abstractions dans Domain, Implémentations dans Infrastructure (ADR-022)"

### 3.3 Génération des Pages LLD (Low-Level Design)

**Objectif** : Créer les pages de détail technique (Pages 6-18).

**Instructions spécifiques :**

#### Pages 6-7 : Diagrammes de Domaine
- **Type de diagramme** : Diagramme de classes UML simplifié
- **Éléments par entité** :
  - Nom de l'entité (gras, taille 14)
  - Propriétés principales (3-5 max, éviter surcharge)
  - Relations (associations, compositions, héritages)

- **Positionnement automatique** :
  - Algorithme : Regrouper les entités par agrégat
  - Entités principales (Aggregate Roots) : Plus grandes, centrées
  - Entités enfants : Plus petites, autour du root
  - Espacement minimal : 100px entre rectangles

#### Page 10 : Graphe de Dépendances
- **ADR couverts** : ADR-020 (règle dépendance), ADR-022 (Hexagonal), ADR-023 (Centralized Package Management)
- **Type de diagramme** : Graphe orienté (DAG si pas de cycles)
- **Algorithme de layout** : Topological Sort + Layering
  - Layer 0 (racines) : Projets sans dépendances sortantes (Domain, Core)
  - Layer 1 : Projets dépendant uniquement de Layer 0 (Application)
  - Layer N : Projets dépendant de Layer N-1
  - Affichage : Layers empilés verticalement, espacement 250px

- **Représentation visuelle** :
  - Rectangles avec couleur selon couche (voir palette)
  - Flèches noires épaisses pour dépendances conformes
  - Flèches rouges pointillées pour violations (si détectées)
  - **VALIDATION STRICTE - Conformité ADR-020** :
    - ✅ Vérifier que Domain ne dépend de RIEN (autonome)
    - ✅ Vérifier que Infrastructure dépend de Domain.Abstractions uniquement
    - ✅ Vérifier que Hostings dépend de Infrastructure et Domain
    - ❌ Si violation détectée : Flèche ROUGE avec label "VIOLATION ADR-020"
  - **Légende obligatoire** :
    - "Flèche noire : Dépendance conforme ADR-020"
    - "Flèche rouge : Violation architecturale à corriger"

#### Pages 13-15 : Diagrammes de Séquence
- **ADR couverts** : ADR-021 (CQRS), ADR-025 (Repository), ADR-067 (Manual Mapping), ADR-070 (ExecutionResult)
- **Format** : UML Sequence Diagram standard
- **Éléments** :
  - Acteurs/Participants : En haut, espacés horizontalement (200px)
  - Lifelines : Lignes verticales pointillées
  - Messages : Flèches horizontales avec labels
  - Activation boxes : Rectangles sur lifelines
  - Retours : Flèches pointillées

- **Positionnement** :
  - Participants : y=50, x=100, 300, 500, 700, 900
  - Messages : Espacés verticalement de 80px
  - Largeur flèche : Ajuster selon distance entre participants

- **CONFORMITÉ ADR-021 (CQRS)** :
  - **Page 13 (Query)** : 
    - Participant "QueryHandler" (lecture seule)
    - Message "Send(Query)" via Mediator
    - Retour "DTO" (pas d'entité Domain)
    - Annotation : "Pattern CQRS - Query Flow (ADR-021)"
  - **Page 14 (Command)** : 
    - Participant "CommandHandler" (écriture)
    - Message "Send(Command)" via Mediator
    - Retour "ExecutionResult" (ADR-070)
    - Annotation : "Pattern CQRS - Command Flow (ADR-021)"

- **Annotations sur messages** :
  - "IRequestHandlerAdapter abstraction (ADR-021)"
  - "Mapping manuel DTO ↔ Entity (ADR-067)"
  - "Repository pattern (ADR-025)"

**TEMPLATE XML RÉUTILISABLE - DIAGRAMME DE SÉQUENCE CQRS** :
```xml
<!-- Page 13: Query Flow (Lecture) -->

<!-- Participants (espacés de 200px horizontalement) -->
<mxCell id="participant-controller" value="API Controller" style="shape=umlLifeline;perimeter=lifelinePerimeter;whiteSpace=wrap;html=1;container=1;collapsible=0;recursiveResize=0;outlineConnect=0;" vertex="1" parent="1">
  <mxGeometry x="100" y="50" width="120" height="600" as="geometry" />
</mxCell>

<mxCell id="participant-mediator" value="Mediator" style="shape=umlLifeline;perimeter=lifelinePerimeter;whiteSpace=wrap;html=1;container=1;collapsible=0;recursiveResize=0;outlineConnect=0;" vertex="1" parent="1">
  <mxGeometry x="300" y="50" width="120" height="600" as="geometry" />
</mxCell>

<mxCell id="participant-handler" value="QueryHandler" style="shape=umlLifeline;perimeter=lifelinePerimeter;whiteSpace=wrap;html=1;container=1;collapsible=0;recursiveResize=0;outlineConnect=0;" vertex="1" parent="1">
  <mxGeometry x="500" y="50" width="120" height="600" as="geometry" />
</mxCell>

<mxCell id="participant-repo" value="Repository" style="shape=umlLifeline;perimeter=lifelinePerimeter;whiteSpace=wrap;html=1;container=1;collapsible=0;recursiveResize=0;outlineConnect=0;" vertex="1" parent="1">
  <mxGeometry x="700" y="50" width="120" height="600" as="geometry" />
</mxCell>

<mxCell id="participant-db" value="Database" style="shape=umlLifeline;perimeter=lifelinePerimeter;whiteSpace=wrap;html=1;container=1;collapsible=0;recursiveResize=0;outlineConnect=0;" vertex="1" parent="1">
  <mxGeometry x="900" y="50" width="120" height="600" as="geometry" />
</mxCell>

<!-- Message 1: Controller → Mediator (y=150) -->
<mxCell id="msg1" value="Send(GetPromptByIdQuery)" style="html=1;verticalAlign=bottom;endArrow=block;rounded=0;exitX=1;exitY=0.15;exitDx=0;exitDy=0;entryX=0;entryY=0.15;entryDx=0;entryDy=0;" edge="1" parent="1" source="participant-controller" target="participant-mediator">
  <mxGeometry width="80" relative="1" as="geometry">
    <mxPoint x="220" y="150" as="sourcePoint" />
    <mxPoint x="300" y="150" as="targetPoint" />
  </mxGeometry>
</mxCell>
<mxCell id="msg1-note" value="ADR-021: CQRS Query" style="shape=note;whiteSpace=wrap;html=1;backgroundOutline=1;darkOpacity=0.05;fillColor=#fff2cc;strokeColor=#d6b656;fontSize=10;" vertex="1" parent="1">
  <mxGeometry x="220" y="120" width="100" height="40" as="geometry" />
</mxCell>

<!-- Message 2: Mediator → Handler (y=230) -->
<mxCell id="msg2" value="Handle(query)" style="html=1;verticalAlign=bottom;endArrow=block;rounded=0;" edge="1" parent="1" source="participant-mediator" target="participant-handler">
  <mxGeometry width="80" relative="1" as="geometry">
    <mxPoint x="420" y="230" as="sourcePoint" />
    <mxPoint x="500" y="230" as="targetPoint" />
  </mxGeometry>
</mxCell>

<!-- Message 3: Handler → Repository (y=310) -->
<mxCell id="msg3" value="GetByIdAsync(id)" style="html=1;verticalAlign=bottom;endArrow=block;rounded=0;" edge="1" parent="1" source="participant-handler" target="participant-repo">
  <mxGeometry width="80" relative="1" as="geometry">
    <mxPoint x="620" y="310" as="sourcePoint" />
    <mxPoint x="700" y="310" as="targetPoint" />
  </mxGeometry>
</mxCell>
<mxCell id="msg3-note" value="ADR-025: Repository Pattern" style="shape=note;whiteSpace=wrap;html=1;backgroundOutline=1;darkOpacity=0.05;fillColor=#dae8fc;strokeColor=#6c8ebf;fontSize=10;" vertex="1" parent="1">
  <mxGeometry x="620" y="280" width="120" height="40" as="geometry" />
</mxCell>

<!-- Message 4: Repository → Database (y=390) -->
<mxCell id="msg4" value="SELECT * FROM prompts WHERE id = @id" style="html=1;verticalAlign=bottom;endArrow=block;rounded=0;" edge="1" parent="1" source="participant-repo" target="participant-db">
  <mxGeometry width="80" relative="1" as="geometry">
    <mxPoint x="820" y="390" as="sourcePoint" />
    <mxPoint x="900" y="390" as="targetPoint" />
  </mxGeometry>
</mxCell>

<!-- Return 4: Database → Repository (y=450, dashed) -->
<mxCell id="return4" value="Entity" style="html=1;verticalAlign=bottom;endArrow=open;dashed=1;endSize=8;rounded=0;" edge="1" parent="1" source="participant-db" target="participant-repo">
  <mxGeometry relative="1" as="geometry">
    <mxPoint x="900" y="450" as="sourcePoint" />
    <mxPoint x="820" y="450" as="targetPoint" />
  </mxGeometry>
</mxCell>

<!-- Return 3: Repository → Handler (y=510, dashed) -->
<mxCell id="return3" value="Prompt entity" style="html=1;verticalAlign=bottom;endArrow=open;dashed=1;endSize=8;rounded=0;" edge="1" parent="1" source="participant-repo" target="participant-handler">
  <mxGeometry relative="1" as="geometry">
    <mxPoint x="700" y="510" as="sourcePoint" />
    <mxPoint x="620" y="510" as="targetPoint" />
  </mxGeometry>
</mxCell>

<!-- Mapping Entity → DTO (activation box inside Handler) -->
<mxCell id="mapping" value="Map Entity → DTO (manuel)" style="html=1;points=[];perimeter=orthogonalPerimeter;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="participant-handler">
  <mxGeometry x="55" y="480" width="10" height="40" as="geometry" />
</mxCell>
<mxCell id="mapping-note" value="ADR-067: No AutoMapper" style="shape=note;whiteSpace=wrap;html=1;backgroundOutline=1;darkOpacity=0.05;fillColor=#fff2cc;strokeColor=#d6b656;fontSize=10;" vertex="1" parent="1">
  <mxGeometry x="420" y="520" width="120" height="40" as="geometry" />
</mxCell>

<!-- Return 2: Handler → Mediator (y=570, dashed) -->
<mxCell id="return2" value="GetPromptByIdResult (DTO)" style="html=1;verticalAlign=bottom;endArrow=open;dashed=1;endSize=8;rounded=0;" edge="1" parent="1" source="participant-handler" target="participant-mediator">
  <mxGeometry relative="1" as="geometry">
    <mxPoint x="500" y="570" as="sourcePoint" />
    <mxPoint x="420" y="570" as="targetPoint" />
  </mxGeometry>
</mxCell>

<!-- Return 1: Mediator → Controller (y=630, dashed) -->
<mxCell id="return1" value="DTO Result" style="html=1;verticalAlign=bottom;endArrow=open;dashed=1;endSize=8;rounded=0;" edge="1" parent="1" source="participant-mediator" target="participant-controller">
  <mxGeometry relative="1" as="geometry">
    <mxPoint x="300" y="630" as="sourcePoint" />
    <mxPoint x="220" y="630" as="targetPoint" />
  </mxGeometry>
</mxCell>

<!-- Légende ADR en bas -->
<mxCell id="legend-adr" value="Architecture Decision Records appliqués:&#xa;- ADR-021: CQRS Pattern (Query Flow - lecture seule)&#xa;- ADR-025: Repository Pattern avec EF Core&#xa;- ADR-067: Manual Object Mapping (pas d'AutoMapper)" style="text;html=1;strokeColor=#d6b656;fillColor=#fff2cc;align=left;verticalAlign=top;whiteSpace=wrap;rounded=1;fontSize=11;" vertex="1" parent="1">
  <mxGeometry x="100" y="700" width="500" height="80" as="geometry" />
</mxCell>
```

**FORMULE DE CALCUL DES POSITIONS (Séquence Diagram)** :
```python
# Constantes
PARTICIPANT_WIDTH = 120
PARTICIPANT_SPACING = 200  # Espacement horizontal entre participants
MESSAGE_VERTICAL_SPACING = 80  # Espacement vertical entre messages
START_Y = 50  # Y initial pour les participants
START_X = 100  # X initial pour premier participant

# Calcul positions participants
for i in range(nb_participants):
    x = START_X + i * PARTICIPANT_SPACING
    participant_positions[i] = (x, START_Y, PARTICIPANT_WIDTH, 600)

# Calcul positions messages
current_y = START_Y + 100  # Commencer 100px en dessous des participants
for message in messages:
    message.y = current_y
    current_y += MESSAGE_VERTICAL_SPACING
```

### 3.4 Optimisation Anti-Superposition

**OBLIGATOIRE** : Appliquer ces règles pour éviter les superpositions.

#### Règles Globales
1. **Grille de 10px** : Tous les éléments alignés sur grille (`gridSize="10"`)
2. **Espacement minimal** : 50px entre éléments pour aération visuelle
3. **Marges de page** : 50px depuis les bords (x=50, y=50 minimum)
4. **Taille de page** : 1169×827 (A4 paysage) - ne pas dépasser

#### Algorithme de Positionnement
1. **Calcul des dimensions** : Compter le nombre d'éléments à représenter
2. **Choix du layout** :
   - Si < 10 éléments : Layout manuel optimisé
   - Si 10-30 éléments : Grille NxM (ex: 5×6)
   - Si > 30 éléments : Layout hiérarchique (Tree ou Force-Directed)

3. **Calcul des positions** :
   ```
   Pour une grille NxM :
   - largeur_cellule = (pageWidth - 100) / N
   - hauteur_cellule = (pageHeight - 100) / M
   - x[i] = 50 + (i % N) * largeur_cellule + padding
   - y[i] = 50 + (i / N) * hauteur_cellule + padding
   ```

4. **Validation** : Vérifier qu'aucun élément ne chevauche un autre
   - Si chevauchement détecté : Ajuster espacement ou réduire taille éléments

#### Gestion des Flèches
- **Routing orthogonal** : `edgeStyle=orthogonalEdgeStyle` (angles droits)
- **Points de contrôle** : Ajouter `<mxPoint>` intermédiaires si nécessaire
- **Éviter croisements** : Privilégier les chemins sans intersection

### 3.5 Finalisation et Validation

**Actions finales :**
1. **Vérification de la structure XML** : Valider le XML avec parser
2. **Test d'ouverture** : Le fichier doit s'ouvrir dans Draw.io sans erreur
3. **Validation visuelle** : Aucune superposition, alignement propre
4. **Nomenclature** : Fichier nommé `architecture-solution.drawio`
5. **Emplacement** : Sauvegarder dans `docs/schematics/architecture-solution.drawio`

---

## 📋 Phase 4 : Documentation Complémentaire (Optionnel)

### 4.1 Fichier README.md pour docs/schematics/

**Créer un fichier `docs/schematics/README.md`** avec le contenu suivant :

```markdown
# Documentation Architecturale Visuelle

Ce dossier contient les schémas architecturaux de la solution sous format Draw.io.

## 📄 Fichier Principal

- **`architecture-solution.drawio`** : Documentation complète multi-pages

## 📑 Pages Disponibles

### Niveau 1 : Vue d'Ensemble (Novice + Intermédiaire)
1. **Context Diagram (C4 Level 1)** : Système dans son environnement
2. **Container Diagram (C4 Level 2)** : Découpage en conteneurs

### Niveau 2 : Architecture Logicielle (Intermédiaire + Expert)
3. **Architecture en Couches** : Présentation/Application/Domain/Infrastructure
4. **Architecture Onion/Clean** : Couches concentriques avec Domain au centre
5. **Découpage Fonctionnel** : Vertical Slices (si applicable)

### Niveau 3 : Domaine Métier (Expert)
6. **Diagramme de Domaine** : Entités, Agrégats, Relations
7. **Bounded Contexts** : Contextes bornés DDD (si applicable)

### Niveau 4 : Couche Application (Expert)
8. **CQRS Pattern** : Commands vs Queries (si applicable)
9. **Cas d'Usage** : Liste exhaustive des use cases

### Niveau 5 : Dépendances et Techniques (Expert + DevOps)
10. **Graphe de Dépendances** : Dépendances entre projets/modules
11. **Dépendances Externes** : NuGet/npm/Maven packages
12. **Infrastructure Technique** : Bases de données, services externes

### Niveau 6 : Flux et Séquences (Expert)
13. **Flux de Lecture** : Diagramme de séquence Query Flow
14. **Flux d'Écriture** : Diagramme de séquence Command Flow
15. **Flux Complexe** : Cas d'usage complexe (si applicable)

### Niveau 7 : Patterns et Détails (Expert)
16. **Patterns de Conception** : Repository, Factory, Strategy, etc.
17. **Gestion des Erreurs** : Error Handling, Retry, Circuit Breaker
18. **Authentification/Autorisation** : Mécanismes de sécurité

### Niveau 8 : Déploiement et Runtime (DevOps)
19. **Architecture de Déploiement** : Environnements, Infrastructure, CI/CD
20. **Observabilité** : Logging, Metrics, Tracing

## 🛠️ Outils

- **Éditeur recommandé** : [Draw.io Desktop](https://github.com/jgraph/drawio-desktop/releases)
- **Éditeur en ligne** : [app.diagrams.net](https://app.diagrams.net/)
- **VS Code Extension** : [Draw.io Integration](https://marketplace.visualstudio.com/items?itemName=hediet.vscode-drawio)

## 📖 Utilisation

1. Ouvrir `architecture-solution.drawio` avec Draw.io
2. Naviguer entre les pages via l'onglet en bas de l'interface
3. Commencer par les pages 1-2 pour une vue d'ensemble
4. Approfondir avec les pages 6-18 pour les détails techniques

## 🔄 Mise à Jour

Ce fichier est généré automatiquement via l'agent `generate-architecture-drawio`.
Pour régénérer : `/generate-architecture-drawio`
```

### 4.2 Script de Validation Automatisée (PowerShell)

**Créer un fichier `docs/schematics/validate-architecture.ps1`** pour valider la conformité ADR :

```powershell
<#
.SYNOPSIS
Validation automatique de la conformité ADR-020 (Architecture Clean/Onion)

.DESCRIPTION
Analyse les dépendances entre projets et détecte les violations architecturales.
#>

$ErrorActionPreference = "Stop"
$solutionPath = "$PSScriptRoot\..\..\GroupeAdp.Genai.sln"
$violations = @()

# Fonction de parsing des .csproj
function Get-ProjectReferences {
    param([string]$csprojPath)
    
    [xml]$csproj = Get-Content $csprojPath
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($csprojPath)
    
    $references = $csproj.Project.ItemGroup.ProjectReference.Include | ForEach-Object {
        [System.IO.Path]::GetFileNameWithoutExtension($_)
    }
    
    return @{
        Name = $projectName
        References = $references
    }
}

# Analyser tous les projets
$projects = Get-ChildItem -Path "$PSScriptRoot\..\.." -Filter "*.csproj" -Recurse | ForEach-Object {
    Get-ProjectReferences $_.FullName
}

# RÈGLE ADR-020: Domain ne doit dépendre de RIEN
foreach ($project in $projects) {
    if ($project.Name -match "Domain" -and $project.References.Count -gt 0) {
        $violations += "❌ VIOLATION ADR-020: '$($project.Name)' dépend de '$($project.References -join ', ')'"
    }
}

# RÈGLE ADR-020: Infrastructure dépend uniquement de Domain.Abstractions
foreach ($project in $projects) {
    if ($project.Name -match "Infrastructure") {
        foreach ($ref in $project.References) {
            if ($ref -notmatch "Domain" -and $ref -notmatch "Common" -and $ref -notmatch "Abstraction") {
                $violations += "⚠️  WARNING ADR-020: '$($project.Name)' dépend de '$ref' (vérifier si correct)"
            }
        }
    }
}

# RÈGLE ADR-022: Domain ne doit pas référencer EF Core, MediatR
foreach ($project in $projects) {
    if ($project.Name -match "Domain") {
        $csprojPath = Get-ChildItem -Path "$PSScriptRoot\..\.." -Filter "$($project.Name).csproj" -Recurse | Select-Object -First 1
        [xml]$csproj = Get-Content $csprojPath.FullName
        
        $packages = $csproj.Project.ItemGroup.PackageReference.Include
        foreach ($pkg in $packages) {
            if ($pkg -match "EntityFrameworkCore" -or $pkg -match "MediatR" -or $pkg -match "Npgsql") {
                $violations += "❌ VIOLATION ADR-022: '$($project.Name)' référence package externe '$pkg'"
            }
        }
    }
}

# Affichage des résultats
Write-Host "\n=== VALIDATION ADR-020/022 (Architecture Clean/Onion) ===\n" -ForegroundColor Cyan

if ($violations.Count -eq 0) {
    Write-Host "✅ Aucune violation détectée - Architecture conforme aux ADR" -ForegroundColor Green
} else {
    Write-Host "⚠️  $($violations.Count) violation(s) détectée(s):\n" -ForegroundColor Yellow
    $violations | ForEach-Object { Write-Host $_ }
    exit 1
}
```

### 4.3 Fichier Index des Diagrammes

**Créer un fichier `docs/schematics/DIAGRAMS_INDEX.md`** avec une table des matières détaillée :

```markdown
# Index des Diagrammes Architecturaux

| # | Nom de la Page | Type de Diagramme | Public Cible | Concepts Clés |
|---|----------------|-------------------|--------------|---------------|
| 1 | Context Diagram (C4 Level 1) | C4 Context | Novice, Manager | Système, Acteurs, Systèmes externes |
| 2 | Container Diagram (C4 Level 2) | C4 Container | Intermédiaire | WebApp, API, Database, Protocoles |
| ... | ... | ... | ... | ... |

## Navigation Rapide

### Pour les Novices
- Commencer par : Pages 1, 2, 3
- Objectif : Comprendre le système dans sa globalité

### Pour les Développeurs
- Commencer par : Pages 3, 6, 9, 13, 14
- Objectif : Comprendre l'architecture et les flux de données

### Pour les Architectes
- Commencer par : Pages 4, 7, 10, 16, 17
- Objectif : Analyser les patterns et les décisions architecturales

### Pour les DevOps
- Commencer par : Pages 12, 19, 20
- Objectif : Comprendre l'infrastructure et l'observabilité
```

---

## ✅ Checklist de Validation Finale

**VÉRIFIER avant de considérer la tâche terminée :**

### Exhaustivité de l'Analyse
- [ ] Tous les répertoires/projets ont été analysés
- [ ] Toutes les dépendances ont été cartographiées
- [ ] Tous les patterns architecturaux ont été identifiés
- [ ] Tous les flux de données ont été tracés
- [ ] **Phase 0 complétée** : Tous les ADR ont été lus et indexés

### Factualité des Diagrammes
- [ ] Aucun élément inventé - tout provient du code source
- [ ] Les noms de composants/classes correspondent exactement au code
- [ ] Les dépendances respectent les imports/références réels
- [ ] Les flux de données correspondent aux implémentations réelles

### Conformité ADR (CRITIQUE)
- [ ] **Chaque ADR a été vérifié** dans le code source (pas seulement lu)
- [ ] **Aucun diagramme ne contredit un ADR** existant
- [ ] **Toutes les règles INTERDITES** sont respectées (ex: Domain ≠> Infrastructure)
- [ ] **Toutes les règles OBLIGATOIRES** sont appliquées (ex: Abstractions dans Domain)
- [ ] **Chaque page référence les ADR applicables** (annotations, notes)
- [ ] **Les violations détectées** sont marquées en ROUGE avec label "VIOLATION ADR-XXX"
- [ ] **Mapping ADR → Code source** : Chaque décision ADR est vérifiable dans le code
- [ ] **ADR dépréciés/remplacés** : Ignorés, seuls les ADR "Accepté" sont pris en compte

### Validation Automatisée
- [ ] **Script PowerShell** `validate-architecture.ps1` exécuté avec succès (0 violations)
- [ ] **Fichier Draw.io** s'ouvre sans erreur dans Draw.io Desktop/Web
- [ ] **XML valide** : Pas d'erreurs de parsing
- [ ] **Métriques calculées** : Toutes les métriques quantitatives renseignées

### Génération Incrémentale (Si contrainte de temps)
- [ ] **Minimum Viable Documentation** : Pages 3, 8, 10 créées (Priorité CRITIQUE)
- [ ] **Documentation complète** : 20 pages créées (Priorité CRITIQUE + HAUTE + MOYENNE)
- [ ] **Version évolutive** : Possibilité d'ajouter pages supplémentaires ultérieurement

### Qualité Visuelle
- [ ] Aucune superposition d'éléments
- [ ] Alignement propre sur la grille (10px)
- [ ] Espacement cohérent (minimum 50px)
- [ ] Palette de couleurs respectée
- [ ] Lisibilité optimale (tailles de police, contrastes)

### Structure du Fichier Draw.io
- [ ] Fichier XML valide (peut être parsé)
- [ ] Structure multi-pages fonctionnelle (20 pages max)
- [ ] Nommage des pages clair et hiérarchisé
- [ ] Fichier s'ouvre sans erreur dans Draw.io

### Documentation Complémentaire
- [ ] `README.md` créé dans `docs/schematics/`
- [ ] `DIAGRAMS_INDEX.md` créé avec table des matières
- [ ] Instructions d'utilisation claires

### Conformité aux Instructions
- [ ] Sequential Thinking utilisé pour toutes les phases d'analyse
- [ ] Toutes les phases (1 à 4) ont été complétées
- [ ] Les 12 piliers de qualité ont été respectés (Maintenabilité, Simplicité, Précision, etc.)
- [ ] La posture d'Architecte Logiciel a été maintenue

---

## 🚀 Exécution

**Commande pour démarrer :** `/generate-architecture-drawio`

**Durée estimée :** 60-120 minutes selon la taille de la solution

**Livrables finaux :**
1. `docs/schematics/architecture-solution.drawio` (fichier Draw.io multi-pages)
2. `docs/schematics/README.md` (guide d'utilisation)
3. `docs/schematics/DIAGRAMS_INDEX.md` (index des diagrammes)

**Workflow d'exécution :**
1. **Phase 0** : Lecture et indexation des ADR (Sequential Thinking - Thoughts 1-20) ⚠️ OBLIGATOIRE EN PREMIER
2. **Phase 1** : Analyse exhaustive avec validation ADR (Sequential Thinking - Thoughts 21-120)
3. **Phase 2** : Planification des diagrammes avec mapping ADR (Sequential Thinking - Thoughts 121-140)
4. **Phase 3** : Génération du fichier Draw.io avec annotations ADR (Execution)
5. **Phase 4** : Documentation complémentaire avec index ADR (Execution)
6. **Validation finale** : Checklist complète incluant conformité ADR stricte

---

## 📚 Références

- [C4 Model](https://c4model.com/) : Context, Container, Component, Code
- [UML Diagrams](https://www.uml-diagrams.org/) : Standards UML
- [Draw.io Documentation](https://www.drawio.com/doc/) : Format et API
- [Architecture Decision Records (MADR)](https://adr.github.io/madr/) : Documentation des décisions
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) : Robert C. Martin
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html) : Eric Evans

---

**Note finale** : Ce prompt est conçu pour générer une documentation architecturale exhaustive, factuelle et pédagogique. L'agent DOIT utiliser `sequential thinking` pour garantir une analyse complète avant de générer les diagrammes. Aucun élément ne doit être inventé - tout doit provenir du code source réel.
