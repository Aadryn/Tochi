import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../auth'

// Mock localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {}
  return {
    getItem: (key: string) => store[key] || null,
    setItem: (key: string, value: string) => { store[key] = value },
    removeItem: (key: string) => { delete store[key] },
    clear: () => { store = {} },
  }
})()

Object.defineProperty(window, 'localStorage', { value: localStorageMock })

// Mock de l'API auth
vi.mock('@/api/auth', () => ({
  login: vi.fn(),
  logout: vi.fn(),
  refreshToken: vi.fn(),
  getCurrentUser: vi.fn(),
}))

describe('useAuthStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorageMock.clear()
    vi.clearAllMocks()
  })

  describe('état initial', () => {
    it('doit avoir un état initial non authentifié', () => {
      const store = useAuthStore()
      
      expect(store.user).toBeNull()
      expect(store.token).toBeNull()
      expect(store.isAuthenticated).toBe(false)
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('doit charger le token depuis localStorage si présent', () => {
      localStorageMock.setItem('auth_token', 'test-token')
      localStorageMock.setItem('auth_user', JSON.stringify({ id: '1', name: 'Test User', email: 'test@example.com', role: 'admin' }))
      
      const store = useAuthStore()
      
      expect(store.token).toBe('test-token')
      expect(store.user).not.toBeNull()
      expect(store.user?.name).toBe('Test User')
    })
  })

  describe('getters', () => {
    it('doit retourner isAuthenticated true si token et user sont présents', () => {
      localStorageMock.setItem('auth_token', 'test-token')
      localStorageMock.setItem('auth_user', JSON.stringify({ id: '1', name: 'Test', email: 'test@test.com', role: 'admin' }))
      
      const store = useAuthStore()
      
      expect(store.isAuthenticated).toBe(true)
    })

    it('doit calculer isAdmin correctement', () => {
      localStorageMock.setItem('auth_token', 'test-token')
      localStorageMock.setItem('auth_user', JSON.stringify({ id: '1', name: 'Admin', email: 'admin@test.com', role: 'admin' }))
      
      const store = useAuthStore()
      
      expect(store.isAdmin).toBe(true)
    })

    it('doit calculer isTenantAdmin pour admin et tenant-admin', () => {
      localStorageMock.setItem('auth_token', 'test-token')
      localStorageMock.setItem('auth_user', JSON.stringify({ id: '1', name: 'Tenant Admin', email: 'ta@test.com', role: 'tenant-admin' }))
      
      const store = useAuthStore()
      
      expect(store.isTenantAdmin).toBe(true)
    })

    it('doit calculer les initiales du nom', () => {
      localStorageMock.setItem('auth_token', 'test-token')
      localStorageMock.setItem('auth_user', JSON.stringify({ id: '1', name: 'Jean Dupont', email: 'jd@test.com', role: 'user' }))
      
      const store = useAuthStore()
      
      expect(store.userInitials).toBe('JD')
    })

    it('doit retourner les deux premières lettres pour un prénom seul', () => {
      localStorageMock.setItem('auth_token', 'test-token')
      localStorageMock.setItem('auth_user', JSON.stringify({ id: '1', name: 'Jean', email: 'j@test.com', role: 'user' }))
      
      const store = useAuthStore()
      
      expect(store.userInitials).toBe('JE')
    })
  })

  describe('logout', () => {
    it('doit nettoyer l\'état et le localStorage', async () => {
      localStorageMock.setItem('auth_token', 'test-token')
      localStorageMock.setItem('auth_refresh_token', 'refresh-token')
      localStorageMock.setItem('auth_user', JSON.stringify({ id: '1', name: 'Test', email: 't@t.com', role: 'user' }))
      
      const store = useAuthStore()
      expect(store.isAuthenticated).toBe(true)
      
      await store.logout()
      
      expect(store.token).toBeNull()
      expect(store.user).toBeNull()
      expect(store.isAuthenticated).toBe(false)
      expect(localStorageMock.getItem('auth_token')).toBeNull()
      expect(localStorageMock.getItem('auth_user')).toBeNull()
    })
  })

  describe('clearError', () => {
    it('doit effacer le message d\'erreur', () => {
      const store = useAuthStore()
      store.error = 'Une erreur'
      
      store.clearError()
      
      expect(store.error).toBeNull()
    })
  })

  describe('isTokenExpired', () => {
    it('doit retourner true si pas de token', () => {
      const store = useAuthStore()
      
      expect(store.isTokenExpired()).toBe(true)
    })

    it('doit retourner true pour un token mal formé', () => {
      localStorageMock.setItem('auth_token', 'invalid-token')
      
      const store = useAuthStore()
      
      expect(store.isTokenExpired()).toBe(true)
    })
  })
})
