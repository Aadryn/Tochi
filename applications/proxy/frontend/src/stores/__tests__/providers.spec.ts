import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useProvidersStore } from '@/stores/providers'
import * as providersApi from '@/api/providers'
import { createMockProvider, mockProviders } from '@/test-utils/factories'

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

describe('useProvidersStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('état initial', () => {
    it('doit avoir un état initial correct', () => {
      const store = useProvidersStore()

      expect(store.providers).toEqual([])
      expect(store.selectedProvider).toBeNull()
      expect(store.isLoading).toBe(false)
      expect(store.isSaving).toBe(false)
      expect(store.error).toBeNull()
    })
  })

  describe('loadProviders', () => {
    it('doit charger les providers depuis l\'API', async () => {
      vi.mocked(providersApi.fetchProviders).mockResolvedValue(mockProviders)

      const store = useProvidersStore()
      await store.loadProviders(false)

      expect(store.providers).toEqual(mockProviders)
      expect(store.isLoading).toBe(false)
    })

    it('doit utiliser les données mock quand demandé', async () => {
      vi.mocked(providersApi.getMockProviders).mockReturnValue(mockProviders)

      const store = useProvidersStore()
      await store.loadProviders(true)

      expect(providersApi.getMockProviders).toHaveBeenCalled()
      expect(store.providers).toEqual(mockProviders)
    })

    it('doit gérer les erreurs correctement', async () => {
      vi.mocked(providersApi.fetchProviders).mockRejectedValue(
        new Error('Network error'),
      )

      const store = useProvidersStore()
      await store.loadProviders(false)

      expect(store.error).toBe('Network error')
      expect(store.providers).toEqual([])
    })
  })

  describe('addProvider', () => {
    it('doit ajouter un nouveau provider', async () => {
      const newProvider = createMockProvider({ id: '99', name: 'New Provider' })
      vi.mocked(providersApi.createProvider).mockResolvedValue(newProvider)

      const store = useProvidersStore()
      const result = await store.addProvider({
        name: 'New Provider',
        type: 'openai',
        baseUrl: 'https://api.test.com',
        apiKey: 'key',
        model: 'gpt-4',
        isEnabled: true,
      })

      expect(result).toEqual(newProvider)
      expect(store.providers).toContainEqual(newProvider)
    })

    it('doit retourner null en cas d\'erreur', async () => {
      vi.mocked(providersApi.createProvider).mockRejectedValue(
        new Error('Creation failed'),
      )

      const store = useProvidersStore()
      const result = await store.addProvider({
        name: 'New Provider',
        type: 'openai',
        baseUrl: 'https://api.test.com',
        apiKey: 'key',
        model: 'gpt-4',
        isEnabled: true,
      })

      expect(result).toBeNull()
      expect(store.error).toBe('Creation failed')
    })
  })

  describe('editProvider', () => {
    it('doit mettre à jour un provider existant', async () => {
      const provider = createMockProvider({ id: '1', name: 'Original' })
      const updatedProvider = { ...provider, name: 'Updated' }

      vi.mocked(providersApi.getMockProviders).mockReturnValue([provider])
      vi.mocked(providersApi.updateProvider).mockResolvedValue(updatedProvider)

      const store = useProvidersStore()
      await store.loadProviders(true)

      const result = await store.editProvider('1', { name: 'Updated' })

      expect(result).toEqual(updatedProvider)
      expect(store.providers[0].name).toBe('Updated')
    })

    it('doit mettre à jour le provider sélectionné', async () => {
      const provider = createMockProvider({ id: '1', name: 'Original' })
      const updatedProvider = { ...provider, name: 'Updated' }

      vi.mocked(providersApi.getMockProviders).mockReturnValue([provider])
      vi.mocked(providersApi.updateProvider).mockResolvedValue(updatedProvider)

      const store = useProvidersStore()
      await store.loadProviders(true)
      store.selectProvider(provider)

      await store.editProvider('1', { name: 'Updated' })

      expect(store.selectedProvider?.name).toBe('Updated')
    })
  })

  describe('removeProvider', () => {
    it('doit supprimer un provider', async () => {
      vi.mocked(providersApi.getMockProviders).mockReturnValue(mockProviders)
      vi.mocked(providersApi.deleteProvider).mockResolvedValue()

      const store = useProvidersStore()
      await store.loadProviders(true)

      const initialCount = store.providers.length
      const success = await store.removeProvider('1')

      expect(success).toBe(true)
      expect(store.providers.length).toBe(initialCount - 1)
      expect(store.providers.find((p) => p.id === '1')).toBeUndefined()
    })

    it('doit désélectionner le provider supprimé', async () => {
      const provider = createMockProvider({ id: '1' })
      vi.mocked(providersApi.getMockProviders).mockReturnValue([provider])
      vi.mocked(providersApi.deleteProvider).mockResolvedValue()

      const store = useProvidersStore()
      await store.loadProviders(true)
      store.selectProvider(provider)

      await store.removeProvider('1')

      expect(store.selectedProvider).toBeNull()
    })
  })

  describe('checkHealth', () => {
    it('doit mettre à jour le statut du provider', async () => {
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

  describe('getters calculés', () => {
    beforeEach(async () => {
      vi.mocked(providersApi.getMockProviders).mockReturnValue([
        createMockProvider({ id: '1', status: 'healthy', isEnabled: true }),
        createMockProvider({ id: '2', status: 'healthy', isEnabled: false }),
        createMockProvider({ id: '3', status: 'degraded', isEnabled: true }),
      ])
    })

    it('doit calculer totalProviders correctement', async () => {
      const store = useProvidersStore()
      await store.loadProviders(true)

      expect(store.totalProviders).toBe(3)
    })

    it('doit calculer enabledProviders correctement', async () => {
      const store = useProvidersStore()
      await store.loadProviders(true)

      expect(store.enabledProviders.length).toBe(2)
    })

    it('doit calculer healthyProviders correctement', async () => {
      const store = useProvidersStore()
      await store.loadProviders(true)

      expect(store.healthyProviders.length).toBe(2)
    })

    it('doit grouper par type correctement', async () => {
      vi.mocked(providersApi.getMockProviders).mockReturnValue([
        createMockProvider({ id: '1', type: 'openai' }),
        createMockProvider({ id: '2', type: 'openai' }),
        createMockProvider({ id: '3', type: 'anthropic' }),
      ])

      const store = useProvidersStore()
      await store.loadProviders(true)

      expect(store.providersByType['openai'].length).toBe(2)
      expect(store.providersByType['anthropic'].length).toBe(1)
    })
  })
})
