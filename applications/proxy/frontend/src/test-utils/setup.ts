import { config } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import PrimeVue from 'primevue/config'
import { beforeEach, beforeAll, afterAll, vi } from 'vitest'

// Configuration globale pour les tests
config.global.plugins = [PrimeVue]

// Réinitialiser Pinia avant chaque test
beforeEach(() => {
  setActivePinia(createPinia())
})

// Mock localStorage (only in browser-like environment)
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
}

// Setup mocks if running in a DOM environment
if (typeof window !== 'undefined') {
  Object.defineProperty(window, 'localStorage', { value: localStorageMock })

  Object.defineProperty(window, 'location', {
    value: {
      href: '',
      pathname: '/',
      search: '',
      hash: '',
      origin: 'http://localhost:3000',
    },
    writable: true,
  })
}

// Mock console.error et console.warn pour les tests
const originalError = console.error
const originalWarn = console.warn

beforeAll(() => {
  console.error = vi.fn()
  console.warn = vi.fn()
})

afterAll(() => {
  console.error = originalError
  console.warn = originalWarn
})
