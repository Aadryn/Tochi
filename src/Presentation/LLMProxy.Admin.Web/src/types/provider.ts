export interface Provider {
  id: string
  name: string
  type: string
  baseUrl: string
  model: string
  isEnabled: boolean
  status: 'healthy' | 'degraded' | 'unhealthy'
  requestsToday: number
  createdAt: string
}

export interface CreateProviderRequest {
  name: string
  type: string
  baseUrl: string
  apiKey: string
  model: string
  isEnabled: boolean
}
