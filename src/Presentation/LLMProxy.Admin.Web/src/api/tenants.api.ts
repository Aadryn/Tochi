import { apiClient } from './client'
import type { Tenant, CreateTenantRequest } from '@/types/tenant'

/**
 * Récupère la liste de tous les tenants.
 */
export async function fetchTenants(): Promise<Tenant[]> {
  const response = await apiClient.get<Tenant[]>('/tenants')
  return response.data
}

/**
 * Récupère un tenant par son ID.
 */
export async function fetchTenant(id: string): Promise<Tenant> {
  const response = await apiClient.get<Tenant>(`/tenants/${id}`)
  return response.data
}

/**
 * Crée un nouveau tenant.
 */
export async function createTenant(data: CreateTenantRequest): Promise<Tenant> {
  const response = await apiClient.post<Tenant>('/tenants', data)
  return response.data
}

/**
 * Met à jour un tenant existant.
 */
export async function updateTenant(id: string, data: CreateTenantRequest): Promise<Tenant> {
  const response = await apiClient.put<Tenant>(`/tenants/${id}`, data)
  return response.data
}

/**
 * Supprime un tenant.
 */
export async function deleteTenant(id: string): Promise<void> {
  await apiClient.delete(`/tenants/${id}`)
}

/**
 * Récupère les clés API d'un tenant.
 */
export async function fetchTenantApiKeys(tenantId: string): Promise<Array<{
  id: string
  name: string
  prefix: string
  createdAt: string
  lastUsedAt?: string
}>> {
  const response = await apiClient.get(`/tenants/${tenantId}/api-keys`)
  return response.data
}

/**
 * Crée une nouvelle clé API pour un tenant.
 */
export async function createTenantApiKey(
  tenantId: string,
  name: string
): Promise<{ id: string; key: string }> {
  const response = await apiClient.post(`/tenants/${tenantId}/api-keys`, { name })
  return response.data
}

/**
 * Révoque une clé API.
 */
export async function revokeApiKey(tenantId: string, keyId: string): Promise<void> {
  await apiClient.delete(`/tenants/${tenantId}/api-keys/${keyId}`)
}
