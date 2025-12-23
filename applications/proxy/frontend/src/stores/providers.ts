import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { Provider, CreateProviderRequest, ProviderHealthCheck } from '@/types'
import {
  fetchProviders,
  fetchProvider,
  createProvider,
  updateProvider,
  deleteProvider,
  checkProviderHealth,
  getMockProviders,
} from '@/api/providers'

/**
 * Store Pinia pour la gestion des providers LLM.
 */
export const useProvidersStore = defineStore('providers', () => {
  // État
  const providers = ref<Provider[]>([])
  const selectedProvider = ref<Provider | null>(null)
  const isLoading = ref(false)
  const isSaving = ref(false)
  const error = ref<string | null>(null)

  // Getters calculés
  const totalProviders = computed(() => providers.value.length)

  const enabledProviders = computed(() => providers.value.filter((p) => p.isEnabled))

  const healthyProviders = computed(() => providers.value.filter((p) => p.status === 'healthy'))

  const providersByType = computed(() => {
    const grouped: Record<string, Provider[]> = {}
    for (const provider of providers.value) {
      if (!grouped[provider.type]) {
        grouped[provider.type] = []
      }
      grouped[provider.type].push(provider)
    }
    return grouped
  })

  // Actions
  async function loadProviders(useMock = false): Promise<void> {
    isLoading.value = true
    error.value = null

    try {
      if (useMock) {
        await new Promise((resolve) => setTimeout(resolve, 500))
        providers.value = getMockProviders()
      } else {
        providers.value = await fetchProviders()
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors du chargement des providers'
      console.error('Erreur providers:', err)
    } finally {
      isLoading.value = false
    }
  }

  async function loadProvider(id: string): Promise<void> {
    isLoading.value = true
    error.value = null

    try {
      selectedProvider.value = await fetchProvider(id)
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors du chargement du provider'
      console.error('Erreur provider:', err)
    } finally {
      isLoading.value = false
    }
  }

  async function addProvider(data: CreateProviderRequest): Promise<Provider | null> {
    isSaving.value = true
    error.value = null

    try {
      const newProvider = await createProvider(data)
      providers.value.push(newProvider)
      return newProvider
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors de la création du provider'
      console.error('Erreur création provider:', err)
      return null
    } finally {
      isSaving.value = false
    }
  }

  async function editProvider(
    id: string,
    data: Partial<CreateProviderRequest>,
  ): Promise<Provider | null> {
    isSaving.value = true
    error.value = null

    try {
      const updated = await updateProvider(id, data)
      const index = providers.value.findIndex((p) => p.id === id)
      if (index !== -1) {
        providers.value[index] = updated
      }
      if (selectedProvider.value?.id === id) {
        selectedProvider.value = updated
      }
      return updated
    } catch (err) {
      error.value =
        err instanceof Error ? err.message : 'Erreur lors de la mise à jour du provider'
      console.error('Erreur mise à jour provider:', err)
      return null
    } finally {
      isSaving.value = false
    }
  }

  async function removeProvider(id: string): Promise<boolean> {
    isSaving.value = true
    error.value = null

    try {
      await deleteProvider(id)
      providers.value = providers.value.filter((p) => p.id !== id)
      if (selectedProvider.value?.id === id) {
        selectedProvider.value = null
      }
      return true
    } catch (err) {
      error.value =
        err instanceof Error ? err.message : 'Erreur lors de la suppression du provider'
      console.error('Erreur suppression provider:', err)
      return false
    } finally {
      isSaving.value = false
    }
  }

  async function checkHealth(id: string): Promise<ProviderHealthCheck | null> {
    error.value = null

    try {
      const health = await checkProviderHealth(id)
      // Mettre à jour le statut dans la liste
      const index = providers.value.findIndex((p) => p.id === id)
      if (index !== -1) {
        providers.value[index].status = health.status
      }
      return health
    } catch (err) {
      error.value =
        err instanceof Error ? err.message : 'Erreur lors de la vérification de santé'
      console.error('Erreur health check:', err)
      return null
    }
  }

  function selectProvider(provider: Provider | null): void {
    selectedProvider.value = provider
  }

  function clearError(): void {
    error.value = null
  }

  function reset(): void {
    providers.value = []
    selectedProvider.value = null
    isLoading.value = false
    isSaving.value = false
    error.value = null
  }

  return {
    // État
    providers,
    selectedProvider,
    isLoading,
    isSaving,
    error,
    // Getters
    totalProviders,
    enabledProviders,
    healthyProviders,
    providersByType,
    // Actions
    loadProviders,
    loadProvider,
    addProvider,
    editProvider,
    removeProvider,
    checkHealth,
    selectProvider,
    clearError,
    reset,
  }
})
