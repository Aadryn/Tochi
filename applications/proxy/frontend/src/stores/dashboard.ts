import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { DashboardMetrics } from '@/types'
import { fetchDashboardMetrics, getMockDashboardMetrics } from '@/api/dashboard'

/**
 * Store Pinia pour les métriques du dashboard.
 */
export const useDashboardStore = defineStore('dashboard', () => {
  // État
  const metrics = ref<DashboardMetrics | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const lastUpdated = ref<Date | null>(null)

  // Getters calculés
  const hasData = computed(() => metrics.value !== null)

  const healthyProviders = computed(() => {
    if (!metrics.value) return 0
    return metrics.value.providers.filter((p) => p.status === 'healthy').length
  })

  const degradedProviders = computed(() => {
    if (!metrics.value) return 0
    return metrics.value.providers.filter((p) => p.status === 'degraded').length
  })

  const unhealthyProviders = computed(() => {
    if (!metrics.value) return 0
    return metrics.value.providers.filter((p) => p.status === 'unhealthy').length
  })

  // Actions
  async function loadMetrics(useMock = false): Promise<void> {
    isLoading.value = true
    error.value = null

    try {
      if (useMock) {
        // Simuler un délai réseau
        await new Promise((resolve) => setTimeout(resolve, 500))
        metrics.value = getMockDashboardMetrics()
      } else {
        metrics.value = await fetchDashboardMetrics()
      }
      lastUpdated.value = new Date()
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors du chargement des métriques'
      console.error('Erreur dashboard:', err)
    } finally {
      isLoading.value = false
    }
  }

  function clearError(): void {
    error.value = null
  }

  function reset(): void {
    metrics.value = null
    isLoading.value = false
    error.value = null
    lastUpdated.value = null
  }

  return {
    // État
    metrics,
    isLoading,
    error,
    lastUpdated,
    // Getters
    hasData,
    healthyProviders,
    degradedProviders,
    unhealthyProviders,
    // Actions
    loadMetrics,
    clearError,
    reset,
  }
})
