<script setup lang="ts">
interface Props {
  loading?: boolean
  message?: string
}

withDefaults(defineProps<Props>(), {
  loading: true,
  message: 'Chargement en cours...',
})
</script>

<template>
  <div v-if="loading" class="loading-spinner">
    <i class="pi pi-spin pi-spinner spinner-icon"></i>
    <span class="spinner-message">{{ message }}</span>
  </div>
</template>

<style scoped>
.loading-spinner {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  gap: 1rem;
}

.spinner-icon {
  font-size: 2rem;
  color: var(--primary-color);
}

.spinner-message {
  font-size: 0.875rem;
  color: var(--text-color-secondary);
}
</style>
