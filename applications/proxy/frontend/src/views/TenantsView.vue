<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useTenantsStore } from '@/stores'
import {
  AppCard,
  StatusBadge,
  LoadingSpinner,
  EmptyState,
  TenantModal,
  ConfirmDialog,
  AppModal,
} from '@/components'
import type { Tenant, CreateTenantRequest } from '@/types'

const tenantsStore = useTenantsStore()

/** États des modals */
const showTenantModal = ref(false)
const showDeleteConfirm = ref(false)
const showApiKeysModal = ref(false)
const selectedTenantForEdit = ref<Tenant | null>(null)
const newApiKeyName = ref('')

onMounted(async () => {
  await tenantsStore.loadTenants(true)
})

const isLoading = computed(() => tenantsStore.isLoading)
const tenants = computed(() => tenantsStore.tenants)

/**
 * Ouvre le modal de création.
 */
function openCreateModal(): void {
  selectedTenantForEdit.value = null
  showTenantModal.value = true
}

/**
 * Ouvre le modal d'édition.
 */
function openEditModal(tenant: Tenant): void {
  selectedTenantForEdit.value = tenant
  showTenantModal.value = true
}

/**
 * Gère la soumission du formulaire tenant.
 */
async function handleTenantSubmit(data: CreateTenantRequest): Promise<void> {
  let success: boolean

  if (selectedTenantForEdit.value) {
    const result = await tenantsStore.editTenant(selectedTenantForEdit.value.id, data)
    success = result !== null
  } else {
    const result = await tenantsStore.addTenant(data)
    success = result !== null
  }

  if (success) {
    showTenantModal.value = false
    selectedTenantForEdit.value = null
  }
}

/**
 * Ouvre le dialog de confirmation de suppression.
 */
function confirmDelete(tenant: Tenant): void {
  tenantsStore.selectTenant(tenant)
  showDeleteConfirm.value = true
}

/**
 * Supprime le tenant sélectionné.
 */
async function deleteTenant(): Promise<void> {
  if (!tenantsStore.selectedTenant) return

  const success = await tenantsStore.removeTenant(tenantsStore.selectedTenant.id)
  if (success) {
    showDeleteConfirm.value = false
  }
}

/**
 * Ouvre le modal de gestion des clés API.
 */
async function openApiKeysModal(tenant: Tenant): Promise<void> {
  tenantsStore.selectTenant(tenant)
  await tenantsStore.loadApiKeys(tenant.id)
  newApiKeyName.value = ''
  showApiKeysModal.value = true
}

/**
 * Crée une nouvelle clé API.
 */
async function createApiKey(): Promise<void> {
  if (!tenantsStore.selectedTenant || !newApiKeyName.value.trim()) return

  const result = await tenantsStore.addApiKey(
    tenantsStore.selectedTenant.id,
    newApiKeyName.value.trim(),
  )

  if (result) {
    newApiKeyName.value = ''
    alert(`Clé API créée !\n\nClé: ${result.key}\n\nCopiez cette clé maintenant, elle ne sera plus affichée.`)
  }
}

/**
 * Révoque une clé API.
 */
async function revokeApiKey(keyId: string): Promise<void> {
  if (!tenantsStore.selectedTenant) return

  if (confirm('Êtes-vous sûr de vouloir révoquer cette clé API ?')) {
    await tenantsStore.removeApiKey(tenantsStore.selectedTenant.id, keyId)
  }
}

/**
 * Formate un nombre.
 */
function formatNumber(value: number): string {
  if (value >= 1000000) {
    return `${(value / 1000000).toFixed(1)}M`
  }
  if (value >= 1000) {
    return `${(value / 1000).toFixed(1)}k`
  }
  return value.toLocaleString('fr-FR')
}

/**
 * Calcule le pourcentage d'utilisation du quota.
 */
function getQuotaPercentage(current: number, max: number): number {
  if (max === 0) return 0
  return Math.min(100, Math.round((current / max) * 100))
}

/**
 * Retourne la classe CSS selon le pourcentage.
 */
function getQuotaClass(percentage: number): string {
  if (percentage >= 90) return 'quota-danger'
  if (percentage >= 70) return 'quota-warning'
  return 'quota-ok'
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
 * Masque une clé API pour l'affichage.
 */
function maskApiKey(key: string): string {
  if (key.length <= 8) return '••••••••'
  return key.substring(0, 4) + '••••' + key.substring(key.length - 4)
}
</script>

<template>
  <div class="tenants-view">
    <div class="view-header">
      <div class="header-info">
        <h2 class="view-title" data-testid="page-title">Tenants</h2>
        <p class="view-description">
          Gérez les organisations et leurs quotas d'utilisation de l'API.
        </p>
      </div>
      <button class="btn-primary" @click="openCreateModal">
        <i class="pi pi-plus"></i>
        data-testid="btn-new-tenant"
        Nouveau Tenant
      </button>
    </div>

    <LoadingSpinner v-if="isLoading" message="Chargement des tenants..." />

    <EmptyState
      v-else-if="tenants.length === 0"
      icon="pi pi-building"
      title="Aucun tenant configuré"
      description="Créez votre premier tenant pour gérer l'accès à l'API."
      action-label="Créer un tenant"
      @action="openCreateModal"
    />

    <div v-else class="tenants-grid">
      <AppCard v-for="tenant in tenants" :key="tenant.id" class="tenant-card">
        <div class="tenant-header">
          <div class="tenant-info">
            <h3 class="tenant-name">{{ tenant.name }}</h3>
            <span class="tenant-slug">/api/{{ tenant.slug }}/</span>
          </div>
          <StatusBadge
            :status="tenant.isActive ? 'healthy' : 'unhealthy'"
            :label="tenant.isActive ? 'Actif' : 'Inactif'"
          />
        </div>

        <div class="tenant-quotas">
          <div class="quota-item">
            <div class="quota-header">
              <span class="quota-label">Requêtes ce mois</span>
              <span class="quota-value">
                {{ formatNumber(tenant.quota.currentRequests) }} /
                {{ formatNumber(tenant.quota.maxRequestsPerMonth) }}
              </span>
            </div>
            <div class="quota-bar">
              <div
                class="quota-progress"
                :class="getQuotaClass(getQuotaPercentage(tenant.quota.currentRequests, tenant.quota.maxRequestsPerMonth))"
                :style="{
                  width: `${getQuotaPercentage(tenant.quota.currentRequests, tenant.quota.maxRequestsPerMonth)}%`,
                }"
              ></div>
            </div>
          </div>

          <div class="quota-item">
            <div class="quota-header">
              <span class="quota-label">Tokens ce mois</span>
              <span class="quota-value">
                {{ formatNumber(tenant.quota.currentTokens) }} /
                {{ formatNumber(tenant.quota.maxTokensPerMonth) }}
              </span>
            </div>
            <div class="quota-bar">
              <div
                class="quota-progress"
                :class="getQuotaClass(getQuotaPercentage(tenant.quota.currentTokens, tenant.quota.maxTokensPerMonth))"
                :style="{
                  width: `${getQuotaPercentage(tenant.quota.currentTokens, tenant.quota.maxTokensPerMonth)}%`,
                }"
              ></div>
            </div>
          </div>
        </div>

        <div class="tenant-stats">
          <div class="stat-item">
            <i class="pi pi-key"></i>
            <span>{{ tenant.apiKeysCount }} clé(s) API</span>
          </div>
          <div class="stat-item">
            <i class="pi pi-calendar"></i>
            <span>Créé le {{ formatDate(tenant.createdAt) }}</span>
          </div>
        </div>

        <div class="tenant-actions">
          <button
            class="btn-icon"
            title="Gérer les clés API"
            @click="openApiKeysModal(tenant)"
          >
            <i class="pi pi-key"></i>
          </button>
          <button
            class="btn-icon"
            title="Modifier"
            @click="openEditModal(tenant)"
          >
            <i class="pi pi-pencil"></i>
          </button>
          <button
            class="btn-icon danger"
            title="Supprimer"
            @click="confirmDelete(tenant)"
          >
            <i class="pi pi-trash"></i>
          </button>
        </div>
      </AppCard>
    </div>

    <!-- Modal création/édition -->
    <TenantModal
      v-model:visible="showTenantModal"
      :tenant="selectedTenantForEdit"
      :loading="tenantsStore.isSaving"
      @submit="handleTenantSubmit"
    />

    <!-- Dialog de confirmation de suppression -->
    <ConfirmDialog
      v-model:visible="showDeleteConfirm"
      title="Supprimer le tenant"
      :message="`Êtes-vous sûr de vouloir supprimer le tenant « ${tenantsStore.selectedTenant?.name} » ? Toutes ses clés API seront également supprimées. Cette action est irréversible.`"
      confirm-label="Supprimer"
      type="danger"
      :loading="tenantsStore.isSaving"
      @confirm="deleteTenant"
    />

    <!-- Modal gestion des clés API -->
    <AppModal
      v-model:visible="showApiKeysModal"
      :title="`Clés API - ${tenantsStore.selectedTenant?.name}`"
      size="md"
    >
      <div class="api-keys-modal">
        <!-- Formulaire de création -->
        <div class="new-key-form">
          <input
            v-model="newApiKeyName"
            type="text"
            placeholder="Nom de la clé (ex: Production, Development...)"
            class="key-name-input"
            @keyup.enter="createApiKey"
          />
          <button
            class="btn-primary"
            :disabled="!newApiKeyName.trim() || tenantsStore.isSaving"
            @click="createApiKey"
          >
            <i class="pi pi-plus"></i>
            Créer
          </button>
        </div>

        <!-- Liste des clés -->
        <div class="api-keys-list">
          <div
            v-for="apiKey in tenantsStore.apiKeys"
            :key="apiKey.id"
            class="api-key-item"
          >
            <div class="key-info">
              <span class="key-name">{{ apiKey.name }}</span>
              <code class="key-value">{{ apiKey.keyPreview || maskApiKey(apiKey.prefix) }}</code>
            </div>
            <div class="key-meta">
              <span class="key-date">Créée le {{ formatDate(apiKey.createdAt) }}</span>
            </div>
            <button
              class="btn-icon danger"
              title="Révoquer"
              @click="revokeApiKey(apiKey.id)"
            >
              <i class="pi pi-ban"></i>
            </button>
          </div>

          <div v-if="tenantsStore.apiKeys.length === 0" class="no-keys">
            <i class="pi pi-key"></i>
            <p>Aucune clé API pour ce tenant</p>
          </div>
        </div>
      </div>
    </AppModal>
  </div>
</template>

<style scoped>
.tenants-view {
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

/* Grille des tenants */
.tenants-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
  gap: 1rem;
}

.tenant-card {
  display: flex;
  flex-direction: column;
}

.tenant-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  margin-bottom: 1rem;
}

.tenant-name {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--app-text, #1e293b);
}

.tenant-slug {
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.75rem;
  color: var(--app-text-muted, #64748b);
}

/* Quotas */
.tenant-quotas {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.quota-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.quota-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.75rem;
}

.quota-label {
  color: var(--app-text-muted, #64748b);
}

.quota-value {
  font-weight: 500;
  color: var(--app-text, #1e293b);
}

.quota-bar {
  height: 6px;
  background: var(--app-bg-subtle, #f1f5f9);
  border-radius: 3px;
  overflow: hidden;
}

.quota-progress {
  height: 100%;
  border-radius: 3px;
  transition: width 0.3s ease;
}

.quota-ok {
  background: #22c55e;
}

.quota-warning {
  background: #f59e0b;
}

.quota-danger {
  background: #ef4444;
}

/* Stats */
.tenant-stats {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
  padding-top: 0.75rem;
  border-top: 1px solid var(--app-border, #e2e8f0);
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.75rem;
  color: var(--app-text-muted, #64748b);
}

.stat-item i {
  font-size: 0.875rem;
}

/* Actions */
.tenant-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: auto;
  padding-top: 0.75rem;
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

/* Modal clés API */
.api-keys-modal {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.new-key-form {
  display: flex;
  gap: 0.75rem;
}

.key-name-input {
  flex: 1;
  padding: 0.75rem 1rem;
  font-size: 0.875rem;
  border: 1px solid var(--app-border, #e2e8f0);
  border-radius: var(--app-radius-md, 8px);
  background: var(--app-input-bg, #ffffff);
  color: var(--app-text, #1e293b);
}

.key-name-input:focus {
  outline: none;
  border-color: var(--app-primary, #3b82f6);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.api-keys-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.api-key-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: var(--app-bg-subtle, #f8fafc);
  border-radius: var(--app-radius-md, 8px);
}

.key-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.key-name {
  font-weight: 500;
  color: var(--app-text, #1e293b);
}

.key-value {
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.75rem;
  color: var(--app-text-muted, #64748b);
  background: transparent;
  padding: 0;
}

.key-meta {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.25rem;
}

.key-date {
  font-size: 0.75rem;
  color: var(--app-text-muted, #64748b);
}

.no-keys {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 2rem;
  color: var(--app-text-muted, #64748b);
  text-align: center;
}

.no-keys i {
  font-size: 2rem;
  opacity: 0.5;
}

.no-keys p {
  margin: 0;
}
</style>
