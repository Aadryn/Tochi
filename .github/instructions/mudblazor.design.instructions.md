---
description: "⚠️ FICHIER DÉPRÉCIÉ - Voir fichiers spécialisés : mudblazor.core, mudblazor.design.principles, mudblazor.components, mudblazor.architecture, css.architecture"
name: MudBlazor_Design_System_DEPRECATED
applyTo: "**/.deprecated/**"
---

# ⚠️ FICHIER DÉPRÉCIÉ

## ⛔ À NE PAS FAIRE

- **Ne lis plus** ce fichier - il est obsolète
- **N'applique plus** les règles de ce fichier
- **Ne référence plus** ce fichier dans les imports

## ✅ À FAIRE

- **Consulte** les fichiers spécialisés à la place :
  - `mudblazor.core.instructions.md` - Règles fondamentales
  - `mudblazor.design.principles.instructions.md` - Principes de design
  - `mudblazor.components.instructions.md` - Patterns de composants
  - `mudblazor.architecture.instructions.md` - Architecture
  - `css.architecture.instructions.md` - Classes CSS

Ce fichier a été **découpé en plusieurs fichiers spécialisés** pour une meilleure maintenabilité :

## 📂 Nouveaux Fichiers d'Instructions

### 1. **mudblazor.core.instructions.md**
**Règles fondamentales obligatoires :**
- Composants MudBlazor UNIQUEMENT (❌ pas de HTML natif)
- Séparation stricte des fichiers (.razor, .razor.cs, .razor.css)
- Internationalisation obligatoire (IStringLocalizer)
- Propriétés natives MudBlazor en priorité
- Layout avec MudStack et MudGrid

**applyTo:** `**/*.razor,**/*.razor.cs`

---

### 2. **mudblazor.design.principles.instructions.md**
**Principes de design minimaliste :**
- Palette de couleurs clair/gris/blanc (#0288d1, #f5f5f5, #ffffff, #e0e0e0)
- Espacement cohérent (Spacing="3" ≈ 24px)
- Typographie (Typo.h5, Typo.h6, Typo.body1, Typo.body2)
- Icônes Material Design uniquement
- Élévations minimales (Elevation="0" ou "1")
- Bordures subtiles au lieu d'ombres fortes

**applyTo:** `**/*.razor,**/*.razor.cs`

---

### 3. **mudblazor.components.instructions.md**
**Patterns de composants :**
- AppBar et Navigation (AppBar, Drawer, NavLink)
- Cards et Papers (Card stat, Card section, Card interactive)
- Boutons (Hiérarchie, IconButton, Groupes d'actions)
- Tables (MudTable, MudDataGrid)
- Formulaires (TextField, Select, Autocomplete, DatePicker)
- Alerts et Messages (MudAlert, Snackbar)
- États de chargement (Progress, État vide)

**applyTo:** `**/*.razor`

---

### 4. **mudblazor.architecture.instructions.md**
**Architecture et découpage :**
- Principe de responsabilité unique
- Organisation par couches (Foundation, Composition, Features)
- Documentation des composants (README.md obligatoire)
- State management (EventCallback, ViewModel)
- Performance (Virtualisation, Debounce, ShouldRender)
- Sécurité (Validation, Anti-Forgery, InputType)
- Tests (bUnit, data-test attributes)

**applyTo:** `**/*.razor,**/*.razor.cs`

---

### 5. **css.architecture.instructions.md**
**Architecture CSS complète :**
- Hiérarchie de styling (5 niveaux)
- Classes CSS composables (atomiques et composées)
- Variables CSS (design tokens)
- Convention de nommage (BEM simplifié)
- Organisation des fichiers (app.css, tokens.css, utilities.css)
- Migration guide (PowerShell scripts de validation)

**applyTo:** `**/*.{css,razor.css}`

---

## 🔄 Migration

**Ce fichier sera supprimé dans une prochaine version.**

**Action requise :** Mettre à jour les références pour pointer vers les nouveaux fichiers spécialisés.

---

# ⬇️ CONTENU DÉPRÉCIÉ CI-DESSOUS (À IGNORER)

---

# MudBlazor - Design System Minimaliste (DÉPRÉCIÉ)

## 🎨 Philosophie de Design

### Design Minimaliste Clair/Gris/Blanc

**Palette de couleurs OBLIGATOIRE :**
```csharp
private readonly MudTheme _theme = new()
{
  PaletteLight = new PaletteLight
  {
    Primary = "#0288d1",        // Bleu clair principal
    Secondary = "#78909c",      // Gris-bleu secondaire
    Background = "#f5f5f5",     // Gris très clair
    Surface = "#ffffff",        // Blanc
    AppbarBackground = "#ffffff", // Blanc pour AppBar
    DrawerBackground = "#fafafa", // Gris ultra-clair pour Drawer
    TextPrimary = "#212121",    // Gris très foncé pour texte principal
    TextSecondary = "#757575",  // Gris moyen pour texte secondaire
    Divider = "#e0e0e0",        // Gris clair pour séparateurs
    LinesDefault = "#e0e0e0"    // Gris clair pour bordures
  }
};
```

**Principes du design minimaliste :**
- ✅ Espaces blancs généreux (padding, margin)
- ✅ Bordures subtiles (#e0e0e0) au lieu d'ombres fortes
- ✅ Élévations minimales (Elevation="0" ou "1")
- ✅ Typographie claire et lisible
- ✅ Icônes Material Design uniquement
- ❌ Pas de dégradés colorés
- ❌ Pas d'ombres portées lourdes
- ❌ Pas de couleurs vives multiples

**Principes de code OBLIGATOIRES :**

### 1. Composants MudBlazor UNIQUEMENT
- ✅ **TOUJOURS** utiliser les composants MudBlazor (MudStack, MudPaper, MudText, MudButton, etc.)
- ❌ **JAMAIS** de balises HTML natives (`<div>`, `<span>`, `<p>`, `<section>`, `<header>`, etc.)
- ❌ **INTERDICTION ABSOLUE** de créer des `<div>` pour layout - utiliser `MudStack` ou `MudGrid`

### 2. Classes CSS Composables OBLIGATOIRES
- ✅ **SYSTÉMATIQUEMENT** créer des classes CSS composables et réutilisables
- ✅ **CENTRALISER** tous les styles dans `wwwroot/app.css` (styles globaux)
- ✅ **ISOLER** uniquement les styles spécifiques au composant dans `.razor.css` (scoped)
- ❌ **INTERDICTION** d'utiliser l'attribut `Style=""` pour des styles statiques
- ✅ **AUTORISER** `Style=""` UNIQUEMENT pour valeurs dynamiques calculées en C#

### 3. Propriétés Natives MudBlazor en Priorité
- ✅ **TOUJOURS** privilégier les propriétés natives : `Elevation`, `Color`, `Variant`, `Size`, `Dense`, `Outlined`
- ✅ Utiliser `Class="ma-classe"` pour appliquer des styles personnalisés
- ❌ Ne JAMAIS dupliquer ce que MudBlazor offre nativement

### Exemples de Composants Minimalistes avec Classes Composables

**AppBar minimaliste :**
```razor
<!-- ✅ EXCELLENT : Propriétés natives + Classes composables -->
<MudAppBar Elevation="0" Dense="true" Class="app-bar-minimal">
  <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="3" Class="w-100">
    <MudIcon Icon="@Icons.Material.Filled.FlightTakeoff" Color="Color.Primary" Size="Size.Large"/>
    <MudText Typo="Typo.h6" Class="app-title">Application</MudText>
    <MudSpacer/>
    <MudChip T="string" Icon="@Icons.Material.Filled.Person" Color="Color.Default">Utilisateur</MudChip>
  </MudStack>
</MudAppBar>
```

```css
/* wwwroot/app.css - Classes réutilisables globales */
.app-bar-minimal {
  background: white;
  border-bottom: 1px solid #e0e0e0;
}

.app-title {
  font-weight: 600;
  color: #212121;
  line-height: 1.2;
}

.w-100 {
  width: 100%;
}
```

**❌ MAUVAIS - Balises HTML + Style inline :**
```razor
<!-- INTERDIT : <div> + Style inline -->
<div style="background: white; border-bottom: 1px solid #e0e0e0; padding: 8px 16px;">
  <div style="display: flex; align-items: center;">
    <span style="font-weight: 600; color: #212121;">Application</span>
  </div>
</div>
```

**Drawer minimaliste :**
```razor
<MudDrawer Open="true" 
           ClipMode="DrawerClipMode.Always" 
           Variant="DrawerVariant.Mini" 
           OpenMiniOnHover="true"
           Elevation="0"
           Class="drawer-minimal">
  <!-- Navigation -->
</MudDrawer>
```

**NavMenu minimaliste :**
```razor
<MudNavLink Href="/dashboard"
            Icon="@Icons.Material.Filled.Dashboard"
            IconColor="Color.Default"
            Class="nav-link-minimal"
            ActiveClass="active-nav-item">
  Dashboard
</MudNavLink>
```

**Card minimaliste :**
```razor
<MudPaper Elevation="0" Class="card-stat">
  <MudStack Spacing="2">
    <MudStack Row="true" AlignItems="AlignItems.Center" Justify="Justify.SpaceBetween">
      <MudIcon Icon="@Icons.Material.Filled.Analytics" Color="Color.Primary" Size="Size.Medium"/>
      <MudChip T="string" Size="Size.Small" Color="Color.Info" Variant="Variant.Filled">156</MudChip>
    </MudStack>
    <MudText Typo="Typo.body2" Color="Color.Secondary">Analytics</MudText>
  </MudStack>
</MudPaper>
```

**Boutons minimalistes :**
```razor
<!-- Bouton principal -->
<MudButton Variant="Variant.Filled" Color="Color.Primary" Class="text-none">
  Enregistrer
</MudButton>

<!-- Bouton secondaire -->
<MudButton Variant="Variant.Outlined" Color="Color.Default" Class="text-none">
  Annuler
</MudButton>

<!-- Bouton texte -->
<MudButton Variant="Variant.Text" Color="Color.Secondary" Class="text-none">
  Retour
</MudButton>
```

## 🚨 RÈGLES ABSOLUES (NON NÉGOCIABLES)

### 0.1. Séparation Stricte des Fichiers

**Structure OBLIGATOIRE pour TOUS les composants :**
```
MonComposant.razor       <!-- Markup Razor UNIQUEMENT -->
MonComposant.razor.cs    <!-- Code C# (logique, méthodes, propriétés) -->
MonComposant.razor.css   <!-- Styles CSS (scoped, DERNIER RECOURS uniquement) -->
```

❌ **INTERDIT - Code dans .razor :**
```razor
@* MAUVAIS EXEMPLE *@
<MudButton OnClick="HandleClick">Cliquer</MudButton>

<style>
  .my-button { color: red; }  <!-- INTERDIT -->
</style>

@code {
  private void HandleClick() { }  <!-- INTERDIT -->
}
```

✅ **OBLIGATOIRE - Séparation stricte :**
```razor
@* MonComposant.razor *@
<MudButton OnClick="HandleClick">Cliquer</MudButton>
```

```csharp
// MonComposant.razor.cs
namespace MyApp.Components;

public partial class MonComposant
{
  private void HandleClick()
  {
    // Logique ici
  }
}
```

### 0.2. Internationalisation OBLIGATOIRE

**JAMAIS de texte en dur - TOUJOURS utiliser IStringLocalizer :**

❌ **INTERDIT :**
```razor
<MudButton>Enregistrer</MudButton>
<MudAlert>Opération réussie</MudAlert>
```

✅ **OBLIGATOIRE :**
```razor
@inject IStringLocalizer<SharedResources> Localizer

<MudButton>@Localizer["Common.Save"]</MudButton>
<MudAlert>@Localizer["Common.OperationSuccess"]</MudAlert>
```

## 1. Hiérarchie de Styling et Classes Composables

### Ordre de Priorité OBLIGATOIRE (du plus global au plus local)

**1. Propriétés natives MudBlazor (PRIORITÉ ABSOLUE #1)**
```razor
<!-- ✅ TOUJOURS utiliser les propriétés natives en premier -->
<MudPaper Elevation="0" Outlined="true">
  <MudStack Spacing="3">
    <MudText Typo="Typo.h6" Color="Color.Primary">Titre</MudText>
    <MudButton Variant="Variant.Filled" Color="Color.Primary">Action</MudButton>
  </MudStack>
</MudPaper>
```

**2. Thème MudBlazor personnalisé (PRIORITÉ #2)**
```csharp
// MainLayout.razor.cs
private readonly MudTheme _theme = new()
{
  PaletteLight = new PaletteLight
  {
    Primary = "#0288d1",
    Background = "#f5f5f5",
    Surface = "#ffffff",
    TextPrimary = "#212121",
    TextSecondary = "#757575"
  }
};
```

**3. Classes CSS globales composables dans wwwroot/app.css (PRIORITÉ #3)**
```css
/* app.css - Classes RÉUTILISABLES pour toute l'application */

/* === Utility Classes === */
.w-100 { width: 100%; }
.text-none { text-transform: none; }
.mt-2 { margin-top: 16px; }
.mt-3 { margin-top: 24px; }

/* === Layout Classes === */
.app-bar-minimal {
  background: white;
  border-bottom: 1px solid #e0e0e0;
}

.drawer-minimal {
  background: #fafafa;
  border-right: 1px solid #e0e0e0;
}

.main-content {
  background: #f5f5f5;
  min-height: 100vh;
  padding: 24px;
}

/* === Navigation Classes === */
.nav-link-minimal {
  color: #616161;
  margin: 2px 8px;
  border-radius: 4px;
  padding: 8px 12px;
}

.nav-link-minimal:hover {
  background-color: #f5f5f5;
}

.active-nav-item {
  background-color: #e3f2fd !important;
  color: #0288d1 !important;
  font-weight: 500;
}

/* === Card Classes === */
.card-stat {
  background: white;
  padding: 20px;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
}

.card-section {
  background: white;
  padding: 24px;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
}

/* === Typography Classes === */
.app-title {
  font-weight: 600;
  color: #212121;
  line-height: 1.2;
}

/* === Button Classes === */
.btn-outlined-minimal {
  border-color: #e0e0e0;
}
```

**4. Classes CSS scoped dans .razor.css (PRIORITÉ #4 - UNIQUEMENT SI SPÉCIFIQUE)**
```css
/* MonComposant.razor.css - SEULEMENT styles uniques à CE composant */
.mon-composant-specifique {
  /* Styles qui n'existent NULLE PART ailleurs */
  background: linear-gradient(45deg, #0288d1, #78909c);
}
```

**5. Attribut Style (PRIORITÉ #5 - INTERDIT SAUF CALCUL DYNAMIQUE)**
```razor
<!-- ❌ INTERDIT : Style statique -->
<MudPaper Style="background: white; padding: 20px;">...</MudPaper>

<!-- ✅ AUTORISÉ : Style dynamique calculé en C# -->
<MudPaper Style="@($"width: {CalculatedWidth}px; height: {CalculatedHeight}px;")">
  ...
</MudPaper>
```

### Principe de Composition des Classes CSS

**SYSTÉMATIQUEMENT créer des classes composables :**

```css
/* wwwroot/app.css */

/* === Atomic Classes (Tokens) === */
.spacing-sm { padding: 8px; }
.spacing-md { padding: 16px; }
.spacing-lg { padding: 24px; }

.border-light { border: 1px solid #e0e0e0; }
.border-radius { border-radius: 4px; }

.bg-surface { background: #ffffff; }
.bg-background { background: #f5f5f5; }

.text-primary { color: #212121; }
.text-secondary { color: #757575; }

/* === Composed Classes (Combinations) === */
.card-base {
  background: var(--bg-surface, #ffffff);
  border: 1px solid #e0e0e0;
  border-radius: 4px;
}

.card-stat {
  /* Compose existing classes */
  composes: card-base;
  padding: 20px;
}

.card-section {
  composes: card-base;
  padding: 24px;
}
```

**Utilisation dans Razor :**
```razor
<!-- ✅ EXCELLENT : Classes composables -->
<MudPaper Elevation="0" Class="card-stat">
  <MudStack Spacing="2">
    <MudText Typo="Typo.h6" Class="app-title">Titre</MudText>
    <MudText Typo="Typo.body2" Color="Color.Secondary">Description</MudText>
  </MudStack>
</MudPaper>

<!-- ✅ BON : Plusieurs classes composées -->
<MudPaper Elevation="0" Class="card-base spacing-lg">
  <!-- Contenu -->
</MudPaper>

<!-- ❌ MAUVAIS : Style inline -->
<MudPaper Elevation="0" Style="background: white; padding: 20px; border: 1px solid #e0e0e0;">
  <!-- Contenu -->
</MudPaper>
```

### ❌ INTERDICTIONS ABSOLUES

**1. Balises HTML Natives - INTERDIT**
```razor
<!-- ❌ INTERDIT : Utilisation de HTML natif -->
<div class="container">
  <div class="d-flex justify-content-between">
    <span>Titre</span>
    <p>Description</p>
  </div>
  <section>
    <header>En-tête</header>
  </section>
</div>
```

**2. Classes Bootstrap/Tailwind - INTERDIT**
```razor
<!-- ❌ INTERDIT : Classes utilitaires externes -->
<MudPaper Class="d-flex pa-4 mb-2 justify-content-between">...</MudPaper>
<MudStack Class="flex flex-col gap-4">...</MudStack>
```

**3. Style Inline Statique - INTERDIT**
```razor
<!-- ❌ INTERDIT : Style inline pour styles statiques -->
<MudPaper Style="background: white; padding: 20px; border: 1px solid #e0e0e0;">
  <MudText Style="color: #212121; font-weight: 600;">Titre</MudText>
</MudPaper>
```

**4. CSS Dupliqué dans Composants - INTERDIT**
```css
/* ❌ INTERDIT : Dupliquer les mêmes styles dans plusieurs .razor.css */

/* ComponentA.razor.css */
.my-card {
  background: white;
  padding: 20px;
  border: 1px solid #e0e0e0;
}

/* ComponentB.razor.css */
.my-other-card {
  background: white;  /* ← Duplication ! */
  padding: 20px;
  border: 1px solid #e0e0e0;
}
```

### ✅ À FAIRE - Composants MudBlazor + Classes Composables

**1. TOUJOURS Utiliser MudBlazor pour Layout**
```razor
<!-- ✅ EXCELLENT : MudStack pour layout -->
<MudStack Row="true" Justify="Justify.SpaceBetween" AlignItems="AlignItems.Center" Spacing="2" Class="page-header">
  <MudText Typo="Typo.h6" Color="Color.Primary" Class="app-title">Titre</MudText>
  <MudButton Variant="Variant.Filled" Color="Color.Primary" Class="text-none">Action</MudButton>
</MudStack>

<!-- ✅ EXCELLENT : MudGrid pour grilles -->
<MudGrid Spacing="3">
  <MudItem xs="12" sm="6" md="4">
    <MudPaper Elevation="0" Class="card-stat">
      <!-- Contenu -->
    </MudPaper>
  </MudItem>
</MudGrid>
```

**2. SYSTÉMATIQUEMENT Utiliser Classes CSS Composables**
```razor
<!-- ✅ EXCELLENT : Classes globales réutilisables -->
<MudPaper Elevation="0" Class="card-stat">
  <MudStack Spacing="2">
    <MudStack Row="true" AlignItems="AlignItems.Center" Justify="Justify.SpaceBetween">
      <MudIcon Icon="@Icons.Material.Filled.Analytics" Color="Color.Primary" Size="Size.Medium"/>
      <MudChip T="string" Size="Size.Small" Color="Color.Info" Class="chip-stat">156</MudChip>
    </MudStack>
    <MudText Typo="Typo.body2" Color="Color.Secondary">Analytics</MudText>
  </MudStack>
</MudPaper>
```

```css
/* wwwroot/app.css - Centralisé et réutilisable */
.card-stat {
  background: white;
  padding: 20px;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
}

.chip-stat {
  background: #e3f2fd;
  color: #0288d1;
}
```

**3. Style Dynamique UNIQUEMENT pour Calculs**
```razor
<!-- ✅ AUTORISÉ : Style dynamique calculé -->
@code {
  private int CalculatedWidth => IsExpanded ? 400 : 200;
  private string BackgroundColor => IsActive ? "#e3f2fd" : "#ffffff";
}

<MudPaper Elevation="0" 
          Class="card-base"
          Style="@($"width: {CalculatedWidth}px; background: {BackgroundColor};")">
  <!-- Contenu -->
</MudPaper>
```

**4. Classes Scoped UNIQUEMENT si Vraiment Spécifique**
```css
/* MonComposant.razor.css - Seulement si unique à CE composant */
.mon-composant-animation-speciale {
  animation: pulse 2s infinite;
  /* Style vraiment unique qui ne sera jamais réutilisé ailleurs */
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.7; }
}
```

## 2. Composants MudBlazor - Design Minimaliste

### AppBar et Navigation

**AppBar minimaliste :**
```razor
<MudAppBar Elevation="0" Dense="true" Style="background: white; border-bottom: 1px solid #e0e0e0;">
  <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="3" Style="width: 100%;">
    <MudButton Href="./" Variant="Variant.Text" Color="Color.Default" Style="text-transform: none;">
      <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="2">
        <MudIcon Icon="@Icons.Material.Filled.FlightTakeoff" Color="Color.Primary" Size="Size.Large"/>
        <MudText Typo="Typo.h6" Style="font-weight: 600; color: #212121;">Application</MudText>
      </MudStack>
    </MudButton>
    <MudSpacer/>
    <MudChip T="string" Icon="@Icons.Material.Filled.Person" Color="Color.Default" Style="color: #616161;">
      @currentUserDisplayName
    </MudChip>
  </MudStack>
</MudAppBar>
```

**Drawer minimaliste :**
```razor
<MudDrawer Open="true" 
           ClipMode="DrawerClipMode.Always" 
           Variant="DrawerVariant.Mini" 
           OpenMiniOnHover="true"
           Elevation="0"
           Style="background: #fafafa; border-right: 1px solid #e0e0e0;">
  <MudStack Spacing="0" Style="padding-top: 16px;">
    <MudStack AlignItems="AlignItems.Center" Spacing="1" Style="padding: 16px 8px; margin-bottom: 8px;">
      <MudIcon Icon="@Icons.Material.Filled.AdminPanelSettings" Size="Size.Large" Color="Color.Primary"/>
      <MudText Typo="Typo.caption" Style="color: #757575; font-weight: 500;">Management</MudText>
    </MudStack>
    <NavMenu/>
  </MudStack>
</MudDrawer>
```

### Cards et Papers

**Card statistique minimaliste :**
```razor
<MudPaper Elevation="0" Style="background: white; padding: 20px; border: 1px solid #e0e0e0; border-radius: 4px;">
  <MudStack Spacing="2">
    <MudStack Row="true" AlignItems="AlignItems.Center" Justify="Justify.SpaceBetween">
      <MudIcon Icon="@Icons.Material.Filled.Collections" Color="Color.Primary" Size="Size.Medium"/>
      <MudChip T="string" Size="Size.Small" Color="Color.Default" Style="background: #e3f2fd; color: #0288d1;">
        156
      </MudChip>
    </MudStack>
    <MudText Typo="Typo.body2" Style="color: #757575; font-weight: 500;">Collections</MudText>
  </MudStack>
</MudPaper>
```

**Section avec titre :**
```razor
<MudPaper Elevation="0" Style="background: white; padding: 20px 24px; margin-bottom: 24px; border-bottom: 1px solid #e0e0e0;">
  <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="2">
    <MudIcon Icon="@Icons.Material.Filled.Dashboard" Color="Color.Primary" Size="Size.Large"/>
    <MudText Typo="Typo.h5" Style="font-weight: 600; color: #212121;">Dashboard</MudText>
  </MudStack>
</MudPaper>
```

### Boutons

**Hiérarchie de boutons :**
```razor
<!-- Bouton principal (action primaire) -->
<MudButton Variant="Variant.Filled" 
           Color="Color.Primary" 
           StartIcon="@Icons.Material.Filled.Save"
           Style="text-transform: none;">
  Enregistrer
</MudButton>

<!-- Bouton secondaire (action secondaire) -->
<MudButton Variant="Variant.Outlined" 
           Color="Color.Default"
           StartIcon="@Icons.Material.Filled.Cancel"
           Style="border-color: #e0e0e0; text-transform: none;">
  Annuler
</MudButton>

<!-- Bouton tertiaire (action mineure) -->
<MudButton Variant="Variant.Text" 
           Color="Color.Default"
           StartIcon="@Icons.Material.Filled.ArrowBack"
           Style="color: #757575; text-transform: none;">
  Retour
</MudButton>
```

### Navigation Links

**NavLink minimaliste :**
```razor
<MudNavLink Href="./dashboard"
            Icon="@Icons.Material.Filled.Dashboard"
            IconColor="Color.Default"
            Style="color: #616161; margin: 2px 8px; border-radius: 4px; padding: 8px 12px;"
            ActiveClass="active-nav-item">
  Dashboard
</MudNavLink>

<style>
  .active-nav-item {
    background-color: #e3f2fd !important;
    color: #0288d1 !important;
    font-weight: 500;
  }
  
  .mud-nav-link:hover {
    background-color: #f5f5f5;
  }
</style>
```

### Tables

**Table minimaliste :**
```razor
<MudTable Items="@items" 
          Elevation="0"
          Style="background: white; border: 1px solid #e0e0e0;">
  <HeaderContent>
    <MudTh Style="color: #757575; font-weight: 600;">Nom</MudTh>
    <MudTh Style="color: #757575; font-weight: 600;">Email</MudTh>
    <MudTh Style="color: #757575; font-weight: 600;">Actions</MudTh>
  </HeaderContent>
  <RowTemplate>
    <MudTd>@context.Name</MudTd>
    <MudTd>@context.Email</MudTd>
    <MudTd>
      <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                     Size="Size.Small" 
                     Color="Color.Default"/>
      <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                     Size="Size.Small" 
                     Color="Color.Default"/>
    </MudTd>
  </RowTemplate>
</MudTable>
```

### Formulaires

**Champs de formulaire minimalistes :**
```razor
<MudTextField @bind-Value="model.Name" 
              Label="@Localizer["User.Name"]"
              Variant="Variant.Outlined"
              Margin="Margin.Dense"
              Style="margin-bottom: 16px;" />

<MudSelect @bind-Value="model.Role" 
           Label="@Localizer["User.Role"]"
           Variant="Variant.Outlined"
           Margin="Margin.Dense"
           Style="margin-bottom: 16px;">
  <MudSelectItem Value="@("Admin")">Admin</MudSelectItem>
  <MudSelectItem Value="@("User")">User</MudSelectItem>
</MudSelect>
```

## 3. Layout et Espacement

### MudStack pour Layouts

**Layout vertical :**
```razor
<MudStack Spacing="3" Style="padding: 24px;">
  <MudText Typo="Typo.h5">Titre</MudText>
  <MudText Typo="Typo.body1">Contenu</MudText>
  <MudButton Color="Color.Primary">Action</MudButton>
</MudStack>
```

**Layout horizontal :**
```razor
<MudStack Row="true" AlignItems="AlignItems.Center" Justify="Justify.SpaceBetween" Spacing="2">
  <MudText Typo="Typo.h6">Titre</MudText>
  <MudButton Color="Color.Primary">Action</MudButton>
</MudStack>
```

### MudGrid pour Grilles Responsives

```razor
<MudGrid Spacing="3">
  <MudItem xs="12" sm="6" md="4">
    <MudPaper Elevation="0" Style="padding: 16px;">Colonne 1</MudPaper>
  </MudItem>
  <MudItem xs="12" sm="6" md="4">
    <MudPaper Elevation="0" Style="padding: 16px;">Colonne 2</MudPaper>
  </MudItem>
  <MudItem xs="12" sm="6" md="4">
    <MudPaper Elevation="0" Style="padding: 16px;">Colonne 3</MudPaper>
  </MudItem>
</MudGrid>
```

### Espacement Cohérent

**Conventions d'espacement :**
- Padding des containers : `padding: 24px`
- Spacing entre éléments : `Spacing="3"` (≈24px)
- Margin entre sections : `margin-bottom: 24px`
- Padding des cards : `padding: 20px`
- Spacing compact : `Spacing="2"` (≈16px)

## 4. Typographie

**Hiérarchie typographique :**
```razor
<!-- Titre de page -->
<MudText Typo="Typo.h5" Style="font-weight: 600; color: #212121;">
  Titre Principal
</MudText>

<!-- Titre de section -->
<MudText Typo="Typo.h6" Style="font-weight: 600; color: #212121;">
  Titre de Section
</MudText>

<!-- Corps de texte -->
<MudText Typo="Typo.body1" Style="color: #212121;">
  Texte principal
</MudText>

<!-- Texte secondaire -->
<MudText Typo="Typo.body2" Style="color: #757575;">
  Texte secondaire
</MudText>

<!-- Caption -->
<MudText Typo="Typo.caption" Style="color: #9e9e9e;">
  Légende ou note
</MudText>
```

## 5. Icônes

**Utiliser uniquement Material Icons :**
```razor
@using static MudBlazor.Icons.Material.Filled

<!-- Icônes courantes -->
<MudIcon Icon="@Dashboard" Color="Color.Primary"/>
<MudIcon Icon="@Person" Color="Color.Default"/>
<MudIcon Icon="@Settings" Color="Color.Default"/>
<MudIcon Icon="@Search" Color="Color.Default"/>
<MudIcon Icon="@Edit" Color="Color.Default"/>
<MudIcon Icon="@Delete" Color="Color.Default"/>
<MudIcon Icon="@Add" Color="Color.Primary"/>
<MudIcon Icon="@Close" Color="Color.Default"/>
<MudIcon Icon="@Check" Color="Color.Success"/>
<MudIcon Icon="@Warning" Color="Color.Warning"/>
<MudIcon Icon="@Error" Color="Color.Error"/>
```

**Tailles d'icônes :**
```razor
<MudIcon Icon="@Dashboard" Size="Size.Small"/>   <!-- 16px -->
<MudIcon Icon="@Dashboard" Size="Size.Medium"/>  <!-- 24px, défaut -->
<MudIcon Icon="@Dashboard" Size="Size.Large"/>   <!-- 32px -->
```

## 6. États et Feedback

### Messages de Statut

```razor
<!-- Succès -->
<MudAlert Severity="Severity.Success" Style="background: #e8f5e9; color: #4caf50;">
  @Localizer["Common.OperationSuccess"]
</MudAlert>

<!-- Information -->
<MudAlert Severity="Severity.Info" Style="background: #e3f2fd; color: #0288d1;">
  @Localizer["Common.Information"]
</MudAlert>

<!-- Avertissement -->
<MudAlert Severity="Severity.Warning" Style="background: #fff3e0; color: #ff9800;">
  @Localizer["Common.Warning"]
</MudAlert>

<!-- Erreur -->
<MudAlert Severity="Severity.Error" Style="background: #ffebee; color: #f44336;">
  @Localizer["Common.Error"]
</MudAlert>
```

### Snackbar

```csharp
// Dans .razor.cs
[Inject] private ISnackbar Snackbar { get; set; }
[Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }

private void ShowSuccess()
{
  Snackbar.Add(Localizer["Common.OperationSuccess"], Severity.Success);
}
```

### États de Chargement

```razor
@if (isLoading)
{
  <MudProgressLinear Color="Color.Primary" Indeterminate="true" Style="margin-bottom: 16px;"/>
}
else if (!items.Any())
{
  <MudPaper Elevation="0" Style="padding: 40px; text-align: center; border: 1px solid #e0e0e0;">
    <MudIcon Icon="@Icons.Material.Filled.Info" Size="Size.Large" Style="color: #9e9e9e; margin-bottom: 16px;"/>
    <MudText Typo="Typo.body1" Style="color: #757575;">
      @Localizer["Common.NoDataAvailable"]
    </MudText>
  </MudPaper>
}
```

## 7. Pages Dashboard

**Structure de page Dashboard minimaliste :**

```razor
@page "/"
@page "/dashboard"

<MudContainer MaxWidth="MaxWidth.False" Style="padding: 0;">
  <!-- En-tête de page -->
  <MudPaper Elevation="0" Style="background: white; padding: 20px 24px; margin-bottom: 24px; border-bottom: 1px solid #e0e0e0;">
    <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="2">
      <MudIcon Icon="@Icons.Material.Filled.Dashboard" Color="Color.Primary" Size="Size.Large"/>
      <MudText Typo="Typo.h5" Style="font-weight: 600; color: #212121;">Dashboard</MudText>
    </MudStack>
  </MudPaper>

  <!-- Statistiques -->
  <MudGrid Spacing="3">
    <MudItem xs="12" sm="6" md="3">
      <MudPaper Elevation="0" Style="background: white; padding: 20px; border: 1px solid #e0e0e0;">
        <MudStack Spacing="2">
          <MudStack Row="true" AlignItems="AlignItems.Center" Justify="Justify.SpaceBetween">
            <MudIcon Icon="@Icons.Material.Filled.Collections" Color="Color.Primary" Size="Size.Medium"/>
            <MudChip T="string" Size="Size.Small" Color="Color.Default" Style="background: #e3f2fd; color: #0288d1;">156</MudChip>
          </MudStack>
          <MudText Typo="Typo.body2" Style="color: #757575; font-weight: 500;">Collections</MudText>
        </MudStack>
      </MudPaper>
    </MudItem>
  </MudGrid>
</MudContainer>
```

## 8. Architecture et Découpage des Composants

### Principe de Responsabilité Unique
- ✅ Éviter les composants "god object" : extraire les groupes MudBlazor cohérents dans des composants enfants
- ✅ Préférer des composants **stateless** pour l'affichage (données via `[Parameter]`)
- ✅ Encapsuler la logique métier dans des services ou composants conteneurs
- ✅ Partager les fragments répétitifs via `RenderFragment` (ex: `MudTable`, `MudGrid`, `MudTimeline`)

### Organisation par Couches
```
Components/
├── Foundation/          # Briques UI génériques
│   ├── EnhancedButton/
│   ├── StatChip/
│   └── StatusBadge/
├── Composition/         # Assemblages d'interactions
│   ├── Toolbar/
│   ├── DynamicForm/
│   └── EnrichedCard/
└── Features/            # Composants métier
    ├── PromptManagement/
    ├── CollectionFilter/
    └── AnalyticsDashboard/
```

**Nommage des composants :**
- ✅ Noms descriptifs basés sur le rôle : `PromptToolbar`, `CollectionFilterChip`
- ❌ JAMAIS utiliser "Atom", "Molecule", "Organism"

### Documentation des Composants Partagés
Chaque composant partagé DOIT avoir un `README.md` documentant :
- Paramètres (`[Parameter]`)
- Slots (`RenderFragment`)
- Dépendances MudBlazor
- Exemples d'utilisation

## 9. State Management et Données

### Gestion des Formulaires
```razor
<!-- ✅ BON : MudForm avec synchronisation explicite -->
<MudForm @ref="form" IsValid="@IsFormValid">
  <MudTextField @bind-Value="model.Name" 
                Label="@Localizer["User.Name"]"
                Required="true" />
</MudForm>
```

```csharp
// Code-behind
[Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }

private MudForm form;
private bool IsFormValid => form?.IsValid ?? false;

private async Task SubmitAsync()
{
  await form.Validate();
  if (!IsFormValid) return;
  // Traitement
}
```

### EventCallback pour Communication
```csharp
// ✅ TOUJOURS utiliser EventCallback au lieu de Action/Func
[Parameter] public EventCallback<string> OnSearchChanged { get; set; }

private async Task HandleSearchAsync(string searchTerm)
{
  // InvokeAsync pour contexte Blazor synchrone
  await OnSearchChanged.InvokeAsync(searchTerm);
}
```

### ViewModels pour États Complexes
```csharp
// Services/ViewModels/PromptListViewModel.cs
public class PromptListViewModel
{
  public string SearchTerm { get; set; }
  public int CurrentPage { get; set; }
  public int PageSize { get; set; } = 20;
  public List<string> SelectedIds { get; set; } = new();
}

// Composant - Injection scoped
[Inject] private PromptListViewModel ViewModel { get; set; }
```

## 10. Performance et Réactivité

### Virtualisation pour Listes Volumineuses
```razor
<!-- ✅ Utiliser MudVirtualize ou ServerData paginé -->
<MudDataGrid T="PromptDto" 
             ServerData="@LoadDataAsync"
             Virtualize="true">
  <!-- Colonnes -->
</MudDataGrid>
```

### Contrôle des Re-renders
```csharp
// Code-behind
protected override bool ShouldRender()
{
  // Rendre seulement si paramètres critiques ont changé
  return _criticalDataChanged;
}
```

### Debounce sur Entrées Utilisateur
```razor
<MudTextField @bind-Value="searchTerm"
              Label="Recherche"
              DebounceInterval="500"
              OnDebounceIntervalElapsed="@HandleSearchAsync" />
```

## 11. Sécurité et Robustesse

### Validation et Sanitation
- ❌ JAMAIS afficher du HTML non maîtrisé via `MarkupString`
- ✅ TOUJOURS valider via DataAnnotations ou FluentValidation
- ✅ Utiliser `InputType` appropriés (`Password`, `Email`, etc.)

### Gestion des Secrets
- ❌ Ne JAMAIS logger d'informations sensibles
- ✅ Chiffrer les secrets côté serveur
- ✅ Manipuler uniquement des substituts (`ReferenceId`) côté composant

### Anti-Forgery et CSRF
```razor
@attribute [ValidateAntiForgeryToken]
```

## 12. Tests et Maintenance

### Tests avec bUnit
```csharp
// Tests/Components/MyComponentTests.cs
public class MyComponentTests : TestContext
{
  [Fact]
  public void Component_ShouldRender_WithCorrectTitle()
  {
    // Arrange
    Services.AddMudServices();
    
    // Act
    var cut = RenderComponent<MyComponent>(parameters => 
      parameters.Add(p => p.Title, "Test Title"));
    
    // Assert
    cut.Find("[data-test='title']").TextContent.Should().Be("Test Title");
  }
}
```

### Data Attributes pour Tests
```razor
<!-- ✅ Ajouter data-test pour ciblage stable -->
<MudButton data-test="submit-button" OnClick="@SubmitAsync">
  @Localizer["Common.Submit"]
</MudButton>
```

## 13. Checklist de Validation Complète

### ✅ Architecture
- [ ] Composants découpés selon responsabilité unique
- [ ] Logique métier isolée dans services
- [ ] Fragments réutilisables (`RenderFragment`)
- [ ] Documentation README.md pour composants partagés
- [ ] Nommage descriptif (pas Atom/Molecule)

### ✅ Design Minimaliste
- [ ] Palette clair/gris/blanc respectée (#0288d1, #f5f5f5, #ffffff, #e0e0e0)
- [ ] Élévations minimales (Elevation="0" ou "1")
- [ ] Bordures grises (#e0e0e0) au lieu d'ombres fortes
- [ ] Espacement généreux (Spacing="3" ≈ 24px)
- [ ] Icônes Material Design uniquement
- [ ] Aucun dégradé coloré
- [ ] Aucune couleur vive multiple

### ✅ Composants MudBlazor
- [ ] MudBlazor UNIQUEMENT (zéro balise HTML native)
- [ ] MudStack pour layouts (pas de `<div>`)
- [ ] MudGrid pour grilles responsive
- [ ] Propriétés natives MudBlazor utilisées en priorité
- [ ] Aucune classe Bootstrap/Tailwind (d-flex, pa-*, gap-*, etc.)

### ✅ Classes CSS Composables
- [ ] Classes réutilisables centralisées dans `wwwroot/app.css`
- [ ] Aucune duplication de styles entre composants
- [ ] Classes scoped `.razor.css` UNIQUEMENT si styles vraiment uniques
- [ ] Aucun style inline sauf calcul dynamique en C#
- [ ] Nomenclature cohérente et sémantique

### ✅ Séparation des Fichiers
- [ ] Aucun `@code{}` dans .razor
- [ ] Aucun `<style>` dans .razor
- [ ] Fichier `.razor.cs` pour tout code C#
- [ ] Fichier `.razor.css` uniquement si nécessaire

### ✅ Internationalisation
- [ ] `IStringLocalizer` injecté dans tous les composants
- [ ] Zéro texte en dur dans `.razor` ou `.razor.cs`
- [ ] Tous messages localisés (labels, erreurs, tooltips, placeholders)
- [ ] Clés de ressources structurées (`Common.Save`, `User.Name`)

### ✅ Accessibilité (a11y)
- [ ] `AriaLabel` sur tous les `MudIconButton`
- [ ] Labels sur tous les champs de formulaire
- [ ] Contrastes suffisants (#212121 sur #ffffff ≥ 4.5:1)
- [ ] Navigation clavier fonctionnelle
- [ ] Focus visible sur éléments interactifs

### ✅ Performance
- [ ] Virtualisation pour listes > 100 items
- [ ] Debounce sur inputs intensifs (500ms)
- [ ] `ShouldRender()` implémenté si nécessaire
- [ ] `@key` sur listes pour stabilité DOM
- [ ] Lazy loading de modules JS optionnels

### ✅ Sécurité
- [ ] Validation DataAnnotations/FluentValidation
- [ ] Aucun `MarkupString` sans sanitation
- [ ] `InputType` appropriés (Password, Email)
- [ ] `[ValidateAntiForgeryToken]` sur pages avec formulaires
- [ ] Aucune information sensible loggée

### ✅ Tests
- [ ] Tests bUnit pour composants critiques
- [ ] `data-test` attributes sur éléments interactifs
- [ ] Scénarios accessibilité vérifiés (Playwright/Axe)
- [ ] Tests de non-régression visuelle si nécessaire

---

**Commande de validation automatique :**

```powershell
# Vérifier les violations de design
Get-ChildItem -Recurse -Filter "*.razor" | Select-String -Pattern "(class=\"d-flex|class=\"pa-|bootstrap|gradient)" | Select-Object Path, LineNumber

# Vérifier les textes en dur
Get-ChildItem -Recurse -Filter "*.razor" | Select-String -Pattern '(>)[A-Za-zÀ-ÿ\s]{3,}(<|@)' | Where-Object { $_.Line -notmatch "@Localizer" } | Select-Object Path, LineNumber

# Vérifier la séparation des fichiers
Get-ChildItem -Recurse -Filter "*.razor" | Select-String -Pattern "(@code|<style)" | Select-Object Path, LineNumber
```

**Si ces commandes retournent des résultats, le code ne respecte PAS les règles.**
