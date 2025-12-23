/**
 * Métriques du dashboard.
 */
export interface DashboardMetrics {
  totalRequests: number
  activeProviders: number
  activeTenants: number
  avgLatencyMs: number
  successRate: number
  providers: ProviderSummary[]
}

/**
 * Résumé d'un provider pour le dashboard.
 */
export interface ProviderSummary {
  name: string
  status: 'healthy' | 'degraded' | 'unhealthy'
  requestsToday: number
}

/**
 * Statistiques de requêtes par période.
 */
export interface RequestStats {
  period: string
  requests: number
  errors: number
  avgLatencyMs: number
}

/**
 * Configuration de route proxy.
 */
export interface ProxyRoute {
  id: string
  name: string
  path: string
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH'
  providerId: string
  providerName: string
  isEnabled: boolean
  rateLimitPerMinute: number
  rateLimit?: {
    requestsPerMinute: number
    requestsPerHour: number
  }
  requestsToday?: number
  avgLatencyMs?: number
  createdAt: string
}

/**
 * Requête de création/mise à jour de route.
 */
export interface CreateRouteRequest {
  name: string
  path: string
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH'
  providerId: string
  isEnabled: boolean
  rateLimitPerMinute: number
  rateLimit?: {
    requestsPerMinute: number
    requestsPerHour: number
  }
}

/**
 * Configuration système de l'application (API).
 */
export interface ApiSettings {
  defaultRateLimitPerMinute: number
  enableTelemetry: boolean
  logLevel: 'debug' | 'info' | 'warning' | 'error'
  retryPolicy: RetryPolicy
}

/**
 * Politique de retry.
 */
export interface RetryPolicy {
  maxRetries: number
  delayMs: number
  useExponentialBackoff: boolean
}

/**
 * Paramètres d'interface utilisateur de l'application.
 */
export interface AppSettings {
  theme: 'light' | 'dark'
  refreshInterval: number
  showNotifications: boolean
  compactMode: boolean
  language: 'fr' | 'en'
}
