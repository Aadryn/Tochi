import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { flushPromises } from '@vue/test-utils'
import { renderWithPlugins } from '@/test-utils'
import ProvidersView from '@/views/ProvidersView.vue'
import { useProvidersStore } from '@/stores/providers'
import { createMockProvider, mockProviders } from '@/test-utils/factories'
import * as providersApi from '@/api/providers'

// Mock du module API
vi.mock('@/api/providers', () => ({
  fetchProviders: vi.fn(),
  fetchProvider: vi.fn(),
  createProvider: vi.fn(),
  updateProvider: vi.fn(),
  deleteProvider: vi.fn(),
  checkProviderHealth: vi.fn(),
  getMockProviders: vi.fn(),
}))

describe('ProvidersView - Tests d\'intégration', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('liste des providers', () => {
    it('doit charger les providers dans le store', async () => {
      vi.mocked(providersApi.getMockProviders).mockReturnValue(mockProviders)

      const store = useProvidersStore()
      await store.loadProviders(true)

      expect(store.providers.length).toBe(mockProviders.length)
    })

    it('doit rendre le composant sans erreur', async () => {
      vi.mocked(providersApi.getMockProviders).mockReturnValue([])

      const { container } = renderWithPlugins(ProvidersView)

      expect(container.querySelector('.providers-view')).toBeTruthy()
    })
  })

  describe('filtrage et recherche', () => {
    it('doit calculer les getters de filtrage correctement', async () => {
      vi.mocked(providersApi.getMockProviders).mockReturnValue([
        createMockProvider({ id: '1', status: 'healthy' }),
        createMockProvider({ id: '2', status: 'degraded' }),
        createMockProvider({ id: '3', status: 'unhealthy' }),
      ])

      const store = useProvidersStore()
      await store.loadProviders(true)

      expect(store.healthyProviders.length).toBe(1)
    })
  })

  describe('actions CRUD', () => {
    it('doit pouvoir sélectionner un provider', async () => {
      const provider = createMockProvider({ id: '1', name: 'Test Provider' })
      vi.mocked(providersApi.getMockProviders).mockReturnValue([provider])

      const store = useProvidersStore()
      await store.loadProviders(true)
      store.selectProvider(provider)

      expect(store.selectedProvider?.id).toBe('1')
    })

    it('doit pouvoir supprimer un provider', async () => {
      const provider = createMockProvider({ id: '1' })
      vi.mocked(providersApi.getMockProviders).mockReturnValue([provider])
      vi.mocked(providersApi.deleteProvider).mockResolvedValue()

      const store = useProvidersStore()
      await store.loadProviders(true)
      await store.removeProvider('1')

      expect(store.providers.length).toBe(0)
    })
  })

  describe('vérification de santé', () => {
    it('doit pouvoir vérifier la santé d\'un provider', async () => {
      const provider = createMockProvider({ id: '1', status: 'healthy' })
      vi.mocked(providersApi.getMockProviders).mockReturnValue([provider])
      vi.mocked(providersApi.checkProviderHealth).mockResolvedValue({
        status: 'degraded',
        latencyMs: 500,
        lastChecked: new Date().toISOString(),
      })

      const store = useProvidersStore()
      await store.loadProviders(true)
      await store.checkHealth('1')

      expect(store.providers[0].status).toBe('degraded')
    })
  })
})
