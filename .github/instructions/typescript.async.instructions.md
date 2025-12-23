---
description: TypeScript Async - Promises, async/await, error handling, concurrency patterns
name: TypeScript_Async_Patterns
applyTo: "**/*.ts"
---

# TypeScript - Programmation Asynchrone

Guide complet pour la programmation asynchrone en TypeScript.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** de callbacks imbriqués (callback hell)
- **N'oublie jamais** de gérer les rejections de Promise
- **N'utilise jamais** `.then()` dans du code async/await (choisir un style)
- **Ne bloque jamais** avec des boucles synchrones sur des opérations async
- **N'ignore jamais** les erreurs dans les catch blocks
- **N'utilise jamais** `Promise.all()` sans gestion d'erreur appropriée
- **Ne crée jamais** de Promise sans timeout pour les opérations réseau

## ✅ À FAIRE

- **Utilise toujours** async/await pour la lisibilité
- **Type toujours** explicitement les retours de fonctions async
- **Gère toujours** les erreurs avec try/catch ou .catch()
- **Utilise toujours** `Promise.allSettled()` quand toutes les promesses doivent s'exécuter
- **Annule toujours** les requêtes avec AbortController quand possible
- **Utilise toujours** des timeouts pour les opérations externes
- **Parallélise toujours** les opérations indépendantes avec `Promise.all()`

## 📦 Fondamentaux Async/Await

### Syntaxe de Base

```typescript
/ ✅ BON : Fonction async typée
async function fetchUser(id: string): Promise<User> {
  const response = await fetch(`/api/users/${id}`)
  
  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`)
  }
  
  return response.json() as Promise<User>
}

/ ✅ BON : Arrow function async
const fetchUsers = async (): Promise<User[]> => {
  const response = await fetch('/api/users')
  return response.json()
}

/ ✅ BON : Méthode async dans une classe
class UserService {
  async getById(id: string): Promise<User | null> {
    try {
      return await fetchUser(id)
    } catch {
      return null
    }
  }
}
```

### Gestion d'Erreurs

```typescript
/ ✅ BON : Try/catch avec type d'erreur
async function safelyFetchUser(id: string): Promise<Result<User, Error>> {
  try {
    const user = await fetchUser(id)
    return { success: true, data: user }
  } catch (error) {
    const message = error instanceof Error ? error.message : 'Unknown error'
    return { success: false, error: new Error(message) }
  }
}

/ ✅ BON : Fonction utilitaire pour wrapper les erreurs
async function tryCatch<T>(
  promise: Promise<T>
): Promise<[T, null] | [null, Error]> {
  try {
    const data = await promise
    return [data, null]
  } catch (error) {
    const err = error instanceof Error ? error : new Error(String(error))
    return [null, err]
  }
}

/ Utilisation
const [user, error] = await tryCatch(fetchUser('123'))
if (error) {
  console.error('Failed to fetch user:', error.message)
  return
}
console.log('User:', user.name)
```

## 🔄 Patterns de Concurrence

### Promise.all - Exécution Parallèle

```typescript
/ ✅ BON : Opérations indépendantes en parallèle
async function fetchUserWithDetails(userId: string): Promise<UserWithDetails> {
  const [user, orders, preferences] = await Promise.all([
    fetchUser(userId),
    fetchUserOrders(userId),
    fetchUserPreferences(userId)
  ])
  
  return { ...user, orders, preferences }
}

/ ✅ BON : Avec typage explicite
interface FetchResult {
  users: User[]
  products: Product[]
  orders: Order[]
}

async function fetchDashboardData(): Promise<FetchResult> {
  const [users, products, orders] = await Promise.all([
    fetchUsers(),
    fetchProducts(),
    fetchOrders()
  ] as const)
  
  return { users, products, orders }
}
```

### Promise.allSettled - Tolérance aux Erreurs

```typescript
/ ✅ BON : Toutes les promesses s'exécutent même si certaines échouent
async function fetchAllUsers(ids: string[]): Promise<(User | null)[]> {
  const results = await Promise.allSettled(
    ids.map(id => fetchUser(id))
  )
  
  return results.map(result => 
    result.status === 'fulfilled' ? result.value : null
  )
}

/ ✅ BON : Avec rapport d'erreurs
interface BatchResult<T> {
  successful: T[]
  failed: Array<{ index: number; error: Error }>
}

async function batchProcess<T, R>(
  items: T[],
  processor: (item: T) => Promise<R>
): Promise<BatchResult<R>> {
  const results = await Promise.allSettled(items.map(processor))
  
  const successful: R[] = []
  const failed: Array<{ index: number; error: Error }> = []
  
  results.forEach((result, index) => {
    if (result.status === 'fulfilled') {
      successful.push(result.value)
    } else {
      failed.push({ 
        index, 
        error: result.reason instanceof Error 
          ? result.reason 
          : new Error(String(result.reason))
      })
    }
  })
  
  return { successful, failed }
}
```

### Promise.race - Premier Arrivé

```typescript
/ ✅ BON : Timeout avec Promise.race
function withTimeout<T>(
  promise: Promise<T>,
  timeoutMs: number,
  errorMessage = 'Operation timed out'
): Promise<T> {
  const timeout = new Promise<never>((_, reject) => {
    setTimeout(() => reject(new Error(errorMessage)), timeoutMs)
  })
  
  return Promise.race([promise, timeout])
}

/ Utilisation
const user = await withTimeout(
  fetchUser('123'),
  5000,
  'Failed to fetch user within 5 seconds'
)

/ ✅ BON : Premier résultat réussi
async function fetchFromFastestMirror<T>(urls: string[]): Promise<T> {
  return Promise.race(
    urls.map(url => fetch(url).then(r => r.json()))
  )
}
```

## 🛑 Annulation avec AbortController

### Pattern de Base

```typescript
/ ✅ BON : Requête annulable
async function fetchWithAbort(
  url: string,
  signal?: AbortSignal
): Promise<Response> {
  const response = await fetch(url, { signal })
  
  if (!response.ok) {
    throw new Error(`HTTP ${response.status}`)
  }
  
  return response
}

/ Utilisation dans un composant Vue
import { ref, onUnmounted } from 'vue'

export function useFetch<T>(url: string) {
  const data = ref<T | null>(null)
  const error = ref<Error | null>(null)
  const loading = ref(false)
  const controller = ref<AbortController | null>(null)
  
  async function execute(): Promise<void> {
    / Annuler la requête précédente
    controller.value?.abort()
    controller.value = new AbortController()
    
    loading.value = true
    error.value = null
    
    try {
      const response = await fetch(url, { 
        signal: controller.value.signal 
      })
      data.value = await response.json()
    } catch (err) {
      if (err instanceof Error && err.name !== 'AbortError') {
        error.value = err
      }
    } finally {
      loading.value = false
    }
  }
  
  function abort(): void {
    controller.value?.abort()
  }
  
  onUnmounted(() => {
    abort()
  })
  
  return { data, error, loading, execute, abort }
}
```

### Annulation de Plusieurs Opérations

```typescript
/ ✅ BON : Groupe d'opérations annulables
class CancellableOperations {
  private controller: AbortController
  
  constructor() {
    this.controller = new AbortController()
  }
  
  get signal(): AbortSignal {
    return this.controller.signal
  }
  
  async fetch<T>(url: string): Promise<T> {
    const response = await fetch(url, { signal: this.signal })
    return response.json()
  }
  
  cancelAll(): void {
    this.controller.abort()
    this.controller = new AbortController()
  }
}

/ Utilisation
const ops = new CancellableOperations()

/ Ces requêtes peuvent être annulées ensemble
const userPromise = ops.fetch<User>('/api/user')
const ordersPromise = ops.fetch<Order[]>('/api/orders')

/ Annuler toutes les requêtes en cours
ops.cancelAll()
```

## 🔁 Itération Asynchrone

### for await...of

```typescript
/ ✅ BON : Itérer sur un async iterable
async function* fetchUsersInBatches(
  ids: string[],
  batchSize: number
): AsyncGenerator<User[], void, unknown> {
  for (let i = 0; i < ids.length; i += batchSize) {
    const batch = ids.slice(i, i + batchSize)
    const users = await Promise.all(batch.map(fetchUser))
    yield users
  }
}

/ Utilisation
async function processAllUsers(ids: string[]): Promise<void> {
  for await (const batch of fetchUsersInBatches(ids, 10)) {
    console.log(`Processing batch of ${batch.length} users`)
    await processBatch(batch)
  }
}
```

### Séquentiel vs Parallèle

```typescript
/ ❌ MAUVAIS : Exécution séquentielle involontaire
async function fetchUsersSequential(ids: string[]): Promise<User[]> {
  const users: User[] = []
  for (const id of ids) {
    const user = await fetchUser(id) / Attend chaque requête
    users.push(user)
  }
  return users / Très lent!
}

/ ✅ BON : Exécution parallèle
async function fetchUsersParallel(ids: string[]): Promise<User[]> {
  return Promise.all(ids.map(id => fetchUser(id)))
}

/ ✅ BON : Parallèle avec limite de concurrence
async function fetchUsersWithLimit(
  ids: string[],
  concurrencyLimit: number
): Promise<User[]> {
  const results: User[] = []
  
  for (let i = 0; i < ids.length; i += concurrencyLimit) {
    const batch = ids.slice(i, i + concurrencyLimit)
    const batchResults = await Promise.all(batch.map(fetchUser))
    results.push(...batchResults)
  }
  
  return results
}
```

## 🔄 Retry Pattern

```typescript
/ ✅ BON : Retry avec backoff exponentiel
interface RetryOptions {
  maxRetries: number
  initialDelayMs: number
  maxDelayMs: number
  backoffFactor: number
  shouldRetry?: (error: unknown) => boolean
}

async function withRetry<T>(
  operation: () => Promise<T>,
  options: RetryOptions
): Promise<T> {
  const {
    maxRetries,
    initialDelayMs,
    maxDelayMs,
    backoffFactor,
    shouldRetry = () => true
  } = options
  
  let lastError: unknown
  let delay = initialDelayMs
  
  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      return await operation()
    } catch (error) {
      lastError = error
      
      if (attempt === maxRetries || !shouldRetry(error)) {
        break
      }
      
      console.log(`Attempt ${attempt + 1} failed, retrying in ${delay}ms...`)
      await sleep(delay)
      delay = Math.min(delay * backoffFactor, maxDelayMs)
    }
  }
  
  throw lastError
}

/ Fonction utilitaire sleep
function sleep(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms))
}

/ Utilisation
const user = await withRetry(
  () => fetchUser('123'),
  {
    maxRetries: 3,
    initialDelayMs: 1000,
    maxDelayMs: 10000,
    backoffFactor: 2,
    shouldRetry: (error) => {
      / Retry uniquement pour les erreurs réseau
      return error instanceof Error && 
             error.message.includes('network')
    }
  }
)
```

## 📊 Queue de Tâches

```typescript
/ ✅ BON : Queue avec limite de concurrence
class AsyncQueue<T> {
  private queue: Array<() => Promise<T>> = []
  private running = 0
  private readonly concurrency: number
  
  constructor(concurrency: number) {
    this.concurrency = concurrency
  }
  
  async add(task: () => Promise<T>): Promise<T> {
    return new Promise((resolve, reject) => {
      const wrappedTask = async (): Promise<T> => {
        try {
          const result = await task()
          resolve(result)
          return result
        } catch (error) {
          reject(error)
          throw error
        } finally {
          this.running--
          this.processNext()
        }
      }
      
      this.queue.push(wrappedTask)
      this.processNext()
    })
  }
  
  private processNext(): void {
    if (this.running >= this.concurrency || this.queue.length === 0) {
      return
    }
    
    const task = this.queue.shift()
    if (task) {
      this.running++
      task()
    }
  }
  
  get pendingCount(): number {
    return this.queue.length
  }
  
  get runningCount(): number {
    return this.running
  }
}

/ Utilisation
const queue = new AsyncQueue<User>(3) / Max 3 requêtes simultanées

const userPromises = userIds.map(id =>
  queue.add(() => fetchUser(id))
)

const users = await Promise.all(userPromises)
```

## ⚠️ Anti-Patterns à Éviter

```typescript
/ ❌ MAUVAIS : Promise non retournée
async function badExample(): Promise<void> {
  fetchUser('123') / Promise ignorée!
}

/ ✅ BON : Toujours retourner ou await
async function goodExample(): Promise<void> {
  await fetchUser('123')
}

/ ❌ MAUVAIS : Catch vide
try {
  await fetchUser('123')
} catch {
  / Erreur silencieuse!
}

/ ✅ BON : Logger ou propager l'erreur
try {
  await fetchUser('123')
} catch (error) {
  console.error('Failed to fetch user:', error)
  throw error / ou gérer explicitement
}

/ ❌ MAUVAIS : await dans une boucle forEach
userIds.forEach(async (id) => {
  await fetchUser(id) / Ne marche pas comme attendu!
})

/ ✅ BON : for...of ou Promise.all
for (const id of userIds) {
  await fetchUser(id)
}
/ ou
await Promise.all(userIds.map(id => fetchUser(id)))
```
