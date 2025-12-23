---
description: CSS Utilities - Classes utilitaires, spacing, layout, text, visibility, responsive utilities
name: CSS_Utilities
applyTo: "**/*.css,**/*.scss,**/*.vue"
---

# CSS Utilities

Guide complet pour créer et utiliser des classes CSS utilitaires.

## ⛔ À NE PAS FAIRE

- **Ne crée jamais** des utilities avec des valeurs magiques
- **N'utilise jamais** `!important` dans les utilities (sauf exceptions documentées)
- **Ne nomme jamais** les utilities avec des valeurs concrètes (`mt-27px`)
- **N'ajoute jamais** de logique métier dans les utilities
- **Ne duplique jamais** les utilities existantes du framework

## ✅ À FAIRE

- **Utilise toujours** une échelle de design cohérente
- **Utilise toujours** des noms sémantiques ou basés sur l'échelle
- **Documente toujours** les utilities custom
- **Préfixe toujours** les utilities projet pour éviter les conflits
- **Teste toujours** la responsivité des utilities

## 📐 Système d'Échelle

### Design Tokens

```css
/* utilities/tokens.css */
:root {
  /* Échelle de spacing (base 4px) */
  --space-0: 0;
  --space-px: 1px;
  --space-0-5: 0.125rem; /* 2px */
  --space-1: 0.25rem;    /* 4px */
  --space-1-5: 0.375rem; /* 6px */
  --space-2: 0.5rem;     /* 8px */
  --space-2-5: 0.625rem; /* 10px */
  --space-3: 0.75rem;    /* 12px */
  --space-3-5: 0.875rem; /* 14px */
  --space-4: 1rem;       /* 16px */
  --space-5: 1.25rem;    /* 20px */
  --space-6: 1.5rem;     /* 24px */
  --space-7: 1.75rem;    /* 28px */
  --space-8: 2rem;       /* 32px */
  --space-9: 2.25rem;    /* 36px */
  --space-10: 2.5rem;    /* 40px */
  --space-11: 2.75rem;   /* 44px */
  --space-12: 3rem;      /* 48px */
  --space-14: 3.5rem;    /* 56px */
  --space-16: 4rem;      /* 64px */
  --space-20: 5rem;      /* 80px */
  --space-24: 6rem;      /* 96px */
  --space-28: 7rem;      /* 112px */
  --space-32: 8rem;      /* 128px */
  --space-36: 9rem;      /* 144px */
  --space-40: 10rem;     /* 160px */
  --space-44: 11rem;     /* 176px */
  --space-48: 12rem;     /* 192px */
  --space-52: 13rem;     /* 208px */
  --space-56: 14rem;     /* 224px */
  --space-60: 15rem;     /* 240px */
  --space-64: 16rem;     /* 256px */
  --space-72: 18rem;     /* 288px */
  --space-80: 20rem;     /* 320px */
  --space-96: 24rem;     /* 384px */

  /* Tailles de texte */
  --text-xs: 0.75rem;    /* 12px */
  --text-sm: 0.875rem;   /* 14px */
  --text-base: 1rem;     /* 16px */
  --text-lg: 1.125rem;   /* 18px */
  --text-xl: 1.25rem;    /* 20px */
  --text-2xl: 1.5rem;    /* 24px */
  --text-3xl: 1.875rem;  /* 30px */
  --text-4xl: 2.25rem;   /* 36px */
  --text-5xl: 3rem;      /* 48px */
  --text-6xl: 3.75rem;   /* 60px */
  --text-7xl: 4.5rem;    /* 72px */
  --text-8xl: 6rem;      /* 96px */
  --text-9xl: 8rem;      /* 128px */

  /* Line heights */
  --leading-none: 1;
  --leading-tight: 1.25;
  --leading-snug: 1.375;
  --leading-normal: 1.5;
  --leading-relaxed: 1.625;
  --leading-loose: 2;

  /* Font weights */
  --font-thin: 100;
  --font-extralight: 200;
  --font-light: 300;
  --font-normal: 400;
  --font-medium: 500;
  --font-semibold: 600;
  --font-bold: 700;
  --font-extrabold: 800;
  --font-black: 900;

  /* Border radius */
  --rounded-none: 0;
  --rounded-sm: 0.125rem;
  --rounded: 0.25rem;
  --rounded-md: 0.375rem;
  --rounded-lg: 0.5rem;
  --rounded-xl: 0.75rem;
  --rounded-2xl: 1rem;
  --rounded-3xl: 1.5rem;
  --rounded-full: 9999px;

  /* Shadows */
  --shadow-sm: 0 1px 2px 0 rgb(0 0 0 / 0.05);
  --shadow: 0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1);
  --shadow-md: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1);
  --shadow-lg: 0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1);
  --shadow-xl: 0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1);
  --shadow-2xl: 0 25px 50px -12px rgb(0 0 0 / 0.25);
  --shadow-inner: inset 0 2px 4px 0 rgb(0 0 0 / 0.05);
  --shadow-none: 0 0 #0000;

  /* Z-index */
  --z-0: 0;
  --z-10: 10;
  --z-20: 20;
  --z-30: 30;
  --z-40: 40;
  --z-50: 50;
  --z-auto: auto;

  /* Transitions */
  --duration-75: 75ms;
  --duration-100: 100ms;
  --duration-150: 150ms;
  --duration-200: 200ms;
  --duration-300: 300ms;
  --duration-500: 500ms;
  --duration-700: 700ms;
  --duration-1000: 1000ms;
}
```

## 📦 Spacing Utilities

### Margin

```css
/* utilities/margin.css */

/* Margin all sides */
.m-0 { margin: var(--space-0); }
.m-1 { margin: var(--space-1); }
.m-2 { margin: var(--space-2); }
.m-3 { margin: var(--space-3); }
.m-4 { margin: var(--space-4); }
.m-5 { margin: var(--space-5); }
.m-6 { margin: var(--space-6); }
.m-8 { margin: var(--space-8); }
.m-10 { margin: var(--space-10); }
.m-12 { margin: var(--space-12); }
.m-16 { margin: var(--space-16); }
.m-auto { margin: auto; }

/* Margin X (horizontal) */
.mx-0 { margin-left: var(--space-0); margin-right: var(--space-0); }
.mx-1 { margin-left: var(--space-1); margin-right: var(--space-1); }
.mx-2 { margin-left: var(--space-2); margin-right: var(--space-2); }
.mx-3 { margin-left: var(--space-3); margin-right: var(--space-3); }
.mx-4 { margin-left: var(--space-4); margin-right: var(--space-4); }
.mx-6 { margin-left: var(--space-6); margin-right: var(--space-6); }
.mx-8 { margin-left: var(--space-8); margin-right: var(--space-8); }
.mx-auto { margin-left: auto; margin-right: auto; }

/* Margin Y (vertical) */
.my-0 { margin-top: var(--space-0); margin-bottom: var(--space-0); }
.my-1 { margin-top: var(--space-1); margin-bottom: var(--space-1); }
.my-2 { margin-top: var(--space-2); margin-bottom: var(--space-2); }
.my-3 { margin-top: var(--space-3); margin-bottom: var(--space-3); }
.my-4 { margin-top: var(--space-4); margin-bottom: var(--space-4); }
.my-6 { margin-top: var(--space-6); margin-bottom: var(--space-6); }
.my-8 { margin-top: var(--space-8); margin-bottom: var(--space-8); }
.my-auto { margin-top: auto; margin-bottom: auto; }

/* Margin Top */
.mt-0 { margin-top: var(--space-0); }
.mt-1 { margin-top: var(--space-1); }
.mt-2 { margin-top: var(--space-2); }
.mt-3 { margin-top: var(--space-3); }
.mt-4 { margin-top: var(--space-4); }
.mt-5 { margin-top: var(--space-5); }
.mt-6 { margin-top: var(--space-6); }
.mt-8 { margin-top: var(--space-8); }
.mt-10 { margin-top: var(--space-10); }
.mt-12 { margin-top: var(--space-12); }
.mt-16 { margin-top: var(--space-16); }
.mt-auto { margin-top: auto; }

/* Margin Right */
.mr-0 { margin-right: var(--space-0); }
.mr-1 { margin-right: var(--space-1); }
.mr-2 { margin-right: var(--space-2); }
.mr-3 { margin-right: var(--space-3); }
.mr-4 { margin-right: var(--space-4); }
.mr-6 { margin-right: var(--space-6); }
.mr-8 { margin-right: var(--space-8); }
.mr-auto { margin-right: auto; }

/* Margin Bottom */
.mb-0 { margin-bottom: var(--space-0); }
.mb-1 { margin-bottom: var(--space-1); }
.mb-2 { margin-bottom: var(--space-2); }
.mb-3 { margin-bottom: var(--space-3); }
.mb-4 { margin-bottom: var(--space-4); }
.mb-5 { margin-bottom: var(--space-5); }
.mb-6 { margin-bottom: var(--space-6); }
.mb-8 { margin-bottom: var(--space-8); }
.mb-10 { margin-bottom: var(--space-10); }
.mb-12 { margin-bottom: var(--space-12); }
.mb-16 { margin-bottom: var(--space-16); }
.mb-auto { margin-bottom: auto; }

/* Margin Left */
.ml-0 { margin-left: var(--space-0); }
.ml-1 { margin-left: var(--space-1); }
.ml-2 { margin-left: var(--space-2); }
.ml-3 { margin-left: var(--space-3); }
.ml-4 { margin-left: var(--space-4); }
.ml-6 { margin-left: var(--space-6); }
.ml-8 { margin-left: var(--space-8); }
.ml-auto { margin-left: auto; }

/* Negative margins */
.-mt-1 { margin-top: calc(var(--space-1) * -1); }
.-mt-2 { margin-top: calc(var(--space-2) * -1); }
.-mt-4 { margin-top: calc(var(--space-4) * -1); }
.-mb-1 { margin-bottom: calc(var(--space-1) * -1); }
.-mb-2 { margin-bottom: calc(var(--space-2) * -1); }
.-ml-1 { margin-left: calc(var(--space-1) * -1); }
.-mr-1 { margin-right: calc(var(--space-1) * -1); }
```

### Padding

```css
/* utilities/padding.css */

/* Padding all sides */
.p-0 { padding: var(--space-0); }
.p-1 { padding: var(--space-1); }
.p-2 { padding: var(--space-2); }
.p-3 { padding: var(--space-3); }
.p-4 { padding: var(--space-4); }
.p-5 { padding: var(--space-5); }
.p-6 { padding: var(--space-6); }
.p-8 { padding: var(--space-8); }
.p-10 { padding: var(--space-10); }
.p-12 { padding: var(--space-12); }
.p-16 { padding: var(--space-16); }

/* Padding X (horizontal) */
.px-0 { padding-left: var(--space-0); padding-right: var(--space-0); }
.px-1 { padding-left: var(--space-1); padding-right: var(--space-1); }
.px-2 { padding-left: var(--space-2); padding-right: var(--space-2); }
.px-3 { padding-left: var(--space-3); padding-right: var(--space-3); }
.px-4 { padding-left: var(--space-4); padding-right: var(--space-4); }
.px-5 { padding-left: var(--space-5); padding-right: var(--space-5); }
.px-6 { padding-left: var(--space-6); padding-right: var(--space-6); }
.px-8 { padding-left: var(--space-8); padding-right: var(--space-8); }

/* Padding Y (vertical) */
.py-0 { padding-top: var(--space-0); padding-bottom: var(--space-0); }
.py-1 { padding-top: var(--space-1); padding-bottom: var(--space-1); }
.py-2 { padding-top: var(--space-2); padding-bottom: var(--space-2); }
.py-3 { padding-top: var(--space-3); padding-bottom: var(--space-3); }
.py-4 { padding-top: var(--space-4); padding-bottom: var(--space-4); }
.py-5 { padding-top: var(--space-5); padding-bottom: var(--space-5); }
.py-6 { padding-top: var(--space-6); padding-bottom: var(--space-6); }
.py-8 { padding-top: var(--space-8); padding-bottom: var(--space-8); }

/* Padding individual sides - pt, pr, pb, pl */
.pt-0 { padding-top: var(--space-0); }
.pt-1 { padding-top: var(--space-1); }
.pt-2 { padding-top: var(--space-2); }
.pt-4 { padding-top: var(--space-4); }
.pt-6 { padding-top: var(--space-6); }
.pt-8 { padding-top: var(--space-8); }

.pr-0 { padding-right: var(--space-0); }
.pr-2 { padding-right: var(--space-2); }
.pr-4 { padding-right: var(--space-4); }

.pb-0 { padding-bottom: var(--space-0); }
.pb-2 { padding-bottom: var(--space-2); }
.pb-4 { padding-bottom: var(--space-4); }
.pb-6 { padding-bottom: var(--space-6); }
.pb-8 { padding-bottom: var(--space-8); }

.pl-0 { padding-left: var(--space-0); }
.pl-2 { padding-left: var(--space-2); }
.pl-4 { padding-left: var(--space-4); }
```

### Gap (Flexbox/Grid)

```css
/* utilities/gap.css */

.gap-0 { gap: var(--space-0); }
.gap-1 { gap: var(--space-1); }
.gap-2 { gap: var(--space-2); }
.gap-3 { gap: var(--space-3); }
.gap-4 { gap: var(--space-4); }
.gap-5 { gap: var(--space-5); }
.gap-6 { gap: var(--space-6); }
.gap-8 { gap: var(--space-8); }
.gap-10 { gap: var(--space-10); }
.gap-12 { gap: var(--space-12); }

.gap-x-1 { column-gap: var(--space-1); }
.gap-x-2 { column-gap: var(--space-2); }
.gap-x-4 { column-gap: var(--space-4); }
.gap-x-6 { column-gap: var(--space-6); }
.gap-x-8 { column-gap: var(--space-8); }

.gap-y-1 { row-gap: var(--space-1); }
.gap-y-2 { row-gap: var(--space-2); }
.gap-y-4 { row-gap: var(--space-4); }
.gap-y-6 { row-gap: var(--space-6); }
.gap-y-8 { row-gap: var(--space-8); }
```

## 📐 Layout Utilities

### Display

```css
/* utilities/display.css */

.block { display: block; }
.inline-block { display: inline-block; }
.inline { display: inline; }
.flex { display: flex; }
.inline-flex { display: inline-flex; }
.grid { display: grid; }
.inline-grid { display: inline-grid; }
.contents { display: contents; }
.hidden { display: none; }

/* Table display */
.table { display: table; }
.table-row { display: table-row; }
.table-cell { display: table-cell; }
```

### Flexbox

```css
/* utilities/flexbox.css */

/* Flex Direction */
.flex-row { flex-direction: row; }
.flex-row-reverse { flex-direction: row-reverse; }
.flex-col { flex-direction: column; }
.flex-col-reverse { flex-direction: column-reverse; }

/* Flex Wrap */
.flex-wrap { flex-wrap: wrap; }
.flex-nowrap { flex-wrap: nowrap; }
.flex-wrap-reverse { flex-wrap: wrap-reverse; }

/* Flex Grow & Shrink */
.flex-1 { flex: 1 1 0%; }
.flex-auto { flex: 1 1 auto; }
.flex-initial { flex: 0 1 auto; }
.flex-none { flex: none; }
.grow { flex-grow: 1; }
.grow-0 { flex-grow: 0; }
.shrink { flex-shrink: 1; }
.shrink-0 { flex-shrink: 0; }

/* Justify Content */
.justify-start { justify-content: flex-start; }
.justify-end { justify-content: flex-end; }
.justify-center { justify-content: center; }
.justify-between { justify-content: space-between; }
.justify-around { justify-content: space-around; }
.justify-evenly { justify-content: space-evenly; }

/* Align Items */
.items-start { align-items: flex-start; }
.items-end { align-items: flex-end; }
.items-center { align-items: center; }
.items-baseline { align-items: baseline; }
.items-stretch { align-items: stretch; }

/* Align Self */
.self-auto { align-self: auto; }
.self-start { align-self: flex-start; }
.self-end { align-self: flex-end; }
.self-center { align-self: center; }
.self-stretch { align-self: stretch; }

/* Align Content */
.content-start { align-content: flex-start; }
.content-end { align-content: flex-end; }
.content-center { align-content: center; }
.content-between { align-content: space-between; }
.content-around { align-content: space-around; }
.content-evenly { align-content: space-evenly; }

/* Order */
.order-first { order: -9999; }
.order-last { order: 9999; }
.order-none { order: 0; }
.order-1 { order: 1; }
.order-2 { order: 2; }
.order-3 { order: 3; }
```

### Grid

```css
/* utilities/grid.css */

/* Grid Template Columns */
.grid-cols-1 { grid-template-columns: repeat(1, minmax(0, 1fr)); }
.grid-cols-2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
.grid-cols-3 { grid-template-columns: repeat(3, minmax(0, 1fr)); }
.grid-cols-4 { grid-template-columns: repeat(4, minmax(0, 1fr)); }
.grid-cols-5 { grid-template-columns: repeat(5, minmax(0, 1fr)); }
.grid-cols-6 { grid-template-columns: repeat(6, minmax(0, 1fr)); }
.grid-cols-12 { grid-template-columns: repeat(12, minmax(0, 1fr)); }
.grid-cols-none { grid-template-columns: none; }

/* Grid Template Rows */
.grid-rows-1 { grid-template-rows: repeat(1, minmax(0, 1fr)); }
.grid-rows-2 { grid-template-rows: repeat(2, minmax(0, 1fr)); }
.grid-rows-3 { grid-template-rows: repeat(3, minmax(0, 1fr)); }
.grid-rows-4 { grid-template-rows: repeat(4, minmax(0, 1fr)); }
.grid-rows-none { grid-template-rows: none; }

/* Grid Column Span */
.col-span-1 { grid-column: span 1 / span 1; }
.col-span-2 { grid-column: span 2 / span 2; }
.col-span-3 { grid-column: span 3 / span 3; }
.col-span-4 { grid-column: span 4 / span 4; }
.col-span-6 { grid-column: span 6 / span 6; }
.col-span-12 { grid-column: span 12 / span 12; }
.col-span-full { grid-column: 1 / -1; }

/* Grid Row Span */
.row-span-1 { grid-row: span 1 / span 1; }
.row-span-2 { grid-row: span 2 / span 2; }
.row-span-3 { grid-row: span 3 / span 3; }
.row-span-full { grid-row: 1 / -1; }

/* Grid Auto Flow */
.grid-flow-row { grid-auto-flow: row; }
.grid-flow-col { grid-auto-flow: column; }
.grid-flow-dense { grid-auto-flow: dense; }
.grid-flow-row-dense { grid-auto-flow: row dense; }
.grid-flow-col-dense { grid-auto-flow: column dense; }

/* Place Items */
.place-items-start { place-items: start; }
.place-items-end { place-items: end; }
.place-items-center { place-items: center; }
.place-items-stretch { place-items: stretch; }

/* Place Content */
.place-content-start { place-content: start; }
.place-content-end { place-content: end; }
.place-content-center { place-content: center; }
.place-content-between { place-content: space-between; }
.place-content-around { place-content: space-around; }
.place-content-evenly { place-content: space-evenly; }
.place-content-stretch { place-content: stretch; }
```

### Position

```css
/* utilities/position.css */

.static { position: static; }
.fixed { position: fixed; }
.absolute { position: absolute; }
.relative { position: relative; }
.sticky { position: sticky; }

/* Inset */
.inset-0 { inset: 0; }
.inset-auto { inset: auto; }
.inset-x-0 { left: 0; right: 0; }
.inset-y-0 { top: 0; bottom: 0; }

/* Top, Right, Bottom, Left */
.top-0 { top: 0; }
.top-1 { top: var(--space-1); }
.top-2 { top: var(--space-2); }
.top-4 { top: var(--space-4); }
.top-full { top: 100%; }
.top-auto { top: auto; }
.-top-1 { top: calc(var(--space-1) * -1); }

.right-0 { right: 0; }
.right-2 { right: var(--space-2); }
.right-4 { right: var(--space-4); }
.right-auto { right: auto; }

.bottom-0 { bottom: 0; }
.bottom-2 { bottom: var(--space-2); }
.bottom-4 { bottom: var(--space-4); }
.bottom-auto { bottom: auto; }

.left-0 { left: 0; }
.left-2 { left: var(--space-2); }
.left-4 { left: var(--space-4); }
.left-auto { left: auto; }

/* Z-Index */
.z-0 { z-index: var(--z-0); }
.z-10 { z-index: var(--z-10); }
.z-20 { z-index: var(--z-20); }
.z-30 { z-index: var(--z-30); }
.z-40 { z-index: var(--z-40); }
.z-50 { z-index: var(--z-50); }
.z-auto { z-index: var(--z-auto); }
```

## 📝 Typography Utilities

```css
/* utilities/typography.css */

/* Font Size */
.text-xs { font-size: var(--text-xs); line-height: 1rem; }
.text-sm { font-size: var(--text-sm); line-height: 1.25rem; }
.text-base { font-size: var(--text-base); line-height: 1.5rem; }
.text-lg { font-size: var(--text-lg); line-height: 1.75rem; }
.text-xl { font-size: var(--text-xl); line-height: 1.75rem; }
.text-2xl { font-size: var(--text-2xl); line-height: 2rem; }
.text-3xl { font-size: var(--text-3xl); line-height: 2.25rem; }
.text-4xl { font-size: var(--text-4xl); line-height: 2.5rem; }
.text-5xl { font-size: var(--text-5xl); line-height: 1; }
.text-6xl { font-size: var(--text-6xl); line-height: 1; }

/* Font Weight */
.font-thin { font-weight: var(--font-thin); }
.font-extralight { font-weight: var(--font-extralight); }
.font-light { font-weight: var(--font-light); }
.font-normal { font-weight: var(--font-normal); }
.font-medium { font-weight: var(--font-medium); }
.font-semibold { font-weight: var(--font-semibold); }
.font-bold { font-weight: var(--font-bold); }
.font-extrabold { font-weight: var(--font-extrabold); }
.font-black { font-weight: var(--font-black); }

/* Font Style */
.italic { font-style: italic; }
.not-italic { font-style: normal; }

/* Text Align */
.text-left { text-align: left; }
.text-center { text-align: center; }
.text-right { text-align: right; }
.text-justify { text-align: justify; }

/* Text Decoration */
.underline { text-decoration-line: underline; }
.overline { text-decoration-line: overline; }
.line-through { text-decoration-line: line-through; }
.no-underline { text-decoration-line: none; }

/* Text Transform */
.uppercase { text-transform: uppercase; }
.lowercase { text-transform: lowercase; }
.capitalize { text-transform: capitalize; }
.normal-case { text-transform: none; }

/* Line Height */
.leading-none { line-height: var(--leading-none); }
.leading-tight { line-height: var(--leading-tight); }
.leading-snug { line-height: var(--leading-snug); }
.leading-normal { line-height: var(--leading-normal); }
.leading-relaxed { line-height: var(--leading-relaxed); }
.leading-loose { line-height: var(--leading-loose); }

/* Letter Spacing */
.tracking-tighter { letter-spacing: -0.05em; }
.tracking-tight { letter-spacing: -0.025em; }
.tracking-normal { letter-spacing: 0; }
.tracking-wide { letter-spacing: 0.025em; }
.tracking-wider { letter-spacing: 0.05em; }
.tracking-widest { letter-spacing: 0.1em; }

/* Text Overflow */
.truncate {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.text-ellipsis { text-overflow: ellipsis; }
.text-clip { text-overflow: clip; }

/* Whitespace */
.whitespace-normal { white-space: normal; }
.whitespace-nowrap { white-space: nowrap; }
.whitespace-pre { white-space: pre; }
.whitespace-pre-line { white-space: pre-line; }
.whitespace-pre-wrap { white-space: pre-wrap; }

/* Word Break */
.break-normal { word-break: normal; overflow-wrap: normal; }
.break-words { overflow-wrap: break-word; }
.break-all { word-break: break-all; }

/* Line Clamp */
.line-clamp-1 {
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.line-clamp-3 {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
```

## 🎨 Visual Utilities

### Size

```css
/* utilities/size.css */

/* Width */
.w-0 { width: 0; }
.w-1 { width: var(--space-1); }
.w-2 { width: var(--space-2); }
.w-4 { width: var(--space-4); }
.w-8 { width: var(--space-8); }
.w-16 { width: var(--space-16); }
.w-auto { width: auto; }
.w-full { width: 100%; }
.w-screen { width: 100vw; }
.w-min { width: min-content; }
.w-max { width: max-content; }
.w-fit { width: fit-content; }

/* Height */
.h-0 { height: 0; }
.h-1 { height: var(--space-1); }
.h-2 { height: var(--space-2); }
.h-4 { height: var(--space-4); }
.h-8 { height: var(--space-8); }
.h-16 { height: var(--space-16); }
.h-auto { height: auto; }
.h-full { height: 100%; }
.h-screen { height: 100vh; }
.h-dvh { height: 100dvh; }
.h-min { height: min-content; }
.h-max { height: max-content; }
.h-fit { height: fit-content; }

/* Min/Max Width */
.min-w-0 { min-width: 0; }
.min-w-full { min-width: 100%; }
.max-w-xs { max-width: 20rem; }
.max-w-sm { max-width: 24rem; }
.max-w-md { max-width: 28rem; }
.max-w-lg { max-width: 32rem; }
.max-w-xl { max-width: 36rem; }
.max-w-2xl { max-width: 42rem; }
.max-w-3xl { max-width: 48rem; }
.max-w-4xl { max-width: 56rem; }
.max-w-5xl { max-width: 64rem; }
.max-w-6xl { max-width: 72rem; }
.max-w-7xl { max-width: 80rem; }
.max-w-full { max-width: 100%; }
.max-w-prose { max-width: 65ch; }
.max-w-none { max-width: none; }

/* Min/Max Height */
.min-h-0 { min-height: 0; }
.min-h-full { min-height: 100%; }
.min-h-screen { min-height: 100vh; }
.max-h-full { max-height: 100%; }
.max-h-screen { max-height: 100vh; }
```

### Border & Rounded

```css
/* utilities/border.css */

/* Border Width */
.border-0 { border-width: 0; }
.border { border-width: 1px; }
.border-2 { border-width: 2px; }
.border-4 { border-width: 4px; }
.border-8 { border-width: 8px; }

.border-t { border-top-width: 1px; }
.border-r { border-right-width: 1px; }
.border-b { border-bottom-width: 1px; }
.border-l { border-left-width: 1px; }

/* Border Style */
.border-solid { border-style: solid; }
.border-dashed { border-style: dashed; }
.border-dotted { border-style: dotted; }
.border-double { border-style: double; }
.border-none { border-style: none; }

/* Border Radius */
.rounded-none { border-radius: var(--rounded-none); }
.rounded-sm { border-radius: var(--rounded-sm); }
.rounded { border-radius: var(--rounded); }
.rounded-md { border-radius: var(--rounded-md); }
.rounded-lg { border-radius: var(--rounded-lg); }
.rounded-xl { border-radius: var(--rounded-xl); }
.rounded-2xl { border-radius: var(--rounded-2xl); }
.rounded-3xl { border-radius: var(--rounded-3xl); }
.rounded-full { border-radius: var(--rounded-full); }

/* Individual corners */
.rounded-t-lg { border-top-left-radius: var(--rounded-lg); border-top-right-radius: var(--rounded-lg); }
.rounded-r-lg { border-top-right-radius: var(--rounded-lg); border-bottom-right-radius: var(--rounded-lg); }
.rounded-b-lg { border-bottom-left-radius: var(--rounded-lg); border-bottom-right-radius: var(--rounded-lg); }
.rounded-l-lg { border-top-left-radius: var(--rounded-lg); border-bottom-left-radius: var(--rounded-lg); }
```

### Shadow & Effects

```css
/* utilities/effects.css */

/* Box Shadow */
.shadow-sm { box-shadow: var(--shadow-sm); }
.shadow { box-shadow: var(--shadow); }
.shadow-md { box-shadow: var(--shadow-md); }
.shadow-lg { box-shadow: var(--shadow-lg); }
.shadow-xl { box-shadow: var(--shadow-xl); }
.shadow-2xl { box-shadow: var(--shadow-2xl); }
.shadow-inner { box-shadow: var(--shadow-inner); }
.shadow-none { box-shadow: var(--shadow-none); }

/* Opacity */
.opacity-0 { opacity: 0; }
.opacity-5 { opacity: 0.05; }
.opacity-10 { opacity: 0.1; }
.opacity-20 { opacity: 0.2; }
.opacity-25 { opacity: 0.25; }
.opacity-30 { opacity: 0.3; }
.opacity-40 { opacity: 0.4; }
.opacity-50 { opacity: 0.5; }
.opacity-60 { opacity: 0.6; }
.opacity-70 { opacity: 0.7; }
.opacity-75 { opacity: 0.75; }
.opacity-80 { opacity: 0.8; }
.opacity-90 { opacity: 0.9; }
.opacity-95 { opacity: 0.95; }
.opacity-100 { opacity: 1; }

/* Overflow */
.overflow-auto { overflow: auto; }
.overflow-hidden { overflow: hidden; }
.overflow-visible { overflow: visible; }
.overflow-scroll { overflow: scroll; }
.overflow-x-auto { overflow-x: auto; }
.overflow-y-auto { overflow-y: auto; }
.overflow-x-hidden { overflow-x: hidden; }
.overflow-y-hidden { overflow-y: hidden; }
```

## 👁️ Visibility Utilities

```css
/* utilities/visibility.css */

/* Visibility */
.visible { visibility: visible; }
.invisible { visibility: hidden; }
.collapse { visibility: collapse; }

/* Screen Reader Only */
.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border-width: 0;
}

.not-sr-only {
  position: static;
  width: auto;
  height: auto;
  padding: 0;
  margin: 0;
  overflow: visible;
  clip: auto;
  white-space: normal;
}

/* Pointer Events */
.pointer-events-none { pointer-events: none; }
.pointer-events-auto { pointer-events: auto; }

/* User Select */
.select-none { user-select: none; }
.select-text { user-select: text; }
.select-all { user-select: all; }
.select-auto { user-select: auto; }

/* Cursor */
.cursor-auto { cursor: auto; }
.cursor-default { cursor: default; }
.cursor-pointer { cursor: pointer; }
.cursor-wait { cursor: wait; }
.cursor-text { cursor: text; }
.cursor-move { cursor: move; }
.cursor-not-allowed { cursor: not-allowed; }
.cursor-grab { cursor: grab; }
.cursor-grabbing { cursor: grabbing; }
```

## 📱 Responsive Utilities

```css
/* utilities/responsive.css */

/* Breakpoints */
@media (min-width: 640px) {
  .sm\:hidden { display: none; }
  .sm\:block { display: block; }
  .sm\:flex { display: flex; }
  .sm\:grid { display: grid; }
  .sm\:grid-cols-2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .sm\:flex-row { flex-direction: row; }
  .sm\:text-left { text-align: left; }
  /* ... autres utilities sm */
}

@media (min-width: 768px) {
  .md\:hidden { display: none; }
  .md\:block { display: block; }
  .md\:flex { display: flex; }
  .md\:grid { display: grid; }
  .md\:grid-cols-3 { grid-template-columns: repeat(3, minmax(0, 1fr)); }
  .md\:grid-cols-4 { grid-template-columns: repeat(4, minmax(0, 1fr)); }
  .md\:flex-row { flex-direction: row; }
  .md\:w-1\/2 { width: 50%; }
  .md\:w-1\/3 { width: 33.333333%; }
  /* ... autres utilities md */
}

@media (min-width: 1024px) {
  .lg\:hidden { display: none; }
  .lg\:block { display: block; }
  .lg\:flex { display: flex; }
  .lg\:grid { display: grid; }
  .lg\:grid-cols-4 { grid-template-columns: repeat(4, minmax(0, 1fr)); }
  .lg\:grid-cols-6 { grid-template-columns: repeat(6, minmax(0, 1fr)); }
  /* ... autres utilities lg */
}

@media (min-width: 1280px) {
  .xl\:hidden { display: none; }
  .xl\:block { display: block; }
  .xl\:grid-cols-5 { grid-template-columns: repeat(5, minmax(0, 1fr)); }
  .xl\:grid-cols-6 { grid-template-columns: repeat(6, minmax(0, 1fr)); }
  /* ... autres utilities xl */
}

/* Dark mode */
@media (prefers-color-scheme: dark) {
  .dark\:hidden { display: none; }
  .dark\:block { display: block; }
}

/* Print */
@media print {
  .print\:hidden { display: none; }
  .print\:block { display: block; }
}
```

## 🎯 Usage Patterns

```html
<!-- Layout responsive -->
<div class="flex flex-col md:flex-row gap-4 md:gap-6">
  <aside class="w-full md:w-1/4 p-4">Sidebar</aside>
  <main class="flex-1 p-4 md:p-6">Content</main>
</div>

<!-- Card avec utilities -->
<article class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow">
  <h2 class="text-xl font-semibold mb-2 truncate">Title</h2>
  <p class="text-sm text-gray-600 line-clamp-3">Description...</p>
  <div class="mt-4 flex gap-2">
    <button class="px-4 py-2 rounded bg-blue-500 text-white">Action</button>
  </div>
</article>

<!-- Grid responsive -->
<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
  <div class="p-4 border rounded">Item 1</div>
  <div class="p-4 border rounded">Item 2</div>
  <div class="p-4 border rounded">Item 3</div>
  <div class="p-4 border rounded">Item 4</div>
</div>

<!-- Centrage avec flex -->
<div class="flex items-center justify-center min-h-screen">
  <div class="text-center">Centered content</div>
</div>
```
