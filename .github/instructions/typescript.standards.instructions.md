---
description: TypeScript Standards - Naming conventions, code style, ESLint, Prettier, best practices
name: TypeScript_Standards
applyTo: "**/*.ts"
---

# TypeScript - Standards et Conventions

Guide des conventions de codage et standards TypeScript.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** de noms abrégés ou cryptiques (`usr`, `mgr`, `cnt`)
- **N'utilise jamais** de préfixes hongrois (`strName`, `bIsActive`)
- **Ne mélange jamais** les styles de nommage dans un même fichier
- **N'utilise jamais** de fichiers de plus de 300 lignes sans découper
- **Ne laisse jamais** de code mort ou commenté
- **N'ignore jamais** les warnings ESLint/TypeScript
- **Ne commite jamais** de `console.log()` en production

## ✅ À FAIRE

- **Nomme toujours** de façon explicite et descriptive
- **Utilise toujours** le même style de nommage par catégorie
- **Configure toujours** ESLint + Prettier pour l'uniformité
- **Découpe toujours** les gros fichiers en modules cohérents
- **Supprime toujours** le code mort avant de commiter
- **Traite toujours** les warnings comme des erreurs
- **Utilise toujours** un logger approprié au lieu de console.log

## 📛 Conventions de Nommage

### Variables et Fonctions

```typescript
/ ✅ BON : camelCase pour variables et fonctions
const userName = 'John'
const isAuthenticated = true
const userCount = 42

function fetchUserById(id: string): Promise<User> { }
function calculateTotalPrice(items: Item[]): number { }
const handleSubmit = async (data: FormData): Promise<void> => { }

/ ❌ MAUVAIS : Autres conventions
const user_name = 'John'      / snake_case
const UserName = 'John'       / PascalCase
const ISACTIVE = true         / UPPERCASE
const usr = 'John'            / Abréviation
```

### Classes et Interfaces

```typescript
/ ✅ BON : PascalCase pour classes, interfaces, types, enums
class UserService { }
class AuthenticationManager { }

interface User { }
interface ApiResponse<T> { }

type UserId = string
type AsyncResult<T> = Promise<Result<T>>

enum UserRole {
  Admin = 'ADMIN',
  User = 'USER',
  Guest = 'GUEST'
}

/ ❌ MAUVAIS : Préfixes/suffixes non nécessaires
interface IUser { }           / Préfixe I
type TUserId = string         / Préfixe T
class UserServiceClass { }    / Suffixe Class
```

### Constantes

```typescript
/ ✅ BON : UPPER_SNAKE_CASE pour les vraies constantes
const MAX_RETRY_COUNT = 3
const API_BASE_URL = 'https:/api.example.com'
const DEFAULT_PAGE_SIZE = 20

/ Constantes d'énumération ou configuration
const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  BAD_REQUEST: 400,
  NOT_FOUND: 404
} as const

/ ✅ BON : camelCase pour les constantes de référence
const defaultUser = { name: 'Guest', role: 'guest' } as const
const emptyArray: readonly string[] = []
```

### Fichiers et Dossiers

```typescript
/ ✅ BON : kebab-case pour les noms de fichiers
/ user-service.ts
/ api-client.ts
/ use-auth.ts
/ user.types.ts

/ ✅ BON : PascalCase pour les composants Vue/React
/ UserCard.vue
/ AuthProvider.tsx
/ NavigationMenu.vue

/ ✅ BON : Structure cohérente
src/
├── services/
│   ├── user.service.ts        / kebab-case
│   └── auth.service.ts
├── composables/
│   ├── useAuth.ts             / camelCase avec "use"
│   └── useFetch.ts
├── components/
│   ├── UserCard.vue           / PascalCase
│   └── LoginForm.vue
├── types/
│   ├── user.types.ts          / kebab-case.types.ts
│   └── api.types.ts
└── utils/
    ├── formatters.ts          / kebab-case
    └── validators.ts

/ ❌ MAUVAIS
/ UserService.ts              / PascalCase pour service
/ user_service.ts             / snake_case
/ userService.ts              / camelCase pour fichier
```

## 📐 Structure de Code

### Organisation d'un Fichier

```typescript
/ ✅ BON : Ordre d'organisation standardisé

/ 1. Imports (groupés par catégorie)
/ Imports tiers
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'

/ Imports internes (avec alias @/)
import { useAuth } from '@/composables/useAuth'
import { UserService } from '@/services/user.service'

/ Imports de types (avec 'type')
import type { User, CreateUserDto } from '@/types'

/ 2. Constantes
const API_TIMEOUT = 5000
const MAX_ITEMS = 100

/ 3. Types locaux (si non exportés)
interface LocalState {
  loading: boolean
  error: Error | null
}

/ 4. Fonctions utilitaires privées
function validateInput(input: string): boolean {
  return input.length > 0
}

/ 5. Export principal (fonction, classe, composant)
export function useUsers() {
  / ...
}

/ 6. Exports secondaires
export { validateInput }
```

### Longueur et Complexité

```typescript
/ ✅ BON : Fonctions courtes et focalisées
function calculateDiscount(price: number, discountPercent: number): number {
  if (discountPercent < 0 || discountPercent > 100) {
    throw new Error('Discount must be between 0 and 100')
  }
  return price * (1 - discountPercent / 100)
}

/ ✅ BON : Extraire la logique complexe
function processOrder(order: Order): ProcessedOrder {
  const validatedOrder = validateOrder(order)
  const pricedOrder = calculatePrices(validatedOrder)
  const discountedOrder = applyDiscounts(pricedOrder)
  return finalizeOrder(discountedOrder)
}

/ ❌ MAUVAIS : Fonction trop longue
function doEverything(data: unknown): unknown {
  / 200 lignes de code...
}
```

## 🔧 Configuration ESLint

### .eslintrc.cjs

```javascript
module.exports = {
  root: true,
  env: {
    browser: true,
    es2022: true,
    node: true
  },
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/strict-type-checked',
    'plugin:@typescript-eslint/stylistic-type-checked',
    'plugin:vue/vue3-recommended',
    'prettier'
  ],
  parser: 'vue-eslint-parser',
  parserOptions: {
    parser: '@typescript-eslint/parser',
    project: './tsconfig.json',
    extraFileExtensions: ['.vue']
  },
  plugins: ['@typescript-eslint'],
  rules: {
    / TypeScript strict
    '@typescript-eslint/no-explicit-any': 'error',
    '@typescript-eslint/explicit-function-return-type': 'error',
    '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
    '@typescript-eslint/strict-boolean-expressions': 'error',
    '@typescript-eslint/no-floating-promises': 'error',
    '@typescript-eslint/await-thenable': 'error',
    '@typescript-eslint/no-misused-promises': 'error',
    
    / Naming conventions
    '@typescript-eslint/naming-convention': [
      'error',
      { selector: 'default', format: ['camelCase'] },
      { selector: 'variable', format: ['camelCase', 'UPPER_CASE'] },
      { selector: 'parameter', format: ['camelCase'], leadingUnderscore: 'allow' },
      { selector: 'typeLike', format: ['PascalCase'] },
      { selector: 'enumMember', format: ['PascalCase'] },
      { selector: 'property', format: ['camelCase', 'UPPER_CASE'] }
    ],
    
    / Best practices
    'no-console': 'warn',
    'no-debugger': 'error',
    'prefer-const': 'error',
    'no-var': 'error'
  }
}
```

## 🎨 Configuration Prettier

### .prettierrc

```json
{
  "semi": false,
  "singleQuote": true,
  "tabWidth": 2,
  "trailingComma": "none",
  "printWidth": 100,
  "bracketSpacing": true,
  "arrowParens": "avoid",
  "endOfLine": "lf",
  "vueIndentScriptAndStyle": false
}
```

### .prettierignore

```
dist/
node_modules/
coverage/
*.min.js
*.d.ts
```

## 📝 Documentation JSDoc

```typescript
/**
 * Service de gestion des utilisateurs.
 * 
 * @example
 * ```typescript
 * const service = new UserService()
 * const user = await service.getById('123')
 * ```
 */
export class UserService {
  /**
   * Récupère un utilisateur par son identifiant.
   * 
   * @param id - Identifiant unique de l'utilisateur
   * @returns L'utilisateur trouvé ou null si inexistant
   * @throws {ApiError} Si l'API renvoie une erreur
   * 
   * @example
   * ```typescript
   * const user = await service.getById('user-123')
   * if (user) {
   *   console.log(user.name)
   * }
   * ```
   */
  async getById(id: string): Promise<User | null> {
    / ...
  }
  
  /**
   * Crée un nouvel utilisateur.
   * 
   * @param data - Données de création de l'utilisateur
   * @returns L'utilisateur créé avec son ID
   * @throws {ValidationError} Si les données sont invalides
   * @throws {ConflictError} Si l'email existe déjà
   */
  async create(data: CreateUserDto): Promise<User> {
    / ...
  }
}

/**
 * Calcule le prix total avec réduction.
 * 
 * @param basePrice - Prix de base en euros
 * @param discountPercent - Pourcentage de réduction (0-100)
 * @returns Prix final après réduction
 * 
 * @remarks
 * La réduction est plafonnée à 100% (prix final minimum = 0)
 * 
 * @see {@link calculateTax} pour ajouter les taxes
 */
export function calculateFinalPrice(
  basePrice: number,
  discountPercent: number
): number {
  / ...
}
```

## ⚠️ Patterns à Respecter

### Early Return

```typescript
/ ✅ BON : Early return pour réduire l'imbrication
function processUser(user: User | null): string {
  if (!user) {
    return 'No user'
  }
  
  if (!user.isActive) {
    return 'User inactive'
  }
  
  if (user.role !== 'admin') {
    return 'Not an admin'
  }
  
  return `Admin: ${user.name}`
}

/ ❌ MAUVAIS : Imbrication profonde
function processUser(user: User | null): string {
  if (user) {
    if (user.isActive) {
      if (user.role === 'admin') {
        return `Admin: ${user.name}`
      } else {
        return 'Not an admin'
      }
    } else {
      return 'User inactive'
    }
  } else {
    return 'No user'
  }
}
```

### Destructuring

```typescript
/ ✅ BON : Destructuring pour la clarté
function displayUser({ name, email, role }: User): void {
  console.log(`${name} (${role}): ${email}`)
}

/ ✅ BON : Avec renommage
const { id: userId, name: userName } = user

/ ✅ BON : Avec valeurs par défaut
function createConfig({ timeout = 5000, retries = 3 }: Partial<Config>): Config {
  return { timeout, retries }
}

/ ❌ MAUVAIS : Accès répétitif aux propriétés
function displayUser(user: User): void {
  console.log(`${user.name} (${user.role}): ${user.email}`)
}
```

### Null Coalescing et Optional Chaining

```typescript
/ ✅ BON : Optional chaining
const userName = user?.profile?.name

/ ✅ BON : Nullish coalescing
const displayName = user?.name ?? 'Anonymous'

/ ✅ BON : Combinaison
const city = user?.address?.city ?? 'Unknown'

/ ❌ MAUVAIS : Vérifications manuelles
const userName = user && user.profile && user.profile.name
const displayName = user && user.name ? user.name : 'Anonymous'
```

## 🚫 Code à Éviter

```typescript
/ ❌ MAUVAIS : Magic numbers
if (status === 200) { }
const timeout = 5000

/ ✅ BON : Constantes nommées
const HTTP_OK = 200
const DEFAULT_TIMEOUT_MS = 5000

if (status === HTTP_OK) { }
const timeout = DEFAULT_TIMEOUT_MS

/ ❌ MAUVAIS : Conditions complexes
if (user.age >= 18 && user.country === 'FR' && user.hasAcceptedTerms && !user.isBanned) { }

/ ✅ BON : Fonctions prédicat
function canAccessService(user: User): boolean {
  return user.age >= 18 && 
         user.country === 'FR' && 
         user.hasAcceptedTerms && 
         !user.isBanned
}

if (canAccessService(user)) { }

/ ❌ MAUVAIS : Type assertions dangereuses
const data = response.data as User

/ ✅ BON : Type guard
if (isUser(response.data)) {
  const data = response.data / Typé automatiquement
}
```
