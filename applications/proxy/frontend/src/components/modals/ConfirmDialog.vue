<script setup lang="ts">
import AppModal from './AppModal.vue'

/**
 * Props du composant ConfirmDialog.
 */
interface Props {
  /** Afficher ou masquer le dialog */
  visible: boolean
  /** Titre du dialog */
  title: string
  /** Message de confirmation */
  message: string
  /** Texte du bouton de confirmation */
  confirmLabel?: string
  /** Texte du bouton d'annulation */
  cancelLabel?: string
  /** Type de confirmation (danger affiche le bouton en rouge) */
  type?: 'default' | 'danger'
  /** État de chargement */
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  confirmLabel: 'Confirmer',
  cancelLabel: 'Annuler',
  type: 'default',
  loading: false,
})

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'confirm'): void
  (e: 'cancel'): void
}>()

/**
 * Ferme le dialog.
 */
function close(): void {
  emit('update:visible', false)
  emit('cancel')
}

/**
 * Confirme l'action.
 */
function confirm(): void {
  emit('confirm')
}
</script>

<template>
  <AppModal
    :visible="visible"
    :title="title"
    size="sm"
    @update:visible="$emit('update:visible', $event)"
    @close="close"
  >
    <div class="confirm-dialog">
      <div v-if="type === 'danger'" class="confirm-dialog__icon confirm-dialog__icon--danger">
        <i class="pi pi-exclamation-triangle"></i>
      </div>
      <p class="confirm-dialog__message">{{ message }}</p>
    </div>

    <template #footer>
      <button
        type="button"
        class="btn-secondary"
        :disabled="loading"
        @click="close"
      >
        {{ cancelLabel }}
      </button>
      <button
        type="button"
        :class="type === 'danger' ? 'btn-danger' : 'btn-primary'"
        :disabled="loading"
        @click="confirm"
      >
        <i v-if="loading" class="pi pi-spin pi-spinner"></i>
        {{ confirmLabel }}
      </button>
    </template>
  </AppModal>
</template>

<style scoped>
.confirm-dialog {
  text-align: center;
  padding: 1rem 0;
}

.confirm-dialog__icon {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1rem;
  font-size: 1.75rem;
}

.confirm-dialog__icon--danger {
  background: #fef2f2;
  color: #dc2626;
}

.confirm-dialog__message {
  color: var(--app-text, #1e293b);
  font-size: 1rem;
  line-height: 1.5;
  margin: 0;
}

.btn-danger {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.625rem 1.25rem;
  font-size: 0.875rem;
  font-weight: 500;
  border: none;
  border-radius: var(--app-radius-md, 8px);
  cursor: pointer;
  transition: all 0.2s ease;
  background: #dc2626;
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background: #b91c1c;
}

.btn-danger:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
