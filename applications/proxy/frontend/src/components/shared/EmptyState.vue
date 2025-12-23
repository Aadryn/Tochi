<script setup lang="ts">
interface Props {
  icon?: string
  title: string
  description?: string
  actionLabel?: string
}

const props = defineProps<Props>()
const emit = defineEmits<{
  action: []
}>()
</script>

<template>
  <div class="empty-state">
    <i v-if="icon" :class="icon" class="empty-icon"></i>
    <h3 class="empty-title">{{ title }}</h3>
    <p v-if="description" class="empty-description">{{ description }}</p>
    <button v-if="actionLabel" class="empty-action" @click="emit('action')">
      {{ actionLabel }}
    </button>
    <slot></slot>
  </div>
</template>

<style scoped>
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 2rem;
  text-align: center;
}

.empty-icon {
  font-size: 3rem;
  color: var(--text-color-secondary);
  opacity: 0.5;
  margin-bottom: 1rem;
}

.empty-title {
  margin: 0 0 0.5rem;
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--text-color);
}

.empty-description {
  margin: 0 0 1.5rem;
  font-size: 0.875rem;
  color: var(--text-color-secondary);
  max-width: 320px;
}

.empty-action {
  padding: 0.625rem 1.25rem;
  background: var(--primary-color);
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
}

.empty-action:hover {
  background: var(--primary-700);
}
</style>
