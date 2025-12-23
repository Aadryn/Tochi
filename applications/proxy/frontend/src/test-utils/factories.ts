import type { Provider, Tenant, DashboardMetrics, ProxyRoute } from '@/types'

/**
 * Factory de données de test pour les providers.
 */
export function createMockProvider(overrides: Partial<Provider> = {}): Provider {
  return {
    id: '1',
    name: 'Test Provider',
    type: 'openai',
    baseUrl: 'https://api.test.com/v1',
    model: 'gpt-4',
    isEnabled: true,
    status: 'healthy',
    requestsToday: 1000,
    createdAt: '2024-01-01T00:00:00Z',
    ...overrides,
  }
}

/**
 * Factory de données de test pour les tenants.
 */
export function createMockTenant(overrides: Partial<Tenant> = {}): Tenant {
  return {
    id: '1',
    name: 'Test Tenant',
    slug: 'test-tenant',
    isActive: true,
    quota: {
      maxRequestsPerMonth: 100000,
      maxTokensPerMonth: 10000000,
      currentRequests: 50000,
      currentTokens: 5000000,
    },
    apiKeysCount: 2,
    requestsThisMonth: 50000,
    createdAt: '2024-01-01T00:00:00Z',
    ...overrides,
  }
}

/**
 * Factory de données de test pour les métriques du dashboard.
 */
export function createMockDashboardMetrics(
  overrides: Partial<DashboardMetrics> = {},
): DashboardMetrics {
  return {
    totalRequests: 100000,
    activeProviders: 4,
    activeTenants: 10,
    avgLatencyMs: 200,
    successRate: 99.5,
    providers: [
      { name: 'OpenAI', status: 'healthy', requestsToday: 40000 },
      { name: 'Anthropic', status: 'healthy', requestsToday: 30000 },
      { name: 'Ollama', status: 'degraded', requestsToday: 10000 },
      { name: 'Azure', status: 'healthy', requestsToday: 20000 },
    ],
    ...overrides,
  }
}

/**
 * Factory de données de test pour les routes.
 */
export function createMockRoute(overrides: Partial<ProxyRoute> = {}): ProxyRoute {
  return {
    id: '1',
    name: 'Chat Completions Route',
    path: '/v1/chat/completions',
    providerId: '1',
    providerName: 'Test Provider',
    isEnabled: true,
    rateLimitPerMinute: 60,
    createdAt: '2024-01-01T00:00:00Z',
    ...overrides,
  }
}

/**
 * Liste de providers mock.
 */
export const mockProviders: Provider[] = [
  createMockProvider({ id: '1', name: 'OpenAI GPT-4', type: 'openai' }),
  createMockProvider({ id: '2', name: 'Anthropic Claude', type: 'anthropic' }),
  createMockProvider({
    id: '3',
    name: 'Ollama Local',
    type: 'ollama',
    status: 'degraded',
  }),
]

/**
 * Liste de tenants mock.
 */
export const mockTenants: Tenant[] = [
  createMockTenant({ id: '1', name: 'Acme Corp', slug: 'acme-corp' }),
  createMockTenant({ id: '2', name: 'Tech Startup', slug: 'tech-startup' }),
  createMockTenant({
    id: '3',
    name: 'Research Lab',
    slug: 'research-lab',
    isActive: false,
  }),
]

/**
 * Liste de routes mock.
 */
export const mockRoutes: ProxyRoute[] = [
  createMockRoute({ id: '1', path: '/v1/chat/completions' }),
  createMockRoute({ id: '2', path: '/v1/embeddings', name: 'Embeddings Route' }),
  createMockRoute({ id: '3', path: '/v1/messages', providerId: '2', name: 'Messages Route' }),
]
