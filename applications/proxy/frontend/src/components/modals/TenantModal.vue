<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import AppModal from './AppModal.vue'
import type { Tenant, CreateTenantRequest } from '@/types'

/**
 * Props du composant TenantModal.
 */
interface Props {
  /** Afficher ou masquer le modal */
  visible: boolean
  /** Tenant à éditer (null pour création) */
  tenant?: Tenant | null
  /** État de chargement */
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  tenant: null,
  loading: false,
})

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'submit', data: CreateTenantRequest): void
  (e: 'cancel'): void
}>()

/** Formulaire */
const form = ref<{
  name: string
  slug: string
  isActive: boolean
  maxRequestsPerMonth: number
  maxTokensPerMonth: number
}>({
  name: '',
  slug: '',
  isActive: true,
  maxRequestsPerMonth: 100000,
  maxTokensPerMonth: 10000000,
})

/** Erreurs de validation */
const errors = ref<Record<string, string>>({})

/** Mode édition ou création */
const isEditing = computed(() => props.tenant !== null)

/** Titre du modal */
const modalTitle = computed(() =>
  isEditing.value ? 'Modifier le Tenant' : 'Nouveau Tenant',
)

/** Quotas prédéfinis */
const quotaPresets = [
  { label: 'Starter', requests: 10000, tokens: 1000000 },
  { label: 'Standard', requests: 100000, tokens: 10000000 },
  { label: 'Pro', requests: 500000, tokens: 50000000 },
  { label: 'Enterprise', requests: 2000000, tokens: 200000000 },
]

/** Génère un slug à partir du nom */
function generateSlug(name: string): string {
  return name
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-|-$/g, '')
}

/** Réinitialise le formulaire */
function resetForm(): void {
  form.value = {
    name: '',
    slug: '',
    isActive: true,
    maxRequestsPerMonth: 100000,
    maxTokensPerMonth: 10000000,
  }
  errors.value = {}
}

/** Remplit le formulaire avec les données du tenant */
function fillForm(tenant: Tenant): void {
  form.value = {
    name: tenant.name,
    slug: tenant.slug,
    isActive: tenant.isActive,
    maxRequestsPerMonth: tenant.quota.maxRequestsPerMonth,
    maxTokensPerMonth: tenant.quota.maxTokensPerMonth,
  }
  errors.value = {}
}

/** Valide le formulaire */
function validate(): boolean {
  errors.value = {}

  if (!form.value.name.trim()) {
    errors.value.name = 'Le nom est requis'
  }

  if (!form.value.slug.trim()) {
    errors.value.slug = 'Le slug est requis'
  } else if (!/^[a-z0-9-]+$/.test(form.value.slug)) {
    errors.value.slug = 'Le slug ne peut contenir que des lettres minuscules, chiffres et tirets'
  }

  if (form.value.maxRequestsPerMonth <= 0) {
    errors.value.maxRequestsPerMonth = 'Le quota de requêtes doit être positif'
  }

  if (form.value.maxTokensPerMonth <= 0) {
    errors.value.maxTokensPerMonth = 'Le quota de tokens doit être positif'
  }

  return Object.keys(errors.value).length === 0
}

/** Soumet le formulaire */
function submit(): void {
  if (!validate()) return

  const data: CreateTenantRequest = {
    name: form.value.name,
    slug: form.value.slug,
    maxRequestsPerMonth: form.value.maxRequestsPerMonth,
    maxTokensPerMonth: form.value.maxTokensPerMonth,
    isActive: form.value.isActive,
  }

  emit('submit', data)
}

/** Ferme le modal */
function close(): void {
  emit('update:visible', false)
  emit('cancel')
}

/** Applique un preset de quota */
function applyQuotaPreset(preset: (typeof quotaPresets)[number]): void {
  form.value.maxRequestsPerMonth = preset.requests
  form.value.maxTokensPerMonth = preset.tokens
}

/** Met à jour le slug quand le nom change (seulement en mode création) */
function handleNameInput(): void {
  if (!isEditing.value) {
    form.value.slug = generateSlug(form.value.name)
  }
}

/** Formate un nombre avec séparateurs */
function formatNumber(value: number): string {
  return value.toLocaleString('fr-FR')
}

// Surveille les changements de visibilité pour réinitialiser ou remplir le formulaire
watch(
  () => props.visible,
  (isVisible) => {
    if (isVisible) {
      if (props.tenant) {
        fillForm(props.tenant)
      } else {
        resetForm()
      }
    }
  },
)
</script>

<template>
  <AppModal
    :visible="visible"
    :title="modalTitle"
    size="md"
    @update:visible="$emit('update:visible', $event)"
    @close="close"
  >
    <form @submit.prevent="submit" class="tenant-form">
      <!-- Nom -->
      <div class="form-group" :class="{ 'has-error': errors.name }">
        <label for="tenant-name">Nom <span class="required">*</span></label>
        <input
          id="tenant-name"
          v-model="form.name"
          type="text"
          placeholder="Mon Application"
          autocomplete="off"
          @input="handleNameInput"
        />
        <span v-if="errors.name" class="error-message">{{ errors.name }}</span>
      </div>

      <!-- Slug -->
      <div class="form-group" :class="{ 'has-error': errors.slug }">
        <label for="tenant-slug">
          Slug <span class="required">*</span>
          <span class="hint">(identifiant unique dans l'URL)</span>
        </label>
        <div class="slug-input">
          <span class="slug-prefix">/api/</span>
          <input
            id="tenant-slug"
            v-model="form.slug"
            type="text"
            placeholder="mon-application"
            autocomplete="off"
          />
          <span class="slug-suffix">/...</span>
        </div>
        <span v-if="errors.slug" class="error-message">{{ errors.slug }}</span>
      </div>

      <!-- Quotas -->
      <div class="form-section">
        <h4 class="section-title">Quotas mensuels</h4>
        
        <div class="quota-presets">
          <button
            v-for="preset in quotaPresets"
            :key="preset.label"
            type="button"
            :class="[
              'quota-preset-btn',
              {
                active:
                  form.maxRequestsPerMonth === preset.requests &&
                  form.maxTokensPerMonth === preset.tokens,
              },
            ]"
            @click="applyQuotaPreset(preset)"
          >
            <span class="preset-label">{{ preset.label }}</span>
            <span class="preset-details">{{ formatNumber(preset.requests) }} req</span>
          </button>
        </div>

        <div class="quotas-grid">
          <div class="form-group" :class="{ 'has-error': errors.maxRequestsPerMonth }">
            <label for="tenant-requests">Requêtes max</label>
            <input
              id="tenant-requests"
              v-model.number="form.maxRequestsPerMonth"
              type="number"
              min="1"
              step="1000"
            />
            <span class="field-hint">{{ formatNumber(form.maxRequestsPerMonth) }} requêtes/mois</span>
            <span v-if="errors.maxRequestsPerMonth" class="error-message">
              {{ errors.maxRequestsPerMonth }}
            </span>
          </div>

          <div class="form-group" :class="{ 'has-error': errors.maxTokensPerMonth }">
            <label for="tenant-tokens">Tokens max</label>
            <input
              id="tenant-tokens"
              v-model.number="form.maxTokensPerMonth"
              type="number"
              min="1"
              step="100000"
            />
            <span class="field-hint">{{ formatNumber(form.maxTokensPerMonth) }} tokens/mois</span>
            <span v-if="errors.maxTokensPerMonth" class="error-message">
              {{ errors.maxTokensPerMonth }}
            </span>
          </div>
        </div>
      </div>

      <!-- Activer -->
      <div class="form-group checkbox">
        <label class="checkbox-label">
          <input
            id="tenant-active"
            v-model="form.isActive"
            type="checkbox"
          />
          <span class="checkmark"></span>
          Activer le tenant
        </label>
        <span class="field-hint">
          Un tenant inactif ne pourra pas utiliser l'API
        </span>
      </div>
    </form>

    <template #footer>
      <button type="button" class="btn-secondary" :disabled="loading" @click="close">
        Annuler
      </button>
      <button type="button" class="btn-primary" :disabled="loading" @click="submit">
        <i v-if="loading" class="pi pi-spin pi-spinner"></i>
        {{ isEditing ? 'Enregistrer' : 'Créer' }}
      </button>
    </template>
  </AppModal>
</template>

<style scoped>
.tenant-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--app-text, #1e293b);
}

.required {
  color: #dc2626;
}

.hint {
  font-weight: 400;
  color: var(--app-text-muted, #64748b);
  font-size: 0.75rem;
}

.field-hint {
  font-size: 0.75rem;
  color: var(--app-text-muted, #64748b);
}

.form-group input[type='text'],
.form-group input[type='number'] {
  padding: 0.75rem 1rem;
  font-size: 0.875rem;
  border: 1px solid var(--app-border, #e2e8f0);
  border-radius: var(--app-radius-md, 8px);
  background: var(--app-input-bg, #ffffff);
  color: var(--app-text, #1e293b);
  transition: all 0.2s ease;
}

.form-group input:focus {
  outline: none;
  border-color: var(--app-primary, #3b82f6);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-group.has-error input {
  border-color: #dc2626;
}

.error-message {
  font-size: 0.75rem;
  color: #dc2626;
}

/* Slug input */
.slug-input {
  display: flex;
  align-items: center;
  border: 1px solid var(--app-border, #e2e8f0);
  border-radius: var(--app-radius-md, 8px);
  background: var(--app-input-bg, #ffffff);
  overflow: hidden;
}

.slug-prefix,
.slug-suffix {
  padding: 0.75rem;
  font-size: 0.875rem;
  color: var(--app-text-muted, #64748b);
  background: var(--app-bg-subtle, #f8fafc);
  white-space: nowrap;
}

.slug-input input {
  flex: 1;
  padding: 0.75rem;
  border: none;
  background: transparent;
  font-size: 0.875rem;
  color: var(--app-text, #1e293b);
}

.slug-input input:focus {
  outline: none;
}

.form-group.has-error .slug-input {
  border-color: #dc2626;
}

/* Section */
.form-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.section-title {
  margin: 0;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--app-text, #1e293b);
}

/* Quota presets */
.quota-presets {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.quota-preset-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.25rem;
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
  border: 1px solid var(--app-border, #e2e8f0);
  border-radius: var(--app-radius-md, 8px);
  background: var(--app-card-bg, #ffffff);
  cursor: pointer;
  transition: all 0.2s ease;
}

.quota-preset-btn:hover {
  border-color: var(--app-primary, #3b82f6);
  background: rgba(59, 130, 246, 0.05);
}

.quota-preset-btn.active {
  border-color: var(--app-primary, #3b82f6);
  background: rgba(59, 130, 246, 0.1);
  color: var(--app-primary, #3b82f6);
}

.preset-label {
  font-weight: 600;
  color: inherit;
}

.preset-details {
  font-size: 0.75rem;
  color: var(--app-text-muted, #64748b);
}

.quota-preset-btn.active .preset-details {
  color: var(--app-primary, #3b82f6);
}

/* Quotas grid */
.quotas-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
}

/* Checkbox */
.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  cursor: pointer;
  user-select: none;
}

.checkbox-label input {
  position: absolute;
  opacity: 0;
  cursor: pointer;
  height: 0;
  width: 0;
}

.checkmark {
  width: 20px;
  height: 20px;
  border: 2px solid var(--app-border, #e2e8f0);
  border-radius: 4px;
  background: var(--app-card-bg, #ffffff);
  position: relative;
  transition: all 0.2s ease;
}

.checkbox-label input:checked ~ .checkmark {
  background: var(--app-primary, #3b82f6);
  border-color: var(--app-primary, #3b82f6);
}

.checkmark::after {
  content: '';
  position: absolute;
  display: none;
  left: 6px;
  top: 2px;
  width: 5px;
  height: 10px;
  border: solid white;
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
}

.checkbox-label input:checked ~ .checkmark::after {
  display: block;
}
</style>
