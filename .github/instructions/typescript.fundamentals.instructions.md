---
description: TypeScript Fundamentals - ADR compliance, folder structure, types, interfaces, generics, best practices
name: TypeScript_Fundamentals
applyTo: "**/*.ts"
---

# TypeScript - Règles Fondamentales

Guide des principes fondamentaux pour le développement TypeScript.

## � Types de Fichiers à Créer

| Type de fichier | Usage | Nomenclature |
|----------------|-------|-------------|
| `api/[domain].ts` | Services d'appels API par domaine | `[domain].ts` (ex: `users.ts`, `tenants.ts`, `auth.ts`) |
| `types/[domain].types.ts` | Types et interfaces métier | `[domain].types.ts` (ex: `user.types.ts`, `api.types.ts`) |
| `composables/use[Feature].ts` | Hooks réutilisables Vue 3 | `use[Feature].ts` (ex: `useAuth.ts`, `useApi.ts`) |
| `stores/use[Domain]Store.ts` | Stores Pinia par domaine | `use[Domain]Store.ts` (ex: `useUserStore.ts`) |
| `utils/[category].ts` | Utilitaires génériques | `[category].ts` (ex: `formatters.ts`, `validators.ts`) |
| `*.spec.ts` | Tests unitaires Vitest | `[file].spec.ts` (même nom que le fichier testé) |
| `*.d.ts` | Déclarations de types globaux | `[module].d.ts` (ex: `global.d.ts`, `env.d.ts`) |

## ⛔ À NE PAS FAIRE

- **Ne génère jamais** de code sans avoir lu les ADR dans `docs/adr/`
- **N'utilise jamais** `any` (utilise `unknown` si type inconnu)
- **N'utilise jamais** de type assertion `as` sans justification
- **Ne désactive jamais** les règles TypeScript strictes
- **N'omets jamais** les types de retour explicites sur les fonctions publiques
- **N'utilise jamais** `!` (non-null assertion) sans vérification préalable
- **Ne crée jamais** de types imbriqués complexes (extraire en types nommés)

## ✅ À FAIRE

- **Consulte toujours** les ADR avant de coder (surtout ADR-015, ADR-016, ADR-024)
- **Active toujours** le mode strict dans `tsconfig.json`
- **Préfère toujours** les interfaces pour les objets, types pour les unions
- **Type toujours** explicitement les fonctions publiques (paramètres + retour)
- **Utilise toujours** les génériques pour la réutilisabilité
- **Utilise toujours** `readonly` pour les propriétés immutables
- **Utilise toujours** les discriminated unions pour les types variant

## 🎯 Actions Obligatoires (Mandatory)

### ⚠️ LECTURE ADR OBLIGATOIRE

**AVANT de générer du code TypeScript, TOUJOURS lire les ADR applicables dans `docs/adr/` :**

1. ✅ **Consulter les ADR architecturaux** :
   - [002-principe-kiss.adr.md](../../docs/adr/002-principe-kiss.adr.md) - Keep It Simple, Stupid
   - [003-principe-dry.adr.md](../../docs/adr/003-principe-dry.adr.md) - Don't Repeat Yourself
   - [004-principe-yagni.adr.md](../../docs/adr/004-principe-yagni.adr.md) - You Ain't Gonna Need It
   - [015-immutability.adr.md](../../docs/adr/015-immutability.adr.md) - Immutability
   - [016-explicit-over-implicit.adr.md](../../docs/adr/016-explicit-over-implicit.adr.md) - Explicit over Implicit
   - [024-value-objects.adr.md](../../docs/adr/024-value-objects.adr.md) - Value Objects

2. ✅ **Vérifier les ADR spécifiques au projet** avant toute implémentation

3. ✅ **Respecter les décisions documentées** - Ne jamais contourner un ADR sans justification

## 📁 Structure de Dossiers OBLIGATOIRE

### Application Frontend (Vue/React)

```
src/
├── api/                      # Services d'appels API
│   ├── client.ts             # Client HTTP configuré (Axios/Fetch)
│   ├── index.ts              # Export centralisé
│   └── [domain].ts           # Service par domaine
│
├── components/               # Composants UI
│   ├── layout/               # Layout components
│   ├── shared/               # Composants réutilisables
│   └── [feature]/            # Composants par feature
│
├── composables/              # Hooks/Composables
│   └── use[Feature].ts
│
├── stores/                   # State management
│   └── [domain].ts
│
├── types/                    # Types et interfaces
│   ├── index.ts              # Export centralisé
│   ├── api.types.ts          # Types API (requests/responses)
│   ├── [domain].types.ts     # Types par domaine
│   └── global.d.ts           # Déclarations globales
│
├── utils/                    # Utilitaires
│   ├── constants.ts          # Constantes
│   ├── formatters.ts         # Fonctions de formatage
│   ├── validators.ts         # Validations
│   └── helpers.ts            # Helpers génériques
│
├── views/                    # Pages/Vues
│   └── [Module]/
│       └── [Page].vue
│
└── main.ts                   # Point d'entrée
```

### Application Backend (Node.js/Express)

```
src/
├── config/                   # Configuration
│   ├── index.ts              # Export centralisé
│   ├── database.ts           # Config DB
│   └── env.ts                # Variables d'environnement typées
│
├── controllers/              # Controllers HTTP
│   └── [domain].controller.ts
│
├── middlewares/              # Express middlewares
│   ├── auth.middleware.ts
│   ├── validation.middleware.ts
│   └── error.middleware.ts
│
├── models/                   # Modèles de données
│   └── [entity].model.ts
│
├── repositories/             # Accès données
│   └── [entity].repository.ts
│
├── routes/                   # Définition des routes
│   ├── index.ts
│   └── [domain].routes.ts
│
├── services/                 # Logique métier
│   └── [domain].service.ts
│
├── types/                    # Types TypeScript
│   ├── index.ts
│   ├── express.d.ts          # Extensions Express
│   └── [domain].types.ts
│
├── utils/                    # Utilitaires
│   └── [helper].ts
│
├── validators/               # Schémas de validation
│   └── [domain].validator.ts
│
├── app.ts                    # Configuration Express
└── server.ts                 # Point d'entrée
```

### Bibliothèque/Package

```
src/
├── index.ts                  # Point d'entrée principal
├── types.ts                  # Types publics exportés
│
├── core/                     # Fonctionnalités principales
│   └── [feature].ts
│
├── utils/                    # Utilitaires internes
│   └── [helper].ts
│
└── __tests__/                # Tests
    └── [feature].spec.ts
```

## 📝 Conventions de Typage

### Types vs Interfaces

```typescript
/ ✅ Interface : Pour les objets et contrats
interface User {
  id: string
  email: string
  firstName: string
  lastName: string
}

/ ✅ Interface : Extensible par d'autres modules
interface UserWithRole extends User {
  role: 'admin' | 'user' | 'guest'
}

/ ✅ Type : Pour unions, intersections, types mappés
type UserStatus = 'active' | 'inactive' | 'pending'
type Nullable<T> = T | null
type AsyncFunction<T> = () => Promise<T>

/ ✅ Type : Pour les alias de types complexes
type UserResponse = ApiResponse<User>
type UserCreateDto = Omit<User, 'id'>
type UserUpdateDto = Partial<UserCreateDto>
```

### Nommage des Types

| Type | Convention | Exemple |
|------|------------|---------|
| **Interface** | PascalCase | `User`, `ApiResponse` |
| **Type alias** | PascalCase | `UserStatus`, `Nullable<T>` |
| **Enum** | PascalCase | `OrderStatus` |
| **Generic** | T, U, K, V ou descriptif | `T`, `TEntity`, `TResponse` |
| **DTO** | PascalCase + Dto | `CreateUserDto`, `UpdateOrderDto` |

### Générics

```typescript
/ ✅ BON : Génériques avec contraintes
interface Repository<TEntity extends { id: string }> {
  findById(id: string): Promise<TEntity | null>
  save(entity: TEntity): Promise<TEntity>
  delete(id: string): Promise<void>
}

/ ✅ BON : Génériques multiples avec noms descriptifs
type ApiResponse<TData, TError = Error> = 
  | { success: true; data: TData }
  | { success: false; error: TError }

/ ✅ BON : Utility types
type PartialBy<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>
type RequiredBy<T, K extends keyof T> = T & Required<Pick<T, K>>

/ Utilisation
type UserUpdate = PartialBy<User, 'firstName' | 'lastName'>
```

### Types Stricts

```typescript
/ ✅ BON : Types stricts, pas de any
function processUser(user: User): UserResponse {
  return { success: true, data: user }
}

/ ❌ MAUVAIS : any
function processData(data: any): any { / ❌
  return data
}

/ ✅ BON : unknown pour données inconnues
function parseJson(json: string): unknown {
  return JSON.parse(json)
}

/ ✅ BON : Type guards pour unknown
function isUser(value: unknown): value is User {
  return (
    typeof value === 'object' &&
    value !== null &&
    'id' in value &&
    'email' in value
  )
}
```

## 🔒 Immutabilité

```typescript
/ ✅ BON : readonly pour propriétés immuables
interface Config {
  readonly apiUrl: string
  readonly timeout: number
}

/ ✅ BON : Readonly utility type
type ImmutableUser = Readonly<User>

/ ✅ BON : as const pour littéraux immuables
const STATUSES = ['active', 'inactive', 'pending'] as const
type Status = typeof STATUSES[number] / 'active' | 'inactive' | 'pending'

/ ✅ BON : ReadonlyArray pour tableaux immuables
function processItems(items: ReadonlyArray<Item>): void {
  / items.push(newItem) / ❌ Erreur de compilation
}

/ ✅ BON : Object.freeze pour runtime
const config = Object.freeze({
  apiUrl: 'https:/api.example.com',
  timeout: 5000,
})
```

## 📦 Exports et Imports

### Organisation des Exports

```typescript
/ types/user.types.ts
export interface User {
  id: string
  email: string
}

export interface UserCreateDto {
  email: string
  password: string
}

export type UserRole = 'admin' | 'user' | 'guest'

/ types/index.ts (Barrel export)
export * from './user.types'
export * from './api.types'
export * from './order.types'

/ Utilisation
import type { User, UserCreateDto, UserRole } from '@/types'
```

### Import Type-Only

```typescript
/ ✅ BON : import type pour les types uniquement
import type { User, ApiResponse } from '@/types'
import { formatDate } from '@/utils/formatters'

/ ✅ BON : Séparation claire types vs valeurs
import type { AxiosInstance, AxiosRequestConfig } from 'axios'
import axios from 'axios'
```

## ⚠️ Erreurs et Exceptions

### Result Pattern

```typescript
/ types/result.types.ts
type Result<T, E = Error> = 
  | { ok: true; value: T }
  | { ok: false; error: E }

/ Helpers
function ok<T>(value: T): Result<T, never> {
  return { ok: true, value }
}

function err<E>(error: E): Result<never, E> {
  return { ok: false, error }
}

/ Utilisation
async function findUser(id: string): Promise<Result<User, 'NOT_FOUND' | 'DB_ERROR'>> {
  try {
    const user = await db.users.findUnique({ where: { id } })
    if (!user) {
      return err('NOT_FOUND')
    }
    return ok(user)
  } catch {
    return err('DB_ERROR')
  }
}

/ Consommation
const result = await findUser('123')
if (result.ok) {
  console.log(result.value.email)
} else {
  console.error(result.error) / 'NOT_FOUND' | 'DB_ERROR'
}
```

### Custom Errors

```typescript
/ errors/app.errors.ts
export class AppError extends Error {
  constructor(
    message: string,
    public readonly code: string,
    public readonly statusCode: number = 500
  ) {
    super(message)
    this.name = this.constructor.name
  }
}

export class NotFoundError extends AppError {
  constructor(resource: string, id: string) {
    super(`${resource} with id ${id} not found`, 'NOT_FOUND', 404)
  }
}

export class ValidationError extends AppError {
  constructor(
    message: string,
    public readonly errors: Record<string, string[]>
  ) {
    super(message, 'VALIDATION_ERROR', 400)
  }
}

/ Utilisation
throw new NotFoundError('User', '123')
throw new ValidationError('Invalid input', {
  email: ['Invalid email format'],
  password: ['Password too short'],
})
```

## 🧪 Tests

### Types pour Tests

```typescript
/ types/test.types.ts
import type { Mock } from 'vitest'

export type MockedFunction<T extends (...args: any[]) => any> = Mock<
  Parameters<T>,
  ReturnType<T>
>

/ Utilisation dans les tests
import type { MockedFunction } from '@/types/test.types'

const mockFetch: MockedFunction<typeof fetch> = vi.fn()
```

## ✅ Checklist TypeScript

**Avant de compléter du code TypeScript, VÉRIFIER :**

- [ ] ADR pertinents consultés et respectés
- [ ] Structure de dossiers conforme
- [ ] Pas de `any` (utiliser `unknown` si nécessaire)
- [ ] Types/Interfaces correctement nommés (PascalCase)
- [ ] `import type` pour imports de types uniquement
- [ ] Exports centralisés dans `index.ts` (barrel exports)
- [ ] Propriétés `readonly` quand approprié
- [ ] Génériques avec contraintes si nécessaire
- [ ] Erreurs typées (pas de `throw new Error('...')` générique)
- [ ] Enums évités (préférer union types ou `as const`)
- [ ] `strict: true` dans tsconfig.json
