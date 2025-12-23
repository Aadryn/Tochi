---
description: Architecture de composants MudBlazor - Découpage, responsabilité unique, state management, performance
name: MudBlazor_Architecture
applyTo: "**/backend/Presentation/**/*.razor,**/backend/Presentation/**/*.razor.cs"
---

# MudBlazor - Architecture et Découpage des Composants

## ⛔ À NE PAS FAIRE

- **Ne crée jamais** de composant >150 lignes sans découper
- **Ne mélange jamais** logique métier et affichage dans un même composant
- **Ne passe jamais** >5 paramètres à un composant (extraire en objet ou découper)
- **Ne duplique jamais** le markup MudBlazor - extraire en composant réutilisable
- **N'utilise jamais** de state global pour des données locales au composant
- **Ne crée jamais** de composant "God Object" avec multiples responsabilités
- **N'appelle jamais** des services directement depuis le composant présentation

## ✅ À FAIRE

- **Sépare toujours** Container (logique) et Presenter (affichage)
- **Crée toujours** des composants petits et focalisés (<150 lignes)
- **Utilise toujours** `[Parameter]` pour les données descendantes
- **Utilise toujours** `EventCallback<T>` pour remonter les événements
- **Extrais toujours** les fragments répétitifs en `RenderFragment` ou composants
- **Préfère toujours** les composants stateless pour la présentation
- **Encapsule toujours** la logique métier dans des services injectés

## 📐 Principe de Responsabilité Unique

### Éviter les Composants "God Object"

**Règles de découpage :**
- ✅ Un composant = Une responsabilité claire
- ✅ Extraire les groupes MudBlazor cohérents dans des composants enfants
- ✅ Préférer des composants **stateless** pour l'affichage (données via `[Parameter]`)
- ✅ Encapsuler la logique métier dans des services ou composants conteneurs
- ✅ Partager les fragments répétitifs via `RenderFragment`

**Exemple de découpage :**

```razor
<!-- ❌ MAUVAIS : Composant monolithique -->
<MudPaper>
  <MudStack Spacing="3">
    <!-- 200 lignes de markup mélangées -->
    <MudText>...</MudText>
    <MudTextField>...</MudTextField>
    <!-- ... -->
  </MudStack>
</MudPaper>

<!-- ✅ BON : Composants découpés -->
<PromptListPage>
  <PromptToolbar OnSearch="@HandleSearch" OnFilter="@HandleFilter"/>
  <PromptGrid Items="@filteredItems" OnEdit="@HandleEdit"/>
  <PromptPagination CurrentPage="@page" TotalPages="@totalPages"/>
</PromptListPage>
```

### Organisation par Couches

```
Components/
├── Foundation/          # Briques UI génériques (réutilisables partout)
│   ├── EnhancedButton/
│   │   ├── EnhancedButton.razor
│   │   ├── EnhancedButton.razor.cs
│   │   └── EnhancedButton.razor.css
│   ├── StatChip/
│   │   ├── StatChip.razor
│   │   ├── StatChip.razor.cs
│   │   └── README.md
│   └── StatusBadge/
│       ├── StatusBadge.razor
│       ├── StatusBadge.razor.cs
│       └── README.md
│
├── Composition/         # Assemblages d'interactions (composants composés)
│   ├── Toolbar/
│   │   ├── Toolbar.razor
│   │   ├── Toolbar.razor.cs
│   │   └── README.md
│   ├── DynamicForm/
│   │   ├── DynamicForm.razor
│   │   ├── DynamicForm.razor.cs
│   │   └── README.md
│   └── EnrichedCard/
│       ├── EnrichedCard.razor
│       ├── EnrichedCard.razor.cs
│       └── README.md
│
└── Features/            # Composants métier (domaine spécifique)
    ├── PromptManagement/
    │   ├── PromptList.razor
    │   ├── PromptList.razor.cs
    │   ├── PromptToolbar.razor
    │   ├── PromptGrid.razor
    │   └── README.md
    ├── CollectionFilter/
    │   ├── CollectionFilterPanel.razor
    │   └── CollectionFilterChip.razor
    └── AnalyticsDashboard/
        ├── DashboardSummary.razor
        └── DashboardChart.razor
```

### Nommage des Composants

**✅ Noms descriptifs basés sur le rôle :**
```
PromptToolbar           (Toolbar pour les prompts)
CollectionFilterChip    (Chip de filtre pour collections)
AnalyticsDashboard      (Dashboard d'analytique)
StatCard                (Card de statistique)
UserProfileMenu         (Menu de profil utilisateur)
```

**❌ JAMAIS utiliser les termes Atomic Design :**
```
PromptAtom              ❌ Non
CollectionMolecule      ❌ Non
DashboardOrganism       ❌ Non
```

## 📦 Documentation des Composants Partagés

### README.md Obligatoire

**Chaque composant partagé (Foundation, Composition) DOIT avoir un `README.md` :**

```markdown
# StatChip

Chip de statistique avec icône et valeur numérique.

## Paramètres

| Paramètre | Type | Obligatoire | Description |
|-----------|------|-------------|-------------|
| `Icon` | `string` | ✅ | Icône Material Design |
| `Value` | `int` | ✅ | Valeur numérique à afficher |
| `Label` | `string` | ✅ | Libellé localisé |
| `Color` | `Color` | ❌ | Couleur du chip (défaut: Info) |

## Exemples

```razor
<StatChip Icon="@Icons.Material.Filled.Collections" 
          Value="156" 
          Label="@Localizer["Dashboard.Collections"]"/>
```

## Dépendances

- MudBlazor : MudChip, MudIcon
- IStringLocalizer (pour Label)

## Notes

- Le composant est **stateless**
- Optimisé pour performance (pas de re-render inutile)
```

### Structure Minimal d'un Composant Partagé

```razor
<!-- StatChip.razor -->
<MudChip T="string" 
         Icon="@Icon" 
         Color="@Color" 
         Size="Size.Small"
         Class="chip-stat">
  @Value
</MudChip>
```

```csharp
// StatChip.razor.cs
namespace GroupeAdp.GenAi.Components.Foundation;

public partial class StatChip
{
  [Parameter, EditorRequired]
  public string Icon { get; set; } = string.Empty;
  
  [Parameter, EditorRequired]
  public int Value { get; set; }
  
  [Parameter, EditorRequired]
  public string Label { get; set; } = string.Empty;
  
  [Parameter]
  public Color Color { get; set; } = Color.Info;
}
```

## 🔄 State Management et Données

### Gestion des Formulaires

```razor
<!-- ✅ BON : MudForm avec synchronisation explicite -->
<MudForm @ref="form" @bind-IsValid="@isValid">
  <MudTextField @bind-Value="model.Name" 
                Label="@Localizer["User.Name"]"
                Required="true" />
</MudForm>
```

```csharp
// Code-behind
[Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }

private MudForm form;
private bool isValid;

private async Task SubmitAsync()
{
  await form.Validate();
  if (!isValid) return;
  
  // Traitement
  await SaveAsync();
}
```

### EventCallback pour Communication

```csharp
// ✅ TOUJOURS utiliser EventCallback au lieu de Action/Func
[Parameter] public EventCallback<string> OnSearchChanged { get; set; }
[Parameter] public EventCallback<PromptDto> OnItemSelected { get; set; }

private async Task HandleSearchAsync(string searchTerm)
{
  // InvokeAsync pour contexte Blazor synchrone
  await OnSearchChanged.InvokeAsync(searchTerm);
}

private async Task HandleItemClickAsync(PromptDto item)
{
  await OnItemSelected.InvokeAsync(item);
}
```

**Exemple d'utilisation :**

```razor
<!-- Composant Parent -->
<PromptToolbar OnSearchChanged="@HandleSearchAsync" />

@code {
  private async Task HandleSearchAsync(string searchTerm)
  {
    // Traiter la recherche
    await LoadItemsAsync(searchTerm);
  }
}
```

### ViewModels pour États Complexes

```csharp
// Services/ViewModels/PromptListViewModel.cs
public class PromptListViewModel
{
  public string SearchTerm { get; set; } = string.Empty;
  public int CurrentPage { get; set; } = 1;
  public int PageSize { get; set; } = 20;
  public List<string> SelectedIds { get; set; } = new();
  public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
  public string SortColumn { get; set; } = "CreatedAt";
}
```

```csharp
// Composant - Injection scoped
[Inject] private PromptListViewModel ViewModel { get; set; }

protected override void OnInitialized()
{
  // Le ViewModel persiste entre navigations dans la même portée
  searchTerm = ViewModel.SearchTerm;
  currentPage = ViewModel.CurrentPage;
}

private async Task HandleSearchAsync(string term)
{
  ViewModel.SearchTerm = term;
  ViewModel.CurrentPage = 1; // Reset pagination
  await LoadItemsAsync();
}
```

## ⚡ Performance et Réactivité

### Virtualisation pour Listes Volumineuses

```razor
<!-- ✅ Utiliser MudVirtualize pour > 100 items -->
<MudVirtualize Items="@items" Context="item">
  <MudListItem>
    @item.Name
  </MudListItem>
</MudVirtualize>

<!-- ✅ Utiliser ServerData paginé pour DataGrid -->
<MudDataGrid T="PromptDto" 
             ServerData="@LoadDataAsync"
             Virtualize="true">
  <Columns>
    <PropertyColumn Property="x => x.Title"/>
    <PropertyColumn Property="x => x.Description"/>
  </Columns>
</MudDataGrid>
```

```csharp
// Code-behind
private async Task<GridData<PromptDto>> LoadDataAsync(GridState<PromptDto> state)
{
  var items = await _service.GetPagedAsync(
    page: state.Page,
    pageSize: state.PageSize,
    sortLabel: state.SortLabel,
    sortDirection: state.SortDirection
  );
  
  return new GridData<PromptDto>
  {
    Items = items.Data,
    TotalItems = items.TotalCount
  };
}
```

### Contrôle des Re-renders

```csharp
// Code-behind
protected override bool ShouldRender()
{
  // Rendre seulement si paramètres critiques ont changé
  return _dataChanged || _stateChanged;
}

protected override void OnParametersSet()
{
  // Détecter les changements de paramètres
  if (Items != _previousItems)
  {
    _dataChanged = true;
    _previousItems = Items;
  }
}
```

### Debounce sur Entrées Utilisateur

```razor
<MudTextField @bind-Value="searchTerm"
              Label="@Localizer["Common.Search"]"
              DebounceInterval="500"
              OnDebounceIntervalElapsed="@HandleSearchAsync"
              Immediate="false"/>
```

```csharp
// Code-behind
private string searchTerm = string.Empty;

private async Task HandleSearchAsync(string term)
{
  // Exécuté 500ms après la dernière frappe
  await LoadItemsAsync(term);
}
```

### Utilisation de @key pour Stabilité DOM

```razor
<MudStack Spacing="2">
  @foreach (var item in items)
  {
    <!-- ✅ @key stabilise le DOM lors des mises à jour -->
    <PromptCard @key="item.Id" Item="@item" OnEdit="@HandleEdit"/>
  }
</MudStack>
```

## 🔒 Sécurité et Robustesse

### Validation et Sanitation

```csharp
// ❌ JAMAIS afficher du HTML non maîtrisé
<MudText>@((MarkupString)userInput)</MudText>  <!-- DANGEREUX -->

// ✅ TOUJOURS valider et échapper
<MudText>@userInput</MudText>  <!-- Échappé automatiquement -->

// ✅ TOUJOURS utiliser DataAnnotations
public class PromptCreateModel
{
  [Required(ErrorMessage = "Title is required")]
  [StringLength(200, ErrorMessage = "Title max 200 characters")]
  public string Title { get; set; }
  
  [Required]
  [StringLength(2000)]
  public string Description { get; set; }
  
  [EmailAddress]
  public string ContactEmail { get; set; }
}
```

### Gestion des Secrets

```csharp
// ❌ Ne JAMAIS logger d'informations sensibles
_logger.LogInformation("User {Email} logged in with password {Password}", email, password);

// ✅ Logger uniquement des substituts
_logger.LogInformation("User {UserId} logged in successfully", userId);

// ✅ Chiffrer les secrets côté serveur
// ✅ Manipuler uniquement des ReferenceId côté composant
[Parameter] public string ApiKeyReferenceId { get; set; }
```

### Anti-Forgery et CSRF

```razor
<!-- Page avec formulaire -->
@page "/prompts/create"
@attribute [ValidateAntiForgeryToken]

<MudForm @ref="form" OnValidSubmit="@SubmitAsync">
  <!-- Formulaire -->
</MudForm>
```

### InputType Appropriés

```razor
<!-- ✅ InputType pour sécurité -->
<MudTextField @bind-Value="model.Email" 
              Label="@Localizer["User.Email"]"
              InputType="InputType.Email"/>

<MudTextField @bind-Value="model.Password" 
              Label="@Localizer["User.Password"]"
              InputType="InputType.Password"/>

<MudTextField @bind-Value="model.Website" 
              Label="@Localizer["User.Website"]"
              InputType="InputType.Url"/>
```

## 🧪 Tests et Maintenance

### Tests avec bUnit

```csharp
// Tests/Components/StatChipTests.cs
using Bunit;
using FluentAssertions;
using Xunit;

public class StatChipTests : TestContext
{
  public StatChipTests()
  {
    Services.AddMudServices();
  }
  
  [Fact]
  public void StatChip_ShouldRender_WithCorrectValue()
  {
    // Arrange
    var icon = Icons.Material.Filled.Collections;
    var value = 156;
    var label = "Collections";
    
    // Act
    var cut = RenderComponent<StatChip>(parameters => parameters
      .Add(p => p.Icon, icon)
      .Add(p => p.Value, value)
      .Add(p => p.Label, label));
    
    // Assert
    cut.Find("[data-test='chip-value']").TextContent.Should().Be("156");
    cut.Find("[data-test='chip-label']").TextContent.Should().Be("Collections");
  }
  
  [Fact]
  public void StatChip_ShouldUse_DefaultColor_WhenNotSpecified()
  {
    // Arrange & Act
    var cut = RenderComponent<StatChip>(parameters => parameters
      .Add(p => p.Icon, Icons.Material.Filled.Info)
      .Add(p => p.Value, 10)
      .Add(p => p.Label, "Test"));
    
    // Assert
    var chip = cut.FindComponent<MudChip<string>>();
    chip.Instance.Color.Should().Be(Color.Info);
  }
}
```

### Data Attributes pour Tests

```razor
<!-- ✅ Ajouter data-test pour ciblage stable -->
<MudButton data-test="submit-button" OnClick="@SubmitAsync">
  @Localizer["Common.Submit"]
</MudButton>

<MudTextField data-test="search-input" 
              @bind-Value="searchTerm"
              Label="@Localizer["Common.Search"]"/>

<MudDataGrid data-test="prompts-grid" Items="@items">
  <!-- Colonnes -->
</MudDataGrid>
```

## 📋 Checklist Architecture

### ✅ Découpage des Composants
- [ ] Un composant = Une responsabilité unique
- [ ] Composants Foundation pour briques réutilisables
- [ ] Composants Composition pour assemblages
- [ ] Composants Features pour domaine métier
- [ ] Nommage descriptif basé sur le rôle

### ✅ Documentation
- [ ] README.md pour chaque composant partagé
- [ ] Paramètres documentés avec types et descriptions
- [ ] Exemples d'utilisation fournis
- [ ] Dépendances listées

### ✅ State Management
- [ ] EventCallback pour communication parent-enfant
- [ ] ViewModel pour états complexes (injection scoped)
- [ ] MudForm pour validation de formulaires
- [ ] @key sur listes pour stabilité DOM

### ✅ Performance
- [ ] Virtualisation pour listes > 100 items
- [ ] Debounce sur inputs intensifs (500ms)
- [ ] ShouldRender() implémenté si nécessaire
- [ ] ServerData paginé pour DataGrid volumineuses

### ✅ Sécurité
- [ ] Validation DataAnnotations sur tous les modèles
- [ ] Aucun MarkupString sans sanitation
- [ ] InputType appropriés (Password, Email, Url)
- [ ] [ValidateAntiForgeryToken] sur pages avec formulaires
- [ ] Aucune information sensible loggée

### ✅ Tests
- [ ] Tests bUnit pour composants Foundation et Composition
- [ ] data-test attributes sur éléments interactifs
- [ ] Tests d'intégration pour Features
- [ ] Couverture > 80% pour composants critiques

## 📚 Ressources

### Documentation Officielle
- [Blazor Component Lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle)
- [Blazor Event Handling](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling)
- [bUnit Documentation](https://bunit.dev/)
- [FluentValidation with Blazor](https://docs.fluentvalidation.net/en/latest/blazor.html)

### Patterns et Best Practices
- [Component Architecture Guidelines](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/)
- [State Management in Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management)
