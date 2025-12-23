import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useDashboardStore } from '@/stores/dashboard'
import * as dashboardApi from '@/api/dashboard'
import { createMockDashboardMetrics } from '@/test-utils/factories'

// Mock du module API
vi.mock('@/api/dashboard', () => ({
  fetchDashboardMetrics: vi.fn(),
  getMockDashboardMetrics: vi.fn(),
}))

describe('useDashboardStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('état initial', () => {
    it('doit avoir un état initial correct', () => {
      const store = useDashboardStore()

      expect(store.metrics).toBeNull()
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.lastUpdated).toBeNull()
    })

    it('doit retourner false pour hasData sans métriques', () => {
      const store = useDashboardStore()

      expect(store.hasData).toBe(false)
    })
  })

  describe('loadMetrics', () => {
    it('doit charger les métriques depuis l\'API', async () => {
      const mockMetrics = createMockDashboardMetrics()
      vi.mocked(dashboardApi.fetchDashboardMetrics).mockResolvedValue(mockMetrics)

      const store = useDashboardStore()
      await store.loadMetrics(false)

      expect(store.metrics).toEqual(mockMetrics)
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.lastUpdated).toBeInstanceOf(Date)
    })

    it('doit utiliser les données mock quand demandé', async () => {
      const mockMetrics = createMockDashboardMetrics()
      vi.mocked(dashboardApi.getMockDashboardMetrics).mockReturnValue(mockMetrics)

      const store = useDashboardStore()
      await store.loadMetrics(true)

      expect(dashboardApi.getMockDashboardMetrics).toHaveBeenCalled()
      expect(store.metrics).toEqual(mockMetrics)
    })

    it('doit gérer les erreurs correctement', async () => {
      const errorMessage = 'Network error'
      vi.mocked(dashboardApi.fetchDashboardMetrics).mockRejectedValue(
        new Error(errorMessage),
      )

      const store = useDashboardStore()
      await store.loadMetrics(false)

      expect(store.error).toBe(errorMessage)
      expect(store.metrics).toBeNull()
      expect(store.isLoading).toBe(false)
    })

    it('doit définir isLoading pendant le chargement', async () => {
      vi.mocked(dashboardApi.fetchDashboardMetrics).mockImplementation(
        () => new Promise((resolve) => setTimeout(resolve, 100)),
      )

      const store = useDashboardStore()
      const loadPromise = store.loadMetrics(false)

      expect(store.isLoading).toBe(true)

      await loadPromise
      expect(store.isLoading).toBe(false)
    })
  })

  describe('getters calculés', () => {
    it('doit calculer healthyProviders correctement', async () => {
      const mockMetrics = createMockDashboardMetrics({
        providers: [
          { name: 'P1', status: 'healthy', requestsToday: 100 },
          { name: 'P2', status: 'healthy', requestsToday: 100 },
          { name: 'P3', status: 'degraded', requestsToday: 100 },
          { name: 'P4', status: 'unhealthy', requestsToday: 100 },
        ],
      })
      vi.mocked(dashboardApi.getMockDashboardMetrics).mockReturnValue(mockMetrics)

      const store = useDashboardStore()
      await store.loadMetrics(true)

      expect(store.healthyProviders).toBe(2)
    })

    it('doit calculer degradedProviders correctement', async () => {
      const mockMetrics = createMockDashboardMetrics({
        providers: [
          { name: 'P1', status: 'healthy', requestsToday: 100 },
          { name: 'P2', status: 'degraded', requestsToday: 100 },
          { name: 'P3', status: 'degraded', requestsToday: 100 },
        ],
      })
      vi.mocked(dashboardApi.getMockDashboardMetrics).mockReturnValue(mockMetrics)

      const store = useDashboardStore()
      await store.loadMetrics(true)

      expect(store.degradedProviders).toBe(2)
    })

    it('doit retourner 0 pour les compteurs sans métriques', () => {
      const store = useDashboardStore()

      expect(store.healthyProviders).toBe(0)
      expect(store.degradedProviders).toBe(0)
      expect(store.unhealthyProviders).toBe(0)
    })
  })

  describe('actions', () => {
    it('doit effacer les erreurs avec clearError', async () => {
      vi.mocked(dashboardApi.fetchDashboardMetrics).mockRejectedValue(
        new Error('Error'),
      )

      const store = useDashboardStore()
      await store.loadMetrics(false)

      expect(store.error).not.toBeNull()

      store.clearError()
      expect(store.error).toBeNull()
    })

    it('doit réinitialiser l\'état avec reset', async () => {
      const mockMetrics = createMockDashboardMetrics()
      vi.mocked(dashboardApi.getMockDashboardMetrics).mockReturnValue(mockMetrics)

      const store = useDashboardStore()
      await store.loadMetrics(true)

      expect(store.metrics).not.toBeNull()

      store.reset()

      expect(store.metrics).toBeNull()
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
      expect(store.lastUpdated).toBeNull()
    })
  })
})
