---
description: CSS Responsive - Media queries, mobile-first, breakpoints, container queries, fluid design
name: CSS_Responsive
applyTo: "**/*.css,**/*.scss,**/*.vue"
---

# CSS Responsive Design

Guide complet pour créer des interfaces adaptatives et responsive.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** de largeurs fixes en pixels pour les conteneurs principaux
- **Ne cible jamais** des appareils spécifiques - cible des breakpoints de contenu
- **N'écris jamais** de media queries desktop-first puis override mobile
- **N'utilise jamais** `!important` pour forcer des styles responsive
- **Ne néglige jamais** les états tactiles sur mobile
- **N'oublie jamais** de tester sur de vrais appareils

## ✅ À FAIRE

- **Adopte toujours** l'approche mobile-first
- **Utilise toujours** des unités relatives (rem, %, vw/vh)
- **Définis toujours** des breakpoints basés sur le contenu
- **Teste toujours** avec les DevTools et appareils réels
- **Utilise toujours** `min-width` pour les media queries mobile-first
- **Pense toujours** aux interactions tactiles (44px minimum pour touch targets)

## 📱 Approche Mobile-First

### Principe Fondamental

```css
/* ✅ MOBILE-FIRST : Styles de base pour mobile, puis enrichissement */

/* 1. Styles mobiles (par défaut) */
.container {
  padding: 1rem;
  font-size: 1rem;
}

.grid {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

/* 2. Tablette (min-width) */
@media (min-width: 768px) {
  .container {
    padding: 1.5rem;
  }
  
  .grid {
    flex-direction: row;
    flex-wrap: wrap;
  }
  
  .grid > * {
    flex: 0 0 calc(50% - 0.5rem);
  }
}

/* 3. Desktop (min-width) */
@media (min-width: 1024px) {
  .container {
    padding: 2rem;
    max-width: 1200px;
    margin: 0 auto;
  }
  
  .grid > * {
    flex: 0 0 calc(33.333% - 0.67rem);
  }
}

/* ❌ DESKTOP-FIRST (à éviter) */
.bad-example {
  width: 1200px;
  padding: 2rem;
}

@media (max-width: 1024px) {
  .bad-example {
    width: 100%;
    padding: 1.5rem;
  }
}

@media (max-width: 768px) {
  .bad-example {
    padding: 1rem;
  }
}
```

## 📏 Système de Breakpoints

### Breakpoints Standards

```css
:root {
  /* Définition des breakpoints */
  --bp-xs: 0;        /* Très petit mobile */
  --bp-sm: 576px;    /* Mobile paysage */
  --bp-md: 768px;    /* Tablette portrait */
  --bp-lg: 992px;    /* Tablette paysage / petit desktop */
  --bp-xl: 1200px;   /* Desktop standard */
  --bp-2xl: 1400px;  /* Grand écran */
}

/* 
 * BREAKPOINTS MOBILE-FIRST
 * Toujours utiliser min-width pour ajouter des styles
 */

/* Extra small (xs): < 576px - Styles par défaut */
.element {
  /* Styles mobile de base */
}

/* Small (sm): ≥ 576px */
@media (min-width: 576px) {
  .element {
    /* Ajustements pour mobile large/paysage */
  }
}

/* Medium (md): ≥ 768px */
@media (min-width: 768px) {
  .element {
    /* Styles tablette */
  }
}

/* Large (lg): ≥ 992px */
@media (min-width: 992px) {
  .element {
    /* Styles petit desktop */
  }
}

/* Extra large (xl): ≥ 1200px */
@media (min-width: 1200px) {
  .element {
    /* Styles desktop standard */
  }
}

/* Extra extra large (2xl): ≥ 1400px */
@media (min-width: 1400px) {
  .element {
    /* Styles grand écran */
  }
}
```

### Media Queries Utiles

```css
/* === ORIENTATION === */
@media (orientation: portrait) {
  /* Orientation portrait */
}

@media (orientation: landscape) {
  /* Orientation paysage */
}

/* === PRÉFÉRENCES UTILISATEUR === */
@media (prefers-reduced-motion: reduce) {
  /* Désactiver les animations */
  * {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}

@media (prefers-color-scheme: dark) {
  /* Thème sombre automatique */
}

@media (prefers-contrast: high) {
  /* Contraste élevé pour accessibilité */
}

/* === HOVER DISPONIBLE === */
@media (hover: hover) {
  /* Effets hover uniquement si souris disponible */
  .button:hover {
    background-color: var(--color-primary-hover);
  }
}

@media (hover: none) {
  /* Alternatives pour écrans tactiles */
  .button:active {
    background-color: var(--color-primary-hover);
  }
}

/* === POINTEUR PRÉCIS === */
@media (pointer: fine) {
  /* Souris précise - éléments plus petits possibles */
  .small-button {
    padding: 4px 8px;
  }
}

@media (pointer: coarse) {
  /* Écran tactile - éléments plus grands */
  .small-button {
    padding: 12px 16px;
    min-height: 44px;
  }
}

/* === ASPECT RATIO === */
@media (min-aspect-ratio: 16/9) {
  /* Écran très large */
}

@media (max-aspect-ratio: 4/3) {
  /* Écran plus carré */
}

/* === COMBINAISONS === */
@media (min-width: 768px) and (orientation: landscape) {
  /* Tablette en paysage */
}

@media (min-width: 992px) and (hover: hover) {
  /* Desktop avec souris */
}
```

## 📦 Container Queries

### Configuration des Container Queries

```css
/* 1. Définir le contexte de containment */
.card-container {
  container-type: inline-size;
  container-name: card;
}

/* 2. Styles de base (plus petit) */
.card {
  display: flex;
  flex-direction: column;
  padding: 1rem;
}

.card__image {
  width: 100%;
  aspect-ratio: 16/9;
  object-fit: cover;
}

.card__content {
  padding: 1rem 0;
}

/* 3. Container query pour largeur moyenne */
@container card (min-width: 300px) {
  .card {
    flex-direction: row;
  }
  
  .card__image {
    width: 40%;
    aspect-ratio: 1;
  }
  
  .card__content {
    flex: 1;
    padding: 0 1rem;
  }
}

/* 4. Container query pour grande largeur */
@container card (min-width: 500px) {
  .card__image {
    width: 30%;
  }
  
  .card__title {
    font-size: 1.5rem;
  }
}
```

### Container Queries vs Media Queries

```css
/* 
 * MEDIA QUERIES : basées sur la fenêtre (viewport)
 * Utiliser pour les layouts globaux
 */
@media (min-width: 768px) {
  .main-layout {
    display: grid;
    grid-template-columns: 250px 1fr;
  }
}

/*
 * CONTAINER QUERIES : basées sur le conteneur parent
 * Utiliser pour les composants réutilisables
 */
.widget-container {
  container-type: inline-size;
}

@container (min-width: 400px) {
  .widget {
    /* S'adapte à son conteneur, pas à la fenêtre */
    flex-direction: row;
  }
}
```

## 📐 Grilles Responsive

### CSS Grid Responsive

```css
/* Grille auto-responsive avec minmax */
.auto-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(250px, 100%), 1fr));
  gap: 1rem;
}

/* Grille avec changement de colonnes explicite */
.explicit-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
}

@media (min-width: 576px) {
  .explicit-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 992px) {
  .explicit-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (min-width: 1200px) {
  .explicit-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}

/* Grille avec zones nommées responsive */
.page-layout {
  display: grid;
  grid-template-areas:
    "header"
    "nav"
    "main"
    "aside"
    "footer";
  gap: 1rem;
}

@media (min-width: 768px) {
  .page-layout {
    grid-template-columns: 200px 1fr;
    grid-template-areas:
      "header header"
      "nav main"
      "nav aside"
      "footer footer";
  }
}

@media (min-width: 1024px) {
  .page-layout {
    grid-template-columns: 200px 1fr 250px;
    grid-template-areas:
      "header header header"
      "nav main aside"
      "footer footer footer";
  }
}

.header { grid-area: header; }
.nav { grid-area: nav; }
.main { grid-area: main; }
.aside { grid-area: aside; }
.footer { grid-area: footer; }
```

### Flexbox Responsive

```css
/* Flex wrap responsive */
.flex-container {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
}

.flex-item {
  /* Mobile: 100% */
  flex: 1 1 100%;
}

@media (min-width: 576px) {
  .flex-item {
    /* 2 colonnes */
    flex: 1 1 calc(50% - 0.5rem);
  }
}

@media (min-width: 992px) {
  .flex-item {
    /* 3 colonnes */
    flex: 1 1 calc(33.333% - 0.67rem);
  }
}

/* Navigation responsive avec flex */
.nav {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

@media (min-width: 768px) {
  .nav {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
  }
}
```

## 🔤 Typographie Fluide

### Clamp pour Tailles Fluides

```css
/* Typographie fluide avec clamp() */
:root {
  /* Taille minimale, préférée, maximale */
  --font-size-fluid-sm: clamp(0.875rem, 0.8rem + 0.25vw, 1rem);
  --font-size-fluid-base: clamp(1rem, 0.9rem + 0.35vw, 1.125rem);
  --font-size-fluid-lg: clamp(1.125rem, 1rem + 0.5vw, 1.5rem);
  --font-size-fluid-xl: clamp(1.5rem, 1.2rem + 1vw, 2.5rem);
  --font-size-fluid-2xl: clamp(2rem, 1.5rem + 2vw, 4rem);
}

h1 {
  font-size: var(--font-size-fluid-2xl);
}

h2 {
  font-size: var(--font-size-fluid-xl);
}

p {
  font-size: var(--font-size-fluid-base);
}

/* Espacement fluide */
:root {
  --spacing-fluid-sm: clamp(0.5rem, 0.4rem + 0.5vw, 1rem);
  --spacing-fluid-md: clamp(1rem, 0.8rem + 1vw, 2rem);
  --spacing-fluid-lg: clamp(2rem, 1.5rem + 2vw, 4rem);
}

section {
  padding: var(--spacing-fluid-lg) var(--spacing-fluid-md);
}
```

### Échelle Typographique Modulaire

```css
:root {
  --base-size: 1rem;
  --ratio: 1.25; /* Major third */
  
  /* Calculé à partir du ratio */
  --size-sm: calc(var(--base-size) / var(--ratio));
  --size-base: var(--base-size);
  --size-lg: calc(var(--base-size) * var(--ratio));
  --size-xl: calc(var(--base-size) * var(--ratio) * var(--ratio));
  --size-2xl: calc(var(--base-size) * var(--ratio) * var(--ratio) * var(--ratio));
  --size-3xl: calc(var(--base-size) * var(--ratio) * var(--ratio) * var(--ratio) * var(--ratio));
}

/* Ajustement du ratio sur mobile */
@media (max-width: 576px) {
  :root {
    --ratio: 1.2; /* Ratio plus petit sur mobile */
  }
}
```

## 📷 Images Responsive

### Images Fluides

```css
/* Image de base responsive */
img {
  max-width: 100%;
  height: auto;
  display: block;
}

/* Image avec aspect ratio fixe */
.image-container {
  position: relative;
  width: 100%;
  aspect-ratio: 16/9;
  overflow: hidden;
}

.image-container img {
  position: absolute;
  width: 100%;
  height: 100%;
  object-fit: cover;
}

/* Object-fit responsive */
.hero-image {
  width: 100%;
  height: 200px;
  object-fit: cover;
  object-position: center;
}

@media (min-width: 768px) {
  .hero-image {
    height: 400px;
  }
}

@media (min-width: 1024px) {
  .hero-image {
    height: 500px;
  }
}
```

### Picture Element pour Art Direction

```html
<!-- HTML pour images responsive -->
<picture>
  <!-- Mobile: image recadrée portrait -->
  <source 
    media="(max-width: 576px)" 
    srcset="image-mobile.jpg"
  >
  <!-- Tablette: image moyenne -->
  <source 
    media="(max-width: 992px)" 
    srcset="image-tablet.jpg"
  >
  <!-- Desktop: image complète -->
  <img 
    src="image-desktop.jpg" 
    alt="Description"
    loading="lazy"
  >
</picture>
```

```css
/* Styles pour picture responsive */
picture {
  display: block;
  width: 100%;
}

picture img {
  width: 100%;
  height: auto;
  display: block;
}
```

## 📱 Patterns Responsive Courants

### Navigation Mobile (Hamburger)

```css
/* Navigation de base */
.nav {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
}

.nav__toggle {
  display: block;
  background: none;
  border: none;
  padding: 0.5rem;
  cursor: pointer;
}

.nav__menu {
  position: fixed;
  top: 60px;
  left: 0;
  right: 0;
  background: var(--color-surface);
  transform: translateX(-100%);
  transition: transform 0.3s ease;
  padding: 1rem;
}

.nav__menu.is-open {
  transform: translateX(0);
}

.nav__menu ul {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  list-style: none;
  padding: 0;
  margin: 0;
}

/* Desktop: navigation horizontale */
@media (min-width: 768px) {
  .nav__toggle {
    display: none;
  }
  
  .nav__menu {
    position: static;
    transform: none;
    padding: 0;
    background: transparent;
  }
  
  .nav__menu ul {
    flex-direction: row;
  }
}
```

### Sidebar Responsive

```css
.layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.sidebar {
  /* Mobile: masqué ou drawer */
  position: fixed;
  top: 0;
  left: 0;
  bottom: 0;
  width: 280px;
  transform: translateX(-100%);
  transition: transform 0.3s ease;
  background: var(--color-surface);
  z-index: var(--z-modal);
  overflow-y: auto;
}

.sidebar.is-open {
  transform: translateX(0);
}

.sidebar-overlay {
  display: none;
  position: fixed;
  inset: 0;
  background: var(--color-overlay);
  z-index: calc(var(--z-modal) - 1);
}

.sidebar.is-open + .sidebar-overlay {
  display: block;
}

.main-content {
  flex: 1;
  padding: 1rem;
}

/* Desktop: sidebar fixe */
@media (min-width: 992px) {
  .layout {
    flex-direction: row;
  }
  
  .sidebar {
    position: sticky;
    top: 0;
    height: 100vh;
    transform: none;
    flex-shrink: 0;
  }
  
  .sidebar-overlay {
    display: none !important;
  }
  
  .main-content {
    padding: 2rem;
  }
}
```

### Table Responsive

```css
/* Table scrollable horizontalement */
.table-container {
  width: 100%;
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}

.table {
  width: 100%;
  min-width: 600px; /* Largeur minimum */
  border-collapse: collapse;
}

/* Alternative: Table en cartes sur mobile */
@media (max-width: 768px) {
  .table--cards thead {
    display: none;
  }
  
  .table--cards tbody tr {
    display: block;
    margin-bottom: 1rem;
    padding: 1rem;
    background: var(--color-surface);
    border-radius: var(--radius-md);
    box-shadow: var(--shadow-sm);
  }
  
  .table--cards tbody td {
    display: flex;
    justify-content: space-between;
    padding: 0.5rem 0;
    border-bottom: 1px solid var(--color-border);
  }
  
  .table--cards tbody td::before {
    content: attr(data-label);
    font-weight: 600;
    margin-right: 1rem;
  }
  
  .table--cards tbody td:last-child {
    border-bottom: none;
  }
}
```

## ⚠️ Bonnes Pratiques

### Touch Targets

```css
/* Minimum 44x44px pour les éléments tactiles */
.touch-target {
  min-width: 44px;
  min-height: 44px;
  padding: 12px;
}

/* Boutons avec zone de toucher étendue */
.button {
  position: relative;
  padding: 8px 16px;
}

/* Zone de toucher étendue invisible */
.button::before {
  content: '';
  position: absolute;
  top: -8px;
  right: -8px;
  bottom: -8px;
  left: -8px;
}

/* Espacement suffisant entre éléments cliquables */
.nav-list {
  display: flex;
  gap: 8px; /* Minimum 8px entre éléments tactiles */
}
```

### Performance Mobile

```css
/* Éviter les animations coûteuses sur mobile */
@media (max-width: 768px) {
  .heavy-animation {
    animation: none;
  }
}

/* Réduire pour prefers-reduced-motion */
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
    scroll-behavior: auto !important;
  }
}

/* GPU acceleration pour les transformations */
.smooth-transform {
  will-change: transform;
  transform: translateZ(0);
}
```

### Viewport Units

```css
/* Utiliser les nouvelles unités viewport */
.full-height {
  /* Ancien: 100vh peut avoir des problèmes sur mobile */
  height: 100vh;
  
  /* Nouveau: prend en compte la barre d'adresse mobile */
  height: 100dvh; /* Dynamic viewport height */
}

/* Small viewport (avec barres d'UI) */
.min-height {
  min-height: 100svh;
}

/* Large viewport (sans barres d'UI) */
.max-height {
  max-height: 100lvh;
}

/* Largeur avec fallback */
.full-width {
  width: 100vw;
  width: 100dvw;
}
```
