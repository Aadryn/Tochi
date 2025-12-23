---
description: Vue 3 Testing - Vitest, Vue Test Utils, Testing Library, MSW mocks, Playwright E2E
name: Vue3_Testing
applyTo: "**/frontend/**/*.spec.ts,**/frontend/**/*.test.ts,**/frontend/e2e/**/*.spec.ts"
---

# Vue 3 - Testing Guide

Guide complet pour tester les applications Vue 3 avec Vitest, Vue Test Utils et Playwright.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** Jest (Vitest est le standard Vue 3/Vite)
- **Ne teste jamais** l'implémentation, teste le comportement
- **N'utilise jamais** de timeouts arbitraires (utilise `waitFor()`)
- **Ne mock jamais** plus que nécessaire (tests fragiles)
- **N'oublie jamais** d'utiliser MSW pour mocker les appels API
- **Ne teste jamais** plusieurs comportements dans un même test
- **N'ignore jamais** les tests E2E pour les parcours critiques

## ✅ À FAIRE

- **Utilise toujours** Vitest pour les tests unitaires et d'intégration
- **Utilise toujours** Vue Test Utils pour monter les composants
- **Utilise toujours** `@testing-library/vue` pour les tests orientés utilisateur
- **Utilise toujours** MSW (Mock Service Worker) pour mocker les API
- **Structure toujours** avec Arrange-Act-Assert (Given-When-Then)
- **Isole toujours** les tests (pas de state partagé entre tests)
- **Utilise toujours** Playwright pour les tests E2E

## 🎯 Actions Obligatoires (Mandatory)

### ⚠️ LECTURE ADR OBLIGATOIRE

**AVANT d'écrire des tests, TOUJOURS consulter :**
- [009-principe-fail-fast.adr.md](../../docs/adr/009-principe-fail-fast.adr.md) - Fail Fast
- [022-idempotence.adr.md](../../docs/adr/022-idempotence.adr.md) - Tests idempotents

## 📁 Structure des Tests

```
src/
├── components/
│   └── UserCard.vue
│   └── __tests__/
│       └── UserCard.spec.ts      # Tests unitaires
├── composables/
│   └── useAuth.ts
│   └── __tests__/
│       └── useAuth.spec.ts
├── stores/
│   └── auth.ts
│   └── __tests__/
│       └── auth.spec.ts
tests/
├── e2e/                          # Tests E2E Playwright
│   └── auth.spec.ts
├── mocks/
│   └── handlers.ts               # MSW handlers
│   └── server.ts                 # MSW server setup
└── setup.ts                      # Configuration globale
```

## 🧪 Tests Unitaires avec Vitest

### Configuration Vitest

```typescript
// vitest.config.ts
import { fileURLToPath } from 'node:url'
import { defineConfig, mergeConfig } from 'vitest/config'
import viteConfig from './vite.config'

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'happy-dom',
      globals: true,
      setupFiles: ['./tests/setup.ts'],
      include: ['**/*.{test,spec}.{js,ts}'],
      coverage: {
        provider: 'v8',
        reporter: ['text', 'json', 'html'],
        exclude: ['node_modules', 'tests', '**/*.d.ts'],
      },
    },
  })
)
```

### Setup Global

```typescript
// tests/setup.ts
import { config } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { vi } from 'vitest'

// Mock global des composants PrimeVue si nécessaire
config.global.stubs = {
  // Stub des composants lourds
  DataTable: true,
  Dialog: true,
}

// Mock de window.matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
})
```

## 📦 Tests de Composants

### Test Basique

```typescript
// components/__tests__/UserCard.spec.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import UserCard from '../UserCard.vue'

describe('UserCard', () => {
  const defaultProps = {
    user: {
      id: '1',
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
    },
  }

  it('renders user name correctly', () => {
    const wrapper = mount(UserCard, {
      props: defaultProps,
    })

    expect(wrapper.text()).toContain('John Doe')
  })

  it('renders user email', () => {
    const wrapper = mount(UserCard, {
      props: defaultProps,
    })

    expect(wrapper.text()).toContain('john@example.com')
  })

  it('emits select event when clicked', async () => {
    const wrapper = mount(UserCard, {
      props: defaultProps,
    })

    await wrapper.trigger('click')

    expect(wrapper.emitted('select')).toBeTruthy()
    expect(wrapper.emitted('select')![0]).toEqual([defaultProps.user])
  })
})
```

### Test avec Pinia

```typescript
// components/__tests__/UserList.spec.ts
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import UserList from '../UserList.vue'
import { useUsersStore } from '@/stores'

describe('UserList', () => {
  const mockUsers = [
    { id: '1', firstName: 'John', lastName: 'Doe' },
    { id: '2', firstName: 'Jane', lastName: 'Smith' },
  ]

  function createWrapper(options = {}) {
    return mount(UserList, {
      global: {
        plugins: [
          createTestingPinia({
            createSpy: vi.fn,
            initialState: {
              users: { users: mockUsers, isLoading: false },
            },
          }),
        ],
      },
      ...options,
    })
  }

  it('renders list of users', () => {
    const wrapper = createWrapper()

    expect(wrapper.text()).toContain('John Doe')
    expect(wrapper.text()).toContain('Jane Smith')
  })

  it('shows loading state', () => {
    const wrapper = mount(UserList, {
      global: {
        plugins: [
          createTestingPinia({
            initialState: {
              users: { users: [], isLoading: true },
            },
          }),
        ],
      },
    })

    expect(wrapper.find('[data-testid="loading"]').exists()).toBe(true)
  })

  it('calls fetchUsers on mount', async () => {
    createWrapper()
    
    const store = useUsersStore()
    expect(store.fetchUsers).toHaveBeenCalled()
  })
})
```

### Test avec Vue Router

```typescript
// views/__tests__/UserDetail.spec.ts
import { describe, it, expect, vi } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import UserDetail from '../UserDetail.vue'

describe('UserDetail', () => {
  const routes = [
    { path: '/users/:id', component: UserDetail },
  ]

  function createRouterWithHistory(initialRoute: string) {
    const router = createRouter({
      history: createMemoryHistory(),
      routes,
    })
    router.push(initialRoute)
    return router
  }

  it('displays user based on route param', async () => {
    const router = createRouterWithHistory('/users/123')
    await router.isReady()

    const wrapper = mount(UserDetail, {
      global: {
        plugins: [router],
      },
    })

    await flushPromises()

    // Vérifier que le bon utilisateur est chargé
    expect(wrapper.vm.userId).toBe('123')
  })
})
```

### Test avec Slots

```typescript
// components/__tests__/BaseCard.spec.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import BaseCard from '../BaseCard.vue'

describe('BaseCard', () => {
  it('renders default slot content', () => {
    const wrapper = mount(BaseCard, {
      slots: {
        default: '<p>Card content</p>',
      },
    })

    expect(wrapper.html()).toContain('Card content')
  })

  it('renders header slot', () => {
    const wrapper = mount(BaseCard, {
      slots: {
        header: '<h2>Custom Header</h2>',
        default: 'Content',
      },
    })

    expect(wrapper.html()).toContain('Custom Header')
  })

  it('renders footer slot when provided', () => {
    const wrapper = mount(BaseCard, {
      slots: {
        default: 'Content',
        footer: '<button>Save</button>',
      },
    })

    expect(wrapper.html()).toContain('Save')
  })
})
```

## 🧩 Tests de Composables

```typescript
// composables/__tests__/useAuth.spec.ts
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuth } from '../useAuth'

// Mock de l'API
vi.mock('@/api', () => ({
  authApi: {
    login: vi.fn(),
    logout: vi.fn(),
    getCurrentUser: vi.fn(),
  },
}))

import { authApi } from '@/api'

describe('useAuth', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    localStorage.clear()
  })

  it('should login user successfully', async () => {
    const mockUser = { id: '1', email: 'test@example.com' }
    vi.mocked(authApi.login).mockResolvedValue({
      token: 'test-token',
      user: mockUser,
    })

    const { login, isAuthenticated, user } = useAuth()

    await login({ email: 'test@example.com', password: 'password' })

    expect(isAuthenticated.value).toBe(true)
    expect(user.value).toEqual(mockUser)
  })

  it('should handle login error', async () => {
    vi.mocked(authApi.login).mockRejectedValue(new Error('Invalid credentials'))

    const { login, error, isAuthenticated } = useAuth()

    await expect(
      login({ email: 'test@example.com', password: 'wrong' })
    ).rejects.toThrow('Invalid credentials')

    expect(isAuthenticated.value).toBe(false)
    expect(error.value).toBe('Invalid credentials')
  })

  it('should logout and clear state', async () => {
    const { login, logout, isAuthenticated } = useAuth()
    
    vi.mocked(authApi.login).mockResolvedValue({
      token: 'test-token',
      user: { id: '1' },
    })

    await login({ email: 'test@example.com', password: 'password' })
    expect(isAuthenticated.value).toBe(true)

    await logout()
    expect(isAuthenticated.value).toBe(false)
  })
})
```

## 🌐 Mocking avec MSW

### Configuration MSW

```typescript
// tests/mocks/handlers.ts
import { http, HttpResponse } from 'msw'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:3000'

export const handlers = [
  // GET /api/users
  http.get(`${API_URL}/api/users`, () => {
    return HttpResponse.json([
      { id: '1', firstName: 'John', lastName: 'Doe' },
      { id: '2', firstName: 'Jane', lastName: 'Smith' },
    ])
  }),

  // GET /api/users/:id
  http.get(`${API_URL}/api/users/:id`, ({ params }) => {
    const { id } = params
    return HttpResponse.json({
      id,
      firstName: 'John',
      lastName: 'Doe',
    })
  }),

  // POST /api/auth/login
  http.post(`${API_URL}/api/auth/login`, async ({ request }) => {
    const body = await request.json() as { email: string; password: string }
    
    if (body.email === 'test@example.com' && body.password === 'password') {
      return HttpResponse.json({
        token: 'mock-jwt-token',
        user: { id: '1', email: body.email },
      })
    }
    
    return HttpResponse.json(
      { message: 'Invalid credentials' },
      { status: 401 }
    )
  }),

  // Erreur 404 par défaut
  http.get(`${API_URL}/api/*`, () => {
    return HttpResponse.json(
      { message: 'Not found' },
      { status: 404 }
    )
  }),
]
```

```typescript
// tests/mocks/server.ts
import { setupServer } from 'msw/node'
import { handlers } from './handlers'

export const server = setupServer(...handlers)
```

```typescript
// tests/setup.ts
import { beforeAll, afterAll, afterEach } from 'vitest'
import { server } from './mocks/server'

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))
afterAll(() => server.close())
afterEach(() => server.resetHandlers())
```

## 🎭 Tests E2E avec Playwright

### Configuration Playwright

```typescript
// playwright.config.ts
import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
  },
})
```

### Test E2E

```typescript
// tests/e2e/auth.spec.ts
import { test, expect } from '@playwright/test'

test.describe('Authentication', () => {
  test('should login successfully', async ({ page }) => {
    await page.goto('/login')

    // Remplir le formulaire
    await page.getByLabel('Email').fill('test@example.com')
    await page.getByLabel('Mot de passe').fill('password')
    
    // Soumettre
    await page.getByRole('button', { name: 'Se connecter' }).click()

    // Vérifier la redirection
    await expect(page).toHaveURL('/dashboard')
    await expect(page.getByText('Bienvenue')).toBeVisible()
  })

  test('should show error on invalid credentials', async ({ page }) => {
    await page.goto('/login')

    await page.getByLabel('Email').fill('wrong@example.com')
    await page.getByLabel('Mot de passe').fill('wrongpassword')
    await page.getByRole('button', { name: 'Se connecter' }).click()

    await expect(page.getByText('Identifiants invalides')).toBeVisible()
    await expect(page).toHaveURL('/login')
  })

  test('should logout successfully', async ({ page }) => {
    // Login d'abord
    await page.goto('/login')
    await page.getByLabel('Email').fill('test@example.com')
    await page.getByLabel('Mot de passe').fill('password')
    await page.getByRole('button', { name: 'Se connecter' }).click()
    await expect(page).toHaveURL('/dashboard')

    // Logout
    await page.getByRole('button', { name: 'Déconnexion' }).click()
    
    await expect(page).toHaveURL('/login')
  })
})
```

### Page Object Model

```typescript
// tests/e2e/pages/LoginPage.ts
import { type Page, type Locator } from '@playwright/test'

export class LoginPage {
  readonly page: Page
  readonly emailInput: Locator
  readonly passwordInput: Locator
  readonly submitButton: Locator
  readonly errorMessage: Locator

  constructor(page: Page) {
    this.page = page
    this.emailInput = page.getByLabel('Email')
    this.passwordInput = page.getByLabel('Mot de passe')
    this.submitButton = page.getByRole('button', { name: 'Se connecter' })
    this.errorMessage = page.getByRole('alert')
  }

  async goto() {
    await this.page.goto('/login')
  }

  async login(email: string, password: string) {
    await this.emailInput.fill(email)
    await this.passwordInput.fill(password)
    await this.submitButton.click()
  }
}
```

```typescript
// tests/e2e/auth-pom.spec.ts
import { test, expect } from '@playwright/test'
import { LoginPage } from './pages/LoginPage'

test('should login with POM', async ({ page }) => {
  const loginPage = new LoginPage(page)
  
  await loginPage.goto()
  await loginPage.login('test@example.com', 'password')
  
  await expect(page).toHaveURL('/dashboard')
})
```

## ✅ Checklist Testing

- [ ] Tests unitaires pour chaque composant
- [ ] Tests des props, emits, slots
- [ ] Tests des composables isolés
- [ ] Tests des stores Pinia
- [ ] Mocks API avec MSW
- [ ] Tests E2E pour flux critiques
- [ ] Coverage > 80%
- [ ] Tests idempotents (pas d'ordre)
