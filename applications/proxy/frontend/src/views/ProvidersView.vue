<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useProvidersStore } from '@/stores'
import {
  AppCard,
  StatusBadge,
  LoadingSpinner,
  EmptyState,
  ProviderModal,
  ConfirmDialog,
} from '@/components'
import type { Provider, CreateProviderRequest } from '@/types'

const providersStore = useProvidersStore()

/** États des modals */
const showProviderModal = ref(false)
const showDeleteConfirm = ref(false)
const selectedProviderForEdit = ref<Provider | null>(null)

onMounted(async () => {
  await providersStore.loadProviders(true)
})

const isLoading = computed(() => providersStore.isLoading)
const providers = computed(() => providersStore.providers)

/**
 * Ouvre le modal de création.
 */
function openCreateModal(): void {
  selectedProviderForEdit.value = null
  showProviderModal.value = true
}

/**
 * Ouvre le modal d'édition.
 */
function openEditModal(provider: Provider): void {
  selectedProviderForEdit.value = provider
  showProviderModal.value = true
}

/**
 * Gère la soumission du formulaire provider.
 */
async function handleProviderSubmit(data: CreateProviderRequest): Promise<void> {
  let success: boolean

  if (selectedProviderForEdit.value) {
    const result = await providersStore.editProvider(selectedProviderForEdit.value.id, data)
    success = result !== null
  } else {
    const result = await providersStore.addProvider(data)
    success = result !== null
  }

  if (success) {
    showProviderModal.value = false
    selectedProviderForEdit.value = null
  }
}

/**
 * Ouvre le dialog de confirmation de suppression.
 */
function confirmDelete(provider: Provider): void {
  providersStore.selectProvider(provider)
  showDeleteConfirm.value = true
}

/**
 * Supprime le provider sélectionné.
 */
async function deleteProvider(): Promise<void> {
  if (!providersStore.selectedProvider) return

  const success = await providersStore.removeProvider(providersStore.selectedProvider.id)
  if (success) {
    showDeleteConfirm.value = false
  }
}

/**
 * Vérifie la santé d'un provider.
 */
async function checkHealth(provider: Provider): Promise<void> {
  await providersStore.checkHealth(provider.id)
}

/**
 * Formate une date.
 */
function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('fr-FR', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

/**
 * Formate un nombre.
 */
function formatNumber(value: number): string {
  return value.toLocaleString('fr-FR')
}
</script>

<template>
  <div class="providers-view">
    <div class="view-header">
      <div class="header-info">
        <h2 class="view-title" data-testid="page-title">Providers LLM</h2>
        <p class="view-description">
          Gérez vos fournisseurs de modèles de langage (OpenAI, Anthropic, Ollama, etc.)
        </p>
      </div>
      <button class="btn-primary" @click="openCreateModal">
        <i class="pi pi-plus"></i>
        Nouveau Provider
      </button>
    </div>

    <LoadingSpinner v-if="isLoading" message="Chargement des providers..." />

    <EmptyState
      v-else-if="providers.length === 0"
      icon="pi pi-server"
      title="Aucun provider configuré"
      description="Ajoutez votre premier provider LLM pour commencer à router vos requêtes."
      action-label="Ajouter un provider"
      @action="openCreateModal"
    />

    <div v-else class="providers-grid">
      <AppCard v-for="provider in providers" :key="provider.id" class="provider-card">
        <div class="provider-header">
          <div class="provider-info">
            <h3 class="provider-name">{{ provider.name }}</h3>
            <span class="provider-type">{{ provider.type }}</span>
          </div>
          <StatusBadge
            :status="provider.status"
            :label="
              provider.status === 'healthy'
                ? 'En ligne'
                : provider.status === 'degraded'
                  ? 'Dégradé'
                  : 'Hors ligne'
            "
          />
        </div>

        <div class="provider-details">
          <div class="detail-row">
            <span class="detail-label">URL</span>
            <span class="detail-value url" :title="provider.baseUrl">
              {{ provider.baseUrl }}
            </span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Modèle</span>
            <span class="detail-value">{{ provider.model }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Requêtes aujourd'hui</span>
            <span class="detail-value">{{ formatNumber(provider.requestsToday) }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Créé le</span>
            <span class="detail-value">{{ formatDate(provider.createdAt) }}</span>
          </div>
        </div>

        <div class="provider-actions">
          <button
            class="btn-icon"
            title="Vérifier la santé"
            @click="checkHealth(provider)"
          >
            <i class="pi pi-heart"></i>
          </button>
          <button
            class="btn-icon"
            title="Modifier"
            @click="openEditModal(provider)"
          >
            <i class="pi pi-pencil"></i>
          </button>
          <button
            class="btn-icon danger"
            title="Supprimer"
            @click="confirmDelete(provider)"
          >
            <i class="pi pi-trash"></i>
          </button>
        </div>
      </AppCard>
    </div>

    <!-- Modal création/édition -->
    <ProviderModal
      v-model:visible="showProviderModal"
      :provider="selectedProviderForEdit"
      :loading="providersStore.isSaving"
      @submit="handleProviderSubmit"
    />

    <!-- Dialog de confirmation de suppression -->
    <ConfirmDialog
      v-model:visible="showDeleteConfirm"
      title="Supprimer le provider"
      :message="`Êtes-vous sûr de vouloir supprimer le provider « ${providersStore.selectedProvider?.name} » ? Cette action est irréversible.`"
      confirm-label="Supprimer"
      type="danger"
      :loading="providersStore.isSaving"
      @confirm="deleteProvider"
    />
  </div>
</template>

<style scoped>
.providers-view {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.view-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.view-title {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--app-text, #1e293b);
}

.view-description {
  margin: 0.25rem 0 0;
  color: var(--app-text-muted, #64748b);
}

/* Grille des providers */
.providers-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1rem;
}

.provider-card {
  display: flex;
  flex-direction: column;
}

.provider-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  margin-bottom: 1rem;
}

.provider-name {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--app-text, #1e293b);
}

.provider-type {
  font-size: 0.75rem;
  color: var(--app-text-muted, #64748b);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.provider-details {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  flex: 1;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.875rem;
}

.detail-label {
  color: var(--app-text-muted, #64748b);
}

.detail-value {
  font-weight: 500;
  color: var(--app-text, #1e293b);
}

.detail-value.url {
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.75rem;
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.provider-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--app-border, #e2e8f0);
}

/* Boutons */
.btn-primary {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.625rem 1rem;
  background: var(--app-primary, #3b82f6);
  color: white;
  border: none;
  border-radius: var(--app-radius-md, 8px);
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-primary:hover:not(:disabled) {
  background: var(--app-primary-dark, #2563eb);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  background: transparent;
  border: 1px solid var(--app-border, #e2e8f0);
  border-radius: var(--app-radius-md, 8px);
  color: var(--app-text-muted, #64748b);
  cursor: pointer;
  transition: all 0.2s;
}

.btn-icon:hover {
  background: var(--app-hover-bg, #f1f5f9);
  color: var(--app-text, #1e293b);
}

.btn-icon.danger:hover {
  background: #fef2f2;
  border-color: #fecaca;
  color: #dc2626;
}
</style>
