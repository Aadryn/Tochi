---
description: Règles fondamentales Vue 3 - Architecture, ADR, Folder Structure, Composition API, Lifecycle, TypeScript
name: Vue3_Fundamentals
applyTo: "**/frontend/**/*.vue"
---

# Vue 3 - Règles Fondamentales

Guide des principes fondamentaux pour le développement Vue 3 avec TypeScript.

## � Types de Fichiers à Créer

| Type de fichier | Usage | Nomenclature |
|----------------|-------|-------------|
| `components/[feature]/[Component].vue` | Composants Vue réutilisables | PascalCase (ex: `UserCard.vue`, `DataTable.vue`) |
| `views/[Module]/[View].vue` | Vues/Pages de l'application | PascalCase (ex: `DashboardView.vue`, `UsersView.vue`) |
| `composables/use[Feature].ts` | Composition functions réutilisables | camelCase avec préfixe `use` (ex: `useAuth.ts`, `useApi.ts`) |
| `stores/use[Domain]Store.ts` | Stores Pinia | camelCase avec suffixe `Store` (ex: `useUserStore.ts`) |
| `directives/v[Directive].ts` | Directives Vue custom | camelCase avec préfixe `v` (ex: `vTooltip.ts`, `vFocus.ts`) |
| `*Form.vue` | Composants de formulaires | Suffixe `Form` (ex: `UserForm.vue`, `LoginForm.vue`) |
| `*Modal.vue` | Composants modaux/dialogues | Suffixe `Modal` (ex: `ConfirmModal.vue`, `EditModal.vue`) |
| `*.spec.ts` | Tests unitaires Vitest | Même nom que fichier testé (ex: `UserCard.spec.ts`) |
| `router/index.ts` | Configuration du routeur | Point d'entrée unique pour Vue Router |

## ⛔ À NE PAS FAIRE

- **Ne génère jamais** de code Vue 3 sans avoir lu les ADR dans `docs/adr/`
- **N'utilise jamais** Options API (Composition API obligatoire avec `<script setup>`)
- **N'utilise jamais** `any` comme type TypeScript
- **Ne crée jamais** de composant sans typage strict des props/emits
- **Ne place jamais** de logique métier dans les composants (utilise composables)
- **N'ignore jamais** le lifecycle (onMounted, onUnmounted pour le cleanup)
- **Ne mélange jamais** state local et state global sans justification

## ✅ À FAIRE

- **Consulte toujours** les ADR avant de coder (surtout ADR-002 à ADR-011)
- **Utilise toujours** `<script setup lang="ts">` pour les composants
- **Type toujours** explicitement props, emits, et retours de fonctions
- **Respecte toujours** la structure de dossiers standardisée du projet
- **Utilise toujours** Pinia pour le state management global
- **Crée toujours** des composables pour la logique réutilisable
- **Nettoie toujours** les ressources dans `onUnmounted()`

## 🎯 Actions Obligatoires (Mandatory)

### ⚠️ LECTURE ADR OBLIGATOIRE

**AVANT de générer du code Vue 3, TOUJOURS lire les ADR applicables dans `docs/adr/` :**

1. ✅ **Consulter les ADR architecturaux** :
   - [002-principe-kiss.adr.md](../../docs/adr/002-principe-kiss.adr.md) - Keep It Simple, Stupid
   - [003-principe-dry.adr.md](../../docs/adr/003-principe-dry.adr.md) - Don't Repeat Yourself
   - [004-principe-yagni.adr.md](../../docs/adr/004-principe-yagni.adr.md) - You Ain't Gonna Need It
   - [010-separation-of-concerns.adr.md](../../docs/adr/010-separation-of-concerns.adr.md) - Separation of Concerns
   - [011-composition-over-inheritance.adr.md](../../docs/adr/011-composition-over-inheritance.adr.md) - Composition over Inheritance

2. ✅ **Vérifier les ADR spécifiques au projet** avant toute implémentation

3. ✅ **Respecter les décisions documentées** - Ne jamais contourner un ADR sans justification

## 📁 Structure de Dossiers OBLIGATOIRE

```
src/
├── api/                      # Services d'appels API
│   ├── client.ts             # Client HTTP Axios configuré
│   ├── index.ts              # Export centralisé des services
│   └── [domain].ts           # Service par domaine métier (tenants.ts, users.ts)
│
├── assets/                   # Ressources statiques
│   ├── images/               # Images (SVG, PNG, JPG)
│   ├── fonts/                # Polices personnalisées
│   └── styles/               # Styles globaux SCSS/CSS
│       ├── _variables.scss   # Variables CSS/SCSS
│       ├── _mixins.scss      # Mixins SCSS
│       └── main.scss         # Point d'entrée styles
│
├── components/               # Composants Vue réutilisables
│   ├── layout/               # Composants de mise en page
│   │   ├── AppHeader.vue     # En-tête principal
│   │   ├── AppSidebar.vue    # Barre latérale
│   │   └── AppFooter.vue     # Pied de page
│   ├── shared/               # Composants partagés génériques
│   │   ├── BaseButton.vue    # Bouton de base
│   │   ├── BaseInput.vue     # Input de base
│   │   ├── BaseModal.vue     # Modal de base
│   │   └── BaseTable.vue     # Tableau de base
│   └── [feature]/            # Composants par fonctionnalité
│       └── FeatureCard.vue
│
├── composables/              # Composition Functions (hooks Vue 3)
│   ├── useApi.ts             # Hook pour appels API
│   ├── useAuth.ts            # Hook authentification
│   ├── useForm.ts            # Hook gestion formulaires
│   ├── useNotification.ts    # Hook notifications
│   └── use[Feature].ts       # Hook par fonctionnalité
│
├── router/                   # Configuration Vue Router
│   ├── index.ts              # Configuration principale
│   ├── guards.ts             # Navigation guards
│   └── routes/               # Définitions de routes par module
│       ├── index.ts
│       └── [module].routes.ts
│
├── stores/                   # State Management Pinia
│   ├── index.ts              # Export centralisé des stores
│   └── [domain].ts           # Store par domaine (auth.ts, settings.ts)
│
├── types/                    # Types et interfaces TypeScript
│   ├── index.ts              # Export centralisé des types
│   ├── api.types.ts          # Types pour réponses API
│   ├── [domain].types.ts     # Types par domaine métier
│   └── global.d.ts           # Déclarations globales
│
├── utils/                    # Utilitaires et helpers
│   ├── constants.ts          # Constantes de l'application
│   ├── formatters.ts         # Fonctions de formatage
│   ├── validators.ts         # Fonctions de validation
│   └── helpers.ts            # Helpers génériques
│
├── views/                    # Pages/Vues de l'application
│   ├── [Module]/             # Groupement par module métier
│   │   ├── [Module]View.vue  # Vue principale du module
│   │   ├── [Module]List.vue  # Liste des éléments
│   │   └── [Module]Detail.vue# Détail d'un élément
│   └── errors/               # Pages d'erreur
│       ├── NotFound.vue      # Page 404
│       └── ServerError.vue   # Page 500
│
├── App.vue                   # Composant racine
└── main.ts                   # Point d'entrée de l'application
```

### Règles de Nommage des Fichiers

| Type | Convention | Exemple |
|------|------------|---------|
| **Composants** | PascalCase.vue | `UserCard.vue`, `BaseButton.vue` |
| **Composables** | use + camelCase.ts | `useAuth.ts`, `useForm.ts` |
| **Stores** | camelCase.ts | `auth.ts`, `settings.ts` |
| **Types** | camelCase.types.ts | `user.types.ts`, `api.types.ts` |
| **API Services** | camelCase.ts | `users.ts`, `tenants.ts` |
| **Utils** | camelCase.ts | `formatters.ts`, `validators.ts` |
| **Vues** | PascalCase.vue | `DashboardView.vue`, `UserList.vue` |

## 🔄 Component Lifecycle Vue 3

### Ordre d'Exécution (Composition API)

```
1. setup()                    → Configuration du composant
   ↓
2. onBeforeMount()            → Avant insertion dans le DOM
   ↓
3. onMounted()                → Après insertion dans le DOM
   ↓
4. onBeforeUpdate()           → Avant mise à jour réactive
   ↓
5. onUpdated()                → Après mise à jour réactive
   ↓
6. onBeforeUnmount()          → Avant destruction
   ↓
7. onUnmounted()              → Après destruction
```

### Utilisation Correcte du Lifecycle

```vue
<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import type { User } from '@/types'
import { useApi } from '@/composables/useApi'

// Props typées
interface Props {
  userId: string
}
const props = defineProps<Props>()

// État réactif
const user = ref<User | null>(null)
const isLoading = ref(true)

// Composables
const { get } = useApi()

// ✅ BON : Chargement initial dans onMounted
onMounted(async () => {
  await loadUser()
})

// ✅ BON : Watch pour réagir aux changements de props
watch(() => props.userId, async (newId) => {
  if (newId) {
    await loadUser()
  }
})

// ✅ BON : Nettoyage dans onUnmounted
onUnmounted(() => {
  // Annuler abonnements, timers, etc.
})

async function loadUser() {
  isLoading.value = true
  try {
    user.value = await get<User>(`/users/${props.userId}`)
  } finally {
    isLoading.value = false
  }
}
</script>
```

### ❌ Erreurs Courantes

```vue
<script setup lang="ts">
// ❌ MAUVAIS : Appel API directement dans setup (pas async/await géré)
const user = await fetchUser() // Ne pas faire ça !

// ❌ MAUVAIS : Accès au DOM dans setup
const element = document.getElementById('myElement') // null !

// ❌ MAUVAIS : Oublier le nettoyage
const interval = setInterval(() => {}, 1000) // Memory leak !
</script>
```

## 📦 Imports et Exports

### Organisation des Imports

```typescript
// 1. Imports Vue/bibliothèques tierces
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'

// 2. Imports stores
import { useAuthStore } from '@/stores/auth'
import { useSettingsStore } from '@/stores/settings'

// 3. Imports composables
import { useApi } from '@/composables/useApi'
import { useNotification } from '@/composables/useNotification'

// 4. Imports composants
import BaseButton from '@/components/shared/BaseButton.vue'
import UserCard from '@/components/users/UserCard.vue'

// 5. Imports types
import type { User, ApiResponse } from '@/types'

// 6. Imports utilitaires
import { formatDate, formatCurrency } from '@/utils/formatters'
```

### Exports Centralisés (Barrel Exports)

```typescript
// types/index.ts
export * from './user.types'
export * from './api.types'
export * from './tenant.types'

// composables/index.ts
export { useApi } from './useApi'
export { useAuth } from './useAuth'
export { useForm } from './useForm'

// stores/index.ts
export { useAuthStore } from './auth'
export { useSettingsStore } from './settings'
```

## ✅ Checklist de Validation

**Avant de compléter un composant Vue 3, VÉRIFIER :**

- [ ] ADR pertinents consultés et respectés
- [ ] Structure de dossiers conforme
- [ ] Nommage des fichiers correct (PascalCase pour .vue)
- [ ] TypeScript strict (pas de `any`)
- [ ] Props typées avec `defineProps<T>()`
- [ ] Emits typés avec `defineEmits<T>()`
- [ ] Lifecycle hooks utilisés correctement
- [ ] Nettoyage dans `onUnmounted` si nécessaire
- [ ] Imports organisés par catégorie
- [ ] Pas de logique métier dans les templates
- [ ] Composables extraits pour logique réutilisable
