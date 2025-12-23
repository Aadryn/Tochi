import { apiClient } from './client'
import type { Tenant, CreateTenantRequest, TenantApiKey, CreateApiKeyResult } from '@/types'
import { API_CONFIG } from './config'

/**
 * Récupère la liste de tous les tenants.
 */
export async function fetchTenants(): Promise<Tenant[]> {
  if (API_CONFIG.USE_MOCK_DATA) {
    return getMockTenants()
  }
  
  const response = await apiClient.get<Tenant[]>(`/${API_CONFIG.API_VERSION}/tenants`)
  return response.data
}

/**
 * Récupère un tenant par son ID.
 */
export async function fetchTenant(id: string): Promise<Tenant> {
  if (API_CONFIG.USE_MOCK_DATA) {
    const tenants = getMockTenants()
    const tenant = tenants.find(t => t.id === id)
    if (!tenant) throw new Error('Tenant not found')
    return tenant
  }
  
  const response = await apiClient.get<Tenant>(`/${API_CONFIG.API_VERSION}/tenants/${id}`)
  return response.data
}

/**
 * Crée un nouveau tenant.
 */
export async function createTenant(data: CreateTenantRequest): Promise<Tenant> {
  if (API_CONFIG.USE_MOCK_DATA) {
    // Simuler un délai
    await new Promise(resolve => setTimeout(resolve, 500))
    return {
      id: 'mock-' + Date.now(),
      name: data.name,
      slug: data.slug,
      isActive: true,
      quota: {
        maxRequestsPerMonth: data.maxRequestsPerMonth || 100000,
        maxTokensPerMonth: data.maxTokensPerMonth || 10000000,
        currentRequests: 0,
        currentTokens: 0,
      },
      apiKeysCount: 0,
      requestsThisMonth: 0,
      createdAt: new Date().toISOString(),
    }
  }
  
  const response = await apiClient.post<Tenant>(`/${API_CONFIG.API_VERSION}/tenants`, data)
  return response.data
}

/**
 * Met à jour un tenant existant (via settings endpoint).
 */
export async function updateTenant(
  id: string,
  data: Partial<CreateTenantRequest>,
): Promise<Tenant> {
  if (API_CONFIG.USE_MOCK_DATA) {
    await new Promise(resolve => setTimeout(resolve, 500))
    const tenants = getMockTenants()
    const tenant = tenants.find(t => t.id === id)
    if (!tenant) throw new Error('Tenant not found')
    // Mettre à jour seulement les champs autorisés
    return {
      ...tenant,
      name: data.name || tenant.name,
      slug: data.slug || tenant.slug,
      quota: {
        ...tenant.quota,
        maxRequestsPerMonth: data.maxRequestsPerMonth || tenant.quota.maxRequestsPerMonth,
        maxTokensPerMonth: data.maxTokensPerMonth || tenant.quota.maxTokensPerMonth,
      },
    }
  }
  
  const response = await apiClient.put<Tenant>(
    `/${API_CONFIG.API_VERSION}/tenants/${id}/settings`, 
    data
  )
  return response.data
}

/**
 * Désactive un tenant.
 */
export async function deactivateTenant(id: string): Promise<void> {
  if (API_CONFIG.USE_MOCK_DATA) {
    await new Promise(resolve => setTimeout(resolve, 500))
    return
  }
  
  await apiClient.post(`/${API_CONFIG.API_VERSION}/tenants/${id}/deactivate`)
}

/**
 * Active un tenant.
 */
export async function activateTenant(id: string): Promise<void> {
  if (API_CONFIG.USE_MOCK_DATA) {
    await new Promise(resolve => setTimeout(resolve, 500))
    return
  }
  
  await apiClient.post(`/${API_CONFIG.API_VERSION}/tenants/${id}/activate`)
}

/**
 * Supprime un tenant (backend ne supporte pas DELETE, on utilise deactivate).
 */
export async function deleteTenant(id: string): Promise<void> {
  // Le backend n'a pas de DELETE pour tenants, on utilise deactivate
  return deactivateTenant(id)
}

/**
 * Récupère les clés API d'un tenant.
 */
export async function fetchTenantApiKeys(tenantId: string): Promise<TenantApiKey[]> {
  if (API_CONFIG.USE_MOCK_DATA) {
    return getMockApiKeys(tenantId)
  }
  
  const response = await apiClient.get<TenantApiKey[]>(
    `/${API_CONFIG.API_VERSION}/apikeys/tenant/${tenantId}`
  )
  return response.data
}

/**
 * Crée une nouvelle clé API pour un tenant.
 */
export async function createTenantApiKey(
  tenantId: string,
  name: string,
): Promise<CreateApiKeyResult> {
  if (API_CONFIG.USE_MOCK_DATA) {
    await new Promise(resolve => setTimeout(resolve, 500))
    const randomKey = 'sk-mock-' + Math.random().toString(36).substring(2)
    return {
      id: 'mock-key-' + Date.now(),
      key: randomKey,
    }
  }
  
  const response = await apiClient.post<CreateApiKeyResult>(
    `/${API_CONFIG.API_VERSION}/apikeys`, 
    {
      name,
      tenantId,
    }
  )
  return response.data
}

/**
 * Révoque une clé API.
 */
export async function revokeApiKey(keyId: string): Promise<void> {
  if (API_CONFIG.USE_MOCK_DATA) {
    await new Promise(resolve => setTimeout(resolve, 500))
    return
  }
  
  await apiClient.post(`/${API_CONFIG.API_VERSION}/apikeys/${keyId}/revoke`)
}

/**
 * Supprime une clé API.
 */
export async function deleteTenantApiKey(keyId: string): Promise<void> {
  if (API_CONFIG.USE_MOCK_DATA) {
    await new Promise(resolve => setTimeout(resolve, 500))
    return
  }
  
  await apiClient.delete(`/${API_CONFIG.API_VERSION}/apikeys/${keyId}`)
}

/**
 * Retourne des données mock pour le développement.
 */
export function getMockTenants(): Tenant[] {
  return [
    {
      id: '1',
      name: 'Acme Corp',
      slug: 'acme-corp',
      isActive: true,
      quota: {
        maxRequestsPerMonth: 100000,
        maxTokensPerMonth: 10000000,
        currentRequests: 45230,
        currentTokens: 4523000,
      },
      apiKeysCount: 3,
      requestsThisMonth: 45230,
      createdAt: '2024-01-10T08:00:00Z',
    },
    {
      id: '2',
      name: 'Tech Startup',
      slug: 'tech-startup',
      isActive: true,
      quota: {
        maxRequestsPerMonth: 50000,
        maxTokensPerMonth: 5000000,
        currentRequests: 32150,
        currentTokens: 3215000,
      },
      apiKeysCount: 2,
      requestsThisMonth: 32150,
      createdAt: '2024-01-15T12:30:00Z',
    },
    {
      id: '3',
      name: 'Research Lab',
      slug: 'research-lab',
      isActive: true,
      quota: {
        maxRequestsPerMonth: 200000,
        maxTokensPerMonth: 20000000,
        currentRequests: 156420,
        currentTokens: 15642000,
      },
      apiKeysCount: 5,
      requestsThisMonth: 156420,
      createdAt: '2024-02-01T09:00:00Z',
    },
  ]
}

/**
 * Retourne des clés API mock pour le développement.
 */
export function getMockApiKeys(tenantId: string): TenantApiKey[] {
  return [
    {
      id: 'key-1',
      name: 'Production Key',
      prefix: 'sk-prod',
      keyPreview: 'sk-prod-...abc123',
      createdAt: '2024-01-10T08:00:00Z',
      lastUsedAt: '2024-12-22T10:30:00Z',
    },
    {
      id: 'key-2',
      name: 'Development Key',
      prefix: 'sk-dev',
      keyPreview: 'sk-dev-...def456',
      createdAt: '2024-01-15T14:00:00Z',
      lastUsedAt: '2024-12-20T15:45:00Z',
    },
    {
      id: 'key-3',
      name: 'Testing Key',
      prefix: 'sk-test',
      keyPreview: 'sk-test-...ghi789',
      createdAt: '2024-02-01T09:00:00Z',
      lastUsedAt: undefined,
    },
  ]
}
