/**
 * Types spécifiques aux formulaires UI.
 * Ces types sont utilisés pour les formulaires de création/édition
 * et peuvent être différents des types API.
 */

/**
 * Formulaire de création de tenant utilisé dans l'UI.
 */
export interface TenantFormData {
  name: string
  slug: string
  isActive: boolean
  quota: {
    maxRequestsPerMonth: number
    maxTokensPerMonth: number
  }
}

/**
 * Formulaire de création de route utilisé dans l'UI.
 */
export interface RouteFormData {
  name: string
  path: string
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH'
  providerId: string
  isEnabled: boolean
  rateLimit: {
    requestsPerMinute: number
    requestsPerHour: number
  }
}

/**
 * Route avec toutes les propriétés pour l'affichage UI.
 */
export interface RouteDisplayData {
  id: string
  name: string
  path: string
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH'
  providerId: string
  providerName: string
  isEnabled: boolean
  requestsToday: number
  avgLatencyMs: number
  rateLimit: {
    requestsPerMinute: number
    requestsPerHour: number
  }
  createdAt: string
}
