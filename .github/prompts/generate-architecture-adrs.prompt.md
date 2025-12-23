---
description: Analyse architecturale complète de la solution pour générer des ADR cohérents et pertinents
name: generate-architecture-adrs
agent: agent
model: Claude Sonet 4.5
---

# Génération d'Architecture Decision Records (ADR)

## 🎯 Objectif

Analyser l'architecture complète de la solution pour identifier et documenter les décisions architecturales significatives sous forme d'ADR (Architecture Decision Records) au format MADR.

## 🎯 Contexte Métier et Alignement Stratégique

**IMPORTANT : Chaque ADR doit être évalué selon son alignement avec les objectifs métier**

### Questions Stratégiques à Explorer

1. **Quel est le domaine métier de la solution ?**
   - Secteur d'activité (Finance, Santé, E-commerce, etc.)
   - Problèmes métier résolus
   - Utilisateurs cibles (internes, externes, B2B, B2C)

2. **Quelles sont les contraintes métier identifiables ?**
   - Réglementations (RGPD, SOX, HIPAA, etc.)
   - Exigences de disponibilité (SLA : 99.9%, 99.99% ?)
   - Volume de données (ordre de grandeur)
   - Charge utilisateurs (concurrent users)

3. **Quelle est la stratégie d'évolution ?**
   - Ajout de nouvelles fonctionnalités fréquent ?
   - Stabilité et maintenance prioritaire ?
   - Scalabilité horizontale nécessaire ?
   - Multi-tenancy requis ?

4. **Quelles sont les contraintes organisationnelles ?**
   - Taille de l'équipe de développement
   - Compétences techniques disponibles
   - Processus de déploiement (CI/CD mature ?)
   - Structure d'équipe (mono-équipe, multi-équipes, distributed)

### Alignement Architecture ↔ Métier

**Chaque ADR doit justifier comment la décision architecturale :**
- ✅ **Supporte** les objectifs métier
- ✅ **Respecte** les contraintes réglementaires
- ✅ **Facilite** l'évolution future
- ✅ **S'adapte** aux capacités de l'équipe
- ✅ **Optimise** le Time-to-Market vs Qualité

**Exemple de section à ajouter dans chaque ADR :**
```markdown
## Alignement Stratégique

**Objectifs métier supportés :**
- [Objectif 1] : Cette décision permet [justification]
- [Objectif 2] : Cette décision facilite [justification]

**Contraintes respectées :**
- [Contrainte 1] : Implémentée via [mécanisme]
- [Contrainte 2] : Garantie par [pattern/technologie]

**Risques métier atténués :**
- [Risque 1] : Mitigation via [approche]
```

## 🧠 Utilisation du Sequential Thinking

**OBLIGATOIRE** : Utiliser l'outil `sequential thinking` pour :

1. **Décomposer l'analyse** en étapes de réflexion structurées
2. **Remettre en question** les premières observations si nécessaire
3. **Explorer des branches alternatives** pour chaque décision identifiée
4. **Générer des hypothèses** sur les raisons des choix architecturaux
5. **Vérifier les hypothèses** en confrontant au code existant
6. **Itérer jusqu'à certitude** avant de créer un ADR

**Processus de réflexion attendu :**
- Thought 1-3 : Explorer la structure globale, identifier les patterns
- Thought 4-6 : Formuler des hypothèses sur les décisions architecturales
- Thought 7-10 : Vérifier les hypothèses dans le code source
- Thought 11-15 : Évaluer la conformité SOLID/KISS/YAGNI/DRY/Onion
- Thought 16-20 : Identifier les alternatives et raisons de rejet
- Thought 21+ : Finaliser la rédaction de l'ADR avec certitude absolue

## 🎭 Posture d'Analyse

Tu adoptes la posture d'un **Architecte Logiciel Senior** avec expertise en :
- Architecture en couches (Onion/Clean/Hexagonal Architecture)
- Architecture par tranches verticales (Vertical Slice Architecture)
- Principes SOLID, KISS, YAGNI, DRY
- Domain-Driven Design (DDD)
- Patterns architecturaux (agnostique du langage/framework)
- Reverse engineering et documentation d'architecture

**IMPORTANT** : Rester **AGNOSTIQUE des technologies** :
- Ne PAS présumer des frameworks spécifiques (ex: .NET, Java, Node.js)
- Observer les patterns indépendamment de leur implémentation technologique
- Identifier les décisions architecturales universelles (découpage, responsabilités, dépendances)
- Mentionner les technologies observées UNIQUEMENT comme faits constatés, pas comme hypothèses

## 📋 Principes d'Analyse

### 1. Factualité Absolue
- ✅ **S'appuyer uniquement sur le code existant** : Observer ce qui EST, pas ce qui devrait être
- ✅ **Identifier les patterns réels** : Détecter les décisions architecturales déjà prises et appliquées
- ✅ **Constats vérifiables** : Chaque ADR doit refléter une réalité observable dans le code
- ❌ **Jamais** : Inventer des décisions, supposer des intentions non documentées

### 2. Pertinence et Priorité
- ✅ **Décisions structurantes** : Focus sur les choix qui impactent l'architecture globale
- ✅ **Patterns récurrents** : Documenter les approches appliquées systématiquement
- ✅ **Contraintes majeures** : Identifier les limitations techniques ou métier importantes
- ❌ **Éviter** : Détails d'implémentation mineurs, décisions locales sans impact global

### 3. Cohérence avec les Piliers Fondamentaux

Chaque ADR doit être évalué selon les **12 Piliers de Qualité** :

#### Piliers Techniques
1. **Maintenabilité** : Le code est-il lisible et évolutif ?
2. **Simplicité (KISS)** : Solution la plus simple qui fonctionne ?
3. **Précision** : Comportements déterministes et contrats clairs ?
4. **Robustesse** : Gestion des erreurs et résilience ?
5. **Sécurité** : Protection des données et gestion des secrets ?
6. **Extensibilité** : Capacité à absorber les changements ?
7. **Performance/Scalabilité** : Optimisation mesurée et charge croissante ?
8. **Factualité** : Constats vérifiables et mesurables ?

#### Piliers Structurels
9. **Rationalisation** : Justifications objectives des choix ?
10. **Harmonisation** : Conventions et patterns uniformes ?
11. **Mutualisation (DRY)** : Élimination de la duplication ?
12. **Modularisation** : Responsabilités séparées et couplage faible ?

## 🔍 Méthodologie d'Analyse

### Phase 1 : Exploration de l'Architecture Globale

**Explorer systématiquement :**

1. **Structure de la solution (.sln)**
   - Organisation des projets (naming conventions, groupes logiques)
   - Dépendances inter-projets (analyse du graph de dépendances)
   - Séparation des responsabilités (couches identifiées)

2. **Architecture en couches**
   - Domain Layer : `GroupeAdp.GenAi.Domains.*`
   - Application Layer : Détecter services applicatifs, use cases
   - Infrastructure Layer : `GroupeAdp.GenAi.Infrastructures.*`
   - Presentation Layer : `GroupeAdp.GenAi.Hostings.*`

3. **Patterns architecturaux détectés**
   - Onion Architecture / Clean Architecture / Hexagonal Architecture ?
   - CQRS (Command Query Responsibility Segregation) ?
   - Mediator Pattern (MediatR, autre ?) ?
   - Repository Pattern ?
   - Unit of Work ?
   - Dependency Injection (DI container) ?

4. **Abstractions et contrats**
   - Projets `*.Abstractions` : Interfaces, DTOs, contrats
   - Découplage entre couches (dépendances inversées ?)
   - Utilisation d'interfaces vs classes concrètes

### Phase 2 : Analyse des Décisions Techniques Majeures

**Pour chaque catégorie, identifier les décisions prises :**

#### A. Architecture Applicative

- **Type d'architecture global**
  - **Layered Architecture (Horizontale)** : Découpage par couches techniques (UI, Business, Data) ?
  - **Vertical Slice Architecture** : Découpage par features/use cases ?
  - **Hybride** : Combinaison (couches + slices) ?

- **Si Layered Architecture détectée :**
  - Convention de nommage des projets/dossiers par couche
  - Granularité des projets (monolithique, modulaire, microservices ?)
  - Séparation Abstractions vs Implémentations
  - Direction des dépendances (vers le domaine, inversion ?)

- **Si Vertical Slice Architecture détectée :**
  - Organisation par features/use cases (ex: `/Features/`, `/UseCases/`) ?
  - Chaque slice contient-elle toutes les couches nécessaires (UI, logique, data) ?
  - Code partagé entre slices : où et comment ? (ex: `/Shared/`, `/Common/`) ?
  - Couplage entre slices : faible ou fort ? Mécanisme de communication ?
  - Avantages observés : déploiement indépendant, équipes autonomes ?
  - Inconvénients observés : duplication de code, difficultés de refactoring transverse ?

- **Gestion des dépendances**
  - `Directory.Build.props`, `Directory.Packages.props`
  - Centralisation des versions de packages
  - Stratégie de gestion des références (PackageReference, ProjectReference)

- **Hébergement (Hostings)**
  - WebApp vs WebApi vs ConsoleApp
  - Bootstrapping séparé de l'Endpoint
  - Configuration et démarrage des applications

#### B. Domain Layer

- **Organisation du domaine (DDD Tactical Patterns)**
  - **Aggregates** : Racine d'agrégat identifiée ? Frontières transactionnelles ?
  - **Entities** : Identité unique (ID) ? Cycle de vie géré ?
  - **Value Objects** : Immutabilité ? Égalité par valeur ? Validation dans constructeur ?
  - **Domain Services** : Logique métier ne relevant pas d'une entité unique ?
  - **Domain Events** : Publication d'événements métier ? Handlers ?
  - **Factories** : Création d'agrégats/entités complexes ?
  - **Specifications** : Règles métier encapsulées et réutilisables ?
  
- **Logique métier**
  - Où est localisée la logique (dans les entités, services domaine, handlers) ?
  - **Rich Domain Model** vs **Anemic Domain Model** (anti-pattern) ?
  - Règles de validation : Dans le domaine, via Specifications, ou externalisées ?
  - Invariants métier : Garantis par le domaine ?
  
- **Abstractions du domaine**
  - Interfaces de repositories (abstraction de la persistance)
  - Interfaces de services métier (découplage)
  - Interfaces d'adapters externes (ports)
  - Events / Domain Events : Contrats d'événements ?

#### C. Infrastructure Layer

- **Adapters (Ports & Adapters pattern / Hexagonal)**
  - **Primary Adapters (Driving)** : UI, API, Controllers, Pages
  - **Secondary Adapters (Driven)** : Repositories, External APIs, File System
  - Abstractions (Ports) définies dans le domaine/application ?
  - Implémentations concrètes dans l'infrastructure ?
  
- **Patterns d'Intégration**
  - **API Gateway** : Point d'entrée unique pour APIs ?
  - **Service Locator** : Résolution dynamique de dépendances (anti-pattern si mal utilisé) ?
  - **Strangler Fig** : Migration progressive d'ancien système ?
  - **Anti-Corruption Layer** : Protection contre systèmes externes ?
  - **Message Broker** : Communication asynchrone inter-services ?
  
- **Persistance des données**
  - **Repository Pattern** : Implémentation générique ou spécifique ?
  - **Unit of Work** : Gestion transactionnelle groupée ?
  - **ORM** : Abstraction objet-relationnel (Entity Framework, Dapper, autre) ?
  - **Query Object** : Encapsulation de requêtes complexes ?
  - **Data Mapper** : Séparation entités domaine / entités persistance ?
  - **Identity Map** : Cache de première niveau ?
  - Stratégie de migration : Code First, Database First, Schema First ?
  - Gestion des connexions : Pooling, scoped, transient ?
  - Transactions : Explicites, implicites, distribuées ?
  - **Optimistic Locking** : Contrôle de concurrence (version, timestamp) ?
  
- **Caching Patterns**
  - **Cache-Aside** : Application gère le cache
  - **Read-Through / Write-Through** : Cache transparent
  - **Write-Behind** : Écriture asynchrone
  - Niveaux : Memory Cache (L1), Distributed Cache (L2 - Redis, etc.) ?
  
- **Configuration**
  - **Options Pattern** : Strongly-typed configuration ?
  - Configuration hiérarchique : appsettings.json, environnement, secrets
  - **Feature Flags** : Activation/désactivation dynamique de features ?
  - Secrets management : Chiffrement, coffre-fort (Vault, Key Vault) ?

#### D. Presentation Layer (Agnostique Framework UI)

- **Architecture UI Globale**
  - **Rendering mode** : Server-side, Client-side, Hybrid ?
  - Organisation : Pages, Components, Layouts, Shared ?
  - **Component-Based Architecture** : Composants réutilisables ?
  - **Micro-frontends** : Découpage UI en sous-applications ?
  
- **Patterns de Présentation**
  - **MVC (Model-View-Controller)** : Séparation modèle/vue/contrôleur ?
  - **MVP (Model-View-Presenter)** : Presenter comme médiateur ?
  - **MVVM (Model-View-ViewModel)** : Binding bidirectionnel ?
  - **Presentation Model** : État UI séparé du domaine ?
  - **Humble Object** : Logique minimale dans la vue ?
  - **Smart/Dumb Components** : Composants conteneurs vs présentation ?
  
- **State Management**
  - **Local State** : État dans le composant ?
  - **Shared State** : Services partagés, contextes ?
  - **Global State** : Store centralisé (Flux, Redux-like) ?
  - **Cascading Parameters** : Propagation de données parent→enfant ?
  - **Event Aggregator** : Communication inter-composants ?
  
- **Design System et UI**
  - **Component Library** : Bibliothèque UI réutilisable (Material, Bootstrap, custom) ?
  - **Atomic Design** : Atoms, Molecules, Organisms, Templates, Pages ?
  - **Design Tokens** : Variables de design centralisées ?
  - **Theming** : Support multi-thèmes (clair/sombre) ?
  - **CSS Architecture** : BEM, SMACSS, CSS Modules, Isolation ?
  - **Responsive Design** : Mobile-first, breakpoints ?
  
- **Patterns d'Interaction**
  - **Observer** : Notification de changements (data binding) ?
  - **Command Pattern** : Actions utilisateur encapsulées ?
  - **Memento** : Undo/Redo, historique d'état ?
  - **Lazy Loading** : Chargement différé de composants/données ?
  - **Virtualization** : Rendu optimisé de grandes listes ?
  - **Optimistic UI** : Feedback immédiat avant confirmation serveur ?

#### E. Tests Unitaires

- **Organisation des tests**
  - Convention de nommage (`*.Unit.Tests`)
  - Structure des projets de tests (miroir du code source ?)
  - Framework de tests (xUnit, NUnit, MSTest ?)

- **Stratégie de tests**
  - Coverage attendu (ligne de base, objectif ?)
  - Mocking (Moq, NSubstitute ?)
  - Mutation testing (Stryker) ?

#### F. Cross-Cutting Concerns

- **Logging et Observabilité**
  - **Structured Logging** : Logs structurés (JSON, key-value) ?
  - **Correlation ID** : Traçage de requêtes distribuées ?
  - **Niveaux de logs** : Trace, Debug, Info, Warning, Error, Critical ?
  - **Log Aggregation** : Centralisation (ELK, Seq, Splunk) ?
  - **Distributed Tracing** : OpenTelemetry, Jaeger, Zipkin ?
  - **Metrics** : Prometheus, StatsD, Application Insights ?
  - **Health Checks** : Endpoints de santé (liveness, readiness) ?
  
- **Exception Handling**
  - **Global Exception Handler** : Middleware/Filter centralisé ?
  - **Result Pattern** : Retour explicite succès/erreur (vs exceptions) ?
  - **Exception Shielding** : Masquage détails techniques ?
  - **Custom Exceptions** : Hiérarchie d'exceptions métier ?
  - **Problem Details (RFC 7807)** : Format standard d'erreur HTTP ?
  - **Retry Policy** : Réessai automatique sur erreurs transitoires ?
  
- **Sécurité**
  - **Authentication Patterns** :
    - Token-based (JWT, OAuth 2.0, OIDC)
    - Session-based (Cookies)
    - Certificate-based
    - Multi-factor Authentication (MFA)
  - **Authorization Patterns** :
    - Role-Based Access Control (RBAC)
    - Attribute-Based Access Control (ABAC)
    - Policy-Based Authorization
    - Claims-Based Authorization
  - **Security Headers** : CORS, CSP, HSTS, X-Frame-Options
  - **Input Validation** : Sanitization, whitelisting
  - **Encryption** : At rest, in transit (TLS/SSL)
  - **API Security** : Rate limiting, API keys, throttling
  - **Secret Management** : Vault, Key Management Service
  
- **Performance et Scalabilité**
  - **Caching Strategies** :
    - Memory Cache (in-process)
    - Distributed Cache (Redis, Memcached)
    - HTTP Caching (ETags, Cache-Control)
    - Query Result Caching
  - **Pagination Patterns** :
    - Offset-based pagination
    - Cursor-based pagination
    - Keyset pagination
  - **Lazy Loading** : Chargement différé de données
  - **Eager Loading** : Chargement anticipé (éviter N+1)
  - **Async/Await** : Programmation asynchrone
  - **Parallel Processing** : Tasks, threads, TPL
  - **Connection Pooling** : Réutilisation de connexions
  - **Batch Processing** : Traitement par lots
  - **Compression** : Gzip, Brotli
  
- **Validation**
  - **Input Validation** : À la frontière de l'application
  - **Domain Validation** : Règles métier dans le domaine
  - **Fluent Validation** : Règles déclaratives
  - **Data Annotations** : Validation par attributs
  - **Fail Fast** : Validation précoce
  
- **Communication et Messaging**
  - **Synchronous** : HTTP/REST, gRPC
  - **Asynchronous** : Message Queue, Event Bus
  - **Request-Reply** : Pattern demande-réponse
  - **Fire-and-Forget** : Envoi sans attente de réponse
  - **Idempotency** : Traitement sûr de requêtes dupliquées

### Phase 3 : Identification des ADR à Créer

**Pour chaque décision architecturale majeure identifiée :**

1. **Évaluer la pertinence avec Sequential Thinking**
   - **Hypothèse initiale** : Pourquoi cette décision a-t-elle été prise ?
   - **Vérification dans le code** : Où est-elle appliquée ? Systématiquement ou localement ?
   - **Impact architectural** : Quelles couches sont affectées ? (High, Medium, Low)
   - **Application systématique** : Pattern récurrent ou cas isolé ? (Oui/Non)
   - **Contraintes structurantes** : Impose-t-elle des choix sur le reste du code ? (Oui/Non)
   - **Révision si nécessaire** : L'hypothèse initiale est-elle confirmée par les faits ?

2. **Vérifier l'absence d'ADR existant**
   - Lister les ADR déjà présents dans `docs/adr/`
   - Éviter les doublons ou les redondances
   - Si ADR existant partiel, envisager de le compléter plutôt que créer un nouveau

3. **Prioriser les ADR à créer**
   - **PRIORITÉ 1 (Critique)** : Décisions structurantes, impact global, contraintes fortes
   - **PRIORITÉ 2 (Important)** : Patterns majeurs, contraintes techniques fortes
   - **PRIORITÉ 3 (Utile)** : Détails architecturaux, choix tactiques

4. **Analyser les Trade-offs (Compromis)**
   - **Quels bénéfices** cette décision apporte-t-elle ? (Mesurables si possible)
   - **Quels coûts** engendre-t-elle ? (Complexité, performance, maintenance)
   - **Quel équilibre** a été trouvé entre les piliers contradictoires ?
   - **Exemple** : Performance vs Maintenabilité, Simplicité vs Extensibilité

5. **Identifier les Points de Divergence**
   - Y a-t-il des **incohérences** dans l'application de cette décision ?
   - Certaines parties du code **violent-elles** cette décision ?
   - Si oui, documenter ces exceptions et leurs raisons dans l'ADR

## 📝 Format des ADR à Générer

**OBLIGATOIRE : Respecter le format MADR**

### Structure de l'ADR

```markdown
# [Numéro]. [Titre court et descriptif de la décision]

Date: YYYY-MM-DD

## Statut

Accepté

## Contexte

[Description du problème ou de la question qui nécessite une décision]

**Observations factuelles dans le code :**
- [Fichier/Projet 1] : [Observation concrète]
- [Fichier/Projet 2] : [Observation concrète]

**Contraintes identifiées :**
- [Contrainte technique 1]
- [Contrainte métier 1]

**Piliers impactés :** [Liste des piliers parmi les 12]

## Décision

[Description claire et factuelle de la décision prise, telle qu'observable dans le code]

**Approche technique :**
[Description de l'implémentation choisie]

**Exemples concrets :**
```[langage]
// Code extrait réel de la solution
```

**Justification (basée sur les piliers) :**
- **[Pilier 1]** : [Explication de comment ce pilier justifie la décision]
- **[Pilier 2]** : [Explication de comment ce pilier justifie la décision]

## Conséquences

### Positives
- [Avantage 1 - avec référence au code si possible]
- [Avantage 2 - avec métrique si applicable]

### Négatives
- [Inconvénient 1 - avec stratégie de mitigation]
- [Inconvénient 2 - avec compromis accepté]

### Neutres
- [Impact neutre 1]

## Alternatives considérées

### Option A: [Nom de l'alternative]
- **Description** : [Brève description]
- **Avantages** : [Points positifs]
- **Inconvénients** : [Points négatifs]
- **Raison du rejet** : [Pourquoi cette option n'a pas été choisie - basée sur observations]

### Option B: [Nom de l'alternative]
- [Idem]

## Conformité aux Principes

**SOLID :**
- **[S] Single Responsibility** : [Évaluation avec exemples concrets du code]
  - Chaque classe/module a-t-il une seule raison de changer ?
  - Exemple : [Fichier] respecte/viole ce principe car [raison]
- **[O] Open/Closed** : [Évaluation avec exemples concrets]
  - Extension possible sans modification ? Utilisation d'abstractions ?
  - Exemple : [Pattern observé] permet l'extension via [mécanisme]
- **[L] Liskov Substitution** : [Évaluation avec exemples concrets]
  - Les implémentations sont-elles substituables à leurs abstractions ?
  - Exemple : [Interface] et ses implémentations respectent le contrat
- **[I] Interface Segregation** : [Évaluation avec exemples concrets]
  - Les interfaces sont-elles focalisées et cohésives ?
  - Exemple : [Interface] expose uniquement [méthodes pertinentes]
- **[D] Dependency Inversion** : [Évaluation avec exemples concrets]
  - Dépendance sur abstractions, pas sur concrétions ?
  - Exemple : [Couche haute] dépend de [IInterface] et non [ConcreteClass]

**KISS (Keep It Simple, Stupid) :**
- **Complexité intrinsèque vs accidentelle** : [Analyse]
- **Nombre de concepts introduits** : [Nombre] concepts pour résoudre [problème]
- **Compréhensibilité** : Junior peut comprendre en [temps estimé] ?
- **Exemple concret** : [Code snippet] démontre la simplicité/complexité

**YAGNI (You Aren't Gonna Need It) :**
- **Fonctionnalités anticipées** : Y a-t-il du code pour des besoins futurs ?
- **Abstractions prématurées** : [Analyse des abstractions justifiées vs spéculatives]
- **Exemple** : [Pattern] est justifié car [usage actuel] ou spéculatif car [pas d'usage]

**DRY (Don't Repeat Yourself) :**
- **Duplication détectée** : [Oui/Non] - Si oui, où et pourquoi ?
- **Mutualisation effective** : [Liste des composants réutilisables]
- **Compromis acceptés** : [Cas où duplication est préférée à la mutualisation]
- **Exemple** : [Code mutualisé] dans [Fichier] utilisé par [N projets]

**Onion Architecture :**
- **Respect des couches** : [Analyse de la séparation Domain/Application/Infrastructure/Presentation]
- **Direction des dépendances** : [Schéma ou description : vers le centre uniquement ?]
- **Inversions de dépendances** : [Mécanismes utilisés : interfaces, DI, events]
- **Violations identifiées** : [Aucune / Liste des couplages inappropriés]

## Trade-offs et Compromis

**Compromis acceptés dans cette décision :**
- **[Pilier sacrifié]** vs **[Pilier favorisé]**
  - Exemple : Complexité accrue (contre KISS) pour gagner en Extensibilité
  - Justification : [Pourquoi ce compromis est acceptable dans ce contexte]

- **[Autre compromis]**
  - [Description et justification]

**Seuils et limites identifiés :**
- À partir de [condition], cette décision pourrait être remise en question
- Si [métrique] dépasse [valeur], considérer [alternative]

## Évolution et Révision

**Conditions de révision de cette décision :**
- [ ] Si [technologie/framework] évolue vers [changement majeur]
- [ ] Si le nombre de [composants/modules] dépasse [seuil]
- [ ] Si [métrique de performance/complexité] atteint [limite]
- [ ] Si [contrainte métier] change radicalement

**Indicateurs de santé de la décision :**
- **Métrique 1** : [Nom] - Valeur actuelle : [X] - Seuil critique : [Y]
- **Métrique 2** : [Nom] - Valeur actuelle : [X] - Seuil critique : [Y]

## Références

- **Code source** :
  - [Fichier source 1] : `chemin/vers/fichier.cs` (lignes X-Y)
  - [Fichier source 2] : `chemin/vers/fichier.cs` (lignes X-Y)
- **Documentation externe** :
  - [Lien vers doc officielle si applicable]
  - [Article/Blog pertinent]
- **Discussions et contexte** :
  - [Lien PR/Issue si applicable]
```

### Nommage des ADR

**Convention OBLIGATOIRE** : `NNN-titre-en-kebab-case.adr.md`

- `NNN` : Numéro séquentiel incrémental (001, 002, 003, ...)
  - **VÉRIFIER** le dernier numéro utilisé dans `docs/adr/`
  - **INCRÉMENTER** à partir du dernier numéro existant
- Titre en kebab-case (minuscules, mots séparés par tirets)
- Extension `.adr.md` OBLIGATOIRE

### Organisation des ADR

**Structure de dossiers :**
```
docs/adr/
├── README.md                          # Index de tous les ADR (actifs + dépréciés)
├── 001-titre.adr.md                   # ADR actifs (Accepté, Proposé)
├── 002-titre.adr.md
├── deprecated/                        # ADR dépréciés ou remplacés
│   ├── 007-ancien-titre.adr.md        # Statut: Déprécié ou Remplacé par ADR-XXX
│   └── 008-autre-ancien.adr.md
```

**Règles de gestion :**
- **ADR actifs** (statut "Accepté" ou "Proposé") : Rester dans `docs/adr/`
- **ADR dépréciés** (statut "Déprécié" ou "Remplacé par ADR-XXX") : **DÉPLACER** vers `docs/adr/deprecated/`
- **Index `README.md`** : Conserver la trace de TOUS les ADR (actifs + dépréciés) avec lien vers leur emplacement

**Exemples (agnostiques des technologies spécifiques) :**
```
001-layered-architecture-with-dependency-inversion.adr.md
002-vertical-slice-architecture-by-features.adr.md
003-mediator-pattern-for-command-handling.adr.md
004-repository-pattern-for-data-access.adr.md
005-cqrs-separation-commands-queries.adr.md
006-domain-events-for-cross-aggregate-communication.adr.md
```

**Note** : Si des technologies spécifiques sont observées (ex: framework UI, ORM), les mentionner comme FAITS CONSTATÉS dans l'ADR, pas comme hypothèses :
- ✅ Correct : "Architecture UI implémentée avec [Framework observé]"
- ❌ Incorrect : "Architecture UI pourrait utiliser [Framework supposé]"

## 🚀 Processus d'Exécution

### Étape 0 : Préparation et Cartographie Initiale

**Avant toute analyse, construire une carte mentale :**

1. **Lire et Analyser TOUS les ADR existants** dans `docs/adr/`
   - Comprendre les décisions déjà documentées
   - Identifier les zones non couvertes
   - **OBLIGATOIRE** : Détecter les contradictions entre ADR existants
   - **OBLIGATOIRE** : Identifier les doublons ou redondances entre ADR
   - **OBLIGATOIRE** : Vérifier la cohérence de la numérotation (pas de trou, pas de doublon)
   - Construire une matrice de cohérence : ADR vs ADR vs Code
   
   **Checklist de cohérence des ADR existants :**
   - [ ] Aucune contradiction directe (ADR-X dit A, ADR-Y dit non-A)
   - [ ] Aucune contradiction implicite (ADR-X implique A, ADR-Y implique non-A)
   - [ ] Aucun doublon (même décision documentée 2 fois)
   - [ ] Aucune redondance (décisions se chevauchant partiellement)
   - [ ] Statuts cohérents (si ADR-Y remplace ADR-X, alors ADR-X statut = "Remplacé par ADR-Y")
   - [ ] Numérotation séquentielle sans trou ni doublon
   - [ ] Décisions ADR toujours appliquées dans le code (pas obsolètes)
   
   **En cas de contradiction/doublon détecté :**
   - **DOCUMENTER** précisément la contradiction dans le rapport d'analyse
   - **PROPOSER** une résolution (réviser ADR-X, réviser ADR-Y, ou créer ADR-Z pour clarifier)
   - **NE JAMAIS** créer un nouvel ADR qui contredit un ADR existant sans justification explicite

2. **Analyser la documentation existante**
   - `README.md` : Vue d'ensemble du projet
   - `docs/techniques/` : Spécifications techniques
   - `docs/fonctionnelles/` : Exigences métier
   - Détecter les décisions architecturales mentionnées mais non formalisées en ADR

3. **Utiliser Sequential Thinking pour établir des hypothèses initiales**
   - Thought 1-5 : Que nous apprennent les noms des projets sur l'architecture ?
   - Thought 6-10 : Quels patterns sont probablement utilisés (DDD, CQRS, Clean Arch) ?
   - Thought 11-15 : Quelles sont les zones à risque de dette technique ?

### Étape 1 : Exploration Structurée

**Commencer par une analyse top-down :**

1. **Lire `GroupeAdp.Genai.sln`** pour identifier tous les projets
   - Compter le nombre total de projets
   - Identifier les groupes logiques (Domains, Infrastructures, Hostings)
   - Détecter la convention de nommage globale

2. **Analyser les fichiers de configuration centralisés**
   - `Directory.Build.props` : Propriétés communes (cibles, versions, règles)
   - `Directory.Packages.props` : Gestion centralisée des packages NuGet
   - `global.json` : Version du SDK .NET
   - `coverlet.runsettings` : Configuration des tests et coverage
   - `stryker-config.json` : Configuration du mutation testing

3. **Explorer la structure par catégorie**
   - **Domains** : Logique métier, agrégats, entités, règles
   - **Infrastructures** : Adapters, repositories, services externes
   - **Hostings** : Points d'entrée (WebApp, WebApi, ConsoleApp)
   - **Tests** : Stratégie de tests (unitaires, intégration, mutation)

4. **Construire un graphe de dépendances mentalement**
   - Qui dépend de qui ? (utiliser `grep` ou lire les `.csproj`)
   - Détecter les cycles de dépendances (violations d'architecture)
   - Identifier les projets "fondations" vs "applications"

### Étape 1.5 : Analyse de la Dette Technique et Code Smells

**OBLIGATOIRE : Identifier systématiquement la dette technique**

1. **Dette Technique Architecturale**
   - **Violations de séparation des responsabilités** : Couplage entre couches
   - **Dépendances circulaires** : Cycles dans le graphe de dépendances
   - **God Objects** : Classes avec trop de responsabilités (>500 lignes, >20 méthodes)
   - **Anemic Domain Model** : Domaine sans logique métier
   - **Big Ball of Mud** : Absence de structure claire
   - **Feature Envy** : Classe qui utilise plus les données d'une autre classe
   - **Shotgun Surgery** : Modification nécessitant changements dans multiples fichiers

2. **Dette Technique de Code**
   - **Code dupliqué** : Violations du principe DRY (>3 occurrences similaires)
   - **Méthodes longues** : >50 lignes, complexité cognitive élevée
   - **Paramètres multiples** : Méthodes avec >5 paramètres
   - **Classes trop grandes** : >500 lignes, responsabilités multiples
   - **Commentaires excessifs** : Code nécessitant commentaires pour être compris
   - **Code mort** : Code non appelé, inaccessible
   - **Magic Numbers/Strings** : Valeurs hardcodées sans constantes

3. **Dette de Tests**
   - **Couverture insuffisante** : Zones non testées
   - **Tests fragiles** : Tests couplés à l'implémentation
   - **Tests non maintenus** : Tests commentés ou ignorés
   - **Absence de tests d'intégration** : Uniquement tests unitaires

4. **Dette de Documentation**
   - **Documentation obsolète** : README, wikis non à jour
   - **ADR manquants** : Décisions non documentées
   - **API non documentée** : Endpoints sans spécification
   - **Diagrammes absents** : Architecture non visualisée

**Quantifier la dette :**
- **Faible** : Correctifs mineurs, <1 semaine
- **Moyenne** : Refactoring significatif, 1-4 semaines
- **Élevée** : Re-architecture nécessaire, >1 mois

### Étape 2 : Détection des Patterns avec Sequential Thinking

**Pour chaque projet majeur, appliquer la méthode scientifique :**

1. **Formuler une hypothèse architecturale**
   - Thought : "Ce projet semble implémenter [pattern] car [indices]"
   - Lister les fichiers clés à examiner pour confirmer

2. **Vérifier l'hypothèse dans le code**
   - Lire `Program.cs`, `Startup.cs`, fichiers de configuration
   - Examiner les services injectés (DI registration)
   - Observer les conventions de nommage (suffixes, préfixes)

3. **Identifier les patterns architecturaux (agnostique technologique)**
   
   **Patterns Architecturaux Globaux :**
   - **Layered Architecture** : Séparation en couches (Presentation, Business, Data)
   - **Onion/Clean Architecture** : Dépendances vers le centre (Domain)
   - **Hexagonal Architecture (Ports & Adapters)** : Isolation du domaine, adapters externes
   - **Vertical Slice Architecture** : Organisation par fonctionnalités/features
   - **Microservices** : Services autonomes, communication inter-services
   - **Event-Driven Architecture** : Communication asynchrone par événements
   - **SOA (Service-Oriented Architecture)** : Services réutilisables, contrats
   
   **Patterns Tactiques DDD :**
   - **Aggregate Pattern** : Racine d'agrégat, cohérence transactionnelle
   - **Entity** : Identité unique, cycle de vie
   - **Value Object** : Immutabilité, égalité par valeur
   - **Domain Service** : Logique métier ne relevant pas d'une entité
   - **Domain Events** : Événements de domaine et handlers associés
   - **Repository Pattern** : Abstraction d'accès aux données
   - **Specification Pattern** : Encapsulation de règles de requêtage
   - **Factory Pattern** : Création d'objets complexes du domaine
   
   **Patterns Applicatifs :**
   - **CQRS** : Séparation Commands/Queries
   - **Mediator** : Découplage via médiateur (handlers, commands, queries)
   - **Unit of Work** : Gestion transactionnelle groupée
   - **Application Services** : Orchestration de use cases
   - **DTO (Data Transfer Object)** : Transfert de données entre couches
   
   **Patterns de Création (GoF) :**
   - **Factory Method** : Création déléguée aux sous-classes
   - **Abstract Factory** : Familles d'objets liés
   - **Builder** : Construction d'objets complexes étape par étape
   - **Prototype** : Clonage d'objets
   - **Singleton** : Instance unique (attention : anti-pattern si mal utilisé)
   - **Dependency Injection** : Inversion de contrôle, découplage
   
   **Patterns Structurels (GoF) :**
   - **Adapter** : Conversion d'interface
   - **Bridge** : Séparation abstraction/implémentation
   - **Composite** : Arbre d'objets
   - **Decorator** : Ajout dynamique de responsabilités
   - **Facade** : Interface simplifiée
   - **Proxy** : Contrôle d'accès, lazy loading
   - **Flyweight** : Partage d'objets légers
   
   **Patterns Comportementaux (GoF) :**
   - **Chain of Responsibility** : Chaîne de traitement
   - **Command** : Encapsulation de requêtes
   - **Iterator** : Parcours de collections
   - **Observer** : Notification d'événements
   - **Strategy** : Algorithmes interchangeables
   - **Template Method** : Squelette d'algorithme
   - **State** : Changement de comportement selon l'état
   - **Visitor** : Opérations sur structure d'objets
   
   **Patterns de Résilience :**
   - **Circuit Breaker** : Protection contre défaillances en cascade
   - **Retry Pattern** : Réessai automatique
   - **Timeout Pattern** : Limite de temps d'attente
   - **Bulkhead** : Isolation de ressources
   - **Cache-Aside** : Mise en cache explicite
   
   **Patterns de Communication :**
   - **Request-Response** : Communication synchrone
   - **Publish-Subscribe** : Communication asynchrone, découplage
   - **Message Queue** : File d'attente de messages
   - **Event Sourcing** : Stockage d'événements
   - **Saga Pattern** : Transactions distribuées
   
   **Anti-Patterns à Détecter :**
   - **God Object** : Classe trop responsable
   - **Anemic Domain Model** : Domaine sans logique
   - **Big Ball of Mud** : Architecture désorganisée
   - **Golden Hammer** : Sur-utilisation d'un pattern
   - **Spaghetti Code** : Couplage excessif
   - **Lava Flow** : Code mort non supprimé

4. **Noter les conventions et idiomes**
   - Nommage des fichiers (suffixes : `Service`, `Repository`, `Handler`)
   - Organisation des dossiers (Features, Entities, Services)
   - Utilisation de records vs classes, structs

5. **Détecter les abstractions et contrats**
   - Projets `*.Abstractions` : Rôle et contenu
   - Interfaces vs classes abstraites : Quand et pourquoi ?
   - DTOs, ViewModels, Requests/Responses

6. **Réviser et ajuster les hypothèses**
   - Thought : "Mon hypothèse initiale était [X], mais je constate [Y]"
   - Thought : "Cela suggère plutôt [nouveau pattern] pour [raison]"

### Étape 3 : Consolidation et Analyse Transversale

**Regrouper les observations par thème avec analyse critique :**

1. **Architecture globale**
   - Quelle architecture est réellement implémentée ?
     - **Layered (Couches horizontales)** : Onion, Clean, Hexagonal ?
     - **Vertical Slice (Tranches verticales)** : Organisation par features/use cases ?
     - **Hybride** : Combinaison des deux approches ?
   - La séparation des responsabilités est-elle respectée partout ?
   - Y a-t-il des violations ou des incohérences ?
   - **Si Vertical Slice détecté** :
     - Chaque slice est-elle autonome ?
     - Y a-t-il du code partagé entre slices ? (Justifié ou couplage excessif ?)
     - La navigation entre slices est-elle claire ?

2. **Domain Layer**
   - DDD appliqué ? (Agrégats, Entités, Value Objects, Domain Services)
   - Logique métier localisée dans le domaine ou fuite dans d'autres couches ?
   - Validation : Où et comment ? (FluentValidation, Data Annotations, dans le domaine)

3. **Infrastructure Layer**
   - Adapters identifiés : External, Mediator, Metrics, autres ?
   - Persistance : EF Core ? Repositories génériques ou spécifiques ?
   - Configuration : Strongly-typed configuration objects ?

4. **Presentation Layer**
   - Blazor Server, WebAssembly, ou mode Auto ?
   - MudBlazor : Usage exclusif ou mixte avec composants custom ?
   - State management : Services Scoped, Cascading Parameters, State Container ?

5. **Cross-cutting concerns**
   - Logging : Serilog ? Structured logging ? Enrichers ?
   - Exception handling : Middleware global ? Result objects ?
   - Sécurité : Authentication schemes, Authorization policies

6. **Tests unitaires**
   - Framework : xUnit, NUnit, MSTest ?
   - Mocking : Moq, NSubstitute, FakeItEasy ?
   - Coverage : Objectif ? Stryker pour mutation testing ?
   - Convention : Tests miroir de la structure du code ?

**Utiliser Sequential Thinking pour détecter les incohérences :**
- Thought : "Le projet A applique [pattern], mais le projet B fait [différemment]"
- Thought : "Est-ce intentionnel (contexte différent) ou une incohérence à documenter ?"
- Thought : "Si incohérence, quel est l'impact ? Critique ou mineur ?"

### Étape 4 : Génération des ADR avec Réflexion Profonde

**Pour chaque décision majeure identifiée, processus en 6 sous-étapes :**

#### 4.1 : Analyse Préliminaire (Sequential Thinking)

**Utiliser 10-15 thoughts pour explorer la décision :**

- Thought 1-3 : Quelle est la décision observée ? Où apparaît-elle ?
- Thought 4-6 : Pourquoi cette décision a-t-elle probablement été prise ?
- Thought 7-9 : Quelles contraintes ou objectifs motivent ce choix ?
- Thought 10-12 : Quelles alternatives existent ? Pourquoi pas celles-ci ?
- Thought 13-15 : Cette décision est-elle cohérente avec les autres ADR/code ?

#### 4.2 : Collecte de Preuves Factuelles

**Rechercher des preuves concrètes dans le code :**

- **Utiliser `grep_search`** pour trouver tous les usages du pattern
- **Lire les fichiers clés** (5-10 fichiers) pour documenter le pattern
- **Extraire des code snippets** représentatifs (3-5 exemples)
- **Compter les occurrences** : Combien de projets/fichiers appliquent ce pattern ?

#### 4.3 : Évaluation Multi-Piliers

**Pour chaque pilier pertinent, analyse approfondie :**

- **Maintenabilité** : 
  - Temps estimé pour un nouveau dev de comprendre ? (heures/jours)
  - Documentation existante suffisante ?
- **Simplicité (KISS)** : 
  - Complexité cyclomatique moyenne observable ?
  - Nombre de concepts à maîtriser ?
- **Robustesse** : 
  - Gestion des erreurs visible dans le code ?
  - Tests pour cas limites ?
- **Extensibilité** : 
  - Combien de points d'extension identifiés ?
  - Exemple d'extension déjà réalisée ?

#### 4.4 : Analyse des Alternatives (Sequential Thinking)

**Pour chaque alternative potentielle :**

- Thought : "Alternative A serait [description]"
- Thought : "Avantages : [liste basée sur piliers]"
- Thought : "Inconvénients : [liste basée sur piliers]"
- Thought : "Pourquoi rejetée ? Probablement car [hypothèse à vérifier]"
- Vérification : Chercher des traces dans l'historique Git, commentaires, etc.

#### 4.5 : Identification des Trade-offs

**Analyser les compromis explicites :**

- Quel pilier est favorisé ? (ex: Extensibilité)
- Quel pilier est sacrifié ? (ex: Simplicité)
- Ce compromis est-il justifié par le contexte ? (ex: domaine complexe)
- Y a-t-il un seuil où ce compromis devient problématique ?

#### 4.6 : Rédaction de l'ADR

**Suivre le format MADR avec toutes les sections enrichies**

1. **VÉRIFIER** si un ADR existe déjà dans `docs/adr/` couvrant ce sujet
2. **CRÉER** un nouvel ADR si nécessaire avec le bon numéro séquentiel
3. **RESPECTER** strictement le format MADR
4. **INCLURE** des exemples de code réels extraits de la solution
5. **ÉVALUER** la conformité SOLID, KISS, YAGNI, DRY, Onion Architecture

### Étape 5 : Validation et Cohérence

**Pour chaque ADR créé, validation en 3 niveaux :**

#### 5.1 : Validation Formelle

**Vérifier que chaque ADR créé :**

- [ ] A un numéro unique et séquentiel (pas de trou, pas de doublon)
- [ ] Respecte la convention de nommage `NNN-titre-kebab-case.adr.md`
- [ ] Suit strictement le format MADR avec toutes les sections
- [ ] Contient des observations factuelles et vérifiables (pas d'opinions)
- [ ] Référence des fichiers sources concrets avec chemins exacts
- [ ] Évalue les 12 Piliers de Qualité pertinents (minimum 4-6 piliers)
- [ ] Justifie la décision avec SOLID, KISS, YAGNI, DRY, Onion
- [ ] Documente au moins 2-3 alternatives considérées
- [ ] Liste les conséquences (positives, négatives, neutres)
- [ ] Inclut des code snippets réels extraits de la solution
- [ ] Documente les trade-offs et compromis acceptés
- [ ] Définit des conditions de révision futures

#### 5.2 : Validation de Cohérence Globale (Existants + Nouveaux)

**OBLIGATOIRE : Vérifier la cohérence entre TOUS les ADR (existants + nouveaux) :**

**Matrice de Cohérence à Construire :**

| ADR-X | ADR-Y | Relation | Cohérent ? | Action si Incohérent |
|-------|-------|----------|------------|----------------------|
| ADR-001 | ADR-017 | [Complémentaire/Contradictoire/Redondant] | ✅/❌ | [Action] |

**Checklist de Cohérence Inter-ADR :**

- [ ] **Aucune contradiction directe** entre ADR existants
- [ ] **Aucune contradiction directe** entre nouveaux ADR créés
- [ ] **Aucune contradiction** entre ADR existants et nouveaux ADR
- [ ] **Aucun doublon** : Même décision documentée plusieurs fois
- [ ] **Aucune redondance partielle** : Décisions se chevauchant sans référence croisée
- [ ] Les décisions s'appuient les unes sur les autres logiquement
- [ ] Les références croisées sont explicites (ex: "Voir ADR-003", "Complète ADR-005")
- [ ] Les statuts sont cohérents :
   - Si ADR-Y remplace ADR-X → ADR-X statut = "Remplacé par ADR-Y"
   - Si ADR-Y dépend de ADR-X → Mentionné dans "Contexte" de ADR-Y
- [ ] La numérotation reflète l'ordre chronologique ou logique

**Utiliser Sequential Thinking pour détecter les incohérences :**
- Thought : "ADR-X dit [A], mais ADR-Y dit/implique [B contradictoire]"
- Thought : "Est-ce une contradiction réelle ou deux contextes différents ?"
- Thought : "Si contradiction : Laquelle est correcte selon le code observé ?"
- Thought : "Action nécessaire : Réviser ADR-X, réviser ADR-Y, créer ADR-Z pour clarifier, ou marquer ADR obsolète"

**En cas de contradiction détectée :**
1. **DOCUMENTER** dans le rapport d'analyse :
   - ADR concernés (numéros et titres)
   - Nature de la contradiction (directe, implicite, contexte)
   - Preuve factuelle dans le code (quel ADR est appliqué ?)
2. **PROPOSER** une résolution :
   - Créer nouvel ADR qui remplace/clarifie les précédents
   - Mettre à jour statut des ADR obsolètes ("Remplacé par ADR-XXX" ou "Déprécié")
   - **DÉPLACER** les ADR dépréciés vers `docs/adr/deprecated/`
   - Ajouter notes de clarification dans les ADR existants
3. **NE JAMAIS** laisser des contradictions non résolues

**En cas de doublon détecté :**
1. **IDENTIFIER** quel ADR est le plus complet/précis
2. **PROPOSER** de marquer l'autre comme "Déprécié" ou de fusionner les contenus
3. **DÉPLACER** l'ADR déprécié vers `docs/adr/deprecated/`
4. **DOCUMENTER** dans le rapport d'analyse

#### 5.3 : Validation de Complétude

**Vérifier que l'analyse couvre tous les aspects majeurs :**

- [ ] **Architecture globale** : Structure en couches documentée
- [ ] **Domain Layer** : Patterns DDD documentés (si applicables)
- [ ] **Infrastructure** : Adapters et persistance documentés
- [ ] **Presentation** : Patterns UI documentés (Blazor, MudBlazor)
- [ ] **Cross-cutting** : Logging, exceptions, sécurité documentés
- [ ] **Tests** : Stratégie de tests documentée
- [ ] **Configuration** : Gestion centralisée documentée
- [ ] **Dépendances** : Stratégie de gestion documentée

**Identifier les zones non documentées :**
- Lister les domaines explorés mais sans ADR (justification : non pertinent ?)
- Lister les domaines non explorés (limitation de temps, hors périmètre ?)

## 📊 Livrables Attendus

### 1. Rapport d'Analyse Architecturale Complet

**Créer un fichier de synthèse :** `docs/architecture-analysis-report.md`

**Contenu OBLIGATOIRE :**

```markdown
# Rapport d'Analyse Architecturale

Date: YYYY-MM-DD
Analysé par: [Agent/Architecte]
Scope: Solution complète `GroupeAdp.Genai.sln`

## Résumé Exécutif

**Nombre de projets analysés** : [X]
**Nombre d'ADR créés** : [Y]
**Niveau de conformité architecturale** : [Excellent / Bon / Moyen / Faible]
**Dette technique détectée** : [Faible / Moyenne / Élevée]

**Principales forces identifiées :**
- [Force 1]
- [Force 2]

**Principales zones d'amélioration :**
- [Amélioration 1]
- [Amélioration 2]

## Vue d'Ensemble de l'Architecture

### Structure Générale

[Description de haut niveau de l'architecture observée]

**Type d'architecture** : [Onion / Clean / Hexagonal / Autre]

**Couches identifiées :**
1. **Domain Layer** : [Liste des projets]
2. **Application Layer** : [Liste des projets]
3. **Infrastructure Layer** : [Liste des projets]
4. **Presentation Layer** : [Liste des projets]

### Graphe de Dépendances

```
[Schéma textuel des dépendances principales entre projets]
Example:
Hostings.WebApp.Management.Endpoint
  ├─> Hostings.WebApp.Management.Bootstrapping
  ├─> Domains.Commons
  └─> Infrastructures.Adapters.Mediator.Mediatr
```

### Technologies Clés Utilisées

- **Framework** : .NET [version]
- **ORM** : [Entity Framework Core / Autre]
- **UI Framework** : [Blazor Server/WASM/Auto]
- **Design System** : [MudBlazor]
- **Mediator** : [MediatR / Autre]
- **Logging** : [Serilog / Microsoft.Extensions.Logging]
- **Tests** : [xUnit / NUnit / MSTest]

## Patterns Architecturaux Identifiés

### Pattern 1: [Nom du Pattern]
- **Description** : [Description détaillée]
- **Localisation** : [Projets/Fichiers où observé]
- **Fréquence d'usage** : [Systématique / Fréquent / Occasionnel]
- **ADR associé** : `NNN-titre.adr.md`

### Pattern 2: [Nom du Pattern]
[Idem]

## Décisions Architecturales Majeures Identifiées

### Priorité 1 (Critique - Impact Global)
- **[Décision 1]** → ADR créé : `NNN-titre.adr.md`
  - Impact : [Description courte]
  - Piliers principaux : [Liste]
  
- **[Décision 2]** → ADR créé : `NNN-titre.adr.md`
  - Impact : [Description courte]
  - Piliers principaux : [Liste]

### Priorité 2 (Important - Patterns Majeurs)
- **[Décision 3]** → ADR créé : `NNN-titre.adr.md`
- **[Décision 4]** → ADR créé : `NNN-titre.adr.md`

### Priorité 3 (Utile - Détails Architecturaux)
- **[Décision 5]** → ADR créé : `NNN-titre.adr.md`

## Conformité aux Principes Architecturaux

### SOLID

#### [S] Single Responsibility Principle
- **Évaluation** : ⭐⭐⭐⭐☆ (4/5)
- **Observations** :
  - ✅ [Point fort 1]
  - ✅ [Point fort 2]
  - ⚠️ [Point d'attention 1]
- **Exemples** : [Fichiers respectant / violant le principe]

#### [O] Open/Closed Principle
- **Évaluation** : ⭐⭐⭐⭐⭐ (5/5)
- **Observations** : [...]

#### [L] Liskov Substitution Principle
- **Évaluation** : ⭐⭐⭐☆☆ (3/5)
- **Observations** : [...]

#### [I] Interface Segregation Principle
- **Évaluation** : ⭐⭐⭐⭐☆ (4/5)
- **Observations** : [...]

#### [D] Dependency Inversion Principle
- **Évaluation** : ⭐⭐⭐⭐⭐ (5/5)
- **Observations** : [...]

**Note globale SOLID** : ⭐⭐⭐⭐☆ (4.2/5)

### KISS (Keep It Simple, Stupid)

- **Évaluation** : ⭐⭐⭐⭐☆ (4/5)
- **Observations** :
  - ✅ [Zone de simplicité exemplaire]
  - ⚠️ [Zone de complexité accidentelle détectée]
- **Recommandations** : [Actions pour simplifier]

### YAGNI (You Aren't Gonna Need It)

- **Évaluation** : ⭐⭐⭐☆☆ (3/5)
- **Observations** :
  - ✅ [Absence de sur-ingénierie dans X]
  - ⚠️ [Abstractions prématurées détectées dans Y]
- **Recommandations** : [...]

### DRY (Don't Repeat Yourself)

- **Évaluation** : ⭐⭐⭐⭐☆ (4/5)
- **Observations** :
  - ✅ [Mutualisation effective de X]
  - ⚠️ [Duplication détectée dans Y (justifiée/injustifiée)]
- **Recommandations** : [...]

### Onion Architecture / Clean Architecture

- **Évaluation** : ⭐⭐⭐⭐⭐ (5/5)
- **Observations** :
  - ✅ [Respect strict des couches]
  - ✅ [Direction des dépendances correcte]
  - ⚠️ [Violations mineures : liste]
- **Recommandations** : [...]

**Score global de conformité** : ⭐⭐⭐⭐☆ (4.0/5)

## Évaluation selon les 12 Piliers de Qualité

| Pilier | Score | Observations Clés |
|--------|-------|-------------------|
| Maintenabilité | ⭐⭐⭐⭐☆ | [Résumé 1 ligne] |
| Simplicité (KISS) | ⭐⭐⭐⭐☆ | [Résumé 1 ligne] |
| Précision | ⭐⭐⭐⭐⭐ | [Résumé 1 ligne] |
| Robustesse | ⭐⭐⭐⭐☆ | [Résumé 1 ligne] |
| Sécurité | ⭐⭐⭐☆☆ | [Résumé 1 ligne] |
| Extensibilité | ⭐⭐⭐⭐⭐ | [Résumé 1 ligne] |
| Performance | ⭐⭐⭐⭐☆ | [Résumé 1 ligne] |
| Factualité | ⭐⭐⭐⭐⭐ | [Résumé 1 ligne] |
| Rationalisation | ⭐⭐⭐⭐☆ | [Résumé 1 ligne] |
| Harmonisation | ⭐⭐⭐⭐⭐ | [Résumé 1 ligne] |
| Mutualisation | ⭐⭐⭐⭐☆ | [Résumé 1 ligne] |
| Modularisation | ⭐⭐⭐⭐⭐ | [Résumé 1 ligne] |

**Score moyen** : ⭐⭐⭐⭐☆ (4.2/5)

## Incohérences et Violations Détectées

### Contradictions entre ADR Existants
**OBLIGATOIRE : Documenter toutes les contradictions détectées**

| ADR-X | ADR-Y | Type Contradiction | Preuve Code | Résolution Proposée |
|-------|-------|-------------------|-------------|---------------------|
| ADR-001 | ADR-017 | [Directe/Implicite] | [Fichiers observés] | [Action recommandée] |

**Détails des contradictions :**
- **Contradiction 1** : ADR-XXX vs ADR-YYY
  - **Nature** : [Directe : dit A vs non-A / Implicite : implique A vs non-A]
  - **Preuve code** : [Fichiers montrant quelle décision est appliquée]
  - **Impact** : [Faible/Moyen/Élevé]
  - **Résolution proposée** : [Créer ADR-ZZZ qui remplace/clarifie, ou mettre à jour statuts]

### Doublons entre ADR Existants
**OBLIGATOIRE : Documenter tous les doublons détectés**

- **Doublon 1** : ADR-XXX et ADR-YYY
  - **Même décision** : [Description de la décision dupliquée]
  - **Différences** : [Nuances si elles existent]
  - **Résolution proposée** : [Fusionner, ou marquer l'un comme "Déprécié"]

### Incohérences Mineures
- [Incohérence 1] : [Description et localisation]
- [Incohérence 2] : [Description et localisation]

### Violations Architecturales
- [Violation 1] : [Description, impact, recommandation]

### Zones de Dette Technique
- [Dette 1] : [Description, sévérité : Faible/Moyenne/Élevée]
- [Dette 2] : [...]

## Recommandations Prioritaires et Roadmap

### Matrice de Priorisation (Effort vs Impact)

```
        Impact
          ^
    High  |  [2] [1] [3]     Légende:
          |                 [1] = Quick Wins (priorité max)
   Medium |  [4] [5] [6]     [2] = Strategic (important)
          |                 [3] = Major Projects
     Low  |  [7] [8] [9]     [4-6] = Fill-ins
          +----------------> [7-9] = Avoid/Delegate
            Low  Med  High
                Effort
```

### Court Terme (0-3 mois) - Quick Wins & Critical

**PRIORITÉ 1 (Impact High, Effort Low) - Quick Wins**
1. **[Recommandation 1]**
   - **Impact** : High (sécurité/performance/dette critique)
   - **Effort** : Low (1-5 jours-personnes)
   - **ROI** : High (×10)
   - **Risque** : Low
   - **Action concrète** : [Description détaillée]
   - **Critère de succès** : [Métrique mesurable]
   - **Responsable suggéré** : [Équipe/Personne]

**PRIORITÉ 2 (Impact High, Effort Medium) - Strategic**
2. **[Recommandation 2]**
   - **Impact** : High
   - **Effort** : Medium (1-2 semaines)
   - **ROI** : High (×5)
   - **Risque** : Medium
   - **Action** : [Description]
   - **Dépendances** : [Autres recommandations]

### Moyen Terme (3-6 mois) - Strategic Improvements

**PRIORITÉ 3 (Impact High, Effort High) - Major Projects**
3. **[Recommandation 3]**
   - **Impact** : High
   - **Effort** : High (1-2 mois)
   - **ROI** : Medium (×3)
   - **Risque** : High
   - **Phase 1** : [Milestone 1]
   - **Phase 2** : [Milestone 2]

4. **[Recommandation 4]**
   [Idem]

### Long Terme (6+ mois) - Transformational

5. **[Recommandation 5]**
   - **Impact** : Very High
   - **Effort** : Very High (>3 mois)
   - **ROI** : Strategic
   - **Roadmap** :
     - Q1 : [Objectif]
     - Q2 : [Objectif]
     - Q3 : [Objectif]

### Recommandations par Catégorie

**🏛️ Architecture**
- [Recommandation architecturale 1]
- [Recommandation architecturale 2]

**🔐 Sécurité**
- [Recommandation sécurité 1]
- [Recommandation sécurité 2]

**⚡ Performance**
- [Recommandation performance 1]

**🧪 Tests et Qualité**
- [Recommandation tests 1]

**📚 Documentation**
- [Recommandation doc 1]

### Tableau de Bord de Suivi (KPIs)

| KPI | Baseline | Target 3M | Target 6M | Target 12M |
|-----|----------|-----------|-----------|------------|
| Score Qualité | [X]/100 | [Y]/100 | [Z]/100 | [W]/100 |
| Test Coverage | [X]% | 70% | 80% | 85% |
| Dette Technique | [X]j | [Y]j | [Z]j | [W]j |
| Violations SOLID | [X] | [Y] | [Z] | 0 |
| Code Smells | [X] | [Y] | [Z] | <10 |

## Zones Non Couvertes

**Domaines explorés mais non documentés en ADR :**
- [Domaine 1] : Raison = [Trop spécifique / Non pertinent / ...]

**Domaines non explorés :**
- [Domaine 2] : Raison = [Hors périmètre / Manque de temps / ...]

## Métriques Quantitatives et Qualitatives

### Métriques Structurelles

| Métrique | Valeur | Cible | Statut | Priorité Amélioration |
|----------|--------|-------|--------|----------------------|
| **Nombre total de projets** | [X] | - | - | - |
| **Projets avec tests unitaires** | [Y] | [X] (100%) | ✅ / ⚠️ / ❌ | High/Medium/Low |
| **Coverage de tests** | [Z%] | 80% | ✅ / ⚠️ / ❌ | High |
| **Nombre d'abstractions (interfaces)** | [N] | - | - | - |
| **Ratio Abstractions/Concrétions** | [N/M] | >0.3 | ✅ / ⚠️ / ❌ | Medium |
| **Violations SOLID détectées** | [M] | 0 | ✅ / ⚠️ / ❌ | High |
| **Profondeur max du graphe de dépendances** | [P] | ≤5 | ✅ / ⚠️ / ❌ | Medium |
| **Dépendances circulaires** | [C] | 0 | ✅ / ⚠️ / ❌ | High |

### Métriques de Complexité

| Métrique | Valeur | Cible | Statut | Impact |
|----------|--------|-------|--------|--------|
| **Complexité cyclomatique moyenne** | [CC] | ≤10 | ✅ / ⚠️ / ❌ | High |
| **Complexité cognitive moyenne** | [CG] | ≤15 | ✅ / ⚠️ / ❌ | High |
| **Lignes de code par méthode (moyenne)** | [LOC/M] | ≤30 | ✅ / ⚠️ / ❌ | Medium |
| **Nombre de paramètres (moyenne)** | [P/M] | ≤4 | ✅ / ⚠️ / ❌ | Medium |
| **Profondeur d'héritage max** | [DIT] | ≤4 | ✅ / ⚠️ / ❌ | Medium |
| **Couplage entre objets (CBO)** | [CBO] | ≤10 | ✅ / ⚠️ / ❌ | High |
| **Manque de cohésion (LCOM)** | [LCOM] | ≤0.5 | ✅ / ⚠️ / ❌ | High |

### Métriques de Maintenabilité

| Métrique | Valeur | Cible | Statut |
|----------|--------|-------|--------|
| **Maintenability Index (MI)** | [MI] | ≥70 | ✅ / ⚠️ / ❌ |
| **Code duplication (%)** | [D%] | ≤5% | ✅ / ⚠️ / ❌ |
| **Technical Debt Ratio** | [TDR] | ≤5% | ✅ / ⚠️ / ❌ |
| **Code Smells détectés** | [CS] | ≤10 | ✅ / ⚠️ / ❌ |
| **Classes "God Object" (>500 LOC)** | [GO] | 0 | ✅ / ⚠️ / ❌ |
| **Méthodes longues (>50 LOC)** | [ML] | ≤5% | ✅ / ⚠️ / ❌ |

### Métriques de Qualité des Tests

| Métrique | Valeur | Cible | Statut |
|----------|--------|-------|--------|
| **Line Coverage** | [LC%] | ≥80% | ✅ / ⚠️ / ❌ |
| **Branch Coverage** | [BC%] | ≥75% | ✅ / ⚠️ / ❌ |
| **Mutation Score** | [MS%] | ≥75% | ✅ / ⚠️ / ❌ |
| **Tests unitaires / classe** | [T/C] | ≥1 | ✅ / ⚠️ / ❌ |
| **Ratio Arrange/Act/Assert** | - | Équilibré | ✅ / ⚠️ / ❌ |

### Score Global de Qualité Architecturale

**Formule de calcul :**
```
Score = (SOLID×0.20) + (KISS×0.15) + (DRY×0.15) + (Tests×0.15) + 
        (Complexité×0.15) + (Dette×0.10) + (Documentation×0.10)
```

**Résultat :** [Score/100] → ⭐⭐⭐⭐⭐ (X/5)

**Classification :**
- 90-100 : ⭐⭐⭐⭐⭐ Excellent
- 75-89 : ⭐⭐⭐⭐☆ Très Bon
- 60-74 : ⭐⭐⭐☆☆ Bon
- 40-59 : ⭐⭐☆☆☆ Acceptable
- <40 : ⭐☆☆☆☆ Critique

## Analyse Comparative et Benchmarking

### Comparaison avec les Standards de l'Industrie

**Évaluer la solution par rapport aux pratiques recommandées :**

| Aspect | Standard Industrie | Solution Actuelle | Gap | Recommandation |
|--------|-------------------|-------------------|-----|----------------|
| **Architecture** | Clean/Onion/Hexagonal | [Observé] | [Écart] | [Action] |
| **Tests** | >80% coverage, mutation testing | [X%] | [Écart] | [Action] |
| **CI/CD** | Déploiement continu, <10min | [État] | [Écart] | [Action] |
| **Monitoring** | Observabilité complète (logs, metrics, traces) | [État] | [Écart] | [Action] |
| **Sécurité** | DevSecOps, SAST/DAST | [État] | [Écart] | [Action] |
| **Documentation** | Architecture as Code, ADR systématiques | [État] | [Écart] | [Action] |

### Maturité Architecturale (Architecture Maturity Model)

**Niveau 1 - Initial (Ad-hoc)** :
- Architecture émerge sans planification
- Décisions non documentées
- Pas de standards

**Niveau 2 - Répétable** :
- Patterns récurrents identifiables
- Documentation basique
- Standards locaux

**Niveau 3 - Défini** :
- Architecture intentionnelle et documentée
- ADR systématiques
- Standards d'équipe clairs

**Niveau 4 - Géré** :
- Métriques de qualité architecturale
- Revues d'architecture régulières
- Gouvernance active

**Niveau 5 - Optimisé** :
- Amélioration continue
- Innovation architecturale
- Excellence technique

**→ Niveau actuel de la solution : [1-5]**

### Comparaison avec Solutions Similaires

**Si applicable, comparer avec :**
- Solutions open-source du même domaine
- Architectures de référence (Microsoft, AWS, Google)
- Cas d'études publiés

| Aspect | Solution Référence | Notre Solution | Avantage | Inconvénient |
|--------|-------------------|----------------|----------|-------------|
| [Aspect 1] | [Approche ref] | [Notre approche] | [Avantage] | [Inconvénient] |

## Annexes

### Annexe A : Liste Complète des Projets Analysés
[Liste exhaustive avec description 1 ligne par projet]

**Format :**
```
- [Nom Projet] : [Type] - [Responsabilité] - [Dépendances principales]
  LOC: [X], Tests: [Oui/Non], Coverage: [Y%]
```

### Annexe B : Liste Complète des ADR Créés

| # | Titre | Date | Priorité | Piliers Impactés | Statut | Emplacement |
|---|-------|------|----------|------------------|--------|-------------|
| ADR-001 | [...] | YYYY-MM-DD | Critique | [Liste] | Accepté | docs/adr/ |
| ADR-XXX | [...] | YYYY-MM-DD | Déprécié | [Liste] | Remplacé par ADR-YYY | docs/adr/deprecated/ |

### Annexe C : Glossaire des Termes Techniques

**Architecture Patterns :**
- **Onion Architecture** : [Définition courte]
- **CQRS** : [Définition courte]
- **DDD** : [Définition courte]

**Acronymes :**
- **ADR** : Architecture Decision Record
- **DRY** : Don't Repeat Yourself
- **SOLID** : Single Responsibility, Open-Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **YAGNI** : You Aren't Gonna Need It
- **KISS** : Keep It Simple, Stupid

### Annexe D : Outils et Ressources

**Outils d'Analyse Recommandés :**
- **Architecture** : ArchUnit, NDepend, SonarQube Architecture
- **Qualité Code** : SonarQube, CodeClimate, ReSharper
- **Tests** : Coverage tools, Mutation testing (Stryker, PIT)
- **Métriques** : CodeScene, CodeMR
- **Visualisation** : PlantUML, C4 Model, Structurizr

**Références Documentaires :**
- [Architecture patterns documentation]
- [Best practices guides]
- [Industry standards]
```

### KISS
[Évaluation globale]

### YAGNI
[Évaluation globale]

### DRY
[Évaluation globale]

### Onion Architecture
[Évaluation globale]

## Recommandations

[Liste des recommandations pour améliorer la cohérence architecturale]
```

### 2. ADR Générés

**Créer tous les ADR identifiés dans :** `docs/adr/`

**Numérotation séquentielle :**
- Vérifier le dernier numéro existant
- Incrémenter pour chaque nouvel ADR
- Maintenir la cohérence de la numérotation

### 3. Mise à Jour de l'Index et Archivage

**Mettre à jour :** `docs/adr/README.md`

**Ajouter chaque nouvel ADR à l'index avec :**
- Numéro et titre
- Date de création
- Statut (Accepté / Proposé / Déprécié / Remplacé par ADR-XXX)
- Emplacement (`docs/adr/` ou `docs/adr/deprecated/`)
- Brève description (1 ligne)
- Piliers impactés

**Gérer les ADR dépréciés :**
1. **Si ADR existant doit être déprécié** :
   - Mettre à jour son statut dans le fichier
   - **DÉPLACER** vers `docs/adr/deprecated/`
   - Mettre à jour l'index avec le nouveau chemin
2. **Si nouvel ADR remplace un ancien** :
   - Statut ancien ADR : "Remplacé par ADR-XXX"
   - **DÉPLACER** ancien ADR vers `docs/adr/deprecated/`
   - Référencer l'ancien ADR dans le nouveau (section "Contexte" ou "Références")

## ⚠️ Contraintes et Interdictions

### TOUJOURS

- ✅ **Factuel** : S'appuyer uniquement sur le code existant
- ✅ **Vérifiable** : Chaque affirmation doit être observable dans le code
- ✅ **Exhaustif** : Explorer systématiquement toute la solution
- ✅ **Cohérent** : Maintenir la numérotation séquentielle des ADR
- ✅ **Conforme** : Respecter strictement le format MADR
- ✅ **Agnostique** : Observer les patterns indépendamment des technologies
- ✅ **Vérifier contradictions** : OBLIGATOIRE avant de créer un nouvel ADR
- ✅ **Vérifier doublons** : OBLIGATOIRE avant de créer un nouvel ADR
- ✅ **Vertical Slice** : Considérer cette architecture si observée dans le code

### JAMAIS

- ❌ **Inventer** des décisions non observables dans le code
- ❌ **Supposer** des intentions sans preuve factuelle
- ❌ **Présumer** des technologies sans les avoir observées dans le code
- ❌ **Créer** des ADR pour des détails mineurs sans impact architectural
- ❌ **Dupliquer** des ADR existants (même décision documentée 2 fois)
- ❌ **Contredire** un ADR existant sans justification explicite et mise à jour des statuts
- ❌ **Ignorer** les principes SOLID, KISS, YAGNI, DRY, Onion/Clean/Vertical Slice
- ❌ **Créer** un ADR technologie-spécifique si c'est un pattern universel

## 🎯 Critères de Succès

**L'analyse est complète et réussie si :**

### Critères Quantitatifs
- [ ] **Exhaustivité** : 100% des projets de la solution ont été explorés
- [ ] **Volume** : Minimum 10-15 ADR créés (couvrant les décisions majeures)
- [ ] **Traçabilité** : 100% des ADR référencent au moins 3 fichiers sources concrets
- [ ] **Couverture** : Les 8 catégories (Architecture, Domain, Infrastructure, Presentation, Cross-cutting, Tests, Config, Dépendances) sont documentées

### Critères Qualitatifs
- [ ] **Cohérence** : Les ADR reflètent fidèlement les décisions observées dans le code (vérification par relecture)
- [ ] **Pertinence** : Seules les décisions architecturales majeures sont documentées (Priorité 1 et 2)
- [ ] **Qualité formelle** : Chaque ADR respecte strictement le format MADR enrichi
- [ ] **Évaluation multi-piliers** : Chaque ADR évalue minimum 4-6 piliers parmi les 12
- [ ] **Conformité SOLID/KISS/YAGNI/DRY/Onion** : Chaque ADR contient une section dédiée avec évaluation détaillée
- [ ] **Alternatives documentées** : Chaque ADR documente minimum 2-3 alternatives avec raisons de rejet
- [ ] **Trade-offs explicites** : Chaque ADR identifie les compromis acceptés entre piliers
- [ ] **Conditions de révision** : Chaque ADR définit des seuils ou conditions de remise en question

### Critères d'Actionnabilité
- [ ] **Rapport complet** : `docs/architecture-analysis-report.md` créé avec toutes les sections
- [ ] **Scores et métriques** : Évaluation chiffrée (étoiles) pour SOLID, KISS, YAGNI, DRY, Onion, 12 Piliers
- [ ] **Recommandations concrètes** : Minimum 5 recommandations avec priorité (Court/Moyen/Long terme)
- [ ] **Incohérences identifiées** : Liste explicite des violations et incohérences détectées
- [ ] **Index mis à jour** : `docs/adr/README.md` contient tous les nouveaux ADR avec métadonnées

### Validation Finale

**Auto-évaluation avec Sequential Thinking :**
- Thought : "Ai-je exploré tous les projets systématiquement ?"
- Thought : "Chaque ADR est-il basé sur des preuves factuelles du code ?"
- Thought : "Les alternatives documentées sont-elles crédibles et vérifiables ?"
- Thought : "Les recommandations sont-elles actionnables et priorisées ?"
- Thought : "Un architecte externe pourrait-il reproduire mes conclusions ?"

**Score minimal pour considérer l'analyse comme réussie :**
- ✅ **Excellent** : 90%+ des critères validés
- ⚠️ **Bon** : 70-89% des critères validés
- ❌ **Insuffisant** : <70% des critères validés → Reprendre l'analyse

---

## 🚀 Plan d'Exécution Recommandé

**Déroulement chronologique suggéré :**

### Phase 1 : Préparation (30 min - 1h)
1. **Lire TOUS les ADR existants** dans `docs/adr/` (OBLIGATOIRE)
2. **Construire matrice de cohérence** des ADR existants :
   - Détecter contradictions (directes, implicites)
   - Détecter doublons et redondances
   - Vérifier numérotation séquentielle
   - Vérifier cohérence des statuts
3. Lire `README.md` et documentation existante
4. Utiliser Sequential Thinking pour formuler hypothèses initiales (10-15 thoughts)
   - **IMPORTANT** : Rester agnostique des technologies, observer sans présumer
5. Définir le dernier numéro ADR pour numérotation séquentielle

### Phase 2 : Exploration (2-3h)
1. Lire `GroupeAdp.Genai.sln` et lister tous les projets
2. Analyser `Directory.Build.props`, `Directory.Packages.props`, `global.json`
3. Explorer chaque catégorie de projets (Domains, Infrastructures, Hostings)
4. Construire le graphe de dépendances mental

### Phase 3 : Détection des Patterns (3-4h)
1. Pour chaque projet majeur : formuler hypothèse → vérifier dans code
2. **Identifier patterns (agnostique technologique)** :
   - Mediator, Repository, CQRS, DDD
   - Layered Architecture (Onion/Clean/Hexagonal)
   - **Vertical Slice Architecture** (organisation par features)
   - Hybrid (combinaison Layered + Vertical Slice)
3. Noter conventions de nommage, organisation de dossiers
4. Détecter abstractions et contrats
5. **Observer technologies UNIQUEMENT comme faits** (ne pas présumer)

### Phase 4 : Consolidation (1-2h)
1. Regrouper observations par thème (8 catégories)
2. Utiliser Sequential Thinking pour détecter incohérences
3. Identifier 15-20 décisions architecturales majeures
4. Prioriser en 3 niveaux (Critique, Important, Utile)

### Phase 5 : Génération des ADR (4-6h)
1. **AVANT chaque ADR** : Vérifier qu'aucun ADR existant ne couvre déjà cette décision
2. **AVANT chaque ADR** : Vérifier qu'aucun ADR existant ne contredit cette décision
3. Pour chaque décision : Sequential Thinking 10-15 thoughts
4. Collecter preuves factuelles (grep_search, read_file)
5. Évaluer multi-piliers (SOLID, KISS, YAGNI, DRY, Onion/Vertical Slice, 12 Piliers)
6. Analyser alternatives et trade-offs
7. Rédiger ADR complet au format MADR enrichi (agnostique technologique)
8. Créer 10-15 ADR minimum

### Phase 6 : Validation et Rapport (2-3h)
1. Validation formelle (format, numérotation, sections complètes)
2. **Validation de cohérence GLOBALE** (ADR existants + nouveaux ADR) :
   - Construire matrice de cohérence complète
   - Détecter contradictions, doublons, redondances
   - Proposer résolutions si incohérences détectées
3. Validation de complétude (8 catégories couvertes)
4. Rédiger rapport d'analyse complet avec :
   - Section dédiée aux contradictions/doublons détectés
   - Scores et métriques
   - Recommandations de résolution
5. **Archiver les ADR dépréciés** :
   - Créer `docs/adr/deprecated/` si n'existe pas
   - Déplacer tous les ADR avec statut "Déprécié" ou "Remplacé par"
6. Mettre à jour index `docs/adr/README.md` avec emplacements corrects

**Durée totale estimée : 12-19 heures**

---

**🚀 DÉMARRER L'ANALYSE MAINTENANT**

**Commencer par la Phase 1 : Préparation (CRITIQUE)**

1. **Lire TOUS les ADR existants** dans `docs/adr/` (19 ADR identifiés)
2. **Construire matrice de cohérence** des ADR existants :
   - Vérifier contradictions entre ADR-001 à ADR-019
   - Vérifier doublons et redondances
   - Vérifier que numérotation est cohérente (trous détectés : ADR-007 à ADR-009 manquants)
3. **Utiliser Sequential Thinking** pour hypothèses initiales (agnostique technologique)
4. Déterminer le prochain numéro ADR disponible (après ADR-019 ou combler trous ?)
5. Puis passer à la Phase 2 : Exploration de la solution complète
