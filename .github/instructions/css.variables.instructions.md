---
description: CSS Variables - Custom properties, theming, dark mode, design tokens
name: CSS_Variables
applyTo: "**/*.css,**/*.scss,**/*.vue"
---

# CSS Variables (Custom Properties)

Guide complet pour l'utilisation des variables CSS et la création de design tokens.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** de valeurs en dur répétées - crée une variable
- **Ne nomme jamais** les variables par leur valeur (`--blue`) - utilise des noms sémantiques
- **Ne déclare jamais** de variables dans des sélecteurs spécifiques sans raison
- **N'oublie jamais** de définir des fallbacks pour les variables
- **Ne mélange jamais** les conventions de nommage
- **Ne crée jamais** de variables non utilisées

## ✅ À FAIRE

- **Déclare toujours** les variables globales dans `:root`
- **Utilise toujours** des noms sémantiques (`--color-primary`, `--spacing-md`)
- **Fournis toujours** des valeurs de fallback (`var(--color, #000)`)
- **Organise toujours** les variables par catégorie
- **Documente toujours** les variables du design system
- **Préfère toujours** les variables CSS aux valeurs SCSS pour le theming

## 🎨 Structure des Design Tokens

### Hiérarchie des Tokens

```css
/* 
 * NIVEAU 1 : Tokens de Base (Primitives)
 * Valeurs brutes, génériques, sans contexte
 */
:root {
  /* Couleurs primitives */
  --color-blue-50: #e3f2fd;
  --color-blue-100: #bbdefb;
  --color-blue-500: #2196f3;
  --color-blue-900: #0d47a1;
  
  --color-gray-50: #fafafa;
  --color-gray-100: #f5f5f5;
  --color-gray-500: #9e9e9e;
  --color-gray-900: #212121;
  
  /* Espacements primitifs */
  --space-1: 0.25rem;
  --space-2: 0.5rem;
  --space-3: 0.75rem;
  --space-4: 1rem;
  --space-6: 1.5rem;
  --space-8: 2rem;
  
  /* Tailles de police primitives */
  --font-size-12: 0.75rem;
  --font-size-14: 0.875rem;
  --font-size-16: 1rem;
  --font-size-18: 1.125rem;
  --font-size-24: 1.5rem;
  --font-size-32: 2rem;
}

/*
 * NIVEAU 2 : Tokens Sémantiques (Alias)
 * Noms avec signification, référencent les primitives
 */
:root {
  /* Couleurs sémantiques */
  --color-primary: var(--color-blue-500);
  --color-primary-light: var(--color-blue-100);
  --color-primary-dark: var(--color-blue-900);
  
  --color-text-primary: var(--color-gray-900);
  --color-text-secondary: var(--color-gray-500);
  --color-text-inverse: white;
  
  --color-background: var(--color-gray-50);
  --color-surface: white;
  --color-border: var(--color-gray-200);
  
  /* Espacements sémantiques */
  --spacing-xs: var(--space-1);
  --spacing-sm: var(--space-2);
  --spacing-md: var(--space-4);
  --spacing-lg: var(--space-6);
  --spacing-xl: var(--space-8);
  
  /* Typographie sémantique */
  --font-size-body: var(--font-size-16);
  --font-size-small: var(--font-size-14);
  --font-size-heading: var(--font-size-24);
}

/*
 * NIVEAU 3 : Tokens de Composant
 * Spécifiques à un composant, référencent les sémantiques
 */
:root {
  /* Boutons */
  --button-padding-x: var(--spacing-md);
  --button-padding-y: var(--spacing-sm);
  --button-border-radius: var(--radius-md);
  --button-font-size: var(--font-size-body);
  
  /* Cards */
  --card-padding: var(--spacing-lg);
  --card-border-radius: var(--radius-lg);
  --card-background: var(--color-surface);
  --card-shadow: var(--shadow-md);
  
  /* Inputs */
  --input-padding-x: var(--spacing-sm);
  --input-padding-y: var(--spacing-sm);
  --input-border-color: var(--color-border);
  --input-border-radius: var(--radius-md);
}
```

## 🌗 Theming et Dark Mode

### Structure de Thème

```css
/* === THÈME CLAIR (défaut) === */
:root,
[data-theme="light"] {
  /* Couleurs de fond */
  --color-background: #f5f5f5;
  --color-surface: #ffffff;
  --color-surface-elevated: #ffffff;
  
  /* Couleurs de texte */
  --color-text-primary: #212121;
  --color-text-secondary: #757575;
  --color-text-disabled: #bdbdbd;
  
  /* Couleurs de bordure */
  --color-border: #e0e0e0;
  --color-border-focus: #2196f3;
  
  /* Ombres */
  --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px rgba(0, 0, 0, 0.1);
  --shadow-lg: 0 10px 15px rgba(0, 0, 0, 0.1);
  
  /* Overlays */
  --color-overlay: rgba(0, 0, 0, 0.5);
}

/* === THÈME SOMBRE === */
[data-theme="dark"] {
  /* Couleurs de fond */
  --color-background: #121212;
  --color-surface: #1e1e1e;
  --color-surface-elevated: #2d2d2d;
  
  /* Couleurs de texte */
  --color-text-primary: #e0e0e0;
  --color-text-secondary: #9e9e9e;
  --color-text-disabled: #616161;
  
  /* Couleurs de bordure */
  --color-border: #424242;
  --color-border-focus: #64b5f6;
  
  /* Ombres (plus subtiles en dark mode) */
  --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.2);
  --shadow-md: 0 4px 6px rgba(0, 0, 0, 0.3);
  --shadow-lg: 0 10px 15px rgba(0, 0, 0, 0.4);
  
  /* Overlays */
  --color-overlay: rgba(0, 0, 0, 0.7);
}

/* === DÉTECTION AUTOMATIQUE === */
@media (prefers-color-scheme: dark) {
  :root:not([data-theme="light"]) {
    /* Appliquer le thème sombre si pas de préférence explicite light */
    --color-background: #121212;
    --color-surface: #1e1e1e;
    /* ... etc ... */
  }
}
```

### Changement de Thème avec JavaScript

```css
/* Variables de transition pour le changement de thème */
:root {
  --theme-transition-duration: 200ms;
}

/* Appliquer la transition lors du changement */
[data-theme-transitioning] * {
  transition: 
    background-color var(--theme-transition-duration) ease,
    color var(--theme-transition-duration) ease,
    border-color var(--theme-transition-duration) ease,
    box-shadow var(--theme-transition-duration) ease !important;
}
```

```typescript
/ themeManager.ts
export function setTheme(theme: 'light' | 'dark' | 'system'): void {
  const root = document.documentElement;
  
  / Ajouter classe de transition
  root.setAttribute('data-theme-transitioning', '');
  
  if (theme === 'system') {
    root.removeAttribute('data-theme');
  } else {
    root.setAttribute('data-theme', theme);
  }
  
  / Retirer classe après transition
  setTimeout(() => {
    root.removeAttribute('data-theme-transitioning');
  }, 200);
  
  / Sauvegarder la préférence
  localStorage.setItem('theme', theme);
}

/ Initialisation au chargement
export function initTheme(): void {
  const saved = localStorage.getItem('theme') as 'light' | 'dark' | 'system' | null;
  if (saved) {
    setTheme(saved);
  }
}
```

## 📐 Catégories de Variables

### Couleurs Complètes

```css
:root {
  /* === COULEURS DE MARQUE === */
  --color-primary: #0288d1;
  --color-primary-hover: #0277bd;
  --color-primary-active: #01579b;
  --color-primary-subtle: #e1f5fe;
  --color-primary-contrast: #ffffff;
  
  --color-secondary: #78909c;
  --color-secondary-hover: #607d8b;
  --color-secondary-active: #546e7a;
  --color-secondary-subtle: #eceff1;
  --color-secondary-contrast: #ffffff;
  
  /* === COULEURS SÉMANTIQUES === */
  --color-success: #4caf50;
  --color-success-hover: #43a047;
  --color-success-subtle: #e8f5e9;
  --color-success-contrast: #ffffff;
  
  --color-warning: #ff9800;
  --color-warning-hover: #f57c00;
  --color-warning-subtle: #fff3e0;
  --color-warning-contrast: #000000;
  
  --color-error: #f44336;
  --color-error-hover: #e53935;
  --color-error-subtle: #ffebee;
  --color-error-contrast: #ffffff;
  
  --color-info: #2196f3;
  --color-info-hover: #1e88e5;
  --color-info-subtle: #e3f2fd;
  --color-info-contrast: #ffffff;
  
  /* === COULEURS NEUTRES === */
  --color-white: #ffffff;
  --color-black: #000000;
  
  --color-gray-50: #fafafa;
  --color-gray-100: #f5f5f5;
  --color-gray-200: #eeeeee;
  --color-gray-300: #e0e0e0;
  --color-gray-400: #bdbdbd;
  --color-gray-500: #9e9e9e;
  --color-gray-600: #757575;
  --color-gray-700: #616161;
  --color-gray-800: #424242;
  --color-gray-900: #212121;
}
```

### Typographie

```css
:root {
  /* === FAMILLES DE POLICE === */
  --font-family-sans: 'Inter', -apple-system, BlinkMacSystemFont, 
    'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  --font-family-mono: 'Fira Code', 'Consolas', 'Monaco', 
    'Liberation Mono', monospace;
  --font-family-serif: 'Georgia', 'Times New Roman', serif;
  
  /* === TAILLES DE POLICE === */
  --font-size-2xs: 0.625rem;  /* 10px */
  --font-size-xs: 0.75rem;    /* 12px */
  --font-size-sm: 0.875rem;   /* 14px */
  --font-size-base: 1rem;     /* 16px */
  --font-size-lg: 1.125rem;   /* 18px */
  --font-size-xl: 1.25rem;    /* 20px */
  --font-size-2xl: 1.5rem;    /* 24px */
  --font-size-3xl: 1.875rem;  /* 30px */
  --font-size-4xl: 2.25rem;   /* 36px */
  --font-size-5xl: 3rem;      /* 48px */
  
  /* === POIDS DE POLICE === */
  --font-weight-light: 300;
  --font-weight-normal: 400;
  --font-weight-medium: 500;
  --font-weight-semibold: 600;
  --font-weight-bold: 700;
  
  /* === HAUTEUR DE LIGNE === */
  --line-height-none: 1;
  --line-height-tight: 1.25;
  --line-height-snug: 1.375;
  --line-height-normal: 1.5;
  --line-height-relaxed: 1.625;
  --line-height-loose: 2;
  
  /* === ESPACEMENT DES LETTRES === */
  --letter-spacing-tighter: -0.05em;
  --letter-spacing-tight: -0.025em;
  --letter-spacing-normal: 0;
  --letter-spacing-wide: 0.025em;
  --letter-spacing-wider: 0.05em;
  --letter-spacing-widest: 0.1em;
}
```

### Espacements

```css
:root {
  /* === ÉCHELLE D'ESPACEMENT === */
  /* Basée sur une unité de 4px */
  --spacing-0: 0;
  --spacing-px: 1px;
  --spacing-0-5: 0.125rem;  /* 2px */
  --spacing-1: 0.25rem;     /* 4px */
  --spacing-1-5: 0.375rem;  /* 6px */
  --spacing-2: 0.5rem;      /* 8px */
  --spacing-2-5: 0.625rem;  /* 10px */
  --spacing-3: 0.75rem;     /* 12px */
  --spacing-3-5: 0.875rem;  /* 14px */
  --spacing-4: 1rem;        /* 16px */
  --spacing-5: 1.25rem;     /* 20px */
  --spacing-6: 1.5rem;      /* 24px */
  --spacing-7: 1.75rem;     /* 28px */
  --spacing-8: 2rem;        /* 32px */
  --spacing-9: 2.25rem;     /* 36px */
  --spacing-10: 2.5rem;     /* 40px */
  --spacing-11: 2.75rem;    /* 44px */
  --spacing-12: 3rem;       /* 48px */
  --spacing-14: 3.5rem;     /* 56px */
  --spacing-16: 4rem;       /* 64px */
  --spacing-20: 5rem;       /* 80px */
  --spacing-24: 6rem;       /* 96px */
  --spacing-28: 7rem;       /* 112px */
  --spacing-32: 8rem;       /* 128px */
  
  /* === ALIAS SÉMANTIQUES === */
  --gap-xs: var(--spacing-1);
  --gap-sm: var(--spacing-2);
  --gap-md: var(--spacing-4);
  --gap-lg: var(--spacing-6);
  --gap-xl: var(--spacing-8);
}
```

### Bordures et Arrondis

```css
:root {
  /* === LARGEUR DE BORDURE === */
  --border-width-0: 0;
  --border-width-1: 1px;
  --border-width-2: 2px;
  --border-width-4: 4px;
  --border-width-8: 8px;
  
  /* === RAYON DE BORDURE === */
  --radius-none: 0;
  --radius-sm: 0.125rem;    /* 2px */
  --radius-md: 0.25rem;     /* 4px */
  --radius-lg: 0.5rem;      /* 8px */
  --radius-xl: 0.75rem;     /* 12px */
  --radius-2xl: 1rem;       /* 16px */
  --radius-3xl: 1.5rem;     /* 24px */
  --radius-full: 9999px;
  
  /* === STYLES DE BORDURE COURANTS === */
  --border-default: var(--border-width-1) solid var(--color-border);
  --border-focus: var(--border-width-2) solid var(--color-primary);
}
```

### Ombres

```css
:root {
  /* === OMBRES DE BOÎTE === */
  --shadow-xs: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  --shadow-sm: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 
               0 1px 2px -1px rgba(0, 0, 0, 0.1);
  --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 
               0 2px 4px -2px rgba(0, 0, 0, 0.1);
  --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 
               0 4px 6px -4px rgba(0, 0, 0, 0.1);
  --shadow-xl: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 
               0 8px 10px -6px rgba(0, 0, 0, 0.1);
  --shadow-2xl: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  --shadow-inner: inset 0 2px 4px 0 rgba(0, 0, 0, 0.05);
  --shadow-none: 0 0 #0000;
  
  /* === OMBRES COLORÉES === */
  --shadow-primary: 0 4px 14px 0 rgba(2, 136, 209, 0.39);
  --shadow-error: 0 4px 14px 0 rgba(244, 67, 54, 0.39);
}
```

### Transitions et Animations

```css
:root {
  /* === DURÉES === */
  --duration-75: 75ms;
  --duration-100: 100ms;
  --duration-150: 150ms;
  --duration-200: 200ms;
  --duration-300: 300ms;
  --duration-500: 500ms;
  --duration-700: 700ms;
  --duration-1000: 1000ms;
  
  /* === FONCTIONS DE TIMING === */
  --ease-linear: linear;
  --ease-in: cubic-bezier(0.4, 0, 1, 1);
  --ease-out: cubic-bezier(0, 0, 0.2, 1);
  --ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
  --ease-bounce: cubic-bezier(0.68, -0.55, 0.265, 1.55);
  
  /* === TRANSITIONS PRÉDÉFINIES === */
  --transition-none: none;
  --transition-all: all var(--duration-200) var(--ease-in-out);
  --transition-colors: background-color, border-color, color, fill, stroke 
                       var(--duration-200) var(--ease-in-out);
  --transition-opacity: opacity var(--duration-200) var(--ease-in-out);
  --transition-shadow: box-shadow var(--duration-200) var(--ease-in-out);
  --transition-transform: transform var(--duration-200) var(--ease-in-out);
}
```

### Z-Index

```css
:root {
  /* === ÉCHELLE Z-INDEX === */
  --z-auto: auto;
  --z-0: 0;
  --z-10: 10;
  --z-20: 20;
  --z-30: 30;
  --z-40: 40;
  --z-50: 50;
  
  /* === COUCHES SÉMANTIQUES === */
  --z-base: var(--z-0);
  --z-dropdown: 1000;
  --z-sticky: 1020;
  --z-fixed: 1030;
  --z-modal-backdrop: 1040;
  --z-modal: 1050;
  --z-popover: 1060;
  --z-tooltip: 1070;
  --z-toast: 1080;
}
```

## 📱 Variables Responsive

### Container Queries avec Variables

```css
/* Définir les largeurs de container */
:root {
  --container-sm: 640px;
  --container-md: 768px;
  --container-lg: 1024px;
  --container-xl: 1280px;
  --container-2xl: 1536px;
}

/* Container principal */
.container {
  --container-padding: var(--spacing-4);
  
  width: 100%;
  max-width: var(--container-xl);
  margin-inline: auto;
  padding-inline: var(--container-padding);
}

@media (min-width: 768px) {
  .container {
    --container-padding: var(--spacing-6);
  }
}

@media (min-width: 1024px) {
  .container {
    --container-padding: var(--spacing-8);
  }
}
```

### Variables de Breakpoint

```css
:root {
  /* Breakpoints pour référence (non utilisables directement en media queries) */
  --breakpoint-sm: 640px;
  --breakpoint-md: 768px;
  --breakpoint-lg: 1024px;
  --breakpoint-xl: 1280px;
  --breakpoint-2xl: 1536px;
}
```

## ⚠️ Bonnes Pratiques Avancées

### Fallbacks et Valeurs par Défaut

```css
/* ✅ BON : Toujours fournir un fallback */
.element {
  color: var(--color-text-primary, #212121);
  padding: var(--spacing-md, 1rem);
  border-radius: var(--radius-lg, 8px);
}

/* ✅ BON : Chaînage de variables avec fallback final */
.element {
  background-color: var(--button-bg, var(--color-primary, #0288d1));
}

/* ❌ MAUVAIS : Pas de fallback */
.element {
  color: var(--undefined-variable);
}
```

### Scoping de Variables

```css
/* Variables globales dans :root */
:root {
  --color-primary: #0288d1;
}

/* Variables scopées au composant */
.card {
  --card-padding: var(--spacing-4);
  --card-radius: var(--radius-lg);
  
  padding: var(--card-padding);
  border-radius: var(--card-radius);
}

/* Override pour une variante */
.card--compact {
  --card-padding: var(--spacing-2);
}

/* Override en contexte */
.sidebar .card {
  --card-padding: var(--spacing-3);
}
```

### Calculs avec Variables

```css
:root {
  --base-size: 1rem;
  --scale-ratio: 1.25;
}

.text-xs { font-size: calc(var(--base-size) / var(--scale-ratio)); }
.text-sm { font-size: var(--base-size); }
.text-md { font-size: calc(var(--base-size) * var(--scale-ratio)); }
.text-lg { font-size: calc(var(--base-size) * var(--scale-ratio) * var(--scale-ratio)); }

/* Espacement dynamique */
.spacing-dynamic {
  --multiplier: 1;
  padding: calc(var(--spacing-4) * var(--multiplier));
}

.spacing-dynamic.large {
  --multiplier: 2;
}
```
