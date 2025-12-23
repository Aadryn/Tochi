---
description: TypeScript Testing - Vitest, Jest, mocking, type-safe assertions, test patterns
name: TypeScript_Testing
applyTo: "**/*.spec.ts,**/*.test.ts"
---

# TypeScript - Guide de Testing

Guide complet pour écrire des tests TypeScript avec Vitest/Jest.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** `any` dans les tests (même pour les mocks)
- **Ne teste jamais** l'implémentation, teste le comportement
- **N'écris jamais** de test sans assertions
- **Ne partage jamais** d'état mutable entre tests
- **N'utilise jamais** de timeouts arbitraires (utilise `waitFor`)
- **Ne mock jamais** plus que nécessaire (tests fragiles)
- **Ne skip jamais** de tests sans `TODO` documenté

## ✅ À FAIRE

- **Type toujours** les mocks explicitement
- **Isole toujours** chaque test (pas d'effets de bord)
- **Nomme toujours** les tests avec le pattern Given/When/Then ou Should
- **Utilise toujours** des factories pour créer les données de test
- **Structure toujours** avec Arrange-Act-Assert (AAA)
- **Mock toujours** les dépendances externes (API, DB)
- **Vérifie toujours** les types avec `expectTypeOf` (Vitest)

## 📦 Configuration Vitest

### vitest.config.ts

```typescript
import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

export default defineConfig({
  plugins: [vue()],
  test: {
    globals: true,
    environment: 'jsdom',
    include: ['**/*.{test,spec}.{ts,tsx}'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/**',
        'dist/**',
        '**/*.d.ts',
        '**/*.config.ts',
        '**/types/**'
      ]
    },
    setupFiles: ['./src/test-utils/setup.ts']
  },
  resolve: {
    alias: {
      '@': resolve(__dirname, './src')
    }
  }
})
```

### Setup File

```typescript
/ src/test-utils/setup.ts
import { vi, beforeEach, afterEach } from 'vitest'
import { config } from '@vue/test-utils'

/ Reset tous les mocks entre les tests
beforeEach(() => {
  vi.clearAllMocks()
})

afterEach(() => {
  vi.restoreAllMocks()
})

/ Configuration globale Vue Test Utils
config.global.stubs = {
  / Stub des composants externes
  teleport: true
}

/ Mock global de fetch
global.fetch = vi.fn()

/ Mock de localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn()
}
Object.defineProperty(window, 'localStorage', { value: localStorageMock })
```

## 🏭 Test Factories

### User Factory

```typescript
/ src/test-utils/factories/user.factory.ts
import type { User, CreateUserDto } from '@/types'

let userIdCounter = 1

export function createUser(overrides: Partial<User> = {}): User {
  return {
    id: `user-${userIdCounter++}`,
    name: 'John Doe',
    email: 'john@example.com',
    role: 'user',
    createdAt: new Date('2024-01-01'),
    updatedAt: new Date('2024-01-01'),
    ...overrides
  }
}

export function createUsers(count: number, overrides: Partial<User> = {}): User[] {
  return Array.from({ length: count }, (_, i) =>
    createUser({ ...overrides, name: `User ${i + 1}` })
  )
}

export function createUserDto(overrides: Partial<CreateUserDto> = {}): CreateUserDto {
  return {
    name: 'John Doe',
    email: 'john@example.com',
    password: 'SecureP@ss123',
    ...overrides
  }
}
```

### API Response Factory

```typescript
/ src/test-utils/factories/api.factory.ts
import type { ApiResponse, PaginatedResponse } from '@/types'

export function createApiResponse<T>(
  data: T,
  overrides: Partial<ApiResponse<T>> = {}
): ApiResponse<T> {
  return {
    success: true,
    data,
    message: 'Success',
    ...overrides
  }
}

export function createPaginatedResponse<T>(
  items: T[],
  overrides: Partial<PaginatedResponse<T>> = {}
): PaginatedResponse<T> {
  return {
    items,
    total: items.length,
    page: 1,
    pageSize: 10,
    totalPages: Math.ceil(items.length / 10),
    ...overrides
  }
}

export function createErrorResponse(
  message: string,
  code = 'ERROR'
): ApiResponse<never> {
  return {
    success: false,
    error: { message, code }
  } as ApiResponse<never>
}
```

## 🎭 Mocking Patterns

### Mock de Modules

```typescript
/ ✅ BON : Mock typé de module
import { vi, describe, it, expect, beforeEach } from 'vitest'
import { useAuth } from '@/composables/useAuth'
import type { User } from '@/types'

/ Mock du module
vi.mock('@/composables/useAuth', () => ({
  useAuth: vi.fn()
}))

/ Type helper pour le mock
const mockedUseAuth = vi.mocked(useAuth)

describe('UserProfile', () => {
  beforeEach(() => {
    mockedUseAuth.mockReturnValue({
      user: ref<User | null>(createUser()),
      isAuthenticated: ref(true),
      login: vi.fn(),
      logout: vi.fn()
    })
  })
  
  it('should display user name', () => {
    / Test...
  })
})
```

### Mock de Fetch/API

```typescript
/ ✅ BON : Mock typé de fetch
import { vi, describe, it, expect, beforeEach } from 'vitest'

const mockFetch = vi.fn()
global.fetch = mockFetch

function mockFetchResponse<T>(data: T, status = 200): void {
  mockFetch.mockResolvedValueOnce({
    ok: status >= 200 && status < 300,
    status,
    json: () => Promise.resolve(data)
  } as Response)
}

describe('UserService', () => {
  beforeEach(() => {
    mockFetch.mockClear()
  })
  
  it('should fetch user by id', async () => {
    const user = createUser({ id: '123', name: 'Alice' })
    mockFetchResponse(user)
    
    const result = await userService.getById('123')
    
    expect(result).toEqual(user)
    expect(mockFetch).toHaveBeenCalledWith('/api/users/123', expect.any(Object))
  })
  
  it('should handle 404 error', async () => {
    mockFetchResponse({ error: 'Not found' }, 404)
    
    await expect(userService.getById('999')).rejects.toThrow('User not found')
  })
})
```

### Mock de Services

```typescript
/ ✅ BON : Service mock avec type complet
import type { UserService } from '@/services/user.service'

function createMockUserService(): jest.Mocked<UserService> {
  return {
    getById: vi.fn(),
    getAll: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn()
  }
}

describe('UserController', () => {
  let userService: jest.Mocked<UserService>
  let controller: UserController
  
  beforeEach(() => {
    userService = createMockUserService()
    controller = new UserController(userService)
  })
  
  it('should get user by id', async () => {
    const user = createUser()
    userService.getById.mockResolvedValue(user)
    
    const result = await controller.getUser('123')
    
    expect(result).toEqual(user)
    expect(userService.getById).toHaveBeenCalledWith('123')
  })
})
```

## 📝 Patterns de Test

### Test Unitaire - Fonction Pure

```typescript
/ src/utils/formatters.ts
export function formatCurrency(amount: number, currency = 'EUR'): string {
  return new Intl.NumberFormat('fr-FR', {
    style: 'currency',
    currency
  }).format(amount)
}

/ src/utils/__tests__/formatters.spec.ts
import { describe, it, expect } from 'vitest'
import { formatCurrency } from '../formatters'

describe('formatCurrency', () => {
  it('should format amount in EUR by default', () => {
    expect(formatCurrency(1234.56)).toBe('1 234,56 €')
  })
  
  it('should format amount in specified currency', () => {
    expect(formatCurrency(1234.56, 'USD')).toBe('1 234,56 $US')
  })
  
  it('should handle zero', () => {
    expect(formatCurrency(0)).toBe('0,00 €')
  })
  
  it('should handle negative amounts', () => {
    expect(formatCurrency(-100)).toBe('-100,00 €')
  })
})
```

### Test de Service Async

```typescript
/ src/services/__tests__/user.service.spec.ts
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { UserService } from '../user.service'
import { createUser, createUsers } from '@/test-utils/factories'

describe('UserService', () => {
  let service: UserService
  const mockFetch = vi.fn()
  
  beforeEach(() => {
    global.fetch = mockFetch
    mockFetch.mockClear()
    service = new UserService()
  })
  
  describe('getById', () => {
    it('should return user when found', async () => {
      / Arrange
      const expectedUser = createUser({ id: '123' })
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(expectedUser)
      })
      
      / Act
      const result = await service.getById('123')
      
      / Assert
      expect(result).toEqual(expectedUser)
      expect(mockFetch).toHaveBeenCalledWith(
        '/api/users/123',
        expect.objectContaining({ method: 'GET' })
      )
    })
    
    it('should throw when user not found', async () => {
      / Arrange
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 404
      })
      
      / Act & Assert
      await expect(service.getById('999'))
        .rejects
        .toThrow('User not found')
    })
  })
  
  describe('getAll', () => {
    it('should return paginated users', async () => {
      / Arrange
      const users = createUsers(5)
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({
          items: users,
          total: 5,
          page: 1,
          pageSize: 10
        })
      })
      
      / Act
      const result = await service.getAll({ page: 1, pageSize: 10 })
      
      / Assert
      expect(result.items).toHaveLength(5)
      expect(result.total).toBe(5)
    })
  })
})
```

### Test de Composable Vue

```typescript
/ src/composables/__tests__/useCounter.spec.ts
import { describe, it, expect } from 'vitest'
import { useCounter } from '../useCounter'

describe('useCounter', () => {
  it('should initialize with default value', () => {
    const { count } = useCounter()
    
    expect(count.value).toBe(0)
  })
  
  it('should initialize with provided value', () => {
    const { count } = useCounter(10)
    
    expect(count.value).toBe(10)
  })
  
  it('should increment counter', () => {
    const { count, increment } = useCounter()
    
    increment()
    
    expect(count.value).toBe(1)
  })
  
  it('should decrement counter', () => {
    const { count, decrement } = useCounter(5)
    
    decrement()
    
    expect(count.value).toBe(4)
  })
  
  it('should reset to initial value', () => {
    const { count, increment, reset } = useCounter(10)
    
    increment()
    increment()
    reset()
    
    expect(count.value).toBe(10)
  })
})
```

### Test de Composant Vue

```typescript
/ src/components/__tests__/UserCard.spec.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import UserCard from '../UserCard.vue'
import { createUser } from '@/test-utils/factories'

describe('UserCard', () => {
  it('should render user name', () => {
    const user = createUser({ name: 'Alice' })
    
    const wrapper = mount(UserCard, {
      props: { user }
    })
    
    expect(wrapper.text()).toContain('Alice')
  })
  
  it('should render user email', () => {
    const user = createUser({ email: 'alice@example.com' })
    
    const wrapper = mount(UserCard, {
      props: { user }
    })
    
    expect(wrapper.text()).toContain('alice@example.com')
  })
  
  it('should emit delete event when delete button clicked', async () => {
    const user = createUser()
    
    const wrapper = mount(UserCard, {
      props: { user }
    })
    
    await wrapper.find('[data-testid="delete-button"]').trigger('click')
    
    expect(wrapper.emitted('delete')).toBeTruthy()
    expect(wrapper.emitted('delete')![0]).toEqual([user.id])
  })
  
  it('should show loading state', () => {
    const user = createUser()
    
    const wrapper = mount(UserCard, {
      props: { user, loading: true }
    })
    
    expect(wrapper.find('[data-testid="loading-spinner"]').exists()).toBe(true)
  })
})
```

## ✅ Type Testing avec expectTypeOf

```typescript
import { describe, it, expectTypeOf } from 'vitest'
import type { User, CreateUserDto } from '@/types'
import { createUser } from '@/test-utils/factories'

describe('Type Tests', () => {
  it('should have correct User type', () => {
    const user = createUser()
    
    expectTypeOf(user).toMatchTypeOf<User>()
    expectTypeOf(user.id).toBeString()
    expectTypeOf(user.createdAt).toMatchTypeOf<Date>()
  })
  
  it('should have correct function signature', () => {
    expectTypeOf(createUser).toBeFunction()
    expectTypeOf(createUser).parameters.toMatchTypeOf<[Partial<User>?]>()
    expectTypeOf(createUser).returns.toMatchTypeOf<User>()
  })
})
```

## 📊 Coverage et Assertions

```typescript
/ Configuration coverage minimum
/ vitest.config.ts
{
  test: {
    coverage: {
      thresholds: {
        branches: 80,
        functions: 80,
        lines: 80,
        statements: 80
      }
    }
  }
}

/ Script package.json
{
  "scripts": {
    "test": "vitest",
    "test:run": "vitest run",
    "test:coverage": "vitest run --coverage",
    "test:ui": "vitest --ui"
  }
}
```
