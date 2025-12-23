<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { AppCard, LoadingSpinner, EmptyState } from '@/components'
import type { RouteFormData, RouteDisplayData } from '@/types'

const routes = ref<RouteDisplayData[]>([])
const isLoading = ref(true)
const showCreateDialog = ref(false)
const showEditDialog = ref(false)
const showDeleteConfirm = ref(false)
const selectedRoute = ref<RouteDisplayData | null>(null)

const newRoute = ref<RouteFormData>({
  name: '',
  path: '',
  providerId: '',
  method: 'POST',
  isEnabled: true,
  rateLimit: {
    requestsPerMinute: 60,
    requestsPerHour: 1000,
  },
})

// Données mock
const mockRoutes: RouteDisplayData[] = [
  {
    id: '1',
    name: 'Chat Completions',
    path: '/v1/chat/completions',
    providerId: '1',
    providerName: 'OpenAI GPT-4',
    method: 'POST',
    isEnabled: true,
    requestsToday: 12450,
    avgLatencyMs: 245,
    rateLimit: {
      requestsPerMinute: 60,
      requestsPerHour: 1000,
    },
    createdAt: '2024-01-15T10:30:00Z',
  },
  {
    id: '2',
    name: 'Embeddings',
    path: '/v1/embeddings',
    providerId: '1',
    providerName: 'OpenAI GPT-4',
    method: 'POST',
    isEnabled: true,
    requestsToday: 8920,
    avgLatencyMs: 120,
    rateLimit: {
      requestsPerMinute: 100,
      requestsPerHour: 2000,
    },
    createdAt: '2024-01-16T14:00:00Z',
  },
  {
    id: '3',
    name: 'Messages',
    path: '/v1/messages',
    providerId: '2',
    providerName: 'Anthropic Claude',
    method: 'POST',
    isEnabled: true,
    requestsToday: 5630,
    avgLatencyMs: 312,
    rateLimit: {
      requestsPerMinute: 40,
      requestsPerHour: 800,
    },
    createdAt: '2024-01-20T09:15:00Z',
  },
]

onMounted(async () => {
  // Simuler un chargement
  await new Promise((resolve) => setTimeout(resolve, 500))
  routes.value = mockRoutes
  isLoading.value = false
})

function openCreateDialog(): void {
  newRoute.value = {
    name: '',
    path: '',
    providerId: '',
    method: 'POST',
    isEnabled: true,
    rateLimit: {
      requestsPerMinute: 60,
      requestsPerHour: 1000,
    },
  }
  showCreateDialog.value = true
}

function openEditDialog(route: RouteDisplayData): void {
  selectedRoute.value = route
  newRoute.value = {
    name: route.name,
    path: route.path,
    providerId: route.providerId,
    method: route.method,
    isEnabled: route.isEnabled,
    rateLimit: { ...route.rateLimit },
  }
  showEditDialog.value = true
}

function confirmDelete(route: RouteDisplayData): void {
  selectedRoute.value = route
  showDeleteConfirm.value = true
}

function createRoute(): void {
  // Mock: ajouter la route à la liste
  const newId = String(routes.value.length + 1)
  routes.value.push({
    id: newId,
    ...newRoute.value,
    providerName: 'Provider Mock',
    requestsToday: 0,
    avgLatencyMs: 0,
    createdAt: new Date().toISOString(),
  })
  showCreateDialog.value = false
}

function updateRoute(): void {
  if (!selectedRoute.value) return

  const index = routes.value.findIndex((r) => r.id === selectedRoute.value!.id)
  if (index !== -1) {
    routes.value[index] = {
      ...routes.value[index],
      ...newRoute.value,
    }
  }
  showEditDialog.value = false
  selectedRoute.value = null
}

function deleteRoute(): void {
  if (!selectedRoute.value) return

  routes.value = routes.value.filter((r) => r.id !== selectedRoute.value!.id)
  showDeleteConfirm.value = false
  selectedRoute.value = null
}

function formatNumber(value: number): string {
  return value.toLocaleString('fr-FR')
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('fr-FR', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}
</script>

<template>
  <div class="routes-view">
    <div class="view-header">
      <div class="header-info">
        <h2 class="view-title">Routes</h2>
        <p class="view-description">
          Configurez le routage des requêtes vers vos providers LLM
        </p>
      </div>
      <button class="btn-primary" @click="openCreateDialog">
        <i class="pi pi-plus"></i>
        Nouvelle Route
      </button>
    </div>

    <LoadingSpinner v-if="isLoading" message="Chargement des routes..." />

    <EmptyState
      v-else-if="routes.length === 0"
      icon="pi pi-directions"
      title="Aucune route configurée"
      description="Créez votre première route pour commencer à acheminer les requêtes."
      action-label="Créer une route"
      @action="openCreateDialog"
    />

    <AppCard v-else :padding="false">
      <table class="routes-table">
        <thead>
          <tr>
            <th>Chemin</th>
            <th>Provider</th>
            <th>Méthode</th>
            <th>Rate Limit</th>
            <th>Requêtes/jour</th>
            <th>Latence moy.</th>
            <th>Statut</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="route in routes" :key="route.id">
            <td class="route-path">{{ route.path }}</td>
            <td>{{ route.providerName }}</td>
            <td>
              <span class="method-badge">{{ route.method }}</span>
            </td>
            <td class="rate-limit">
              {{ route.rateLimit.requestsPerMinute }}/min
            </td>
            <td>{{ formatNumber(route.requestsToday) }}</td>
            <td>{{ route.avgLatencyMs }}ms</td>
            <td>
              <span
                class="status-indicator"
                :class="{ enabled: route.isEnabled }"
              >
                {{ route.isEnabled ? 'Actif' : 'Inactif' }}
              </span>
            </td>
            <td class="actions-cell">
              <button
                class="btn-icon"
                @click="openEditDialog(route)"
                title="Modifier"
              >
                <i class="pi pi-pencil"></i>
              </button>
              <button
                class="btn-icon danger"
                @click="confirmDelete(route)"
                title="Supprimer"
              >
                <i class="pi pi-trash"></i>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </AppCard>

    <!-- Dialog de création -->
    <div v-if="showCreateDialog" class="dialog-overlay" @click.self="showCreateDialog = false">
      <div class="dialog">
        <div class="dialog-header">
          <h3>Nouvelle Route</h3>
          <button class="btn-icon" @click="showCreateDialog = false">
            <i class="pi pi-times"></i>
          </button>
        </div>
        <div class="dialog-body">
          <div class="form-group">
            <label for="path">Chemin de l'endpoint</label>
            <input
              id="path"
              v-model="newRoute.path"
              type="text"
              placeholder="/v1/chat/completions"
            />
          </div>
          <div class="form-group">
            <label for="method">Méthode HTTP</label>
            <select id="method" v-model="newRoute.method">
              <option value="GET">GET</option>
              <option value="POST">POST</option>
              <option value="PUT">PUT</option>
              <option value="DELETE">DELETE</option>
            </select>
          </div>
          <div class="form-group">
            <label for="rpm">Requêtes par minute</label>
            <input
              id="rpm"
              v-model.number="newRoute.rateLimit.requestsPerMinute"
              type="number"
              min="1"
            />
          </div>
          <div class="form-group">
            <label for="rph">Requêtes par heure</label>
            <input
              id="rph"
              v-model.number="newRoute.rateLimit.requestsPerHour"
              type="number"
              min="1"
            />
          </div>
          <div class="form-group checkbox">
            <input id="isEnabled" v-model="newRoute.isEnabled" type="checkbox" />
            <label for="isEnabled">Activer la route</label>
          </div>
        </div>
        <div class="dialog-footer">
          <button class="btn-secondary" @click="showCreateDialog = false">
            Annuler
          </button>
          <button class="btn-primary" @click="createRoute">
            Créer
          </button>
        </div>
      </div>
    </div>

    <!-- Dialog de modification -->
    <div v-if="showEditDialog" class="dialog-overlay" @click.self="showEditDialog = false">
      <div class="dialog">
        <div class="dialog-header">
          <h3>Modifier la Route</h3>
          <button class="btn-icon" @click="showEditDialog = false">
            <i class="pi pi-times"></i>
          </button>
        </div>
        <div class="dialog-body">
          <div class="form-group">
            <label for="edit-path">Chemin de l'endpoint</label>
            <input id="edit-path" v-model="newRoute.path" type="text" />
          </div>
          <div class="form-group">
            <label for="edit-method">Méthode HTTP</label>
            <select id="edit-method" v-model="newRoute.method">
              <option value="GET">GET</option>
              <option value="POST">POST</option>
              <option value="PUT">PUT</option>
              <option value="DELETE">DELETE</option>
            </select>
          </div>
          <div class="form-group">
            <label for="edit-rpm">Requêtes par minute</label>
            <input
              id="edit-rpm"
              v-model.number="newRoute.rateLimit.requestsPerMinute"
              type="number"
              min="1"
            />
          </div>
          <div class="form-group">
            <label for="edit-rph">Requêtes par heure</label>
            <input
              id="edit-rph"
              v-model.number="newRoute.rateLimit.requestsPerHour"
              type="number"
              min="1"
            />
          </div>
          <div class="form-group checkbox">
            <input id="edit-isEnabled" v-model="newRoute.isEnabled" type="checkbox" />
            <label for="edit-isEnabled">Activer la route</label>
          </div>
        </div>
        <div class="dialog-footer">
          <button class="btn-secondary" @click="showEditDialog = false">
            Annuler
          </button>
          <button class="btn-primary" @click="updateRoute">
            Enregistrer
          </button>
        </div>
      </div>
    </div>

    <!-- Dialog de confirmation de suppression -->
    <div v-if="showDeleteConfirm" class="dialog-overlay" @click.self="showDeleteConfirm = false">
      <div class="dialog dialog-sm">
        <div class="dialog-header">
          <h3>Confirmer la suppression</h3>
          <button class="btn-icon" @click="showDeleteConfirm = false">
            <i class="pi pi-times"></i>
          </button>
        </div>
        <div class="dialog-body">
          <p>
            Êtes-vous sûr de vouloir supprimer la route
            <strong>{{ selectedRoute?.path }}</strong> ?
          </p>
        </div>
        <div class="dialog-footer">
          <button class="btn-secondary" @click="showDeleteConfirm = false">
            Annuler
          </button>
          <button class="btn-danger" @click="deleteRoute">
            Supprimer
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.routes-view {
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
}

.view-description {
  margin: 0.25rem 0 0;
  color: var(--text-color-secondary);
}

/* Table */
.routes-table {
  width: 100%;
  border-collapse: collapse;
}

.routes-table th,
.routes-table td {
  padding: 1rem 1.25rem;
  text-align: left;
  border-bottom: 1px solid var(--surface-border);
}

.routes-table th {
  font-weight: 600;
  font-size: 0.875rem;
  color: var(--text-color-secondary);
  background: var(--surface-ground);
}

.routes-table tr:last-child td {
  border-bottom: none;
}

.route-path {
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.875rem;
}

.method-badge {
  display: inline-block;
  padding: 0.25rem 0.5rem;
  background: var(--blue-100);
  color: var(--blue-700);
  font-size: 0.75rem;
  font-weight: 600;
  border-radius: 4px;
}

.rate-limit {
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.875rem;
}

.status-indicator {
  display: inline-block;
  padding: 0.25rem 0.5rem;
  background: var(--red-100);
  color: var(--red-700);
  font-size: 0.75rem;
  font-weight: 500;
  border-radius: 4px;
}

.status-indicator.enabled {
  background: var(--green-100);
  color: var(--green-700);
}

.actions-cell {
  display: flex;
  gap: 0.5rem;
}

/* Boutons */
.btn-primary {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.625rem 1rem;
  background: var(--primary-color);
  color: white;
  border: none;
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
}

.btn-primary:hover {
  background: var(--primary-700);
}

.btn-secondary {
  padding: 0.625rem 1rem;
  background: var(--surface-100);
  color: var(--text-color);
  border: 1px solid var(--surface-border);
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
}

.btn-danger {
  padding: 0.625rem 1rem;
  background: var(--red-500);
  color: white;
  border: none;
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
}

.btn-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  background: transparent;
  border: 1px solid var(--surface-border);
  border-radius: 6px;
  color: var(--text-color-secondary);
  cursor: pointer;
}

.btn-icon:hover {
  background: var(--surface-hover);
  color: var(--text-color);
}

.btn-icon.danger:hover {
  background: var(--red-50);
  border-color: var(--red-200);
  color: var(--red-600);
}

/* Dialog */
.dialog-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
}

.dialog {
  background: var(--surface-card);
  border-radius: 12px;
  width: 100%;
  max-width: 480px;
  max-height: 90vh;
  overflow: hidden;
}

.dialog-sm {
  max-width: 400px;
}

.dialog-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.25rem;
  border-bottom: 1px solid var(--surface-border);
}

.dialog-header h3 {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
}

.dialog-body {
  padding: 1.25rem;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1rem 1.25rem;
  border-top: 1px solid var(--surface-border);
  background: var(--surface-ground);
}

/* Form */
.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
}

.form-group input[type='text'],
.form-group input[type='number'],
.form-group select {
  width: 100%;
  padding: 0.625rem 0.75rem;
  border: 1px solid var(--surface-border);
  border-radius: 6px;
  font-size: 0.875rem;
  background: var(--surface-card);
  color: var(--text-color);
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: var(--primary-color);
}

.form-group.checkbox {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.form-group.checkbox input {
  width: auto;
}

.form-group.checkbox label {
  margin-bottom: 0;
}
</style>
