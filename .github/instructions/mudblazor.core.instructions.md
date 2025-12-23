---
description: Règles fondamentales MudBlazor - Composants uniquement, séparation des fichiers, internationalisation obligatoire
name: MudBlazor_Core_Rules
applyTo: "**/backend/Presentation/**/*.razor,**/backend/Presentation/**/*.razor.cs"
---

# MudBlazor - Règles Fondamentales

## ⛔ À NE PAS FAIRE

- **N'écris jamais** de balise HTML native (`<div>`, `<span>`, `<p>`, `<section>`)
- **Ne mélange jamais** markup et code C# dans le même fichier .razor (séparer en .razor.cs)
- **Ne hardcode jamais** de texte affiché - utilise `IStringLocalizer<T>`
- **N'utilise jamais** de CSS inline - utilise les propriétés MudBlazor natives
- **N'écris jamais** de CSS custom si MudBlazor offre une propriété native
- **Ne crée jamais** de layout avec `<div>` - utilise `MudStack` ou `MudGrid`
- **N'ignore jamais** les `[Parameter]` requis des composants MudBlazor

## ✅ À FAIRE

- **Utilise toujours** les composants MudBlazor exclusivement (MudStack, MudPaper, MudText)
- **Sépare toujours** le code dans un fichier `.razor.cs` pour les composants >20 lignes C#
- **Utilise toujours** `IStringLocalizer<T>` pour tout texte affiché (i18n obligatoire)
- **Privilégie toujours** les propriétés natives (`Class`, `Style`, `Elevation`, `Spacing`)
- **Utilise toujours** `MudStack` pour les layouts linéaires, `MudGrid` pour les grilles
- **Crée toujours** un fichier `.razor.css` pour les styles spécifiques au composant
- **Documente toujours** les `[Parameter]` publics avec des commentaires XML

## 🚨 RÈGLES ABSOLUES (NON NÉGOCIABLES)

### 1. Composants MudBlazor UNIQUEMENT

**OBLIGATOIRE :**
- ✅ **TOUJOURS** utiliser les composants MudBlazor (MudStack, MudPaper, MudText, MudButton, etc.)
- ❌ **JAMAIS** de balises HTML natives (`<div>`, `<span>`, `<p>`, `<section>`, `<header>`, etc.)
- ❌ **INTERDICTION ABSOLUE** de créer des `<div>` pour layout - utiliser `MudStack` ou `MudGrid`

**Exemples :**

```razor
<!-- ✅ EXCELLENT : Composants MudBlazor -->
<MudStack Row="true" Justify="Justify.SpaceBetween" AlignItems="AlignItems.Center" Spacing="2">
  <MudText Typo="Typo.h6">Titre</MudText>
  <MudButton Variant="Variant.Filled" Color="Color.Primary">Action</MudButton>
</MudStack>

<!-- ❌ INTERDIT : Balises HTML natives -->
<div class="d-flex justify-content-between align-items-center">
  <span>Titre</span>
  <button>Action</button>
</div>
```

### 2. Séparation Stricte des Fichiers

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

### 3. Internationalisation OBLIGATOIRE

**JAMAIS de texte en dur - TOUJOURS utiliser IStringLocalizer :**

❌ **INTERDIT :**
```razor
<MudButton>Enregistrer</MudButton>
<MudAlert>Opération réussie</MudAlert>
<MudTextField Label="Nom" />
```

✅ **OBLIGATOIRE :**
```razor
@inject IStringLocalizer<SharedResources> Localizer

<MudButton>@Localizer["Common.Save"]</MudButton>
<MudAlert>@Localizer["Common.OperationSuccess"]</MudAlert>
<MudTextField Label="@Localizer["User.Name"]" />
```

**Structure des clés de ressources :**
```
Common.Save
Common.Cancel
Common.Delete
Common.Edit
Common.OperationSuccess
Common.OperationError
Common.NoDataAvailable

User.Name
User.Email
User.Role

Prompt.Title
Prompt.Description
```

### 4. Propriétés Natives MudBlazor en Priorité

**TOUJOURS privilégier les propriétés natives :**

```razor
<!-- ✅ BON : Propriétés natives -->
<MudPaper Elevation="0" Outlined="true">
  <MudStack Spacing="3">
    <MudText Typo="Typo.h6" Color="Color.Primary">Titre</MudText>
    <MudButton Variant="Variant.Filled" Color="Color.Primary" Size="Size.Medium">Action</MudButton>
  </MudStack>
</MudPaper>

<!-- ❌ MAUVAIS : Dupliquer ce que MudBlazor offre -->
<MudPaper Class="elevation-none outlined">
  <div class="spacing-3">
    <h6 class="text-primary">Titre</h6>
    <button class="btn-filled btn-primary btn-medium">Action</button>
  </div>
</MudPaper>
```

**Propriétés natives courantes :**
- `Elevation` : 0-24 (élévation Material Design)
- `Color` : Primary, Secondary, Success, Error, Warning, Info, Dark, Transparent
- `Variant` : Text, Filled, Outlined
- `Spacing` : 0-16 (multiples de 8px)
- `Size` : Small, Medium, Large
- `Dense` : Réduction de la densité
- `Outlined` : Bordure au lieu de remplissage
- `Square` : Coins carrés au lieu d'arrondis

## 🎯 Layout et Structure

### MudStack pour Layouts

**Layout vertical :**
```razor
<MudStack Spacing="3">
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

**Layout mixte :**
```razor
<MudStack Spacing="3">
  <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="2">
    <MudIcon Icon="@Icons.Material.Filled.Dashboard" Color="Color.Primary"/>
    <MudText Typo="Typo.h6">Dashboard</MudText>
  </MudStack>
  
  <MudStack Row="true" Justify="Justify.FlexEnd" Spacing="2">
    <MudButton Variant="Variant.Text">Annuler</MudButton>
    <MudButton Variant="Variant.Filled" Color="Color.Primary">Enregistrer</MudButton>
  </MudStack>
</MudStack>
```

### MudGrid pour Grilles Responsives

```razor
<MudGrid Spacing="3">
  <MudItem xs="12" sm="6" md="4">
    <MudPaper Elevation="0" Class="card-stat">Colonne 1</MudPaper>
  </MudItem>
  <MudItem xs="12" sm="6" md="4">
    <MudPaper Elevation="0" Class="card-stat">Colonne 2</MudPaper>
  </MudItem>
  <MudItem xs="12" sm="6" md="4">
    <MudPaper Elevation="0" Class="card-stat">Colonne 3</MudPaper>
  </MudItem>
</MudGrid>
```

**Breakpoints MudBlazor :**
- `xs` : Extra small (< 600px)
- `sm` : Small (≥ 600px)
- `md` : Medium (≥ 960px)
- `lg` : Large (≥ 1280px)
- `xl` : Extra large (≥ 1920px)

## 📋 Checklist de Validation

### ✅ Composants
- [ ] MudBlazor UNIQUEMENT (zéro balise HTML native)
- [ ] MudStack pour layouts (pas de `<div>`)
- [ ] MudGrid pour grilles responsive
- [ ] Propriétés natives MudBlazor utilisées en priorité
- [ ] Aucune classe Bootstrap/Tailwind (d-flex, pa-*, gap-*, etc.)

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

### ✅ Propriétés Natives
- [ ] `Elevation`, `Color`, `Variant`, `Size` utilisés au lieu de classes CSS
- [ ] `Spacing` utilisé au lieu de classes de margin/padding
- [ ] Aucune duplication de propriétés natives via CSS

## 🔍 Validation Automatique

```powershell
# Vérifier les balises HTML natives
Get-ChildItem -Recurse -Filter "*.razor" | 
  Select-String -Pattern "<(div|span|p|section|header|footer|article|aside|nav|main|h1|h2|h3|h4|h5|h6|ul|ol|li|table|tr|td|th|thead|tbody|form|input|button|a|img)" | 
  Select-Object Path, LineNumber

# Vérifier les @code{} dans .razor
Get-ChildItem -Recurse -Filter "*.razor" | 
  Select-String -Pattern "@code\s*{" | 
  Select-Object Path, LineNumber

# Vérifier les <style> dans .razor
Get-ChildItem -Recurse -Filter "*.razor" | 
  Select-String -Pattern "<style" | 
  Select-Object Path, LineNumber

# Vérifier les textes en dur (heuristique)
Get-ChildItem -Recurse -Filter "*.razor" | 
  Select-String -Pattern '(>)[A-Za-zÀ-ÿ\s]{4,}(<|@)' | 
  Where-Object { $_.Line -notmatch "@Localizer" -and $_.Line -notmatch "@@" } | 
  Select-Object Path, LineNumber

# Vérifier les classes Bootstrap/Tailwind
Get-ChildItem -Recurse -Filter "*.razor" | 
  Select-String -Pattern 'Class="[^"]*\b(d-flex|flex|pa-\d|p-\d|ma-\d|m-\d|gap-\d|col-\d|row|container)' | 
  Select-Object Path, LineNumber
```

**Si ces commandes retournent des résultats, le code ne respecte PAS les règles fondamentales.**

## 📚 Ressources

### Documentation Officielle
- [MudBlazor Components](https://mudblazor.com/components)
- [MudBlazor API Reference](https://mudblazor.com/api)
- [Blazor Component Basics](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/)
- [Localization in Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization)
