---
description: SCSS Mixins - Mixins avancés, responsive design, grilles, animations, utilities
name: SCSS_Mixins
applyTo: "**/frontend/assets/scss/abstracts/_mixins.scss,**/frontend/assets/scss/**/_*.scss"
---

# SCSS - Mixins Avancés

Guide complet des mixins SCSS pour le design system.

## ⛔ À NE PAS FAIRE

- **Ne crée jamais** de mixin sans paramètre si un placeholder suffit
- **N'utilise jamais** de valeurs en dur dans les mixins
- **Ne duplique jamais** la logique entre mixins
- **N'oublie jamais** de documenter les paramètres
- **Ne crée jamais** de mixin trop spécifique (non réutilisable)
- **N'ignore jamais** les valeurs par défaut

## ✅ À FAIRE

- **Utilise toujours** des paramètres avec valeurs par défaut
- **Documente toujours** avec `///` et `@param`
- **Valide toujours** les paramètres avec `@if` et `@warn`
- **Utilise toujours** des maps pour les configurations complexes
- **Préfère toujours** les mixins composables aux mixins monolithiques
- **Teste toujours** les mixins avec différentes valeurs

## 🎨 Mixins de Couleurs

### Génération de Palette

```scss
// abstracts/_mixins-colors.scss
@use 'sass:color';
@use 'sass:map';
@use 'variables' as *;

/// Génère une palette de couleurs à partir d'une couleur de base
/// @param {String} $name - Nom de la palette
/// @param {Color} $base-color - Couleur de base
/// @output Variables CSS pour la palette
@mixin generate-color-palette($name, $base-color) {
  --#{$name}-50: #{color.mix(white, $base-color, 95%)};
  --#{$name}-100: #{color.mix(white, $base-color, 90%)};
  --#{$name}-200: #{color.mix(white, $base-color, 70%)};
  --#{$name}-300: #{color.mix(white, $base-color, 50%)};
  --#{$name}-400: #{color.mix(white, $base-color, 30%)};
  --#{$name}-500: #{$base-color};
  --#{$name}-600: #{color.mix(black, $base-color, 10%)};
  --#{$name}-700: #{color.mix(black, $base-color, 30%)};
  --#{$name}-800: #{color.mix(black, $base-color, 50%)};
  --#{$name}-900: #{color.mix(black, $base-color, 70%)};
}

/// Applique des styles de surface avec fond et texte contrasté
/// @param {Color} $background - Couleur de fond
/// @param {Color} $text - Couleur de texte (auto si null)
@mixin surface($background, $text: null) {
  background-color: $background;
  
  @if $text {
    color: $text;
  } @else {
    // Calcul automatique du contraste
    $luminance: color.lightness($background);
    @if $luminance > 60% {
      color: $color-gray-900;
    } @else {
      color: $color-white;
    }
  }
}

/// Génère un dégradé
/// @param {Color} $start - Couleur de départ
/// @param {Color} $end - Couleur de fin
/// @param {String} $direction - Direction (to right, to bottom, etc.)
@mixin gradient($start, $end, $direction: to right) {
  background: linear-gradient($direction, $start, $end);
}
```

## 📐 Mixins de Layout

### Grid System

```scss
// abstracts/_mixins-grid.scss
@use 'sass:math';
@use 'variables' as *;
@use 'mixins' as *;

/// Conteneur de grille avec colonnes auto-fill
/// @param {Length} $min-width - Largeur minimum des colonnes
/// @param {Length} $gap - Espacement entre les éléments
@mixin auto-grid($min-width: 250px, $gap: $spacing-4) {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax($min-width, 1fr));
  gap: $gap;
}

/// Grille avec nombre fixe de colonnes
/// @param {Number} $columns - Nombre de colonnes
/// @param {Length} $gap - Espacement
@mixin grid-columns($columns: 12, $gap: $spacing-4) {
  display: grid;
  grid-template-columns: repeat($columns, 1fr);
  gap: $gap;
}

/// Élément de grille avec span
/// @param {Number} $span - Nombre de colonnes à occuper
/// @param {Number} $start - Colonne de départ (optionnel)
@mixin grid-span($span, $start: null) {
  @if $start {
    grid-column: $start / span $span;
  } @else {
    grid-column: span $span;
  }
}

/// Grille responsive avec breakpoints
/// @param {Map} $config - Configuration par breakpoint
/// @example
///   @include responsive-grid((
///     xs: 1,
///     sm: 2,
///     md: 3,
///     lg: 4
///   ));
@mixin responsive-grid($config, $gap: $spacing-4) {
  display: grid;
  gap: $gap;
  
  @each $breakpoint, $columns in $config {
    @if $breakpoint == xs {
      grid-template-columns: repeat($columns, 1fr);
    } @else {
      @include media-up($breakpoint) {
        grid-template-columns: repeat($columns, 1fr);
      }
    }
  }
}

/// Container avec largeur max et centrage
/// @param {Length} $max-width - Largeur maximum
/// @param {Length} $padding - Padding horizontal
@mixin container($max-width: 1200px, $padding: $spacing-4) {
  width: 100%;
  max-width: $max-width;
  margin-left: auto;
  margin-right: auto;
  padding-left: $padding;
  padding-right: $padding;
}
```

### Flexbox Avancé

```scss
// abstracts/_mixins-flex.scss
@use 'variables' as *;

/// Flexbox avec configuration complète
/// @param {String} $direction - Direction (row, column)
/// @param {String} $justify - Justification
/// @param {String} $align - Alignement
/// @param {String} $wrap - Wrapping
/// @param {Length} $gap - Espacement
@mixin flex(
  $direction: row,
  $justify: flex-start,
  $align: stretch,
  $wrap: nowrap,
  $gap: null
) {
  display: flex;
  flex-direction: $direction;
  justify-content: $justify;
  align-items: $align;
  flex-wrap: $wrap;
  
  @if $gap {
    gap: $gap;
  }
}

/// Élément flex avec taille
/// @param {Number} $grow - Facteur de croissance
/// @param {Number} $shrink - Facteur de rétrécissement
/// @param {Length} $basis - Taille de base
@mixin flex-item($grow: 0, $shrink: 1, $basis: auto) {
  flex: $grow $shrink $basis;
}

/// Stack vertical avec espacement
/// @param {Length} $gap - Espacement entre éléments
@mixin stack($gap: $spacing-4) {
  display: flex;
  flex-direction: column;
  gap: $gap;
}

/// Stack horizontal avec espacement
/// @param {Length} $gap - Espacement entre éléments
@mixin row($gap: $spacing-4) {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: $gap;
}
```

## 🎬 Mixins d'Animation

### Animations de Base

```scss
// abstracts/_mixins-animations.scss
@use 'sass:math';
@use 'variables' as *;

/// Définit une animation avec keyframes
/// @param {String} $name - Nom de l'animation
/// @param {Time} $duration - Durée
/// @param {String} $timing - Fonction de timing
/// @param {Number} $iteration - Nombre d'itérations
@mixin animation(
  $name,
  $duration: $transition-duration-base,
  $timing: $transition-timing,
  $iteration: 1
) {
  animation-name: $name;
  animation-duration: $duration;
  animation-timing-function: $timing;
  animation-iteration-count: $iteration;
  animation-fill-mode: both;
}

/// Animation de fondu entrant
/// @param {Time} $duration - Durée de l'animation
@mixin fade-in($duration: $transition-duration-base) {
  @include animation(fadeIn, $duration);
}

/// Animation de glissement depuis le bas
/// @param {Time} $duration - Durée
/// @param {Length} $distance - Distance de départ
@mixin slide-up($duration: $transition-duration-slow, $distance: 20px) {
  @include animation(slideUp, $duration);
  
  @at-root {
    @keyframes slideUp {
      from {
        opacity: 0;
        transform: translateY($distance);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
  }
}

/// Animation de glissement depuis la gauche
/// @param {Time} $duration - Durée
/// @param {Length} $distance - Distance de départ
@mixin slide-right($duration: $transition-duration-slow, $distance: 20px) {
  @include animation(slideRight, $duration);
  
  @at-root {
    @keyframes slideRight {
      from {
        opacity: 0;
        transform: translateX(-$distance);
      }
      to {
        opacity: 1;
        transform: translateX(0);
      }
    }
  }
}

/// Animation de zoom
/// @param {Time} $duration - Durée
/// @param {Number} $from-scale - Échelle de départ
@mixin zoom-in($duration: $transition-duration-base, $from-scale: 0.95) {
  @include animation(zoomIn, $duration);
  
  @at-root {
    @keyframes zoomIn {
      from {
        opacity: 0;
        transform: scale($from-scale);
      }
      to {
        opacity: 1;
        transform: scale(1);
      }
    }
  }
}

/// Animation de pulsation
/// @param {Time} $duration - Durée d'un cycle
@mixin pulse($duration: 1s) {
  @include animation(pulse, $duration, ease-in-out, infinite);
  
  @at-root {
    @keyframes pulse {
      0%, 100% {
        opacity: 1;
      }
      50% {
        opacity: 0.5;
      }
    }
  }
}

/// Animation de rotation (spinner)
/// @param {Time} $duration - Durée d'une rotation
@mixin spin($duration: 1s) {
  @include animation(spin, $duration, linear, infinite);
  
  @at-root {
    @keyframes spin {
      from {
        transform: rotate(0deg);
      }
      to {
        transform: rotate(360deg);
      }
    }
  }
}

/// Animation de secousse
@mixin shake($duration: 500ms) {
  @include animation(shake, $duration);
  
  @at-root {
    @keyframes shake {
      0%, 100% { transform: translateX(0); }
      10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
      20%, 40%, 60%, 80% { transform: translateX(5px); }
    }
  }
}
```

### Animations de Transition Vue

```scss
// abstracts/_mixins-vue-transitions.scss
@use 'variables' as *;

/// Transition Vue.js fade
/// @param {String} $name - Nom de la transition
/// @param {Time} $duration - Durée
@mixin vue-fade($name: 'fade', $duration: $transition-duration-base) {
  .#{$name}-enter-active,
  .#{$name}-leave-active {
    transition: opacity $duration $transition-timing;
  }
  
  .#{$name}-enter-from,
  .#{$name}-leave-to {
    opacity: 0;
  }
}

/// Transition Vue.js slide
/// @param {String} $name - Nom de la transition
/// @param {String} $direction - Direction (up, down, left, right)
/// @param {Time} $duration - Durée
/// @param {Length} $distance - Distance de déplacement
@mixin vue-slide(
  $name: 'slide',
  $direction: 'up',
  $duration: $transition-duration-slow,
  $distance: 20px
) {
  .#{$name}-enter-active,
  .#{$name}-leave-active {
    transition: all $duration $transition-timing;
  }
  
  .#{$name}-enter-from,
  .#{$name}-leave-to {
    opacity: 0;
    
    @if $direction == 'up' {
      transform: translateY($distance);
    } @else if $direction == 'down' {
      transform: translateY(-$distance);
    } @else if $direction == 'left' {
      transform: translateX($distance);
    } @else if $direction == 'right' {
      transform: translateX(-$distance);
    }
  }
}

/// Transition Vue.js scale
/// @param {String} $name - Nom de la transition
/// @param {Time} $duration - Durée
@mixin vue-scale($name: 'scale', $duration: $transition-duration-base) {
  .#{$name}-enter-active,
  .#{$name}-leave-active {
    transition: all $duration $transition-timing;
  }
  
  .#{$name}-enter-from,
  .#{$name}-leave-to {
    opacity: 0;
    transform: scale(0.9);
  }
}

/// Transition de liste Vue.js
/// @param {String} $name - Nom de la transition
/// @param {Time} $duration - Durée
@mixin vue-list($name: 'list', $duration: $transition-duration-slow) {
  .#{$name}-move,
  .#{$name}-enter-active,
  .#{$name}-leave-active {
    transition: all $duration $transition-timing;
  }
  
  .#{$name}-enter-from,
  .#{$name}-leave-to {
    opacity: 0;
    transform: translateX(-30px);
  }
  
  .#{$name}-leave-active {
    position: absolute;
  }
}
```

## 📱 Mixins Responsive

### Container Queries

```scss
// abstracts/_mixins-container.scss
@use 'variables' as *;

/// Définit un contexte de container query
/// @param {String} $name - Nom du container (optionnel)
@mixin container-context($name: null) {
  container-type: inline-size;
  
  @if $name {
    container-name: $name;
  }
}

/// Container query pour largeur minimum
/// @param {Length} $width - Largeur minimum
/// @param {String} $name - Nom du container (optionnel)
@mixin container-up($width, $name: null) {
  @if $name {
    @container #{$name} (min-width: #{$width}) {
      @content;
    }
  } @else {
    @container (min-width: #{$width}) {
      @content;
    }
  }
}
```

### Responsive Spacing

```scss
// abstracts/_mixins-spacing.scss
@use 'sass:map';
@use 'variables' as *;
@use 'mixins' as *;

/// Espacement responsive
/// @param {String} $property - Propriété CSS (margin, padding)
/// @param {Map} $values - Map de valeurs par breakpoint
/// @example
///   @include responsive-spacing(padding, (xs: 2, md: 4, lg: 6));
@mixin responsive-spacing($property, $values) {
  @each $breakpoint, $size in $values {
    $value: map.get($spacings, $size);
    
    @if $breakpoint == xs {
      #{$property}: $value;
    } @else {
      @include media-up($breakpoint) {
        #{$property}: $value;
      }
    }
  }
}

/// Margin responsive
/// @param {Map} $values - Map de valeurs par breakpoint
@mixin responsive-margin($values) {
  @include responsive-spacing(margin, $values);
}

/// Padding responsive
/// @param {Map} $values - Map de valeurs par breakpoint
@mixin responsive-padding($values) {
  @include responsive-spacing(padding, $values);
}

/// Font-size responsive
/// @param {Map} $values - Map de font-sizes par breakpoint
/// @example
///   @include responsive-font-size((xs: sm, md: base, lg: lg));
@mixin responsive-font-size($values) {
  @each $breakpoint, $size in $values {
    $font-sizes: (
      xs: $font-size-xs,
      sm: $font-size-sm,
      base: $font-size-base,
      lg: $font-size-lg,
      xl: $font-size-xl,
      2xl: $font-size-2xl,
      3xl: $font-size-3xl,
      4xl: $font-size-4xl
    );
    
    $value: map.get($font-sizes, $size);
    
    @if $breakpoint == xs {
      font-size: $value;
    } @else {
      @include media-up($breakpoint) {
        font-size: $value;
      }
    }
  }
}
```

## 🧩 Mixins de Composants

### Boutons

```scss
// abstracts/_mixins-buttons.scss
@use 'variables' as *;
@use 'mixins' as *;

/// Bouton de base
/// @param {Length} $padding-y - Padding vertical
/// @param {Length} $padding-x - Padding horizontal
/// @param {Length} $font-size - Taille de police
@mixin button-base(
  $padding-y: $spacing-2,
  $padding-x: $spacing-4,
  $font-size: $font-size-base
) {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: $spacing-2;
  padding: $padding-y $padding-x;
  font-family: $font-family-base;
  font-size: $font-size;
  font-weight: $font-weight-medium;
  line-height: $line-height-normal;
  text-decoration: none;
  border: none;
  border-radius: $border-radius-md;
  cursor: pointer;
  @include transition(background-color, color, border-color, box-shadow);
  @include focus-ring;
  
  &:disabled {
    cursor: not-allowed;
    opacity: 0.5;
  }
}

/// Variante de bouton rempli
/// @param {Color} $bg - Couleur de fond
/// @param {Color} $color - Couleur de texte
/// @param {Color} $hover-bg - Couleur de fond au hover
@mixin button-filled($bg, $color: $color-white, $hover-bg: null) {
  background-color: $bg;
  color: $color;
  
  &:hover:not(:disabled) {
    @if $hover-bg {
      background-color: $hover-bg;
    } @else {
      background-color: darken-color($bg, 10%);
    }
  }
  
  &:active:not(:disabled) {
    background-color: darken-color($bg, 15%);
  }
}

/// Variante de bouton outline
/// @param {Color} $color - Couleur de la bordure et du texte
/// @param {Color} $hover-bg - Couleur de fond au hover
@mixin button-outlined($color, $hover-bg: null) {
  background-color: transparent;
  color: $color;
  border: $border-width solid $color;
  
  &:hover:not(:disabled) {
    @if $hover-bg {
      background-color: $hover-bg;
    } @else {
      background-color: alpha-color($color, 0.1);
    }
  }
}

/// Variante de bouton texte (ghost)
/// @param {Color} $color - Couleur du texte
@mixin button-ghost($color) {
  background-color: transparent;
  color: $color;
  
  &:hover:not(:disabled) {
    background-color: alpha-color($color, 0.1);
  }
}

/// Tailles de bouton
/// @param {String} $size - Taille (sm, md, lg)
@mixin button-size($size: md) {
  @if $size == sm {
    padding: $spacing-1 $spacing-3;
    font-size: $font-size-sm;
  } @else if $size == lg {
    padding: $spacing-3 $spacing-6;
    font-size: $font-size-lg;
  } @else {
    // md par défaut
    padding: $spacing-2 $spacing-4;
    font-size: $font-size-base;
  }
}
```

### Cards

```scss
// abstracts/_mixins-cards.scss
@use 'variables' as *;
@use 'mixins' as *;

/// Card de base
/// @param {Length} $padding - Padding interne
/// @param {String} $radius - Border radius (sm, md, lg, xl)
@mixin card($padding: $spacing-4, $radius: lg) {
  $radius-map: (
    sm: $border-radius-sm,
    md: $border-radius-md,
    lg: $border-radius-lg,
    xl: $border-radius-xl
  );
  
  background-color: $color-surface;
  border-radius: map-get($radius-map, $radius);
  padding: $padding;
}

/// Card avec ombre
/// @param {String} $elevation - Niveau d'élévation (sm, md, lg, xl)
@mixin card-elevated($elevation: md) {
  $shadow-map: (
    sm: $shadow-sm,
    md: $shadow-md,
    lg: $shadow-lg,
    xl: $shadow-xl
  );
  
  @include card;
  box-shadow: map-get($shadow-map, $elevation);
}

/// Card avec bordure
/// @param {Color} $border-color - Couleur de bordure
@mixin card-bordered($border-color: $border-color) {
  @include card;
  border: $border-width solid $border-color;
}

/// Card cliquable (interactive)
@mixin card-clickable {
  @include card-elevated(sm);
  cursor: pointer;
  @include transition(box-shadow, transform);
  
  &:hover {
    box-shadow: $shadow-md;
    transform: translateY(-2px);
  }
  
  &:active {
    transform: translateY(0);
  }
}
```

### Formulaires

```scss
// abstracts/_mixins-forms.scss
@use 'variables' as *;
@use 'mixins' as *;

/// Input de base
/// @param {String} $size - Taille (sm, md, lg)
@mixin input-base($size: md) {
  display: block;
  width: 100%;
  font-family: $font-family-base;
  border: $border-width solid $border-color;
  border-radius: $border-radius-md;
  background-color: $color-surface;
  @include transition(border-color, box-shadow);
  
  @if $size == sm {
    padding: $spacing-1 $spacing-2;
    font-size: $font-size-sm;
  } @else if $size == lg {
    padding: $spacing-3 $spacing-4;
    font-size: $font-size-lg;
  } @else {
    padding: $spacing-2 $spacing-3;
    font-size: $font-size-base;
  }
  
  &::placeholder {
    color: $color-text-disabled;
  }
  
  &:focus {
    outline: none;
    border-color: $color-primary;
    box-shadow: 0 0 0 3px alpha-color($color-primary, 0.15);
  }
  
  &:disabled {
    background-color: $color-gray-100;
    cursor: not-allowed;
    opacity: 0.7;
  }
}

/// Input avec état d'erreur
@mixin input-error {
  border-color: $color-error;
  
  &:focus {
    border-color: $color-error;
    box-shadow: 0 0 0 3px alpha-color($color-error, 0.15);
  }
}

/// Input avec état de succès
@mixin input-success {
  border-color: $color-success;
  
  &:focus {
    border-color: $color-success;
    box-shadow: 0 0 0 3px alpha-color($color-success, 0.15);
  }
}

/// Label de formulaire
@mixin form-label {
  display: block;
  margin-bottom: $spacing-1;
  font-size: $font-size-sm;
  font-weight: $font-weight-medium;
  color: $color-text-primary;
}

/// Message d'aide
@mixin form-helper {
  margin-top: $spacing-1;
  font-size: $font-size-sm;
  color: $color-text-secondary;
}

/// Message d'erreur
@mixin form-error-message {
  @include form-helper;
  color: $color-error;
}

/// Groupe de formulaire
@mixin form-group {
  margin-bottom: $spacing-4;
}
```

## 🛠️ Mixins Utilitaires

### Génération de Classes Utilitaires

```scss
// abstracts/_mixins-utilities.scss
@use 'sass:map';
@use 'variables' as *;
@use 'mixins' as *;

/// Génère des classes d'espacement
/// @param {String} $prefix - Préfixe (m pour margin, p pour padding)
/// @param {String} $property - Propriété CSS
@mixin generate-spacing-utilities($prefix, $property) {
  @each $key, $value in $spacings {
    .#{$prefix}-#{$key} {
      #{$property}: $value;
    }
    
    .#{$prefix}t-#{$key} {
      #{$property}-top: $value;
    }
    
    .#{$prefix}r-#{$key} {
      #{$property}-right: $value;
    }
    
    .#{$prefix}b-#{$key} {
      #{$property}-bottom: $value;
    }
    
    .#{$prefix}l-#{$key} {
      #{$property}-left: $value;
    }
    
    .#{$prefix}x-#{$key} {
      #{$property}-left: $value;
      #{$property}-right: $value;
    }
    
    .#{$prefix}y-#{$key} {
      #{$property}-top: $value;
      #{$property}-bottom: $value;
    }
  }
}

/// Génère des classes de display
@mixin generate-display-utilities {
  $displays: (block, inline-block, inline, flex, inline-flex, grid, none);
  
  @each $display in $displays {
    .d-#{$display} {
      display: $display;
    }
  }
  
  // Responsive
  @each $breakpoint, $value in $breakpoints {
    @if $breakpoint != xs {
      @include media-up($breakpoint) {
        @each $display in $displays {
          .d-#{$breakpoint}-#{$display} {
            display: $display;
          }
        }
      }
    }
  }
}

/// Génère des classes de texte
@mixin generate-text-utilities {
  // Alignement
  .text-left { text-align: left; }
  .text-center { text-align: center; }
  .text-right { text-align: right; }
  
  // Transformation
  .text-uppercase { text-transform: uppercase; }
  .text-lowercase { text-transform: lowercase; }
  .text-capitalize { text-transform: capitalize; }
  
  // Taille
  .text-xs { font-size: $font-size-xs; }
  .text-sm { font-size: $font-size-sm; }
  .text-base { font-size: $font-size-base; }
  .text-lg { font-size: $font-size-lg; }
  .text-xl { font-size: $font-size-xl; }
  .text-2xl { font-size: $font-size-2xl; }
  .text-3xl { font-size: $font-size-3xl; }
  
  // Poids
  .font-light { font-weight: $font-weight-light; }
  .font-normal { font-weight: $font-weight-normal; }
  .font-medium { font-weight: $font-weight-medium; }
  .font-semibold { font-weight: $font-weight-semibold; }
  .font-bold { font-weight: $font-weight-bold; }
  
  // Truncate
  .truncate {
    @include text-truncate;
  }
}
```

### Aspect Ratio

```scss
/// Aspect ratio pour conteneurs
/// @param {Number} $width - Largeur du ratio
/// @param {Number} $height - Hauteur du ratio
@mixin aspect-ratio($width, $height) {
  aspect-ratio: #{$width} / #{$height};
  
  // Fallback pour anciens navigateurs
  @supports not (aspect-ratio: 1) {
    &::before {
      content: '';
      display: block;
      padding-top: calc(#{$height} / #{$width} * 100%);
    }
    
    > * {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
    }
  }
}

// Raccourcis courants
@mixin aspect-square { aspect-ratio: 1; }
@mixin aspect-video { aspect-ratio: 16 / 9; }
@mixin aspect-photo { aspect-ratio: 4 / 3; }
```
