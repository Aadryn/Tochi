---
agent: 'agent'
model: 'Claude Sonnet 4.5'
description: 'Finaliser implémentation complète des pages du Management.Endpoint avec approche TDD'
---

# Objectif Principal
Finaliser l'implémentation complète de toutes les pages du projet `GroupeAdp.GenAi.Hostings.WebApp.Management.Endpoint`, en remplaçant toutes les simulations/mocks par des implémentations réelles, en complétant les fonctionnalités manquantes, et en garantissant une couverture de tests robuste (≥80% coverage, ≥85% mutation score).

# Méthodologie de Finalisation

## Phase 0 : Préparation et Audit Initial

### 0.1 Lecture du Contexte Architectural
- Lire **TOUS** les ADRs dans `documentations/technicals/adr/**/*.adr.md`
- Comprendre les décisions architecturales (Onion Architecture, CQRS, MediatR, etc.)
- Identifier les patterns établis (Result Pattern, ViewModels, Helpers, etc.)
- Vérifier les contraintes de sécurité et permissions

### 0.2 Audit Complet des Pages
Parcourir **TOUTES** les pages dans `GroupeAdp.GenAi.Hostings.WebApp.Management.Endpoint/Features/` :

#### Modules à auditer :
1. **Collections** (`Features/Collections/Pages/`)
   - List.razor.cs, Create.razor.cs, Read.razor.cs, Update.razor.cs, Delete.razor.cs
2. **Prompts** (`Features/Prompts/Pages/`)
   - List.razor.cs, Create.razor.cs, Read.razor.cs, Update.razor.cs, Delete.razor.cs
3. **Tags** (`Features/Tags/Pages/`)
   - List.razor.cs, Create.razor.cs, Read.razor.cs, Update.razor.cs, Delete.razor.cs
4. **FeaturedPrompts** (`Features/FeaturedPrompts/Pages/`)
   - List.razor.cs, Create.razor.cs, Read.razor.cs, Update.razor.cs, Delete.razor.cs
5. **FeaturedCollections** (`Features/FeaturedCollections/Pages/`)
   - List.razor.cs, Create.razor.cs, Read.razor.cs, Update.razor.cs, Delete.razor.cs
6. **FeaturedTags** (`Features/FeaturedTags/Pages/`)
   - List.razor.cs, Create.razor.cs, Read.razor.cs, Update.razor.cs, Delete.razor.cs
7. **CollectionPermissions** (`Features/CollectionPermissions/Pages/`)
   - List.razor.cs, Create.razor.cs, Read.razor.cs, Update.razor.cs, Delete.razor.cs
8. **FavoritePrompts** (`Features/FavoritePrompts/Pages/`)
   - List.razor.cs, Create.razor.cs, Delete.razor.cs
9. **FavoriteCollections** (`Features/FavoriteCollections/Pages/`)
   - List.razor.cs, Create.razor.cs, Delete.razor.cs
10. **FavoriteTags** (`Features/FavoriteTags/Pages/`)
    - List.razor.cs, Create.razor.cs, Delete.razor.cs

### 0.3 Identification des Problèmes
Pour chaque page, détecter :

#### ❌ Simulations à remplacer :
- `Task.Delay()` utilisé pour simuler des appels API
- Données mockées en dur au lieu de vraies requêtes
- Commentaires `IMPLEMENTATION_NOTE:` indiquant du code temporaire
- Méthodes commentées avec instructions de remplacement

#### ❌ TODOs et FIXMEs :
```bash
# Rechercher dans le code :
grep -r "TODO" --include="*.cs" Features/
grep -r "FIXME" --include="*.cs" Features/
grep -r "HACK" --include="*.cs" Features/
grep -r "IMPLEMENTATION_NOTE" --include="*.cs" Features/
```

#### ❌ Fonctionnalités manquantes :
- Commandes/Queries non créées dans les couches Domain
- Helpers ViewModels incomplets (méthodes `ToDomainModelForCreate`, `ToDomainModelForUpdate`)
- Conversions de types manquantes
- Validations incomplètes
- Gestion d'erreurs insuffisante

#### ❌ Tests manquants ou incomplets :
- Pages sans tests unitaires
- Tests avec couverture < 80%
- Scénarios d'erreur non testés
- Edge cases non couverts
- Mutation score < 85%

#### ❌ Problèmes de performances :
- Requêtes N+1 potentielles
- Chargement de données excessif
- Manque de pagination/filtrage
- Appels API non optimisés

## Phase 1 : Inventaire Détaillé et Priorisation

### 1.1 Créer l'inventaire structuré

Pour **chaque page** auditée, documenter dans un fichier `FINALIZATION_INVENTORY.md` :

```markdown
## Module: [NomModule] - Page: [NomPage]

### État Actuel
- ✅ **Fonctionnel** : Oui/Non
- ⚠️ **Simulations** : Liste des simulations détectées
- 🔧 **TODOs** : Liste des TODOs avec localisation (fichier:ligne)
- 🐛 **Bugs potentiels** : Liste des bugs détectés
- 📊 **Couverture tests** : X% (état actuel)
- 🧬 **Mutation score** : X% (état actuel)

### Fonctionnalités Manquantes
1. [Description fonctionnalité 1]
2. [Description fonctionnalité 2]
...

### Dépendances Requises
- Domaine : [Commandes/Queries à créer]
- Infrastructure : [Services à implémenter]
- Helpers : [Méthodes à ajouter]

### Estimation Complexité
- **Effort** : Simple / Moyen / Complexe
- **Risque** : Faible / Moyen / Élevé
- **Impact** : Faible / Moyen / Critique

### Tests à Créer/Améliorer
- [ ] Test nominal
- [ ] Tests erreurs
- [ ] Tests permissions
- [ ] Tests validation
- [ ] Tests edge cases
```

### 1.2 Priorisation des tâches

Appliquer la matrice de priorisation :

#### Priorité CRITIQUE (P0) - À faire immédiatement :
- Pages avec simulations bloquant la production
- Bugs de sécurité ou corruption de données
- Features critiques pour les utilisateurs finaux
- Tests manquants sur code en production

#### Priorité HAUTE (P1) - À faire rapidement :
- Pages partiellement fonctionnelles avec workarounds
- TODOs marqués comme urgents
- Features importantes mais non bloquantes
- Couverture tests < 60%

#### Priorité MOYENNE (P2) - À planifier :
- Optimisations de performance
- Amélioration UX
- Refactoring qualité code
- Couverture tests 60-80%

#### Priorité BASSE (P3) - Nice to have :
- Documentation supplémentaire
- Refactoring cosmétique
- Tests > 80% mais < 90%

### 1.3 Créer le Plan d'Action Atomique

Pour chaque tâche identifiée, créer une entrée détaillée :

```markdown
### Tâche: [ID] - [Titre Court]

**Priorité**: P0 / P1 / P2 / P3
**Module**: [NomModule]
**Page**: [NomPage]
**Estimation**: XhY (heures/jours)

**Objectif**:
[Description précise de ce qui doit être fait]

**Fichiers Impactés**:
- `Path/To/File1.cs` (lignes XX-YY)
- `Path/To/File2.cs` (lignes AA-BB)

**Actions Détaillées**:
1. [Action atomique 1]
2. [Action atomique 2]
3. [Action atomique 3]

**Dépendances**:
- Tâche #XX doit être complétée avant
- Nécessite création de [NomCommande/Query]

**Critères d'Acceptance**:
- [ ] Simulation remplacée par appel MediatR réel
- [ ] Tous les tests unitaires passent
- [ ] Couverture ≥ 80%
- [ ] Mutation score ≥ 85%
- [ ] Build réussit sans warnings
- [ ] Documentation mise à jour

**Tests à Créer**:
- [ ] `[NomTest]_WhenNominal_ShouldSucceed`
- [ ] `[NomTest]_WhenInvalidData_ShouldFail`
- [ ] `[NomTest]_WhenUnauthorized_ShouldDeny`
```

## Phase 2 : Implémentation TDD par Module

Pour **chaque tâche**, suivre strictement le cycle **RED-GREEN-REFACTOR** :

### 2.1 Préparation du Module

#### Vérifier les Dépendances Domain
Avant de modifier une page, s'assurer que la couche Domain est complète :

1. **Commandes** (`Domains.Commons.Abstractions.Management.[Module].Handlers.Commands`)
   - `Create[Entity]Command`
   - `Update[Entity]Command`
   - `Delete[Entity]Command`
   
   Si manquantes : **CRÉER D'ABORD** avec tests unitaires complets

2. **Queries** (`Domains.Commons.Abstractions.Management.[Module].Handlers.Queries`)
   - `Get[Entity]ByIdQuery`
   - `Get[Entities]Query` (avec pagination, filtres, tri)
   
   Si manquantes : **CRÉER D'ABORD** avec tests unitaires complets

3. **Handlers** 
   - `Create[Entity]CommandHandler`
   - `Update[Entity]CommandHandler`
   - `Delete[Entity]CommandHandler`
   - `Get[Entity]ByIdQueryHandler`
   - `Get[Entities]QueryHandler`
   
   Si manquants : **CRÉER D'ABORD** avec tests unitaires complets

4. **Validators** (FluentValidation)
   - `Create[Entity]CommandValidator`
   - `Update[Entity]CommandValidator`
   
   Si manquants : **CRÉER D'ABORD** avec tests unitaires complets

#### Vérifier les Helpers ViewModels
S'assurer que les helpers de conversion sont complets :

```csharp
// Exemple pour FeaturedPrompts
public static class FeaturedPromptViewModelHelper
{
  // ✅ REQUIS pour List
  public static FeaturedPromptViewModel FromDomainModel(DomainModel model);
  public static List<FeaturedPromptViewModel> FromDomainModels(IEnumerable<DomainModel> models);
  
  // ✅ REQUIS pour Read
  public static FeaturedPromptReadViewModel ToReadViewModel(FeaturedPromptViewModel vm);
  
  // ✅ REQUIS pour Create
  public static DomainModel ToDomainModelForCreate(FeaturedPromptViewModel vm, Guid userId);
  public static FeaturedPromptViewModel CreateForNew();
  
  // ✅ REQUIS pour Update
  public static DomainModel ToDomainModelForUpdate(FeaturedPromptViewModel vm, Guid userId);
  
  // ✅ REQUIS pour Delete
  // Généralement pas besoin de méthode spéciale, juste l'ID
}
```

Si méthodes manquantes : **CRÉER D'ABORD** avec tests unitaires

### 2.2 Cycle RED-GREEN-REFACTOR pour chaque Page

#### 🔴 RED : Écrire les Tests qui Échouent

Avant de toucher au code de la page, créer les tests dans :
`GroupeAdp.GenAi.Hostings.WebApp.Management.Endpoint.Unit.Tests/Features/[Module]/Pages/[PageName]Tests.cs`

**Tests obligatoires pour chaque page** :

##### Pour List.razor.cs :
```csharp
[Fact]
public async Task OnInitializedAsync_WhenCalled_ShouldLoadItems()

[Fact]
public async Task OnInitializedAsync_WhenMediatorFails_ShouldDisplayError()

[Fact]
public async Task OnSearchChanged_WhenSearchTermProvided_ShouldFilterItems()

[Fact]
public async Task OnPageChanged_WhenPageChanged_ShouldLoadNewPage()

[Fact]
public async Task OnSortChanged_WhenSortChanged_ShouldReorderItems()

[Fact]
public async Task OnDeleteClicked_WhenUserConfirms_ShouldDeleteItem()

[Fact]
public async Task OnDeleteClicked_WhenUserCancels_ShouldNotDelete()

[Fact]
public async Task ReloadAsync_WhenCalled_ShouldRefreshData()
```

##### Pour Create.razor.cs :
```csharp
[Fact]
public async Task OnInitializedAsync_WhenCalled_ShouldInitializeModel()

[Fact]
public async Task OnValidSubmit_WhenModelValid_ShouldCreateEntityAndNavigate()

[Fact]
public async Task OnValidSubmit_WhenModelInvalid_ShouldDisplayErrors()

[Fact]
public async Task OnValidSubmit_WhenMediatorFails_ShouldDisplayError()

[Fact]
public async Task OnValidSubmit_WhenUnauthorized_ShouldDenyAccess()

[Fact]
public async Task OnCancelClicked_WhenCalled_ShouldNavigateBack()

[Fact]
public async Task Validation_WhenRequiredFieldsEmpty_ShouldFail()

[Fact]
public async Task Validation_WhenDataInvalid_ShouldShowMessages()
```

##### Pour Read.razor.cs :
```csharp
[Fact]
public async Task OnInitializedAsync_WhenIdValid_ShouldLoadEntity()

[Fact]
public async Task OnInitializedAsync_WhenIdInvalid_ShouldDisplayError()

[Fact]
public async Task OnInitializedAsync_WhenEntityNotFound_ShouldDisplay404()

[Fact]
public async Task OnInitializedAsync_WhenUnauthorized_ShouldDenyAccess()

[Fact]
public async Task OnEditClicked_WhenUserHasPermission_ShouldNavigateToEdit()

[Fact]
public async Task OnDeleteClicked_WhenUserHasPermission_ShouldNavigateToDelete()

[Fact]
public async Task Permissions_WhenUserNotApprover_ShouldHideEditDeleteButtons()
```

##### Pour Update.razor.cs :
```csharp
[Fact]
public async Task OnInitializedAsync_WhenIdValid_ShouldLoadEntityForEdit()

[Fact]
public async Task OnInitializedAsync_WhenIdInvalid_ShouldDisplayError()

[Fact]
public async Task OnValidSubmit_WhenModelValid_ShouldUpdateEntityAndNavigate()

[Fact]
public async Task OnValidSubmit_WhenModelInvalid_ShouldDisplayErrors()

[Fact]
public async Task OnValidSubmit_WhenConcurrencyConflict_ShouldHandleETag()

[Fact]
public async Task OnValidSubmit_WhenMediatorFails_ShouldDisplayError()

[Fact]
public async Task OnValidSubmit_WhenUnauthorized_ShouldDenyAccess()

[Fact]
public async Task OnCancelClicked_WhenCalled_ShouldNavigateBack()

[Fact]
public async Task Validation_WhenRequiredFieldsEmpty_ShouldFail()
```

##### Pour Delete.razor.cs :
```csharp
[Fact]
public async Task OnInitializedAsync_WhenIdValid_ShouldLoadEntityForDeletion()

[Fact]
public async Task OnInitializedAsync_WhenIdInvalid_ShouldDisplayError()

[Fact]
public async Task OnConfirmDelete_WhenUserConfirms_ShouldDeleteAndNavigate()

[Fact]
public async Task OnConfirmDelete_WhenMediatorFails_ShouldDisplayError()

[Fact]
public async Task OnConfirmDelete_WhenUnauthorized_ShouldDenyAccess()

[Fact]
public async Task OnCancelClicked_WhenCalled_ShouldNavigateBack()

[Fact]
public async Task ConfirmDialog_WhenEntityHasRelations_ShouldWarnUser()
```

**Exécuter les tests** → Ils doivent **ÉCHOUER** (RED)

#### 🟢 GREEN : Implémenter le Code Minimum

Maintenant, modifier la page pour faire passer les tests :

##### Exemple : Finaliser FeaturedPrompts/Create.razor.cs

**AVANT** (simulation) :
```csharp
// IMPLEMENTATION_NOTE: When Management domain for FeaturedPrompts is available:
// 1. Create CreateFeaturedPromptCommand in Management.FeaturedPrompts.Handlers.Commands
// 2. Replace simulation with: var command = FeaturedPromptViewModelHelper.ToDomainModelForCreate(Model, CurrentUserService.GetCurrentUserIdOrEmpty());
// 3. Replace simulation with: var result = await MediatorAdapter.Send(new CreateFeaturedPromptCommand(command));
// For now, we'll simulate the creation

await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
```

**APRÈS** (implémentation réelle) :
```csharp
private async Task<bool> HandleSubmitAsync(CancellationToken cancellationToken)
{
  try
  {
    _isSubmitting = true;

    // 1. Convert ViewModel to Domain Model
    var domainModel = FeaturedPromptViewModelHelper.ToDomainModelForCreate(
      Model, 
      CurrentUserService.GetCurrentUserIdOrEmpty()
    );

    // 2. Create Command
    var command = new CreateFeaturedPromptCommand
    {
      FeaturedPrompt = domainModel
    };

    // 3. Send via MediatR
    var result = await MediatorAdapter.Send(command, cancellationToken).ConfigureAwait(false);

    // 4. Handle Result
    if (result.IsSuccess)
    {
      Snackbar.Add(
        string.Format(ManagementResources.FeaturedPrompt_Created_Success, Model.PromptTitle),
        Severity.Success
      );
      NavigationManager.NavigateTo(ManagementRoutes.FeaturedPrompts_List);
      return true;
    }
    else
    {
      Snackbar.Add(
        result.ErrorMessage ?? ManagementResources.FeaturedPrompt_Created_Error,
        Severity.Error
      );
      return false;
    }
  }
  catch (OperationCanceledException)
  {
    Logger.LogInformation("FeaturedPrompt creation cancelled by user");
    return false;
  }
  catch (Exception ex)
  {
    Logger.LogError(ex, "Error creating FeaturedPrompt: {PromptId}", Model.PromptId);
    Snackbar.Add(
      ManagementResources.FeaturedPrompt_Created_Exception,
      Severity.Error
    );
    return false;
  }
  finally
  {
    _isSubmitting = false;
    await InvokeAsync(StateHasChanged).ConfigureAwait(false);
  }
}
```

**Exécuter les tests** → Ils doivent **PASSER** (GREEN)

#### 🔵 REFACTOR : Améliorer la Qualité

Une fois les tests verts, améliorer le code :

1. **Éliminer duplication** : Extraire dans helpers si code répété
2. **Améliorer lisibilité** : Noms explicites, extraction méthodes
3. **Optimiser performance** : Async, caching si pertinent
4. **Respecter conventions** : Nommage ADR-013, style guide
5. **Ajouter logging** : Structured logging avec contexte
6. **Gérer erreurs** : Try-catch avec Result Pattern

**Exécuter les tests** → Ils doivent **RESTER VERTS**

### 2.3 Validation par Mutation Testing

Après chaque page finalisée :

```bash
# Naviguer vers le projet de tests
cd GroupeAdp.GenAi.Hostings.WebApp.Management.Endpoint.Unit.Tests

# Exécuter Stryker.NET sur le module spécifique
dotnet stryker --project ../GroupeAdp.GenAi.Hostings.WebApp.Management.Endpoint.csproj \
  --target-file-filter "**/Features/[Module]/Pages/[Page].razor.cs" \
  --reporter html --reporter json --reporter progress \
  --verbosity info
```

**Critères d'acceptance** :
- ✅ Mutation Score ≥ 85%
- ✅ Aucun mutant survivant critique
- ✅ Documentation des mutants intentionnellement ignorés

Si mutation score < 85% :
1. Identifier les mutants survivants
2. Ajouter tests pour tuer les mutants
3. Relancer Stryker jusqu'à atteindre 85%+

## Phase 3 : Finalisation Globale et Validation

### 3.1 Revue Complète des Helpers ViewModels

Vérifier que **TOUS** les helpers sont complets et testés :

```bash
# Lister tous les helpers
find Features/ -name "*ViewModelHelper.cs" -o -name "*ViewModelExtensions.cs"
```

Pour chaque helper :
- ✅ Toutes les méthodes de conversion implémentées
- ✅ Tests unitaires pour chaque méthode (≥80% coverage)
- ✅ Gestion des valeurs nulles/edge cases
- ✅ Documentation XML complète

### 3.2 Résolution des TODOs Restants

```bash
# Lister TOUS les TODOs
grep -r "TODO" --include="*.cs" Features/ > todos_remaining.txt
grep -r "FIXME" --include="*.cs" Features/ >> todos_remaining.txt
grep -r "IMPLEMENTATION_NOTE" --include="*.cs" Features/ >> todos_remaining.txt
```

Pour chaque TODO :
1. **Évaluer** : Critique / Important / Nice to have
2. **Traiter ou documenter** : Corriger ou créer ticket backlog
3. **Supprimer le commentaire** une fois résolu

**Objectif** : 0 TODO/FIXME dans le code final

### 3.3 Validation des Routes et Navigation

Vérifier dans `Features/Shared/Constants/ManagementRoutes.cs` :

```csharp
// Pour chaque module, vérifier que toutes les routes existent et sont cohérentes
[Module]_List           → "/[module]"
[Module]_Create         → "/[module]/create"
[Module]_Read           → "/[module]/read/{Id:guid}"
[Module]_Update         → "/[module]/update/{Id:guid}"
[Module]_Delete         → "/[module]/delete/{Id:guid}"
```

Tester manuellement :
- [ ] Navigation List → Create
- [ ] Navigation List → Read → Update
- [ ] Navigation List → Delete
- [ ] Retour arrière (Cancel buttons)
- [ ] Paramètres d'URL corrects

### 3.4 Validation Permissions et Sécurité

Pour **chaque page**, vérifier :

#### Authorization
```csharp
[Authorize] // ✅ Attribut présent sur toutes les pages sensibles
```

#### Permission Checks
```csharp
// Dans Read.razor.cs
var canEdit = CurrentUserService.IsCurrentUserApprover();
var canDelete = CurrentUserService.IsCurrentUserApprover();

// Dans les templates Razor
@if (ReadModel.CanEdit)
{
  <MudButton OnClick="NavigateToEdit">Edit</MudButton>
}
```

#### Tests de sécurité
```csharp
[Fact]
public async Task Create_WhenUserNotApprover_ShouldDenyAccess()

[Fact]
public async Task Update_WhenUserNotOwnerNorApprover_ShouldDenyAccess()

[Fact]
public async Task Delete_WhenUserNotApprover_ShouldDenyAccess()
```

### 3.5 Build et Tests Complets

#### Étape 1 : Build Solution
```bash
dotnet build GroupeAdp.Genai.sln --configuration Release
```
**Critère** : 0 erreurs, 0 warnings

#### Étape 2 : Exécuter TOUS les Tests
```bash
dotnet test GroupeAdp.Genai.sln \
  --configuration Release \
  --logger "trx" \
  --results-directory TestResults \
  --collect "XPlat Code Coverage"
```
**Critère** : 100% tests passants

#### Étape 3 : Vérifier la Couverture
```bash
# Générer rapport de couverture
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:"TestResults/CoverageReport" \
  -reporttypes:"Html;TextSummary"
```
**Critère** : 
- Management.Endpoint ≥ 80% coverage
- Pas de classes critiques < 70%

#### Étape 4 : Mutation Testing Global
```bash
dotnet stryker \
  --solution GroupeAdp.Genai.sln \
  --project GroupeAdp.GenAi.Hostings.WebApp.Management.Endpoint.csproj \
  --reporter html --reporter json --reporter progress \
  --verbosity info \
  --concurrency 6
```
**Critère** : Mutation Score ≥ 85% sur l'ensemble du projet

### 3.6 Tests Manuels Exploratoires

Démarrer l'application et tester manuellement :

```bash
# Terminal 1 : WebApi
cd GroupeAdp.GenAi.Hostings.WebApi.Default.Endpoint
dotnet run

# Terminal 2 : Management WebApp
cd GroupeAdp.GenAi.Hostings.WebApp.Management.Endpoint
dotnet run
```

**Checklist par module** :

#### Collections Management
- [ ] Créer nouvelle collection
- [ ] Modifier collection existante
- [ ] Supprimer collection (avec confirmation)
- [ ] Filtrer/rechercher collections
- [ ] Trier par colonnes
- [ ] Pagination fonctionne
- [ ] Voir détails collection
- [ ] Messages d'erreur appropriés

#### Prompts Management
- [ ] Créer nouveau prompt
- [ ] Modifier prompt existant
- [ ] Supprimer prompt
- [ ] Recherche full-text
- [ ] Filtres avancés
- [ ] Voir statistiques

#### Tags Management
- [ ] Créer tag
- [ ] Approuver tag (si approver)
- [ ] Modifier tag
- [ ] Supprimer tag
- [ ] Voir usage count

#### FeaturedPrompts Management
- [ ] Mettre prompt en avant
- [ ] Modifier ordre (DisplayOrder)
- [ ] Activer/Désactiver
- [ ] Retirer de la une
- [ ] Visualiser aperçu

#### FeaturedCollections Management
- [ ] Mettre collection en avant
- [ ] Modifier ordre
- [ ] Activer/Désactiver
- [ ] Retirer de la une

#### FeaturedTags Management
- [ ] Mettre tag en avant
- [ ] Modifier ordre
- [ ] Activer/Désactiver
- [ ] Retirer de la une

#### CollectionPermissions Management
- [ ] Assigner permission utilisateur
- [ ] Assigner permission groupe
- [ ] Modifier permission
- [ ] Révoquer permission
- [ ] Voir permissions héritées

#### Favorites Management
- [ ] Ajouter prompt en favoris
- [ ] Retirer prompt des favoris
- [ ] Ajouter collection en favoris
- [ ] Retirer collection des favoris
- [ ] Ajouter tag en favoris
- [ ] Retirer tag des favoris
- [ ] Voir liste favorites

### 3.7 Tests de Performance

Si applicable, vérifier les performances :

```csharp
// Exemple : Benchmark pour chargement de listes
[Fact]
public async Task List_LoadWith1000Items_ShouldComplete_InLessThan2Seconds()
{
  // Arrange
  var stopwatch = Stopwatch.StartNew();
  
  // Act
  await Page.OnInitializedAsync();
  stopwatch.Stop();
  
  // Assert
  Check.That(stopwatch.ElapsedMilliseconds).IsLessOrEqualThan(2000);
}
```

Critères de performance :
- [ ] List pages : < 2s pour 1000 items
- [ ] Create/Update : < 500ms submit
- [ ] Search : < 1s pour résultats
- [ ] Pagination : < 500ms changement page

## Phase 4 : Documentation et Livraison

### 4.1 Mettre à Jour la Documentation

#### Documentation Fonctionnelle
Créer/Mettre à jour dans `documentations/functionnals/` :

```markdown
# Management.Endpoint - Guide Utilisateur

## Vue d'ensemble
Le Management.Endpoint permet aux administrateurs et approvers de gérer :
- Collections de prompts
- Prompts individuels
- Tags et catégories
- Contenu mis en avant (Featured)
- Permissions d'accès
- Favoris utilisateurs

## Modules Disponibles

### Collections Management
[Description, captures d'écran, workflows]

### Prompts Management
[Description, captures d'écran, workflows]

### Tags Management
[Description, captures d'écran, workflows]

[etc. pour chaque module]
```

#### Documentation Technique
Créer/Mettre à jour dans `documentations/technicals/` :

```markdown
# Management.Endpoint - Architecture Technique

## Architecture
- **Pattern** : Onion Architecture, CQRS, MediatR
- **UI Framework** : Blazor Server + MudBlazor
- **State Management** : Component-based
- **Validation** : FluentValidation
- **Permissions** : Role-based + custom policies

## Structure des Pages
Chaque module suit le pattern CRUD standardisé :
- `List.razor.cs` : Liste paginée, filtrable, triable
- `Create.razor.cs` : Formulaire création avec validation
- `Read.razor.cs` : Vue détail en lecture seule
- `Update.razor.cs` : Formulaire édition avec gestion ETag
- `Delete.razor.cs` : Confirmation suppression

## Conventions
[Détails conventions nommage, helpers, patterns]

## Diagrammes
[Diagrammes d'architecture, flux de données]
```

### 4.2 Créer/Mettre à Jour ADRs si Nécessaire

Si des décisions architecturales importantes ont été prises :

```markdown
# ADR-XXX : Finalisation Management.Endpoint Pages

## Statut
Accepté

## Contexte
Plusieurs pages du Management.Endpoint utilisaient des simulations (Task.Delay, données mockées) 
au lieu d'implémentations réelles avec MediatR.

## Décision
Toutes les pages ont été finalisées avec :
1. Appels MediatR réels vers les handlers Domain
2. Helpers ViewModels complets pour conversions
3. Gestion d'erreurs robuste avec Result Pattern
4. Tests unitaires avec couverture ≥80% et mutation score ≥85%

## Conséquences
### Positives
- Application production-ready
- Qualité code élevée
- Tests robustes garantissant non-régression
- Maintenance facilitée

### Négatives
- Aucune
```

### 4.3 Changelog et Release Notes

Mettre à jour `CHANGELOG.md` :

```markdown
## [Version X.Y.Z] - 2025-11-24

### ✨ Features Finalisées
- **FeaturedPrompts** : Implémentation complète Create/Update/Read (remplace simulations)
- **FeaturedCollections** : Finalisation tous CRUD operations
- **FeaturedTags** : Pages management opérationnelles
- **Collections ViewModels** : Ajout CreatedByDisplayName
- **Permissions** : Validation rôles et autorisations

### 🧪 Tests
- Couverture globale Management.Endpoint : XX%
- Mutation score : XX%
- Tous les modules avec tests complets

### 📚 Documentation
- Guide utilisateur Management.Endpoint
- Documentation technique architecture
- ADR-XXX : Finalisation pages

### 🐛 Bugs Corrigés
- [Liste bugs résolus pendant finalisation]

### ⚡ Performances
- Optimisation chargement listes (pagination)
- Réduction requêtes N+1
```

### 4.4 Checklist Finale de Livraison

Avant de considérer le travail terminé, valider :

#### Code Quality
- [ ] 0 erreurs de build
- [ ] 0 warnings (ou warnings documentés/justifiés)
- [ ] 0 TODOs/FIXMEs dans le code
- [ ] Tous les commentaires `IMPLEMENTATION_NOTE` supprimés
- [ ] Respecte conventions nommage (ADR-013)
- [ ] Code review effectué

#### Tests
- [ ] 100% des tests passants
- [ ] Couverture ≥ 80% sur Management.Endpoint
- [ ] Mutation score ≥ 85%
- [ ] Tests d'intégration réussis
- [ ] Tests manuels exploratoires effectués

#### Fonctionnalités
- [ ] Toutes les pages CRUD fonctionnelles
- [ ] Navigation entre pages opérationnelle
- [ ] Gestion d'erreurs robuste
- [ ] Messages utilisateur appropriés
- [ ] Permissions correctement appliquées
- [ ] Validation formulaires complète

#### Performance
- [ ] Pas de requêtes N+1
- [ ] Pagination implémentée
- [ ] Temps réponse acceptables (< 2s lists, < 500ms actions)
- [ ] Optimisations DB queries

#### Sécurité
- [ ] Attributs [Authorize] sur pages sensibles
- [ ] Validation permissions utilisateur
- [ ] Pas de secrets hardcodés
- [ ] Validation inputs côté serveur
- [ ] Protection CSRF

#### Documentation
- [ ] README.md à jour
- [ ] Guide utilisateur complet
- [ ] Documentation technique
- [ ] ADRs créés/mis à jour
- [ ] CHANGELOG.md mis à jour
- [ ] Release notes rédigées

#### CI/CD
- [ ] Pipeline build réussit
- [ ] Pipeline tests réussit
- [ ] Artefacts générés correctement
- [ ] Déploiement staging validé

## Phase 5 : Améliorations Continues (Post-Livraison)

### 5.1 Monitoring et Feedback

Après déploiement :
- Collecter feedback utilisateurs
- Monitorer logs erreurs
- Analyser métriques performance
- Identifier points amélioration

### 5.2 Backlog Optimisations

Créer tickets pour :
- Refactoring code smell mineurs
- Optimisations performance supplémentaires
- Features nice-to-have
- Améliorations UX

### 5.3 Maintenance Continue

Planifier :
- Revue code régulière
- Mise à jour dépendances
- Amélioration couverture tests (viser 90%+)
- Amélioration mutation score (viser 90%+)

---

# Résumé de la Méthodologie

## 🎯 Objectifs Clairs
- ✅ Remplacer toutes les simulations par implémentations réelles
- ✅ Compléter toutes les fonctionnalités manquantes
- ✅ Atteindre ≥80% couverture tests, ≥85% mutation score
- ✅ 0 TODOs/FIXMEs restants
- ✅ Documentation complète
- ✅ Application production-ready

## 📋 Processus Systématique
1. **Audit Initial** : Inventorier tous les problèmes
2. **Priorisation** : P0 → P1 → P2 → P3
3. **Implémentation TDD** : RED → GREEN → REFACTOR
4. **Validation Continue** : Tests + Mutation Testing
5. **Finalisation Globale** : Build + Tests + Docs
6. **Livraison** : Checklist complète validée

## ⚡ Principes Fondamentaux
- **Pragmatisme** : Solutions simples et efficaces
- **Qualité** : Tests robustes, code maintenable
- **Incrémental** : Tâche atomique par tâche atomique
- **Traçabilité** : Tout documenté et testé
- **Non-régression** : Tests automatisés garantissent stabilité

---

**🚀 Prêt à finaliser le Management.Endpoint !**
