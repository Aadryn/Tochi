---
applyTo: "**/*.razor,**/*.razor.cs,**/*.razor.css,**/*.razor.js"
---
Always use code behind files (.razor.cs) for C# code and separate files for CSS and JavaScript when working with MudBlazor components in a Blazor application. Never include C#, CSS, or JavaScript directly in the .razor files. Follow Atomic Design principles for component organization and ensure all text is localized using IStringLocalizer.
# MudBlazor - Règles de Développement

## 0. RÈGLES ABSOLUES (NON NÉGOCIABLES)

### 0.1. Séparation des Fichiers

**JAMAIS de code C#, CSS ou JavaScript dans le fichier .razor**

**Structure OBLIGATOIRE pour TOUS les composants :**
```
MonComposant.razor       <!-- Markup Razor UNIQUEMENT -->
MonComposant.razor.cs    <!-- Code C# (logique, méthodes, propriétés) -->
MonComposant.razor.css   <!-- Styles CSS (scoped ou global) -->
MonComposant.razor.js    <!-- JavaScript (si nécessaire) -->
```

### ❌ INTERDIT - Code dans .razor

```razor
@* MonComposant.razor - MAUVAIS EXEMPLE *@
<MudButton OnClick="HandleClick">Cliquer</MudButton>

<style>
    .my-button { color: red; }  <!-- INTERDIT -->
</style>

@code {
    private void HandleClick() { }  <!-- INTERDIT -->
}
```

### ✅ OBLIGATOIRE - Séparation stricte

**MonComposant.razor** (Markup uniquement)
```razor
<MudButton OnClick="HandleClick" Class="my-button">Cliquer</MudButton>
```

**MonComposant.razor.cs** (Code-behind)
```csharp
namespace MyApp.Components;

public partial class MonComposant
{
    private void HandleClick()
    {
        // Logique ici
    }
}
```

**MonComposant.razor.css** (Styles)
```css
.my-button {
    color: red;
}
```

**MonComposant.razor.js** (JavaScript si nécessaire)
```javascript
export function initializeComponent(element) {
    // JavaScript ici
}
```

### Exceptions (AUCUNE)

**Il n'y a AUCUNE exception à cette règle :**
- ❌ Pas de `@code { }` dans .razor
- ❌ Pas de `<style>` dans .razor
- ❌ Pas de `<script>` dans .razor
- ❌ Pas de CSS inline `style="..."`
- ❌ Pas de paramètres, propriétés ou méthodes dans .razor

**Même pour un composant simple d'une seule ligne de code C#, créer un .razor.cs**

### 0.2. Atomic Design (OBLIGATOIRE)

**TOUJOURS organiser les composants selon les principes d'Atomic Design :**

```
Components/
├── Atoms/                       <!-- Composants de base indivisibles -->
│   ├── Buttons/
│   │   ├── PrimaryButton.razor
│   │   ├── PrimaryButton.razor.cs
│   │   ├── PrimaryButton.razor.css
│   │   ├── IconButton.razor
│   │   └── LinkButton.razor
│   ├── Inputs/
│   │   ├── TextField.razor
│   │   ├── NumberField.razor
│   │   └── SelectField.razor
│   ├── Labels/
│   │   ├── StatusLabel.razor
│   │   └── Badge.razor
│   └── Icons/
│       └── CustomIcon.razor
│
├── Molecules/                   <!-- Combinaisons d'atomes -->
│   ├── Forms/
│   │   ├── SearchBox.razor          <!-- TextField + IconButton -->
│   │   ├── FormField.razor          <!-- Label + TextField + ValidationMessage -->
│   │   └── DateRangePicker.razor
│   ├── Cards/
│   │   ├── InfoCard.razor           <!-- Card + Icon + Text -->
│   │   └── StatsCard.razor
│   └── Navigation/
│       ├── Breadcrumb.razor
│       └── TabItem.razor
│
├── Organisms/                   <!-- Combinaisons de molécules -->
│   ├── Forms/
│   │   ├── LoginForm.razor          <!-- Plusieurs FormField + Buttons -->
│   │   ├── UserEditForm.razor
│   │   └── SearchFilters.razor
│   ├── Tables/
│   │   ├── UsersTable.razor         <!-- MudTable + Actions + Filters -->
│   │   └── DataGrid.razor
│   ├── Navigation/
│   │   ├── MainNavBar.razor
│   │   ├── SideMenu.razor
│   │   └── TabsContainer.razor
│   └── Dialogs/
│       ├── ConfirmDialog.razor
│       └── FormDialog.razor
│
├── Templates/                   <!-- Layouts de pages -->
│   ├── MainLayout.razor
│   ├── AdminLayout.razor
│   ├── AuthLayout.razor
│   └── DashboardLayout.razor
│
└── Pages/                       <!-- Pages complètes -->
    ├── Areas/
    │   ├── Identity/
    │   │   ├── Account/
    │   │   │   ├── Login/
    │   │   │   │   └── Login.razor  <!-- Utilise LoginForm (Organism) -->
    │   │   │   └── Register/
    │   │   │       └── Register.razor
    │   │   └── Profile/
    │   │       └── Index/
    │   │           └── Profile.razor
    │   └── Administration/
    │       ├── Users/
    │       │   ├── List/
    │       │   │   └── UsersList.razor  <!-- Utilise UsersTable (Organism) -->
    │       │   └── Edit/
    │       │       └── UserEdit.razor   <!-- Utilise UserEditForm (Organism) -->
    │       └── Roles/
    │           └── List/
    │               └── RolesList.razor
    └── Dashboard/
        └── Index.razor
```

**Règles Atomic Design :**

1. **Atoms** : Composants de base (boutons, inputs, labels, icônes)
   - Ne contiennent que des composants MudBlazor ou HTML de base
   - Réutilisables dans toute l'application
   - Exemple : `PrimaryButton.razor` encapsule `MudButton` avec styles standards

2. **Molecules** : Combinaisons simples d'atomes
   - Combinent 2-5 atomes pour une fonctionnalité simple
   - Exemple : `SearchBox.razor` = `TextField` + `IconButton`

3. **Organisms** : Composants complexes
   - Combinent molécules et atomes pour fonctionnalité complète
   - Exemple : `UsersTable.razor` = Table + Filters + Actions + Pagination

4. **Templates** : Layouts réutilisables
   - Définissent la structure globale des pages
   - Contiennent navigation, header, footer, sidebar

5. **Pages** : Pages complètes de l'application
   - Utilisent templates + organisms + routing
   - Organisées par Area/Concerns/Subconcerns

**Hiérarchie de réutilisation :**
- **Atoms** → Utilisés partout (application-wide)
- **Molecules** → Utilisés par Organisms et Pages
- **Organisms** → Utilisés par Pages
- **Templates** → Utilisés par Pages pour layout
- **Pages** → Point d'entrée routing

### 0.3. Internationalisation (OBLIGATOIRE)

**JAMAIS de texte en dur dans les composants - TOUJOURS utiliser IStringLocalizer**

❌ **INTERDIT - Texte en dur :**
```razor
<MudButton>Enregistrer</MudButton>
<MudAlert Severity="Severity.Success">Opération réussie</MudAlert>
<MudTextField Label="Nom d'utilisateur" />
```

✅ **OBLIGATOIRE - IStringLocalizer :**
```razor
@inject IStringLocalizer<SharedResources> Localizer

<MudButton>@Localizer["Common.Save"]</MudButton>
<MudAlert Severity="Severity.Success">@Localizer["Common.OperationSuccess"]</MudAlert>
<MudTextField Label="@Localizer["User.Username"]" />
```

**Structure des fichiers de ressources :**
```
Resources/
├── SharedResources.resx              <!-- Textes communs -->
├── SharedResources.fr.resx           <!-- Français -->
├── SharedResources.en.resx           <!-- Anglais -->
├── Areas/
│   ├── Identity/
│   │   ├── AccountResources.resx
│   │   ├── AccountResources.fr.resx
│   │   └── AccountResources.en.resx
│   └── Administration/
│       ├── UsersResources.resx
│       ├── UsersResources.fr.resx
│       └── UsersResources.en.resx
└── Components/
    ├── Atoms/
    │   ├── ButtonResources.resx
    │   ├── ButtonResources.fr.resx
    │   └── ButtonResources.en.resx
    └── Organisms/
        ├── FormsResources.resx
        ├── FormsResources.fr.resx
        └── FormsResources.en.resx
```

**Exemples d'utilisation par niveau Atomic Design :**

**Atom - PrimaryButton.razor.cs**
```csharp
[Inject] private IStringLocalizer<ButtonResources> Localizer { get; set; }

[Parameter] public string TextKey { get; set; } = "Common.Save";

private string GetButtonText() => Localizer[TextKey];
```

**Molecule - SearchBox.razor.cs**
```csharp
[Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }

private string PlaceholderText => Localizer["Common.Search"];
private string SearchButtonAriaLabel => Localizer["Common.SearchButtonAriaLabel"];
```

**Organism - UsersTable.razor.cs**
```csharp
[Inject] private IStringLocalizer<UsersResources> Localizer { get; set; }

private string ColumnName => Localizer["Users.ColumnName"];
private string ColumnEmail => Localizer["Users.ColumnEmail"];
private string DeleteConfirmMessage => Localizer["Users.DeleteConfirm"];
```

**Règles d'internationalisation :**
1. ❌ **ZÉRO texte en dur** dans .razor ou .razor.cs
2. ✅ Toujours injecter `IStringLocalizer<T>` dans .razor.cs
3. ✅ Utiliser clés hiérarchiques : `"Area.Concern.Action"` (ex: `"Users.List.Title"`)
4. ✅ Fichiers .resx par niveau (Shared, Area, Component)
5. ✅ Messages d'erreur, labels, tooltips, aria-labels → **TOUS localisés**
6. ✅ Formats dates/nombres via `CultureInfo.CurrentCulture`

**Configuration Program.cs :**
```csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "fr-FR", "en-US" };
    options.SetDefaultCulture("fr-FR")
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});
```

## 1. Composants MudBlazor (OBLIGATOIRE)

**Utiliser systématiquement les composants MudBlazor au lieu des éléments HTML natifs :**

❌ **NE PAS FAIRE :**
```razor
<button class="btn btn-primary">Cliquer</button>
<div class="card">...</div>
<input type="text" />
```

✅ **FAIRE :**
```razor
<MudButton Variant="Variant.Filled" Color="Color.Primary">Cliquer</MudButton>
<MudCard>...</MudCard>
<MudTextField @bind-Value="value" Label="Libellé" />
```

**Composants principaux à utiliser :**
- Boutons : `MudButton`, `MudIconButton`, `MudFab`
- Saisie : `MudTextField`, `MudNumericField`, `MudSelect`, `MudAutocomplete`
- Affichage : `MudCard`, `MudPaper`, `MudChip`, `MudAlert`, `MudBadge`
- Navigation : `MudTabs`, `MudNavMenu`, `MudBreadcrumbs`, `MudPagination`
- Données : `MudTable`, `MudDataGrid`, `MudList`, `MudTreeView`
- Dialogs : `MudDialog`, `MudDrawer`, `MudPopover`, `MudMenu`
- Feedback : `MudProgressCircular`, `MudProgressLinear`, `MudSkeleton`, `MudSnackbar`

## 2. Styling et Classes CSS

**Hiérarchie de styling (du plus global au plus local - TOUJOURS privilégier le global) :**

### 1. Classes utilitaires MudBlazor (PRIORITÉ #1)
```razor
<MudPaper Class="pa-4 ma-4 d-flex">...</MudPaper>
```

### 2. Fichier CSS global (PRIORITÉ #2)
```css
/* wwwroot/css/site.css ou wwwroot/css/app.css */
.company-brand-primary { color: #1976D2; }
.card-elevated { box-shadow: 0 4px 8px rgba(0,0,0,0.1); }
.status-active { background-color: #4CAF50; }
```

### 3. Thème MudBlazor personnalisé (PRIORITÉ #3)
```csharp
private MudTheme _theme = new()
{
    Palette = new PaletteLight()
    {
        Primary = "#1976D2",
        Secondary = "#424242"
    }
};
```

### 4. Fichier .razor.css scoped (DERNIER RECOURS UNIQUEMENT)
**À utiliser SEULEMENT si :**
- Le style est absolument spécifique à CE composant
- Aucune possibilité de réutilisation dans d'autres composants
- Le style ne peut pas être exprimé avec les classes utilitaires MudBlazor

❌ **NE PAS FAIRE (styles qui devraient être globaux) :**
```css
/* UsersList.razor.css - MAUVAIS : ces styles sont réutilisables */
.data-table { width: 100%; }
.action-button { margin-left: 8px; }
.status-badge { border-radius: 4px; }
```

✅ **FAIRE (style vraiment spécifique) :**
```css
/* ComplexChart.razor.css - BON : style très spécifique à ce graphique */
.chart-container {
    position: relative;
    height: 400px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.chart-container::before {
    content: "";
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    opacity: 0.1;
    background-image: url('/images/chart-grid.svg');
}
```

### ❌ INTERDICTIONS ABSOLUES

```razor
@* MyComponent.razor *@
<div style="margin: 20px;">...</div>  <!-- INTERDIT : CSS inline -->

<style>
    .my-class { color: red; }  <!-- INTERDIT : <style> dans .razor -->
</style>
```

### 📋 Ordre de Décision pour le Styling

**Question à se poser DANS CET ORDRE :**

1. ✅ **Puis-je utiliser une classe MudBlazor ?** (`pa-*`, `ma-*`, `d-flex`, etc.)
   - OUI → Utiliser MudBlazor
   - NON → Passer à l'étape 2

2. ✅ **Ce style sera-t-il réutilisé ailleurs ?**
   - OUI → Créer une classe globale dans `site.css`
   - NON → Passer à l'étape 3

3. ✅ **Puis-je utiliser le thème MudBlazor ?** (couleurs, typographie)
   - OUI → Configurer le thème
   - NON → Passer à l'étape 4

4. ✅ **Le style est-il absolument unique à ce composant ?**
   - OUI → Créer `.razor.css` (scoped)
   - NON → Retour à l'étape 2, créer classe globale

**Classes utilitaires MudBlazor courantes :**
- Spacing : `pa-{0-16}` (padding all), `ma-{0-16}` (margin all), `mt-4` (margin-top), `pl-2` (padding-left)
- Layout : `d-flex`, `justify-center`, `align-center`, `flex-wrap`, `flex-column`
- Text : `text-center`, `text-left`, `text-right`, `text-uppercase`, `text-truncate`
- Display : `d-none`, `d-block`, `d-inline`, `d-flex`, `d-grid`

## 3. Services MudBlazor (Injection Requise)

**S'assurer que les services sont configurés dans `Program.cs` :**
```csharp
builder.Services.AddMudServices();
```

**Utiliser les services injectés pour les interactions (avec textes localisés) :**

### ISnackbar - Notifications
```csharp
// Dans .razor.cs
[Inject] private ISnackbar Snackbar { get; set; }
[Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }

private void ShowNotification() 
{
    Snackbar.Add(Localizer["Common.OperationSuccess"], Severity.Success);
    Snackbar.Add(Localizer["Common.ErrorOccurred"], Severity.Error);
    Snackbar.Add(Localizer["Common.AttentionRequired"], Severity.Warning);
}
```

### IDialogService - Dialogues
```csharp
// Dans .razor.cs
[Inject] private IDialogService DialogService { get; set; }
[Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }

private async Task OpenDialog() 
{
    var parameters = new DialogParameters 
    { 
        ["Message"] = Localizer["Common.ConfirmAction"] 
    };
    var dialog = await DialogService.ShowAsync<MyDialog>(
        Localizer["Common.ConfirmationTitle"], 
        parameters);
    var result = await dialog.Result;
    if (!result.Canceled) 
    {
        // Action confirmée
    }
}
```

### IResizeObserver - Responsive
```razor
@inject IResizeObserver ResizeObserver

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            await ResizeObserver.Observe(element);
        }
    }
}
```

## 4. Organisation des Composants - Atomic Design + Area/Concerns

**Structure OBLIGATOIRE combinant Atomic Design et organisation par fonctionnalités :**

```
Components/
├── Atoms/                           <!-- Niveau 1 : Composants de base -->
│   ├── Buttons/
│   │   ├── PrimaryButton.razor
│   │   ├── PrimaryButton.razor.cs   <!-- IStringLocalizer<ButtonResources> -->
│   │   ├── SecondaryButton.razor
│   │   └── IconButton.razor
│   ├── Inputs/
│   │   ├── TextField.razor          <!-- IStringLocalizer pour labels -->
│   │   ├── NumberField.razor
│   │   └── SelectField.razor
│   ├── Labels/
│   │   ├── StatusBadge.razor
│   │   └── ChipLabel.razor
│   └── Icons/
│       └── CustomIcon.razor
│
├── Molecules/                       <!-- Niveau 2 : Combinaisons simples -->
│   ├── Forms/
│   │   ├── SearchBox.razor          <!-- TextField + IconButton -->
│   │   ├── SearchBox.razor.cs       <!-- IStringLocalizer<SharedResources> -->
│   │   ├── FormField.razor          <!-- Label + Input + ValidationMessage -->
│   │   └── DateRangePicker.razor
│   ├── Cards/
│   │   ├── InfoCard.razor
│   │   └── StatsCard.razor
│   └── Navigation/
│       ├── BreadcrumbItem.razor
│       └── TabItem.razor
│
├── Organisms/                       <!-- Niveau 3 : Composants complexes -->
│   ├── Forms/
│   │   ├── LoginForm.razor          <!-- Plusieurs Molecules -->
│   │   ├── LoginForm.razor.cs       <!-- IStringLocalizer<AccountResources> -->
│   │   ├── UserEditForm.razor
│   │   └── SearchFilters.razor
│   ├── Tables/
│   │   ├── UsersTable.razor
│   │   ├── UsersTable.razor.cs      <!-- IStringLocalizer<UsersResources> -->
│   │   └── DataGrid.razor
│   ├── Navigation/
│   │   ├── MainNavBar.razor
│   │   ├── SideMenu.razor
│   │   └── TabsContainer.razor
│   └── Dialogs/
│       ├── ConfirmDialog.razor
│       ├── ConfirmDialog.razor.cs   <!-- IStringLocalizer<SharedResources> -->
│       └── FormDialog.razor
│
├── Templates/                       <!-- Niveau 4 : Layouts -->
│   ├── MainLayout.razor
│   ├── MainLayout.razor.cs
│   ├── AdminLayout.razor
│   ├── AuthLayout.razor
│   └── DashboardLayout.razor
│
└── Pages/                           <!-- Niveau 5 : Pages avec routing -->
    ├── Areas/
    │   ├── Identity/                <!-- Area fonctionnelle -->
    │   │   ├── Account/             <!-- Concern -->
    │   │   │   ├── Login/           <!-- Subconcern -->
    │   │   │   │   ├── Login.razor
    │   │   │   │   └── Login.razor.cs  <!-- Utilise LoginForm (Organism) -->
    │   │   │   └── Register/
    │   │   │       ├── Register.razor
    │   │   │       └── Register.razor.cs
    │   │   └── Profile/
    │   │       └── Index/
    │   │           ├── Profile.razor
    │   │           └── Profile.razor.cs
    │   │
    │   └── Administration/          <!-- Area fonctionnelle -->
    │       ├── Users/               <!-- Concern -->
    │       │   ├── List/            <!-- Subconcern -->
    │       │   │   ├── UsersList.razor
    │       │   │   └── UsersList.razor.cs  <!-- Utilise UsersTable (Organism) -->
    │       │   └── Edit/
    │       │       ├── UserEdit.razor
    │       │       └── UserEdit.razor.cs   <!-- Utilise UserEditForm (Organism) -->
    │       └── Roles/
    │           └── List/
    │               ├── RolesList.razor
    │               └── RolesList.razor.cs
    └── Dashboard/
        ├── Index.razor
        └── Index.razor.cs
```

**Règles d'organisation Atomic Design + Fonctionnel :**

1. **Atoms** (Composants de base)
   - Encapsulent composants MudBlazor avec styles/comportements standards
   - IStringLocalizer injecté pour tous les textes
   - Réutilisables application-wide
   - Exemple : `PrimaryButton.razor` avec `TextKey` paramétrable

2. **Molecules** (Combinaisons simples)
   - Combinent 2-5 Atoms pour fonctionnalité ciblée
   - IStringLocalizer pour labels/placeholders
   - Exemple : `SearchBox` = `TextField` + `IconButton`

3. **Organisms** (Composants complexes)
   - Combinent Molecules/Atoms pour fonctionnalité complète
   - IStringLocalizer spécifique par domaine (UsersResources, AccountResources)
   - Exemple : `UsersTable` = Table + Filters + Actions + Pagination

4. **Templates** (Layouts)
   - Structures réutilisables avec RenderFragment
   - Navigation, header, footer, sidebar
   - IStringLocalizer pour navigation/menu

5. **Pages** (Routes + Composition)
   - Organisées par Area/Concerns/Subconcerns
   - Utilisent Organisms + Templates
   - Route définie avec @page
   - IStringLocalizer pour titres de page et messages spécifiques

**Mapping Atomic Design ↔ Ancien système Parts/ :**
- **Atoms** remplacent les composants dans `Shared/Common/` de base
- **Molecules** remplacent les composants simples dans `Parts/Subconcern`
- **Organisms** remplacent les composants dans `Parts/Concern` et `Parts/Area`
- **Templates** remplacent `Shared/Layout/`
- **Pages** gardent la structure `Areas/Concerns/Subconcerns/`

**Flux de réutilisation :**
```
Pages → Organisms → Molecules → Atoms → MudBlazor
```

**Exemple concret - UsersList.razor :**
```razor
@* Pages/Areas/Administration/Users/List/UsersList.razor *@
@page "/admin/users"
@layout AdminLayout  <!-- Template -->

<PageTitle>@Localizer["Users.ListTitle"]</PageTitle>

<UsersTable Items="@users" 
            OnEdit="HandleEdit" 
            OnDelete="HandleDelete" />  <!-- Organism -->
```

```csharp
// UsersList.razor.cs
[Inject] private IStringLocalizer<UsersResources> Localizer { get; set; }
```

**Exemple concret - UsersTable.razor (Organism) :**
```razor
@* Organisms/Tables/UsersTable.razor *@
<MudTable Items="@Items">
    <ToolBarContent>
        <SearchBox Placeholder="@Localizer["Users.SearchPlaceholder"]" 
                   OnSearch="HandleSearch" />  <!-- Molecule -->
    </ToolBarContent>
    <HeaderContent>
        <MudTh>@Localizer["Users.ColumnName"]</MudTh>
        <MudTh>@Localizer["Users.ColumnEmail"]</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.Name</MudTd>
        <MudTd>@context.Email</MudTd>
        <MudTd>
            <IconButton Icon="@Icons.Material.Filled.Edit" 
                        TextKey="Common.Edit"
                        OnClick="() => OnEdit.InvokeAsync(context.Id)" />  <!-- Atom -->
        </MudTd>
    </RowTemplate>
</MudTable>
```

**Ancienne structure (OBSOLÈTE - NE PLUS UTILISER) :**
```
Components/
├── Areas/
│   ├── Identity/
│   │   ├── Parts/                   <!-- OBSOLÈTE -->
│   │   ├── Account/
│   │   │   ├── Parts/               <!-- OBSOLÈTE -->
```

**Nouvelle structure (OBLIGATOIRE) :**
```
Components/
├── Atoms/                           <!-- Remplace Shared/Common de base -->
├── Molecules/                       <!-- Remplace Parts/Subconcern simples -->
├── Organisms/                       <!-- Remplace Parts/Concern et Parts/Area -->
├── Templates/                       <!-- Remplace Shared/Layout -->
└── Pages/Areas/...                  <!-- Garde la structure fonctionnelle -->
│   │       ├── Parts/               <!-- Composants partagés dans Profile -->
│   │       │   └── ProfileNav.razor
│   │       └── Index/
│   │           ├── Profile.razor
│   │           └── ProfileCard.razor
│   │
│   ├── Administration/              <!-- Area : Admin -->
│   │   ├── Parts/                   <!-- Composants partagés dans l'Area Administration -->
│   │   │   ├── AdminSidebar.razor
│   │   │   └── AdminBreadcrumb.razor
│   │   ├── Users/                   <!-- Concern : Gestion utilisateurs -->
│   │   │   ├── Parts/               <!-- Composants partagés dans Users -->
│   │   │   │   ├── UserAvatar.razor
│   │   │   │   ├── UserBadge.razor
│   │   │   │   └── UserStatusChip.razor
│   │   │   ├── List/                <!-- Subconcern : Liste -->
│   │   │   │   ├── Parts/           <!-- Composants partagés dans List -->
│   │   │   │   │   ├── UsersFilter.razor
│   │   │   │   │   └── UsersToolbar.razor
│   │   │   │   ├── UsersList.razor
│   │   │   │   ├── UsersList.razor.css
│   │   │   │   └── UsersTable.razor  <!-- Composant spécifique à UsersList.razor -->
│   │   │   ├── Edit/                <!-- Subconcern : Édition -->
│   │   │   │   ├── Parts/           <!-- Composants partagés dans Edit -->
│   │   │   │   │   └── UserEditForm.razor
│   │   │   │   ├── UserEdit.razor
│   │   │   │   └── UserEditDialog.razor
│   │   │   └── Create/              <!-- Subconcern : Création -->
│   │   │       ├── Parts/
│   │   │       │   └── UserCreateWizard.razor
│   │   │       └── UserCreate.razor
│   │   │
│   │   └── Roles/                   <!-- Concern : Gestion rôles -->
│   │       ├── Parts/               <!-- Composants partagés dans Roles -->
│   │       │   └── RoleChip.razor
│   │       └── List/
│   │           ├── Parts/
│   │           │   └── RolesFilter.razor
│   │           └── RolesList.razor
│   │
│   └── Dashboard/                   <!-- Area : Tableau de bord -->
│       ├── Parts/                   <!-- Composants partagés dans l'Area Dashboard -->
│       │   ├── DashboardCard.razor
│       │   └── DashboardWidget.razor
│       ├── Overview/                <!-- Concern : Vue d'ensemble -->
│       │   ├── Parts/               <!-- Composants partagés dans Overview -->
│       │   │   └── OverviewChart.razor
│       │   └── Index/               <!-- Subconcern : Page principale -->
│       │       ├── Parts/           <!-- Composants partagés dans Index -->
│       │       │   └── StatsCard.razor
│       │       ├── Dashboard.razor
│       │       └── Dashboard.razor.css
│       └── Analytics/               <!-- Concern : Analytiques -->
│           ├── Parts/
│           │   └── AnalyticsFilter.razor
│           └── Reports/
│               ├── Parts/
│               │   └── ReportExport.razor
│               └── Report.razor
│
└── Shared/                          <!-- Composants transverses globaux -->
    ├── Layout/
    │   ├── MainLayout.razor
    │   ├── MainLayout.razor.css
    │   └── NavMenu.razor
    ├── Common/                      <!-- Composants réutilisables globaux -->
    │   ├── DataCard.razor
    │   ├── DataCard.razor.css
    │   ├── ConfirmDialog.razor
    │   └── PageHeader.razor
    └── Forms/                       <!-- Composants de formulaires globaux -->
        ├── FormButtons.razor
        └── ValidationSummary.razor
```

**Règles d'organisation :**
1. **Area** : Regroupement fonctionnel majeur (Identity, Administration, Sales, etc.)
   - Peut avoir un dossier `Parts/` pour composants partagés dans toute l'Area
2. **Concern** : Fonctionnalité principale (Users, Products, Orders, etc.)
   - Peut avoir un dossier `Parts/` pour composants partagés dans tout le Concern
3. **Subconcern** : Action spécifique (List, Edit, Create, Delete, Details, etc.)
   - Peut avoir un dossier `Parts/` pour composants partagés dans tout le Subconcern
4. **Page** : Fichier .razor principal + composants locaux spécifiques dans le même dossier
5. **Parts/** : Composants mutualisés utilisés par plusieurs pages/subconcerns au même niveau

**Hiérarchie de réutilisation (du plus spécifique au plus global) :**
1. **Composant local** (dans le dossier de la page) : Utilisé uniquement par cette page
2. **Parts/Subconcern** : Partagé entre les pages du même Subconcern
3. **Parts/Concern** : Partagé entre les Subconcerns du même Concern
4. **Parts/Area** : Partagé dans toute l'Area
5. **Shared/** : Partagé dans toute l'application

**Exemples de routing :**
- `/identity/account/login` → `Areas/Identity/Account/Login/Login.razor`
- `/admin/users/list` → `Areas/Administration/Users/List/UsersList.razor`
- `/admin/users/edit/{id}` → `Areas/Administration/Users/Edit/UserEdit.razor`
- `/dashboard` → `Areas/Dashboard/Overview/Index/Dashboard.razor`

## 5. Composants Réutilisables - Hiérarchie de Mutualisation

**Extraire les patterns répétés selon le niveau de réutilisation :**

### Niveau 1 : Composant spécifique (dans le dossier de la page)
```
Areas/Administration/Users/List/
  UsersList.razor          <!-- Page principale -->
  UsersTable.razor         <!-- Composant utilisé UNIQUEMENT par UsersList.razor -->
  UserRowActions.razor     <!-- Composant utilisé UNIQUEMENT par UsersTable.razor -->
```

### Niveau 2 : Composant partagé dans Parts/Subconcern
```
Areas/Administration/Users/List/
  Parts/                   <!-- Composants partagés entre pages de List -->
    UsersFilter.razor      <!-- Utilisé par plusieurs pages du Subconcern -->
    UsersToolbar.razor
  UsersList.razor
  UsersArchive.razor       <!-- Utilise aussi UsersFilter et UsersToolbar -->
```

### Niveau 3 : Composant partagé dans Parts/Concern
```
Areas/Administration/Users/
  Parts/                   <!-- Composants partagés dans tout Users -->
    UserAvatar.razor       <!-- Utilisé par List, Edit, Create, etc. -->
    UserBadge.razor
    UserStatusChip.razor
  List/
  Edit/
  Create/
```

### Niveau 4 : Composant partagé dans Parts/Area
```
Areas/Administration/
  Parts/                   <!-- Composants partagés dans toute l'Area Admin -->
    AdminSidebar.razor     <!-- Utilisé par Users, Roles, Settings, etc. -->
    AdminBreadcrumb.razor
  Users/
  Roles/
  Settings/
```

### Niveau 5 : Composant global dans Shared/
```
Components/Shared/Common/
  DataCard.razor           <!-- Composant générique réutilisable globalement -->
  DataCard.razor.css       <!-- TOUJOURS un .css si styles personnalisés -->
  ConfirmDialog.razor
  PageHeader.razor
```

**Exemple de composant réutilisable avec styles isolés :**
```razor
@* Components/Shared/Common/DataCard.razor *@
<MudCard Elevation="@Elevation" Class="data-card">
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">@Title</MudText>
        </CardHeaderContent>
        <CardHeaderActions>
            @HeaderActions
        </CardHeaderActions>
    </MudCardHeader>
    <MudCardContent>
        @ChildContent
    </MudCardContent>
</MudCard>

@code {
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public RenderFragment? HeaderActions { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public int Elevation { get; set; } = 2;
}
```

```css
/* Components/Shared/Common/DataCard.razor.css */
.data-card {
    transition: transform 0.2s ease-in-out;
}

.data-card:hover {
    transform: translateY(-2px);
}
```

```csharp
// Components/Shared/Common/DataCard.razor.cs
namespace MyApp.Components.Shared.Common;

public partial class DataCard
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public RenderFragment? HeaderActions { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public int Elevation { get; set; } = 2;
}
```

## 6. Cycle de Vie des Composants Blazor

**Comprendre et utiliser correctement les méthodes du cycle de vie :**

### OnInitialized / OnInitializedAsync
```csharp
// Appelé une seule fois lors de l'initialisation du composant
protected override async Task OnInitializedAsync()
{
    // Chargement initial des données
    await LoadDataAsync();
}
```

### OnParametersSet / OnParametersSetAsync
```csharp
// Appelé chaque fois que les paramètres changent
protected override async Task OnParametersSetAsync()
{
    // Réagir aux changements de paramètres
    if (UserId != _previousUserId)
    {
        await LoadUserDataAsync(UserId);
        _previousUserId = UserId;
    }
}
```

### OnAfterRender / OnAfterRenderAsync
```csharp
// Appelé après chaque rendu du composant
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // Initialisation JavaScript, focus, etc.
        await JSRuntime.InvokeVoidAsync("initializeComponent", ElementRef);
    }
}
```

### Dispose / DisposeAsync
```csharp
// Implémenter IDisposable pour nettoyer les ressources
public void Dispose()
{
    // Désabonner les événements
    EventService.OnDataChanged -= HandleDataChanged;
    
    // Annuler les tokens
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource?.Dispose();
}
```

## 7. Gestion de l'État et EventCallback

**Utiliser EventCallback pour la communication parent-enfant :**

```razor
@* ChildComponent.razor *@
<MudButton OnClick="HandleClick">Cliquer</MudButton>
```

```csharp
// ChildComponent.razor.cs
[Parameter] public EventCallback<string> OnItemSelected { get; set; }

private async Task HandleClick()
{
    await OnItemSelected.InvokeAsync("Item1");
}
```

```razor
@* ParentComponent.razor *@
<ChildComponent OnItemSelected="HandleItemSelected" />
<MudText>Selected: @selectedItem</MudText>
```

```csharp
// ParentComponent.razor.cs
private string selectedItem = "";

private void HandleItemSelected(string item)
{
    selectedItem = item;
}
```

**Utiliser CascadingParameter pour données partagées :**

```razor
@* MainLayout.razor *@
<CascadingValue Value="@currentUser">
    @Body
</CascadingValue>
```

```csharp
// Composant enfant
[CascadingParameter] public User CurrentUser { get; set; }
```

## 8. Accessibilité et Responsive

**Utiliser les breakpoints MudBlazor :**
```razor
<MudGrid>
    <MudItem xs="12" sm="6" md="4" lg="3">
        <!-- 12 cols mobile, 6 tablet, 4 desktop, 3 large -->
    </MudItem>
</MudGrid>

<MudHidden Breakpoint="Breakpoint.SmAndDown">
    <!-- Caché sur mobile et tablette -->
</MudHidden>

<MudTable Breakpoint="Breakpoint.Sm">
    <!-- Table responsive devient liste sur mobile -->
</MudTable>
```

**Toujours fournir des labels et aria-labels :**
```razor
<MudIconButton Icon="@Icons.Material.Filled.Delete" 
               AriaLabel="Supprimer l'élément"
               OnClick="Delete" />

<MudTextField Label="Nom d'utilisateur" 
              HelperText="Saisissez votre nom"
              Required="true" />
```

## 6. Thèmes et Personnalisation

**Définir un thème personnalisé dans `MainLayout.razor` ou `App.razor` :**
```razor
<MudThemeProvider Theme="@_theme" />

@code {
    private MudTheme _theme = new MudTheme()
    {
        Palette = new PaletteLight()
        {
            Primary = "#1976D2",
            Secondary = "#424242",
            Success = "#4CAF50",
            Error = "#F44336",
            AppbarBackground = "#1976D2"
        },
        Typography = new Typography()
        {
            Default = new Default()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" }
            }
        }
    };
}
```

## 7. Validation et Formulaires

**Utiliser MudForm avec FluentValidation :**
```razor
<MudForm @ref="form" Model="@model" Validation="@(validator.ValidateValue)">
    <MudTextField @bind-Value="model.Name" 
                  For="@(() => model.Name)"
                  Label="Nom" 
                  Required="true" />
    
    <MudButton OnClick="Submit" 
               Disabled="@(!form.IsValid)"
               Variant="Variant.Filled" 
               Color="Color.Primary">
        Enregistrer
    </MudButton>
</MudForm>

@code {
    MudForm form;
    MyModel model = new();
    MyModelValidator validator = new();
}
```

## 8. Bonnes Pratiques MudBlazor

### 8.1. Performance et Optimisation

**Utiliser Virtualization pour grandes listes :**
```razor
<MudVirtualize Items="@largeList" Context="item">
    <MudListItem>@item.Name</MudListItem>
</MudVirtualize>
```

**Éviter les re-renders inutiles :**
```csharp
// Dans .razor.cs
protected override bool ShouldRender()
{
    return _stateHasChanged; // Contrôler manuellement si nécessaire
}
```

**Utiliser @key pour listes dynamiques :**
```razor
@foreach (var item in items)
{
    <MudCard @key="item.Id">...</MudCard>
}
```

### 8.2. Gestion des États de Chargement

**Toujours indiquer l'état de chargement :**
```razor
@if (isLoading)
{
    <MudProgressLinear Color="Color.Primary" Indeterminate="true" />
}
else if (error != null)
{
    <MudAlert Severity="Severity.Error">@error</MudAlert>
}
else if (!data.Any())
{
    <MudAlert Severity="Severity.Info">Aucune donnée disponible</MudAlert>
}
else
{
    <MudTable Items="@data">...</MudTable>
}
```

**Utiliser les skeletons pour un meilleur UX :**
```razor
@if (isLoading)
{
    <MudGrid>
        @for (int i = 0; i < 6; i++)
        {
            <MudItem xs="12" sm="6" md="4">
                <MudCard>
                    <MudCardContent>
                        <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="200px" />
                        <MudSkeleton SkeletonType="SkeletonType.Text" />
                        <MudSkeleton SkeletonType="SkeletonType.Text" />
                    </MudCardContent>
                </MudCard>
            </MudItem>
        }
    </MudGrid>
}
```

### 8.3. Gestion des Erreurs et Validation

**Toujours gérer les erreurs avec try-catch et feedback utilisateur :**
```csharp
// Dans .razor.cs
private async Task SaveAsync()
{
    try
    {
        isLoading = true;
        await Service.SaveAsync(model);
        Snackbar.Add("Enregistré avec succès", Severity.Success);
        NavigationManager.NavigateTo("/list");
    }
    catch (ValidationException ex)
    {
        Snackbar.Add(ex.Message, Severity.Warning);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur lors de l'enregistrement");
        Snackbar.Add("Une erreur est survenue", Severity.Error);
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

**Utiliser Required et Validation dans les formulaires :**
```razor
<MudTextField @bind-Value="model.Email" 
              For="@(() => model.Email)"
              Label="Email" 
              Required="true"
              RequiredError="L'email est obligatoire"
              Validation="@(new EmailAddressAttribute() { ErrorMessage = "Email invalide" })" />
```

### 8.4. Dialogues et Confirmations

**Créer des dialogues réutilisables :**
```razor
@* Components/Shared/Common/ConfirmDialog.razor *@
<MudDialog>
    <DialogContent>
        <MudText>@ContentText</MudText>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Annuler</MudButton>
        <MudButton Color="Color.Error" Variant="Variant.Filled" OnClick="Submit">@ButtonText</MudButton>
    </DialogActions>
</MudDialog>
```

```csharp
// Dans .razor.cs
[CascadingParameter] MudDialogInstance MudDialog { get; set; }
[Parameter] public string ContentText { get; set; }
[Parameter] public string ButtonText { get; set; } = "Supprimer";

void Submit() => MudDialog.Close(DialogResult.Ok(true));
void Cancel() => MudDialog.Cancel();
```

**Utiliser les dialogues pour actions destructives :**
```csharp
private async Task DeleteUserAsync(int userId)
{
    var parameters = new DialogParameters
    {
        ["ContentText"] = "Êtes-vous sûr de vouloir supprimer cet utilisateur ?",
        ["ButtonText"] = "Supprimer"
    };
    
    var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmation", parameters);
    var result = await dialog.Result;
    
    if (!result.Canceled)
    {
        await UserService.DeleteAsync(userId);
        Snackbar.Add("Utilisateur supprimé", Severity.Success);
        await RefreshAsync();
    }
}
```

### 8.5. Tables et Grilles de Données

**Utiliser MudTable avec pagination et tri :**
```razor
<MudTable Items="@Elements" 
          Dense="true" 
          Hover="true" 
          Striped="true"
          Filter="new Func<Element, bool>(FilterFunc)"
          @bind-SelectedItem="selectedItem"
          SortLabel="Trier par"
          CommitEditTooltip="Enregistrer"
          RowEditPreview="BackupItem"
          RowEditCancel="ResetItemToOriginalValues"
          RowEditCommit="ItemHasBeenCommitted">
    <ToolBarContent>
        <MudText Typo="Typo.h6">Utilisateurs</MudText>
        <MudSpacer />
        <MudTextField @bind-Value="searchString" 
                      Placeholder="Rechercher" 
                      Adornment="Adornment.Start" 
                      AdornmentIcon="@Icons.Material.Filled.Search" 
                      IconSize="Size.Medium" 
                      Class="mt-0" />
    </ToolBarContent>
    <HeaderContent>
        <MudTh><MudTableSortLabel SortBy="new Func<Element, object>(x => x.Name)">Nom</MudTableSortLabel></MudTh>
        <MudTh><MudTableSortLabel SortBy="new Func<Element, object>(x => x.Email)">Email</MudTableSortLabel></MudTh>
        <MudTh>Actions</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="Nom">@context.Name</MudTd>
        <MudTd DataLabel="Email">@context.Email</MudTd>
        <MudTd DataLabel="Actions">
            <MudIconButton Icon="@Icons.Material.Filled.Edit" OnClick="@(() => EditAsync(context.Id))" />
            <MudIconButton Icon="@Icons.Material.Filled.Delete" Color="Color.Error" OnClick="@(() => DeleteAsync(context.Id))" />
        </MudTd>
    </RowTemplate>
    <PagerContent>
        <MudTablePager PageSizeOptions="new int[] { 10, 25, 50, 100 }" />
    </PagerContent>
</MudTable>
```

### 8.6. Responsive Design

**Toujours définir les breakpoints :**
```razor
<MudGrid>
    <MudItem xs="12" sm="6" md="4" lg="3">
        <!-- 12 cols mobile, 6 tablet, 4 desktop, 3 large -->
    </MudItem>
</MudGrid>
```

**Utiliser MudHidden pour masquer sur certains écrans :**
```razor
<MudHidden Breakpoint="Breakpoint.SmAndDown">
    <MudText>Visible uniquement sur desktop</MudText>
</MudHidden>

<MudHidden Breakpoint="Breakpoint.MdAndUp" Invert="true">
    <MudText>Visible uniquement sur mobile/tablet</MudText>
</MudHidden>
```

**Tables responsive :**
```razor
<MudTable Breakpoint="Breakpoint.Sm" Items="@items">
    <!-- Devient une liste sur mobile -->
</MudTable>
```

### 8.7. Accessibilité (a11y)

**Toujours fournir AriaLabel pour les icônes :**
```razor
<MudIconButton Icon="@Icons.Material.Filled.Delete" 
               Color="Color.Error" 
               AriaLabel="Supprimer l'utilisateur"
               OnClick="Delete" />
```

**Utiliser HelperText et RequiredError :**
```razor
<MudTextField Label="Nom d'utilisateur" 
              HelperText="Entre 3 et 20 caractères"
              Required="true"
              RequiredError="Le nom est obligatoire" />
```

**Définir les rôles ARIA si nécessaire :**
```razor
<MudPaper role="alert" aria-live="polite">
    <MudAlert Severity="Severity.Warning">Attention</MudAlert>
</MudPaper>
```

### 8.8. Thèmes et Dark Mode

**Implémenter le Dark Mode :**
```razor
@* MainLayout.razor *@
<MudThemeProvider @bind-IsDarkMode="@_isDarkMode" Theme="@_theme" />
<MudToggleIconButton @bind-Toggled="@_isDarkMode"
                     Icon="@Icons.Material.Filled.DarkMode" 
                     ToggledIcon="@Icons.Material.Filled.LightMode"
                     Color="Color.Inherit" />
```

```csharp
// MainLayout.razor.cs
private bool _isDarkMode;
private MudTheme _theme = new()
{
    Palette = new PaletteLight()
    {
        Primary = "#1976D2",
        AppbarBackground = "#1976D2"
    },
    PaletteDark = new PaletteDark()
    {
        Primary = "#90CAF9",
        AppbarBackground = "#1A1A1A"
    }
};
```

### 8.9. Icons et Personnalisation

**Utiliser les icons Material Design :**
```razor
@using static MudBlazor.Icons.Material.Filled

<MudIcon Icon="@Home" />
<MudIcon Icon="@Person" Color="Color.Primary" />
<MudIcon Icon="@Delete" Color="Color.Error" Size="Size.Large" />
```

**Créer des icônes custom si nécessaire :**
```razor
<MudIcon Icon="@CustomIcons.MyIcon" />
```

```csharp
public static class CustomIcons
{
    public const string MyIcon = "<svg>...</svg>";
}
```

### 8.10. Éviter les Anti-Patterns

**❌ NE PAS utiliser StateHasChanged() partout :**
```csharp
// MAUVAIS
private void UpdateData()
{
    data = newData;
    StateHasChanged(); // Inutile dans la plupart des cas
}
```

**✅ Blazor gère automatiquement le re-render après les événements :**
```csharp
// BON
private void UpdateData()
{
    data = newData;
    // Pas besoin de StateHasChanged()
}
```

**❌ NE PAS créer des composants trop gros :**
```razor
<!-- MAUVAIS : 500+ lignes dans un seul composant -->
<MudGrid>
    <!-- Trop de logique ici -->
</MudGrid>
```

**✅ Décomposer en composants plus petits :**
```
UsersList.razor (page principale)
├── Parts/UsersFilter.razor
├── Parts/UsersToolbar.razor
└── UsersTable.razor (composant spécifique)
```

**❌ NE PAS ignorer le cycle de vie Blazor :**
```csharp
// MAUVAIS : Chargement synchrone dans OnInitialized
protected override void OnInitialized()
{
    data = Service.GetData(); // Bloque le thread
}
```

**✅ Utiliser les méthodes async :**
```csharp
// BON
protected override async Task OnInitializedAsync()
{
    isLoading = true;
    data = await Service.GetDataAsync();
    isLoading = false;
}
```

## 12. Bonnes Pratiques Avancées MudBlazor

### 12.1. Debouncing et Throttling

**Utiliser DebounceInterval pour recherches :**
```razor
<MudTextField @bind-Value="searchTerm" 
              Label="Recherche" 
              Immediate="false"
              DebounceInterval="300"
              OnDebounceIntervalElapsed="SearchAsync" />
```

```csharp
// Dans .razor.cs
private async Task SearchAsync()
{
    filteredItems = await SearchService.SearchAsync(searchTerm);
}
```

### 12.2. Gestion Mémoire et Disposables

**Toujours disposer les ressources :**
```csharp
public partial class MyComponent : IAsyncDisposable
{
    private CancellationTokenSource _cts = new();
    private IDisposable? _subscription;
    
    protected override void OnInitialized()
    {
        _subscription = EventService.Subscribe(HandleEvent);
    }
    
    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _subscription?.Dispose();
        
        if (_jsModule is not null)
        {
            await _jsModule.DisposeAsync();
        }
    }
}
```

### 12.3. Lazy Loading de Composants

**Charger des composants à la demande :**
```razor
@if (showHeavyComponent)
{
    <Suspense>
        <ChildContent>
            @* Composant lourd chargé dynamiquement *@
            <HeavyComponent />
        </ChildContent>
        <FallbackContent>
            <MudProgressCircular Indeterminate="true" />
        </FallbackContent>
    </Suspense>
}
```

### 12.4. Utilisation de RenderFragment

**Créer des composants flexibles avec RenderFragment :**
```csharp
[Parameter] public RenderFragment? Header { get; set; }
[Parameter] public RenderFragment<ItemType>? ItemTemplate { get; set; }
[Parameter] public RenderFragment? Footer { get; set; }
```

```razor
<MyList Items="@items">
    <Header>
        <MudText Typo="Typo.h5">Liste Custom</MudText>
    </Header>
    <ItemTemplate Context="item">
        <MudListItem>@item.Name</MudListItem>
    </ItemTemplate>
    <Footer>
        <MudButton>Charger plus</MudButton>
    </Footer>
</MyList>
```

### 12.5. Optimisation des Bindings

**Éviter les bindings bidirectionnels inutiles :**
```razor
@* MAUVAIS : Re-render à chaque touche *@
<MudTextField @bind-Value="model.Name" />

@* BON : Contrôle manuel *@
<MudTextField Value="@model.Name" 
              ValueChanged="@((string value) => HandleNameChanged(value))" 
              Immediate="false" />
```

### 12.6. Navigation et URL Management

**Utiliser NavigationManager correctement :**
```csharp
[Inject] private NavigationManager Navigation { get; set; }

private void NavigateToDetails(int id)
{
    Navigation.NavigateTo($"/users/{id}");
}

private void NavigateWithForce(string url)
{
    Navigation.NavigateTo(url, forceLoad: true); // Force full page reload
}

protected override void OnInitialized()
{
    Navigation.LocationChanged += HandleLocationChanged;
}

private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
{
    // Réagir aux changements d'URL
}
```

### 12.7. Interop JavaScript Optimisé

**Module JavaScript isolé :**
```csharp
// Dans .razor.cs
private IJSObjectReference? _jsModule;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./Components/MyComponent.razor.js");
            
        await _jsModule.InvokeVoidAsync("initialize", ElementRef);
    }
}

public async ValueTask DisposeAsync()
{
    if (_jsModule is not null)
    {
        await _jsModule.InvokeVoidAsync("dispose");
        await _jsModule.DisposeAsync();
    }
}
```

```javascript
// MyComponent.razor.js
export function initialize(element) {
    // Initialisation
}

export function dispose() {
    // Nettoyage
}
```

### 12.8. Gestion des Formulaires Complexes

**EditContext et FieldIdentifier :**
```csharp
private EditContext _editContext;

protected override void OnInitialized()
{
    _editContext = new EditContext(model);
    _editContext.OnFieldChanged += HandleFieldChanged;
}

private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
{
    // Valider un champ spécifique
    var messages = _editContext.GetValidationMessages(e.FieldIdentifier);
}
```

### 12.9. Patterns de Chargement et Cache

**Implémenter un cache simple :**
```csharp
private readonly Dictionary<int, UserDto> _cache = new();

private async Task<UserDto> GetUserAsync(int id)
{
    if (_cache.TryGetValue(id, out var cachedUser))
    {
        return cachedUser;
    }
    
    var user = await UserService.GetByIdAsync(id);
    _cache[id] = user;
    return user;
}

private void InvalidateCache(int id)
{
    _cache.Remove(id);
}
```

### 12.10. Composants Génériques

**Créer des composants réutilisables avec génériques :**
```razor
@* GenericList.razor *@
@typeparam TItem

<MudList>
    @foreach (var item in Items)
    {
        <MudListItem>
            @ItemTemplate(item)
        </MudListItem>
    }
</MudList>
```

```csharp
// GenericList.razor.cs
public partial class GenericList<TItem>
{
    [Parameter] public List<TItem> Items { get; set; } = new();
    [Parameter] public RenderFragment<TItem> ItemTemplate { get; set; }
}
```

**Utilisation :**
```razor
<GenericList TItem="UserDto" Items="@users">
    <ItemTemplate Context="user">
        <MudText>@user.Name</MudText>
    </ItemTemplate>
</GenericList>
```

### 12.11. Logging et Diagnostics

**Logger correctement dans les composants :**
```csharp
[Inject] private ILogger<MyComponent> Logger { get; set; }

private async Task LoadDataAsync()
{
    try
    {
        Logger.LogInformation("Chargement des données pour l'utilisateur {UserId}", userId);
        
        data = await DataService.GetDataAsync(userId);
        
        Logger.LogDebug("Données chargées: {Count} éléments", data.Count);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur lors du chargement des données pour {UserId}", userId);
        throw;
    }
}
```

### 12.12. Patterns de Retry et Circuit Breaker

**Utiliser Polly pour la résilience :**
```csharp
private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

public MyComponent()
{
    _retryPolicy = Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

private async Task<List<UserDto>> LoadUsersWithRetryAsync()
{
    var response = await _retryPolicy.ExecuteAsync(async () =>
        await HttpClient.GetAsync("/api/users"));
        
    return await response.Content.ReadFromJsonAsync<List<UserDto>>();
}
```

### 12.13. Authorization et Sécurité

**Vérifications d'autorisation dans les composants (avec textes localisés) :**
```razor
@attribute [Authorize(Roles = "Admin")]

<AuthorizeView Roles="Admin" Context="authContext">
    <Authorized>
        <MudButton Color="Color.Error" OnClick="DeleteAsync">
            @Localizer["Common.Delete"]
        </MudButton>
    </Authorized>
    <NotAuthorized>
        <MudText Color="Color.Secondary">
            @Localizer["Common.AccessDenied"]
        </MudText>
    </NotAuthorized>
</AuthorizeView>
```

```csharp
[Inject] private IAuthorizationService AuthService { get; set; }
[Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
[CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; }

private async Task<bool> CanDeleteAsync()
{
    var authState = await AuthStateTask;
    var result = await AuthService.AuthorizeAsync(
        authState.User, 
        "DeleteUserPolicy");
        
    return result.Succeeded;
}
```

### 12.14. Localisation et Internationalisation

**Support multilingue avec IStringLocalizer (OBLIGATOIRE partout) :**

❌ **INTERDIT - Texte en dur :**
```csharp
Snackbar.Add("Opération réussie", Severity.Success);
```

✅ **OBLIGATOIRE - IStringLocalizer :**
```csharp
[Inject] private IStringLocalizer<MyComponent> Localizer { get; set; }

private void SaveSuccess() 
{
    Snackbar.Add(Localizer["Common.OperationSuccess"], Severity.Success);
}
```

**Utilisation dans markup :**
```razor
<MudText>@Localizer["Welcome.Message"]</MudText>
<MudButton>@Localizer["Common.Save"]</MudButton>
<MudTextField Label="@Localizer["User.EmailLabel"]" 
              HelperText="@Localizer["User.EmailHelper"]" 
              RequiredError="@Localizer["Validation.EmailRequired"]" />
```

**Convention de nommage des clés :**
- `Common.*` : Textes réutilisables (Save, Cancel, Delete, etc.)
- `Validation.*` : Messages de validation
- `[Area].[Concern].*` : Textes spécifiques (Users.ListTitle, Account.LoginButton)
- `Error.*` : Messages d'erreur
- `Success.*` : Messages de succès

### 12.15. SignalR et Temps Réel

**Intégration SignalR avec MudBlazor (avec textes localisés) :**
```csharp
[Inject] private HubConnection HubConnection { get; set; }
[Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }

protected override async Task OnInitializedAsync()
{
    HubConnection.On<string, string>("ReceiveMessage", (user, message) =>
    {
        messages.Add($"{user}: {message}");
        var notification = Localizer["Chat.NewMessageFrom", user];
        Snackbar.Add(notification, Severity.Info);
        StateHasChanged();
    });
    
    await HubConnection.StartAsync();
}

private async Task SendMessageAsync()
{
    await HubConnection.SendAsync("SendMessage", userName, messageText);
}
```

## 13. Tests avec bUnit

**Créer des tests unitaires pour les composants Blazor :**
```csharp
public class MyComponentTests : TestContext
{
    [Fact]
    public void Component_Renders_With_MudButton()
    {
        // Arrange
        Services.AddMudServices();
        
        // Act
        var cut = RenderComponent<MyComponent>();
        
        // Assert
        cut.FindComponent<MudButton>().Should().NotBeNull();
    }
    
    [Fact]
    public async Task Button_Click_Shows_Snackbar()
    {
        // Arrange
        Services.AddMudServices();
        var cut = RenderComponent<MyComponent>();
        
        // Act
        var button = cut.Find("button");
        button.Click();
        
        // Assert
        var snackbar = cut.FindComponent<MudSnackbarProvider>();
        snackbar.Markup.Should().Contain("Success");
    }
}
```

## 10. Documentation de Référence

**Consulter systématiquement :**
- Documentation officielle : https://mudblazor.com/
- API Reference : https://mudblazor.com/api
- Exemples : https://mudblazor.com/components
- GitHub : https://github.com/MudBlazor/MudBlazor

## 11. Checklist de Révision de Code

Avant chaque commit, vérifier :

### 🚨 Critiques (Bloquants)
- [ ] **Séparation des fichiers** : 
  - Aucun `@code { }` dans .razor
  - Aucun `<style>` dans .razor
  - Aucun `<script>` dans .razor
  - Fichiers .razor.cs créés pour TOUT code C#
  - Fichiers .razor.css SEULEMENT si styles vraiment spécifiques (dernier recours)
  - Fichiers .razor.js créés si JavaScript nécessaire

- [ ] **Atomic Design** :
  - Composants organisés en Atoms/Molecules/Organisms/Templates/Pages
  - Réutilisabilité respectée (Atoms → Molecules → Organisms → Pages)
  - Pas de duplication entre niveaux

- [ ] **Internationalisation** :
  - ZÉRO texte en dur dans .razor ou .razor.cs
  - IStringLocalizer injecté dans TOUS les composants avec texte
  - Clés de ressources hiérarchiques (`Area.Concern.Action`)
  - Messages d'erreur, labels, tooltips, aria-labels → TOUS localisés

### 🎨 Styles et UI
- [ ] **Hiérarchie de styling respectée** :
  1. Classes utilitaires MudBlazor (`pa-*`, `ma-*`, `d-flex`)
  2. Classes CSS globales dans `site.css`
  3. Thème MudBlazor personnalisé
  4. `.razor.css` scoped (dernier recours uniquement)
- [ ] **Composants MudBlazor** : Aucun élément HTML natif (`<button>`, `<input>`, etc.)
- [ ] **Responsive** : Breakpoints définis (`xs="12" sm="6" md="4"`)
- [ ] **Dark Mode** : Composants compatibles avec les deux thèmes

### 📁 Structure et Organisation
- [ ] **Architecture Atomic Design** :
  - Atoms : Composants de base (boutons, inputs, labels)
  - Molecules : Combinaisons simples (SearchBox, FormField)
  - Organisms : Composants complexes (UsersTable, LoginForm)
  - Templates : Layouts (MainLayout, AdminLayout)
  - Pages : Routes organisées par Area/Concerns/Subconcerns
- [ ] **Réutilisabilité** : Composants au bon niveau Atomic Design
- [ ] **Nomenclature** : PascalCase pour composants, camelCase pour variables
- [ ] **Localisation** : Fichiers .resx organisés par niveau (Shared, Area, Component)

### ⚡ Performance
- [ ] **Virtualization** : Utilisé pour listes > 100 items
- [ ] **@key** : Défini pour listes dynamiques
- [ ] **Async/await** : Utilisé dans OnInitializedAsync
- [ ] **ShouldRender** : Optimisé si nécessaire
- [ ] **Disposables** : IDisposable/IAsyncDisposable implémenté si ressources/abonnements
- [ ] **CancellationToken** : Utilisé pour opérations async annulables

### 🎯 UX et États
- [ ] **Loading states** : MudProgressLinear ou MudSkeleton pendant chargement
- [ ] **États vides** : MudAlert avec message si aucune donnée
- [ ] **Gestion erreurs** : Try-catch avec Snackbar pour feedback
- [ ] **Confirmations** : Dialogues pour actions destructives
- [ ] **Snackbar** : Feedback utilisateur pour toutes les actions
- [ ] **Debouncing** : DebounceInterval sur champs de recherche

### ♿ Accessibilité
- [ ] **AriaLabel** : Défini sur tous les MudIconButton
- [ ] **Labels** : Présents sur tous les champs de formulaire
- [ ] **HelperText** : Fourni pour expliquer les champs complexes
- [ ] **RequiredError** : Message d'erreur personnalisé pour champs requis
- [ ] **Rôles ARIA** : Définis si nécessaire (role="alert", aria-live)

### 🔧 Services et Injection
- [ ] **Services MudBlazor** : ISnackbar, IDialogService injectés dans .razor.cs
- [ ] **Dépendances** : Injectées via @inject ou [Inject]
- [ ] **Navigation** : NavigationManager utilisé pour redirections
- [ ] **Logging** : ILogger utilisé pour diagnostics

### 📊 Tables et Données
- [ ] **MudTable** : Dense, Hover, Striped activés si approprié
- [ ] **Pagination** : MudTablePager avec options (10, 25, 50, 100)
- [ ] **Tri** : MudTableSortLabel sur colonnes pertinentes
- [ ] **Filtres** : Recherche avec MudTextField dans ToolBarContent
- [ ] **Actions** : Boutons Edit/Delete avec confirmations

### ✅ Validation et Formulaires
- [ ] **MudForm** : Utilisé avec validation
- [ ] **Required** : Défini avec RequiredError personnalisé
- [ ] **Validation** : FluentValidation ou DataAnnotations
- [ ] **For** : Expression lambda pour liaison validation
- [ ] **IsValid** : Vérifié avant soumission
- [ ] **EditContext** : Utilisé pour validation avancée si nécessaire

### 🧪 Tests
- [ ] **bUnit** : Tests créés pour composants critiques
- [ ] **Services mockés** : AddMudServices() dans tests
- [ ] **Interactions** : Click, input testés
- [ ] **Assertions** : FindComponent utilisé pour vérifier rendu
- [ ] **EventCallback** : Testés avec InvokeAsync

### 📚 Documentation
- [ ] **Commentaires** : XML comments sur composants publics
- [ ] **README** : Documentation d'usage si composant réutilisable
- [ ] **Exemples** : Fournis pour composants complexes

### 🔐 Sécurité
- [ ] **Authorization** : AuthorizeView ou [Authorize] utilisé si nécessaire
- [ ] **Validation serveur** : Toujours valider côté serveur aussi
- [ ] **Données sensibles** : Jamais exposées côté client
- [ ] **CORS** : Configuré correctement si appels API externes

### 🚀 Bonnes Pratiques Avancées
- [ ] **EventCallback** : Utilisé pour communication parent-enfant
- [ ] **CascadingParameter** : Utilisé pour données partagées
- [ ] **Cycle de vie** : Méthodes OnInitialized/OnParametersSet utilisées correctement
- [ ] **JavaScript Interop** : Module isolé (.razor.js) si JavaScript nécessaire
- [ ] **Génériques** : Composants génériques pour réutilisabilité maximale
- [ ] **RenderFragment** : Utilisé pour flexibilité des composants
- [ ] **Cache** : Implémenté pour données fréquemment accédées
- [ ] **Retry Policy** : Polly utilisé pour résilience si appels externes
- [ ] **SignalR** : Intégré correctement si temps réel nécessaire
- [ ] **Localisation** : IStringLocalizer utilisé PARTOUT (OBLIGATOIRE)

### 🌍 Internationalisation
- [ ] **Textes localisés** : ZÉRO texte en dur, tout via IStringLocalizer
- [ ] **Clés ressources** : Convention `Area.Concern.Action` respectée
- [ ] **Messages validation** : Tous les RequiredError, validation messages localisés
- [ ] **AriaLabel** : Tous les aria-labels localisés
- [ ] **Snackbar/Dialogs** : Tous les messages localisés
- [ ] **Fichiers .resx** : Organisés par niveau (Shared, Area, Component)
- [ ] **Culture** : Dates/nombres formatés selon CultureInfo.CurrentCulture

### Vérification Automatique

**Commandes PowerShell pour valider les règles :**

```powershell
# 1. Trouver les fichiers .razor qui violent la séparation
Get-ChildItem -Recurse -Filter "*.razor" | Select-String -Pattern "(@code|<style|<script)" | Select-Object Path, LineNumber

# 2. Trouver les éléments HTML natifs dans .razor
Get-ChildItem -Recurse -Filter "*.razor" | Select-String -Pattern "(<button|<input|<div class=)" | Select-Object Path, LineNumber

# 3. Trouver les CSS inline
Get-ChildItem -Recurse -Filter "*.razor" | Select-String -Pattern 'style="' | Select-Object Path, LineNumber

# 4. Lister tous les .razor.css (vérifier qu'ils sont justifiés)
Get-ChildItem -Recurse -Filter "*.razor.css" | Select-Object FullName

# 5. Trouver les composants sans Dispose si IDisposable/EventCallback utilisés
Get-ChildItem -Recurse -Filter "*.razor.cs" | Select-String -Pattern "EventCallback|Timer|HttpClient" | Where-Object { $_.Line -notmatch "Dispose" }

# 6. Vérifier la présence de AriaLabel sur IconButton
Get-ChildItem -Recurse -Filter "*.razor" | Select-String -Pattern "MudIconButton" | Where-Object { $_.Line -notmatch "AriaLabel" }

# 7. Trouver les textes en dur dans .razor (violation internationalisation)
Get-ChildItem -Recurse -Filter "*.razor" | Select-String -Pattern '(>)[A-Za-zÀ-ÿ\s]{3,}(<|@)' | Where-Object { $_.Line -notmatch "@Localizer" } | Select-Object Path, LineNumber

# 8. Trouver les textes en dur dans .razor.cs (Snackbar, dialogs, etc.)
Get-ChildItem -Recurse -Filter "*.razor.cs" | Select-String -Pattern '(Snackbar\.Add|DialogParameters.*=|RequiredError\s*=)\s*"[^@]' | Where-Object { $_.Line -notmatch "Localizer" } | Select-Object Path, LineNumber

# 9. Vérifier l'organisation Atomic Design
$atomsCount = (Get-ChildItem -Path "Components/Atoms" -Recurse -Filter "*.razor" -ErrorAction SilentlyContinue).Count
$moleculesCount = (Get-ChildItem -Path "Components/Molecules" -Recurse -Filter "*.razor" -ErrorAction SilentlyContinue).Count
$organismsCount = (Get-ChildItem -Path "Components/Organisms" -Recurse -Filter "*.razor" -ErrorAction SilentlyContinue).Count
Write-Host "Atomic Design: Atoms=$atomsCount, Molecules=$moleculesCount, Organisms=$organismsCount"
```

**Si ces commandes (1, 2, 3, 6, 7, 8) retournent des résultats, le code ne respecte PAS les règles.**
**La commande 9 doit montrer des composants dans Atoms, Molecules et Organisms.**

---

## Résumé des 3 Règles Absolues

### ⚠️ RÈGLE #1 : Séparation des Fichiers
- ❌ Jamais de `@code {}`, `<style>`, `<script>` dans .razor
- ✅ Toujours .razor.cs pour le code C#
- ✅ .razor.css uniquement si style vraiment spécifique au composant

### ⚠️ RÈGLE #2 : Atomic Design
- ✅ Atoms : Composants de base (boutons, inputs)
- ✅ Molecules : Combinaisons simples (SearchBox = TextField + Button)
- ✅ Organisms : Composants complexes (UsersTable avec filtres)
- ✅ Templates : Layouts réutilisables
- ✅ Pages : Routes organisées par Area/Concerns/Subconcerns

### ⚠️ RÈGLE #3 : Internationalisation
- ❌ ZÉRO texte en dur dans .razor ou .razor.cs
- ✅ IStringLocalizer injecté partout
- ✅ Clés hiérarchiques : `Area.Concern.Action`
- ✅ Tous les textes : labels, messages, erreurs, tooltips, aria-labels

**Flux de développement :**
1. Créer fichiers de ressources .resx (fr, en)
2. Créer Atom avec IStringLocalizer
3. Composer Molecule à partir d'Atoms
4. Composer Organism à partir de Molecules
5. Composer Page utilisant Organisms dans Template
6. Vérifier avec commandes PowerShell avant commit