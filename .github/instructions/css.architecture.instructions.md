---
description: Architecture CSS - Classes composables, hiérarchie de styling et conventions de nommage
name: CSS_Architecture
applyTo: "**/*.{css,razor.css}"
---

# Instructions CSS - Architecture et Composabilité

## ⚠️ RÈGLES MANDATORY

### 🚫 INTERDICTIONS ABSOLUES

- ❌ **INTERDICTION** d'utiliser l'attribut `Style=""` pour des valeurs statiques
- ❌ **INTERDICTION** de dupliquer des styles entre plusieurs fichiers `.razor.css`
- ❌ **INTERDICTION** d'utiliser des classes Bootstrap (`d-flex`, `pa-4`, `mb-2`, `col-*`)
- ❌ **INTERDICTION** d'utiliser des classes Tailwind (`flex`, `p-4`, `mb-2`, `grid`)
- ❌ **INTERDICTION** de créer des classes spécifiques à un composant dans `wwwroot/app.css`
- ❌ **INTERDICTION** d'utiliser `!important` (sauf cas exceptionnel documenté)
- ❌ **INTERDICTION** de styles inline dans HTML/Razor (sauf calculs dynamiques C#)

### ✅ ACTIONS OBLIGATOIRES

1. **CENTRALISER** tous les styles réutilisables dans `wwwroot/app.css`
2. **COMPOSER** les classes CSS pour créer des styles réutilisables
3. **UTILISER** des classes scoped `.razor.css` UNIQUEMENT pour styles vraiment uniques au composant
4. **NOMMER** les classes selon une convention sémantique cohérente
5. **DOCUMENTER** les classes globales dans `wwwroot/app.css` avec commentaires
6. **VALIDER** la duplication avant de créer une nouvelle classe

## 📐 Hiérarchie de Styling (5 Niveaux)

### Niveau 1 : Propriétés Natives MudBlazor (PRIORITÉ ABSOLUE)

**✅ TOUJOURS utiliser en premier :**
```razor
<MudPaper Elevation="1" 
          Class="rounded pa-4">
  <!-- Elevation, Class, Variant sont des propriétés natives -->
</MudPaper>

<MudButton Variant="Variant.Filled" 
           Color="Color.Primary">
  <!-- Variant et Color sont natifs -->
</MudButton>

<MudStack Spacing="3" Row="true">
  <!-- Spacing est natif (1 unité ≈ 8px) -->
</MudStack>
```

**Propriétés natives courantes :**
- `Elevation` : 0-24 (élévation Material Design)
- `Color` : Primary, Secondary, Success, Error, Warning, Info, Dark, Transparent
- `Variant` : Text, Filled, Outlined
- `Spacing` : 0-16 (multiples de 8px)
- `Class` : Classes CSS custom
- `Dense` : Réduction de la densité
- `Square` : Coins carrés au lieu d'arrondis

### Niveau 2 : Customisation du MudTheme

**✅ Pour les styles globaux de l'application :**
```csharp
// Program.cs ou Startup
services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
});

// Theme customization
var theme = new MudTheme()
{
    Palette = new PaletteLight()
    {
        Primary = "#0288d1",
        Secondary = "#757575",
        Background = "#f5f5f5",
        Surface = "#ffffff",
        AppbarBackground = "#0288d1",
        DrawerBackground = "#ffffff"
    },
    Typography = new Typography()
    {
        Default = new Default()
        {
            FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" }
        }
    }
};
```

### Niveau 3 : Classes CSS Globales Composables (RECOMMANDÉ)

**✅ Centraliser dans `wwwroot/app.css` :**

#### Classes Atomiques (Tokens)
```css
/* === SPACING TOKENS === */
.spacing-xs { padding: 4px; }
.spacing-sm { padding: 8px; }
.spacing-md { padding: 16px; }
.spacing-lg { padding: 24px; }
.spacing-xl { padding: 32px; }

.margin-xs { margin: 4px; }
.margin-sm { margin: 8px; }
.margin-md { margin: 16px; }
.margin-lg { margin: 24px; }
.margin-xl { margin: 32px; }

/* === BORDER TOKENS === */
.border-light { border: 1px solid #e0e0e0; }
.border-medium { border: 1px solid #bdbdbd; }
.border-dark { border: 1px solid #757575; }
.border-radius-sm { border-radius: 4px; }
.border-radius-md { border-radius: 8px; }
.border-radius-lg { border-radius: 12px; }

/* === COLOR TOKENS === */
.bg-surface { background-color: #ffffff; }
.bg-background { background-color: #f5f5f5; }
.bg-primary { background-color: #0288d1; }
.bg-primary-light { background-color: #e3f2fd; }
.text-primary { color: #0288d1; }
.text-secondary { color: #757575; }
.text-dark { color: #212121; }

/* === LAYOUT TOKENS === */
.flex-center { 
    display: flex; 
    align-items: center; 
    justify-content: center; 
}
.flex-between { 
    display: flex; 
    align-items: center; 
    justify-content: space-between; 
}
.gap-xs { gap: 4px; }
.gap-sm { gap: 8px; }
.gap-md { gap: 16px; }
.gap-lg { gap: 24px; }
```

#### Classes Composées (Composition)
```css
/* === BASE COMPONENTS === */
.card-base {
    background-color: #ffffff;
    border: 1px solid #e0e0e0;
    border-radius: 4px;
}

.card-elevated {
    composes: card-base;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.card-stat {
    composes: card-base;
    padding: 20px;
    transition: box-shadow 0.2s ease;
}

.card-stat:hover {
    box-shadow: 0 4px 8px rgba(0,0,0,0.15);
}

/* === LAYOUT PATTERNS === */
.page-container {
    padding: 24px;
    background-color: #f5f5f5;
    min-height: calc(100vh - 64px);
}

.section-header {
    padding: 20px 24px;
    background-color: #ffffff;
    border-bottom: 1px solid #e0e0e0;
}

.content-wrapper {
    max-width: 1200px;
    margin: 0 auto;
    padding: 24px;
}

/* === UTILITY PATTERNS === */
.toolbar-actions {
    display: flex;
    gap: 12px;
    align-items: center;
}

.status-badge {
    padding: 4px 12px;
    border-radius: 12px;
    font-size: 0.875rem;
    font-weight: 500;
}

.truncate-text {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.truncate-multiline {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
}
```

#### Utilisation dans Razor
```razor
<!-- ✅ BON : Composition de classes globales -->
<MudPaper Class="card-stat bg-surface">
    <MudStack Spacing="2">
        <MudText Typo="Typo.h6" Class="text-primary">Titre</MudText>
        <MudText Typo="Typo.body2" Class="text-secondary truncate-text">
            Description
        </MudText>
    </MudStack>
</MudPaper>

<!-- ✅ BON : Composition de classes atomiques -->
<MudStack Class="spacing-lg gap-md">
    <MudButton Class="bg-primary">Action</MudButton>
</MudStack>

<!-- ❌ MAUVAIS : Style inline statique -->
<MudPaper Style="background: white; padding: 20px; border: 1px solid #e0e0e0;">
    <!-- INTERDIT -->
</MudPaper>
```

### Niveau 4 : Classes Scoped `.razor.css` (UNIQUEMENT SI NÉCESSAIRE)

**✅ Utiliser UNIQUEMENT pour styles vraiment spécifiques :**

```css
/* Components/Dashboard/AnalyticsCard.razor.css */

/* ✅ BON : Style unique à ce composant */
.analytics-card-sparkline {
    height: 60px;
    margin-top: 12px;
}

.analytics-card-trend-indicator {
    position: absolute;
    top: 8px;
    right: 8px;
    font-size: 0.75rem;
}

/* ❌ MAUVAIS : Style réutilisable ailleurs */
.card-stat {
    /* DEVRAIT être dans app.css */
    padding: 20px;
    background: white;
}
```

**Questions à se poser AVANT de créer une classe scoped :**
1. Ce style pourrait-il être réutilisé ailleurs ? → Si OUI, le mettre dans `app.css`
2. Ce style peut-il être composé de classes atomiques existantes ? → Si OUI, utiliser la composition
3. Ce style est-il vraiment unique à ce composant ? → Si NON, le généraliser
4. Ce style peut-il être géré par une propriété MudBlazor ? → Si OUI, utiliser la propriété native

### Niveau 5 : Attribut `Style=""` (DERNIER RECOURS)

**✅ AUTORISÉ UNIQUEMENT pour valeurs dynamiques calculées en C# :**

```razor
@code {
    private int CalculatedWidth { get; set; } = 250;
    private string DynamicColor { get; set; } = "#ff0000";
    private double Progress { get; set; } = 0.75;
}

<!-- ✅ BON : Valeur dynamique calculée -->
<MudPaper Style="@($"width: {CalculatedWidth}px;")">
    <!-- Largeur calculée dynamiquement -->
</MudPaper>

<div Style="@($"background-color: {DynamicColor};")">
    <!-- Couleur déterminée par logique métier -->
</div>

<div Style="@($"transform: scaleX({Progress});")">
    <!-- Animation basée sur progression -->
</div>

<!-- ❌ MAUVAIS : Valeur statique -->
<MudPaper Style="width: 250px;">
    <!-- DEVRAIT être une classe CSS -->
</MudPaper>

<div Style="background-color: #ff0000;">
    <!-- DEVRAIT être une classe .bg-error -->
</div>
```

## 🎨 Principe de Composition CSS

### Concept de Composition
La composition permet de créer des styles complexes en combinant des classes simples et réutilisables.

#### Approche 1 : `composes` (CSS Modules)
```css
/* wwwroot/app.css */

/* Classes de base */
.card-base {
    background-color: #ffffff;
    border: 1px solid #e0e0e0;
    border-radius: 4px;
}

.padding-default {
    padding: 20px;
}

.shadow-light {
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

/* Composition avec 'composes' */
.card-elevated {
    composes: card-base;
    composes: padding-default;
    composes: shadow-light;
}

.card-interactive {
    composes: card-elevated;
    transition: all 0.2s ease;
    cursor: pointer;
}

.card-interactive:hover {
    box-shadow: 0 4px 12px rgba(0,0,0,0.2);
    transform: translateY(-2px);
}
```

#### Approche 2 : Classes Multiples (Préféré pour Blazor)
```razor
<!-- ✅ RECOMMANDÉ : Composition via Class multi-classes -->
<MudPaper Class="card-base padding-default shadow-light">
    <!-- 3 classes atomiques combinées -->
</MudPaper>

<MudStack Class="spacing-lg gap-md flex-center">
    <!-- Composition de layout utilities -->
</MudStack>

<!-- ✅ BON : Classe composée + modificateur -->
<MudPaper Class="card-stat @(IsHighlighted ? "highlighted" : "")">
    <!-- Classe de base + modificateur conditionnel -->
</MudPaper>
```

#### Approche 3 : Variables CSS (Design Tokens)
```css
/* wwwroot/app.css */

:root {
    /* Spacing tokens */
    --spacing-xs: 4px;
    --spacing-sm: 8px;
    --spacing-md: 16px;
    --spacing-lg: 24px;
    --spacing-xl: 32px;

    /* Color tokens */
    --color-primary: #0288d1;
    --color-surface: #ffffff;
    --color-background: #f5f5f5;
    --color-border: #e0e0e0;

    /* Border tokens */
    --border-radius-sm: 4px;
    --border-radius-md: 8px;

    /* Shadow tokens */
    --shadow-sm: 0 1px 3px rgba(0,0,0,0.12);
    --shadow-md: 0 2px 4px rgba(0,0,0,0.1);
}

/* Classes utilisant les tokens */
.card-base {
    background-color: var(--color-surface);
    border: 1px solid var(--color-border);
    border-radius: var(--border-radius-sm);
    box-shadow: var(--shadow-sm);
}

.section-spacing {
    padding: var(--spacing-lg);
    gap: var(--spacing-md);
}
```

```razor
<!-- ✅ Utilisation cohérente des tokens -->
<MudPaper Class="card-base section-spacing">
    <!-- Tous les tokens réutilisés -->
</MudPaper>
```

## 📋 Convention de Nommage

### Structure de Nommage BEM Simplifié

**Format : `{composant}[-{élément}][--{modificateur}]`**

#### Exemples de Nommage
```css
/* === COMPOSANTS === */
.card { }
.card-header { }
.card-content { }
.card-footer { }

.card--elevated { }
.card--interactive { }
.card--compact { }

/* === LAYOUT === */
.page-container { }
.section-header { }
.content-wrapper { }

.toolbar { }
.toolbar-actions { }
.toolbar-title { }

/* === ÉTATS === */
.is-loading { }
.is-active { }
.is-disabled { }
.is-hidden { }

/* === UTILITAIRES === */
.flex-center { }
.flex-between { }
.truncate-text { }
.truncate-multiline { }

.bg-surface { }
.bg-background { }
.text-primary { }
.text-secondary { }

.spacing-sm { }
.spacing-md { }
.gap-sm { }
.gap-md { }

.border-light { }
.border-radius-sm { }
.shadow-light { }
```

### Règles de Nommage

#### ✅ À FAIRE
- **Noms descriptifs et sémantiques** : `.card-stat`, `.section-header`, `.toolbar-actions`
- **Préfixes cohérents** : `.bg-*`, `.text-*`, `.spacing-*`, `.gap-*`, `.border-*`
- **Modificateurs avec double tiret** : `.card--elevated`, `.button--large`
- **États avec préfixe `is-`** : `.is-active`, `.is-loading`, `.is-disabled`
- **Tokens en kebab-case** : `--spacing-md`, `--color-primary`, `--shadow-light`

#### ❌ À ÉVITER
- **Noms abrégés cryptiques** : `.cd`, `.hdr`, `.btn-lg` (sauf conventions standards)
- **Noms basés sur l'apparence** : `.red-box`, `.big-text` (préférer `.alert-error`, `.heading-large`)
- **Noms trop longs** : `.card-with-shadow-and-border-radius` (décomposer)
- **Noms basés sur la position** : `.left-sidebar`, `.top-menu` (préférer `.sidebar`, `.main-menu`)

## 🔧 Organisation des Fichiers CSS

### Structure Recommandée

```
wwwroot/
├── css/
│   ├── app.css                    # Point d'entrée principal
│   ├── tokens.css                 # Variables CSS (design tokens)
│   ├── reset.css                  # Normalization (optionnel)
│   ├── utilities.css              # Classes utilitaires
│   ├── components/
│   │   ├── cards.css              # Styles de cartes
│   │   ├── buttons.css            # Boutons custom
│   │   ├── forms.css              # Formulaires
│   │   └── layouts.css            # Layouts globaux
│   └── themes/
│       ├── light.css              # Thème clair
│       └── dark.css               # Thème sombre (si applicable)
```

### app.css - Point d'Entrée

```css
/* wwwroot/css/app.css */

/* === IMPORTS === */
@import 'tokens.css';
@import 'utilities.css';
@import 'components/cards.css';
@import 'components/buttons.css';
@import 'components/forms.css';
@import 'components/layouts.css';

/* === GLOBAL STYLES === */
html, body {
    font-family: 'Roboto', 'Helvetica', 'Arial', sans-serif;
    font-size: 16px;
    line-height: 1.5;
    color: #212121;
    background-color: #f5f5f5;
}

* {
    box-sizing: border-box;
}

/* === MUDBLAZOR OVERRIDES === */
.mud-paper {
    /* Customisations globales MudBlazor si nécessaire */
}

/* === UTILITY CLASSES === */
/* (Voir section Classes Atomiques ci-dessus) */
```

### tokens.css - Variables Globales

```css
/* wwwroot/css/tokens.css */

:root {
    /* === COLORS === */
    --color-primary: #0288d1;
    --color-primary-light: #e3f2fd;
    --color-primary-dark: #01579b;
    
    --color-secondary: #757575;
    --color-secondary-light: #e0e0e0;
    
    --color-success: #4caf50;
    --color-warning: #ff9800;
    --color-error: #f44336;
    --color-info: #2196f3;
    
    --color-surface: #ffffff;
    --color-background: #f5f5f5;
    --color-border: #e0e0e0;
    
    --color-text-primary: #212121;
    --color-text-secondary: #757575;
    --color-text-disabled: #bdbdbd;
    
    /* === SPACING === */
    --spacing-xs: 4px;
    --spacing-sm: 8px;
    --spacing-md: 16px;
    --spacing-lg: 24px;
    --spacing-xl: 32px;
    --spacing-xxl: 48px;
    
    /* === BORDERS === */
    --border-width: 1px;
    --border-color: #e0e0e0;
    --border-radius-sm: 4px;
    --border-radius-md: 8px;
    --border-radius-lg: 12px;
    --border-radius-xl: 16px;
    --border-radius-full: 9999px;
    
    /* === SHADOWS === */
    --shadow-xs: 0 1px 2px rgba(0,0,0,0.05);
    --shadow-sm: 0 1px 3px rgba(0,0,0,0.12);
    --shadow-md: 0 2px 4px rgba(0,0,0,0.1);
    --shadow-lg: 0 4px 8px rgba(0,0,0,0.15);
    --shadow-xl: 0 8px 16px rgba(0,0,0,0.2);
    
    /* === TRANSITIONS === */
    --transition-fast: 150ms ease;
    --transition-base: 200ms ease;
    --transition-slow: 300ms ease;
    
    /* === Z-INDEX === */
    --z-dropdown: 1000;
    --z-sticky: 1020;
    --z-fixed: 1030;
    --z-modal-backdrop: 1040;
    --z-modal: 1050;
    --z-popover: 1060;
    --z-tooltip: 1070;
}
```

### utilities.css - Classes Utilitaires

```css
/* wwwroot/css/utilities.css */

/* === SPACING UTILITIES === */
.p-xs { padding: var(--spacing-xs); }
.p-sm { padding: var(--spacing-sm); }
.p-md { padding: var(--spacing-md); }
.p-lg { padding: var(--spacing-lg); }
.p-xl { padding: var(--spacing-xl); }

.m-xs { margin: var(--spacing-xs); }
.m-sm { margin: var(--spacing-sm); }
.m-md { margin: var(--spacing-md); }
.m-lg { margin: var(--spacing-lg); }
.m-xl { margin: var(--spacing-xl); }

/* === LAYOUT UTILITIES === */
.flex { display: flex; }
.flex-center { display: flex; align-items: center; justify-content: center; }
.flex-between { display: flex; align-items: center; justify-content: space-between; }
.flex-column { flex-direction: column; }

.gap-xs { gap: var(--spacing-xs); }
.gap-sm { gap: var(--spacing-sm); }
.gap-md { gap: var(--spacing-md); }
.gap-lg { gap: var(--spacing-lg); }

/* === TEXT UTILITIES === */
.text-primary { color: var(--color-text-primary); }
.text-secondary { color: var(--color-text-secondary); }
.text-disabled { color: var(--color-text-disabled); }

.truncate { 
    overflow: hidden; 
    text-overflow: ellipsis; 
    white-space: nowrap; 
}

.truncate-2 {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

/* === BACKGROUND UTILITIES === */
.bg-surface { background-color: var(--color-surface); }
.bg-background { background-color: var(--color-background); }
.bg-primary { background-color: var(--color-primary); }
.bg-primary-light { background-color: var(--color-primary-light); }
```

## ✅ Checklist de Validation CSS

### ✅ Architecture
- [ ] Tous les styles réutilisables sont dans `wwwroot/app.css` ou sous-modules
- [ ] Variables CSS (tokens) définies dans `:root` ou `tokens.css`
- [ ] Classes scoped `.razor.css` UNIQUEMENT si style vraiment unique
- [ ] Aucune duplication de styles entre fichiers
- [ ] Organisation claire en sections commentées

### ✅ Composition
- [ ] Classes atomiques définies (spacing, colors, borders)
- [ ] Classes composées créées avec `composes` ou multi-classes
- [ ] Utilisation de variables CSS pour cohérence
- [ ] Modificateurs avec double tiret (`--`) pour variantes
- [ ] États avec préfixe `is-` pour conditions

### ✅ Nommage
- [ ] Noms descriptifs et sémantiques
- [ ] Convention BEM simplifiée respectée
- [ ] Préfixes cohérents (`bg-*`, `text-*`, `spacing-*`)
- [ ] Pas de noms basés sur l'apparence visuelle
- [ ] Variables CSS en kebab-case (`--spacing-md`)

### ✅ Usage
- [ ] Aucun `Style=""` avec valeur statique dans Razor
- [ ] `Style=""` utilisé UNIQUEMENT pour valeurs dynamiques C#
- [ ] Aucune classe Bootstrap/Tailwind
- [ ] Propriétés natives MudBlazor utilisées en priorité
- [ ] Classes composées via `Class="class1 class2 class3"`

### ✅ Performance
- [ ] CSS minifié en production
- [ ] Sélecteurs simples et performants (éviter descendants profonds)
- [ ] Aucun `!important` (sauf cas exceptionnel documenté)
- [ ] Transitions/animations optimisées (transform, opacity)
- [ ] Images/fonts optimisées et chargées efficacement

### ✅ Accessibilité
- [ ] Contrastes de couleurs suffisants (≥ 4.5:1 pour texte)
- [ ] Focus visible sur éléments interactifs
- [ ] Pas de contenu masqué uniquement en CSS (utiliser `aria-hidden`)
- [ ] Tailles de touch targets ≥ 44x44px sur mobile
- [ ] Respect des préférences utilisateur (`prefers-reduced-motion`)

### ✅ Responsive
- [ ] Design mobile-first avec media queries
- [ ] Breakpoints cohérents (sm: 600px, md: 960px, lg: 1280px, xl: 1920px)
- [ ] Unités flexibles (rem, %, vw/vh) plutôt que px fixes
- [ ] Typographie responsive (clamp, calc, fluid typography)
- [ ] Images/contenus adaptés selon viewport

## 📊 Exemples Complets

### Exemple 1 : Page Dashboard

#### wwwroot/css/components/cards.css
```css
/* === BASE CARD === */
.card-base {
    background-color: var(--color-surface);
    border: var(--border-width) solid var(--color-border);
    border-radius: var(--border-radius-sm);
    transition: box-shadow var(--transition-base);
}

.card-stat {
    composes: card-base;
    padding: var(--spacing-lg);
}

.card-stat:hover {
    box-shadow: var(--shadow-md);
}

.card-stat-value {
    font-size: 2rem;
    font-weight: 700;
    color: var(--color-primary);
    line-height: 1.2;
}

.card-stat-label {
    font-size: 0.875rem;
    color: var(--color-text-secondary);
    margin-top: var(--spacing-sm);
}
```

#### Components/Dashboard/StatsCard.razor
```razor
<MudPaper Class="card-stat" Elevation="0">
    <MudStack Spacing="2">
        <div Class="card-stat-value">@Value</div>
        <div Class="card-stat-label">@Label</div>
    </MudStack>
</MudPaper>
```

#### Components/Dashboard/StatsCard.razor.cs
```csharp
public partial class StatsCard
{
    [Parameter] public string Value { get; set; } = "0";
    [Parameter] public string Label { get; set; } = "";
}
```

### Exemple 2 : Formulaire avec Validation

#### wwwroot/css/components/forms.css
```css
.form-section {
    background-color: var(--color-surface);
    padding: var(--spacing-lg);
    border-radius: var(--border-radius-sm);
    margin-bottom: var(--spacing-md);
}

.form-section-title {
    font-size: 1.125rem;
    font-weight: 600;
    color: var(--color-text-primary);
    margin-bottom: var(--spacing-md);
    padding-bottom: var(--spacing-sm);
    border-bottom: var(--border-width) solid var(--color-border);
}

.form-actions {
    display: flex;
    gap: var(--spacing-md);
    justify-content: flex-end;
    padding-top: var(--spacing-lg);
    border-top: var(--border-width) solid var(--color-border);
}
```

#### Components/Forms/UserForm.razor
```razor
<MudForm @ref="form" Class="form-section">
    <div Class="form-section-title">
        @Localizer["User.Information"]
    </div>
    
    <MudStack Spacing="3">
        <MudTextField @bind-Value="model.Name"
                      Label="@Localizer["User.Name"]"
                      Required="true" />
        
        <MudTextField @bind-Value="model.Email"
                      Label="@Localizer["User.Email"]"
                      Required="true"
                      InputType="InputType.Email" />
    </MudStack>
    
    <div Class="form-actions">
        <MudButton OnClick="@Cancel"
                   Variant="Variant.Text">
            @Localizer["Common.Cancel"]
        </MudButton>
        
        <MudButton OnClick="@SubmitAsync"
                   Variant="Variant.Filled"
                   Color="Color.Primary">
            @Localizer["Common.Save"]
        </MudButton>
    </div>
</MudForm>
```

### Exemple 3 : Style Dynamique

#### Components/Charts/ProgressBar.razor
```razor
@code {
    [Parameter] public double Progress { get; set; } = 0.0; // 0.0 à 1.0
    [Parameter] public string Color { get; set; } = "#0288d1";
    
    private string ProgressBarStyle => $"width: {Progress * 100}%; background-color: {Color};";
}

<MudPaper Class="progress-container" Elevation="0">
    <div Class="progress-bar" Style="@ProgressBarStyle"></div>
</MudPaper>
```

#### Components/Charts/ProgressBar.razor.css
```css
/* Styles spécifiques à ProgressBar */
.progress-container {
    width: 100%;
    height: 8px;
    background-color: #e0e0e0;
    border-radius: 4px;
    overflow: hidden;
}

.progress-bar {
    height: 100%;
    transition: width 0.3s ease, background-color 0.3s ease;
    border-radius: 4px;
}
```

## 🎓 Migration Guide

### Étape 1 : Identifier les Violations

```powershell
# Trouver les Style="" statiques dans Razor
Get-ChildItem -Recurse -Filter "*.razor" | 
    Select-String -Pattern 'Style="[^@]' | 
    Select-Object Path, LineNumber

# Trouver les classes Bootstrap
Get-ChildItem -Recurse -Filter "*.razor" | 
    Select-String -Pattern 'Class="[^"]*\b(d-flex|pa-\d|mb-\d)' | 
    Select-Object Path, LineNumber

# Trouver les styles dupliqués entre .razor.css
Get-ChildItem -Recurse -Filter "*.razor.css" | 
    ForEach-Object { Get-Content $_.FullName }
```

### Étape 2 : Créer Classes Globales

**Avant (Violation) :**
```razor
<MudPaper Style="background: white; padding: 20px; border: 1px solid #e0e0e0;">
    <div style="display: flex; gap: 16px;">
        <span class="d-flex pa-4">Contenu</span>
    </div>
</MudPaper>
```

**Après (Conforme) :**

```css
/* wwwroot/app.css */
.card-content {
    background-color: var(--color-surface);
    padding: var(--spacing-lg);
    border: var(--border-width) solid var(--color-border);
}

.flex-gap-md {
    display: flex;
    gap: var(--spacing-md);
}

.content-box {
    padding: var(--spacing-md);
}
```

```razor
<MudPaper Class="card-content" Elevation="0">
    <div Class="flex-gap-md">
        <MudStack Class="content-box">Contenu</MudStack>
    </div>
</MudPaper>
```

### Étape 3 : Extraire Styles Communs

**Avant (Duplication) :**
```css
/* ComponentA.razor.css */
.card {
    background: white;
    padding: 20px;
    border: 1px solid #e0e0e0;
}

/* ComponentB.razor.css */
.container {
    background: white;
    padding: 20px;
    border: 1px solid #e0e0e0;
}
```

**Après (Mutualisé) :**
```css
/* wwwroot/app.css */
.card-base {
    background-color: var(--color-surface);
    padding: var(--spacing-lg);
    border: var(--border-width) solid var(--color-border);
}
```

```razor
<!-- ComponentA.razor et ComponentB.razor -->
<MudPaper Class="card-base" Elevation="0">
    <!-- Contenu -->
</MudPaper>
```

## 🔍 Debugging et Outils

### DevTools - Inspection CSS

```css
/* Ajouter en développement pour debug */
* {
    outline: 1px solid rgba(255,0,0,0.1);
}

/* Identifier les éléments avec Style inline */
[style] {
    outline: 2px solid red !important;
}
```

### Validation Automatique

```javascript
// Script de validation (intégrer en CI/CD si nécessaire)
const razorFiles = getAllRazorFiles();
const violations = [];

razorFiles.forEach(file => {
    const content = readFile(file);
    
    // Détecter Style="" statiques
    if (/Style="[^@]/.test(content)) {
        violations.push({ file, issue: 'Static Style attribute found' });
    }
    
    // Détecter classes Bootstrap
    if (/class="[^"]*\b(d-flex|pa-\d|mb-\d)/.test(content)) {
        violations.push({ file, issue: 'Bootstrap classes found' });
    }
});

if (violations.length > 0) {
    console.error('CSS violations detected:', violations);
    process.exit(1);
}
```

## 📚 Ressources

### Documentation Officielle
- [MudBlazor Styles & Colors](https://mudblazor.com/features/colors)
- [CSS Variables (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties)
- [BEM Methodology](http://getbem.com/)
- [WCAG 2.1 (Accessibilité)](https://www.w3.org/WAI/WCAG21/quickref/)

### Outils
- [Contrast Checker](https://webaim.org/resources/contrastchecker/)
- [CSS Validator](https://jigsaw.w3.org/css-validator/)
- [PurgeCSS](https://purgecss.com/) - Éliminer CSS non utilisé

### Conventions
- Variables CSS : `--{category}-{property}-{variant}`
- Classes utilitaires : `{property}-{size}`
- Composants : `{component}[-{element}][--{modifier}]`
- États : `is-{state}`
