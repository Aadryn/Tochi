---
description: CSS Grid - Grid Layout, Templates, Areas, Alignment, Responsive Grids, Subgrid
name: CSS_Grid
applyTo: "**/*.css,**/*.scss,**/*.vue"
---

# CSS Grid Layout

Guide complet pour CSS Grid Layout.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** float ou inline-block pour les layouts modernes
- **Ne mélange jamais** grid-template-rows/columns avec grid-template-areas sans cohérence
- **N'utilise jamais** des nombres magiques pour les colonnes (1, 2, 3 sans explication)
- **Ne fixe jamais** les hauteurs de grid quand auto ou min-content suffit
- **N'oublie jamais** les fallbacks pour les navigateurs anciens si nécessaire

## ✅ À FAIRE

- **Utilise toujours** des noms explicites pour les lignes et areas
- **Utilise toujours** minmax() pour les colonnes flexibles
- **Utilise toujours** auto-fit/auto-fill pour les grids responsives
- **Documente toujours** les layouts complexes avec des commentaires
- **Utilise toujours** gap au lieu de margins pour l'espacement

## 📌 Grid Container

### Définition de Base

```css
/* Activer Grid */
.container {
  display: grid;
  /* ou inline-grid pour inline */
  display: inline-grid;
}
```

### Grid Template Columns

```css
/* Colonnes fixes */
.grid-fixed {
  display: grid;
  grid-template-columns: 200px 200px 200px;
}

/* Colonnes fractionnelles (fr) */
.grid-fractions {
  display: grid;
  grid-template-columns: 1fr 2fr 1fr; /* 25% 50% 25% */
}

/* Colonnes mixtes */
.grid-mixed {
  display: grid;
  grid-template-columns: 200px 1fr 100px;
  /* Sidebar fixe | Contenu flexible | Aside fixe */
}

/* Repeat */
.grid-repeat {
  display: grid;
  grid-template-columns: repeat(3, 1fr); /* 3 colonnes égales */
  grid-template-columns: repeat(4, 100px); /* 4 colonnes de 100px */
  grid-template-columns: repeat(3, 1fr 2fr); /* Pattern répété */
}

/* minmax() pour colonnes adaptatives */
.grid-minmax {
  display: grid;
  grid-template-columns: minmax(200px, 1fr) 2fr minmax(100px, 300px);
  /* Sidebar 200-flex | Content 2fr | Aside 100-300px */
}

/* auto-fit : colonnes qui remplissent l'espace */
.grid-auto-fit {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  /* Colonnes qui wrap automatiquement */
}

/* auto-fill : garde l'espace pour les colonnes vides */
.grid-auto-fill {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
}

/* fit-content() */
.grid-fit-content {
  display: grid;
  grid-template-columns: fit-content(300px) 1fr fit-content(200px);
  /* S'adapte au contenu jusqu'à max */
}
```

### Grid Template Rows

```css
/* Rows fixes */
.grid-rows-fixed {
  display: grid;
  grid-template-rows: 100px 1fr 50px;
  /* Header 100px | Content flexible | Footer 50px */
}

/* Rows auto */
.grid-rows-auto {
  display: grid;
  grid-template-rows: auto 1fr auto;
  /* Header auto | Content flex | Footer auto */
}

/* Rows min-content / max-content */
.grid-rows-content {
  display: grid;
  grid-template-rows: min-content 1fr min-content;
  /* Header minimal | Content flex | Footer minimal */
}

/* Rows avec minmax */
.grid-rows-minmax {
  display: grid;
  grid-template-rows: minmax(100px, auto) 1fr minmax(50px, auto);
}
```

### Grid Template Areas

```css
/* Layout avec areas nommées */
.layout {
  display: grid;
  grid-template-columns: 250px 1fr 200px;
  grid-template-rows: auto 1fr auto;
  grid-template-areas:
    "header  header  header"
    "sidebar content aside"
    "footer  footer  footer";
  min-height: 100vh;
}

.header  { grid-area: header; }
.sidebar { grid-area: sidebar; }
.content { grid-area: content; }
.aside   { grid-area: aside; }
.footer  { grid-area: footer; }

/* Areas avec cellules vides (.) */
.layout-with-gaps {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  grid-template-rows: auto 1fr auto;
  grid-template-areas:
    "header header header"
    "main   .      sidebar"
    "footer footer footer";
}

/* Shorthand grid-template */
.layout-shorthand {
  display: grid;
  grid-template:
    "header header header" auto
    "sidebar content aside" 1fr
    "footer footer footer" auto
    / 250px 1fr 200px;
}
```

### Gap (Espacement)

```css
.grid-gap {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  
  /* Gap uniforme */
  gap: 20px;
  
  /* Row gap et column gap différents */
  gap: 20px 30px; /* row-gap column-gap */
  
  /* Propriétés séparées */
  row-gap: 20px;
  column-gap: 30px;
}
```

## 📏 Grid Items

### Placement Explicite

```css
/* Par numéros de ligne */
.item {
  grid-column-start: 1;
  grid-column-end: 3;
  grid-row-start: 1;
  grid-row-end: 2;
}

/* Shorthand */
.item {
  grid-column: 1 / 3; /* start / end */
  grid-row: 1 / 2;
}

/* Avec span */
.item {
  grid-column: 1 / span 2; /* De 1, occupe 2 colonnes */
  grid-row: span 3; /* Occupe 3 lignes */
}

/* Depuis la fin (-1) */
.item-full-width {
  grid-column: 1 / -1; /* Toute la largeur */
}

/* grid-area shorthand */
.item {
  grid-area: 1 / 1 / 3 / 4; /* row-start / col-start / row-end / col-end */
}
```

### Placement avec Lignes Nommées

```css
/* Définir des lignes nommées */
.grid-named-lines {
  display: grid;
  grid-template-columns: 
    [full-start sidebar-start] 250px 
    [sidebar-end content-start] 1fr 
    [content-end aside-start] 200px 
    [aside-end full-end];
  grid-template-rows:
    [header-start] auto
    [header-end main-start] 1fr
    [main-end footer-start] auto
    [footer-end];
}

/* Utiliser les lignes nommées */
.header {
  grid-column: full-start / full-end;
  grid-row: header-start / header-end;
}

.sidebar {
  grid-column: sidebar-start / sidebar-end;
  grid-row: main-start / main-end;
}

.content {
  grid-column: content-start / content-end;
  grid-row: main-start / main-end;
}
```

### Ordre et Z-Index

```css
/* Changer l'ordre visuel */
.item-first {
  order: -1;
}

.item-last {
  order: 1;
}

/* Z-index pour superposition */
.item-overlay {
  grid-column: 1 / 3;
  grid-row: 1 / 3;
  z-index: 10;
}
```

## 🎯 Alignement

### Alignement du Container (justify/align-content)

```css
.grid-container {
  display: grid;
  grid-template-columns: repeat(3, 100px);
  grid-template-rows: repeat(3, 100px);
  height: 500px;
  
  /* Alignement horizontal des colonnes dans le container */
  justify-content: start;       /* Gauche */
  justify-content: end;         /* Droite */
  justify-content: center;      /* Centre */
  justify-content: space-between; /* Espace entre */
  justify-content: space-around;  /* Espace autour */
  justify-content: space-evenly;  /* Espace égal */
  justify-content: stretch;       /* Étire (défaut) */
  
  /* Alignement vertical des lignes dans le container */
  align-content: start;
  align-content: end;
  align-content: center;
  align-content: space-between;
  align-content: space-around;
  align-content: space-evenly;
  align-content: stretch;
  
  /* Shorthand */
  place-content: center; /* align-content justify-content */
  place-content: center start;
}
```

### Alignement des Items (justify/align-items)

```css
.grid-container {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  
  /* Alignement horizontal des items dans leurs cellules */
  justify-items: start;   /* Gauche de la cellule */
  justify-items: end;     /* Droite de la cellule */
  justify-items: center;  /* Centre de la cellule */
  justify-items: stretch; /* Étire (défaut) */
  
  /* Alignement vertical des items dans leurs cellules */
  align-items: start;
  align-items: end;
  align-items: center;
  align-items: stretch;
  align-items: baseline; /* Alignement sur la baseline du texte */
  
  /* Shorthand */
  place-items: center; /* align-items justify-items */
}
```

### Alignement Individuel (justify/align-self)

```css
.item {
  /* Surcharger l'alignement pour cet item */
  justify-self: start;
  justify-self: end;
  justify-self: center;
  justify-self: stretch;
  
  align-self: start;
  align-self: end;
  align-self: center;
  align-self: stretch;
  
  /* Shorthand */
  place-self: center;
  place-self: end start;
}
```

## 🔄 Grids Responsives

### Auto-fit vs Auto-fill

```css
/* Auto-fit : les colonnes s'étirent pour remplir */
.grid-auto-fit {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  /* Avec 3 items sur 1200px : chaque colonne ~= 400px */
}

/* Auto-fill : garde la place pour les colonnes vides */
.grid-auto-fill {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 20px;
  /* Avec 3 items sur 1200px : chaque item = 200px, espace vide visible */
}
```

### Layout Responsive avec Media Queries

```css
.responsive-layout {
  display: grid;
  gap: 20px;
  
  /* Mobile first : 1 colonne */
  grid-template-columns: 1fr;
  grid-template-areas:
    "header"
    "content"
    "sidebar"
    "footer";
}

/* Tablet */
@media (min-width: 768px) {
  .responsive-layout {
    grid-template-columns: 200px 1fr;
    grid-template-areas:
      "header  header"
      "sidebar content"
      "footer  footer";
  }
}

/* Desktop */
@media (min-width: 1024px) {
  .responsive-layout {
    grid-template-columns: 250px 1fr 200px;
    grid-template-areas:
      "header  header  header"
      "sidebar content aside"
      "footer  footer  footer";
  }
}
```

### Cards Responsives

```css
.card-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 24px;
  padding: 24px;
}

.card {
  display: grid;
  grid-template-rows: auto 1fr auto;
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.card-image {
  aspect-ratio: 16 / 9;
  object-fit: cover;
}

.card-content {
  padding: 16px;
}

.card-footer {
  padding: 16px;
  border-top: 1px solid #e5e7eb;
}
```

## 🏗️ Layouts Courants

### Holy Grail Layout

```css
.holy-grail {
  display: grid;
  grid-template-columns: minmax(200px, 1fr) minmax(0, 3fr) minmax(200px, 1fr);
  grid-template-rows: auto 1fr auto;
  grid-template-areas:
    "header header header"
    "nav    main   aside"
    "footer footer footer";
  min-height: 100vh;
  gap: 20px;
}

/* Responsive */
@media (max-width: 768px) {
  .holy-grail {
    grid-template-columns: 1fr;
    grid-template-areas:
      "header"
      "nav"
      "main"
      "aside"
      "footer";
  }
}
```

### Dashboard Layout

```css
.dashboard {
  display: grid;
  grid-template-columns: auto 1fr;
  grid-template-rows: auto 1fr;
  min-height: 100vh;
}

.dashboard-header {
  grid-column: 1 / -1;
  padding: 16px 24px;
  background: white;
  border-bottom: 1px solid #e5e7eb;
}

.dashboard-sidebar {
  width: 250px;
  padding: 24px;
  background: #1f2937;
  color: white;
  overflow-y: auto;
}

.dashboard-main {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 24px;
  padding: 24px;
  overflow-y: auto;
  align-content: start;
}

/* Sidebar collapsible */
.dashboard.collapsed .dashboard-sidebar {
  width: 64px;
}
```

### Masonry-like Layout (avec Grid)

```css
/* Pseudo-masonry avec grid-auto-rows */
.masonry-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  grid-auto-rows: 10px; /* Petites unités pour la granularité */
  gap: 16px;
}

.masonry-item {
  /* Calcul dynamique avec JS ou classes prédéfinies */
  grid-row-end: span var(--rows, 20);
}

.masonry-item.small { --rows: 15; }
.masonry-item.medium { --rows: 25; }
.masonry-item.large { --rows: 35; }
```

### Image Gallery

```css
.gallery {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  grid-auto-rows: 200px;
  gap: 8px;
}

.gallery-item {
  overflow: hidden;
  border-radius: 4px;
}

.gallery-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.gallery-item:hover img {
  transform: scale(1.05);
}

/* Featured items */
.gallery-item.featured {
  grid-column: span 2;
  grid-row: span 2;
}

@media (max-width: 600px) {
  .gallery-item.featured {
    grid-column: span 1;
    grid-row: span 1;
  }
}
```

### Form Layout

```css
.form-grid {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 16px 24px;
  align-items: center;
}

.form-label {
  justify-self: end;
  font-weight: 500;
}

.form-input {
  padding: 8px 12px;
  border: 1px solid #d1d5db;
  border-radius: 4px;
}

/* Input pleine largeur */
.form-input.full-width {
  grid-column: 1 / -1;
}

/* Groupes de champs */
.form-group {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  grid-column: 2; /* Seulement dans la zone input */
}

/* Actions */
.form-actions {
  grid-column: 2;
  display: flex;
  gap: 12px;
  justify-content: flex-end;
}

/* Responsive */
@media (max-width: 600px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
  
  .form-label {
    justify-self: start;
  }
  
  .form-group {
    grid-column: 1;
    grid-template-columns: 1fr;
  }
  
  .form-actions {
    grid-column: 1;
  }
}
```

## 🆕 Subgrid

```css
/* Parent grid */
.parent-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  grid-template-rows: auto auto;
  gap: 20px;
}

/* Child hérite des tracks du parent */
.child-subgrid {
  grid-column: span 4;
  
  display: grid;
  grid-template-columns: subgrid; /* Hérite des colonnes du parent */
  gap: 20px;
}

/* Subgrid pour rows */
.row-subgrid {
  grid-row: span 2;
  
  display: grid;
  grid-template-rows: subgrid;
}

/* Subgrid complet */
.full-subgrid {
  grid-column: span 4;
  grid-row: span 2;
  
  display: grid;
  grid-template-columns: subgrid;
  grid-template-rows: subgrid;
}

/* Cas d'usage : Card alignées */
.card-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 24px;
}

.card {
  display: grid;
  grid-template-rows: auto 1fr auto;
  /* Les cards individuelles ont leur propre grid */
}

/* Avec subgrid pour aligner les parties internes */
.card-container-subgrid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  grid-template-rows: repeat(3, auto); /* header, content, footer */
  gap: 24px 24px;
}

.card-subgrid {
  display: grid;
  grid-template-rows: subgrid;
  grid-row: span 3;
  
  background: white;
  border-radius: 8px;
  overflow: hidden;
}
```

## 🎨 Patterns Avancés

### Overlay Pattern

```css
.overlay-container {
  display: grid;
}

.overlay-container > * {
  grid-area: 1 / 1; /* Tous les enfants dans la même cellule */
}

.overlay-background {
  /* Image ou contenu de fond */
}

.overlay-content {
  z-index: 1;
  /* Contenu superposé */
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.5);
  color: white;
}
```

### Aspect Ratio avec Grid

```css
.aspect-ratio-grid {
  display: grid;
}

/* Utilise un pseudo-element pour forcer le ratio */
.aspect-ratio-grid::before {
  content: "";
  grid-area: 1 / 1;
  padding-bottom: 56.25%; /* 16:9 */
}

.aspect-ratio-grid > * {
  grid-area: 1 / 1;
}

/* Moderne : utiliser aspect-ratio directement */
.modern-aspect-ratio {
  aspect-ratio: 16 / 9;
}
```

### Dense Packing

```css
.dense-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  grid-auto-rows: 100px;
  grid-auto-flow: dense; /* Remplit les trous */
  gap: 10px;
}

.large-item {
  grid-column: span 2;
  grid-row: span 2;
}
```

## 📱 Grid + Container Queries

```css
/* Container avec grid */
.component-container {
  container-type: inline-size;
  container-name: card;
}

.adaptive-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 16px;
}

/* Layout différent selon la taille du container */
@container card (min-width: 400px) {
  .adaptive-grid {
    grid-template-columns: 120px 1fr;
    grid-template-areas:
      "image content"
      "image actions";
  }
}

@container card (min-width: 600px) {
  .adaptive-grid {
    grid-template-columns: 200px 1fr auto;
    grid-template-areas:
      "image content actions";
  }
}
```

## 🔧 CSS Custom Properties avec Grid

```css
:root {
  --grid-columns: 12;
  --grid-gap: 24px;
  --grid-max-width: 1200px;
}

.grid-system {
  display: grid;
  grid-template-columns: repeat(var(--grid-columns), 1fr);
  gap: var(--grid-gap);
  max-width: var(--grid-max-width);
  margin: 0 auto;
  padding: 0 var(--grid-gap);
}

/* Classes utilitaires pour colonnes */
.col-1 { grid-column: span 1; }
.col-2 { grid-column: span 2; }
.col-3 { grid-column: span 3; }
.col-4 { grid-column: span 4; }
.col-6 { grid-column: span 6; }
.col-8 { grid-column: span 8; }
.col-12 { grid-column: span 12; }

/* Responsive */
@media (max-width: 768px) {
  :root {
    --grid-columns: 4;
    --grid-gap: 16px;
  }
  
  .col-md-2 { grid-column: span 2; }
  .col-md-4 { grid-column: span 4; }
}

@media (max-width: 480px) {
  :root {
    --grid-columns: 1;
    --grid-gap: 12px;
  }
}
```

## ⚡ Performance

```css
/* Contenir les reflows */
.grid-container {
  contain: layout style;
}

/* Éviter les recalculs fréquents */
.stable-grid {
  /* Préférer les valeurs fixes quand possible */
  grid-template-columns: 250px 1fr 200px;
  /* Plutôt que des calculs complexes répétés */
}

/* Will-change pour animations */
.animated-grid-item {
  will-change: transform;
  transition: transform 0.3s ease;
}
```
