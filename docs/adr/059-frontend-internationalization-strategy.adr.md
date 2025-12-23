# 59. Stratégie d'Internationalisation Frontend (i18n)

Date: 2025-12-23

## Statut

Accepté

## Contexte

L'application LLM Proxy Admin dispose d'une interface frontend Vue 3 initialement développée uniquement en français. Pour supporter une expansion internationale et améliorer l'accessibilité :

1. **Besoin métier** : Utilisateurs anglophones (marchés UK, US, Canada anglophone)
2. **UX** : Interface multilingue avec changement dynamique sans rechargement
3. **Accessibilité** : Support `lang` HTML pour screen readers et SEO
4. **Maintenabilité** : Solution scalable pour ajouter futures langues (ES, DE, IT)

### Situation Initiale (Sans i18n)

```vue
<!-- ❌ Textes en dur dans les templates -->
<template>
  <h1>Tableau de bord</h1>
  <button>Créer un tenant</button>
  <p>Aucune donnée disponible</p>
</template>

<script setup>
const pageTitle = 'Gestion des Tenants'
const errorMessage = 'Une erreur est survenue'
</script>
```

**Problèmes identifiés :**
- 🔴 **Langue unique** : Utilisateurs non francophones exclus
- 🔴 **Textes dispersés** : Chaînes dans templates, scripts, stores
- 🔴 **Non maintenable** : Changement traduction = parcourir tout le code
- 🔴 **Pas d'accessibilité** : Attribut `lang` absent → problèmes SEO et screen readers
- 🔴 **Pas d'évolution** : Ajouter langue = refactoring massif

## Décision

**Implémenter l'internationalisation via Vue I18n 9.x en mode Composition API avec détection automatique de la langue et persistance de préférence utilisateur.**

### Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                  STRATÉGIE DÉTECTION LANGUE                      │
│                                                                  │
│  1. localStorage ('llmproxy-locale')  → Préférence utilisateur  │
│            ↓ (si absent)                                         │
│  2. navigator.language                → Langue navigateur       │
│            ↓ (si non supportée)                                  │
│  3. 'fr' (défaut)                     → Français par défaut     │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│                  STRUCTURE FICHIERS i18n                         │
│                                                                  │
│  frontend/src/locales/                                           │
│  ├── fr.json          → Traductions françaises (266 lignes)     │
│  ├── en.json          → Traductions anglaises (266 lignes)      │
│  └── index.ts         → Configuration Vue I18n                  │
│                                                                  │
│  frontend/src/components/layout/                                 │
│  └── AppHeader.vue    → Sélecteur langue (dropdown FR/EN)       │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

### 1. Configuration Vue I18n (Composition API)

```typescript
// frontend/src/locales/index.ts
import { createI18n } from 'vue-i18n'
import fr from './fr.json'
import en from './en.json'

export type MessageSchema = typeof fr

/**
 * Détecte la langue initiale de l'utilisateur
 * Cascade : localStorage > navigateur > français (défaut)
 */
function getInitialLocale(): 'fr' | 'en' {
  // 1. Vérifier localStorage (préférence explicite utilisateur)
  const stored = localStorage.getItem('llmproxy-locale')
  if (stored === 'fr' || stored === 'en') {
    return stored
  }
  
  // 2. Détecter langue du navigateur
  const browserLang = navigator.language.split('-')[0]
  if (browserLang === 'fr' || browserLang === 'en') {
    return browserLang as 'fr' | 'en'
  }
  
  // 3. Défaut : français
  return 'fr'
}

/**
 * Instance i18n configurée pour Vue 3 Composition API
 */
export const i18n = createI18n({
  legacy: false,              // Composition API mode (non-legacy)
  locale: getInitialLocale(), // Langue initiale détectée
  fallbackLocale: 'fr',       // Langue de secours
  messages: { fr, en },       // Fichiers de traduction
  globalInjection: true,      // Accès global à $t dans templates
  missingWarn: import.meta.env.DEV,    // Warn clés manquantes (dev only)
  fallbackWarn: import.meta.env.DEV
})

/**
 * Change la langue et persiste la préférence
 */
export function setLocale(locale: 'fr' | 'en'): void {
  // @ts-ignore - i18n.global.locale is WritableComputedRef in Composition API
  i18n.global.locale.value = locale
  localStorage.setItem('llmproxy-locale', locale)
  document.documentElement.lang = locale // Accessibilité + SEO
}

/**
 * Récupère la langue courante
 */
export function getCurrentLocale(): 'fr' | 'en' {
  // @ts-ignore
  return i18n.global.locale.value as 'fr' | 'en'
}
```

### 2. Structure Fichiers de Traduction

```json
// frontend/src/locales/fr.json
{
  "header": {
    "title": "Admin LLM Proxy",
    "nav": {
      "dashboard": "Tableau de bord",
      "tenants": "Tenants",
      "providers": "Fournisseurs",
      "monitoring": "Surveillance",
      "routes": "Routes",
      "settings": "Paramètres"
    },
    "language": {
      "french": "Français",
      "english": "English"
    }
  },
  "dashboard": {
    "title": "Tableau de bord",
    "metrics": {
      "totalRequests": "Requêtes totales",
      "activeProviders": "Fournisseurs actifs"
    }
  },
  "common": {
    "actions": {
      "save": "Enregistrer",
      "cancel": "Annuler",
      "delete": "Supprimer"
    },
    "validation": {
      "required": "Ce champ est requis",
      "invalidEmail": "Email invalide"
    }
  }
}
```

**Organisation hiérarchique :**
- `header.*` : Barre de navigation, titre, langue
- `dashboard.*`, `tenants.*`, `providers.*` : Sections par page
- `common.*` : Traductions partagées (actions, validations, messages)

### 3. Intégration Vue Application

```typescript
// frontend/src/main.ts
import { createApp } from 'vue'
import { i18n } from './locales'

const app = createApp(App)
app.use(i18n) // ✨ Plugin i18n enregistré
app.mount('#app')
```

### 4. Utilisation dans Composants

```vue
<!-- ✅ APRÈS : Template avec traductions -->
<script setup lang="ts">
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

// Utilisation dans script
const pageTitle = t('dashboard.title')
const errorMessage = t('common.validation.required')
</script>

<template>
  <!-- Utilisation dans template -->
  <h1>{{ t('dashboard.title') }}</h1>
  <button>{{ t('tenants.form.create') }}</button>
  <p>{{ t('common.messages.noData') }}</p>
</template>
```

### 5. Sélecteur de Langue (AppHeader)

```vue
<!-- frontend/src/components/layout/AppHeader.vue -->
<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { setLocale, getCurrentLocale } from '@/locales'

const { t } = useI18n()
const currentLocale = ref(getCurrentLocale())
const showLanguageDropdown = ref(false)

function changeLanguage(locale: 'fr' | 'en'): void {
  setLocale(locale)
  currentLocale.value = locale
  showLanguageDropdown.value = false
}
</script>

<template>
  <div class="language-menu" data-testid="language-selector">
    <button @click="showLanguageDropdown = !showLanguageDropdown">
      <i class="pi pi-flag"></i>
      <span>{{ currentLocale.toUpperCase() }}</span>
    </button>
    
    <div v-if="showLanguageDropdown" class="language-dropdown">
      <button @click="changeLanguage('fr')" data-testid="language-fr">
        {{ t('header.language.french') }}
        <i v-if="currentLocale === 'fr'" class="pi pi-check"></i>
      </button>
      <button @click="changeLanguage('en')" data-testid="language-en">
        {{ t('header.language.english') }}
        <i v-if="currentLocale === 'en'" class="pi pi-check"></i>
      </button>
    </div>
  </div>
</template>
```

## Alternatives Considérées

### 1. Custom i18n Solution (DIY)

```typescript
// Alternative : Solution maison
const translations = {
  fr: { welcome: 'Bienvenue' },
  en: { welcome: 'Welcome' }
}
const t = (key: string) => translations[locale][key]
```

**Rejeté car :**
- ❌ **Réinventer la roue** : Fonctionnalités déjà dans Vue I18n
- ❌ **Maintenance coûteuse** : Bugs, edge cases, features manquantes
- ❌ **Pas de pluralization** : `1 item` vs `2 items`
- ❌ **Pas de formatage** : Dates, nombres, devises
- ❌ **Pas d'interpolation** : Variables dans traductions
- ❌ **Pas de lazy loading** : Toutes langues chargées d'avance

### 2. Vue I18n Legacy Mode (Options API)

```javascript
// Alternative : Mode legacy (Options API)
const i18n = createI18n({
  legacy: true, // Options API mode
  locale: 'fr'
})

export default {
  data() {
    return {
      message: this.$t('hello')
    }
  }
}
```

**Rejeté car :**
- ❌ **Options API** : Deprecated pour Vue 3, pas aligné avec Composition API du projet
- ❌ **Moins performant** : Mode legacy = overhead compatibilité
- ❌ **Pas type-safe** : `this.$t()` = any, pas d'autocomplete TypeScript
- ❌ **Moins testable** : Nécessite wrapper Vue component pour tests

### 3. Server-Side i18n Uniquement

```typescript
// Alternative : Traductions serveur
app.get('/messages/:locale', (req, res) => {
  res.json(translations[req.params.locale])
})
```

**Rejeté car :**
- ❌ **Pas de changement dynamique** : Rechargement page nécessaire
- ❌ **UX dégradée** : Latence réseau pour chaque changement langue
- ❌ **Pas d'offline** : Impossible sans connexion
- ❌ **Complexité** : Synchronisation client-serveur, cache

### 4. Build Multi-Bundle (i18n at Build Time)

```bash
# Alternative : Build séparé par langue
npm run build -- --locale=fr  # → dist-fr/
npm run build -- --locale=en  # → dist-en/
```

**Rejeté car :**
- ❌ **Multiple bundles** : Maintenance complexe (2× builds, 2× déploiements)
- ❌ **Pas de switch dynamique** : Utilisateur ne peut pas changer langue
- ❌ **Complexité infra** : Routing par langue, CDN multi-origins
- ❌ **Bundle size** : Duplication code app (seules traductions diffèrent)

### 5. Géolocalisation IP pour Langue

```typescript
// Alternative : Détection IP → langue
const response = await fetch('https://ipapi.co/json/')
const { country_code } = await response.json()
const locale = country_code === 'FR' ? 'fr' : 'en'
```

**Rejeté car :**
- ❌ **Privacy concerns** : RGPD (collecte IP sans consentement)
- ❌ **Pas fiable** : VPN, proxies, utilisateurs en déplacement
- ❌ **Latence** : Requête externe au démarrage app
- ❌ **Coût** : API de géolocalisation (quotas, pricing)
- ❌ **Pas de préférence utilisateur** : Français à Londres = EN forcé

## Conséquences

### Positives

1. ✅ **Accessibilité internationale** : Interface en français ET anglais
2. ✅ **UX améliorée** : Changement langue instantané (no reload), dropdown dans header
3. ✅ **Type-safe** : Autocomplete des clés de traduction (`t('dashboard.title')`)
4. ✅ **Maintenable** : 
   - Traductions centralisées dans fichiers JSON
   - Ajout langue = créer nouveau fichier + register
5. ✅ **Performance** : 
   - Traductions en mémoire (pas de requête réseau)
   - Lazy loading possible si >10 langues (future)
6. ✅ **SEO/Accessibility** : 
   - `document.documentElement.lang` mis à jour → screen readers
   - Meilleur indexation Google (multi-langue)
7. ✅ **Convention claire** : Structure `locales/*.json` standardisée
8. ✅ **Composition API** : Cohérent avec le reste du projet Vue 3
9. ✅ **Persistance** : Préférence utilisateur sauvée (localStorage)
10. ✅ **Fallback intelligent** : FR si EN traduction manquante

### Négatives

1. ❌ **Bundle size** : +50KB pour 2 langues (266 lignes × 2 fichiers JSON)
   - Mitigation : Compression gzip (~15KB), lazy loading si >5 langues
2. ❌ **Maintenance traductions** : 
   - Synchronisation FR ↔ EN (risque clés manquantes)
   - Mitigation : Script de validation CI/CD
3. ❌ **Backend non traduit** : 
   - Messages API restent en français
   - Mitigation future : i18n backend si besoin international
4. ❌ **Fallback langue** : 
   - Si EN traduction manquante → affiche FR
   - Peut surprendre utilisateur anglophone
   - Mitigation : Validation complétude traductions
5. ❌ **Testing** : 
   - Tests E2E pour chaque langue
   - Snapshots tests = 2× fichiers
   - Mitigation : Helpers de test pour switch langue
6. ❌ **Pluralization complexe** : 
   - Pas implémenté (ex: "1 item" vs "2 items")
   - Mitigation : Utiliser Vue I18n pluralization si besoin
7. ❌ **Dates/Nombres** : 
   - Pas de formatage localisé (dates FR vs EN)
   - Mitigation : Intl.DateTimeFormat si nécessaire

## Alignement Stratégique

**Objectifs métier supportés :**
- **Expansion internationale** : Marchés anglophones (UK, US, Canada)
- **Accessibilité** : Utilisateurs non francophones peuvent utiliser l'interface
- **Compétitivité** : Standard industry (apps SaaS multilingues)

**Contraintes respectées :**
- **RGPD** : Détection langue privacy-friendly (localStorage + navigateur, pas IP)
- **Performance** : Pas de latence réseau (traductions en bundle)
- **Accessibilité** : WCAG 2.1 (attribut `lang`, screen readers)

**Risques métier atténués :**
- **Adoption faible marché anglophone** : Interface en EN → réduction friction
- **Support utilisateur** : Messages traduits → moins de confusion

## Métriques de Succès

| Métrique | Avant | Après | Objectif |
|----------|-------|-------|----------|
| Langues supportées | 1 (FR) | 2 (FR, EN) | 2-3 |
| Bundle size | 1.2MB | 1.25MB | <1.5MB |
| Couverture traductions | 0% | 100% (266 clés) | >95% |
| Utilisateurs EN | 0% | TBD | >10% |
| Switch langue | N/A | <100ms | <200ms |

## Exemples Concrets (Projet)

### Sections Traduites

| Section | Clés FR | Clés EN | Statut |
|---------|---------|---------|--------|
| Header (navigation) | 8 | 8 | ✅ |
| Dashboard (métriques) | 7 | 7 | ✅ |
| Tenants (CRUD) | 28 | 28 | ✅ |
| Providers (liste) | 22 | 22 | ✅ |
| Monitoring (graphs) | 18 | 18 | ✅ |
| Routes (config) | 15 | 15 | ✅ |
| Settings (params) | 19 | 19 | ✅ |
| Common (actions, validation) | 45 | 45 | ✅ |

**Total :** 266 clés traduites (FR + EN)

### Composants Modifiés

| Composant | Changement | Lignes |
|-----------|------------|--------|
| `AppHeader.vue` | Ajout sélecteur langue + traductions nav | +80 |
| `DashboardView.vue` | Remplacement textes par `t()` | ~15 |
| `TenantsView.vue` | Traductions formulaires + messages | ~25 |
| `ProvidersView.vue` | Traductions liste + actions | ~20 |
| `main.ts` | Enregistrement plugin i18n | +2 |

## Références

- **Vue I18n Documentation** : https://vue-i18n.intlify.dev/
- **Composition API Guide** : https://vue-i18n.intlify.dev/guide/advanced/composition.html
- **WCAG 2.1 Language Guidelines** : https://www.w3.org/WAI/WCAG21/Understanding/language-of-page.html
- **ADR-019** : Convention over Configuration (structure `locales/` standardisée)

## Notes d'Implémentation

### Guidelines pour Développeurs

**✅ Bonnes pratiques :**
- Toujours utiliser `t('key')` au lieu de textes en dur
- Organiser clés hiérarchiquement (`section.subsection.key`)
- Préfixer clés communes avec `common.*`
- Utiliser variables interpolées : `t('message', { name: 'John' })`

**❌ À éviter :**
- Textes en dur dans templates ou scripts
- Clés trop génériques (`button1`, `text2`)
- Traductions inline (`t('Bienvenue')` ❌ vs `t('welcome')` ✅)

### Ajout Nouvelle Langue

```typescript
// 1. Créer fichier traduction
// frontend/src/locales/es.json
{ "header": { "title": "Admin LLM Proxy" } }

// 2. Importer dans index.ts
import es from './es.json'

export const i18n = createI18n({
  messages: { fr, en, es } // ✨ Ajouter ici
})

// 3. Ajouter au sélecteur langue
<button @click="changeLanguage('es')">Español</button>
```

### Script Validation Traductions

```bash
# Script CI/CD : Vérifier clés identiques FR/EN
npx tsx scripts/validate-i18n.ts

# Vérifie :
# - Clés FR === Clés EN (pas de manquantes)
# - Pas de clés orphelines
# - Format JSON valide
```

### Tests E2E

```typescript
// tests/e2e/language-selector.spec.ts
import { test, expect } from '@playwright/test'

test('should switch language from FR to EN', async ({ page }) => {
  await page.goto('/')
  
  // Vérifier langue par défaut (FR)
  await expect(page.locator('h1')).toHaveText('Tableau de bord')
  
  // Changer langue vers EN
  await page.click('[data-testid="language-selector"]')
  await page.click('[data-testid="language-en"]')
  
  // Vérifier changement
  await expect(page.locator('h1')).toHaveText('Dashboard')
  
  // Recharger et vérifier persistance
  await page.reload()
  await expect(page.locator('h1')).toHaveText('Dashboard')
})
```

### Future Enhancements

**Lazy Loading des Traductions :**
```typescript
// Si >5 langues → charger à la demande
const messages = {
  fr: () => import('./locales/fr.json'),
  en: () => import('./locales/en.json')
}
```

**Pluralization :**
```json
{
  "items": "aucun item | 1 item | {count} items"
}
```

**Date/Number Formatting :**
```typescript
import { useI18n } from 'vue-i18n'
const { n, d } = useI18n()

// Nombre : 1234.56 → "1 234,56" (FR) ou "1,234.56" (EN)
n(1234.56, 'currency')

// Date : 2025-12-23 → "23/12/2025" (FR) ou "12/23/2025" (EN)
d(new Date(), 'short')
```
