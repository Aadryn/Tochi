---
description: CSS Flexbox - Flex containers, items, alignment, distribution, responsive patterns
name: CSS_Flexbox
applyTo: "**/*.css,**/*.scss,**/*.vue"
---

# CSS Flexbox

Guide complet pour CSS Flexible Box Layout.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** flexbox pour des layouts 2D complexes (utiliser Grid)
- **Ne mélange jamais** flex-grow/shrink/basis sans comprendre leur interaction
- **N'utilise jamais** margin: auto sans comprendre son comportement dans flex
- **Ne fixe jamais** flex-basis à 0 avec du contenu qui a une largeur intrinsèque
- **N'oublie jamais** min-width: 0 pour éviter les débordements

## ✅ À FAIRE

- **Utilise toujours** gap au lieu de margins pour l'espacement
- **Utilise toujours** flex shorthand quand approprié
- **Utilise toujours** min-width: 0 sur les items avec overflow
- **Documente toujours** les layouts flexbox complexes
- **Teste toujours** le comportement avec différentes tailles de contenu

## 📌 Flex Container

### Propriétés de Base

```css
/* Activer Flexbox */
.container {
  display: flex;
  /* ou inline-flex pour inline */
  display: inline-flex;
}

/* Direction */
.row {
  flex-direction: row;          /* Défaut: horizontal gauche → droite */
}

.row-reverse {
  flex-direction: row-reverse;  /* Horizontal droite → gauche */
}

.column {
  flex-direction: column;       /* Vertical haut → bas */
}

.column-reverse {
  flex-direction: column-reverse; /* Vertical bas → haut */
}

/* Wrap */
.nowrap {
  flex-wrap: nowrap;           /* Défaut: pas de wrap */
}

.wrap {
  flex-wrap: wrap;             /* Wrap sur plusieurs lignes */
}

.wrap-reverse {
  flex-wrap: wrap-reverse;     /* Wrap inversé */
}

/* Shorthand flex-flow */
.flex-flow {
  flex-flow: row wrap;         /* direction wrap */
  flex-flow: column nowrap;
}
```

### Alignement Principal (justify-content)

```css
.container {
  display: flex;
  
  /* Alignement sur l'axe principal (horizontal par défaut) */
  justify-content: flex-start;    /* Début (défaut) */
  justify-content: flex-end;      /* Fin */
  justify-content: center;        /* Centre */
  justify-content: space-between; /* Espace entre les items */
  justify-content: space-around;  /* Espace autour des items */
  justify-content: space-evenly;  /* Espace égal partout */
}

/* Exemples visuels */

/* flex-start */
/* [Item1][Item2][Item3]                    */

/* flex-end */
/*                     [Item1][Item2][Item3] */

/* center */
/*           [Item1][Item2][Item3]           */

/* space-between */
/* [Item1]        [Item2]        [Item3] */

/* space-around */
/*  [Item1]     [Item2]     [Item3]  */

/* space-evenly */
/*   [Item1]    [Item2]    [Item3]   */
```

### Alignement Transversal (align-items)

```css
.container {
  display: flex;
  height: 200px;
  
  /* Alignement sur l'axe transversal (vertical par défaut) */
  align-items: stretch;    /* Étire (défaut) */
  align-items: flex-start; /* Haut */
  align-items: flex-end;   /* Bas */
  align-items: center;     /* Centre vertical */
  align-items: baseline;   /* Aligné sur la baseline du texte */
}
```

### Alignement Multi-lignes (align-content)

```css
.container {
  display: flex;
  flex-wrap: wrap;
  height: 400px;
  
  /* Distribution des lignes quand wrap */
  align-content: flex-start;    /* Lignes groupées en haut */
  align-content: flex-end;      /* Lignes groupées en bas */
  align-content: center;        /* Lignes centrées */
  align-content: space-between; /* Espace entre les lignes */
  align-content: space-around;  /* Espace autour des lignes */
  align-content: space-evenly;  /* Espace égal */
  align-content: stretch;       /* Lignes étirées (défaut) */
}
```

### Gap (Espacement)

```css
.container {
  display: flex;
  gap: 20px;           /* Gap uniforme */
  gap: 20px 30px;      /* row-gap column-gap */
  
  /* Propriétés séparées */
  row-gap: 20px;
  column-gap: 30px;
}
```

## 📏 Flex Items

### Propriétés Fondamentales

```css
/* flex-grow : capacité à grandir */
.item {
  flex-grow: 0;  /* Défaut: ne grandit pas */
  flex-grow: 1;  /* Prend l'espace disponible */
  flex-grow: 2;  /* Prend 2x plus que flex-grow: 1 */
}

/* flex-shrink : capacité à rétrécir */
.item {
  flex-shrink: 1;  /* Défaut: peut rétrécir */
  flex-shrink: 0;  /* Ne rétrécit jamais */
  flex-shrink: 2;  /* Rétrécit 2x plus */
}

/* flex-basis : taille initiale */
.item {
  flex-basis: auto;   /* Défaut: basé sur width/height */
  flex-basis: 0;      /* Ignore la taille intrinsèque */
  flex-basis: 200px;  /* Taille fixe de départ */
  flex-basis: 25%;    /* Pourcentage du container */
  flex-basis: content; /* Basé sur le contenu */
}
```

### Shorthand flex

```css
/* flex: grow shrink basis */

.item {
  flex: 0 1 auto;  /* Défaut complet */
  flex: auto;      /* = 1 1 auto (flexible) */
  flex: none;      /* = 0 0 auto (rigide) */
  flex: 1;         /* = 1 1 0% (remplit l'espace) */
  flex: 2;         /* = 2 1 0% (prend 2x plus) */
  flex: 1 0 200px; /* Grandit à partir de 200px, ne rétrécit pas */
}

/* Valeurs communes et leur signification */

/* flex: 1 */
/* Prend tout l'espace disponible, taille de base = 0 */
.fill-space {
  flex: 1;
}

/* flex: 0 0 auto */
/* Taille fixe basée sur le contenu */
.fixed-size {
  flex: none;
}

/* flex: 0 1 auto */
/* Peut rétrécir mais ne grandit pas */
.shrink-only {
  flex: 0 1 auto;
}

/* flex: 1 0 300px */
/* Base de 300px, peut grandir, ne rétrécit pas */
.min-300 {
  flex: 1 0 300px;
}
```

### Alignement Individuel (align-self)

```css
.item {
  align-self: auto;       /* Hérite de align-items */
  align-self: flex-start; /* Aligné en haut */
  align-self: flex-end;   /* Aligné en bas */
  align-self: center;     /* Centré */
  align-self: baseline;   /* Sur la baseline */
  align-self: stretch;    /* Étiré */
}
```

### Ordre

```css
.item {
  order: 0;  /* Défaut */
  order: -1; /* Avant les autres */
  order: 1;  /* Après les autres */
}

/* Les items sont triés par ordre croissant */
.first  { order: -1; }
.normal { order: 0; }
.last   { order: 1; }
```

## 🎯 Patterns Courants

### Centrage Parfait

```css
/* Centrage horizontal et vertical */
.center-both {
  display: flex;
  justify-content: center;
  align-items: center;
}

/* Alternative avec place-items (CSS shorthand) */
.center-both-alt {
  display: flex;
  place-content: center;
}
```

### Header avec Logo et Navigation

```css
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
}

.logo {
  flex-shrink: 0; /* Logo ne rétrécit jamais */
}

.nav {
  display: flex;
  gap: 24px;
}

.header-actions {
  display: flex;
  gap: 12px;
  flex-shrink: 0;
}

/* Responsive */
@media (max-width: 768px) {
  .header {
    flex-wrap: wrap;
  }
  
  .nav {
    order: 3;
    width: 100%;
    margin-top: 16px;
  }
}
```

### Sidebar Layout

```css
.layout {
  display: flex;
  min-height: 100vh;
}

.sidebar {
  flex: 0 0 250px; /* Largeur fixe */
  /* ou avec min/max */
  flex: 0 0 auto;
  width: clamp(200px, 20vw, 300px);
}

.main {
  flex: 1;        /* Prend le reste */
  min-width: 0;   /* Permet le shrink avec overflow */
}

/* Sidebar collapsible */
.sidebar.collapsed {
  flex-basis: 64px;
}
```

### Card Layout

```css
.card {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.card-header {
  flex-shrink: 0;
  padding: 16px;
}

.card-content {
  flex: 1;        /* Prend l'espace disponible */
  padding: 16px;
  overflow-y: auto;
}

.card-footer {
  flex-shrink: 0;
  padding: 16px;
  margin-top: auto; /* Pousse vers le bas */
}
```

### Media Object

```css
.media {
  display: flex;
  gap: 16px;
}

.media-image {
  flex-shrink: 0;
  width: 64px;
  height: 64px;
  border-radius: 50%;
}

.media-content {
  flex: 1;
  min-width: 0; /* Pour text-overflow: ellipsis */
}

.media-content h3 {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
```

### Navigation Pills/Tabs

```css
.tabs {
  display: flex;
  gap: 4px;
  border-bottom: 1px solid #e5e7eb;
}

.tab {
  flex: 0 0 auto; /* Taille basée sur le contenu */
  padding: 12px 16px;
  border: none;
  background: transparent;
  cursor: pointer;
}

.tab.active {
  border-bottom: 2px solid #3b82f6;
  margin-bottom: -1px;
}

/* Tabs égales */
.tabs.equal {
  .tab {
    flex: 1;
    text-align: center;
  }
}
```

### Input Group

```css
.input-group {
  display: flex;
}

.input-group-prepend,
.input-group-append {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  padding: 0 12px;
  background: #f3f4f6;
  border: 1px solid #d1d5db;
}

.input-group-prepend {
  border-right: none;
  border-radius: 4px 0 0 4px;
}

.input-group-append {
  border-left: none;
  border-radius: 0 4px 4px 0;
}

.input-group input {
  flex: 1;
  min-width: 0;
  padding: 8px 12px;
  border: 1px solid #d1d5db;
}

.input-group input:first-child {
  border-radius: 4px 0 0 4px;
}

.input-group input:last-child {
  border-radius: 0 4px 4px 0;
}
```

### Sticky Footer

```css
.page {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.page-header {
  flex-shrink: 0;
}

.page-main {
  flex: 1;
}

.page-footer {
  flex-shrink: 0;
  margin-top: auto; /* Redondant avec flex: 1 sur main, mais explicite */
}
```

### Equal Height Columns

```css
.row {
  display: flex;
  gap: 24px;
}

.column {
  flex: 1;
  display: flex;
  flex-direction: column;
}

/* Les colonnes ont automatiquement la même hauteur */
/* Le contenu interne peut être flexible */
.column-content {
  flex: 1;
}
```

## 🔄 Patterns Responsives

### Stack to Row

```css
.stack-to-row {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

@media (min-width: 768px) {
  .stack-to-row {
    flex-direction: row;
    align-items: center;
  }
}
```

### Wrap avec Minimum

```css
.flex-wrap-min {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
}

.flex-wrap-min > * {
  flex: 1 1 200px; /* Minimum 200px avant wrap */
  max-width: 100%;
}
```

### Holy Grail avec Flexbox

```css
.holy-grail {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.holy-grail-header,
.holy-grail-footer {
  flex-shrink: 0;
}

.holy-grail-body {
  display: flex;
  flex: 1;
}

.holy-grail-nav {
  flex: 0 0 200px;
  order: -1; /* Avant le main dans le DOM mais à gauche */
}

.holy-grail-main {
  flex: 1;
  min-width: 0;
}

.holy-grail-aside {
  flex: 0 0 200px;
}

@media (max-width: 768px) {
  .holy-grail-body {
    flex-direction: column;
  }
  
  .holy-grail-nav,
  .holy-grail-aside {
    flex-basis: auto;
  }
}
```

## ⚠️ Pièges Courants

### Overflow et min-width

```css
/* ❌ PROBLÈME : Le texte déborde */
.container {
  display: flex;
}

.item {
  flex: 1;
  /* Le texte long ne wrap pas et déborde */
}

.item p {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  /* Ne fonctionne pas sans min-width: 0 */
}

/* ✅ SOLUTION */
.item {
  flex: 1;
  min-width: 0; /* Permet au flex item de rétrécir sous sa taille min */
}
```

### flex-basis vs width

```css
/* flex-basis est préféré dans un contexte flex */

/* ❌ Peut causer des conflits */
.item {
  width: 200px;
  flex-grow: 1;
}

/* ✅ Plus prévisible */
.item {
  flex: 1 0 200px; /* grow shrink basis */
}
```

### margin: auto dans Flexbox

```css
.container {
  display: flex;
}

/* margin: auto absorbe l'espace disponible */
.item-pushed-right {
  margin-left: auto; /* Pousse vers la droite */
}

.item-centered {
  margin: 0 auto; /* Centre horizontalement */
}

/* Utile pour spacer des éléments */
.header {
  display: flex;
  align-items: center;
}

.logo { /* Premier élément */ }

.nav {
  margin-left: auto; /* Pousse la nav vers la droite */
}

.user-menu { /* Après la nav */ }
```

### Nested Flex Containers

```css
/* Attention à la propagation des styles */

.outer {
  display: flex;
}

.inner {
  display: flex;
  flex: 1;
  /* L'inner est un flex item ET un flex container */
  
  /* Problème potentiel : les enfants de inner 
     ne sont pas affectés par outer */
}

/* Pour que les petits-enfants participent au layout global,
   utiliser Grid avec subgrid plutôt */
```

## 🎨 Patterns Avancés

### Flexbox avec Order pour Réorganisation

```css
.reorderable {
  display: flex;
  flex-wrap: wrap;
}

/* Mobile : ordre du DOM */
/* Desktop : réorganisé */
@media (min-width: 768px) {
  .reorderable .important {
    order: -1;
  }
  
  .reorderable .secondary {
    order: 1;
  }
}
```

### Ratio d'Espace

```css
.space-ratio {
  display: flex;
}

/* Ratio 1:2:1 */
.left   { flex: 1; }
.center { flex: 2; }
.right  { flex: 1; }

/* Ratio personnalisé avec CSS custom properties */
.space-ratio-custom {
  display: flex;
}

.space-ratio-custom > * {
  flex: var(--ratio, 1);
}

/* Usage */
/* <div class="space-ratio-custom">
     <div style="--ratio: 1">1</div>
     <div style="--ratio: 3">3</div>
     <div style="--ratio: 2">2</div>
   </div> */
```

### Intrinsic Sizing

```css
.intrinsic {
  display: flex;
}

/* Taille basée sur le contenu avec max */
.intrinsic-item {
  flex: 0 1 auto;
  max-width: 300px;
}

/* Taille basée sur le contenu avec min */
.intrinsic-item-min {
  flex: 1 0 auto;
  min-width: 150px;
}
```

### Alignment avec Pseudo-elements

```css
/* Centrer avec nombre impair d'éléments */
.center-odd {
  display: flex;
  justify-content: space-between;
}

.center-odd::before,
.center-odd::after {
  content: "";
  flex: 1;
}

.center-odd .center-item {
  flex: 0 0 auto;
}
```

## 🔧 CSS Custom Properties

```css
:root {
  --flex-gap: 16px;
  --flex-direction: row;
}

.flex-container {
  display: flex;
  flex-direction: var(--flex-direction);
  gap: var(--flex-gap);
}

/* Modifiable selon le contexte */
.vertical-layout {
  --flex-direction: column;
  --flex-gap: 24px;
}

/* Responsive */
@media (max-width: 768px) {
  :root {
    --flex-direction: column;
    --flex-gap: 12px;
  }
}
```

## ⚡ Performance

```css
/* Éviter le reflow */
.flex-container {
  contain: layout; /* Isole le layout */
}

/* Éviter les recalculs */
.stable-flex {
  /* Préférer flex-basis fixe quand possible */
  flex: 0 0 200px;
  /* Plutôt que des calculs dynamiques répétés */
}

/* Animation optimisée */
.flex-item-animated {
  /* Animer transform plutôt que flex properties */
  transition: transform 0.3s ease;
  will-change: transform;
}
```

## 📊 Flexbox vs Grid

```css
/* Utiliser Flexbox pour : */
/* - Layouts 1D (ligne ou colonne) */
/* - Distribution d'espace sur un axe */
/* - Navigation, barres d'outils */
/* - Alignement de contenus */

.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

/* Utiliser Grid pour : */
/* - Layouts 2D (lignes et colonnes) */
/* - Placements précis */
/* - Overlays et superpositions */
/* - Layouts de page complets */

.page-layout {
  display: grid;
  grid-template-columns: 200px 1fr;
  grid-template-rows: auto 1fr auto;
}

/* Combiner les deux */
.hybrid-layout {
  display: grid;
  grid-template-columns: 200px 1fr;
}

.sidebar {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.main-content {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
}
```
