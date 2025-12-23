<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import AppModal from './AppModal.vue'
import type { Provider, CreateProviderRequest, ProviderType } from '@/types'

/**
 * Props du composant ProviderModal.
 */
interface Props {
  /** Afficher ou masquer le modal */
  visible: boolean
  /** Provider à éditer (null pour création) */
  provider?: Provider | null
  /** État de chargement */
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  provider: null,
  loading: false,
})

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'submit', data: CreateProviderRequest): void
  (e: 'cancel'): void
}>()

/** Types de providers disponibles */
const providerTypes: { value: ProviderType; label: string; icon: string; placeholder: string }[] = [
  { value: 'openai', label: 'OpenAI', icon: '🤖', placeholder: 'https://api.openai.com/v1' },
  { value: 'anthropic', label: 'Anthropic', icon: '🧠', placeholder: 'https://api.anthropic.com' },
  { value: 'azure-openai', label: 'Azure OpenAI', icon: '☁️', placeholder: 'https://your-resource.openai.azure.com' },
  { value: 'ollama', label: 'Ollama', icon: '🦙', placeholder: 'http://localhost:11434' },
  { value: 'custom', label: 'Custom', icon: '⚙️', placeholder: 'https://your-api.com' },
]

/** Modèles suggérés par type de provider */
const suggestedModels: Record<ProviderType, string[]> = {
  openai: ['gpt-4o', 'gpt-4o-mini', 'gpt-4-turbo', 'gpt-3.5-turbo'],
  anthropic: ['claude-3-5-sonnet-20241022', 'claude-3-opus-20240229', 'claude-3-haiku-20240307'],
  'azure-openai': ['gpt-4o', 'gpt-4', 'gpt-35-turbo'],
  ollama: ['llama3.2', 'llama3.1', 'mistral', 'codellama', 'phi3'],
  custom: [],
}

/** Formulaire */
const form = ref<CreateProviderRequest>({
  name: '',
  type: 'openai',
  baseUrl: '',
  apiKey: '',
  model: '',
  isEnabled: true,
})

/** Erreurs de validation */
const errors = ref<Record<string, string>>({})

/** Mode édition ou création */
const isEditing = computed(() => props.provider !== null)

/** Titre du modal */
const modalTitle = computed(() =>
  isEditing.value ? 'Modifier le Provider' : 'Nouveau Provider',
)

/** URL placeholder selon le type */
const urlPlaceholder = computed(() =>
  providerTypes.find((t) => t.value === form.value.type)?.placeholder ?? '',
)

/** Modèles suggérés pour le type sélectionné */
const currentSuggestedModels = computed(() =>
  suggestedModels[form.value.type] ?? [],
)

/** Réinitialise le formulaire */
function resetForm(): void {
  form.value = {
    name: '',
    type: 'openai',
    baseUrl: '',
    apiKey: '',
    model: '',
    isEnabled: true,
  }
  errors.value = {}
}

/** Remplit le formulaire avec les données du provider */
function fillForm(provider: Provider): void {
  form.value = {
    name: provider.name,
    type: provider.type,
    baseUrl: provider.baseUrl,
    apiKey: '',
    model: provider.model,
    isEnabled: provider.isEnabled,
  }
  errors.value = {}
}

/** Valide le formulaire */
function validate(): boolean {
  errors.value = {}

  if (!form.value.name.trim()) {
    errors.value.name = 'Le nom est requis'
  }

  if (!form.value.baseUrl.trim()) {
    errors.value.baseUrl = "L'URL de base est requise"
  } else {
    try {
      new URL(form.value.baseUrl)
    } catch {
      errors.value.baseUrl = "L'URL n'est pas valide"
    }
  }

  if (!isEditing.value && !form.value.apiKey.trim()) {
    errors.value.apiKey = 'La clé API est requise'
  }

  if (!form.value.model.trim()) {
    errors.value.model = 'Le modèle est requis'
  }

  return Object.keys(errors.value).length === 0
}

/** Soumet le formulaire */
function submit(): void {
  if (!validate()) return
  emit('submit', { ...form.value })
}

/** Ferme le modal */
function close(): void {
  emit('update:visible', false)
  emit('cancel')
}

/** Sélectionne un modèle suggéré */
function selectModel(model: string): void {
  form.value.model = model
}

// Surveille les changements de visibilité pour réinitialiser ou remplir le formulaire
watch(
  () => props.visible,
  (isVisible) => {
    if (isVisible) {
      if (props.provider) {
        fillForm(props.provider)
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
    <form @submit.prevent="submit" class="provider-form">
      <!-- Nom -->
      <div class="form-group" :class="{ 'has-error': errors.name }">
        <label for="provider-name">Nom <span class="required">*</span></label>
        <input
          id="provider-name"
          v-model="form.name"
          type="text"
          placeholder="Mon Provider OpenAI"
          autocomplete="off"
        />
        <span v-if="errors.name" class="error-message">{{ errors.name }}</span>
      </div>

      <!-- Type -->
      <div class="form-group">
        <label for="provider-type">Type <span class="required">*</span></label>
        <div class="provider-types">
          <button
            v-for="pType in providerTypes"
            :key="pType.value"
            type="button"
            :class="['provider-type-btn', { active: form.type === pType.value }]"
            @click="form.type = pType.value"
          >
            <span class="provider-type-icon">{{ pType.icon }}</span>
            <span class="provider-type-label">{{ pType.label }}</span>
          </button>
        </div>
      </div>

      <!-- URL de base -->
      <div class="form-group" :class="{ 'has-error': errors.baseUrl }">
        <label for="provider-url">URL de base <span class="required">*</span></label>
        <input
          id="provider-url"
          v-model="form.baseUrl"
          type="url"
          :placeholder="urlPlaceholder"
          autocomplete="off"
        />
        <span v-if="errors.baseUrl" class="error-message">{{ errors.baseUrl }}</span>
      </div>

      <!-- Clé API -->
      <div class="form-group" :class="{ 'has-error': errors.apiKey }">
        <label for="provider-apikey">
          Clé API
          <span v-if="!isEditing" class="required">*</span>
          <span v-else class="hint">(laisser vide pour conserver)</span>
        </label>
        <input
          id="provider-apikey"
          v-model="form.apiKey"
          type="password"
          :placeholder="isEditing ? '••••••••••••' : 'sk-...'"
          autocomplete="new-password"
        />
        <span v-if="errors.apiKey" class="error-message">{{ errors.apiKey }}</span>
      </div>

      <!-- Modèle -->
      <div class="form-group" :class="{ 'has-error': errors.model }">
        <label for="provider-model">Modèle <span class="required">*</span></label>
        <input
          id="provider-model"
          v-model="form.model"
          type="text"
          placeholder="gpt-4o"
          autocomplete="off"
        />
        <div v-if="currentSuggestedModels.length > 0" class="suggested-models">
          <span class="suggested-label">Suggestions :</span>
          <button
            v-for="model in currentSuggestedModels"
            :key="model"
            type="button"
            :class="['suggested-model-btn', { active: form.model === model }]"
            @click="selectModel(model)"
          >
            {{ model }}
          </button>
        </div>
        <span v-if="errors.model" class="error-message">{{ errors.model }}</span>
      </div>

      <!-- Activer -->
      <div class="form-group checkbox">
        <label class="checkbox-label">
          <input
            id="provider-enabled"
            v-model="form.isEnabled"
            type="checkbox"
          />
          <span class="checkmark"></span>
          Activer le provider
        </label>
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
.provider-form {
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

.form-group input[type='text'],
.form-group input[type='url'],
.form-group input[type='password'] {
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

/* Provider type buttons */
.provider-types {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.provider-type-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0.75rem;
  font-size: 0.875rem;
  border: 1px solid var(--app-border, #e2e8f0);
  border-radius: var(--app-radius-md, 8px);
  background: var(--app-card-bg, #ffffff);
  color: var(--app-text, #1e293b);
  cursor: pointer;
  transition: all 0.2s ease;
}

.provider-type-btn:hover {
  border-color: var(--app-primary, #3b82f6);
  background: rgba(59, 130, 246, 0.05);
}

.provider-type-btn.active {
  border-color: var(--app-primary, #3b82f6);
  background: rgba(59, 130, 246, 0.1);
  color: var(--app-primary, #3b82f6);
}

.provider-type-icon {
  font-size: 1.25rem;
}

/* Suggested models */
.suggested-models {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem;
  margin-top: 0.25rem;
}

.suggested-label {
  font-size: 0.75rem;
  color: var(--app-text-muted, #64748b);
}

.suggested-model-btn {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
  border: 1px solid var(--app-border, #e2e8f0);
  border-radius: var(--app-radius-sm, 6px);
  background: var(--app-card-bg, #ffffff);
  color: var(--app-text-muted, #64748b);
  cursor: pointer;
  transition: all 0.2s ease;
}

.suggested-model-btn:hover {
  border-color: var(--app-primary, #3b82f6);
  color: var(--app-primary, #3b82f6);
}

.suggested-model-btn.active {
  background: var(--app-primary, #3b82f6);
  border-color: var(--app-primary, #3b82f6);
  color: white;
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
