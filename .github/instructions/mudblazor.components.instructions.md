---
description: Composants MudBlazor - AppBar, Navigation, Cards, Boutons, Tables, Formulaires, Alerts
name: MudBlazor_Components_Patterns
applyTo: "**Presentation/**/*.razor"
---

# MudBlazor - Patterns de Composants

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** de balises HTML natives (`<div>`, `<span>`) - composants MudBlazor uniquement
- **N'oublie jamais** `Dense="true"` pour les interfaces compactes
- **Ne hardcode jamais** les textes - utilise `IStringLocalizer` pour l'i18n
- **N'utilise jamais** `Elevation` >2 (design minimaliste)
- **Ne crée jamais** de bouton sans `Variant` explicite
- **N'imbrique jamais** excessivement les composants (max 4 niveaux)
- **N'oublie jamais** le type générique `T="string"` sur MudChip, MudSelect, etc.

## ✅ À FAIRE

- **Utilise toujours** les composants MudBlazor natifs (MudStack, MudPaper, MudText)
- **Spécifie toujours** `Variant`, `Color`, `Size` explicitement
- **Utilise toujours** `MudSpacer` pour l'espacement flexible
- **Préfère toujours** `MudStack` à `MudGrid` pour les layouts simples
- **Utilise toujours** les icônes Material Design (`Icons.Material.*`)
- **Applique toujours** `Class="text-none"` sur les boutons pour éviter le uppercase
- **Valide toujours** les formulaires avec `MudForm` et `FluentValidation`

## 🎯 AppBar et Navigation

### AppBar Minimaliste

```razor
<MudAppBar Elevation="0" Dense="true" Class="app-bar-minimal">
  <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="3" Class="w-100">
    <MudButton Href="./" Variant="Variant.Text" Color="Color.Default" Class="text-none">
      <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="2">
        <MudIcon Icon="@Icons.Material.Filled.FlightTakeoff" Color="Color.Primary" Size="Size.Large"/>
        <MudText Typo="Typo.h6" Class="app-title">Application</MudText>
      </MudStack>
    </MudButton>
    <MudSpacer/>
    <MudChip T="string" Icon="@Icons.Material.Filled.Person" Color="Color.Default">
      @currentUserDisplayName
    </MudChip>
  </MudStack>
</MudAppBar>
```

### Drawer Minimaliste

```razor
<MudDrawer Open="true" 
           ClipMode="DrawerClipMode.Always" 
           Variant="DrawerVariant.Mini" 
           OpenMiniOnHover="true"
           Elevation="0"
           Class="drawer-minimal">
  <MudStack Spacing="0" Class="drawer-content">
    <MudStack AlignItems="AlignItems.Center" Spacing="1" Class="drawer-header">
      <MudIcon Icon="@Icons.Material.Filled.AdminPanelSettings" Size="Size.Large" Color="Color.Primary"/>
      <MudText Typo="Typo.caption" Color="Color.Secondary" Class="drawer-caption">
        @Localizer["App.Management"]
      </MudText>
    </MudStack>
    <NavMenu/>
  </MudStack>
</MudDrawer>
```

### NavLink Minimaliste

```razor
<MudNavLink Href="./dashboard"
            Icon="@Icons.Material.Filled.Dashboard"
            IconColor="Color.Default"
            Class="nav-link-minimal"
            ActiveClass="active-nav-item">
  @Localizer["Nav.Dashboard"]
</MudNavLink>

<MudNavLink Href="./collections"
            Icon="@Icons.Material.Filled.Collections"
            IconColor="Color.Default"
            Class="nav-link-minimal"
            ActiveClass="active-nav-item">
  @Localizer["Nav.Collections"]
</MudNavLink>
```

## 🃏 Cards et Papers

### Card Statistique

```razor
<MudPaper Elevation="0" Class="card-stat">
  <MudStack Spacing="2">
    <MudStack Row="true" AlignItems="AlignItems.Center" Justify="Justify.SpaceBetween">
      <MudIcon Icon="@Icons.Material.Filled.Collections" Color="Color.Primary" Size="Size.Medium"/>
      <MudChip T="string" Size="Size.Small" Color="Color.Info" Class="chip-stat">
        @count
      </MudChip>
    </MudStack>
    <MudText Typo="Typo.body2" Color="Color.Secondary">
      @Localizer["Dashboard.Collections"]
    </MudText>
  </MudStack>
</MudPaper>
```

### Card Section avec Titre

```razor
<MudPaper Elevation="0" Class="card-section">
  <MudStack Spacing="3">
    <!-- En-tête de section -->
    <MudStack Row="true" AlignItems="AlignItems.Center" Justify="Justify.SpaceBetween">
      <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="2">
        <MudIcon Icon="@Icons.Material.Filled.Analytics" Color="Color.Primary" Size="Size.Large"/>
        <MudText Typo="Typo.h6" Class="section-title">
          @Localizer["Dashboard.Analytics"]
        </MudText>
      </MudStack>
      <MudButton Variant="Variant.Text" 
                 Color="Color.Primary" 
                 EndIcon="@Icons.Material.Filled.ArrowForward"
                 Class="text-none">
        @Localizer["Common.ViewAll"]
      </MudButton>
    </MudStack>
    
    <!-- Contenu de la section -->
    <MudStack Spacing="2">
      <!-- Contenu ici -->
    </MudStack>
  </MudStack>
</MudPaper>
```

### Card Interactive

```razor
<MudPaper Elevation="0" Class="card-interactive" @onclick="@HandleClick">
  <MudStack Spacing="2">
    <MudStack Row="true" AlignItems="AlignItems.Center" Justify="Justify.SpaceBetween">
      <MudText Typo="Typo.h6">@title</MudText>
      <MudIcon Icon="@Icons.Material.Filled.ArrowForward" Color="Color.Primary"/>
    </MudStack>
    <MudText Typo="Typo.body2" Color="Color.Secondary">
      @description
    </MudText>
  </MudStack>
</MudPaper>
```

## 🔘 Boutons

### Hiérarchie de Boutons

```razor
<MudStack Row="true" Spacing="2" AlignItems="AlignItems.Center">
  <!-- Bouton principal (action primaire) -->
  <MudButton Variant="Variant.Filled" 
             Color="Color.Primary" 
             StartIcon="@Icons.Material.Filled.Save"
             Class="text-none"
             OnClick="@SaveAsync">
    @Localizer["Common.Save"]
  </MudButton>

  <!-- Bouton secondaire (action secondaire) -->
  <MudButton Variant="Variant.Outlined" 
             Color="Color.Default"
             StartIcon="@Icons.Material.Filled.Cancel"
             Class="text-none btn-outlined-minimal"
             OnClick="@Cancel">
    @Localizer["Common.Cancel"]
  </MudButton>

  <!-- Bouton tertiaire (action mineure) -->
  <MudButton Variant="Variant.Text" 
             Color="Color.Secondary"
             StartIcon="@Icons.Material.Filled.ArrowBack"
             Class="text-none"
             OnClick="@GoBack">
    @Localizer["Common.Back"]
  </MudButton>
</MudStack>
```

### IconButton

```razor
<!-- IconButton avec tooltip -->
<MudTooltip Text="@Localizer["Common.Edit"]">
  <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                 Size="Size.Small" 
                 Color="Color.Default"
                 AriaLabel="@Localizer["Common.Edit"]"
                 OnClick="@EditAsync"/>
</MudTooltip>

<MudTooltip Text="@Localizer["Common.Delete"]">
  <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                 Size="Size.Small" 
                 Color="Color.Default"
                 AriaLabel="@Localizer["Common.Delete"]"
                 OnClick="@DeleteAsync"/>
</MudTooltip>
```

### Boutons d'Actions en Groupe

```razor
<MudStack Row="true" Spacing="1" Class="action-buttons">
  <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                 Size="Size.Small" 
                 Color="Color.Default"
                 AriaLabel="@Localizer["Common.Edit"]"/>
  <MudIconButton Icon="@Icons.Material.Filled.ContentCopy" 
                 Size="Size.Small" 
                 Color="Color.Default"
                 AriaLabel="@Localizer["Common.Duplicate"]"/>
  <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                 Size="Size.Small" 
                 Color="Color.Default"
                 AriaLabel="@Localizer["Common.Delete"]"/>
</MudStack>
```

## 📋 Tables

### Table Simple

```razor
<MudTable Items="@items" 
          Elevation="0"
          Class="table-minimal"
          Dense="true"
          Hover="true">
  <HeaderContent>
    <MudTh Class="table-header">@Localizer["User.Name"]</MudTh>
    <MudTh Class="table-header">@Localizer["User.Email"]</MudTh>
    <MudTh Class="table-header">@Localizer["Common.Actions"]</MudTh>
  </HeaderContent>
  <RowTemplate>
    <MudTd>@context.Name</MudTd>
    <MudTd>@context.Email</MudTd>
    <MudTd>
      <MudStack Row="true" Spacing="1">
        <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                       Size="Size.Small" 
                       Color="Color.Default"
                       AriaLabel="@Localizer["Common.Edit"]"/>
        <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                       Size="Size.Small" 
                       Color="Color.Default"
                       AriaLabel="@Localizer["Common.Delete"]"/>
      </MudStack>
    </MudTd>
  </RowTemplate>
</MudTable>
```

### DataGrid Avancé

```razor
<MudDataGrid T="PromptDto" 
             Items="@items"
             Elevation="0"
             Class="table-minimal"
             Dense="true"
             Hover="true"
             ReadOnly="false"
             EditMode="DataGridEditMode.Cell">
  <Columns>
    <PropertyColumn Property="x => x.Title" Title="@Localizer["Prompt.Title"]"/>
    <PropertyColumn Property="x => x.Description" Title="@Localizer["Prompt.Description"]"/>
    <PropertyColumn Property="x => x.CreatedAt" Title="@Localizer["Common.CreatedAt"]"/>
    <TemplateColumn Title="@Localizer["Common.Actions"]">
      <CellTemplate>
        <MudStack Row="true" Spacing="1">
          <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                         Size="Size.Small" 
                         Color="Color.Default"
                         AriaLabel="@Localizer["Common.Edit"]"/>
          <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                         Size="Size.Small" 
                         Color="Color.Default"
                         AriaLabel="@Localizer["Common.Delete"]"/>
        </MudStack>
      </CellTemplate>
    </TemplateColumn>
  </Columns>
</MudDataGrid>
```

## 📝 Formulaires

### Champs de Formulaire

```razor
<MudForm @ref="form" @bind-IsValid="@isValid">
  <MudStack Spacing="3">
    <!-- TextField -->
    <MudTextField @bind-Value="model.Name" 
                  Label="@Localizer["User.Name"]"
                  Variant="Variant.Outlined"
                  Margin="Margin.Dense"
                  Required="true"
                  RequiredError="@Localizer["Validation.Required"]"/>

    <!-- TextField multiline -->
    <MudTextField @bind-Value="model.Description" 
                  Label="@Localizer["Common.Description"]"
                  Variant="Variant.Outlined"
                  Margin="Margin.Dense"
                  Lines="4"/>

    <!-- Select -->
    <MudSelect @bind-Value="model.Role" 
               Label="@Localizer["User.Role"]"
               Variant="Variant.Outlined"
               Margin="Margin.Dense"
               Required="true">
      <MudSelectItem Value="@("Admin")">@Localizer["Role.Admin"]</MudSelectItem>
      <MudSelectItem Value="@("User")">@Localizer["Role.User"]</MudSelectItem>
    </MudSelect>

    <!-- Autocomplete -->
    <MudAutocomplete T="string" 
                     @bind-Value="model.Category"
                     Label="@Localizer["Common.Category"]"
                     Variant="Variant.Outlined"
                     Margin="Margin.Dense"
                     SearchFunc="@SearchCategories"
                     ResetValueOnEmptyText="true"/>

    <!-- DatePicker -->
    <MudDatePicker @bind-Date="model.Date"
                   Label="@Localizer["Common.Date"]"
                   Variant="Variant.Outlined"
                   Margin="Margin.Dense"/>

    <!-- Checkbox -->
    <MudCheckBox @bind-Value="model.IsActive" 
                 Label="@Localizer["Common.Active"]"
                 Color="Color.Primary"/>

    <!-- Switch -->
    <MudSwitch @bind-Value="model.EnableNotifications" 
               Label="@Localizer["Settings.Notifications"]"
               Color="Color.Primary"/>
  </MudStack>
</MudForm>
```

### Actions de Formulaire

```razor
<MudStack Row="true" Justify="Justify.FlexEnd" Spacing="2" Class="form-actions">
  <MudButton Variant="Variant.Text" 
             Color="Color.Secondary"
             OnClick="@Cancel"
             Class="text-none">
    @Localizer["Common.Cancel"]
  </MudButton>
  <MudButton Variant="Variant.Filled" 
             Color="Color.Primary"
             OnClick="@SubmitAsync"
             Disabled="@(!isValid)"
             Class="text-none">
    @Localizer["Common.Save"]
  </MudButton>
</MudStack>
```

## 🔔 Alerts et Messages

### Alerts

```razor
<!-- Succès -->
<MudAlert Severity="Severity.Success" Class="alert-success">
  @Localizer["Common.OperationSuccess"]
</MudAlert>

<!-- Information -->
<MudAlert Severity="Severity.Info" Class="alert-info">
  @Localizer["Common.Information"]
</MudAlert>

<!-- Avertissement -->
<MudAlert Severity="Severity.Warning" Class="alert-warning">
  @Localizer["Common.Warning"]
</MudAlert>

<!-- Erreur -->
<MudAlert Severity="Severity.Error" Class="alert-error">
  @Localizer["Common.Error"]
</MudAlert>

<!-- Alert avec icône et actions -->
<MudAlert Severity="Severity.Info" 
          Variant="Variant.Outlined"
          ContentAlignment="HorizontalAlignment.Left"
          CloseIcon="@Icons.Material.Filled.Close"
          CloseIconClicked="@(() => showAlert = false)">
  @Localizer["Common.InfoMessage"]
</MudAlert>
```

### Snackbar (Code-behind)

```csharp
/ Component.razor.cs
[Inject] private ISnackbar Snackbar { get; set; }
[Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }

private void ShowSuccess()
{
  Snackbar.Add(
    Localizer["Common.OperationSuccess"], 
    Severity.Success,
    config => config.CloseAfterSeconds = 3
  );
}

private void ShowError(string message)
{
  Snackbar.Add(
    message, 
    Severity.Error,
    config => config.CloseAfterSeconds = 5
  );
}
```

## 🔄 États de Chargement

### Progress Linear

```razor
@if (isLoading)
{
  <MudProgressLinear Color="Color.Primary" Indeterminate="true" Class="loading-bar"/>
}
```

### Progress Circular

```razor
@if (isLoading)
{
  <MudStack AlignItems="AlignItems.Center" Spacing="3" Class="loading-container">
    <MudProgressCircular Color="Color.Primary" Indeterminate="true"/>
    <MudText Typo="Typo.body2" Color="Color.Secondary">
      @Localizer["Common.Loading"]
    </MudText>
  </MudStack>
}
```

### État Vide

```razor
@if (!items.Any())
{
  <MudPaper Elevation="0" Class="empty-state">
    <MudStack AlignItems="AlignItems.Center" Spacing="3">
      <MudIcon Icon="@Icons.Material.Filled.Info" 
               Size="Size.Large" 
               Color="Color.Secondary"/>
      <MudText Typo="Typo.body1" Color="Color.Secondary">
        @Localizer["Common.NoDataAvailable"]
      </MudText>
      <MudButton Variant="Variant.Filled" 
                 Color="Color.Primary"
                 StartIcon="@Icons.Material.Filled.Add"
                 OnClick="@CreateNew"
                 Class="text-none">
        @Localizer["Common.CreateNew"]
      </MudButton>
    </MudStack>
  </MudPaper>
}
```

## 📋 Checklist Composants

### ✅ AppBar et Navigation
- [ ] AppBar avec Elevation="0" et bordure bottom
- [ ] Drawer avec Variant.Mini et OpenMiniOnHover
- [ ] NavLink avec classes custom et ActiveClass
- [ ] Icônes Material Design uniquement

### ✅ Cards
- [ ] Elevation="0" pour toutes les cards
- [ ] Padding 20-24px cohérent
- [ ] Bordure #e0e0e0 au lieu d'ombre
- [ ] Spacing="2" ou "3" pour contenu interne

### ✅ Boutons
- [ ] Hiérarchie claire : Filled > Outlined > Text
- [ ] text-none pour désactiver text-transform
- [ ] StartIcon ou EndIcon pour icônes
- [ ] AriaLabel obligatoire sur IconButton

### ✅ Tables
- [ ] Elevation="0" et Dense="true"
- [ ] Classes custom pour en-têtes
- [ ] Hover="true" pour interactivité
- [ ] Actions groupées avec IconButton

### ✅ Formulaires
- [ ] Variant.Outlined pour tous les champs
- [ ] Margin.Dense pour espacement compact
- [ ] Required et validation obligatoires
- [ ] Labels localisés avec IStringLocalizer

### ✅ Alerts et États
- [ ] Classes custom pour couleurs cohérentes
- [ ] Snackbar avec durée appropriée
- [ ] États de chargement avec Progress
- [ ] État vide avec icône et message

## 📚 Ressources

### Documentation Officielle
- [MudBlazor Components](https:/mudblazor.com/components)
- [MudAppBar](https:/mudblazor.com/components/appbar)
- [MudDrawer](https:/mudblazor.com/components/drawer)
- [MudButton](https:/mudblazor.com/components/button)
- [MudTable](https:/mudblazor.com/components/table)
- [MudDataGrid](https:/mudblazor.com/components/datagrid)
- [MudForm](https:/mudblazor.com/components/form)
