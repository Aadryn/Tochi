import { apiClient } from './client'
import type { DashboardMetrics } from '@/types'

/**
 * Récupère les métriques du dashboard.
 */
export async function fetchDashboardMetrics(): Promise<DashboardMetrics> {
  const response = await apiClient.get<DashboardMetrics>('/dashboard/metrics')
  return response.data
}

/**
 * Retourne des données mock pour le développement.
 */
export function getMockDashboardMetrics(): DashboardMetrics {
  return {
    totalRequests: 125847,
    activeProviders: 4,
    activeTenants: 12,
    avgLatencyMs: 245,
    successRate: 99.2,
    providers: [
      { name: 'OpenAI', status: 'healthy', requestsToday: 45230 },
      { name: 'Anthropic', status: 'healthy', requestsToday: 32150 },
      { name: 'Ollama Local', status: 'degraded', requestsToday: 8420 },
      { name: 'Azure OpenAI', status: 'healthy', requestsToday: 28970 },
    ],
  }
}
