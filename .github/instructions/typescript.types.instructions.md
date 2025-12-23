---
description: TypeScript Types - Interfaces, types, generics, utility types, discriminated unions, type guards
name: TypeScript_Types
applyTo: "**/frontend/types/**/*.ts,**/frontend/**/*.types.ts,**/frontend/**/*.d.ts"
---

# TypeScript - Système de Types

Guide complet pour maîtriser le système de types TypeScript.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** `any` (utilise `unknown` pour les types inconnus)
- **Ne crée jamais** de types avec plus de 3 niveaux d'imbrication
- **N'utilise jamais** `as` sans vérification runtime préalable
- **Ne mélange jamais** `interface` et `type` pour le même concept
- **N'omets jamais** le typage des fonctions publiques
- **Ne duplique jamais** les définitions de types (centraliser dans `/types/`)
- **N'utilise jamais** `Object`, `Function`, `String` (types primitifs uniquement)

## ✅ À FAIRE

- **Préfère toujours** `interface` pour les objets extensibles
- **Utilise toujours** `type` pour les unions, intersections et alias
- **Exporte toujours** les types depuis un fichier index.ts centralisé
- **Utilise toujours** les utility types (`Partial`, `Pick`, `Omit`, `Required`)
- **Crée toujours** des type guards pour les validations runtime
- **Utilise toujours** `readonly` pour les propriétés immutables
- **Documente toujours** les types complexes avec JSDoc

## 📦 Interface vs Type

### Quand utiliser `interface`

```typescript
// ✅ BON : Interface pour les objets avec potentiel d'extension
interface User {
  readonly id: string
  name: string
  email: string
  createdAt: Date
}

// ✅ BON : Extension d'interface
interface AdminUser extends User {
  permissions: string[]
  role: 'admin' | 'superadmin'
}

// ✅ BON : Interface pour les contrats de service
interface UserService {
  getById(id: string): Promise<User | null>
  create(data: CreateUserDto): Promise<User>
  update(id: string, data: UpdateUserDto): Promise<User>
  delete(id: string): Promise<void>
}

// ✅ BON : Declaration merging (augmentation)
interface Window {
  analytics: AnalyticsClient
}
```

### Quand utiliser `type`

```typescript
// ✅ BON : Type pour les unions
type Status = 'pending' | 'active' | 'inactive' | 'deleted'

// ✅ BON : Type pour les intersections
type UserWithMetadata = User & { metadata: Record<string, unknown> }

// ✅ BON : Type pour les alias de primitives
type UserId = string
type Email = string

// ✅ BON : Type pour les tuples
type Coordinates = [latitude: number, longitude: number]

// ✅ BON : Type pour les mapped types
type Readonly<T> = { readonly [K in keyof T]: T[K] }

// ✅ BON : Type pour les types conditionnels
type NonNullable<T> = T extends null | undefined ? never : T
```

## 🔧 Utility Types Essentiels

### Types de Transformation

```typescript
interface User {
  id: string
  name: string
  email: string
  password: string
  createdAt: Date
}

// Partial<T> - Toutes les propriétés optionnelles
type UpdateUserDto = Partial<Omit<User, 'id' | 'createdAt'>>
// { name?: string; email?: string; password?: string; }

// Required<T> - Toutes les propriétés requises
type RequiredUser = Required<Partial<User>>

// Pick<T, K> - Sélectionner des propriétés
type UserCredentials = Pick<User, 'email' | 'password'>
// { email: string; password: string; }

// Omit<T, K> - Exclure des propriétés
type PublicUser = Omit<User, 'password'>
// { id: string; name: string; email: string; createdAt: Date; }

// Readonly<T> - Toutes les propriétés en lecture seule
type ImmutableUser = Readonly<User>

// Record<K, V> - Objet avec clés et valeurs typées
type UserRoles = Record<string, 'admin' | 'user' | 'guest'>

// Extract<T, U> - Extraire les types qui correspondent
type NumericStatus = Extract<Status, 'pending' | 'active'>

// Exclude<T, U> - Exclure les types qui correspondent
type ActiveStatus = Exclude<Status, 'deleted'>
```

### Création de Utility Types Custom

```typescript
// DeepPartial - Partial récursif
type DeepPartial<T> = {
  [P in keyof T]?: T[P] extends object ? DeepPartial<T[P]> : T[P]
}

// DeepReadonly - Readonly récursif
type DeepReadonly<T> = {
  readonly [P in keyof T]: T[P] extends object ? DeepReadonly<T[P]> : T[P]
}

// Nullable - Rendre nullable
type Nullable<T> = T | null

// NonNullableFields - Rendre tous les champs non-nullable
type NonNullableFields<T> = {
  [P in keyof T]: NonNullable<T[P]>
}

// PartialBy - Rendre certains champs optionnels
type PartialBy<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>

// RequiredBy - Rendre certains champs requis
type RequiredBy<T, K extends keyof T> = T & Required<Pick<T, K>>
```

## 🎯 Discriminated Unions

### Pattern Standard

```typescript
// ✅ BON : Union discriminée avec propriété commune
interface LoadingState {
  status: 'loading'
}

interface SuccessState<T> {
  status: 'success'
  data: T
}

interface ErrorState {
  status: 'error'
  error: Error
  retryCount: number
}

type AsyncState<T> = LoadingState | SuccessState<T> | ErrorState

// Utilisation avec narrowing automatique
function handleState<T>(state: AsyncState<T>): void {
  switch (state.status) {
    case 'loading':
      console.log('Chargement...')
      break
    case 'success':
      // TypeScript sait que state.data existe
      console.log('Données:', state.data)
      break
    case 'error':
      // TypeScript sait que state.error existe
      console.error('Erreur:', state.error.message)
      break
  }
}
```

### Result Pattern

```typescript
// ✅ BON : Result type pour la gestion d'erreurs
interface Success<T> {
  success: true
  data: T
}

interface Failure<E = Error> {
  success: false
  error: E
}

type Result<T, E = Error> = Success<T> | Failure<E>

// Fonctions utilitaires
function ok<T>(data: T): Success<T> {
  return { success: true, data }
}

function fail<E>(error: E): Failure<E> {
  return { success: false, error }
}

// Utilisation
async function fetchUser(id: string): Promise<Result<User, ApiError>> {
  try {
    const response = await api.get<User>(`/users/${id}`)
    return ok(response.data)
  } catch (error) {
    return fail(new ApiError('User not found', 404))
  }
}

// Consommation
const result = await fetchUser('123')
if (result.success) {
  console.log(result.data.name) // TypeScript sait que data existe
} else {
  console.error(result.error.message) // TypeScript sait que error existe
}
```

## 🛡️ Type Guards

### Type Guards Built-in

```typescript
// typeof - Pour les primitives
function processValue(value: string | number): void {
  if (typeof value === 'string') {
    console.log(value.toUpperCase()) // value est string
  } else {
    console.log(value.toFixed(2)) // value est number
  }
}

// instanceof - Pour les classes
function handleError(error: Error | string): void {
  if (error instanceof TypeError) {
    console.log('Type error:', error.message)
  } else if (typeof error === 'string') {
    console.log('String error:', error)
  }
}

// in - Pour les propriétés
interface Dog { bark(): void }
interface Cat { meow(): void }

function makeSound(animal: Dog | Cat): void {
  if ('bark' in animal) {
    animal.bark()
  } else {
    animal.meow()
  }
}
```

### Custom Type Guards

```typescript
// ✅ BON : Type guard avec is
function isUser(value: unknown): value is User {
  return (
    typeof value === 'object' &&
    value !== null &&
    'id' in value &&
    'name' in value &&
    'email' in value &&
    typeof (value as User).id === 'string' &&
    typeof (value as User).name === 'string' &&
    typeof (value as User).email === 'string'
  )
}

// ✅ BON : Type guard pour discriminated union
function isSuccessState<T>(state: AsyncState<T>): state is SuccessState<T> {
  return state.status === 'success'
}

// ✅ BON : Type guard pour array
function isNonEmptyArray<T>(arr: T[]): arr is [T, ...T[]] {
  return arr.length > 0
}

// ✅ BON : Assertion function
function assertIsUser(value: unknown): asserts value is User {
  if (!isUser(value)) {
    throw new Error('Value is not a User')
  }
}

// Utilisation
function processUnknownData(data: unknown): void {
  assertIsUser(data)
  // Après l'assertion, data est typé comme User
  console.log(data.name)
}
```

## 📐 Generics Avancés

### Contraintes de Type

```typescript
// ✅ BON : Contrainte avec extends
function getProperty<T, K extends keyof T>(obj: T, key: K): T[K] {
  return obj[key]
}

// ✅ BON : Contrainte multiple
function merge<T extends object, U extends object>(obj1: T, obj2: U): T & U {
  return { ...obj1, ...obj2 }
}

// ✅ BON : Contrainte avec interface
interface HasId {
  id: string
}

function findById<T extends HasId>(items: T[], id: string): T | undefined {
  return items.find(item => item.id === id)
}
```

### Inférence de Types

```typescript
// ✅ BON : Inférence avec infer
type ReturnTypeOf<T> = T extends (...args: any[]) => infer R ? R : never

type ArrayElement<T> = T extends (infer E)[] ? E : never

type PromiseValue<T> = T extends Promise<infer V> ? V : never

// Exemple d'utilisation
type UserArrayElement = ArrayElement<User[]> // User
type FetchReturnType = ReturnTypeOf<typeof fetchUser> // Promise<Result<User, ApiError>>
```

## 📁 Organisation des Types

### Structure Recommandée

```
src/types/
├── index.ts              # Export centralisé
├── api.types.ts          # Types API (requests/responses)
├── domain/
│   ├── user.types.ts     # Types domaine User
│   ├── product.types.ts  # Types domaine Product
│   └── order.types.ts    # Types domaine Order
├── utils/
│   ├── result.types.ts   # Result pattern
│   ├── async.types.ts    # AsyncState pattern
│   └── common.types.ts   # Types utilitaires
└── global.d.ts           # Déclarations globales
```

### Fichier index.ts

```typescript
// src/types/index.ts
// Export centralisé de tous les types

// Domain types
export type { User, CreateUserDto, UpdateUserDto } from './domain/user.types'
export type { Product, CreateProductDto } from './domain/product.types'
export type { Order, OrderItem, OrderStatus } from './domain/order.types'

// API types
export type { ApiResponse, ApiError, PaginatedResponse } from './api.types'

// Utility types
export type { Result, Success, Failure } from './utils/result.types'
export type { AsyncState, LoadingState, SuccessState, ErrorState } from './utils/async.types'
export type { Nullable, DeepPartial, DeepReadonly } from './utils/common.types'
```

## ⚠️ Anti-Patterns à Éviter

```typescript
// ❌ MAUVAIS : any
function processData(data: any): any {
  return data.value
}

// ✅ BON : unknown avec type guard
function processData(data: unknown): string {
  if (isValidData(data)) {
    return data.value
  }
  throw new Error('Invalid data')
}

// ❌ MAUVAIS : Type assertion sans vérification
const user = response.data as User

// ✅ BON : Type guard avant utilisation
if (isUser(response.data)) {
  const user = response.data // Typé automatiquement
}

// ❌ MAUVAIS : Non-null assertion
const name = user!.name

// ✅ BON : Optional chaining avec fallback
const name = user?.name ?? 'Unknown'

// ❌ MAUVAIS : Types imbriqués complexes
type Complex = { a: { b: { c: { d: string }[] }[] }[] }

// ✅ BON : Types décomposés
interface DItem { d: string }
interface CItem { c: DItem[] }
interface BItem { b: CItem[] }
interface Complex { a: BItem[] }
```
