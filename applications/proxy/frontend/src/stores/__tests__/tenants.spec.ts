import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useTenantsStore } from '@/stores/tenants'
import * as tenantsApi from '@/api/tenants'
import { createMockTenant, mockTenants } from '@/test-utils/factories'

// Mock du module API
vi.mock('@/api/tenants', () => ({
  fetchTenants: vi.fn(),
  fetchTenant: vi.fn(),
  createTenant: vi.fn(),
  updateTenant: vi.fn(),
  deleteTenant: vi.fn(),
  fetchTenantApiKeys: vi.fn(),
  createTenantApiKey: vi.fn(),
  revokeApiKey: vi.fn(),
  getMockTenants: vi.fn(),
}))

describe('useTenantsStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('état initial', () => {
    it('doit avoir un état initial correct', () => {
      const store = useTenantsStore()

      expect(store.tenants).toEqual([])
      expect(store.selectedTenant).toBeNull()
      expect(store.apiKeys).toEqual([])
      expect(store.isLoading).toBe(false)
      expect(store.isSaving).toBe(false)
      expect(store.error).toBeNull()
    })
  })

  describe('loadTenants', () => {
    it('doit charger les tenants depuis l\'API', async () => {
      vi.mocked(tenantsApi.fetchTenants).mockResolvedValue(mockTenants)

      const store = useTenantsStore()
      await store.loadTenants(false)

      expect(store.tenants).toEqual(mockTenants)
      expect(store.isLoading).toBe(false)
    })

    it('doit utiliser les données mock quand demandé', async () => {
      vi.mocked(tenantsApi.getMockTenants).mockReturnValue(mockTenants)

      const store = useTenantsStore()
      await store.loadTenants(true)

      expect(tenantsApi.getMockTenants).toHaveBeenCalled()
      expect(store.tenants).toEqual(mockTenants)
    })

    it('doit gérer les erreurs correctement', async () => {
      vi.mocked(tenantsApi.fetchTenants).mockRejectedValue(
        new Error('Network error'),
      )

      const store = useTenantsStore()
      await store.loadTenants(false)

      expect(store.error).toBe('Network error')
      expect(store.tenants).toEqual([])
    })
  })

  describe('addTenant', () => {
    it('doit ajouter un nouveau tenant', async () => {
      const newTenant = createMockTenant({ id: '99', name: 'New Tenant' })
      vi.mocked(tenantsApi.createTenant).mockResolvedValue(newTenant)

      const store = useTenantsStore()
      const result = await store.addTenant({
        name: 'New Tenant',
        slug: 'new-tenant',
        maxRequestsPerMonth: 100000,
        maxTokensPerMonth: 10000000,
      })

      expect(result).toEqual(newTenant)
      expect(store.tenants).toContainEqual(newTenant)
    })

    it('doit retourner null en cas d\'erreur', async () => {
      vi.mocked(tenantsApi.createTenant).mockRejectedValue(
        new Error('Creation failed'),
      )

      const store = useTenantsStore()
      const result = await store.addTenant({
        name: 'New Tenant',
        slug: 'new-tenant',
        maxRequestsPerMonth: 100000,
        maxTokensPerMonth: 10000000,
      })

      expect(result).toBeNull()
      expect(store.error).toBe('Creation failed')
    })
  })

  describe('editTenant', () => {
    it('doit mettre à jour un tenant existant', async () => {
      const tenant = createMockTenant({ id: '1', name: 'Original' })
      const updatedTenant = { ...tenant, name: 'Updated' }

      vi.mocked(tenantsApi.getMockTenants).mockReturnValue([tenant])
      vi.mocked(tenantsApi.updateTenant).mockResolvedValue(updatedTenant)

      const store = useTenantsStore()
      await store.loadTenants(true)

      const result = await store.editTenant('1', { name: 'Updated' })

      expect(result).toEqual(updatedTenant)
      expect(store.tenants[0].name).toBe('Updated')
    })
  })

  describe('removeTenant', () => {
    it('doit supprimer un tenant', async () => {
      vi.mocked(tenantsApi.getMockTenants).mockReturnValue(mockTenants)
      vi.mocked(tenantsApi.deleteTenant).mockResolvedValue()

      const store = useTenantsStore()
      await store.loadTenants(true)

      const initialCount = store.tenants.length
      const success = await store.removeTenant('1')

      expect(success).toBe(true)
      expect(store.tenants.length).toBe(initialCount - 1)
    })

    it('doit désélectionner le tenant supprimé', async () => {
      const tenant = createMockTenant({ id: '1' })
      vi.mocked(tenantsApi.getMockTenants).mockReturnValue([tenant])
      vi.mocked(tenantsApi.deleteTenant).mockResolvedValue()

      const store = useTenantsStore()
      await store.loadTenants(true)
      store.selectTenant(tenant)

      await store.removeTenant('1')

      expect(store.selectedTenant).toBeNull()
    })
  })

  describe('API Keys', () => {
    it('doit charger les clés API d\'un tenant', async () => {
      const mockApiKeys = [
        {
          id: '1',
          name: 'Production Key',
          prefix: 'sk-***abc',
          createdAt: '2024-01-01T00:00:00Z',
        },
      ]
      vi.mocked(tenantsApi.fetchTenantApiKeys).mockResolvedValue(mockApiKeys)

      const store = useTenantsStore()
      await store.loadApiKeys('tenant-1')

      expect(store.apiKeys).toEqual(mockApiKeys)
    })

    it('doit créer une nouvelle clé API', async () => {
      const createResult = { id: '1', key: 'sk-full-key-here' }
      vi.mocked(tenantsApi.createTenantApiKey).mockResolvedValue(createResult)
      vi.mocked(tenantsApi.fetchTenantApiKeys).mockResolvedValue([])

      const store = useTenantsStore()
      store.selectTenant(createMockTenant({ id: 'tenant-1' }))

      const result = await store.addApiKey('tenant-1', 'New Key')

      expect(result).toEqual(createResult)
      expect(tenantsApi.createTenantApiKey).toHaveBeenCalledWith('tenant-1', 'New Key')
    })

    it('doit révoquer une clé API', async () => {
      vi.mocked(tenantsApi.revokeApiKey).mockResolvedValue()

      const store = useTenantsStore()
      store.selectTenant(createMockTenant({ id: 'tenant-1' }))
      store.apiKeys = [
        {
          id: 'key-1',
          name: 'Key',
          prefix: 'sk-***',
          createdAt: '2024-01-01T00:00:00Z',
        },
      ]

      const success = await store.removeApiKey('tenant-1', 'key-1')

      expect(success).toBe(true)
      expect(store.apiKeys.length).toBe(0)
    })
  })

  describe('getters calculés', () => {
    beforeEach(async () => {
      vi.mocked(tenantsApi.getMockTenants).mockReturnValue([
        createMockTenant({
          id: '1',
          isActive: true,
          requestsThisMonth: 50000,
          quota: {
            maxRequestsPerMonth: 100000,
            maxTokensPerMonth: 10000000,
            currentRequests: 50000,
            currentTokens: 5000000,
          },
        }),
        createMockTenant({
          id: '2',
          isActive: true,
          requestsThisMonth: 30000,
          quota: {
            maxRequestsPerMonth: 100000,
            maxTokensPerMonth: 10000000,
            currentRequests: 95000, // Près de la limite
            currentTokens: 9500000,
          },
        }),
        createMockTenant({
          id: '3',
          isActive: false,
          requestsThisMonth: 0,
          quota: {
            maxRequestsPerMonth: 100000,
            maxTokensPerMonth: 10000000,
            currentRequests: 0,
            currentTokens: 0,
          },
        }),
      ])
    })

    it('doit calculer totalTenants correctement', async () => {
      const store = useTenantsStore()
      await store.loadTenants(true)

      expect(store.totalTenants).toBe(3)
    })

    it('doit calculer activeTenants correctement', async () => {
      const store = useTenantsStore()
      await store.loadTenants(true)

      expect(store.activeTenants.length).toBe(2)
    })

    it('doit calculer totalRequestsThisMonth correctement', async () => {
      const store = useTenantsStore()
      await store.loadTenants(true)

      expect(store.totalRequestsThisMonth).toBe(80000) // 50000 + 30000 + 0
    })

    it('doit identifier les tenants proches de la limite de quota', async () => {
      const store = useTenantsStore()
      await store.loadTenants(true)

      expect(store.tenantsNearQuotaLimit.length).toBe(1)
      expect(store.tenantsNearQuotaLimit[0].id).toBe('2')
    })
  })
})
