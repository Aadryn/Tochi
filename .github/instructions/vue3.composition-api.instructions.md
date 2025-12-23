---
description: Vue 3 Composition API - Composables, Reactivity, ref, reactive, computed, watch, provide/inject
name: Vue3_Composition_API
applyTo: "**/composables/use*.ts,**/*.vue"
---

# Vue 3 - Composition API

Guide complet de la Composition API Vue 3 avec TypeScript.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** `reactive()` pour des valeurs primitives (utilise `ref()`)
- **Ne mute jamais** directement un objet `reactive()` entièrement (perte de réactivité)
- **N'oublie jamais** `.value` pour accéder aux `ref()` dans le script
- **Ne crée jamais** de computed avec effets de bord
- **N'utilise jamais** `watch` sans option `{ immediate: true }` si nécessaire au mount
- **Ne fais jamais** de destructuring direct sur `reactive()` (utilise `toRefs()`)
- **N'oublie jamais** le cleanup dans les watchers (`onCleanup`)

## ✅ À FAIRE

- **Utilise toujours** `ref()` pour les primitives et `reactive()` pour les objets complexes
- **Utilise toujours** `computed()` pour les valeurs dérivées
- **Utilise toujours** `toRefs()` pour déstructurer un `reactive()`
- **Crée toujours** des composables pour la logique réutilisable (`useXxx()`)
- **Retourne toujours** des `ref()` depuis les composables (cohérence)
- **Utilise toujours** `watchEffect()` pour les effets de bord simples
- **Type toujours** les composables avec leurs paramètres et retours

## 🎯 Actions Obligatoires (Mandatory)

### ⚠️ LECTURE ADR OBLIGATOIRE

**AVANT de créer des composables, TOUJOURS consulter :**
- [003-principe-dry.adr.md](../../docs/adr/003-principe-dry.adr.md) - Don't Repeat Yourself
- [010-separation-of-concerns.adr.md](../../docs/adr/010-separation-of-concerns.adr.md) - Separation of Concerns
- [011-composition-over-inheritance.adr.md](../../docs/adr/011-composition-over-inheritance.adr.md) - Composition over Inheritance

## 📦 Réactivité Vue 3

### ref vs reactive

```typescript
import { ref, reactive, toRefs } from 'vue'

/ ✅ ref : Pour valeurs primitives et objets simples
const count = ref(0)
const user = ref<User | null>(null)
const isLoading = ref(false)

/ Accès avec .value en TypeScript
count.value++
user.value = { id: '1', name: 'John' }

/ ✅ reactive : Pour objets complexes avec structure stable
const state = reactive({
  users: [] as User[],
  filters: {
    search: '',
    status: 'all',
  },
  pagination: {
    page: 1,
    pageSize: 10,
    total: 0,
  },
})

/ Accès direct (pas de .value)
state.users.push(newUser)
state.filters.search = 'query'

/ ❌ MAUVAIS : reactive avec valeurs primitives
/ const count = reactive(0) / Non réactif !

/ ✅ BON : Extraire refs depuis reactive
const { users, filters, pagination } = toRefs(state)
```

### Computed

```typescript
import { computed, ref } from 'vue'

const items = ref<Item[]>([])
const searchQuery = ref('')
const selectedStatus = ref<string>('all')

/ ✅ Computed simple (getter uniquement)
const activeItems = computed(() =>
  items.value.filter(item => item.status === 'active')
)

/ ✅ Computed avec filtres multiples
const filteredItems = computed(() => {
  let result = items.value

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(item =>
      item.name.toLowerCase().includes(query)
    )
  }

  if (selectedStatus.value !== 'all') {
    result = result.filter(item => item.status === selectedStatus.value)
  }

  return result
})

/ ✅ Computed avec getter/setter
const fullName = computed({
  get: () => `${firstName.value} ${lastName.value}`,
  set: (value: string) => {
    const [first, ...rest] = value.split(' ')
    firstName.value = first
    lastName.value = rest.join(' ')
  },
})
```

### Watch et WatchEffect

```typescript
import { watch, watchEffect, ref } from 'vue'

const userId = ref<string>('')
const user = ref<User | null>(null)

/ ✅ watch : Réaction à des sources spécifiques
watch(userId, async (newId, oldId) => {
  if (newId && newId !== oldId) {
    user.value = await fetchUser(newId)
  }
})

/ ✅ watch avec options
watch(
  userId,
  async (newId) => {
    if (newId) {
      user.value = await fetchUser(newId)
    }
  },
  {
    immediate: true,  / Exécuter immédiatement
    deep: false,      / Pas de surveillance profonde (performance)
  }
)

/ ✅ watch multiple sources
watch(
  [searchQuery, selectedStatus],
  ([newSearch, newStatus]) => {
    fetchItems(newSearch, newStatus)
  },
  { debounce: 300 } / Avec debounce si disponible
)

/ ✅ watchEffect : Réaction automatique aux dépendances
watchEffect(async () => {
  / Toute ref/reactive utilisée est automatiquement trackée
  if (userId.value) {
    user.value = await fetchUser(userId.value)
  }
})

/ ✅ watchEffect avec cleanup
watchEffect((onCleanup) => {
  const controller = new AbortController()
  
  fetchData(controller.signal)
  
  onCleanup(() => {
    controller.abort()
  })
})
```

## 🧩 Composables (Custom Hooks)

### Structure Standard d'un Composable

```typescript
/ composables/useUsers.ts
import { ref, computed, readonly } from 'vue'
import type { User } from '@/types'
import { usersApi } from '@/api'

/**
 * Composable pour la gestion des utilisateurs
 * 
 * @example
 * ```ts
 * const { users, isLoading, fetchUsers, createUser } = useUsers()
 * ```
 */
export function useUsers() {
  / État interne
  const users = ref<User[]>([])
  const isLoading = ref(false)
  const error = ref<Error | null>(null)

  / Computed
  const activeUsers = computed(() =>
    users.value.filter(u => u.status === 'active')
  )

  const userCount = computed(() => users.value.length)

  / Actions
  async function fetchUsers() {
    isLoading.value = true
    error.value = null
    
    try {
      users.value = await usersApi.getAll()
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('Unknown error')
      throw e
    } finally {
      isLoading.value = false
    }
  }

  async function createUser(userData: CreateUserDto) {
    isLoading.value = true
    
    try {
      const newUser = await usersApi.create(userData)
      users.value.push(newUser)
      return newUser
    } finally {
      isLoading.value = false
    }
  }

  async function deleteUser(userId: string) {
    await usersApi.delete(userId)
    users.value = users.value.filter(u => u.id !== userId)
  }

  / Retour avec readonly pour les états non modifiables directement
  return {
    / État (readonly pour prévenir les mutations externes)
    users: readonly(users),
    isLoading: readonly(isLoading),
    error: readonly(error),
    
    / Computed
    activeUsers,
    userCount,
    
    / Actions
    fetchUsers,
    createUser,
    deleteUser,
  }
}
```

### Composable avec Paramètres

```typescript
/ composables/useApi.ts
import { ref, type Ref } from 'vue'
import axios, { type AxiosRequestConfig } from 'axios'

interface UseApiOptions {
  /** URL de base pour les requêtes */
  baseURL?: string
  /** Headers par défaut */
  headers?: Record<string, string>
}

interface UseApiReturn<T> {
  data: Ref<T | null>
  isLoading: Ref<boolean>
  error: Ref<Error | null>
  execute: () => Promise<T>
}

/**
 * Composable générique pour les appels API
 */
export function useApi<T>(
  url: string,
  options: UseApiOptions = {}
): UseApiReturn<T> {
  const data = ref<T | null>(null) as Ref<T | null>
  const isLoading = ref(false)
  const error = ref<Error | null>(null)

  const config: AxiosRequestConfig = {
    baseURL: options.baseURL ?? import.meta.env.VITE_API_URL,
    headers: options.headers,
  }

  async function execute(): Promise<T> {
    isLoading.value = true
    error.value = null

    try {
      const response = await axios.get<T>(url, config)
      data.value = response.data
      return response.data
    } catch (e) {
      error.value = e instanceof Error ? e : new Error('API Error')
      throw error.value
    } finally {
      isLoading.value = false
    }
  }

  return {
    data,
    isLoading,
    error,
    execute,
  }
}
```

### Composable avec État Partagé

```typescript
/ composables/useNotification.ts
import { ref } from 'vue'

export interface Notification {
  id: string
  type: 'success' | 'error' | 'warning' | 'info'
  message: string
  duration?: number
}

/ État global partagé entre toutes les instances
const notifications = ref<Notification[]>([])

/**
 * Composable pour les notifications globales
 */
export function useNotification() {
  function show(notification: Omit<Notification, 'id'>) {
    const id = crypto.randomUUID()
    const newNotification: Notification = { ...notification, id }
    
    notifications.value.push(newNotification)
    
    if (notification.duration !== 0) {
      setTimeout(() => {
        remove(id)
      }, notification.duration ?? 5000)
    }
    
    return id
  }

  function success(message: string, duration?: number) {
    return show({ type: 'success', message, duration })
  }

  function error(message: string, duration?: number) {
    return show({ type: 'error', message, duration })
  }

  function warning(message: string, duration?: number) {
    return show({ type: 'warning', message, duration })
  }

  function info(message: string, duration?: number) {
    return show({ type: 'info', message, duration })
  }

  function remove(id: string) {
    const index = notifications.value.findIndex(n => n.id === id)
    if (index !== -1) {
      notifications.value.splice(index, 1)
    }
  }

  function clear() {
    notifications.value = []
  }

  return {
    notifications,
    show,
    success,
    error,
    warning,
    info,
    remove,
    clear,
  }
}
```

## 🔗 Provide / Inject

### Provider (Composant Parent)

```typescript
/ composables/useTheme.ts
import { ref, provide, inject, type InjectionKey, type Ref } from 'vue'

export interface Theme {
  isDark: Ref<boolean>
  toggle: () => void
  setDark: (value: boolean) => void
}

export const ThemeKey: InjectionKey<Theme> = Symbol('theme')

export function provideTheme() {
  const isDark = ref(false)

  function toggle() {
    isDark.value = !isDark.value
  }

  function setDark(value: boolean) {
    isDark.value = value
  }

  const theme: Theme = {
    isDark,
    toggle,
    setDark,
  }

  provide(ThemeKey, theme)

  return theme
}
```

### Consumer (Composant Enfant)

```typescript
/ composables/useTheme.ts (suite)
export function useTheme(): Theme {
  const theme = inject(ThemeKey)
  
  if (!theme) {
    throw new Error('useTheme must be used within a ThemeProvider')
  }
  
  return theme
}
```

### Utilisation

```vue
<!-- App.vue (Provider) -->
<script setup lang="ts">
import { provideTheme } from '@/composables/useTheme'

provideTheme()
</script>

<!-- ChildComponent.vue (Consumer) -->
<script setup lang="ts">
import { useTheme } from '@/composables/useTheme'

const { isDark, toggle } = useTheme()
</script>
```

## 🚫 Anti-Patterns

### ❌ Réactivité Perdue

```typescript
/ ❌ MAUVAIS : Destructuration qui perd la réactivité
const { users } = useUsers() / Si users est reactive, la réactivité est perdue !

/ ✅ BON : Utiliser toRefs ou retourner des refs
const { users } = useUsers() / Si users est ref, OK
const { users } = storeToRefs(useUsersStore()) / Pour Pinia
```

### ❌ Effets de Bord dans Computed

```typescript
/ ❌ MAUVAIS : Effet de bord dans computed
const total = computed(() => {
  console.log('Computing...') / ❌ Effet de bord
  localStorage.setItem('total', sum.toString()) / ❌ Effet de bord
  return sum
})

/ ✅ BON : Computed pur
const total = computed(() => items.value.reduce((acc, i) => acc + i.price, 0))
```

### ❌ Oublier le Cleanup

```typescript
/ ❌ MAUVAIS : Pas de cleanup
onMounted(() => {
  window.addEventListener('resize', handleResize)
})

/ ✅ BON : Avec cleanup
onMounted(() => {
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})
```

## ✅ Checklist Composable

- [ ] Nom en `use[Feature]`
- [ ] TypeScript strict
- [ ] États exposés en `readonly()` si non modifiables
- [ ] JSDoc avec `@example`
- [ ] Cleanup des effets si nécessaire
- [ ] Gestion des erreurs
- [ ] Tests unitaires
