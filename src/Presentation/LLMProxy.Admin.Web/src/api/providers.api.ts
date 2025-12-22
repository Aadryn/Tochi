import { apiClient } from './client'
import type { Provider, CreateProviderRequest } from '@/types/provider'

/**
 * Récupère la liste de tous les providers.
 */
export async function fetchProviders(): Promise<Provider[]> {
  const response = await apiClient.get<Provider[]>('/providers')
  return response.data
}

/**
 * Récupère un provider par son ID.
 */
export async function fetchProvider(id: string): Promise<Provider> {
  const response = await apiClient.get<Provider>(`/providers/${id}`)
  return response.data
}

/**
 * Crée un nouveau provider.
 */
export async function createProvider(data: CreateProviderRequest): Promise<Provider> {
  const response = await apiClient.post<Provider>('/providers', data)
  return response.data
}

/**
 * Met à jour un provider existant.
 */
export async function updateProvider(id: string, data: CreateProviderRequest): Promise<Provider> {
  const response = await apiClient.put<Provider>(`/providers/${id}`, data)
  return response.data
}

/**
 * Supprime un provider.
 */
export async function deleteProvider(id: string): Promise<void> {
  await apiClient.delete(`/providers/${id}`)
}

/**
 * Vérifie la santé d'un provider.
 */
export async function checkProviderHealth(id: string): Promise<{ status: string; latencyMs: number }> {
  const response = await apiClient.get<{ status: string; latencyMs: number }>(
    `/providers/${id}/health`
  )
  return response.data
}
