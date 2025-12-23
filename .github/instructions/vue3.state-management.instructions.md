---
description: Vue 3 State Management - Pinia stores, actions, getters, composition, persistence
name: Vue3_State_Management
applyTo: "**/frontend/stores/use*Store.ts,**/frontend/stores/*.ts"
---

# Vue 3 - State Management avec Pinia

Guide complet pour la gestion d'état avec Pinia dans Vue 3.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** Vuex (Pinia est le standard Vue 3)
- **Ne mute jamais** le state directement hors des actions
- **Ne crée jamais** de store monolithique (un store par domaine)
- **Ne stocke jamais** de données dérivées dans le state (utilise getters)
- **N'appelle jamais** d'actions depuis les getters (effets de bord)
- **N'oublie jamais** de typer le state et les actions
- **Ne persiste jamais** de données sensibles sans chiffrement

## ✅ À FAIRE

- **Utilise toujours** Pinia avec la syntaxe `defineStore()`
- **Sépare toujours** les stores par domaine métier
- **Type toujours** le state avec une interface explicite
- **Utilise toujours** les getters pour les valeurs dérivées
- **Utilise toujours** les actions pour les mutations et appels API
- **Compose toujours** les stores avec `useOtherStore()` si besoin
- **Utilise toujours** `pinia-plugin-persistedstate` pour la persistance

## 🎯 Actions Obligatoires (Mandatory)

### ⚠️ LECTURE ADR OBLIGATOIRE

**AVANT de créer des stores, TOUJOURS consulter :**
- [010-separation-of-concerns.adr.md](../../docs/adr/010-separation-of-concerns.adr.md) - Separation of Concerns
- [013-cqrs.adr.md](../../docs/adr/013-cqrs.adr.md) - CQRS si applicable
- [015-immutability.adr.md](../../docs/adr/015-immutability.adr.md) - Immutability

## 📁 Structure des Stores

```
src/stores/
├── index.ts              # Export centralisé
├── auth.ts               # Store authentification
├── settings.ts           # Store paramètres utilisateur
├── notifications.ts      # Store notifications
└── [domain].ts           # Store par domaine métier
```

### Export Centralisé

```typescript
// stores/index.ts
export { useAuthStore } from './auth'
export { useSettingsStore } from './settings'
export { useNotificationsStore } from './notifications'
```

## 📦 Structure d'un Store Pinia

### Setup Store (Recommandé)

```typescript
// stores/auth.ts
import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { User, LoginCredentials } from '@/types'
import { authApi } from '@/api'

/**
 * Store pour la gestion de l'authentification
 */
export const useAuthStore = defineStore('auth', () => {
  // ============================================================
  // STATE
  // ============================================================
  
  const user = ref<User | null>(null)
  const token = ref<string | null>(localStorage.getItem('token'))
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // ============================================================
  // GETTERS (Computed)
  // ============================================================
  
  /** Vérifie si l'utilisateur est authentifié */
  const isAuthenticated = computed(() => Boolean(token.value && user.value))
  
  /** Vérifie si l'utilisateur est admin */
  const isAdmin = computed(() => user.value?.role === 'admin')
  
  /** Nom complet de l'utilisateur */
  const fullName = computed(() => 
    user.value ? `${user.value.firstName} ${user.value.lastName}` : ''
  )

  // ============================================================
  // ACTIONS
  // ============================================================
  
  /**
   * Connexion utilisateur
   */
  async function login(credentials: LoginCredentials): Promise<void> {
    isLoading.value = true
    error.value = null
    
    try {
      const response = await authApi.login(credentials)
      token.value = response.token
      user.value = response.user
      localStorage.setItem('token', response.token)
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Erreur de connexion'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Déconnexion utilisateur
   */
  async function logout(): Promise<void> {
    try {
      await authApi.logout()
    } finally {
      // Toujours nettoyer même en cas d'erreur
      user.value = null
      token.value = null
      localStorage.removeItem('token')
    }
  }

  /**
   * Vérifier et restaurer la session
   */
  async function checkAuth(): Promise<boolean> {
    if (!token.value) return false
    
    isLoading.value = true
    
    try {
      user.value = await authApi.getCurrentUser()
      return true
    } catch {
      // Token invalide, nettoyer
      await logout()
      return false
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Reset le store (utile pour les tests)
   */
  function $reset() {
    user.value = null
    token.value = null
    isLoading.value = false
    error.value = null
    localStorage.removeItem('token')
  }

  // ============================================================
  // RETURN
  // ============================================================
  
  return {
    // State
    user,
    token,
    isLoading,
    error,
    
    // Getters
    isAuthenticated,
    isAdmin,
    fullName,
    
    // Actions
    login,
    logout,
    checkAuth,
    $reset,
  }
})
```

### Options Store (Alternative)

```typescript
// stores/settings.ts
import { defineStore } from 'pinia'

interface SettingsState {
  theme: 'light' | 'dark' | 'system'
  locale: string
  sidebarCollapsed: boolean
  itemsPerPage: number
}

export const useSettingsStore = defineStore('settings', {
  state: (): SettingsState => ({
    theme: 'system',
    locale: 'fr',
    sidebarCollapsed: false,
    itemsPerPage: 10,
  }),

  getters: {
    isDarkMode(): boolean {
      if (this.theme === 'system') {
        return window.matchMedia('(prefers-color-scheme: dark)').matches
      }
      return this.theme === 'dark'
    },
  },

  actions: {
    setTheme(theme: SettingsState['theme']) {
      this.theme = theme
      this.applyTheme()
    },

    toggleSidebar() {
      this.sidebarCollapsed = !this.sidebarCollapsed
    },

    setLocale(locale: string) {
      this.locale = locale
    },

    applyTheme() {
      document.documentElement.classList.toggle('dark', this.isDarkMode)
    },
  },

  // Persistence avec plugin
  persist: {
    key: 'app-settings',
    storage: localStorage,
    paths: ['theme', 'locale', 'sidebarCollapsed', 'itemsPerPage'],
  },
})
```

## 🔗 Utilisation dans les Composants

### Avec `storeToRefs` (Recommandé)

```vue
<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { useAuthStore } from '@/stores'

const authStore = useAuthStore()

// ✅ BON : storeToRefs pour garder la réactivité des states/getters
const { user, isAuthenticated, isLoading } = storeToRefs(authStore)

// ✅ BON : Actions directement depuis le store
const { login, logout } = authStore

async function handleLogin(credentials: LoginCredentials) {
  await login(credentials)
}
</script>

<template>
  <div v-if="isLoading">Chargement...</div>
  <div v-else-if="isAuthenticated">
    Bienvenue {{ user?.firstName }}
    <button @click="logout">Déconnexion</button>
  </div>
</template>
```

### ❌ Anti-Pattern : Destructuration Sans storeToRefs

```typescript
// ❌ MAUVAIS : Perd la réactivité !
const { user, isAuthenticated } = useAuthStore()

// ✅ BON : Utiliser storeToRefs
const { user, isAuthenticated } = storeToRefs(useAuthStore())

// ✅ BON : Ou accéder directement
const store = useAuthStore()
// Dans le template : {{ store.user }}
```

## 🧩 Composition de Stores

### Store qui Dépend d'un Autre

```typescript
// stores/notifications.ts
import { defineStore, storeToRefs } from 'pinia'
import { ref, watch } from 'vue'
import { useAuthStore } from './auth'

export const useNotificationsStore = defineStore('notifications', () => {
  const notifications = ref<Notification[]>([])
  const unreadCount = ref(0)

  // Utiliser un autre store
  const authStore = useAuthStore()
  const { isAuthenticated, user } = storeToRefs(authStore)

  // Réagir aux changements d'auth
  watch(isAuthenticated, async (authenticated) => {
    if (authenticated) {
      await fetchNotifications()
    } else {
      notifications.value = []
      unreadCount.value = 0
    }
  })

  async function fetchNotifications() {
    if (!user.value) return
    
    // Charger les notifications...
  }

  return {
    notifications,
    unreadCount,
    fetchNotifications,
  }
})
```

### Composition Pattern

```typescript
// stores/composables/useLoadable.ts
import { ref, type Ref } from 'vue'

export interface Loadable<T> {
  data: Ref<T>
  isLoading: Ref<boolean>
  error: Ref<Error | null>
  execute: () => Promise<void>
}

export function useLoadable<T>(
  fetcher: () => Promise<T>,
  initialValue: T
): Loadable<T> {
  const data = ref(initialValue) as Ref<T>
  const isLoading = ref(false)
  const error = ref<Error | null>(null)

  async function execute() {
    isLoading.value = true
    error.value = null
    
    try {
      data.value = await fetcher()
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Unknown error')
    } finally {
      isLoading.value = false
    }
  }

  return { data, isLoading, error, execute }
}
```

```typescript
// stores/users.ts - Utilisation du composable
import { defineStore } from 'pinia'
import { useLoadable } from './composables/useLoadable'
import { usersApi } from '@/api'
import type { User } from '@/types'

export const useUsersStore = defineStore('users', () => {
  const {
    data: users,
    isLoading,
    error,
    execute: fetchUsers,
  } = useLoadable<User[]>(() => usersApi.getAll(), [])

  return {
    users,
    isLoading,
    error,
    fetchUsers,
  }
})
```

## 💾 Persistence

### Avec pinia-plugin-persistedstate

```typescript
// main.ts
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'

const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)
```

```typescript
// stores/cart.ts
import { defineStore } from 'pinia'

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: [] as CartItem[],
  }),
  
  // Configuration de persistence
  persist: {
    key: 'shopping-cart',
    storage: localStorage,
    paths: ['items'], // Uniquement persister 'items'
    beforeRestore: (ctx) => {
      console.log('Restoring cart...')
    },
    afterRestore: (ctx) => {
      console.log('Cart restored')
    },
  },
})
```

### Persistence Manuelle

```typescript
// stores/auth.ts
import { defineStore } from 'pinia'
import { ref, watch } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  // Charger depuis localStorage au démarrage
  const token = ref<string | null>(localStorage.getItem('auth_token'))
  
  // Synchroniser avec localStorage
  watch(token, (newToken) => {
    if (newToken) {
      localStorage.setItem('auth_token', newToken)
    } else {
      localStorage.removeItem('auth_token')
    }
  })

  return { token }
})
```

## 🧪 Tests des Stores

```typescript
// stores/__tests__/auth.spec.ts
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../auth'

describe('useAuthStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
  })

  it('should start with no user', () => {
    const store = useAuthStore()
    
    expect(store.user).toBeNull()
    expect(store.isAuthenticated).toBe(false)
  })

  it('should login successfully', async () => {
    const store = useAuthStore()
    
    // Mock API si nécessaire
    vi.mock('@/api', () => ({
      authApi: {
        login: vi.fn().mockResolvedValue({
          token: 'test-token',
          user: { id: '1', firstName: 'John' },
        }),
      },
    }))

    await store.login({ email: 'test@example.com', password: 'password' })

    expect(store.isAuthenticated).toBe(true)
    expect(store.user?.firstName).toBe('John')
    expect(localStorage.getItem('token')).toBe('test-token')
  })

  it('should logout and clear state', async () => {
    const store = useAuthStore()
    store.user = { id: '1', firstName: 'John' } as User
    store.token = 'test-token'

    await store.logout()

    expect(store.user).toBeNull()
    expect(store.token).toBeNull()
    expect(store.isAuthenticated).toBe(false)
  })
})
```

## ✅ Checklist Store Pinia

- [ ] Nom en `use[Domain]Store`
- [ ] Sections STATE / GETTERS / ACTIONS clairement séparées
- [ ] TypeScript strict pour le state
- [ ] Actions avec gestion d'erreurs
- [ ] `$reset()` pour réinitialisation
- [ ] Persistence si données doivent survivre au refresh
- [ ] Tests unitaires
- [ ] Utiliser `storeToRefs()` dans les composants
