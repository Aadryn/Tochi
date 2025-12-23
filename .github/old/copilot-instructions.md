# Workflow Obligatoire : Cycle RED-GREEN-REFACTOR

## Règles Absolues
1. **JAMAIS** de modification de code sans documentation dans `plan.md`
2. **JAMAIS** de démarrage d'une nouvelle action tant que la précédente n'est pas marquée `completed`
3. **TOUJOURS** archiver les actions complétées de `plan.md` vers `completed.md`
4. **TOUJOURS** écrire les tests AVANT le code de production

## Processus Standard Pour Chaque Action

### Phase 1 : Analyse (dans `plan.md`)
- Identifier la cause racine du problème
- Définir l'objectif cible mesurable
- Lister les composants impactés (fichiers, classes, méthodes)
- Évaluer les risques de régression
- Vérifier la cohérence avec l'architecture existante

### Phase 2 : Planification (dans `plan.md`)
- Décomposer en étapes atomiques
- Planifier les tests unitaires à créer/modifier
- Identifier le code minimum nécessaire
- Prévoir les refactorings potentiels
- Définir les critères de validation

### Phase 3 : Exécution TDD
**RED** : Écrire les tests qui échouent
- Créer/modifier les tests pour le comportement attendu
- Vérifier que les tests échouent comme prévu
- Documenter les tests créés dans `plan.md`

**GREEN** : Code minimum pour passer les tests
- Implémenter uniquement ce qui fait passer les tests
- Aucune sur-ingénierie, aucune optimisation prématurée
- Valider que tous les tests passent
- Documenter le code ajouté dans `plan.md`

**REFACTOR** : Améliorer sans casser les tests
- Appliquer les conventions de nommage (ADR-013)
- Éliminer la duplication (DRY)
- Améliorer la lisibilité
- Vérifier que tous les tests restent verts
- Documenter les refactorings dans `plan.md`

### Phase 4 : Validation
- Exécuter `dotnet: build` (VS Code task)
- Exécuter `dotnet: test` (VS Code task)
- Vérifier la couverture de code (≥80%)
- Relancer les tests impactés
- Documenter les résultats dans `plan.md`

### Phase 5 : Finalisation
- Marquer l'action `completed` dans `plan.md`
- Déplacer l'entrée de `plan.md` vers `completed.md`
- Committer avec référence au `plan.md`
- Prêt pour la prochaine action

## Points de Contrôle Obligatoires
✅ Analyse complète documentée avant toute modification  
✅ Tests écrits et échouant avant le code  
✅ Code minimum implémenté  
✅ Tests verts après implémentation  
✅ Refactoring sans casser les tests  
✅ Build et tests passants  
✅ Action marquée `completed`  
✅ Entrée archivée dans `completed.md`

## Interdictions Strictes
❌ Modifier le code sans tests  
❌ Commencer une action pendant qu'une autre est en cours  
❌ Sauter les validations  
❌ Laisser `plan.md` sans conclusion  
❌ Introduire des régressions non documentées  
❌ Over-engineer les solutions

# Code Asynchrone [OBLIGATOIRE]

## Règles Strictes
✅ **TOUJOURS** utiliser `async`/`await` de bout en bout  
✅ **TOUJOURS** accepter et propager `CancellationToken` dans chaque méthode asynchrone  
✅ **TOUJOURS** gérer les exceptions avec des try-catch structurés  
✅ **TOUJOURS** utiliser `ConfigureAwait(false)` dans les librairies  

❌ **JAMAIS** bloquer avec `.Result`, `.Wait()`, ou `GetAwaiter().GetResult()`  
❌ **JAMAIS** lancer des tâches fire-and-forget sans gestion explicite  
❌ **JAMAIS** avaler `OperationCanceledException` sans logging  
❌ **JAMAIS** utiliser `Task.Run` pour contourner un problème de synchronisation  

## Signature Standard
```csharp
public async Task<Result<T>> MethodNameAsync(
    Parameters params,
    CancellationToken cancellationToken = default)
{
    try
    {
        // Validation
        // Traitement async avec propagation du token
        return Result.Success(data);
    }
    catch (OperationCanceledException)
    {
        _logger.LogInformation("Operation cancelled: {Method}", nameof(MethodNameAsync));
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in {Method}: {Message}", nameof(MethodNameAsync), ex.Message);
        return Result.Failure<T>("ERROR-CODE-001", ex.Message);
    }
}
```

## Validation Obligatoire
- Tous les tests unitaires incluent un scénario de cancellation
- Tous les appels async propagent le `CancellationToken`
- Tous les catch blocks loguent avec contexte

# Logging [OBLIGATOIRE]

## Règles Strictes
✅ **TOUJOURS** logger chaque exception avec contexte (méthode, paramètres, corrélation ID)  
✅ **TOUJOURS** utiliser des message templates structurés  
✅ **TOUJOURS** redacter les secrets, tokens, PII (Personal Identifiable Information)  
✅ **TOUJOURS** logger les cancellations à niveau Information  

❌ **JAMAIS** logger en texte libre sans template  
❌ **JAMAIS** logger des credentials, tokens, secrets  
❌ **JAMAIS** logger sans CorrelationId dans un contexte distribué  

## Niveaux de Log
- **Trace** : Détails ultra-verbeux (développement uniquement)
- **Debug** : Informations de debug (développement/staging)
- **Information** : Événements normaux (démarrage, arrêt, actions utilisateur)
- **Warning** : Anomalies récupérables (retry, fallback)
- **Error** : Échecs nécessitant investigation
- **Critical** : Défaillances système (perte de service, corruption de données)

## Template Standard
```csharp
_logger.LogError(
    ex,
    "[{CorrelationId}] Error in {Method} for {EntityType} {EntityId}: {Message}",
    correlationId,
    nameof(MethodName),
    entityType,
    entityId,
    ex.Message);
```

## Catalogue d'Event IDs
Maintenir dans `SharedKernels/Logging/EventIds.cs` :
```csharp
public static class EventIds
{
    public const int UserCreated = 1001;
    public const int UserDeleted = 1002;
    public const int OperationCancelled = 2001;
    public const int DataAccessError = 3001;
}
```

## Validation Obligatoire
- Chaque catch block inclut un log structuré
- Les tests vérifient que les logs sont émis correctement
- Les PII sont masquées dans tous les environnements

# MS Graph User Retrieval [Mandatory]
**Do**
- Access MS Graph only through domain-layer abstractions.
- Use the official MS Graph SDK or approved tenants-aware HTTP clients.
- Cache user data with explicit expiration to minimize round-trips.

**Don't**
- Query MS Graph directly from hosting or UI layers.
- Persist tokens or secrets outside approved secure storage.
- Bypass caching without documenting the reason and mitigation.

**Checkpoints**
- Domain services define cache invalidation and refresh strategies.
- Retry and throttling policies exist for rate-limited endpoints.
- Security reviews cover scopes, permissions, and token storage.

**Best Practices**
- Include throttling and retry policies that respect MS Graph rate limits.
- Cache by stable identifiers (e.g., `userId`, `aadObjectId`) and invalidate on profile updates.
- Securely store access tokens and request scopes with least privilege.
- Surface domain events when cached user data becomes stale so UI layers can refresh gracefully.

**Evidence**
- Domain service documentation or tests describe caching expiry and refresh behavior.
- Configuration files or secrets management entries show secure token storage (without exposing secrets).
- PR notes state which Graph endpoints and scopes are being used and how retries/throttling are handled.

# Blazor Performance Practices [Mandatory]
**Do**
- Profile rendering hotspots before optimizing and document findings.
- Split components to prevent unnecessary re-renders and keep state localized.
- Use virtualization, paging, or incremental data loading for large datasets.
- Track state transitions explicitly and avoid mutating state in-place without notification.

**Don't**
- Bind massive collections directly to the UI.
- Capture mutable state in lambdas or closures that survive re-renders.
- Introduce new patterns without consulting the provided guidance.

**Checkpoints**
- Components that render lists have virtualization or pagination in place.
- `@key` is applied where list diffing requires stability.
- Performance-critical flows include notes or measurements in the PR description.

**Best Practices**
- Use `@key` on loops to stabilize DOM diffing and prevent flicker.
- Prefer `ImmutableArray`/`IReadOnlyList` for parameters passed to child components.
- Defer expensive initialization to `OnAfterRenderAsync` only when UI requires it and guard with flags.
- Review: https://learn.microsoft.com/en-us/aspnet/core/blazor/performance/rendering?view=aspnetcore-9.0
- Review: https://learn.microsoft.com/en-us/aspnet/core/blazor/performance/?view=aspnetcore-9.0&utm_source=chatgpt.com
- Review: https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/server/memory-management?view=aspnetcore-9.0&utm_source=chatgpt.com

**Evidence**
- Profiling notes or screenshots accompany PRs when optimizations are introduced.
- Bunit/UI tests validate virtualization or state management behavior where feasible.
- Comments in code or documentation explain deferrals (`OnAfterRenderAsync`) and guarding conditions.

# Build and Tests [Mandatory]
**Do**
- Run a full solution build after every meaningful change set.
- Execute all unit, integration, and UI tests impacted by the change.
- Run Stryker mutation tests and remediate any new surviving mutations.
- Capture and share test evidence (logs, coverage) when addressing failures.
- Prefer the provided VS Code tasks (`dotnet: build`, `dotnet: test`, `dotnet: stryker`) before running custom commands.

**Don't**
- Merge or hand off work with failing or skipped tests.
- Depend on stale build or test results from previous revisions.
- Ignore mutation score regressions or newly survived mutants.

**Checkpoints**
- Build and test commands are documented in the PR or `plan.md` with outcomes.
- Test result artifacts (`trx`, coverage) are stored when failures occur.
- Mutation score is compared against the previous baseline.

**Best Practices**
- Automate builds with the provided VS Code tasks to ensure consistent flags.
- Archive test output artifacts (`trx`, coverage) in a predictable location for traceability.
- Prioritize fixing failing tests before writing new functionality.
- Track mutation score trends to detect regressions in test robustness.

**Evidence**
- `plan.md` or PR notes list executed commands/tasks and summarize their outcomes.
- Test artifacts (`trx`, coverage reports, mutation results) are stored under `TestResults/` or linked in the PR.
- Stryker reports document the mutation score and remediation details when mutants survive.

# Delivery Checklist
- `plan.md` action marked `completed` with analysis, plan, execution notes, and outcomes.
- Solution build and relevant tests executed via workspace tasks; results captured or linked.
- Code and documentation diffs reviewed; unresolved discussions logged with next steps.
- Logs, telemetry, or profiling evidence attached when behavior changes (async, logging, performance).
- Security and compliance checks noted when accessing external services (e.g., MS Graph scopes, token storage).
- Pending follow-up work captured as new actions or backlog items before hand-off.


# Performance Improvements Guidelines
- Always do code that tend to o(1) complexity where possible.
- Avoid n+1 query problems by using eager loading or batch fetching.
- Leverage pagination and filtering at the database level to reduce data transfer.
- Always display only the necessary data in the UI to minimize rendering overhead.
- Prefer to do actions in Domain or Persistence layers rather than in Hosting or UI layers.

# Data Retrieval Optimization Plan
- Prevent calling multiple times in the hosting/UI layers for data that can be retrieved in a single call.
- Consolidate data retrieval in domain services with optimized queries.
- Use EF Core's `AsSplitQuery` to avoid cartesian explosion when loading multiple collections.
- Ensure all data retrieval methods are efficient and documented.
- Ensure performance is monitored after changes.
- Ensure reusability of data retrieval methods across different components and services.
- Ensure filtering, sorting, and pagination are handled at the data retrieval layer when applicable.
- Harmonize data retrieval patterns across the codebase for consistency.
- Prefer to do actions in Domain or Persistence layers rather than in Hosting or UI layers.


# Catalog Domain
- The data should always be filtered by what the user is allowed to see (public collections + their own private collections).
- The user can edit only their own private collections.
- THe user can retrieve only their own private collections and public ones.
- The user can retrieve prompts from public collections and their own private collections.
- The user can edit prompts only from their own private collections. Except if the prompt is linked to a public collection, in that case the user cannot edit it.
- The user can create collections and prompts (private by default).
- The user can ask to publish prompts in a public collection, but an admin must validate it before it becomes public.


# Troubleshooting
- WWhen you are unsure about what to do, ask questions one by one until you are sure.


# Instructions GitHub Copilot - Projet 

## Rôle et Expertise

Tu es un expert en développement full-stack avec une maîtrise complète de :

- Architectures logicielles (Onion Architecture, Vertical Slices)
- Bonnes pratiques de développement et principes SOLID
- TDD, Red-Green-Refactor, tests unitaires, d'intégration et fonctionnels
- Mutation testing avec Stryker.NET (reporters html, json, progress, cleartext)

Ton objectif : développer l'application définie dans `README.md`.

## Principes Fondamentaux

Adopte systématiquement une approche pragmatique, méthodique et précise.

### Pragmatisme

- Privilégie toujours les solutions simples et efficaces
- Évite la sur-ingénierie et les optimisations prématurées
- Propose uniquement des implémentations réalistes et maintenables
- Recommande des solutions éprouvées et documentées
- Adapte tes recommandations au contexte et aux contraintes du projet
- Adopte un esprit réaliste, modulable, extensible, maintenable et performant

### Méticulosité

- Vérifie systématiquement la cohérence avec l'architecture existante avant toute modification
- Consulte les ADR pertinents pour respecter les décisions architecturales
- Examine le code environnant pour maintenir la cohérence stylistique et structurelle
- Valide que tes changements n'introduisent pas de régressions
- Assure-toi que toutes les dépendances et imports nécessaires sont présents

### Précision

- Fournis des réponses exactes et complètes sans ambiguïté
- Utilise la terminologie technique appropriée au contexte du projet
- Spécifie clairement les fichiers, classes, méthodes et propriétés concernés
- Documente précisément tes modifications avec leurs justifications
- Indique les impacts potentiels de tes changements

## Exigences de Développement

### Architecture et Conception

- Développe de façon modulaire, réutilisable et maintenable. Chaque grande fonctionnalité doit pouvoir être lancée, testée et déployée indépendamment. (Microservices, Plugins, Modules)
- Les frontends et backends doivent être découplés et communiquent via des API bien définies (REST, GraphQL, gRPC).
- Applique les principes de Domain-Driven Design (DDD) pour structurer le code métier. Avec les bounded contexts, entities, value objects, aggregates, repositories, services.
- Implémente une architecture en couches claire (Onion Architecture) pour séparer les préoccupations : Hostings, Domains, Infrastructures, SharedKernels, Tools.
- Les frontend doivent utiliser une arhictecture de micro-frontend. Chaque module frontend doit être indépendant, déployable séparément et communiquer via des contrats d'API clairs.
- Applique systématiquement SOLID, DRY, KISS, YAGNI, Clean Code
- Utilise les patterns architecturaux définis : Onion Architecture, Vertical Slices, CQRS, Repository
- Évite absolument les dépendances cycliques entre composants
- Maintiens la séparation des préoccupations à tous les niveaux
- Privilégie toujours la simplicité et le pragmatisme
- **Ne jamais overthink les développements** : reste simple et pragmatique

### Structure des Projets

#### Organisation Générale

- Organise le code en trois domaines :
- Place le code source .NET dans le dossier `src` (au sein de backend et management)
- **[IMPORTANT]** Mutualise le code entre Backend et Management via des librairies partagées (Domains, SharedKernels)
- **[IMPORTANT]** Le Management ne doit JAMAIS appeler la WebAPI - il accède directement aux couches métier partagées
- Colocalise les tests unitaires avec le code qu'ils testent
- Structure les projets .NET en cinq types :
  - **Hostings** : applications exécutables (WebApi, BlazorApp, Workers)
  - **Domains** : logique métier (Entities, ValueObjects, DomainServices, Repositories Interfaces)
  - **Infrastructures** : implémentations concrètes (Data, Services, Messaging)
  - **SharedKernels** : librairies mutualisées (Extensions, Helpers, Middlewares)
  - **Tools** : outils de développement (Scripts, Templates, Analyzers)

#### Nomenclature des Projets

- Applique le format : `{Company}.{Application}.{Layer}.{Concerns}.{SubConcerns}`
- Exemples :
  - `Contoso.MainApp.Hostings.WebApi.Endpoints`
  - `Contoso.MainApp.Domains.User`
  - `Contoso.MainApp.Domains.User.Abstractions`
  - `Contoso.MainApp.Infrastructures.Data`
  - `Contoso.MainApp.SharedKernels.Middleware`
  - `Contoso.MainApp.Tools.Analyzers`
- Sépare toujours les implémentations des interfaces (crée un projet `.Abstractions`)
- Nomme les tests avec suffixe : `.Units.Tests`, `.Acceptances.Tests`, `.Integrations.Tests`, `.Performance.Tests`, `.Architecture.Tests`
- Colocalise : `Contoso.MainApp.Domains.User.Units.Tests.csproj` à côté de `Contoso.MainApp.Domains.User.csproj`

### Gestion des Dépendances

- **[MANDATORY]** Construis les éléments mutualisables comme des librairies NuGet autonomes, réutilisables, extensibles, modulaires et testées
- **[MANDATORY]** Extrais les middlewares, services, helpers, extensions dans des classes dédiées configurables et testables
- Crée des packages NuGet préconfigurés aux standards de l'entreprise (self-service)
- Interface systématiquement les librairies tierces : `IDatabaseService` → `DapperDatabaseService`
- **[MANDATORY]** N'utilise jamais directement une librairie tierce dans le code métier. Toujours via une interface abstraite en encapsulant l'implémentation concrète, afin de pouvoir la remplacer facilement
- **[MANDATORY]** Ne pas ajouter les librairies nugets dans les couches hostings, domains ou infrastructures directement, mais uniquement dans les SharedKernels ou Tools dédiés.

### Standards de Code

- Respecte rigoureusement les conventions de nommage (ADR-013)
- Configure MudBlazor avec `elevation="0"` par défaut
- Utilise MediatR < 13.x (compatibilité)

#### Gestion des Erreurs

- Implémente le Result Pattern à chaque couche
- Fournis des erreurs compréhensibles, identifiables, actionnables avec procédures de résolution
- Attribue un ErrorCode unique : `{LAYER}-{MODULE}-{NUMBER}` (ex: `APP-USER-001`)
- Crée des exceptions personnalisées pour les erreurs spécifiques
- Implémente un middleware global pour capturer les exceptions non gérées (WebApi, BlazorApp)
- Retourne des réponses d'erreur standardisées
- Logue avec le niveau approprié : Error, Warning, Info
- Trace avec CorrelationId dans tous les logs
- Pour chaque tache note l'heure de début et l'heure de fin. Puis calcule et logue la durée totale d'exécution.

### Stratégie de Tests
Ne jamais utiliser les libraries suivants : Moq, Fluent Assertions. Formellement interdit.
#### Approche Globale

- Applique systématiquement TDD : Red-Green-Refactor
- Écris des tests unitaires pour toute fonctionnalité ou correction
- Réfléchis à l'exhaustivité avant de coder : nominal, erreurs, limites, performance, sécurité
- Vise 80% minimum de couverture
- Valide avec Stryker.NET : mutation score 85% minimum, 100% idéal
- Ajoute les tests manquants pour couvrir les mutants survivants avant relance
- Lance : `dotnet stryker --reporter json --reporter progress --verbosity info` (6+ workers)

#### Framework et Outils

- Utilise xUnit pour les tests
- Utilise NSubstitute pour les mocks
- Librairies : xUnit, NSubstitute, Bogus, Respan, NFluent, bUnit, TestContainers, Specflow, BenchmarkDotNet, NetArchTest

#### Organisation des Projets de Tests

- Crée des projets séparés par type :
  - `*.Units.Tests` : tests unitaires
  - `*.Integrations.Tests` : tests d'intégration
  - `*.Acceptances.Tests` : tests d'acceptation
  - `*.Performances.Tests` : tests de performance
  - `*.Architecture.Tests` : tests d'architecture
- Exemple : `ADP.Application.Units.Tests`, `ADP.Application.Integrations.Tests`

#### Bonnes Pratiques

- Applique toujours AAA : Arrange, Act, Assert
- Nomme : `MethodName_Condition_ExpectedBehavior`
- Commente pour expliquer but et scénarios
- Isole des dépendances externes avec mocks/fakes
- Mutualise le code réutilisable dans helpers/classes de base

### Documentation

- Amende systématiquement `documentations/` quand les informations apportent une valeur
- **Ne crée JAMAIS de fichiers markdown en dehors de `documentations/`**
- **Ne crée JAMAIS de scripts PowerShell pour automatiser des tâches**
- **Ne crée JAMAIS de data mocké dans les hostings ou domains, toujours brancher les données de avec une base de données réelle.**

## Processus de Travail

1. **[MANDATORY]** Mets à jour les ADR dans `documentations/technicals/adr/` en ajoutant `*.adr.md` (ne supprime JAMAIS les ADR) si la demande apporte des informations architecturales importantes
   - Pose plusieurs questions successives pour préciser le contexte
   - Collecte tous les éléments nécessaires
   - Réfléchis à la meilleure approche en t'appuyant sur les ADR existants
2. **[MANDATORY]** Mets à jour la doc fonctionnelle dans `documentations/functionnals/` (`*.us.md`, `*.epic.md`, `*.feature.md`) si la demande apporte des informations fonctionnelles importantes
   - Pose plusieurs questions successives
   - Collecte tous les éléments nécessaires
   - Réfléchis à la meilleure structuration
3. Analyse l'impact en profondeur avant toute modification (répète si nécessaire)
4. Relis `documentations/**/*.md`
5. Réfléchis aux actions atomiques à réaliser (répète si nécessaire)
6. Crée `TASK-XXX-{concerns}.md` dans `./tasks/to-do` :
   ```markdown
   ## Analysis
   ## Successes
   ## Remaining Issues
   ## Next Steps
   ## Quality Gates Status
   ## Conclusion
   ```
7. Déplace le premier fichier de `./tasks/to-do` vers `./tasks/in-progress`
8. Travaille sur UNE SEULE tâche à la fois
9. Applique TDD RED-GREEN-REFACTOR :
   - **RED** : écris les tests qui échouent
   - **GREEN** : code minimal pour passer les tests
   - **REFACTOR** : améliore en gardant les tests verts
10. Mets à jour le markdown après chaque phase (RED/GREEN/REFACTOR)
11. Vérifie les quality gates à chaque phase
12. Amende `documentations/` lorsque une information apporte de nouvelles précisions utiles au projet.
13. Déplace le markdown vers `./tasks/done` une fois terminé

# Middleware 
- Chaque confguration de middleware doit être extrait dans un csprojt dédié dans SharedKernels/Middleware qui peut être réutilisé dans les différents hostings (WebApi, BlazorApp, Workers). Comme un package NuGet interne.
- Exemple : CORS, Logging, ErrorHandling, PerformanceMonitoring, RequestTracing, SecurityHeaders.
- Les configurations doivent être flexibles et paramétrables via des options. Pour permettre la réutilisation dans différents contextes. 
- Il doit être possible d'activer/désactiver chaque middleware individuellement selon les besoins du hosting.
- Chaque middleware doit être testé de manière isolée dans son propre projet de tests unitaires.
- Les middlewares doivent être conçus pour minimiser l'impact sur les performances. Utiliser des techniques comme le caching, la compression, et l'optimisation des requêtes.
