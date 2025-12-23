import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { flushPromises } from '@vue/test-utils'
import { renderWithPlugins } from '@/test-utils'
import DashboardView from '@/views/DashboardView.vue'
import { useDashboardStore } from '@/stores/dashboard'
import { createMockDashboardMetrics } from '@/test-utils/factories'
import * as dashboardApi from '@/api/dashboard'

// Mock du module API
vi.mock('@/api/dashboard', () => ({
  fetchDashboardMetrics: vi.fn(),
  getMockDashboardMetrics: vi.fn(),
}))

describe('DashboardView - Tests d\'intégration', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('chargement initial', () => {
    it('doit avoir un store fonctionnel', async () => {
      const mockMetrics = createMockDashboardMetrics({
        totalRequests: 15000,
        successRate: 98.5,
      })
      vi.mocked(dashboardApi.getMockDashboardMetrics).mockReturnValue(mockMetrics)

      const store = useDashboardStore()

      // Charger les données
      await store.loadMetrics(true)

      // Vérifier que le store a été mis à jour
      expect(store.metrics).not.toBeNull()
      expect(store.metrics?.totalRequests).toBe(15000)
    })

    it('doit rendre le composant sans erreur', async () => {
      const mockMetrics = createMockDashboardMetrics()
      vi.mocked(dashboardApi.getMockDashboardMetrics).mockReturnValue(mockMetrics)

      const { container } = renderWithPlugins(DashboardView)

      // Vérifier que le composant est rendu
      expect(container.querySelector('.dashboard-view')).toBeTruthy()
    })
  })

  describe('interaction avec le store', () => {
    it('doit calculer correctement les getters du store', async () => {
      const mockMetrics = createMockDashboardMetrics({
        providers: [
          { name: 'OpenAI', status: 'healthy', requestsToday: 100 },
          { name: 'Anthropic', status: 'degraded', requestsToday: 50 },
        ],
      })
      vi.mocked(dashboardApi.getMockDashboardMetrics).mockReturnValue(mockMetrics)

      const store = useDashboardStore()
      await store.loadMetrics(true)

      expect(store.healthyProviders).toBe(1)
      expect(store.degradedProviders).toBe(1)
    })
  })
})
