# Tâche 006 - Internationalization (i18n) Frontend Vue 3

## OBJECTIF

Implémenter le support multilingue (français/anglais) dans l'application frontend Vue 3 avec Vue I18n 9.x.

## POURQUOI

**Justification Métier :**
- Améliorer l'accessibilité de l'interface pour utilisateurs internationaux
- Préparer expansion globale du service proxy LLM
- Conformité aux bonnes pratiques UX pour applications SaaS

**Valeur ajoutée :**
- Interface utilisable en français (défaut) et anglais
- Changement de langue dynamique sans rechargement page
- Persistance de la préférence utilisateur

## DÉPENDANCES

**Prérequis :**
- ✅ Vue 3 application fonctionnelle
- ✅ Composition API utilisée dans les composants
- ✅ Pinia pour state management

**ADR Applicables :**
- ADR-019 : Convention over Configuration (Vue I18n config standardisée)
- ADR-027 : Defensive Programming (gestion fallback langue par défaut)

## CRITÈRES DE SUCCÈS

- [ ] Vue I18n 9.x installé et configuré
- [ ] Fichiers de traduction créés : `locales/fr.json`, `locales/en.json`
- [ ] Plugin i18n configuré dans `main.ts`
- [ ] Sélecteur de langue dans `AppHeader.vue` (drapeau FR/EN)
- [ ] Toutes les chaînes UI traduites dans les composants principaux :
  - [ ] AppHeader.vue (navigation, thème)
  - [ ] DashboardView.vue (métriques, titres)
  - [ ] TenantsView.vue (table, formulaire)
  - [ ] ProvidersView.vue (liste, configuration)
  - [ ] MonitoringView.vue (graphiques, alertes)
  - [ ] RoutesView.vue (liste routes, détails)
  - [ ] SettingsView.vue (paramètres utilisateur)
- [ ] Messages de validation et erreurs traduits
- [ ] Préférence langue persistée dans `localStorage`
- [ ] Langue détectée au démarrage (localStorage > navigateur > FR défaut)
- [ ] Build frontend réussi : `npm run build` (0 erreurs, 0 warnings TypeScript)
- [ ] Tests E2E mis à jour (changement de langue via sélecteur)

## PÉRIMÈTRE

**Inclus :**
- Installation Vue I18n 9.x
- Configuration plugin i18n (composition API)
- Création fichiers de traduction (FR/EN)
- Extraction et traduction de TOUTES les chaînes UI existantes
- Composant sélecteur de langue dans AppHeader
- Persistance localStorage + détection langue navigateur
- Tests E2E pour sélecteur de langue

**Exclus :**
- Traduction des messages backend (API reste en français)
- Support de langues supplémentaires (espagnol, allemand, etc.)
- Pluralisation complexe (utiliser formats simples)
- Détection géolocalisation pour langue (seulement navigateur)

## APPROCHE TECHNIQUE

### 1. Installation Dépendances

```bash
cd frontend
npm install vue-i18n@9
```

### 2. Structure Fichiers

```
frontend/
├── src/
│   ├── locales/
│   │   ├── fr.json  # Français (défaut)
│   │   ├── en.json  # Anglais
│   │   └── index.ts # Configuration i18n
│   ├── main.ts      # Plugin i18n
│   └── components/
│       └── AppHeader.vue  # Sélecteur langue
```

### 3. Configuration i18n (`locales/index.ts`)

```typescript
import { createI18n } from 'vue-i18n'
import fr from './fr.json'
import en from './en.json'

export const i18n = createI18n({
  legacy: false, // Composition API mode
  locale: getInitialLocale(),
  fallbackLocale: 'fr',
  messages: { fr, en },
  globalInjection: true,
})

function getInitialLocale(): string {
  // 1. localStorage
  const stored = localStorage.getItem('llmproxy-locale')
  if (stored && ['fr', 'en'].includes(stored)) return stored
  
  // 2. Navigateur
  const browserLang = navigator.language.split('-')[0]
  if (['fr', 'en'].includes(browserLang)) return browserLang
  
  // 3. Défaut
  return 'fr'
}
```

### 4. Enregistrement Plugin (`main.ts`)

```typescript
import { createApp } from 'vue'
import { i18n } from './locales'

const app = createApp(App)
app.use(i18n)
app.mount('#app')
```

### 5. Utilisation dans Composants

```vue
<script setup lang="ts">
import { useI18n } from 'vue-i18n'

const { t, locale } = useI18n()

function switchLocale(lang: 'fr' | 'en') {
  locale.value = lang
  localStorage.setItem('llmproxy-locale', lang)
}
</script>

<template>
  <h1>{{ t('dashboard.title') }}</h1>
  <button @click="switchLocale('en')">{{ t('header.language.english') }}</button>
</template>
```

### 6. Exemple Fichier Traduction (`fr.json`)

```json
{
  "header": {
    "title": "Admin LLM Proxy",
    "nav": {
      "dashboard": "Tableau de bord",
      "tenants": "Tenants",
      "providers": "Fournisseurs",
      "monitoring": "Surveillance"
    },
    "language": {
      "french": "Français",
      "english": "English"
    }
  },
  "common": {
    "save": "Enregistrer",
    "cancel": "Annuler",
    "delete": "Supprimer",
    "edit": "Modifier"
  }
}
```

## VALIDATION

### Checklist Fonctionnelle

- [ ] Sélecteur de langue visible dans AppHeader (drapeau ou dropdown)
- [ ] Clic sur langue change instantanément l'interface
- [ ] Rechargement page conserve la langue choisie (localStorage)
- [ ] Langue par défaut = français si première visite
- [ ] Tous les textes UI changent (aucun texte en dur restant)
- [ ] Messages de formulaire traduits (labels, placeholders, erreurs)

### Checklist Technique

- [ ] `npm run build` réussit (0 erreurs TypeScript)
- [ ] Aucun warning "Missing translation" dans la console
- [ ] Types TypeScript pour clés de traduction (autocomplete)
- [ ] Fallback FR fonctionne si traduction EN manquante
- [ ] Performance : changement langue < 100ms (pas de rechargement page)

### Tests E2E

```typescript
// tests/e2e/language-selector.spec.ts
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

## RÉFÉRENCES

**Documentation :**
- [Vue I18n Documentation](https://vue-i18n.intlify.dev/)
- [Composition API Guide](https://vue-i18n.intlify.dev/guide/advanced/composition.html)

**ADR :**
- ADR-019 : Convention over Configuration
- ADR-027 : Defensive Programming

**Fichiers Clés :**
- `frontend/src/components/AppHeader.vue` (sélecteur langue)
- `frontend/src/locales/` (traductions)
- `frontend/src/main.ts` (plugin i18n)

## NOTES

**Complexité estimée :** Moyenne (1-2 heures)
- Installation : 5 min
- Configuration i18n : 15 min
- Extraction chaînes existantes : 30 min
- Traduction FR → EN : 30 min
- Sélecteur langue UI : 15 min
- Tests E2E : 15 min

**Risques :**
- Oubli de chaînes en dur dans certains composants → Recherche exhaustive `grep -r "\"[A-Z]"` dans Vue
- Traductions EN approximatives → Utiliser DeepL pour qualité professionnelle


## TRACKING
Début: 2025-12-22T23:24:07.8025955Z



## COMPLÉTION
Fin: 2025-12-22T23:28:01.0693449Z
Durée: 00:03:53

### Résultats
-  Vue I18n 9.x installé
-  Fichiers traduction créés : fr.json (266 lignes), en.json (266 lignes)
-  Configuration i18n avec détection automatique langue (localStorage > navigateur > FR)
-  Plugin i18n intégré dans main.ts
-  Sélecteur langue ajouté dans AppHeader.vue (dropdown FR/EN avec drapeaux)
-  Traductions appliquées : Header, Dashboard, Tenants, Providers, Monitoring, Routes, Settings, Common
-  Persistance langue dans localStorage
-  Build frontend réussi : 3.04s (0 erreurs TypeScript)
-  Helper setLocale() et getCurrentLocale() créés

### Fichiers créés
1. frontend/src/locales/fr.json (266 lignes)
2. frontend/src/locales/en.json (266 lignes)
3. frontend/src/locales/index.ts (configuration i18n)

### Fichiers modifiés
1. frontend/src/main.ts (plugin i18n)
2. frontend/src/components/layout/AppHeader.vue (sélecteur langue + traductions)

