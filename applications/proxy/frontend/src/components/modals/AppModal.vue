<script setup lang="ts">
import { watch, onMounted, onBeforeUnmount } from 'vue'

/**
 * Props du composant AppModal.
 */
interface Props {
  /** Afficher ou masquer le modal */
  visible: boolean
  /** Titre du modal */
  title: string
  /** Taille du modal */
  size?: 'sm' | 'md' | 'lg'
  /** Afficher le bouton de fermeture */
  showClose?: boolean
  /** Fermer au clic sur l'overlay */
  closeOnOverlay?: boolean
  /** Fermer avec la touche Escape */
  closeOnEscape?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  showClose: true,
  closeOnOverlay: true,
  closeOnEscape: true,
})

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'close'): void
}>()

/**
 * Ferme le modal.
 */
function close(): void {
  emit('update:visible', false)
  emit('close')
}

/**
 * Gère le clic sur l'overlay.
 */
function handleOverlayClick(): void {
  if (props.closeOnOverlay) {
    close()
  }
}

/**
 * Gère l'appui sur Escape.
 */
function handleKeydown(event: KeyboardEvent): void {
  if (event.key === 'Escape' && props.closeOnEscape && props.visible) {
    close()
  }
}

// Bloquer le scroll du body quand le modal est ouvert
watch(
  () => props.visible,
  (isVisible) => {
    if (isVisible) {
      document.body.style.overflow = 'hidden'
    } else {
      document.body.style.overflow = ''
    }
  },
)

onMounted(() => {
  document.addEventListener('keydown', handleKeydown)
})

onBeforeUnmount(() => {
  document.removeEventListener('keydown', handleKeydown)
  document.body.style.overflow = ''
})
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="visible"
        class="modal-overlay"
        @click.self="handleOverlayClick"
      >
        <div :class="['modal', `modal--${size}`]" role="dialog" aria-modal="true">
          <div class="modal__header">
            <h3 class="modal__title">{{ title }}</h3>
            <button
              v-if="showClose"
              type="button"
              class="modal__close"
              aria-label="Fermer"
              @click="close"
            >
              <i class="pi pi-times"></i>
            </button>
          </div>
          <div class="modal__body">
            <slot></slot>
          </div>
          <div v-if="$slots.footer" class="modal__footer">
            <slot name="footer"></slot>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
  backdrop-filter: blur(2px);
}

.modal {
  background: var(--app-card-bg, #ffffff);
  border-radius: var(--app-radius-lg, 12px);
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  width: 100%;
  max-height: calc(100vh - 2rem);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.modal--sm {
  max-width: 400px;
}

.modal--md {
  max-width: 560px;
}

.modal--lg {
  max-width: 800px;
}

.modal__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid var(--app-border, #e2e8f0);
}

.modal__title {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--app-text, #1e293b);
}

.modal__close {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border: none;
  background: transparent;
  border-radius: var(--app-radius-sm, 6px);
  cursor: pointer;
  color: var(--app-text-muted, #64748b);
  transition: all 0.2s ease;
}

.modal__close:hover {
  background: var(--app-hover-bg, #f1f5f9);
  color: var(--app-text, #1e293b);
}

.modal__body {
  padding: 1.5rem;
  overflow-y: auto;
  flex: 1;
}

.modal__footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid var(--app-border, #e2e8f0);
  background: var(--app-bg-subtle, #f8fafc);
}

/* Animations */
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.2s ease;
}

.modal-enter-active .modal,
.modal-leave-active .modal {
  transition: transform 0.2s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .modal,
.modal-leave-to .modal {
  transform: scale(0.95) translateY(-10px);
}
</style>
