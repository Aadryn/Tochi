---
description: SCSS Functions - Fonctions mathématiques, couleurs, strings, lists, maps, custom functions
name: SCSS_Functions
applyTo: "**/assets/scss/**/_*.scss,**/assets/scss/abstracts/**/*.scss"
---

# SCSS Functions

Guide complet pour les fonctions SCSS built-in et custom.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** de fonctions dépréciées (comme `lighten()`, `darken()` direct)
- **Ne crée jamais** de fonctions avec des effets de bord
- **N'imbrique jamais** trop de fonctions (max 3 niveaux)
- **N'oublie jamais** les valeurs par défaut pour les paramètres optionnels
- **Ne retourne jamais** de valeurs non typées sans @return

## ✅ À FAIRE

- **Utilise toujours** les modules SCSS modernes (@use sass:color, etc.)
- **Documente toujours** les fonctions avec des commentaires
- **Valide toujours** les arguments d'entrée
- **Utilise toujours** @error pour les validations
- **Nomme toujours** les fonctions avec des verbes d'action

## 📌 Fonctions Built-in Modernes

### Module sass:math

```scss
@use 'sass:math';

/ Division (/ est déprécié)
.element {
  width: math.div(100%, 3);
  height: math.div(500px, 2);
}

/ Arrondis
.rounded {
  border-radius: math.round(4.7px);   / 5px
  padding: math.ceil(15.2px);          / 16px
  margin: math.floor(15.8px);          / 15px
}

/ Min/Max
.constrained {
  width: math.min(100%, 500px);
  height: math.max(200px, 50vh);
}

/ Clamp
.clamped {
  font-size: math.clamp(14px, 1rem + 0.5vw, 24px);
}

/ Puissance
.scale {
  transform: scale(math.pow(1.2, 3)); / 1.728
}

/ Racine carrée
.diagonal {
  --diagonal: #{math.sqrt(2)};
}

/ Valeur absolue
.spacing {
  gap: math.abs(-16px); / 16px
}

/ Unités
.conversion {
  / Vérifier si a une unité
  @if math.is-unitless(16) {
    font-size: 16px;
  }
  
  / Supprimer l'unité
  --value: #{math.div(16px, 1px)}; / 16 (sans unité)
  
  / Obtenir l'unité
  --unit: #{math.unit(16px)}; / 'px'
  
  / Compatible
  @if math.compatible(16px, 2em) {
    / false - unités incompatibles
  }
}

/ Pourcentage
.progress {
  width: math.percentage(math.div(3, 4)); / 75%
}

/ Random
.random {
  --rotation: #{math.random() * 360}deg;
  --index: #{math.random(100)}; / entier entre 1 et 100
}

/ Trigonométrie
.trig {
  --sin: #{math.sin(45deg)};
  --cos: #{math.cos(45deg)};
  --tan: #{math.tan(45deg)};
  --asin: #{math.asin(0.5)};
  --acos: #{math.acos(0.5)};
  --atan: #{math.atan(1)};
  --atan2: #{math.atan2(1, 1)};
}

/ Logarithmes
.log {
  --log: #{math.log(10)};
  --log-base: #{math.log(8, 2)}; / 3
}

/ Constantes
.constants {
  --pi: #{math.$pi};
  --e: #{math.$e};
}
```

### Module sass:color

```scss
@use 'sass:color';

$primary: #3b82f6;

/ Ajuster les composants (moderne)
.adjusted {
  / Luminosité
  background: color.scale($primary, $lightness: 20%);
  border-color: color.scale($primary, $lightness: -20%);
  
  / Saturation
  color: color.scale($primary, $saturation: -50%);
  
  / Alpha
  box-shadow: 0 4px 6px color.scale($primary, $alpha: -60%);
}

/ Mixer des couleurs
.mixed {
  background: color.mix($primary, white, 80%);
  border-color: color.mix($primary, black, 90%);
}

/ Extraire des composants
.components {
  --hue: #{color.hue($primary)};
  --saturation: #{color.saturation($primary)};
  --lightness: #{color.lightness($primary)};
  --red: #{color.red($primary)};
  --green: #{color.green($primary)};
  --blue: #{color.blue($primary)};
  --alpha: #{color.alpha($primary)};
}

/ Changer des composants
.changed {
  / Changer la teinte
  background: color.change($primary, $hue: 120deg);
  
  / Changer la saturation
  color: color.change($primary, $saturation: 50%);
  
  / Changer l'alpha
  border-color: color.change($primary, $alpha: 0.5);
}

/ Ajuster relativement
.relative {
  / Augmenter la luminosité de 10%
  background: color.adjust($primary, $lightness: 10%);
  
  / Diminuer la saturation de 20%
  color: color.adjust($primary, $saturation: -20%);
  
  / Décaler la teinte
  border-color: color.adjust($primary, $hue: 30deg);
}

/ Complément et inversion
.complement {
  background: color.complement($primary);
  color: color.invert($primary);
  border-color: color.grayscale($primary);
}

/ Conversion d'espace colorimétrique
.spaces {
  / HSL
  --hsl: #{color.hwb($primary)};
  
  / Vérifier si opaque
  @if color.alpha($primary) == 1 {
    / Couleur opaque
  }
}
```

### Module sass:string

```scss
@use 'sass:string';

$class-name: 'button-primary';

/ Longueur
.info {
  --length: #{string.length($class-name)}; / 14
}

/ Quotes
.quoted {
  content: string.quote(hello); / "hello"
  --unquoted: #{string.unquote("hello")}; / hello
}

/ Recherche
.search {
  / Index (1-based)
  --index: #{string.index($class-name, '-')}; / 7
  
  / Contient
  @if string.index($class-name, 'primary') {
    / Contient 'primary'
  }
}

/ Extraction
.extract {
  / Slice (1-based, inclusive)
  --slice: #{string.slice($class-name, 8)}; / "primary"
  --slice-range: #{string.slice($class-name, 1, 6)}; / "button"
}

/ Case
.case {
  --upper: #{string.to-upper-case($class-name)};
  --lower: #{string.to-lower-case($class-name)};
}

/ Insert
.insert {
  --inserted: #{string.insert($class-name, '-new', 7)}; / "button-new-primary"
}

/ Unique ID
.unique {
  --id: #{string.unique-id()}; / u8f3a2c1
}

/ Split (custom function needed)
@function str-split($string, $separator) {
  $list: ();
  $index: string.index($string, $separator);
  
  @while $index != null {
    $list: append($list, string.slice($string, 1, $index - 1));
    $string: string.slice($string, $index + string.length($separator));
    $index: string.index($string, $separator);
  }
  
  @return append($list, $string);
}
```

### Module sass:list

```scss
@use 'sass:list';

$colors: red, green, blue, yellow;
$sizes: 'sm', 'md', 'lg', 'xl';

/ Longueur
.info {
  --count: #{list.length($colors)}; / 4
}

/ Accès
.access {
  --first: #{list.nth($colors, 1)}; / red
  --last: #{list.nth($colors, -1)}; / yellow
}

/ Modification
.modify {
  / Append
  $extended: list.append($colors, purple);
  / (red, green, blue, yellow, purple)
  
  / Prepend (via join)
  $prepended: list.join(orange, $colors);
  / (orange, red, green, blue, yellow)
  
  / Set
  $modified: list.set-nth($colors, 2, cyan);
  / (red, cyan, blue, yellow)
}

/ Recherche
.search {
  --index: #{list.index($colors, blue)}; / 3
  
  @if list.index($colors, purple) == null {
    / purple n'est pas dans la liste
  }
}

/ Jointure
.join {
  $combined: list.join($colors, $sizes);
  / (red, green, blue, yellow, 'sm', 'md', 'lg', 'xl')
  
  $comma-separated: list.join($colors, $sizes, comma);
  $space-separated: list.join($colors, $sizes, space);
}

/ Zip
$widths: 100px, 200px, 300px;
$heights: 50px, 100px, 150px;

.zip {
  $pairs: list.zip($widths, $heights);
  / ((100px, 50px), (200px, 100px), (300px, 150px))
}

/ Slash séparateur
.slash {
  $ratio: list.slash(16, 9);
  aspect-ratio: $ratio; / 16 / 9
}

/ Séparateur
.separator {
  --sep: #{list.separator($colors)}; / comma
  --is-bracketed: #{list.is-bracketed($colors)}; / false
}

/ Itération avec @each (pas une fonction mais lié)
@each $color in $colors {
  .text-#{$color} {
    color: $color;
  }
}

@each $size, $value in (sm: 14px, md: 16px, lg: 18px) {
  .text-#{$size} {
    font-size: $value;
  }
}
```

### Module sass:map

```scss
@use 'sass:map';

$theme: (
  'primary': #3b82f6,
  'secondary': #64748b,
  'success': #22c55e,
  'error': #ef4444,
);

$breakpoints: (
  'sm': 640px,
  'md': 768px,
  'lg': 1024px,
  'xl': 1280px,
);

/ Accès
.access {
  color: map.get($theme, 'primary');
  
  / Accès profond
  $nested: (
    'colors': (
      'brand': (
        'primary': blue,
        'secondary': green,
      ),
    ),
  );
  
  background: map.get($nested, 'colors', 'brand', 'primary');
}

/ Vérification
.check {
  @if map.has-key($theme, 'primary') {
    / La clé existe
  }
  
  @if not map.has-key($theme, 'warning') {
    / La clé n'existe pas
  }
}

/ Clés et valeurs
.keys-values {
  $keys: map.keys($theme); / ('primary', 'secondary', 'success', 'error')
  $values: map.values($theme); / (#3b82f6, #64748b, #22c55e, #ef4444)
}

/ Modification
.modify {
  / Set
  $updated: map.set($theme, 'warning', #f59e0b);
  
  / Remove
  $reduced: map.remove($theme, 'error');
  
  / Merge
  $extended: map.merge($theme, (
    'warning': #f59e0b,
    'info': #06b6d4,
  ));
  
  / Deep merge
  $deep: map.deep-merge(
    ('a': ('b': 1)),
    ('a': ('c': 2))
  );
  / ('a': ('b': 1, 'c': 2))
}

/ Itération
@each $name, $color in $theme {
  .bg-#{$name} {
    background-color: $color;
  }
  
  .text-#{$name} {
    color: $color;
  }
}
```

### Module sass:meta

```scss
@use 'sass:meta';

/ Inspection de type
$value: 16px;

.type-check {
  @if meta.type-of($value) == 'number' {
    / C'est un nombre
  }
  
  / Types possibles: number, string, color, list, map, bool, null, function
}

/ Variables globales
.global {
  @if meta.global-variable-exists('primary-color') {
    color: $primary-color;
  }
}

/ Variables locales
@mixin check-var($name) {
  @if meta.variable-exists($name) {
    / La variable existe
  }
}

/ Fonctions
.function-check {
  @if meta.function-exists('darken') {
    / La fonction existe
  }
}

/ Mixins
.mixin-check {
  @if meta.mixin-exists('responsive') {
    @include responsive;
  }
}

/ Obtenir une fonction
$my-function: meta.get-function('rgba');
.call {
  color: meta.call($my-function, blue, 0.5);
}

/ Charger CSS
/ @include meta.load-css('other-module');

/ Inspection de modules
/ $module: meta.module-functions('sass:color');
/ $variables: meta.module-variables('sass:math');
```

### Module sass:selector

```scss
@use 'sass:selector';

/ Nest
.nested {
  $result: selector.nest('.parent', '.child');
  / .parent .child
  
  $complex: selector.nest('.a', '&.b', '.c');
  / .a.b .c
}

/ Append
.appended {
  $result: selector.append('.btn', '-primary', ':hover');
  / .btn-primary:hover
}

/ Extend
.extended {
  $result: selector.extend('.a .b', '.b', '.c .d');
  / .a .b, .a .c .d, .c .a .d
}

/ Replace
.replaced {
  $result: selector.replace('.a .b.c', '.b', '.d');
  / .a .d.c
}

/ Unify
.unified {
  $result: selector.unify('.a.b', '.b.c');
  / .a.b.c
  
  $impossible: selector.unify('.a', '.b');
  / null (ne peuvent pas coexister)
}

/ Simple selectors
.simple {
  $selectors: selector.simple-selectors('.btn.primary:hover');
  / (.btn, .primary, :hover)
}

/ Parse
.parsed {
  $parsed: selector.parse('.a, .b > .c');
  / (('.a',), ('.b', '>', '.c'))
}

/ Is superselector
.super {
  $is-super: selector.is-superselector('.parent', '.parent .child');
  / true
}
```

## 🛠️ Fonctions Custom

### Conversion d'Unités

```scss
@use 'sass:math';

// Convertit des pixels en rem
// @param {Number} $pixels - Valeur en pixels
// @param {Number} $base [16px] - Taille de base
// @return {Number} - Valeur en rem
@function px-to-rem($pixels, $base: 16px) {
  @if math.is-unitless($pixels) {
    $pixels: $pixels * 1px;
  }
  
  @if math.is-unitless($base) {
    $base: $base * 1px;
  }
  
  @return math.div($pixels, $base) * 1rem;
}

// Convertit des pixels en em
// @param {Number} $pixels - Valeur en pixels
// @param {Number} $context [16px] - Contexte en pixels
// @return {Number} - Valeur en em
@function px-to-em($pixels, $context: 16px) {
  @if math.is-unitless($pixels) {
    $pixels: $pixels * 1px;
  }
  
  @return math.div($pixels, $context) * 1em;
}

// Convertit rem en pixels
// @param {Number} $rem - Valeur en rem
// @param {Number} $base [16px] - Taille de base
// @return {Number} - Valeur en pixels
@function rem-to-px($rem, $base: 16px) {
  @return math.div($rem, 1rem) * $base;
}

/ Usage
.element {
  font-size: px-to-rem(18); / 1.125rem
  padding: px-to-rem(24);   / 1.5rem
  margin: px-to-em(32, 18px); / 1.778em
}
```

### Couleurs Avancées

```scss
@use 'sass:color';
@use 'sass:math';
@use 'sass:map';

// Génère une palette de couleurs
// @param {Color} $base - Couleur de base
// @return {Map} - Palette avec nuances
@function generate-palette($base) {
  @return (
    50: color.scale($base, $lightness: 95%),
    100: color.scale($base, $lightness: 80%),
    200: color.scale($base, $lightness: 60%),
    300: color.scale($base, $lightness: 40%),
    400: color.scale($base, $lightness: 20%),
    500: $base,
    600: color.scale($base, $lightness: -20%),
    700: color.scale($base, $lightness: -40%),
    800: color.scale($base, $lightness: -60%),
    900: color.scale($base, $lightness: -80%),
  );
}

// Obtient une couleur contrastée (noir ou blanc)
// @param {Color} $background - Couleur de fond
// @return {Color} - Noir ou blanc selon le contraste
@function contrast-color($background) {
  $luminance: color.lightness($background);
  
  @if $luminance > 50% {
    @return #000;
  } @else {
    @return #fff;
  }
}

// Génère une couleur avec alpha
// @param {Color} $color - Couleur de base
// @param {Number} $opacity - Opacité (0-1)
// @return {Color} - Couleur avec alpha
@function alpha($color, $opacity) {
  @return color.change($color, $alpha: $opacity);
}

// Mélange une couleur avec du blanc (tint)
// @param {Color} $color - Couleur de base
// @param {Number} $percentage - Pourcentage de blanc
// @return {Color} - Couleur éclaircie
@function tint($color, $percentage) {
  @return color.mix(white, $color, $percentage);
}

// Mélange une couleur avec du noir (shade)
// @param {Color} $color - Couleur de base
// @param {Number} $percentage - Pourcentage de noir
// @return {Color} - Couleur assombrie
@function shade($color, $percentage) {
  @return color.mix(black, $color, $percentage);
}

/ Usage
$primary: #3b82f6;
$palette: generate-palette($primary);

.button {
  background: $primary;
  color: contrast-color($primary);
  
  &:hover {
    background: map.get($palette, 600);
  }
  
  &:disabled {
    background: alpha($primary, 0.5);
  }
}
```

### Spacing et Layout

```scss
@use 'sass:math';
@use 'sass:map';

// Base spacing unit
$spacing-unit: 4px !default;

// Génère un spacing multiplié
// @param {Number} $multiplier - Multiplicateur
// @return {Number} - Spacing calculé
@function spacing($multiplier) {
  @return $spacing-unit * $multiplier;
}

// Spacing map pour lookup rapide
$spacing-scale: (
  0: 0,
  1: spacing(1),   / 4px
  2: spacing(2),   / 8px
  3: spacing(3),   / 12px
  4: spacing(4),   / 16px
  5: spacing(5),   / 20px
  6: spacing(6),   / 24px
  8: spacing(8),   / 32px
  10: spacing(10), / 40px
  12: spacing(12), / 48px
  16: spacing(16), / 64px
  20: spacing(20), / 80px
  24: spacing(24), / 96px
);

// Obtient un spacing depuis l'échelle
// @param {Number} $key - Clé du spacing
// @return {Number} - Valeur du spacing
@function space($key) {
  @if not map.has-key($spacing-scale, $key) {
    @error "Invalid spacing key: #{$key}. Available: #{map.keys($spacing-scale)}";
  }
  
  @return map.get($spacing-scale, $key);
}

// Calcule un ratio fluide entre deux valeurs
// @param {Number} $min - Valeur minimum
// @param {Number} $max - Valeur maximum
// @param {Number} $min-vw [320px] - Viewport minimum
// @param {Number} $max-vw [1200px] - Viewport maximum
// @return {String} - Valeur clamp()
@function fluid($min, $max, $min-vw: 320px, $max-vw: 1200px) {
  $slope: math.div($max - $min, $max-vw - $min-vw);
  $y-intercept: $min - $slope * $min-vw;
  
  $preferred: calc(#{$y-intercept} + #{$slope * 100vw});
  
  @return clamp(#{$min}, #{$preferred}, #{$max});
}

/ Usage
.card {
  padding: space(4) space(6);
  margin-bottom: space(8);
  font-size: fluid(16px, 24px);
}
```

### Typography

```scss
@use 'sass:math';
@use 'sass:map';

// Type scale ratio
$type-ratio: 1.25 !default; / Major Third

// Base font size
$base-font-size: 16px !default;

// Génère une taille de font basée sur l'échelle
// @param {Number} $step - Étape dans l'échelle
// @return {Number} - Taille calculée
@function type-scale($step) {
  @return $base-font-size * math.pow($type-ratio, $step);
}

// Type scale map
$type-sizes: (
  'xs': type-scale(-2),    / ~10px
  'sm': type-scale(-1),    / ~13px
  'base': type-scale(0),   / 16px
  'lg': type-scale(1),     / 20px
  'xl': type-scale(2),     / 25px
  '2xl': type-scale(3),    / 31px
  '3xl': type-scale(4),    / 39px
  '4xl': type-scale(5),    / 49px
  '5xl': type-scale(6),    / 61px
);

// Obtient une taille de font
// @param {String} $size - Nom de la taille
// @return {Number} - Valeur de la taille
@function font-size($size) {
  @if not map.has-key($type-sizes, $size) {
    @error "Invalid font size: #{$size}";
  }
  
  @return map.get($type-sizes, $size);
}

// Calcule une line-height idéale
// @param {Number} $font-size - Taille de font
// @return {Number} - Line-height calculée
@function ideal-line-height($font-size) {
  / Plus la font est grande, moins la line-height doit être grande
  @if $font-size > 40px {
    @return 1.2;
  } @else if $font-size > 24px {
    @return 1.3;
  } @else if $font-size > 18px {
    @return 1.4;
  } @else {
    @return 1.5;
  }
}

/ Usage
.heading-1 {
  font-size: font-size('4xl');
  line-height: ideal-line-height(font-size('4xl'));
}

.body {
  font-size: font-size('base');
  line-height: ideal-line-height(font-size('base'));
}
```

### Breakpoints

```scss
@use 'sass:map';

// Breakpoints configuration
$breakpoints: (
  'sm': 640px,
  'md': 768px,
  'lg': 1024px,
  'xl': 1280px,
  '2xl': 1536px,
) !default;

// Obtient une valeur de breakpoint
// @param {String} $name - Nom du breakpoint
// @return {Number} - Valeur en pixels
@function breakpoint($name) {
  @if not map.has-key($breakpoints, $name) {
    @error "Invalid breakpoint: #{$name}. Available: #{map.keys($breakpoints)}";
  }
  
  @return map.get($breakpoints, $name);
}

// Obtient le breakpoint suivant
// @param {String} $name - Nom du breakpoint actuel
// @return {Number|null} - Valeur du breakpoint suivant
@function breakpoint-next($name) {
  $keys: map.keys($breakpoints);
  $index: index($keys, $name);
  
  @if $index == null {
    @error "Invalid breakpoint: #{$name}";
  }
  
  @if $index >= length($keys) {
    @return null;
  }
  
  @return map.get($breakpoints, nth($keys, $index + 1));
}

// Vérifie si c'est un breakpoint mobile
// @param {String} $name - Nom du breakpoint
// @return {Boolean} - True si mobile
@function is-mobile($name) {
  @return breakpoint($name) < 768px;
}

/ Mixins utilisant ces fonctions
@mixin media-up($name) {
  @media (min-width: breakpoint($name)) {
    @content;
  }
}

@mixin media-down($name) {
  $next: breakpoint-next($name);
  
  @if $next {
    @media (max-width: $next - 0.02px) {
      @content;
    }
  } @else {
    @content;
  }
}

@mixin media-only($name) {
  $min: breakpoint($name);
  $next: breakpoint-next($name);
  
  @if $next {
    @media (min-width: $min) and (max-width: $next - 0.02px) {
      @content;
    }
  } @else {
    @media (min-width: $min) {
      @content;
    }
  }
}
```

### Z-Index Management

```scss
@use 'sass:map';

// Z-index scale
$z-layers: (
  'base': 0,
  'dropdown': 100,
  'sticky': 200,
  'fixed': 300,
  'modal-backdrop': 400,
  'modal': 500,
  'popover': 600,
  'tooltip': 700,
  'toast': 800,
) !default;

// Obtient un z-index
// @param {String} $layer - Nom de la couche
// @param {Number} $offset [0] - Décalage optionnel
// @return {Number} - Z-index calculé
@function z($layer, $offset: 0) {
  @if not map.has-key($z-layers, $layer) {
    @error "Invalid z-layer: #{$layer}. Available: #{map.keys($z-layers)}";
  }
  
  @return map.get($z-layers, $layer) + $offset;
}

/ Usage
.dropdown {
  z-index: z('dropdown');
}

.modal {
  z-index: z('modal');
}

.tooltip {
  z-index: z('tooltip');
}

/ Avec offset pour empiler
.notification-1 {
  z-index: z('toast');
}

.notification-2 {
  z-index: z('toast', 1);
}
```

### Validation et Erreurs

```scss
@use 'sass:meta';
@use 'sass:list';

// Vérifie le type d'une valeur
// @param {*} $value - Valeur à vérifier
// @param {String} $type - Type attendu
// @param {String} $name - Nom du paramètre (pour le message d'erreur)
@function assert-type($value, $type, $name: 'value') {
  @if meta.type-of($value) != $type {
    @error "#{$name} must be a #{$type}, got #{meta.type-of($value)}";
  }
  
  @return $value;
}

// Vérifie qu'une valeur est dans une liste
// @param {*} $value - Valeur à vérifier
// @param {List} $allowed - Valeurs autorisées
// @param {String} $name - Nom du paramètre
@function assert-one-of($value, $allowed, $name: 'value') {
  @if not list.index($allowed, $value) {
    @error "#{$name} must be one of #{$allowed}, got #{$value}";
  }
  
  @return $value;
}

// Vérifie qu'un nombre est dans une range
// @param {Number} $value - Valeur à vérifier
// @param {Number} $min - Valeur minimum
// @param {Number} $max - Valeur maximum
// @param {String} $name - Nom du paramètre
@function assert-range($value, $min, $max, $name: 'value') {
  @if $value < $min or $value > $max {
    @error "#{$name} must be between #{$min} and #{$max}, got #{$value}";
  }
  
  @return $value;
}

/ Usage dans une fonction
@function opacity-class($level) {
  $level: assert-type($level, 'number', '$level');
  $level: assert-range($level, 0, 100, '$level');
  
  @return #{$level};
}
```

## 📦 Organisation des Fonctions

```scss
/ functions/_index.scss
@forward 'units';
@forward 'colors';
@forward 'spacing';
@forward 'typography';
@forward 'breakpoints';
@forward 'z-index';
@forward 'validation';

/ functions/_units.scss
@use 'sass:math';

@function px-to-rem($pixels, $base: 16px) { /* ... */ }
@function px-to-em($pixels, $context: 16px) { /* ... */ }
@function rem-to-px($rem, $base: 16px) { /* ... */ }

/ functions/_colors.scss
@use 'sass:color';

@function generate-palette($base) { /* ... */ }
@function contrast-color($background) { /* ... */ }
@function alpha($color, $opacity) { /* ... */ }
@function tint($color, $percentage) { /* ... */ }
@function shade($color, $percentage) { /* ... */ }

/ Utilisation
/ styles/main.scss
@use '../functions' as fn;

.element {
  font-size: fn.px-to-rem(18);
  padding: fn.space(4);
  background: fn.alpha(#3b82f6, 0.5);
}
```
