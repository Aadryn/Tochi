import { apiClient } from './client'

export interface DashboardMetrics {
  totalRequests: number
  activeProviders: number
  activeTenants: number
  avgLatencyMs: number
  successRate: number
  providers: Array<{
    name: string
    status: 'healthy' | 'degraded' | 'unhealthy'
    requestsToday: number
  }>
}

/**
 * Récupère les métriques du dashboard.
 */
export async function fetchDashboardMetrics(): Promise<DashboardMetrics> {
  const response = await apiClient.get<DashboardMetrics>('/dashboard/metrics')
  return response.data
}
