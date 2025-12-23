---
description: CSS Animations - Transitions, keyframes, motion design, performance, accessibilité
name: CSS_Animations
applyTo: "**/*.css,**/*.scss,**/*.vue"
---

# CSS Animations et Transitions

Guide complet pour créer des animations fluides, performantes et accessibles.

## ⛔ À NE PAS FAIRE

- **N'anime jamais** les propriétés `width`, `height`, `top`, `left` (coûteux)
- **N'utilise jamais** d'animations infinies sans option de pause
- **N'ignore jamais** `prefers-reduced-motion`
- **N'applique jamais** `will-change` sur trop d'éléments
- **Ne crée jamais** d'animations qui durent plus de 500ms sans raison
- **N'utilise jamais** de délais supérieurs à 200ms pour les interactions

## ✅ À FAIRE

- **Anime toujours** `transform` et `opacity` (GPU-accelerated)
- **Respecte toujours** les préférences de mouvement réduit
- **Utilise toujours** des durées appropriées (150-300ms pour les interactions)
- **Préfère toujours** `cubic-bezier` pour des animations naturelles
- **Utilise toujours** `will-change` avec parcimonie et retrait après
- **Teste toujours** les performances avec DevTools Performance tab

## ⚡ Transitions

### Transitions de Base

```css
/* Transition simple */
.button {
  background-color: var(--color-primary);
  transition: background-color 200ms ease;
}

.button:hover {
  background-color: var(--color-primary-hover);
}

/* Transition multiple */
.card {
  transition: 
    transform 200ms ease,
    box-shadow 200ms ease;
}

.card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-lg);
}

/* Transition avec délai (staggered) */
.nav-item {
  opacity: 0;
  transform: translateY(-10px);
  transition: 
    opacity 200ms ease,
    transform 200ms ease;
}

.nav-item:nth-child(1) { transition-delay: 0ms; }
.nav-item:nth-child(2) { transition-delay: 50ms; }
.nav-item:nth-child(3) { transition-delay: 100ms; }
.nav-item:nth-child(4) { transition-delay: 150ms; }

.nav.is-open .nav-item {
  opacity: 1;
  transform: translateY(0);
}
```

### Fonctions de Timing

```css
:root {
  /* Fonctions prédéfinies */
  --ease-linear: linear;
  --ease-in: ease-in;        /* Démarre lentement */
  --ease-out: ease-out;      /* Finit lentement */
  --ease-in-out: ease-in-out;
  
  /* Cubic-bezier personnalisées */
  --ease-in-quad: cubic-bezier(0.55, 0.085, 0.68, 0.53);
  --ease-out-quad: cubic-bezier(0.25, 0.46, 0.45, 0.94);
  --ease-in-out-quad: cubic-bezier(0.455, 0.03, 0.515, 0.955);
  
  --ease-in-cubic: cubic-bezier(0.55, 0.055, 0.675, 0.19);
  --ease-out-cubic: cubic-bezier(0.215, 0.61, 0.355, 1);
  --ease-in-out-cubic: cubic-bezier(0.645, 0.045, 0.355, 1);
  
  --ease-in-expo: cubic-bezier(0.95, 0.05, 0.795, 0.035);
  --ease-out-expo: cubic-bezier(0.19, 1, 0.22, 1);
  
  /* Bounce et Elastic */
  --ease-out-back: cubic-bezier(0.34, 1.56, 0.64, 1);
  --ease-in-back: cubic-bezier(0.6, -0.28, 0.735, 0.045);
  --ease-bounce: cubic-bezier(0.68, -0.55, 0.265, 1.55);
  
  /* Spring-like */
  --ease-spring: cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

/* Usage */
.dropdown {
  transform: scaleY(0);
  transform-origin: top;
  transition: transform 250ms var(--ease-out-back);
}

.dropdown.is-open {
  transform: scaleY(1);
}
```

### Durées Recommandées

```css
:root {
  /* Micro-interactions (hover, focus) */
  --duration-instant: 75ms;
  --duration-fast: 150ms;
  
  /* Interactions standard (click, toggle) */
  --duration-normal: 200ms;
  --duration-moderate: 250ms;
  
  /* Transitions complexes (modals, pages) */
  --duration-slow: 300ms;
  --duration-slower: 400ms;
  
  /* Animations décoratives */
  --duration-decorative: 500ms;
}

/* Exemples d'utilisation */
.button {
  /* Interaction rapide */
  transition: all var(--duration-fast) ease;
}

.modal {
  /* Apparition plus lente */
  transition: all var(--duration-slow) ease;
}

.page-transition {
  /* Transition de page */
  transition: all var(--duration-slower) ease;
}
```

## 🎬 Keyframes

### Animations de Base

```css
/* Fade In */
@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

/* Fade In Up */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Fade In Down */
@keyframes fadeInDown {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Slide In Left */
@keyframes slideInLeft {
  from {
    opacity: 0;
    transform: translateX(-30px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

/* Slide In Right */
@keyframes slideInRight {
  from {
    opacity: 0;
    transform: translateX(30px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

/* Scale In */
@keyframes scaleIn {
  from {
    opacity: 0;
    transform: scale(0.9);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

/* Usage */
.animate-fadeIn {
  animation: fadeIn 300ms ease forwards;
}

.animate-fadeInUp {
  animation: fadeInUp 400ms ease forwards;
}
```

### Animations d'Attention

```css
/* Pulse */
@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

/* Ping (notification) */
@keyframes ping {
  75%, 100% {
    transform: scale(2);
    opacity: 0;
  }
}

/* Bounce */
@keyframes bounce {
  0%, 100% {
    transform: translateY(0);
    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
  }
  50% {
    transform: translateY(-25%);
    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
  }
}

/* Shake */
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  10%, 30%, 50%, 70%, 90% { transform: translateX(-4px); }
  20%, 40%, 60%, 80% { transform: translateX(4px); }
}

/* Wiggle */
@keyframes wiggle {
  0%, 100% { transform: rotate(0deg); }
  25% { transform: rotate(-5deg); }
  75% { transform: rotate(5deg); }
}

/* Usage */
.notification-badge::after {
  content: '';
  position: absolute;
  inset: 0;
  border-radius: 50%;
  background: inherit;
  animation: ping 1s cubic-bezier(0, 0, 0.2, 1) infinite;
}

.error-input {
  animation: shake 500ms ease;
}
```

### Animations de Chargement

```css
/* Spin */
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* Spinner avec points */
@keyframes dotPulse {
  0%, 80%, 100% {
    transform: scale(0);
    opacity: 0;
  }
  40% {
    transform: scale(1);
    opacity: 1;
  }
}

/* Skeleton loading */
@keyframes shimmer {
  0% {
    background-position: -200% 0;
  }
  100% {
    background-position: 200% 0;
  }
}

/* Progress indeterminate */
@keyframes indeterminate {
  0% {
    transform: translateX(-100%);
  }
  100% {
    transform: translateX(400%);
  }
}

/* Usage */
.spinner {
  width: 24px;
  height: 24px;
  border: 2px solid var(--color-gray-200);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 800ms linear infinite;
}

.skeleton {
  background: linear-gradient(
    90deg,
    var(--color-gray-200) 25%,
    var(--color-gray-100) 50%,
    var(--color-gray-200) 75%
  );
  background-size: 200% 100%;
  animation: shimmer 1.5s ease-in-out infinite;
}

.loading-dots span {
  display: inline-block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--color-primary);
  animation: dotPulse 1.4s ease-in-out infinite;
}

.loading-dots span:nth-child(1) { animation-delay: -0.32s; }
.loading-dots span:nth-child(2) { animation-delay: -0.16s; }
.loading-dots span:nth-child(3) { animation-delay: 0s; }
```

## 🎭 Animations Vue.js

### Transition Components

```css
/* === FADE === */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 200ms ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* === SLIDE FADE === */
.slide-fade-enter-active {
  transition: all 300ms ease-out;
}

.slide-fade-leave-active {
  transition: all 200ms ease-in;
}

.slide-fade-enter-from,
.slide-fade-leave-to {
  opacity: 0;
  transform: translateX(20px);
}

/* === SCALE === */
.scale-enter-active,
.scale-leave-active {
  transition: all 200ms ease;
}

.scale-enter-from,
.scale-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

/* === EXPAND (height) === */
.expand-enter-active,
.expand-leave-active {
  transition: all 300ms ease;
  overflow: hidden;
}

.expand-enter-from,
.expand-leave-to {
  opacity: 0;
  max-height: 0;
  padding-top: 0;
  padding-bottom: 0;
}

/* === MODAL === */
.modal-enter-active,
.modal-leave-active {
  transition: all 300ms ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
  transform: scale(0.95) translateY(-20px);
}

/* === OVERLAY === */
.overlay-enter-active,
.overlay-leave-active {
  transition: opacity 200ms ease;
}

.overlay-enter-from,
.overlay-leave-to {
  opacity: 0;
}

/* === DRAWER === */
.drawer-enter-active,
.drawer-leave-active {
  transition: transform 300ms ease;
}

.drawer-enter-from,
.drawer-leave-to {
  transform: translateX(-100%);
}

.drawer-right-enter-from,
.drawer-right-leave-to {
  transform: translateX(100%);
}
```

### Transitions de Liste

```css
/* === LIST ANIMATION === */
.list-enter-active,
.list-leave-active {
  transition: all 300ms ease;
}

.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translateX(-30px);
}

/* Important: pour les éléments qui restent */
.list-move {
  transition: transform 300ms ease;
}

/* Pour éviter les sauts lors de la suppression */
.list-leave-active {
  position: absolute;
}

/* === STAGGER ANIMATION === */
.stagger-enter-active {
  transition: all 400ms ease;
}

.stagger-enter-from {
  opacity: 0;
  transform: translateY(20px);
}

/* Délais calculés via CSS custom properties */
.stagger-item {
  transition-delay: calc(var(--index, 0) * 50ms);
}

/* === GRID ANIMATION === */
.grid-enter-active,
.grid-leave-active,
.grid-move {
  transition: all 500ms ease;
}

.grid-enter-from {
  opacity: 0;
  transform: scale(0.5);
}

.grid-leave-to {
  opacity: 0;
  transform: scale(0.5);
}

.grid-leave-active {
  position: absolute;
}
```

## ⚡ Performance

### Propriétés Optimisées

```css
/* ✅ PERFORMANT : transform et opacity */
.card-hover {
  transition: transform 200ms ease, opacity 200ms ease;
}

.card-hover:hover {
  transform: translateY(-4px) scale(1.02);
  opacity: 0.95;
}

/* ✅ PERFORMANT : filter (GPU) */
.blur-on-hover {
  transition: filter 200ms ease;
}

.blur-on-hover:hover {
  filter: blur(2px);
}

/* ❌ NON PERFORMANT : propriétés de layout */
.bad-animation {
  /* Ces propriétés déclenchent un recalcul du layout */
  transition: width 200ms, height 200ms, padding 200ms, margin 200ms;
}

/* ✅ ALTERNATIVE PERFORMANTE */
.good-animation {
  transform: scale(1);
  transition: transform 200ms ease;
}

.good-animation:hover {
  transform: scale(1.1);
}
```

### Will-Change

```css
/* Utiliser will-change avec parcimonie */
.animated-element {
  /* Annoncer au navigateur les propriétés qui vont changer */
  will-change: transform, opacity;
}

/* Retirer will-change après l'animation */
.animated-element.animation-done {
  will-change: auto;
}

/* ❌ MAUVAIS : will-change sur tout */
* {
  will-change: transform; /* NON! */
}

/* ✅ BON : ciblé et temporaire */
.modal.is-animating {
  will-change: transform, opacity;
}

.modal:not(.is-animating) {
  will-change: auto;
}
```

### Contain Property

```css
/* Isoler les recalculs de layout */
.animation-container {
  contain: layout style;
}

/* Pour les éléments animés fréquemment */
.frequent-animation {
  contain: strict;
  content-visibility: auto;
}
```

## ♿ Accessibilité

### Respect des Préférences

```css
/* Toujours respecter prefers-reduced-motion */
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

/* Alternative: réduire plutôt que supprimer */
@media (prefers-reduced-motion: reduce) {
  .animated-element {
    /* Garder une transition minimale pour le feedback */
    transition-duration: 100ms;
    animation: none;
  }
  
  /* Remplacer les animations par des changements simples */
  .slide-in {
    animation: none;
    opacity: 1;
    transform: none;
  }
}
```

### Animations Pausables

```css
/* Bouton de pause pour animations */
.animation-container {
  --animation-state: running;
}

.animation-container.paused {
  --animation-state: paused;
}

.animated-element {
  animation: myAnimation 2s infinite;
  animation-play-state: var(--animation-state);
}

/* Pause au hover pour les utilisateurs */
.auto-scroll:hover {
  animation-play-state: paused;
}

/* Focus visible pour les contrôles */
.animation-control:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}
```

### Focus Animations

```css
/* Animation de focus accessible */
.focusable {
  outline: 2px solid transparent;
  outline-offset: 2px;
  transition: outline-color 150ms ease;
}

.focusable:focus-visible {
  outline-color: var(--color-primary);
}

/* Animation de focus plus visible */
@keyframes focusPulse {
  0%, 100% {
    outline-offset: 2px;
  }
  50% {
    outline-offset: 4px;
  }
}

.focusable:focus-visible {
  outline: 2px solid var(--color-primary);
  animation: focusPulse 1s ease infinite;
}

@media (prefers-reduced-motion: reduce) {
  .focusable:focus-visible {
    animation: none;
  }
}
```

## 🎨 Patterns d'Animation

### Micro-interactions

```css
/* Button press effect */
.button {
  transition: transform 100ms ease;
}

.button:active {
  transform: scale(0.98);
}

/* Checkbox animation */
.checkbox-indicator {
  transform: scale(0);
  transition: transform 200ms var(--ease-spring);
}

.checkbox-input:checked + .checkbox-indicator {
  transform: scale(1);
}

/* Toggle switch */
.toggle-thumb {
  transform: translateX(0);
  transition: transform 200ms var(--ease-out-back);
}

.toggle-input:checked + .toggle-track .toggle-thumb {
  transform: translateX(24px);
}

/* Input focus */
.input-field {
  border-bottom: 2px solid var(--color-gray-300);
  transition: border-color 200ms ease;
}

.input-field:focus {
  border-color: var(--color-primary);
}

/* Input label animation */
.floating-label {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  transition: 
    top 200ms ease,
    transform 200ms ease,
    font-size 200ms ease;
}

.input-field:focus + .floating-label,
.input-field:not(:placeholder-shown) + .floating-label {
  top: 0;
  transform: translateY(-100%);
  font-size: 0.75rem;
}
```

### Feedback Visuel

```css
/* Success animation */
@keyframes successPop {
  0% {
    transform: scale(0);
    opacity: 0;
  }
  50% {
    transform: scale(1.2);
  }
  100% {
    transform: scale(1);
    opacity: 1;
  }
}

.success-icon {
  animation: successPop 400ms var(--ease-out-back) forwards;
}

/* Error shake */
.error-field {
  animation: shake 400ms ease;
}

/* Like/Heart animation */
@keyframes heartBeat {
  0% { transform: scale(1); }
  14% { transform: scale(1.3); }
  28% { transform: scale(1); }
  42% { transform: scale(1.3); }
  70% { transform: scale(1); }
}

.like-button.liked .heart-icon {
  animation: heartBeat 1s ease-in-out;
}

/* Copy to clipboard feedback */
@keyframes copyFeedback {
  0% {
    opacity: 0;
    transform: translateY(10px);
  }
  20% {
    opacity: 1;
    transform: translateY(0);
  }
  80% {
    opacity: 1;
    transform: translateY(0);
  }
  100% {
    opacity: 0;
    transform: translateY(-10px);
  }
}

.copy-feedback {
  animation: copyFeedback 2s ease forwards;
}
```

### Page Transitions

```css
/* Page enter */
@keyframes pageEnter {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.page-content {
  animation: pageEnter 400ms ease forwards;
}

/* Staggered page content */
.page-header {
  animation: pageEnter 400ms ease forwards;
  animation-delay: 0ms;
}

.page-main {
  animation: pageEnter 400ms ease forwards;
  animation-delay: 100ms;
}

.page-sidebar {
  animation: pageEnter 400ms ease forwards;
  animation-delay: 200ms;
}

/* View transition API (modern browsers) */
::view-transition-old(root) {
  animation: fadeOut 200ms ease;
}

::view-transition-new(root) {
  animation: fadeIn 200ms ease;
}
```
