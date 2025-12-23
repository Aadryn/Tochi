import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { Tenant, CreateTenantRequest, TenantApiKey, CreateApiKeyResult } from '@/types'
import {
  fetchTenants,
  fetchTenant,
  createTenant,
  updateTenant,
  deleteTenant,
  fetchTenantApiKeys,
  createTenantApiKey,
  revokeApiKey,
  getMockTenants,
} from '@/api/tenants'

/**
 * Store Pinia pour la gestion des tenants.
 */
export const useTenantsStore = defineStore('tenants', () => {
  // État
  const tenants = ref<Tenant[]>([])
  const selectedTenant = ref<Tenant | null>(null)
  const apiKeys = ref<TenantApiKey[]>([])
  const isLoading = ref(false)
  const isSaving = ref(false)
  const error = ref<string | null>(null)

  // Getters calculés
  const totalTenants = computed(() => tenants.value.length)

  const activeTenants = computed(() => tenants.value.filter((t) => t.isActive))

  const totalRequestsThisMonth = computed(() =>
    tenants.value.reduce((sum, t) => sum + t.requestsThisMonth, 0),
  )

  const tenantsNearQuotaLimit = computed(() =>
    tenants.value.filter((t) => {
      const usage = t.quota.currentRequests / t.quota.maxRequestsPerMonth
      return usage >= 0.9 // 90% ou plus
    }),
  )

  // Actions
  async function loadTenants(useMock = false): Promise<void> {
    isLoading.value = true
    error.value = null

    try {
      if (useMock) {
        await new Promise((resolve) => setTimeout(resolve, 500))
        tenants.value = getMockTenants()
      } else {
        tenants.value = await fetchTenants()
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors du chargement des tenants'
      console.error('Erreur tenants:', err)
    } finally {
      isLoading.value = false
    }
  }

  async function loadTenant(id: string): Promise<void> {
    isLoading.value = true
    error.value = null

    try {
      selectedTenant.value = await fetchTenant(id)
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors du chargement du tenant'
      console.error('Erreur tenant:', err)
    } finally {
      isLoading.value = false
    }
  }

  async function addTenant(data: CreateTenantRequest): Promise<Tenant | null> {
    isSaving.value = true
    error.value = null

    try {
      const newTenant = await createTenant(data)
      tenants.value.push(newTenant)
      return newTenant
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors de la création du tenant'
      console.error('Erreur création tenant:', err)
      return null
    } finally {
      isSaving.value = false
    }
  }

  async function editTenant(
    id: string,
    data: Partial<CreateTenantRequest>,
  ): Promise<Tenant | null> {
    isSaving.value = true
    error.value = null

    try {
      const updated = await updateTenant(id, data)
      const index = tenants.value.findIndex((t) => t.id === id)
      if (index !== -1) {
        tenants.value[index] = updated
      }
      if (selectedTenant.value?.id === id) {
        selectedTenant.value = updated
      }
      return updated
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors de la mise à jour du tenant'
      console.error('Erreur mise à jour tenant:', err)
      return null
    } finally {
      isSaving.value = false
    }
  }

  async function removeTenant(id: string): Promise<boolean> {
    isSaving.value = true
    error.value = null

    try {
      await deleteTenant(id)
      tenants.value = tenants.value.filter((t) => t.id !== id)
      if (selectedTenant.value?.id === id) {
        selectedTenant.value = null
      }
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors de la suppression du tenant'
      console.error('Erreur suppression tenant:', err)
      return false
    } finally {
      isSaving.value = false
    }
  }

  async function loadApiKeys(tenantId: string): Promise<void> {
    isLoading.value = true
    error.value = null

    try {
      apiKeys.value = await fetchTenantApiKeys(tenantId)
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors du chargement des clés API'
      console.error('Erreur clés API:', err)
    } finally {
      isLoading.value = false
    }
  }

  async function addApiKey(tenantId: string, name: string): Promise<CreateApiKeyResult | null> {
    isSaving.value = true
    error.value = null

    try {
      const result = await createTenantApiKey(tenantId, name)
      // Recharger les clés API
      await loadApiKeys(tenantId)
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors de la création de la clé API'
      console.error('Erreur création clé API:', err)
      return null
    } finally {
      isSaving.value = false
    }
  }

  async function removeApiKey(tenantId: string, keyId: string): Promise<boolean> {
    isSaving.value = true
    error.value = null

    try {
      await revokeApiKey(keyId)
      apiKeys.value = apiKeys.value.filter((k) => k.id !== keyId)
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors de la révocation de la clé API'
      console.error('Erreur révocation clé API:', err)
      return false
    } finally {
      isSaving.value = false
    }
  }

  function selectTenant(tenant: Tenant | null): void {
    selectedTenant.value = tenant
    if (!tenant) {
      apiKeys.value = []
    }
  }

  function clearError(): void {
    error.value = null
  }

  function reset(): void {
    tenants.value = []
    selectedTenant.value = null
    apiKeys.value = []
    isLoading.value = false
    isSaving.value = false
    error.value = null
  }

  return {
    // État
    tenants,
    selectedTenant,
    apiKeys,
    isLoading,
    isSaving,
    error,
    // Getters
    totalTenants,
    activeTenants,
    totalRequestsThisMonth,
    tenantsNearQuotaLimit,
    // Actions
    loadTenants,
    loadTenant,
    addTenant,
    editTenant,
    removeTenant,
    loadApiKeys,
    addApiKey,
    removeApiKey,
    selectTenant,
    clearError,
    reset,
  }
})
