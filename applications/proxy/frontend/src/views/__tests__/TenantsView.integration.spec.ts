import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { flushPromises } from '@vue/test-utils'
import { renderWithPlugins } from '@/test-utils'
import TenantsView from '@/views/TenantsView.vue'
import { useTenantsStore } from '@/stores/tenants'
import { createMockTenant, mockTenants } from '@/test-utils/factories'
import * as tenantsApi from '@/api/tenants'

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

describe('TenantsView - Tests d\'intégration', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('liste des tenants', () => {
    it('doit charger les tenants dans le store', async () => {
      vi.mocked(tenantsApi.getMockTenants).mockReturnValue(mockTenants)

      const store = useTenantsStore()
      await store.loadTenants(true)

      expect(store.tenants.length).toBe(mockTenants.length)
    })

    it('doit rendre le composant sans erreur', async () => {
      vi.mocked(tenantsApi.getMockTenants).mockReturnValue([])

      const { container } = renderWithPlugins(TenantsView)

      expect(container.querySelector('.tenants-view')).toBeTruthy()
    })
  })

  describe('gestion des quotas', () => {
    it('doit identifier les tenants proches de leur limite', async () => {
      vi.mocked(tenantsApi.getMockTenants).mockReturnValue([
        createMockTenant({
          id: '1',
          quota: {
            maxRequestsPerMonth: 100000,
            maxTokensPerMonth: 10000000,
            currentRequests: 95000, // 95% - proche de la limite
            currentTokens: 5000000,
          },
        }),
        createMockTenant({
          id: '2',
          quota: {
            maxRequestsPerMonth: 100000,
            maxTokensPerMonth: 10000000,
            currentRequests: 50000, // 50% - loin de la limite
            currentTokens: 5000000,
          },
        }),
      ])

      const store = useTenantsStore()
      await store.loadTenants(true)

      expect(store.tenantsNearQuotaLimit.length).toBe(1)
      expect(store.tenantsNearQuotaLimit[0].id).toBe('1')
    })
  })

  describe('actions CRUD', () => {
    it('doit pouvoir créer un nouveau tenant', async () => {
      const newTenant = createMockTenant({ id: '99', name: 'New Tenant' })
      vi.mocked(tenantsApi.getMockTenants).mockReturnValue([])
      vi.mocked(tenantsApi.createTenant).mockResolvedValue(newTenant)

      const store = useTenantsStore()
      await store.addTenant({
        name: 'New Tenant',
        slug: 'new-tenant',
        maxRequestsPerMonth: 100000,
        maxTokensPerMonth: 10000000,
      })

      expect(store.tenants.length).toBe(1)
      expect(store.tenants[0].name).toBe('New Tenant')
    })

    it('doit pouvoir supprimer un tenant', async () => {
      vi.mocked(tenantsApi.getMockTenants).mockReturnValue(mockTenants)
      vi.mocked(tenantsApi.deleteTenant).mockResolvedValue()

      const store = useTenantsStore()
      await store.loadTenants(true)
      const initialCount = store.tenants.length

      await store.removeTenant('1')

      expect(store.tenants.length).toBe(initialCount - 1)
    })
  })

  describe('gestion des clés API', () => {
    it('doit pouvoir charger les clés API d\'un tenant', async () => {
      const mockKeys = [
        {
          id: 'key-1',
          name: 'Production Key',
          prefix: 'sk-***abc',
          createdAt: '2024-01-01T00:00:00Z',
        },
      ]
      vi.mocked(tenantsApi.getMockTenants).mockReturnValue(mockTenants)
      vi.mocked(tenantsApi.fetchTenantApiKeys).mockResolvedValue(mockKeys)

      const store = useTenantsStore()
      await store.loadApiKeys('1')

      expect(store.apiKeys.length).toBe(1)
      expect(store.apiKeys[0].name).toBe('Production Key')
    })

    it('doit pouvoir créer une nouvelle clé API', async () => {
      vi.mocked(tenantsApi.getMockTenants).mockReturnValue(mockTenants)
      vi.mocked(tenantsApi.createTenantApiKey).mockResolvedValue({
        id: 'new-key',
        key: 'sk-new-full-key',
      })
      vi.mocked(tenantsApi.fetchTenantApiKeys).mockResolvedValue([])

      const store = useTenantsStore()
      store.selectTenant(mockTenants[0])

      const result = await store.addApiKey('1', 'New API Key')

      expect(result?.key).toBe('sk-new-full-key')
    })
  })

  describe('statistiques globales', () => {
    it('doit calculer le total des requêtes ce mois', async () => {
      vi.mocked(tenantsApi.getMockTenants).mockReturnValue([
        createMockTenant({ id: '1', requestsThisMonth: 50000 }),
        createMockTenant({ id: '2', requestsThisMonth: 30000 }),
        createMockTenant({ id: '3', requestsThisMonth: 20000 }),
      ])

      const store = useTenantsStore()
      await store.loadTenants(true)

      expect(store.totalRequestsThisMonth).toBe(100000)
    })
  })
})
