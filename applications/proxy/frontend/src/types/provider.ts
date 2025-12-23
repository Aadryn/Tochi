/**
 * Représente un provider LLM configuré dans le système.
 */
export interface Provider {
  id: string
  name: string
  type: ProviderType
  baseUrl: string
  model: string
  isEnabled: boolean
  status: ProviderStatus
  requestsToday: number
  createdAt: string
}

/**
 * Types de providers supportés.
 */
export type ProviderType = 'openai' | 'anthropic' | 'azure-openai' | 'ollama' | 'custom'

/**
 * Statut de santé d'un provider.
 */
export type ProviderStatus = 'healthy' | 'degraded' | 'unhealthy'

/**
 * Requête de création d'un provider.
 */
export interface CreateProviderRequest {
  name: string
  type: ProviderType
  baseUrl: string
  apiKey: string
  model: string
  isEnabled: boolean
}

/**
 * Requête de mise à jour d'un provider.
 */
export interface UpdateProviderRequest extends Partial<CreateProviderRequest> {
  id: string
}

/**
 * Résultat d'un health check sur un provider.
 */
export interface ProviderHealthCheck {
  status: ProviderStatus
  latencyMs: number
  lastChecked: string
  errorMessage?: string
}
