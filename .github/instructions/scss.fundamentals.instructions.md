---
description: SCSS Fundamentals - Variables, mixins, functions, nesting, imports, best practices
name: SCSS_Fundamentals
applyTo: "**/*.scss,**/*.scss"
---

# SCSS - Règles Fondamentales

Guide complet pour le développement SCSS/Sass.

## � Types de Fichiers à Créer

| Type de fichier | Usage | Nomenclature |
|----------------|-------|-------------|
| `abstracts/_variables.scss` | Variables SCSS globales | Préfixe `_` pour partials (couleurs, espacements, breakpoints) |
| `abstracts/_mixins.scss` | Mixins réutilisables | Préfixe `_` pour partials (responsive, animations, utilitaires) |
| `abstracts/_functions.scss` | Fonctions SCSS custom | Préfixe `_` pour partials (calculs, conversions, helpers) |
| `abstracts/_placeholders.scss` | Placeholders `%` pour @extend | Préfixe `_` pour partials (styles sans paramètres) |
| `base/_reset.scss` | Reset/Normalize CSS | Préfixe `_` pour partials |
| `base/_typography.scss` | Règles typographiques | Préfixe `_` pour partials |
| `components/_[component].scss` | Styles de composants | `_buttons.scss`, `_cards.scss`, `_forms.scss` |
| `layout/_[layout].scss` | Styles de layout | `_grid.scss`, `_header.scss`, `_footer.scss` |
| `main.scss` | Point d'entrée SCSS | Fichier principal qui importe tous les partials |

## ⛔ À NE PAS FAIRE

- **N'imbrique jamais** plus de 3 niveaux de profondeur
- **N'utilise jamais** de sélecteurs ID (#id) pour le styling
- **Ne duplique jamais** de valeurs - utilise des variables
- **N'écris jamais** de couleurs en dur - utilise des variables
- **Ne crée jamais** de mixins sans paramètres (utilise des placeholders)
- **N'importe jamais** de fichiers SCSS sans underscore prefix
- **N'utilise jamais** `@import` (déprécié) - utilise `@use` et `@forward`

## ✅ À FAIRE

- **Utilise toujours** des variables pour les couleurs, espacements, breakpoints
- **Préfère toujours** `@use` et `@forward` à `@import`
- **Organise toujours** les fichiers avec le pattern 7-1
- **Utilise toujours** des mixins pour le code réutilisable avec paramètres
- **Utilise toujours** des placeholders `%` pour les styles sans paramètres
- **Nomme toujours** les fichiers partiels avec underscore (`_variables.scss`)
- **Documente toujours** les mixins et fonctions complexes

## 📁 Architecture 7-1

### Structure de Dossiers

```
scss/
├── abstracts/           # Variables, mixins, functions (pas de CSS généré)
│   ├── _index.scss      # Forward all abstracts
│   ├── _variables.scss  # Variables globales
│   ├── _mixins.scss     # Mixins
│   ├── _functions.scss  # Fonctions SCSS
│   └── _placeholders.scss # Placeholders (%)
│
├── base/                # Reset, typography, base styles
│   ├── _index.scss
│   ├── _reset.scss      # Reset/Normalize
│   ├── _typography.scss # Règles typographiques
│   └── _base.scss       # Styles de base (html, body)
│
├── components/          # Composants réutilisables
│   ├── _index.scss
│   ├── _buttons.scss
│   ├── _cards.scss
│   ├── _forms.scss
│   └── _modals.scss
│
├── layout/              # Layout global
│   ├── _index.scss
│   ├── _grid.scss
│   ├── _header.scss
│   ├── _footer.scss
│   └── _sidebar.scss
│
├── pages/               # Styles spécifiques aux pages
│   ├── _index.scss
│   ├── _home.scss
│   └── _dashboard.scss
│
├── themes/              # Thèmes (dark, light, etc.)
│   ├── _index.scss
│   ├── _dark.scss
│   └── _light.scss
│
├── vendors/             # CSS tiers (override)
│   ├── _index.scss
│   └── _primevue.scss
│
└── main.scss            # Point d'entrée principal
```

### Fichier Principal (main.scss)

```scss
/ main.scss - Point d'entrée unique

/ Abstracts (pas de CSS généré)
@use 'abstracts';

/ Vendors (overrides tiers)
@use 'vendors';

/ Base styles
@use 'base';

/ Layout
@use 'layout';

/ Components
@use 'components';

/ Pages
@use 'pages';

/ Themes
@use 'themes';
```

## 📦 Variables

### Organisation des Variables

```scss
/ abstracts/_variables.scss

/ ============================================
/ COULEURS
/ ============================================

/ Couleurs de base
$color-primary: #0288d1 !default;
$color-secondary: #78909c !default;
$color-accent: #ff5722 !default;

/ Couleurs sémantiques
$color-success: #4caf50 !default;
$color-warning: #ff9800 !default;
$color-error: #f44336 !default;
$color-info: #2196f3 !default;

/ Couleurs neutres
$color-white: #ffffff !default;
$color-black: #000000 !default;
$color-gray-50: #fafafa !default;
$color-gray-100: #f5f5f5 !default;
$color-gray-200: #eeeeee !default;
$color-gray-300: #e0e0e0 !default;
$color-gray-400: #bdbdbd !default;
$color-gray-500: #9e9e9e !default;
$color-gray-600: #757575 !default;
$color-gray-700: #616161 !default;
$color-gray-800: #424242 !default;
$color-gray-900: #212121 !default;

/ Couleurs de texte
$color-text-primary: $color-gray-900 !default;
$color-text-secondary: $color-gray-600 !default;
$color-text-disabled: $color-gray-400 !default;

/ Couleurs de fond
$color-background: $color-gray-100 !default;
$color-surface: $color-white !default;

/ ============================================
/ TYPOGRAPHIE
/ ============================================

/ Familles de polices
$font-family-base: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif !default;
$font-family-mono: 'Fira Code', 'Consolas', monospace !default;

/ Tailles de police
$font-size-xs: 0.75rem !default;    / 12px
$font-size-sm: 0.875rem !default;   / 14px
$font-size-base: 1rem !default;     / 16px
$font-size-lg: 1.125rem !default;   / 18px
$font-size-xl: 1.25rem !default;    / 20px
$font-size-2xl: 1.5rem !default;    / 24px
$font-size-3xl: 1.875rem !default;  / 30px
$font-size-4xl: 2.25rem !default;   / 36px

/ Poids de police
$font-weight-light: 300 !default;
$font-weight-normal: 400 !default;
$font-weight-medium: 500 !default;
$font-weight-semibold: 600 !default;
$font-weight-bold: 700 !default;

/ Hauteurs de ligne
$line-height-tight: 1.25 !default;
$line-height-normal: 1.5 !default;
$line-height-relaxed: 1.75 !default;

/ ============================================
/ ESPACEMENTS
/ ============================================

$spacing-unit: 0.25rem !default; / 4px

$spacing-0: 0 !default;
$spacing-1: $spacing-unit !default;       / 4px
$spacing-2: $spacing-unit * 2 !default;   / 8px
$spacing-3: $spacing-unit * 3 !default;   / 12px
$spacing-4: $spacing-unit * 4 !default;   / 16px
$spacing-5: $spacing-unit * 5 !default;   / 20px
$spacing-6: $spacing-unit * 6 !default;   / 24px
$spacing-8: $spacing-unit * 8 !default;   / 32px
$spacing-10: $spacing-unit * 10 !default; / 40px
$spacing-12: $spacing-unit * 12 !default; / 48px
$spacing-16: $spacing-unit * 16 !default; / 64px

/ Map pour les itérations
$spacings: (
  0: $spacing-0,
  1: $spacing-1,
  2: $spacing-2,
  3: $spacing-3,
  4: $spacing-4,
  5: $spacing-5,
  6: $spacing-6,
  8: $spacing-8,
  10: $spacing-10,
  12: $spacing-12,
  16: $spacing-16
) !default;

/ ============================================
/ BREAKPOINTS
/ ============================================

$breakpoint-xs: 0 !default;
$breakpoint-sm: 576px !default;
$breakpoint-md: 768px !default;
$breakpoint-lg: 992px !default;
$breakpoint-xl: 1200px !default;
$breakpoint-2xl: 1400px !default;

$breakpoints: (
  xs: $breakpoint-xs,
  sm: $breakpoint-sm,
  md: $breakpoint-md,
  lg: $breakpoint-lg,
  xl: $breakpoint-xl,
  2xl: $breakpoint-2xl
) !default;

/ ============================================
/ BORDURES ET OMBRES
/ ============================================

$border-radius-sm: 0.25rem !default;  / 4px
$border-radius-md: 0.375rem !default; / 6px
$border-radius-lg: 0.5rem !default;   / 8px
$border-radius-xl: 0.75rem !default;  / 12px
$border-radius-full: 9999px !default;

$border-width: 1px !default;
$border-color: $color-gray-300 !default;

$shadow-sm: 0 1px 2px 0 rgb(0 0 0 / 5%) !default;
$shadow-md: 0 4px 6px -1px rgb(0 0 0 / 10%) !default;
$shadow-lg: 0 10px 15px -3px rgb(0 0 0 / 10%) !default;
$shadow-xl: 0 20px 25px -5px rgb(0 0 0 / 10%) !default;

/ ============================================
/ TRANSITIONS
/ ============================================

$transition-duration-fast: 150ms !default;
$transition-duration-base: 200ms !default;
$transition-duration-slow: 300ms !default;

$transition-timing: ease-in-out !default;

/ ============================================
/ Z-INDEX
/ ============================================

$z-index-dropdown: 1000 !default;
$z-index-sticky: 1020 !default;
$z-index-fixed: 1030 !default;
$z-index-modal-backdrop: 1040 !default;
$z-index-modal: 1050 !default;
$z-index-popover: 1060 !default;
$z-index-tooltip: 1070 !default;
```

## 🔧 Mixins

### Mixins Essentiels

```scss
/ abstracts/_mixins.scss
@use 'variables' as *;

/ ============================================
/ RESPONSIVE
/ ============================================

// Media query pour écran minimum
// @param {String} $breakpoint - Nom du breakpoint (sm, md, lg, xl, 2xl)
@mixin media-up($breakpoint) {
  $value: map-get($breakpoints, $breakpoint);
  @if $value {
    @media (min-width: $value) {
      @content;
    }
  } @else {
    @warn "Breakpoint `#{$breakpoint}` non trouvé dans $breakpoints.";
  }
}

// Media query pour écran maximum
// @param {String} $breakpoint - Nom du breakpoint
@mixin media-down($breakpoint) {
  $value: map-get($breakpoints, $breakpoint);
  @if $value {
    @media (max-width: ($value - 0.02px)) {
      @content;
    }
  }
}

// Media query entre deux breakpoints
// @param {String} $lower - Breakpoint minimum
// @param {String} $upper - Breakpoint maximum
@mixin media-between($lower, $upper) {
  $min: map-get($breakpoints, $lower);
  $max: map-get($breakpoints, $upper);
  @if $min and $max {
    @media (min-width: $min) and (max-width: ($max - 0.02px)) {
      @content;
    }
  }
}

/ ============================================
/ FLEXBOX
/ ============================================

// Conteneur flex centré
@mixin flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
}

// Conteneur flex avec espacement entre éléments
@mixin flex-between {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

// Conteneur flex colonne
@mixin flex-column {
  display: flex;
  flex-direction: column;
}

/ ============================================
/ TYPOGRAPHY
/ ============================================

// Style de titre
// @param {Number} $level - Niveau du titre (1-6)
@mixin heading($level: 1) {
  font-family: $font-family-base;
  font-weight: $font-weight-semibold;
  line-height: $line-height-tight;
  
  @if $level == 1 {
    font-size: $font-size-4xl;
  } @else if $level == 2 {
    font-size: $font-size-3xl;
  } @else if $level == 3 {
    font-size: $font-size-2xl;
  } @else if $level == 4 {
    font-size: $font-size-xl;
  } @else if $level == 5 {
    font-size: $font-size-lg;
  } @else {
    font-size: $font-size-base;
  }
}

// Texte tronqué avec ellipsis
@mixin text-truncate {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

// Texte tronqué multi-lignes
// @param {Number} $lines - Nombre de lignes max
@mixin text-clamp($lines: 2) {
  display: -webkit-box;
  -webkit-line-clamp: $lines;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/ ============================================
/ TRANSITIONS
/ ============================================

// Transition standard
// @param {List} $properties - Propriétés à animer (all par défaut)
@mixin transition($properties: all) {
  transition-property: $properties;
  transition-duration: $transition-duration-base;
  transition-timing-function: $transition-timing;
}

// Transition rapide
@mixin transition-fast($properties: all) {
  transition-property: $properties;
  transition-duration: $transition-duration-fast;
  transition-timing-function: $transition-timing;
}

/ ============================================
/ ÉTATS INTERACTIFS
/ ============================================

// États hover et focus
@mixin interactive {
  cursor: pointer;
  @include transition(background-color, color, border-color, box-shadow);
  
  &:hover {
    @content;
  }
  
  &:focus-visible {
    outline: 2px solid $color-primary;
    outline-offset: 2px;
  }
  
  &:disabled {
    cursor: not-allowed;
    opacity: 0.5;
  }
}

/ ============================================
/ POSITION
/ ============================================

// Position absolute centrée
@mixin absolute-center {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
}

// Position absolute plein écran
@mixin absolute-fill {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
}

/ ============================================
/ ACCESSIBILITÉ
/ ============================================

// Masquer visuellement mais garder accessible
@mixin visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

// Focus visible pour accessibilité
@mixin focus-ring {
  &:focus-visible {
    outline: 2px solid $color-primary;
    outline-offset: 2px;
  }
}
```

## 📐 Fonctions SCSS

```scss
/ abstracts/_functions.scss
@use 'sass:math';
@use 'sass:color';
@use 'sass:map';
@use 'variables' as *;

/ ============================================
/ COULEURS
/ ============================================

// Éclaircir une couleur
// @param {Color} $color - Couleur de base
// @param {Number} $amount - Pourcentage (0-100)
// @return {Color}
@function lighten-color($color, $amount) {
  @return color.mix(white, $color, $amount);
}

// Assombrir une couleur
// @param {Color} $color - Couleur de base
// @param {Number} $amount - Pourcentage (0-100)
// @return {Color}
@function darken-color($color, $amount) {
  @return color.mix(black, $color, $amount);
}

// Obtenir une couleur avec transparence
// @param {Color} $color - Couleur de base
// @param {Number} $alpha - Opacité (0-1)
// @return {Color}
@function alpha-color($color, $alpha) {
  @return color.change($color, $alpha: $alpha);
}

/ ============================================
/ ESPACEMENTS
/ ============================================

// Obtenir une valeur d'espacement
// @param {Number} $key - Clé de l'espacement
// @return {Length}
@function spacing($key) {
  @if map.has-key($spacings, $key) {
    @return map.get($spacings, $key);
  }
  @warn "Spacing `#{$key}` non trouvé.";
  @return 0;
}

/ ============================================
/ CONVERSIONS
/ ============================================

// Convertir pixels en rem
// @param {Number} $px - Valeur en pixels
// @param {Number} $base - Taille de base (16px par défaut)
// @return {Length}
@function px-to-rem($px, $base: 16) {
  @return math.div($px, $base) * 1rem;
}

// Convertir rem en pixels
// @param {Number} $rem - Valeur en rem
// @param {Number} $base - Taille de base (16px par défaut)
// @return {Length}
@function rem-to-px($rem, $base: 16) {
  @return math.div($rem, 1rem) * $base * 1px;
}

/ ============================================
/ BREAKPOINTS
/ ============================================

// Obtenir une valeur de breakpoint
// @param {String} $name - Nom du breakpoint
// @return {Length}
@function breakpoint($name) {
  @if map.has-key($breakpoints, $name) {
    @return map.get($breakpoints, $name);
  }
  @warn "Breakpoint `#{$name}` non trouvé.";
  @return 0;
}
```

## 🎨 Placeholders

```scss
/ abstracts/_placeholders.scss
@use 'variables' as *;
@use 'mixins' as *;

/ ============================================
/ LAYOUTS COMMUNS
/ ============================================

%flex-center {
  @include flex-center;
}

%flex-between {
  @include flex-between;
}

%flex-column {
  @include flex-column;
}

/ ============================================
/ CARDS
/ ============================================

%card-base {
  background-color: $color-surface;
  border-radius: $border-radius-lg;
  box-shadow: $shadow-sm;
}

%card-elevated {
  @extend %card-base;
  box-shadow: $shadow-md;
}

/ ============================================
/ BUTTONS
/ ============================================

%button-reset {
  appearance: none;
  border: none;
  background: none;
  padding: 0;
  margin: 0;
  font: inherit;
  cursor: pointer;
}

%button-base {
  @extend %button-reset;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: $spacing-2;
  padding: $spacing-2 $spacing-4;
  border-radius: $border-radius-md;
  font-weight: $font-weight-medium;
  @include transition(background-color, color, box-shadow);
  @include focus-ring;
}

/ ============================================
/ INPUTS
/ ============================================

%input-base {
  width: 100%;
  padding: $spacing-2 $spacing-3;
  border: $border-width solid $border-color;
  border-radius: $border-radius-md;
  font-family: $font-family-base;
  font-size: $font-size-base;
  @include transition(border-color, box-shadow);
  
  &:focus {
    outline: none;
    border-color: $color-primary;
    box-shadow: 0 0 0 3px alpha-color($color-primary, 0.1);
  }
  
  &::placeholder {
    color: $color-text-disabled;
  }
}

/ ============================================
/ ACCESSIBILITÉ
/ ============================================

%visually-hidden {
  @include visually-hidden;
}
```

## ⚠️ Bonnes Pratiques

### Nesting (Maximum 3 niveaux)

```scss
/ ✅ BON : 3 niveaux max
.card {
  padding: $spacing-4;
  
  &__header {
    margin-bottom: $spacing-3;
    
    &-title {
      font-size: $font-size-lg;
    }
  }
}

/ ❌ MAUVAIS : Trop de niveaux
.page {
  .section {
    .container {
      .card {
        .header {
          .title {
            / 6 niveaux = trop profond!
          }
        }
      }
    }
  }
}
```

### BEM avec SCSS

```scss
/ ✅ BON : BEM avec parent selector
.button {
  / Block
  @extend %button-base;
  
  / Element
  &__icon {
    width: 1.25em;
    height: 1.25em;
  }
  
  &__text {
    font-weight: $font-weight-medium;
  }
  
  / Modifier
  &--primary {
    background-color: $color-primary;
    color: $color-white;
    
    &:hover {
      background-color: darken-color($color-primary, 10%);
    }
  }
  
  &--secondary {
    background-color: transparent;
    border: $border-width solid $color-primary;
    color: $color-primary;
  }
  
  &--large {
    padding: $spacing-3 $spacing-6;
    font-size: $font-size-lg;
  }
  
  / États
  &:disabled,
  &--disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
}
```
