---
description: TypeScript Utilities - Utility types, helper functions, type guards, branded types
name: TypeScript_Utilities
applyTo: "**/utils/**/*.ts,**/helpers/**/*.ts,**/test-utils/**/*.ts"
---

# TypeScript Utilities

Guide complet pour les utility types et helper functions TypeScript.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** `any` comme fallback dans les utility types
- **Ne crée jamais** de types récursifs sans limite de profondeur
- **N'utilise jamais** `as` pour forcer un type (préférer les type guards)
- **Ne duplique jamais** des utility types qui existent déjà en built-in
- **N'oublie jamais** de tester les edge cases des utility types custom

## ✅ À FAIRE

- **Utilise toujours** les utility types built-in quand disponibles
- **Documente toujours** les utility types custom avec JSDoc
- **Type toujours** les retours des type guards comme predicates
- **Utilise toujours** `satisfies` pour valider les literal types
- **Teste toujours** les utility types avec des assertions de type

## 📌 Utility Types Built-in

### Manipulation de Propriétés

```typescript
interface User {
  id: string;
  name: string;
  email: string;
  age: number;
  isAdmin: boolean;
}

/ Partial<T> - Toutes les propriétés optionnelles
type PartialUser = Partial<User>;
/ { id?: string; name?: string; email?: string; age?: number; isAdmin?: boolean; }

/ Required<T> - Toutes les propriétés requises
interface Config {
  apiUrl?: string;
  timeout?: number;
}
type RequiredConfig = Required<Config>;
/ { apiUrl: string; timeout: number; }

/ Readonly<T> - Toutes les propriétés en lecture seule
type ReadonlyUser = Readonly<User>;

/ Pick<T, K> - Sélectionner certaines propriétés
type UserCredentials = Pick<User, 'email' | 'name'>;
/ { email: string; name: string; }

/ Omit<T, K> - Exclure certaines propriétés
type PublicUser = Omit<User, 'isAdmin' | 'email'>;
/ { id: string; name: string; age: number; }

/ Record<K, V> - Créer un objet avec des clés K et valeurs V
type UserRoles = Record<string, 'admin' | 'user' | 'guest'>;

const roles: UserRoles = {
  alice: 'admin',
  bob: 'user',
};
```

### Manipulation de Types

```typescript
/ Exclude<T, U> - Exclure U de T (union types)
type Status = 'pending' | 'approved' | 'rejected' | 'cancelled';
type ActiveStatus = Exclude<Status, 'cancelled'>;
/ 'pending' | 'approved' | 'rejected'

/ Extract<T, U> - Extraire U de T (union types)
type SuccessStatus = Extract<Status, 'approved' | 'pending'>;
/ 'approved' | 'pending'

/ NonNullable<T> - Exclure null et undefined
type MaybeString = string | null | undefined;
type DefiniteString = NonNullable<MaybeString>;
/ string

/ ReturnType<T> - Type de retour d'une fonction
function createUser(name: string): User {
  return { id: '1', name, email: '', age: 0, isAdmin: false };
}
type CreateUserReturn = ReturnType<typeof createUser>;
/ User

/ Parameters<T> - Types des paramètres d'une fonction
function greet(name: string, age: number): string {
  return `Hello ${name}, ${age}`;
}
type GreetParams = Parameters<typeof greet>;
/ [name: string, age: number]

/ ConstructorParameters<T> - Paramètres d'un constructeur
class UserClass {
  constructor(public name: string, public age: number) {}
}
type UserConstructorParams = ConstructorParameters<typeof UserClass>;
/ [name: string, age: number]

/ InstanceType<T> - Type de l'instance d'une classe
type UserInstance = InstanceType<typeof UserClass>;
/ UserClass

/ ThisParameterType<T> - Type du paramètre this
function getUserName(this: User): string {
  return this.name;
}
type UserThis = ThisParameterType<typeof getUserName>;
/ User

/ OmitThisParameter<T> - Fonction sans le paramètre this
type GetUserNameFn = OmitThisParameter<typeof getUserName>;
/ () => string
```

### Manipulation de Strings (Template Literal Types)

```typescript
/ Uppercase<T>
type UpperStatus = Uppercase<'pending' | 'approved'>;
/ 'PENDING' | 'APPROVED'

/ Lowercase<T>
type LowerStatus = Lowercase<'PENDING' | 'APPROVED'>;
/ 'pending' | 'approved'

/ Capitalize<T>
type CapStatus = Capitalize<'pending' | 'approved'>;
/ 'Pending' | 'Approved'

/ Uncapitalize<T>
type UncapStatus = Uncapitalize<'Pending' | 'Approved'>;
/ 'pending' | 'approved'
```

## 🛠️ Utility Types Custom

### DeepPartial et DeepRequired

```typescript
/**
 * Rend toutes les propriétés optionnelles récursivement
 */
type DeepPartial<T> = T extends object
  ? { [P in keyof T]?: DeepPartial<T[P]> }
  : T;

/**
 * Rend toutes les propriétés requises récursivement
 */
type DeepRequired<T> = T extends object
  ? { [P in keyof T]-?: DeepRequired<T[P]> }
  : T;

/ Usage
interface NestedConfig {
  api: {
    baseUrl: string;
    timeout: number;
    headers: {
      authorization?: string;
      contentType: string;
    };
  };
  features: {
    darkMode: boolean;
    notifications: boolean;
  };
}

type PartialConfig = DeepPartial<NestedConfig>;
/ Toutes les propriétés sont optionnelles, y compris les nested

const config: PartialConfig = {
  api: {
    baseUrl: 'https:/api.example.com',
    / timeout et headers sont optionnels
  },
};
```

### DeepReadonly

```typescript
/**
 * Rend toutes les propriétés readonly récursivement
 */
type DeepReadonly<T> = T extends (infer R)[]
  ? ReadonlyArray<DeepReadonly<R>>
  : T extends Function
    ? T
    : T extends object
      ? { readonly [P in keyof T]: DeepReadonly<T[P]> }
      : T;

/ Usage
interface State {
  user: {
    name: string;
    preferences: string[];
  };
  items: { id: string; value: number }[];
}

type ImmutableState = DeepReadonly<State>;

const state: ImmutableState = {
  user: { name: 'Alice', preferences: ['dark'] },
  items: [{ id: '1', value: 10 }],
};

/ ❌ Erreur de compilation
/ state.user.name = 'Bob';
/ state.items.push({ id: '2', value: 20 });
/ state.items[0].value = 15;
```

### Nullable et NonNullableDeep

```typescript
/**
 * Ajoute null et undefined à toutes les propriétés
 */
type Nullable<T> = { [P in keyof T]: T[P] | null | undefined };

/**
 * Supprime null et undefined de toutes les propriétés récursivement
 */
type NonNullableDeep<T> = T extends object
  ? { [P in keyof T]: NonNullableDeep<NonNullable<T[P]>> }
  : NonNullable<T>;

/ Usage
interface ApiResponse {
  data: {
    user: {
      name: string | null;
      email: string | null | undefined;
    } | null;
  } | null;
}

type CleanResponse = NonNullableDeep<ApiResponse>;
/ { data: { user: { name: string; email: string; } } }
```

### Mutable

```typescript
/**
 * Supprime readonly de toutes les propriétés
 */
type Mutable<T> = {
  -readonly [P in keyof T]: T[P];
};

/**
 * Supprime readonly récursivement
 */
type DeepMutable<T> = T extends object
  ? { -readonly [P in keyof T]: DeepMutable<T[P]> }
  : T;

/ Usage
interface ImmutableUser {
  readonly id: string;
  readonly name: string;
  readonly settings: {
    readonly theme: string;
  };
}

type MutableUser = DeepMutable<ImmutableUser>;
/ { id: string; name: string; settings: { theme: string; } }
```

### PickByType et OmitByType

```typescript
/**
 * Sélectionne les propriétés dont la valeur est de type V
 */
type PickByType<T, V> = {
  [P in keyof T as T[P] extends V ? P : never]: T[P];
};

/**
 * Exclut les propriétés dont la valeur est de type V
 */
type OmitByType<T, V> = {
  [P in keyof T as T[P] extends V ? never : P]: T[P];
};

/ Usage
interface Mixed {
  id: string;
  name: string;
  age: number;
  isActive: boolean;
  score: number;
}

type StringProps = PickByType<Mixed, string>;
/ { id: string; name: string; }

type NonStringProps = OmitByType<Mixed, string>;
/ { age: number; isActive: boolean; score: number; }
```

### RequiredKeys et OptionalKeys

```typescript
/**
 * Obtient les clés requises d'un type
 */
type RequiredKeys<T> = {
  [K in keyof T]-?: {} extends Pick<T, K> ? never : K;
}[keyof T];

/**
 * Obtient les clés optionnelles d'un type
 */
type OptionalKeys<T> = {
  [K in keyof T]-?: {} extends Pick<T, K> ? K : never;
}[keyof T];

/ Usage
interface Form {
  required: string;
  alsoRequired: number;
  optional?: boolean;
  maybeOptional?: string;
}

type FormRequired = RequiredKeys<Form>;
/ 'required' | 'alsoRequired'

type FormOptional = OptionalKeys<Form>;
/ 'optional' | 'maybeOptional'
```

### FunctionKeys et NonFunctionKeys

```typescript
/**
 * Obtient les clés dont la valeur est une fonction
 */
type FunctionKeys<T> = {
  [K in keyof T]: T[K] extends Function ? K : never;
}[keyof T];

/**
 * Obtient les clés dont la valeur n'est pas une fonction
 */
type NonFunctionKeys<T> = {
  [K in keyof T]: T[K] extends Function ? never : K;
}[keyof T];

/ Usage
interface UserService {
  id: string;
  name: string;
  greet(): string;
  update(name: string): void;
}

type Methods = FunctionKeys<UserService>;
/ 'greet' | 'update'

type Properties = NonFunctionKeys<UserService>;
/ 'id' | 'name'
```

## 🏷️ Branded Types

```typescript
/**
 * Crée un type branded (nominal typing)
 */
declare const __brand: unique symbol;
type Brand<T, B> = T & { readonly [__brand]: B };

/ Types branded pour IDs
type UserId = Brand<string, 'UserId'>;
type PostId = Brand<string, 'PostId'>;
type OrderId = Brand<string, 'OrderId'>;

/ Fonctions de création (runtime validation optionnelle)
function createUserId(id: string): UserId {
  / Validation optionnelle
  if (!id.startsWith('usr_')) {
    throw new Error('Invalid user ID format');
  }
  return id as UserId;
}

function createPostId(id: string): PostId {
  return id as PostId;
}

/ Usage sécurisé - impossible de mélanger les types
function getUser(id: UserId): User {
  / ...
}

function getPost(id: PostId): Post {
  / ...
}

const userId = createUserId('usr_123');
const postId = createPostId('post_456');

getUser(userId); / ✅ OK
/ getUser(postId); / ❌ Erreur : PostId n'est pas assignable à UserId
/ getUser('usr_789'); / ❌ Erreur : string n'est pas assignable à UserId

/ Types branded pour valeurs validées
type Email = Brand<string, 'Email'>;
type PhoneNumber = Brand<string, 'PhoneNumber'>;
type PositiveNumber = Brand<number, 'PositiveNumber'>;

function parseEmail(value: string): Email | null {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(value) ? (value as Email) : null;
}

function parsePositive(value: number): PositiveNumber | null {
  return value > 0 ? (value as PositiveNumber) : null;
}
```

## 🛡️ Type Guards

### Type Guards de Base

```typescript
/**
 * Vérifie si une valeur est définie (non null, non undefined)
 */
function isDefined<T>(value: T | null | undefined): value is T {
  return value !== null && value !== undefined;
}

/**
 * Vérifie si une valeur est une string
 */
function isString(value: unknown): value is string {
  return typeof value === 'string';
}

/**
 * Vérifie si une valeur est un nombre
 */
function isNumber(value: unknown): value is number {
  return typeof value === 'number' && !Number.isNaN(value);
}

/**
 * Vérifie si une valeur est un objet non null
 */
function isObject(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

/**
 * Vérifie si une valeur est un tableau
 */
function isArray<T>(value: unknown, guard?: (item: unknown) => item is T): value is T[] {
  if (!Array.isArray(value)) return false;
  if (guard) return value.every(guard);
  return true;
}

/ Usage
const values: unknown[] = [1, 'hello', null, { key: 'value' }];

values.filter(isDefined); / (string | number | { key: string })[]
values.filter(isString);  / string[]
values.filter(isNumber);  / number[]
```

### Type Guards pour Objets

```typescript
interface User {
  id: string;
  name: string;
  email: string;
}

interface Admin extends User {
  permissions: string[];
}

/**
 * Vérifie si un objet a une propriété
 */
function hasProperty<K extends string>(
  obj: unknown,
  key: K
): obj is Record<K, unknown> {
  return isObject(obj) && key in obj;
}

/**
 * Vérifie si un objet a des propriétés
 */
function hasProperties<K extends string>(
  obj: unknown,
  keys: K[]
): obj is Record<K, unknown> {
  return isObject(obj) && keys.every((key) => key in obj);
}

/**
 * Type guard pour User
 */
function isUser(value: unknown): value is User {
  return (
    isObject(value) &&
    hasProperty(value, 'id') && isString(value.id) &&
    hasProperty(value, 'name') && isString(value.name) &&
    hasProperty(value, 'email') && isString(value.email)
  );
}

/**
 * Type guard pour Admin
 */
function isAdmin(value: unknown): value is Admin {
  return (
    isUser(value) &&
    hasProperty(value, 'permissions') &&
    isArray(value.permissions, isString)
  );
}

/ Usage
function handleUser(data: unknown): void {
  if (isAdmin(data)) {
    console.log('Admin permissions:', data.permissions);
  } else if (isUser(data)) {
    console.log('User:', data.name);
  } else {
    console.log('Unknown data');
  }
}
```

### Type Guards pour Discriminated Unions

```typescript
type Result<T, E = Error> =
  | { success: true; data: T }
  | { success: false; error: E };

/**
 * Type guard pour Result success
 */
function isSuccess<T, E>(result: Result<T, E>): result is { success: true; data: T } {
  return result.success === true;
}

/**
 * Type guard pour Result error
 */
function isError<T, E>(result: Result<T, E>): result is { success: false; error: E } {
  return result.success === false;
}

/ Événements discriminés
type AppEvent =
  | { type: 'user.created'; payload: { userId: string; name: string } }
  | { type: 'user.deleted'; payload: { userId: string } }
  | { type: 'order.placed'; payload: { orderId: string; amount: number } };

/**
 * Type guard générique pour événements discriminés
 */
function isEventType<T extends AppEvent['type']>(
  event: AppEvent,
  type: T
): event is Extract<AppEvent, { type: T }> {
  return event.type === type;
}

/ Usage
function handleEvent(event: AppEvent): void {
  if (isEventType(event, 'user.created')) {
    console.log('New user:', event.payload.name);
  } else if (isEventType(event, 'order.placed')) {
    console.log('Order amount:', event.payload.amount);
  }
}
```

## 🔧 Fonctions Helper

### Array Helpers

```typescript
/**
 * Filtre les valeurs null et undefined d'un tableau
 */
function compact<T>(array: (T | null | undefined)[]): T[] {
  return array.filter(isDefined);
}

/**
 * Déduplique un tableau
 */
function unique<T>(array: T[]): T[] {
  return [...new Set(array)];
}

/**
 * Déduplique par une clé
 */
function uniqueBy<T, K>(array: T[], getKey: (item: T) => K): T[] {
  const seen = new Set<K>();
  return array.filter((item) => {
    const key = getKey(item);
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
}

/**
 * Groupe par une clé
 */
function groupBy<T, K extends string | number>(
  array: T[],
  getKey: (item: T) => K
): Record<K, T[]> {
  return array.reduce((groups, item) => {
    const key = getKey(item);
    groups[key] = groups[key] ?? [];
    groups[key].push(item);
    return groups;
  }, {} as Record<K, T[]>);
}

/**
 * Crée un Map à partir d'un tableau
 */
function keyBy<T, K extends string | number>(
  array: T[],
  getKey: (item: T) => K
): Record<K, T> {
  return array.reduce((map, item) => {
    map[getKey(item)] = item;
    return map;
  }, {} as Record<K, T>);
}

/**
 * Partition un tableau en deux groupes
 */
function partition<T>(
  array: T[],
  predicate: (item: T) => boolean
): [T[], T[]] {
  const pass: T[] = [];
  const fail: T[] = [];

  for (const item of array) {
    if (predicate(item)) {
      pass.push(item);
    } else {
      fail.push(item);
    }
  }

  return [pass, fail];
}

/**
 * Chunk un tableau
 */
function chunk<T>(array: T[], size: number): T[][] {
  const chunks: T[][] = [];
  for (let i = 0; i < array.length; i += size) {
    chunks.push(array.slice(i, i + size));
  }
  return chunks;
}

/**
 * First avec type guard
 */
function first<T>(array: T[]): T | undefined {
  return array[0];
}

/**
 * Last avec type guard
 */
function last<T>(array: T[]): T | undefined {
  return array[array.length - 1];
}
```

### Object Helpers

```typescript
/**
 * Sélectionne des clés d'un objet
 */
function pick<T extends object, K extends keyof T>(
  obj: T,
  keys: K[]
): Pick<T, K> {
  return keys.reduce((result, key) => {
    if (key in obj) {
      result[key] = obj[key];
    }
    return result;
  }, {} as Pick<T, K>);
}

/**
 * Exclut des clés d'un objet
 */
function omit<T extends object, K extends keyof T>(
  obj: T,
  keys: K[]
): Omit<T, K> {
  const result = { ...obj };
  for (const key of keys) {
    delete result[key];
  }
  return result;
}

/**
 * Merge profond d'objets
 */
function deepMerge<T extends object>(target: T, ...sources: Partial<T>[]): T {
  if (sources.length === 0) return target;

  const source = sources.shift();
  if (!source) return target;

  for (const key in source) {
    const sourceValue = source[key];
    const targetValue = target[key];

    if (isObject(sourceValue) && isObject(targetValue)) {
      (target as Record<string, unknown>)[key] = deepMerge(
        { ...targetValue },
        sourceValue as object
      );
    } else if (sourceValue !== undefined) {
      (target as Record<string, unknown>)[key] = sourceValue;
    }
  }

  return deepMerge(target, ...sources);
}

/**
 * Clone profond (structure only, pas de fonctions)
 */
function deepClone<T>(value: T): T {
  return JSON.parse(JSON.stringify(value));
}

/**
 * Clone profond avec support des fonctions
 */
function structuredClone<T>(value: T): T {
  return globalThis.structuredClone(value);
}

/**
 * Vérifie l'égalité profonde
 */
function deepEqual<T>(a: T, b: T): boolean {
  if (a === b) return true;
  if (typeof a !== typeof b) return false;
  if (a === null || b === null) return a === b;

  if (typeof a === 'object') {
    const aKeys = Object.keys(a);
    const bKeys = Object.keys(b);

    if (aKeys.length !== bKeys.length) return false;

    return aKeys.every((key) =>
      deepEqual(
        (a as Record<string, unknown>)[key],
        (b as Record<string, unknown>)[key]
      )
    );
  }

  return false;
}
```

### String Helpers

```typescript
/**
 * Capitalize la première lettre
 */
function capitalize<T extends string>(str: T): Capitalize<T> {
  return (str.charAt(0).toUpperCase() + str.slice(1)) as Capitalize<T>;
}

/**
 * camelCase vers kebab-case
 */
function toKebabCase(str: string): string {
  return str.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase();
}

/**
 * kebab-case vers camelCase
 */
function toCamelCase(str: string): string {
  return str.replace(/-([a-z])/g, (_, char) => char.toUpperCase());
}

/**
 * snake_case vers camelCase
 */
function snakeToCamel(str: string): string {
  return str.replace(/_([a-z])/g, (_, char) => char.toUpperCase());
}

/**
 * Truncate une string
 */
function truncate(str: string, maxLength: number, suffix = '...'): string {
  if (str.length <= maxLength) return str;
  return str.slice(0, maxLength - suffix.length) + suffix;
}

/**
 * Génère un slug
 */
function slugify(str: string): string {
  return str
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '');
}
```

### Async Helpers

```typescript
/**
 * Delay/Sleep
 */
function delay(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

/**
 * Retry avec backoff exponentiel
 */
async function retry<T>(
  fn: () => Promise<T>,
  options: {
    maxRetries?: number;
    initialDelay?: number;
    maxDelay?: number;
    factor?: number;
  } = {}
): Promise<T> {
  const {
    maxRetries = 3,
    initialDelay = 1000,
    maxDelay = 30000,
    factor = 2,
  } = options;

  let lastError: Error | undefined;
  let currentDelay = initialDelay;

  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      return await fn();
    } catch (error) {
      lastError = error instanceof Error ? error : new Error(String(error));
      
      if (attempt < maxRetries) {
        await delay(currentDelay);
        currentDelay = Math.min(currentDelay * factor, maxDelay);
      }
    }
  }

  throw lastError;
}

/**
 * Timeout pour une Promise
 */
function withTimeout<T>(
  promise: Promise<T>,
  ms: number,
  message = 'Operation timed out'
): Promise<T> {
  return Promise.race([
    promise,
    new Promise<never>((_, reject) =>
      setTimeout(() => reject(new Error(message)), ms)
    ),
  ]);
}

/**
 * Debounce une fonction
 */
function debounce<T extends (...args: unknown[]) => unknown>(
  fn: T,
  ms: number
): (...args: Parameters<T>) => void {
  let timeoutId: ReturnType<typeof setTimeout>;

  return (...args: Parameters<T>) => {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => fn(...args), ms);
  };
}

/**
 * Throttle une fonction
 */
function throttle<T extends (...args: unknown[]) => unknown>(
  fn: T,
  ms: number
): (...args: Parameters<T>) => void {
  let lastCall = 0;
  let timeoutId: ReturnType<typeof setTimeout> | null = null;

  return (...args: Parameters<T>) => {
    const now = Date.now();
    const remaining = ms - (now - lastCall);

    if (remaining <= 0) {
      if (timeoutId) {
        clearTimeout(timeoutId);
        timeoutId = null;
      }
      lastCall = now;
      fn(...args);
    } else if (!timeoutId) {
      timeoutId = setTimeout(() => {
        lastCall = Date.now();
        timeoutId = null;
        fn(...args);
      }, remaining);
    }
  };
}
```

## 📦 Organisation des Utils

```typescript
/ utils/index.ts (barrel file)
export * from './array';
export * from './object';
export * from './string';
export * from './async';
export * from './guards';
export * from './types';

/ utils/types.ts
export type {
  DeepPartial,
  DeepRequired,
  DeepReadonly,
  Nullable,
  Brand,
  PickByType,
  OmitByType,
} from './utility-types';

/ utils/guards.ts
export {
  isDefined,
  isString,
  isNumber,
  isObject,
  isArray,
  hasProperty,
  hasProperties,
} from './type-guards';

/ utils/array.ts
export {
  compact,
  unique,
  uniqueBy,
  groupBy,
  keyBy,
  partition,
  chunk,
  first,
  last,
} from './array-helpers';

/ utils/object.ts
export {
  pick,
  omit,
  deepMerge,
  deepClone,
  deepEqual,
} from './object-helpers';

/ utils/string.ts
export {
  capitalize,
  toKebabCase,
  toCamelCase,
  truncate,
  slugify,
} from './string-helpers';

/ utils/async.ts
export {
  delay,
  retry,
  withTimeout,
  debounce,
  throttle,
} from './async-helpers';
```

## 🧪 Tests pour Utility Types

```typescript
/ utils/__tests__/types.test.ts
import { describe, it, expectTypeOf } from 'vitest';
import type { DeepPartial, DeepReadonly, Brand } from '../types';

describe('Utility Types', () => {
  describe('DeepPartial', () => {
    it('should make nested properties optional', () => {
      interface Nested {
        a: {
          b: {
            c: string;
          };
        };
      }

      type Result = DeepPartial<Nested>;

      / Test de type
      const valid: Result = {};
      const alsoValid: Result = { a: {} };
      const complete: Result = { a: { b: { c: 'test' } } };

      expectTypeOf(valid).toMatchTypeOf<Result>();
      expectTypeOf(alsoValid).toMatchTypeOf<Result>();
      expectTypeOf(complete).toMatchTypeOf<Result>();
    });
  });

  describe('Brand', () => {
    it('should create nominal types', () => {
      type UserId = Brand<string, 'UserId'>;
      type PostId = Brand<string, 'PostId'>;

      const userId = 'user_1' as UserId;
      const postId = 'post_1' as PostId;

      / Ces lignes devraient causer des erreurs TypeScript
      / @ts-expect-error - PostId n'est pas assignable à UserId
      const _badAssignment: UserId = postId;

      / @ts-expect-error - string n'est pas assignable à UserId
      const _badAssignment2: UserId = 'raw_string';
    });
  });
});
```
