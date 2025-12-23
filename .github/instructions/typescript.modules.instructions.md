---
description: TypeScript Modules - Import/Export, Namespaces, Declaration files, Barrel exports
name: TypeScript_Modules
applyTo: "**/frontend/**/*.ts,**/frontend/**/*.d.ts"
---

# TypeScript Modules

Guide complet pour l'organisation des modules et imports en TypeScript.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** `export default` pour les utilitaires et types
- **N'importe jamais** tout un module avec `import *` sans raison
- **Ne crée jamais** de dépendances circulaires entre modules
- **N'utilise jamais** les namespaces dans du code moderne (préférer les modules ES)
- **Ne mélange jamais** CommonJS et ES modules dans le même projet

## ✅ À FAIRE

- **Utilise toujours** les named exports pour la cohérence
- **Utilise toujours** les barrel files (index.ts) pour les exports publics
- **Organise toujours** les imports par catégorie
- **Utilise toujours** les path aliases pour éviter les imports relatifs profonds
- **Documente toujours** les exports publics d'un module

## 📦 Organisation des Imports

### Ordre des Imports

```typescript
// ✅ BON : Imports organisés par catégorie

// 1. Imports Node.js built-in
import { readFile, writeFile } from 'node:fs/promises';
import { join, resolve } from 'node:path';

// 2. Imports de frameworks/librairies externes
import { ref, computed, watch } from 'vue';
import { useRouter } from 'vue-router';
import { z } from 'zod';

// 3. Imports internes avec alias (@/)
import { useAuth } from '@/composables/useAuth';
import { UserService } from '@/services/UserService';
import type { User, UserRole } from '@/types/user.types';

// 4. Imports relatifs (même feature/module)
import { validateEmail } from './validators';
import { formatUserName } from './formatters';
import type { FormState } from './types';

// 5. Imports de styles (si applicable)
import './UserForm.css';
```

### Configuration des Path Aliases

```json
// tsconfig.json
{
  "compilerOptions": {
    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"],
      "@/components/*": ["src/components/*"],
      "@/composables/*": ["src/composables/*"],
      "@/services/*": ["src/services/*"],
      "@/stores/*": ["src/stores/*"],
      "@/types/*": ["src/types/*"],
      "@/utils/*": ["src/utils/*"],
      "@/api/*": ["src/api/*"],
      "#shared/*": ["shared/*"]
    }
  }
}
```

```typescript
// vite.config.ts
import { defineConfig } from 'vite';
import { resolve } from 'node:path';

export default defineConfig({
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
      '#shared': resolve(__dirname, 'shared'),
    },
  },
});
```

## 📤 Patterns d'Export

### Named Exports (Recommandé)

```typescript
// ✅ BON : Named exports
// services/UserService.ts
export class UserService {
  async findById(id: string): Promise<User | null> {
    // ...
  }
}

// Exports multiples
export function validateUser(user: User): boolean {
  // ...
}

export const USER_ROLES = ['admin', 'user', 'moderator'] as const;

export type UserRole = (typeof USER_ROLES)[number];

// Import côté consommateur
import { UserService, validateUser, USER_ROLES, type UserRole } from '@/services/UserService';
```

### Export Default (Cas Spécifiques)

```typescript
// ✅ Acceptable pour les composants Vue
// components/UserCard.vue
export default defineComponent({
  name: 'UserCard',
  // ...
});

// ✅ Acceptable pour la configuration
// config/database.config.ts
const databaseConfig = {
  host: process.env.DB_HOST,
  port: Number(process.env.DB_PORT),
  database: process.env.DB_NAME,
} as const;

export default databaseConfig;

// ❌ MAUVAIS : Export default pour utilitaires
// utils/format.ts
export default function formatDate(date: Date): string {
  // Difficile à refactorer, pas d'auto-import intelligent
}

// ✅ BON : Named export
export function formatDate(date: Date): string {
  // Facile à refactorer, auto-import intelligent
}
```

### Re-exports et Barrel Files

```typescript
// ✅ Barrel file pour exports publics
// types/index.ts
export type { User, UserRole, UserStatus } from './user.types';
export type { Product, ProductCategory } from './product.types';
export type { Order, OrderStatus, OrderItem } from './order.types';
export type { ApiResponse, PaginatedResponse, ApiError } from './api.types';

// ✅ Re-export avec renommage
export { UserService as UserRepository } from './UserService';
export { default as config } from './config';

// ✅ Re-export de tout un module (à utiliser avec parcimonie)
export * from './constants';

// ❌ MAUVAIS : Re-export de * depuis plusieurs modules (conflits possibles)
export * from './user.types';
export * from './product.types'; // Risque de conflits de noms
```

### Structure de Feature Module

```
src/
├── features/
│   └── users/
│       ├── index.ts              # Barrel file (exports publics)
│       ├── types.ts              # Types internes
│       ├── api.ts                # Appels API
│       ├── store.ts              # State management
│       ├── composables/
│       │   ├── index.ts          # Barrel
│       │   ├── useUsers.ts
│       │   └── useUserForm.ts
│       ├── components/
│       │   ├── index.ts          # Barrel
│       │   ├── UserList.vue
│       │   ├── UserCard.vue
│       │   └── UserForm.vue
│       └── views/
│           ├── index.ts          # Barrel
│           ├── UsersView.vue
│           └── UserDetailView.vue
```

```typescript
// features/users/index.ts (Barrel file principal)

// Types publics
export type { User, UserRole, CreateUserDTO, UpdateUserDTO } from './types';

// Composables publics
export { useUsers, useUserForm } from './composables';

// Composants publics
export { UserList, UserCard, UserForm } from './components';

// Store public
export { useUserStore } from './store';

// API publique (si nécessaire)
export { userApi } from './api';
```

## 📝 Declaration Files (.d.ts)

### Déclarations de Types Globaux

```typescript
// types/global.d.ts
declare global {
  // Étendre Window
  interface Window {
    __APP_VERSION__: string;
    __APP_CONFIG__: {
      apiUrl: string;
      debug: boolean;
    };
  }

  // Variables globales
  const __DEV__: boolean;
  const __PROD__: boolean;

  // Types utilitaires globaux
  type Nullable<T> = T | null;
  type Optional<T> = T | undefined;
  type Maybe<T> = T | null | undefined;
}

export {};
```

### Déclarations pour Modules sans Types

```typescript
// types/modules.d.ts

// Module sans types natifs
declare module 'some-untyped-library' {
  export function doSomething(value: string): number;
  export const VERSION: string;
  
  export interface LibraryOptions {
    debug?: boolean;
    timeout?: number;
  }
  
  export default class Library {
    constructor(options?: LibraryOptions);
    process(data: unknown): Promise<unknown>;
  }
}

// Fichiers non-TypeScript
declare module '*.vue' {
  import type { DefineComponent } from 'vue';
  const component: DefineComponent<object, object, unknown>;
  export default component;
}

declare module '*.svg' {
  const content: string;
  export default content;
}

declare module '*.svg?component' {
  import type { FunctionalComponent, SVGAttributes } from 'vue';
  const component: FunctionalComponent<SVGAttributes>;
  export default component;
}

declare module '*.png' {
  const value: string;
  export default value;
}

declare module '*.jpg' {
  const value: string;
  export default value;
}

declare module '*.json' {
  const value: Record<string, unknown>;
  export default value;
}

declare module '*.css' {
  const classes: Record<string, string>;
  export default classes;
}

declare module '*.module.css' {
  const classes: Record<string, string>;
  export default classes;
}
```

### Augmentation de Modules Existants

```typescript
// types/vue-router.d.ts
import 'vue-router';

declare module 'vue-router' {
  interface RouteMeta {
    /** Titre de la page */
    title?: string;
    
    /** Requiert une authentification */
    requiresAuth?: boolean;
    
    /** Rôles autorisés */
    roles?: string[];
    
    /** Layout à utiliser */
    layout?: 'default' | 'admin' | 'auth';
    
    /** Icône pour le menu */
    icon?: string;
    
    /** Ordre dans le menu */
    order?: number;
  }
}

// types/pinia.d.ts
import 'pinia';

declare module 'pinia' {
  export interface PiniaCustomProperties {
    /** Router instance */
    $router: import('vue-router').Router;
    
    /** Toast service */
    $toast: import('@/services/toast').ToastService;
  }
}

// types/axios.d.ts
import 'axios';

declare module 'axios' {
  export interface AxiosRequestConfig {
    /** Skip authentication header */
    skipAuth?: boolean;
    
    /** Retry configuration */
    retry?: {
      count: number;
      delay: number;
    };
  }
}
```

### Déclarations de Types Ambient

```typescript
// types/env.d.ts
/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** URL de base de l'API */
  readonly VITE_API_URL: string;
  
  /** Environnement */
  readonly VITE_APP_ENV: 'development' | 'staging' | 'production';
  
  /** Version de l'application */
  readonly VITE_APP_VERSION: string;
  
  /** Clé API Analytics */
  readonly VITE_ANALYTICS_KEY?: string;
  
  /** Mode debug */
  readonly VITE_DEBUG?: 'true' | 'false';
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
```

## 🔄 Gestion des Dépendances Circulaires

### Détection des Cycles

```typescript
// ❌ MAUVAIS : Dépendance circulaire
// services/UserService.ts
import { OrderService } from './OrderService';

export class UserService {
  constructor(private orderService: OrderService) {}
}

// services/OrderService.ts
import { UserService } from './UserService'; // Cycle!

export class OrderService {
  constructor(private userService: UserService) {}
}

// ✅ BON : Extraire l'interface commune
// interfaces/services.interfaces.ts
export interface IUserService {
  findById(id: string): Promise<User | null>;
}

export interface IOrderService {
  findByUserId(userId: string): Promise<Order[]>;
}

// services/UserService.ts
import type { IOrderService } from '@/interfaces/services.interfaces';

export class UserService implements IUserService {
  constructor(private orderService: IOrderService) {}
}

// services/OrderService.ts
import type { IUserService } from '@/interfaces/services.interfaces';

export class OrderService implements IOrderService {
  constructor(private userService: IUserService) {}
}
```

### Pattern d'Injection Tardive

```typescript
// ✅ Injection tardive pour éviter les cycles
// services/ServiceContainer.ts
class ServiceContainer {
  private services = new Map<string, unknown>();

  register<T>(key: string, factory: () => T): void {
    // Lazy instantiation
    Object.defineProperty(this, key, {
      get: () => {
        if (!this.services.has(key)) {
          this.services.set(key, factory());
        }
        return this.services.get(key);
      },
      configurable: true,
    });
  }

  get<T>(key: string): T {
    return (this as Record<string, unknown>)[key] as T;
  }
}

// Configuration
const container = new ServiceContainer();

container.register('userService', () => new UserService(container.get('orderService')));
container.register('orderService', () => new OrderService(container.get('userService')));
```

### Séparation des Couches

```typescript
// ✅ Architecture en couches pour éviter les cycles
// domain/entities/User.ts
export class User {
  constructor(
    public readonly id: string,
    public readonly email: string,
    public readonly name: string,
  ) {}
}

// domain/entities/Order.ts
export class Order {
  constructor(
    public readonly id: string,
    public readonly userId: string, // Référence par ID, pas par instance
    public readonly items: OrderItem[],
  ) {}
}

// application/services/UserApplicationService.ts
// Dépend uniquement de la couche domain et infrastructure
import type { User } from '@/domain/entities/User';
import type { IUserRepository } from '@/domain/repositories/IUserRepository';

export class UserApplicationService {
  constructor(private userRepo: IUserRepository) {}

  async getUserWithOrders(userId: string): Promise<{ user: User; orders: Order[] }> {
    const user = await this.userRepo.findById(userId);
    const orders = await this.orderRepo.findByUserId(userId);
    return { user, orders };
  }
}
```

## 📁 Organisation des Types

### Structure Recommandée

```
src/
├── types/
│   ├── index.ts              # Barrel principal
│   │
│   ├── common/               # Types partagés
│   │   ├── index.ts
│   │   ├── api.types.ts      # Types API génériques
│   │   ├── pagination.types.ts
│   │   └── result.types.ts
│   │
│   ├── domain/               # Types métier
│   │   ├── index.ts
│   │   ├── user.types.ts
│   │   ├── product.types.ts
│   │   └── order.types.ts
│   │
│   ├── dto/                  # Data Transfer Objects
│   │   ├── index.ts
│   │   ├── user.dto.ts
│   │   ├── product.dto.ts
│   │   └── order.dto.ts
│   │
│   └── guards/               # Type guards
│       ├── index.ts
│       └── type-guards.ts
```

### Types Partagés

```typescript
// types/common/api.types.ts
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  timestamp: string;
}

export interface ApiError {
  code: string;
  message: string;
  details?: Record<string, string[]>;
}

export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

// types/common/result.types.ts
export type Result<T, E = Error> =
  | { success: true; data: T }
  | { success: false; error: E };

export function ok<T>(data: T): Result<T, never> {
  return { success: true, data };
}

export function err<E>(error: E): Result<never, E> {
  return { success: false, error };
}

export function isOk<T, E>(result: Result<T, E>): result is { success: true; data: T } {
  return result.success;
}

export function isErr<T, E>(result: Result<T, E>): result is { success: false; error: E } {
  return !result.success;
}
```

### Types de Domaine

```typescript
// types/domain/user.types.ts

/** Statuts possibles d'un utilisateur */
export const UserStatus = {
  Active: 'active',
  Inactive: 'inactive',
  Pending: 'pending',
  Banned: 'banned',
} as const;

export type UserStatus = (typeof UserStatus)[keyof typeof UserStatus];

/** Rôles utilisateur */
export const UserRole = {
  Admin: 'admin',
  Moderator: 'moderator',
  User: 'user',
  Guest: 'guest',
} as const;

export type UserRole = (typeof UserRole)[keyof typeof UserRole];

/** Entité User complète */
export interface User {
  readonly id: string;
  email: string;
  name: string;
  role: UserRole;
  status: UserStatus;
  avatar?: string;
  readonly createdAt: Date;
  updatedAt: Date;
}

/** User public (sans données sensibles) */
export type PublicUser = Pick<User, 'id' | 'name' | 'avatar'>;

/** User pour les formulaires */
export type UserFormData = Omit<User, 'id' | 'createdAt' | 'updatedAt'>;
```

### DTOs

```typescript
// types/dto/user.dto.ts
import type { User, UserRole, UserStatus } from '../domain/user.types';

/** DTO pour la création d'un utilisateur */
export interface CreateUserDTO {
  email: string;
  name: string;
  password: string;
  role?: UserRole;
}

/** DTO pour la mise à jour d'un utilisateur */
export interface UpdateUserDTO {
  email?: string;
  name?: string;
  role?: UserRole;
  status?: UserStatus;
  avatar?: string;
}

/** DTO de réponse (mapping depuis API) */
export interface UserResponseDTO {
  id: string;
  email: string;
  name: string;
  role: string;
  status: string;
  avatar: string | null;
  created_at: string;
  updated_at: string;
}

/** Mapper DTO vers Entity */
export function mapUserResponseToUser(dto: UserResponseDTO): User {
  return {
    id: dto.id,
    email: dto.email,
    name: dto.name,
    role: dto.role as UserRole,
    status: dto.status as UserStatus,
    avatar: dto.avatar ?? undefined,
    createdAt: new Date(dto.created_at),
    updatedAt: new Date(dto.updated_at),
  };
}
```

## ⚡ Dynamic Imports

### Lazy Loading de Modules

```typescript
// ✅ Import dynamique pour code splitting
async function loadAnalytics(): Promise<typeof import('@/services/analytics')> {
  return import('@/services/analytics');
}

// Usage conditionnel
if (shouldTrackAnalytics()) {
  const analytics = await loadAnalytics();
  analytics.track('page_view', { page: '/home' });
}

// ✅ Avec gestion d'erreur
async function loadHeavyModule(): Promise<void> {
  try {
    const { HeavyProcessor } = await import('@/services/HeavyProcessor');
    const processor = new HeavyProcessor();
    await processor.process(data);
  } catch (error) {
    console.error('Failed to load HeavyProcessor:', error);
    // Fallback ou notification utilisateur
  }
}

// ✅ Chargement parallèle de modules
async function initializeApp(): Promise<void> {
  const [
    { AuthService },
    { ApiService },
    { StorageService },
  ] = await Promise.all([
    import('@/services/AuthService'),
    import('@/services/ApiService'),
    import('@/services/StorageService'),
  ]);

  const auth = new AuthService();
  const api = new ApiService();
  const storage = new StorageService();
  
  // Initialize...
}
```

### Factory avec Import Dynamique

```typescript
// ✅ Factory pattern avec lazy loading
type ModuleName = 'stripe' | 'paypal' | 'mollie';

async function getPaymentProvider(name: ModuleName): Promise<PaymentProvider> {
  switch (name) {
    case 'stripe': {
      const { StripeProvider } = await import('@/providers/StripeProvider');
      return new StripeProvider();
    }
    case 'paypal': {
      const { PayPalProvider } = await import('@/providers/PayPalProvider');
      return new PayPalProvider();
    }
    case 'mollie': {
      const { MollieProvider } = await import('@/providers/MollieProvider');
      return new MollieProvider();
    }
    default:
      throw new Error(`Unknown payment provider: ${name satisfies never}`);
  }
}

// Usage
const provider = await getPaymentProvider('stripe');
await provider.processPayment(amount);
```

### Preloading de Modules

```typescript
// ✅ Preload modules qui seront probablement utilisés
function preloadModules(): void {
  // Utiliser webpackPrefetch ou import() selon le bundler
  const modules = [
    () => import('@/features/dashboard'),
    () => import('@/features/settings'),
  ];

  // Preload après le chargement initial
  if ('requestIdleCallback' in window) {
    window.requestIdleCallback(() => {
      modules.forEach(loadModule => loadModule());
    });
  } else {
    setTimeout(() => {
      modules.forEach(loadModule => loadModule());
    }, 2000);
  }
}

// Dans Vue Router
const routes = [
  {
    path: '/dashboard',
    component: () => import(/* webpackPrefetch: true */ '@/views/DashboardView.vue'),
  },
];
```

## 🔧 Configuration TypeScript Avancée

### tsconfig.json Optimisé

```json
{
  "compilerOptions": {
    // Modules
    "module": "ESNext",
    "moduleResolution": "bundler",
    "resolveJsonModule": true,
    "allowSyntheticDefaultImports": true,
    "esModuleInterop": true,
    
    // Types
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true,
    "strictFunctionTypes": true,
    "strictBindCallApply": true,
    "strictPropertyInitialization": true,
    "noImplicitThis": true,
    "useUnknownInCatchVariables": true,
    "noUncheckedIndexedAccess": true,
    
    // Imports/Exports
    "isolatedModules": true,
    "verbatimModuleSyntax": true,
    "noEmit": true,
    
    // Path aliases
    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"]
    },
    
    // Types
    "types": ["vite/client", "node"],
    "typeRoots": ["./node_modules/@types", "./src/types"]
  },
  "include": ["src/**/*", "tests/**/*"],
  "exclude": ["node_modules", "dist"]
}
```

### Project References

```json
// tsconfig.json (racine)
{
  "files": [],
  "references": [
    { "path": "./packages/core" },
    { "path": "./packages/ui" },
    { "path": "./packages/api" }
  ]
}

// packages/core/tsconfig.json
{
  "compilerOptions": {
    "composite": true,
    "rootDir": "./src",
    "outDir": "./dist"
  },
  "include": ["src/**/*"]
}

// packages/ui/tsconfig.json
{
  "compilerOptions": {
    "composite": true,
    "rootDir": "./src",
    "outDir": "./dist"
  },
  "references": [
    { "path": "../core" }
  ],
  "include": ["src/**/*"]
}
```
