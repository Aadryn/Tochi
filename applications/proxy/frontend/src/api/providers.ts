import { apiClient } from './client'
import type { Provider, CreateProviderRequest, ProviderHealthCheck } from '@/types'
import { API_CONFIG } from './config'

/**
 * Récupère la liste de tous les providers pour un tenant.
 */
export async function fetchProviders(tenantId?: string): Promise<Provider[]> {
  if (API_CONFIG.USE_MOCK_DATA) {
    return getMockProviders()
  }
  
  // Si un tenantId est fourni, récupérer uniquement les providers de ce tenant
  const endpoint = tenantId 
    ? `/${API_CONFIG.API_VERSION}/providers/tenant/${tenantId}`
    : `/${API_CONFIG.API_VERSION}/providers`
  
  const response = await apiClient.get<Provider[]>(endpoint)
  return response.data
}

/**
 * Récupère un provider par son ID.
 */
export async function fetchProvider(id: string): Promise<Provider> {
  if (API_CONFIG.USE_MOCK_DATA) {
    const providers = getMockProviders()
    const provider = providers.find(p => p.id === id)
    if (!provider) throw new Error('Provider not found')
    return provider
  }
  
  const response = await apiClient.get<Provider>(`/${API_CONFIG.API_VERSION}/providers/${id}`)
  return response.data
}

/**
 * Crée un nouveau provider.
 */
export async function createProvider(data: CreateProviderRequest): Promise<Provider> {
  if (API_CONFIG.USE_MOCK_DATA) {
    await new Promise(resolve => setTimeout(resolve, 500))
    return {
      id: 'mock-' + Date.now(),
      name: data.name,
      type: data.type,
      baseUrl: data.baseUrl,
      model: data.model,
      isEnabled: data.isEnabled ?? true,
      status: 'healthy',
      requestsToday: 0,
      createdAt: new Date().toISOString(),
    }
  }
  
  const response = await apiClient.post<Provider>(`/${API_CONFIG.API_VERSION}/providers`, data)
  return response.data
}

/**
 * Met à jour un provider existant.
 */
export async function updateProvider(
  id: string,
  data: Partial<CreateProviderRequest>,
): Promise<Provider> {
  if (API_CONFIG.USE_MOCK_DATA) {
    await new Promise(resolve => setTimeout(resolve, 500))
    const providers = getMockProviders()
    const provider = providers.find(p => p.id === id)
    if (!provider) throw new Error('Provider not found')
    return { ...provider, ...data }
  }
  
  const response = await apiClient.put<Provider>(`/${API_CONFIG.API_VERSION}/providers/${id}`, data)
  return response.data
}

/**
 * Supprime un provider.
 */
export async function deleteProvider(id: string): Promise<void> {
  if (API_CONFIG.USE_MOCK_DATA) {
    await new Promise(resolve => setTimeout(resolve, 500))
    return
  }
  
  await apiClient.delete(`/${API_CONFIG.API_VERSION}/providers/${id}`)
}

/**
 * Vérifie la santé d'un provider (si endpoint disponible).
 */
export async function checkProviderHealth(id: string): Promise<ProviderHealthCheck> {
  if (API_CONFIG.USE_MOCK_DATA) {
    return {
      status: Math.random() > 0.2 ? 'healthy' : 'degraded',
      latencyMs: Math.floor(Math.random() * 500) + 50,
      lastChecked: new Date().toISOString(),
    }
  }
  
  // Note: Le backend ne semble pas avoir d'endpoint health pour providers
  // On pourrait le retourner depuis les données du provider
  throw new Error('Health check endpoint not implemented')
}

/**
 * Retourne des données mock pour le développement.
 */
export function getMockProviders(): Provider[] {
  return [
    {
      id: '1',
      name: 'OpenAI GPT-4',
      type: 'openai',
      baseUrl: 'https://api.openai.com/v1',
      model: 'gpt-4',
      isEnabled: true,
      status: 'healthy',
      requestsToday: 45230,
      createdAt: '2024-01-15T10:30:00Z',
    },
    {
      id: '2',
      name: 'Anthropic Claude',
      type: 'anthropic',
      baseUrl: 'https://api.anthropic.com',
      model: 'claude-3-opus',
      isEnabled: true,
      status: 'healthy',
      requestsToday: 32150,
      createdAt: '2024-01-20T14:00:00Z',
    },
    {
      id: '3',
      name: 'Ollama Local',
      type: 'ollama',
      baseUrl: 'http://localhost:11434',
      model: 'llama2',
      isEnabled: true,
      status: 'degraded',
      requestsToday: 8420,
      createdAt: '2024-02-01T09:15:00Z',
    },
    {
      id: '4',
      name: 'Azure OpenAI',
      type: 'azure-openai',
      baseUrl: 'https://myresource.openai.azure.com',
      model: 'gpt-4-turbo',
      isEnabled: true,
      status: 'healthy',
      requestsToday: 28970,
      createdAt: '2024-02-10T16:45:00Z',
    },
  ]
}
